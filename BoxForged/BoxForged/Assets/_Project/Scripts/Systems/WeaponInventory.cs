using System;
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Systems
{
    [RequireComponent(typeof(WeaponHolder))]
    public class WeaponInventory : MonoBehaviour
    {
        public const int WeaponSlotCount = 3;
        public const int MaterialBagCapacity = 3;

        public WeaponInstance[] WeaponSlots { get; private set; }
        public WeaponObjectSO[] MaterialBag { get; private set; }
        public int ActiveSlotIndex { get; private set; }
        public WeaponInstance ActiveWeapon => WeaponSlots[ActiveSlotIndex];

        public event Action OnInventoryChanged;

        private WeaponHolder  _weaponHolder;
        private WeaponCycler  _weaponCycler;

        private void Awake()
        {
            TryGetComponent(out _weaponHolder);
            TryGetComponent(out _weaponCycler);
            WeaponSlots = new WeaponInstance[WeaponSlotCount];
            MaterialBag = new WeaponObjectSO[MaterialBagCapacity];
        }

        // --- Weapon slots ---

        public bool AddToWeaponSlot(WeaponInstance instance)
        {
            for (int i = 0; i < WeaponSlotCount; i++)
            {
                if (WeaponSlots[i] == null)
                {
                    WeaponSlots[i] = instance;
                    // Auto-equip when filling the active slot (first forge lands in slot 0 = active)
                    if (i == ActiveSlotIndex)
                        _weaponHolder.EquipWeapon(ResolveEquipData(instance.Data), instance.Tier);
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public void RemoveFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= WeaponSlotCount) return;
            WeaponSlots[slotIndex] = null;

            if (slotIndex == ActiveSlotIndex)
            {
                int next = FindNextNonNullSlot();
                if (next >= 0)
                {
                    ActiveSlotIndex = next;
                    _weaponHolder.EquipWeapon(ResolveEquipData(WeaponSlots[ActiveSlotIndex].Data), WeaponSlots[ActiveSlotIndex].Tier);
                }
                else
                {
                    _weaponHolder.UnequipCurrentWeapon();
                }
            }

            OnInventoryChanged?.Invoke();
        }

        public void SetActiveSlot(int slotIndex)
        {
            slotIndex = Mathf.Clamp(slotIndex, 0, WeaponSlotCount - 1);
            ActiveSlotIndex = slotIndex;

            if (WeaponSlots[ActiveSlotIndex] != null)
                _weaponHolder.EquipWeapon(ResolveEquipData(WeaponSlots[ActiveSlotIndex].Data), WeaponSlots[ActiveSlotIndex].Tier);
            else
                _weaponHolder.UnequipCurrentWeapon();

            OnInventoryChanged?.Invoke();
        }

        public void CycleActiveSlot(int direction)
        {
            // Only cycle if at least one slot is non-null; otherwise there is nothing to equip.
            bool anyNonNull = false;
            for (int i = 0; i < WeaponSlotCount; i++)
            {
                if (WeaponSlots[i] != null) { anyNonNull = true; break; }
            }
            if (!anyNonNull) return;

            // Wrap and skip empty slots so the active weapon always changes to a filled one.
            int candidate = ActiveSlotIndex;
            for (int i = 0; i < WeaponSlotCount; i++)
            {
                candidate = (candidate + direction + WeaponSlotCount) % WeaponSlotCount;
                if (WeaponSlots[candidate] != null) break;
            }

            SetActiveSlot(candidate);
        }

        // --- Material bag ---

        public bool AddToMaterialBag(WeaponObjectSO weaponObject)
        {
            for (int i = 0; i < MaterialBagCapacity; i++)
            {
                if (MaterialBag[i] == null)
                {
                    MaterialBag[i] = weaponObject;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public bool RemoveFromMaterialBag(int bagIndex)
        {
            if (bagIndex < 0 || bagIndex >= MaterialBagCapacity) return false;
            if (MaterialBag[bagIndex] == null) return false;
            MaterialBag[bagIndex] = null;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public WeaponObjectSO GetMaterialBagItem(int bagIndex)
        {
            if (bagIndex < 0 || bagIndex >= MaterialBagCapacity) return null;
            return MaterialBag[bagIndex];
        }

        public void NotifyInventoryChanged() => OnInventoryChanged?.Invoke();

        /// <summary>
        /// Restores forged weapon slots and the active slot index after a scene load
        /// (see ProgressionSystem.RestoreRunLoadout). Writes the slot array directly —
        /// like ForgeController.TryUpgrade — then re-equips the active weapon through
        /// WeaponHolder via SetActiveSlot so the model attaches to the correct hand bone.
        /// The material bag is intentionally not restored (persistence covers slots only).
        /// </summary>
        public void RestoreState(WeaponInstance[] slots, int activeIndex)
        {
            for (int i = 0; i < WeaponSlotCount; i++)
                WeaponSlots[i] = (slots != null && i < slots.Length) ? slots[i] : null;

            // SetActiveSlot clamps the index, equips (or unequips) through WeaponHolder,
            // and fires OnInventoryChanged. The explicit NotifyInventoryChanged below is
            // redundant-safe and guarantees a refresh even if SetActiveSlot's path changes.
            SetActiveSlot(activeIndex);
            NotifyInventoryChanged();
        }

        public void ResetForRun()
        {
            for (int i = 0; i < WeaponSlotCount; i++) WeaponSlots[i] = null;
            for (int i = 0; i < MaterialBagCapacity; i++) MaterialBag[i] = null;
            ActiveSlotIndex = 0;
            OnInventoryChanged?.Invoke();
        }

        // Resolves the correct character-variant WeaponData via WeaponCycler.
        // If the slot holds a WeaponObjectSO with a baseEquippedData reference,
        // WeaponCycler maps it to the active character's _nm/_nf variant automatically.
        private WeaponData ResolveEquipData(WeaponData data)
        {
            if (_weaponCycler == null) return data;
            var woso = data as WeaponObjectSO;
            if (woso != null && woso.baseEquippedData != null)
                return _weaponCycler.ResolveWeapon(woso.baseEquippedData) ?? woso.baseEquippedData;
            return _weaponCycler.ResolveWeapon(data) ?? data;
        }

        // Returns the first non-null slot index, or -1 if all slots are empty.
        private int FindNextNonNullSlot()
        {
            for (int i = 0; i < WeaponSlotCount; i++)
            {
                if (WeaponSlots[i] != null) return i;
            }
            return -1;
        }
    }
}
