using UnityEngine;

namespace Boxhead.Player
{
    public enum AerialAttackType
    {
        DiveKick,
        LassoSlam
    }

    public enum SpecialMoveType
    {
        ShadowDash,
        Tumbleshot
    }

    public enum PassiveType
    {
        DodgeInvincibility,
        WiderParryWindow
    }

    [CreateAssetMenu(menuName = "Boxhead/FightingStyleData", fileName = "FightingStyle_New")]
    public class FightingStyleData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _styleName;

        [Header("Move Types")]
        [SerializeField] private AerialAttackType _aerialAttack;
        [SerializeField] private SpecialMoveType  _specialMove;
        [SerializeField] private PassiveType      _passive;

        [Header("Core Tuning")]
        [SerializeField] private float _specialCooldownDuration = 8f;
        [SerializeField] private float _aerialDamageMultiplier  = 1.5f;

        [Header("Shadow Dash (SpecialMove = ShadowDash only)")]
        [SerializeField] private float _shadowDashDistance = 5f;
        [SerializeField] private int   _shadowDashDamage   = 20;

        [Header("Tumbleshot (SpecialMove = Tumbleshot only)")]
        [SerializeField] private int   _tumbleshotDamage = 20;
        [SerializeField] private float _tumbleshotSpeed  = 20f;
        [SerializeField] private float _tumbleshotRange  = 15f;

        [Header("Passive")]
        [SerializeField] private float _passiveParryWindow = 0.3f;

        [Header("HUD")]
        [SerializeField] private Sprite _styleIcon;

        public string         StyleName               => _styleName;
        public AerialAttackType AerialAttack          => _aerialAttack;
        public SpecialMoveType  SpecialMove           => _specialMove;
        public PassiveType      Passive               => _passive;
        public float          SpecialCooldownDuration => _specialCooldownDuration;
        public float          AerialDamageMultiplier  => _aerialDamageMultiplier;
        public float          ShadowDashDistance      => _shadowDashDistance;
        public int            ShadowDashDamage        => _shadowDashDamage;
        public int            TumbleshotDamage        => _tumbleshotDamage;
        public float          TumbleshotSpeed         => _tumbleshotSpeed;
        public float          TumbleshotRange         => _tumbleshotRange;
        public float          PassiveParryWindow      => _passiveParryWindow;
        public Sprite         StyleIcon               => _styleIcon;
    }
}
