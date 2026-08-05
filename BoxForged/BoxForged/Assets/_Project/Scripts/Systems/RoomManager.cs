using System;
using System.Collections.Generic;
using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    [Serializable]
    public class RoomData
    {
        [SerializeField] public string roomName;
        // Pre-placed instances — used by boss room (no spawn points). Normal rooms
        // should populate spawnPoints instead and leave this list empty.
        [SerializeField] public List<GameObject> enemies = new List<GameObject>();
        [SerializeField] public List<EnemySpawnPoint> spawnPoints = new List<EnemySpawnPoint>();
        [SerializeField] public int maxConcurrentEnemies = 2;
        [SerializeField] public RoomGate exitGate;
        [SerializeField] public GameObject propsGroup;
        // When true, RoomCleared does not call TriggerWin — the boss AI owns the win trigger.
        [SerializeField] public bool bossOwnedWin;
    }

    /// <summary>
    /// Owns room-by-room progression: activates enemy sets (or spawns wave enemies)
    /// when the player enters each room, opens the exit gate when all enemies are dead,
    /// and signals GameManager when the final room is cleared.
    ///
    /// Two activation paths:
    ///   Spawn-point path — rooms with spawnPoints populated. Maintains a concurrent
    ///     cap of maxConcurrentEnemies alive at once; refills from spawn points as
    ///     enemies die. Zero GC after Start(): the death delegate is allocated once.
    ///   Legacy pre-placed path — rooms with an enemies list (boss room). Behaviour
    ///     is identical to the original implementation.
    ///
    /// GC strategy (both paths): all Action delegates are allocated once and stored
    /// in _allHandlers / _onSpawnedEnemyDeath — zero allocations after init.
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [SerializeField] private List<RoomData> _rooms = new List<RoomData>();

        private int _currentRoom = -1;

        // ─── Spawn-point path state ───────────────────────────────────────────────

        // Number of spawned enemies currently alive this wave.
        private int _aliveCount;
        // Next spawn point to draw from within the active room.
        private int _nextSpawnPointIndex;
        // Reference to the active room's spawn point list — assigned by reference, never copied.
        // Null until the first ActivateRoom call to avoid wasting the initializer allocation.
        private List<EnemySpawnPoint> _activeSpawnPoints;
        // Single delegate allocated once per RoomManager lifetime (??= in ActivateRoom) and
        // reused for every spawn in every room. A scene reload creates a new allocation.
        private Action _onSpawnedEnemyDeath;
        // Tracks spawned enemies so their OnDeath subscriptions can be cleaned up on
        // room transition or RoomManager destruction — mirrors the legacy path's tracking.
        private readonly List<EnemyStats> _activeSpawnedStats = new List<EnemyStats>();

        // ─── Legacy pre-placed path state ────────────────────────────────────────

        // Active tracking — parallel lists, indices always in sync.
        private readonly List<EnemyStats> _activeEnemyStats = new List<EnemyStats>();
        private readonly List<Action> _activeEnemyHandlers = new List<Action>();

        // Pre-allocated handler cache: built once in Start(), never re-allocated.
        private readonly Dictionary<EnemyStats, Action> _allHandlers =
            new Dictionary<EnemyStats, Action>();

        // ─── Lifecycle ───────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Build handler cache for legacy pre-placed enemies — one allocation per
            // enemy, done at init, never again during gameplay.
            foreach (var room in _rooms)
            {
                foreach (var enemy in room.enemies)
                {
                    if (enemy == null) continue;
                    if (!enemy.TryGetComponent<EnemyStats>(out var stats)) continue;
                    if (_allHandlers.ContainsKey(stats)) continue;

                    EnemyStats captured = stats;
                    _allHandlers[stats] = () => OnEnemyDeath(captured);
                }
            }

            // Deactivate all pre-placed room enemies — RoomManager owns their activation.
            foreach (var room in _rooms)
                foreach (var enemy in room.enemies)
                    if (enemy != null) enemy.SetActive(false);

            // Room 0 activates immediately — player spawns there.
            ActivateRoom(0);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            UnsubscribeAllActive();
        }

        // Fires whenever a room becomes active — subscribers receive the 0-based room index.
        public static event Action<int> OnRoomActivated;

        /// <summary>
        /// Fires when a non-boss room is cleared. Argument is the 0-based room index.
        /// GameManager listens to this event to show the upgrade screen or shop before
        /// the player can advance. Not fired for rooms with bossOwnedWin = true.
        /// </summary>
        public static event Action<int> OnRoomCleared;

        // ─── Public API ──────────────────────────────────────────────────────────

        /// <summary>Called by RoomTrigger when the player crosses a room boundary.</summary>
        public void OnRoomEntered(int roomIndex)
        {
            if (roomIndex <= _currentRoom) return;
            ActivateRoom(roomIndex);
        }

        /// <summary>
        /// Resets all spawn point quotas across every room and re-activates room 0.
        /// Iterates all rooms (not just the last active one) so every EnemySpawnPoint
        /// is restored. In practice GameManager.Restart() calls SceneManager.LoadScene,
        /// making this a scene reload — this method exists for completeness only.
        /// </summary>
        public void ResetForRestart()
        {
            foreach (var room in _rooms)
            {
                if (room.spawnPoints == null) continue;
                foreach (var sp in room.spawnPoints)
                    if (sp != null) sp.ResetSpawner();
            }
            _currentRoom = -1;
            ActivateRoom(0);
        }

        // ─── Private helpers — shared ─────────────────────────────────────────────

        private void ActivateRoom(int index)
        {
            if (index < 0 || index >= _rooms.Count) return;

            _currentRoom = index;
            OnRoomActivated?.Invoke(index);
            UnsubscribeAllActive();

            _aliveCount = 0;
            _nextSpawnPointIndex = 0;

            var room = _rooms[index];

            if (room.propsGroup != null) room.propsGroup.SetActive(true);

            // ── Spawn-point path ──────────────────────────────────────────────────
            if (room.spawnPoints != null && room.spawnPoints.Count > 0)
            {
                _activeSpawnPoints = room.spawnPoints;

                // Allocate the death delegate exactly once across all rooms/restarts.
                _onSpawnedEnemyDeath ??= OnSpawnedEnemyDied;

                // Seed the initial concurrent batch.
                while (_aliveCount < room.maxConcurrentEnemies)
                {
                    if (!TrySpawnNext()) break;
                }

                // Edge case: all spawn points were empty/null prefabs.
                if (_aliveCount == 0) RoomCleared(index);
                return;
            }

            // ── Legacy pre-placed path (boss room) ────────────────────────────────
            foreach (var enemy in room.enemies)
            {
                if (enemy == null) continue;
                enemy.SetActive(true);

                if (!enemy.TryGetComponent<EnemyStats>(out var stats)) continue;
                if (!_allHandlers.TryGetValue(stats, out var handler)) continue;

                _activeEnemyStats.Add(stats);
                _activeEnemyHandlers.Add(handler);
                stats.OnDeath += handler;
            }

            // No enemies and no spawn points — open gate immediately.
            if (_activeEnemyStats.Count == 0)
                RoomCleared(index);
        }

        private void RoomCleared(int index)
        {
            if (index < 0 || index >= _rooms.Count) return;

            var room = _rooms[index];

            // Boss room — SpinCycleAI owns the win trigger via DefeatSequence.
            // Evaluated first so a boss room that also has an exitGate never
            // bypasses this guard and never double-triggers TriggerWin.
            if (room.bossOwnedWin) return;

            // Fire before opening the gate — GameManager listens to pause the game
            // and show the upgrade screen or shop before the player can proceed.
            OnRoomCleared?.Invoke(index);

            if (room.exitGate != null)
            {
                room.exitGate.Open();
                return;
            }

            // Signal win only for the final non-boss room (no gate and no boss override).
            if (index == _rooms.Count - 1)
                Boxhead.Core.GameManager.Instance?.TriggerWin();
        }

        // ─── Private helpers — spawn-point path ──────────────────────────────────

        /// <summary>
        /// Attempts to spawn one enemy from the next eligible spawn point.
        /// Advances _nextSpawnPointIndex when a point's quota is exhausted.
        /// Returns true if a new enemy was successfully spawned and tracked.
        /// </summary>
        private bool TrySpawnNext()
        {
            if (_activeSpawnPoints == null) return false;

            while (_nextSpawnPointIndex < _activeSpawnPoints.Count)
            {
                var sp = _activeSpawnPoints[_nextSpawnPointIndex];

                if (sp == null)
                {
                    _nextSpawnPointIndex++;
                    continue;
                }

                if (sp.HasMore)
                {
                    var go = sp.SpawnNext();
                    if (go == null)
                    {
                        // SpawnNext returned null despite HasMore — prefab missing, skip.
                        _nextSpawnPointIndex++;
                        continue;
                    }

                    // Increment alive count only when EnemyStats is confirmed present.
                    // A prefab missing EnemyStats cannot decrement the counter on death,
                    // so we destroy it immediately rather than blocking the room clear.
                    if (go.TryGetComponent<EnemyStats>(out var stats))
                    {
                        stats.OnDeath += _onSpawnedEnemyDeath;
                        _activeSpawnedStats.Add(stats);
                        _aliveCount++;
                        return true;
                    }
                    else
                    {
                        Debug.LogError(
                            $"[RoomManager] Spawned enemy '{go.name}' is missing EnemyStats. " +
                            "Destroying to prevent room-clear deadlock. Add EnemyStats to the prefab.",
                            go);
                        Destroy(go);
                        _nextSpawnPointIndex++;
                        continue;
                    }
                }
                else
                {
                    _nextSpawnPointIndex++;
                }
            }

            return false; // All spawn points exhausted.
        }

        /// <summary>
        /// Invoked when any spawn-wave enemy dies. Decrements the live count, refills
        /// one slot from the next available spawn point, and clears the room when both
        /// all points are exhausted and no enemies remain alive.
        /// </summary>
        private void OnSpawnedEnemyDied()
        {
            _aliveCount = Mathf.Max(0, _aliveCount - 1);

            if (!TrySpawnNext() && _aliveCount == 0)
                RoomCleared(_currentRoom);
        }

        // ─── Private helpers — legacy pre-placed path ─────────────────────────────

        private void OnEnemyDeath(EnemyStats deceased)
        {
            int idx = _activeEnemyStats.IndexOf(deceased);
            if (idx >= 0)
            {
                deceased.OnDeath -= _activeEnemyHandlers[idx];
                _activeEnemyStats.RemoveAt(idx);
                _activeEnemyHandlers.RemoveAt(idx);
            }

            // Sweep for any stale entries (destroyed mid-fight).
            for (int i = _activeEnemyStats.Count - 1; i >= 0; i--)
            {
                var s = _activeEnemyStats[i];
                if (s == null || s.IsDead)
                {
                    if (s != null) s.OnDeath -= _activeEnemyHandlers[i];
                    _activeEnemyStats.RemoveAt(i);
                    _activeEnemyHandlers.RemoveAt(i);
                }
            }

            if (_activeEnemyStats.Count == 0)
                RoomCleared(_currentRoom);
        }

        private void UnsubscribeAllActive()
        {
            // Spawn-point path — unsubscribe all live spawned enemies.
            // Prevents stale delegates firing into a destroyed RoomManager on scene reload.
            if (_onSpawnedEnemyDeath != null)
            {
                for (int i = 0; i < _activeSpawnedStats.Count; i++)
                {
                    if (_activeSpawnedStats[i] != null)
                        _activeSpawnedStats[i].OnDeath -= _onSpawnedEnemyDeath;
                }
            }
            _activeSpawnedStats.Clear();
            _aliveCount = 0;
            _nextSpawnPointIndex = 0;

            // Legacy pre-placed path.
            for (int i = 0; i < _activeEnemyStats.Count; i++)
            {
                if (_activeEnemyStats[i] != null)
                    _activeEnemyStats[i].OnDeath -= _activeEnemyHandlers[i];
            }
            _activeEnemyStats.Clear();
            _activeEnemyHandlers.Clear();
        }
    }
}
