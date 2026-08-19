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
    /// </summary>
    [RequireComponent(typeof(ForgeController))]
    public class ForgePresenter : MonoBehaviour
    {
        private const string ImaginationVolumeName = "ImaginationRestore_Volume";

        [Header("Anchors (optional — resolved from sibling components if left empty)")]
        [SerializeField] private WeaponHolder    _weaponHolder;
        [SerializeField] private WeaponInventory _weaponInventory;

        [Header("Anticipation")]
        [Tooltip("Household object rises briefly before becoming the weapon. Uses WeaponObjectSO.rawObjectPrefab; skipped (timing still holds) if that field is unassigned.")]
        [SerializeField] private float _anticipationDuration = 0.35f;
        [SerializeField] private float _anticipationRiseHeight = 0.4f;
        [SerializeField] private float _anticipationSpinDegreesPerSecond = 180f;
        [SerializeField] private float _anticipationGhostScale = 1f;

        [Header("Transformation")]
        [SerializeField] private float _transformDuration = 0.5f;
        [SerializeField] private float _transformSpinDegrees = 360f;

        [Header("Colour Bloom (drives the ImaginationRestore volume by name, if present in-scene)")]
        [SerializeField] private Volume _imaginationVolume;
        [SerializeField] private float _bloomFadeInDuration = 0.25f;
        [SerializeField] private float _bloomHoldDuration = 0.4f;
        [SerializeField] private float _bloomFadeOutDuration = 0.6f;
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
        [SerializeField] private Vector3 _revealTextOffset = new Vector3(0f, 1.4f, 0f);
        [SerializeField] private float _revealHoldDuration = 1.1f;
        [SerializeField] private float _revealFadeOutDuration = 0.4f;
        [Tooltip("Placeholder copy, not final narrative text — content/writer decision. {0}=raw object name, {1}=forged weapon name.")]
        [SerializeField, TextArea(1, 2)] private string _forgeRevealFormat = "{0}\n<size=140%><b>{1}</b></size>";
        [Tooltip("Placeholder copy, not final narrative text — content/writer decision. {0}=weapon name, {1}=new tier.")]
        [SerializeField, TextArea(1, 2)] private string _upgradeRevealFormat = "<b>{0}</b>\n<size=120%>{1}</size>";

        [Header("Upgrade pacing")]
        [Tooltip("Upgrade beats should feel like an escalation of the same object, not an identical first-time reveal — scales every timed duration above.")]
        [SerializeField] private float _upgradeDurationScale = 0.6f;

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
            Vector3 anchor = _weaponHolder != null ? _weaponHolder.MuzzlePosition : transform.position;

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
            yield return AnticipationBeat(ghost, anchor, durationScale);
            if (ghost != null) Destroy(ghost);

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
