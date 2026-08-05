using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boxhead.Core;

namespace Boxhead.Enemy
{
    public enum SpawnerType { Grunt, Roller, Sentinel }

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawner Type")]
        [SerializeField] private SpawnerType _spawnerType = SpawnerType.Grunt;
        public SpawnerType Type => _spawnerType;

        [Header("Prefab")]
        [SerializeField] private GameObject enemyPrefab;

        [Header("Skeptic")]
        [SerializeField] private GameObject skepticPrefab;
        [Min(1)]
        [SerializeField] private int skepticSpawnInterval = 5;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("Tuning")]
        [SerializeField] private int maxActiveEnemies = 2;
        [SerializeField] private int maxTotalSpawns = 20;
        [Tooltip("Added on top of the difficulty-data value — use to scale this zone higher than others without changing shared DifficultyData assets.")]
        [SerializeField] private int _spawnCountBonus = 0;

        public int MaxTotalSpawns => maxTotalSpawns;
        [SerializeField] private float initialDelay = 1f;
        [SerializeField] private float respawnCheckInterval = 5f;

        private readonly List<GameObject> _active = new List<GameObject>();
        private WaitForSeconds _waitInitial;
        private WaitForSeconds _waitInterval;
        private int _killCount;
        private int _totalSpawned;

        private void Awake()
        {
            _waitInitial = new WaitForSeconds(initialDelay);
            _waitInterval = new WaitForSeconds(respawnCheckInterval);
        }

        private void OnEnable()
        {
            EnemyStats.OnAnyEnemyDeath += OnEnemyKilled;
        }

        private void OnDisable()
        {
            EnemyStats.OnAnyEnemyDeath -= OnEnemyKilled;
            StopAllCoroutines();
        }

        private void Start()
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[EnemySpawner] No enemy prefab assigned.", this);
                return;
            }
            if (spawnPoints.Length == 0)
            {
                Debug.LogWarning("[EnemySpawner] No spawn points assigned.", this);
                return;
            }
            if (maxActiveEnemies <= 0)
            {
                Debug.LogWarning("[EnemySpawner] maxActiveEnemies must be > 0.", this);
                return;
            }

            StartCoroutine(SpawnLoop());
        }

        private void OnEnemyKilled()
        {
            _killCount++;
            if (skepticSpawnInterval > 0 && _killCount % skepticSpawnInterval == 0)
            {
                SpawnSkeptic();
            }
        }

        private IEnumerator SpawnLoop()
        {
            yield return _waitInitial;
            while (_totalSpawned < maxTotalSpawns)
            {
                PruneDestroyed();
                if (_active.Count < maxActiveEnemies)
                    SpawnOne();
                yield return _waitInterval;
            }
            // Spawning complete — stop the loop
        }

        /// <summary>
        /// Apply difficulty-tier spawn limits. Called once by RunStartUI before the spawner starts.
        /// maxActive clamps how many live enemies the spawner maintains at once;
        /// maxTotal is the lifetime cap before the spawner stops.
        /// </summary>
        public void ApplyDifficulty(int maxActive, int maxTotal)
        {
            maxActiveEnemies = Mathf.Max(1, maxActive);
            maxTotalSpawns   = Mathf.Max(1, maxTotal + _spawnCountBonus);
        }

        private void SpawnOne()
        {
            if (_totalSpawned >= maxTotalSpawns) return;

            Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            var spawned = Instantiate(enemyPrefab, point.position, point.rotation);
            _active.Add(spawned);
            _totalSpawned++;

            if (spawned.TryGetComponent<EnemyStats>(out var stats))
                DifficultyManager.Instance?.ApplyToStats(stats);
            else
                Debug.LogWarning($"[EnemySpawner] Spawned enemy '{spawned.name}' has no EnemyStats component.", this);
        }

        private void SpawnSkeptic()
        {
            if (_totalSpawned >= maxTotalSpawns) return;

            if (skepticPrefab == null)
            {
                Debug.LogWarning("[EnemySpawner] skepticPrefab is not assigned — cannot spawn Skeptic.", this);
                return;
            }

            Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            var spawned = Instantiate(skepticPrefab, point.position, point.rotation);
            _active.Add(spawned);
            _totalSpawned++;

            if (spawned.TryGetComponent<EnemyStats>(out var stats))
                DifficultyManager.Instance?.ApplyToStats(stats);
            else
                Debug.LogWarning($"[EnemySpawner] Spawned skeptic '{spawned.name}' has no EnemyStats component.", this);
        }

        // Destroyed GameObjects become null in a managed List — prune them before counting
        private void PruneDestroyed()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i] == null)
                {
                    _active[i] = _active[_active.Count - 1];
                    _active.RemoveAt(_active.Count - 1);
                }
            }
        }
    }
}
