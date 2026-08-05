using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Boxhead.Core;
using Boxhead.UI;

namespace Boxhead.Editor
{
    public static class Sprint4SceneSetup
    {
        [MenuItem("BoxForged/Setup Sprint 4 Scene")]
        public static void SetupScene()
        {
            // ── AudioManager ─────────────────────────────────────────────
            var audioManagerGO = new GameObject("AudioManager");
            audioManagerGO.AddComponent<AudioManager>();
            Undo.RegisterCreatedObjectUndo(audioManagerGO, "Create AudioManager");

            // ── GameManager ───────────────────────────────────────────────
            var gameManagerGO = new GameObject("GameManager");
            var gameManager = gameManagerGO.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gameManagerGO, "Create GameManager");

            // ── HUD Canvas ────────────────────────────────────────────────
            var hudCanvas = CreateCanvas("HUD_Canvas", sortOrder: 0);

            // Health bar background panel
            var healthBg = CreatePanel(hudCanvas.transform, "HealthBar_BG",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(0, 1),
                pivot: new Vector2(0, 1),
                offsetMin: new Vector2(20, -80), offsetMax: new Vector2(220, -20),
                color: new Color(0, 0, 0, 0.6f));

            // Health slider
            var sliderGO = new GameObject("HealthSlider");
            sliderGO.transform.SetParent(healthBg.transform, false);
            var sliderRect = sliderGO.AddComponent<RectTransform>();
            sliderRect.anchorMin = Vector2.zero;
            sliderRect.anchorMax = Vector2.one;
            sliderRect.offsetMin = new Vector2(8, 8);
            sliderRect.offsetMax = new Vector2(-8, -8);
            var slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.interactable = false;

            // Slider fill area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGO.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.8f, 0.2f);
            slider.fillRect = fillRect;

            // Counter window indicator (small pulsing square top-right)
            var counterIndicator = CreatePanel(hudCanvas.transform, "CounterWindowIndicator",
                anchorMin: new Vector2(1, 1), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(1, 1),
                offsetMin: new Vector2(-80, -80), offsetMax: new Vector2(-20, -20),
                color: Color.yellow);
            counterIndicator.SetActive(false);

            // HUDController component
            var hudGO = new GameObject("HUDController");
            hudGO.transform.SetParent(hudCanvas.transform, false);
            var hudController = hudGO.AddComponent<HUDController>();

            // Wire HUD references via SerializedObject
            var hudSO = new SerializedObject(hudController);
            hudSO.FindProperty("healthSlider").objectReferenceValue = slider;
            hudSO.FindProperty("healthFill").objectReferenceValue = fillImage;
            hudSO.FindProperty("counterWindowIndicator").objectReferenceValue = counterIndicator;
            hudSO.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(hudCanvas, "Create HUD Canvas");

            // ── Game Over Canvas ──────────────────────────────────────────
            var goCanvas = CreateCanvas("GameOver_Canvas", sortOrder: 10);

            // Full-screen dark panel
            var overlayPanel = CreatePanel(goCanvas.transform, "Overlay",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f),
                offsetMin: Vector2.zero, offsetMax: Vector2.zero,
                color: new Color(0, 0, 0, 0.75f));

            // Title text
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(overlayPanel.transform, false);
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0.55f);
            titleRect.anchorMax = new Vector2(0.8f, 0.75f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "You Win!";
            titleTMP.fontSize = 64;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.white;

            // Subtitle text
            var subtitleGO = new GameObject("Subtitle");
            subtitleGO.transform.SetParent(overlayPanel.transform, false);
            var subtitleRect = subtitleGO.AddComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.2f, 0.42f);
            subtitleRect.anchorMax = new Vector2(0.8f, 0.56f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;
            var subtitleTMP = subtitleGO.AddComponent<TextMeshProUGUI>();
            subtitleTMP.text = "Imagination prevails.";
            subtitleTMP.fontSize = 28;
            subtitleTMP.alignment = TextAlignmentOptions.Center;
            subtitleTMP.color = new Color(0.9f, 0.9f, 0.9f);

            // Restart button
            var btnGO = new GameObject("RestartButton");
            btnGO.transform.SetParent(overlayPanel.transform, false);
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.35f, 0.25f);
            btnRect.anchorMax = new Vector2(0.65f, 0.38f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            var btnImage = btnGO.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.5f, 0.9f);
            var btn = btnGO.AddComponent<Button>();

            var btnLabelGO = new GameObject("Label");
            btnLabelGO.transform.SetParent(btnGO.transform, false);
            var btnLabelRect = btnLabelGO.AddComponent<RectTransform>();
            btnLabelRect.anchorMin = Vector2.zero;
            btnLabelRect.anchorMax = Vector2.one;
            btnLabelRect.offsetMin = Vector2.zero;
            btnLabelRect.offsetMax = Vector2.zero;
            var btnLabel = btnLabelGO.AddComponent<TextMeshProUGUI>();
            btnLabel.text = "Restart";
            btnLabel.fontSize = 32;
            btnLabel.alignment = TextAlignmentOptions.Center;
            btnLabel.color = Color.white;

            // GameOverUI component on the panel
            var gameOverUI = overlayPanel.AddComponent<GameOverUI>();
            overlayPanel.SetActive(false);

            var goSO = new SerializedObject(gameOverUI);
            goSO.FindProperty("panel").objectReferenceValue = overlayPanel;
            goSO.FindProperty("titleText").objectReferenceValue = titleTMP;
            goSO.FindProperty("subtitleText").objectReferenceValue = subtitleTMP;
            goSO.FindProperty("restartButton").objectReferenceValue = btn;
            goSO.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(goCanvas, "Create GameOver Canvas");

            // Wire GameOverUI into GameManager
            var gmSO = new SerializedObject(gameManager);
            gmSO.FindProperty("gameOverUI").objectReferenceValue = gameOverUI;
            gmSO.ApplyModifiedProperties();

            // ── Save Scene ────────────────────────────────────────────────
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[BoxForged] Sprint 4 scene setup complete. Save the scene (Ctrl+S) to persist.");
            EditorUtility.DisplayDialog(
                "Sprint 4 Setup",
                "Scene wired!\n\n• AudioManager\n• GameManager\n• HUD Canvas (health bar + counter indicator)\n• GameOver Canvas (overlay + restart button)\n\nSave the scene with Ctrl+S.",
                "OK");
        }

        private static GameObject CreateCanvas(string name, int sortOrder)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }
    }
}
