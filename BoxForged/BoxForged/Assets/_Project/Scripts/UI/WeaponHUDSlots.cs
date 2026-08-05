using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Boxhead.Systems;

namespace Boxhead.UI
{
    /// <summary>
    /// 3-slot weapon HUD. Replaces the 2-slot WeaponSlotUI (that file is kept for reference).
    /// Shows weapon icons, durability bars, active highlight, and cardboard count.
    /// Also handles mobile swipe-to-cycle and exposes cycle buttons for on-screen controls.
    /// </summary>
    public class WeaponHUDSlots : MonoBehaviour
    {
        [Header("Weapon Slots")]
        [SerializeField] private Image[]           _slotIcons;         // length 3
        [SerializeField] private Image[]           _durabilityBars;    // length 3, filled images
        [SerializeField] private TextMeshProUGUI[] _slotNameTexts;     // length 3, optional
        [SerializeField] private Image[]           _activeHighlights;  // length 3

        [Header("Slot Colors")]
        [SerializeField] private Color _activeSlotColor   = Color.white;
        [SerializeField] private Color _inactiveSlotColor = new Color(1f, 1f, 1f, 0.4f);
        [SerializeField] private Color _emptySlotColor    = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Header("Durability Bar Colors")]
        [SerializeField] private Color _durabilityFull = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _durabilityMid  = new Color(0.9f, 0.7f, 0.1f, 1f);
        [SerializeField] private Color _durabilityLow  = new Color(0.85f, 0.15f, 0.1f, 1f);

        [Header("Cardboard Counter")]
        [SerializeField] private TextMeshProUGUI _cardboardCountText;

        [Header("Slot Buttons (optional — auto-wired in Awake)")]
        [SerializeField] private UnityEngine.UI.Button[] _slotButtons; // length 3, tapping switches active slot

        [Header("Swipe Detection")]
        [SerializeField] private float _swipeThreshold = 50f;

        // Cached component references — resolved in Awake, used in OnEnable/OnDisable.
        private WeaponInventory   _weaponInventory;
        private CardboardResource _cardboardResource;
        private WeaponDurability  _weaponDurability;

        // Zero-alloc text building for cardboard count.
        private readonly StringBuilder _sb = new StringBuilder(8);

        // Swipe state — all value types, no GC.
        private bool  _swipeLock;
        private float _swipeLockTimer;
        private const float SwipeLockDuration = 0.25f;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null)
            {
                Debug.LogError("[WeaponHUDSlots] No GameObject with tag 'Player' found.", this);
                return;
            }

            playerGO.TryGetComponent(out _weaponInventory);
            playerGO.TryGetComponent(out _cardboardResource);
            playerGO.TryGetComponent(out _weaponDurability);

            if (_weaponInventory == null)
                Debug.LogWarning("[WeaponHUDSlots] Player has no WeaponInventory.", this);
            if (_cardboardResource == null)
                Debug.LogWarning("[WeaponHUDSlots] Player has no CardboardResource.", this);
            if (_weaponDurability == null)
                Debug.LogWarning("[WeaponHUDSlots] Player has no WeaponDurability.", this);

            // Wire slot buttons — find Button components on child slot panels if not serialized
            if (_slotButtons == null || _slotButtons.Length < 3)
            {
                _slotButtons = GetComponentsInChildren<UnityEngine.UI.Button>(true);
            }
            for (int i = 0; i < _slotButtons.Length && i < WeaponInventory.WeaponSlotCount; i++)
            {
                int slotIndex = i;
                _slotButtons[i].onClick.AddListener(() => _weaponInventory?.SetActiveSlot(slotIndex));
            }
        }

        private void OnEnable()
        {
            if (_weaponInventory != null)
                _weaponInventory.OnInventoryChanged += OnInventoryChanged;

            if (_cardboardResource != null)
                _cardboardResource.OnCardboardChanged += OnCardboardChanged;

            if (_weaponDurability != null)
                _weaponDurability.OnWeaponDamaged += OnWeaponDamaged;

            // Sync UI immediately when this component becomes active.
            RefreshAllSlots();
            SyncCardboard(_cardboardResource != null ? _cardboardResource.Current : 0);
        }

        private void OnDisable()
        {
            if (_weaponInventory != null)
                _weaponInventory.OnInventoryChanged -= OnInventoryChanged;

            if (_cardboardResource != null)
                _cardboardResource.OnCardboardChanged -= OnCardboardChanged;

            if (_weaponDurability != null)
                _weaponDurability.OnWeaponDamaged -= OnWeaponDamaged;
        }

        private void Update()
        {
            DetectSwipe();
        }

        // ── Event Handlers ────────────────────────────────────────────────────

        // Called by WeaponInventory.OnInventoryChanged (no parameters).
        private void OnInventoryChanged()
        {
            RefreshAllSlots();
        }

        // Called by CardboardResource.OnCardboardChanged — zero GC: int param, no boxing.
        private void OnCardboardChanged(int amount)
        {
            SyncCardboard(amount);
        }

        // Called by WeaponDurability.OnWeaponDamaged — refresh only the affected slot.
        private void OnWeaponDamaged(WeaponInstance damaged)
        {
            if (_weaponInventory == null) return;

            WeaponInstance[] slots = _weaponInventory.WeaponSlots;
            for (int i = 0; i < slots.Length; i++)
            {
                if (ReferenceEquals(slots[i], damaged))
                {
                    RefreshSlot(i, slots[i], _weaponInventory.ActiveSlotIndex == i);
                    return;
                }
            }
        }

        // ── Public API for mobile HUD buttons ────────────────────────────────

        public void CycleWeaponForward()  { _weaponInventory?.CycleActiveSlot(1); }
        public void CycleWeaponBack()     { _weaponInventory?.CycleActiveSlot(-1); }

        // ── Swipe Detection ───────────────────────────────────────────────────

        // Touch-delta swipe detection. No coroutines, no GC — bool + float timer.
        private void DetectSwipe()
        {
            // Cooldown so a single swipe doesn't cycle multiple times.
            if (_swipeLock)
            {
                _swipeLockTimer -= Time.deltaTime;
                if (_swipeLockTimer <= 0f) _swipeLock = false;
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            // In the editor/desktop fall back to mouse delta for testing.
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                float delta = Mouse.current.delta.x.ReadValue();
                if (Mathf.Abs(delta) > _swipeThreshold)
                {
                    if (delta > 0f) CycleWeaponForward();
                    else            CycleWeaponBack();
                    LockSwipe();
                }
            }
#else
            if (Touchscreen.current == null) return;

            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    float delta = touch.delta.x.ReadValue();
                    if (Mathf.Abs(delta) > _swipeThreshold)
                    {
                        if (delta > 0f) CycleWeaponForward();
                        else            CycleWeaponBack();
                        LockSwipe();
                        return;
                    }
                }
            }
#endif
        }

        private void LockSwipe()
        {
            _swipeLock      = true;
            _swipeLockTimer = SwipeLockDuration;
        }

        // ── Refresh Helpers ───────────────────────────────────────────────────

        private void RefreshAllSlots()
        {
            if (_weaponInventory == null) return;

            WeaponInstance[] slots = _weaponInventory.WeaponSlots;
            // WeaponSlots is initialized in WeaponInventory.Awake. If the HUD's Awake runs
            // first (Unity execution order is not guaranteed across GameObjects), WeaponSlots
            // will be null on the first OnEnable call. Guard here instead of relying on order.
            if (slots == null) return;

            int activeIdx  = _weaponInventory.ActiveSlotIndex;
            int slotCount  = Mathf.Min(slots.Length, WeaponInventory.WeaponSlotCount);

            for (int i = 0; i < slotCount; i++)
                RefreshSlot(i, slots[i], activeIdx == i);
        }

        // Refreshes one slot. No allocation — all fields are value types or cached refs.
        private void RefreshSlot(int i, WeaponInstance weapon, bool isActive)
        {
            bool hasWeapon = weapon != null;

            // ── Icon ──────────────────────────────────────────────────────────
            if (_slotIcons != null && i < _slotIcons.Length && _slotIcons[i] != null)
            {
                if (hasWeapon)
                {
                    Sprite icon = GetTierIcon(weapon);
                    _slotIcons[i].sprite = icon;
                    _slotIcons[i].color  = isActive ? _activeSlotColor : _inactiveSlotColor;
                    _slotIcons[i].enabled = true;
                }
                else
                {
                    _slotIcons[i].sprite  = null;
                    _slotIcons[i].color   = _emptySlotColor;
                    _slotIcons[i].enabled = true; // keep visible as placeholder
                }
            }

            // ── Name Text ─────────────────────────────────────────────────────
            if (_slotNameTexts != null && i < _slotNameTexts.Length && _slotNameTexts[i] != null)
            {
                if (hasWeapon)
                {
                    string displayName = GetDisplayName(weapon);
                    // Equality guard avoids SetText call when nothing changed.
                    if (_slotNameTexts[i].text != displayName)
                        _slotNameTexts[i].SetText(displayName);
                }
                else
                {
                    if (_slotNameTexts[i].text.Length > 0)
                        _slotNameTexts[i].SetText(string.Empty);
                }
            }

            // ── Active Highlight ──────────────────────────────────────────────
            if (_activeHighlights != null && i < _activeHighlights.Length && _activeHighlights[i] != null)
                _activeHighlights[i].enabled = isActive && hasWeapon;

            // ── Durability Bar ────────────────────────────────────────────────
            if (_durabilityBars != null && i < _durabilityBars.Length && _durabilityBars[i] != null)
            {
                if (!hasWeapon)
                {
                    _durabilityBars[i].enabled = false;
                }
                else
                {
                    _durabilityBars[i].enabled = true;
                    float fill = weapon.MaxDurability > 0
                        ? (float)weapon.CurrentDurability / weapon.MaxDurability
                        : 0f;
                    _durabilityBars[i].fillAmount = fill;
                    _durabilityBars[i].color      = DurabilityColor(fill);
                }
            }
        }

        // Zero-alloc cardboard display — appends int to StringBuilder, passes to SetText.
        private void SyncCardboard(int amount)
        {
            if (_cardboardCountText == null) return;
            _sb.Clear();
            _sb.Append("Cardboard: ");
            _sb.Append(amount);
            _cardboardCountText.SetText(_sb);
        }

        // Returns the appropriate tier icon from WeaponObjectSO. No allocation.
        private static Sprite GetTierIcon(WeaponInstance weapon)
        {
            if (weapon.Tier == WeaponTier.Epic      && weapon.Data.epicIcon      != null) return weapon.Data.epicIcon;
            if (weapon.Tier == WeaponTier.Legendary && weapon.Data.legendaryIcon != null) return weapon.Data.legendaryIcon;
            return weapon.Data.weaponIcon; // Standard (or fallback when tier icons are missing)
        }

        // Legendary weapons show the designed weapon name; others show the raw object name.
        private static string GetDisplayName(WeaponInstance weapon)
        {
            if (weapon.Tier == WeaponTier.Legendary && !string.IsNullOrEmpty(weapon.Data.weaponName))
                return weapon.Data.weaponName;
            return weapon.Data.rawObjectName;
        }

        // Maps a normalized fill value to the correct durability colour.
        private Color DurabilityColor(float fill)
        {
            if (fill > 0.5f)  return _durabilityFull;
            if (fill > 0.2f)  return _durabilityMid;
            return _durabilityLow;
        }
    }
}
