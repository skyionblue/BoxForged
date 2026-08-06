using System;
using UnityEngine;
using Boxhead.Enemy;
using Boxhead.Player;

namespace Boxhead.Core
{
    /// <summary>
    /// Singleton that manages XP accumulation, level-up logic, Spark earnings,
    /// and the permanent stat overlay rebuilt from save data each run.
    ///
    /// Survives scene reloads (DontDestroyOnLoad) so XP and event subscriptions
    /// are not lost when GameManager calls SceneManager.LoadScene on Restart().
    /// ResetRunState() must be called at the start of each new run to zero in-run XP.
    ///
    /// Boss spark bonus (_sparkPerBoss) is reserved for future use. EnemyStats.OnAnyEnemyDeath
    /// carries no parameter, so boss vs. grunt distinction requires a separate hook —
    /// wire it via a dedicated boss death event in SpinCycleAI when that system is extended.
    /// </summary>
    public class ProgressionSystem : MonoBehaviour
    {
        public static ProgressionSystem Instance { get; private set; }

        [SerializeField] private LevelData _levelData;
        [SerializeField] private int       _xpPerKill         = 10;
        [SerializeField] private int       _sparkPerKill      = 1;
        [SerializeField] private int       _sparkPerBoss      = 5;
        [SerializeField] private int       _statBonusPerLevel = 5;

        // ── IP / Spark tuning (all inspector-adjustable) ──────────────────────
        [SerializeField] private int _ipPerKill  = 10;
        [SerializeField] private int _ipPerBoss  = 50;
        /// <summary>How many IP it takes to earn 1 Spark at run end. 1 Spark per 10 IP.</summary>
        [SerializeField] private int _sparkPerIP = 10;

        // _currentLevel starts at 1: "level 1" means working toward xpThresholds[0].
        // GetThreshold(0) returns int.MaxValue (out of range) — never use level 0.
        private int         _currentXP    = 0;
        private int         _currentLevel = 1;
        private StatOverlay _overlay;
        private StatOverlay _runOverlay;

        // ── In-run IP and combo tracking ──────────────────────────────────────
        private int   _currentIP       = 0;
        private float _comboMultiplier = 1f;
        private int   _killsThisRun    = 0;

        private const float ComboStep    = 0.1f;
        private const float ComboMax     = 3f;
        private const float ComboDefault = 1f;

        public int         CurrentXP       => _currentXP;
        public int         CurrentLevel    => _currentLevel;
        public int         SparkTotal      => SaveSystem.Instance != null ? SaveSystem.Instance.Data.sparkTotal : 0;
        public StatOverlay Overlay         => _overlay;
        public StatOverlay TotalOverlay => new StatOverlay
        {
            maxHealthBonus    = _overlay.maxHealthBonus    + _runOverlay.maxHealthBonus,
            attackPowerBonus  = _overlay.attackPowerBonus  + _runOverlay.attackPowerBonus,
            agilityBonus      = _overlay.agilityBonus      + _runOverlay.agilityBonus,
            luckBonus         = _overlay.luckBonus         + _runOverlay.luckBonus,
            defenseBonus      = _overlay.defenseBonus      + _runOverlay.defenseBonus,
        };
        public int         CurrentIP       => _currentIP;
        public float       ComboMultiplier => _comboMultiplier;
        public int         KillsThisRun    => _killsThisRun;

        // ── Run selection persistence (session-scoped, not saved to disk) ─────────
        public bool HasRunSelection { get; private set; }
        public int  RunGender       { get; private set; }   // 0=Male  1=Female
        public int  RunStyle        { get; private set; }   // 0=Ninja 1=Cowboy
        public int  RunBoxIndex     { get; private set; }
        public int  RunDifficulty   { get; private set; }   // 0=Easy 1=Medium 2=Hard

        public void SetRunSelection(int gender, int style, int boxIndex, int difficulty)
        {
            RunGender       = gender;
            RunStyle        = style;
            RunBoxIndex     = boxIndex;
            RunDifficulty   = difficulty;
            HasRunSelection = true;
        }

        public void UpdateBoxIndex(int index) => RunBoxIndex = index;

        /// <summary>Clears the stored run selection so the next scene load shows the character picker.</summary>
        public void ClearRunSelection() => HasRunSelection = false;

        // ── Run loadout persistence (session-scoped, not saved to disk) ───────────
        // Mirrors the HasRunSelection pattern: snapshot the player's cardboard and forged
        // weapon slots at each room transition, restore them after the next scene loads.
        // Cleared only on a brand-new run or a new zone — never on a room→room transition.
        // NOTE: ResetRunState() must NOT touch these fields — it runs every room load.

        private struct StoredWeapon
        {
            public Boxhead.Systems.WeaponObjectSO data;
            public Boxhead.Systems.WeaponTier     tier;
            public int                            durability;
        }

        public bool HasRunLoadout      { get; private set; }
        public int  RunCardboard       { get; private set; }
        public int  RunActiveSlotIndex { get; private set; }

        // Sized to WeaponInventory.WeaponSlotCount lazily on first capture.
        private StoredWeapon[] _runWeaponSlots;

        /// <summary>
        /// Snapshots the player's cardboard count and forged weapon slots into session state.
        /// Called at every room transition (via GameManager.CaptureLoadoutForTransition).
        /// The material bag is deliberately not captured (persistence covers slots only).
        /// </summary>
        public void CaptureRunLoadout(Boxhead.Systems.CardboardResource cardboard,
                                      Boxhead.Systems.WeaponInventory inventory)
        {
            if (inventory == null) return;

            RunCardboard = cardboard != null ? cardboard.Current : 0;

            var slots = inventory.WeaponSlots;
            int count = slots != null ? slots.Length : 0;

            if (_runWeaponSlots == null || _runWeaponSlots.Length != count)
                _runWeaponSlots = new StoredWeapon[count];

            for (int i = 0; i < count; i++)
            {
                var weapon = slots[i];
                if (weapon != null)
                {
                    _runWeaponSlots[i] = new StoredWeapon
                    {
                        data       = weapon.Data,
                        tier       = weapon.Tier,
                        durability = weapon.CurrentDurability
                    };
                }
                else
                {
                    _runWeaponSlots[i] = default;
                }
            }

            RunActiveSlotIndex = inventory.ActiveSlotIndex;
            HasRunLoadout      = true;
        }

        /// <summary>
        /// Rebuilds WeaponInstances from the captured snapshot and pushes them, the active
        /// slot index, and the cardboard count back onto the freshly loaded player.
        /// Call from GameManager.Start() AFTER the character model swap (RunStartUI.Show)
        /// so the restored active weapon equips onto the correct hand bone.
        /// </summary>
        public void RestoreRunLoadout(Boxhead.Systems.CardboardResource cardboard,
                                      Boxhead.Systems.WeaponInventory inventory)
        {
            if (!HasRunLoadout) return;

            cardboard?.SetCurrent(RunCardboard);

            if (inventory == null || _runWeaponSlots == null) return;

            var restored = new Boxhead.Systems.WeaponInstance[_runWeaponSlots.Length];
            for (int i = 0; i < _runWeaponSlots.Length; i++)
            {
                var stored = _runWeaponSlots[i];
                restored[i] = stored.data != null
                    ? new Boxhead.Systems.WeaponInstance(stored.data, stored.tier, stored.durability)
                    : null;
            }

            inventory.RestoreState(restored, RunActiveSlotIndex);
        }

        /// <summary>
        /// Clears the persisted loadout so the next run/zone starts fresh (no cardboard,
        /// no forged weapons). Called on Restart(), fresh character pick, and zone advance.
        /// </summary>
        public void ClearRunLoadout()
        {
            HasRunLoadout      = false;
            RunCardboard       = 0;
            RunActiveSlotIndex = 0;
            _runWeaponSlots    = null;
        }

        // Reserved for V3-06 boss hook — EnemyStats.OnAnyEnemyDeath carries no parameter today.
        public int SparkPerBoss => _sparkPerBoss;

        public event Action<int>   OnXPChanged;    // new XP total
        public event Action<int>   OnLevelUp;      // new level
        public event Action<int>   OnSparkChanged; // new spark total
        public event Action<int>   OnIPChanged;    // new IP total (in-run only)
        public event Action<float> OnComboChanged; // new combo multiplier

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe here in Awake (not Start) so no kill fired between Awake and Start
            // can slip past the subscription window. Safe because this is a DontDestroyOnLoad
            // singleton — Awake runs once and the subscription persists across scene reloads.
            EnemyStats.OnAnyEnemyDeath += HandleKill;

            RebuildOverlay();
        }

        private void OnDestroy()
        {
            EnemyStats.OnAnyEnemyDeath -= HandleKill;
            if (Instance == this) Instance = null;
        }

        // --- Kill handling -------------------------------------------------------

        private void HandleKill()
        {
            // ── XP (level system) ─────────────────────────────────────────────
            _currentXP += _xpPerKill;
            OnXPChanged?.Invoke(_currentXP);

            // Loop to handle multiple level-ups from a single large XP gain.
            // XP carries forward (no reset to zero) — the surplus applies to the next threshold.
            if (_levelData != null)
            {
                while (_currentXP >= _levelData.GetThreshold(_currentLevel))
                    LevelUp();
            }

            // ── IP (in-run currency) + combo multiplier ───────────────────────
            int ip = Mathf.RoundToInt(_ipPerKill * _comboMultiplier);
            _currentIP     += ip;
            _killsThisRun++;
            _comboMultiplier = Mathf.Min(ComboMax, _comboMultiplier + ComboStep);
            OnIPChanged?.Invoke(_currentIP);
            OnComboChanged?.Invoke(_comboMultiplier);

            // ── Spark (meta currency, persisted) ─────────────────────────────
            if (SaveSystem.Instance == null)
            {
                Debug.LogWarning("[ProgressionSystem] SaveSystem not available — Spark not awarded for kill.");
                return;
            }

            SaveSystem.Instance.Data.sparkTotal += _sparkPerKill;
            OnSparkChanged?.Invoke(SparkTotal);
        }

        private void LevelUp()
        {
            _currentLevel++;

            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Data.characterLevel = _currentLevel;

            OnLevelUp?.Invoke(_currentLevel);
        }

        // --- Overlay -------------------------------------------------------------

        /// <summary>
        /// Accumulates an in-run upgrade card effect into _runOverlay.
        /// HealFlat and SpecialCooldownDown are immediate effects handled by UpgradeScreen
        /// directly — they are not overlay-based and should not be passed here.
        /// </summary>
        public void ApplyRunUpgrade(Boxhead.Systems.UpgradeEffect effect, float magnitude)
        {
            switch (effect)
            {
                case Boxhead.Systems.UpgradeEffect.AttackUp:
                    _runOverlay.attackPowerBonus += Mathf.RoundToInt(magnitude);
                    break;
                case Boxhead.Systems.UpgradeEffect.DefenseUp:
                    _runOverlay.defenseBonus += Mathf.RoundToInt(magnitude);
                    break;
                case Boxhead.Systems.UpgradeEffect.AgilityUp:
                    _runOverlay.agilityBonus += magnitude;
                    break;
                case Boxhead.Systems.UpgradeEffect.LuckUp:
                    _runOverlay.luckBonus += magnitude;
                    break;
                // HealFlat and SpecialCooldownDown are immediate effects, not overlay-based
            }
        }

        /// <summary>
        /// Pushes TotalOverlay stat bonuses to the live player components.
        /// Call at run start after RebuildOverlay() so permanent Spark upgrades
        /// are active from the first frame.
        /// </summary>
        public void ApplyOverlayToPlayer()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null) return;

            if (playerGO.TryGetComponent<Boxhead.Player.PlayerStats>(out var stats))
                stats.SetMaxHealthBonus(TotalOverlay.maxHealthBonus);
        }

        /// <summary>
        /// Reads SaveData.statLevels[] and rebuilds the stat overlay from scratch.
        /// Call once at run start and after any stat upgrade purchase.
        /// </summary>
        public void RebuildOverlay()
        {
            if (SaveSystem.Instance == null) return;

            var levels = SaveSystem.Instance.Data.statLevels;
            _overlay = StatOverlay.Zero;

            if (levels == null || levels.Length < 5) return;

            _overlay.maxHealthBonus   = levels[0] * _statBonusPerLevel;
            _overlay.attackPowerBonus = levels[1] * _statBonusPerLevel;
            _overlay.agilityBonus     = levels[2] * _statBonusPerLevel * 0.1f;
            _overlay.luckBonus        = levels[3] * _statBonusPerLevel * 0.1f;
            _overlay.defenseBonus     = levels[4] * _statBonusPerLevel;
        }

        // --- Upgrade -------------------------------------------------------------

        /// <summary>
        /// Spends sparkCost, increments the stat at statIndex, saves, and
        /// rebuilds the overlay. Fires OnSparkChanged with the new total.
        /// Stat index mapping: 0=MaxHealth, 1=AttackPower, 2=Agility, 3=Luck, 4=Defense.
        /// </summary>
        public void UpgradeStat(int statIndex, int sparkCost)
        {
            if (statIndex < 0) return;
            if (SaveSystem.Instance == null) return;

            var data = SaveSystem.Instance.Data;

            // Guard against null or undersized array rather than magic-number 4.
            if (data.statLevels == null || statIndex >= data.statLevels.Length) return;
            if (data.sparkTotal < sparkCost) return;

            data.sparkTotal -= sparkCost;
            data.statLevels[statIndex]++;

            SaveSystem.Instance.Save();
            RebuildOverlay();
            OnSparkChanged?.Invoke(SparkTotal);
        }

        // --- Run lifecycle -------------------------------------------------------

        /// <summary>
        /// Resets in-run XP and level to their starting values.
        /// Call at the start of each new run before RebuildOverlay().
        /// Level resets to 1 (not 0) — level 0 is out of range in LevelData.GetThreshold.
        /// </summary>
        public void ResetRunState()
        {
            _currentXP       = 0;
            _currentLevel    = 1;
            _currentIP       = 0;
            _killsThisRun    = 0;
            _comboMultiplier = ComboDefault;
            _runOverlay      = StatOverlay.Zero;

            // Keep SaveData in sync so readers see level 1 from the start of every run,
            // not 0 (the SaveData default before the first level-up event).
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Data.characterLevel = 1;

        }

        // ── IP public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Awards boss IP. Call this from GameManager when the boss dies, before showing RunEndScreen.
        /// </summary>
        public void AddBossIP()
        {
            _currentIP += _ipPerBoss;
            _killsThisRun++;
            OnIPChanged?.Invoke(_currentIP);
        }

        /// <summary>
        /// Resets the combo multiplier to 1×. Call this when the player takes damage.
        /// </summary>
        public void ResetCombo()
        {
            if (_comboMultiplier <= ComboDefault) return;
            _comboMultiplier = ComboDefault;
            OnComboChanged?.Invoke(_comboMultiplier);
        }

        /// <summary>
        /// Converts accumulated in-run IP into permanent Spark at the configured rate and saves.
        /// Returns the number of Spark earned. Call once at run end before resetting run state.
        /// </summary>
        public int ConvertIPToSpark()
        {
            if (SaveSystem.Instance == null) return 0;

            int earned = _currentIP / _sparkPerIP;
            _currentIP = 0;   // zero after conversion so double-calls are safe
            OnIPChanged?.Invoke(_currentIP);
            if (earned <= 0) return 0;

            SaveSystem.Instance.Data.sparkTotal += earned;
            SaveSystem.Instance.Save();
            OnSparkChanged?.Invoke(SparkTotal);
            return earned;
        }

        /// <summary>
        /// Spends in-run IP. Used by ShopScreen for item purchases. Clamps to zero.
        /// </summary>
        public void SpendIP(int amount)
        {
            _currentIP = Mathf.Max(0, _currentIP - amount);
            OnIPChanged?.Invoke(_currentIP);
        }

        /// <summary>
        /// Fires OnSparkChanged with the current total so any live UI (MetaScreen, HUD)
        /// refreshes after an external reset without needing to reload the scene.
        /// </summary>
        public void ForceRefreshSparkUI()
        {
            OnSparkChanged?.Invoke(SparkTotal);
        }
    }
}
