using System;
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Systems
{
    /// <summary>
    /// Quickdraw Epic — "The First Strike".
    /// Tracks whether the player has landed a hit yet in the current combat encounter.
    /// The very first OnHit after equip (or after each reset) applies a crit multiplier.
    /// Resets when the player takes damage (OnPlayerStaggered) — treated as a new encounter.
    /// </summary>
    [CreateAssetMenu(fileName = "TheFirstStrikeBehaviour",
                     menuName = "Boxhead/Abilities/Behaviours/TheFirstStrike")]
    public class TheFirstStrikeBehaviour : AbilityBehaviour
    {
        [SerializeField] private float _critMultiplier = 2f;

        private bool _firstHitReady = true;

        // Cached delegate references — never create closures in OnEquipped/OnUnequipped.
        private Action _onStaggered;
        private CombatController _cachedCombat;

        public override void OnEquipped(AbilityExecutionContext ctx)
        {
            _firstHitReady = true;
            _cachedCombat  = ctx.Combat;

            if (_onStaggered == null)
                _onStaggered = OnPlayerStaggered;

            if (_cachedCombat != null)
                _cachedCombat.OnPlayerStaggered += _onStaggered;
        }

        public override void OnUnequipped()
        {
            if (_cachedCombat != null && _onStaggered != null)
                _cachedCombat.OnPlayerStaggered -= _onStaggered;

            _firstHitReady = true;
            _cachedCombat  = null;
        }

        public override void Execute(AbilityExecutionContext ctx)
        {
            if (!_firstHitReady) return;
            _firstHitReady = false;
            ctx.Combat.SetNextHitCritMultiplier(_critMultiplier);
        }

        private void OnPlayerStaggered()
        {
            // Taking a hit resets the streak — next hit is a "first strike" again.
            _firstHitReady = true;
        }
    }
}
