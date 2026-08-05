using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Iron Standard Legendary — "Right Back".
    /// Fires on OnBlock trigger. After a successful parry, auto-triggers the counter strike
    /// without requiring the player to press the attack button during the counter window.
    /// Uses CombatController.TriggerCounterStrike — the public wrapper around the private
    /// ExecuteCounterStrike. Only fires if the player is still in Countering state when
    /// this Execute runs (CombatController guard handles the no-op if not in window).
    /// </summary>
    [CreateAssetMenu(fileName = "RightBackBehaviour",
                     menuName = "Boxhead/Abilities/Behaviours/RightBack")]
    public class RightBackBehaviour : AbilityBehaviour
    {
        public override void Execute(AbilityExecutionContext ctx)
        {
            // The OnParrySuccess event fires before the counter window state is fully entered
            // (ParryRoutine -> TryReceiveAttack -> CounterWindowRoutine). We invoke immediately —
            // CombatController.TriggerCounterStrike guards against non-Countering states.
            ctx.Combat.TriggerCounterStrike();
        }
    }
}
