using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    /// <summary>
    /// Magic Wand Epic — "Mixed Up".
    /// OnHit trigger. Confuses the nearest enemy within 4m, slowing it to 30% speed for
    /// magnitude seconds (3f). Simulates confusion: the slowed enemy stumbles rather than
    /// pursuing — visually reads as disorientation without requiring a separate targeting system.
    ///
    /// IEnemyBehavior.SetSpeedMultiplier(0.3f, duration) is used for consistency with
    /// LassoCaughtBehaviour and SoakedBehaviour. BasicEnemyAI's RestoreSpeedAfter coroutine
    /// handles timed restoration internally — no external coroutine needed here.
    /// </summary>
    [CreateAssetMenu(fileName = "BHV_MixedUp",
                     menuName = "Boxhead/Abilities/MixedUpBehaviour")]
    public class MixedUpBehaviour : AbilityBehaviour
    {
        [SerializeField] private float _searchRadius    = 4f;
        [SerializeField] private float _confusionSpeed  = 0.3f;   // 30% of base speed

        // Pre-allocated buffer — zero GC in Execute.
        private readonly Collider[] _buffer = new Collider[8];

        public override void Execute(AbilityExecutionContext ctx)
        {
            // magnitude on the SO is the confusion duration in seconds.
            float duration = ctx.ActiveWeapon?.Data.epicAbility?.magnitude ?? 3f;

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

            nearest.SetSpeedMultiplier(_confusionSpeed, duration);

            Debug.Log($"[MixedUpBehaviour] Mixed Up: enemy confused for {duration}s at {_confusionSpeed * 100f}% speed.");
        }
    }
}
