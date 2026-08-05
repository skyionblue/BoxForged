using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Abstract ScriptableObject base for complex abilities that require custom per-frame or
    /// multi-step logic beyond what the data-driven inline switch in AbilityExecutor can express.
    /// Phase 1 only ships the base — concrete implementations arrive in Phase 3.
    /// </summary>
    public abstract class AbilityBehaviour : ScriptableObject
    {
        public abstract void Execute(AbilityExecutionContext ctx);
        public virtual void OnEquipped(AbilityExecutionContext ctx) { }
        public virtual void OnUnequipped() { }
    }
}
