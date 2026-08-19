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
    [RequireComponent(typeof(Animator))]
    public class HitchingHoundAI : MonoBehaviour
    {
        private enum HoundState { Idle, Chase, Lunge, SnapBack, Stunned, Dead }

        [Header("Detection")]
        [SerializeField] private float _aggroRange   = 7f;
        [SerializeField] private float _deaggroRange = 10f;
        [SerializeField] private float _lungeRange   = 2.5f;

        [Header("Chain")]
        [Tooltip("How far from the spawn point the dog can travel before the chain yanks it back.")]
        [SerializeField] private float _chainRadius = 5f;

        [Header("Movement")]
        [SerializeField] private float _patrolSpeed = 1.5f;
        [SerializeField] private float _chaseSpeed  = 4.5f;
        [SerializeField] private float _turnSpeed   = 240f;

        [Header("Lunge")]
        [SerializeField] private float     _lungeSpeed       = 10f;
        [SerializeField] private float     _lungeDuration    = 0.7f;
        [SerializeField] private float     _contactRadius    = 0.8f;
        [SerializeField] private int       _lungeDamage      = 10;
        [SerializeField] private float     _lungeCooldown    = 2f;
        [SerializeField] private LayerMask _playerLayer      = ~0;

        [Header("Snap-Back")]
        [SerializeField] private float _snapBackSpeed    = 9f;
        [SerializeField] private float _snapBackArrival  = 0.5f;

        [Header("Stun")]
        [SerializeField] private float _stunnedDuration         = 2f;
        [SerializeField] private float _stunnedDamageMultiplier = 1.5f;

        [Header("Visuals")]
        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private Material _stunnedMaterial;
        [SerializeField] private Material _deadMaterial;

        // ── Public events ─────────────────────────────────────────────────────

        public event Action OnLunge;
        public event Action OnSnapBack;
        public event Action OnStunned;

        // ── State ─────────────────────────────────────────────────────────────

        private HoundState _state = HoundState.Idle;

        // ── Runtime references ────────────────────────────────────────────────

        private EnemyStats       _stats;
        private Rigidbody        _rb;
        private NavMeshAgent     _agent;
        private Animator         _animator;
        private Transform        _player;
        private CombatController _playerCombat;
        private Coroutine        _stateRoutine;

        // Animator parameter hashes — cached to avoid string allocation each frame
        private static readonly int HashSpeed   = Animator.StringToHash("Speed");
        private static readonly int HashAttack  = Animator.StringToHash("AttackTrigger");
        private static readonly int HashStagger = Animator.StringToHash("StaggerTrigger");
        private static readonly int HashIsDead  = Animator.StringToHash("IsDead");

        // ── Cached data ───────────────────────────────────────────────────────

        private Material _normalMaterial;
        private Material _stunnedMaterialInstance;
        private Material _deadMaterialInstance;

        private Vector3 _spawnPosition;
        private Vector3 _patrolTarget;
        private Vector3 _lungeDirection;

        private float _aggroRangeSq;
        private float _deaggroRangeSq;
        private float _lungeRangeSq;
        private float _chainRadiusSq;

        private int  _prevHealth;
        private bool _processingHit;
        private bool _isStunned;
        private bool _hasHitPlayerThisLunge;

        private WaitForSeconds _waitStunned;
        private WaitForSeconds _waitLunge;

        private float _pathUpdateTimer;
        private float _nextLungeTime;
        private const float PathUpdateInterval = 0.25f;

        private const float PatrolRadius = 2.5f;

        private readonly Collider[] _hitBuffer = new Collider[4];

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            _rb    = GetComponent<Rigidbody>();

            _rb.constraints   = RigidbodyConstraints.FreezePositionY
                              | RigidbodyConstraints.FreezeRotationX
                              | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.isKinematic   = false;

            _aggroRangeSq   = _aggroRange   * _aggroRange;
            _deaggroRangeSq = _deaggroRange * _deaggroRange;
            _lungeRangeSq   = _lungeRange   * _lungeRange;
            _chainRadiusSq  = _chainRadius  * _chainRadius;

            _spawnPosition = transform.position;
            _patrolTarget  = PickPatrolTarget();
            _waitStunned   = new WaitForSeconds(_stunnedDuration);
            _waitLunge     = new WaitForSeconds(_lungeDuration);

            _agent = GetComponent<NavMeshAgent>();
            _agent.speed            = _chaseSpeed;
            _agent.stoppingDistance = _lungeRange - 0.1f;
            _agent.updateRotation   = false;
            _agent.isStopped        = true;

            _animator = GetComponent<Animator>();

            if (_bodyRenderer != null)
            {
                _normalMaterial = _bodyRenderer.sharedMaterial;
                if (_normalMaterial == null && _bodyRenderer.sharedMaterials.Length > 0)
                    _normalMaterial = _bodyRenderer.sharedMaterials[0];
                if (_normalMaterial == null)
                    Debug.LogError("[HitchingHoundAI] _bodyRenderer has no sharedMaterial — grey mesh during lunge will occur.", this);
                if (_stunnedMaterial != null)
                    _stunnedMaterialInstance = new Material(_stunnedMaterial);
                if (_deadMaterial != null)
                    _deadMaterialInstance = new Material(_deadMaterial);
            }
            else
            {
                Debug.LogError("[HitchingHoundAI] _bodyRenderer is not assigned — material changes will be silently skipped.", this);
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
                Debug.LogWarning("[HitchingHoundAI] No GameObject tagged 'Player' found.", this);
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

            if (_bodyRenderer != null && _normalMaterial != null)
                _bodyRenderer.sharedMaterial = _normalMaterial;

            if (_stunnedMaterialInstance != null) Destroy(_stunnedMaterialInstance);
            if (_deadMaterialInstance    != null) Destroy(_deadMaterialInstance);
        }

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (_state == HoundState.Dead) return;

            switch (_state)
            {
                case HoundState.Idle:
                    UpdateIdle();
                    break;
                case HoundState.Chase:
                    UpdateChase();
                    break;
                case HoundState.Lunge:
                    UpdateLunge();
                    break;
                // SnapBack and Stunned are fully coroutine-driven — no Update work.
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
            if (_player == null) { EnterIdle(); return; }

            float toPlayerSqr      = (_player.position - transform.position).sqrMagnitude;
            float distFromSpawnSqr = (transform.position - _spawnPosition).sqrMagnitude;

            if (toPlayerSqr > _deaggroRangeSq)     { EnterIdle();     return; }
            if (distFromSpawnSqr >= _chainRadiusSq) { EnterSnapBack(); return; }
            if (toPlayerSqr <= _lungeRangeSq && Time.time >= _nextLungeTime) { EnterLunge(); return; }

            _pathUpdateTimer -= Time.deltaTime;
            if (_pathUpdateTimer <= 0f)
            {
                _pathUpdateTimer = PathUpdateInterval;
                if (_agent.isOnNavMesh)
                {
                    Vector3 clampedTarget = _spawnPosition + Vector3.ClampMagnitude(
                        _player.position - _spawnPosition, _chainRadius * 0.9f);
                    _agent.SetDestination(clampedTarget);
                }
            }
            if (_agent.velocity.sqrMagnitude > 0.01f)
                FaceDirection(_agent.velocity.normalized);
        }

        private void UpdateLunge()
        {
            _rb.linearVelocity = _lungeDirection * _lungeSpeed;
            FaceDirection(_lungeDirection);

            // Chain snaps the lunge — dog is yanked back mid-leap
            if ((transform.position - _spawnPosition).sqrMagnitude >= _chainRadiusSq)
            {
                EnterSnapBack();
                return;
            }

            if (_hasHitPlayerThisLunge) return;

            int count = Physics.OverlapSphereNonAlloc(transform.position, _contactRadius, _hitBuffer, _playerLayer);
            for (int i = 0; i < count; i++)
            {
                if (!_hitBuffer[i].CompareTag("Player")) continue;

                _hasHitPlayerThisLunge = true;
                _nextLungeTime = Time.time + _lungeCooldown;
                if (_playerCombat != null)
                    _playerCombat.TryReceiveAttack(_lungeDamage, attacker: gameObject);

                // Successful bite — dog circles back rather than stumbling
                EnterChase();
                return;
            }
        }

        // ── State transitions ─────────────────────────────────────────────────

        private void EnterIdle()
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            _rb.isKinematic = false;
            StopStateRoutine();
            _isStunned = false;
            _state     = HoundState.Idle;
            _rb.linearVelocity = Vector3.zero;
            _patrolTarget = PickPatrolTarget();
            SetMaterial(_normalMaterial);
            _animator.SetFloat(HashSpeed, 0f);
        }

        private void EnterChase()
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.isKinematic    = true;
            _agent.Warp(transform.position);
            _agent.isStopped   = false;
            StopStateRoutine();
            _isStunned = false;
            _state     = HoundState.Chase;
            SetMaterial(_normalMaterial);
            _animator.SetFloat(HashSpeed, 1f);
        }

        private void EnterLunge()
        {
            if (_player == null) { EnterChase(); return; }

            _agent.isStopped = true;
            _agent.ResetPath();
            _rb.isKinematic  = false;
            StopStateRoutine();
            _hasHitPlayerThisLunge = false;

            Vector3 dir = _player.position - transform.position;
            dir.y = 0f;
            _lungeDirection = dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward;

            // Snap rotation immediately so the dog faces the player at lunge start.
            // Without this, the smooth turn-speed causes the model to visually rotate
            // during flight instead of lunging head-first.
            // Note: Model faces backward in Blender (180° Z from import), so negate direction
            if (_lungeDirection.sqrMagnitude > 0.001f)
                _rb.MoveRotation(Quaternion.LookRotation(-_lungeDirection));

            _state        = HoundState.Lunge;
            _stateRoutine = StartCoroutine(LungeDurationRoutine());
            _animator.SetFloat(HashSpeed, 0f);
            _animator.SetTrigger(HashAttack);

            // ADR-0003: the Hound has no separate stationary wind-up — the lunge itself is the
            // tell, so raise the telegraph for the lunge's own travel time. Parryable today
            // (TryReceiveAttack in UpdateLunge does not override the default).
            Boxhead.Core.AttackTelegraphService.Show(
                transform, Boxhead.Core.AttackTelegraphKind.MeleeParryable, _lungeDuration);

            OnLunge?.Invoke();
        }

        private void EnterSnapBack()
        {
            _agent.isStopped = true;
            _rb.isKinematic  = false;
            StopStateRoutine();
            _state = HoundState.SnapBack;
            _rb.linearVelocity = Vector3.zero;
            _stateRoutine = StartCoroutine(SnapBackRoutine());
            _animator.SetFloat(HashSpeed, 1f);
            OnSnapBack?.Invoke();
        }

        private void EnterStunned()
        {
            StopStateRoutine();
            _isStunned = true;
            _state     = HoundState.Stunned;
            _rb.linearVelocity = Vector3.zero;
            _rb.isKinematic    = true;
            _agent.isStopped   = true;
            SetMaterial(_stunnedMaterialInstance);
            _animator.SetFloat(HashSpeed, 0f);
            _animator.SetTrigger(HashStagger);
            OnStunned?.Invoke();
            _stateRoutine = StartCoroutine(StunnedRoutine());
        }

        private void EnterDead()
        {
            StopStateRoutine();
            _state = HoundState.Dead;
            _rb.linearVelocity = Vector3.zero;
            _rb.isKinematic    = true;
            _agent.isStopped   = true;
            // Restore to normal first so a parry flash or stunned material can never leave a grey mesh
            // if death fires in the same frame as a material transition.
            SetMaterial(_normalMaterial);
            SetMaterial(_deadMaterialInstance);
            _animator.SetFloat(HashSpeed, 0f);
            _animator.SetBool(HashIsDead, true);
        }

        // ── Coroutines ────────────────────────────────────────────────────────

        private IEnumerator LungeDurationRoutine()
        {
            yield return _waitLunge;
            // Timed out without hitting player or chain — dog stumbles
            if (_state == HoundState.Lunge)
                EnterStunned();
        }

        private IEnumerator SnapBackRoutine()
        {
            float timeout = 5f;
            float elapsed = 0f;
            while (elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                Vector3 toSpawn = _spawnPosition - transform.position;
                toSpawn.y = 0f;

                if (toSpawn.sqrMagnitude <= _snapBackArrival * _snapBackArrival)
                {
                    transform.position = new Vector3(_spawnPosition.x, transform.position.y, _spawnPosition.z);
                    _rb.linearVelocity = Vector3.zero;
                    break;
                }

                Vector3 dir = toSpawn.normalized;
                _rb.linearVelocity = dir * _snapBackSpeed;
                FaceDirection(dir);
                yield return null;
            }

            EnterStunned();
        }

        private IEnumerator StunnedRoutine()
        {
            yield return _waitStunned;
            if (_state == HoundState.Stunned)
                EnterChase();
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
            FaceDirection(dir);
            _rb.linearVelocity = dir * speed;
        }

        private void FaceDirection(Vector3 dir)
        {
            if (dir.sqrMagnitude < 0.001f) return;
            // Model faces backward (180° Z from Blender import), so negate direction
            Quaternion target = Quaternion.LookRotation(-dir);
            _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, target, _turnSpeed * Time.deltaTime));
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void HandleHit()
        {
            if (_state == HoundState.Dead) return;
            if (_processingHit) return;

            _processingHit = true;
            try
            {
                int damageDealt = _prevHealth - _stats.CurrentHealth;
                if (damageDealt > 0 && _isStunned && !_stats.IsDead)
                {
                    int bonus = Mathf.RoundToInt(damageDealt * (_stunnedDamageMultiplier - 1f));
                    if (bonus > 0) _stats.TakeDamage(bonus);
                }
                if (_state != HoundState.Dead)
                    _prevHealth = _stats.CurrentHealth;
            }
            finally
            {
                _processingHit = false;
            }
        }

        private void HandleDeath()
        {
            if (_state == HoundState.Dead) return;
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
            Vector3 pos = Application.isPlaying ? _spawnPosition : transform.position;

            UnityEditor.Handles.color = new Color(0.8f, 0.4f, 0f, 0.15f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, _chainRadius);
            UnityEditor.Handles.color = new Color(0.8f, 0.4f, 0f, 1f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, _chainRadius);

            UnityEditor.Handles.color = new Color(1f, 0.8f, 0f, 0.08f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, _aggroRange);
            UnityEditor.Handles.color = new Color(1f, 0.8f, 0f, 1f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, _aggroRange);

            UnityEditor.Handles.color = new Color(1f, 0.1f, 0.1f, 0.1f);
            UnityEditor.Handles.DrawSolidDisc(pos, Vector3.up, _lungeRange);
            UnityEditor.Handles.color = new Color(1f, 0.1f, 0.1f, 1f);
            UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, _lungeRange);
        }
#endif
    }
}
