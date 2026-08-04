using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Marks a world-space position from which RoomManager can spawn enemies one
    /// at a time up to a configurable quota. Holds no runtime update logic — it is
    /// a pure data/factory component driven entirely by RoomManager.
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private int _spawnCount = 2;

        private void OnValidate()
        {
            if (_enemyPrefab == null)
                Debug.LogWarning($"[EnemySpawnPoint] '{name}': _enemyPrefab is not assigned.", this);
            if (_spawnCount <= 0)
                Debug.LogWarning($"[EnemySpawnPoint] '{name}': _spawnCount must be > 0.", this);
        }

        public int SpawnCount => _spawnCount;
        public int Remaining { get; private set; }
        public bool HasMore => Remaining > 0;

        private void Awake()
        {
            Remaining = _spawnCount;
        }

        /// <summary>
        /// Instantiates the next enemy at this point's transform. Returns null when
        /// the quota is exhausted or the prefab reference is missing.
        /// </summary>
        public GameObject SpawnNext()
        {
            if (Remaining <= 0 || _enemyPrefab == null) return null;

            var instance = Instantiate(_enemyPrefab, transform.position, transform.rotation);
            Remaining--;
            return instance;
        }

        /// <summary>
        /// Restores the full spawn quota. Called by RoomManager.ResetForRestart()
        /// before a scene-level restart is initiated.
        /// </summary>
        public void ResetSpawner()
        {
            Remaining = _spawnCount;
        }
    }
}
