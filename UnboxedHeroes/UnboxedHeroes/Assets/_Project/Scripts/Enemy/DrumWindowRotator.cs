using UnityEngine;

namespace Boxhead.Enemy
{
    public class DrumWindowRotator : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float slowRPM = 60f;
        [SerializeField] private float fastRPM = 240f;
        [SerializeField] private float rpmLerpSpeed = 20f;

        [Header("Intro Build-up")]
        [SerializeField] private float introBuildDuration = 3f;

        [Header("Stopping")]
        [SerializeField] private float stopRPMRate = 30f;

        [Header("Pendulum (attack mode)")]
        [SerializeField] private float pendulumAmplitude = 12f; // degrees left/right
        [SerializeField] private float pendulumSpeed     = 2.5f; // oscillations per second

        [Header("Parry Window")]
        [SerializeField] private float parryAngleTolerance = 35f;

        private float _targetRPM;
        private float _currentRPM;
        private bool  _stopping;
        private bool  _introBuildActive;
        private float _introBuildElapsed;

        // Accumulated spin angle — survives animator resets by storing the total offset
        // and reapplying it on top of the animator's base pose each LateUpdate.
        private float _accumAngle;

        private bool  _pendulumMode;
        private float _pendulumTime;

        private Transform _player;

        // Returns true when the drum porthole is facing the player within the tolerance arc.
        // IMPORTANT: This object must be authored with its porthole face aligned to local +Z (transform.forward).
        public bool IsParryWindowOpen
        {
            get
            {
                if (_player == null) return false;

                Vector3 forward = transform.forward;
                forward.y = 0f;

                Vector3 toPlayer = _player.position - transform.position;
                toPlayer.y = 0f;

                if (toPlayer.sqrMagnitude < 0.001f) return false;

                return Vector3.Angle(forward, toPlayer) <= parryAngleTolerance;
            }
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;

            _targetRPM  = slowRPM;
            _currentRPM = 0f;
            _accumAngle = 0f;
        }

        // LateUpdate runs after the Animator applies bone transforms each frame,
        // so this rotation wins over any Humanoid head pose the animator writes.
        private void LateUpdate()
        {
            if (_pendulumMode)
            {
                // Swing left-to-right: sine wave oscillation on Y axis.
                // We apply the angle as an ABSOLUTE offset on top of the animator's base pose
                // this frame — matching the _accumAngle pattern used in spin mode. Compounding
                // localRotation * Euler each frame would accumulate the sine result incorrectly.
                _pendulumTime += Time.deltaTime;
                float angle = Mathf.Sin(_pendulumTime * pendulumSpeed * Mathf.PI * 2f) * pendulumAmplitude;
                transform.localRotation = transform.localRotation * Quaternion.Euler(0f, angle, 0f);
                return;
            }

            if (_stopping)
            {
                _currentRPM = Mathf.MoveTowards(_currentRPM, 0f, stopRPMRate * Time.deltaTime);
            }
            else if (_introBuildActive)
            {
                // Ease from 0 → fastRPM over introBuildDuration using smooth curve.
                _introBuildElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(_introBuildElapsed / introBuildDuration);
                _currentRPM = Mathf.Lerp(0f, fastRPM, t * t); // quadratic ease-in = starts slow
                if (t >= 1f) { _introBuildActive = false; _targetRPM = fastRPM; }
            }
            else
            {
                _currentRPM = Mathf.MoveTowards(_currentRPM, _targetRPM, rpmLerpSpeed * Time.deltaTime);
            }

            // Accumulate the spin angle independently of what the Animator wrote this frame.
            // transform.Rotate() only adds a single-frame delta — the Animator resets the
            // bone rotation every AnimationUpdate, so Rotate() in LateUpdate gives a constant
            // offset rather than a cumulative spin. Tracking _accumAngle fixes this.
            _accumAngle += -_currentRPM * 6f * Time.deltaTime; // negative = left (Y axis)

            // Apply accumulated Y-axis spin ON TOP of the Animator's base pose this frame.
            transform.localRotation = transform.localRotation * Quaternion.Euler(0f, _accumAngle, 0f);
        }

        /// <summary>Switch to pendulum swing mode (used during attacks).</summary>
        public void BeginPendulum()
        {
            _pendulumMode = true;
            _pendulumTime = 0f;
        }

        /// <summary>Return to continuous spin mode (called when attack ends).</summary>
        public void EndPendulum()
        {
            _pendulumMode = false;
            // Reset accumAngle so spin resumes from a clean position, not a stale value.
            _accumAngle = 0f;
        }

        /// <summary>Reset accumAngle to zero so the drum faces its rest position (porthole forward).
        /// Call before StartIntroBuildUp() to guarantee correct facing at intro start.</summary>
        public void ResetToForward()
        {
            _stopping          = false;
            _introBuildActive  = false;
            _pendulumMode      = false;
            _accumAngle        = 0f;
            _currentRPM        = 0f;
            _targetRPM         = 0f;  // prevent drift toward slowRPM during walk-out
        }

        /// <summary>Starts the cinematic intro build-up: 0 → fastRPM over introBuildDuration.</summary>
        public void StartIntroBuildUp()
        {
            _stopping          = false;
            _introBuildActive  = true;
            _introBuildElapsed = 0f;
            _currentRPM        = 0f;
        }

        /// <summary>Return drum to slow idle speed — call after intro spin-up completes.</summary>
        public void SetSlowPhase()
        {
            _stopping         = false;
            _introBuildActive = false;
            _pendulumMode     = false;
            _targetRPM        = slowRPM;
        }

        public void SetFastPhase()
        {
            _stopping         = false;
            _introBuildActive = false;
            _pendulumMode     = false;
            _targetRPM        = fastRPM;
            _currentRPM       = fastRPM;
        }

        public void BeginStopDrum()
        {
            _pendulumMode = false;
            _stopping     = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (transform.parent == null || transform.GetComponentInParent<SpinCycleAI>() == null)
                Debug.LogWarning("[DrumWindowRotator] Should be a child of a SpinCycleAI GameObject.", this);
        }
#endif
    }
}
