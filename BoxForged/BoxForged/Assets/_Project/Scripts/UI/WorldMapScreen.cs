using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Boxhead.Core;

namespace Boxhead.UI
{
    /// <summary>
    /// World Map screen — lets the player choose which zone to start a new run in.
    /// Zone 0 (Cul-de-Sac) is always available.
    /// Zone 1 (Town Square) is unlocked when SaveData.highestZoneReached >= 1.
    ///
    /// Panel starts INACTIVE in the scene — do not call SetActive(false) in Awake.
    /// MetaScreen holds a [SerializeField] reference and calls Show()/Hide() directly.
    /// This class intentionally has no singleton — it is a peer panel owned by MetaScreen.
    ///
    /// Zone index mapping lives in GameManager.ZoneIndexByScene — single source of truth.
    /// </summary>
    public class WorldMapScreen : MonoBehaviour
    {

        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("Zone 0 — Cul-de-Sac")]
        [SerializeField] private Button _culDeSacButton;
        [SerializeField] private Image  _culDeSacNode;

        [Header("Zone 1 — Town Square")]
        [SerializeField] private Button _townSquareButton;
        [SerializeField] private Image  _townSquareNode;

        [Header("Navigation")]
        [SerializeField] private Button _closeButton;

        // Owner — set by MetaScreen so OnClose() can return to it without a singleton lookup.
        private MetaScreen _owner;

        // Alpha values for locked vs unlocked node appearance.
        private const float AlphaUnlocked = 1.0f;
        private const float AlphaDimmed   = 0.4f;

        // -----------------------------------------------------------------------

        private void Awake()
        {
            // Wire listeners once — no per-Show lambda allocation.
            if (_culDeSacButton  != null) _culDeSacButton.onClick.AddListener(OnCulDeSacSelected);
            if (_townSquareButton != null) _townSquareButton.onClick.AddListener(OnTownSquareSelected);
            if (_closeButton      != null) _closeButton.onClick.AddListener(OnClose);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Press M in-editor or dev builds to open the World Map without running a full session.
        // Guard: only fires when the game is not actively playing (avoids freezing boss fights).
        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameManager.GameState.Playing)
                return;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.mKey.wasPressedThisFrame)
                Show(null);
        }
#endif

        // -----------------------------------------------------------------------

        /// <summary>
        /// Activates the panel and configures zone availability from save data.
        /// </summary>
        /// <param name="owner">The MetaScreen that opened this panel — used by OnClose to return.</param>
        public void Show(MetaScreen owner)
        {
            Debug.LogWarning($"[WorldMapScreen] Show() called — _panel={((_panel != null) ? _panel.name : "NULL")} gameObject.activeInHierarchy={gameObject.activeInHierarchy}");
            if (_panel == null)
            {
                Debug.LogError("[WorldMapScreen] _panel is not assigned — cannot open World Map.", this);
                return;
            }

            _owner = owner;
            _panel.SetActive(true);
            Time.timeScale      = 0f;
            AudioListener.pause = true;
            Debug.LogWarning($"[WorldMapScreen] Panel activated — TimeScale={Time.timeScale} panel.activeSelf={_panel.activeSelf} panel.activeInHierarchy={_panel.activeInHierarchy}");

            int highestZone = SaveSystem.Instance != null
                ? SaveSystem.Instance.Data.highestZoneReached
                : 0;

            // The current scene's zone is always considered reached — the player is already there.
            string currentScene = SceneManager.GetActiveScene().name;
            if (GameManager.ZoneIndexByScene.TryGetValue(currentScene, out int currentZoneIndex))
                highestZone = Mathf.Max(highestZone, currentZoneIndex);

            // Zone 0 is always available.
            SetNodeState(_culDeSacNode, _culDeSacButton, unlocked: true);

            // Zone 1 unlocks once the player has reached Town Square (beaten Cul-de-Sac boss
            // OR is currently in Town Square).
            bool townSquareUnlocked = highestZone >= 1;
            SetNodeState(_townSquareNode, _townSquareButton, townSquareUnlocked);
        }

        public void Hide()
        {
            AudioListener.pause = false;
            Time.timeScale      = 1f;
            if (_panel != null) _panel.SetActive(false);
        }

        // -----------------------------------------------------------------------

        // Zone 0's start scene — GameManager.ZoneStartScene[0] is the single source of truth
        // (currently CulDeSac_WildWestCity, ADR-0004). Do not hardcode a scene name here.
        private void OnCulDeSacSelected()  => OnZoneSelected(GameManager.ZoneStartScene[0]);
        private void OnTownSquareSelected() => OnZoneSelected("TownSquare_Room1");

        private void OnZoneSelected(string sceneName)
        {
            // Clear run selection so the new zone shows the character picker, not a silent restore.
            Boxhead.Core.ProgressionSystem.Instance?.ClearRunSelection();
            // New zone = fresh run: wipe carried cardboard/weapons so the next zone's
            // GameManager.Start() does not restore the prior zone's loadout.
            Boxhead.Core.ProgressionSystem.Instance?.ClearRunLoadout();
            // Restore time before loading — SceneManager.LoadScene does not reset timeScale.
            Hide();
            SceneManager.LoadScene(sceneName);
        }

        private void OnClose()
        {
            Hide();
            _owner?.Show();
        }

        // -----------------------------------------------------------------------

        private static void SetNodeState(Image nodeImage, Button nodeButton, bool unlocked)
        {
            if (nodeImage != null)
            {
                Color c = nodeImage.color;
                c.a = unlocked ? AlphaUnlocked : AlphaDimmed;
                nodeImage.color = c;
            }

            if (nodeButton != null)
                nodeButton.interactable = unlocked;
        }

        private void OnDestroy()
        {
            if (_culDeSacButton  != null) _culDeSacButton.onClick.RemoveListener(OnCulDeSacSelected);
            if (_townSquareButton != null) _townSquareButton.onClick.RemoveListener(OnTownSquareSelected);
            if (_closeButton      != null) _closeButton.onClick.RemoveListener(OnClose);
        }
    }
}
