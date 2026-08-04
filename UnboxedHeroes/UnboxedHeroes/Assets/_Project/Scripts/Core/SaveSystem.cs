using UnityEngine;

namespace Boxhead.Core
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private SaveData _data = new SaveData();
        private string   _savePath;

        // Read-only access to live data.  Other systems read and mutate fields
        // directly — e.g. SaveSystem.Instance.Data.sparkTotal++; then Save().
        public SaveData Data => _data;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _savePath = System.IO.Path.Combine(Application.persistentDataPath, "save.json");

            // Load immediately so data is ready before any other system's Start().
            Load();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Flush to disk when the app is backgrounded on mobile — prevents losing
        // Spark earned mid-run if the OS terminates the process while paused.
        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        // Serialize live data to disk synchronously.
        public void Save()
        {
            string json = JsonUtility.ToJson(_data, prettyPrint: true);
            System.IO.File.WriteAllText(_savePath, json);
            Debug.Log($"[SaveSystem] Saved to {_savePath}");
        }

        // Deserialize data from disk.  If the file is missing, _data stays as a
        // fresh SaveData.  JsonUtility.FromJson does NOT throw on corrupt JSON —
        // it silently returns null or a zeroed struct.  We detect both cases by
        // checking for null and by using version as a sentinel (default = 1;
        // a zeroed struct would produce version = 0).
        public void Load()
        {
            if (!System.IO.File.Exists(_savePath))
            {
                _data = new SaveData();
                return;
            }

            try
            {
                string json   = System.IO.File.ReadAllText(_savePath);
                var    loaded = JsonUtility.FromJson<SaveData>(json);

                // null  → completely invalid JSON (non-object root).
                // version < 1 → JsonUtility silently zeroed the struct (corrupt JSON).
                if (loaded == null || loaded.version < 1)
                {
                    Debug.LogWarning("[SaveSystem] Save file invalid or corrupt — resetting.");
                    _data = new SaveData();
                    Save();
                    return;
                }

                _data = loaded;
                Debug.Log($"[SaveSystem] Loaded from {_savePath}");
            }
            catch (System.Exception e)
            {
                // File.ReadAllText can throw on IO errors, access-denied, etc.
                Debug.LogWarning($"[SaveSystem] Save file unreadable — resetting. Exception: {e.Message}");
                _data = new SaveData();
                Save();
            }
        }

        // Wipe the save file from disk and reset in-memory state to defaults.
        public void DeleteAll()
        {
            if (System.IO.File.Exists(_savePath))
                System.IO.File.Delete(_savePath);

            _data = new SaveData();
            Debug.Log("[SaveSystem] Save data deleted.");
        }
    }
}
