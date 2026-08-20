using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using Boxhead.Core;
using Boxhead.Player;

namespace Boxhead.Systems
{
    /// <summary>
    /// Plays the forge "transformation moment" in-world, at the workbench — not inside the
    /// paused ForgePanel modal. Subscribes to ForgeController.OnWeaponForged /
    /// OnWeaponUpgraded and drives an unscaled-time sequence: anticipation, transformation,
    /// colour bloom, impact, and an imaginative-text reveal. Weapon glow is handled separately,
    /// as persistent state applied by WeaponHolder at equip time (see WeaponTierGlow) —
    /// it is not part of this timed sequence.
    ///
    /// Deliberately kept separate from ForgeController: that system is close to pure,
    /// testable domain logic (slot-check-before-spend) and should not gain presentation
    /// concerns. This component only reads ForgeController's public event payload; it does
    /// not call back into forge rules or change their ordering.
    ///
    /// Lives on the Player GameObject alongside ForgeController, WeaponHolder, WeaponInventory.
    /// Runs entirely on unscaled time (WaitForSecondsRealtime / Time.unscaledDeltaTime) per
    /// docs/TECHNICAL_DESIGN.md §5.4 gotcha #1 — GameManager.cs:77,82 is the existing precedent —
    /// so the sequence still advances correctly even if it is ever triggered while ForgePanel
    /// ("ForgePanel.cs:83-86) has Time.timeScale = 0f. In the current flow ForgePanel closes
    /// (and unpauses) immediately after a successful Forge/Upgrade call, before this sequence's
    /// audio/bloom beats fire — but the AudioSource below still sets ignoreListenerPause = true
    /// (docs/TECHNICAL_DESIGN.md §5.4 gotcha #2, precedent CutscenePlayer.cs:520-523) as a
    /// defensive measure against that ordering ever changing.
    ///
    /// 2026-08-20 feel pass: the anticipation/transformation beats previously had zero particle
    /// VFX of their own — "the raw object gathers" was purely a transform tween on the ghost
    /// mesh (see SpawnAnticipationGhost/TransformationBeat below), with no spark/energy effect
    /// at all. A fully-authored burst prefab (Assets/_Project/Prefabs/VFX/pfb_forge_vfx.prefab,
    /// gold sparks, negative gravity so they rise, single 60-particle burst, self-destroying via
    /// its own stopAction) already existed but was wired to nothing in this file or any scene —
    /// this pass wires it into both beats (see _anticipationVfxPrefab/_transformationVfxPrefab)
    /// and fixes the same opaque-material/undersized-particle bug B24 already found once for
    /// WeaponTierGlow (pfb_forge_vfx's MAT_Spark.mat was still _Surface:0/Opaque, and its
    /// particles were sized for a close-up shot, not this project's actual pulled-back gameplay
    /// camera). Also widened every beat duration below — 0.35s/0.5s beats with nothing to look
    /// at read as "nothing happened," per owner feedback after playing the real flow.
    /// </summary>
    [RequireComponent(typeof(ForgeController))]
    public class ForgePresenter : MonoBehaviour
    {
        private const string ImaginationVolumeName = "ImaginationRestore_Volume";

        /// <summary>Where the whole beat (ghost, VFX, reveal text) is anchored in world space.</summary>
        private enum PresentationAnchor
        {
            /// <summary>Original hand-height staging (WeaponHolder.MuzzlePosition).</summary>
            WeaponMuzzle,
            /// <summary>Above the character's head — easier to read at the pulled-back gameplay
            /// camera distance/yaw this project landed on (B27/B59). Owner's 2026-08-20 ask.</summary>
            AboveHead
        }

        [Header("Anchors (optional — resolved from sibling components if left empty)")]
        [SerializeField] private WeaponHolder    _weaponHolder;
        [SerializeField] private WeaponInventory _weaponInventory;

        [Header("Presentation anchor")]
        [Tooltip("AboveHead reads more clearly at real gameplay camera distance than the original hand-height WeaponMuzzle anchor — flip this back to WeaponMuzzle to compare the two directly, no other change needed.")]
        [SerializeField] private PresentationAnchor _presentationAnchor = PresentationAnchor.AboveHead;
        [Tooltip("Height above the player's feet (transform.position.y) used by the AboveHead anchor. pfb_player's CharacterController is 1.8m tall, so this sits a bit clear of the head.")]
        [SerializeField] private float _aboveHeadHeight = 2.1f;

        [Header("Anticipation")]
        [Tooltip("Household object rises briefly before becoming the weapon. Uses WeaponObjectSO.rawObjectPrefab; skipped (timing still holds) if that field is unassigned.")]
        [SerializeField] private float _anticipationDuration = 0.8f;
        [SerializeField] private float _anticipationRiseHeight = 0.4f;
        [SerializeField] private float _anticipationSpinDegreesPerSecond = 180f;
        [SerializeField] private float _anticipationGhostScale = 1f;
        [Tooltip("Particle burst played at the anchor when the anticipation beat starts (the 'gathering energy' read). Self-destroys via its own ParticleSystem.stopAction — no manual cleanup needed. Leave unassigned to skip.")]
        [SerializeField] private ParticleSystem _anticipationVfxPrefab;

        [Header("Transformation")]
        [SerializeField] private float _transformDuration = 0.9f;
        [SerializeField] private float _transformSpinDegrees = 360f;
        [Tooltip("Particle burst played at the anchor when the transformation beat starts (the 'reveal' pop). Same prefab as anticipation by default, but kept as a separate field so the two beats can diverge later. Leave unassigned to skip.")]
        [SerializeField] private ParticleSystem _transformationVfxPrefab;

        [Header("Colour Bloom (drives the ImaginationRestore volume by name, if present in-scene)")]
        [SerializeField] private Volume _imaginationVolume;
        [SerializeField] private float _bloomFadeInDuration = 0.35f;
        [SerializeField] private float _bloomHoldDuration = 0.5f;
        [SerializeField] private float _bloomFadeOutDuration = 0.7f;
        [SerializeField] private float _bloomWeightStandard = 0.25f;
        [SerializeField] private float _bloomWeightEpic = 0.55f;
        [SerializeField] private float _bloomWeightLegendary = 0.9f;

        [Header("Impact")]
        [Tooltip("Legendary tier uses HitStopManager's heavy beat/impulse; Standard and Epic use the light one.")]
        [SerializeField] private bool _useHeavyImpactForLegendary = true;

        [Header("Audio")]
        [SerializeField] private AudioClip _forgeRevealSfx;
        [SerializeField] private AudioClip _upgradeRevealSfx;
        [SerializeField] private float _sfxVolume = 0.85f;

        [Header("Imaginative Text Reveal")]
        [Tooltip("A world-space TextMeshPro (3D) prefab. If left unassigned, the rest of the sequence still plays — only the text beat is skipped.")]
        [SerializeField] private TextMeshPro _revealTextPrefab;
        [Tooltip("Extra offset added on top of whichever PresentationAnchor is active. Was tuned at 1.4 for the old hand-height WeaponMuzzle anchor (~1m) to land text near head height; lowered to 0.4 now that AboveHead (2.1m) is the default anchor, so text doesn't end up floating well above the character.")]
        [SerializeField] private Vector3 _revealTextOffset = new Vector3(0f, 0.4f, 0f);
        [SerializeField] private float _revealHoldDuration = 1.3f;
        [SerializeField] private float _revealFadeOutDuration = 0.4f;
        [Tooltip("Placeholder copy, not final narrative text — content/writer decision. {0}=raw object name, {1}=forged weapon name.")]
        [SerializeField, TextArea(1, 2)] private string _forgeRevealFormat = "{0}\n<size=140%><b>{1}</b></size>";
        [Tooltip("Placeholder copy, not final narrative text — content/writer decision. {0}=weapon name, {1}=new tier.")]
        [SerializeField, TextArea(1, 2)] private string _upgradeRevealFormat = "<b>{0}</b>\n<size=120%>{1}</size>";

        [Header("Upgrade pacing")]
        [Tooltip("Upgrade beats should feel like an escalation of the same object, not an identical first-time reveal — scales every timed duration above. Raised from 0.6 (2026-08-20): at the old anticipation/transform durations, 0.6x made upgrade beats sub-0.3s and unreadable on top of the base 'too fast' feedback.")]
        [SerializeField] private float _upgradeDurationScale = 0.75f;

        private ForgeController  _forgeController;
        private AudioSource      _audioSource;
        private Coroutine        _activeSequence;
        private Coroutine        _colorBloomSequence;

        // Tracks the weapon transform (and its target end-state) currently hidden/mid-tween by
        // the active sequence, so an interruption (a second forge/upgrade landing before this
        // one finishes, or this component being disabled mid-sequence) can always restore it
        // rather than leaving the equipped weapon's visible scale permanently at zero (B34).
        private Transform  _pendingWeaponTransform;
        private Vector3    _pendingWeaponFinalScale = Vector3.one;
        private Quaternion _pendingWeaponFinalRotation = Quaternion.identity;

        // Guards the GameObject.Find(ImaginationVolumeName) lookup below so a scene missing the
        // volume only pays for that scene-wide search once per session, not once per forge (B37).
        private bool _imaginationVolumeSearchAttempted;

        private void Awake()
        {
            _forgeController = GetComponent<ForgeController>();
            if (_weaponHolder == null) TryGetComponent(out _weaponHolder);
            if (_weaponInventory == null) TryGetComponent(out _weaponInventory);

            if (!TryGetComponent(out _audioSource))
                _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            // See gotcha #2 in the class doc — audible even if AudioListener.pause is still set.
            _audioSource.ignoreListenerPause = true;
        }

        private void OnEnable()
        {
            if (_forgeController != null)
            {
                _forgeController.OnWeaponForged   += HandleForged;
                _forgeController.OnWeaponUpgraded += HandleUpgraded;
            }
        }

        private void OnDisable()
        {
            if (_forgeController != null)
            {
                _forgeController.OnWeaponForged   -= HandleForged;
                _forgeController.OnWeaponUpgraded -= HandleUpgraded;
            }

            CancelActiveSequence();
        }

        private void HandleForged(WeaponInstance instance)   => Dispatch(instance, isUpgrade: false);
        private void HandleUpgraded(WeaponInstance instance) => Dispatch(instance, isUpgrade: true);

        private void Dispatch(WeaponInstance instance, bool isUpgrade)
        {
            if (instance == null) return;
            CancelActiveSequence();
            _activeSequence = StartCoroutine(PlaySequence(instance, isUpgrade));
        }

        /// <summary>
        /// Stops the in-flight sequence (if any) and guarantees the state it was mid-tweening
        /// is left in a valid, visible end-state rather than wherever the coroutine happened to
        /// be interrupted (B34). Called both when a new forge/upgrade interrupts a previous one
        /// and from OnDisable, so a disabled/destroyed player object can never leave the
        /// equipped weapon's scale stuck at zero or the imagination-restore bloom stuck on.
        /// </summary>
        private void CancelActiveSequence()
        {
            if (_activeSequence != null)
            {
                StopCoroutine(_activeSequence);
                _activeSequence = null;
            }

            if (_colorBloomSequence != null)
            {
                StopCoroutine(_colorBloomSequence);
                _colorBloomSequence = null;
                if (_imaginationVolume != null) _imaginationVolume.weight = 0f;
            }

            if (_pendingWeaponTransform != null)
            {
                _pendingWeaponTransform.localScale    = _pendingWeaponFinalScale;
                _pendingWeaponTransform.localRotation = _pendingWeaponFinalRotation;
                _pendingWeaponTransform = null;
            }
        }

        /// <summary>
        /// World-space point the whole beat plays around. AboveHead is computed relative to
        /// this GameObject's own transform (the player root, since ForgePresenter lives on
        /// pfb_player) rather than any bone lookup — deliberately simple so the two anchor modes
        /// are a one-field comparison, not a rewrite the next time this needs tuning.
        /// </summary>
        private Vector3 ResolveAnchor()
        {
            if (_presentationAnchor == PresentationAnchor.AboveHead)
                return transform.position + Vector3.up * _aboveHeadHeight;
            return _weaponHolder != null ? _weaponHolder.MuzzlePosition : transform.position;
        }

        /// <summary>
        /// Spawns a one-shot particle burst at the anchor. No pooling/tracking needed: forge
        /// frequency is low (player-initiated, not a hot path) and pfb_forge_vfx's own
        /// ParticleSystem.stopAction is Destroy, so the instance removes itself once its single
        /// burst finishes playing — this call is fire-and-forget by design.
        /// Rotated so local +Z (the shape module's emit direction) points world-up, matching the
        /// prefab's authored negative gravityModifier (sparks drift upward) regardless of which
        /// PresentationAnchor is active.
        /// </summary>
        private static void SpawnBeatVfx(ParticleSystem prefab, Vector3 anchor)
        {
            if (prefab == null) return;
            Instantiate(prefab, anchor, Quaternion.Euler(-90f, 0f, 0f));
        }

        private IEnumerator PlaySequence(WeaponInstance instance, bool isUpgrade)
        {
            // Only animate the held-weapon transform when this instance is actually the one
            // equipped right now. TryForge/TryUpgrade can land in (or upgrade) a slot that
            // is not the active slot — WeaponHolder's currently-held mesh is then a different,
            // unrelated weapon, and must not be hidden/tweened. The rest of the beat (ghost,
            // bloom, impact, text) still plays either way so a background-slot forge/upgrade
            // still reads as an event, just without a held-weapon reveal.
            bool isActiveWeapon = _weaponInventory != null
                && ReferenceEquals(_weaponInventory.ActiveWeapon, instance);

            Transform weaponT = (isActiveWeapon && _weaponHolder != null)
                ? _weaponHolder.WeaponVisualTransform
                : null;

            float durationScale = isUpgrade ? _upgradeDurationScale : 1f;
            Vector3 anchor = ResolveAnchor();

            Vector3 finalScale = Vector3.one;
            Quaternion finalRot = Quaternion.identity;
            if (weaponT != null)
            {
                finalScale = weaponT.localScale;
                finalRot   = weaponT.localRotation;

                // Track this transform + its target end-state for the duration of the beat, so
                // CancelActiveSequence() can always restore it if this coroutine is interrupted
                // anywhere between here and the end of TransformationBeat below (B34) — including
                // a second forge/upgrade landing before this one finishes, or this component
                // being disabled mid-sequence.
                _pendingWeaponTransform      = weaponT;
                _pendingWeaponFinalScale     = finalScale;
                _pendingWeaponFinalRotation  = finalRot;

                // Hide instantly, before this frame renders, so the transformation beat below
                // reads as a reveal rather than an instant pop-in of the already-attached mesh.
                weaponT.localScale = Vector3.zero;
            }

            GameObject ghost = SpawnAnticipationGhost(instance.Data.rawObjectPrefab, anchor);
            SpawnBeatVfx(_anticipationVfxPrefab, anchor);
            yield return AnticipationBeat(ghost, anchor, durationScale);
            if (ghost != null) Destroy(ghost);

            SpawnBeatVfx(_transformationVfxPrefab, anchor);
            if (weaponT != null)
                yield return TransformationBeat(weaponT, finalScale, finalRot, durationScale);

            // weaponT (if any) is now sitting at its correct final scale/rotation — no longer
            // needs a guaranteed-restore safety net for the rest of this sequence.
            _pendingWeaponTransform = null;

            _colorBloomSequence = StartCoroutine(ColorBloomBeat(instance.Tier, durationScale));
            FireImpactBeat(instance.Tier);
            PlayRevealAudio(isUpgrade);

            yield return RevealText(instance, isUpgrade, anchor, durationScale);

            _activeSequence = null;
        }

        // ── Anticipation ─────────────────────────────────────────────────────

        private GameObject SpawnAnticipationGhost(GameObject rawObjectPrefab, Vector3 anchor)
        {
            if (rawObjectPrefab == null) return null;
            GameObject ghost = Instantiate(rawObjectPrefab, anchor, Quaternion.identity);
            ghost.transform.localScale *= _anticipationGhostScale;
            return ghost;
        }

        private IEnumerator AnticipationBeat(GameObject ghost, Vector3 anchor, float durationScale)
        {
            float duration = Mathf.Max(0.01f, _anticipationDuration * durationScale);
            Vector3 start = anchor;
            Vector3 end   = anchor + Vector3.up * _anticipationRiseHeight;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (ghost != null)
                {
                    float t = Mathf.Clamp01(elapsed / duration);
                    ghost.transform.position = Vector3.Lerp(start, end, t);
                    ghost.transform.Rotate(Vector3.up, _anticipationSpinDegreesPerSecond * Time.unscaledDeltaTime, Space.World);
                }
                yield return null;
            }
        }

        // ── Transformation ───────────────────────────────────────────────────

        private IEnumerator TransformationBeat(Transform weaponT, Vector3 finalScale, Quaternion finalRot, float durationScale)
        {
            float duration = Mathf.Max(0.01f, _transformDuration * durationScale);
            Quaternion startRot = finalRot * Quaternion.Euler(0f, -_transformSpinDegrees, 0f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (weaponT == null) yield break; // weapon re-equipped/destroyed mid-beat
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t * (3f - 2f * t); // smoothstep
                weaponT.localScale    = finalScale * eased;
                weaponT.localRotation = Quaternion.Slerp(startRot, finalRot, eased);
                yield return null;
            }

            if (weaponT != null)
            {
                weaponT.localScale    = finalScale;
                weaponT.localRotation = finalRot;
            }
        }

        // ── Colour bloom ─────────────────────────────────────────────────────

        private IEnumerator ColorBloomBeat(WeaponTier tier, float durationScale)
        {
            if (_imaginationVolume == null && !_imaginationVolumeSearchAttempted)
            {
                // Only ever search once per session — a scene missing the volume would otherwise
                // repeat this GameObject.Find on every single forge/upgrade for the rest of the
                // run (B37), a standing violation of the project's own no-runtime-Find-in-hot-paths
                // convention even though forge frequency itself is low.
                _imaginationVolumeSearchAttempted = true;
                _imaginationVolume = GameObject.Find(ImaginationVolumeName)?.GetComponent<Volume>();
            }
            if (_imaginationVolume == null)
            {
                // No ImaginationRestore volume in this scene — skip gracefully.
                _colorBloomSequence = null;
                yield break;
            }

            float target = tier switch
            {
                WeaponTier.Epic      => _bloomWeightEpic,
                WeaponTier.Legendary => _bloomWeightLegendary,
                _                    => _bloomWeightStandard
            };

            yield return LerpVolumeWeight(0f, target, _bloomFadeInDuration * durationScale);
            yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, _bloomHoldDuration * durationScale));
            yield return LerpVolumeWeight(target, 0f, _bloomFadeOutDuration * durationScale);

            // Finished naturally (weight is already back at 0) — no longer needs
            // CancelActiveSequence() to intervene on its behalf.
            _colorBloomSequence = null;
        }

        private IEnumerator LerpVolumeWeight(float from, float to, float duration)
        {
            if (_imaginationVolume == null) yield break;
            if (duration <= 0f) { _imaginationVolume.weight = to; yield break; }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (_imaginationVolume == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                _imaginationVolume.weight = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            if (_imaginationVolume != null) _imaginationVolume.weight = to;
        }

        // ── Impact ────────────────────────────────────────────────────────────

        private void FireImpactBeat(WeaponTier tier)
        {
            if (HitStopManager.Instance == null) return;

            if (_useHeavyImpactForLegendary && tier == WeaponTier.Legendary)
                HitStopManager.Instance.TriggerHeavyHitStop(null);
            else
                HitStopManager.Instance.TriggerHitStop(null);
        }

        private void PlayRevealAudio(bool isUpgrade)
        {
            AudioClip clip = isUpgrade ? _upgradeRevealSfx : _forgeRevealSfx;
            if (clip == null || _audioSource == null) return;
            _audioSource.PlayOneShot(clip, _sfxVolume);
        }

        // ── Imaginative text reveal ──────────────────────────────────────────

        private IEnumerator RevealText(WeaponInstance instance, bool isUpgrade, Vector3 anchor, float durationScale)
        {
            if (_revealTextPrefab == null) yield break; // presenter still runs the rest of the beat without it

            TextMeshPro label = Instantiate(_revealTextPrefab, anchor + _revealTextOffset, Quaternion.identity);
            label.text = isUpgrade
                ? string.Format(_upgradeRevealFormat, instance.Data.weaponName, instance.Tier)
                : string.Format(_forgeRevealFormat, instance.Data.rawObjectName, instance.Data.weaponName);

            Camera cam = Camera.main;
            Color baseColor = label.color;
            float hold = Mathf.Max(0f, _revealHoldDuration * durationScale);
            float fade = Mathf.Max(0.01f, _revealFadeOutDuration * durationScale);

            float elapsed = 0f;
            while (elapsed < hold)
            {
                elapsed += Time.unscaledDeltaTime;
                if (cam != null) label.transform.rotation = cam.transform.rotation;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < fade)
            {
                elapsed += Time.unscaledDeltaTime;
                if (cam != null) label.transform.rotation = cam.transform.rotation;
                label.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(baseColor.a, 0f, elapsed / fade));
                yield return null;
            }

            Destroy(label.gameObject);
        }
    }
}
