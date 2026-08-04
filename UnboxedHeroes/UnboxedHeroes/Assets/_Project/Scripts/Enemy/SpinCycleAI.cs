using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Boxhead.Core;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class SpinCycleAI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Movement")]
        [SerializeField] private float walkSpeed          = 2f;
        [SerializeField] private float runSpeed           = 4f;
        [SerializeField] private float chaseRange         = 12f;
        [SerializeField] private float meleeRange         = 4f;
        [SerializeField] private float rangedRange        = 8f;

        [Header("Attack Timing")]
        [SerializeField] private float windUpDuration        = 1.0f;
        [SerializeField] private float attackActiveDuration  = 0.4f;
        [SerializeField] private float attackCooldown        = 2.5f;
        [SerializeField] private float phase2CooldownMult    = 0.7f;

        [Header("Stagger")]
        [SerializeField] private float staggerDuration = 2.5f;

        [Header("SpinCharge")]
        [SerializeField] private float spinChargeSpeed    = 8f;
        [SerializeField] private float spinChargeDuration = 0.6f;

        [Header("JumpBack")]
        [SerializeField] private float jumpBackDistance = 4f;
        [SerializeField] private float jumpBackHeight   = 1.8f;
        [SerializeField] private float jumpBackDuration = 0.45f;

        [Header("JumpCharge")]
        [SerializeField] private float jumpChargeDuration = 0.8f;
        [SerializeField] private float jumpChargeHeight   = 2f;

        [Header("FullSpin")]
        [SerializeField] private float fullSpinRadius = 3f;

        [Header("Phase Transition")]
        [SerializeField] private float phaseTransitionPause = 1.5f;

        [Header("Defeat")]
        [SerializeField] private float defeatHoldDuration = 2.5f;

        [Header("Counter Strike")]
        [SerializeField] private int counterStrikeDamage = 40;

        [Header("Projectiles")]
        [SerializeField] private GameObject _clothesTossPrefab;
        [SerializeField] private GameObject _sudsBlobPrefab;
        [SerializeField] private float _clothesTossSpeed      = 8f;
        [SerializeField] private float _sudsBlobSpeed         = 6f;
        [SerializeField] private float _sudsBlobArcVelocity   = 1.5f;

        [Header("Held Props")]
        [Tooltip("Visual-only ClothesBall parented to the left hand hold point.")]
        [SerializeField] private GameObject _heldClothesBall;
        [Tooltip("Visual-only SudsBlob parented to the right hand hold point.")]
        [SerializeField] private GameObject _heldSudsBlob;

        [Header("Screen Shake")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Imagination Restore")]
        [SerializeField] private Volume _imaginationVolume;
        [SerializeField] private float _imaginationLerpDuration = 1.5f;

        [Header("References")]
        [SerializeField] private DrumWindowRotator drumWindow;

        // ── State ─────────────────────────────────────────────────────────────

        private enum BossState
        {
            Idle, Approaching, WindUp, Attacking, Staggered, PhaseTransition, Dead
        }

        private enum Phase { One, Two }

        private BossState _state = BossState.Idle;
        private Phase _phase = Phase.One;

        // Attack pools cycle in order; index wraps on each completion.
        private int _attackIndex;
        private bool _phaseTransitioned;
        private float _attackCooldownTimer;

        // ── References ────────────────────────────────────────────────────────

        private Transform _player;
        private CombatController _playerCombat;
        private EnemyStats _stats;
        private Animator _animator;
        private Material _material;
        private Color _baseColor;
        private Coroutine _activeRoutine;
        private Coroutine _defeatRoutine;

        // Pre-allocated overlap buffer — FullSpin hits up to 4 colliders.
        private readonly Collider[] _overlapBuffer = new Collider[4];

        // ── Animator param hashes ─────────────────────────────────────────────

        private static readonly int AnimSpeed   = Animator.StringToHash("Speed");
        private static readonly int AnimAttack  = Animator.StringToHash("AttackTrigger");
        private static readonly int AnimStagger = Animator.StringToHash("StaggerTrigger");
        private static readonly int AnimIsDead  = Animator.StringToHash("IsDead");

        // Suds burst fires 3 blobs: center, left -25°, right +25°.
        private static readonly float[] _sudsBurstAngles = { 0f, -25f, 25f };

        // ── NavMeshAgent ──────────────────────────────────────────────────────

        private NavMeshAgent _agent;
        private static readonly float PathUpdateInterval = 0.25f;
        private float _pathUpdateTimer;

        // ── Cached yields ─────────────────────────────────────────────────────

        private WaitForSeconds _waitWindUp;
        private WaitForSeconds _waitAttackActive;
        private WaitForSeconds _waitStagger;
        private WaitForSeconds _waitPhaseTransition;
        private WaitForSeconds _waitDefeatHold;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _stats    = GetComponent<EnemyStats>();
            _animator = GetComponentInChildren<Animator>();

            var rend = GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                _material  = rend.material;
                _baseColor = _material.color;
            }

            _waitWindUp          = new WaitForSeconds(windUpDuration);
            _waitAttackActive    = new WaitForSeconds(attackActiveDuration);
            _waitStagger         = new WaitForSeconds(staggerDuration);
            _waitPhaseTransition = new WaitForSeconds(phaseTransitionPause);
            _waitDefeatHold      = new WaitForSeconds(defeatHoldDuration);

            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.speed            = walkSpeed;
                _agent.stoppingDistance = meleeRange - 0.5f;
                _agent.updateRotation   = false;
                _agent.isStopped        = true;
            }

            // Bump amplitude so shake is noticeable on mobile
            if (_impulseSource != null)
                _impulseSource.ImpulseDefinition.AmplitudeGain = 3f;
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
                    if (Vector3.Distance(transform.position, _player.position) <= chaseRange)
                        _state = BossState.Approaching;
                    break;

                case BossState.Approaching:
                    Approach();
                    break;
            }

            float speed = _state == BossState.Approaching
                ? (_agent != null ? _agent.velocity.magnitude : (_phase == Phase.One ? walkSpeed : runSpeed))
                : 0f;
            _animator?.SetFloat(AnimSpeed, speed);
        }

        // ── Movement ──────────────────────────────────────────────────────────

        // ClothesToss (Phase 1, slot 3) and SudsBurst (Phase 2, slot 1) fire at range.
        private bool NextAttackIsRanged()
        {
            if (_phase == Phase.One) return (_attackIndex % 4) == 3;
            return (_attackIndex % 4) == 1;
        }

        private void Approach()
        {
            float dist = Vector3.Distance(transform.position, _player.position);

            if (dist > chaseRange)
            {
                if (_agent != null) { _agent.isStopped = true; _agent.ResetPath(); }
                _state = BossState.Idle;
                return;
            }

            float attackRange = NextAttackIsRanged() ? rangedRange : meleeRange;
            if (dist <= attackRange && _attackCooldownTimer <= 0f)
            {
                float cooldown = _phase == Phase.One
                    ? attackCooldown
                    : attackCooldown * phase2CooldownMult;
                _attackCooldownTimer = cooldown;
                if (_agent != null) { _agent.isStopped = true; }
                StopActive();
                StartActive(AttackRoutine());
                return;
            }

            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.speed     = _phase == Phase.One ? walkSpeed : runSpeed;
                _agent.isStopped = false;

                _pathUpdateTimer -= Time.deltaTime;
                if (_pathUpdateTimer <= 0f)
                {
                    _pathUpdateTimer = PathUpdateInterval;
                    _agent.SetDestination(_player.position);
                }

                if (_agent.velocity.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(_agent.velocity.normalized);
            }
            else
            {
                // Fallback: direct movement when no NavMeshAgent OR agent not yet on NavMesh.
                // Attempt a Warp each frame until the agent snaps to the mesh.
                if (_agent != null && !_agent.isOnNavMesh)
                    _agent.Warp(transform.position);

                Vector3 dir = (_player.position - transform.position).normalized;
                dir.y = 0f;
                float speed = _phase == Phase.One ? walkSpeed : runSpeed;
                transform.position += dir * speed * Time.deltaTime;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        // ── Attack dispatch ────────────────────────────────────────────────────

        private IEnumerator AttackRoutine()
        {
            if (_phase == Phase.One)
                yield return StartCoroutine(Phase1Attack());
            else
                yield return StartCoroutine(Phase2Attack());
        }

        // Phase 1 pool: DrumSlam → Haymaker → SpinCharge → ClothesToss (repeating)
        private IEnumerator Phase1Attack()
        {
            switch (_attackIndex % 4)
            {
                case 0: yield return StartCoroutine(DrumSlam());       break;
                case 1: yield return StartCoroutine(Haymaker());       break;
                case 2: yield return StartCoroutine(SpinCharge());     break;
                case 3: yield return StartCoroutine(ClothesToss());    break;
            }
            _attackIndex++;
            drumWindow?.EndPendulum();

            // Check for phase transition after each attack completes.
            if (!_phaseTransitioned && _stats.CurrentHealth <= _stats.MaxHealth * 0.5f)
                yield return StartCoroutine(PhaseTransitionRoutine());

            if (_state != BossState.Dead && _state != BossState.Staggered)
                _state = BossState.Approaching;
        }

        // Phase 2 pool: FullSpin → SudsBurst → DoubleHaymaker → JumpCharge (repeating)
        private IEnumerator Phase2Attack()
        {
            switch (_attackIndex % 4)
            {
                case 0: yield return StartCoroutine(FullSpin());          break;
                case 1: yield return StartCoroutine(SudsBurst());         break;
                case 2: yield return StartCoroutine(DoubleHaymaker());    break;
                case 3: yield return StartCoroutine(JumpCharge());        break;
            }
            _attackIndex++;
            drumWindow?.EndPendulum();

            if (_state != BossState.Dead)
                _state = BossState.Approaching;
        }

        // ── Individual attacks ────────────────────────────────────────────────

        private IEnumerator DrumSlam()
        {
            yield return StartCoroutine(WindUp(Color.red));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (IsPlayerWithinRange(meleeRange + 0.5f) && _playerCombat != null)
            {
                // DrumSlam is never parryable — the drum face blocks the player's counter.
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse(Vector3.down * 2f);
                }
            }

            yield return _waitAttackActive;
        }

        private IEnumerator Haymaker()
        {
            yield return StartCoroutine(WindUp(Color.yellow));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (IsPlayerWithinRange(meleeRange + 0.5f) && _playerCombat != null)
            {
                // Haymaker is only parryable when the drum window is facing the player.
                bool canParry = drumWindow != null && drumWindow.IsParryWindowOpen;
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: canParry, attacker: gameObject);

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
            }

            yield return _waitAttackActive;
        }

        private IEnumerator SpinCharge()
        {
            yield return StartCoroutine(WindUp(new Color(1f, 0.5f, 0f))); // orange
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            // SpinCharge uses a frame-by-frame loop so contact detection runs every frame.
            // A WaitForSeconds here would skip frames and miss short contact windows.
            // Capture the player's position at charge start so the rush has a fixed target.
            Vector3 chargeTarget = _player != null ? _player.position : transform.position;
            Vector3 chargeDir    = (chargeTarget - transform.position).normalized;
            chargeDir.y = 0f;
            if (chargeDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(chargeDir);

            // Yield one frame before contact detection so the boss visibly moves before hitting.
            yield return null;

            float elapsed = 0f;
            bool hitLanded = false;

            if (_agent != null) _agent.enabled = false;

            while (elapsed < spinChargeDuration)
            {
                transform.position += chargeDir * spinChargeSpeed * Time.deltaTime;
                elapsed += Time.deltaTime;

                if (!hitLanded && IsPlayerWithinRange(1.2f) && _playerCombat != null)
                {
                    // SpinCharge is a moving tackle — not parryable.
                    AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                    if (result == AttackResult.Hit)
                    {
                        AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                        _impulseSource?.GenerateImpulse(chargeDir * 1.5f);
                    }
                    hitLanded = true;
                    break; // End charge early on contact.
                }

                yield return null;
            }

            if (_agent != null) { _agent.enabled = true; _agent.Warp(transform.position); }
        }

        private IEnumerator ClothesToss()
        {
            yield return StartCoroutine(JumpBack());
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (_heldClothesBall != null) _heldClothesBall.SetActive(false);

            if (_clothesTossPrefab != null && _player != null)
            {
                // Use held prop world position as spawn origin if available, else default offset.
                Vector3 spawnPos = _heldClothesBall != null
                    ? _heldClothesBall.transform.position
                    : transform.position + Vector3.up * 1.5f;
                GameObject ball = Instantiate(_clothesTossPrefab, spawnPos, Quaternion.identity);
                if (ball.TryGetComponent<BossProjectile>(out var proj))
                    proj.Initialize(_playerCombat);
                if (ball.TryGetComponent<Rigidbody>(out var rb))
                {
                    // Solve velocity to land at player's current position in flightTime seconds.
                    Vector3 toTarget = _player.position - spawnPos;
                    float horizDist  = new Vector3(toTarget.x, 0f, toTarget.z).magnitude;
                    float T          = Mathf.Max(0.5f, horizDist / _clothesTossSpeed);
                    Vector3 vel      = toTarget / T;
                    // Gravity compensation: add what gravity removes over the flight.
                    vel.y -= 0.5f * Physics.gravity.y * T;
                    rb.linearVelocity = vel;
                }
            }
            else if (IsPlayerWithinRange(meleeRange + 1f) && _playerCombat != null)
            {
                // Fallback when no prefab is assigned — melee hit as placeholder.
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
            }

            yield return _waitAttackActive;
            if (_heldClothesBall != null) _heldClothesBall.SetActive(true);
        }

        private IEnumerator FullSpin()
        {
            yield return StartCoroutine(WindUp(Color.magenta));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            int count = Physics.OverlapSphereNonAlloc(transform.position, fullSpinRadius, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                if (!_overlapBuffer[i].CompareTag("Player")) continue;
                if (_playerCombat == null) break;

                // AoE full rotation — no safe angle, not parryable.
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (result == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse();
                }
                break;
            }

            yield return _waitAttackActive;
        }

        private IEnumerator SudsBurst()
        {
            yield return StartCoroutine(JumpBack());
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (_heldSudsBlob != null) _heldSudsBlob.SetActive(false);

            if (_sudsBlobPrefab != null && _player != null)
            {
                // Use held prop world position as spawn origin if available, else default offset.
                Vector3 spawnPos = _heldSudsBlob != null
                    ? _heldSudsBlob.transform.position
                    : transform.position + Vector3.up * 0.5f;
                Vector3 forward  = (_player.position - transform.position);
                forward.y = 0f;
                if (forward == Vector3.zero) forward = transform.forward;
                forward.Normalize();

                for (int i = 0; i < _sudsBurstAngles.Length; i++)
                {
                    Vector3 dir    = Quaternion.Euler(0f, _sudsBurstAngles[i], 0f) * forward;
                    GameObject blob = Instantiate(_sudsBlobPrefab, spawnPos, Quaternion.identity);
                    if (blob.TryGetComponent<BossProjectile>(out var proj))
                        proj.Initialize(_playerCombat);
                    if (blob.TryGetComponent<Rigidbody>(out var rb))
                        rb.linearVelocity = dir * _sudsBlobSpeed + Vector3.up * _sudsBlobArcVelocity;
                }
            }

            yield return _waitAttackActive;
            if (_heldSudsBlob != null) _heldSudsBlob.SetActive(true);
        }

        private IEnumerator DoubleHaymaker()
        {
            // First swing: parryable via drum window.
            yield return StartCoroutine(WindUp(Color.yellow));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (IsPlayerWithinRange(meleeRange + 0.5f) && _playerCombat != null)
            {
                bool canParry  = drumWindow != null && drumWindow.IsParryWindowOpen;
                AttackResult r = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: canParry, attacker: gameObject);

                if (r == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse();
                }

                if (r == AttackResult.Parried)
                {
                    // Parrying the first arm aborts the combo.
                    yield return StartCoroutine(StaggerRoutine());
                    yield break;
                }
            }

            yield return _waitAttackActive;

            if (_state == BossState.Dead) yield break;

            // Second swing: always un-parryable — SpinCycle commits with the off-hand.
            yield return StartCoroutine(WindUp(Color.yellow));
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            if (IsPlayerWithinRange(meleeRange + 0.5f) && _playerCombat != null)
            {
                AttackResult r = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (r == AttackResult.Hit)
                {
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                    _impulseSource?.GenerateImpulse();
                }
            }

            yield return _waitAttackActive;
        }

        private IEnumerator JumpCharge()
        {
            yield return StartCoroutine(WindUp(new Color(0.5f, 0f, 1f))); // purple
            _state = BossState.Attacking;
            SetColor(_baseColor);
            _animator?.SetTrigger(AnimAttack);

            // Capture landing target at jump start.
            Vector3 startPos  = transform.position;
            Vector3 targetPos = _player != null ? _player.position : transform.position;
            targetPos.y = startPos.y; // stay on ground plane until landing

            if (_agent != null) _agent.enabled = false;

            float elapsed = 0f;
            while (elapsed < jumpChargeDuration)
            {
                float t = elapsed / jumpChargeDuration;

                // Linear XZ interpolation with a Sin arc for height.
                Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
                flatPos.y = startPos.y + jumpChargeHeight * Mathf.Sin(t * Mathf.PI);
                transform.position = flatPos;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Snap to ground on landing.
            transform.position = targetPos;
            if (_agent != null) { _agent.enabled = true; _agent.Warp(transform.position); }
            _impulseSource?.GenerateImpulse(Vector3.down * 3f);

            // Landing hit — not parryable, boss falls from above.
            if (IsPlayerWithinRange(meleeRange + 1f) && _playerCombat != null)
            {
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false);
                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
            }
        }

        // ── Shared helpers ────────────────────────────────────────────────────

        private IEnumerator WindUp(Color color)
        {
            _state = BossState.WindUp;
            SetColor(color);
            drumWindow?.BeginPendulum();
            yield return _waitWindUp;
        }

        private IEnumerator JumpBack()
        {
            _state = BossState.WindUp;

            // Fire a brief color flash to telegraph the jump.
            SetColor(Color.cyan);
            drumWindow?.BeginPendulum();

            Vector3 startPos = transform.position;

            // Jump directly away from the player.
            Vector3 awayDir = _player != null
                ? (transform.position - _player.position).normalized
                : -transform.forward;
            awayDir.y = 0f;
            if (awayDir == Vector3.zero) awayDir = -transform.forward;

            Vector3 landPos = startPos + awayDir * jumpBackDistance;
            landPos.y = startPos.y;

            if (_agent != null) _agent.enabled = false;

            float elapsed = 0f;
            while (elapsed < jumpBackDuration)
            {
                float t = elapsed / jumpBackDuration;

                Vector3 flatPos = Vector3.Lerp(startPos, landPos, t);
                flatPos.y = startPos.y + jumpBackHeight * Mathf.Sin(t * Mathf.PI);
                transform.position = flatPos;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Snap to landing position and face the player before firing.
            transform.position = landPos;
            if (_agent != null) { _agent.enabled = true; _agent.Warp(transform.position); }
            if (_player != null)
            {
                Vector3 toPlayer = (_player.position - transform.position).normalized;
                toPlayer.y = 0f;
                if (toPlayer != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(toPlayer);
            }

            SetColor(_baseColor);
        }

        private bool IsPlayerWithinRange(float range)
        {
            if (_player == null) return false;
            return Vector3.Distance(transform.position, _player.position) <= range;
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

            drumWindow?.SetFastPhase();

            _phase       = Phase.Two;
            _attackIndex = 0;
            _state       = BossState.Approaching;
        }

        // ── Stagger ───────────────────────────────────────────────────────────

        private IEnumerator StaggerRoutine()
        {
            _state = BossState.Staggered;
            drumWindow?.EndPendulum();
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
            if (target != null && target != gameObject) return;
            // Counter strike only deals damage during stagger, matching BasicEnemyAI convention.
            if (_state != BossState.Staggered) return;
            _stats.TakeDamage(counterStrikeDamage);
        }

        private void HandleDeath()
        {
            _state = BossState.Dead;

            if (_agent != null) { _agent.isStopped = true; _agent.enabled = false; }

            // StopAllCoroutines kills nested attack routines (DrumSlam, Haymaker, etc.)
            // that are started via yield return StartCoroutine() — StopCoroutine on the
            // outer routine alone leaves those inner routines running.
            StopAllCoroutines();
            _activeRoutine = null;

            if (_animator != null)
            {
                _animator.speed = 1f;           // restore speed in case wind-up changed it
                _animator.SetFloat(AnimSpeed, 0f);
                _animator.SetBool(AnimIsDead, true);
            }

            SetColor(Color.gray);
            if (_heldClothesBall != null) _heldClothesBall.SetActive(false);
            if (_heldSudsBlob    != null) _heldSudsBlob.SetActive(false);

            // Destroy any in-flight projectiles so they cannot kill the player
            // during the defeat sequence.
            foreach (var proj in FindObjectsByType<BossProjectile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                Destroy(proj.gameObject);

            _defeatRoutine = StartCoroutine(DefeatSequence());
        }

        private IEnumerator DefeatSequence()
        {
            drumWindow?.BeginStopDrum();

            // Shrink over the full defeatHoldDuration so the death animation plays
            // simultaneously with the boss disappearing — not before it.
            // TriggerWin MUST be called before Destroy — Destroy would end the coroutine
            // immediately, preventing TriggerWin from ever executing.
            yield return StartCoroutine(ShrinkAndVanish(defeatHoldDuration));
            GameManager.Instance?.TriggerWin();
            Destroy(gameObject);
        }

        private IEnumerator ShrinkAndVanish(float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }
            // Do NOT call Destroy here — DefeatSequence must invoke TriggerWin first.
        }

        private IEnumerator LerpImagination(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (_imaginationVolume == null) yield break;
                elapsed += Time.deltaTime;
                _imaginationVolume.weight = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            if (_imaginationVolume != null)
                _imaginationVolume.weight = 1f;
            GameManager.Instance?.TriggerWin();
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
            // HandleDeath calls StopAllCoroutines; if destroyed without dying, clean up here.
            StopAllCoroutines();
            _activeRoutine = null;
            _defeatRoutine = null;

            if (_playerCombat != null)
                _playerCombat.OnCounterStrike -= OnCounterStrikeLanded;

            if (_stats != null)
                _stats.OnDeath -= HandleDeath;

            if (_material != null)
                Destroy(_material);
        }
    }
}
