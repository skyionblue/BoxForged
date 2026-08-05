using UnityEngine;

namespace Boxhead.Systems
{
    public enum WeaponRarity { Common, Rare, Legendary }
    public enum WeaponType { Melee, Ranged, Defensive, Utility }

    [CreateAssetMenu(fileName = "WeaponObjectSO_", menuName = "Boxhead/Weapon Object")]
    public class WeaponObjectSO : WeaponData
    {
        [Header("Raw Object")]
        public string rawObjectName;
        public GameObject rawObjectPrefab;
        public Sprite rawObjectIcon;

        [Header("Rarity and Type")]
        public WeaponRarity rarity;
        public WeaponType weaponType;

        [Header("Forge Costs")]
        [Tooltip("Cardboard cost to forge this weapon at a workbench. Always 2 for Standard tier.")]
        public int forgeCost = 2;
        [Tooltip("Additional cardboard cost to upgrade to Epic. Set to 5 for Rare/Legendary weapons, 0 for Common.")]
        public int epicUpgradeCost;
        [Tooltip("Additional cardboard cost to upgrade to Legendary. Set to 10 for Legendary weapons, 0 for others.")]
        public int legendaryUpgradeCost;

        [Header("Tier Durability")]
        public int standardDurability = 30;
        public int epicDurability = 60;
        public int legendaryDurability = 100;

        [Header("Tier Visuals")]
        public Sprite epicIcon;
        public Sprite legendaryIcon;
        public GameObject epicWeaponPrefab;
        public GameObject legendaryWeaponPrefab;

        [Header("Equipped Weapon Data")]
        [Tooltip("V3 base WeaponData asset (e.g. WeaponData_obj_bostaff_equipped). WeaponCycler resolves the correct character variant from this at equip time.")]
        public WeaponData baseEquippedData;

        [Header("Abilities")]
        public AbilitySO epicAbility;
        public AbilitySO legendaryAbility;
    }
}
