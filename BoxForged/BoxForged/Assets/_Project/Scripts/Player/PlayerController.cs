using UnityEngine;
using UnityEngine.InputSystem;
using Boxhead.Systems;

namespace Boxhead.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 1440f;
        [SerializeField] private float gravity = -20f;

        [Header("Jump")]
        [SerializeField] private float _jumpHeight = 2.5f;
        [SerializeField] private float _apexGravityScale = 0.3f;
        [SerializeField] private float _maxFallSpeed = 25f;
        [SerializeField] private float _fastFallSpeed = -15f;

        [Header("Arena Boundary")]
        // ADR-0001: the fixed low-angle camera (5.5 m height, 36° pitch, FOV 45) sees only
        // ~16.8 m of lateral width at the player's depth. An 18 m radius (36 m diameter) put
        // more than half of any arena fight off-screen. Shrunk to the ADR's recommended 8-9 m
        // range so combat arenas fit inside what the camera actually shows.
        [SerializeField] private float _arenaBoundaryRadius = 8.5f;
        [SerializeField] private Vector3 _arenaCenter = Vector3.zero;

        [Header("Death")]
        [SerializeField] private float _deathAnimSpeed = 1.8f;

        [Header("Auto-Face Enemy")]
        [SerializeField] private bool _autoFaceEnemy = true;
        [SerializeField] private float _autoFaceRadius = 5f;
        [SerializeField] private float _autoFaceCloseRange = 2.5f;
        [SerializeField] private float _autoFaceRotationSpeed = 900f;
        [SerializeField] private LayerMask _enemyLayerMask = ~0;

        public Vector3 CurrentMoveDirection { get; private set; }
        public bool IsGrounded => _controller.isGrounded;
        // Raw isGrounded read — flickers false during running. Only use for double-jump prevention.
        // For any other aerial check, use IsReliablyAirborne.
        public bool IsAirborne => !_controller.isGrounded;
        // Requires the character to have been continuously airborne for longer than the debounce
        // window before reporting true — prevents isGrounded flicker during running from being
        // misread as a genuine aerial state by CombatController.OnAttack.
        public bool IsReliablyAirborne => !_controller.isGrounded && _ungroundedTimer >= GroundedGraceTime;

        private CharacterController _controller;
        private PlayerStats _stats;
        private CombatController _combat;
        private Vector2 _moveInput;
        // Additive world-space displacement queued by external systems that need to move the
        // player without owning input (e.g. GrasscutterAI's Whirlwind Pull — Boxhead.Enemy has
        // no other way to move a CharacterController-driven player). Zero by default, so this
        // is fully backward-compatible: no caller means no behavior change. Applied once per
        // frame via CharacterController.Move alongside normal movement, then cleared.
        private Vector3 _externalDisplacement;
        private float _verticalVelocity;
        private float _jumpVelocity;
        private bool _wasGrounded;
        private bool _isFastFalling;
        private float _ungroundedTimer;
        private const float GroundedGraceTime = 0.12f;

        private Camera _mainCamera;
        private Animator _animator;
        private BoxSystem _boxSystem;

        private readonly Collider[] _autoFaceBuffer = new Collider[4];

        private static readonly int AnimJump = Animator.StringToHash("Jump");
        private static readonly int AnimIsDead = Animator.StringToHash("IsDead");

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();
            _combat = GetComponent<CombatController>();
            _mainCamera = Camera.main;
            _animator = GetComponentInChildren<Animator>();
            _boxSystem = GetComponent<BoxSystem>();

            // v = sqrt(2 * |g| * h) — derived from kinematic equations for jump height
            _jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * _jumpHeight);
            _wasGrounded = true;
        }

        private void Start()
        {
            if (_boxSystem != null) _boxSystem.OnModelChanged += RefreshAnimator;
            if (_stats != null) _stats.OnDeath += OnPlayerDeath;

            // Ensure Jump action is enabled (workaround for iOS input issue)
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                var jumpAction = playerInput.actions?.FindAction("Jump");
                if (jumpAction != null && !jumpAction.enabled)
                    jumpAction.Enable();
            }
        }

        private void RefreshAnimator()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        private void OnDestroy()
        {
            if (_boxSystem != null) _boxSystem.OnModelChanged -= RefreshAnimator;
            if (_stats != null) _stats.OnDeath -= OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            if (_animator != null)
            {
                _animator.speed = _deathAnimSpeed;
                _animator.SetBool(AnimIsDead, true);
            }
        }

        // Called by the Input System via PlayerInput component
        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            if (!value.isPressed) return;
            if (_stats.IsDead) return;
            if (_combat != null && _combat.State == CombatState.Staggered) return;
            if (_combat != null && _combat.State == CombatState.Parrying) return;
            if (_combat != null && _combat.State == CombatState.Countering) return;
            if (IsReliablyAirborne) return;  // Use debounced check — raw isGrounded flickers during running
            _verticalVelocity = _jumpVelocity;
            _isFastFalling = false;
            _animator?.ResetTrigger(AnimJump);
            _animator?.SetTrigger(AnimJump);
            Core.AudioManager.Instance?.Play(Core.SoundEvent.PlayerJump);
        }

        /// <summary>
        /// Called by CombatController to trigger aerial-attack fast-fall.
        /// Suppresses normal apex gravity and drives the player downward at a fixed speed.
        /// </summary>
        public void SetFastFall()
        {
            _isFastFalling = true;
            _verticalVelocity = _fastFallSpeed;
        }

        /// <summary>
        /// Queues a world-space displacement applied this frame via CharacterController.Move,
        /// additive to normal input-driven movement. Lets an external hazard (e.g. a boss's
        /// sustained pull attack) reposition the player without needing to own input or fight
        /// the CharacterController directly. Safe to call every frame from multiple sources —
        /// displacements accumulate and are cleared after being applied.
        /// </summary>
        public void ApplyExternalDisplacement(Vector3 worldDelta)
        {
            _externalDisplacement += worldDelta;
        }

        private void Update()
        {
            ApplyGravity();
            Move();
            if (_autoFaceEnemy && !_stats.IsDead)
                AutoFaceNearestEnemy();

            // CharacterController.isGrounded can flicker false for 1–2 frames after a Move() call,
            // which would incorrectly fire the Walk→Jump transition. GroundedGraceTime debounces it:
            // the Animator only sees IsGrounded=false after the character has been truly airborne for
            // longer than the grace window. Real jumps still work because _verticalVelocity carries
            // the character upward for far longer than 0.12 s.
            bool physicsGrounded = _controller.isGrounded;
            if (physicsGrounded)
                _ungroundedTimer = 0f;
            else
                _ungroundedTimer += Time.deltaTime;
            bool isGrounded = physicsGrounded || _ungroundedTimer < GroundedGraceTime;
            _animator?.SetBool("IsGrounded", isGrounded);
            if (!_wasGrounded && physicsGrounded)
                _combat?.NotifyLanded();
            _wasGrounded = physicsGrounded;

            if (_externalDisplacement != Vector3.zero)
            {
                _controller.Move(_externalDisplacement);
                _externalDisplacement = Vector3.zero;
            }

            ClampToArenaBounds();
        }

        private void Move()
        {
            if (_stats != null && _stats.IsDead)
            {
                _animator?.SetFloat("Speed", 0f);
                return;
            }

            if (_moveInput.sqrMagnitude < 0.01f)
            {
                _animator?.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
                return;
            }

            if (_mainCamera == null) return;

            if (_combat != null && _combat.State == CombatState.Parrying)
            {
                _animator?.SetFloat("Speed", 0f);
                return;
            }

            // Convert joystick input to world-space direction relative to camera
            Vector3 camForward = _mainCamera.transform.forward;
            Vector3 camRight = _mainCamera.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
            CurrentMoveDirection = moveDir;

            // Scale movement speed by how well the character faces the target direction.
            // When turning sharply the character slows so it pivots rather than arcing wide.
            // dot=1 (aligned) → full speed. dot=0 (90°) → 50% speed. dot=-1 (180°) → min speed.
            float dot         = Vector3.Dot(-transform.forward, moveDir); // model faces -forward
            float speedFactor = Mathf.Lerp(0.35f, 1f, (dot + 1f) * 0.5f);
            float inputMag = _moveInput.magnitude;
            _controller.Move(moveDir * _stats.MoveSpeed * inputMag * speedFactor * Time.deltaTime);

            // Damped Speed — smooths blend tree transitions so walk/run don't pop
            _animator?.SetFloat("Speed", inputMag * speedFactor, 0.1f, Time.deltaTime);
            _animator?.SetFloat("StrafeX", 0f);

            // Rotate to face movement direction
            if (moveDir != Vector3.zero)
            {
                // CharacterModel child has Y=180° to face the FBX's -Z forward.
                // Rotating the root toward -moveDir keeps the model's face pointing at moveDir.
                Quaternion targetRotation = Quaternion.LookRotation(-moveDir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        private void AutoFaceNearestEnemy()
        {
            if (_combat != null &&
                (_combat.State == CombatState.Dodging || _combat.State == CombatState.Staggered))
                return;

            int count = Physics.OverlapSphereNonAlloc(
                transform.position, _autoFaceRadius, _autoFaceBuffer, _enemyLayerMask);

            Transform nearest = null;
            float nearestSqDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!_autoFaceBuffer[i].CompareTag("Enemy")) continue;
                var stats = _autoFaceBuffer[i].GetComponentInParent<Boxhead.Enemy.EnemyStats>();
                if (stats != null && stats.IsDead) continue;
                float sqDist = (_autoFaceBuffer[i].transform.position - transform.position).sqrMagnitude;
                if (sqDist < nearestSqDist)
                {
                    nearestSqDist = sqDist;
                    nearest = _autoFaceBuffer[i].transform;
                }
            }

            if (nearest == null) return;

            // Always auto-face when standing still. When moving, only override the movement
            // direction rotation when the enemy is within close combat range — otherwise the
            // player controls their own facing while running around the arena.
            bool isMoving = _moveInput.sqrMagnitude > 0.1f;
            bool withinCloseRange = nearestSqDist <= _autoFaceCloseRange * _autoFaceCloseRange;
            if (isMoving && !withinCloseRange) return;

            Vector3 toEnemy = nearest.position - transform.position;
            toEnemy.y = 0f;
            if (toEnemy.sqrMagnitude < 0.01f) return;
            toEnemy.Normalize();

            // Same convention as movement rotation: root faces -dir so child's Y=180° offset faces the target
            Quaternion target = Quaternion.LookRotation(-toEnemy);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, _autoFaceRotationSpeed * Time.deltaTime);
        }

        private void ClampToArenaBounds()
        {
            if (_arenaBoundaryRadius <= 0f) return;
            Vector3 pos = transform.position;
            Vector3 flatOffset = new Vector3(pos.x - _arenaCenter.x, 0f, pos.z - _arenaCenter.z);
            if (flatOffset.sqrMagnitude <= _arenaBoundaryRadius * _arenaBoundaryRadius) return;
            Vector3 clamped = _arenaCenter + flatOffset.normalized * _arenaBoundaryRadius;
            transform.position = new Vector3(clamped.x, pos.y, clamped.z);
        }

        private void ApplyGravity()
        {
            if (_controller.isGrounded)
            {
                // -10f instead of the typical -2f: at rest the only Move() call is ApplyGravity's
                // vertical push, which must exceed skinWidth (0.08 m) per frame to reliably
                // register isGrounded = true. -2 * 0.016 = 0.032 < 0.08 → isGrounded flickers
                // false → IsReliablyAirborne becomes true → jump silently blocked when standing still.
                _verticalVelocity = -10f;
                _isFastFalling = false;
            }
            else if (_isFastFalling)
            {
                // Fast-fall locks velocity — no gravity accumulation, constant downward speed
                _verticalVelocity = _fastFallSpeed;
            }
            else
            {
                // Reduced gravity near apex (rising and near peak) for floatier feel
                // Apex threshold: still ascending but under 3 m/s — reduces gravity to create the 0.3s hold
                float gravityThisFrame = (_verticalVelocity > 0f && _verticalVelocity < 3f)
                    ? gravity * _apexGravityScale
                    : gravity;
                _verticalVelocity += gravityThisFrame * Time.deltaTime;
                // Clamp terminal fall speed to prevent excessive acceleration on mobile
                if (_verticalVelocity < -_maxFallSpeed)
                    _verticalVelocity = -_maxFallSpeed;
            }

            _controller.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
        }

        // Silent receivers for RPG Mecanim animation events on character model children.
        // Without these the console floods with "no receiver" errors every footstep.
        private void FootR() { }
        private void FootL() { }
        private void Hit()   { }
    }
}
