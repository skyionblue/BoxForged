using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Systems;
using Boxhead.Player;

namespace Boxhead.UI
{
    /// <summary>
    /// Full-screen upgrade card picker shown between rooms.
    /// Displays 3 randomly selected UpgradeCardData assets. The player picks one
    /// and its effect is applied immediately; the screen then unpauses the game.
    ///
    /// Panel starts INACTIVE in the scene — do not call SetActive(false) in Awake.
    /// Call Show() from GameManager when a room clears.
    /// </summary>
    public class UpgradeScreen : MonoBehaviour
    {
        public static UpgradeScreen Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("Cards — exactly 3 slots")]
        [SerializeField] private Button[]          _cardButtons; // 3
        [SerializeField] private Image[]           _cardIcons;   // 3
        [SerializeField] private TextMeshProUGUI[] _cardNames;   // 3
        [SerializeField] private TextMeshProUGUI[] _cardDescs;   // 3

        [Header("Card Pool")]
        [SerializeField] private UpgradeCardData[] _cardPool;

        private readonly UpgradeCardData[] _offered = new UpgradeCardData[3];
        private PlayerStats       _playerStats;
        private CombatController  _combat;

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
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null)
            {
                Debug.LogWarning("[UpgradeScreen] No GameObject with tag 'Player' found.", this);
                return;
            }
            _playerStats = playerGO.GetComponent<PlayerStats>();
            _combat      = playerGO.GetComponent<CombatController>();

            if (_playerStats == null)
                Debug.LogWarning("[UpgradeScreen] Player found but missing PlayerStats — HealFlat upgrades will have no effect.", this);
        }

        public void Show()
        {
            DealCards();
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

        // ── Card dealing ──────────────────────────────────────────────────────

        private void DealCards()
        {
            int poolSize = _cardPool != null ? _cardPool.Length : 0;

            for (int i = 0; i < 3 && i < poolSize; i++)
            {
                int j = UnityEngine.Random.Range(i, poolSize);
                UpgradeCardData tmp = _cardPool[i];
                _cardPool[i] = _cardPool[j];
                _cardPool[j] = tmp;
                _offered[i]  = _cardPool[i];
            }

            for (int i = poolSize; i < 3; i++)
                _offered[i] = null;

            for (int i = 0; i < 3; i++)
            {
                int captured = i;
                _cardButtons[i].onClick.RemoveAllListeners();
                _cardButtons[i].onClick.AddListener(() => OnCardPicked(captured));

                UpgradeCardData card  = i < poolSize ? _offered[i] : null;
                bool            valid = card != null;
                _cardButtons[i].interactable = valid;

                if (_cardIcons[i] != null) _cardIcons[i].sprite = valid ? card.Icon : null;
                if (_cardNames[i] != null) _cardNames[i].SetText(valid ? card.DisplayName  : string.Empty);
                if (_cardDescs[i] != null) _cardDescs[i].SetText(valid ? card.Description  : string.Empty);
            }
        }

        // ── Card selection ────────────────────────────────────────────────────

        private void OnCardPicked(int index)
        {
            if (index >= _offered.Length || _offered[index] == null) return;
            ApplyCard(_offered[index]);
            Hide();
        }

        private void ApplyCard(UpgradeCardData card)
        {
            switch (card.Effect)
            {
                case UpgradeEffect.HealFlat:
                    _playerStats?.Heal(Mathf.RoundToInt(card.Magnitude));
                    break;

                case UpgradeEffect.AttackUp:
                case UpgradeEffect.DefenseUp:
                case UpgradeEffect.AgilityUp:
                case UpgradeEffect.LuckUp:
                    Boxhead.Core.ProgressionSystem.Instance?.ApplyRunUpgrade(card.Effect, card.Magnitude);
                    break;

                case UpgradeEffect.SpecialCooldownDown:
                    _combat?.ResetSpecialCooldown();
                    break;

                case UpgradeEffect.DodgeSpeedUp:
                    // Not yet wired — remove SwiftFeet/AgileWarrior from _cardPool Inspector until V3-09.
                    Debug.LogWarning($"[UpgradeScreen] DodgeSpeedUp picked but has no effect yet. Remove from card pool.", this);
                    break;

                default:
                    Debug.LogWarning($"[UpgradeScreen] Unhandled UpgradeEffect: {card.Effect}", this);
                    break;
            }
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale      = 1f;
            AudioListener.pause = false;

            if (_cardButtons == null) return;
            for (int i = 0; i < _cardButtons.Length; i++)
                if (_cardButtons[i] != null) _cardButtons[i].onClick.RemoveAllListeners();
        }
    }
}
