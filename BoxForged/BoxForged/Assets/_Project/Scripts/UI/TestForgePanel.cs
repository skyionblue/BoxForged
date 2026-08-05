using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Systems;

namespace Boxhead.UI
{
    // Minimal forge panel for the test scene.
    // Replace with the full ForgeUI when scene wiring is complete.
    public class TestForgePanel : MonoBehaviour
    {
        [SerializeField] private GameObject      _panel;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Button          _forgeSlot0Button;
        [SerializeField] private Button          _forgeSlot1Button;
        [SerializeField] private Button          _forgeSlot2Button;
        [SerializeField] private Button          _upgradeSlot0Button;
        [SerializeField] private Button          _upgradeSlot1Button;
        [SerializeField] private Button          _upgradeSlot2Button;
        [SerializeField] private Button          _closeButton;

        private ForgeController  _forgeController;
        private WeaponInventory  _inventory;
        private CardboardResource _cardboard;

        private void Start()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (_forgeSlot0Button != null) _forgeSlot0Button.onClick.AddListener(() => Forge(0));
            if (_forgeSlot1Button != null) _forgeSlot1Button.onClick.AddListener(() => Forge(1));
            if (_forgeSlot2Button != null) _forgeSlot2Button.onClick.AddListener(() => Forge(2));
            if (_upgradeSlot0Button != null) _upgradeSlot0Button.onClick.AddListener(() => Upgrade(0));
            if (_upgradeSlot1Button != null) _upgradeSlot1Button.onClick.AddListener(() => Upgrade(1));
            if (_upgradeSlot2Button != null) _upgradeSlot2Button.onClick.AddListener(() => Upgrade(2));
            // Do NOT set _panel inactive here — the panel starts inactive in the scene.
            // Calling SetActive(false) in Start() would hide it one frame after Open() shows it.
        }

        private void OnDestroy()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            if (_forgeSlot0Button != null) _forgeSlot0Button.onClick.RemoveAllListeners();
            if (_forgeSlot1Button != null) _forgeSlot1Button.onClick.RemoveAllListeners();
            if (_forgeSlot2Button != null) _forgeSlot2Button.onClick.RemoveAllListeners();
        }

        public void Open(ForgeController fc)
        {
            if (fc == null) return;
            _forgeController = fc;

            var player = GameObject.FindWithTag("Player");
            _inventory = player != null ? player.GetComponent<WeaponInventory>() : null;
            _cardboard  = player != null ? player.GetComponent<CardboardResource>() : null;

            if (_inventory != null)  _inventory.OnInventoryChanged  += Refresh;
            if (_cardboard != null)  _cardboard.OnCardboardChanged  += OnCardboardChanged;

            if (_panel != null) _panel.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (_inventory != null)  _inventory.OnInventoryChanged  -= Refresh;
            if (_cardboard != null)  _cardboard.OnCardboardChanged  -= OnCardboardChanged;

            if (_panel != null) _panel.SetActive(false);
            _forgeController = null;
        }

        private void OnCardboardChanged(int _) => Refresh();

        private void Forge(int bagIndex)
        {
            _forgeController?.TryForge(bagIndex);
        }

        private void Upgrade(int slotIndex)
        {
            _forgeController?.TryUpgrade(slotIndex);
        }

        private void Refresh()
        {
            if (_statusText == null) return;

            int cardboard = _cardboard?.Current ?? 0;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<b>FORGE</b>   Cardboard: {cardboard}");
            sb.AppendLine("");
            bool slotsFull = _inventory != null && System.Array.TrueForAll(_inventory.WeaponSlots, s => s != null);
            string bagHeader = slotsFull
                ? "<b>Material Bag</b> <color=#FF6666>— SLOTS FULL. Open BAG and DROP a weapon first.</color>"
                : "<b>Material Bag</b> (tap Forge [0/1/2] to forge):";
            sb.AppendLine(bagHeader);

            for (int i = 0; i < WeaponInventory.MaterialBagCapacity; i++)
            {
                var item = _inventory?.GetMaterialBagItem(i);
                string name  = item != null ? item.rawObjectName : "empty";
                string cost  = item != null ? $" — costs {item.forgeCost} cardboard" : "";
                sb.AppendLine($"  [{i}] {name}{cost}");
            }

            sb.AppendLine("");
            sb.AppendLine("<b>Weapon Slots</b> (Upgrade [0/1/2] to Epic/Legendary):");
            if (_inventory != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    var slot = _inventory.WeaponSlots[i];
                    string info = slot != null
                        ? $"{slot.Data.weaponName} ({slot.Tier}) [{slot.CurrentDurability}/{slot.MaxDurability}]"
                        : "empty";
                    string active = (_inventory.ActiveSlotIndex == i) ? " ◄" : "";
                    string upgrade = "";
                    if (slot != null) {
                        var woso = slot.Data as Boxhead.Systems.WeaponObjectSO;
                        if (woso != null) {
                            if (slot.Tier == WeaponTier.Standard && woso.rarity >= WeaponRarity.Rare)
                                upgrade = $" [Epic costs {woso.epicUpgradeCost}cb]";
                            else if (slot.Tier == WeaponTier.Epic && woso.rarity == WeaponRarity.Legendary)
                                upgrade = $" [Leg costs {woso.legendaryUpgradeCost}cb]";
                            else if (slot.Tier == WeaponTier.Standard && woso.rarity == WeaponRarity.Common)
                                upgrade = " [Common: no upgrade]";
                        }
                    }
                    sb.AppendLine($"  [{i}] {info}{upgrade}{active}");
                }
            }

            _statusText.SetText(sb);

            UpdateForgeButton(_forgeSlot0Button, 0, cardboard);
            UpdateForgeButton(_forgeSlot1Button, 1, cardboard);
            UpdateForgeButton(_forgeSlot2Button, 2, cardboard);
        }

        private void UpdateForgeButton(Button btn, int bagIndex, int cardboard)
        {
            if (btn == null) return;
            var item = _inventory?.GetMaterialBagItem(bagIndex);
            if (item == null) { btn.interactable = false; return; }
            bool slotsAvailable = false;
            if (_inventory != null)
                for (int i = 0; i < WeaponInventory.WeaponSlotCount; i++)
                    if (_inventory.WeaponSlots[i] == null) { slotsAvailable = true; break; }
            btn.interactable = slotsAvailable && cardboard >= item.forgeCost;
        }
    }
}
