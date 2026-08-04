using UnityEngine;

namespace Boxhead.Player
{
    [CreateAssetMenu(menuName = "Unboxed Heroes/Character Stats")]
    public class CharacterStatsSO : ScriptableObject
    {
        [Header("Base Stats")]
        [SerializeField] private int   _maxHealth   = 100;
        [SerializeField] private int   _attackPower = 10;
        [SerializeField] private float _agility     = 1f;   // dodge speed multiplier
        [SerializeField] private float _luck        = 1f;   // upgrade rarity multiplier
        [SerializeField] private int   _defense     = 0;    // flat damage reduction

        public int   MaxHealth   => _maxHealth;
        public int   AttackPower => _attackPower;
        public float Agility     => _agility;
        public float Luck        => _luck;
        public int   Defense     => _defense;
    }
}
