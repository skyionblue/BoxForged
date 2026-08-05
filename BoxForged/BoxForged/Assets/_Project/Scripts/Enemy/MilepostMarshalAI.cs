using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Rigidbody))]
    public class MilepostMarshalAI : MonoBehaviour
    {
        private enum MarshalState
        {
            Patrol,
            Alert,
            WindUp,
            Slam,
            Retreat,
            SweepWindUp,
            Sweep,
            Stunned,
            Dead
        }

        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Patrol")]
        [SerializeField] private Transform[] _waypoints;
        [SerializeField] private float _patrolSpeed = 2f;

        [Header("Alert")]
        [SerializeField] private float _alertRange      = 5f;
        [SerializeField] private float _alertSlowRange  = 8f;
        [SerializeField] private float _alertDuration   = 1.0f;
        [SerializeField] private float _alertSlowSpeed  = 1.5f;

        [Header("WindUp")]
        [SerializeField] private float      _windUpDuration  = 1.2f;
        [SerializeField] private GameObject _groundIndicator;

        [Header("Slam")]
        [SerializeField] private Transform _slamPoint;
        [SerializeField] private float     _slamRadius       = 1.5f;
        [SerializeField] private int       _slamDamage       = 20;
        [SerializeField] private float     _slamSwingDuration = 0.3f;

        [Header("Retreat")]
        [SerializeField] private float _retreatDistance = 3f;
        [SerializeField] private float _retreatDuration = 1.5f;

        [Header("Sweep")]
        [SerializeField] private float _sweepRadius         = 2.5f;
        [SerializeField] private int   _sweepDamage         = 15;
        [SerializeField] private float _sweepKnockbackForce = 8f;
        [SerializeField] private float _sweepTriggerRange   = 3f;
        [SerializeField] private float _sweepWindUpDuration = 0.5f;

        [Header("Stunned")]
        [SerializeField] private float _stunDuration          = 2.0f;
        [SerializeField] private float _counterKnockbackForce = 10f;

        [Header("Arms")]
        [SerializeField] private Transform _leftArm;
        [SerializeField] private Transform _rightArm;
        [SerializeField] private float     _rightArmRaisedAngle = 60f;
        [SerializeField] private float     _leftArmRaisedAngle  = 60f;
        [SerializeField] private float     _sweepArmRaisedAngle = 80f;

        [Header("Visuals")]
        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private Material _stunnedMaterial;
        [SerializeField] private Material _deadMaterial;
        [SerializeField] private float    _despawnDelay = 3f;

        [Header("Layers")]
        // Must be set to the Player layer in the Inspector — default ~0 hits every layer.
        [SerializeField] private LayerMask _playerLayer = 0;

        // ── State ─────────────────────────────────────────────────────────────

        private MarshalState _state = MarshalState.Patrol;

        // ── Runtime references ────────────────────────────────────────────────

        private EnemyStats       _stats;
        private NavMeshAgent     _agent;
        private Rigidbody        _rb;
        private Animator         _animator;
        private Transform        _player;
        private CombatController _playerCombat;
        private Rigidbody        _playerRigidbody;
        private Coroutine        _stateRoutine;

        // ── Cached data ───────────────────────────────────────────────────────

        private Material _normalMaterial;
        private Material _stunnedMaterialInstance;
        private Material _deadMaterialInstance;

        private int _waypointIndex;

        // Arm rotation targets — 0 = identity, 1 = fully raised.
        // Updated by UpdateArmRotations() each frame, driven by state.
        private float _rightArmBlend;
        private float _leftArmBlend;

        private Quaternion _rightArmIdentity;
        private Quaternion _leftArmIdentity;
        private Quaternion _rightArmRaised;
        private Quaternion _leftArmRaised;
        private Quaternion _sweepArmRaised; // used for both arms during Retreat/SweepWindUp

        // Arm blend speed — lerp scale per second.
        private const float ArmBlendSpeed = 3f;

        // Pre-allocated overlap buffer — never reallocated during gameplay.
        private readonly Collider[] _hitBuffer = new Collider[4];

        // Path update throttle.
        private float _pathUpdateTimer;
        private const float PathUpdateInterval = 0.25f;

        // Cached WaitForSeconds — never new'd inside coroutines.
        private WaitForSeconds _waitAlert;
        private WaitForSeconds _waitWindUp;
        private WaitForSeconds _waitSlamSwing;
        private WaitForSeconds _waitRetreat;
        private WaitForSeconds _waitSweepWindUp;
        private WaitForSeconds _waitStun;
        private WaitForSeconds _waitDie;

        // Sweep hit flag — ensures one hit per sweep even if overlap fires multiple frames.
        private bool _sweepHitDelivered;

        private static readonly int AnimAttack  = Animator.StringToHash("AttackTrigger");
        private static readonly int AnimStagger = Animator.StringToHash("StaggerTrigger");
        private static readonly int AnimIsDead  = Animator.StringToHash("IsDead");

        // Squared range caches — avoids Mathf.Sqrt per frame.
        private float _alertRangeSq;
        private float _alertSlowRangeSq;
        private float _sweepTriggerRangeSq;

        // cos(60°) = 0.5 — half-angle for 120° sweep arc.
        private const float SweepCosHalfAngle = 0.5f;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _stats    = GetComponent<EnemyStats>();
            _agent    = GetComponent<NavMeshAgent>();
            _rb       = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();

            // Rigidbody on the Marshal is kinematic for NavMesh-driven movement; it is
            // never used for Marshal physics — only the player's Rigidbody receives impulse.
            _rb.isKinematic = true;

            _agent.speed            = _patrolSpeed;
            _agent.stoppingDistance = 0.1f;
            _agent.updateRotation   = true;
            _agent.isStopped        = false;

            if (_bodyRenderer != null)
            {
                _normalMaterial = _bodyRenderer.sharedMaterial;

                if (_stunnedMaterial != null)
                    _stunnedMaterialInstance = new Material(_stunnedMaterial);
                if (_deadMaterial != null)
                    _deadMaterialInstance = new Material(_deadMaterial);
            }

            _alertRangeSq        = _alertRange * _alertRange;
            _alertSlowRangeSq    = _alertSlowRange * _alertSlowRange;
            _sweepTriggerRangeSq = _sweepTriggerRange * _sweepTriggerRange;

            // Snapshot arm rest poses so we can lerp back to them cleanly.
            if (_rightArm != null)
            {
                _rightArmIdentity = _rightArm.localRotation;
                _rightArmRaised   = _rightArm.localRotation * Quaternion.Euler(-_rightArmRaisedAngle, 0f, 0f);
            }

            if (_leftArm != null)
            {
                _leftArmIdentity  = _leftArm.localRotation;
                _leftArmRaised    = _leftArm.localRotation * Quaternion.Euler(-_leftArmRaisedAngle, 0f, 0f);
                _sweepArmRaised   = _leftArm.localRotation * Quaternion.Euler(-_sweepArmRaisedAngle, 0f, 0f);
            }

            // Disable ground indicator at startup.
            if (_groundIndicator != null)
                _groundIndicator.SetActive(false);

            // Pre-cache all WaitForSeconds — called zero times per frame during gameplay.
            _waitAlert       = new WaitForSeconds(_alertDuration);
            _waitWindUp      = new WaitForSeconds(_windUpDuration);
            _waitSlamSwing   = new WaitForSeconds(_slamSwingDuration);
            _waitRetreat     = new WaitForSeconds(_retreatDuration);
            _waitSweepWindUp = new WaitForSeconds(_sweepWindUpDuration);
            _waitStun        = new WaitForSeconds(_stunDuration);
            _waitDie         = new WaitForSeconds(_despawnDelay);
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _player       = playerObj.transform;
                _playerCombat = playerObj.GetComponent<CombatController>();
                playerObj.TryGetComponent(out _playerRigidbody);
            }
            else
            {
                Debug.LogWarning("[MilepostMarshalAI] No GameObject tagged 'Player' found.", this);
            }

            _stats.OnDeath += HandleDeath;

            if (_playerCombat != null)
                _playerCombat.OnCounterStrike += OnCounterStrikeLanded;

            // Seed first waypoint.
            if (_waypoints != null && _waypoints.Length > 0)
                SetPatrolDestination();
        }

        private void OnDestroy()
        {
            if (_stats != null)
                _stats.OnDeath -= HandleDeath;

            if (_playerCombat != null)
                _playerCombat.OnCounterStrike -= OnCounterStrikeLanded;

            StopStateRoutine();

            // Restore original shared material so the prefab asset is not dirtied.
            if (_bodyRenderer != null && _normalMaterial != null)
                _bodyRenderer.sharedMaterial = _normalMaterial;

            if (_stunnedMaterialInstance != null) Destroy(_stunnedMaterialInstance);
            if (_deadMaterialInstance    != null) Destroy(_deadMaterialInstance);
        }

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (_state == MarshalState.Dead) return;
            if (_player == null) return;

            switch (_state)
            {
                case MarshalState.Patrol:
                    UpdatePatrol();
                    break;
                case MarshalState.Alert:
                    UpdateAlert();
                    break;
                case MarshalState.Retreat:
                    UpdateRetreat();
                    break;
                // WindUp, Slam, SweepWindUp, Sweep, Stunned are fully coroutine-driven.
            }
        }

        // LateUpdate runs after the Animator applies transforms, so arm overrides and
        // the Speed parameter are guaranteed to win each frame.
        private void LateUpdate()
        {
            if (_state == MarshalState.Dead) return;

            UpdateArmRotations();

            if (_animator != null)
            {
                // Normalize against alert slow speed so patrol maps to ~0.5 (walk blend)
                // and alert maps to ~0.75. Using patrolSpeed as denominator always gave 1.0
                // during patrol → Run clip played at full speed → visual foot sliding.
                float normalizedSpeed = Mathf.Clamp01(_agent.velocity.magnitude / (_alertSlowSpeed * 2f));
                _animator.SetFloat("Speed", normalizedSpeed, 0.1f, Time.deltaTime);
            }
        }

        // ── State update methods ──────────────────────────────────────────────

        private void UpdatePatrol()
        {
            if ((transform.position - _player.position).sqrMagnitude <= _alertRangeSq)
            {
                EnterAlert();
                return;
            }

            // Advance waypoint when close enough to the current target.
            if (_waypoints != null && _waypoints.Length > 0 && _agent.isOnNavMesh)
            {
                if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                {
                    _waypointIndex = (_waypointIndex + 1) % _waypoints.Length;
                    SetPatrolDestination();
                }
            }
        }

        private void UpdateAlert()
        {
            float sqDist = (transform.position - _player.position).sqrMagnitude;

            // Slow approach while in alert range.
            if (sqDist > _alertRangeSq && sqDist <= _alertSlowRangeSq)
            {
                _agent.isStopped = false;
                _agent.speed = _alertSlowSpeed;

                _pathUpdateTimer -= Time.deltaTime;
                if (_pathUpdateTimer <= 0f)
                {
                    _pathUpdateTimer = PathUpdateInterval;
                    if (_agent.isOnNavMesh)
                        _agent.SetDestination(_player.position);
                }
            }
            else
            {
                // Player too close or too far — stop and face.
                _agent.isStopped = true;
            }

            FacePlayer();
        }

        private void UpdateRetreat()
        {
            // If the player closes in during retreat, trigger sweep.
            if ((transform.position - _player.position).sqrMagnitude <= _sweepTriggerRangeSq)
            {
                EnterSweepWindUp();
                return;
            }

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                // Retreat complete — arms were already raising; lower them and return to patrol.
                EnterPatrol();
            }
        }

        // ── Arm rotation ──────────────────────────────────────────────────────

        // Drives smooth arm blend toward targets set by each state.
        // right/left blend 0 = identity, 1 = raised, with SweepArmRaised target used when > 1 sentinel.
        // Using a simple two-float lerp is zero-allocation and avoids coroutine overhead.
        private const float SweepBlendSentinel = 2f;

        private void UpdateArmRotations()
        {
            float t = ArmBlendSpeed * Time.deltaTime;

            if (_rightArm != null)
            {
                Quaternion target = _rightArmBlend >= 1f
                    ? _rightArmRaised
                    : Quaternion.Slerp(_rightArmIdentity, _rightArmRaised, _rightArmBlend);
                _rightArm.localRotation = Quaternion.Slerp(_rightArm.localRotation, target, t);
            }

            if (_leftArm != null)
            {
                Quaternion raisedTarget = _leftArmBlend >= SweepBlendSentinel
                    ? _sweepArmRaised
                    : Quaternion.Slerp(_leftArmIdentity, _leftArmRaised, Mathf.Clamp01(_leftArmBlend));
                _leftArm.localRotation = Quaternion.Slerp(_leftArm.localRotation, raisedTarget, t);
            }
        }

        // ── State transitions ─────────────────────────────────────────────────

        private void EnterPatrol()
        {
            StopStateRoutine();
            _state                = MarshalState.Patrol;
            _agent.speed          = _patrolSpeed;
            _agent.isStopped      = false;
            _agent.updateRotation = true;
            _rightArmBlend        = 0f;
            _leftArmBlend         = 0f;
            SetMaterial(_normalMaterial);
            if (_waypoints != null && _waypoints.Length > 0)
                SetPatrolDestination();
        }

        private void EnterAlert()
        {
            StopStateRoutine();
            _state                = MarshalState.Alert;
            _agent.isStopped      = true;
            _agent.updateRotation = false; // FacePlayer takes over manual rotation
            _rightArmBlend        = 1f;
            _leftArmBlend         = 0f;
            _stateRoutine         = StartCoroutine(AlertRoutine());
        }

        private void EnterWindUp()
        {
            StopStateRoutine();
            _state           = MarshalState.WindUp;
            _agent.isStopped = true;
            _leftArmBlend    = 1f; // left arm raises during wind-up
            if (_groundIndicator != null)
                _groundIndicator.SetActive(true);
            _stateRoutine = StartCoroutine(WindUpRoutine());
        }

        private void EnterSlam()
        {
            StopStateRoutine();
            _state           = MarshalState.Slam;
            _agent.isStopped = true;
            _animator?.SetTrigger(AnimAttack);
            _stateRoutine    = StartCoroutine(SlamRoutine());
        }

        private void EnterRetreat()
        {
            StopStateRoutine();
            _state           = MarshalState.Retreat;
            _agent.speed     = _alertSlowSpeed;
            _agent.isStopped = false;

            // Both arms raise to sweep-arm angle as a telegraph.
            _rightArmBlend = SweepBlendSentinel;
            _leftArmBlend  = SweepBlendSentinel;

            // Back away from the player.
            Vector3 retreatDir = (transform.position - _player.position).normalized;
            retreatDir.y = 0f;
            Vector3 retreatPos = transform.position + retreatDir * _retreatDistance;

            if (NavMesh.SamplePosition(retreatPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                retreatPos = hit.position;

            if (_agent.isOnNavMesh)
                _agent.SetDestination(retreatPos);

            _stateRoutine = StartCoroutine(RetreatTimeoutRoutine());
        }

        private void EnterSweepWindUp()
        {
            StopStateRoutine();
            _state           = MarshalState.SweepWindUp;
            _agent.isStopped = true;
            _rightArmBlend   = SweepBlendSentinel;
            _leftArmBlend    = SweepBlendSentinel;
            _stateRoutine    = StartCoroutine(SweepWindUpRoutine());
        }

        private void EnterSweep()
        {
            StopStateRoutine();
            _state              = MarshalState.Sweep;
            _sweepHitDelivered  = false;
            _animator?.SetTrigger(AnimAttack);
            _stateRoutine       = StartCoroutine(SweepRoutine());
        }

        private void EnterStunned(Vector3 knockbackDir)
        {
            StopStateRoutine();
            _state           = MarshalState.Stunned;
            _agent.isStopped = true;
            _rightArmBlend   = 0f;
            _leftArmBlend    = 0f;
            _animator?.SetTrigger(AnimStagger);
            Boxhead.Core.AudioManager.Instance?.Play(Boxhead.Core.SoundEvent.EnemyHit);
            SetMaterial(_stunnedMaterialInstance);

            if (_rb != null && knockbackDir.sqrMagnitude > 0.001f)
            {
                _rb.isKinematic  = false;
                _rb.AddForce(knockbackDir.normalized * _counterKnockbackForce, ForceMode.Impulse);
            }

            _stateRoutine = StartCoroutine(StunnedRoutine());
        }

        private void EnterDead()
        {
            StopStateRoutine();
            _state           = MarshalState.Dead;
            _agent.isStopped = true;
            _agent.enabled   = false;
            _rightArmBlend   = 0f;
            _leftArmBlend    = 0f;
            _animator?.SetBool(AnimIsDead, true);
            if (_groundIndicator != null)
                _groundIndicator.SetActive(false);
            SetMaterial(_deadMaterialInstance);
            _stateRoutine = StartCoroutine(DieRoutine());
        }

        // ── Coroutines ────────────────────────────────────────────────────────

        private IEnumerator AlertRoutine()
        {
            // Wait in alert state; UpdateAlert handles approach logic during this time.
            yield return _waitAlert;
            if (_state == MarshalState.Alert)
                EnterWindUp();
        }

        private IEnumerator WindUpRoutine()
        {
            yield return _waitWindUp;
            if (_state == MarshalState.WindUp)
                EnterSlam();
        }

        private IEnumerator SlamRoutine()
        {
            // Arm swings down: drive left arm blend back to identity over the swing duration.
            float elapsed = 0f;
            float startBlend = _leftArmBlend;
            while (elapsed < _slamSwingDuration)
            {
                elapsed += Time.deltaTime;
                _leftArmBlend = Mathf.Lerp(startBlend, 0f, elapsed / _slamSwingDuration);
                yield return null;
            }
            _leftArmBlend = 0f;

            if (_groundIndicator != null)
                _groundIndicator.SetActive(false);

            // Resolve slam hit.
            AttackResult result = AttackResult.Hit;
            Transform origin = _slamPoint != null ? _slamPoint : transform;
            int count = Physics.OverlapSphereNonAlloc(origin.position, _slamRadius, _hitBuffer, _playerLayer);
            for (int i = 0; i < count; i++)
            {
                if (!_hitBuffer[i].CompareTag("Player")) continue;
                if (_playerCombat != null)
                    result = _playerCombat.TryReceiveAttack(_slamDamage, parryable: true, attacker: gameObject);
                break;
            }

            if (result == AttackResult.Parried)
            {
                // Parry immediately staggers the Marshal — use incoming direction as knockback.
                Vector3 knockDir = _player != null
                    ? (transform.position - _player.position).normalized
                    : -transform.forward;
                EnterStunned(knockDir);
                yield break;
            }

            EnterRetreat();
        }

        private IEnumerator RetreatTimeoutRoutine()
        {
            // Timeout fallback — UpdateRetreat handles early sweep trigger and arrival.
            yield return _waitRetreat;
            if (_state == MarshalState.Retreat)
                EnterPatrol();
        }

        private IEnumerator SweepWindUpRoutine()
        {
            yield return _waitSweepWindUp;
            if (_state == MarshalState.SweepWindUp)
                EnterSweep();
        }

        private IEnumerator SweepRoutine()
        {
            // Apply sweep: 120° front arc, OverlapSphere at Marshal's position.
            int count = Physics.OverlapSphereNonAlloc(transform.position, _sweepRadius, _hitBuffer, _playerLayer);
            for (int i = 0; i < count; i++)
            {
                if (!_hitBuffer[i].CompareTag("Player")) continue;
                if (_sweepHitDelivered) break;

                Vector3 toPlayer = _hitBuffer[i].transform.position - transform.position;
                toPlayer.y = 0f;

                // Dot product check: player must be within the 120° forward arc.
                if (toPlayer.sqrMagnitude > 0.001f &&
                    Vector3.Dot(toPlayer.normalized, transform.forward) > SweepCosHalfAngle)
                {
                    _sweepHitDelivered = true;

                    if (_playerCombat != null)
                        _playerCombat.TryReceiveAttack(_sweepDamage, parryable: false, attacker: gameObject);

                    // Knockback: push the player away from the Marshal.
                    if (_playerRigidbody != null)
                    {
                        Vector3 force = toPlayer.normalized * _sweepKnockbackForce;
                        _playerRigidbody.AddForce(force, ForceMode.Impulse);
                    }
                }
                break;
            }

            // Lower arms and return to patrol.
            _rightArmBlend = 0f;
            _leftArmBlend  = 0f;
            yield return null;

            if (_state == MarshalState.Sweep)
                EnterPatrol();
        }

        private IEnumerator StunnedRoutine()
        {
            yield return _waitStun;

            // Re-enable kinematic nav after impulse settles.
            if (_rb != null)
            {
                _rb.linearVelocity  = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic     = true;
            }

            if (_state == MarshalState.Stunned)
            {
                SetMaterial(_normalMaterial);
                EnterPatrol();
            }
        }

        private IEnumerator DieRoutine()
        {
            yield return _waitDie;
            Destroy(gameObject);
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void OnCounterStrikeLanded(GameObject target)
        {
            // Only stagger this specific Marshal — not all enemies in the scene.
            if (target != gameObject) return;
            if (_state == MarshalState.Dead) return;

            // Knockback direction: away from the player (player is counter-striking toward us).
            Vector3 knockDir = _player != null
                ? (transform.position - _player.position).normalized
                : -transform.forward;
            knockDir.y = 0f;

            EnterStunned(knockDir);
        }

        private void HandleDeath()
        {
            if (_state == MarshalState.Dead) return;
            EnterDead();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetPatrolDestination()
        {
            if (_waypoints == null || _waypoints.Length == 0) return;
            Transform wp = _waypoints[_waypointIndex];
            if (wp != null && _agent.isOnNavMesh)
                _agent.SetDestination(wp.position);
        }

        private void FacePlayer()
        {
            if (_player == null) return;
            Vector3 dir = _player.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        private void StopStateRoutine()
        {
            if (_stateRoutine != null)
            {
                StopCoroutine(_stateRoutine);
                _stateRoutine = null;
            }
        }

        private void SetMaterial(Material mat)
        {
            if (_bodyRenderer == null || mat == null) return;
            _bodyRenderer.sharedMaterial = mat;
        }

        // ── Editor gizmos ─────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 pos = transform.position;

            // Alert range — yellow
            UnityEditor.Handles.color = new Color(1f, 0.9f, 0f, 0.08f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, _alertRange);
            UnityEditor.Handles.color = new Color(1f, 0.9f, 0f, 1f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, _alertRange);

            // Alert slow-approach range — orange
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.06f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, _alertSlowRange);
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.7f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, _alertSlowRange);

            // Sweep range — red
            UnityEditor.Handles.color = new Color(1f, 0.1f, 0.1f, 0.08f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, _sweepRadius);
            UnityEditor.Handles.color = new Color(1f, 0.1f, 0.1f, 1f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, _sweepRadius);

            // Sweep trigger range — magenta
            UnityEditor.Handles.color = new Color(1f, 0f, 1f, 0.06f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, _sweepTriggerRange);
            UnityEditor.Handles.color = new Color(1f, 0f, 1f, 0.8f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, _sweepTriggerRange);

            // Slam sphere — at slam point if assigned, else at Marshal's position.
            Vector3 slamPos = Application.isPlaying && _slamPoint != null
                ? _slamPoint.position
                : pos;
            Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
            Gizmos.DrawWireSphere(slamPos, _slamRadius);

            // Waypoints
            if (_waypoints != null)
            {
                Gizmos.color = new Color(0f, 0.8f, 1f, 0.8f);
                for (int i = 0; i < _waypoints.Length; i++)
                {
                    if (_waypoints[i] == null) continue;
                    Gizmos.DrawSphere(_waypoints[i].position, 0.2f);
                    if (i > 0 && _waypoints[i - 1] != null)
                        Gizmos.DrawLine(_waypoints[i - 1].position, _waypoints[i].position);
                }
                // Close the loop.
                if (_waypoints.Length > 1 && _waypoints[0] != null && _waypoints[_waypoints.Length - 1] != null)
                    Gizmos.DrawLine(_waypoints[_waypoints.Length - 1].position, _waypoints[0].position);
            }
        }
#endif
    }
}
