using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Core
{
    /// <summary>
    /// Singleton that holds the player's chosen difficulty for the current run.
    /// Persists across scene loads so difficulty survives a Restart() scene reload.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        public DifficultyData Current { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Set the active difficulty for this run.</summary>
        public void Set(DifficultyData data)
        {
            Current = data;
        }

        /// <summary>
        /// Scale an enemy's stats at spawn time.
        /// Call from EnemySpawner immediately after Instantiate.
        /// Safe to call with a null stats reference — it will no-op.
        /// </summary>
        public void ApplyToStats(EnemyStats stats)
        {
            if (Current == null || stats == null) return;
            stats.ApplyDifficultyMultipliers(Current.EnemyHealthMultiplier, Current.EnemyDamageMultiplier);
        }
    }
}
