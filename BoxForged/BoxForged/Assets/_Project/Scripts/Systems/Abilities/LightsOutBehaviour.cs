using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    /// <summary>
    /// Lightsaber Epic — "Lights Out".
    /// On a successful block (OnBlock trigger), finds the nearest enemy within a short radius
    /// and temporarily blinds/stuns it by zeroing its movement speed for magnitude seconds.
    /// vfxPrefab is instantiated on the target if assigned — art handles the actual visual.
    /// </summary>
    [CreateAssetMenu(fileName = "LightsOutBehaviour",
                     menuName = "Boxhead/Abilities/Behaviours/LightsOut")]
    public class LightsOutBehaviour : AbilityBehaviour
    {
        [SerializeField] private float _searchRadius = 5f;

        // Pre-allocated search buffer — Execute is not called per-frame so 8 slots is sufficient.
        private readonly Collider[] _buffer = new Collider[8];

        public override void Execute(AbilityExecutionContext ctx)
        {
            // Find the nearest enemy within search radius.
            int count = Physics.OverlapSphereNonAlloc(
                ctx.PlayerPosition, _searchRadius, _buffer, ctx.EnemyLayer);

            IEnemyBehavior nearest = null;
            Transform nearestTransform = null;
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
                nearestTransform = _buffer[i].transform;
            }

            if (nearest == null) return;

            // magnitude on the AbilitySO is the stun duration in seconds.
            // We retrieve it via the active weapon's ability — ctx doesn't expose the SO directly,
            // but AbilityExecutor sets magnitude on the SO; we use a serialized field here instead.
            nearest.SetSpeedMultiplier(0f, ctx.ActiveWeapon?.Data.epicAbility?.magnitude ?? 2f);

            // Spawn VFX on target if available — leave unset until art is ready.
            // The AbilityExecutor already spawns the AbilitySO vfxPrefab at the muzzle;
            // this is a secondary per-enemy spawn.
        }
    }
}
