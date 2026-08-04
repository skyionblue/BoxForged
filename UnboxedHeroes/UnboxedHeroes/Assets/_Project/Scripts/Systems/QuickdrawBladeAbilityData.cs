using System.Collections;
using UnityEngine;
using Boxhead.Player;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    [CreateAssetMenu(menuName = "Boxhead/Abilities/QuickdrawBlade")]
    public class QuickdrawBladeAbilityData : WeaponAbilityData
    {
        [SerializeField] private float slashRadius              = 1.2f;
        [SerializeField] private int   slashDamage              = 8;
        [SerializeField] private float comboChainWindow         = 0.4f;
        [SerializeField] private float pommelnDamageMultiplier  = 1.5f;
        [SerializeField] private float pommelStaggerMultiplier  = 2f;

        // Pre-allocated physics buffers — allocated in OnEnable (not here, to avoid double-allocation).
        private Collider[]   _buffer;
        private EnemyStats[] _hitBuffer;

        private WaitForSeconds _waitCombo;

        private void OnEnable()
        {
            _buffer    = new Collider[8];
            _hitBuffer = new EnemyStats[8];
            _waitCombo = new WaitForSeconds(comboChainWindow);
        }

        public override IEnumerator Activate(AbilityActivationContext ctx, CombatController combat)
        {
            // Guard against stale SO state when domain reload is disabled.
            System.Array.Clear(_hitBuffer, 0, _hitBuffer.Length);

            // Slash 1
            ApplySlash(ctx, slashDamage);
            yield return _waitCombo;

            // Slash 2
            ApplySlash(ctx, slashDamage);
            yield return _waitCombo;

            // Pommel
            int pommelDamage = Mathf.RoundToInt(slashDamage * pommelnDamageMultiplier);
            ApplySlash(ctx, pommelDamage);

            int count = Physics.OverlapSphereNonAlloc(ctx.PlayerPosition, slashRadius, _buffer, ctx.EnemyLayerMask);
            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                if (!_buffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;
                var behavior = _buffer[i].GetComponentInParent<IEnemyBehavior>();
                behavior?.ApplyHitStagger(pommelStaggerMultiplier);
            }
        }

        private void ApplySlash(AbilityActivationContext ctx, int damage)
        {
            int count    = Physics.OverlapSphereNonAlloc(ctx.PlayerPosition, slashRadius, _buffer, ctx.EnemyLayerMask);
            int hitCount = 0;

            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                if (!_buffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;

                // De-duplicate: skip if already hit this activation.
                bool seen = false;
                for (int j = 0; j < hitCount; j++)
                {
                    if (_hitBuffer[j] == stats) { seen = true; break; }
                }
                if (seen) continue;
                if (hitCount >= _hitBuffer.Length) break;

                _hitBuffer[hitCount++] = stats;
                stats.TakeDamage(damage);
            }

            // Clear hit buffer for next slash in the combo.
            for (int i = 0; i < hitCount; i++) _hitBuffer[i] = null;
        }
    }
}
