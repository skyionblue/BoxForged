using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Core;

namespace Boxhead.UI
{
    /// <summary>
    /// End-of-run meta screen for spending Spark on permanent stat upgrades.
    /// Panel starts INACTIVE in the scene — do not call SetActive(false) in Awake.
    /// Call Show() from GameManager or RunEndScreen when the run ends.
    ///
    /// Stat index mapping (matches ProgressionSystem.UpgradeStat):
    ///   0 = MaxHealth  1 = AttackPower  2 = Agility  3 = Luck  4 = Defense
    /// </summary>
    public class MetaScreen : MonoBehaviour
    {
        public static MetaScreen Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Wire button listeners once in Awake — avoids per-Show lambda allocation.
            if (_continueButton != null) _continueButton.onClick.AddListener(OnContinue);
            if (_worldMapButton  != null) _worldMapButton.onClick.AddListener(OnWorldMap);
            for (int i = 0; i < _upgradeButtons.Length; i++)
            {
                int index = i;
                _upgradeButtons[i].onClick.AddListener(() => OnUpgradeClicked(index));
            }
        }



        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI _sparkText;
        [SerializeField] private TextMeshProUGUI _runsText;

        [Header("Stat Buttons (order: MaxHealth, AttackPower, Agility, Luck, Defense)")]
        [SerializeField] private Button[]          _upgradeButtons;  // 5 buttons
        [SerializeField] private TextMeshProUGUI[] _statLevelTexts;  // 5 labels

        [Header("Upgrade")]
        [SerializeField] private int _upgradeCost = 10;

        [Header("Navigation")]
        [SerializeField] private Button        _continueButton;
        [SerializeField] private Button        _worldMapButton;
        [SerializeField] private WorldMapScreen _worldMapScreen;

        // -----------------------------------------------------------------------

        public void Show()
        {
            if (_panel == null)
            {
                Debug.LogError("[MetaScreen] _panel is not assigned — cannot open MetaScreen.", this);
                return;
            }
            _panel.SetActive(true);
            Time.timeScale          = 0f;
            AudioListener.pause     = true;
            RefreshUI();
        }

        public void Hide()
        {
            AudioListener.pause = false;
            Time.timeScale      = 1f;
            _panel.SetActive(false);
        }

        private void OnContinue()
        {
            Hide();

            // If there is a next zone unlocked, load it with a fresh character picker.
            // e.g. winning CulDeSac (highestZone=1) → loads TownSquare_Room1.
            // If no next zone is mapped (final zone), fall back to restarting the current zone.
            var save = Boxhead.Core.SaveSystem.Instance;
            if (save != null)
            {
                int nextZone = save.Data.highestZoneReached;
                if (Boxhead.Core.GameManager.ZoneStartScene.TryGetValue(nextZone, out string nextScene))
                {
                    Boxhead.Core.ProgressionSystem.Instance?.ClearRunSelection();
                    UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
                    return;
                }
            }

            Boxhead.Core.GameManager.Instance?.Restart();
        }

        private void OnWorldMap()
        {
            // Self-heal: find WorldMapScreen if inspector ref was dropped by a scene re-save.
            if (_worldMapScreen == null)
                _worldMapScreen = Object.FindAnyObjectByType<WorldMapScreen>(FindObjectsInactive.Include);
            Hide();
            _worldMapScreen?.Show(this);
        }

        // -----------------------------------------------------------------------

        private void RefreshUI()
        {
            if (SaveSystem.Instance == null) return;

            var data = SaveSystem.Instance.Data;

            // SetText(string, T) avoids the string allocation that $"..." interpolation causes.
            if (_sparkText != null)
                _sparkText.SetText("Spark: {0}", data.sparkTotal);

            if (_runsText != null)
                _runsText.SetText("Runs: {0}", data.totalRunsCompleted);

            for (int i = 0; i < _upgradeButtons.Length; i++)
            {
                if (_upgradeButtons[i] == null) continue;
                _upgradeButtons[i].interactable = data.sparkTotal >= _upgradeCost;
            }

            for (int i = 0; i < _statLevelTexts.Length; i++)
            {
                if (_statLevelTexts[i] == null) continue;

                int level = (data.statLevels != null && i < data.statLevels.Length)
                    ? data.statLevels[i]
                    : 0;
                _statLevelTexts[i].SetText("Lv {0}", level);
            }
        }

        private void OnUpgradeClicked(int index)
        {
            if (ProgressionSystem.Instance == null) return;

            ProgressionSystem.Instance.UpgradeStat(index, _upgradeCost);
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_continueButton != null) _continueButton.onClick.RemoveAllListeners();
            if (_worldMapButton != null) _worldMapButton.onClick.RemoveAllListeners();
            if (_upgradeButtons == null) return;
            for (int i = 0; i < _upgradeButtons.Length; i++)
                if (_upgradeButtons[i] != null) _upgradeButtons[i].onClick.RemoveAllListeners();
        }
    }
}
