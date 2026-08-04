using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Systems;

namespace Boxhead.UI
{
    /// <summary>
    /// Forge panel UI. Opened when the player enters a workbench trigger.
    /// Does NOT pause time. Wired to WorkbenchProp via RegisterWorkbench / UnregisterWorkbench
    /// so the scene setup (not WorkbenchProp itself) owns the relationship.
    /// </summary>
    public class ForgeUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;   // starts inactive in scene

        [Header("Cardboard")]
        [SerializeField] private TextMeshProUGUI _cardboardText;

        [Header("Material Bag Slots (3)")]
        [SerializeField] private Button[]          _bagButtons;       // length 3
        [SerializeField] private Image[]           _bagIcons;         // length 3
        [SerializeField] private TextMeshProUGUI[] _bagNameTexts;     // length 3

        [Header("Weapon Slots (3)")]
        [SerializeField] private Button[]          _weaponButtons;    // length 3
        [SerializeField] private Image[]           _weaponIcons;      // length 3
        [SerializeField] private TextMeshProUGUI[] _weaponNameTexts;  // length 3

        [Header("Selection Info")]
        [SerializeField] private TextMeshProUGUI _selectionNameText;
        [SerializeField] private TextMeshProUGUI _selectionRarityText;
        [SerializeField] private TextMeshProUGUI _selectionCostText;
        [SerializeField] private Button          _forgeButton;
        [SerializeField] private TextMeshProUGUI _forgeButtonText;

        [Header("Close")]
        [SerializeField] private Button _closeButton;

        // Runtime references — null when panel is closed.
        private ForgeController   _forgeController;
        private WeaponInventory   _weaponInventory;
        private CardboardResource _cardboardResource;

        // Selection state — mutually exclusive.
        private int _selectedBagIndex        = -1;
        private int _selectedWeaponSlotIndex = -1;

        // Zero-alloc text building.
        private readonly StringBuilder _sb = new StringBuilder(32);

        // Static strings — no allocation.
        private const string LabelMax     = "MAX";
        private const string LabelForge   = "Forge";
        private const string LabelUpgrade = "Upgrade";

        // Rarity label lookup — avoids enum.ToString() heap allocation.
        private static readonly string[] RarityLabels = { "Common", "Rare", "Legendary" };

        // Guard flag preventing double-subscribe when Open is called without a matching Close.
        private bool _isOpen;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        // Awake is intentionally empty — panel starts inactive, so Awake may not run
        // until the panel is first activated. Button listeners are wired in Start instead.

        private void Start()
        {
            WireButtonListeners();
        }

        private void WireButtonListeners()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);

            if (_forgeButton != null)
                _forgeButton.onClick.AddListener(OnForgeButtonClicked);

            for (int i = 0; i < WeaponInventory.MaterialBagCapacity; i++)
            {
                int capturedIndex = i; // Capture for lambda — i changes in loop.
                if (_bagButtons != null && i < _bagButtons.Length && _bagButtons[i] != null)
                    _bagButtons[i].onClick.AddListener(() => OnBagSlotSelected(capturedIndex));
            }

            for (int i = 0; i < WeaponInventory.WeaponSlotCount; i++)
            {
                int capturedIndex = i;
                if (_weaponButtons != null && i < _weaponButtons.Length && _weaponButtons[i] != null)
                    _weaponButtons[i].onClick.AddListener(() => OnWeaponSlotSelected(capturedIndex));
            }
        }

        private void OnDestroy()
        {
            // Close unsubscribes events if still open when the object is destroyed.
            Close();

            if (_closeButton != null)   _closeButton.onClick.RemoveAllListeners();
            if (_forgeButton != null)   _forgeButton.onClick.RemoveAllListeners();

            if (_bagButtons != null)
                for (int i = 0; i < _bagButtons.Length; i++)
                    if (_bagButtons[i] != null) _bagButtons[i].onClick.RemoveAllListeners();

            if (_weaponButtons != null)
                for (int i = 0; i < _weaponButtons.Length; i++)
                    if (_weaponButtons[i] != null) _weaponButtons[i].onClick.RemoveAllListeners();
        }

        // ── Workbench Registration ────────────────────────────────────────────

        /// <summary>
        /// Call this from scene setup (e.g. a bootstrapper or a scene-level wiring script)
        /// to connect a workbench to this ForgeUI. WorkbenchProp does not call this directly.
        /// </summary>
        public void RegisterWorkbench(WorkbenchProp workbench)
        {
            if (workbench == null) return;
            workbench.OnPlayerEntered += OnPlayerEnteredWorkbench;
            workbench.OnPlayerExited  += OnPlayerExitedWorkbench;
        }

        /// <summary>
        /// Unsubscribes from a previously registered workbench. Call when the prop is destroyed
        /// or the scene is torn down.
        /// </summary>
        public void UnregisterWorkbench(WorkbenchProp workbench)
        {
            if (workbench == null) return;
            workbench.OnPlayerEntered -= OnPlayerEnteredWorkbench;
            workbench.OnPlayerExited  -= OnPlayerExitedWorkbench;
        }

        private void OnPlayerEnteredWorkbench(ForgeController fc)
        {
            if (fc == null) return;

            // Resolve WeaponInventory and CardboardResource from the same GameObject as ForgeController.
            WeaponInventory   inv;
            CardboardResource cbr;
            fc.TryGetComponent(out inv);
            fc.TryGetComponent(out cbr);

            Open(fc, inv, cbr);
        }

        private void OnPlayerExitedWorkbench()
        {
            Close();
        }

        // ── Open / Close ──────────────────────────────────────────────────────

        /// <summary>
        /// Opens the forge panel and subscribes to inventory/cardboard events.
        /// </summary>
        public void Open(ForgeController forgeController, WeaponInventory inventory, CardboardResource cardboard)
        {
            if (_isOpen) Close();
            _isOpen = true;

            _forgeController   = forgeController;
            _weaponInventory   = inventory;
            _cardboardResource = cardboard;

            if (_weaponInventory != null)
                _weaponInventory.OnInventoryChanged += OnInventoryChanged;

            if (_cardboardResource != null)
                _cardboardResource.OnCardboardChanged += OnCardboardChanged;

            ClearSelection();
            RefreshAll();

            if (_panel != null) _panel.SetActive(true);
        }

        /// <summary>
        /// Closes the forge panel and unsubscribes from all events.
        /// </summary>
        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (_weaponInventory != null)
                _weaponInventory.OnInventoryChanged -= OnInventoryChanged;

            if (_cardboardResource != null)
                _cardboardResource.OnCardboardChanged -= OnCardboardChanged;

            _forgeController   = null;
            _weaponInventory   = null;
            _cardboardResource = null;

            ClearSelection();

            if (_panel != null) _panel.SetActive(false);
        }

        // ── Event Handlers ────────────────────────────────────────────────────

        private void OnInventoryChanged()
        {
            RefreshAll();
        }

        private void OnCardboardChanged(int amount)
        {
            SyncCardboardText(amount);
            UpdateForgeButtonState();
        }

        // ── Selection ─────────────────────────────────────────────────────────

        private void OnBagSlotSelected(int index)
        {
            if (_weaponInventory == null) return;
            if (_weaponInventory.MaterialBag[index] == null) return;

            _selectedBagIndex        = index;
            _selectedWeaponSlotIndex = -1;
            UpdateInfoPanel();
            UpdateForgeButtonState();
        }

        private void OnWeaponSlotSelected(int index)
        {
            if (_weaponInventory == null) return;
            if (_weaponInventory.WeaponSlots[index] == null) return;

            _selectedWeaponSlotIndex = index;
            _selectedBagIndex        = -1;
            UpdateInfoPanel();
            UpdateForgeButtonState();
        }

        private void ClearSelection()
        {
            _selectedBagIndex        = -1;
            _selectedWeaponSlotIndex = -1;
            UpdateInfoPanel();
            UpdateForgeButtonState();
        }

        // ── Forge Button ──────────────────────────────────────────────────────

        private void OnForgeButtonClicked()
        {
            if (_forgeController == null) return;

            bool success = false;
            if (_selectedBagIndex >= 0)
                success = _forgeController.TryForge(_selectedBagIndex);
            else if (_selectedWeaponSlotIndex >= 0)
                success = _forgeController.TryUpgrade(_selectedWeaponSlotIndex);

            if (success)
            {
                // OnInventoryChanged will fire automatically and refresh the panel.
                ClearSelection();
            }
        }

        // ── Refresh Helpers ───────────────────────────────────────────────────

        private void RefreshAll()
        {
            SyncCardboardText(_cardboardResource != null ? _cardboardResource.Current : 0);
            RefreshBagSlots();
            RefreshWeaponSlots();
            UpdateInfoPanel();
            UpdateForgeButtonState();
        }

        private void RefreshBagSlots()
        {
            if (_weaponInventory == null) return;

            for (int i = 0; i < WeaponInventory.MaterialBagCapacity; i++)
            {
                WeaponObjectSO item = _weaponInventory.MaterialBag[i];
                bool hasItem = item != null;

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

                if (_bagButtons != null && i < _bagButtons.Length && _bagButtons[i] != null)
                    _bagButtons[i].interactable = hasItem;
            }
        }

        private void RefreshWeaponSlots()
        {
            if (_weaponInventory == null) return;

            for (int i = 0; i < WeaponInventory.WeaponSlotCount; i++)
            {
                WeaponInstance weapon = _weaponInventory.WeaponSlots[i];
                bool hasWeapon = weapon != null;

                if (_weaponIcons != null && i < _weaponIcons.Length && _weaponIcons[i] != null)
                {
                    _weaponIcons[i].sprite  = hasWeapon ? GetTierIcon(weapon) : null;
                    _weaponIcons[i].color   = hasWeapon ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.5f);
                    _weaponIcons[i].enabled = true;
                }

                if (_weaponNameTexts != null && i < _weaponNameTexts.Length && _weaponNameTexts[i] != null)
                {
                    string label = BuildWeaponSlotLabel(weapon);
                    if (_weaponNameTexts[i].text != label)
                        _weaponNameTexts[i].SetText(label);
                }

                if (_weaponButtons != null && i < _weaponButtons.Length && _weaponButtons[i] != null)
                    _weaponButtons[i].interactable = hasWeapon;
            }
        }

        // Builds the weapon slot label: name + upgrade cost, or "MAX" at ceiling.
        // Uses static _sb; call only from the main thread.
        private string BuildWeaponSlotLabel(WeaponInstance weapon)
        {
            if (weapon == null) return string.Empty;

            _sb.Clear();
            _sb.Append(weapon.Data.rawObjectName);

            // Append upgrade cost or MAX indicator.
            if (weapon.Tier == WeaponTier.Standard)
            {
                if (weapon.Data.rarity == WeaponRarity.Common)
                {
                    _sb.Append(" [MAX]");
                }
                else
                {
                    _sb.Append(" [+");
                    _sb.Append(weapon.Data.epicUpgradeCost);
                    _sb.Append("]");
                }
            }
            else if (weapon.Tier == WeaponTier.Epic)
            {
                if (weapon.Data.rarity == WeaponRarity.Legendary)
                {
                    _sb.Append(" [+");
                    _sb.Append(weapon.Data.legendaryUpgradeCost);
                    _sb.Append("]");
                }
                else
                {
                    _sb.Append(" [MAX]");
                }
            }
            else
            {
                // Legendary tier — ceiling.
                _sb.Append(" [MAX]");
            }

            return _sb.ToString();
        }

        // Updates the info panel to reflect the current selection.
        private void UpdateInfoPanel()
        {
            if (_selectedBagIndex >= 0 && _weaponInventory != null)
            {
                WeaponObjectSO item = _weaponInventory.MaterialBag[_selectedBagIndex];
                if (item != null)
                {
                    SetInfoText(_selectionNameText,   item.rawObjectName);
                    SetInfoText(_selectionRarityText, RarityLabels[(int)item.rarity]);
                    _sb.Clear();
                    _sb.Append(item.forgeCost);
                    SetInfoSb(_selectionCostText, _sb);
                    return;
                }
            }

            if (_selectedWeaponSlotIndex >= 0 && _weaponInventory != null)
            {
                WeaponInstance weapon = _weaponInventory.WeaponSlots[_selectedWeaponSlotIndex];
                if (weapon != null)
                {
                    SetInfoText(_selectionNameText,   weapon.Data.rawObjectName);
                    SetInfoText(_selectionRarityText, RarityLabels[(int)weapon.Data.rarity]);
                    int upgradeCost = GetUpgradeCost(weapon);
                    _sb.Clear();
                    if (upgradeCost > 0)
                        _sb.Append(upgradeCost);
                    else
                        _sb.Append(LabelMax);
                    SetInfoSb(_selectionCostText, _sb);
                    return;
                }
            }

            // Nothing selected — clear all info fields.
            SetInfoText(_selectionNameText,   string.Empty);
            SetInfoText(_selectionRarityText, string.Empty);
            SetInfoText(_selectionCostText,   string.Empty);
        }

        // Controls whether the forge button is interactable.
        private void UpdateForgeButtonState()
        {
            if (_forgeButton == null) return;

            bool canForge = false;

            if (_selectedBagIndex >= 0 && _weaponInventory != null && _cardboardResource != null)
            {
                WeaponObjectSO item = _weaponInventory.MaterialBag[_selectedBagIndex];
                canForge = item != null && _cardboardResource.CanAfford(item.forgeCost);
            }
            else if (_selectedWeaponSlotIndex >= 0 && _weaponInventory != null && _cardboardResource != null)
            {
                WeaponInstance weapon = _weaponInventory.WeaponSlots[_selectedWeaponSlotIndex];
                if (weapon != null)
                {
                    int cost = GetUpgradeCost(weapon);
                    canForge = cost > 0 && _cardboardResource.CanAfford(cost);
                }
            }

            _forgeButton.interactable = canForge;

            // Update button label to distinguish forge vs upgrade.
            if (_forgeButtonText != null)
            {
                string label = _selectedWeaponSlotIndex >= 0 ? LabelUpgrade : LabelForge;
                if (_forgeButtonText.text != label)
                    _forgeButtonText.SetText(label);
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

        // Returns the cardboard cost of the next upgrade, or 0 if at ceiling.
        private static int GetUpgradeCost(WeaponInstance weapon)
        {
            if (weapon.Tier == WeaponTier.Standard)
            {
                if (weapon.Data.rarity == WeaponRarity.Common) return 0;
                return weapon.Data.epicUpgradeCost;
            }
            if (weapon.Tier == WeaponTier.Epic)
            {
                if (weapon.Data.rarity == WeaponRarity.Legendary) return weapon.Data.legendaryUpgradeCost;
                return 0;
            }
            return 0; // Already Legendary.
        }

        private static Sprite GetTierIcon(WeaponInstance weapon)
        {
            if (weapon.Tier == WeaponTier.Epic      && weapon.Data.epicIcon      != null) return weapon.Data.epicIcon;
            if (weapon.Tier == WeaponTier.Legendary && weapon.Data.legendaryIcon != null) return weapon.Data.legendaryIcon;
            return weapon.Data.weaponIcon;
        }

        // Guards SetText against redundant calls — equality check is cheaper than text re-layout.
        private static void SetInfoText(TextMeshProUGUI label, string text)
        {
            if (label == null) return;
            if (label.text != text) label.SetText(text);
        }

        // StringBuilder overload for zero-alloc int display.
        private static void SetInfoSb(TextMeshProUGUI label, StringBuilder sb)
        {
            if (label == null) return;
            label.SetText(sb);
        }
    }
}
