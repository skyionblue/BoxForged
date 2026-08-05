using System;
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Systems
{
    /// <summary>
    /// In-run two-slot weapon inventory. Resets each run — nothing persists to SaveData.
    /// Sits between WeaponPickup and WeaponHolder: callers always go through Inventory.Equip()
    /// rather than calling WeaponHolder.EquipWeapon() directly.
    /// </summary>
    [RequireComponent(typeof(WeaponHolder))]
    public class Inventory : MonoBehaviour
    {
        public WeaponData EquippedSlot { get; private set; }
        public WeaponData BackpackSlot { get; private set; }

        /// <summary>
        /// Fired after every slot change. Arguments are (equippedSlot, backpackSlot) — either
        /// or both may be null. Subscribers must handle null without error.
        /// </summary>
        public event Action<WeaponData, WeaponData> OnInventoryChanged;

        private WeaponHolder _weaponHolder;

        private void Awake()
        {
            TryGetComponent(out _weaponHolder);
        }

        /// <summary>
        /// Route a newly picked-up weapon through the inventory.
        /// Rules (in priority order):
        ///   1. EquippedSlot empty  → equip directly.
        ///   2. EquippedSlot full, BackpackSlot empty → move equipped to backpack, equip new.
        ///   3. Both slots full → discard BackpackSlot, move equipped to backpack, equip new.
        /// After every path that changes state, OnInventoryChanged fires.
        /// </summary>
        public void Equip(WeaponData weapon)
        {
            if (weapon == null) return;

            if (EquippedSlot == null)
            {
                EquippedSlot = weapon;
                _weaponHolder.EquipWeapon(weapon);
                OnInventoryChanged?.Invoke(EquippedSlot, BackpackSlot);
                return;
            }

            if (BackpackSlot == null)
            {
                BackpackSlot = EquippedSlot;
                EquippedSlot = weapon;
                _weaponHolder.EquipWeapon(weapon);
                OnInventoryChanged?.Invoke(EquippedSlot, BackpackSlot);
                return;
            }

            // Both slots full — silently drop the BackpackSlot (no physical world drop yet).
            BackpackSlot = EquippedSlot;
            EquippedSlot = weapon;
            _weaponHolder.EquipWeapon(weapon);
            OnInventoryChanged?.Invoke(EquippedSlot, BackpackSlot);
        }

        /// <summary>
        /// Swaps EquippedSlot and BackpackSlot. No-op when BackpackSlot is null.
        /// </summary>
        public void Swap()
        {
            if (BackpackSlot == null) return;

            WeaponData temp = EquippedSlot;
            EquippedSlot = BackpackSlot;
            BackpackSlot = temp;

            _weaponHolder.EquipWeapon(EquippedSlot);
            OnInventoryChanged?.Invoke(EquippedSlot, BackpackSlot);
        }

        /// <summary>
        /// Directly replaces EquippedSlot without pushing the current weapon to BackpackSlot.
        /// Use for programmatic starts (WeaponCycler, scene setup, load-from-save) where the
        /// "pickup pushes old weapon to backpack" rule would produce incorrect state.
        /// BackpackSlot is left unchanged. Fires OnInventoryChanged.
        /// </summary>
        public void SetEquipped(WeaponData weapon)
        {
            if (weapon == null) return;
            EquippedSlot = weapon;
            _weaponHolder.EquipWeapon(weapon);
            OnInventoryChanged?.Invoke(EquippedSlot, BackpackSlot);
        }

        /// <summary>
        /// Drops EquippedSlot and promotes BackpackSlot to equipped.
        /// If BackpackSlot is also null after the drop, calls WeaponHolder.UnequipCurrentWeapon().
        /// </summary>
        public void Drop()
        {
            if (EquippedSlot == null) return;

            EquippedSlot = BackpackSlot;
            BackpackSlot = null;

            if (EquippedSlot != null)
                _weaponHolder.EquipWeapon(EquippedSlot);
            else
                _weaponHolder.UnequipCurrentWeapon();

            OnInventoryChanged?.Invoke(EquippedSlot, BackpackSlot);
        }
    }
}
