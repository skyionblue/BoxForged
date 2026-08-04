using System.Collections;
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Systems
{
    public abstract class WeaponAbilityData : ScriptableObject
    {
        [SerializeField] private float _cooldownDuration = 2f;
        public float CooldownDuration => _cooldownDuration;

        /// <summary>
        /// Readiness fraction 0–1. 1 = fully ready, 0 = just used / reloading.
        /// Default delegates to the CombatController cooldown timer via SpecialCooldownProgress.
        /// Override in abilities that manage their own timing (e.g. SixShooter per-shot reload).
        /// </summary>
        public virtual float ProgressFraction => -1f; // -1 = use CombatController fallback

        /// <summary>
        /// When true, this ability fires on the Attack button instead of the Special button.
        /// Ranged weapons (Six Shooter, Dynamite) set this to true.
        /// </summary>
        public virtual bool FiresOnAttackButton => false;

        /// <summary>
        /// Whether this ability is ready to activate. Default true.
        /// Override to block activation on external state — e.g. Dynamite blocks while a
        /// projectile is still in flight, regardless of the cooldown timer.
        /// </summary>
        public virtual bool IsReadyToActivate => true;

        public abstract IEnumerator Activate(AbilityActivationContext ctx, CombatController combat);
    }
}
