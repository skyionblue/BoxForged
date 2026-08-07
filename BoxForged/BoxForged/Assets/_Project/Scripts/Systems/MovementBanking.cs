using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Procedural "movement banking": leans a character forward when moving forward and
    /// rolls (banks) into turns, purely from the ROOT transform's own position/rotation
    /// deltas each frame — so it works regardless of the mover (NavMeshAgent, CharacterController,
    /// Rigidbody, or scripted). The tilt is applied to a dedicated child pivot, never the root,
    /// so the root stays upright for colliders and logic.
    /// Runs in LateUpdate so it composes on TOP of the Animator's bone writes rather than being
    /// overwritten by them (the pivot is a parent of the rig, so this is a clean parent-level tilt).
    /// </summary>
    public class MovementBanking : MonoBehaviour
    {
        [Header("Pivot")]
        [Tooltip("Child transform to tilt (Mesh + armature live under it). Never the root.")]
        [SerializeField] private Transform _visualPivot;

        [Header("Lean (forward pitch)")]
        [Tooltip("Max forward pitch in degrees at/above _speedForMaxLean.")]
        [SerializeField] private float _maxLeanAngle = 12f;
        [Tooltip("Forward speed (units/sec) at which lean reaches its max angle.")]
        [SerializeField] private float _speedForMaxLean = 4f;

        [Header("Bank (roll into turns)")]
        [Tooltip("Max roll in degrees at/above _turnRateForMaxBank.")]
        [SerializeField] private float _maxBankAngle = 15f;
        [Tooltip("Yaw rate (deg/sec) at which bank reaches its max angle.")]
        [SerializeField] private float _turnRateForMaxBank = 180f;
        [Tooltip("Flip the bank direction if the visual leans the wrong way into turns.")]
        [SerializeField] private bool _invertBank = false;

        [Header("Smoothing")]
        [Tooltip("Exponential smoothing responsiveness (higher = snappier). " +
                 "Blend factor per frame is (1 - exp(-k*dt)), which is frame-rate independent.")]
        [SerializeField] private float _smoothing = 10f;

        [Header("General")]
        [Tooltip("Master toggle. When off, the pivot is held at its rest pose.")]
        [SerializeField] private bool _tiltEnabled = true;

        /// <summary>
        /// Runtime toggle for the banking effect. Boss logic can set this false to suppress
        /// banking during staggers/knockdowns, then restore it afterward.
        /// </summary>
        public bool TiltEnabled { get => _tiltEnabled; set => _tiltEnabled = value; }

        // Captured in Awake so the lean/bank compose on top of the pivot's authored orientation.
        private Quaternion _baseLocalRot;

        // Frame-to-frame sampling state for deriving motion from the root transform.
        private Vector3 _lastPosition;
        private float   _lastYaw;
        private bool    _hasSample;   // guards against first-frame garbage deltas

        // Current smoothed tilt angles (degrees). Smoothed toward per-frame targets.
        private float _currentPitch;
        private float _currentRoll;

        private void Awake()
        {
            // Fail loud but safe: with no pivot there is nothing to tilt, so disable the
            // component (LateUpdate keeps its own null guard as a defensive backup).
            if (_visualPivot == null)
            {
                Debug.LogWarning("[MovementBanking] _visualPivot is not assigned — banking disabled.", this);
                enabled = false;
                return;
            }

            _baseLocalRot = _visualPivot.localRotation;
            CaptureSampleState();
        }

        // Re-sample the root's pose on enable so re-enabling at runtime (after a boss toggle,
        // teleport, etc.) does not derive a stale-delta spike from a long-ago sample.
        private void OnEnable()
        {
            if (_visualPivot != null)
                CaptureSampleState();
        }

        // Reset frame-to-frame sampling so the next LateUpdate starts from a clean baseline.
        private void CaptureSampleState()
        {
            _lastPosition = transform.position;
            _lastYaw      = transform.eulerAngles.y;
            _hasSample    = false;
        }

        // LateUpdate: after the Animator writes bone transforms this frame, apply the pivot tilt
        // on top so it is not overwritten. The pivot parents the rig, so this is a stable
        // parent-level rotation rather than a per-bone override.
        private void LateUpdate()
        {
            if (_visualPivot == null) return;

            float dt = Time.deltaTime;

            if (!_tiltEnabled)
            {
                // Ease back to rest so toggling off does not snap.
                float relax = 1f - Mathf.Exp(-_smoothing * Mathf.Max(dt, 0f));
                _currentPitch = Mathf.Lerp(_currentPitch, 0f, relax);
                _currentRoll  = Mathf.Lerp(_currentRoll, 0f, relax);
                ApplyTilt();
                // Keep sampling state fresh so re-enabling doesn't produce a huge delta.
                _lastPosition = transform.position;
                _lastYaw      = transform.eulerAngles.y;
                return;
            }

            // Skip sampling when paused or on the very first frame — avoids divide-by-zero and
            // NaN from a zero dt, and avoids a garbage delta before we have a prior sample.
            if (dt <= 0f)
                return;

            float targetPitch = 0f;
            float targetRoll  = 0f;

            if (_hasSample)
            {
                // --- Forward speed: horizontal position delta projected onto root forward ---
                Vector3 posDelta = transform.position - _lastPosition;
                posDelta.y = 0f;
                Vector3 forward = transform.forward;
                forward.y = 0f;
                forward.Normalize();
                float forwardSpeed = Vector3.Dot(posDelta, forward) / dt;

                // Lean forward proportional to forward speed (only forward motion leans in).
                float leanT   = Mathf.Clamp01(forwardSpeed / Mathf.Max(_speedForMaxLean, 0.0001f));
                targetPitch   = leanT * _maxLeanAngle;

                // --- Yaw angular velocity: signed change in yaw per second (wrap-safe) ---
                float currentYaw = transform.eulerAngles.y;
                float yawDelta   = Mathf.DeltaAngle(_lastYaw, currentYaw); // handles 360 wrap-around
                float yawRate    = yawDelta / dt;

                // Bank into the turn. Turning right (+yaw) rolls one way, left the other.
                // Roll around local Z is negated so a right turn banks the top of the body
                // toward the turn centre (feels natural); _invertBank flips it if authored differently.
                float bankT = Mathf.Clamp(yawRate / Mathf.Max(_turnRateForMaxBank, 0.0001f), -1f, 1f);
                float bankSign = _invertBank ? 1f : -1f;
                targetRoll  = bankT * _maxBankAngle * bankSign;
            }

            // Exponential smoothing: blend = 1 - exp(-k*dt). Frame-rate independent (unlike a
            // raw Lerp with a constant t) and alloc-free — no SmoothDamp velocity refs needed.
            float blend = 1f - Mathf.Exp(-_smoothing * dt);
            _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, blend);
            _currentRoll  = Mathf.Lerp(_currentRoll, targetRoll, blend);

            ApplyTilt();

            // Store this frame's sample for next frame's delta.
            _lastPosition = transform.position;
            _lastYaw      = transform.eulerAngles.y;
            _hasSample    = true;
        }

        // Compose the tilt on top of the pivot's authored rest orientation.
        private void ApplyTilt()
        {
            _visualPivot.localRotation = _baseLocalRot * Quaternion.Euler(_currentPitch, 0f, _currentRoll);
        }

        /// <summary>Snap the pivot back to its captured rest orientation and clear tilt state.</summary>
        public void ResetToRest()
        {
            _currentPitch = 0f;
            _currentRoll  = 0f;
            if (_visualPivot != null)
                _visualPivot.localRotation = _baseLocalRot;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_visualPivot == null)
                Debug.LogWarning("[MovementBanking] _visualPivot is not assigned — assign the child transform that holds the mesh/armature.", this);
            else if (_visualPivot == transform)
                Debug.LogWarning("[MovementBanking] _visualPivot must be a CHILD, not the root — tilting the root will rotate colliders/logic.", this);
        }
#endif
    }
}
