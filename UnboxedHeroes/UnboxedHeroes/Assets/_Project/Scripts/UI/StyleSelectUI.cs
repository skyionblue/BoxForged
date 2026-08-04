using UnityEngine;
using UnityEngine.UI;
using Boxhead.Player;

namespace Boxhead.UI
{
    public class StyleSelectUI : MonoBehaviour
    {
        [SerializeField] private FightingStyleData[] _availableStyles;

        private CombatController _playerCombat;
        private UnityEngine.InputSystem.PlayerInput _playerInput;

        private void Awake()
        {
            _playerCombat = FindAnyObjectByType<CombatController>();
            if (_playerCombat != null)
                _playerInput = _playerCombat.GetComponent<UnityEngine.InputSystem.PlayerInput>();

            Debug.Log($"[StyleSelectUI] Awake — combat={(_playerCombat != null ? "found" : "NULL")} input={(_playerInput != null ? "found" : "NULL")}");
        }

        private void Start()
        {
            // Wire buttons in code — bypasses any persistent listener deserialization issues
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                string n = btn.name;
                if (n == "Button_Ninja")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnStyleSelected(0));
                    Debug.Log("[StyleSelectUI] Wired Button_Ninja -> OnStyleSelected(0)");
                }
                else if (n == "Button_Cowboy")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnStyleSelected(1));
                    Debug.Log("[StyleSelectUI] Wired Button_Cowboy -> OnStyleSelected(1)");
                }
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (_playerInput != null) _playerInput.enabled = false;
            Debug.Log($"[StyleSelectUI] Show — playerInput disabled: {(_playerInput != null)}");
        }

        public void Hide()
        {
            Debug.Log("[StyleSelectUI] Hide called");
            if (_playerInput != null) _playerInput.enabled = true;
            gameObject.SetActive(false);
        }

        public void OnStyleSelected(int index)
        {
            Debug.Log($"[StyleSelectUI] OnStyleSelected({index}) — styles={(_availableStyles?.Length ?? -1)} combat={(_playerCombat != null ? "ok" : "NULL")}");

            if (_availableStyles == null || index < 0 || index >= _availableStyles.Length)
            {
                Debug.LogWarning($"[StyleSelectUI] Style index {index} out of range.");
                return;
            }

            _playerCombat?.SetFightingStyle(_availableStyles[index]);
            Hide();
        }
    }
}
