using System;
using UnityEngine;
using Boxhead.Core;
using Boxhead.Player;

namespace Boxhead.Systems
{
    public class BoxSystem : MonoBehaviour
    {
        [SerializeField] private BoxData[] availableBoxes;
        [SerializeField] private Renderer boxRenderer;
        [SerializeField] private GameObject[] characterModels;

        public BoxData CurrentBox => (availableBoxes != null && availableBoxes.Length > 0)
            ? availableBoxes[_currentBoxIndex] : null;

        public int CurrentBoxIndex => _currentBoxIndex;

        /// <summary>Applies a box index immediately, bypassing the safe-zone guard. Use only at scene load.</summary>
        public void ForceApplyBox(int index)
        {
            if (availableBoxes == null || index < 0 || index >= availableBoxes.Length) return;
            if (_stats == null) { Debug.LogError("[BoxSystem] PlayerStats not found — ForceApplyBox aborted.", this); return; }
            _currentBoxIndex = index;
            BoxData box = availableBoxes[index];
            _stats.ApplyBox(box);
            ApplyBoxVisuals(box);
            ProgressionSystem.Instance?.UpdateBoxIndex(index);
        }

        public event Action<bool> OnSafeZoneChanged;
        public event Action OnModelChanged;

        private PlayerStats _stats;
        private Material    _boxMaterial;
        private bool _inSafeZone;
        private int _currentBoxIndex;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            if (boxRenderer != null) _boxMaterial = boxRenderer.material;
        }

        private void OnDestroy()
        {
            if (_boxMaterial != null) Destroy(_boxMaterial);
        }

        /// <summary>
        /// Fire OnModelChanged without swapping boxes. Used by RunStartUI to refresh
        /// the CombatController's animator cache after a character model switch at run start.
        /// </summary>
        public void NotifyModelChanged() => OnModelChanged?.Invoke();

        [ContextMenu("Enter Safe Zone")]
        public void EnterSafeZone() { _inSafeZone = true;  OnSafeZoneChanged?.Invoke(true); }

        [ContextMenu("Exit Safe Zone")]
        public void ExitSafeZone()  { _inSafeZone = false; OnSafeZoneChanged?.Invoke(false); }

        [ContextMenu("Switch to Next Box")]
        private void DebugSwitchToNext() => TrySwitchToNext();

        public bool TrySwitchBox(int index)
        {
            if (!_inSafeZone)
            {
                Debug.Log("Can only switch boxes in a safe zone.");
                return false;
            }

            if (index < 0 || index >= availableBoxes.Length) return false;
            if (index == _currentBoxIndex) return false;

            _currentBoxIndex = index;
            BoxData box = availableBoxes[index];
            _stats.ApplyBox(box);
            ApplyBoxVisuals(box);
            ProgressionSystem.Instance?.UpdateBoxIndex(index);
            return true;
        }

        public bool TrySwitchToNext()
        {
            int next = (_currentBoxIndex + 1) % availableBoxes.Length;
            return TrySwitchBox(next);
        }

        private void ApplyBoxVisuals(BoxData box)
        {
            // Swap character model — activate the one matching the current index
            if (characterModels != null)
            {
                for (int i = 0; i < characterModels.Length; i++)
                {
                    if (characterModels[i] != null)
                        characterModels[i].SetActive(i == _currentBoxIndex);
                }
            }

            OnModelChanged?.Invoke();

            if (_boxMaterial == null) return;
            _boxMaterial.color = box.primaryColor;
        }
    }
}
