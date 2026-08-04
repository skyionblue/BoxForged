using UnityEngine;

namespace Boxhead.Core
{
    /// <summary>
    /// Runtime test harness for SaveSystem. Add to a scene alongside SaveSystem
    /// to verify persist/load/delete behaviour without a Canvas.
    ///
    /// Buttons are driven by pure IMGUI (OnGUI) — no Canvas needed.
    /// Also callable from the Inspector right-click context menu.
    /// </summary>
    public class SaveTester : MonoBehaviour
    {
        // Panel dimensions — fixed so the layout never reflows unexpectedly.
        private const int PanelX      = 10;
        private const int PanelY      = 10;
        private const int PanelWidth  = 360;
        private const int RowHeight   = 24;

        // Cached save-file path shown in the status line.
        // Built once in Start so we never allocate in OnGUI.
        private string _savePath;

        // Status message shown after each action (e.g. "Saved." / "Loaded." / "Deleted.").
        private string _statusMessage = "";

        private void Start()
        {
            _savePath = System.IO.Path.Combine(
                Application.persistentDataPath, "save.json");
        }

        // ------------------------------------------------------------------ //
        //  IMGUI panel                                                         //
        // ------------------------------------------------------------------ //

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(PanelX, PanelY, PanelWidth, 400));

            if (SaveSystem.Instance == null)
            {
                GUILayout.Label("SaveSystem not found in scene.");
                GUILayout.EndArea();
                return;
            }

            var data = SaveSystem.Instance.Data;

            // --- Title ---
            GUILayout.Label("=== Save System Test ===");

            // --- Current values ---
            GUILayout.Label("sparkTotal:          " + data.sparkTotal);
            GUILayout.Label("totalRunsCompleted:  " + data.totalRunsCompleted);
            GUILayout.Label("version:             " + data.version);
            GUILayout.Label("lastFightingStyle:   " + data.lastFightingStyle);

            GUILayout.Space(6f);

            // --- Mutator buttons ---
            if (GUILayout.Button("+ Add Spark (10)", GUILayout.Width(PanelWidth - 4)))
            {
                SaveSystem.Instance.Data.sparkTotal += 10;
                SaveSystem.Instance.Save();
                _statusMessage = "Added 10 sparks and saved.";
            }

            if (GUILayout.Button("+ Run Complete", GUILayout.Width(PanelWidth - 4)))
            {
                SaveSystem.Instance.Data.totalRunsCompleted++;
                SaveSystem.Instance.Save();
                _statusMessage = "Run incremented and saved.";
            }

            GUILayout.Space(6f);

            // --- Core action buttons ---
            if (GUILayout.Button("Save", GUILayout.Width(PanelWidth - 4)))
            {
                DoSave();
            }

            if (GUILayout.Button("Load", GUILayout.Width(PanelWidth - 4)))
            {
                DoLoad();
            }

            if (GUILayout.Button("Delete All", GUILayout.Width(PanelWidth - 4)))
            {
                DoDeleteAll();
            }

            GUILayout.Space(6f);

            // --- Status and path ---
            GUILayout.Label(_statusMessage);
            GUILayout.Label("Path: " + _savePath);

            GUILayout.EndArea();
        }

        // ------------------------------------------------------------------ //
        //  Context-menu actions (also wired to OnGUI buttons above)           //
        // ------------------------------------------------------------------ //

        [ContextMenu("Save")]
        private void DoSave()
        {
            if (SaveSystem.Instance == null)
            {
                Debug.LogWarning("[SaveTester] SaveSystem.Instance is null — cannot save.");
                _statusMessage = "ERROR: SaveSystem not found.";
                return;
            }

            SaveSystem.Instance.Save();
            _statusMessage = "Saved.";
        }

        [ContextMenu("Load")]
        private void DoLoad()
        {
            if (SaveSystem.Instance == null)
            {
                Debug.LogWarning("[SaveTester] SaveSystem.Instance is null — cannot load.");
                _statusMessage = "ERROR: SaveSystem not found.";
                return;
            }

            SaveSystem.Instance.Load();
            _statusMessage = "Loaded.";
        }

        [ContextMenu("Delete All")]
        private void DoDeleteAll()
        {
            if (SaveSystem.Instance == null)
            {
                Debug.LogWarning("[SaveTester] SaveSystem.Instance is null — cannot delete.");
                _statusMessage = "ERROR: SaveSystem not found.";
                return;
            }

            SaveSystem.Instance.DeleteAll();
            _statusMessage = "Deleted. Data reset to defaults.";
        }
    }
}
