using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Boxhead.Core;

namespace Boxhead.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private InputActionReference _pauseAction;

        private bool _isPaused;

        private void Awake()
        {
            if (_panel == null)
            {
                Debug.LogError($"[PauseMenu] _panel is not assigned on {gameObject.name}. Disabling component.", this);
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if (_pauseAction == null) return;
            // Do not call Enable()/Disable() on a shared InputActionReference — PlayerInput owns the lifecycle.
            _pauseAction.action.performed += OnPauseInput;
        }

        private void OnDisable()
        {
            if (_pauseAction == null) return;
            _pauseAction.action.performed -= OnPauseInput;
        }

        private void OnDestroy()
        {
            if (_isPaused)
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
            }
        }

        private void OnPauseInput(InputAction.CallbackContext ctx)
        {
            if (_isPaused)
            {
                Resume();
                return;
            }

            if (!CanPause()) return;

            Pause();
        }

        /// <summary>Called by on-screen Pause button.</summary>
        public void TogglePause()
        {
            if (!_isPaused)
            {
                if (!CanPause()) return;
                Pause();
            }
            else
            {
                Resume();
            }
        }

        private bool CanPause()
        {
            var gm = GameManager.Instance;
            return gm == null || gm.State == GameManager.GameState.Playing;
        }

        private void Pause()
        {
            _isPaused = true;
            _panel.SetActive(true);
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        public void Resume()
        {
            _isPaused = false;
            _panel.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        public void Restart()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Resets all meta-progression (Spark, stat levels, permanent unlocks) to zero.
        /// Useful for a fresh-start experience. Does NOT reset run count or scene state.
        /// </summary>
        public void ResetProgression()
        {
            var save = Core.SaveSystem.Instance;
            if (save == null) return;

            save.Data.sparkTotal       = 0;
            save.Data.statLevels       = new int[5];
            save.Data.characterLevel   = 0;
            save.Data.permanentUnlocks = new string[0];
            save.Save();

            // A fresh start also replays the one-shot cutscenes (intro, showcases, first forge).
            Core.CutsceneFlags.ClearAll();

            // Rebuild the in-memory stat overlay from the now-zeroed save data so the
            // next run starts clean without requiring a full app restart.
            // ProgressionSystem is DontDestroyOnLoad — its Awake only runs once, so
            // _overlay is never rebuilt automatically on scene reload.
            Core.ProgressionSystem.Instance?.RebuildOverlay();
            Core.ProgressionSystem.Instance?.ForceRefreshSparkUI();

            Debug.Log("[PauseMenu] Progression reset: Spark and stat levels cleared.");

            Restart();
        }

        /// <summary>Quit button — restores timeScale before quitting so the editor doesn't get stuck.</summary>
        public void Quit()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
