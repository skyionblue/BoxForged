using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Boxhead.Core;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class SkepticGruntAI : MonoBehaviour, IEnemyBehavior
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Movement")]
        [SerializeField] private float moveSpeed    = 3f;
        [SerializeField] private float patrolSpeed  = 1.5f;
        [SerializeField] private float chaseRange   = 8f;
        [SerializeField] private float attackRange  = 1.5f;
        [SerializeField] private float patrolRadius = 3f;

        [Header("Attack")]
        [SerializeField] private float windUpDuration       = 0.6f;
        [SerializeField] private float attackActiveDuration = 0.3f;
        [SerializeField] private float attackCooldown       = 2f;
        [Tooltip("Overhead telegraph height. This model's top is ~1.89m (0.9m taller than WagonWheelRoller), so the shared AttackTelegraphService default reads too low here — see BACKLOG.md B56/B69.")]
        [SerializeField] private float telegraphHeightOffset = 2.8f;

        [Header("Stagger")]
        [SerializeField] private float staggerDuration    = 1.5f;
        [SerializeField] private float hitStaggerDuration = 0.4f;

        [Header("Counter Strike")]
        [SerializeField] private int counterStrikeDamage = 25;

        // ── State ─────────────────────────────────────────────────────────────

        private enum State { Idle, Chase, WindUp, Attacking, Staggered, Dead, PlayerDead }
        private State _state = State.Chase; // Start chasing immediately

        // ── Runtime references ────────────────────────────────────────────────

        private EnemyStats       _stats;
        private NavMeshAgent     _agent;
        private Animator         _animator;
        private SkinnedMeshRenderer _renderer;
        private Transform        _player;
        private CombatController _playerCombat;
        private Boxhead.Player.PlayerStats _playerStats;
        private Coroutine        _activeRoutine;

        // ── Cached materials ──────────────────────────────────────────────────

        // sharedMaterial is read-only reference — never mutated.
        private Material _normalMaterial;
        // Single instance created once in Awake for colour flashing — no .material per-frame access.
        private Material _flashMaterialInstance;

        private static readonly Color ColorWindUp   = Color.red;
        private static readonly Color ColorStagger  = Color.yellow;
        private static readonly Color ColorDead     = Color.gray;

        // ── Cached WaitForSeconds ─────────────────────────────────────────────

        private WaitForSeconds _waitWindUp;
        private WaitForSeconds _waitAttackActive;
        private WaitForSeconds _waitStagger;
        private WaitForSeconds _waitDie;

        // ── Root / stagger overrides ──────────────────────────────────────────

        private bool  _isRooted;
        private float _hitStaggerMultiplier = 1f;
        private float _baseSpeed;
        private Coroutine _speedRestoreRoutine;

        // ── Precomputed scalars ───────────────────────────────────────────────

        private float _chaseRangeSq;
        private float _attackRangeSq;
        private float _attackRangeExtendedSq; // attackRange + 0.5f, squared

        // ── Per-frame timers ──────────────────────────────────────────────────

        private float _attackCooldownTimer;
        private float _pathUpdateTimer;
        private const float PathUpdateInterval = 0.25f;

        // ── Patrol ────────────────────────────────────────────────────────────

        private Vector3 _spawnPosition;
        private Vector3 _patrolTarget;

        // ── Animator hashes ───────────────────────────────────────────────────

        private static readonly int AnimSpeed  = Animator.StringToHash("Speed");
        private static readonly int AnimAttack = Animator.StringToHash("AttackTrigger");
        private static readonly int AnimDeath  = Animator.StringToHash("DeathTrigger");

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _stats    = GetComponent<EnemyStats>();
            _agent    = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();

            // SkinnedMeshRenderer — cache sharedMaterial, build one flash instance.
            _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (_renderer != null)
            {
                _normalMaterial       = _renderer.sharedMaterial;
                _flashMaterialInstance = new Material(_normalMaterial);
            }
            else
            {
                Debug.LogWarning("[SkepticGruntAI] No SkinnedMeshRenderer found — colour flashing disabled.", this);
            }

            // NavMeshAgent setup.
            _agent.speed            = moveSpeed;
            _agent.stoppingDistance = attackRange - 0.1f;
            _agent.updateRotation   = false;
            _agent.autoBraking      = false;
            _agent.isStopped        = true;
            _baseSpeed = moveSpeed;

            // Precompute squared ranges to avoid per-frame sqrts.
            _chaseRangeSq           = chaseRange * chaseRange;
            _attackRangeSq          = attackRange * attackRange;
            _attackRangeExtendedSq  = (attackRange + 0.5f) * (attackRange + 0.5f);

            // Cached allocs — zero GC in coroutines.
            _waitWindUp       = new WaitForSeconds(windUpDuration);
            _waitAttackActive = new WaitForSeconds(attackActiveDuration);
            _waitStagger      = new WaitForSeconds(staggerDuration);
            _waitDie          = new WaitForSeconds(2.5f);

            _spawnPosition = transform.position;
            _patrolTarget  = PickPatrolTarget();
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _player       = playerObj.transform;
                _playerCombat = playerObj.GetComponent<CombatController>();
                _playerStats  = playerObj.GetComponent<Boxhead.Player.PlayerStats>();

                if (_playerCombat != null)
                    _playerCombat.OnCounterStrike += OnCounterStrikeLanded;
                if (_playerStats != null)
                    _playerStats.OnDeath += HandlePlayerDeath;
            }
            else
            {
                Debug.LogWarning("[SkepticGruntAI] No GameObject tagged 'Player' found.", this);
            }

            _stats.OnDeath += HandleDeath;
            _stats.OnHit   += OnHitReceived;
        }

        private void OnDestroy()
        {
            StopActive();
            if (_speedRestoreRoutine != null)
                StopCoroutine(_speedRestoreRoutine);

            if (_playerCombat != null)
                _playerCombat.OnCounterStrike -= OnCounterStrikeLanded;
            if (_playerStats != null)
                _playerStats.OnDeath -= HandlePlayerDeath;

            if (_stats != null)
            {
                _stats.OnDeath -= HandleDeath;
                _stats.OnHit   -= OnHitReceived;
            }

            // Restore shared material so other prefab instances aren't affected.
            if (_renderer != null && _normalMaterial != null)
                _renderer.sharedMaterial = _normalMaterial;

            if (_flashMaterialInstance != null)
                Destroy(_flashMaterialInstance);
        }

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (_state == State.Dead || _state == State.PlayerDead) return;
            if (_player == null) return;

            if (_attackCooldownTimer > 0f)
                _attackCooldownTimer -= Time.deltaTime;

            switch (_state)
            {
                case State.Idle:
                    UpdateIdle();
                    break;
                case State.Chase:
                    UpdateChase();
                    break;
            }

            // Animator Speed param — drives Locomotion blend tree.
            // Smooth damping prevents "skipping" when transitioning from idle to run.
            if (_animator != null)
            {
                float targetSpeed = _agent.velocity.magnitude;
                float dampTime = 0.15f; // 150ms smooth blend
                _animator.SetFloat(AnimSpeed, targetSpeed, dampTime, Time.deltaTime);
            }
        }

        // ── IEnemyBehavior ────────────────────────────────────────────────────

        public void SetRooted(bool rooted)
        {
            _isRooted = rooted;
            if (_state == State.Dead || _state == State.PlayerDead) return;

            if (rooted)
            {
                _agent.isStopped = true;
            }
            else if (_state == State.Chase || _state == State.Idle)
            {
                _agent.isStopped = false;
            }
            // WindUp / Attacking / Staggered — agent is intentionally stopped; do not restart it.
        }

        public void ApplyHitStagger(float durationMultiplier = 1f)
        {
            if (_state == State.Dead || _state == State.Staggered || _state == State.PlayerDead) return;
            _hitStaggerMultiplier = durationMultiplier;
            StopActive();
            StartActive(HitStaggerRoutine());
        }

        public void SetSpeedMultiplier(float multiplier, float duration)
        {
            if (_state == State.Dead || _state == State.PlayerDead) return;
            if (_speedRestoreRoutine != null)
                StopCoroutine(_speedRestoreRoutine);
            _agent.speed = _baseSpeed * multiplier;
            _speedRestoreRoutine = StartCoroutine(RestoreSpeedAfter(duration));
        }

        private IEnumerator RestoreSpeedAfter(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            _agent.speed = _baseSpeed;
            _speedRestoreRoutine = null;
        }

        // ── State update methods ──────────────────────────────────────────────

        private void UpdateIdle()
        {
            if (_isRooted) return;
            // Aggro check — use sqrMagnitude to avoid sqrt.
            float toPlayerSqr = (_player.position - transform.position).sqrMagnitude;
            if (toPlayerSqr <= _chaseRangeSq)
            {
                _agent.speed     = moveSpeed;
                _agent.isStopped = false;
                _state           = State.Chase;
                return;
            }

            // Patrol: walk to target, pick a new one on arrival.
            _pathUpdateTimer -= Time.deltaTime;
            if (_pathUpdateTimer <= 0f)
            {
                _pathUpdateTimer = PathUpdateInterval;
                if (_agent.isOnNavMesh)
                {
                    _agent.speed     = patrolSpeed;
                    _agent.isStopped = false;
                    _agent.SetDestination(_patrolTarget);
                }
            }

            // On arrival at patrol waypoint, pick the next one.
            if ((transform.position - _patrolTarget).sqrMagnitude < 0.25f)
                _patrolTarget = PickPatrolTarget();

            // Rotate toward velocity during patrol.
            if (_agent.velocity.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(_agent.velocity.normalized);
        }

        private void UpdateChase()
        {
            if (_isRooted) return;
            float toPlayerSqr = (_player.position - transform.position).sqrMagnitude;

            // De-aggro — return to patrol.
            if (toPlayerSqr > _chaseRangeSq)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _patrolTarget = PickPatrolTarget();
                _state = State.Idle;
                return;
            }

            // Attack check.
            if (toPlayerSqr <= _attackRangeSq && _attackCooldownTimer <= 0f)
            {
                _attackCooldownTimer = attackCooldown;
                _agent.isStopped     = true;
                StopActive();
                StartActive(AttackRoutine());
                return;
            }

            // Path update throttled to PathUpdateInterval.
            _pathUpdateTimer -= Time.deltaTime;
            if (_pathUpdateTimer <= 0f)
            {
                _pathUpdateTimer = PathUpdateInterval;
                if (_agent.isOnNavMesh)
                {
                    _agent.speed     = moveSpeed;
                    _agent.isStopped = false;
                    _agent.SetDestination(_player.position);
                }
            }

            // Manual rotation — updateRotation is false.
            if (_agent.velocity.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(_agent.velocity.normalized);
        }

        // ── Coroutines ────────────────────────────────────────────────────────

        private IEnumerator AttackRoutine()
        {
            // WindUp: face player, stop, flash, trigger animation.
            _state           = State.WindUp;
            _agent.isStopped = true;

            if (_player != null)
            {
                Vector3 dir = _player.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(dir.normalized);
            }

            _animator?.SetTrigger(AnimAttack);
            SetFlashColor(ColorWindUp);
            // ADR-0003: occlusion-independent overhead telegraph, additive to the flash above.
            // Explicit height — the shared _defaultHeightOffset is WagonWheelRoller-specific
            // per BACKLOG.md B56 and reads too low on this taller model.
            AttackTelegraphService.Show(transform, AttackTelegraphKind.MeleeParryable, windUpDuration, telegraphHeightOffset);
            yield return _waitWindUp;

            // Attacking: check distance and apply damage.
            _state = State.Attacking;
            SetNormalMaterial();

            float distSqr = (_player.position - transform.position).sqrMagnitude;
            if (distSqr <= _attackRangeExtendedSq && _playerCombat != null)
            {
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, attacker: gameObject);

                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);

                if (result == AttackResult.Parried)
                {
                    // Parry triggers a full stagger — break out before cooldown wait.
                    _activeRoutine = null;
                    StartActive(StaggerRoutine());
                    yield break;
                }
            }

            yield return _waitAttackActive;

            _agent.isStopped = false;
            _state           = State.Chase;
        }

        private IEnumerator StaggerRoutine()
        {
            // No StaggerTrigger in AC — state is handled entirely in code.
            _agent.isStopped = true;
            _state           = State.Staggered;
            SetFlashColor(ColorStagger);
            yield return _waitStagger;
            SetNormalMaterial();
            _agent.isStopped = false;
            _state           = State.Chase;
        }

        private IEnumerator HitStaggerRoutine()
        {
            float multiplier      = _hitStaggerMultiplier;
            _hitStaggerMultiplier = 1f;

            _agent.isStopped = true;
            _state           = State.Staggered;
            SetFlashColor(ColorStagger);

            float elapsed  = 0f;
            float duration = hitStaggerDuration * multiplier;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_state == State.Staggered && !_stats.IsDead)
            {
                SetNormalMaterial();
                _agent.isStopped = false;
                _state           = State.Chase;
            }
        }

        private IEnumerator DieRoutine()
        {
            yield return _waitDie;
            Destroy(gameObject);
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void OnHitReceived()
        {
            if (_state == State.Dead || _state == State.Staggered) return;
            Boxhead.Core.AudioManager.Instance?.Play(Boxhead.Core.SoundEvent.EnemyHit);
            StopActive();
            StartActive(HitStaggerRoutine());
        }

        // Called when the player executes a counter strike.
        // target == null means legacy broadcast (pre-targeting API) — apply damage to self.
        // target == gameObject means this grunt was the one being attacked when the parry landed.
        private void OnCounterStrikeLanded(GameObject target)
        {
            if (target != null && target != gameObject) return;
            if (_state == State.Dead) return;
            _stats.TakeDamage(counterStrikeDamage);
        }

        private void HandlePlayerDeath()
        {
            if (_state == State.Dead) return;
            StopActive();
            _state           = State.PlayerDead;
            _attackCooldownTimer = 0f;
            SetNormalMaterial();
            StartCoroutine(StandOverPlayerRoutine());
        }

        private IEnumerator StandOverPlayerRoutine()
        {
            if (_player == null || !_agent.isOnNavMesh) yield break;

            // Walk to the player's body
            _agent.isStopped = false;
            _agent.speed     = patrolSpeed;
            _agent.SetDestination(_player.position);

            float sqStandDist = 1.5f * 1.5f;
            while (_player != null &&
                   (_player.position - transform.position).sqrMagnitude > sqStandDist)
            {
                if (_agent.isOnNavMesh)
                    _agent.SetDestination(_player.position);
                yield return null;
            }

            // Arrived — stop and face the body
            _agent.isStopped = true;
            _agent.ResetPath();

            if (_player != null)
            {
                Vector3 dir = _player.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(dir.normalized);
            }

            _animator?.SetFloat(AnimSpeed, 0f);
        }

        private void HandleDeath()
        {
            _state = State.Dead;
            StopActive();

            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }
            _agent.enabled = false;

            SetNormalMaterial();
            _animator?.SetTrigger(AnimDeath);
            StartCoroutine(DieRoutine());
        }

        // ── Material helpers ──────────────────────────────────────────────────

        // Assigns the single cached flash instance with a new colour and swaps it in.
        private void SetFlashColor(Color color)
        {
            if (_renderer == null || _flashMaterialInstance == null) return;
            _flashMaterialInstance.color = color;
            _renderer.sharedMaterial     = _flashMaterialInstance;
        }

        // Restores the original shared material (no instance involved).
        private void SetNormalMaterial()
        {
            if (_renderer == null || _normalMaterial == null) return;
            _renderer.sharedMaterial = _normalMaterial;
        }

        // ── Routine helpers ───────────────────────────────────────────────────

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

        // ── Patrol ────────────────────────────────────────────────────────────

        private Vector3 PickPatrolTarget()
        {
            Vector3 candidate = _spawnPosition + new Vector3(
                Random.Range(-patrolRadius, patrolRadius),
                0f,
                Random.Range(-patrolRadius, patrolRadius));

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, 2f, NavMesh.AllAreas))
                return hit.position;

            return _spawnPosition;
        }

        // ── Editor gizmos ─────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 pos = Application.isPlaying ? _spawnPosition : transform.position;

            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.08f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, chaseRange);
            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 1f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, chaseRange);

            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.08f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, attackRange);
            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 1f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, attackRange);

            UnityEditor.Handles.color = new Color(0f, 0.6f, 1f, 0.08f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, patrolRadius);
            UnityEditor.Handles.color = new Color(0f, 0.6f, 1f, 1f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, patrolRadius);
        }
#endif
    }
}
