using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Activates or deactivates VFX child GameObjects based on the WeaponRarity
    /// declared on a WeaponObjectSO. Attach to the pickup prefab root alongside
    /// WeaponPickup. All state is resolved once in Awake — zero per-frame cost.
    /// </summary>
    public class RarityIndicator : MonoBehaviour
    {
        [SerializeField] private WeaponObjectSO _weaponObject;

        [Tooltip("Gold shimmer particle child — shown for Rare weapons.")]
        [SerializeField] private GameObject _rareVFX;

        [Tooltip("Aura + float particle child — shown for Legendary weapons.")]
        [SerializeField] private GameObject _legendaryVFX;

        private void Awake()
        {
            if (_rareVFX != null)
                _rareVFX.SetActive(_weaponObject != null && _weaponObject.rarity == WeaponRarity.Rare);

            if (_legendaryVFX != null)
                _legendaryVFX.SetActive(_weaponObject != null && _weaponObject.rarity == WeaponRarity.Legendary);
        }
    }
}
