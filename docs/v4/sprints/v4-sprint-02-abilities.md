# V4 Sprint 02 — Epic & Legendary Abilities

**Goal:** Implement the full 24-ability weapon system across 3 phases. Simple abilities ship first as pure data; complex abilities ship last with custom behaviours.
**Branch:** `feature/v4-sprint-02-abilities`
**Base branch:** `main`
**Design doc:** `docs/v4/design/weapon-creation-system.md`
**Ability names:** `docs/v4/design/ability-names.md`
**Status:** ✅ Complete — 2026-08-05

---

## Key Architectural Decisions

**1. Direct SO references, not string IDs.**
`WeaponObjectSO.epicAbilityId` and `legendaryAbilityId` (string fields) are replaced with `public AbilitySO epicAbility` and `public AbilitySO legendaryAbility`. String ID lookup requires a registry singleton and fails silently at runtime on typos. Direct SO references are validated at edit time by Unity.

**2. `AbilityExecutor` is a MonoBehaviour on the Player.**
It subscribes to `WeaponInventory.OnInventoryChanged`, `CombatController` events, and `WeaponDurability.OnWeaponDamaged`. It maintains per-ability cooldown timers and routes triggers to the active ability with zero GC in hot paths.

**3. Simple/medium abilities are data-driven via `AbilityEffectType` enum.**
No new MonoBehaviour per ability. `AbilityExecutor.ApplyInlineEffect()` switches on an enum value baked into `AbilitySO`. Complex abilities use `AbilityBehaviour` — an abstract ScriptableObject subclass dragged into `AbilitySO._behaviour`.

**4. V3 ability system untouched.**
`WeaponAbilityData`, `AbilityActivationContext`, and all 5 existing V3 ability implementations remain unchanged.

---

## New Scripts (15 total)

**Core system:**
- `AbilitySO.cs` — data asset defining trigger, effect type, magnitude, cooldown, VFX, SFX, and optional behaviour
- `AbilityExecutor.cs` — MonoBehaviour on Player; routes triggers, manages cooldowns, fires effects

**Abstract base:**
- `Abilities/AbilityBehaviour.cs` — abstract ScriptableObject; `Execute(ctx)`, `OnEquipped(ctx)`, `OnUnequipped()`

**Complex ability behaviours (11):**
- `DrawAttackBehaviour` — Katana Legendary (dodge → dash-attack)
- `LassoCaughtBehaviour` — Lasso Epic (1.5s root)
- `SpinAndThrowBehaviour` — Lasso Legendary (throw grabbed enemy as projectile)
- `MixedUpBehaviour` — Magic Wand Epic (enemy retargeting)
- `AllEightBehaviour` — Magic Wand Legendary (8-direction cast)
- `DynamiteSpreadBehaviour` — Dynamite Legendary (chain to cardboard pickups)
- `LightsOutBehaviour` — Lightsaber Epic (timed attacker blindness)
- `SendItBackBehaviour` — Lightsaber Legendary (per-room projectile deflect)
- `ThreePumpsBehaviour` — Pressure Cannon Epic (hold-attack charge)
- `HoldTheLineBehaviour` — Iron Standard Epic (hold-block melee deflect)
- `RightBackBehaviour` — Iron Standard Legendary (auto counter on block)

**New runtime component:**
- `EnemyThrowProjectile.cs` — applied at runtime to grabbed enemies for Spin and Throw

---

## Files Modified (additive only)

| File | Change |
|---|---|
| `WeaponObjectSO.cs` | Replace 2 string fields with `AbilitySO epicAbility` + `AbilitySO legendaryAbility` |
| `WeaponInstance.cs` | Add `RestoreDurability(int amount)` method |
| `CombatController.cs` | Add `SetDodgeDistanceMultiplier(float)` + dodge-intercept event for Katana Legendary |
| `IEnemyBehavior.cs` | Add `SetSpeedMultiplier(float, float)` + `SetTemporaryTarget(Transform, float)` |
| `BasicEnemyAI.cs` + AI implementors | Implement the 2 new interface methods |
| `AudioManager.cs` | Add `PlayClip(AudioClip clip, float volume)` one-shot method |
| `DynamiteProjectile.cs` | Expose `RadiusMultiplier` property |

---

## `AbilitySO` Field Specification

```
string        abilityId          // machine-readable, matches asset name
string        displayName        // flavor name (e.g. "The Morning Sweep")
string        flavorDescription  // tooltip text (e.g. "One spin. Every direction.")
AbilityTrigger trigger           // OnHit / OnSpecial / OnDodge / OnBlock / Passive
AbilityEffectType effectType     // for inline data-driven effects
float         magnitude          // radius, damage mult, duration, etc.
float         cooldown           // 0 = no cooldown
GameObject    vfxPrefab
AudioClip     sfx
AbilityBehaviour behaviour       // null = data-driven only
```

**`AbilityTrigger` enum:** `OnHit`, `OnSpecial`, `OnDodge`, `OnBlock`, `Passive`

**`AbilityEffectType` enum:** `None`, `AoeSweep`, `CounterStrike`, `DisableDurability`, `RestoreDurability`, `DodgeDistanceMult`, `CritMultiplier`, `AoeKnockback`, `ExplosionRadiusMult`

---

## Implementation Phases

### Phase 1 — Infrastructure + 4 Simple Abilities

1. Modify `WeaponObjectSO` — replace string fields with SO references
2. Create `AbilityTrigger` + `AbilityEffectType` enums
3. Create `AbilitySO.cs`
4. Create `AbilityBehaviour.cs` abstract base
5. Create `AbilityExecutor.cs` — subscriptions, cooldowns, VFX/SFX dispatch, inline effect switch
6. Add `SetDodgeDistanceMultiplier` to `CombatController`, `RestoreDurability` to `WeaponInstance`
7. Wire `AbilityExecutor` to Player prefab
8. Create 4 simple ability SO assets (pure data, no new C#):

| Ability | Trigger | Effect Type | Magnitude |
|---|---|---|---|
| Bo Staff — The Morning Sweep | OnSpecial | AoeSweep | 2.5 (radius) |
| Bo Staff — Third Strike Rule | OnHit | CounterStrike | 3 (every 3rd) |
| Foam Sword — It Never Breaks | Passive | DisableDurability | — |
| Foam Sword — Gets Stronger | Passive (OnPlayerStaggered) | RestoreDurability | 1 |

- `unity-code-reviewer` sign-off before Phase 2

### Phase 2 — Medium Abilities (~14 abilities)

Add `SetSpeedMultiplier` + `SetTemporaryTarget` to `IEnemyBehavior` and all AI implementors.

**Inline effects (pure data SO assets):**
- Quickdraw — The Long Dodge: Passive, DodgeDistanceMult, magnitude 1.5
- Katana — Clean Cut: OnHit, CritMultiplier, magnitude 2.0
- Pressure Cannon — Full Blast: OnSpecial, AoeKnockback, magnitude 3.0
- Dynamite — Bigger Bang: Passive, ExplosionRadiusMult, magnitude 1.5

**Small AbilityBehaviour subclasses (new code per ability):**
- Shurikens Epic — Fold and Return (bounce flag on ShurikenProjectile)
- Shurikens Legendary — Three at Once (3-projectile fan)
- Quickdraw Epic — The First Strike (per-combat first-hit crit)
- Lightsaber Epic — Lights Out (timed blindness VFX)
- Lightsaber Legendary — Send It Back (per-room deflect flag)
- Water Whip Epic — Come Here (pull enemy toward player)
- Water Whip Legendary — It Slows Them Down (SetSpeedMultiplier 0.7)
- Iron Standard Epic — Hold the Line (hold-block deflect)
- Iron Standard Legendary — Right Back (auto counter on parry)
- Pressure Cannon Epic — Three Pumps (hold-attack charge state)

- `unity-code-reviewer` sign-off before Phase 3

### Phase 3 — Complex Abilities (6 abilities)

- Katana Legendary — The Draw Attack
- Lasso Epic — Caught
- Lasso Legendary — Spin and Throw (+ `EnemyThrowProjectile.cs`)
- Magic Wand Epic — Mixed Up (+ `IEnemyBehavior.SetTemporaryTarget`)
- Magic Wand Legendary — All Eight
- Dynamite Legendary — It Spreads

- `unity-code-reviewer` final pass on all new scripts before merge

---

## Asset Naming Convention

Assets go in `Assets/_Project/ScriptableObjects/Weapons/Abilities/`

Format: `Ability_[WeaponName]_[Tier].asset`

Examples:
- `Ability_BoStaff_Epic.asset`
- `Ability_BoStaff_Legendary.asset`
- `Ability_Lasso_Epic.asset`

---

## Definition of Done

**Phase 1:**
- [ ] `AbilitySO.cs`, `AbilityExecutor.cs`, `AbilityBehaviour.cs` exist and compile
- [ ] `AbilityExecutor` on Player prefab, events wired
- [ ] 4 simple ability SO assets configured and tested in ForgeLoop_Test scene
- [ ] Foam Sword durability immunity confirmed (It Never Breaks)
- [ ] Bo Staff sweep hits multiple enemies (The Morning Sweep)
- [ ] `unity-code-reviewer` approved

**Phase 2:**
- [ ] 10 medium abilities all functional
- [ ] Water Whip correctly slows enemies
- [ ] Iron Standard block/counter timing feels correct
- [ ] `unity-code-reviewer` approved

**Phase 3:**
- [ ] Katana dodge-to-attack transition smooth
- [ ] Lasso grab → throw works on all enemy types
- [ ] Magic Wand confusion doesn't crash with 1 enemy in room
- [ ] Chain Reaction only triggers cardboard pickups (not weapons)
- [ ] 0 GC alloc on OnHit, OnSpecial trigger paths (Profiler verified)
- [ ] `unity-code-reviewer` final approval
- [ ] Branch merged to `main`

---

---

## Completion Notes — 2026-08-05

All 24 abilities implemented and tested across 3 phases:

**Phase 1 (4 simple abilities):** The Morning Sweep, Third Strike Rule, It Never Breaks, Gets Stronger — all working ✅

**Phase 2 (14 medium abilities):** The First Strike, The Long Dodge, Clean Cut, Fold and Return, Three at Once, Lights Out, Send It Back, Come Here, It Slows Them Down, Hold the Line, Right Back, Three Pumps, Full Blast, Bigger Bang — all working ✅

**Phase 3 (6 complex abilities):** The Draw Attack, Caught, Spin and Throw, Mixed Up, All Eight, It Spreads — all working ✅

**Design decisions made during testing:**
- Weapon ability replaces style special when Epic/Legendary weapon is equipped (OnSpecial trigger suppresses Shadow Dash)
- Durability decrements on enemy contact, not on every swing
- All Eight fires forward on casts 1-4, fires all 8 directions on every 5th cast
- Flashlight and Garden Hose set to Legendary rarity (they have Legendary abilities)
- QuickDraw and Ruler remain Common (Epic abilities accessible only in testing via rarity override)

*Sprint owner: Louie Celli | Created: 2026-08-04 | Completed: 2026-08-05*
