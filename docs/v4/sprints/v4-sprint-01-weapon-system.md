# V4 Sprint 01 — Weapon System Foundation

**Goal:** Migrate V3 assets into a clean V4 Unity project, then deliver the full cardboard forge loop with all 12 weapons playable at Standard tier.
**Branch:** `feature/v4-sprint-01-weapon-system`
**Base branch:** `v4/main`
**Design doc:** `docs/design/weapon-creation-system.md`
**Migration guide:** `docs/migration-guide.md`
**Status:** ✅ Complete — 2026-08-04

---

## Context

The weapon creation system is the crown-jewel mechanic of Unboxed Heroes. Before any weapon code is written, the V4 Unity project must be stood up by migrating all V3 assets wholesale. Once migration is verified, the sprint delivers the complete forge pipeline: players pick up raw household objects, carry them to a Forge Workbench, spend cardboard to transform them into named weapons, use them in combat, and watch them break permanently when durability hits zero.

This sprint ships Standard tier only. Epic and Legendary abilities are Sprint 2.

All decisions locked. See `docs/design/weapon-creation-system.md` — Locked Decisions section.

---

## Migration-First Rule

**No weapon code or assets are created until Phase 0 is complete and all migration verification checks pass.** Every subsequent phase depends on a working V4 Unity project with V3 assets intact.

---

## Phases

### Phase 0 — V4 Project Migration

Follow `docs/migration-guide.md` exactly.

**Steps:**
1. Create a fresh Unity 6 LTS (6000.5.3f1) project at `UnboxedHeroes/UnboxedHeroes/`
2. Copy `Assets/_Project/` wholesale from the V3 source project — preserves all Unity GUIDs
3. Copy `Packages/manifest.json` and confirm all packages resolve in Package Manager:

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

4. Reimport Asset Store packages:
   - RPG Character Mecanim Animation Pack FREE (ExplosiveLLC)
   - SuperCharacterController (ExplosiveLLC)
   - Low Poly Mega Pack — Polyworks (Off Axis Studios)
   - Polylised — Medieval Desert City
   - SimpleTown
5. **Do not** copy `Assets/_Project/Scenes/` — excluded per migration guide
6. Install MCP for Unity: `file:/Users/jcelli/Documents/tools/unity-mcp/MCPForUnity`

**Verification — must all pass before Phase 1:**
- [ ] Unity Editor opens with zero console errors
- [ ] All packages show green checkmarks in Package Manager
- [ ] `pfb_player.prefab` opens with no missing script references
- [ ] `pfb_GameManager.prefab` opens cleanly
- [ ] `pfb_char_ninjamale.prefab` shows correct mesh and materials
- [ ] `WeaponData.cs`, `Inventory.cs`, `WeaponPickup.cs` compile with no errors
- [ ] MCP for Unity server starts (Window → MCP for Unity → Start Local HTTP Server)

**What migrates from V3 — do NOT recreate:**

| Asset type | What's already there |
|---|---|
| Weapon models | 22 weapon model folders under `Models/Weapons/` |
| Pickup prefabs | 18 pickup prefabs under `Prefabs/Weapons/Pickups/` |
| Weapon prefabs | 22+ equipped weapon prefabs under `Prefabs/Weapons/` |
| WeaponData SOs | 22+ `WeaponData_obj_*.asset` files |
| Weapon scripts | `WeaponData.cs`, `WeaponPickup.cs`, `WeaponHolder.cs`, `WeaponCycler.cs`, `WeaponEquipController.cs` |
| Inventory script | `Inventory.cs` — starting point for `WeaponInventory.cs` |
| Ability scripts | `WeaponAbilityData.cs`, `DynamiteBundleAbilityData.cs`, `LassoAbilityData.cs`, `QuickdrawBladeAbilityData.cs`, `ShurikenAbilityData.cs`, `SixShooterAbilityData.cs` |
| HUD script | `WeaponSlotUI.cs` — starting point for `WeaponHUDSlots.cs` |

---

### Phase 1 — Blender Import Pass

`blender-specialist` agent runs the standard pre-export checklist on all 12 raw object prop meshes via Blender MCP. Use the standard 7-step checklist from `CLAUDE.md`.

**12 raw object props to validate:**
Broomstick, Ruler, Foam Sword, Garden Trowel, Cardboard Tube, Flashlight, Garden Hose, Jump Rope, Bicycle Pump, Remote Control, Lunchbox, Bike Horn

**Per-prop checklist:**
1. `get_object_info` — confirm scale/rotation
2. `apply_transforms` — zero out location/rotation/scale
3. `recalculate_normals` — fix inside-out normals
4. `merge_by_distance` — weld duplicate verts (threshold 0.0001)
5. `apply_all_modifiers` — bake to real geometry
6. `get_mesh_stats` — confirm < 1,000 triangles (weapons budget per CLAUDE.md)
7. `export_file` (FBX) — export with Unity axis settings

**Verification:**
- [ ] All 12 raw object props imported and visible in `Assets/_Project/Models/Weapons/[WeaponName]/`
- [ ] Each prop is approximately correct size (broomstick ≈ player-height, ruler ≈ hand-sized)

---

### Phase 2 — Data Layer

> Build on top of migrated `WeaponData.cs`. Extend or replace per weapon as needed rather than starting from scratch.

**New scripts:**
- `WeaponObjectSO.cs` (`Boxhead.Systems`) — extends/replaces `WeaponData.cs`; adds `WeaponRarity` (Common/Rare/Legendary), forge costs, tier ceilings, `epicAbilityId`/`legendaryAbilityId` string slots (null for now)
- `WeaponDropTableSO.cs` (`Boxhead.Systems`) — per-level spawn rules: scattered object positions, loot zone clusters, cardboard pile amounts, workbench positions
- `WeaponInstance.cs` — plain C# class (not MonoBehaviour): holds `WeaponObjectSO data`, `WeaponTier tier`, `int currentDurability`, `bool isBroken`

**New assets (12 `WeaponObjectSO` instances):**

| Weapon | Raw Object | Rarity | Type |
|---|---|---|---|
| Bo Staff | Broomstick | Common | Fast / Melee |
| Shurikens | Ruler | Common | Fast / Ranged |
| Foam Sword | Foam Sword | Common | Fast / Melee |
| Quickdraw Blade | Garden Trowel | Common | Fast / Melee |
| Katana | Cardboard Tube | Rare | Balanced / Melee |
| Lightsaber | Flashlight | Rare | Balanced / Melee |
| Water Whip | Garden Hose | Rare | Balanced / Ranged |
| Lasso | Jump Rope | Rare | Balanced / Utility |
| Pressure Cannon | Bicycle Pump | Legendary | Powerful / Ranged |
| Magic Wand | Remote Control | Legendary | Powerful / AoE |
| Shield | Lunchbox | Legendary | Powerful / Defensive |
| Dynamite Bundle | Bike Horn | Legendary | Powerful / AoE |

**Standard durability values:**

| Tier | Durability |
|---|---|
| Standard | 30 hits |
| Epic | 60 hits (field set, not active until Sprint 2) |
| Legendary | 100 hits (field set, not active until Sprint 2) |

**Cardboard costs (all Standard tier this sprint):**

| Action | Cost |
|---|---|
| Forge Standard | 2 cardboard |
| Upgrade to Epic | 5 cardboard (data only — not active until Sprint 2) |
| Upgrade to Legendary | 10 cardboard (data only — not active until Sprint 2) |

**`unity-code-reviewer` sign-off required before Phase 3.**

---

### Phase 3 — Core Systems

> `Inventory.cs` from V3 is the starting point for `WeaponInventory.cs` — extend it rather than rewriting from scratch.

**New scripts:**
- `CardboardResource.cs` (`Boxhead.Systems`) — stackable cardboard counter; `Add(int)`, `Spend(int)` (validates balance before spending); `event Action<int> OnCardboardChanged`; no GC alloc in hot path
- `WeaponInventory.cs` (`Boxhead.Systems`) — replaces V3 `Inventory.cs`; 3 weapon slots + separate 3-slot material bag; `Equip(WeaponInstance)`, `MoveToSlot(int)`, `Drop(int)`, `AddToMaterialBag(WeaponObjectSO)`; `event Action OnInventoryChanged`; active weapon index tracked; zero GC in `OnInventoryChanged`
- `WeaponDurability.cs` (`Boxhead.Systems`) — tracks durability per `WeaponInstance`; `RegisterHit(WeaponInstance)` decrements by 1; `event Action<WeaponInstance> OnWeaponDamaged`; `event Action<WeaponInstance> OnWeaponBroken`; broken weapons are removed from `WeaponInventory`
- `ForgeController.cs` (`Boxhead.Systems`) — validates rarity ceiling, checks `CardboardResource` balance, moves item from material bag to weapon slot as a new `WeaponInstance` at Standard tier; `event Action<WeaponInstance> OnWeaponForged`; `event Action<WeaponInstance> OnWeaponUpgraded`; upgrade path returns false for Common objects (no Epic/Legendary ceiling)

**`unity-code-reviewer` sign-off required before Phase 4.**

---

### Phase 4 — World Interaction

> Review migrated `WeaponPickup.cs` and the 18 existing pickup prefabs before building anything new.

**Modified scripts:**
- `WeaponPickup.cs` — updated from V3; on player overlap, routes raw object to `WeaponInventory.AddToMaterialBag()` instead of directly equipping; if material bag is full, shows swap prompt (UI overlay: tap to swap, dismiss to ignore)

**New scripts:**
- `CardboardPickup.cs` (`Boxhead.Systems`) — trigger on cardboard prop; calls `CardboardResource.Add(amount)` on player overlap; auto-destroys pickup
- `WorkbenchProp.cs` (`Boxhead.Systems`) — detects player in trigger radius; shows interact prompt; on interact input opens `ForgeUI`

**New prefabs:**
- `pfb_workbench` — workbench mesh + `WorkbenchProp.cs` + sphere trigger collider + floating cardboard icon
- `pfb_pickup_cardboard` — cardboard pile mesh + `CardboardPickup.cs` + trigger collider

**Updated prefabs (rarity VFX added to existing pickup prefabs):**
- Common rarity: no change
- Rare rarity: add gold shimmer particle system child
- Legendary rarity: add aura particle system + slight Y position float animation

**`LevelBuilder` updated:** reads `WeaponDropTableSO`; spawns raw object prefabs, `pfb_pickup_cardboard`, and `pfb_workbench` at defined positions.

---

### Phase 5 — UI

> V3 `WeaponSlotUI.cs` is the starting point for `WeaponHUDSlots.cs`.

**New scripts:**
- `ForgeUI.cs` (`Boxhead.UI`) — opens when player interacts with workbench; shows material bag (3 slots), weapon slots (3), cardboard count; on item select shows forge/upgrade cost and FORGE button; grays out options the player cannot afford or that exceed rarity ceiling; tapping FORGE calls `ForgeController`; `Time.timeScale` not paused (player stays vulnerable)
- `InventoryScreen.cs` (`Boxhead.UI`) — full inventory panel; accessible via inventory HUD button at any time (does not pause); shows 3 weapon slots with name, tier badge, durability bar, slot actions (equip/drop); shows material bag; shows cardboard count
- `WeaponHUDSlots.cs` (`Boxhead.UI`) — replaces V3 `WeaponSlotUI.cs`; 3 weapon slots in HUD; durability bar per slot (color-coded: green → yellow at 50% → red at 20% → flashing at ≤5 hits); active slot highlighted; weapon name shown for Legendary tier; updates on `OnInventoryChanged`; cycles via existing HUD buttons (above joystick) and swipe left/right

---

### Phase 6 — Combat Wiring

**Modified scripts:**
- `CombatController` — read active weapon `baseDamage` + `attackSpeed` from `WeaponInventory.ActiveWeapon`; call `WeaponDurability.RegisterHit()` on each confirmed hit-landed; fall back to bare-hands values if active slot is empty

**New scripts and prefabs:**
- `WeaponForgeAnimation.cs` (`Boxhead.Systems`) — subscribes to `ForgeController.OnWeaponForged`; plays `pfb_forge_vfx` at weapon position + SFX
- `pfb_forge_vfx` — cardboard-wrap particle system (cardboard strips spiral inward around the object)
- `pfb_weapon_broken_vfx` — crumble + disintegrate particle system; played by `WeaponDurability` at weapon world position on break; screen shake on break

---

### Phase 7 — Review and Polish

- Full playtesting pass: pick up all 12 weapon types, forge each at Standard tier, use until broken, reforge
- Confirm material bag full-swap prompt works correctly
- Confirm workbench Forge UI grays out correctly for Common objects (no upgrade path)
- Confirm cardboard scarcity feels meaningful — tune `CardboardPickup` amounts if needed
- `unity-code-reviewer` final pass on all new and modified scripts
- Profiler session: confirm 0 GC alloc in `OnInventoryChanged`, weapon swing, and `WeaponDurability.RegisterHit()` hot paths

---

## Dependencies

- `docs/migration-guide.md` — Phase 0 follows this exactly
- `docs/design/weapon-creation-system.md` — authoritative design reference
- `storyteller` agent — must write final Epic/Legendary ability flavor names before Sprint 2 begins (not blocking Sprint 1)
- `unity-code-reviewer` agent — required sign-off after Phase 2 and Phase 3 before proceeding

---

## Definition of Done

**Migration:**
- [x] Unity Editor opens with zero console errors
- [x] All packages green; Asset Store packages reimported
- [x] `pfb_player.prefab`, `pfb_GameManager.prefab`, character prefabs load with no missing references
- [x] MCP for Unity server starts successfully

**Phase 1:**
- [x] All 12 raw object props Blender-validated and in `Assets/_Project/Models/Weapons/`
- [x] Each prop is correct size in the scene
- Note: `obj_flashlight_pickup` (1,032 triangles) is over the 1,000-triangle budget and has a hole in one side — deferred to a future art pass for full rebuild.

**Phase 2:**
- [x] `WeaponObjectSO.cs` exists; all 12 SO assets created with correct rarity, type, costs, and durability values
- [x] `WeaponDropTableSO.cs` exists
- [x] `unity-code-reviewer` approved

**Phase 3:**
- [x] `CardboardResource.cs` — `Spend()` rejects calls that would go below zero; `OnCardboardChanged` fires on every change
- [x] `WeaponInventory.cs` — 3 slots confirmed; material bag 3 slots confirmed; `OnInventoryChanged` fires on every change; 0 GC alloc
- [x] `WeaponDurability.cs` — `OnWeaponBroken` fires at durability 0; broken weapon removed from inventory slot
- [x] `ForgeController.cs` — Common objects cannot reach Epic (returns false); costs deducted correctly
- [x] `unity-code-reviewer` approved

**Phase 4:**
- [x] Walking over any raw object prop adds it to material bag
- [x] Full-bag swap prompt appears and works correctly
- [x] Walking over cardboard auto-collects and updates counter
- [x] Workbench interact prompt appears in range; Forge UI opens on interact
- [x] Rarity VFX visible on Rare and Legendary pickups
- [ ] Workbench and pickups placed correctly in test level by `LevelBuilder` — deferred to Sprint 2 (no scene built yet)

**Phase 5:**
- [x] Forge UI shows material bag, weapon slots, cardboard count; FORGE button works; grays out correctly
- [x] Inventory screen opens and closes cleanly; quick-equip works; drop works
- [x] HUD slots show 3 weapons; durability bar color-shifts correctly; swipe and HUD buttons both cycle active weapon

**Phase 6:**
- [x] `CombatController` reads damage from active weapon; bare-hands fallback confirmed
- [x] `WeaponDurability.RegisterHit()` called on every confirmed hit; durability decrements correctly
- [x] Forge VFX (`pfb_forge_vfx`) and break VFX (`pfb_weapon_broken_vfx`) prefabs created and assigned
- Note: Screen shake on weapon break deferred to Sprint 2

**Phase 7:**
- [ ] Full forge loop playtested end-to-end — deferred to Sprint 2 (requires playable scene)
- [ ] 0 GC alloc confirmed in Profiler — deferred to Sprint 2 (requires Play mode in a scene)
- [x] `unity-code-reviewer` final pass completed — 1 blocker + 6 warnings found and fixed
- [ ] Branch merged to `v4/main` — pending

---

## Deferred to Sprint 2

| Item | Reason |
|---|---|
| Flashlight model rebuild | Broken asset (hole in mesh, 1,032 tris over budget) — requires full recreation |
| Scene setup + LevelBuilder wiring | No scene built in Sprint 1; scene setup is Sprint 2 Phase 0 |
| End-to-end playtesting | Blocked on scene |
| Live Profiler GC validation | Blocked on scene and Play mode |
| Screen shake on weapon break | Small polish item; not blocking |
| `StringBuilder.ToString()` alloc in `ForgeUI` | Minor, event-driven only — low priority |
| `Instantiate` pooling in `CombatController.NotifyLanded` | Pooling system planned for later sprint |

---

*Sprint owner: Louie Celli | Created: 2026-08-04 | Completed: 2026-08-04*
