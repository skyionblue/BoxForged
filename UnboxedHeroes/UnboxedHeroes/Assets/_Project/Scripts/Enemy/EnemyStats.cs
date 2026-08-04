using System;
using UnityEngine;

namespace Boxhead.Enemy
{
    [DefaultExecutionOrder(-10)]
    public class EnemyStats : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth = 60;
        [SerializeField] private int attackDamage = 20;
        [SerializeField] private bool _countsForWinCondition = true;
        [SerializeField] private bool _startInvulnerable = false;

        [Header("VFX")]
        [SerializeField] private ParticleSystem hitSparkPrefab;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public int AttackDamage => attackDamage;
        public bool IsDead => CurrentHealth <= 0;
        public bool CountsForWinCondition => _countsForWinCondition;

        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;
        public event Action OnHit;

        // Fired by any EnemyStats instance on death — subscribe once, hear all kills.
        public static event Action OnAnyEnemyDeath;

        // Ensures OnAnyEnemyDeath fires exactly once per enemy, even if TakeDamage
        // is re-entered mid-event-chain (e.g. counter strike cascades killing a second enemy
        // whose IsDead is then true when the outer ApplyAttackDamage loop reaches it).
        private bool _deathRewardedIP;
        private bool _invulnerable;

        private void Awake() => ResetStats();

        // OnEnable runs even when "Enter Play Mode - Reload Scene" is disabled.
        // Without it, Awake is skipped for pre-placed enemies between play sessions
        // and CurrentHealth stays at 0 (persisted from the previous session's death).
        private void OnEnable() => ResetStats();

        private void ResetStats()
        {
            CurrentHealth    = maxHealth;
            _deathRewardedIP = false;
            _invulnerable    = _startInvulnerable;
        }

        /// <summary>Called by boss intro scripts to make the boss damageable after the cutscene ends.</summary>
        public void SetInvulnerable(bool value) => _invulnerable = value;

        /// <summary>
        /// Scales base stats by difficulty multipliers. Must be called before the enemy
        /// takes its first action so Awake's CurrentHealth = maxHealth is superseded here.
        /// Intended to be called by DifficultyManager immediately after Instantiate.
        /// </summary>
        public void ApplyDifficultyMultipliers(float healthMult, float damageMult)
        {
            maxHealth    = Mathf.RoundToInt(maxHealth    * healthMult);
            attackDamage = Mathf.RoundToInt(attackDamage * damageMult);
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || _invulnerable) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            OnHit?.Invoke();

            // Spawn hit spark VFX
            if (amount > 0 && hitSparkPrefab != null)
            {
                Instantiate(hitSparkPrefab, transform.position, Quaternion.identity);
            }

            if (CurrentHealth == 0)
            {
                // Fire IP reward before OnDeath so no chain inside OnDeath can
                // inadvertently mark this enemy as already rewarded.
                if (!_deathRewardedIP)
                {
                    _deathRewardedIP = true;
                    OnAnyEnemyDeath?.Invoke();
                }
                OnDeath?.Invoke();
            }
        }
    }
}
