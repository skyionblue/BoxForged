using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Core;

namespace Boxhead.UI
{
    /// <summary>
    /// Post-run summary screen shown after the boss dies.
    /// Displays IP earned, kills, and Spark converted this run.
    /// "Continue" leads to the MetaScreen for permanent stat upgrades.
    ///
    /// Panel starts INACTIVE in the scene — do not call SetActive(false) in Awake.
    /// Call Show() from GameManager inside TriggerWin().
    /// </summary>
    public class RunEndScreen : MonoBehaviour
    {
        public static RunEndScreen Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _ipEarnedText;
        [SerializeField] private TextMeshProUGUI _killsText;
        [SerializeField] private TextMeshProUGUI _sparkEarnedText;

        [Header("Navigation")]
        [SerializeField] private Button _continueButton;

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
            _continueButton?.onClick.AddListener(OnContinue);
        }

        public void Show()
        {
            // Guard _panel before ConvertIPToSpark — conversion writes to disk and cannot be undone.
            if (_panel == null)
            {
                Debug.LogError("[RunEndScreen] _panel is not assigned. Spark conversion aborted.", this);
                return;
            }

            var prog = ProgressionSystem.Instance;

            // Read IP and kills BEFORE ConvertIPToSpark() — that method zeroes _currentIP.
            // Reordering these lines would display 0 IP earned.
            int ip    = prog != null ? prog.CurrentIP    : 0;
            int kills = prog != null ? prog.KillsThisRun : 0;
            int spark = prog != null ? prog.ConvertIPToSpark() : 0;

            if (_ipEarnedText    != null) _ipEarnedText.SetText("IP Earned: {0}",    ip);
            if (_killsText       != null) _killsText.SetText("Kills: {0}",           kills);
            if (_sparkEarnedText != null) _sparkEarnedText.SetText("Spark Earned: {0}", spark);

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

        private void OnContinue()
        {
            Hide();
            MetaScreen.Instance?.Show();
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            // Only restore time if this screen was the one that paused it.
            if (_panel != null && _panel.activeSelf)
            {
                Time.timeScale      = 1f;
                AudioListener.pause = false;
            }

            _continueButton?.onClick.RemoveAllListeners();
        }
    }
}
