using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Boxhead.Enemy;
using Boxhead.Player;
using Boxhead.Systems;
using Boxhead.UI;

namespace Boxhead.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Maps scene names to zone indices for World Map progression tracking.
        // Add new zones here as they ship — unrecognized scenes return -1 (no update).
        internal static readonly Dictionary<string, int> ZoneIndexByScene = new Dictionary<string, int>
        {
            { "CulDeSac_Room1",        0 },
            { "CulDeSac_AmbushAlley",  0 },
            { "CulDeSac_SaloonFront",  0 },
            { "CulDeSac_MailboxRow",   0 },
            { "CulDeSac_BossArena",    0 },
            { "TownSquare_Room1",      1 },
            { "TownSquare_BossHall",   1 }, // Boss hall counts as same zone as Town Square Room 1
        };

        // Maps a zone index to the first scene the player enters when starting that zone.
        // Used by MetaScreen.OnContinue() to advance to the next zone after a win.
        internal static readonly Dictionary<int, string> ZoneStartScene = new Dictionary<int, string>
        {
            { 0, "CulDeSac_Room1"   },
            { 1, "TownSquare_Room1" },
        };

        // ── Random Room Pool (Sprint 3 Phase 2) ──────────────────────────────────
        // The three CulDeSac random rooms that follow Room 1 via the upgrade screen flow.
        private static readonly string[] RandomRoomPool = {
            "CulDeSac_AmbushAlley",
            "CulDeSac_SaloonFront",
            "CulDeSac_MailboxRow",
        };

        // Static so queue state survives scene reloads — GameManager is per-scene.
        private static List<string> s_roomQueue      = new List<string>();
        private static int          s_roomQueueIndex = 0;
        private        List<string> _roomQueue       = new List<string>();
        private        int          _roomQueueIndex  = 0;

        public enum GameState { Playing, Won, Lost }
        public GameState State { get; private set; } = GameState.Playing;

        [SerializeField] private GameOverUI      _gameOverUI;
        [SerializeField] private HUDController_V2 hudController;
        [SerializeField] private RunStartUI      _runStartUI;
        [SerializeField] private MetaScreen      _metaScreen;
        [SerializeField] private UpgradeScreen   _upgradeScreen;
        [SerializeField] private ShopScreen      _shopScreen;
        [SerializeField] private RunEndScreen    _runEndScreen;

        private PlayerStats          _playerStats;
        private Animator             _playerAnimator;
        private SaveSystem           _saveSystem;
        private SpinCycleAI          _spinCycleAI;          // cached at Start() to avoid FindObjectOfType per-death
        private PermitPulperBossAI   _permitPulperBossAI;   // cached at Start() to guard win condition for TownSquare
        private Boxhead.Systems.BossHallDoor _bossHallDoor; // cached at Start() — when present, the door owns progression

        private readonly List<EnemyStats> _trackedEnemies = new List<EnemyStats>();
        private int _livingEnemyCount;
        private int _totalEnemyCount; // Total that need to be killed to win
        private int _deadCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            QualitySettings.vSyncCount  = 0;  // must be 0 for targetFrameRate to work on iOS
            Application.targetFrameRate = 30;
        }

        private void Start()
        {
            // Cache SaveSystem here (not in Awake) — all Awake() calls across all GameObjects
            // complete before any Start() runs, so SaveSystem.Instance is guaranteed non-null.
            _saveSystem = SaveSystem.Instance;

            // Reset per-run progression state
            ProgressionSystem.Instance?.ResetRunState();

            // Build the shuffled random room queue for this run.
            InitRoomQueue();

            // Subscribe via the singleton instance (not a static event) to avoid
            // cross-scene delegate accumulation on scene reload.
            if (_upgradeScreen == null)
                _upgradeScreen = Object.FindAnyObjectByType<UpgradeScreen>(FindObjectsInactive.Include);
            if (_upgradeScreen != null)
                _upgradeScreen.OnUpgradeSelected += OnUpgradePicked;

            // Apply permanent stat overlay to player components at run start
            ProgressionSystem.Instance?.ApplyOverlayToPlayer();

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _playerStats    = playerObj.GetComponent<PlayerStats>();
                _playerAnimator = playerObj.GetComponentInChildren<Animator>();
                if (_playerStats != null)
                    _playerStats.OnDeath += HandlePlayerDeath;
            }

            // Cache boss AI types once at Start so CheckWinCondition never calls FindObjectOfType at runtime.
            _spinCycleAI        = Object.FindAnyObjectByType<SpinCycleAI>(FindObjectsInactive.Include);
            _permitPulperBossAI = Object.FindAnyObjectByType<PermitPulperBossAI>(FindObjectsInactive.Include);
            _bossHallDoor       = Object.FindAnyObjectByType<Boxhead.Systems.BossHallDoor>(FindObjectsInactive.Include);

            // Track initial enemies (the two bosses)
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
            {
                var stats = e.GetComponent<EnemyStats>();
                if (stats == null || stats.IsDead) continue;
                if (!stats.CountsForWinCondition) continue;
                _trackedEnemies.Add(stats);
                stats.OnDeath += OnTrackedEnemyDeath;
                _livingEnemyCount++;
            }

            // Subscribe to spawned enemies via static event
            EnemyStats.OnAnyEnemyDeath += OnAnyEnemyDeath;

            // Subscribe to room-clear events to show upgrade/shop screens between rooms.
            RoomManager.OnRoomCleared += HandleRoomCleared;

            if (_livingEnemyCount == 0 && enemies.Length == 0)
                Debug.LogWarning("[GameManager] No enemies found at Start — win condition will not trigger via enemy counter.");

            // Calculate total enemies: initial enemies + max spawns from spawner
            var spawnerScript = Object.FindAnyObjectByType<Boxhead.Enemy.EnemySpawner>(FindObjectsInactive.Include);
            if (spawnerScript == null)
                Debug.LogWarning("[GameManager] No EnemySpawner found in scene — using default maxSpawns of 20.");
            int maxSpawns = spawnerScript != null ? spawnerScript.MaxTotalSpawns : 20;
            _totalEnemyCount = _livingEnemyCount + maxSpawns;
            _deadCount = 0;

            UpdateEnemyCounter();

            // Restore any inspector refs that scene re-saves may have dropped.
            if (_runStartUI    == null) _runStartUI    = Object.FindAnyObjectByType<RunStartUI>(FindObjectsInactive.Include);
            if (_gameOverUI    == null) _gameOverUI    = Object.FindAnyObjectByType<GameOverUI>(FindObjectsInactive.Include);
            if (_runEndScreen  == null) _runEndScreen  = Object.FindAnyObjectByType<RunEndScreen>(FindObjectsInactive.Include);
            if (_metaScreen    == null) _metaScreen    = Object.FindAnyObjectByType<MetaScreen>(FindObjectsInactive.Include);
            if (hudController  == null) hudController  = Object.FindAnyObjectByType<HUDController_V2>(FindObjectsInactive.Include);

            _runStartUI?.Show();

            // Restore the in-run loadout (cardboard + forged weapons) LAST — after the
            // player is resolved and after RunStartUI.Show() swaps the character model.
            // Restoring earlier would equip the active weapon onto an inactive model's
            // hand bone. Gated on HasRunLoadout so the very first room is untouched.
            if (playerObj != null && ProgressionSystem.Instance != null
                && ProgressionSystem.Instance.HasRunLoadout)
            {
                var cardboard = playerObj.GetComponent<CardboardResource>();
                var inventory = playerObj.GetComponent<WeaponInventory>();
                ProgressionSystem.Instance.RestoreRunLoadout(cardboard, inventory);
            }

            TryPlayEntryCutscenes();
        }

        /// <summary>
        /// Plays boot / zone-entry cutscenes on scene load. Runs once at the end of Start():
        ///   • First boot ever → game intro (once ever).
        ///   • Entering the Cul-de-Sac zone start room → the wild-west transform (once per zone).
        /// Cutscenes render above the RunStartUI picker (which is paused via timeScale = 0) and
        /// play on the video's own clock, so no gameplay flow is blocked. There's no boot/menu
        /// scene, so the first playable room doubles as the intro trigger point.
        /// </summary>
        private void TryPlayEntryCutscenes()
        {
            var cutscene = CutscenePlayer.Instance;
            if (cutscene == null) return;

            string currentScene = SceneManager.GetActiveScene().name;

            // Game intro — first boot ever, at the first playable room (zone-0 start room).
            if (currentScene == ZoneStartScene[0]
                && !CutsceneFlags.HasSeen(CutsceneCatalog.KeyGameIntro))
            {
                CutsceneFlags.MarkSeen(CutsceneCatalog.KeyGameIntro);
                // Also mark the zone-enter as seen so the intro doesn't immediately chain into a
                // second cutscene on the very first boot; the zone cut plays on later fresh runs.
                CutsceneFlags.MarkSeen(CutsceneCatalog.KeyCulDeSacEnter);
                cutscene.Play(CutsceneCatalog.GameIntro, onFinished: null, skippable: true, showLoadingScreen: true);
                return;
            }

            // Enter Cul-de-Sac zone — once per zone, at its start room.
            if (currentScene == ZoneStartScene[0]
                && !CutsceneFlags.HasSeen(CutsceneCatalog.KeyCulDeSacEnter))
            {
                CutsceneFlags.MarkSeen(CutsceneCatalog.KeyCulDeSacEnter);
                cutscene.Play(CutsceneCatalog.CulDeSacEnter);
            }
        }

        /// <summary>
        /// Snapshots the player's cardboard and forged weapons into ProgressionSystem before
        /// a scene load so they survive the transition. Called at the top of LoadNextRoom()
        /// (room→room and room→boss) and by BossHallDoor before loading the boss scene.
        /// </summary>
        public void CaptureLoadoutForTransition()
        {
            if (ProgressionSystem.Instance == null) return;

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) return;

            var cardboard = playerObj.GetComponent<CardboardResource>();
            var inventory = playerObj.GetComponent<WeaponInventory>();
            ProgressionSystem.Instance.CaptureRunLoadout(cardboard, inventory);
        }

        private void OnTrackedEnemyDeath()
        {
            _livingEnemyCount = Mathf.Max(0, _livingEnemyCount - 1);
            CheckWinCondition();
        }

        private void OnAnyEnemyDeath()
        {
            // Called for all enemies including spawned grunts
            _deadCount++;
            CheckWinCondition();
        }

        private void CheckWinCondition()
        {
            UpdateEnemyCounter();

            // Win when: all initially tracked enemies (bosses) are dead AND no spawned enemies remain alive
            if (_livingEnemyCount > 0) return; // Bosses still alive

            // Boss AI types control TriggerWin via their own DefeatSequence coroutines
            // so the death animation can play before the win screen appears.
            // All three are cached at Start() — no per-death FindObjectOfType scan.
            if (_spinCycleAI != null) return;
            if (_permitPulperBossAI != null) return;
            // BossHallDoor owns progression in TownSquare — killing outdoor enemies only opens the door.
            if (_bossHallDoor != null) return;

            // Use counter-based check — no per-frame FindGameObjectsWithTag allocation.
            // _deadCount tracks all enemy deaths (bosses + spawned grunts via OnAnyEnemyDeath).
            if (_deadCount < _totalEnemyCount) return;

            // All enemies dead
            TriggerWin();
        }

        private void UpdateEnemyCounter()
        {
            if (hudController == null) return;

            // Show remaining enemies (starts at total, counts down)
            int remaining = _totalEnemyCount - _deadCount;
            hudController.SetEnemyCount(remaining);
        }

        private void HandlePlayerDeath()
        {
            if (State != GameState.Playing) return;
            State = GameState.Lost;

            if (_saveSystem != null)
            {
                _saveSystem.Data.totalRunsCompleted++;
                _saveSystem.Save();
            }

            AudioManager.Instance?.Play(SoundEvent.PlayerDeath);
            StartCoroutine(ShowDeathScreenDelayed());
        }

        private IEnumerator ShowDeathScreenDelayed()
        {
            // Brief pause so the death animation has a moment to play, then show immediately.
            // Previous version waited for normalizedTime < 0.9 but looping clips reset to 0,
            // causing the screen to delay until the 5s timeout played the animation 4 times.
            float waited = 0f;
            while (waited < 0.8f)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            _gameOverUI?.Show(won: false);
        }

        // Called by RoomManager when the final room is cleared, or directly when all tracked enemies die.
        public void TriggerWin()
        {
            if (State != GameState.Playing) return;
            State = GameState.Won;

            if (_saveSystem != null)
            {
                _saveSystem.Data.totalRunsCompleted++;

                // Completing zone N unlocks zone N+1 for the World Map.
                // Use zoneIndex+1 so beating CulDeSac (zone 0) makes TownSquare (zone 1) selectable.
                string currentScene = SceneManager.GetActiveScene().name;
                if (ZoneIndexByScene.TryGetValue(currentScene, out int zoneIndex))
                {
                    int unlocked = zoneIndex + 1;
                    if (unlocked > _saveSystem.Data.highestZoneReached)
                        _saveSystem.Data.highestZoneReached = unlocked;
                }

                _saveSystem.Save();
            }

            AudioManager.Instance?.Play(SoundEvent.EnemyDeath);

            // Award boss IP before showing the run-end screen so the summary is accurate.
            ProgressionSystem.Instance?.AddBossIP();

            // Run-end screen leads to MetaScreen via its Continue button.
            // GameOverUI win path is replaced by this flow — do not call gameOverUI.Show(won:true) here.
            _runEndScreen?.Show();
        }

        // ── Room progression ──────────────────────────────────────────────────

        /// <summary>
        /// Invoked by RoomManager.OnRoomCleared when a non-boss room is fully cleared.
        /// Room 0 (first combat room) → show upgrade screen.
        /// Room 1 (second combat room) → show shop before the boss.
        /// Higher indices are boss-owned and never fire this event.
        /// </summary>
        private void HandleRoomCleared(int roomIndex)
        {
            // Use the cached _bossHallDoor reference — never FindAnyObjectByType at runtime.
            if (_bossHallDoor != null) return;

            if (roomIndex == 0)
                _upgradeScreen?.Show();
            else if (roomIndex == 1)
                _shopScreen?.Show();
        }

        // ── Random Room Progression ───────────────────────────────────────────────

        /// <summary>
        /// Builds a Fisher-Yates shuffled queue of the three random CulDeSac rooms.
        /// Call once at the start of a run (after Room 1 loads).
        /// </summary>
        public void InitRoomQueue()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            // Only build a fresh queue when starting from Room 1.
            // Boss arena and random rooms preserve the existing static queue.
            if (currentScene == ZoneStartScene[0])
            {
                s_roomQueue = new List<string>(RandomRoomPool);
                for (int i = s_roomQueue.Count - 1; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    string tmp = s_roomQueue[i];
                    s_roomQueue[i] = s_roomQueue[j];
                    s_roomQueue[j] = tmp;
                }
                s_roomQueueIndex = 0;
            }
            _roomQueue      = s_roomQueue;
            _roomQueueIndex = s_roomQueueIndex;
        }

        /// <summary>
        /// Loads the next room from the shuffled queue. When all three random rooms
        /// are exhausted, falls back to CulDeSac_Room1 (loops the zone).
        /// </summary>
        public void LoadNextRoom()
        {
            // Snapshot cardboard + forged weapons before the scene unloads so they carry
            // into the next room (and into the boss arena on the exhausted-queue branch).
            CaptureLoadoutForTransition();

            if (_roomQueueIndex < _roomQueue.Count)
            {
                string nextScene = _roomQueue[_roomQueueIndex++];
                s_roomQueueIndex = _roomQueueIndex; // persist across scene reload
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                // All 3 random rooms done — load boss arena and reset queue for next run.
                s_roomQueue.Clear();
                s_roomQueueIndex = 0;
                SceneManager.LoadScene("CulDeSac_BossArena");
            }
        }

        /// <summary>Called when the player picks an upgrade card — loads the next queued room.</summary>
        private void OnUpgradePicked()
        {
            LoadNextRoom();
        }

        public void Restart()
        {
            // Always restart from Room1 so the run queue rebuilds correctly.
            // Reloading a random room mid-run would strand the player in that room forever.
            s_roomQueue.Clear();
            s_roomQueueIndex = 0;
            ProgressionSystem.Instance?.ClearRunSelection();
            ProgressionSystem.Instance?.ClearRunLoadout();
            SceneManager.LoadScene(ZoneStartScene[0]);
        }

        private void OnDestroy()
        {
            if (_playerStats != null) _playerStats.OnDeath -= HandlePlayerDeath;

            for (int i = 0; i < _trackedEnemies.Count; i++)
            {
                if (_trackedEnemies[i] != null)
                    _trackedEnemies[i].OnDeath -= OnTrackedEnemyDeath;
            }
            _trackedEnemies.Clear();

            EnemyStats.OnAnyEnemyDeath -= OnAnyEnemyDeath;
            RoomManager.OnRoomCleared  -= HandleRoomCleared;
            if (_upgradeScreen != null)
                _upgradeScreen.OnUpgradeSelected -= OnUpgradePicked;

            if (Instance == this) Instance = null;
        }
    }
}
