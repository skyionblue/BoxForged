using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Core;
using Boxhead.Player;
using Boxhead.Systems;

namespace Boxhead.UI
{
    public class HUDController_V2 : MonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField] private Image _healthFill;

        [Header("IP Counter")]
        [SerializeField] private TextMeshProUGUI _ipCounterText;

        [Header("Enemy Counter")]
        [SerializeField] private TextMeshProUGUI _enemyCounterText;

        [Header("Special Button")]
        [SerializeField] private GameObject _specialButtonRoot;
        [SerializeField] private GameObject _specialButtonInner;
        [SerializeField] private Image _specialButtonImage;
        [SerializeField] private Image _specialBustImage;
        [SerializeField] private Image _chargeFill;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private Color _specialActiveColor   = new Color(0.784f, 0.545f, 0.325f, 1f);
        [SerializeField] private Color _specialInactiveColor = new Color(0.35f, 0.35f, 0.35f, 1f);

        private static readonly Color s_ColorHealthLow  = new Color(0.8f, 0.2f, 0.2f, 1f);
        private static readonly Color s_ColorHealthFull = new Color(0.2f, 0.8f, 0.2f, 1f);

        private PlayerStats      _playerStats;
        private CombatController _combat;
        private bool             _hadSpecialLastFrame;

        private void Start()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                if (playerGO.TryGetComponent(out _playerStats))
                {
                    _playerStats.OnHealthChanged += HandleHealthChanged;
                    HandleHealthChanged(_playerStats.CurrentHealth, _playerStats.MaxHealth);
                }

                if (playerGO.TryGetComponent(out _combat))
                {
                    _combat.OnSpecialEquipped += HandleSpecialEquipped;
                    _combat.OnStyleChanged    += HandleStyleChanged;
                }
            }
            else
            {
                Debug.LogWarning("[HUDController_V2] No Player found — HUD will not update.");
            }

            // Initialise special button to correct state
            RefreshSpecialButton(_combat != null && _combat.HasSpecialAbility);

            // Subscribe to IP changes from ProgressionSystem (DontDestroyOnLoad singleton).
            if (ProgressionSystem.Instance != null)
                ProgressionSystem.Instance.OnIPChanged += UpdateIPDisplay;

            // Initialise IP counter to 0 at run start (ProgressionSystem resets in GameManager.Start).
            UpdateIPDisplay(0);
        }

        private void OnDestroy()
        {
            if (_playerStats != null) _playerStats.OnHealthChanged -= HandleHealthChanged;
            if (_combat != null)
            {
                _combat.OnSpecialEquipped -= HandleSpecialEquipped;
                _combat.OnStyleChanged    -= HandleStyleChanged;
            }
            if (ProgressionSystem.Instance != null)
                ProgressionSystem.Instance.OnIPChanged -= UpdateIPDisplay;
        }

        private void Update()
        {
            if (_combat == null) return;

            bool hasAbility = _combat.HasSpecialAbility;

            // Refresh button visibility whenever the has-ability state changes —
            // covers both weapon equip/unequip and fighting style selection.
            if (hasAbility != _hadSpecialLastFrame)
            {
                RefreshSpecialButton(hasAbility);
                _hadSpecialLastFrame = hasAbility;
            }

            if (!hasAbility) return;

            // Update charge fill every frame — covers style cooldown and weapon ability progress
            float progress = _combat.SpecialAbilityProgress;
            bool recharged = progress >= 1f;
            SetChargeFill(progress);
            SetCooldownOverlay(progress);

            if (_specialButtonInner != null)
                _specialButtonInner.SetActive(recharged);

            if (_specialButtonImage != null)
                _specialButtonImage.color = recharged ? _specialActiveColor : _specialInactiveColor;
        }

        // ─── Event handlers ──────────────────────────────────────────────────

        private void HandleSpecialEquipped(WeaponAbilityData ability)
        {
            // Use HasSpecialAbility so fighting style specials keep the button visible
            // even when the equipped weapon has no ability (ability == null).
            RefreshSpecialButton(_combat != null && _combat.HasSpecialAbility);
        }

        private void HandleStyleChanged(FightingStyleData style) { }

        private void HandleHealthChanged(int current, int max)
        {
            if (_healthFill == null) return;
            float ratio = max > 0 ? (float)current / max : 0f;
            _healthFill.fillAmount = ratio;
            _healthFill.color = Color.Lerp(s_ColorHealthLow, s_ColorHealthFull, ratio);

        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private void RefreshSpecialButton(bool hasAbility)
        {
            // Always keep the button visible — the player arrives at every scene with a
            // fighting style already chosen. Hiding it causes confusion when the scene
            // loads before the CombatController's ability reference is fully wired.
            if (_specialButtonRoot != null)
                _specialButtonRoot.SetActive(true);

            if (hasAbility)
            {
                float progress = _combat != null ? _combat.SpecialAbilityProgress : 1f;
                bool recharged = progress >= 1f;
                if (_specialButtonInner != null) _specialButtonInner.SetActive(recharged);
                if (_specialButtonImage != null) _specialButtonImage.color = recharged ? _specialActiveColor : _specialInactiveColor;
                SetChargeFill(progress);
                SetCooldownOverlay(progress);
            }
            else
            {
                SetCooldownOverlay(1f); // hide overlay when no ability equipped
            }
        }

        private void SetChargeFill(float normalizedCharge)
        {
            if (_chargeFill != null)
                _chargeFill.fillAmount = Mathf.Clamp01(normalizedCharge);
        }

        // Dark overlay sits on top of the icon — sweeps away clockwise as cooldown completes.
        // fillAmount = 1 = fully covered (just used), 0 = fully clear (ready).
        private void SetCooldownOverlay(float progress)
        {
            if (_cooldownOverlay == null) return;
            float cover = Mathf.Clamp01(1f - progress);
            _cooldownOverlay.fillAmount = cover;
            _cooldownOverlay.enabled    = cover > 0.01f;
        }

        private void UpdateIPDisplay(int ip)
        {
            if (_ipCounterText != null) _ipCounterText.SetText("IP: {0}", ip);
        }

        /// <summary>
        /// Public accessor for external callers that need to set IP display directly
        /// (e.g. GameManager on scene load before ProgressionSystem fires its first event).
        /// </summary>
        public void SetIPCount(int count)
        {
            UpdateIPDisplay(count);
        }

        public void SetEnemyCount(int count)
        {
            if (_enemyCounterText != null)
                _enemyCounterText.SetText("{0}", count);
        }
    }
}
