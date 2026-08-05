using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Iron Standard Epic — "Hold the Line".
    /// Fires on OnBlock trigger (successful parry). Triggers the attack animation to
    /// give the block a heavy, impactful visual response. The full counter-strike mechanic
    /// is handled by CombatController's existing counter window — this behaviour adds the
    /// visual confirmation that the block landed and hints at the counter opportunity.
    /// Full hold-block input mechanic deferred to Phase 3.
    /// </summary>
    [CreateAssetMenu(fileName = "HoldTheLineBehaviour",
                     menuName = "Boxhead/Abilities/Behaviours/HoldTheLine")]
    public class HoldTheLineBehaviour : AbilityBehaviour
    {
        public override void Execute(AbilityExecutionContext ctx)
        {
            // TriggerAttackAnimation is already called by AbilityExecutor.ExecuteAbility before
            // reaching this method. This behaviour exists as a hook for future Phase 3 logic
            // (e.g. a temporary shield raise, a damage reduction window, screen shake) without
            // requiring AbilityExecutor changes.
            // For now: the animation trigger from ExecuteAbility provides the block-impact feel.
            Debug.Log("[HoldTheLineBehaviour] Hold the Line activated — block confirmed.");
        }
    }
}
