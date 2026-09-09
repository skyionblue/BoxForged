using System;
using System.IO;
using UnityEngine;

namespace Boxhead.Core
{
    /// <summary>
    /// Mirrors every Debug.Log/Warning/Error/Exception to a plain-text file in
    /// Application.persistentDataPath for the current session. Added for B142 (docs/BACKLOG.md):
    /// an intermittent SpinCycle win-screen/movement-freeze bug that has never reproduced on
    /// demand, and needs the [SpinCycleAI]/[GameManager] diagnostic trace from whichever real
    /// play session it next occurs in — which usually will not be tethered to Xcode at that
    /// exact moment. Pull the file afterward via Xcode's Window > Devices and Simulators >
    /// select the device > select BoxForged > "Download Container", then look inside the
    /// extracted .xcappdata under AppData/Documents/session_log.txt.
    /// Overwrites on each app launch — this is a single current-session trace, not a
    /// long-term log archive.
    /// </summary>
    public class PersistentLogger : MonoBehaviour
    {
        public static PersistentLogger Instance { get; private set; }

        private const string FileName = "session_log.txt";
        private StreamWriter _writer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            string logPath = Path.Combine(Application.persistentDataPath, FileName);
            try
            {
                _writer = new StreamWriter(logPath, append: false) { AutoFlush = true };
                _writer.WriteLine($"=== Session start {DateTime.Now:yyyy-MM-dd HH:mm:ss} — {Application.version} ===");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PersistentLogger] Could not open {logPath}: {e.Message}");
                _writer = null;
            }

            Application.logMessageReceived += HandleLog;
        }

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (_writer == null) return;
            try
            {
                _writer.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{type}] {condition}");
                if (type == LogType.Exception || type == LogType.Error)
                    _writer.WriteLine(stackTrace);
            }
            catch
            {
                // Best-effort diagnostic aid — a logging failure must never affect gameplay.
            }
        }

        // iOS can terminate a backgrounded app without warning — flush eagerly rather than
        // waiting for OnDestroy, which is not guaranteed to run in that case.
        private void OnApplicationPause(bool paused)
        {
            if (paused) _writer?.Flush();
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
            if (Instance == this) Instance = null;
            _writer?.Flush();
            _writer?.Dispose();
        }
    }
}
