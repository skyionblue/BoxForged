# Unboxed Heroes — Weapon Creation System Design Document

**Version:** V4 Sprint (to be assigned)
**Status:** Decisions locked — ready for implementation
**Last updated:** 2026-08-03

---

## Context

The weapon creation system is the crown-jewel mechanic of Unboxed Heroes. The core fantasy: a kid in a post-apocalyptic neighborhood picks up ordinary household objects, wraps them in precious cardboard, and transforms them into legendary weapons. This document defines the full design and implementation plan — from data architecture through the forge UI — so the team can review and align before a single line of V4 code is written.

---

## Design Philosophy

Three feelings the player must experience:

1. **"I made that."** — The creativity of turning a broom into a Bo Staff. The weapon has the kid's name on it.
2. **"Watch it transform."** — A visible, satisfying visual moment when cardboard wraps around a household object and it becomes something more.
3. **"I should have saved my cardboard."** — Scarcity drives decisions. Cardboard is precious. Choosing when and what to forge creates tension.

---

## The Weapon Catalog

Twelve household objects. Each has a raw state (what it physically is) and an imagined state (what Kid calls it after forging). The flavor text lives in `docs/story/weapons/weapon-flavor-text.md` — these names are canonical.

Object rarity determines the ceiling of how far a weapon can be forged. Cardboard investment unlocks the tiers up to that ceiling.

### Common Objects → Standard ceiling only

| Raw Object | Forged Weapon | Type |
|---|---|---|
| Broomstick | Bo Staff | Fast / Melee |
| Ruler | Shurikens | Fast / Ranged |
| Foam Sword | Foam Sword | Fast / Melee |
| Garden Trowel | Quickdraw Blade | Fast / Melee |

### Rare Objects → Epic ceiling

| Raw Object | Forged Weapon | Type |
|---|---|---|
| Cardboard Tube | Katana | Balanced / Melee |
| Flashlight | Lightsaber | Balanced / Melee |
| Garden Hose | Water Whip | Balanced / Ranged |
| Jump Rope | Lasso | Balanced / Utility |

### Legendary Objects → Legendary ceiling

| Raw Object | Forged Weapon | Type |
|---|---|---|
| Bicycle Pump | Pressure Cannon | Powerful / Ranged |
| Remote Control | Magic Wand | Powerful / AoE |
| Lunchbox | Shield | Powerful / Defensive |
| Bike Horn | Dynamite Bundle | Powerful / AoE |

---

## Cardboard Resource Economy

Cardboard is a single stackable numeric resource. It drops from enemies, is found in dedicated loot zones, and is scattered throughout levels. It is **never** refunded when a weapon breaks.

| Action | Cardboard Cost |
|---|---|
| Forge Standard weapon (from any raw object) | 2 |
| Upgrade Standard → Epic (Rare objects only) | 5 |
| Upgrade Epic → Legendary (Legendary objects only) | 10 |

**Total cost to fully upgrade a Legendary-ceiling weapon:** 17 cardboard.

Cardboard persists through the current run. It resets at run start.

---

## Object Rarity and Tiers

### Pickup Rarity
Objects in the world have a visual rarity signal the player learns over time:

| Rarity | Visual Signal | Forge Ceiling |
|---|---|---|
| Common | No glow. Normal appearance. | Standard |
| Rare | Subtle gold shimmer / particle. | Epic |
| Legendary | Bright glow, floating slightly off ground, distinct aura. | Legendary |

### Forged Weapon Tiers

| Tier | Abilities Unlocked | Visual |
|---|---|---|
| Standard | Base stats only. No special ability. | Cardboard wrap, no glow. |
| Epic | 1 unique ability unlocked. | Cardboard wrap + colored edge glow (weapon-specific color). |
| Legendary | 2nd ability unlocked. Weapon "named" (shown in HUD). | Full glow + particle trail on swing. |

---

## Weapon Abilities

Each weapon unlocks abilities as it is upgraded. Abilities should feel like the weapon's personality, not just stat bumps.

| Weapon | Epic Ability | Legendary Ability |
|---|---|---|
| Bo Staff | Spin Strike — one 360° sweep hit on special | Stagger — every 3rd hit staggers the enemy |
| Shurikens | Ricochet — shurikens bounce once off walls | Triple Throw — fires 3 per throw |
| Foam Sword | Unbreakable — this weapon has no durability loss | Resilience — taking a hit restores 1 durability |
| Quickdraw Blade | Flash Draw — first hit of each combat is a guaranteed crit | Ghost Step — dodge distance +50% when weapon is equipped |
| Katana | One Cut — crits deal double damage | Iaijutsu — instant dash-attack on dodge input |
| Lightsaber | Parry Flash — successful parry blinds the attacker briefly | Deflect — can deflect one projectile per room |
| Water Whip | Pull — long-range attack pulls enemy toward player | Soaked — whip hits reduce enemy movement speed 30% |
| Lasso | Grab — catches enemy and holds them for 1.5s | Rodeo — grabbed enemies become throwable projectiles |
| Pressure Cannon | Charge — hold attack to charge for 2× damage | Blast Wave — charged shot creates AoE knockback |
| Magic Wand | Confusion — hit enemies occasionally attack each other | Overload — every 5th cast fires in all 8 directions |
| Lunchbox Shield | Block — hold block button to deflect melee attacks | Counter — successful block triggers an automatic counter-hit |
| Dynamite Bundle | Wide Blast — explosion radius +50% | Chain Reaction — explosion can trigger nearby pickups to detonate |

> **Note to storyteller agent:** Epic and Legendary ability names above are functional placeholders. The storyteller should give each ability a flavor name that fits the kid's imagination. Example: "Flash Draw" might become "The Fastest Blade in the Cul-de-Sac."

---

## Pickup System

- **Auto-pickup on walk-over.** No button press required.
- Raw objects (pre-forge) go to the **material bag** — a separate holding area that does NOT consume a weapon slot.
- The material bag holds up to **3 unforged objects**. If full, walking over a new object presents a quick-swap prompt (tap to swap, ignore to leave it on the ground).
- Cardboard is auto-collected and added to the resource counter with no prompt.

---

## The Forge Workbench

The workbench is a physical prop in the level. It is the only place where forging and upgrading happen in V4 Sprint 1. (On-the-fly crafting is a later sprint.)

### Workbench Placement
- Each level has **1–2 workbench props** placed by the `LevelBuilder` ScriptableObject data.
- Workbenches appear in dedicated "safe" areas or mid-level loot zones — never in active combat rooms.
- The prop is visually distinct (a cardboard-covered workbench with the cardboard counter glowing above it).

### Forge UI Flow

When the player walks up to the workbench and presses Interact:

```
┌─────────────────────────────────────────────┐
│  FORGE                    [Cardboard: 12]   │
│                                             │
│  Material Bag                               │
│  [ Rolling Pin ]  [ Garden Hose ]  [ --- ]  │
│                                             │
│  Weapon Slots                               │
│  [ Bo Staff ██░░ ] [ Katana ████ ] [ --- ]  │
│                                             │
│  > Select an object to forge or upgrade     │
│                                             │
│  [Rolling Pin selected]                     │
│  Rarity: Common  →  Ceiling: Standard       │
│  Forge Standard    Cost: 2 cardboard [FORGE]│
└─────────────────────────────────────────────┘
```

- Selecting a weapon slot item (if it has an upgrade path) shows the upgrade cost.
- Selecting a material bag item shows the forge cost and ceiling.
- Grayed-out options when player cannot afford or rarity ceiling is reached.
- Tapping FORGE plays the transformation animation and sound, then returns control.

---

## Inventory System

### Weapon Slots
- **3 weapon slots** in V4 Sprint 1. Expandable in a future sprint via progression unlock.
- Forged weapons occupy a weapon slot.
- Unforged objects live in the **material bag** (separate, not a weapon slot).
- Player can **drop** a weapon from inventory to free a slot (dropped weapons land in the world and can be picked up by anyone).

### Inventory Screen
Accessible at any time via the inventory button (pause-safe — does not pause time). Shows:
- All 3 weapon slots with: weapon name, tier, durability bar, abilities unlocked
- Material bag (up to 3 unforged objects)
- Current cardboard count
- Quick-equip: tap a slot to make it the active weapon

### Active Weapon
- One weapon slot is "active" at a time — this is what the player swings.
- Cycle through equipped weapons via swipe or button.
- The active weapon is displayed prominently in the HUD with its name (Legendary only) or icon (Standard/Epic).

---

## Durability System

### How It Works
- Every forged weapon has a durability value. Standard weapons are more fragile; higher tiers are more durable.
- Durability is consumed on each successful hit landed (not on swings that miss).

| Tier | Starting Durability |
|---|---|
| Standard | 30 hits |
| Epic | 60 hits |
| Legendary | 100 hits |

- Some weapons are more durable by nature (Lunchbox Shield — defensive use) and some are more fragile (Shurikens — thrown and retrieved).
- Final per-weapon durability values are tunable in `WeaponObjectSO`.

### On Break
- When durability reaches 0: **BROKEN animation plays** (cardboard crumbles, object reverts to its raw form briefly, then disintegrates entirely).
- The weapon slot becomes empty.
- No recovery — the object is gone. Cardboard spent on it is gone.
- A sound cue and screen shake communicate the loss clearly.
- The HUD flashes the empty slot.

### Durability HUD
- A thin bar beneath each equipped weapon icon in the HUD.
- Color shifts: green (full) → yellow (50%) → red (20%) → flashing red (5 hits remaining).

---

## Loot Distribution

Two source types coexist in every level:

### Scattered Objects
Ordinary household objects placed naturally around the level environment — a broom leaning against a fence, a flashlight on a table, a jump rope on the ground. The player discovers these by exploring. Most are Common rarity.

### Dedicated Loot Zones
Specific areas of the level (garages, sheds, a storage room) contain clusters of objects, including Rare and occasionally Legendary ones. These areas are marked on the minimap after discovery. The `LevelBuilder` ScriptableObject defines their locations and contents.

### Cardboard Sources
- Enemies drop 1–3 cardboard on death (configurable per enemy type).
- Loot zones contain cardboard piles (5–10 per pile).
- Boxes and crates in the environment can be broken to reveal cardboard.

---

## Data Architecture

### ScriptableObjects

**`WeaponObjectSO`** — one asset per weapon (12 total):
```
string rawObjectName          // "Rolling Pin"
string weaponName             // "Mace"
Sprite rawObjectIcon
Sprite standardIcon
Sprite epicIcon
Sprite legendaryIcon
WeaponRarity rarity           // Common, Rare, Legendary
WeaponType type               // Melee, Ranged, Defensive, Utility
int baseDamage
float attackSpeed
float attackRange
int standardDurability
int epicDurability
int legendaryDurability
string epicAbilityId          // references AbilitySO
string legendaryAbilityId     // references AbilitySO
int forgeCost                 // always 2 (Standard)
int epicUpgradeCost           // 5 for Rare/Legendary objects, 0 for Common
int legendaryUpgradeCost      // 10 for Legendary objects, 0 for others
```

**`AbilitySO`** — one per unique ability (24 total, 2 per weapon):
```
string abilityId
string displayName
string flavorDescription
AbilityTrigger trigger        // OnHit, OnSpecial, OnDodge, OnBlock, Passive
float magnitude               // damage mult, radius, duration, etc.
float cooldown
GameObject vfxPrefab
AudioClip sfx
```

**`WeaponDropTableSO`** — per level, defines spawn rules:
```
WeaponSpawnEntry[] scatteredObjects   // object type, world position, rarity override
WeaponSpawnEntry[] lootZoneObjects    // clustered spawns
int[] cardboardPileAmounts            // how much cardboard per pile
Vector3[] workbenchPositions
```

### Runtime Data (not ScriptableObjects)

**`WeaponInstance`** (plain C# class, no MonoBehaviour):
```
WeaponObjectSO data
WeaponTier tier               // Standard, Epic, Legendary
int currentDurability
bool isBroken
```

---

## Script Architecture

All scripts live in `Assets/_Project/Scripts/` under the namespaces defined in CLAUDE.md. **Implementation delegated to `unity-senior-developer` agent** — this section defines the contract.

### New Scripts Required

| Script | Namespace | Responsibility |
|---|---|---|
| `WeaponInventory.cs` | `Boxhead.Systems` | 3-slot weapon slot manager; material bag; active weapon tracking; fires `OnInventoryChanged` |
| `CardboardResource.cs` | `Boxhead.Systems` | Stackable cardboard counter; `Add(int)`, `Spend(int)` with validation; fires `OnCardboardChanged` |
| `ForgeController.cs` | `Boxhead.Systems` | Forge logic — validates rarity ceiling, costs, calls `WeaponInventory` and `CardboardResource`; fires `OnWeaponForged`, `OnWeaponUpgraded` |
| `ForgeUI.cs` | `Boxhead.UI` | Workbench UI panel; reads `WeaponInventory` and `CardboardResource`; calls `ForgeController` |
| `WeaponPickup.cs` | `Boxhead.Systems` | Trigger on raw object prop; auto-adds to material bag on player overlap; handles full-bag swap prompt |
| `CardboardPickup.cs` | `Boxhead.Systems` | Trigger on cardboard prop; auto-adds to `CardboardResource` on overlap |
| `WeaponDurability.cs` | `Boxhead.Systems` | Tracks durability per `WeaponInstance`; called by `CombatController` on hit-landed; fires `OnWeaponDamaged`, `OnWeaponBroken` |
| `WeaponForgeAnimation.cs` | `Boxhead.Systems` | Plays cardboard-wrap VFX and SFX during forge; driven by `ForgeController.OnWeaponForged` |
| `InventoryScreen.cs` | `Boxhead.UI` | Full inventory panel; shows weapon slots, material bag, cardboard count; handles equip/drop |
| `WeaponHUDSlots.cs` | `Boxhead.UI` | 3-slot HUD display; durability bars; active slot highlight; weapon name for Legendary; updates on `OnInventoryChanged` |
| `WorkbenchProp.cs` | `Boxhead.Systems` | MonoBehaviour on the workbench prefab; detects player proximity; opens ForgeUI on interact |

### Scripts Modified

| Script | Change |
|---|---|
| `CombatController` | Call `WeaponDurability.RegisterHit()` on confirmed hit-landed; read active weapon stats from `WeaponInventory.ActiveWeapon` |
| `LevelBuilder` | Read `WeaponDropTableSO`; spawn raw object props and workbench props at defined positions |
| `GameManager` | Reset `WeaponInventory` and `CardboardResource` at run start |

---

## Prefabs Required

| Prefab | Description |
|---|---|
| `pfb_workbench` | The forge workbench prop with `WorkbenchProp.cs` and trigger collider |
| `pfb_pickup_[weaponId]` | One per raw object (12 total); mesh + `WeaponPickup.cs` + rarity VFX |
| `pfb_pickup_cardboard` | Cardboard pile prop; `CardboardPickup.cs` |
| `pfb_forge_vfx` | Particle system for the cardboard-wrap transformation |
| `pfb_weapon_broken_vfx` | Particle system for weapon destruction |

---

## Locked Decisions

All decisions confirmed by Louie Celli on 2026-08-03.

| # | Decision | Answer |
|---|---|---|
| 1 | Material bag capacity | **3 slots** |
| 2 | Durability values | **Standard=30, Epic=60, Legendary=100** — confirmed as baseline |
| 3 | Cardboard drop rates | **1–3 per enemy** — confirmed as starting point; tune after first playtest |
| 4 | Active weapon cycling | **Both inputs:** existing HUD buttons (above joystick) AND swipe left/right. Both ship in Sprint 1. |
| 5 | Sprint 1 weapon scope | **All 12 weapons at Standard tier.** 3D models are already built — remaining per-weapon setup is ~1 hour of data/prefab work. Full catalog ships in Sprint 1. |
| 6 | Ability scope | **Sprint 1 = Standard tier only (no abilities).** Epic/Legendary abilities ship in Sprint 2, simple ones first (Unbreakable, Triple Throw, Stagger), complex ones last (Rodeo, Chain Reaction, Confusion). |

---

## Sprint Breakdown

### Sprint 1 — Weapon System Foundation
**Scope: V4 project migration → full forge loop → all 12 weapons at Standard tier**

> **Migration-first rule:** No weapon code or assets are created until the migration is complete and verified. All phases below depend on a working V4 Unity project with V3 assets intact.

---

**Phase 0 — V4 Project Migration**

Follow `docs/migration-guide.md` exactly. Steps in order:

1. Create a fresh Unity 6 LTS project at `UnboxedHeroes/UnboxedHeroes/`
2. Copy `Assets/_Project/` wholesale from the V3 source project — preserves all Unity GUIDs so cross-references between prefabs, materials, and ScriptableObjects remain intact
3. Copy `Packages/manifest.json` — then open Package Manager and confirm all packages resolved:

   | Package | Version |
   |---|---|
   | `com.unity.cinemachine` | 3.1.7 |
   | `com.unity.ai.navigation` | 2.0.14 |
   | `com.unity.inputsystem` | 1.19.0 |
   | `com.unity.render-pipelines.universal` | 17.5.0 |
   | `com.unity.timeline` | 1.8.12 |
   | `com.unity.ugui` | 2.5.0 |
   | `com.unity.test-framework` | 1.7.0 |
   | `com.unity.visualscripting` | 1.9.12 |

4. Reimport Asset Store packages from the Unity Asset Store:
   - RPG Character Mecanim Animation Pack FREE (ExplosiveLLC)
   - SuperCharacterController (ExplosiveLLC)
   - Low Poly Mega Pack — Polyworks (Off Axis Studios)
   - Polylised — Medieval Desert City
   - SimpleTown
5. Do **not** copy `Assets/_Project/Scenes/` — scenes are excluded per the migration guide
6. Install MCP for Unity: `file:/Users/jcelli/Documents/tools/unity-mcp/MCPForUnity`

**Migration verification checklist (must pass before Phase 1):**
- [ ] Unity Editor opens with zero console errors
- [ ] All packages show green checkmarks in Package Manager
- [ ] `pfb_player.prefab` opens in the Inspector with no missing script references
- [ ] `pfb_GameManager.prefab` opens cleanly
- [ ] At least one character prefab (`pfb_char_ninjamale.prefab`) shows correct mesh and materials
- [ ] `WeaponData.cs`, `Inventory.cs`, `WeaponPickup.cs` all compile with no errors
- [ ] MCP for Unity server starts successfully (Window → MCP for Unity → Start Local HTTP Server)

> **What already exists from V3 — do NOT recreate these:**
> The migration brings in all V3 weapon assets. Review before building anything new:
> - **22 weapon model folders** under `Models/Weapons/` — all forged weapon meshes
> - **18 pickup prefabs** under `Prefabs/Weapons/Pickups/` — raw object world props
> - **22+ weapon prefabs** under `Prefabs/Weapons/` — the held/equipped versions
> - **22+ `WeaponData` SO assets** — existing stat definitions (will be extended, not replaced)
> - `WeaponData.cs`, `WeaponPickup.cs`, `Inventory.cs`, `WeaponHolder.cs`, `WeaponCycler.cs`, `WeaponEquipController.cs` — existing scripts (will be extended or replaced by the new forge-aware versions)
> - `WeaponAbilityData.cs` + 5 existing ability data scripts — starting point for the `AbilitySO` system

---

**Phase 1 — Blender Import Pass**
- `blender-specialist` runs the standard pre-export checklist on all 12 raw object props (pivot, axis, scale fix, FBX re-export) via Blender MCP
- Verify each raw object prop is imported at correct scale in Unity (~human-sized for broomstick/hose, smaller for ruler/trowel)
- All 12 raw object props confirmed in `Assets/_Project/Models/Weapons/[WeaponName]/`

---

**Phase 2 — Data Layer**
> Build on top of migrated V3 `WeaponData.cs` — extend or replace as needed rather than starting from scratch.

- `WeaponObjectSO.cs` — extends/replaces `WeaponData.cs`; adds `WeaponRarity`, forge costs, tier ceilings, ability ID slots
- `WeaponDropTableSO.cs` — new; defines per-level spawn rules for raw objects, cardboard piles, and workbench positions
- 12 `WeaponObjectSO` assets configured (Standard tier; Epic/Legendary cost fields present but ability IDs null until Sprint 2)
- Review migrated `WeaponData` SO assets — decide per-weapon whether to extend in-place or create new `WeaponObjectSO` assets
- `unity-code-reviewer` sign-off before Phase 3

---

**Phase 3 — Core Systems**
> `Inventory.cs` migrated from V3 is the starting point for `WeaponInventory.cs`.

- `CardboardResource.cs` — new; stackable counter with `Add(int)`, `Spend(int)`, `OnCardboardChanged` event
- `WeaponInventory.cs` — replaces V3 `Inventory.cs`; 3 weapon slots + separate 3-slot material bag; `OnInventoryChanged` event
- `WeaponDurability.cs` — new; per-`WeaponInstance` durability tracking; `OnWeaponDamaged`, `OnWeaponBroken` events
- `ForgeController.cs` — new; validates rarity ceiling and cardboard cost; calls `WeaponInventory` + `CardboardResource`; fires `OnWeaponForged`, `OnWeaponUpgraded`
- `unity-code-reviewer` sign-off before Phase 4

---

**Phase 4 — World Interaction**
> Review migrated `WeaponPickup.cs` and existing pickup prefabs before building new ones.

- `WeaponPickup.cs` — updated from V3 version; now routes to material bag instead of directly equipping
- `CardboardPickup.cs` — new; auto-adds to `CardboardResource` on player overlap
- `WorkbenchProp.cs` — new; detects player proximity, opens ForgeUI on interact
- Rarity VFX added to existing pickup prefabs (Common = no change, Rare = gold shimmer, Legendary = aura + float)
- `pfb_pickup_cardboard` — new prefab for cardboard piles
- `pfb_workbench` — new prefab with `WorkbenchProp.cs` and trigger collider
- Wire `LevelBuilder` to read `WeaponDropTableSO` and spawn props + workbenches at defined positions

---

**Phase 5 — UI**
> V3 `WeaponSlotUI.cs` is the starting point for `WeaponHUDSlots.cs`.

- `ForgeUI.cs` — new; workbench UI panel with material bag, weapon slots, cardboard count, forge/upgrade buttons
- `InventoryScreen.cs` — new; full inventory panel accessible at any time; quick-equip, drop
- `WeaponHUDSlots.cs` — replaces/extends V3 `WeaponSlotUI.cs`; 3 slots, durability bars, HUD button cycling + swipe cycling
- Durability bar color: green → yellow (50%) → red (20%) → flashing red (5 hits remaining)

---

**Phase 6 — Combat Wiring**
- `CombatController` updated: read active weapon stats from `WeaponInventory.ActiveWeapon`; call `WeaponDurability.RegisterHit()` on confirmed hit-landed
- `WeaponForgeAnimation.cs` + `pfb_forge_vfx` — cardboard-wrap transform VFX + SFX on forge
- `pfb_weapon_broken_vfx` — crumble + disintegrate VFX on durability zero

---

**Phase 7 — Review and Polish**
- Full playtesting pass: pick up all 12 weapon types, forge each at Standard tier, use until broken, reforge
- `unity-code-reviewer` final pass on all new and modified scripts
- Profiler: 0 GC alloc in `OnInventoryChanged`, weapon swing, and durability hot paths

---

### Sprint 2 — Epic and Legendary Abilities
**Scope: AbilitySO framework + all 24 abilities across 12 weapons**

**Phase 0 — Pre-Sprint**
- `storyteller` agent writes final flavor names for all 24 abilities before any SO assets are created
- `AbilitySO.cs` definition built and reviewed

**Phase 1 — Ability Framework**
- `AbilityExecutor.cs` (`Boxhead.Systems`) — reads an `AbilitySO`, fires on the correct trigger (OnHit, OnSpecial, OnDodge, OnBlock, Passive), handles cooldowns
- 24 `AbilitySO` assets created with final flavor names
- `ForgeController` updated to wire abilities when upgrading to Epic/Legendary
- `WeaponHUDSlots` updated to show ability name for Epic+ weapons
- `unity-code-reviewer` sign-off

**Phase 2 — Simple Abilities** (2–4 hrs each)
Bo Staff: Spin Strike, Stagger | Shurikens: Triple Throw | Foam Sword: Unbreakable, Resilience | Pressure Cannon: Charge, Blast Wave

**Phase 3 — Medium Abilities** (4–8 hrs each)
Shurikens: Ricochet | Quickdraw Blade: Flash Draw, Ghost Step | Katana: One Cut, Iaijutsu | Lightsaber: Parry Flash, Deflect | Water Whip: Pull, Soaked | Lunchbox Shield: Block, Counter | Dynamite Bundle: Wide Blast

**Phase 4 — Complex Abilities** (8–16 hrs each)
Lasso: Grab, Rodeo | Magic Wand: Confusion, Overload | Dynamite Bundle: Chain Reaction

**Phase 5 — Review and Polish**
- Full ability pass: verify every ability triggers correctly at Epic and Legendary tier
- Balance tuning: ability magnitudes, cooldowns
- `unity-code-reviewer` final pass
- Profiler: verify no GC alloc from ability trigger path

---

## Definition of Done

### Sprint 1

- [ ] **Migration complete:** Unity Editor opens with zero errors; all packages green; player/weapon prefabs load cleanly; MCP for Unity server starts
- [ ] All 12 weapon models Blender-validated and re-exported (pivot, axis, scale confirmed)
- [ ] All 12 `WeaponObjectSO` assets exist and configured
- [ ] Player walks over any of the 12 raw object props → enters material bag (no weapon slot consumed)
- [ ] Material bag holds 3; full-bag swap prompt works
- [ ] Player walks over cardboard → counter increments; HUD updates
- [ ] Player approaches workbench → Forge UI opens
- [ ] Forge UI shows material bag contents, weapon slots, cardboard count, and correct forge cost per object
- [ ] Forging any object costs 2 cardboard → enters a weapon slot at Standard tier with correct stats
- [ ] Upgrade options for Rare/Legendary objects shown but grayed (Epic/Legendary not yet implemented — Sprint 2)
- [ ] Common objects show no upgrade path
- [ ] Durability bar visible in HUD; depletes on hits landed; color shifts correctly
- [ ] When durability hits 0: BROKEN animation + sound plays, slot empties, no recovery
- [ ] Inventory screen opens and closes; shows all 3 slots and cardboard count; quick-equip works
- [ ] Active weapon cycles via HUD buttons and swipe; combat reads stats from active weapon
- [ ] Workbench placed correctly in test level by `LevelBuilder`
- [ ] 0 GC alloc in `OnInventoryChanged`, weapon swing, and durability hot paths
- [ ] `unity-code-reviewer` approved all scripts before merge

### Sprint 2

- [ ] `storyteller` ability flavor names finalized before any SO assets are created
- [ ] `AbilityExecutor.cs` built and reviewed
- [ ] All 24 `AbilitySO` assets exist with final names and correct trigger types
- [ ] Every Epic ability triggers correctly for all 12 weapons
- [ ] Every Legendary ability triggers correctly for all 12 weapons
- [ ] Epic weapons show glow; Legendary weapons show full glow + swing trail
- [ ] Legendary weapon name displayed in HUD
- [ ] Upgrade cost gates enforced: Common objects cannot reach Epic/Legendary (grayed in Forge UI)
- [ ] No GC alloc on ability trigger path
- [ ] `unity-code-reviewer` approved all new and modified scripts before merge

---

*Document owner: Louie Celli | Decisions locked 2026-08-03 | Hand to `unity-senior-developer` for Sprint 1 Phase 0.*
