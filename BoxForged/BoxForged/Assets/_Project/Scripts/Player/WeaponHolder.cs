using System.Collections.Generic;
using UnityEngine;
using Boxhead.Systems;

namespace Boxhead.Player
{
    [RequireComponent(typeof(CombatController))]
    public class WeaponHolder : MonoBehaviour
    {
        /// <summary>
        /// Canonical weapon-attachment convention (B63 socket refactor, 2026-08-20): every
        /// playable character model must have a child GameObject named exactly
        /// "WeaponGripPoint" somewhere under its hand hierarchy (in practice, parented directly
        /// to the <see cref="WeaponHandBone"/> bone). FindHandBone() always prefers this socket
        /// over the raw bone name below.
        ///
        /// Why a socket instead of a bone-relative offset: two characters' hand bones can have
        /// completely different local rotation/scale conventions depending on how each rig was
        /// authored (see docs/PROJECT_CONTEXT.md "Model orientation correction" — never assume
        /// one rig's axis convention applies to another). A single shared WeaponData grip offset
        /// (gripPositionOffset/gripRotationOffset/gripScale) therefore cannot be correct for both
        /// characters' *raw bone* space at once — only one character can be "the one it was
        /// tuned against" at a time, and every other character's weapon ends up misplaced.
        ///
        /// The socket fixes this by absorbing all of the *character-specific* hand correction
        /// (its own local position/rotation, tuned once per character by hand — the same manual
        /// tuning that used to get redone per WeaponData asset) so that WeaponData's grip offset
        /// only has to express *weapon-specific* variance (a big vs. small prop) and can be
        /// shared across every character. See docs/BACKLOG.md for the checklist to apply this to
        /// a new character model (e.g. the upcoming Female Ninja / Cowgirl re-rigs).
        ///
        /// Retired 2026-08-20 (B63 follow-up): Cowboy and Ninja Male's
        /// <c>WeaponCycler.characterWeaponSets</c> entries and their 19-asset
        /// <c>Assets/_Project/ScriptableObjects/Weapons/{Cowboy,NinjaMale}/</c> variant folders
        /// have been deleted. Root cause of the recurring per-weapon misplacement bug this
        /// session: those variant assets' <c>gripPositionOffset</c>/<c>gripRotationOffset</c>
        /// were hand-tuned back when the socket was still identity — so once the socket itself
        /// carried a real correction, every one of those 19 offsets *doubled up* with it
        /// (verified directly: the bostaff and sixshooter swung down near the ankle/hip on both
        /// characters when equipped through the variant path, while the exact same shared/
        /// default WeaponData equipped through the same socket landed correctly — see B63 in
        /// docs/BACKLOG.md for the full numeric + screenshot evidence across all 19 weapons).
        /// Cowboy and Ninja Male now equip purely through <c>defaultWeapons</c>/shared
        /// WeaponData, same as Cowgirl always has. Ninja Female's variant folder and
        /// characterWeaponSets entry are intentionally left in place — she's getting a new
        /// model and will follow the checklist below when that lands, same as any future
        /// character. New characters going forward should not need a variant folder at all: an
        /// untuned/default WeaponData grip offset plus a correctly-placed socket is sufficient.
        /// </summary>
        private const string WeaponGripPointName = "WeaponGripPoint";
        private const string WeaponHandBone      = "LeftHand";

        [SerializeField] private WeaponData   defaultWeaponData;
        [SerializeField] private GameObject   weaponPrefab;
        [SerializeField] private GameObject[] weaponPool;
        [SerializeField] private Transform    handBone;

        // _weaponInstance is parented to handBone; Unity destroys it with this GO.
        private GameObject _weaponInstance;
        // Separates pool/EquipWeapon selection from the designer-assigned weaponPrefab field.
        private GameObject _activePrefab;
        // Last equipped WeaponData — used to re-attach after BoxSystem swaps the character model.
        private WeaponData _currentData;
        // Tier of the currently equipped WeaponInstance (Standard/Epic/Legendary). Tracked
        // alongside _currentData so a character-model swap (OnModelChanged) or a live grip
        // re-tune (ReapplyGrip) re-attaches the correct tier visual instead of silently
        // regressing a forged Epic/Legendary weapon back to its Standard mesh.
        private WeaponTier _currentTier = WeaponTier.Standard;
        // No per-instance material tracking needed — sharedMaterials assignment uses asset
        // references directly; Unity destroys renderer materials with the weapon instance.

        private BoxSystem        _boxSystem;
        private CombatController _combatController;
        private WeaponEquipController _weaponEquipController;
        // Source of truth for the forged tier of whatever is currently equipped (see B41). May
        // be null on prefabs that never carry a WeaponInventory (e.g. non-player humanoids using
        // WeaponHolder for a purely cosmetic held weapon).
        private WeaponInventory  _weaponInventory;
        private Animator _animator;
        private static readonly List<Transform> _boneSearchBuffer = new List<Transform>();

        public WeaponAbilityData CurrentAbility => _currentData?.ability;

        /// <summary>Tier of the currently equipped weapon (Standard unless equipped via the forge/upgrade tier-aware overload).</summary>
        public WeaponTier CurrentTier => _currentTier;

        /// <summary>The live instantiated weapon GameObject's transform, or null if nothing is equipped.
        /// Exposed so presentation code (e.g. ForgePresenter) can animate the held weapon directly
        /// without reaching into WeaponHolder's instantiate/destroy lifecycle.</summary>
        public Transform WeaponVisualTransform => _weaponInstance != null ? _weaponInstance.transform : null;

        /// <summary>World position of the barrel tip. Uses WeaponData.muzzleLocalOffset
        /// transformed into world space by the live weapon instance. Falls back to the
        /// hand bone position when no weapon is equipped.</summary>
        public UnityEngine.Vector3 MuzzlePosition
        {
            get
            {
                if (_weaponInstance != null && _currentData != null)
                    return _weaponInstance.transform.TransformPoint(_currentData.muzzleLocalOffset);
                return handBone != null ? handBone.position : transform.position;
            }
        }

        private void Awake()
        {
            _boxSystem        = GetComponent<BoxSystem>();
            _combatController = GetComponent<CombatController>();
            _weaponEquipController = GetComponent<WeaponEquipController>();
            TryGetComponent(out _weaponInventory);
            _animator = GetComponentInChildren<Animator>();

            if (weaponPool != null && weaponPool.Length > 0)
                _activePrefab = weaponPool[Random.Range(0, weaponPool.Length)];
            else
                _activePrefab = weaponPrefab;

            // handBone may not be available yet if BoxSystem spawns the model after Awake.
            // FindDeep here catches the case where the model is already a child at Awake time.
            if (handBone == null)
                handBone = FindHandBone();
        }

        private void Start()
        {
            if (_boxSystem != null)
                _boxSystem.OnModelChanged += OnModelChanged;

            // Try again in Start — BoxSystem may have spawned the model between Awake and Start.
            if (handBone == null)
                handBone = FindHandBone();

            // Skip the default equip if a weapon was already restored by ProgressionSystem
            // before this Start() ran (execution order is not guaranteed between GameManager
            // and WeaponHolder). If _currentData is non-null, EquipWeapon was already called
            // by WeaponInventory.RestoreState — overwriting it here would discard the forged
            // loadout and replace it with the starting default every scene load.
            if (_currentData != null) return;

            // defaultWeaponData uses the full WeaponData grip values (position, rotation, scale).
            // weaponPrefab / weaponPool are legacy fallbacks that use hardcoded defaults.
            if (defaultWeaponData != null)
            {
                EquipWeapon(defaultWeaponData);
                return;
            }

            if (_activePrefab == null) return;

            if (handBone != null)
                Attach(_activePrefab, Vector3.zero, Vector3.zero, 0.35f);
        }

        private void OnDestroy()
        {
            if (_boxSystem != null)
                _boxSystem.OnModelChanged -= OnModelChanged;
        }

        // Called by BoxSystem when the character model is swapped.
        // Re-finds the hand bone in the new model and re-attaches the current weapon.
        private void OnModelChanged()
        {
            _animator = GetComponentInChildren<Animator>();
            handBone = FindHandBone();
            if (handBone == null) return;

            if (_currentData != null)
            {
                Attach(ResolveTierPrefab(_currentData, _currentTier), _currentData.gripPositionOffset, _currentData.gripRotationOffset, _currentData.gripScale, _currentTier);
                _combatController?.OnWeaponEquipped(_currentData.ability);
            }
            else if (defaultWeaponData != null)
            {
                // EquipWeapon calls OnWeaponEquipped internally — no second call needed.
                EquipWeapon(defaultWeaponData);
            }
            else if (_activePrefab != null)
            {
                Attach(_activePrefab, Vector3.zero, Vector3.zero, 0.35f);
                _combatController?.OnWeaponEquipped(null);
            }
        }

        // Searches active children for WeaponGripPoint first, then the named hand bone.
        // WeaponGripPoint is an empty GO added to each character's hand with its localScale
        // set so lossyScale == 1 — this normalises the per-character armature scale so
        // every WeaponData grip offset is expressed in the same world-scale units.
        private Transform FindHandBone()
        {
            _boneSearchBuffer.Clear();
            GetComponentsInChildren<Transform>(false, _boneSearchBuffer);
            Transform fallback = null;
            string namespacedHand = ":" + WeaponHandBone;
            for (int i = 0; i < _boneSearchBuffer.Count; i++)
            {
                string n = _boneSearchBuffer[i].name;
                if (n == WeaponGripPointName)                          return _boneSearchBuffer[i];
                if (fallback == null && (n == WeaponHandBone || n.EndsWith(namespacedHand)))
                    fallback = _boneSearchBuffer[i];
            }

            // Falling back to the raw hand bone means this character model has no
            // WeaponGripPoint socket yet — every WeaponData grip offset will be interpreted in
            // this bone's own local space, which is very unlikely to match whichever character
            // the shared offset was actually tuned against (see the class-level doc comment on
            // WeaponGripPointName). This is expected/harmless for non-humanoid or cosmetic-only
            // rigs that never equip a real weapon, but for a playable character it is almost
            // always a missed onboarding step — warn once per model-swap so it surfaces
            // immediately instead of shipping as a silently-misplaced weapon (see B63,
            // docs/BACKLOG.md).
            if (fallback != null)
                Debug.LogWarning($"[WeaponHolder] No '{WeaponGripPointName}' socket found under " +
                    $"'{gameObject.name}' — falling back to raw '{fallback.name}' bone. Add a " +
                    $"correctly-placed child Transform named '{WeaponGripPointName}' to this " +
                    "character's hand bone (see docs/BACKLOG.md for the checklist).", this);

            return fallback;
        }

        private Transform FindDeep(string boneName)
        {
            _boneSearchBuffer.Clear();
            // includeInactive=false — only search the currently active character model.
            // Inactive models (swapped out by BoxSystem) must not be used as bone targets;
            // a weapon parented to an inactive Transform is invisible.
            GetComponentsInChildren<Transform>(false, _boneSearchBuffer);
            string namespacedName = ":" + boneName;
            for (int i = 0; i < _boneSearchBuffer.Count; i++)
            {
                string n = _boneSearchBuffer[i].name;
                if (n == boneName || n.EndsWith(namespacedName))
                    return _boneSearchBuffer[i];
            }
            return null;
        }

        private void Attach(GameObject prefab, Vector3 positionOffset, Vector3 rotationOffset, float scale, WeaponTier tier = WeaponTier.Standard)
        {

            // TODO: replace Destroy/Instantiate with an object pool before EquipWeapon is called from combat or inventory.
            if (_weaponInstance != null) Destroy(_weaponInstance);
            _weaponInstance = Instantiate(prefab, handBone);
            _weaponInstance.transform.localPosition = positionOffset;
            _weaponInstance.transform.localRotation = Quaternion.Euler(rotationOffset);
            // *= preserves the FBX fileScale compensation (root scale ~100).
            // Divide out the hand bone's world scale first so gripScale is always in world units,
            // regardless of whether the armature root is 1m-scale (Cowboy) or 0.01m-scale (Cowgirl FBX_SCALE_ALL).
            float boneWorldScale = handBone.lossyScale.x;
            if (!Mathf.Approximately(boneWorldScale, 0f))
                _weaponInstance.transform.localScale /= boneWorldScale;
            _weaponInstance.transform.localScale   *= scale;
            _animator?.SetInteger("WeaponType", 1);

            // Apply the WeaponData material to every renderer on the instantiated weapon.
            // _currentData is null when Start() pools the designer-assigned prefab — in that
            // case the prefab's existing material is left untouched (correct behaviour).
            if (_currentData != null && _currentData.material != null)
            {
                Renderer[] renderers = _weaponInstance.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    int slotCount = renderers[i].sharedMaterials.Length;
                    Material[] mats = new Material[slotCount];
                    for (int j = 0; j < slotCount; j++) mats[j] = _currentData.material;
                    renderers[i].sharedMaterials = mats;
                }
            }

            // Tier glow — no-op unless the attached prefab carries a WeaponTierGlow component
            // (an art/prefab-wiring step; existing weapon prefabs are unaffected until wired).
            var tierGlow = _weaponInstance.GetComponent<WeaponTierGlow>();
            if (tierGlow != null) tierGlow.SetTier(tier);
        }

        /// <summary>
        /// Resolves the tier-specific visual prefab for a WeaponObjectSO, falling back to the
        /// base weaponPrefab when data isn't a WeaponObjectSO, the tier is Standard, or no
        /// tier-specific prefab is assigned. This is the read path for
        /// WeaponObjectSO.epicWeaponPrefab / legendaryWeaponPrefab — declared for exactly this
        /// purpose but never read anywhere before this change.
        ///
        /// Scope note: when a WeaponObjectSO has baseEquippedData assigned, WeaponInventory
        /// resolves the equip data to that (V3-era) WeaponData asset via WeaponCycler before it
        /// ever reaches WeaponHolder — this method only sees the WeaponObjectSO itself when
        /// baseEquippedData is unset. Tier-prefab swapping therefore only applies to weapons on
        /// that direct path; character-variant-resolved weapons still get the persistent
        /// WeaponTierGlow (tier is threaded through regardless). Reconciling the V3/V4 weapon
        /// data split is tracked separately (docs/TECHNICAL_DESIGN.md §7.1) and out of scope here.
        /// </summary>
        private static GameObject ResolveTierPrefab(WeaponData data, WeaponTier tier)
        {
            var woso = data as WeaponObjectSO;
            if (woso == null) return data.weaponPrefab;

            return tier switch
            {
                WeaponTier.Epic      => woso.epicWeaponPrefab      != null ? woso.epicWeaponPrefab      : woso.weaponPrefab,
                WeaponTier.Legendary => woso.legendaryWeaponPrefab != null ? woso.legendaryWeaponPrefab : woso.weaponPrefab,
                _                    => woso.weaponPrefab
            };
        }

        /// <summary>
        /// Re-applies grip offset, rotation, and scale from the current WeaponData without
        /// destroying and re-instantiating the prefab. Call this during Play mode after
        /// tweaking gripScale / gripPositionOffset / gripRotationOffset in the Inspector.
        /// Right-click WeaponHolder in the Inspector → "Reapply Grip (Live Tuning)".
        /// </summary>
        [ContextMenu("Reapply Grip (Live Tuning)")]
        public void ReapplyGrip()
        {
            if (_currentData == null) return;
            if (handBone == null) handBone = FindHandBone();
            if (handBone == null) return;
            // Fully re-instantiate from the prefab so the FBX baked transforms are
            // reset cleanly before grip offsets are applied — patch-in-place doesn't
            // work reliably when the FBX hierarchy has its own baked child rotations.
            Attach(ResolveTierPrefab(_currentData, _currentTier),
                   _currentData.gripPositionOffset,
                   _currentData.gripRotationOffset,
                   _currentData.gripScale,
                   _currentTier);
        }

        /// <summary>
        /// Equip a weapon from WeaponData at Standard tier. Destroys the current weapon
        /// instance and instantiates the new prefab using grip offsets and scale from the
        /// ScriptableObject. Equivalent to EquipWeapon(data, WeaponTier.Standard).
        /// </summary>
        public void EquipWeapon(WeaponData data) => EquipWeapon(data, WeaponTier.Standard);

        /// <summary>
        /// Equip a weapon from WeaponData at the given tier. When data is a WeaponObjectSO and
        /// an Epic/Legendary-tier prefab is assigned (see ResolveTierPrefab), that tier visual
        /// is instantiated instead of the base weaponPrefab, and any WeaponTierGlow on the
        /// resulting instance is set to match — this is what makes a forged Epic/Legendary
        /// weapon look different while held, not just carry a higher tier value internally.
        ///
        /// <paramref name="tier"/> is reasserted from WeaponInventory.ActiveWeapon (the actual
        /// source of truth for what's equipped) when a sibling WeaponInventory exists and has an
        /// active weapon, rather than trusting whichever caller passed last (see B41). Legacy V3
        /// Boxhead.Systems.Inventory call sites (Equip/Swap/SetEquipped/Drop, and by extension
        /// WeaponCycler and BossRoomWeaponSpawner.ClearPlayerWeapons) always call the tier-less
        /// EquipWeapon(WeaponData) overload below, which defaults to WeaponTier.Standard — that
        /// silently regressed a forged Epic/Legendary weapon back to Standard every time one of
        /// those paths fired (concretely: every boss-room entry). WeaponInventory itself already
        /// passes the correct tier when it calls this overload directly, so reasserting here is a
        /// no-op on that path and only changes behaviour where the passed-in tier was wrong.
        /// </summary>
        public void EquipWeapon(WeaponData data, WeaponTier tier)
        {
            if (data == null)
            {
                Debug.LogWarning("[WeaponHolder] EquipWeapon called with null WeaponData.", this);
                return;
            }

            if (data.weaponPrefab == null)
            {
                Debug.LogWarning($"[WeaponHolder] WeaponData '{data.weaponName}' has no weaponPrefab assigned.", this);
                return;
            }

            if (_weaponInventory != null && _weaponInventory.ActiveWeapon != null)
                tier = _weaponInventory.ActiveWeapon.Tier;

            _activePrefab = data.weaponPrefab;
            _currentData  = data;
            _currentTier  = tier;

            // Re-search for hand bone — covers the case where it was null at Start
            // because BoxSystem hadn't spawned the model yet.
            if (handBone == null)
                handBone = FindHandBone();

            if (handBone != null)
                Attach(ResolveTierPrefab(data, tier), data.gripPositionOffset, data.gripRotationOffset, data.gripScale, tier);
            else
                Debug.LogWarning($"[WeaponHolder] Cannot equip weapon '{data.weaponName}' — handBone is null.", this);

            _animator?.SetInteger("WeaponType", 1);
            _combatController?.OnWeaponEquipped(data?.ability);
            _weaponEquipController?.EquipWeapon();
        }

        /// <summary>
        /// Removes the current weapon. Returns to unarmed state.
        /// </summary>
        public void UnequipCurrentWeapon()
        {
            if (_weaponInstance != null) Destroy(_weaponInstance);
            _weaponInstance = null;

            _currentData = null;
            _currentTier = WeaponTier.Standard;
            _activePrefab = null;

            _animator?.SetInteger("WeaponType", 0);
            _combatController?.OnWeaponEquipped(null);
            _weaponEquipController?.UnequipWeapon();
        }
    }
}
