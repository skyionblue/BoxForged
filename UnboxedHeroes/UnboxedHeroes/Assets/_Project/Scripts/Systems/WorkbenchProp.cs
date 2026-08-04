using System;
using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// World prop that broadcasts player proximity to the Forge UI layer.
    /// The prop does not open UI directly — it raises events so the UI
    /// subscribes independently, keeping presentation decoupled from world state.
    /// </summary>
    public class WorkbenchProp : MonoBehaviour
    {
        /// <summary>Raised when a player enters the trigger. Payload is the player's ForgeController.</summary>
        public event Action<ForgeController> OnPlayerEntered;

        /// <summary>Raised when the player leaves the trigger.</summary>
        public event Action OnPlayerExited;

        private bool _playerInRange;

        /// <summary>True while a player is inside the workbench trigger volume.</summary>
        public bool PlayerInRange => _playerInRange;

        // Cached lazily on first trigger enter: the prop is a world object and the player
        // is not in its hierarchy at startup, so Awake resolution would always return null.
        private ForgeController _forgeController;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (_forgeController == null)
                _forgeController = other.GetComponentInParent<ForgeController>();

            if (_forgeController == null)
            {
                Debug.LogWarning("[WorkbenchProp] Player entered but has no ForgeController.", this);
                return;
            }

            _playerInRange = true;
            OnPlayerEntered?.Invoke(_forgeController);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInRange = false;
            OnPlayerExited?.Invoke();
        }
    }
}
