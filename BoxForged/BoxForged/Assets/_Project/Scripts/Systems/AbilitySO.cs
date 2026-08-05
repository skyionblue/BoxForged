using UnityEngine;
using Boxhead.Systems;

namespace Boxhead.Systems
{
    public enum AbilityTrigger
    {
        OnHit,      // fires each time weapon connects (routed from WeaponDurability.OnWeaponDamaged)
        OnSpecial,  // fires when player taps Special button
        OnDodge,    // fires when dodge begins
        OnBlock,    // fires on successful parry (OnParrySuccess)
        Passive,    // applied at equip time, reverted on weapon change
    }

    public enum AbilityEffectType
    {
        None,
        AoeSweep,              // 360° OverlapSphere hit, magnitude = radius
        CounterStrike,         // every Nth hit staggers enemy (N = magnitude cast to int)
        DisableDurability,     // suppress all durability loss while equipped
        RestoreDurability,     // restore magnitude durability on trigger
        DodgeDistanceMult,     // multiply dodge distance by magnitude (passive)
        CritMultiplier,        // next hit after trigger deals magnitude × damage
        AoeKnockback,          // OverlapSphere Rigidbody knockback, magnitude = radius
        ExplosionRadiusMult,   // multiply projectile explosion radius by magnitude (passive)
    }

    [CreateAssetMenu(fileName = "Ability_", menuName = "Boxhead/Abilities/Ability")]
    public class AbilitySO : ScriptableObject
    {
        [Header("Identity")]
        public string abilityId;
        public string displayName;
        [TextArea(2, 4)] public string flavorDescription;

        [Header("Trigger")]
        public AbilityTrigger trigger;
        public AbilityEffectType effectType;

        [Header("Tuning")]
        public float magnitude;
        public float cooldown;

        [Header("Feedback")]
        public GameObject vfxPrefab;
        public AudioClip  sfx;

        [Header("Complex Ability (Sprint 2 Phase 3)")]
        public AbilityBehaviour behaviour;   // null = fully data-driven
    }
}
