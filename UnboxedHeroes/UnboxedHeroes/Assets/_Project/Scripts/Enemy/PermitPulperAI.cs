using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(EnemyStats))]
    public class PermitPulperAI : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float _detectionRadius = 10f;
        [SerializeField] private float _attackRadius = 2.0f;

        [Header("Movement")]
        [SerializeField] private float _patrolSpeed = 1.2f;
        [SerializeField] private float _chaseSpeed = 2.4f;
        [SerializeField] private float _rotateSpeed = 150f;
        [SerializeField] private float _patrolRadius = 3.5f;

        [Header("Attack")]
        [SerializeField] private float _attackCooldown = 2.5f;
        [SerializeField] private int _attackDamage = 20;

        private enum State { Patrol, Chase, Attack, Dead }

        private CharacterController _controller;
        private EnemyStats _stats;
        private Animator _animator;

        private State _state = State.Patrol;
        private Transform _player;
        private Vector3 _patrolCenter;
        private float _patrolAngle;
        private float _attackTimer;

        private static readonly int SpeedHash   = Animator.StringToHash("Speed");
        private static readonly int AttackHash  = Animator.StringToHash("AttackTrigger");
        private static readonly int DeadHash    = Animator.StringToHash("IsDead");

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stats      = GetComponent<EnemyStats>();
            _animator   = GetComponentInChildren<Animator>();
            _patrolCenter = transform.position;
        }

        private void Start()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null) _player = playerGO.transform;
            _stats.OnDeath += OnDeath;
        }

        private void OnDestroy()
        {
            if (_stats != null) _stats.OnDeath -= OnDeath;
        }

        private void Update()
        {
            if (_state == State.Dead) return;

            float dist = _player != null
                ? Vector3.Distance(transform.position, _player.position)
                : float.MaxValue;

            switch (_state)
            {
                case State.Patrol:
                    if (dist <= _detectionRadius) _state = State.Chase;
                    break;
                case State.Chase:
                    if (dist > _detectionRadius * 1.5f) _state = State.Patrol;
                    else if (dist <= _attackRadius)     _state = State.Attack;
                    break;
                case State.Attack:
                    if (dist > _attackRadius * 1.5f) _state = State.Chase;
                    break;
            }

            switch (_state)
            {
                case State.Patrol: DoPatrol(); break;
                case State.Chase:  DoChase();  break;
                case State.Attack: DoAttack(); break;
            }

            if (!_controller.isGrounded)
                _controller.Move(new Vector3(0f, -9.81f * Time.deltaTime, 0f));
        }

        private void DoPatrol()
        {
            _patrolAngle += _patrolSpeed / _patrolRadius * Time.deltaTime * Mathf.Rad2Deg;
            if (_patrolAngle >= 360f) _patrolAngle -= 360f;

            float rad = _patrolAngle * Mathf.Deg2Rad;
            Vector3 target = _patrolCenter + new Vector3(
                Mathf.Sin(rad) * _patrolRadius, 0f,
                Mathf.Cos(rad) * _patrolRadius);

            MoveToward(target, _patrolSpeed);
            SetAnimSpeed(1f);
        }

        private void DoChase()
        {
            if (_player == null) return;
            MoveToward(_player.position, _chaseSpeed);
            SetAnimSpeed(1f);
        }

        private void DoAttack()
        {
            FacePlayer();
            SetAnimSpeed(0f);

            _attackTimer -= Time.deltaTime;
            if (_attackTimer > 0f) return;
            _attackTimer = _attackCooldown;

            if (_animator != null) _animator.SetTrigger(AttackHash);

            if (_player != null)
            {
                var combat = _player.GetComponent<CombatController>();
                if (combat != null)
                    combat.TryReceiveAttack(_attackDamage, true, gameObject);
                else
                    _player.GetComponent<PlayerStats>()?.TakeDamage(_attackDamage);
            }
        }

        private void MoveToward(Vector3 target, float speed)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(dir.normalized),
                    _rotateSpeed * Time.deltaTime);

            _controller.Move(dir.normalized * speed * Time.deltaTime);
        }

        private void FacePlayer()
        {
            if (_player == null) return;
            Vector3 dir = (_player.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(dir.normalized),
                    _rotateSpeed * Time.deltaTime);
        }

        private void SetAnimSpeed(float speed)
        {
            if (_animator != null) _animator.SetFloat(SpeedHash, speed);
        }

        private void OnDeath()
        {
            _state = State.Dead;
            if (_animator != null) _animator.SetBool(DeadHash, true);
            _controller.enabled = false;
        }

        // Silent receivers for RPG Mecanim footstep animation events
        private void FootR() { }
        private void FootL() { }
        private void Hit()   { }
    }
}
