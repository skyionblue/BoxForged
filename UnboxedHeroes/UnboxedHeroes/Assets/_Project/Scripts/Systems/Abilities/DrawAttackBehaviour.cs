using System.Collections;
using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    /// <summary>
    /// Katana Legendary — "The Draw Attack".
    /// OnDodge trigger. Replaces the standard dodge with a short forward dash that deals
    /// damage to all enemies caught in the dash sweep. CombatController intercepts the dodge
    /// input and routes it here via AbilityExecutor.FireDodgeAbility() when this ability is active.
    ///
    /// Dash motion: moves the CharacterController 3m forward over 0.2s in discrete steps,
    /// mirroring DodgeRoutine's physics pattern so CharacterController collision still resolves.
    /// Damage: hits all enemies within 1.5m of the player's final position after the dash.
    /// </summary>
    [CreateAssetMenu(fileName = "BHV_DrawAttack",
                     menuName = "Boxhead/Abilities/DrawAttackBehaviour")]
    public class DrawAttackBehaviour : AbilityBehaviour
    {
        [SerializeField] private float _dashDistance  = 3f;
        [SerializeField] private float _dashDuration  = 0.2f;
        [SerializeField] private float _hitRadius     = 1.5f;
        [SerializeField] private float _baseDamage    = 25f;   // overridden by magnitude at runtime

        // Pre-allocated buffers — zero GC in Execute.
        private readonly Collider[]   _hitBuffer   = new Collider[8];
        private readonly EnemyStats[] _statsBuffer = new EnemyStats[8];

        public override void Execute(AbilityExecutionContext ctx)
        {
            // magnitude on the AbilitySO is used as the damage value.
            float magnitude = ctx.ActiveWeapon?.Data.legendaryAbility?.magnitude ?? _baseDamage;
            int damage = Mathf.RoundToInt((ctx.ActiveWeapon?.Data.damageMultiplier ?? 1f) * magnitude);

            // The dash coroutine must run on a MonoBehaviour — delegate to CombatController.
            ctx.Combat.StartCoroutine(DashAttackRoutine(ctx, damage));
        }

        private IEnumerator DashAttackRoutine(AbilityExecutionContext ctx, int damage)
        {
            CharacterController controller = ctx.Combat.GetComponent<CharacterController>();
            if (controller == null) yield break;

            Vector3 dashDir = ctx.PlayerForward;
            if (dashDir == Vector3.zero) dashDir = Vector3.forward;

            // Play the attack animation at the start of the dash for immediate visual feedback.
            ctx.Combat.TriggerAttackAnimation();

            // Step-move over dashDuration so CharacterController collision still resolves.
            float elapsed = 0f;
            float speed = _dashDistance / _dashDuration;
            while (elapsed < _dashDuration)
            {
                controller.Move(dashDir * (speed * Time.deltaTime));
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Damage pass: OverlapSphere at the final position to hit enemies in the sweep zone.
            LayerMask enemyLayer = ctx.EnemyLayer;
            int count = Physics.OverlapSphereNonAlloc(
                ctx.Combat.transform.position, _hitRadius, _hitBuffer, enemyLayer);
            int hitCount = 0;

            for (int i = 0; i < count; i++)
            {
                if (!_hitBuffer[i].CompareTag("Enemy")) continue;
                if (!_hitBuffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;

                // Deduplicate — a single enemy may have multiple colliders.
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

            Debug.Log($"[DrawAttackBehaviour] The Draw Attack: hit {hitCount} enemies for {damage} damage.");
        }
    }
}
