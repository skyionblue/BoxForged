using UnityEngine;

namespace Boxhead.Systems
{
    public enum WeaponTier { Standard, Epic, Legendary }

    [System.Serializable]
    public class WeaponInstance
    {
        public WeaponObjectSO Data { get; private set; }
        public WeaponTier Tier { get; private set; }
        public int CurrentDurability { get; private set; }
        public bool IsBroken => CurrentDurability <= 0;

        public int MaxDurability => Tier switch
        {
            WeaponTier.Epic => Data.epicDurability,
            WeaponTier.Legendary => Data.legendaryDurability,
            _ => Data.standardDurability
        };

        public WeaponInstance(WeaponObjectSO data, WeaponTier tier)
        {
            Data = data;
            Tier = tier;
            CurrentDurability = tier switch
            {
                WeaponTier.Epic => data.epicDurability,
                WeaponTier.Legendary => data.legendaryDurability,
                _ => data.standardDurability
            };
        }

        /// <summary>
        /// Restore-path constructor that preserves an explicit durability value rather than
        /// resetting to full. Used by ProgressionSystem.RestoreRunLoadout so weapon wear
        /// carries across room transitions. Durability is clamped to [0, MaxDurability].
        /// </summary>
        public WeaponInstance(WeaponObjectSO data, WeaponTier tier, int currentDurability)
        {
            Data = data;
            Tier = tier;
            CurrentDurability = Mathf.Clamp(currentDurability, 0, MaxDurability);
        }

        public void DecrementDurability()
        {
            if (CurrentDurability > 0) CurrentDurability--;
        }

        public void RestoreDurability(int amount)
        {
            if (IsBroken) return;
            CurrentDurability = Mathf.Min(CurrentDurability + amount, MaxDurability);
        }
    }
}
