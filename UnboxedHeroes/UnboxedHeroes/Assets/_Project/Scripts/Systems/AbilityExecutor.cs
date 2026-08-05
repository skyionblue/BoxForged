using System;
using UnityEngine;
using Boxhead.Player;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    /// <summary>
    /// Reads the active WeaponInstance's AbilitySO from WeaponInventory and routes combat
    /// events (hit, dodge, parry, special, stagger) to the correct ability trigger.
    /// Handles all inline Phase 1 effects (AoeSweep, CounterStrike, DisableDurability,
    /// RestoreDurability, DodgeDistanceMult) and delegates complex Phase 3 logic to
    /// AbilityBehaviour.Execute.
    ///
    /// Architecture note: DisableDurability is implemented as a compensating restore —
    /// WeaponDurability.RegisterHit always decrements before firing OnWeaponDamaged, so
    /// we immediately call RestoreDurability(1) in a dedicated handler, achieving net-zero
    /// durability loss without modifying WeaponDurability itself.
    /// </summary>
    [RequireComponent(typeof(WeaponInventory))]
    [RequireComponent(typeof(CombatController))]
    [RequireComponent(typeof(WeaponDurability))]
    [RequireComponent(typeof(WeaponHolder))]
    public class AbilityExecutor : MonoBehaviour
    {
        [SerializeField] private LayerMask _enemyLayer;

        private WeaponInventory  _inventory;
        private CombatController _combat;
        private WeaponDurability _durability;
        private WeaponHolder     _weaponHolder;

        private AbilitySO _activeAbility;
        private float     _cooldownTimer;
        private int       _hitCounter;       // for CounterStrike: counts hits since last trigger

        /// <summary>True when the equipped weapon has an OnSpecial ability — CombatController
        /// uses this to suppress the style special so the weapon ability fires instead.</summary>
        public bool HasActiveSpecialAbility =>
            _activeAbility != null && _activeAbility.trigger == AbilityTrigger.OnSpecial;

        /// <summary>
        /// True when the equipped weapon has an OnDodge ability (e.g. DrawAttack).
        /// CombatController checks this in OnDodge to intercept the input before the standard
        /// dodge coroutine starts — the ability replaces the dodge entirely for that press.
        /// </summary>
        public bool HasActiveDodgeAbility =>
            _activeAbility != null && _activeAbility.trigger == AbilityTrigger.OnDodge;

        /// <summary>
        /// Called by CombatController when HasActiveDodgeAbility is true. Bypasses the trigger
        /// filter (already confirmed by the caller) and runs ExecuteAbility directly so the
        /// ability fires on the same frame the dodge button was pressed.
        /// </summary>
        public void FireDodgeAbility()
        {
            if (_activeAbility == null) return;
            if (_cooldownTimer > 0f) return;
            ExecuteAbility();
        }

        // Pre-allocated physics buffers — zero GC in Update/callbacks.
        private readonly Collider[]   _sweepBuffer      = new Collider[8];
        private readonly EnemyStats[] _sweepStatsBuffer = new EnemyStats[8];

        // Delegate instances held as fields so we can subscribe/unsubscribe the exact same
        // reference — anonymous lambdas create a new object each time and can't be removed.
        private Action                _onDodgeTrigger;
        private Action                _onBlockTrigger;
        private Action                _onSpecialTrigger;
        private Action                _onPlayerStaggeredTrigger;
        private Action<WeaponInstance> _onHitTrigger;
        private Action                _onInventoryChanged;

        // Separate handler for DisableDurability passive — compensates the decrement that
        // WeaponDurability already applied before firing OnWeaponDamaged.
        private Action<WeaponInstance> _disableDurabilityHandler;

        private void Awake()
        {
            _inventory    = GetComponent<WeaponInventory>();
            _combat       = GetComponent<CombatController>();
            _durability   = GetComponent<WeaponDurability>();
            _weaponHolder = GetComponent<WeaponHolder>();

            // Cache delegates once in Awake to avoid per-subscription allocations.
            _onDodgeTrigger           = OnDodgeTrigger;
            _onBlockTrigger           = OnBlockTrigger;
            _onSpecialTrigger         = OnSpecialTrigger;
            _onPlayerStaggeredTrigger = OnPlayerStaggeredTrigger;
            _onHitTrigger             = OnHitTrigger;
            _onInventoryChanged       = OnActiveWeaponChanged;
            _disableDurabilityHandler = OnDisableDurabilityCompensate;
        }

        private void OnEnable()
        {
            _inventory.OnInventoryChanged    += _onInventoryChanged;
            _combat.OnDodgeStarted           += _onDodgeTrigger;
            _combat.OnParrySuccess           += _onBlockTrigger;
            _combat.OnSpecialActivated       += _onSpecialTrigger;
            _combat.OnPlayerStaggered        += _onPlayerStaggeredTrigger;
            _durability.OnWeaponDamaged      += _onHitTrigger;
        }

        private void OnDisable()
        {
            // Notify outgoing behaviour so it can unsubscribe its own event listeners.
            _activeAbility?.behaviour?.OnUnequipped();

            _inventory.OnInventoryChanged    -= _onInventoryChanged;
            _combat.OnDodgeStarted           -= _onDodgeTrigger;
            _combat.OnParrySuccess           -= _onBlockTrigger;
            _combat.OnSpecialActivated       -= _onSpecialTrigger;
            _combat.OnPlayerStaggered        -= _onPlayerStaggeredTrigger;
            _durability.OnWeaponDamaged      -= _onHitTrigger;
            // Ensure compensating handler is removed if it was active.
            _durability.OnWeaponDamaged      -= _disableDurabilityHandler;
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;
        }

        // ── Inventory change ──────────────────────────────────────────────────

        private void OnActiveWeaponChanged()
        {
            // Unequip the outgoing ability's behaviour before clearing the reference.
            _activeAbility?.behaviour?.OnUnequipped();
            RevertPassive();

            WeaponInstance weapon = _inventory.ActiveWeapon;
            AbilitySO newAbility = null;

            if (weapon != null)
            {
                newAbility = weapon.Tier switch
                {
                    WeaponTier.Legendary => weapon.Data.legendaryAbility,
                    WeaponTier.Epic      => weapon.Data.epicAbility,
                    _                    => null
                };
            }

            _activeAbility = newAbility;
            _cooldownTimer = 0f;
            _hitCounter    = 0;

            if (_activeAbility != null && _activeAbility.trigger == AbilityTrigger.Passive)
                ApplyPassive();

            // Notify the new ability's behaviour it is now equipped.
            if (_activeAbility?.behaviour != null)
                _activeAbility.behaviour.OnEquipped(BuildContext());
        }

        // ── Trigger callbacks ─────────────────────────────────────────────────

        private void OnHitTrigger(WeaponInstance weapon)
        {
            if (_activeAbility == null) return;
            if (_activeAbility.trigger != AbilityTrigger.OnHit) return;
            // Only react to hits with the currently active weapon.
            if (!ReferenceEquals(weapon, _inventory.ActiveWeapon)) return;
            if (_cooldownTimer > 0f) return;

            // CounterStrike tracks hit count before checking cooldown — magnitude acts as threshold.
            if (_activeAbility.effectType == AbilityEffectType.CounterStrike)
            {
                _hitCounter++;
                if (_hitCounter < (int)_activeAbility.magnitude) return;
                _hitCounter = 0;
            }

            ExecuteAbility();
        }

        private void OnDodgeTrigger()
        {
            if (_activeAbility == null) return;
            if (_activeAbility.trigger != AbilityTrigger.OnDodge) return;
            if (_cooldownTimer > 0f) return;
            ExecuteAbility();
        }

        private void OnBlockTrigger()
        {
            if (_activeAbility == null) return;
            if (_activeAbility.trigger != AbilityTrigger.OnBlock) return;
            if (_cooldownTimer > 0f) return;
            ExecuteAbility();
        }

        private void OnSpecialTrigger()
        {
            if (_activeAbility == null) return;
            if (_activeAbility.trigger != AbilityTrigger.OnSpecial) return;
            if (_cooldownTimer > 0f) return;
            ExecuteAbility();
        }

        private void OnPlayerStaggeredTrigger()
        {
            // RestoreDurability triggers on stagger regardless of the ability's formal trigger field.
            // This lets a Passive ability restore durability whenever the player takes a hit.
            if (_activeAbility == null) return;
            if (_activeAbility.effectType != AbilityEffectType.RestoreDurability) return;
            _inventory.ActiveWeapon?.RestoreDurability((int)_activeAbility.magnitude);
        }

        // ── Execution ─────────────────────────────────────────────────────────

        private void ExecuteAbility()
        {
            // Play attack animation so the ability has visible character feedback
            _combat.TriggerAttackAnimation();

            // VFX
            if (_activeAbility.vfxPrefab != null)
            {
                Vector3 spawnPos = _weaponHolder != null
                    ? _weaponHolder.MuzzlePosition
                    : transform.position;
                UnityEngine.Object.Instantiate(_activeAbility.vfxPrefab, spawnPos, Quaternion.identity);
            }

            // SFX
            Core.AudioManager.Instance?.PlayClip(_activeAbility.sfx);

            // Inline data-driven effect
            ApplyInlineEffect();

            // Complex custom behaviour (Phase 3)
            _activeAbility.behaviour?.Execute(BuildContext());

            _cooldownTimer = _activeAbility.cooldown;
            // Sync with CombatController so the HUD special charge meter reflects this cooldown
            if (_activeAbility.cooldown > 0f)
                _combat.SetSpecialCooldownTimer(_activeAbility.cooldown);
        }

        private void ApplyInlineEffect()
        {
            switch (_activeAbility.effectType)
            {
                case AbilityEffectType.AoeSweep:
                    ApplyAoeSweep();
                    break;

                case AbilityEffectType.CounterStrike:
                    ApplyCounterStrike();
                    break;

                // DisableDurability is purely passive — handled in ApplyPassive/RevertPassive.
                // No runtime inline effect when it fires.
                case AbilityEffectType.DisableDurability:
                    break;

                // RestoreDurability inline path (non-stagger triggers — e.g. OnBlock, OnDodge).
                case AbilityEffectType.RestoreDurability:
                    _inventory.ActiveWeapon?.RestoreDurability((int)_activeAbility.magnitude);
                    break;

                // DodgeDistanceMult is passive — applied at equip. No per-trigger inline effect.
                case AbilityEffectType.DodgeDistanceMult:
                    break;

                case AbilityEffectType.CritMultiplier:
                    ApplyCritMultiplier();
                    break;

                case AbilityEffectType.AoeKnockback:
                    ApplyAoeKnockback();
                    break;

                // ExplosionRadiusMult is passive — applied at equip. No per-trigger inline effect.
                case AbilityEffectType.ExplosionRadiusMult:
                    break;

                // Phase 3 effects — no-op for now.
                default:
                    break;
            }
        }

        // ── Inline effect implementations ─────────────────────────────────────

        private void ApplyAoeSweep()
        {
            float radius = _activeAbility.magnitude;
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _sweepBuffer, _enemyLayer);
            int hitCount = 0;

            // Base damage mirrors CombatController.ApplyAttackDamage scale: damageMultiplier * 15.
            WeaponInstance activeWeapon = _inventory.ActiveWeapon;
            float damageMultiplier = activeWeapon?.Data.damageMultiplier ?? 1f;
            int baseDamage = Mathf.RoundToInt(damageMultiplier * 15f);

            for (int i = 0; i < count; i++)
            {
                if (!_sweepBuffer[i].CompareTag("Enemy")) continue;
                if (!_sweepBuffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;

                // Deduplicate — same enemy may have multiple colliders in the overlap.
                bool seen = false;
                for (int j = 0; j < hitCount; j++)
                {
                    if (_sweepStatsBuffer[j] == stats) { seen = true; break; }
                }
                if (seen) continue;
                if (hitCount >= _sweepStatsBuffer.Length) break;

                _sweepStatsBuffer[hitCount++] = stats;
                stats.TakeDamage(baseDamage);
            }
            Debug.Log($"[AbilityExecutor] The Morning Sweep: hit {hitCount} enemies in radius {radius}m for {baseDamage} damage each.");
        }

        private void ApplyCounterStrike()
        {
            // Find the nearest enemy within a generous radius and apply a hit stagger.
            // We use the sweep buffer here — radius is fixed at 3 units (slightly larger
            // than standard attack range) to catch the enemy the player has been fighting.
            int count = Physics.OverlapSphereNonAlloc(transform.position, 3f, _sweepBuffer, _enemyLayer);
            EnemyStats nearest = null;
            BasicEnemyAI nearestAI = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!_sweepBuffer[i].CompareTag("Enemy")) continue;
                if (!_sweepBuffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;

                float dist = (_sweepBuffer[i].transform.position - transform.position).sqrMagnitude;
                if (dist >= nearestDist) continue;

                nearestDist = dist;
                nearest     = stats;
                _sweepBuffer[i].TryGetComponent(out nearestAI);
            }

            if (nearest == null) return;

            if (nearestAI != null)
                nearestAI.ApplyHitStagger();
            else
                nearest.TakeDamage(0); // fallback: fire OnHit events without dealing damage
        }

        private void ApplyCritMultiplier()
        {
            // Tell CombatController to multiply the very next hit by magnitude.
            // The multiplier is consumed and reset to 1f inside ApplyAttackDamage after the first
            // enemy is struck — so only one hit per activation is amplified.
            _combat.SetNextHitCritMultiplier(_activeAbility.magnitude);
        }

        private void ApplyAoeKnockback()
        {
            float radius = _activeAbility.magnitude;
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _sweepBuffer, _enemyLayer);

            for (int i = 0; i < count; i++)
            {
                if (!_sweepBuffer[i].CompareTag("Enemy")) continue;
                if (_sweepBuffer[i].TryGetComponent<EnemyStats>(out var stats) && stats.IsDead) continue;
                if (!_sweepBuffer[i].TryGetComponent<Rigidbody>(out var rb)) continue;

                // Deduplicate — same enemy may have multiple colliders.
                bool seen = false;
                for (int j = 0; j < i; j++)
                {
                    if (_sweepBuffer[j] != null && _sweepBuffer[j].attachedRigidbody == rb)
                    {
                        seen = true;
                        break;
                    }
                }
                if (seen) continue;

                Vector3 dir = (_sweepBuffer[i].transform.position - transform.position).normalized;
                rb.AddForce(dir * 8f, ForceMode.Impulse);
            }
            Debug.Log($"[AbilityExecutor] Full Blast: AoE knockback in radius {radius}m.");
        }

        // ── Passive apply / revert ────────────────────────────────────────────

        private void ApplyPassive()
        {
            if (_activeAbility == null) return;

            switch (_activeAbility.effectType)
            {
                case AbilityEffectType.DisableDurability:
                    // Subscribe the compensating handler. It runs after WeaponDurability
                    // decrements durability but before any other listener sees the event.
                    _durability.OnWeaponDamaged += _disableDurabilityHandler;
                    break;

                case AbilityEffectType.DodgeDistanceMult:
                    _combat.SetDodgeDistanceMultiplier(_activeAbility.magnitude);
                    break;

                // ExplosionRadiusMult: stored for DynamiteProjectile to query via a static
                // accessor. Wired in Phase 3 when the projectile reads the active multiplier.
                case AbilityEffectType.ExplosionRadiusMult:
                    ActiveExplosionRadiusMult = _activeAbility.magnitude;
                    break;
            }
        }

        private void RevertPassive()
        {
            if (_activeAbility == null) return;

            switch (_activeAbility.effectType)
            {
                case AbilityEffectType.DisableDurability:
                    _durability.OnWeaponDamaged -= _disableDurabilityHandler;
                    break;

                case AbilityEffectType.DodgeDistanceMult:
                    _combat.SetDodgeDistanceMultiplier(1f);
                    break;

                case AbilityEffectType.ExplosionRadiusMult:
                    ActiveExplosionRadiusMult = 1f;
                    break;
            }
        }

        /// <summary>
        /// Current explosion radius multiplier applied by the Bigger Bang passive.
        /// DynamiteProjectile reads this at detonation time. 1f when no ability is active.
        /// </summary>
        public static float ActiveExplosionRadiusMult { get; private set; } = 1f;

        // Compensating restore for DisableDurability passive.
        // WeaponDurability.RegisterHit decrements before firing this event, so restoring 1
        // achieves net-zero durability loss without touching WeaponDurability's own logic.
        private void OnDisableDurabilityCompensate(WeaponInstance weapon)
        {
            if (!ReferenceEquals(weapon, _inventory.ActiveWeapon)) return;
            weapon.RestoreDurability(1);
        }

        // ── Context builder ───────────────────────────────────────────────────

        private AbilityExecutionContext BuildContext() => new AbilityExecutionContext(
            transform.position,
            -transform.forward,                        // -forward = visual facing (Y=180° child offset)
            _weaponHolder != null
                ? _weaponHolder.MuzzlePosition
                : transform.position,
            _inventory.ActiveWeapon,
            _enemyLayer,
            _combat
        );
    }
}
