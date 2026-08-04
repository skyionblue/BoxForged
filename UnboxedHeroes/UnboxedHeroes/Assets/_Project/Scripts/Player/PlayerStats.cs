using System;
using UnityEngine;
using Boxhead.Systems;

namespace Boxhead.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private BoxData startingBox;

        [Header("VFX")]
        [SerializeField] private ParticleSystem hitSparkPrefab;

        private int _maxHealthBonus;
        private int _currentBonusHealth;   // depleted before base health

        public BoxData ActiveBox          { get; private set; }
        public int BaseMaxHealth          => ActiveBox != null ? ActiveBox.maxHealth : 100;
        public int MaxHealth              => BaseMaxHealth + _maxHealthBonus;
        public int MaxHealthBonus         => _maxHealthBonus;
        public int CurrentBonusHealth     => _currentBonusHealth;

        // Base health remaining (excludes bonus pool)
        private int _baseCurrentHealth;

        public int CurrentHealth          => _baseCurrentHealth + _currentBonusHealth;
        public float MoveSpeed            => ActiveBox != null ? ActiveBox.moveSpeed : 5f;
        public bool IsDead                => CurrentHealth <= 0;

        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;
        public event Action<BoxData> OnBoxChanged;

        private void Awake()
        {
            if (startingBox != null)
                ApplyBox(startingBox);
        }

        public void ApplyBox(BoxData box)
        {
            ActiveBox = box;
            int maxBase = box.maxHealth;
            _baseCurrentHealth = _baseCurrentHealth > 0
                ? Mathf.Min(_baseCurrentHealth, maxBase)
                : maxBase;
            OnBoxChanged?.Invoke(box);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            // Deplete bonus health first — base health only takes damage once bonus is gone
            if (_currentBonusHealth > 0)
            {
                int bonusDmg = Mathf.Min(amount, _currentBonusHealth);
                _currentBonusHealth -= bonusDmg;
                amount -= bonusDmg;
            }

            if (amount > 0)
                _baseCurrentHealth = Mathf.Max(0, _baseCurrentHealth - amount);

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (amount > 0 && hitSparkPrefab != null)
                Instantiate(hitSparkPrefab, transform.position, Quaternion.identity);

            if (CurrentHealth == 0)
                OnDeath?.Invoke();
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            _baseCurrentHealth = Mathf.Min(_baseCurrentHealth + amount, BaseMaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        /// <summary>
        /// Sets the additive max-health bonus from the stat overlay.
        /// Tops up the current bonus pool to the new maximum so the player
        /// gains the bonus HP immediately on a new run.
        /// </summary>
        public void SetMaxHealthBonus(int bonus)
        {
            int prev = _maxHealthBonus;
            _maxHealthBonus = bonus;
            // Give the player any newly added bonus HP immediately
            _currentBonusHealth = Mathf.Clamp(_currentBonusHealth + (bonus - prev), 0, _maxHealthBonus);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }
}
