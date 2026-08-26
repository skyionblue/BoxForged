# ADR-0004: World 1 is one continuous scene, zoned by `RoomManager`

- **Status:** **Accepted** — 2026-08-26. The owner has resolved every question the 2026-08-25 draft left open. This ADR is the implementation spec for `unity-gameplay-engineer`; it does not by itself authorize a commit.
- **Date:** 2026-08-25, finalized 2026-08-26 against the live scene
- **Supersedes (in part):** [ADR-0002](0002-full-scene-rebuild.md) §Decision step 3's "one scene per room" corollary and `docs/ROADMAP.md` Phase 2's room-by-room scene list. ADR-0002's `RoomDataSO` / `LevelBuilder` architecture is **preserved and extended**, not replaced.
- **Related:** [ADR-0001](0001-fixed-low-follow-camera.md) (camera), [ADR-0003](0003-attack-telegraph-channel.md) (telegraphs)

---

## Context

### The owner's directive

World 1 (The Cul-de-Sac) is no longer a set of separate room scenes connected by scene loads. It is **one continuous scene, `CulDeSac_WildWestCity.unity`, played start to finish**:

1. A run is a **single linear walk** down one street. The random room-order draw (`GameManager.RandomRoomPool` / `InitRoomQueue`) is retired for this world.
2. The **SpinCycle boss fight happens in the same scene**, not in `CulDeSac_BossArena`.
3. This intentionally overrides the project's standing "no room may reuse another room's shape or prop layout" rule **for this scene only**. That rule still applies everywhere else.

The city scene is built and hand-tuned by the owner across several iterations. **Its geometry is not in scope and is not modified by this ADR.**

### Owner decisions closing the 2026-08-25 open questions

| # | Decision |
|---|---|
| 1 | **Three zones**: 0 "The Arrival", 1 "Ambush Alley", 2 "The Showdown Circle" (SpinCycle). |
| 2 | Zones 0 and 1 share a **mixed roster** — `WagonWheelRoller`, `SkepticGrunt`, and Gnome Grunt all appear in both, not one type per zone. |
| 3 | Zone 2 is **boss-only**. SpinCycle alone, no regular enemies mixed in. |
| 4 | **No ground expansion and no saloon move.** The boss fight uses the street's existing footprint. The covered-wagon props clear as part of the SpinCycle intro beat instead. |

Decisions 3 and 4 are **locked**. §2 below reports the space measurement honestly against them.

### The mechanism this builds on already exists

`RoomManager` was never a scene-loader. It already models **an ordered list of encounter zones inside a single scene**:

- `_rooms` is an ordered `List<RoomData>`; `RoomTrigger.OnTriggerEnter` calls `RoomManager.OnRoomEntered(index)` when the player crosses a boundary collider.
- `OnRoomEntered` guards with `if (roomIndex <= _currentRoom) return;` — zones advance forward only.
- The spawn-point path instantiates a zone's enemies **only when that zone activates**, holding `maxConcurrentEnemies` alive and refilling from `EnemySpawnPoint` quotas as they die.
- `AppendRoomsFromLevelBuilder()` (ADR-0002) converts `LevelBuilder.RoomData` (a `RoomDataSO[]`) into runtime zones at `Start()`, **appending** them to `_rooms`.
- `OnRoomActivated` is a `static event Action<int>` with **zero subscribers today** — a clean, unused seam.

**The single-scene pivot is a data and scene-composition change, not a redesign of `RoomManager`.**

### What the scene actually contains (re-measured live, 2026-08-26)

Measured through Unity MCP against the open scene, not from the previous draft. The scene has changed materially since 2026-08-25.

| | Value |
|---|---|
| Scene roots | 10: `[ENV - Static]`, `[Lighting]`, `[Player]`, `[Managers]`, `[HUD]`, `pfb_AudioManager`, `pfb_AttackTelegraphService`, `pfb_MinimapCamera`, `pfb_ProgressionSystem`, `pfb_SaveSystem` |
| **`[ENV - Static]` rotation** | **yaw 45°** — the whole environment was rigidly rotated about the world origin. Buildings and props were *not* individually re-arranged; their relative layout is unchanged from the 2026-08-25 measurement. |
| `Ground` | Unity Plane, local pos `(0, 0, −35.7)`, scale `(4, 1, 4.75)` → **40 m × 47.5 m** |
| Buildings | 10. Two facing rows at street-local **X = ±13** (centres), plus `pfb_env_saloon_facade` standing in the street at the north end |
| Street props | 34, of which **31 have colliders**; `broken_wagon_wheel_01/04` and `rope_coil_01/02` have none |
| Covered wagons | **Exactly two** — `pfb_env_covered_wagon_01` and `_02`, both `MeshCollider`, 2.48 m tall |
| Player | `pfb_player` at world `(−41.012, 0, −41.012)`, yaw 45 |
| Player clamp (B87) | `_arenaBoundaryRadius = 23`, `_arenaCenter = (−25.244, 0, −25.244)` — a 23 m circle on the ground's centre. **The 8.5 m blocker in the previous draft is resolved.** |
| Camera | `pfb_CM_FollowCam` at **yaw 45°, pitch 36°** (nudged from 35° to 45° later in the implementation pass, per explicit owner direction — see `_cameraYawDegrees` below and `docs/BACKLOG.md` H3) |
| Scaffolding present | `GameManager`, `AudioManager`, `ProgressionSystem`, `SaveSystem`, `AttackTelegraphService`, HUD, minimap camera |
| Still absent | `LevelBuilder`, `RoomManager`, `RoomTrigger`s, `RoomGate`s, the boss, post-process `Volume`, `ImaginationRestore_Volume`, build-settings entry |

---

## §0 Coordinate convention — read this before authoring anything

`[ENV - Static]` is at **yaw 45°**, so every building and prop world position is its street-local position rotated 45°. All design coordinates in this ADR are given in the **street-local frame** (local +Z = north, up the street; local X = across the street), because that is the frame the level actually reads in. Every authored value goes into Unity in **world space** (`RoomDataSO.position`, `WeaponDropTableSO.worldPosition`, and `SpinCycleAI._introWalkTarget_*` are all world-space — verified).

```
world.x   = (Lx + Lz) × 0.70710678
world.z   = (Lz − Lx) × 0.70710678
world yaw = local yaw + 45
```

Anchors, for checking the transform: player spawn is street-local `(0, −58)` = world `(−41.012, −41.012)`. The saloon facade is street-local `(0.43, −18.93)` = world `(−13.081, −13.690)`.

**Street-local extents (2026-08-26, second geometry revision — see the note below the table).** Ground: X −20…+20, Z −59.45…**+0.05** (was −11.95; the ground was lengthened 12 m to the north). Buildings (visual mesh footprints, south → north):

| West row (X ≈ −13) | Z span | East row (X ≈ +13) | Z span |
|---|---|---|---|
| `stables` | −56.51…−47.49 | `porchcabin` | −54.84…−49.15 |
| `general_store` | −46.50…−37.96 | `barber_shop` | −48.02…−41.96 |
| `bank` | **−24.60…−17.06** | `sheriffs_office` | **−28.57…−22.49** |
| `shedwithcrate` | **−14.09…−8.87** | `blacksmith_forge` | **−22.34…−15.32** |
| | | `twostoryhouse` | **−13.05…−9.19** |
| `saloon_facade` — **stands in the street**, X −3.37…+4.23, Z **−9.15…−4.71**, 7.18 m tall | | | |

**Second geometry revision (2026-08-26): zone 1 widened from 8 m to 20 m.** The owner played a full run and found zone 1 ("Ambush Alley") only 8 m deep — its two forge workbenches sat ~8 m apart, barely separated, with no real room for the zone's 7-enemy mixed encounter. Owner direction: *"Buildings can move, do what it takes."* Fix: **`bank`, `shedwithcrate`, `sheriffs_office`, `blacksmith_forge`, `twostoryhouse`, `saloon_facade`, both covered wagons, 18 of their adjacent street props, `RoomGate_Zone1`, and `RoomTrigger_Zone2` were rigidly translated +12 m along street-local Z** (a uniform world-space delta of `(+8.485, 0, +8.485)`, since local +Z maps to world `(c,c)` at this scene's 45° yaw). `stables`, `general_store`, `porchcabin`, `barber_shop`, `RoomGate_Zone0`, `RoomTrigger_Zone1`, and their adjacent props were **not** moved — zone 0 is unaffected. `Ground` was lengthened to absorb the shift: local Z position −35.7→−29.7, scale.z 4.75→5.95 (47.5 m → 59.5 m; south edge unchanged at −59.45, north edge extended from −11.95 to +0.05). Every object explicitly listed above moved by the *same* delta — but not every object relevant to the boss arena did: `stacked_crates_02`, one of the arena's limiting props (§2), was left in place rather than translated with the rest, which happened to enlarge rather than shrink the arena's usable floor. Because of that, the boss arena's derived floor (§2), the doorway math (§2), and the intro-cam dolly are not simply "mathematically unchanged" by the shift as a pure-translation argument would claim — it is the **empirical re-measurement below**, not the translation math alone, that confirms the boss arena space is still adequate post-shift. `StreetBoundary_West`/`_East` were resized to match Ground's new Z-span (size.z 47.5→59.5) at the same X-range. The player's arena clamp (B87) was re-derived from scratch (not just translated) because the elongated rectangle changes which single circle best balances reaching both the spawn and the boss arena: new `_arenaCenter` world `(−24.218, 0, −24.218)` (street-local `(0, −34.25)`), `_arenaBoundaryRadius = 24.5` (up from 23) — chosen so the circle reaches spawn (23.75 m away) and fully contains the boss arena's 8.39 m fight circle (15.37 m + 8.39 m = 23.76 m) with ~0.75 m margin on both ends, applied as a `pfb_player` scene-instance override exactly like B87's original.

**Correction of record (visual vs. collision).** The building *colliders* are single `BoxCollider`s far wider than the meshes: the collision corridor is roughly X ±12.9, while the *visual* corridor is X −8.8…+10.0. The player can walk into porches and facades. Every clearance number in this ADR is computed against **visual mesh footprints**, the conservative basis. The mismatch is a real defect, logged to `docs/BACKLOG.md`, and deliberately not fixed here.

The usable walking band, once boardwalk props (hitching posts, rain barrels, troughs, lamp posts at |X| ≈ 7.5–10) are accounted for, is about **X −6 … +7**. The `(±8, 7)` flanker positions in the old Ambush Alley data do not fit this street; §5 places flankers at ±5…6 instead.

---

## Decision

### §1 Three zones, with boundaries at the two natural building-to-building chokepoints

**Zone 1 widened 2026-08-26 (second geometry revision, see §0) — table below reflects current values, old values struck through for the record.**

| Idx | Zone | Local Z span | Depth | Activation |
|---|---|---|---|---|
| 0 | **The Arrival** | −59.45 → −44 | 15.5 m | Auto — `RoomManager` activates index 0 on `LevelBuilder.OnNavMeshReady` (B49). No trigger. |
| 1 | **Ambush Alley** | −44 → ~~−36 (8 m)~~ **−24.5 (20.0 m)** | ~~8 m~~ **20.0 m** | `RoomTrigger` (roomIndex 1) |
| 2 | **The Showdown Circle** | ~~−36 → −11.95 (24.55 m)~~ **−24.5 → +0.05 (24.55 m, unchanged)** | 24.55 m | `RoomTrigger` (roomIndex 2) |

The two boundaries sit where **both** building rows have a wall, so a barricade reads as spanning a gap rather than floating in open road: **Z = −44** (`general_store` west, `barber_shop` east, unchanged) and **Z = −24.5** (`bank` west, `sheriffs_office` east, shifted +12 with the rest of the north cluster — still inside both buildings' spans post-shift, but only just on the `bank` side: `bank`'s span starts at −24.60 (§0 table), a margin of only **~0.10 m** past the chokepoint. This is preserved by that 0.10 m of building overlap, verified via the physics flood-fill below — not guaranteed by construction, and a future building move that doesn't re-check this margin could reopen the chokepoint gap).

**Exact placements** (`RoomTrigger` slabs are 3 m deep so a sprinting `CharacterController` cannot tunnel through them in one physics step):

| Object | Local (x, z) | World position | Rotation | Local size |
|---|---|---|---|---|
| `RoomTrigger` roomIndex 1 | (0, −42.5) | `(−30.052, 1.5, −30.052)` | yaw 45 | ~~`(22, 3, 3)`~~ **`(26, 3, 3)`**, `isTrigger` on, renderer off |
| `RoomTrigger` roomIndex 2 | ~~(0, −34.5)~~ **(0, −22.5)** | ~~`(−24.395, 1.5, −24.395)`~~ **`(−15.910, 1.5, −15.910)`** | yaw 45 | ~~`(22, 3, 3)`~~ **`(26, 3, 3)`**, `isTrigger` on, renderer off |
| `RoomGate` 0 (opens on zone 0 clear) | (0, −44.5) | `(−31.466, 0, −31.466)` | yaw 45 | `(26, 4, 1)` — widened from the original 20 m in a `code-reviewer` fix pass (M9) to seal flush with the flanking building colliders, measured at X ±13 |
| `RoomGate` 1 (opens on zone 1 clear) | ~~(0, −36.5)~~ **(0, −24.5)** | ~~`(−25.809, 0, −25.809)`~~ **`(−17.324, 0, −17.324)`** | yaw 45 | `(26, 4, 1)`, same M9 fix |

Order along the player's path is gate → trigger, so the trigger is unreachable until the gate opens. `RoomGate.Open()` disables every child `Collider`, `Renderer`, **and `NavMeshObstacle`** (the last added in the same M9 fix pass — a closed-but-uncarved obstacle otherwise leaves a stale NavMesh hole after the gate opens), so an opened gate leaves nothing behind — confirmed by reading the script. This matters: gate 1 sits 3.1 m inside the boss arena's south lobe and must vanish completely, which it does.

**Second `code-reviewer` fix pass (2026-08-26): the 26 m gates alone do not seal the street.** They only ever spanned the corridor between the two building rows; the ground's true walkable edge is street-local X ±20, roughly 2–7 m further out than the building colliders' outer face (X ±13…18.63, varies per building), and the player's circular arena clamp (B87, radius 23 around the ground centre) does not tuck in to the building line either. The gap let the player walk around the *outside* of the entire building row — gates and all — and reach the boss zone without ever crossing a `RoomTrigger`. Fixed with two permanent, always-solid `BoxCollider`s (no `RoomGate`/`NavMeshObstacle` machinery — these never open), `StreetBoundary_West`/`StreetBoundary_East`, on the `Building` layer under `[Zone Boundaries]`: street-local X ∈ [−20,−17] and [17,20], full ground Z-span (−59.45…+0.05, matching `Ground` exactly post-second-geometry-revision), height 4 m. Verified with a real physics flood-fill (`Physics.OverlapCapsule` at the player's actual `CharacterController` radius, 0.3 m, on a 0.5 m grid over the whole street) rather than static inspection: with the boundary colliders disabled and both gates closed, the boss-zone area (street-local Z ≥ −36) was reachable (5,814 of ~7,700 grid cells connected to spawn) — reproducing the exploit; with the boundaries enabled and gates closed, it is not (1,523 cells, confined to zone 0) — matching the zone-0-only area a closed gate should allow. A positive control (gates disabled, boundaries enabled) confirmed the flood-fill method itself correctly detects connectivity (4,981 cells, boss zone reachable) rather than under- or over-reporting by construction. This also confirms the previously-noted ~66 m NavMesh wraparound around the south end of the building rows (found during the first fix pass) is now physically unreachable by the player, not merely outside the arena clamp as originally assumed.

**Third `code-reviewer` fix pass (2026-08-26): the X ∈ [−20,−17]/[17,20] placement above left two small safe-pockets, since 2 of the 10 buildings' outer faces fall short of X ±17** (`sheriffs_office` at X 16.20, `stables` at X ≈16.75) — not a progression break (`blacksmith_forge` still blocks the corridor further along), but a pocket outside the zone-1 fight the player could stand in. Widened both walls to X ∈ [−20,−16]/[16,20] (`size.x` 3→4, outer face pinned at |X|=20, inner face moved from |X|=17 to |X|=16), overlapping `sheriffs_office` by 0.20 m and `stables` by 0.75 m. Re-verified with a targeted `Physics.CheckCapsule` sweep (same 0.3 m player-capsule radius) of both former gap regions, in both Edit Mode and live in Play Mode after the runtime NavMesh bake: 91/91 and 130/130 sample points now blocked, versus fully open before the fix. Full detail in `docs/BACKLOG.md` B99's correction-of-record note.

**Fourth `code-reviewer` fix pass (2026-08-26): both `RoomTrigger`s were narrower than their paired `RoomGate`.** `RoomTrigger_Zone1`/`RoomTrigger_Zone2` were `size.x = 22` while `RoomGate_Zone0`/`RoomGate_Zone1` (their respective paired gates) were `size.x = 26` — a 4 m mismatch. Not reproducible as a bypass with the current building placement (the walkable corridor at both boundaries is narrower than even the 22 m trigger), but a gate-bypass in `RoomTrigger_Zone2` specifically is a severe, silent failure mode: zone 2 never activates, the boss never gets `SetActive(true)`, and the run becomes permanently unwinnable with zero diagnostic output. Widened both triggers' `size.x` from 22 to 26 to match their paired gates exactly, removing the failure class regardless of future building-placement changes. Re-verified with a `Physics.OverlapBox` scan of each widened trigger volume: every new overlap is with static geometry already tolerated by the paired gate at the same width (buildings, `Ground`, adjacent street props, the gate itself) — no overlap with any other `RoomTrigger` or player-reactive trigger volume, and since both `RoomTrigger`s are pure `isTrigger` colliders that only react to the `Player` tag (`RoomTrigger.OnTriggerEnter`), the wider box changes nothing about physical navigation or NavMesh.

**Zone depths are deliberately uneven.** Gates only ever open, never close behind the player, so a zone's depth bounds where a fight *starts*, not where it can be fought. Zone 1 was originally an 8 m tight ambush corridor by design; the second geometry revision (§0) widened it to 20 m because 8 m proved too tight in a real playthrough (two workbenches nearly overlapping, no room for a 7-enemy encounter), with zone 0 open behind it to retreat into either way.

**Re-verification after the second geometry revision (2026-08-26), all three of this ADR's previously-fixed geometry-dependent bugs re-checked against the new layout, per this ADR's own warning that moving buildings again is exactly the kind of change that could reopen them:**

1. **Gate-bypass (B99/B99-correction).** Re-ran the same physics flood-fill methodology (0.5 m grid, `Physics.OverlapCapsule` at the player's real `CharacterController` radius 0.3 m, `QueryTriggerInteraction.Ignore`, BFS from spawn) with the full three-configuration control, this time also gating each grid cell on a downward raycast finding `Ground` support — the wider street's flood-fill grid extended past the physical `Ground` mesh edge on the first attempt and produced a false-positive "reachable" result by routing through true off-mesh void, which a real `CharacterController` could never actually walk (it would fall, not walk); the ground-support check eliminated that false positive and is a reusable lesson for any future flood-fill on this scene. With the ground-support fix: **config 1 (boundaries disabled, gates closed — bug repro) 7,539 cells reachable, boss zone reachable — reproduces the exploit; config 2 (boundaries enabled, gates closed — shipped state) 1,447 cells, boss zone unreachable — the fix holds on the new layout; config 3 (boundaries enabled, gates open — positive control) 6,214 cells, boss zone reachable — confirms the test itself isn't just reporting "unreachable" by construction.** `StreetBoundary_West`/`_East` were resized to Ground's new Z-span as part of this revision (§0), which is why the fix still holds at 59.5 m instead of 47.5 m.
2. **Enemy spawn / gate-carve overlap (B100).** All 7 zone-1 spawns (rewritten for the new 20 m depth, §5) checked against the **live runtime NavMesh** (Play Mode, `RoomGate_Zone1`'s carving `NavMeshObstacle` active in its default closed state, matching the real zone-1 fight) via `NavMesh.SamplePosition` (0.5 m tolerance): all 7 on-mesh at an identical 0.083 m offset (matching every previously-verified spawn in this scene, a strong internal-consistency signal), zero `Building`-layer overlaps within 1.2 m, minimum pairwise spawn clearance 4.51 m (comfortably above the ≥1.5 m authoring rule — every zone-1 spawn now has 2–3× the clearance the old 8 m corridor allowed).
3. **Camera clearance (ADR-0001 §2.7, `LevelBuilder.ValidateCameraClearance`).** Runtime NavMesh bake after the revision: **1,303 verts, 599 tris** (Play Mode log). Diagnostic (non-blocking, per the script's own design): 104 of 1,303 vertices (8%) have < 8 m clear behind, 0 have < 6 m clear above. Bucketing violations by zone (a supplementary check beyond the stock diagnostic) found zone 1 — the zone this revision reshaped — has proportionally *fewer* behind-violations than zone 0 or zone 2 relative to vertex count, consistent with zone 1 now being mostly open, building-free floor; the revision did not introduce a new clearance regression.
4. **Boss arena floor (§2's r = 8.39 m requirement).** Re-verified via the same method the ADR itself used — a fine angular physics-raycast sweep (1°, 3 heights) from the arena's new center (street-local **(0.80, −18.90)**, world **(−12.799, 0, −13.930)** — the original centre plus the same `(+8.485, 0, +8.485)` shift-group delta, cross-checked live against `pfb_env_covered_wagon_01`'s post-shift world position `(−15.18, 0, −10.44)`, which matches this ADR's own §0 table exactly), with both wagons cleared and `RoomGate_Zone1` open (the real zone-2 state) — rather than trusting the translation math alone: **minimum clear radius 8.44 m, limited by `pfb_env_water_trough_02`**, the same prop the original derivation named as the limiting object, at essentially the same distance (8.39 m → 8.44 m, within the sweep's angular resolution). Confirms §0's revised claim: the boss arena's usable floor is still adequate post-shift, established here by empirical re-measurement rather than the translation math alone (since `stacked_crates_02` did not move with the rest of the shift-group — §0). A supplementary NavMesh ring-sample around the same point showed a lower on-mesh fraction (107/128) than the physics sweep suggested was necessary — traced to `RoomGate_Zone1`'s permanent `NavMeshModifier(ignoreFromBuild)` footprint plus agent-radius erosion, present at this gate regardless of open/closed state and unrelated to this revision (SpinCycleAI's attacks read raw `transform.position`/physics, not the NavMesh, per §2's existing caveat, so this doesn't affect the fight itself) — not filed as a new backlog item since it's a structural characteristic of the gate design, not a defect, but worth knowing before trusting a NavMesh ring-sample over a physics sweep near any gate on this street again.
5. **End-to-end path.** `NavMesh.CalculatePath` with both gates open: spawn → zone-1 midpoint `PathComplete` (7 corners), zone-1 midpoint → boss arena center `PathComplete` (8 corners), spawn → boss arena center `PathComplete` (14 corners).

**Three is what `GameManager` already routes.** `ShowRoomClearScreenDelayed` maps index 0 → upgrade screen, index 1 → shop screen; index 2 is the boss (`bossOwnedWin`, `OnRoomCleared` never fires). A three-zone city consumes that routing **exactly as written, with zero change**. A fourth zone would require generalizing it.

**Run length.** The old model was five encounters (Room 1 + three random + boss); this is three. Rather than add a fourth zone, §5 makes zones 0 and 1 substantially meatier — 5 and 7 enemies instead of 3 and 4. That is the answer to the pacing loss, and it costs no new code.

### §2 The space question, answered with numbers

**Question:** with the covered wagons cleared, is there enough open floor for `SpinCycleAI`'s attack patterns, using the existing ground?

**Answer: yes — but not where the previous draft assumed, and only because both wagons go.**

Method: rasterized the street at 0.1 m, computed the exact largest inscribed circle against visual mesh footprints of every collider-bearing object taller than the 0.75 m NavMesh climb height, with the ground edge treated as a wall.

| Configuration | Largest open circle in the north street |
|---|---|
| Both wagons present | **r = 5.46 m** (10.9 m across) at local (3.70, −27.20) |
| Only `covered_wagon_01` removed | r = 6.88 m at local (−0.90, −28.20) |
| Only `covered_wagon_02` removed | r = 6.19 m at local (4.10, −34.00) |
| **Both wagons removed** | **r = 8.39 m** (16.8 m across) at local **(0.80, −30.90)** = world **(−21.284, 0, −22.415)** |

Clearing both wagons is worth **+2.93 m of radius and 2.35× the fight area**. Clearing only one is not enough. The limiting objects at the final centre are `water_trough_02`, `rain_barrel_01`, and `stacked_crates_02`; removing `rain_barrel_01` as well buys only a further 0.28 m and is not worth it.

**Against SpinCycle's real numbers** (read from the script and prefab, not assumed):

| Demand | Value | Fits in r 8.39? |
|---|---|---|
| `fullSpinRadius` (AoE) | 3 m | Yes, trivially |
| Spin charge travel | `spinChargeSpeed 8` × `spinChargeDuration 0.6` = **4.8 m** | Yes — a charge always aims at the player, who is inside the arena |
| `jumpBackDistance` | 4 m | Yes in normal play; see the caveat below |
| Melee / ranged engage | `meleeRange 4`, `rangedRange 8` | Yes |
| Defeat sequence | 4.9 s, **zero translation** | Needs no floor at all |

**Verdict: the fight works.** One honest caveat, which is pre-existing rather than created here: `JumpBack`, `SpinCharge`, and `JumpCharge` all disable the `NavMeshAgent` and move by raw `transform.position`, with **no bounds or NavMesh validation** on the landing point. In the worst geometry — boss on the outside, player between it and the arena centre — a jump-back can put the boss up to ~3 m outside the arena, into a building. `_agent.Warp()` plus `Approach()`'s off-mesh recovery loop pulls it back, so it self-corrects with a visible pop rather than breaking. The same hole exists in `CulDeSac_BossArena` today. **Recommended cheap guard, not more floor:** clamp the jump-back landing point with `NavMesh.SamplePosition` before committing to it. Logged as a backlog item; it is a robustness fix to `SpinCycleAI`, not a scene decision.

**The one adjustment this forces — and it is a fight-centre move, not a ground change.** The best floor is **not** at the far north end. Past the last buildings (local Z −26 → −12) the largest circle is only **r ≈ 4.9 m**, because `saloon_facade` stands in the middle of that stretch. The 8.39 m arena is ~10 m further south, in the block between the `bank` and the `blacksmith_forge`.

So the boss fight moves south about 10 m. The saloon stops being the arena's back wall and becomes purely **the door SpinCycle walks out of** — which is what the intro was written for. This is the smallest possible adjustment and it needs no geometry change of any kind.

**A significant, previously unnoticed asset:** `SpinCycleAI` locates its doorway at runtime by name — `_saloonNameContains = "saloon_facade"` — and `pfb_env_saloon_facade` **exists only in this scene**. In `CulDeSac_BossArena` the lookup fails and a hardcoded fallback runs. The derived geometry against this scene's live saloon is exact:

| Derived value | World | Local |
|---|---|---|
| Doorway | `(−14.637, 0, −15.245)` | (0.43, −21.13) |
| `insideStart` (boss teleports here at 2% scale) | `(−11.102, 0.5, −11.710)` | (0.43, −16.13) |
| Intro cam start (`_introCamStartDistance 8`) | `(−20.294, 1.8, −20.902)` | (0.43, −29.13) |
| Intro cam end (`_introCamEndDistance 14`) | `(−24.537, 1.8, −25.145)` | (0.43, −35.13) |

`_doorDepthInset = 2.2` matches this facade's mesh half-depth (4.44 m / 2 = 2.22) to two decimal places — the constant was derived from this very asset. The intro camera dollies straight down the street centreline from local Z −29 to −35, looking north at the boss emerging. **`covered_wagon_01` occupies local X −5.18…−1.52, Z −31.40…−28.85 — directly alongside that dolly path.** The wagons must be gone before the intro camera exists, which §4 guarantees.

### §3 Boss binding: a dedicated scene subscriber, **not** `_zoneSceneBindings`

**The `_zoneSceneBindings` array proposed in the 2026-08-25 draft is withdrawn.** Reading the code again, it is unnecessary.

Two facts make it so:

1. A `RoomDataSO` with an **empty `spawnPoints` array** and `bossOwnedWin: true` is already a safe no-op zone. `ActivateRoom(2)` finds `spawnPoints.Count == 0`, falls through to the legacy pre-placed path, finds `enemies` empty, calls `RoomCleared(2)`, which returns immediately on `bossOwnedWin`. Nothing bad happens and nothing needs to happen — the win is owned by `SpinCycleAI.DefeatSequence` → `GameManager.TriggerWin()`, exactly as today.
2. `RoomManager.OnRoomActivated` is a `static event Action<int>` with **zero subscribers project-wide**. It is a finished seam waiting for a consumer.

So the boss is activated by the same small scene script that clears the wagons and opens the gates. `RoomManager` is not modified at all.

**Correction of record:** the previous draft described the boss room's pre-placed `enemies` path as an existing, exercised pattern. It is not. `CulDeSac_BossArena`'s `pfb_RoomManager` has one room, `bossOwnedWin: 1`, and **four null spawn points** with an empty `enemies` list — `RoomManager` does nothing there, and the boss is simply active from scene load. The mechanism exists in code but has never run.

**Corollary, also corrected:** `AppendRoomsFromLevelBuilder()` calls `_rooms.Add(...)`, so LevelBuilder zones are **appended** after Inspector-authored ones, not prepended. With all three zones coming from `RoomDataSO` and `_rooms` left empty in the Inspector, indices are 0, 1, 2 in `LevelBuilder._roomData` array order. No ordering hazard remains.

### §4 `WildWestCityZoneDirector` — one scene-specific script

A single new MonoBehaviour on a `[Managers]` child. It owns every scene-specific consequence of a zone change and keeps all of it out of the shared systems.

```csharp
[SerializeField] private GameObject[] _clearOnBossZone;  // the two covered wagons
[SerializeField] private GameObject   _boss;             // pre-placed, inactive SpinCycle
[SerializeField] private RoomGate[]   _gateByZone;       // _gateByZone[i] opens when zone i clears
[SerializeField] private int          _bossZoneIndex = 2;
```

Behaviour:

- **`Awake`** — `_boss.SetActive(false)` unconditionally. Do not rely on the saved scene flag: Play Mode state does not reliably revert in this project, and a boss left active by a previous session would run its intro at scene load.
- **`OnEnable` / `OnDisable`** — subscribe and unsubscribe `RoomManager.OnRoomActivated` and `RoomManager.OnRoomCleared`. `OnEnable` (not `Start`) is required: `RoomManager.Start()` can call `ActivateRoom(0)` synchronously, and a static event that is not yet subscribed misses it. Unsubscribing is mandatory — these are static events and will leak across scene reloads.
- **`HandleZoneActivated(int i)`** — when `i == _bossZoneIndex`: deactivate every `_clearOnBossZone` entry **first**, then `_boss.SetActive(true)`. The order is load-bearing (see §2 — the intro camera is created in `SpinCycleAI.Awake()` and dollies through where `covered_wagon_01` stands).
- **`HandleZoneCleared(int i)`** — `_gateByZone[i]?.Open()`.

**Which wagons:** both, and only these two.

**Positions shifted +12 m local Z (§0's second geometry revision) — table reflects current values:**

| Object | World position | Local (x, z) | World yaw |
|---|---|---|---|
| `pfb_env_covered_wagon_01` | ~~`(−23.667, 0, −18.929)`~~ **`(−15.182, 0, −10.444)`** | (−3.35, −18.12) | 60 |
| `pfb_env_covered_wagon_02` | ~~`(−21.256, 0, −28.313)`~~ **`(−12.771, 0, −19.828)`** | (4.99, −23.05) | 30 |

**The NavMesh trap, and its fix.** `LevelBuilder.BuildNavMeshDeferred` bakes at `Start()` from `CollectObjects.All` + `NavMeshCollectGeometry.PhysicsColliders`, and calls `NavMesh.RemoveAllNavMeshData()` first — so the Editor-baked mesh is discarded and the runtime bake is authoritative. That bake happens with the wagons **present**. Simply calling `SetActive(false)` later removes the wagon but **leaves its hole in the NavMesh**, so agents would path around empty air in the middle of the boss arena.

Fix, using stock Unity components already available in this project (`Unity.AI.Navigation` is present — `LevelBuilder` uses `NavMeshSurface`; `NavMeshModifier` and `NavMeshObstacle` both resolve). On **each** wagon instance:

- `NavMeshModifier` with **Ignore From Build = true** — the bake skips it, so no permanent hole.
- `NavMeshObstacle`, shape Box, **Carving = true**, **Carve Only Stationary = true** — punches a live hole while the wagon is active, and the mesh **heals the instant the GameObject is deactivated**.

Two carving obstacles is a negligible cost and needs no rebake, no custom code, and no new package.

**Why a dedicated subscriber rather than extending `RoomManager`.** Which props clear, which GameObject is the boss, and which barricade belongs to which zone are facts about one street. `RoomManager` is the reusable encounter system and already exposes the exact seam needed. Putting scene composition into it to serve a single scene is the coupling this project's architecture rules exist to prevent.

### §5 Encounter composition — mixed roster, redesigned for three enemy types

**Correction of record:** the previous draft described `RoomData_CulDeSac_AmbushAlley_v2` as "3 × SkepticGrunt + 1 second type". It is actually **3 × WagonWheelRoller + 1 × SkepticGrunt**, `maxConcurrentEnemies` 2. `RoomData_CulDeSac_Room1_v2` is 3 × WagonWheelRoller, `maxConcurrentEnemies` 3.

Un-rotating the old data by −35° recovers clean intent — Room 1 is a symmetric flanking pair at `(±6, 0)` plus one ahead at `(0, 5)`; Ambush Alley is boardwalk flankers at `(±8, 7)` plus a column at `(0, 11)` and `(0, 16)`. **The shapes migrate; the coordinates do not.** The `±8` flankers land inside this street's boardwalk props, so flankers move to ±5…6.

Roster (verified from prefabs):

| Enemy | Script | HP | Damage | Agent radius | Speed |
|---|---|---|---|---|---|
| Gnome Grunt (`pfb_enemy_gnome_grunt`) | **`BasicEnemyAI`** | 40 | 12 | 0.30 | 3.5 |
| Skeptic Grunt (`pfb_enemy_skeptic_grunt`) | `SkepticGruntAI` | 80 | 20 | 0.50 | 3.0 |
| Wagon Wheel Roller (`pfb_enemy_wagonwheel_roller`) | `WagonWheelRollerAI` | 60 | 20 | 0.95 (+0.9 base offset) | 4.0 |

Gnome Grunt uses the generic `BasicEnemyAI` — confirmed. Note the filename is `wagonwheel_roller`, not `wagon_wheel_roller`.

**Authoring rule:** `RoomManager.TrySpawnNext` walks `spawnPoints` **in array order**, seeding `maxConcurrentEnemies` first and refilling in order as enemies die. **Array order is spawn order** — the tables below are sequenced deliberately. Every position has ≥ 1.5 m clearance so any of the three types can be placed at any slot without re-solving geometry.

**`RoomData_CulDeSac_WildWestCity_Zone0`** — roomName "The Arrival", `maxConcurrentEnemies` **3**, `bossOwnedWin` false. 5 spawns, 280 HP.

| # | Enemy | Local (x, z) | Clear | `position` | `facingY` |
|---|---|---|---|---|---|
| 0 | WagonWheelRoller | (−6.25, −52.0) | 2.20 | `{x: -41.189, y: 0, z: -32.350}` | 225 |
| 1 | WagonWheelRoller | (5.50, −51.0) | 2.88 | `{x: -32.173, y: 0, z: -39.952}` | 225 |
| 2 | GnomeGrunt | (−0.75, −49.0) | 3.05 | `{x: -35.179, y: 0, z: -34.118}` | 225 |
| 3 | GnomeGrunt | (7.00, −48.0) | 2.97 | `{x: -28.991, y: 0, z: -38.891}` | 245 |
| 4 | SkepticGrunt | (−2.25, −47.0) | 4.56 | `{x: -34.825, y: 0, z: -31.643}` | 225 |

Reads as: the flanking roller pair from the original Room 1 opens, gnomes chase in, and the single Skeptic Grunt is the closer — the player's first "that one is tougher" beat.

**`RoomData_CulDeSac_WildWestCity_Zone1`** — roomName "Ambush Alley", `maxConcurrentEnemies` **4**, `bossOwnedWin` false. 7 spawns, 400 HP.

**Rewritten 2026-08-26 (second geometry revision, §0): zone 1 grew from 8 m to 20 m deep, so the original 5 m-deep spawn cluster (Z −37…−42) was re-spread across the new 16 m of usable depth (Z −41…−27) — same roster, same order, same narrative beats, just given real room instead of being crammed against the entrance. Old table struck through for the record.**

| # | Enemy | Local (x, z) | Clear | `position` | `facingY` |
|---|---|---|---|---|---|
| 0 | SkepticGrunt | ~~(−5.25, −42.0)~~ **(−5.25, −41.0)** | 4.51 min | ~~`{x: -33.411, y: 0, z: -25.986}`~~ **`{x: -32.704, y: 0, z: -25.279}`** | 200 |
| 1 | SkepticGrunt | ~~(3.75, −42.0)~~ **(3.75, −41.0)** | 4.51 min | ~~`{x: -27.047, y: 0, z: -32.350}`~~ **`{x: -26.340, y: 0, z: -31.643}`** | 250 |
| 2 | GnomeGrunt | ~~(−5.50, −39.5)~~ **(−5.50, −36.5)** | 4.51 min | ~~`{x: -31.820, y: 0, z: -24.042}`~~ **`{x: -29.698, y: 0, z: -21.920}`** | 205 |
| 3 | GnomeGrunt | ~~(3.25, −39.5)~~ **(3.25, −36.5)** | 4.51 min | ~~`{x: -25.633, y: 0, z: -30.229}`~~ **`{x: -23.511, y: 0, z: -28.107}`** | 245 |
| 4 | WagonWheelRoller | ~~(1.00, −38.0)~~ **(1.00, −31.5)** | 4.51 min | ~~`{x: -26.163, y: 0, z: -27.577}`~~ **`{x: -21.567, y: 0, z: -22.981}`** | 225 |
| 5 | GnomeGrunt | ~~(0.50, −37.0)~~ **(0.50, −27.0)** | 4.51 min | ~~`{x: -25.809, y: 0, z: -26.517}`~~ **`{x: -18.738, y: 0, z: -19.446}`** | 225 |
| 6 | WagonWheelRoller | ~~(−4.75, −37.0)~~ **(−4.75, −27.0)** | 4.51 min | ~~`{x: -29.522, y: 0, z: -22.804}`~~ **`{x: -22.451, y: 0, z: -15.733}`** | 225 |

"Clear" is now a single minimum-pairwise-clearance figure (4.51 m, computed across all 7 points) rather than a per-point nearest-neighbour figure — every spawn has 2–3× the old table's tightest clearance now that there's room. Re-verified on the live runtime NavMesh with `RoomGate_Zone1`'s carving obstacle active (its default closed state, matching the real zone-1 fight): all 7 on `NavMesh.SamplePosition` at 0.083 m offset, zero `Building`-layer overlap within 1.2 m.

Reads as: two Skeptic Grunts step off opposite boardwalks with gnomes behind them — the ambush the zone is named for — then rollers come down the street from the north as the second wave, now with real distance between each beat instead of all seven crammed into 5 m.

`maxConcurrentEnemies` rises from 3 to 4 in zone 1. §8's peak-live-enemy budget is updated to match; this is a deliberate, recorded change, not drift.

**`RoomData_CulDeSac_WildWestCity_Zone2`** — roomName "The Showdown Circle", `bossOwnedWin` **true**, `maxConcurrentEnemies` 1 (unused), **`spawnPoints` empty**. Zone 2 is boss-only per the owner's decision; no regular enemies. Do not copy `CulDeSac_BossArena`'s `_rooms[0]` spawn array — its four entries are null and vestigial.

**Boss placement.** Pre-place `pfb_enemy_spincycle`, inactive, at world ~~`(−11.102, 0, −11.710)`~~ **`(−2.617, 0, −3.225)`** (local (0.43, −16.13), position shifted +12 m local Z with the §0 revision) with yaw 225 — the point `BossIntro()` teleports it to anyway, so the Editor view matches the runtime start. Then set on that instance:

| Field | Value | Why |
|---|---|---|
| `_introWalkTarget_X` | ~~−19.092~~ **−10.607** | world position of local (0.5, −15.5) — 6.4 m south of the doorway, out in open street, 6.3 m clear once the wagons are gone (shifted +12 m local Z with the rest of the boss-zone cluster) |
| `_introWalkTarget_Z` | ~~−19.799~~ **−11.314** | same point |
| `_imaginationVolume` | the scene's `ImaginationRestore_Volume` | must be added to this scene (§9); without it `DefeatSequence` skips the imagination ramp and calls `TriggerWin()` directly |

Also delete the stale `_introCamDistance: 11` override if it is carried over from `CulDeSac_BossArena` — that field no longer exists on the script.

### §6 The `GameManager` fix — six lines, legacy path intact

Today: `_upgradeScreen.OnUpgradeSelected += OnUpgradePicked` → `OnUpgradePicked()` → `LoadNextRoom()` → `SceneManager.LoadScene(...)`. In a single scene there is no next scene.

**Recommended minimal change.** One new property on `RoomManager`:

```csharp
/// <summary>True when a later zone exists in this scene — progression is in-scene, not a scene load.</summary>
public bool HasZoneAfterCurrent => _currentRoom >= 0 && _currentRoom < _rooms.Count - 1;
```

and one guard in `GameManager`:

```csharp
private void OnUpgradePicked()
{
    // ADR-0004: in a single-scene world the next zone's RoomTrigger owns progression.
    // Close the screen and hand control back in place — there is no scene to load.
    if (RoomManager.Instance != null && RoomManager.Instance.HasZoneAfterCurrent) return;

    LoadNextRoom();   // legacy scene-per-room path (World 2 / TownSquare)
}
```

Why this is the right shape:

- **The legacy path is untouched.** Every existing scene has exactly one `RoomDataSO`, so `HasZoneAfterCurrent` is false there and `LoadNextRoom()` runs exactly as it does today. `TownSquare_Room1` / `TownSquare_BossHall` are unaffected.
- **No re-ordering is needed.** `UpgradeScreen.OnCardPicked` already does `Hide()` (restoring `Time.timeScale = 1` and `AudioListener.pause = false`) *before* firing the event, so returning from the handler simply resumes play where the player stands. The previous draft's concern about a live frame at the wrong position does not apply — there is no reposition, by design.
- **It closes two existing holes for free.** `ShopScreen.OnEnterBossFight()` only calls `Hide()` and advances nothing — a dead end under scene-per-room, but exactly correct here: the player walks north to the zone-2 trigger. And `ShopScreen.OnBuyUpgrade()` → `UpgradeScreen.Show()` → pick, which today advances the room as a side effect, now returns harmlessly.
- `RandomRoomPool`, `InitRoomQueue`, `s_roomQueue`, and `CaptureLoadoutForTransition` all become inert for this world without being deleted. **Do not delete them** — World 2 still uses them.

Two supporting `GameManager` edits:

1. `ZoneIndexByScene` gains `{ "CulDeSac_WildWestCity", 0 }` and `ZoneStartScene[0]` repoints to it. **Required** — `TriggerWin` reads `ZoneIndexByScene` to unlock zone 1, and `Start()`'s zone-start check gates `ProgressionSystem.ResetRunState()` and the run-start UI.
2. `_totalEnemyCount` is derived as `_livingEnemyCount + EnemySpawner.MaxTotalSpawns`, defaulting to **20** when no `EnemySpawner` exists — which is the case here. The HUD would count 20 → 7 for a 13-enemy run. Re-derive it by summing `RoomDataSO.spawnPoints[].spawnCount` across `LevelBuilder.RoomData` plus pre-placed bosses: **5 + 7 + 1 = 13**. Cosmetic, but visibly wrong on a podcast.

### §7 Loot: one merged drop table, zone-2 loot placed around the wagons

`LevelBuilder` holds exactly one `_dropTable` per scene, so `WeaponDropTableSO_CulDeSac_Room1_v2`, `..._AmbushAlley_v2`, and `..._BossArena` collapse into a single new **`WeaponDropTableSO_CulDeSac_WildWestCity`**. All `worldPosition` fields are world-space.

**Leave `envProps` empty.** The old tables' env props are the old rooms' dressing (25 entries in Room 1's alone); this city's dressing is hand-placed under `[Street Props]`. Spawning both would double-dress the street — and Room 1's table would spawn four *more* covered wagons.

Suggested anchors, all verified ≥ 2 m clear:

| Purpose | Local (x, z) | `worldPosition` |
|---|---|---|
| Zone 0 scattered weapon A | (−3.0, −55.0) | `{x: -41.012, y: 0, z: -36.770}` |
| Zone 0 scattered weapon B | (3.0, −54.0) | `{x: -36.062, y: 0, z: -40.305}` |
| Zone 0 cardboard pile | (−1.25, −56.0) | `{x: -40.482, y: 0, z: -38.714}` |
| Zone 0 workbench | (−3.25, −47.0) | `{x: -35.532, y: 0, z: -30.936}` |
| Zone 1 scattered weapon C | ~~(−5.0, −43.0)~~ **(−6.5, −43.0)** | ~~`{x: -33.941, y: 0, z: -26.870}`~~ **`{x: -35.002, y: 0, z: -25.809}`** |
| Zone 1 cardboard pile | ~~(5.0, −43.0)~~ **(5.5, −39.0)** | ~~`{x: -26.870, y: 0, z: -33.941}`~~ **`{x: -23.688, y: 0, z: -31.466}`** |
| Zone 1 workbench | ~~(3.0, −42.0)~~ **(6.5, −32.0)** | ~~`{x: -27.577, y: 0, z: -31.820}`~~ **`{x: -18.031, y: 0, z: -27.224}`** |

**Zone 1 loot respaced 2026-08-26 (second geometry revision, §0).** With zone 1 now 20 m deep instead of 8 m, the zone-0/zone-1 workbenches were only ~8 m apart — the owner playthrough that triggered this revision flagged exactly this. The zone-1 workbench moved deep into the new open floor (separation from the zone-0 workbench: 8.0 m → **17.9 m**); the scattered weapon and cardboard pile were nudged to keep ≥1.5 m clearance from the rewritten zone-1 spawn points (§5) rather than left at their old, now-cramped entrance positions. All three re-verified on the live runtime NavMesh (`NavMesh.SamplePosition`, 0.083 m offset, matching every other verified point in this scene) with zero `Building`-layer overlap.

**Constraint for zone-2 loot:** `LevelBuilder` spawns the whole table at `Start()`, while the wagons are still present. Any zone-2 pickup must therefore be ≥ 1.5 m clear **with the wagons in place** — keep zone-2 loot in the local Z −30 … −24 band, which is clear of both wagon footprints. Accepted consequence: all loot for the whole city exists from frame 0. The gates make it unreachable in practice; a per-zone spawning mechanism is not worth building.

### §8 Performance — no new gating system; the real risks are elsewhere

The position from the previous draft holds and is unchanged: **enemies are already gated for free** by `RoomManager`, **gating props would be a net loss** (an open street where everything is visible from everywhere — deactivating distant props buys pop-in, not frames), and **the scene is small** (47 prefab instances, one plane, one light). A streaming system would be more code than the encounter layer it serves.

Two things the re-measurement changes:

**Drop the `_useBakedNavMesh` opt-out proposed on 2026-08-25.** The runtime bake is collider-driven and will correctly carve this scene; `NavMesh.RemoveAllNavMeshData()` means the Editor-baked asset is discarded regardless, so protecting it achieves nothing. Keep the default path every other scene uses and simply measure the bake against the scene-start budget. Simpler, and one fewer branch in a shared system.

**A real, pre-existing NavMesh defect this scene will expose.** The project has exactly **one** NavMesh agent type (ID 0: radius 0.5, height 2, climb 0.75), and the mesh is baked for it. But `pfb_enemy_spincycle` has `m_Radius: 1, m_Height: 4` and `pfb_enemy_wagonwheel_roller` has `m_Radius: 0.95` with `m_BaseOffset: 0.9` — both on agent type 0. They are pathing on a mesh carved for an agent half their width, so any gap between 1.0 m and 2.0 m reads as walkable and physically is not. On this street that is every boardwalk gap. The §1/§5 layouts keep both away from those pinch points, but the fix belongs in the backlog: either add a second agent type for large agents, or reduce those two agents' radius to match their actual visual footprint. The second is almost certainly correct and cheaper.

**Texture residency remains the headline risk.** `docs/TECHNICAL_DESIGN.md` §3.3 sets < 150 MB per room; §3.4 records 2.6 GB of source textures with no platform import overrides. All 10 buildings plus 34 props are now resident for the whole run instead of 3–4 per scene. **The single-scene pivot is likely to breach the per-room texture budget on its own.** It does not create the defect — it removes the scene-boundary partitioning that was hiding it. TDD §3.4's import-policy work (`AssetPostprocessor`, per-platform max sizes, ASTC) is a **prerequisite for this scene shipping**.

| Budget | Target | Source |
|---|---|---|
| Draw calls | < 100 | TDD §3.2 |
| Scene triangles | < 300k | TDD §3.2 |
| Texture memory, steady state | < 150 MB | TDD §3.3 — **at risk** |
| Frame time, minute 1 vs minute 12 | no thermal regression | TDD §3.1 |
| Scene-start hitch (incl. runtime NavMesh bake) | ≤ 500 ms | this ADR |
| Live enemies, peak | **≤ 4** | zone 1 `maxConcurrentEnemies` (was ≤ 3) |

Shadow distance deserves a look: one realtime directional light with soft shadows now covers a 40 × 47.5 m set rather than a ~30 × 30 m room. Capping shadow distance in the URP mobile asset is the cheap first move if GPU time is over.

### §9 Old scenes stay on disk, untouched

`CulDeSac_Room1`, `CulDeSac_Room1_v2`, `CulDeSac_AmbushAlley`, `CulDeSac_AmbushAlley_v2`, `CulDeSac_SaloonFront`, `CulDeSac_MailboxRow`, and `CulDeSac_BossArena` are **kept, not deleted, not edited.** They are the only record of the encounter tuning being migrated. `EditorBuildSettings.asset` is likewise left alone beyond adding `CulDeSac_WildWestCity`; pruning it is a separate owner decision.

`CulDeSac_BossArena` additionally holds two objects that must be **copied** into the city scene: `ImaginationRestore_Volume` (a global `Volume`, priority 10, weight 0, profile GUID `9c77ae4236ddc4313b73531f9e52638c`) and `PostProcess_Volume`.

---

## Required changes outside `RoomManager`

Dependency order. Item 5 is the only change to a shared runtime system, and it is a read-only property.

| # | Change | Severity |
|---|---|---|
| 1 | Add the scene to `EditorBuildSettings.asset` | **Blocker** |
| 2 | Add `{ "CulDeSac_WildWestCity", 0 }` to `ZoneIndexByScene`; repoint `ZoneStartScene[0]` | **Blocker** |
| 3 | `GameManager.OnUpgradePicked` guard (§6) | **Blocker** |
| 4 | Add `LevelBuilder` + `RoomManager` to the scene; assign `_roomData` = the three new SOs **in order**, `_dropTable`, `_spawnPointMarkerPrefab`, `_spawnRoot`, `_workbenchPrefab`, `_cardboardPilePrefab` | **Blocker** |
| 5 | Add `RoomManager.HasZoneAfterCurrent` (§6) | Required |
| 6 | New `WildWestCityZoneDirector` (§4) | Required |
| 7 | `NavMeshModifier` (ignore-from-build) + carving `NavMeshObstacle` on both covered wagons (§4) | **Silent failure if missed** — stale NavMesh hole in the boss arena |
| 8 | Set `_introWalkTarget_X = −19.092`, `_introWalkTarget_Z = −19.799` on the boss instance (§5) | **Silent failure if missed** |
| 9 | Copy `ImaginationRestore_Volume` and `PostProcess_Volume` from `CulDeSac_BossArena`; wire `SpinCycleAI._imaginationVolume` | Required |
| 10 | Author the three `RoomDataSO`s and the merged `WeaponDropTableSO` (§5, §7) | Required |
| 11 | Place two `RoomTrigger`s and two `RoomGate`s at §1's coordinates; set `roomIndex` 1 and 2 by hand | Required |
| 12 | Set `LevelBuilder._cameraYawDegrees = 45` (the **camera's** yaw — matches the ENV root's 45 following the H3 camera nudge; see §note below) | Required |
| 13 | Derive `GameManager._totalEnemyCount` from the zones (§6) | Cosmetic (HUD) |

**`_spawnPointMarkerPrefab` is not optional.** `LevelBuilder.BuildSpawnPoints` logs a warning and returns an empty list without it, which would silently produce three empty zones.

**`_arenaBoundaryRadius` needs no change.** B87 already set it to 23 around the ground centre — the previous draft's blocker is resolved. The boss arena centre sits 4.87 m from the clamp centre, so the clamp never intrudes on the fight.

---

## Alternatives considered

**1. Additive scene loading.** Rejected: the directive is one continuous street; additive loading of overlapping room scenes produces coordinate conflicts, duplicated managers, and a lighting/NavMesh seam at every boundary, and adds an async hitch to a scene too small to need one.

**2. City as a visual backdrop behind the existing rooms.** Cheapest possible change, but it does not deliver the directive at all.

**3. Distance-based activation/streaming for props and enemies.** Rejected on evidence: enemies are already gated, props are always visible on an open street, and the whole scene is 47 prefab instances. §8 records the actual measured risk instead.

**4. `_zoneSceneBindings` on `RoomManager`** (the 2026-08-25 proposal). Rejected in favour of §3's subscriber. An empty-`spawnPoints` `RoomDataSO` is already a safe no-op zone and `OnRoomActivated` is already the right seam, so the array would add serialized structure to a shared system to serve one scene's composition.

**5. Extend `RoomDataSO` to carry gate/boss references.** Rejected on a hard constraint: a ScriptableObject asset cannot reference a scene GameObject. ADR-0002 recorded this.

**6. Hand-author all three zones in `RoomManager._rooms` via the Inspector.** Needs zero code and would work — `RoomData` accepts scene `EnemySpawnPoint` references directly. Rejected because it abandons ADR-0002's data-driven encounter layer, which the audience-contribution goal in `docs/ROADMAP.md` depends on: a zone should be a data asset a non-expert can edit, not a hand-wired object graph.

**7. Boss at the far north end, past the buildings.** The intuitive reading of "boss at the north end". Rejected on measurement: r ≈ 4.9 m there versus 8.39 m ten metres south, because `saloon_facade` stands in the middle of that stretch (§2).

**8. Four or five zones, matching the old run length.** Rejected: a fourth zone breaks `GameManager`'s index-0/index-1 screen routing, and the street cannot hold four combat zones plus the boss arena. §5's larger encounters address pacing instead.

**9. Clear only one covered wagon, keeping the other as cover.** Rejected on measurement: one wagon buys r 6.19–6.88 m, both buy 8.39 m (§2).

---

## Consequences

### Positive

- The player walks one continuous street with no loading screens mid-run.
- `RoomManager`, `RoomDataSO`, `LevelBuilder`, `RoomTrigger`, and `RoomGate` are **used as designed**. The only shared-code change is one read-only property.
- The boss's runtime saloon lookup (`_saloonNameContains`) resolves correctly for the first time — the intro was written for this scene's asset.
- The pivot incidentally fixes `ShopScreen`'s dead-end exit and its upgrade-side-effect room advance (§6).
- Three `RoomDataSO`s plus one drop table is a legible authoring target for an audience contributor.

### Negative / risks

- **Run length drops from five encounters to three**, mitigated but not erased by larger zone-0/zone-1 encounters.
- **Texture residency roughly triples** (§8). The pivot removes the accidental partitioning that was hiding a known import-policy defect.
- **The boss fight sits mid-street, not at the north end** — a different composition from what "The Showdown Circle" implied. The saloon becomes a door rather than a back wall.
- **Boss and roller agents are over-sized for the baked NavMesh** (§8) — pre-existing, but this street's pinch points expose it.
- **Building colliders do not match building meshes** (§0) — the player can walk into porches.
- `GameManager` keeps two progression paths (in-scene and scene-load) until World 2 is designed. Small and explicitly indexed here, but it is debt.
- The "no room reuses another room's layout" rule stays suspended **only** for this scene.

### Out of scope / explicitly deferred

- **Asset defects B78–B85** — baked `X = 270°` rotations on six buildings, the undersized `western_house_tall` (B79), the ~1 cm desert-scatter props (B80), stale duplicate prefabs (B81), inconsistent import scale (B85). The measured scene still shows the symptom: `barber_shop` at scale 110, `twostoryhouse` at 4, `porchcabin` at 0.6, `shedwithcrate` at 0.55, in two rows of buildings.
- Building collider/mesh mismatch (§0) — new backlog item.
- `SpinCycleAI` jump-back NavMesh clamp (§2) — new backlog item.
- Large-agent NavMesh agent type (§8) — new backlog item.
- Deleting or pruning old scenes and build-settings entries.
- **World 2 / TownSquare** stays on scene-per-room; this ADR does not prejudge it.
- Per-zone `_arenaCenter` clamping — designed on 2026-08-25, deliberately not built. B87's 23 m clamp plus gates is sufficient.

---

## Open questions for the owner

Everything blocking implementation is resolved. Three items remain, none of which stop `unity-gameplay-engineer` from starting.

1. **RESOLVED — camera yaw vs. street yaw.** Originally a measured 10° mismatch (`[ENV - Static]` at yaw 45°, `pfb_CM_FollowCam` at yaw 35°, walking "up the street" 10° off screen-vertical). The owner resolved this during implementation by nudging the camera rig from yaw 35° to yaw 45°, matching `[ENV - Static]` exactly (`docs/BACKLOG.md` H3) — `LevelBuilder._cameraYawDegrees` was updated to 45 to match (§ table above, implementation checklist item 12). ADR-0001's accepted text (yaw 0°, offset `(0, 5.5, −7.57)`) still does not match the live rig (yaw 45°, pitch 36°, offset `(−4.8378, 10.02, −13.447)`) — **ADR-0001 should still be amended to match reality**, independent of this resolution.
2. **RESOLVED — barricade art for the two `RoomGate`s.** The owner decided during implementation: **just an invisible wall.** Both `RoomGate`s (now 26 m wide, §1) and the two permanent `StreetBoundary_West`/`StreetBoundary_East` colliders added in the second `code-reviewer` fix pass (§1) ship with no visual barricade prop. Revisiting this with real art remains an option but is not planned work.
3. **ADR status conflict, still unresolved.** `docs/TECHNICAL_DECISIONS.md` lists ADR-0001/0002/0003 as Proposed; ADR-0002's own header says Accepted and `docs/ROADMAP.md` records production authorized 2026-08-19. Sprint 0 shipped against 0001/0003. Carried forward from the 2026-08-25 draft.

---

## Validation before this scene is called done

1. Walk the street start to finish. Each zone activates exactly once, in order; no zone can be reached past a closed gate; walking back into a cleared zone re-triggers nothing.
2. Zone 0's upgrade screen and zone 1's shop screen appear and return control **without a scene load**, and the player resumes where they stood.
3. Every spawn point, workbench, pickup, and cardboard pile is on the NavMesh and reachable — `NavMesh.CalculatePath` returns `PathComplete` from each zone entry.
4. On zone-2 entry, in this order: both covered wagons vanish, the NavMesh heals (confirm with the navigation overlay — **no hole where the wagons stood**), and SpinCycle's intro plays from the saloon doorway with the camera dolly clear of where `covered_wagon_01` was.
5. SpinCycle's full spin charge, jump-back, and airborne slam stay in frame (ADR-0001 acceptance criterion) and the boss never ends an attack inside a building. `DefeatSequence` triggers the win exactly once.
6. HUD enemy count starts at 13 and reaches 0 as the boss dies.
7. Run `LevelBuilder.ValidateCameraClearance` with `_cameraYawDegrees = 45` (updated from the original 35° draft — see the resolved open question above and `docs/BACKLOG.md` H3) and record vertex counts, as B75/B77 did.
8. Profile a full run on a representative 3–4-year-old device against §8's table, recording frame time at minute 1 versus minute 12, and the scene-start hitch including the runtime NavMesh bake. A figure without its scenario is not evidence.
