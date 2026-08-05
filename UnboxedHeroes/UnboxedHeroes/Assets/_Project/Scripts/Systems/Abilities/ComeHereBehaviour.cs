using UnityEngine;
using UnityEngine.AI;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    /// <summary>
    /// Water Whip Epic — "Come Here".
    /// On OnHit trigger, finds the nearest enemy within the whip range and pulls it
    /// 2 metres closer to the player. Uses NavMeshAgent.Warp when available (preserves
    /// nav mesh state); falls back to Rigidbody.AddForce for physics-driven enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "ComeHereBehaviour",
                     menuName = "Boxhead/Abilities/Behaviours/ComeHere")]
    public class ComeHereBehaviour : AbilityBehaviour
    {
        [SerializeField] private float _pullDistance = 2f;

        // Pre-allocated buffer — zero GC in Execute.
        private readonly Collider[] _buffer = new Collider[8];

        public override void Execute(AbilityExecutionContext ctx)
        {
            // magnitude on the ability SO doubles as the search radius (8m for Water Whip Epic).
            float radius = ctx.ActiveWeapon?.Data.epicAbility?.magnitude ?? 8f;

            int count = Physics.OverlapSphereNonAlloc(
                ctx.PlayerPosition, radius, _buffer, ctx.EnemyLayer);

            Transform nearestTransform = null;
            NavMeshAgent nearestAgent = null;
            Rigidbody nearestRb = null;
            float nearestSqDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                if (_buffer[i].TryGetComponent<EnemyStats>(out var stats) && stats.IsDead) continue;

                float sq = (_buffer[i].transform.position - ctx.PlayerPosition).sqrMagnitude;
                if (sq >= nearestSqDist) continue;

                nearestSqDist = sq;
                nearestTransform = _buffer[i].transform;
                _buffer[i].TryGetComponent(out nearestAgent);
                _buffer[i].TryGetComponent(out nearestRb);
            }

            if (nearestTransform == null) return;

            Vector3 toPlayer = (ctx.PlayerPosition - nearestTransform.position).normalized;
            Vector3 pulled = nearestTransform.position + toPlayer * _pullDistance;

            if (nearestAgent != null && nearestAgent.isOnNavMesh)
            {
                // Warp teleports the agent to the position without physics — stays on nav mesh.
                nearestAgent.Warp(pulled);
            }
            else if (nearestRb != null)
            {
                nearestRb.AddForce(toPlayer * 10f, ForceMode.Impulse);
            }
        }
    }
}
