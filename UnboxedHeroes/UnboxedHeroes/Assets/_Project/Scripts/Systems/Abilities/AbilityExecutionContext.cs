using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Systems
{
    /// <summary>
    /// Immutable value-type snapshot passed to AbilityBehaviour.Execute and BuildContext in
    /// AbilityExecutor. Avoids per-activation heap allocation; all fields are value types or
    /// managed references that already exist — no new objects are created at call time.
    /// </summary>
    public readonly struct AbilityExecutionContext
    {
        public readonly Vector3        PlayerPosition;
        public readonly Vector3        PlayerForward;
        public readonly Vector3        WeaponPosition;
        public readonly WeaponInstance ActiveWeapon;
        public readonly LayerMask      EnemyLayer;
        public readonly CombatController Combat;

        public AbilityExecutionContext(Vector3 pos, Vector3 fwd, Vector3 wpnPos,
            WeaponInstance weapon, LayerMask enemyLayer, CombatController combat)
        {
            PlayerPosition = pos;
            PlayerForward  = fwd;
            WeaponPosition = wpnPos;
            ActiveWeapon   = weapon;
            EnemyLayer     = enemyLayer;
            Combat         = combat;
        }
    }
}
