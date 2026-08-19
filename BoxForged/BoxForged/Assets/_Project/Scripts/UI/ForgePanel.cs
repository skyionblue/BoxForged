using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Core;
using Boxhead.Systems;

namespace Boxhead.UI
{
    // The player's forge / weapon-management panel (Forge + Upgrade weapon slots).
    // Opened at a workbench (proximity) or via the HUD bag button (ToggleFromHUD).
    public class ForgePanel : MonoBehaviour
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
        [Tooltip("Optional HUD bag button that toggles this panel open/closed from anywhere.")]
        [SerializeField] private Button          _openButton;

        private ForgeController  _forgeController;
        private WeaponInventory  _inventory;
        private CardboardResource _cardboard;
        private Coroutine        _cutsceneWaitRoutine;

        private void Awake()
        {
            WorkbenchProp.OnSpawned += OnWorkbenchSpawned;
            WorkbenchProp.OnRemoved += OnWorkbenchRemoved;
        }

        private void OnWorkbenchSpawned(WorkbenchProp wb)
        {
            wb.OnPlayerEntered += Open;
            wb.OnPlayerExited  += Close;
        }

        private void OnWorkbenchRemoved(WorkbenchProp wb)
        {
            wb.OnPlayerEntered -= Open;
            wb.OnPlayerExited  -= Close;
        }

        private void Start()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (_openButton  != null) _openButton.onClick.AddListener(ToggleFromHUD);
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
            WorkbenchProp.OnSpawned -= OnWorkbenchSpawned;
            WorkbenchProp.OnRemoved -= OnWorkbenchRemoved;
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            if (_openButton  != null) _openButton.onClick.RemoveAllListeners();
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
            Time.timeScale      = 0f;
            AudioListener.pause = true;
            Refresh();
        }

        public void Close()
        {
            // Cancel any pending "waiting for the first-forge cutscene to finish" watch (see
            // HandleForgeOrUpgradeSuccess/WaitForCutsceneThenClose below) — whatever path Close()
            // is reached through now supersedes it, and leaving it running could fire a second,
            // stale Close() later.
            if (_cutsceneWaitRoutine != null)
            {
                StopCoroutine(_cutsceneWaitRoutine);
                _cutsceneWaitRoutine = null;
            }

            if (_inventory != null)  _inventory.OnInventoryChanged  -= Refresh;
            if (_cardboard != null)  _cardboard.OnCardboardChanged  -= OnCardboardChanged;

            if (_panel != null) _panel.SetActive(false);
            Time.timeScale      = 1f;
            AudioListener.pause = false;
            _forgeController = null;
        }

        /// <summary>
        /// HUD bag-button entry point. Closes the panel if it is open; otherwise resolves the
        /// player's ForgeController and opens it. Lets the player manage/forge weapons anywhere,
        /// not only when standing at a workbench.
        /// </summary>
        public void ToggleFromHUD()
        {
            if (_panel != null && _panel.activeSelf) { Close(); return; }
            var player = GameObject.FindWithTag("Player");
            var fc = player != null ? player.GetComponent<ForgeController>() : null;
            Open(fc); // Open() null-guards a missing ForgeController
        }

        private void OnCardboardChanged(int _) => Refresh();

        // On success, close immediately — the transformation moment itself plays in-world
        // (ForgePresenter, subscribed to ForgeController.OnWeaponForged), not inside this
        // paused modal. This panel's role is slot/item selection only; confirming a choice
        // hands off to the world and resumes normal time scale before the moment plays.
        //
        // EXCEPT for the very first successful forge ever, which also fires the one-shot
        // forge-tutorial cutscene (ForgeController.TryForge → CutscenePlayer.Instance.Play) —
        // see HandleForgeOrUpgradeSuccess (B40).
        private void Forge(int bagIndex)
        {
            if (_forgeController != null && _forgeController.TryForge(bagIndex))
                HandleForgeOrUpgradeSuccess();
        }

        private void Upgrade(int slotIndex)
        {
            if (_forgeController != null && _forgeController.TryUpgrade(slotIndex))
                HandleForgeOrUpgradeSuccess();
        }

        /// <summary>
        /// Decides whether it is safe to unpause immediately after a successful forge/upgrade,
        /// or whether the just-triggered first-forge cutscene needs to run first (B40).
        ///
        /// CutscenePlayer disables player input but never touches Time.timeScale itself. Before
        /// Sprint 0, this panel stayed open (keeping Time.timeScale = 0f) until the player closed
        /// it manually, which incidentally kept the world frozen behind the cutscene too. The
        /// Sprint 0 change made this panel close (and unpause) immediately on every successful
        /// forge — correct for the normal case, but wrong for the first forge specifically: the
        /// cutscene covers the screen and disables input while every enemy in the room keeps
        /// attacking in real time underneath it.
        ///
        /// CutscenePlayer.Play() sets IsPlaying = true synchronously before TryForge() returns,
        /// so checking it here reliably distinguishes "a cutscene just started" from the normal
        /// (non-first-forge) case.
        /// </summary>
        private void HandleForgeOrUpgradeSuccess()
        {
            if (CutscenePlayer.Instance != null && CutscenePlayer.Instance.IsPlaying)
            {
                // Hide this panel's own UI so it isn't visibly stacked under the cutscene overlay,
                // but deliberately leave Time.timeScale / AudioListener.pause untouched — the
                // world (and its enemies) must stay frozen for the cutscene's duration, exactly
                // as it did before Sprint 0. WaitForCutsceneThenClose performs the real Close()
                // once the cutscene actually finishes.
                if (_panel != null) _panel.SetActive(false);

                if (_cutsceneWaitRoutine != null) StopCoroutine(_cutsceneWaitRoutine);
                _cutsceneWaitRoutine = StartCoroutine(WaitForCutsceneThenClose());
                return;
            }

            Close();
        }

        // Polls with a bare `yield return null` rather than WaitForSeconds — this keeps advancing
        // once per Update regardless of Time.timeScale (WaitForSeconds would never elapse while
        // timeScale is 0f, which is exactly the state this coroutine starts in).
        private IEnumerator WaitForCutsceneThenClose()
        {
            while (CutscenePlayer.Instance != null && CutscenePlayer.Instance.IsPlaying)
                yield return null;

            _cutsceneWaitRoutine = null;
            Close();
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
