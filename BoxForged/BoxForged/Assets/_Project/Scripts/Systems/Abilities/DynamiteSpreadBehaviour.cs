using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Dynamite Legendary — "It Spreads".
    /// Passive ability. When equipped, sets a static flag that DynamiteProjectile reads
    /// during its Detonate() pass. Any CardboardPickup within twice the detonation radius
    /// of an explosion is destroyed and a VFX copy of the explosion is spawned at its position.
    ///
    /// The static flag is intentional: DynamiteProjectile is a pooled/spawned object with no
    /// direct reference to this ability, and polling an AbilityExecutor component from a
    /// projectile would couple two independent systems. The flag is cleared on unequip or
    /// scene unload (OnUnequipped), so it cannot leak across sessions.
    /// </summary>
    [CreateAssetMenu(fileName = "BHV_DynamiteSpread",
                     menuName = "Boxhead/Abilities/DynamiteSpreadBehaviour")]
    public class DynamiteSpreadBehaviour : AbilityBehaviour
    {
        /// <summary>
        /// True while the Dynamite Legendary is equipped. Read by DynamiteProjectile.Detonate()
        /// to trigger the cardboard chain-reaction pass.
        /// </summary>
        public static bool SpreadActive { get; private set; }

        public override void OnEquipped(AbilityExecutionContext ctx)
        {
            SpreadActive = true;
            Debug.Log("[DynamiteSpreadBehaviour] It Spreads: active.");
        }

        public override void OnUnequipped()
        {
            SpreadActive = false;
            Debug.Log("[DynamiteSpreadBehaviour] It Spreads: deactivated.");
        }

        /// <summary>
        /// Execute is not called for Passive abilities — this override is a safety no-op.
        /// OnEquipped / OnUnequipped are the only entry points for this behaviour.
        /// </summary>
        public override void Execute(AbilityExecutionContext ctx) { }
    }
}
