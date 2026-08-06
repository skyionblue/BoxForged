using System;
using UnityEngine;

namespace Boxhead.Systems
{
    [RequireComponent(typeof(WeaponInventory))]
    [RequireComponent(typeof(CardboardResource))]
    public class ForgeController : MonoBehaviour
    {
        public event Action<WeaponInstance> OnWeaponForged;
        public event Action<WeaponInstance> OnWeaponUpgraded;

        private WeaponInventory _weaponInventory;
        private CardboardResource _cardboardResource;

        private void Awake()
        {
            TryGetComponent(out _weaponInventory);
            TryGetComponent(out _cardboardResource);
        }

        public bool TryForge(int bagIndex)
        {
            if (bagIndex < 0 || bagIndex >= WeaponInventory.MaterialBagCapacity) return false;

            WeaponObjectSO weaponObject = _weaponInventory.GetMaterialBagItem(bagIndex);
            if (weaponObject == null) return false;
            if (!_cardboardResource.CanAfford(weaponObject.forgeCost)) return false;

            var instance = new WeaponInstance(weaponObject, WeaponTier.Standard);
            if (!_weaponInventory.AddToWeaponSlot(instance)) return false;  // check slots FIRST

            _cardboardResource.Spend(weaponObject.forgeCost);               // spend AFTER success
            _weaponInventory.RemoveFromMaterialBag(bagIndex);

            OnWeaponForged?.Invoke(instance);

            // First successful forge ever → play the forge tutorial cutscene once.
            if (!Boxhead.Core.CutsceneFlags.HasSeen(Boxhead.Core.CutsceneCatalog.KeyForgeFirst))
            {
                Boxhead.Core.CutsceneFlags.MarkSeen(Boxhead.Core.CutsceneCatalog.KeyForgeFirst);
                Boxhead.Core.CutscenePlayer.Instance?.Play(Boxhead.Core.CutsceneCatalog.ForgeFirst);
            }

            return true;
        }

        public bool TryUpgrade(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= WeaponInventory.WeaponSlotCount) return false;

            WeaponInstance existing = _weaponInventory.WeaponSlots[slotIndex];
            if (existing == null) return false;

            WeaponTier nextTier;
            int cost;

            if (!TryGetNextTier(existing, out nextTier, out cost)) return false;
            if (!_cardboardResource.CanAfford(cost)) return false;

            _cardboardResource.Spend(cost);

            var upgraded = new WeaponInstance(existing.Data, nextTier);

            // Replace the slot directly — RemoveFromSlot would trigger unequip, so we write to
            // the array through a local reference and fire the event manually here.
            _weaponInventory.WeaponSlots[slotIndex] = upgraded;

            // Keep the equipped weapon in sync if this slot is active; otherwise still notify UI.
            if (slotIndex == _weaponInventory.ActiveSlotIndex)
                _weaponInventory.SetActiveSlot(slotIndex);
            else
                _weaponInventory.NotifyInventoryChanged();

            OnWeaponUpgraded?.Invoke(upgraded);
            return true;
        }

        // Populates nextTier and cost based on rarity ceiling rules.
        // Returns false when no upgrade path exists (ceiling reached or rarity forbids it).
        private bool TryGetNextTier(WeaponInstance existing, out WeaponTier nextTier, out int cost)
        {
            nextTier = existing.Tier;
            cost = 0;

            WeaponRarity rarity = existing.Data.rarity;

            if (existing.Tier == WeaponTier.Standard)
            {
                // Common weapons have no upgrade path.
                if (rarity == WeaponRarity.Common) return false;

                nextTier = WeaponTier.Epic;
                cost = existing.Data.epicUpgradeCost;
                return true;
            }

            if (existing.Tier == WeaponTier.Epic)
            {
                // Only Legendary-rarity weapons can reach Legendary tier.
                if (rarity != WeaponRarity.Legendary) return false;

                nextTier = WeaponTier.Legendary;
                cost = existing.Data.legendaryUpgradeCost;
                return true;
            }

            // Already at Legendary tier — ceiling reached.
            return false;
        }
    }
}
