using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(Rigidbody))]
    public class SprinklerSentinelAI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Jump Movement")]
        [SerializeField] private float _jumpHeight      = 3f;
        [SerializeField] private float _jumpForwardSpeed = 8f;
        [SerializeField] private float _windUpDuration  = 0.3f;
        [SerializeField] private float _burrowDepth     = -0.35f; // Y position when burrowed
        [SerializeField] private float _burrowSpeed     = 4f;     // how fast it sinks/rises

        [Header("Rotation")]
        [SerializeField] private float _idleSweepSpeed      = 40f;
        [SerializeField] private float _idleSweepRange      = 60f;
        [SerializeField] private float _activeRotateSpeed   = 200f;
        [SerializeField] private float _overheatRotateSpeed = 400f;

        [Header("Detection")]
        [SerializeField] private float _aggroRange = 7f;

        [Header("Arm Pivot")]
        [Tooltip("SprinklerArmPivot child transform.")]
        [SerializeField] private Transform _armPivot;
        [SerializeField] private float _armSpinSpeed = 120f; // X-axis rotation speed (degrees/sec)
        [SerializeField] private Transform[] _armJoints;     // assign Arm_0, Arm_1 in inspector
        [SerializeField] private float _armTwistSpeed = 90f; // degrees per second

        [Header("Nozzles & Firing")]
        [SerializeField] private Transform[] _nozzles;
        [SerializeField] private GameObject  _waterBurstPrefab;
        [SerializeField] private float       _projectileSpeed       = 10f;
        [SerializeField] private float       _burstInterval         = 1.2f;
        [SerializeField] private float       _overheatBurstInterval = 0.6f;
        [SerializeField] private float       _burstLifetime         = 3f;
        [SerializeField] private float       _firingDuration        = 3f;  // how long to fire after landing

        [Header("Weak Point")]
        [Tooltip("Forward points out through the eye. Leave null to use root transform.")]
        [SerializeField] private Transform _eyeTransform;
        [SerializeField] [Range(0f, 1f)] private float _eyeWeakPointDot    = 0.7f;
        [SerializeField]                 private float _eyeDamageMultiplier = 2f;

        [Header("Phase Threshold")]
        [SerializeField] [Range(0f, 1f)] private float _overheatThreshold = 0.5f;

        [Header("Visuals")]
        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private Material _overheatMaterial;
        [SerializeField] private Material _deadMaterial;

        // ── Public events ─────────────────────────────────────────────────────

        public event Action OnEyeHit;
        public event Action OnEnterOverheat;

        // ── State ─────────────────────────────────────────────────────────────

        private enum SentinelState { Idle, WindUp, Jumping, Landing, Firing, Overheated, Dead }

        private SentinelState _state = SentinelState.Idle;

        // ── Runtime references ────────────────────────────────────────────────

        private EnemyStats _stats;
        private Rigidbody  _rb;
        private Transform  _player;
        private Coroutine  _firingRoutine;
        private Coroutine  _jumpRoutine;

        // ── Cached data ───────────────────────────────────────────────────────

        private Material _normalMaterial;
        private Material _overheatMaterialInstance;
        private Material _deadMaterialInstance;
        private int      _prevHealth;
        private float    _aggroRangeSq;

        // Jump tracking
        private bool     _isGrounded = true;
        private bool     _hasLeftGround;
        private Vector3  _jumpTarget;
        private float    _flipStartTime;
        private float    _flipDuration;
        private float    _flipStartRotX;
        private float    _flipFacingY;
        private float    _extraGravity;

        // Idle sweep tracking
        private float _sweepAngle;
        private float _sweepDirection = 1f;

        // Cached WaitForSeconds — never new'd inside coroutines
        private WaitForSeconds _waitBurst;
        private WaitForSeconds _waitOverheatBurst;
        private WaitForSeconds _waitWindUp;

        // Projectile pool
        private ObjectPool<GameObject> _burstPool;

        // Recursion guard: OnHit fires again when bonus TakeDamage is called.
        private bool _processingHit;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            _rb    = GetComponent<Rigidbody>();

            if (_eyeTransform == null) _eyeTransform = transform;

            _aggroRangeSq = _aggroRange * _aggroRange;

            if (_bodyRenderer != null)
            {
                _normalMaterial = _bodyRenderer.sharedMaterial;
                if (_overheatMaterial != null)
                    _overheatMaterialInstance = new Material(_overheatMaterial);
                if (_deadMaterial != null)
                    _deadMaterialInstance = new Material(_deadMaterial);
            }

            _waitBurst         = new WaitForSeconds(_burstInterval);
            _waitOverheatBurst = new WaitForSeconds(_overheatBurstInterval);
            _waitWindUp        = new WaitForSeconds(_windUpDuration);

            if (_waterBurstPrefab != null)
            {
                int capacity = _nozzles != null ? _nozzles.Length * 4 : 16;
                _burstPool = new ObjectPool<GameObject>(
                    createFunc:      CreatePooledBurst,
                    actionOnGet:     go => go.SetActive(true),
                    actionOnRelease: go => go.SetActive(false),
                    actionOnDestroy: go => Destroy(go),
                    collectionCheck: false,
                    defaultCapacity: capacity,
                    maxSize:         capacity * 2);
            }
        }

        private void Start()
        {
            // Start burrowed and kinematic so it doesn't fall over
            _rb.isKinematic = true;
            Vector3 startPos = transform.position;
            startPos.y = _burrowDepth;
            transform.position = startPos;

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
            else
                Debug.LogWarning("[SprinklerSentinelAI] No GameObject tagged 'Player' found. Sentinel will remain dormant.", this);

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

            StopFiring();
            StopJumpRoutine();
            _burstPool?.Dispose();

            if (_overheatMaterialInstance != null) Destroy(_overheatMaterialInstance);
            if (_deadMaterialInstance     != null) Destroy(_deadMaterialInstance);
        }

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (_state == SentinelState.Dead) return;

            switch (_state)
            {
                case SentinelState.Idle:
                    UpdateIdleSweep();
                    CheckAggroRange();
                    break;

                case SentinelState.WindUp:
                    break;

                case SentinelState.Jumping:
                    break;

                case SentinelState.Landing:
                    break;

                case SentinelState.Firing:
                    transform.Rotate(Vector3.up, _activeRotateSpeed * Time.deltaTime, Space.World);
                    RotateArms(_armSpinSpeed);
                    TwistArms(_armTwistSpeed);
                    break;

                case SentinelState.Overheated:
                    transform.Rotate(Vector3.up, _overheatRotateSpeed * Time.deltaTime, Space.World);
                    RotateArms(_armSpinSpeed * 1.5f);
                    TwistArms(_armTwistSpeed * 1.5f);
                    if (_state == SentinelState.Overheated && _isGrounded && _jumpRoutine == null)
                    {
                        // In overheat, continue jumping
                        CheckAggroRange();
                    }
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (_state == SentinelState.Jumping)
            {
                // Apply extra gravity for a snappier arc
                if (_extraGravity > 0f)
                    _rb.AddForce(Vector3.down * _extraGravity, ForceMode.Acceleration);

                // While jumping, use velocity to determine if we've left ground
                if (_rb.linearVelocity.y > 0.5f)
                    _hasLeftGround = true;

                // Only check grounded after we've been airborne
                if (_hasLeftGround)
                    _isGrounded = _rb.linearVelocity.y <= 0f && Physics.Raycast(transform.position + Vector3.up * 0.3f, Vector3.down, 0.5f);
                else
                    _isGrounded = false;
            }
            else
            {
                _isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.3f, Vector3.down, 0.5f);
            }
        }

        // ── Idle sweep ────────────────────────────────────────────────────────

        private void UpdateIdleSweep()
        {
            float halfRange = _idleSweepRange * 0.5f;
            _sweepAngle    += _sweepDirection * _idleSweepSpeed * Time.deltaTime;
            _sweepAngle     = Mathf.Clamp(_sweepAngle, -halfRange, halfRange);

            if      (_sweepAngle >=  halfRange) _sweepDirection = -1f;
            else if (_sweepAngle <= -halfRange) _sweepDirection =  1f;

            // Whole body sweeps as one piece
            transform.rotation = Quaternion.Euler(0f, _sweepAngle, 0f);
        }

        // ── Jump flip ─────────────────────────────────────────────────────────

        private void UpdateBodyFlip()
        {
            // Continuous flip at constant speed. Does NOT clamp to 360 — keeps spinning
            // until landing. LandAndBurrow picks up from whatever angle we're at.
            float elapsed = Time.time - _flipStartTime;
            float rotX = (elapsed / _flipDuration) * 360f;
            transform.rotation = Quaternion.Euler(rotX, _flipFacingY, 0f);
        }

        // ── Burrow ─────────────────────────────────────────────────────────

        private IEnumerator LandAndBurrow()
        {
            // Phase 1: brief settle — stand upright, no spin (0.1s)
            float settleTime = 0.1f;
            float settleElapsed = 0f;
            while (settleElapsed < settleTime)
            {
                settleElapsed += Time.deltaTime;
                yield return null;
            }

            // Phase 2: drill-spin while sinking into ground
            float startY = transform.position.y;
            float targetY = _burrowDepth;
            float totalDistance = startY - targetY;
            if (totalDistance < 0.01f) totalDistance = 0.35f;
            float duration = totalDistance / _burrowSpeed;
            float elapsed = 0f;
            float spinAngle = transform.eulerAngles.y;
            float drillSpinSpeed = 2880f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                spinAngle += drillSpinSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Euler(0f, spinAngle, 0f);

                float newY = Mathf.Lerp(startY, targetY, t);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);

                yield return null;
            }

            transform.rotation = Quaternion.Euler(0f, spinAngle, 0f);
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        }

        private IEnumerator BurrowDown()
        {
            // Simple burrow (used if needed standalone)
            Vector3 pos = transform.position;
            float targetY = _burrowDepth;
            while (pos.y > targetY + 0.01f)
            {
                pos.y = Mathf.MoveTowards(pos.y, targetY, _burrowSpeed * Time.deltaTime);
                transform.position = pos;
                yield return null;
            }
            pos.y = targetY;
            transform.position = pos;
        }

        private IEnumerator RiseUp()
        {
            // Rise so capsule base sits on ground (capsule center at 0.7, radius 0.5 → base at 0.2)
            // Surface Y for transform = capsule needs center.y - height/2 + radius above ground
            // With center(0,0.7,0) height 1.9 radius 0.5: bottom is at 0.7 - 0.95 = -0.25 local
            // So transform Y should be ~0.25 for base to sit on Y=0 ground
            Vector3 pos = transform.position;
            float surfaceY = 0.25f;
            while (pos.y < surfaceY - 0.01f)
            {
                pos.y = Mathf.MoveTowards(pos.y, surfaceY, _burrowSpeed * Time.deltaTime);
                transform.position = pos;
                yield return null;
            }
            pos.y = surfaceY;
            transform.position = pos;
        }

        // ── Aggro detection ───────────────────────────────────────────────────

        private void CheckAggroRange()
        {
            if (_player == null) return;

            if ((_player.position - transform.position).sqrMagnitude <= _aggroRangeSq)
            {
                if (_state == SentinelState.Idle || (_state == SentinelState.Overheated && _isGrounded && _jumpRoutine == null))
                    StartJumpSequence();
            }
        }

        // ── State transitions ─────────────────────────────────────────────────

        private void StartJumpSequence()
        {
            StopJumpRoutine();
            _jumpRoutine = StartCoroutine(JumpSequence());
        }

        private void StopJumpRoutine()
        {
            if (_jumpRoutine != null)
            {
                StopCoroutine(_jumpRoutine);
                _jumpRoutine = null;
            }
        }

        private IEnumerator JumpSequence()
        {
            // 1. Fire while burrowed (arms shoot, body stays underground)
            _state = SentinelState.Firing;
            WaitForSeconds interval = _state == SentinelState.Overheated ? _waitOverheatBurst : _waitBurst;
            StartFiring(interval);
            yield return new WaitForSeconds(_firingDuration);
            StopFiring();

            // 2. Rise up just enough to clear the ground for launch
            yield return RiseUp();

            // 3. Jump in an arc — 3 units up, 3 units horizontal
            _state = SentinelState.Jumping;
            _hasLeftGround = false;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Aim to land 3 units from the player (random angle around them)
            Vector3 playerPos = _player != null ? _player.position : transform.position;
            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            Vector3 landingTarget = playerPos + Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward * 3f;
            Vector3 toTarget = landingTarget - transform.position;
            toTarget.y = 0f;
            float horizontalDist = toTarget.magnitude;
            Vector3 horizontalDir = horizontalDist > 0.1f ? toTarget.normalized : Vector3.forward;

            // Calculate arc velocity with extra gravity for a snappier, faster arc
            float gravityMult = 7f;
            float effectiveGravity = Mathf.Abs(Physics.gravity.y) * gravityMult;
            float upVelocity = Mathf.Sqrt(2f * effectiveGravity * _jumpHeight);
            float airTime = 2f * upVelocity / effectiveGravity;
            float horizontalSpeed = horizontalDist / airTime;

            _extraGravity = effectiveGravity - Mathf.Abs(Physics.gravity.y);
            _rb.angularVelocity = Vector3.zero;
            _rb.linearVelocity = horizontalDir * horizontalSpeed + Vector3.up * upVelocity;

            // 4. Wait until airborne
            yield return new WaitUntil(() => _hasLeftGround);

            // 5. Wait until landed
            yield return new WaitUntil(() => _isGrounded && _hasLeftGround);

            // 6. Kill velocity on landing
            _extraGravity = 0f;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.constraints = RigidbodyConstraints.FreezeRotationZ;
            _rb.isKinematic = true;

            // 7. Smooth landing: lerp rotation upright WHILE burrowing simultaneously
            _state = SentinelState.Landing;
            yield return LandAndBurrow();

            // 8. Back to idle — check if player is still in range
            _state = SentinelState.Idle;
            _jumpRoutine = null;
            CheckAggroRange();
        }

        private void EnterOverheat()
        {
            if (_state == SentinelState.Dead) return;

            _state = SentinelState.Overheated;

            if (_bodyRenderer != null && _overheatMaterialInstance != null)
                _bodyRenderer.sharedMaterial = _overheatMaterialInstance;

            StopFiring();
            StopJumpRoutine();

            OnEnterOverheat?.Invoke();
        }

        // ── Firing ────────────────────────────────────────────────────────────

        private void StartFiring(WaitForSeconds interval)
        {
            StopFiring();
            _firingRoutine = StartCoroutine(FiringLoop(interval));
        }

        private void StopFiring()
        {
            if (_firingRoutine != null)
            {
                StopCoroutine(_firingRoutine);
                _firingRoutine = null;
            }
        }

        private IEnumerator FiringLoop(WaitForSeconds interval)
        {
            while (true)
            {
                FireAllNozzles();
                yield return interval;
            }
        }

        private void FireAllNozzles()
        {
            if (_burstPool == null || _nozzles == null) return;

            for (int i = 0; i < _nozzles.Length; i++)
            {
                Transform nozzle = _nozzles[i];
                if (nozzle == null) continue;

                GameObject burst = _burstPool.Get();
                burst.transform.SetPositionAndRotation(nozzle.position, nozzle.rotation);

                if (burst.TryGetComponent<Rigidbody>(out var rb))
                {
                    // Project onto XZ so projectiles always travel horizontally regardless of arm tilt.
                    Vector3 fireDir = new Vector3(nozzle.forward.x, 0f, nozzle.forward.z);
                    if (fireDir.sqrMagnitude < 0.01f) fireDir = transform.forward;
                    rb.linearVelocity = fireDir.normalized * _projectileSpeed;
                }

                if (burst.TryGetComponent<WaterBurstReturn>(out var ret))
                    ret.Init(_burstPool, _burstLifetime);
            }
        }

        private GameObject CreatePooledBurst()
        {
            var go = Instantiate(_waterBurstPrefab);
            if (go.GetComponent<WaterBurstReturn>() == null)
                go.AddComponent<WaterBurstReturn>();
            return go;
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void HandleHit()
        {
            // _processingHit prevents the bonus TakeDamage call below from recursing
            // back into HandleHit a second time through OnHit.
            if (_processingHit) return;

            _processingHit = true;
            try
            {
                int damageDealt = _prevHealth - _stats.CurrentHealth;

                if (damageDealt > 0 && IsHitFromEyeArc())
                {
                    int bonus = Mathf.RoundToInt(damageDealt * (_eyeDamageMultiplier - 1f));
                    if (bonus > 0)
                        _stats.TakeDamage(bonus);

                    OnEyeHit?.Invoke();
                }

                _prevHealth = _stats.CurrentHealth;

                if (_stats.IsDead) return;

                if (_state == SentinelState.Firing &&
                    _stats.CurrentHealth <= _stats.MaxHealth * _overheatThreshold)
                {
                    EnterOverheat();
                }
            }
            finally
            {
                _processingHit = false;
            }
        }

        private void HandleDeath()
        {
            _state = SentinelState.Dead;
            StopFiring();
            StopJumpRoutine();

            if (_rb != null)
                _rb.linearVelocity = Vector3.zero;

            // Restore normal material first so an overheat flash can't leave a grey mesh
            if (_bodyRenderer != null && _normalMaterial != null)
                _bodyRenderer.sharedMaterial = _normalMaterial;

            if (_bodyRenderer != null && _deadMaterialInstance != null)
                _bodyRenderer.sharedMaterial = _deadMaterialInstance;

            DetachArms();
            Destroy(gameObject);
        }

        private void DetachArms()
        {
            if (_armPivot == null) return;

            // Collect arm children BEFORE detaching — modifying the hierarchy during iteration is unsafe.
            var armChildren = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < _armPivot.childCount; i++)
                armChildren.Add(_armPivot.GetChild(i));

            for (int i = 0; i < armChildren.Count; i++)
            {
                Transform arm = armChildren[i];
                arm.SetParent(null, worldPositionStays: true);

                Rigidbody armRb = arm.GetComponent<Rigidbody>();
                if (armRb == null) armRb = arm.gameObject.AddComponent<Rigidbody>();

                armRb.isKinematic = false;
                armRb.useGravity  = true;

                // Fling outward from body center with randomised spin
                Vector3 outDir = (arm.position - transform.position).normalized;
                if (outDir.sqrMagnitude < 0.01f) outDir = (i % 2 == 0) ? Vector3.right : Vector3.left;
                armRb.linearVelocity  = outDir * 3f + Vector3.up * 2f;
                armRb.angularVelocity = new Vector3(
                    UnityEngine.Random.Range(-8f, 8f),
                    UnityEngine.Random.Range(-8f, 8f),
                    UnityEngine.Random.Range(-8f, 8f));

                Destroy(arm.gameObject, 5f);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private bool IsHitFromEyeArc()
        {
            if (_player == null) return false;

            Vector3 toPlayer = (_player.position - _eyeTransform.position).normalized;
            return Vector3.Dot(_eyeTransform.forward, toPlayer) >= _eyeWeakPointDot;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        // Spins only the nozzle discs at the arm tips — arms stay rigidly horizontal,
        // only the mechanical face of each nozzle rotates around the firing axis.
        private void RotateArms(float degrees)
        {
            if (_nozzles == null) return;
            for (int i = 0; i < _nozzles.Length; i++)
            {
                if (_nozzles[i] == null) continue;
                _nozzles[i].Rotate(Vector3.forward, degrees * Time.deltaTime, Space.Self);
            }
        }

        private void TwistArms(float degrees)
        {
            if (_armJoints == null || _armPivot == null) return;
            for (int i = 0; i < _armJoints.Length; i++)
            {
                var joint = _armJoints[i];
                if (joint == null) continue;
                // Rotate around the arm's own pipe axis (socket → nozzle direction),
                // computed each frame so it follows the spinning body correctly.
                Vector3 outward = joint.position - _armPivot.position;
                outward.y = 0f;
                if (outward.sqrMagnitude < 0.001f) continue;
                joint.Rotate(outward.normalized, degrees * Time.deltaTime, Space.World);
            }
        }

        // ── Editor gizmos ─────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            DrawAggroGizmo();
            DrawEyeArcGizmo();
        }

        private void DrawAggroGizmo()
        {
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.1f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, _aggroRange);
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 1f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _aggroRange);
        }

        private void DrawEyeArcGizmo()
        {
            Transform eye       = _eyeTransform != null ? _eyeTransform : transform;
            float halfAngleDeg  = Mathf.Acos(Mathf.Clamp(_eyeWeakPointDot, -1f, 1f)) * Mathf.Rad2Deg;
            Vector3 arcFrom     = Quaternion.Euler(0f, -halfAngleDeg, 0f) * eye.forward;

            UnityEditor.Handles.color = new Color(0f, 0.5f, 1f, 0.25f);
            UnityEditor.Handles.DrawSolidArc(eye.position, Vector3.up, arcFrom, halfAngleDeg * 2f, 2f);
        }
#endif
    }
}
