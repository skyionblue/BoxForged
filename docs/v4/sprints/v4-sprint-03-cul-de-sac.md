# V4 Sprint 03 — First Playable Zone: The Cul-de-Sac

**Goal:** Turn the current test sandbox into an actual playable game — 5 combat rooms, a boss fight, the full roguelite run loop, and the V3 progression system wired into V4.
**Branch:** `feature/v4-sprint-03-cul-de-sac`
**Base branch:** `main`
**Zone doc:** `docs/story/zones/the-cul-de-sac.md`
**Art doc:** `docs/art/cul-de-sac-room1.md`
**Status:** Not started

---

## Context

Sprint 1 delivered the weapon forge loop. Sprint 2 delivered all 24 weapon abilities. Sprint 3 is the sprint that makes it a game — players will experience a full run from Room 1 through a boss fight, with the progression system tracking kills, awarding IP/Spark, and offering between-room upgrades.

The Cul-de-Sac is Zone 1. In reality it is a cracked suburban dead-end with minivans and houses. Through Kid's imagination it is a Wild West main street — covered wagons, saloons, hitching posts, and a birdbath that is a command node.

---

## What Is Already Done (V3 → V4 Migration)

All of the following exist in the project and are ready to use:

| Asset | Location | Status |
|---|---|---|
| `ProgressionSystem.cs` | `Scripts/Core/` | ✅ Migrated — IP, XP, Spark, combo, level-up |
| `StatOverlay.cs` | `Scripts/Player/` | ✅ Migrated — run overlay struct |
| `UpgradeScreen.cs` + `pfb_upgrade_screen` | `Scripts/UI/`, `Prefabs/UI/` | ✅ Migrated — 3-card between-room picker |
| `RunEndScreen.cs` + `pfb_run_end_screen` | `Scripts/UI/`, `Prefabs/UI/` | ✅ Migrated — post-boss IP/Spark summary |
| `MetaScreen.cs` + `pfb_MetaScreen` | `Scripts/UI/`, `Prefabs/UI/` | ✅ Migrated — permanent stat upgrade screen |
| `pfb_ProgressionSystem` | `Prefabs/Core/` | ✅ In ForgeLoop_Test scene already |
| `pfb_RoomManager` | `Prefabs/Core/` | ✅ Migrated |
| `pfb_EnemySpawner` | `Prefabs/Core/` | ✅ Migrated |
| `pfb_enemy_wagonwheel_roller` | `Prefabs/Enemies/` | ✅ Migrated — Room 1 enemy |
| `pfb_enemy_milepost_marshal` | `Prefabs/Enemies/` | ✅ Migrated — Room 2–4 enemy |
| `pfb_enemy_sprinkler_sentinel` | `Prefabs/Enemies/` | ✅ Migrated — Room 5 enemy |
| `pfb_enemy_spincycle` | `Prefabs/Enemies/` | ✅ Migrated — SpinCycle boss |
| `HitchingHoundAI.cs` | `Scripts/Enemy/` | ✅ Script migrated — prefab missing |

---

## What Needs to Be Built

### Phase 0 — Progression System Wiring (no new scenes)

Wire the V3 progression system into V4's runtime:

- `ProgressionSystem.ResetRunState()` called at run start (when RunStartUI hides)
- `ProgressionSystem.HandleKill()` already subscribes to `EnemyStats.OnAnyEnemyDeath` — verify it fires correctly in V4
- `UpgradeScreen` shown after each room clears (`RoomManager.OnRoomCleared` → `GameManager` routes here)
- `RunEndScreen` shown after boss death
- `MetaScreen` shown after RunEndScreen "Continue" is pressed
- `GameManager` routes between these screens — verify the existing routing works or patch as needed
- Add cardboard drops from enemies: `EnemyStats.OnDeath` → `CardboardResource.Add(dropAmount)` on the player (1–3 cardboard per enemy, configurable per enemy type)

**Deliverable:** Full progression flow works in ForgeLoop_Test scene (kill enemies → IP accumulates → after clearing a room → UpgradeScreen shows).

**`unity-code-reviewer` sign-off before Phase 1.**

---

### Phase 1 — Room 1: The Arrival

**Design reference:** `docs/art/cul-de-sac-room1.md`

**Room spec:**
- Layout: 40m × 30m, mostly open street
- Enemies: 2–3 WagonWheelRollers
- Aggro delay: 3 seconds after room activates (atmosphere absorption)
- Room gate: Closed on entry; opens when all enemies dead
- Forge safe zone: WorkbenchProp at room entrance (player can forge before combat begins)

**Scene:** `CulDeSac_Room1.unity`

**Steps:**
1. Invoke `/level-design` skill with "Cul-de-Sac Room 1" — produces Meshy prop prompts and LevelData SO spec before any scene work begins
2. Build the scene: flat ground (40×30), directional light (warm golden-hour), golden-haze ambient
3. Place ENV props (from Low Poly Mega Pack — Polyworks): covered wagon × 3, saloon facade × 2, hitching post × 4, water trough, lamp post (western) × 2, rain barrel × 3, tumbleweed (static) × 4, wanted poster × 3
4. Add `RoomManager` + `EnemySpawner` + `pfb_GameManager` + `pfb_AudioManager` + `pfb_ProgressionSystem` + `pfb_SaveSystem`
5. Add `pfb_player` at spawn point
6. Add `pfb_Main_Camera` + `pfb_CM_FollowCam`
7. Configure `EnemySpawner` with 2–3 WagonWheelRoller spawn points
8. Add `LevelBuilder` with a `WeaponDropTableSO_Room1` — scattered raw weapon props + cardboard piles + 1 workbench at entrance
9. Bake NavMesh
10. Add `RoomGate` (blocking geometry at exit) — opens on `RoomManager.OnRoomCleared`
11. Add `pfb_upgrade_screen` (inactive) to HUD canvas — wire to `RoomManager.OnRoomCleared`

**Deliverable:** Room 1 playable end-to-end — enter, fight 2–3 WagonWheelRollers, UpgradeScreen appears, exit gate opens.

---

### Phase 2 — HitchingHound Prefab

The `HitchingHoundAI.cs` script is migrated from V3 but has no prefab. Build it before Rooms 2–4.

- Model: check `Assets/_Project/Models/Characters/` — HitchingHound model may exist from V3 migration
- If model exists: run Blender pass (pivot, axis, scale) via `blender-specialist` → import → create prefab
- If model missing: generate Meshy prompt via `art-direction-agent` first
- Prefab: `pfb_enemy_hitching_hound.prefab` — same component pattern as `pfb_enemy_wagonwheel_roller`
- Wire `HitchingHoundAI` + `EnemyStats` + `EnemyHealthBar` + animator

**`unity-code-reviewer` sign-off before Phase 3.**

---

### Phase 3 — Rooms 2–4 (Escalating Encounters)

Three rooms drawn randomly from a pool. Each room is a scene or a `LevelData` SO variation.

**Enemy roster by room:**

| Room | Enemy Mix | Max Concurrent |
|---|---|---|
| Ambush Alley | 2 WagonWheelRoller + 2 HitchingHound | 3 |
| Saloon Front | 2 MilepostMarshal + 1 HitchingHound | 3 |
| Mailbox Row | 1 WagonWheelRoller + 1 MilepostMarshal + 2 HitchingHound | 4 |

**Steps:**
1. Build 3 room scenes with distinct ENV layouts (same Polyworks pack, different prop arrangement)
2. Configure `EnemySpawner` per room with correct enemy mix and `maxConcurrentEnemies`
3. Wire `GameManager` to randomly select from the 3 rooms after Room 1 clears (3 consecutive draws)
4. Add forge workbench in safe zone at entrance of each room

---

### Phase 4 — Room 5: Town Square + Boss Arena

**Room 5 — Town Square (fixed, pre-boss):**
- All 4 enemy types present: WagonWheelRoller, HitchingHound, MilepostMarshal, SprinklerSentinel
- Center: **Command Node** (birdbath prop) — destructible object
- When all enemies die → Command Node shatters (particle effect) → boss door opens
- `CommandNode.cs` — MonoBehaviour with health, `OnDestroyed` event; health pool = 1 hit (purely visual, not a real fight mechanic)

**Boss Arena — The Showdown Circle:**
- Circular clearing, 30m diameter
- SpinCycle V2 (same prefab as V3, already migrated with full 2-phase AI)
- On SpinCycle death → `RunEndScreen.Show()` → Imagination Restore VFX (warm gold bloom across entire zone, then full vivid color)
- `ImaginationRestore.cs` — small script that lerps a post-process volume from desaturated/warm to full color over 3 seconds

**Steps:**
1. Build Room 5 scene with ENV dressing
2. Create `CommandNode.cs` and the birdbath destructible prop
3. Configure SpinCycle V2 spawn in boss arena
4. Build `ImaginationRestore.cs` — triggers on boss death via `GameManager.TriggerWin()` hook
5. Wire `RunEndScreen` to show after `ImaginationRestore` finishes

---

### Phase 5 — Full Run Loop

Wire the complete run flow end-to-end:

```
RunStartUI (character/style select)
  → Room 1 (The Arrival)
    → RoomManager.OnRoomCleared → UpgradeScreen
  → Room 2 (random)
    → RoomManager.OnRoomCleared → UpgradeScreen
  → Room 3 (random)
    → RoomManager.OnRoomCleared → UpgradeScreen
  → Room 4 (random)
    → RoomManager.OnRoomCleared → UpgradeScreen (or Shop — see below)
  → Room 5 (Town Square)
    → Command Node destroyed → Boss door opens
  → Boss Arena (SpinCycle V2)
    → Boss death → ImaginationRestore → RunEndScreen
      → ConvertIPToSpark() → MetaScreen
        → Permanent stat purchase → New Run or Exit
```

**Shop (optional — between Room 4 and 5):**
The `ShopScreen` + `pfb_shop_screen` are migrated from V3. If time allows, wire a shop between the last random room and Room 5 — player spends IP on health refill, weapon upgrade tokens, or a random card. Defer if out of scope.

---

## Cardboard Economy in Combat

With real enemy encounters, cardboard needs to drop from kills. Tuning targets:

| Enemy | Cardboard drop |
|---|---|
| WagonWheelRoller | 1–2 |
| HitchingHound | 1–2 |
| MilepostMarshal | 2–3 |
| SprinklerSentinel | 2–3 |
| SpinCycle (boss) | 10 |

Add `cardboardDrop` field to `EnemyStats` (or a new `EnemyLootData` SO). On `OnDeath`, call `CardboardResource.Add(dropAmount)` on the player.

---

## Dependencies and Routing Rules

- **Zone design** → invoke `/level-design` skill for each room before any scene work
- **HitchingHound model** (if missing) → `art-direction-agent` writes Meshy prompt → `blender-specialist` processes → `unity-senior-developer` imports
- **Any new enemy prefab** → `blender-specialist` Blender pass → `unity-senior-developer` imports → `unity-code-reviewer` approves
- **ENV prop placement** → `art-direction-agent` produces prop list per room before placement
- **SpinCycle V2 new attacks** (Dust Devil, Gallows Run) → `unity-senior-developer` implements → `unity-code-reviewer` approves
- **All scripts** → `unity-code-reviewer` sign-off after each phase

---

## Open Questions

1. **Scene structure:** Is the Cul-de-Sac a single scene with room boundaries, or a scene-per-room with scene loading between rooms? V3 used scene-per-room (`SceneManager.LoadScene`). Recommend: scene-per-room for V4 — simpler NavMesh, no room-visibility management.

2. **HitchingHound model:** Does the migrated V3 project have the Hound model? Needs visual check. If yes, use it. If no, Meshy generation required first.

3. **SpinCycle V2 new attacks:** The GDD mentions "Dust Devil" and "Gallows Run" as new Phase 2 attacks. Are these new movement patterns only, or do they need new animations? Confirm before implementation.

4. **Shop between rooms:** Wire the V3 `ShopScreen` in Sprint 3, or defer to Sprint 4? Recommend: defer — it's a nice-to-have and the run loop works without it.

5. **Imagination Restore:** Is this a URP post-process volume fade or a shader on the terrain? Recommend: URP Global Volume with Saturation/Color Adjustment animated by `ImaginationRestore.cs`.

---

## Definition of Done

**Phase 0 — Progression:**
- [ ] Kill enemies in any scene → IP counter in HUD increments
- [ ] Combo multiplier increases on consecutive kills, resets on player hit
- [ ] Cardboard drops from enemy deaths at specified rates
- [ ] `UpgradeScreen` appears after room clear, 3 cards offered, selection applies effect

**Phase 1 — Room 1:**
- [ ] `CulDeSac_Room1.unity` scene exists, ENV dressed, NavMesh baked
- [ ] 2–3 WagonWheelRollers spawn with 3-second aggro delay
- [ ] Room gate closes on entry, opens on all enemies dead
- [ ] Workbench in safe zone at entrance
- [ ] Player can forge weapons, fight, and clear the room in under 3 minutes

**Phase 2 — HitchingHound:**
- [ ] `pfb_enemy_hitching_hound.prefab` exists, AI functional
- [ ] HitchingHound joins the enemy roster for Rooms 2–4

**Phase 3 — Rooms 2–4:**
- [ ] 3 random-draw room scenes built with correct enemy mixes
- [ ] `GameManager` randomly selects 3 rooms after Room 1

**Phase 4 — Room 5 + Boss:**
- [ ] Room 5 (Town Square) playable with all 4 enemy types
- [ ] Command Node destroys on trigger → boss door opens
- [ ] SpinCycle V2 fight works end-to-end
- [ ] `ImaginationRestore` plays on boss death
- [ ] `RunEndScreen` shows correct IP/kills/Spark after boss

**Phase 5 — Full Run:**
- [ ] Complete run from character selection through boss death and MetaScreen
- [ ] Spark spent on MetaScreen persists across runs (SaveSystem)
- [ ] No P1 bugs in the full run loop
- [ ] Run completes in 10–20 minutes on average

---

*Sprint owner: Louie Celli | Created: 2026-08-05 | Invoke `/level-design` for each room before scene work begins.*
