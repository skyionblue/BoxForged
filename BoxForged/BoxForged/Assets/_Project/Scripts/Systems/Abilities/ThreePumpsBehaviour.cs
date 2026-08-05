using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    /// <summary>
    /// Pressure Cannon Epic — "Three Pumps".
    /// OnSpecial trigger. Fires a concentrated forward blast that deals 2× weapon damage
    /// to enemies in a short forward cone (2m range). Phase 3 will add a full charge hold
    /// mechanic; this MVP always fires at maximum charge immediately.
    /// </summary>
    [CreateAssetMenu(fileName = "ThreePumpsBehaviour",
                     menuName = "Boxhead/Abilities/Behaviours/ThreePumps")]
    public class ThreePumpsBehaviour : AbilityBehaviour
    {
        [SerializeField] private float _blastRange  = 2f;
        [SerializeField] private float _coneAngle   = 45f;  // half-angle of the forward cone
        [SerializeField] private float _damageMultiplier = 2f;

        // Pre-allocated buffer — zero GC in Execute.
        private readonly Collider[] _buffer = new Collider[8];
        private readonly EnemyStats[] _statsBuffer = new EnemyStats[8];

        public override void Execute(AbilityExecutionContext ctx)
        {
            float baseDamage = (ctx.ActiveWeapon?.Data.damageMultiplier ?? 1f) * 15f;
            int damage = Mathf.RoundToInt(baseDamage * _damageMultiplier);

            int count = Physics.OverlapSphereNonAlloc(
                ctx.PlayerPosition, _blastRange, _buffer, ctx.EnemyLayer);
            int hitCount = 0;

            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                if (!_buffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;

                // Cone check: only enemies within the forward cone.
                Vector3 toEnemy = (_buffer[i].transform.position - ctx.PlayerPosition).normalized;
                float angle = Vector3.Angle(ctx.PlayerForward, toEnemy);
                if (angle > _coneAngle) continue;

                // Deduplicate.
                bool seen = false;
                for (int j = 0; j < hitCount; j++)
                {
                    if (_statsBuffer[j] == stats) { seen = true; break; }
                }
                if (seen) continue;
                if (hitCount >= _statsBuffer.Length) break;

                _statsBuffer[hitCount++] = stats;
                stats.TakeDamage(damage);
            }

            Debug.Log($"[ThreePumpsBehaviour] Three Pumps: hit {hitCount} enemies for {damage} damage.");
        }
    }
}
