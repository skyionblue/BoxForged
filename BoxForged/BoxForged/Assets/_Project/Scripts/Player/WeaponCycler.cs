using UnityEngine;
using UnityEngine.InputSystem;
using Boxhead.Systems;

namespace Boxhead.Player
{
    // Editor-only play-mode helper. Cycle weapons with Q (prev) / E (next).
    // Shows current weapon name + index on screen.
    [RequireComponent(typeof(WeaponHolder))]
    [RequireComponent(typeof(Inventory))]
    public class WeaponCycler : MonoBehaviour
    {
        [System.Serializable]
        public class CharacterWeaponSet
        {
            public string      characterModelName;
            public WeaponData[] weapons;
        }

        [SerializeField] private WeaponData[]          defaultWeapons;
        [SerializeField] private CharacterWeaponSet[]  characterWeaponSets;

        private WeaponHolder  _holder;
        private Inventory     _inventory;
        private int           _index;
        private WeaponData[]  _activeWeapons;

        private void Awake()
        {
            _holder    = GetComponent<WeaponHolder>();
            _inventory = GetComponent<Inventory>();
        }

        private void Start()
        {
            _activeWeapons = ResolveWeaponArray();
            if (_activeWeapons == null || _activeWeapons.Length == 0) return;
            // Use SetEquipped (not Equip) so cycling never accumulates a backpack slot.
            // This also fires OnInventoryChanged, which updates the HUD immediately.
            _inventory.SetEquipped(_activeWeapons[_index]);
        }

        private void Update()
        {
            if (_activeWeapons == null || _activeWeapons.Length == 0) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.eKey.wasPressedThisFrame)
            {
                _index = (_index + 1) % _activeWeapons.Length;
                // SetEquipped replaces the equipped slot and fires OnInventoryChanged
                // without pushing the previous weapon to the backpack slot.
                _inventory.SetEquipped(_activeWeapons[_index]);
            }
            else if (keyboard.qKey.wasPressedThisFrame)
            {
                _index = (_index - 1 + _activeWeapons.Length) % _activeWeapons.Length;
                _inventory.SetEquipped(_activeWeapons[_index]);
            }
        }

        private void OnGUI()
        {
            if (_activeWeapons == null || _activeWeapons.Length == 0) return;

            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize  = 20,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = Color.white;

            string label = _activeWeapons[_index] != null
                ? $"[{_index + 1}/{_activeWeapons.Length}]  {_activeWeapons[_index].weaponName}\nQ = prev   E = next"
                : $"[{_index + 1}/{_activeWeapons.Length}]  (null)";

            GUI.Box(new Rect(10, 10, 340, 60), label, style);
        }

        /// <summary>
        /// Given a base WeaponData (no character suffix), returns the variant whose name
        /// starts with the base name from the active character's weapon set.
        /// Falls back to baseWeapon if no character-specific match is found.
        /// Called by WeaponPickup so pickups always equip the correctly-fitted grip.
        /// </summary>
        public WeaponData ResolveWeapon(WeaponData baseWeapon)
        {
            if (baseWeapon == null) return null;
            WeaponData[] charWeapons = ResolveWeaponArray();
            // If ResolveWeaponArray returned the default set, we're already using base weapons
            if (charWeapons == defaultWeapons || charWeapons == null) return baseWeapon;
            // Find the character-specific variant whose name starts with the base name
            string baseName = baseWeapon.name;
            for (int i = 0; i < charWeapons.Length; i++)
                if (charWeapons[i] != null && charWeapons[i].name.StartsWith(baseName))
                    return charWeapons[i];
            return baseWeapon;
        }

        // Walks this transform's direct children to find the active character model,
        // then returns the matching weapon set — or defaultWeapons if no match is found.
        // Always searches `transform` (the player root), never `transform.parent`, so
        // this works correctly whether the player is at scene root or nested under an
        // organiser container like [Player].
        private WeaponData[] ResolveWeaponArray()
        {
            if (characterWeaponSets != null && characterWeaponSets.Length > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    if (!child.gameObject.activeSelf) continue;

                    for (int j = 0; j < characterWeaponSets.Length; j++)
                    {
                        if (child.name == characterWeaponSets[j].characterModelName &&
                            characterWeaponSets[j].weapons != null &&
                            characterWeaponSets[j].weapons.Length > 0)
                        {
                            return characterWeaponSets[j].weapons;
                        }
                    }
                }
            }

            return defaultWeapons;
        }
    }
}
