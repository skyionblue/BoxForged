namespace Boxhead.Systems
{
    public readonly struct AbilityActivationContext
    {
        public readonly UnityEngine.Vector3 PlayerPosition;
        public readonly UnityEngine.Vector3 PlayerForward;
        /// <summary>World position of the weapon's barrel tip (MuzzlePoint child transform).
        /// Falls back to a rough estimate in front of the player when no MuzzlePoint exists.</summary>
        public readonly UnityEngine.Vector3 MuzzlePosition;
        public readonly bool IsCounterWindow;
        public readonly UnityEngine.GameObject LastAttacker;   // may be null
        public readonly UnityEngine.LayerMask EnemyLayerMask;

        public AbilityActivationContext(
            UnityEngine.Vector3 position,
            UnityEngine.Vector3 forward,
            UnityEngine.Vector3 muzzlePosition,
            bool isCounterWindow,
            UnityEngine.GameObject lastAttacker,
            UnityEngine.LayerMask enemyLayerMask)
        {
            PlayerPosition  = position;
            PlayerForward   = forward;
            MuzzlePosition  = muzzlePosition;
            IsCounterWindow = isCounterWindow;
            LastAttacker    = lastAttacker;
            EnemyLayerMask  = enemyLayerMask;
        }
    }
}
