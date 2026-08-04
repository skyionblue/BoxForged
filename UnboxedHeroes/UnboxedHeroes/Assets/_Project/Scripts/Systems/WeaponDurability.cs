using System;
using UnityEngine;

namespace Boxhead.Systems
{
    [RequireComponent(typeof(WeaponInventory))]
    public class WeaponDurability : MonoBehaviour
    {
        public event Action<WeaponInstance> OnWeaponDamaged;
        public event Action<WeaponInstance> OnWeaponBroken;

        private WeaponInventory _weaponInventory;

        private void Awake()
        {
            TryGetComponent(out _weaponInventory);
        }

        public void RegisterHit(WeaponInstance weapon)
        {
            if (weapon == null || weapon.IsBroken) return;

            weapon.DecrementDurability();
            OnWeaponDamaged?.Invoke(weapon);

            if (!weapon.IsBroken) return;

            OnWeaponBroken?.Invoke(weapon);

            // Find the slot by reference — no LINQ, no allocation.
            WeaponInstance[] slots = _weaponInventory.WeaponSlots;
            for (int i = 0; i < slots.Length; i++)
            {
                if (ReferenceEquals(slots[i], weapon))
                {
                    _weaponInventory.RemoveFromSlot(i);
                    return;
                }
            }
        }
    }
}
