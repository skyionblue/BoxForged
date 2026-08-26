using System;
using System.Collections;
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

        [Tooltip("ADR-0002: optional. When assigned, RoomManager pulls this scene's LevelBuilder.RoomData (RoomDataSO[]) at Start() and appends a runtime RoomData per asset, built via LevelBuilder.BuildSpawnPoints(). Leave null for legacy scenes that still author _rooms entirely by hand in the Inspector — nothing below changes for them.")]
        [SerializeField] private LevelBuilder _levelBuilder;

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

        // ─── Room-clear delay ─────────────────────────────────────────────────────
        // Delay before firing OnRoomCleared — lets the last enemy's death animation
        // finish before the upgrade/shop screen appears. Matches BasicEnemyAI.DieRoutine
        // (0.5 s) plus a small buffer. Allocated once in Awake, never re-allocated.
        [SerializeField] private float _roomClearedDelay = 0.7f;
        private WaitForSeconds _waitRoomCleared;
        // True while the delayed RoomCleared coroutine is in flight — prevents a
        // second call from double-firing OnRoomCleared if two deaths occur on the same frame.
        private bool _roomClearedPending;

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
            _waitRoomCleared = new WaitForSeconds(_roomClearedDelay);
        }

        private void Start()
        {
            // B49: fall back to finding the scene's LevelBuilder even when _levelBuilder
            // was never explicitly wired (every scene has exactly one — see ADR-0002).
            // This is what lets the NavMesh-readiness fix below apply uniformly to old,
            // Inspector-authored scenes too, not just new RoomDataSO-driven ones — those
            // scenes currently avoid B49 only by accident (a leftover pre-baked
            // Scenes/<name>/NavMesh.asset masks the race), not by correct sequencing.
            // One-time lookup at Start(), not a hot path — matches the same
            // find-if-not-assigned pattern GameManager already uses for its UI screens.
            if (_levelBuilder == null)
                _levelBuilder = FindAnyObjectByType<LevelBuilder>(FindObjectsInactive.Include);

            // ADR-0002: pull any data-driven rooms from this scene's LevelBuilder before
            // anything below iterates _rooms. Appended after any Inspector-configured
            // legacy rooms, so a scene can (in principle) mix both — new scenes simply
            // start with an empty _rooms and get everything from here.
            AppendRoomsFromLevelBuilder();

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

            // Room 0 activates once a NavMesh actually exists — see BeginRoom0WhenNavMeshReady.
            BeginRoom0WhenNavMeshReady();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_levelBuilder != null) _levelBuilder.OnNavMeshReady -= HandleNavMeshReady;
            UnsubscribeAllActive();
        }

        /// <summary>
        /// B49: activating room 0 (which spawns NavMeshAgent enemies via TrySpawnNext)
        /// used to happen synchronously in Start(), one frame before LevelBuilder's own
        /// deferred NavMesh bake completes — a real race, previously hidden in every old
        /// scene only by a leftover pre-baked NavMesh.asset from before ADR-0002. Waits
        /// for LevelBuilder.OnNavMeshReady instead of changing the bake's own timing,
        /// which is deferred for a correctness reason (freshly-Instantiated colliders
        /// need a frame to register with physics), not a performance one.
        /// </summary>
        private void BeginRoom0WhenNavMeshReady()
        {
            if (_levelBuilder == null || _levelBuilder.IsNavMeshReady)
            {
                // No LevelBuilder in this scene, or the bake somehow already finished
                // (e.g. Start() ran after it) — activate immediately, exactly as before.
                ActivateRoom(0);
                return;
            }

            _levelBuilder.OnNavMeshReady += HandleNavMeshReady;
        }

        private void HandleNavMeshReady()
        {
            _levelBuilder.OnNavMeshReady -= HandleNavMeshReady;
            ActivateRoom(0);
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
        /// ADR-0004: true when a later zone exists in this scene — progression is in-scene
        /// (the next zone's RoomTrigger), not a scene load. GameManager.OnUpgradePicked reads
        /// this to skip LoadNextRoom() for single-scene worlds (e.g. CulDeSac_WildWestCity)
        /// while leaving the legacy scene-per-room path (every RoomDataSO-per-scene world)
        /// untouched — there _rooms.Count is always 1, so this is always false there.
        /// </summary>
        public bool HasZoneAfterCurrent => _currentRoom >= 0 && _currentRoom < _rooms.Count - 1;

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

        /// <summary>
        /// ADR-0002: converts this scene's LevelBuilder.RoomData (RoomDataSO[]) into
        /// runtime RoomData entries and appends them to _rooms. No-ops entirely when
        /// _levelBuilder is unassigned (legacy scenes) or RoomData is empty, so this
        /// is purely additive for scenes that don't use the new data-driven path.
        /// exitGate/propsGroup are intentionally left null/default here — they are
        /// scene-local objects with nothing portable to bind to a data asset; a room
        /// that needs them can still set them via the legacy Inspector-authored path.
        /// </summary>
        private void AppendRoomsFromLevelBuilder()
        {
            if (_levelBuilder == null) return;

            RoomDataSO[] roomDataAssets = _levelBuilder.RoomData;
            if (roomDataAssets == null) return;

            for (int i = 0; i < roomDataAssets.Length; i++)
            {
                RoomDataSO so = roomDataAssets[i];
                if (so == null) continue;

                var room = new RoomData
                {
                    roomName = so.roomName,
                    maxConcurrentEnemies = so.maxConcurrentEnemies,
                    bossOwnedWin = so.bossOwnedWin,
                    spawnPoints = _levelBuilder.BuildSpawnPoints(so.spawnPoints)
                };
                _rooms.Add(room);
            }
        }

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

            // Boss room — SpinCycleAI / PermitPulperBossAI owns the win trigger via
            // DefeatSequence. Evaluated first so a boss room that also has an exitGate
            // never bypasses this guard and never double-triggers TriggerWin.
            if (room.bossOwnedWin) return;

            // Guard against re-entrant calls — two deaths on the same frame or the
            // edge-case guard firing while a prior clear is already pending.
            if (_roomClearedPending) return;
            _roomClearedPending = true;

            StartCoroutine(RoomClearedDelayed(index));
        }

        /// <summary>
        /// Waits for the last enemy's death animation to finish before signalling
        /// the upgrade/shop screen. The delay matches BasicEnemyAI.DieRoutine (0.5 s)
        /// plus a small buffer so the corpse fades before the UI appears.
        /// </summary>
        private IEnumerator RoomClearedDelayed(int index)
        {
            yield return _waitRoomCleared;

            if (index < 0 || index >= _rooms.Count) yield break;
            var room = _rooms[index];

            // Fire before opening the gate — GameManager listens to show the
            // upgrade screen or shop before the player can advance.
            OnRoomCleared?.Invoke(index);

            if (room.exitGate != null)
                room.exitGate.Open();

            // TriggerWin is NEVER called from RoomManager. Win is triggered exclusively
            // by boss AI (SpinCycleAI, PermitPulperBossAI) via their own DefeatSequence.
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
            // Cancel any in-flight room-clear delay so it does not fire after the room
            // has been reset (e.g. ResetForRestart or a future room-transition).
            StopAllCoroutines();
            _roomClearedPending = false;

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
