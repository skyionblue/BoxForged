using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Boxhead.Core;
using Boxhead.Systems;

namespace Boxhead.Player
{
    public enum CombatState { Idle, Attacking, Dodging, Parrying, Countering, Staggered, SpecialAttacking }

    public enum AttackResult { Hit, Dodged, Parried }

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerController))]
    public class CombatController : MonoBehaviour
    {
        [Header("Dodge")]
        [SerializeField] private float dodgeDuration = 0.5f;
        [SerializeField] private float dodgeCooldown = 0.8f;
        [SerializeField] private float dodgeDistance = 3f;
        [SerializeField] private float dodgeMovementDelay = 0.2f;
        private float _dodgeDistanceMultiplier = 1f;

        [Header("Parry")]
        [SerializeField] private float parryActiveWindow = 0.4f;

        [Header("Counter")]
        [SerializeField] private float counterWindowDuration = 1.5f;

        [Header("Stagger")]
        [SerializeField] private float playerStaggerDuration = 0.8f;

        [Header("Attack")]
        [SerializeField] private float attackDuration = 0.7f;
        [SerializeField] private float attackHitDelay = 0.2f;
        [SerializeField] private int attackDamage = 15;
        [SerializeField] private float attackRadius = 1.8f;
        [SerializeField] private float _attackAnimSpeedMultiplier = 1.5f;
        [SerializeField] private LayerMask _enemyLayer = ~0;

        [Header("Aerial Attack")]
        [SerializeField] private float _aerialSlamRadius = 1.5f;

        [Header("Health Regen")]
        [SerializeField] private int _killHealAmount    = 5;
        [SerializeField] private int _parryHealAmount   = 5;
        [SerializeField] private int _counterHealAmount = 10;

        [Header("VFX")]
        [SerializeField] private ParticleSystem _hitSparkVFX;
        [SerializeField] private ParticleSystem _parryRingVFX;
        [SerializeField] private ParticleSystem _aerialSlamVfx;
        [SerializeField] private Material _parryFlashMaterial;

        [Header("Fighting Style")]
        [SerializeField] private GameObject _tumbleshotPrefab;
        [SerializeField] private Material _shadowMaterialTemplate;

        public CombatState State { get; private set; } = CombatState.Idle;
        public bool IsInvincible => State == CombatState.Dodging;
        public bool IsInHitPhase { get; private set; }

        /// <summary>
        /// Duration of the post-parry counter window (see <see cref="CounterWindowRoutine"/>).
        /// Exposed so an attacker whose own off-balance/stagger telegraph is shorter than this
        /// (e.g. CraneDuelistAI's counter-eligibility timer) can size itself to actually cover
        /// the player's real window instead of hardcoding a value that can silently drift out of
        /// sync with this one.
        /// </summary>
        public float CounterWindowDuration => counterWindowDuration;

        // ── Special ability HUD surface ───────────────────────────────────────
        /// <summary>True when a weapon ability or fighting style special is available.</summary>
        public bool HasSpecialAbility => _currentAbility != null || _activeStyle != null;

        /// <summary>
        /// Cooldown progress 0–1. 1 = fully recharged / ready, 0 = just fired.
        /// Reads from the active style's cooldown when no weapon ability is equipped.
        /// </summary>
        public float SpecialCooldownProgress
        {
            get
            {
                if (_currentAbility == null)
                {
                    if (_activeStyle == null) return 1f;
                    float styleDuration = _activeStyle.SpecialCooldownDuration;
                    if (styleDuration <= 0f) return 1f;
                    return Mathf.Clamp01(1f - (_specialCooldownTimer / styleDuration));
                }
                float duration = _currentAbility.CooldownDuration;
                if (duration <= 0f) return 1f;
                return Mathf.Clamp01(1f - (_specialCooldownTimer / duration));
            }
        }

        /// <summary>
        /// Readiness fraction 0–1 for the HUD fill. Prefers the ability's own ProgressFraction
        /// when it provides custom tracking (e.g. SixShooter per-shot reload); falls back to
        /// SpecialCooldownProgress for abilities that use the standard timer or style specials.
        /// </summary>
        public float SpecialAbilityProgress
        {
            get
            {
                // When a fighting style is active, its cooldown always drives the special button —
                // regardless of which weapon is equipped.
                if (_activeStyle != null) return SpecialCooldownProgress;
                // Attack-button weapons with no style active: button stays fully charged.
                if (_currentAbility != null && _currentAbility.FiresOnAttackButton)
                    return 1f;
                if (_currentAbility == null) return SpecialCooldownProgress;
                float abilityFraction = _currentAbility.ProgressFraction;
                return abilityFraction >= 0f ? abilityFraction : SpecialCooldownProgress;
            }
        }

        /// <summary>Fired when the equipped weapon ability changes (including to null).</summary>
        public event Action<WeaponAbilityData> OnSpecialEquipped;

        /// <summary>Fired when the active fighting style changes (including to null).</summary>
        public event Action<FightingStyleData> OnStyleChanged;

        /// <summary>Fired at the top of SpecialAttackRoutine before any style or weapon ability branch executes. AbilityExecutor subscribes to route OnSpecial-triggered abilities.</summary>
        public event Action OnSpecialActivated;

        public event Action OnDodgeStarted;
        public event Action OnParrySuccess;
        public event Action OnCounterWindowOpened;
        public event Action<GameObject> OnCounterStrike;
        public event Action OnCounterWindowClosed;
        public event Action OnPlayerStaggered;
        public event Action OnAerialSlam;

        private CharacterController _controller;
        private PlayerStats _stats;
        private PlayerController _movement;
        private Animator _animator;
        private BoxSystem _boxSystem;
        private Vector3 _dodgeDirection;
        private float _dodgeCooldownTimer;
        private Coroutine _activeRoutine;
        private GameObject _lastAttacker;
        private bool _pendingAerialAttack;
        private bool _attackQueued;
        private readonly Collider[] _attackHitBuffer = new Collider[8];
        private readonly Collider[] _slamHitBuffer = new Collider[8];
        private readonly Collider[] _dashHitBuffer = new Collider[8];
        private readonly Enemy.EnemyStats[] _hitStatsBuffer = new Enemy.EnemyStats[8];
        private readonly Enemy.EnemyStats[] _slamStatsBuffer = new Enemy.EnemyStats[8];
        private readonly Enemy.EnemyStats[] _dashStatsBuffer = new Enemy.EnemyStats[8];
        private Material _lastAttackerOriginalMaterial;
        private Renderer _lastAttackerRenderer;

        // Cached allocations
        private WaitForSeconds _waitDodge;
        private WaitForSeconds _waitDodgeDelay;
        private WaitForSeconds _waitParry;
        private WaitForSeconds _waitCounter;
        private WaitForSeconds _waitStagger;
        private WaitForSeconds _waitAttackHit;
        private WaitForSeconds _waitAttackRecovery;
        private WaitForSeconds _waitParryFlash;

        private Coroutine _parryFlashRoutine;

        // Special ability
        private WeaponAbilityData _currentAbility;
        private WeaponHolder      _weaponHolder;
        private Inventory         _inventory;
        private WeaponInventory   _weaponInventory;
        private Boxhead.Systems.AbilityExecutor _abilityExecutor;
        private WeaponDurability  _weaponDurability;
        private float             _specialCooldownTimer;

        // Fighting style
        private FightingStyleData _activeStyle;
        private Renderer          _playerRenderer;
        private Material          _shadowMaterial;
        private Material          _originalMaterial;

        private static readonly int AnimAttack             = Animator.StringToHash("AttackTrigger");
        private static readonly int AnimDodge             = Animator.StringToHash("DodgeTrigger");
        private static readonly int AnimParry             = Animator.StringToHash("ParryTrigger");
        private static readonly int AnimStagger           = Animator.StringToHash("StaggerTrigger");
        private static readonly int DodgeStateHash        = Animator.StringToHash("Dodge");
        private static readonly int AttackStateHash       = Animator.StringToHash("Attack");
        private static readonly int SwordAttackStateHash  = Animator.StringToHash("SwordAttack");
        private static readonly int LocoStateHash         = Animator.StringToHash("Locomotion");
        private static readonly int SwordLocoStateHash    = Animator.StringToHash("SwordLocomotion");
        private static readonly int WeaponTypeHash        = Animator.StringToHash("WeaponType");
        private static readonly int SlamStateHash         = Animator.StringToHash("SlamAttack");

        private const int AttackLayer = 0;

        private void Awake()
        {
            _controller   = GetComponent<CharacterController>();
            _stats        = GetComponent<PlayerStats>();
            _movement     = GetComponent<PlayerController>();
            _animator     = GetComponentInChildren<Animator>();
            _boxSystem    = GetComponent<BoxSystem>();
            _weaponHolder    = GetComponent<WeaponHolder>();
            _inventory       = GetComponent<Inventory>();
            _weaponInventory = GetComponent<WeaponInventory>();
            _weaponDurability = GetComponent<WeaponDurability>();
            _abilityExecutor = GetComponent<Boxhead.Systems.AbilityExecutor>();

            _waitDodge          = new WaitForSeconds(dodgeDuration);
            _waitDodgeDelay     = new WaitForSeconds(dodgeMovementDelay);
            _waitParry          = new WaitForSeconds(parryActiveWindow);
            _waitCounter        = new WaitForSeconds(counterWindowDuration);
            _waitStagger        = new WaitForSeconds(playerStaggerDuration);
            _waitAttackHit      = new WaitForSeconds(attackHitDelay);
            _waitAttackRecovery = new WaitForSeconds(attackDuration - attackHitDelay);
            _waitParryFlash     = new WaitForSeconds(0.1f);

            // Cache player renderer for Shadow Dash material swap
            _playerRenderer = GetComponentInChildren<Renderer>();
            if (_playerRenderer != null)
                _originalMaterial = _playerRenderer.sharedMaterial;

            // Shadow material: duplicate the serialized template so we own this instance
            if (_shadowMaterialTemplate != null)
            {
                _shadowMaterial = new Material(_shadowMaterialTemplate);
                _shadowMaterial.color = Color.black;
            }
        }

        private void Start()
        {
            if (_boxSystem != null) _boxSystem.OnModelChanged += RefreshAnimator;
            Enemy.EnemyStats.OnAnyEnemyDeath += OnEnemyKilled;
        }

        private void OnEnemyKilled()
        {
            _stats?.Heal(_killHealAmount);
        }

        private void RefreshAnimator()
        {
            _animator = GetComponentInChildren<Animator>();
            _playerRenderer = GetComponentInChildren<Renderer>();
            if (_playerRenderer != null)
                _originalMaterial = _playerRenderer.sharedMaterial;
        }

        private void Update()
        {
            if (_dodgeCooldownTimer > 0f)
                _dodgeCooldownTimer -= Time.deltaTime;

            if (_specialCooldownTimer > 0f)
                _specialCooldownTimer -= Time.deltaTime;
        }

        // ── Fighting Style ────────────────────────────────────────────────────

        /// <summary>
        /// Applies a fighting style to this CombatController. Call this when the player
        /// selects a style at the style selection screen. Passing null clears the active style
        /// and restores all serialized defaults.
        /// </summary>
        public void SetFightingStyle(FightingStyleData style)
        {
            _activeStyle = style;
            OnStyleChanged?.Invoke(style);

            if (style != null && style.Passive == PassiveType.WiderParryWindow)
            {
                // Cowboy passive: widen the active parry frame.
                // Rebuild the cached WaitForSeconds so ParryRoutine uses the new duration.
                parryActiveWindow = style.PassiveParryWindow;
                _waitParry = new WaitForSeconds(parryActiveWindow);
            }
            // Ninja passive (DodgeInvincibility) is enforced inside TryReceiveAttack — no field change needed.
        }

        /// <summary>
        /// Scales the dodge travel distance by mult. Called by AbilityExecutor when a
        /// DodgeDistanceMult passive ability is equipped; reset to 1f on unequip.
        /// </summary>
        public void SetDodgeDistanceMultiplier(float mult)
        {
            _dodgeDistanceMultiplier = Mathf.Max(0.1f, mult);
        }

        // Crit multiplier state — set by AbilityExecutor for CritMultiplier abilities.
        // Default 1f (no crit). Consumed and reset to 1f in ApplyAttackDamage.
        private float _critMultiplier = 1f;

        /// <summary>
        /// Sets a one-shot critical hit multiplier applied to the very next attack.
        /// Automatically resets to 1f after the hit lands. Called by AbilityExecutor for
        /// CritMultiplier-type abilities (e.g. Clean Cut, The First Strike).
        /// </summary>
        public void SetNextHitCritMultiplier(float mult)
        {
            _critMultiplier = mult;
        }

        // ── Input callbacks (PlayerInput SendMessages) ──────────────────────

        public void OnAttack(InputValue value)
        {
            if (!value.isPressed) return;
            if (State == CombatState.Countering)
            {
                ExecuteCounterStrike();
                return;
            }
            if (State == CombatState.Dodging || State == CombatState.Parrying || State == CombatState.Staggered) return;

            // Ranged weapons (Six Shooter, Dynamite, Shuriken) consume the Attack button.
            // fromAttackButton=true ensures the style special is skipped even when a style is active.
            if (_currentAbility != null && _currentAbility.FiresOnAttackButton)
            {
                // Only block on the weapon's own cooldown (e.g. Six Shooter 0.2s between shots).
                // If the weapon has no cooldown (Shuriken, Dynamite), any active timer is from
                // the style special — irrelevant to the weapon's fire rate.
                if (_specialCooldownTimer > 0f && _currentAbility.CooldownDuration > 0f) return;
                if (!_currentAbility.IsReadyToActivate) return;
                if (State == CombatState.Attacking || State == CombatState.SpecialAttacking) return;
                StopActive();
                StartActive(SpecialAttackRoutine(fromAttackButton: true));
                return;
            }

            if (_movement != null && _movement.IsReliablyAirborne)
            {
                _pendingAerialAttack = true;
                _movement.SetFastFall();
                _animator?.CrossFadeInFixedTime(SlamStateHash, 0.05f, 0, 0f);
                return;
            }

            // While an attack is playing, queue one follow-up rather than restarting the timer.
            // Interrupting resets the full recovery window on every press — mashing 4 times keeps
            // the player locked for (n-1)*pressInterval + attackDuration instead of just attackDuration.
            if (State == CombatState.Attacking)
            {
                _attackQueued = true;
                return;
            }

            AudioManager.Instance?.Play(SoundEvent.PlayerAttack);
            if (_animator != null) _animator.ResetTrigger(AnimAttack);
            _attackQueued = false;
            StopActive();
            StartActive(AttackRoutine());
        }

        public void OnDodge(InputValue value)
        {
            if (!value.isPressed) return;
            if (_dodgeCooldownTimer > 0f) return;
            if (State == CombatState.Dodging || State == CombatState.Staggered) return;

            // If an OnDodge ability is equipped (e.g. Draw Attack), route the input directly
            // to the ability and skip the standard dodge coroutine entirely.
            if (AbilityInterceptsDodge)
            {
                _dodgeCooldownTimer = dodgeCooldown;
                AudioManager.Instance?.Play(SoundEvent.PlayerDodge);
                _abilityExecutor.FireDodgeAbility();
                return;
            }

            // transform.forward points opposite to visual facing due to the Y=180° child offset.
            // -transform.forward = the direction the character visually faces.
            _dodgeDirection = _movement.CurrentMoveDirection != Vector3.zero
                ? _movement.CurrentMoveDirection
                : -transform.forward;
            _dodgeCooldownTimer = dodgeCooldown;
            AudioManager.Instance?.Play(SoundEvent.PlayerDodge);
            StopActive();
            StartActive(DodgeRoutine());
            OnDodgeStarted?.Invoke();
        }

        /// <summary>
        /// True when the equipped weapon has an OnDodge ability that should replace the
        /// standard dodge entirely (e.g. Katana Legendary Draw Attack).
        /// Guards with null check — false when no AbilityExecutor is present.
        /// </summary>
        private bool AbilityInterceptsDodge =>
            _abilityExecutor != null && _abilityExecutor.HasActiveDodgeAbility;

        public void OnParry(InputValue value)
        {
            if (!value.isPressed) return;
            if (State != CombatState.Idle) return;
            if (_movement != null && _movement.IsReliablyAirborne) return;
            StopActive();
            StartActive(ParryRoutine());
        }

        public void OnSpecialAttack(InputValue value)
        {
            if (!value.isPressed) return;

            // Allow fire if a weapon ability is equipped OR if the active style provides a special.
            bool hasAbility = _currentAbility != null || _activeStyle != null;
            if (!hasAbility) return;

            if (_specialCooldownTimer > 0f) return;

            // Countering is intentionally not blocked — specials can activate from the counter window.
            // SpecialAttackRoutine captures wasCounterWindow before transitioning to SpecialAttacking.
            if (State == CombatState.Staggered    || State == CombatState.Dodging    ||
                State == CombatState.Parrying      || State == CombatState.Attacking  ||
                State == CombatState.SpecialAttacking) return;

            StopActive();
            StartActive(SpecialAttackRoutine());
        }

        /// <summary>Instantly resets the special ability cooldown. Called by UpgradeScreen for the SpecialCooldownDown card.</summary>
        public void ResetSpecialCooldown() => _specialCooldownTimer = 0f;

        /// <summary>Called by AbilityExecutor after firing a V4 weapon ability so the HUD
        /// charge meter reflects the ability cooldown rather than staying fully charged.</summary>
        public void SetSpecialCooldownTimer(float duration) => _specialCooldownTimer = duration;

        public void OnWeaponEquipped(WeaponAbilityData ability)
        {
            _currentAbility = ability;
            OnSpecialEquipped?.Invoke(ability);
        }

        // ── Called by enemy when its attack resolves ─────────────────────────

        public AttackResult TryReceiveAttack(int damage, bool parryable = true, GameObject attacker = null)
        {
            // Dodge invincibility: Ninja passive grants full i-frames; all other states fall through.
            // Cowboy (WiderParryWindow passive) is NOT invincible during dodge by design.
            if (State == CombatState.Dodging)
            {
                if (_activeStyle == null || _activeStyle.Passive == PassiveType.DodgeInvincibility)
                    return AttackResult.Dodged;
            }

            // Un-parryable attacks (e.g. DrumSlam) skip the parry branch entirely.
            if (parryable && State == CombatState.Parrying)
            {
                _lastAttacker = attacker;
                StopActive();
                StartActive(CounterWindowRoutine());
                AudioManager.Instance?.Play(SoundEvent.ParrySuccess);
                if (_parryRingVFX != null) _parryRingVFX.Play();
                _stats?.Heal(_parryHealAmount);
                OnParrySuccess?.Invoke();

                // Parry flash VFX
                if (attacker != null && _parryFlashMaterial != null)
                {
                    if (_parryFlashRoutine != null) StopCoroutine(_parryFlashRoutine);
                    _parryFlashRoutine = StartCoroutine(ParryFlashRoutine(attacker));
                }

                return AttackResult.Parried;
            }

            AudioManager.Instance?.Play(SoundEvent.PlayerHit);
            if (_hitSparkVFX != null) _hitSparkVFX.Play();
            // Subtract flat defense bonus from overlay (minimum 1 damage always lands)
            int defense = Boxhead.Core.ProgressionSystem.Instance?.TotalOverlay.defenseBonus ?? 0;
            damage = Mathf.Max(1, damage - defense);
            _stats.TakeDamage(damage);
            // Reset the in-run combo multiplier — taking a hit breaks the streak.
            Core.ProgressionSystem.Instance?.ResetCombo();
            _pendingAerialAttack = false;
            StopActive();
            StartActive(StaggerRoutine());
            OnPlayerStaggered?.Invoke();

            // Hit stop: freeze player + attacker; attacker gets the heavier feel
            var attackerAnim = attacker != null ? attacker.GetComponentInChildren<Animator>() : null;
            Core.HitStopManager.Instance?.TriggerHitStop(attackerAnim);

            return AttackResult.Hit;
        }

        // ── Called by PlayerController on landing ────────────────────────────

        /// <summary>
        /// Invoked by PlayerController the frame the character transitions from airborne to grounded.
        /// Resolves any pending aerial slam attack.
        /// </summary>
        public void NotifyLanded()
        {
            if (!_pendingAerialAttack) return;
            if (_stats != null && _stats.IsDead) return;

            _pendingAerialAttack = false;

            int slamDamage = Mathf.RoundToInt(attackDamage * 1.5f);
            // Use box damage as the base if a box is equipped — consistent with ground attack
            if (_boxSystem != null && _boxSystem.CurrentBox != null)
                slamDamage = Mathf.RoundToInt(_boxSystem.CurrentBox.attackDamage * 1.5f);

            // Apply the style's aerial damage multiplier if a style is active
            if (_activeStyle != null)
                slamDamage = Mathf.RoundToInt(slamDamage * _activeStyle.AerialDamageMultiplier);

            // Apply weapon damage multiplier from active forged weapon, or V3 inventory fallback.
            var activeWeapon = _weaponInventory?.ActiveWeapon;
            if (activeWeapon != null)
                slamDamage = Mathf.RoundToInt(slamDamage * activeWeapon.Data.damageMultiplier);
            else if (_inventory != null && _inventory.EquippedSlot != null)
                slamDamage = Mathf.RoundToInt(slamDamage * _inventory.EquippedSlot.damageMultiplier);

            // Style-dependent radius and knockback direction
            float slamRadius;
            bool pullTowardPlayer; // true = LassoSlam pull, false = DiveKick knockback

            if (_activeStyle != null && _activeStyle.AerialAttack == AerialAttackType.DiveKick)
            {
                slamRadius = 1f;
                pullTowardPlayer = false;
            }
            else if (_activeStyle != null && _activeStyle.AerialAttack == AerialAttackType.LassoSlam)
            {
                slamRadius = 1.5f;
                pullTowardPlayer = true;
            }
            else
            {
                // No style: fallback generic slam — keep the existing serialized radius
                slamRadius = _aerialSlamRadius;
                pullTowardPlayer = false;
            }

            int count = Physics.OverlapSphereNonAlloc(transform.position, slamRadius, _slamHitBuffer, _enemyLayer);
            int hitCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (!_slamHitBuffer[i].CompareTag("Enemy")) continue;
                if (!_slamHitBuffer[i].TryGetComponent<Enemy.EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;
                bool seen = false;
                for (int j = 0; j < hitCount; j++)
                {
                    if (_slamStatsBuffer[j] == stats) { seen = true; break; }
                }
                if (seen) continue;
                if (hitCount >= _slamStatsBuffer.Length) break;
                _slamStatsBuffer[hitCount++] = stats;
                stats.TakeDamage(slamDamage);

                // Apply knockback / pull if the enemy has a Rigidbody
                if (_activeStyle != null && _slamHitBuffer[i].TryGetComponent<Rigidbody>(out var rb))
                {
                    Vector3 forceDir = pullTowardPlayer
                        ? (transform.position - _slamHitBuffer[i].transform.position).normalized
                        : (_slamHitBuffer[i].transform.position - transform.position).normalized;
                    rb.AddForce(forceDir * 5f, ForceMode.Impulse);
                }
            }

            OnAerialSlam?.Invoke();

            // Spawn aerial slam VFX
            if (_aerialSlamVfx != null)
            {
                Instantiate(_aerialSlamVfx, transform.position, Quaternion.identity);
            }

            State = CombatState.Idle;
        }

        // ── State routines ────────────────────────────────────────────────────

        private IEnumerator AttackRoutine()
        {
            _attackQueued = false;
            IsInHitPhase = true;
            State = CombatState.Attacking;
            if (_animator != null)
            {
                _animator.ResetTrigger(AnimAttack);
                _animator.speed = _attackAnimSpeedMultiplier;
                int attackHash = _animator.GetInteger(WeaponTypeHash) == 0
                    ? AttackStateHash
                    : SwordAttackStateHash;
                _animator.CrossFadeInFixedTime(attackHash, 0.05f, 0, 0f);
            }
            yield return _waitAttackHit;

            // Keep speed at 1.5x for the full attack — the clip is 1.033s so at 1.5x
            // it completes in ~0.69s, fitting cleanly inside the 0.7s attackDuration.
            // Previously resetting speed to 1x here left 77% of the animation played,
            // causing the character to hold a mid-swing pose during recovery.
            IsInHitPhase = false;
            ApplyAttackDamage();

            // If a tap arrived during the hit phase, fire immediately — don't make the
            // player wait through the full recovery window before the next swing starts.
            if (_attackQueued && _stats != null && !_stats.IsDead)
            {
                _attackQueued = false;
                AudioManager.Instance?.Play(SoundEvent.PlayerAttack);
                StartActive(AttackRoutine());
                yield break;
            }

            yield return _waitAttackRecovery;
            if (State == CombatState.Attacking)
                State = CombatState.Idle;

            if (_attackQueued && _stats != null && !_stats.IsDead)
            {
                _attackQueued = false;
                AudioManager.Instance?.Play(SoundEvent.PlayerAttack);
                StartActive(AttackRoutine());
            }
            else
            {
                if (_animator != null)
                    _animator.speed = 1f;
                CrossFadeToLocomotion();
            }
        }

        /// <summary>
        /// Triggers the standard attack animation from an external caller (e.g., an ability
        /// that wants to show a throw animation during its wind-up without a dedicated clip).
        /// </summary>
        public void TriggerAttackAnimation()
        {
            if (_animator == null) return;
            int attackHash = _animator.GetInteger(WeaponTypeHash) == 0
                ? AttackStateHash
                : SwordAttackStateHash;
            _animator.CrossFadeInFixedTime(attackHash, 0.05f, 0, 0f);
        }

        private void CrossFadeToLocomotion()
        {
            if (_animator == null) return;
            int locoHash = _animator.GetInteger(WeaponTypeHash) == 0
                ? LocoStateHash
                : SwordLocoStateHash;
            _animator.CrossFadeInFixedTime(locoHash, 0.15f, 0);
        }

        private void ApplyAttackDamage()
        {
            // Capture once — avoids re-fetching the property per enemy hit (zero extra alloc).
            var activeWeapon = _weaponInventory?.ActiveWeapon;
            int count = Physics.OverlapSphereNonAlloc(transform.position, attackRadius, _attackHitBuffer, _enemyLayer);
            int hitCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (!_attackHitBuffer[i].CompareTag("Enemy")) continue;
                if (!_attackHitBuffer[i].TryGetComponent<Enemy.EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;
                bool seen = false;
                for (int j = 0; j < hitCount; j++)
                {
                    if (_hitStatsBuffer[j] == stats) { seen = true; break; }
                }
                if (seen) continue;
                if (hitCount >= _hitStatsBuffer.Length) break;
                _hitStatsBuffer[hitCount++] = stats;
                int damage = (_boxSystem != null && _boxSystem.CurrentBox != null)
                    ? _boxSystem.CurrentBox.attackDamage
                    : attackDamage;

                // Apply weapon damage multiplier from active forged weapon, or V3 inventory fallback.
                if (activeWeapon != null)
                    damage = Mathf.RoundToInt(damage * activeWeapon.Data.damageMultiplier);
                else if (_inventory != null && _inventory.EquippedSlot != null)
                    damage = Mathf.RoundToInt(damage * _inventory.EquippedSlot.damageMultiplier);

                // Apply permanent + in-run attack power bonus from stat overlay
                damage += Boxhead.Core.ProgressionSystem.Instance?.TotalOverlay.attackPowerBonus ?? 0;

                // Apply one-shot crit multiplier set by AbilityExecutor; resets after first enemy hit.
                if (_critMultiplier != 1f)
                {
                    damage = Mathf.RoundToInt(damage * _critMultiplier);
                    _critMultiplier = 1f;
                }

                stats.TakeDamage(damage);
                _weaponDurability?.RegisterHit(activeWeapon);

                // Trigger hit stop on the first enemy struck per swing
                if (hitCount == 1)
                {
                    var enemyAnim = _attackHitBuffer[i].GetComponentInParent<Animator>();
                    Core.HitStopManager.Instance?.TriggerHitStop(enemyAnim);
                }
            }
        }

        private IEnumerator DodgeRoutine()
        {
            State = CombatState.Dodging;
            if (_animator != null)
            {
                _animator.ResetTrigger(AnimDodge);
                _animator.CrossFadeInFixedTime(DodgeStateHash, 0f, 0);
            }
            // Wait for the animation's wind-up frames before applying physics movement,
            // so the visual step and the character's actual displacement stay in sync.
            yield return _waitDodgeDelay;
            float moveDuration = dodgeDuration - dodgeMovementDelay;
            float effectiveDodgeDist = (dodgeDistance + (Boxhead.Core.ProgressionSystem.Instance?.TotalOverlay.agilityBonus ?? 0f)) * _dodgeDistanceMultiplier;
            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                _controller.Move(_dodgeDirection * (effectiveDodgeDist / moveDuration) * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            CrossFadeToLocomotion();
            State = CombatState.Idle;
        }

        private IEnumerator ParryRoutine()
        {
            State = CombatState.Parrying;
            _animator?.SetTrigger(AnimParry);
            yield return _waitParry;
            if (State == CombatState.Parrying)
            {
                State = CombatState.Idle;
                CrossFadeToLocomotion();
            }
        }

        private IEnumerator CounterWindowRoutine()
        {
            State = CombatState.Countering;
            AudioManager.Instance?.Play(SoundEvent.CounterWindowOpen);
            OnCounterWindowOpened?.Invoke();
            yield return _waitCounter;
            if (State == CombatState.Countering)
            {
                _lastAttacker = null;
                State = CombatState.Idle;
                OnCounterWindowClosed?.Invoke();
            }
        }

        private IEnumerator StaggerRoutine()
        {
            State = CombatState.Staggered;
            _animator?.SetTrigger(AnimStagger);
            yield return _waitStagger;
            State = CombatState.Idle;
            CrossFadeToLocomotion();
        }

        private IEnumerator SpecialAttackRoutine(bool fromAttackButton = false)
        {
            OnSpecialActivated?.Invoke();

            // Capture counter-window flag BEFORE changing state — the guard in OnSpecialAttack
            // allows Countering, so wasCounterWindow can legitimately be true here.
            bool wasCounterWindow = (State == CombatState.Countering);
            GameObject capturedAttacker = _lastAttacker;
            _lastAttacker = null;   // clear so destroyed-enemy reference doesn't dangle during the ability
            State = CombatState.SpecialAttacking;

            // Style special fires from the Special button — unless an Epic/Legendary weapon ability
            // handles OnSpecial, in which case the weapon ability replaces the style special entirely.
            // fromAttackButton=true: ranged weapons (Dynamite, Shuriken) always bypass the style.
            bool weaponAbilityHandlesSpecial = _abilityExecutor != null && _abilityExecutor.HasActiveSpecialAbility;
            if (_activeStyle != null && !fromAttackButton && !weaponAbilityHandlesSpecial)
            {
                yield return ExecuteStyleSpecial();
                _specialCooldownTimer = _activeStyle.SpecialCooldownDuration;
            }
            else
            {
                Vector3 muzzlePos = _weaponHolder != null
                    ? _weaponHolder.MuzzlePosition
                    : transform.position + (-transform.forward) * 0.5f + Vector3.up * 0.5f;

                var ctx = new AbilityActivationContext(
                    transform.position,
                    -transform.forward,   // -forward = visual facing direction (Y=180° child offset)
                    muzzlePos,
                    wasCounterWindow,
                    capturedAttacker,
                    _enemyLayer);

                yield return StartCoroutine(_currentAbility.Activate(ctx, this));
                _specialCooldownTimer = _currentAbility != null ? _currentAbility.CooldownDuration : 0f;
            }

            if (State == CombatState.SpecialAttacking)
                State = CombatState.Idle;
        }

        // ── Style specials ────────────────────────────────────────────────────

        private IEnumerator ExecuteStyleSpecial()
        {
            switch (_activeStyle.SpecialMove)
            {
                case SpecialMoveType.ShadowDash:
                    yield return ShadowDashCoroutine();
                    break;
                case SpecialMoveType.Tumbleshot:
                    yield return TumbleshotCoroutine();
                    break;
                default:
                    Debug.LogWarning($"[CombatController] No implementation for SpecialMove={_activeStyle.SpecialMove}");
                    break;
            }
        }

        private IEnumerator ShadowDashCoroutine()
        {
            if (_playerRenderer == null || _shadowMaterial == null) yield break;

            // Facing direction: -forward because character root has 180° Y child offset
            Vector3 dashDir = (-transform.forward).normalized;
            float dashDistance = _activeStyle.ShadowDashDistance;

            // Swap to shadow material for the duration of the dash
            _playerRenderer.sharedMaterial = _shadowMaterial;

            // Teleport (step-move over 0.25s so CharacterController collision still resolves)
            float elapsed = 0f;
            float speed = dashDistance / 0.25f;
            while (elapsed < 0.25f)
            {
                float step = speed * Time.deltaTime;
                _controller.Move(dashDir * step);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Detect enemies hit during the dash using a capsule along the dash path.
            // Lift both endpoints by Vector3.up so the capsule axis sits at the character's
            // center height (~Y+1) rather than at foot level — this ensures it overlaps
            // enemies whose colliders are elevated (e.g. SpinCycle's SphereCollider at Y=1.5).
            Vector3 offset = Vector3.up;
            Vector3 point1 = transform.position - dashDir * dashDistance + offset;
            Vector3 point2 = transform.position + offset;
            int hitCount = Physics.OverlapCapsuleNonAlloc(point1, point2, 0.8f, _dashHitBuffer, _enemyLayer);
            int dashHitCount = 0;
            for (int i = 0; i < hitCount; i++)
            {
                if (!_dashHitBuffer[i].CompareTag("Enemy")) continue;
                // Use GetComponentInParent so a collider on a child bone still resolves to
                // the EnemyStats on the enemy root (e.g. SpinCycle's collider hierarchy).
                var stats = _dashHitBuffer[i].GetComponentInParent<Enemy.EnemyStats>();
                if (stats == null || stats.IsDead) continue;
                bool seen = false;
                for (int j = 0; j < dashHitCount; j++)
                    if (_dashStatsBuffer[j] == stats) { seen = true; break; }
                if (seen) continue;
                if (dashHitCount >= _dashStatsBuffer.Length) break;
                _dashStatsBuffer[dashHitCount++] = stats;
                stats.TakeDamage(_activeStyle.ShadowDashDamage);
            }

            // Restore original material (use sharedMaterial to avoid per-instance leak)
            if (_playerRenderer != null)
                _playerRenderer.sharedMaterial = _originalMaterial;
        }

        private IEnumerator TumbleshotCoroutine()
        {
            if (_tumbleshotPrefab == null) yield break;

            Vector3 facingDir = (-transform.forward).normalized;
            Vector3 spawnPos = transform.position + facingDir * 0.6f + Vector3.up * 0.8f;

            GameObject bulletGO = Instantiate(_tumbleshotPrefab, spawnPos, Quaternion.LookRotation(facingDir));
            if (bulletGO.TryGetComponent<TumbleshotBullet>(out var bullet))
            {
                bullet.Damage   = _activeStyle.TumbleshotDamage;
                bullet.Speed    = _activeStyle.TumbleshotSpeed;
                bullet.MaxRange = _activeStyle.TumbleshotRange;
            }

            // Instantaneous fire — no wait needed; the bullet handles its own lifecycle
            yield break;
        }

        // ── Shared helpers ────────────────────────────────────────────────────

        private void ExecuteCounterStrike()
        {
            StopActive();
            State = CombatState.Idle;
            _stats?.Heal(_counterHealAmount);
            OnCounterStrike?.Invoke(_lastAttacker);
            _lastAttacker = null;
            OnCounterWindowClosed?.Invoke();
        }

        /// <summary>
        /// Public wrapper around ExecuteCounterStrike for AbilityBehaviour subclasses
        /// (e.g. RightBackBehaviour) that need to auto-trigger a counter after a successful block.
        /// Only fires when the player is in the Counter window; silently no-ops otherwise.
        /// </summary>
        public void TriggerCounterStrike()
        {
            if (State != CombatState.Countering) return;
            ExecuteCounterStrike();
        }

        private IEnumerator ParryFlashRoutine(GameObject enemy)
        {
            if (enemy == null) yield break;
            if (_parryFlashMaterial == null) yield break;
            Renderer r = enemy.GetComponentInChildren<Renderer>();
            if (r == null) yield break;
            // Capture original BEFORE assigning — guard against concurrent flash routines
            // on the same renderer: only restore if this routine was the last one to apply the flash.
            Material original = r.sharedMaterial;
            if (original == _parryFlashMaterial) yield break; // already flashing from another routine
            r.sharedMaterial = _parryFlashMaterial;
            yield return _waitParryFlash;
            // Only restore if the material hasn't been changed again by another system
            // (e.g. enemy death mid-flash). This prevents leaving a dead enemy with the flash mat.
            if (r != null && r.sharedMaterial == _parryFlashMaterial)
                r.sharedMaterial = original;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

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
            if (_animator != null) _animator.speed = 1f;
            _attackQueued = false;
            IsInHitPhase = false;

            // If the shadow dash was interrupted mid-flash, restore the original material
            if (_playerRenderer != null && _shadowMaterial != null
                && _playerRenderer.sharedMaterial == _shadowMaterial)
                _playerRenderer.sharedMaterial = _originalMaterial;
        }

        private void OnDestroy()
        {
            _pendingAerialAttack = false;
            StopActive();
            if (_parryFlashRoutine != null) StopCoroutine(_parryFlashRoutine);
            if (_animator != null) _animator.speed = 1f;
            if (_boxSystem != null) _boxSystem.OnModelChanged -= RefreshAnimator;
            Enemy.EnemyStats.OnAnyEnemyDeath -= OnEnemyKilled;

            if (_shadowMaterial != null)
            {
                _shadowMaterial.hideFlags = HideFlags.None;
                Destroy(_shadowMaterial);
                _shadowMaterial = null;
            }
        }
    }
}
