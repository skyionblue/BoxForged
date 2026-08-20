using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class WagonWheelRollerAI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Detection")]
        [SerializeField] private float _aggroRange   = 8f;
        [SerializeField] private float _deaggroRange = 12f;
        [SerializeField] private float _chargeRange  = 3f;

        [Header("Movement")]
        [SerializeField] private float _patrolSpeed = 1.5f;
        [SerializeField] private float _chaseSpeed  = 4f;
        [SerializeField] private float _turnSpeed   = 180f;

        [Header("Charge")]
        [SerializeField] private float     _chargeSpeed     = 12f;
        [SerializeField] private float     _contactRadius   = 0.8f;
        [SerializeField] private float     _windUpDuration  = 0.8f;
        [SerializeField] private float     _chargeDuration  = 1.5f;
        [SerializeField] private float     _stunnedDuration = 2f;
        [SerializeField] private int       _chargeDamage    = 15;
        [SerializeField] private LayerMask _playerLayer     = ~0;

        [Header("Rolling")]
        [SerializeField] private float     _wheelRadius = 0.94f;
        [SerializeField] private Transform _wheelVisual;

        [Header("Hit Roll Back")]
        [SerializeField] private float _hitRollSpeed    = 12f;
        [SerializeField] private float _hitRollDuration = 0.35f;

        [Header("Post Attack Jump")]
        [SerializeField] private float _backJumpSpeed     = 14f;
        [SerializeField] private float _backJumpUpForce   = 8f;
        [SerializeField] private float _backJumpDuration  = 0.7f;
        [SerializeField] private float _postAttackSpinSpeed = 1440f; // degrees/sec while rolling in place

        [Header("Death")]
        [SerializeField] private float _disappearDelay = 3f;

        [Header("Visuals")]
        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private Material _stunnedMaterial;
        [SerializeField] private Material _deadMaterial;

        // ── Public events ─────────────────────────────────────────────────────

        public event Action OnCharge;
        public event Action OnStunned;

        // ── State ─────────────────────────────────────────────────────────────

        private enum RollerState { Idle, Chase, ChargeWindUp, Charging, PostAttack, Dead }

        private RollerState _state = RollerState.Idle;

        // ── Runtime references ────────────────────────────────────────────────

        private EnemyStats       _stats;
        private Rigidbody        _rb;
        private NavMeshAgent     _agent;
        private Transform        _player;
        private CombatController _playerCombat;
        private Coroutine        _stateRoutine;

        // ── Cached data ───────────────────────────────────────────────────────

        private Material _normalMaterial;
        private Material _stunnedMaterialInstance;
        private Material _deadMaterialInstance;

        private Vector3 _spawnPosition;
        private Vector3 _patrolTarget;
        private Vector3 _chargeDirection;

        private float _aggroRangeSq;
        private float _deaggroRangeSq;
        private float _chargeRangeSq;

        private int   _prevHealth;
        private bool  _processingHit;

        private const float PatrolRadius             = 3f;
        private const float StunnedDamageMultiplier = 2f;
        private const float WindUpSpinSpeed         = 720f;
        private const float FallOverDuration        = 0.9f;

        private readonly Collider[] _hitBuffer = new Collider[4];

        // Cached WaitForSeconds — never new'd inside coroutines.
        private WaitForSeconds _waitWindUp;
        private WaitForSeconds _waitStunned;
        private WaitForSeconds _waitCharge;
        private WaitForSeconds _waitBackJump;
        private WaitForSeconds _waitDisappear;
        private WaitForSeconds _waitHitRoll;

        private Coroutine _hitRollRoutine;
        private Coroutine _hitWiggleRoutine;

        private float _pathUpdateTimer;
        private const float PathUpdateInterval = 0.25f;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            _rb    = GetComponent<Rigidbody>();

            _rb.constraints   = RigidbodyConstraints.FreezePositionY
                              | RigidbodyConstraints.FreezeRotationX
                              | RigidbodyConstraints.FreezeRotationZ;
            // Extrapolate: render uses current velocity, so the bounce-back velocity
            // change is visible in the same frame contact is detected (not one physics step later).
            _rb.interpolation = RigidbodyInterpolation.Extrapolate;
            _rb.isKinematic   = false;

            _aggroRangeSq   = _aggroRange   * _aggroRange;
            _deaggroRangeSq = _deaggroRange * _deaggroRange;
            _chargeRangeSq  = _chargeRange  * _chargeRange;

            _spawnPosition = transform.position;
            _patrolTarget  = PickPatrolTarget();

            _waitWindUp    = new WaitForSeconds(_windUpDuration);
            _waitStunned   = new WaitForSeconds(_stunnedDuration);
            _waitCharge    = new WaitForSeconds(_chargeDuration);
            _waitBackJump  = new WaitForSeconds(_backJumpDuration);
            _waitDisappear = new WaitForSeconds(_disappearDelay);
            _waitHitRoll   = new WaitForSeconds(_hitRollDuration);

            _agent = GetComponent<NavMeshAgent>();
            _agent.speed            = _chaseSpeed;
            _agent.stoppingDistance = _chargeRange - 0.1f;
            _agent.updateRotation   = false;
            _agent.isStopped        = true;
            _agent.autoBraking      = false;

            if (_bodyRenderer != null)
            {
                _normalMaterial = _bodyRenderer.sharedMaterial;
                if (_normalMaterial == null && _bodyRenderer.sharedMaterials.Length > 0)
                    _normalMaterial = _bodyRenderer.sharedMaterials[0];
                if (_normalMaterial == null)
                    Debug.LogError("[WagonWheelRollerAI] _bodyRenderer has no sharedMaterial — grey mesh during charge will occur.", this);
                if (_stunnedMaterial != null)
                    _stunnedMaterialInstance = new Material(_stunnedMaterial);
                if (_deadMaterial != null)
                    _deadMaterialInstance = new Material(_deadMaterial);
            }
            else
            {
                Debug.LogError("[WagonWheelRollerAI] _bodyRenderer is not assigned — material changes will be silently skipped.", this);
            }
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _player       = playerObj.transform;
                _playerCombat = playerObj.GetComponent<CombatController>();
            }
            else
            {
                Debug.LogWarning("[WagonWheelRollerAI] No GameObject tagged 'Player' found.", this);
            }

            _prevHealth = _stats.CurrentHealth;

            _stats.OnHit   += HandleHit;
            _stats.OnDeath += HandleDeath;
        }

        private void OnDestroy()
        {
            if (_stats != null)
            {
                _stats.OnHit   -= HandleHit;
                _stats.OnDeath -= HandleDeath;
            }

            StopStateRoutine();
            if (_hitRollRoutine   != null) { StopCoroutine(_hitRollRoutine);   _hitRollRoutine   = null; }
            if (_hitWiggleRoutine != null) { StopCoroutine(_hitWiggleRoutine); _hitWiggleRoutine = null; }

            if (_bodyRenderer != null && _normalMaterial != null)
                _bodyRenderer.sharedMaterial = _normalMaterial;

            if (_stunnedMaterialInstance != null) Destroy(_stunnedMaterialInstance);
            if (_deadMaterialInstance    != null) Destroy(_deadMaterialInstance);
        }

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (_state == RollerState.Dead) return;

            switch (_state)
            {
                case RollerState.Idle:
                    UpdateIdle();
                    break;
                case RollerState.Chase:
                    UpdateChase();
                    break;
                case RollerState.ChargeWindUp:
                    SpinInPlace();
                    break;
                case RollerState.Charging:
                    UpdateCharge();
                    break;
                case RollerState.PostAttack:
                    SpinAtSpeed(_postAttackSpinSpeed);
                    break;
            }
        }

        // ── State update methods ──────────────────────────────────────────────

        private void UpdateIdle()
        {
            float toPlayerSqr = _player != null
                ? (_player.position - transform.position).sqrMagnitude
                : float.MaxValue;

            if (toPlayerSqr <= _aggroRangeSq)
            {
                EnterChase();
                return;
            }

            MoveToward(_patrolTarget, _patrolSpeed);

            if ((transform.position - _patrolTarget).sqrMagnitude < 0.25f)
                _patrolTarget = PickPatrolTarget();
        }

        private void UpdateChase()
        {
            if (_player == null) return;

            float toPlayerSqr = (_player.position - transform.position).sqrMagnitude;

            if (toPlayerSqr <= _chargeRangeSq)
            {
                EnterChargeWindUp();
                return;
            }

            if (toPlayerSqr > _deaggroRangeSq)
            {
                EnterIdle();
                return;
            }

            _pathUpdateTimer -= Time.deltaTime;
            if (_pathUpdateTimer <= 0f)
            {
                _pathUpdateTimer = PathUpdateInterval;
                if (_agent.isOnNavMesh)
                    _agent.SetDestination(_player.position);
            }
            if (_agent.velocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(_agent.velocity.normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);
                ApplyRollRotation(_agent.velocity.magnitude);
            }
        }

        private void UpdateCharge()
        {
            // Face locked direction each frame so the wheel doesn't drift visually.
            transform.rotation = Quaternion.LookRotation(_chargeDirection);

            _rb.linearVelocity = _chargeDirection * _chargeSpeed;
            ApplyRollRotation(_chargeSpeed);

            // Direct distance check against the player transform — more reliable than
            // OverlapSphereNonAlloc for CharacterController capsules in Unity 6.
            if (_player != null)
            {
                Vector3 toPlayer = _player.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude <= _contactRadius * _contactRadius)
                {
                    if (_playerCombat != null)
                        _playerCombat.TryReceiveAttack(_chargeDamage, attacker: gameObject);

                    EnterPostAttack();
                    return;
                }
            }
        }

        // ── State transitions ─────────────────────────────────────────────────

        private void EnterIdle()
        {
            if (!_agent.enabled) _agent.enabled = true;
            _agent.isStopped      = true;
            _agent.ResetPath();
            _agent.updatePosition = true;
            _rb.isKinematic       = false;
            StopStateRoutine();
            _state = RollerState.Idle;
            _rb.linearVelocity = Vector3.zero;
            _patrolTarget = PickPatrolTarget();
            SetMaterial(_normalMaterial);
        }

        private void EnterChase()
        {
            // Guard the velocity write — when this is reached from PostAttackRoutine's
            // stun-end transition, the Rigidbody is already kinematic (set during that
            // routine's landing step) and writing velocity on a kinematic body throws.
            // A kinematic body carries no meaningful velocity to clear, so skip the write
            // rather than reorder — the Idle->Chase caller (non-kinematic) is unaffected.
            if (!_rb.isKinematic) _rb.linearVelocity = Vector3.zero;
            _rb.isKinematic       = true;
            if (!_agent.enabled) _agent.enabled = true;
            _agent.updatePosition = true;
            _agent.Warp(transform.position);
            _agent.isStopped      = false;
            StopStateRoutine();
            _state = RollerState.Chase;
            SetMaterial(_normalMaterial);
        }

        private void EnterChargeWindUp()
        {
            _agent.isStopped = true;
            StopStateRoutine();
            // Guard the velocity write — this is only ever reached from UpdateChase() while
            // the Rigidbody is still kinematic (set in EnterChase(); EnterCharging() is what
            // later makes it non-kinematic). Writing velocity on a kinematic body throws.
            if (!_rb.isKinematic) _rb.linearVelocity = Vector3.zero;
            _state = RollerState.ChargeWindUp;
            // ADR-0003: occlusion-independent overhead telegraph. Charge is parryable today
            // (TryReceiveAttack in UpdateCharge/EnterCharging does not override the default) —
            // shape reflects that, not a new balance decision.
            Boxhead.Core.AttackTelegraphService.Show(
                transform, Boxhead.Core.AttackTelegraphKind.MeleeParryable, _windUpDuration);
            _stateRoutine = StartCoroutine(WindUpRoutine());
        }

        private void EnterCharging()
        {
            if (_player == null)
            {
                EnterIdle();
                return;
            }

            Vector3 dir = _player.position - transform.position;
            dir.y = 0f;
            _chargeDirection = dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward;

            // Disable the NavMeshAgent entirely so it cannot fight the Rigidbody
            // for position control (it was re-setting isKinematic to true each frame).
            _agent.enabled = false;
            _rb.isKinematic = false;
            _rb.linearVelocity = _chargeDirection * _chargeSpeed;
            _state = RollerState.Charging;
            _stateRoutine = StartCoroutine(ChargeDurationRoutine());

            OnCharge?.Invoke();
        }

        private void EnterPostAttack()
        {
            StopStateRoutine();
            _state = RollerState.PostAttack;
            // Guard isStopped — EnterCharging() just disabled the agent for the charge
            // attack, so it is not on the NavMesh here (same guard as EnterDead()).
            if (_agent.isOnNavMesh) _agent.isStopped = true;
            _agent.updatePosition = false;

            // Apply the backward jump velocity immediately — no coroutine startup gap.
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationY
                            | RigidbodyConstraints.FreezeRotationZ;

            Vector3 backward = -_chargeDirection;
            backward.y = 0f;
            if (backward.sqrMagnitude < 0.01f) backward = -transform.forward;
            backward.Normalize();
            _rb.linearVelocity = backward * _backJumpSpeed + Vector3.up * _backJumpUpForce;

            OnStunned?.Invoke();
            _stateRoutine = StartCoroutine(PostAttackRoutine());
        }

        private void EnterDead()
        {
            StopStateRoutine();
            // HitRollRoutine isn't tracked by _stateRoutine — cancel it explicitly so it
            // can't restore a stale isKinematic value or zero the velocity of what is now
            // a kinematic body after this method sets isKinematic below.
            if (_hitRollRoutine != null) { StopCoroutine(_hitRollRoutine); _hitRollRoutine = null; }
            _state = RollerState.Dead;
            _rb.linearVelocity = Vector3.zero;
            _rb.isKinematic    = true;
            // Guard isStopped — wheel may have died mid-air (PostAttack jump) and not be on NavMesh
            if (_agent.isOnNavMesh) _agent.isStopped = true;
            _agent.enabled = false;
            SetMaterial(_normalMaterial);
            SetMaterial(_deadMaterialInstance);
            _stateRoutine = StartCoroutine(FallOverRoutine());
        }

        // ── Coroutines ────────────────────────────────────────────────────────

        private IEnumerator WindUpRoutine()
        {
            yield return _waitWindUp;
            if (_state == RollerState.ChargeWindUp)
                EnterCharging();
        }

        private IEnumerator HitRollRoutine(Vector3 pushDir)
        {
            // Brief roll backward from the hit — no state change, just visual nudge
            bool wasKinematic = _rb.isKinematic;
            _rb.isKinematic   = false;
            _rb.constraints   = RigidbodyConstraints.FreezePositionY
                               | RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;

            _rb.linearVelocity = pushDir * _hitRollSpeed;
            yield return _waitHitRoll;

            _rb.linearVelocity = Vector3.zero;
            _rb.isKinematic    = wasKinematic;
            _hitRollRoutine    = null;
        }

        private IEnumerator HitWiggleRoutine()
        {
            // Rapid side-to-side Z tilt that decays over ~0.4s — "stutter" on hit.
            // Runs concurrently with HitRollRoutine; does not affect X spin.
            Transform visual  = _wheelVisual != null ? _wheelVisual : transform;
            const float Duration  = 0.4f;
            const float Frequency = 9f;   // oscillations per second
            const float Amplitude = 18f;  // peak tilt in degrees

            float elapsed = 0f;
            while (elapsed < Duration)
            {
                elapsed += Time.deltaTime;
                float t       = elapsed / Duration;
                float decay   = 1f - (t * t);                       // quadratic ease-out
                float tiltZ   = Amplitude * decay * Mathf.Sin(elapsed * Frequency * 2f * Mathf.PI);

                // Composite: preserve any existing rotation (spin on X), add tilt on Z
                Quaternion existingRot = visual.localRotation;
                Vector3 euler          = existingRot.eulerAngles;
                euler.z = tiltZ;
                visual.localRotation = Quaternion.Euler(euler);

                yield return null;
            }

            // Snap Z tilt back to zero cleanly
            Vector3 finalEuler = visual.localRotation.eulerAngles;
            finalEuler.z       = 0f;
            visual.localRotation = Quaternion.Euler(finalEuler);
            _hitWiggleRoutine  = null;
        }

        private IEnumerator PostAttackRoutine()
        {
            // Jump velocity is already live (set in EnterPostAttack) — wait for arc to complete.
            yield return _waitBackJump;

            // Land — snap to NavMesh ground level plus a small offset to prevent
            // the roller from clipping into the ground plane on landing.
            Vector3 pos = transform.position;
            if (UnityEngine.AI.NavMesh.SamplePosition(pos, out UnityEngine.AI.NavMeshHit navHit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                pos.y = navHit.position.y + _wheelRadius;
            else
                pos.y = _spawnPosition.y + _wheelRadius;
            transform.position = pos;
            _rb.linearVelocity = Vector3.zero;
            _rb.constraints    = RigidbodyConstraints.FreezePositionY
                               | RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;
            _rb.isKinematic = true;

            // Stun window — spin in place.
            yield return _waitStunned;

            if (_state == RollerState.PostAttack)
                EnterChase();
        }

        private IEnumerator ChargeDurationRoutine()
        {
            yield return _waitCharge;
            if (_state == RollerState.Charging)
                EnterPostAttack();
        }

        private IEnumerator FallOverRoutine()
        {
            Transform visual = _wheelVisual != null ? _wheelVisual : transform;

            // Coin-wobble death:
            //   Spin decelerates while the wheel leans outward.
            //   As gyroscopic stabilisation fades the precession (wobble) frequency
            //   climbs and the side-to-side amplitude shrinks — matching a real coin
            //   rattling to rest. Wobble only activates once the wheel has a
            //   meaningful tilt so the oscillation is always visible, never clipped.

            const float Duration     = 1.8f;
            const float StartSpinDPS = 720f;

            float elapsed     = 0f;
            float spinAngle   = 0f;
            float wobblePhase = 0f;

            while (elapsed < Duration)
            {
                elapsed += Time.deltaTime;
                float t  = elapsed / Duration;
                float dt = Time.deltaTime;

                // Spin decelerates asymptotically.
                float spinDPS = StartSpinDPS * Mathf.Pow(Mathf.Max(0f, 1f - t), 1.8f);
                spinAngle += spinDPS * dt;

                // Wobble frequency rises as gyro fades; amplitude shrinks toward zero.
                float wobbleHz = Mathf.Lerp(1f, 14f, Mathf.Pow(t, 0.6f));
                wobblePhase += wobbleHz * dt * 2f * Mathf.PI;
                float wobbleAmp = 16f * Mathf.Pow(Mathf.Max(0f, 1f - t), 1.5f);

                // Base tilt arc.
                float meanTilt;
                if (t < 0.45f)
                {
                    meanTilt = Mathf.Lerp(0f, 20f, t / 0.45f);
                }
                else if (t < 0.80f)
                {
                    float ft = (t - 0.45f) / 0.35f;
                    meanTilt = Mathf.Lerp(20f, 85f, ft * ft);
                }
                else if (t < 0.90f)
                {
                    float bt = (t - 0.80f) / 0.10f;
                    meanTilt = 85f - Mathf.Sin(bt * Mathf.PI) * 8f;
                }
                else
                {
                    float st = (t - 0.90f) / 0.10f;
                    meanTilt = Mathf.Lerp(85f, 90f, st);
                }

                // Wobble only applied once the wheel has a visible lean; rocking is
                // symmetric (positive and negative) around the mean tilt angle.
                float wobble  = meanTilt > 15f ? Mathf.Sin(wobblePhase) * wobbleAmp : 0f;
                float tiltDeg = Mathf.Clamp(meanTilt + wobble, -22f, 92f);

                visual.localRotation =
                    Quaternion.Euler(0f, 0f, tiltDeg) *
                    Quaternion.Euler(spinAngle, 0f, 0f);

                yield return null;
            }

            visual.localRotation = Quaternion.Euler(0f, 0f, 90f);

            yield return _waitDisappear;
            Destroy(gameObject);
        }

        // ── Movement helpers ──────────────────────────────────────────────────

        private void MoveToward(Vector3 target, float speed)
        {
            Vector3 delta = target - transform.position;
            delta.y = 0f;

            if (delta.sqrMagnitude < 0.01f)
            {
                _rb.linearVelocity = Vector3.zero;
                return;
            }

            Vector3 dir = delta.normalized;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation   = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);

            _rb.linearVelocity = dir * speed;
            ApplyRollRotation(speed);
        }

        private void ApplyRollRotation(float linearSpeed)
        {
            // Axle is along local X — face is in the YZ plane.
            // Rotate around right (X) to spin the wheel as it rolls.
            float degsPerSec = (linearSpeed / (_wheelRadius * 2f * Mathf.PI)) * 360f;
            Transform visual  = _wheelVisual != null ? _wheelVisual : transform;
            visual.Rotate(Vector3.right, degsPerSec * Time.deltaTime, Space.Self);
        }

        private void SpinInPlace()
        {
            Transform visual = _wheelVisual != null ? _wheelVisual : transform;
            visual.Rotate(Vector3.right, WindUpSpinSpeed * Time.deltaTime, Space.Self);
        }

        private void SpinAtSpeed(float degsPerSec)
        {
            Transform visual = _wheelVisual != null ? _wheelVisual : transform;
            visual.Rotate(Vector3.right, degsPerSec * Time.deltaTime, Space.Self);
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void HandleHit()
        {
            if (_state == RollerState.Dead) return;
            if (_processingHit) return;

            _processingHit = true;
            try
            {
                int damageDealt = _prevHealth - _stats.CurrentHealth;

                if (damageDealt > 0)
                {
                    // Roll backwards away from the player on every hit
                    if (_player != null && _hitRollRoutine == null)
                    {
                        Vector3 away = (transform.position - _player.position);
                        away.y = 0f;
                        Vector3 pushDir = away.sqrMagnitude > 0.01f ? away.normalized : -transform.forward;
                        _hitRollRoutine = StartCoroutine(HitRollRoutine(pushDir));
                    }

                    // Stutter wiggle — always fires on every hit, even if a roll is already running
                    if (_hitWiggleRoutine != null) StopCoroutine(_hitWiggleRoutine);
                    _hitWiggleRoutine = StartCoroutine(HitWiggleRoutine());

                    // Bonus damage when caught during the post-attack spin window
                    if (_state == RollerState.PostAttack)
                    {
                        int bonus = Mathf.RoundToInt(damageDealt * (StunnedDamageMultiplier - 1f));
                        if (bonus > 0) _stats.TakeDamage(bonus);
                    }
                }

                if (_state != RollerState.Dead)
                    _prevHealth = _stats.CurrentHealth;
            }
            finally
            {
                _processingHit = false;
            }
        }

        private void HandleDeath()
        {
            EnterDead();
        }

        // ── Utility ───────────────────────────────────────────────────────────

        private Vector3 PickPatrolTarget()
        {
            Vector3 candidate = _spawnPosition + new Vector3(
                UnityEngine.Random.Range(-PatrolRadius, PatrolRadius), 0f,
                UnityEngine.Random.Range(-PatrolRadius, PatrolRadius));

            if (UnityEngine.AI.NavMesh.SamplePosition(candidate, out UnityEngine.AI.NavMeshHit hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                return hit.position;

            return _spawnPosition;
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

            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.1f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, _aggroRange);
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 1f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, _aggroRange);

            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.1f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, _chargeRange);
            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 1f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, _chargeRange);
        }
#endif
    }
}
