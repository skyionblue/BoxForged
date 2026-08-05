using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Lightsaber Legendary — "Send It Back".
    /// A passive ability: on equip, activates the ProjectileDeflector flag so the next
    /// enemy projectile that hits the player is reversed instead of dealing damage.
    /// The flag self-clears after one deflection (handled in BossProjectile.OnTriggerEnter).
    /// Resets (re-arms) on each new room entry or on re-equip.
    /// </summary>
    [CreateAssetMenu(fileName = "SendItBackBehaviour",
                     menuName = "Boxhead/Abilities/Behaviours/SendItBack")]
    public class SendItBackBehaviour : AbilityBehaviour
    {
        public override void OnEquipped(AbilityExecutionContext ctx)
        {
            ProjectileDeflector.IsActive = true;
        }

        public override void OnUnequipped()
        {
            ProjectileDeflector.IsActive = false;
        }

        /// <summary>
        /// Execute fires on Passive trigger (equip). Re-arms the deflector in case it was
        /// consumed mid-session — allows it to reset when the player changes rooms.
        /// </summary>
        public override void Execute(AbilityExecutionContext ctx)
        {
            ProjectileDeflector.IsActive = true;
        }
    }
}
