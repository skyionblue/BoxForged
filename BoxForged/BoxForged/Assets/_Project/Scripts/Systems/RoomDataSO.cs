using System;
using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// One enemy spawn point within a room, as portable data (ADR-0002) rather than
    /// a scene-local Transform/EnemySpawnPoint reference. LevelBuilder.BuildSpawnPoints
    /// instantiates one EnemySpawnPoint marker per entry at runtime.
    /// </summary>
    [Serializable]
    public class RoomSpawnPointEntry
    {
        [Tooltip("Enemy prefab to spawn at this point.")]
        public GameObject enemyPrefab;
        public Vector3 position;
        [Tooltip("Facing applied to spawned enemies at this point (rotation around world Y only).")]
        public float facingY;
        [Min(1)] public int spawnCount = 1;
    }

    /// <summary>
    /// Data-driven room definition (ADR-0002). Holds everything about a room's
    /// encounter composition that is portable — room identity, concurrency cap,
    /// enemy composition, and spawn-point positions — as a diffable asset instead
    /// of scene-local prefab-instance overrides and objectReference fileIDs.
    ///
    /// Scene-local wiring that genuinely cannot be data (an exit gate GameObject,
    /// a props group GameObject already placed in the scene) intentionally stays
    /// off this asset — RoomManager binds those per-scene alongside a RoomDataSO
    /// reference rather than forcing them into portable data that has nothing
    /// meaningful to point at until a scene exists.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomData_", menuName = "Boxhead/Room Data")]
    public class RoomDataSO : ScriptableObject
    {
        public string roomName;
        [Min(1)] public int maxConcurrentEnemies = 2;
        public RoomSpawnPointEntry[] spawnPoints;

        [Tooltip("When true, RoomManager never calls TriggerWin for this room — a boss AI owns the win trigger via its own DefeatSequence.")]
        public bool bossOwnedWin;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (spawnPoints == null) return;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i].enemyPrefab == null)
                    Debug.LogWarning($"[RoomDataSO] '{name}': spawnPoints[{i}].enemyPrefab is not assigned.", this);
                if (spawnPoints[i].spawnCount <= 0)
                    Debug.LogWarning($"[RoomDataSO] '{name}': spawnPoints[{i}].spawnCount must be > 0.", this);
            }
        }
#endif
    }
}
