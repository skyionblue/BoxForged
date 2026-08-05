using System.Collections;
using UnityEngine;
using Boxhead.Player;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    [CreateAssetMenu(menuName = "Boxhead/Abilities/Lasso")]
    public class LassoAbilityData : WeaponAbilityData
    {
        [SerializeField] private float arcRadius  = 2.5f;
        [SerializeField] private float baseDamage = 12f;
        [SerializeField] private float rootDuration = 0.5f;

        [SerializeField] private float pullRadius = 6f;

        // Pre-allocated physics buffers — never re-allocated per activation.
        private Collider[] _buffer;

        // Cached WaitForSeconds allocations.
        private WaitForSeconds _waitRoot;

        private void OnEnable()
        {
            _buffer   = new Collider[8];
            _waitRoot = new WaitForSeconds(rootDuration);
        }

        public override IEnumerator Activate(AbilityActivationContext ctx, CombatController combat)
        {
            if (ctx.IsCounterWindow)
                yield return combat.StartCoroutine(PullRoutine(ctx, combat));
            else
                yield return combat.StartCoroutine(SwingRoutine(ctx, combat));
        }

        private IEnumerator SwingRoutine(AbilityActivationContext ctx, CombatController combat)
        {
            int count = Physics.OverlapSphereNonAlloc(ctx.PlayerPosition, arcRadius, _buffer, ctx.EnemyLayerMask);
            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                if (!_buffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;

                stats.TakeDamage(Mathf.RoundToInt(baseDamage));

                var behavior = _buffer[i].GetComponentInParent<IEnemyBehavior>();
                if (behavior != null)
                {
                    behavior.SetRooted(true);
                    // Host the timer on the enemy so it dies with the enemy, not the player.
                    var enemyMB = _buffer[i].GetComponentInParent<MonoBehaviour>();
                    enemyMB?.StartCoroutine(RootEnemy(behavior));
                }
            }
            yield break;
        }

        private IEnumerator RootEnemy(IEnemyBehavior behavior)
        {
            yield return _waitRoot;
            behavior?.SetRooted(false);
        }

        private IEnumerator PullRoutine(AbilityActivationContext ctx, CombatController combat)
        {
            int count = Physics.OverlapSphereNonAlloc(ctx.PlayerPosition, pullRadius, _buffer, ctx.EnemyLayerMask);

            // Find the nearest enemy.
            float nearestSqDist  = float.MaxValue;
            Transform nearestEnemy = null;
            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                if (!_buffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;

                float sqDist = (ctx.PlayerPosition - _buffer[i].transform.position).sqrMagnitude;
                if (sqDist < nearestSqDist)
                {
                    nearestSqDist  = sqDist;
                    nearestEnemy   = _buffer[i].transform;
                }
            }

            if (nearestEnemy == null) yield break;

            Vector3 pullTarget   = ctx.PlayerPosition + ctx.PlayerForward * 1.5f;
            Vector3 startPos     = nearestEnemy.position;
            float   elapsed      = 0f;
            const float duration = 0.3f;

            while (elapsed < duration)
            {
                if (nearestEnemy == null) yield break;
                Vector3 target = Vector3.Lerp(startPos, pullTarget, elapsed / duration);
                Vector3 delta  = target - nearestEnemy.position;
                if (nearestEnemy.TryGetComponent<CharacterController>(out var cc))
                    cc.Move(delta);
                else
                    nearestEnemy.position = target;
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (nearestEnemy != null)
            {
                Vector3 finalDelta = pullTarget - nearestEnemy.position;
                if (nearestEnemy.TryGetComponent<CharacterController>(out var cc))
                    cc.Move(finalDelta);
                else
                    nearestEnemy.position = pullTarget;
            }
        }
    }
}
