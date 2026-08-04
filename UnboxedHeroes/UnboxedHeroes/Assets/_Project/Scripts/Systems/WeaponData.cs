using UnityEngine;

namespace Boxhead.Systems
{
    [CreateAssetMenu(fileName = "WeaponData_", menuName = "Boxhead/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName;
        [TextArea(2, 4)]
        public string description;

        [Header("Stats")]
        public float damageMultiplier = 1f;
        public float attackRange = 1.8f;

        [Header("Visual")]
        public GameObject weaponPrefab;  // The 3D model to show when equipped
        public Material material;        // Applied to all renderers when the weapon is instantiated
        [Tooltip("Icon displayed in the HUD inventory slots. Assign a Sprite for each WeaponData asset.")]
        public Sprite weaponIcon;

        [Header("Grip")]
        public Vector3 gripPositionOffset;
        public Vector3 gripRotationOffset;
        [Tooltip("Multiplied against the FBX baked root scale (~100). Default 0.35 ≈ 35 cm. Do not set to 1.")]
        public float gripScale = 0.35f;
        [Tooltip("Barrel tip position in weapon-local space. Used to spawn bullets/VFX at the correct point. " +
                 "Tune in Play mode: right-click WeaponHolder → Reapply Grip, then adjust until bullets exit the barrel.")]
        public Vector3 muzzleLocalOffset;

        [Header("Special Ability")]
        public WeaponAbilityData ability;   // null = weapon has no special
        public string specialAbilityName;
        [TextArea(2, 4)]
        public string specialAbilityDescription;
    }
}
