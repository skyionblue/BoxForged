using UnityEngine;

namespace Boxhead.Core
{
    [CreateAssetMenu(menuName = "Boxhead/DifficultyData", fileName = "Difficulty_New")]
    public class DifficultyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _difficultyName;
        public string DifficultyName => _difficultyName;

        [Header("Enemy Stat Multipliers")]
        [Range(0.1f, 3f)][SerializeField] private float _enemyHealthMultiplier = 1f;
        public float EnemyHealthMultiplier => _enemyHealthMultiplier;

        [Range(0.1f, 3f)][SerializeField] private float _enemyDamageMultiplier = 1f;
        public float EnemyDamageMultiplier => _enemyDamageMultiplier;

        [Header("Grunt Spawner")]
        [SerializeField] private int _gruntSpawnerMax    = 12;
        public int GruntSpawnerMax => _gruntSpawnerMax;

        [SerializeField] private int _gruntSpawnerActive = 3;
        public int GruntSpawnerActive => _gruntSpawnerActive;

        [Header("Roller Spawner")]
        [SerializeField] private int _rollerSpawnerMax    = 5;
        public int RollerSpawnerMax => _rollerSpawnerMax;

        [SerializeField] private int _rollerSpawnerActive = 2;
        public int RollerSpawnerActive => _rollerSpawnerActive;

        [Header("Sentinel Spawner")]
        [SerializeField] private int _sentinelSpawnerMax    = 3;
        public int SentinelSpawnerMax => _sentinelSpawnerMax;

        [SerializeField] private int _sentinelSpawnerActive = 1;
        public int SentinelSpawnerActive => _sentinelSpawnerActive;
    }
}
