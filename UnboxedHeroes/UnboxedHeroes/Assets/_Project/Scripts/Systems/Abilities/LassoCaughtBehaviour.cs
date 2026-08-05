using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    /// <summary>
    /// Lasso Epic — "Caught".
    /// OnHit trigger. Roots the nearest enemy within 4m for magnitude seconds (1.5f).
    /// Uses IEnemyBehavior.SetSpeedMultiplier(0f, duration) — BasicEnemyAI's implementation
    /// handles the timed restoration internally, so no coroutine is needed here.
    /// </summary>
    [CreateAssetMenu(fileName = "BHV_LassoCaught",
                     menuName = "Boxhead/Abilities/LassoCaughtBehaviour")]
    public class LassoCaughtBehaviour : AbilityBehaviour
    {
        [SerializeField] private float _searchRadius = 4f;

        // Pre-allocated buffer — zero GC in Execute.
        private readonly Collider[] _buffer = new Collider[8];

        public override void Execute(AbilityExecutionContext ctx)
        {
            // magnitude on the SO is the root duration in seconds.
            float duration = ctx.ActiveWeapon?.Data.epicAbility?.magnitude ?? 1.5f;

            int count = Physics.OverlapSphereNonAlloc(
                ctx.PlayerPosition, _searchRadius, _buffer, ctx.EnemyLayer);

            IEnemyBehavior nearest = null;
            float nearestSqDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                if (_buffer[i].TryGetComponent<EnemyStats>(out var stats) && stats.IsDead) continue;
                if (!_buffer[i].TryGetComponent<IEnemyBehavior>(out var behavior)) continue;

                float sq = (_buffer[i].transform.position - ctx.PlayerPosition).sqrMagnitude;
                if (sq >= nearestSqDist) continue;

                nearestSqDist = sq;
                nearest = behavior;
            }

            if (nearest == null) return;

            // SetSpeedMultiplier(0f, duration) zeroes movement and restores it after duration
            // seconds via BasicEnemyAI's internal coroutine — no external coroutine needed.
            nearest.SetSpeedMultiplier(0f, duration);

            Debug.Log($"[LassoCaughtBehaviour] Caught: enemy rooted for {duration}s.");
        }
    }
}
