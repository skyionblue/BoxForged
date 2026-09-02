using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boxhead.Core;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    /// <summary>
    /// The Crane Duelist — World 2 (Backyard/Dojo) zone-1 regular enemy (NOT a boss; the World 2
    /// boss is <see cref="GrasscutterAI"/>). Docs: docs/story/enemies/crane-duelist.md,
    /// docs/v4/levels/World2/backyard-dojo/gdd.md §3/§10.
    ///
    /// Stationary and patient: unlike BasicEnemyAI/SkepticGruntAI it never chases (lore: "It does
    /// not chase. Chasing is for things that haven't decided what they are."). While idle it
    /// slowly rotates in place to keep facing the player ("the head tracks the player in slow,
    /// deliberate turns... the body barely shifts"). When the player is within <see
    /// cref="beakThrustRange"/> and inside its front arc, it "settles" (a committed telegraph —
    /// weight sinks, spear draws back) before a single long-reach Beak Thrust: parryable, with a
    /// tight active window; a clean parry opens an unusually large counter. After every completed
    /// thrust (landed, missed, or parried) it is off-balance for ~1s — the counter window. If the
    /// player instead closes to the much shorter <see cref="pivotSweepRange"/> outside the front
    /// arc ("circles behind" — read as bad manners), it answers with a fast, un-parryable Pivot
    /// Sweep with no counter window — a reflexive punish, not a duel beat.
    ///
    /// Architecture: follows <see cref="BasicEnemyAI"/>'s State-enum / cached-material /
    /// CombatController.OnCounterStrike-subscription pattern per gdd.md §10's explicit
    /// instruction, but replaces its NavMeshAgent-driven Idle/Chase movement with in-place
    /// rotation only — this enemy never has and never will have a NavMeshAgent. Every attack tell
    /// routes through <see cref="AttackTelegraphService"/> per ADR-0003. Every Animator call is
    /// guarded by the SafeSetTrigger/SafeSetFloat/SafeSetBool + CacheAnimatorParameters pattern
    /// <see cref="GrasscutterAI"/> established for incomplete animator coverage — see the remarks
    /// on <see cref="_animatorParamHashes"/> for the specific gaps found in
    /// AC_Crane_duelist_body.controller.
    ///
    /// Facing convention: verified directly against this model (Head→headfront points exactly
    /// along +transform.forward in the rest pose) rather than assumed — this model does NOT need
    /// BasicEnemyAI's -transform.forward compensation, so every LookRotation call below uses the
    /// direction to the player directly. Never copy a facing-negation convention between models
    /// without verifying it per docs/PROJECT_CONTEXT.md's "Model orientation correction" rule.
    /// </summary>
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(CharacterController))]
    public class CraneDuelistAI : MonoBehaviour, IEnemyBehavior
    {
        [Header("Tracking (idle)")]
        [Tooltip("Lore: 'the head tracks the player in slow, deliberate turns' while otherwise still. Purely cosmetic beyond this — the range check itself is cheap enough to always run.")]
        [SerializeField] private float trackingRange = 12f;
        [Tooltip("Degrees/second the body turns to face the player while idle. Deliberately slow — a player who moves faster than this can end up outside the front arc before the crane finishes turning, which is the intended way to trigger Pivot Sweep rather than a distance-only check.")]
        [SerializeField] private float idleTurnSpeed = 70f;

        [Header("Beak Thrust (front arc, long reach)")]
        [Tooltip("GDD §3: 'Attack type: Melee (long reach).' Player must be within this range AND inside frontArcHalfAngle of the crane's current forward for it to settle into a Beak Thrust instead of a Pivot Sweep.")]
        [SerializeField] private float beakThrustRange = 3.8f;
        [Tooltip("Half-angle (degrees) of the front arc the crane considers 'facing you honestly.' Outside this arc but within pivotSweepRange counts as a flank.")]
        [SerializeField] private float frontArcHalfAngle = 50f;
        [Tooltip("Lore: 'it settles — weight sinks, hat tilts down, the beak-spear draws back level. That's the tell.' This is the visible wind-up duration, broadcast through AttackTelegraphService before the thrust becomes active. Facing is locked the instant settling begins (see BeakThrustRoutine) — a committed telegraph, not a live-tracking one, matching GrasscutterAI.SpinDash's committed-heading lesson (ADR-0007): an attack that can still re-aim after its own tell has started is not fairly dodgeable.")]
        [SerializeField] private float settleDuration = 1.1f;
        [Tooltip("GDD: 'a single fast, long-reach lunging thrust' with a 'tight window.' Kept short relative to other enemies' active windows so the parry timing reads as precise rather than generous.")]
        [SerializeField] private float thrustActiveDuration = 0.22f;
        [SerializeField] private float thrustCooldown = 2.2f;

        [Header("Off-Balance / Counter Window")]
        [Tooltip("Lore: 'visibly off-balance on one leg for ~1s — the counter window.' Entered after every completed Beak Thrust (hit, miss, or parried) — not only on a successful parry.")]
        [SerializeField] private float offBalanceDuration = 1f;
        [Tooltip("GDD: 'a clean parry opens an unusually large counter.' Set noticeably higher than a typical Tier-1/2 grunt's counter damage (BasicEnemyAI 30 / SkepticGruntAI 25) to read as 'unusually large.'")]
        [SerializeField] private int counterStrikeDamage = 45;

        [Header("Pivot Sweep (flank punish, short range)")]
        [Tooltip("Lore: 'If the player circles behind, the crane reads it as bad manners and answers with the Pivot Sweep... Short range' (gdd.md §3 Parry rules). Deliberately shorter than beakThrustRange so a flanker who stays at range isn't punished — the crane just keeps turning to face them.")]
        [SerializeField] private float pivotSweepRange = 2f;
        [Tooltip("Fast, low reaction — much shorter than the patient Beak Thrust settle (lore: 'not elegant, almost annoyed').")]
        [SerializeField] private float pivotSweepWindUp = 0.35f;
        [SerializeField] private float pivotSweepActiveDuration = 0.25f;
        [SerializeField] private float pivotSweepCooldown = 1.5f;

        [Header("Hit Stagger (player attack interrupts)")]
        [SerializeField] private float hitStaggerDuration = 0.4f;

        // GDD: "Miss -> heavy player stagger." CombatController.TryReceiveAttack's Hit branch
        // always uses a single project-wide playerStaggerDuration constant — there is no
        // per-attacker lever to make one enemy's miss punish "heavier" than another's. Adding one
        // would be a cross-cutting change to shared player combat code, outside this task's scope
        // (see docs/TECHNICAL_DECISIONS.md architecture-change process) — flagged here as a known,
        // intentionally-unimplemented gap rather than silently worked around.

        [Header("Telegraph")]
        [Tooltip("ADR-0003 overhead telegraph height above this enemy's root. Slightly taller than the ~1.5m default enemies use, tuned for this model's ~2.0m height plus its conical hat.")]
        [SerializeField] private float telegraphHeightOffset = 2.9f;

        private enum State { Idle, Settling, Thrusting, OffBalance, PivotWindUp, PivotSweeping, HitStagger, Dead }
        private State _state = State.Idle;

        private Transform _player;
        private CombatController _playerCombat;
        private EnemyStats _stats;
        private Animator _animator;
        private Material _material;
        private Color _baseColor;

        private float _attackCooldownTimer;
        private float _currentTurnSpeed;
        private bool _isRooted;
        private float _hitStaggerMultiplier = 1f;
        private Coroutine _activeRoutine;
        private Coroutine _speedRestoreRoutine;

        // B4 fix: the real counter-eligibility window, decoupled from _state. OffBalanceRoutine
        // sets this to Time.time + a duration sized to at least cover CombatController's own
        // counterWindowDuration (read live, not hardcoded — see CounterWindowDuration below).
        // _state alone is NOT a reliable proxy for "is a legitimate counter still live": it gets
        // clobbered early by (a) offBalanceDuration being shorter than the player's actual
        // window, and (b) any incoming damage during OffBalance routing through
        // EnemyStats.TakeDamage -> OnHit -> OnHitReceived -> State.HitStagger. Both cases must
        // not cause an earned counter to silently whiff.
        private float _counterWindowEndTime = -1f;

        // M1 fix: handle for the currently-shown AttackTelegraphService indicator, so HandleDeath
        // can Hide() it immediately instead of leaving it hovering over the corpse until the
        // pooled indicator's own _target == null guard kicks in. Only one of
        // BeakThrustRoutine/PivotSweepRoutine is ever active at a time (both are only entered
        // from Idle via StopActive()/StartActive()), so a single shared field is sufficient —
        // mirrors GrasscutterAI.HandleDeath's precedent of explicitly hiding an in-flight handle.
        private AttackTelegraphHandle _activeTelegraphHandle = AttackTelegraphHandle.None;

        private static readonly int AnimAttackTrigger = Animator.StringToHash("AttackTrigger");
        private static readonly int AnimIsDead = Animator.StringToHash("IsDead");

        // AC_Crane_duelist_body.controller gaps found while building this AI:
        //  - No StaggerTrigger parameter exists at all. A "Stagger" AnimatorState is present in
        //    the controller but has zero incoming transitions from anywhere — it is unreachable
        //    regardless of what this script calls. The off-balance/counter-window beat is
        //    therefore carried procedurally (WobbleRoutine, a small rotational wobble — same
        //    technique as GrasscutterAI.WobbleRoutine) plus a colour tint, not by animation.
        //  - AttackTrigger drives a single "Attack" state; firing AttackTrigger again while still
        //    in "Attack" chains into "Attack2" (a double-tap combo baked into the controller, not
        //    an independently-selectable second attack) — there is no way to play "Attack2"
        //    without first playing "Attack". Both Beak Thrust and Pivot Sweep therefore fire the
        //    same AnimAttackTrigger, mirroring GrasscutterAI's own convention of one shared
        //    AttackTrigger across differently-named attacks (BladeCombo / ReelGuardBreak /
        //    PetalToss); a future animator pass could wire a dedicated trigger straight to
        //    "Attack2" for a visually distinct Pivot Sweep.
        // SafeSetTrigger/SafeSetBool below guard every call so a missing parameter is a silent
        // no-op, not a console error — the exact pattern GrasscutterAI established for
        // AC_Grasscutter.controller's equivalent gaps.
        private HashSet<int> _animatorParamHashes;

        private WaitForSeconds _waitSettle;
        private WaitForSeconds _waitThrustActive;
        private WaitForSeconds _waitPivotWindUp;
        private WaitForSeconds _waitPivotActive;
        private WaitForSeconds _waitDie;

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            _animator = GetComponentInChildren<Animator>();
            CacheAnimatorParameters();

            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                _material = renderer.material;
                _baseColor = _material.color;
            }

            _currentTurnSpeed = idleTurnSpeed;

            _waitSettle = new WaitForSeconds(settleDuration);
            _waitThrustActive = new WaitForSeconds(thrustActiveDuration);
            _waitPivotWindUp = new WaitForSeconds(pivotSweepWindUp);
            _waitPivotActive = new WaitForSeconds(pivotSweepActiveDuration);
            _waitDie = new WaitForSeconds(0.5f);
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
            else
            {
                Debug.LogWarning("[CraneDuelistAI] No GameObject tagged 'Player' found.", this);
            }

            _stats.OnDeath += HandleDeath;
            _stats.OnHit += OnHitReceived;
        }

        private void Update()
        {
            if (_state == State.Dead) return;
            if (_player == null) return;

            if (_attackCooldownTimer > 0f)
                _attackCooldownTimer -= Time.deltaTime;

            if (_state == State.Idle)
                UpdateIdle();
        }

        private void UpdateIdle()
        {
            if (_isRooted) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= trackingRange)
                TurnTowardPlayer();

            if (_attackCooldownTimer > 0f) return;

            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.0001f) return;

            float planarDist = toPlayer.magnitude;
            float angle = Vector3.Angle(transform.forward, toPlayer);

            if (planarDist <= beakThrustRange && angle <= frontArcHalfAngle)
            {
                _attackCooldownTimer = thrustCooldown;
                StopActive();
                StartActive(BeakThrustRoutine());
            }
            else if (planarDist <= pivotSweepRange && angle > frontArcHalfAngle)
            {
                _attackCooldownTimer = pivotSweepCooldown;
                StopActive();
                StartActive(PivotSweepRoutine());
            }
        }

        private void TurnTowardPlayer()
        {
            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.0001f) return;
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _currentTurnSpeed * Time.deltaTime);
        }

        private void FaceTowardPlayerInstant()
        {
            if (_player == null) return;
            Vector3 dir = _player.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        // ── Beak Thrust ───────────────────────────────────────────────────────

        private IEnumerator BeakThrustRoutine()
        {
            _state = State.Settling;
            // Facing is locked once, here, before the telegraph is even shown — the settle is a
            // committed pose ("it settles... draws back level"), not a live-tracking wind-up.
            FaceTowardPlayerInstant();
            SetColor(new Color(1f, 0.55f, 0.1f)); // amber "settling" tint, additive to the AttackTelegraphService billboard
            _activeTelegraphHandle = AttackTelegraphService.Show(transform, AttackTelegraphKind.MeleeParryable, settleDuration, telegraphHeightOffset);
            yield return _waitSettle;
            if (_state == State.Dead) yield break;

            _state = State.Thrusting;
            SetColor(_baseColor);
            SafeSetTrigger(AnimAttackTrigger);

            if (_playerCombat != null && IsPlayerWithinRange(beakThrustRange + 0.5f))
            {
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: true, attacker: gameObject);
                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);

                if (result == AttackResult.Parried)
                {
                    // H1 fix: yield the IEnumerator directly rather than wrapping it in a nested
                    // StartCoroutine(...). A nested StartCoroutine creates a second, untracked
                    // Coroutine object that _activeRoutine never points at — StopCoroutine
                    // (via StopActive()/StopAllCoroutines() elsewhere) would stop this outer
                    // routine while leaving OffBalanceRoutine/WobbleRoutine running orphaned.
                    // Yielding directly inlines the whole chain into the single Coroutine object
                    // already tracked by _activeRoutine, so stopping it stops everything.
                    yield return OffBalanceRoutine();
                    yield break;
                }
            }

            yield return _waitThrustActive;
            if (_state == State.Dead) yield break;

            // Lore: "After a full thrust it is off-balance... for a beat" — unconditional, not
            // gated on a parry. A landed hit or an outright miss both end here too.
            yield return OffBalanceRoutine();
        }

        private IEnumerator OffBalanceRoutine()
        {
            _state = State.OffBalance;
            SetColor(Color.yellow);

            // B4 fix: open the real counter-eligibility window here, independent of _state.
            // Sized to at least CombatController.CounterWindowDuration (read live from the
            // player, not hardcoded) so a legitimate parry-triggered counter always lands even
            // if _state has since moved on (back to Idle after offBalanceDuration elapses, or
            // clobbered by HitStagger from an in-flight hit). The wobble visual itself
            // deliberately still only plays for offBalanceDuration below — only the counter
            // eligibility window is widened, not the cosmetic off-balance pose.
            float counterWindow = offBalanceDuration;
            if (_playerCombat != null)
                counterWindow = Mathf.Max(counterWindow, _playerCombat.CounterWindowDuration);
            _counterWindowEndTime = Time.time + counterWindow;

            // H1 fix: yield directly (see the comment at BeakThrustRoutine's call site) so this
            // routine and WobbleRoutine stay part of the single Coroutine object _activeRoutine
            // tracks, instead of spawning an untracked, un-stoppable child.
            yield return WobbleRoutine(offBalanceDuration);
            if (_state == State.OffBalance)
            {
                SetColor(_baseColor);
                _state = State.Idle;
            }
        }

        // Procedural off-balance read — see the class-level remark on _animatorParamHashes for
        // why this can't be a StaggerTrigger animation. Same technique as
        // GrasscutterAI.WobbleRoutine (a decaying sinusoidal yaw wobble), scaled down for a
        // regular enemy rather than a boss.
        private IEnumerator WobbleRoutine(float duration)
        {
            float elapsed = 0f;
            float baseY = transform.eulerAngles.y;
            const float wobbleAmplitude = 10f;
            const float wobbleFrequency = 14f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float angle = Mathf.Sin(elapsed * wobbleFrequency) * wobbleAmplitude * (1f - elapsed / duration);
                transform.rotation = Quaternion.Euler(0f, baseY + angle, 0f);
                yield return null;
            }

            transform.rotation = Quaternion.Euler(0f, baseY, 0f);
        }

        // ── Pivot Sweep ───────────────────────────────────────────────────────

        private IEnumerator PivotSweepRoutine()
        {
            _state = State.PivotWindUp;
            FaceTowardPlayerInstant();
            SetColor(Color.red);
            _activeTelegraphHandle = AttackTelegraphService.Show(transform, AttackTelegraphKind.MeleeUnparryable, pivotSweepWindUp, telegraphHeightOffset);
            yield return _waitPivotWindUp;
            if (_state == State.Dead) yield break;

            _state = State.PivotSweeping;
            SetColor(_baseColor);
            SafeSetTrigger(AnimAttackTrigger);

            if (_playerCombat != null && IsPlayerWithinRange(pivotSweepRange + 0.5f))
            {
                // parryable:false — GDD: "Pivot Sweep un-parryable — jump/dodge only."
                // CombatController.TryReceiveAttack cannot return Parried when parryable is
                // false, so there is deliberately no counter-window branch here.
                AttackResult result = _playerCombat.TryReceiveAttack(_stats.AttackDamage, parryable: false, attacker: gameObject);
                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
            }

            yield return _waitPivotActive;
            if (_state == State.Dead) yield break;
            _state = State.Idle;
        }

        // ── Hit stagger (player's own attack interrupts) ─────────────────────

        private void OnHitReceived()
        {
            if (_state == State.Dead || _state == State.HitStagger) return;
            AudioManager.Instance?.Play(SoundEvent.EnemyHit);
            StopActive();
            StartActive(HitStaggerRoutine());
        }

        private IEnumerator HitStaggerRoutine()
        {
            float multiplier = _hitStaggerMultiplier;
            _hitStaggerMultiplier = 1f;

            _state = State.HitStagger;
            SetColor(Color.yellow);

            float elapsed = 0f;
            float duration = hitStaggerDuration * multiplier;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_state == State.HitStagger && !_stats.IsDead)
            {
                SetColor(_baseColor);
                _state = State.Idle;
            }
        }

        // ── IEnemyBehavior ────────────────────────────────────────────────────

        public void SetRooted(bool rooted)
        {
            _isRooted = rooted;
            // No locomotion to stop/start — this enemy never moves. Rooted only suppresses new
            // engagements from Idle (checked in UpdateIdle); an attack already in progress
            // (Settling/Thrusting/PivotWindUp/PivotSweeping/OffBalance) is not interrupted,
            // matching every other enemy AI's SetRooted convention in this codebase.
        }

        public void ApplyHitStagger(float durationMultiplier = 1f)
        {
            if (_state == State.Dead || _state == State.HitStagger) return;
            _hitStaggerMultiplier = durationMultiplier;
            StopActive();
            StartActive(HitStaggerRoutine());
        }

        /// <summary>
        /// This enemy has no locomotion speed to scale (it never moves), so the multiplier is
        /// applied to its idle head/body turn rate instead — the only "speed" it has. This keeps
        /// crowd-control abilities (Soaked/Mixed Up/Lasso-adjacent) that call
        /// IEnemyBehavior.SetSpeedMultiplier generically meaningful against the Crane Duelist
        /// rather than a silent no-op.
        /// </summary>
        public void SetSpeedMultiplier(float multiplier, float duration)
        {
            if (_state == State.Dead) return;
            if (_speedRestoreRoutine != null)
                StopCoroutine(_speedRestoreRoutine);
            _currentTurnSpeed = idleTurnSpeed * multiplier;
            _speedRestoreRoutine = StartCoroutine(RestoreTurnSpeedAfter(duration));
        }

        private IEnumerator RestoreTurnSpeedAfter(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            _currentTurnSpeed = idleTurnSpeed;
            _speedRestoreRoutine = null;
        }

        // ── Event handlers ────────────────────────────────────────────────────

        // Called when the player executes a counter strike during the Counter Window opened by a
        // successful parry. B4 fix: gated on _counterWindowEndTime (a dedicated real-time
        // timer opened independently in OffBalanceRoutine), NOT on _state == State.OffBalance.
        // The GDD frames the off-balance beat as "the counter window," but offBalanceDuration
        // (1s) is shorter than CombatController.counterWindowDuration (1.5s) — under a pure
        // _state gate, a player attacking in that ~0.5s dead window would have already consumed
        // their real counter window (ExecuteCounterStrike healed them, fired OnCounterStrike)
        // while this enemy silently took zero damage. The same _state gate was also clobbered
        // early by unrelated damage (EnemyStats.TakeDamage -> OnHit -> OnHitReceived ->
        // State.HitStagger) during a legitimate OffBalance window. The timer is immune to both.
        private void OnCounterStrikeLanded(GameObject target)
        {
            if (target != null && target != gameObject) return;
            if (_state == State.Dead) return;
            if (Time.time > _counterWindowEndTime) return;
            _stats.TakeDamage(counterStrikeDamage);
        }

        private void HandleDeath()
        {
            _state = State.Dead;

            // M1 fix: hide any in-progress telegraph immediately. Must happen before
            // StopAllCoroutines() below — a coroutine stopped externally never resumes to reach
            // its own cleanup, so BeakThrustRoutine/PivotSweepRoutine's Show() call has no chance
            // to Hide() itself once its routine is torn down here. Same lesson
            // GrasscutterAI.HandleDeath already applies to _spinDashLaneHandle.
            AttackTelegraphService.Hide(_activeTelegraphHandle);

            // H1 fix: StopAllCoroutines() (not StopActive()'s single StopCoroutine) so that even
            // if a future edit reintroduces a nested StartCoroutine somewhere in this class, the
            // death path still can't leave an orphaned corpse-wobble or speed-restore coroutine
            // running — matches GrasscutterAI.HandleDeath's own precedent for this exact bug.
            StopAllCoroutines();
            _activeRoutine = null;
            _speedRestoreRoutine = null;

            SetColor(Color.gray);
            SafeSetBool(AnimIsDead, true);
            StartCoroutine(DieRoutine());
        }

        private IEnumerator DieRoutine()
        {
            yield return _waitDie;
            Destroy(gameObject);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private bool IsPlayerWithinRange(float range)
        {
            if (_player == null) return false;
            return Vector3.Distance(transform.position, _player.position) <= range;
        }

        private void SetColor(Color color)
        {
            if (_material != null)
                _material.color = color;
        }

        private void CacheAnimatorParameters()
        {
            _animatorParamHashes = new HashSet<int>();
            if (_animator == null) return;
            foreach (var p in _animator.parameters)
                _animatorParamHashes.Add(p.nameHash);
        }

        private void SafeSetTrigger(int hash)
        {
            if (_animator != null && _animatorParamHashes != null && _animatorParamHashes.Contains(hash))
                _animator.SetTrigger(hash);
        }

        private void SafeSetBool(int hash, bool value)
        {
            if (_animator != null && _animatorParamHashes != null && _animatorParamHashes.Contains(hash))
                _animator.SetBool(hash, value);
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
                _stats.OnHit -= OnHitReceived;
            }
            if (_material != null)
                Destroy(_material);
        }

        // ── Editor gizmos ─────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = new Color(0f, 0.6f, 1f, 0.06f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, trackingRange);
            UnityEditor.Handles.color = new Color(0f, 0.6f, 1f, 1f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, trackingRange);

            UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.18f);
            Vector3 arcStart = Quaternion.Euler(0f, -frontArcHalfAngle, 0f) * transform.forward;
            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, arcStart, frontArcHalfAngle * 2f, beakThrustRange);

            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.15f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, pivotSweepRange);
            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 1f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, pivotSweepRange);
        }
#endif
    }
}
