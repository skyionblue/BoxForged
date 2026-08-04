using System.Collections.Generic;
using UnityEngine;
using Boxhead.Systems;

namespace Boxhead.Player
{
    [RequireComponent(typeof(CombatController))]
    public class WeaponHolder : MonoBehaviour
    {
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
        // No per-instance material tracking needed — sharedMaterials assignment uses asset
        // references directly; Unity destroys renderer materials with the weapon instance.

        private BoxSystem        _boxSystem;
        private CombatController _combatController;
        private WeaponEquipController _weaponEquipController;
        private Animator _animator;
        private static readonly List<Transform> _boneSearchBuffer = new List<Transform>();

        public WeaponAbilityData CurrentAbility => _currentData?.ability;

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
                Attach(_currentData.weaponPrefab, _currentData.gripPositionOffset, _currentData.gripRotationOffset, _currentData.gripScale);
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

        private void Attach(GameObject prefab, Vector3 positionOffset, Vector3 rotationOffset, float scale)
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
            Attach(_currentData.weaponPrefab,
                   _currentData.gripPositionOffset,
                   _currentData.gripRotationOffset,
                   _currentData.gripScale);
        }

        /// <summary>
        /// Equip a weapon from WeaponData. Destroys the current weapon instance and
        /// instantiates the new prefab using grip offsets and scale from the ScriptableObject.
        /// </summary>
        public void EquipWeapon(WeaponData data)
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

            _activePrefab = data.weaponPrefab;
            _currentData  = data;

            // Re-search for hand bone — covers the case where it was null at Start
            // because BoxSystem hadn't spawned the model yet.
            if (handBone == null)
                handBone = FindHandBone();

            if (handBone != null)
                Attach(data.weaponPrefab, data.gripPositionOffset, data.gripRotationOffset, data.gripScale);
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
            _activePrefab = null;

            _animator?.SetInteger("WeaponType", 0);
            _combatController?.OnWeaponEquipped(null);
            _weaponEquipController?.UnequipWeapon();
        }
    }
}
