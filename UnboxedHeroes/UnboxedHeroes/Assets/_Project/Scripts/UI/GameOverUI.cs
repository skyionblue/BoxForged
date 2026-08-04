using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Core;

namespace Boxhead.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button _exitButton;

        [Header("Win")]
        [SerializeField] private string winTitle = "You Win!";
        [SerializeField] private string winSubtitle = "Imagination prevails.";

        [Header("Lose")]
        [SerializeField] private string loseTitle = "You Lose";
        [SerializeField] private string loseSubtitle = "The Unimaginative win... for now.";

        private void Awake()
        {
            // Panel starts inactive in the scene; don't set it here or we'll
            // fight with Show() — activating the panel triggers Awake, which
            // would immediately deactivate it again before Show() finishes.
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
            _exitButton?.onClick.AddListener(OnExitClicked);
        }

        public void Show(bool won)
        {
            if (panel != null) panel.SetActive(true);

            if (titleText != null)
                titleText.text = won ? winTitle : loseTitle;

            if (subtitleText != null)
                subtitleText.text = won ? winSubtitle : loseSubtitle;

            // Pause time so combat stops during the overlay
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        private void OnExitClicked()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameManager.Instance?.Restart();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);
            _exitButton?.onClick.RemoveListener(OnExitClicked);
        }
    }
}
