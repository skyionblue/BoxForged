using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Toggles persistent glow VFX children based on a weapon's forged tier — the held-weapon
    /// equivalent of RarityIndicator's rarity-based child toggle. Attach to a weapon's
    /// "equipped" prefab alongside its Epic/Legendary glow child objects.
    ///
    /// Unlike RarityIndicator (which resolves once in Awake from a static WeaponObjectSO
    /// reference on a pickup prefab), a weapon's tier is only known at instantiation time —
    /// WeaponHolder destroys/re-instantiates the equipped prefab per equip, and the same
    /// prefab can be equipped at Standard, Epic, or Legendary tier depending on forge state.
    /// SetTier is therefore called explicitly by WeaponHolder.Attach right after Instantiate,
    /// rather than resolved internally.
    ///
    /// No-op if this component or its VFX children are not present on a given weapon prefab —
    /// existing weapon prefabs are unaffected until an art/prefab-wiring pass adds the glow
    /// children and this component.
    /// </summary>
    public class WeaponTierGlow : MonoBehaviour
    {
        [Tooltip("Faint glow child shown while an Epic-tier weapon is held.")]
        [SerializeField] private GameObject _epicGlowVFX;

        [Tooltip("Stronger glow/aura child shown while a Legendary-tier weapon is held.")]
        [SerializeField] private GameObject _legendaryGlowVFX;

        /// <summary>Activates the glow child matching the given tier and hides the other. Standard tier hides both.</summary>
        public void SetTier(WeaponTier tier)
        {
            if (_epicGlowVFX != null)
                _epicGlowVFX.SetActive(tier == WeaponTier.Epic);

            if (_legendaryGlowVFX != null)
                _legendaryGlowVFX.SetActive(tier == WeaponTier.Legendary);
        }
    }
}
