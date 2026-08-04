using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Systems;

namespace Boxhead.UI
{
    /// <summary>
    /// Full inventory panel. Opened by a HUD button. Does NOT pause time.
    /// Shows all 3 weapon slots and the material bag. Equip/Drop buttons are wired per-slot.
    /// </summary>
    public class InventoryScreen : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;      // starts inactive
        [SerializeField] private Button     _closeButton;
        [SerializeField] private Button     _openButton;  // HUD button; optional

        [Header("Cardboard")]
        [SerializeField] private TextMeshProUGUI _cardboardText;

        [Header("Weapon Slots (3)")]
        [SerializeField] private Image[]           _slotIcons;
        [SerializeField] private TextMeshProUGUI[] _slotNameTexts;
        [SerializeField] private TextMeshProUGUI[] _slotTierTexts;
        [SerializeField] private Image[]           _slotDurabilityBars;
        [SerializeField] private Button[]          _slotEquipButtons;   // "Equip" — calls SetActiveSlot
        [SerializeField] private Button[]          _slotDropButtons;    // "Drop"  — calls RemoveFromSlot

        [Header("Material Bag (3)")]
        [SerializeField] private Image[]           _bagIcons;
        [SerializeField] private TextMeshProUGUI[] _bagNameTexts;

        [Header("Durability Bar Colors")]
        [SerializeField] private Color _durabilityFull = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _durabilityMid  = new Color(0.9f, 0.7f, 0.1f, 1f);
        [SerializeField] private Color _durabilityLow  = new Color(0.85f, 0.15f, 0.1f, 1f);

        // Cached component references — resolved in Awake.
        private WeaponInventory   _weaponInventory;
        private CardboardResource _cardboardResource;

        // Zero-alloc text building.
        private readonly StringBuilder _sb = new StringBuilder(32);

        // Static tier labels — no allocation.
        private const string TierStd  = "STD";
        private const string TierEpic = "EPIC";
        private const string TierLeg  = "LEG";

        // Guard flag preventing double-subscribe when Open is called without a matching Close.
        private bool _isOpen;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Start()
        {
            // Resolve player components in Start so all player Awakes have already run
            // and FindWithTag is guaranteed to find the fully-initialized player.
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null)
            {
                Debug.LogError("[InventoryScreen] No GameObject with tag 'Player' found.", this);
            }
            else
            {
                playerGO.TryGetComponent(out _weaponInventory);
                playerGO.TryGetComponent(out _cardboardResource);

                if (_weaponInventory == null)
                    Debug.LogWarning("[InventoryScreen] Player has no WeaponInventory.", this);
                if (_cardboardResource == null)
                    Debug.LogWarning("[InventoryScreen] Player has no CardboardResource.", this);
            }

            // Wire the open button if supplied.
            if (_openButton != null)
                _openButton.onClick.AddListener(ToggleOpen);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);

            WireSlotButtons();
        }

        private void OnDestroy()
        {
            // Ensure subscriptions are cleaned up even if Close() was not called.
            UnsubscribeEvents();

            if (_openButton  != null) _openButton.onClick.RemoveAllListeners();
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();

            if (_slotEquipButtons != null)
                for (int i = 0; i < _slotEquipButtons.Length; i++)
                    if (_slotEquipButtons[i] != null) _slotEquipButtons[i].onClick.RemoveAllListeners();

            if (_slotDropButtons != null)
                for (int i = 0; i < _slotDropButtons.Length; i++)
                    if (_slotDropButtons[i] != null) _slotDropButtons[i].onClick.RemoveAllListeners();
        }

        // ── Slot Button Wiring ────────────────────────────────────────────────

        private void WireSlotButtons()
        {
            for (int i = 0; i < WeaponInventory.WeaponSlotCount; i++)
            {
                int capturedIndex = i;

                if (_slotEquipButtons != null && i < _slotEquipButtons.Length && _slotEquipButtons[i] != null)
                    _slotEquipButtons[i].onClick.AddListener(() => OnEquipClicked(capturedIndex));

                if (_slotDropButtons != null && i < _slotDropButtons.Length && _slotDropButtons[i] != null)
                    _slotDropButtons[i].onClick.AddListener(() => OnDropClicked(capturedIndex));
            }
        }

        private void OnEquipClicked(int slotIndex)
        {
            _weaponInventory?.SetActiveSlot(slotIndex);
        }

        private void OnDropClicked(int slotIndex)
        {
            _weaponInventory?.RemoveFromSlot(slotIndex);
        }

        // ── Open / Close / Toggle ─────────────────────────────────────────────

        /// <summary>Activates the inventory panel and subscribes to data events.</summary>
        public void Open()
        {
            if (_panel == null) return;
            if (_isOpen) Close();
            _isOpen = true;

            SubscribeEvents();
            RefreshAll();
            _panel.SetActive(true);
        }

        /// <summary>Deactivates the inventory panel and unsubscribes from data events.</summary>
        public void Close()
        {
            if (_panel == null) return;
            if (!_isOpen) return;
            _isOpen = false;

            UnsubscribeEvents();
            _panel.SetActive(false);
        }

        /// <summary>
        /// Opens the panel if it is closed; closes it if it is open.
        /// Wire to the HUD inventory button via the Inspector or from Start.
        /// </summary>
        public void ToggleOpen()
        {
            if (_panel == null) return;

            if (_panel.activeSelf)
                Close();
            else
                Open();
        }

        // ── Event Subscription ────────────────────────────────────────────────

        private void SubscribeEvents()
        {
            if (_weaponInventory != null)
                _weaponInventory.OnInventoryChanged += OnInventoryChanged;

            if (_cardboardResource != null)
                _cardboardResource.OnCardboardChanged += OnCardboardChanged;
        }

        private void UnsubscribeEvents()
        {
            if (_weaponInventory != null)
                _weaponInventory.OnInventoryChanged -= OnInventoryChanged;

            if (_cardboardResource != null)
                _cardboardResource.OnCardboardChanged -= OnCardboardChanged;
        }

        private void OnInventoryChanged()
        {
            RefreshAll();
        }

        private void OnCardboardChanged(int amount)
        {
            SyncCardboardText(amount);
        }

        // ── Refresh ───────────────────────────────────────────────────────────

        private void RefreshAll()
        {
            SyncCardboardText(_cardboardResource != null ? _cardboardResource.Current : 0);
            RefreshWeaponSlots();
            RefreshBagSlots();
        }

        private void RefreshWeaponSlots()
        {
            if (_weaponInventory == null) return;

            WeaponInstance[] slots     = _weaponInventory.WeaponSlots;
            int              activeIdx = _weaponInventory.ActiveSlotIndex;

            for (int i = 0; i < WeaponInventory.WeaponSlotCount; i++)
            {
                WeaponInstance weapon   = slots[i];
                bool           hasWeapon = weapon != null;
                bool           isActive  = hasWeapon && activeIdx == i;

                // Icon
                if (_slotIcons != null && i < _slotIcons.Length && _slotIcons[i] != null)
                {
                    _slotIcons[i].sprite  = hasWeapon ? GetTierIcon(weapon) : null;
                    _slotIcons[i].color   = hasWeapon ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.5f);
                    _slotIcons[i].enabled = true;
                }

                // Name
                if (_slotNameTexts != null && i < _slotNameTexts.Length && _slotNameTexts[i] != null)
                {
                    string name = hasWeapon ? GetDisplayName(weapon) : string.Empty;
                    if (_slotNameTexts[i].text != name)
                        _slotNameTexts[i].SetText(name);
                }

                // Tier
                if (_slotTierTexts != null && i < _slotTierTexts.Length && _slotTierTexts[i] != null)
                {
                    string tierLabel = hasWeapon ? TierLabel(weapon.Tier) : string.Empty;
                    if (_slotTierTexts[i].text != tierLabel)
                        _slotTierTexts[i].SetText(tierLabel);
                }

                // Durability bar
                if (_slotDurabilityBars != null && i < _slotDurabilityBars.Length && _slotDurabilityBars[i] != null)
                {
                    if (!hasWeapon)
                    {
                        _slotDurabilityBars[i].enabled = false;
                    }
                    else
                    {
                        _slotDurabilityBars[i].enabled = true;
                        float fill = weapon.MaxDurability > 0
                            ? (float)weapon.CurrentDurability / weapon.MaxDurability
                            : 0f;
                        _slotDurabilityBars[i].fillAmount = fill;
                        _slotDurabilityBars[i].color      = DurabilityColor(fill);
                    }
                }

                // Equip button — interactable only when there is a weapon in the slot.
                if (_slotEquipButtons != null && i < _slotEquipButtons.Length && _slotEquipButtons[i] != null)
                    _slotEquipButtons[i].interactable = hasWeapon && !isActive;

                // Drop button — interactable when there is a weapon to drop.
                if (_slotDropButtons != null && i < _slotDropButtons.Length && _slotDropButtons[i] != null)
                    _slotDropButtons[i].interactable = hasWeapon;
            }
        }

        private void RefreshBagSlots()
        {
            if (_weaponInventory == null) return;

            for (int i = 0; i < WeaponInventory.MaterialBagCapacity; i++)
            {
                WeaponObjectSO item    = _weaponInventory.MaterialBag[i];
                bool           hasItem = item != null;

                if (_bagIcons != null && i < _bagIcons.Length && _bagIcons[i] != null)
                {
                    _bagIcons[i].sprite  = hasItem ? item.rawObjectIcon : null;
                    _bagIcons[i].color   = hasItem ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.5f);
                    _bagIcons[i].enabled = true;
                }

                if (_bagNameTexts != null && i < _bagNameTexts.Length && _bagNameTexts[i] != null)
                {
                    string name = hasItem ? item.rawObjectName : string.Empty;
                    if (_bagNameTexts[i].text != name)
                        _bagNameTexts[i].SetText(name);
                }
            }
        }

        // Zero-alloc cardboard text update.
        private void SyncCardboardText(int amount)
        {
            if (_cardboardText == null) return;
            _sb.Clear();
            _sb.Append(amount);
            _cardboardText.SetText(_sb);
        }

        // ── Utility ───────────────────────────────────────────────────────────

        private Color DurabilityColor(float fill)
        {
            if (fill > 0.5f)  return _durabilityFull;
            if (fill > 0.2f)  return _durabilityMid;
            return _durabilityLow;
        }

        // Returns the abbreviated tier label string without allocation.
        private static string TierLabel(WeaponTier tier)
        {
            switch (tier)
            {
                case WeaponTier.Epic:      return TierEpic;
                case WeaponTier.Legendary: return TierLeg;
                default:                   return TierStd;
            }
        }

        private static Sprite GetTierIcon(WeaponInstance weapon)
        {
            if (weapon.Tier == WeaponTier.Epic      && weapon.Data.epicIcon      != null) return weapon.Data.epicIcon;
            if (weapon.Tier == WeaponTier.Legendary && weapon.Data.legendaryIcon != null) return weapon.Data.legendaryIcon;
            return weapon.Data.weaponIcon;
        }

        // Legendary shows the designed weapon name; others show the raw object name.
        private static string GetDisplayName(WeaponInstance weapon)
        {
            if (weapon.Tier == WeaponTier.Legendary && !string.IsNullOrEmpty(weapon.Data.weaponName))
                return weapon.Data.weaponName;
            return weapon.Data.rawObjectName;
        }
    }
}
