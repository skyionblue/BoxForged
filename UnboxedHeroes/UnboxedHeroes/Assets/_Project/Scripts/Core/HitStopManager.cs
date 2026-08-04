using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

namespace Boxhead.Core
{
    /// <summary>
    /// Pauses both animators briefly on a hit (no timeScale — projectiles and UI unaffected)
    /// and fires a Cinemachine impulse for camera shake.
    /// Wire _impulseSource in the Inspector to the CinemachineImpulseSource on CM_PlayerFollow.
    /// </summary>
    public class HitStopManager : MonoBehaviour
    {
        public static HitStopManager Instance { get; private set; }

        [Header("Impulse")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Durations")]
        [SerializeField] private float _lightHitDuration = 0.05f;   // ~3 frames at 60fps
        [SerializeField] private float _heavyHitDuration = 0.083f;  // ~5 frames at 60fps

        [Header("Impulse Magnitudes")]
        [SerializeField] private float _lightImpulse = 0.15f;
        [SerializeField] private float _heavyImpulse = 0.40f;

        private Animator         _playerAnimator;
        private Coroutine        _activeRoutine;

        // Cached — never new'd inside coroutines
        private WaitForSecondsRealtime _waitLight;
        private WaitForSecondsRealtime _waitHeavy;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _waitLight = new WaitForSecondsRealtime(_lightHitDuration);
            _waitHeavy = new WaitForSecondsRealtime(_heavyHitDuration);
        }

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                _playerAnimator = player.GetComponentInChildren<Animator>();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Light hit stop + small shake. Call when a normal attack lands.</summary>
        public void TriggerHitStop(Animator hitAnimator)
        {
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(HitStopRoutine(_waitLight, hitAnimator, _lightImpulse));
        }

        /// <summary>Heavy hit stop + large shake. Call when a special ability lands.</summary>
        public void TriggerHeavyHitStop(Animator hitAnimator)
        {
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(HitStopRoutine(_waitHeavy, hitAnimator, _heavyImpulse));
        }

        // ── Internals ─────────────────────────────────────────────────────────

        private IEnumerator HitStopRoutine(WaitForSecondsRealtime wait, Animator hitAnimator, float impulseMag)
        {
            // Save speeds so we restore exactly what CombatController last set
            float playerSpeed = _playerAnimator != null ? _playerAnimator.speed : 1f;
            float enemySpeed  = hitAnimator      != null ? hitAnimator.speed      : 1f;

            if (_playerAnimator != null) _playerAnimator.speed = 0f;
            if (hitAnimator      != null) hitAnimator.speed      = 0f;

            _impulseSource?.GenerateImpulse(impulseMag);

            yield return wait;

            if (_playerAnimator != null) _playerAnimator.speed = playerSpeed;
            if (hitAnimator      != null) hitAnimator.speed      = enemySpeed;

            _activeRoutine = null;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
