using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Systems;

namespace Boxhead.UI
{
    /// <summary>
    /// Displays two weapon slot icons in the HUD — equipped and backpack.
    /// Tapping the backpack slot swaps equipped and backpack weapons.
    /// Resolves the player's Inventory at Start via tag lookup.
    /// </summary>
    public class WeaponSlotUI : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] private Image           _equippedIcon;
        [SerializeField] private Image           _backpackIcon;
        [SerializeField] private Button          _backpackButton;  // tap to swap

        [Header("Name Text")]
        [SerializeField] private TextMeshProUGUI _equippedNameText;
        [SerializeField] private TextMeshProUGUI _backpackNameText;

        [Header("Colors")]
        [SerializeField] private Color _activeColor      = Color.white;
        [SerializeField] private Color _inactiveColor    = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color _placeholderColor = new Color(0.45f, 0.28f, 0.12f, 0.8f);

        private Inventory       _inventory;
        private WeaponInventory _weaponInventory;

        private void Start()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null)
            {
                Debug.LogWarning("[WeaponSlotUI] No GameObject with tag 'Player' found.", this);
                return;
            }

            // V4 WeaponInventory takes priority; fall back to V3 Inventory
            _weaponInventory = playerGO.GetComponent<WeaponInventory>();
            _inventory       = playerGO.GetComponent<Inventory>();

            if (_weaponInventory != null)
                _weaponInventory.OnInventoryChanged += RefreshFromV4;
            else if (_inventory != null)
                _inventory.OnInventoryChanged += RefreshSlots;

            if (_backpackButton != null)
                _backpackButton.onClick.AddListener(OnBackpackTapped);

            RefreshSlots(null, null);
        }

        private void OnDestroy()
        {
            if (_weaponInventory != null)
                _weaponInventory.OnInventoryChanged -= RefreshFromV4;
            if (_inventory != null)
                _inventory.OnInventoryChanged -= RefreshSlots;
            if (_backpackButton != null)
                _backpackButton.onClick.RemoveListener(OnBackpackTapped);
        }

        private void OnBackpackTapped()
        {
            if (_weaponInventory != null)
                _weaponInventory.CycleActiveSlot(1);
            else
                _inventory?.Swap();
        }

        // Reads V4 WeaponInventory — shows active slot as EQ, next slot as BP
        private void RefreshFromV4()
        {
            var active  = _weaponInventory.ActiveWeapon;
            WeaponData eq = active?.Data;

            // Show the next occupied slot as BP
            WeaponData bp = null;
            for (int i = 1; i <= WeaponInventory.WeaponSlotCount; i++)
            {
                int idx = (_weaponInventory.ActiveSlotIndex + i) % WeaponInventory.WeaponSlotCount;
                if (_weaponInventory.WeaponSlots[idx] != null)
                {
                    bp = _weaponInventory.WeaponSlots[idx].Data;
                    break;
                }
            }

            UpdateSlot(_equippedIcon, _equippedNameText, eq, _activeColor);
            UpdateSlot(_backpackIcon, _backpackNameText, bp, _inactiveColor);
            if (_backpackButton != null)
                _backpackButton.interactable = bp != null;
        }

        private void RefreshSlots(WeaponData equipped, WeaponData backpack)
        {
            UpdateSlot(_equippedIcon, _equippedNameText, equipped, _activeColor);
            UpdateSlot(_backpackIcon, _backpackNameText, backpack, _inactiveColor);

            if (_backpackButton != null)
                _backpackButton.interactable = backpack != null;
        }

        private void UpdateSlot(Image icon, TextMeshProUGUI nameText, WeaponData data, Color tint)
        {
            if (icon == null) return;

            bool hasWeapon = data != null;
            icon.enabled = hasWeapon;

            if (!hasWeapon)
            {
                if (nameText != null && nameText.text.Length > 0)
                    nameText.SetText(string.Empty);
                return;
            }

            if (data.weaponIcon != null)
            {
                icon.sprite = data.weaponIcon;
                icon.color  = tint;
            }
            else
            {
                icon.sprite = null;
                icon.color  = _placeholderColor;
            }

            // Equality check suppresses alloc when name hasn't changed
            if (nameText != null && nameText.text != data.weaponName)
                nameText.SetText(data.weaponName);
        }
    }
}
