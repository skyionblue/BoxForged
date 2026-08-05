using UnityEngine;

namespace Boxhead.Systems
{
    public enum UpgradeEffect
    {
        AttackUp,
        DefenseUp,
        HealFlat,
        SpecialCooldownDown,
        DodgeSpeedUp,
        AgilityUp,
        LuckUp
    }

    /// <summary>
    /// Describes a single upgrade card offered to the player between rooms.
    /// Create instances via Assets > Create > BoxForged > Upgrade Card.
    /// </summary>
    [CreateAssetMenu(menuName = "BoxForged/Upgrade Card")]
    public class UpgradeCardData : ScriptableObject
    {
        [SerializeField] private string        _displayName;
        [TextArea(2, 3)]
        [SerializeField] private string        _description;
        [SerializeField] private UpgradeEffect _effect;
        [SerializeField] private float         _magnitude = 5f;
        [SerializeField] private Sprite        _icon;

        public string        DisplayName  => _displayName;
        public string        Description  => _description;
        public UpgradeEffect Effect       => _effect;
        public float         Magnitude    => _magnitude;
        public Sprite        Icon         => _icon;
    }
}
