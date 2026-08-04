using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using Boxhead.Core;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    /// <summary>
    /// Two-phase boss AI for the Permit Pulper. Phase 1 uses bureaucratic melee and paper
    /// volleys; Phase 2 escalates with spin attacks, barrages, and a charge tackle.
    /// All win-gating is routed through GameManager.TriggerWin via DefeatSequence only.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(EnemyStats))]
    public class PermitPulperBossAI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Movement")]
        [SerializeField] private float _chaseSpeed          = 1.1f;
        [SerializeField] private float _rotateSpeed         = 60f;
        [Tooltip("Speed the walk animation was designed for. Animator speed scales with actual/reference so feet never slide.")]
        [SerializeField] private float _walkAnimRefSpeed    = 1.1f;
        [SerializeField] private float _detectionRadius     = 18f;
        [SerializeField] private float _attackRadius        = 2.5f;

        [Header("Phase 2 Scaling")]
        [SerializeField] private float _phase2SpeedMult     = 1.4f;
        [SerializeField] private float _phase2CooldownMult  = 0.65f;

        [Header("Attack Timing")]
        [SerializeField] private float _attackCooldown      = 1.8f;
        [SerializeField] private float _staggerDuration     = 1.5f;

        [Header("Informational")]
        [Tooltip("Informational only — actual HP lives in EnemyStats.")]
        [SerializeField] private float _maxHealth           = 200f;

        [Header("Projectiles")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform  _projectileSpawnPoint;

        [Header("Screen Shake")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        // ── State ─────────────────────────────────────────────────────────────

        private enum BossState { Idle, Approaching, WindUp, Attacking, Staggered, PhaseTransition, Dead }
        private enum Phase      { One, Two }

        private BossState _state  = BossState.Idle;
        private Phase     _phase  = Phase.One;

        private int   _attackIndex;
        private bool  _phaseTransitioned;
        private float _attackCooldownTimer;

        // ── References ────────────────────────────────────────────────────────

        private CharacterController _controller;
        private EnemyStats          _stats;
        private Animator            _animator;
        private CombatController    _playerCombat;
        private Transform           _player;
        private Material            _material;
        private Color               _baseColor;
        private Coroutine           _activeRoutine;

        // Pre-allocated overlap buffer — boss attacks overlap at most 1 player collider.
        private readonly Collider[] _overlapBuffer = new Collider[4];

        // ── Animator param hashes ─────────────────────────────────────────────

        private static readonly int AnimSpeed   = Animator.StringToHash("Speed");
        private static readonly int AnimAttack  = Animator.StringToHash("AttackTrigger");
        private static readonly int AnimStagger = Animator.StringToHash("StaggerTrigger");
        private static readonly int AnimIsDead  = Animator.StringToHash("IsDead");

        // ── Cached yields ─────────────────────────────────────────────────────

        private WaitForSeconds _waitStagger;
        private WaitForSeconds _waitPhaseTransition;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stats      = GetComponent<EnemyStats>();
            _animator   = GetComponentInChildren<Animator>();

            var rend = GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                _material  = rend.material;
                _baseColor = _material.color;
            }

            _waitStagger          = new WaitForSeconds(_staggerDuration);
            _waitPhaseTransition  = new WaitForSeconds(1.5f);
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _player       = playerObj.transform;
                _playerCombat = playerObj.GetComponent<CombatController>();
                if (_playerCombat != null)
                    _playerCombat.OnCounterStrike += OnCounterStrikeLanded;
            }

            _stats.OnDeath += HandleDeath;
        }

        private void Update()
        {
            if (_state == BossState.Dead) return;
            if (_player == null) return;

            if (_attackCooldownTimer > 0f)
                _attackCooldownTimer -= Time.deltaTime;

            switch (_state)
            {
                case BossState.Idle:
                    float distToPlayer = Vector3.Distance(transform.position, _player.position);
                    if (distToPlayer <= _detectionRadius)
                        _state = BossState.Approaching;
                    break;

                case BossState.Approaching:
                    Approach();
                    break;
            }

            bool isMoving = _state == BossState.Approaching;
            float moveSpeed = _phase == Phase.One ? _chaseSpeed : _chaseSpeed * _phase2SpeedMult;

            // Drive walk animation via animator.speed (0=frozen, >0=playing)
            // Speed is calibrated so feet match ground movement exactly.
            if (_animator != null)
            {
                float targetAnimSpeed = isMoving && _walkAnimRefSpeed > 0f
                    ? moveSpeed / _walkAnimRefSpeed
                    : 0f;

                // Ensure we're in Walk state — skip the standing intro frames (0-0.12)
                // so the stride starts immediately when the boss begins moving
                if (isMoving && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                    _animator.Play("Walk", 0, 0.13f);

                _animator.speed = targetAnimSpeed;
            }

            // Apply gravity each frame
            if (!_controller.isGrounded)
                _controller.Move(new Vector3(0f, -9.81f * Time.deltaTime, 0f));
        }

        // ── Movement ──────────────────────────────────────────────────────────

        private void Approach()
        {
            float dist = Vector3.Distance(transform.position, _player.position);

            if (dist > _detectionRadius * 1.5f)
            {
                _state = BossState.Idle;
                return;
            }

            if (dist <= _attackRadius && _attackCooldownTimer <= 0f)
            {
                float cooldown = _phase == Phase.One
                    ? _attackCooldown
                    : _attackCooldown * _phase2CooldownMult;
                _attackCooldownTimer = cooldown;
                StopActive();
                StartActive(AttackRoutine());
                return;
            }

            MoveToward(_player.position, _phase == Phase.One
                ? _chaseSpeed
                : _chaseSpeed * _phase2SpeedMult);
        }

        private void MoveToward(Vector3 target, float speed)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(dir.normalized),
                    _rotateSpeed * Time.deltaTime);
            }

            Vector3 motion = dir.normalized * speed * Time.deltaTime;
            motion.y = 0f;
            _controller.Move(motion);
        }

        private void FacePlayer()
        {
            if (_player == null) return;
            Vector3 dir = _player.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(dir.normalized),
                    _rotateSpeed * Time.deltaTime);
        }

        // ── Attack dispatch ────────────────────────────────────────────────────

        private IEnumerator AttackRoutine()
        {
            _state = BossState.WindUp;

            if (_phase == Phase.One)
                yield return StartCoroutine(Phase1Attack());
            else
                yield return StartCoroutine(Phase2Attack());
        }

        // Phase 1 pool (index 0–3 cycling): StampSlam → FormShove → ClawSwipe → PaperVolley
        private IEnumerator Phase1Attack()
        {
            switch (_attackIndex % 4)
            {
                case 0: yield return StartCoroutine(StampSlam());   break;
                case 1: yield return StartCoroutine(FormShove());   break;
                case 2: yield return StartCoroutine(ClawSwipe());   break;
                case 3: yield return StartCoroutine(PaperVolley()); break;
            }
            _attackIndex++;

            // Check for phase transition after each attack completes.
            if (!_phaseTransitioned && _stats.CurrentHealth <= _stats.MaxHealth * 0.5f)
                yield return StartCoroutine(PhaseTransitionRoutine());

            if (_state != BossState.Dead && _state != BossState.Staggered)
                _state = BossState.Approaching;
        }

        // Phase 2 pool (index 0–3 cycling): ShredSpin → FormBarrage → DoubleStamp → ChargeTackle
        private IEnumerator Phase2Attack()
        {
            switch (_attackIndex % 4)
            {
                case 0: yield return StartCoroutine(ShredSpin());     break;
                case 1: yield return StartCoroutine(FormBarrage());   break;
                case 2: yield return StartCoroutine(DoubleStamp());   break;
                case 3: yield return StartCoroutine(ChargeTackle());  break;
            }
            _attackIndex++;

            if (_state != BossState.Dead)
                _state = BossState.Approaching;
        }

        // ── Phase 1 attacks ───────────────────────────────────────────────────

        private IEnumerator StampSlam()
        {
            // Wind-up 0.8s — telegraph in red
            yield return StartCoroutine(WindUp(Color.red, 0.8f));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            int count = Physics.OverlapSphereNonAlloc(transform.position, 2.0f, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                if (!_overlapBuffer[i].CompareTag("Player")) continue;
                if (_playerCombat == null) break;

                AttackResult result = _playerCombat.TryReceiveAttack(25, parryable: true, attacker: gameObject);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse(Vector3.down * 2f);
                }
                if (result == AttackResult.Parried)
                {
                    yield return StartCoroutine(StaggerRoutine());
                    yield break;
                }
                break;
            }

            yield return new WaitForSeconds(0.3f);
        }

        private IEnumerator FormShove()
        {
            // Short dash toward player at 6 m/s for 0.6s — not parryable
            yield return StartCoroutine(WindUp(new Color(1f, 0.5f, 0f), 0.5f));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (_player == null) yield break;

            Vector3 dashDir = (_player.position - transform.position).normalized;
            dashDir.y = 0f;
            if (dashDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dashDir);

            float elapsed   = 0f;
            bool hitLanded  = false;

            while (elapsed < 0.6f)
            {
                _controller.Move(dashDir * 6f * Time.deltaTime);
                elapsed += Time.deltaTime;

                if (!hitLanded)
                {
                    int count = Physics.OverlapSphereNonAlloc(transform.position, 1.2f, _overlapBuffer);
                    for (int i = 0; i < count; i++)
                    {
                        if (!_overlapBuffer[i].CompareTag("Player")) continue;
                        if (_playerCombat == null) break;
                        AttackResult result = _playerCombat.TryReceiveAttack(20, parryable: false);
                        if (result == AttackResult.Hit)
                        {
                            AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                            _impulseSource?.GenerateImpulse(dashDir * 1.5f);
                        }
                        hitLanded = true;
                        break;
                    }
                }

                yield return null;
            }
        }

        private IEnumerator ClawSwipe()
        {
            // Wide arc — parryable
            yield return StartCoroutine(WindUp(Color.yellow, 0.7f));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            int count = Physics.OverlapSphereNonAlloc(transform.position, 2.8f, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                if (!_overlapBuffer[i].CompareTag("Player")) continue;
                if (_playerCombat == null) break;

                AttackResult result = _playerCombat.TryReceiveAttack(22, parryable: true, attacker: gameObject);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse();
                }
                if (result == AttackResult.Parried)
                {
                    yield return StartCoroutine(StaggerRoutine());
                    yield break;
                }
                break;
            }

            yield return new WaitForSeconds(0.3f);
        }

        private IEnumerator PaperVolley()
        {
            // Step back 1.5 units, then fire 3 projectiles
            yield return StartCoroutine(WindUp(Color.cyan, 0.6f));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (_player != null)
            {
                // Step back from player
                Vector3 awayDir = (transform.position - _player.position).normalized;
                awayDir.y = 0f;
                _controller.Move(awayDir * 1.5f);
            }

            SpawnPaperProjectiles(3, new float[] { 0f, 20f, -20f }, 18);

            yield return new WaitForSeconds(0.4f);
        }

        // ── Phase 2 attacks ───────────────────────────────────────────────────

        private IEnumerator ShredSpin()
        {
            // Rotate 360° over 1.2s; OverlapSphere at peak; parryable via facing check
            yield return StartCoroutine(WindUp(Color.magenta, 0.7f));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            float spinElapsed = 0f;
            float spinDuration = 1.2f;
            float startY = transform.eulerAngles.y;
            bool hitLanded = false;

            while (spinElapsed < spinDuration)
            {
                float t = spinElapsed / spinDuration;
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    startY + 360f * t,
                    transform.eulerAngles.z);

                // Hit window at peak (50% through spin)
                if (!hitLanded && t >= 0.5f)
                {
                    hitLanded = true;
                    int count = Physics.OverlapSphereNonAlloc(transform.position, 3.0f, _overlapBuffer);
                    for (int i = 0; i < count; i++)
                    {
                        if (!_overlapBuffer[i].CompareTag("Player")) continue;
                        if (_playerCombat == null) break;

                        // Parryable if player is facing the boss (dot product check)
                        bool facingBoss = false;
                        if (_player != null)
                        {
                            Vector3 toBoss = (transform.position - _player.position).normalized;
                            facingBoss = Vector3.Dot(_player.forward, toBoss) >= 0.3f;
                        }

                        AttackResult result = _playerCombat.TryReceiveAttack(30, parryable: facingBoss, attacker: gameObject);
                        if (result == AttackResult.Hit)
                        {
                            AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                            _impulseSource?.GenerateImpulse();
                        }
                        if (result == AttackResult.Parried)
                        {
                            yield return StartCoroutine(StaggerRoutine());
                            yield break;
                        }
                        break;
                    }
                }

                spinElapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator FormBarrage()
        {
            // Fire 5 projectiles with 0.1s between each
            yield return StartCoroutine(WindUp(Color.white, 0.6f));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            WaitForSeconds waitBetween = new WaitForSeconds(0.1f);
            for (int i = 0; i < 5; i++)
            {
                SpawnPaperProjectiles(1, new float[] { 0f }, 15);
                yield return waitBetween;
            }

            yield return new WaitForSeconds(0.3f);
        }

        private IEnumerator DoubleStamp()
        {
            // First hit: parryable at 0.5s; second hit: unparryable at 1.1s
            yield return StartCoroutine(WindUp(Color.red, 0.5f));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            int count = Physics.OverlapSphereNonAlloc(transform.position, 2.0f, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                if (!_overlapBuffer[i].CompareTag("Player")) continue;
                if (_playerCombat == null) break;

                AttackResult result = _playerCombat.TryReceiveAttack(25, parryable: true, attacker: gameObject);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse(Vector3.down * 2f);
                }
                if (result == AttackResult.Parried)
                {
                    yield return StartCoroutine(StaggerRoutine());
                    yield break;
                }
                break;
            }

            if (_state == BossState.Dead) yield break;

            // Brief pause between stamps
            yield return new WaitForSeconds(0.6f);

            if (_state == BossState.Dead) yield break;

            // Second stamp: unparryable
            yield return StartCoroutine(WindUp(new Color(0.8f, 0f, 0f), 0.4f));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            count = Physics.OverlapSphereNonAlloc(transform.position, 2.0f, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                if (!_overlapBuffer[i].CompareTag("Player")) continue;
                if (_playerCombat == null) break;

                AttackResult result = _playerCombat.TryReceiveAttack(25, parryable: false);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse(Vector3.down * 2f);
                }
                break;
            }

            yield return new WaitForSeconds(0.3f);
        }

        private IEnumerator ChargeTackle()
        {
            // Capture player position, then sprint at 8 m/s for max 7 units
            yield return StartCoroutine(WindUp(new Color(0.5f, 0f, 1f), 0.7f));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (_player == null) yield break;

            Vector3 capturedTarget = _player.position;
            Vector3 chargeDir      = (capturedTarget - transform.position).normalized;
            chargeDir.y = 0f;
            if (chargeDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(chargeDir);

            float distTraveled = 0f;
            bool hitLanded     = false;

            yield return null; // one frame so boss visibly starts before hit detection

            while (distTraveled < 7f)
            {
                float step = 8f * Time.deltaTime;
                _controller.Move(chargeDir * step);
                distTraveled += step;

                if (!hitLanded)
                {
                    int count = Physics.OverlapSphereNonAlloc(transform.position, 1.2f, _overlapBuffer);
                    for (int i = 0; i < count; i++)
                    {
                        if (!_overlapBuffer[i].CompareTag("Player")) continue;
                        if (_playerCombat == null) break;

                        AttackResult result = _playerCombat.TryReceiveAttack(35, parryable: false);
                        if (result == AttackResult.Hit)
                        {
                            AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                            _impulseSource?.GenerateImpulse(chargeDir * 2f);
                        }
                        hitLanded = true;
                        break;
                    }
                }

                yield return null;
            }
        }

        // ── Shared helpers ────────────────────────────────────────────────────

        private IEnumerator WindUp(Color color, float duration)
        {
            _state = BossState.WindUp;
            SetColor(color);
            FacePlayer();
            yield return new WaitForSeconds(duration);
        }

        /// <summary>
        /// Spawns <paramref name="count"/> paper projectiles, one per angle offset.
        /// Each projectile uses BossProjectile.Initialize() to receive the player combat ref.
        /// </summary>
        private void SpawnPaperProjectiles(int count, float[] angles, int damage)
        {
            if (_projectilePrefab == null || _player == null) return;

            Vector3 spawnPos = _projectileSpawnPoint != null
                ? _projectileSpawnPoint.position
                : transform.position + Vector3.up * 1.5f;

            Vector3 forward = (_player.position - spawnPos).normalized;
            forward.y = 0f;
            if (forward == Vector3.zero) forward = transform.forward;

            for (int i = 0; i < count && i < angles.Length; i++)
            {
                Vector3 dir = Quaternion.Euler(0f, angles[i], 0f) * forward;
                GameObject proj = UnityEngine.Object.Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);
                if (proj.TryGetComponent<BossProjectile>(out var bp))
                    bp.Initialize(_playerCombat);
                if (proj.TryGetComponent<Rigidbody>(out var rb))
                    rb.linearVelocity = dir * 8f + Vector3.up * 1.5f;
            }
        }

        // ── Phase transition ──────────────────────────────────────────────────

        private IEnumerator PhaseTransitionRoutine()
        {
            _phaseTransitioned = true;
            _state = BossState.PhaseTransition;
            _animator?.SetTrigger(AnimStagger);
            SetColor(_baseColor);

            _impulseSource?.GenerateImpulse();

            yield return _waitPhaseTransition;

            _phase       = Phase.Two;
            _attackIndex = 0;
            _state       = BossState.Approaching;
        }

        // ── Stagger ───────────────────────────────────────────────────────────

        private IEnumerator StaggerRoutine()
        {
            _state = BossState.Staggered;
            _animator?.SetTrigger(AnimStagger);
            _impulseSource?.GenerateImpulse(Vector3.up * 0.5f);
            SetColor(Color.yellow);
            yield return _waitStagger;
            SetColor(_baseColor);
            if (_state == BossState.Staggered)
                _state = BossState.Approaching;
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void OnCounterStrikeLanded(GameObject target)
        {
            // Counter strike only deals bonus damage during stagger — same contract as BasicEnemyAI.
            if (target != null && target != gameObject) return;
            if (_state != BossState.Staggered) return;
            // 2× bonus: player's base attackDamage is 15; counter bonus = extra 30 on top
            _stats.TakeDamage(30);
        }

        private void HandleDeath()
        {
            _state = BossState.Dead;

            // StopAllCoroutines terminates all nested attack coroutines started via
            // yield return StartCoroutine() — stopping only the outer routine leaves inners running.
            StopAllCoroutines();
            _activeRoutine = null;

            if (_animator != null)
            {
                _animator.speed = 1f;
                _animator.SetFloat(AnimSpeed, 0f);
                _animator.SetBool(AnimIsDead, true);
            }

            SetColor(Color.gray);

            // Destroy any in-flight paper projectiles so they can't kill the player
            // during the defeat sequence.
            foreach (var proj in FindObjectsByType<BossProjectile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                Destroy(proj.gameObject);

            StartCoroutine(DefeatSequence());
        }

        private IEnumerator DefeatSequence()
        {
            // Disable physics movement
            if (_controller != null) _controller.enabled = false;

            Vector3 startScale = transform.localScale;
            float elapsed = 0f;
            float duration = 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            // TriggerWin MUST be called before Destroy — Destroy ends the coroutine immediately.
            GameManager.Instance?.TriggerWin();
            Destroy(gameObject);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetColor(Color color)
        {
            if (_material != null)
                _material.color = color;
        }

        private void StartActive(IEnumerator routine)
        {
            _activeRoutine = StartCoroutine(routine);
        }

        private void StopActive()
        {
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            _activeRoutine = null;

            if (_playerCombat != null)
                _playerCombat.OnCounterStrike -= OnCounterStrikeLanded;

            if (_stats != null)
                _stats.OnDeath -= HandleDeath;

            if (_material != null)
                Destroy(_material);
        }

        // Silent receivers for RPG Mecanim footstep animation events
        private void FootR() { }
        private void FootL() { }
        private void Hit()   { }
    }
}
