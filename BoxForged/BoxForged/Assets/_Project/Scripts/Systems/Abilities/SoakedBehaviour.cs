using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    /// <summary>
    /// Water Whip Legendary — "It Slows Them Down".
    /// On OnHit trigger, finds the nearest enemy within whip range and slows it to 70%
    /// speed for magnitude seconds (set to 3f on the AbilitySO).
    /// Uses IEnemyBehavior.SetSpeedMultiplier — works on any enemy implementing the interface.
    /// </summary>
    [CreateAssetMenu(fileName = "SoakedBehaviour",
                     menuName = "Boxhead/Abilities/Behaviours/Soaked")]
    public class SoakedBehaviour : AbilityBehaviour
    {
        [SerializeField] private float _slowMultiplier = 0.7f;

        // Pre-allocated buffer — zero GC in Execute.
        private readonly Collider[] _buffer = new Collider[8];

        public override void Execute(AbilityExecutionContext ctx)
        {
            // magnitude on the ability SO is the slow duration in seconds.
            float duration = ctx.ActiveWeapon?.Data.legendaryAbility?.magnitude ?? 3f;
            float radius   = 8f; // Water Whip range

            int count = Physics.OverlapSphereNonAlloc(
                ctx.PlayerPosition, radius, _buffer, ctx.EnemyLayer);

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

            nearest?.SetSpeedMultiplier(_slowMultiplier, duration);
        }
    }
}
