using System;
using UnityEngine;

namespace Boxhead.Player
{
    /// <summary>
    /// Manages the visual weapon model activation and animator weapon-type parameter.
    /// Unlike WeaponHolder (which instantiates weapons from prefabs/WeaponData), this
    /// controller toggles an already-placed weapon GameObject in the hand hierarchy
    /// and syncs the Animator's WeaponType parameter for correct attack animations.
    /// </summary>
    public class WeaponEquipController : MonoBehaviour
    {
        private static readonly int WeaponTypeHash = Animator.StringToHash("WeaponType");

        private const int WeaponTypeUnarmed = 0;
        private const int WeaponTypeTwoHandSword = 1;

        [SerializeField] private Animator _animator;

        private bool _isEquipped;

        /// <summary>Whether the weapon is currently equipped (animator in sword mode).</summary>
        public bool IsWeaponEquipped => _isEquipped;

        /// <summary>Fired when the weapon is equipped.</summary>
        public event Action OnWeaponEquipped;

        /// <summary>Fired when the weapon is unequipped.</summary>
        public event Action OnWeaponUnequipped;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
        }

        /// <summary>
        /// Sets the animator to the sword animation set.
        /// No-op if already equipped.
        /// </summary>
        public void EquipWeapon()
        {
            if (_isEquipped) return;

            _isEquipped = true;

            if (_animator != null)
                _animator.SetInteger(WeaponTypeHash, WeaponTypeTwoHandSword);

            OnWeaponEquipped?.Invoke();
        }

        /// <summary>
        /// Sets the animator to unarmed animations.
        /// No-op if already unequipped.
        /// </summary>
        public void UnequipWeapon()
        {
            if (!_isEquipped) return;

            _isEquipped = false;

            if (_animator != null)
                _animator.SetInteger(WeaponTypeHash, WeaponTypeUnarmed);

            OnWeaponUnequipped?.Invoke();
        }

        /// <summary>
        /// Toggles between equipped and unequipped states.
        /// </summary>
        public void ToggleWeapon()
        {
            if (_isEquipped)
                UnequipWeapon();
            else
                EquipWeapon();
        }
    }
}
