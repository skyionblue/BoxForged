using UnityEngine;

namespace Boxhead.Systems
{
    public enum BoxType
    {
        Ninja,
        Cowboy,
        Knight,
        Wizard,
        Astronaut,
        Pirate
    }

    [CreateAssetMenu(fileName = "BoxData_New", menuName = "Unboxed Heroes/Box Data")]
    public class BoxData : ScriptableObject
    {
        [Header("Identity")]
        public BoxType boxType;
        public string boxDisplayName;
        [TextArea] public string description;

        [Header("Stats")]
        public float moveSpeed = 5f;
        public int maxHealth = 100;
        public int attackDamage = 15;

        [Header("Visuals")]
        public Color primaryColor = Color.white;
        public Color markerColor = Color.black;
        public Sprite portrait;

        [Header("Counter Ability")]
        public string counterAbilityName;
        [TextArea] public string counterAbilityDescription;
    }
}
