using System;
using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// World prop that broadcasts player proximity to the Forge UI layer.
    /// Uses distance-based detection instead of OnTriggerEnter because the player
    /// uses CharacterController with no Rigidbody, which does not reliably fire
    /// trigger events in Unity 6.
    /// </summary>
    public class WorkbenchProp : MonoBehaviour
    {
        /// <summary>Raised when the player enters proximity. Payload is the player's ForgeController.</summary>
        public event Action<ForgeController> OnPlayerEntered;

        /// <summary>Raised when the player leaves proximity.</summary>
        public event Action OnPlayerExited;

        /// <summary>
        /// Broadcast when any WorkbenchProp becomes active so ForgeUI can self-register
        /// without depending on LevelBuilder or a scene bootstrapper script.
        /// </summary>
        public static event Action<WorkbenchProp> OnSpawned;

        /// <summary>
        /// Broadcast when any WorkbenchProp is disabled so ForgeUI can unregister cleanly.
        /// </summary>
        public static event Action<WorkbenchProp> OnRemoved;

        [SerializeField] private float _interactRadius = 3f;

        private bool            _playerInRange;
        private Transform       _playerTransform;
        private ForgeController _forgeController;
        private float           _startupDelay = 1f; // prevent firing on first frame

        /// <summary>True while the player is within interact radius.</summary>
        public bool PlayerInRange => _playerInRange;

        private void OnEnable()  => OnSpawned?.Invoke(this);
        private void OnDisable() => OnRemoved?.Invoke(this);

        private void Start()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null)
            {
                Debug.LogWarning("[WorkbenchProp] No GameObject with tag 'Player' found.", this);
                return;
            }
            _playerTransform = playerGO.transform;
            _forgeController = playerGO.GetComponent<ForgeController>();

            if (_forgeController == null)
                Debug.LogWarning("[WorkbenchProp] Player found but has no ForgeController.", this);
        }

        private void Update()
        {
            if (_playerTransform == null || _forgeController == null) return;

            if (_startupDelay > 0f)
            {
                _startupDelay -= Time.unscaledDeltaTime; // unscaled so delay works while paused
                return;
            }

            // Do not fire proximity events while game is paused (upgrade/run-end screens showing).
            if (Time.timeScale == 0f) return;

            float dist    = Vector3.Distance(transform.position, _playerTransform.position);
            bool  inRange = dist <= _interactRadius;

            if (inRange && !_playerInRange)
            {
                _playerInRange = true;
                OnPlayerEntered?.Invoke(_forgeController);
            }
            else if (!inRange && _playerInRange)
            {
                _playerInRange = false;
                OnPlayerExited?.Invoke();
            }
        }
    }
}
