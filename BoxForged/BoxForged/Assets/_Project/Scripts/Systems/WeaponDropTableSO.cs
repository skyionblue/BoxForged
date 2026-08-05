using UnityEngine;

namespace Boxhead.Systems
{
    [System.Serializable]
    public class WeaponSpawnEntry
    {
        public WeaponObjectSO weaponObject;
        public Vector3 worldPosition;
        [Tooltip("Overrides the rarity set on the WeaponObjectSO. Enable to force a specific rarity at this spawn point.")]
        public bool useRarityOverride = false;
        public WeaponRarity rarityOverride = WeaponRarity.Common;
    }

    [System.Serializable]
    public class CardboardSpawnEntry
    {
        public Vector3 worldPosition;
        public int amount = 3;
    }

    [CreateAssetMenu(fileName = "WeaponDropTable_", menuName = "Boxhead/Weapon Drop Table")]
    public class WeaponDropTableSO : ScriptableObject
    {
        [Header("Scattered Objects")]
        [Tooltip("Raw weapon objects placed freely around the level for the player to discover.")]
        public WeaponSpawnEntry[] scatteredObjects;

        [Header("Loot Zone Objects")]
        [Tooltip("Raw weapon objects placed inside designated loot zones. Higher rarity items go here.")]
        public WeaponSpawnEntry[] lootZoneObjects;

        [Header("Cardboard")]
        public CardboardSpawnEntry[] cardboardPiles;

        [Header("Workbenches")]
        [Tooltip("World-space positions where workbench prefabs will be instantiated by LevelBuilder.")]
        public Vector3[] workbenchPositions;
    }
}
