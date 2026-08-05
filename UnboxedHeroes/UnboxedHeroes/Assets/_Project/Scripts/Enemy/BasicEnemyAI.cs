using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Boxhead.Core;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class BasicEnemyAI : MonoBehaviour, IEnemyBehavior
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float chaseRange = 8f;
        [SerializeField] private float attackRange = 1.5f;

        [Header("Attack")]
        [SerializeField] private float windUpDuration = 0.8f;
        [SerializeField] private float attackActiveDuration = 0.3f;
        [SerializeField] private float attackCooldown = 2f;

        [Header("Stagger")]
        [SerializeField] private float staggerDuration = 2f;
        [SerializeField] private float hitStaggerDuration = 0.45f;

        [Header("Counter Strike")]
        [SerializeField] private int counterStrikeDamage = 30;

        private enum State { Idle, Chase, WindUp, Attacking, Staggered, Dead }
        private State _state = State.Idle;

        private Transform _player;
        private CombatController _playerCombat;
        private EnemyStats _stats;
        private NavMeshAgent _agent;
        private Material _material;
        private Color _baseColor;
        private float _attackCooldownTimer;
        private float _pathUpdateTimer;
        private const float PathUpdateInterval = 0.25f;
        private Coroutine _activeRoutine;
        private Animator _animator;

        private static readonly int AnimAttack  = Animator.StringToHash("AttackTrigger");
        private static readonly int AnimStagger = Animator.StringToHash("StaggerTrigger");
        private static readonly int AnimDeath   = Animator.StringToHash("DeathTrigger");
        private static readonly int AnimSpeed   = Animator.StringToHash("Speed");

        private bool  _isRooted;
        private float _hitStaggerMultiplier = 1f;
        private float _baseSpeed;
        private Coroutine _speedRestoreRoutine;

        // Cached allocations
        private WaitForSeconds _waitWindUp;
        private WaitForSeconds _waitAttackActive;
        private WaitForSeconds _waitStagger;
        private WaitForSeconds _waitDie;

        private void Awake()
        {
            _stats    = GetComponent<EnemyStats>();
            _animator = GetComponentInChildren<Animator>();

            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                _material = renderer.material;
                _baseColor = _material.color;
            }

            _agent = GetComponent<NavMeshAgent>();
            _agent.speed           = moveSpeed;
            _agent.stoppingDistance = attackRange - 0.1f;
            _agent.updateRotation  = false;
            _agent.isStopped       = true;
            _baseSpeed = moveSpeed;

            _waitWindUp       = new WaitForSeconds(windUpDuration);
            _waitAttackActive = new WaitForSeconds(attackActiveDuration);
            _waitStagger      = new WaitForSeconds(staggerDuration);
            _waitDie          = new WaitForSeconds(0.5f);
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _player = playerObj.transform;
                _playerCombat = playerObj.GetComponent<CombatController>();

                if (_playerCombat != null)
                    _playerCombat.OnCounterStrike += OnCounterStrikeLanded;
            }

            _stats.OnDeath += HandleDeath;
            _stats.OnHit   += OnHitReceived;
        }

        private void Update()
        {
            if (_state == State.Dead) return;
            if (_player == null) return;

            if (_attackCooldownTimer > 0f)
                _attackCooldownTimer -= Time.deltaTime;

            switch (_state)
            {
                case State.Idle:
                    CheckForPlayer();
                    break;
                case State.Chase:
                    ChasePlayer();
                    break;
            }

            _animator?.SetFloat(AnimSpeed, _agent.velocity.magnitude);
        }

        public void SetRooted(bool rooted)
        {
            _isRooted = rooted;
            if (_state == State.Dead) return;

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
            if (_state == State.Dead || _state == State.Staggered) return;
            _hitStaggerMultiplier = durationMultiplier;
            StopActive();
            StartActive(HitStaggerRoutine());
        }

        public void SetSpeedMultiplier(float multiplier, float duration)
        {
            if (_state == State.Dead) return;
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

        private void CheckForPlayer()
        {
            if (_isRooted) return;
            if (Vector3.Distance(transform.position, _player.position) <= chaseRange)
            {
                _state = State.Chase;
                _agent.isStopped = false;
            }
        }

        private void ChasePlayer()
        {
            if (_isRooted) return;
            float dist = Vector3.Distance(transform.position, _player.position);

            if (dist > chaseRange)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _state = State.Idle;
                return;
            }

            // Player is elevated (on treehouse deck) — enemy can't follow
            if (_player.position.y - transform.position.y > 1.5f)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _state = State.Idle;
                return;
            }

            if (dist <= attackRange && _attackCooldownTimer <= 0f)
            {
                _attackCooldownTimer = attackCooldown;
                _agent.isStopped = true;
                StopActive();
                StartActive(AttackRoutine());
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
                transform.rotation = Quaternion.LookRotation(-_agent.velocity.normalized);
        }

        private IEnumerator AttackRoutine()
        {
            _agent.isStopped = true;
            _state = State.WindUp;
            _animator?.SetTrigger(AnimAttack);
            SetColor(Color.red);
            yield return _waitWindUp;

            _state = State.Attacking;
            SetColor(_baseColor);

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= attackRange + 0.5f && _playerCombat != null)
            {
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, attacker: gameObject);

                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);

                if (result == AttackResult.Parried)
                {
                    _activeRoutine = null;
                    StartActive(StaggerRoutine());
                    yield break;
                }
            }

            yield return _waitAttackActive;
            _agent.isStopped = false;
            _state = State.Chase;
        }

        private IEnumerator StaggerRoutine()
        {
            _agent.isStopped = true;
            _state = State.Staggered;
            _animator?.SetTrigger(AnimStagger);
            SetColor(Color.yellow);
            yield return _waitStagger;
            SetColor(_baseColor);
            _agent.isStopped = false;
            _state = State.Chase;
        }

        private void OnHitReceived()
        {
            if (_state == State.Dead || _state == State.Staggered) return;
            Boxhead.Core.AudioManager.Instance?.Play(Boxhead.Core.SoundEvent.EnemyHit);
            StopActive();
            StartActive(HitStaggerRoutine());
        }

        private IEnumerator HitStaggerRoutine()
        {
            float multiplier  = _hitStaggerMultiplier;
            _hitStaggerMultiplier = 1f;

            _agent.isStopped = true;
            _state = State.Staggered;
            _animator?.SetTrigger(AnimStagger);

            float elapsed  = 0f;
            float duration = hitStaggerDuration * multiplier;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_state == State.Staggered && !_stats.IsDead)
            {
                _agent.isStopped = false;
                _state = State.Chase;
            }
        }

        // Called when the player executes a counter strike during the Counter Window
        private void OnCounterStrikeLanded(GameObject target)
        {
            if (target != null && target != gameObject) return;
            if (_state == State.Dead) return;
            _stats.TakeDamage(counterStrikeDamage);
        }

        private void HandleDeath()
        {
            _state = State.Dead;
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.enabled = false;
            StopActive();
            SetColor(Color.gray);
            _animator?.SetTrigger(AnimDeath);
            StartCoroutine(DieRoutine());
        }

        private IEnumerator DieRoutine()
        {
            yield return _waitDie;
            Destroy(gameObject);
        }

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
            StopActive();
            if (_speedRestoreRoutine != null)
                StopCoroutine(_speedRestoreRoutine);
            if (_playerCombat != null)
                _playerCombat.OnCounterStrike -= OnCounterStrikeLanded;
            if (_stats != null)
            {
                _stats.OnDeath -= HandleDeath;
                _stats.OnHit   -= OnHitReceived;
            }
            if (_material != null)
                Destroy(_material);
        }
    }
}
