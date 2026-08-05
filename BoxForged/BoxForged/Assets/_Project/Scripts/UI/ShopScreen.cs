using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Player;
using Boxhead.Core;

namespace Boxhead.UI
{
    /// <summary>
    /// Pre-boss shop screen. Lets the player spend in-run IP on a health refill,
    /// an upgrade card, or simply enter the boss fight.
    ///
    /// Panel starts INACTIVE in the scene — do not call SetActive(false) in Awake.
    /// Call Show() from GameManager after Room 2 clears.
    /// </summary>
    public class ShopScreen : MonoBehaviour
    {
        public static ShopScreen Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("IP Display")]
        [SerializeField] private TextMeshProUGUI _ipText;

        [Header("Buttons")]
        [SerializeField] private Button _buyHealthButton;
        [SerializeField] private Button _buyUpgradeButton;
        [SerializeField] private Button _enterBossFightButton;

        [Header("Prices")]
        [SerializeField] private int _healthCost  = 30;
        [SerializeField] private int _upgradeCost = 20;

        [Header("Health Refill")]
        [SerializeField] private int _healthRefillAmount = 50;

        private PlayerStats _playerStats;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            // Intentionally NOT DontDestroyOnLoad — this screen is scene-scoped and re-created on restart.
            Instance = this;
        }

        private void Start()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
                _playerStats = playerGO.GetComponent<PlayerStats>();

            _buyHealthButton?.onClick.AddListener(OnBuyHealth);
            _buyUpgradeButton?.onClick.AddListener(OnBuyUpgrade);
            _enterBossFightButton?.onClick.AddListener(OnEnterBossFight);
        }

        public void Show()
        {
            if (ProgressionSystem.Instance == null)
                Debug.LogWarning("[ShopScreen] ProgressionSystem not available — shop will display zero IP.", this);
            RefreshUI();
            _panel.SetActive(true);
            Time.timeScale      = 0f;
            AudioListener.pause = true;
        }

        public void Hide()
        {
            _panel.SetActive(false);
            Time.timeScale      = 1f;
            AudioListener.pause = false;
        }

        // ── UI ────────────────────────────────────────────────────────────────

        private void RefreshUI()
        {
            int ip = ProgressionSystem.Instance != null ? ProgressionSystem.Instance.CurrentIP : 0;
            if (_ipText != null) _ipText.SetText("IP: {0}", ip);
            if (_buyHealthButton  != null) _buyHealthButton.interactable  = ip >= _healthCost;
            if (_buyUpgradeButton != null) _buyUpgradeButton.interactable = ip >= _upgradeCost;
        }

        // ── Button handlers ───────────────────────────────────────────────────

        private void OnBuyHealth()
        {
            var prog = ProgressionSystem.Instance;
            if (prog == null || prog.CurrentIP < _healthCost) return;
            if (_playerStats == null)
            {
                Debug.LogWarning("[ShopScreen] PlayerStats not found — health refill cancelled.", this);
                return;
            }
            prog.SpendIP(_healthCost);
            _playerStats.Heal(_healthRefillAmount);
            RefreshUI();
        }

        private void OnBuyUpgrade()
        {
            var prog = ProgressionSystem.Instance;
            if (prog == null || prog.CurrentIP < _upgradeCost) return;
            prog.SpendIP(_upgradeCost);
            Hide();
            UpgradeScreen.Instance?.Show();
        }

        private void OnEnterBossFight()
        {
            Hide();
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            // Only restore time if this screen was the one that paused it —
            // avoids un-pausing the game if GameOverUI ran first.
            if (_panel != null && _panel.activeSelf)
            {
                Time.timeScale      = 1f;
                AudioListener.pause = false;
            }

            _buyHealthButton?.onClick.RemoveAllListeners();
            _buyUpgradeButton?.onClick.RemoveAllListeners();
            _enterBossFightButton?.onClick.RemoveAllListeners();
        }
    }
}
