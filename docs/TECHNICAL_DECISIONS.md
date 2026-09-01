# BoxForged — Technical Decisions

Index and summary of accepted technical decisions and package approvals. Full context lives in `docs/adr/`.

---

## Architecture Decision Records

| ADR | Title | Status | Date |
|---|---|---|---|
| [0001](adr/0001-fixed-low-follow-camera.md) | Fixed low-angle follow camera (no rotation) | **Proposed** | 2026-08-19 |
| [0002](adr/0002-full-scene-rebuild.md) | Full scene rebuild on extracted room data | **Proposed** | 2026-08-19 |
| [0003](adr/0003-attack-telegraph-channel.md) | Occlusion-independent attack telegraph channel | **Proposed** | 2026-08-19 |
| [0004](adr/0004-world1-single-continuous-scene.md) | World 1 is one continuous scene, zoned by `RoomManager` | **Accepted** | 2026-08-25, finalized 2026-08-26 |
| [0005](adr/0005-world2-single-continuous-scene.md) | World 2 is one continuous scene; single-scene becomes the default | **Accepted** | 2026-08-31, §3/§4 amended 2026-09-01 by 0006 |
| [0006](adr/0006-world2-zone-scale-and-arena-metric.md) | World 2 zone scale, and the two budget metrics that were measuring the wrong thing | **Accepted** | 2026-09-01 |

ADR-0001–0003 await owner approval. ADR-0004 was accepted 2026-08-26 once the owner resolved every question it had left open; it is the implementation spec for the city scene but does not by itself authorize a commit. ADR-0005 was accepted 2026-08-31 with the owner having explicitly delegated the scene-architecture choice to `technical-director`; its open questions are content decisions and do not block scaffolding. ADR-0006 amends ADR-0005 §3/§4 and restates `TECHNICAL_DESIGN.md` §6.4 after the owner playtested World 2's built geometry.

> **Status conflict, surfaced not resolved (2026-08-25):** this table lists 0001–0003 as Proposed, but `adr/0002-full-scene-rebuild.md`'s own header says **Accepted** and `docs/ROADMAP.md` records production as authorized on 2026-08-19. Sprint 0 shipped against 0001/0003. Owner should confirm which is correct and the losing source should be corrected.

### ADR-0001 — Fixed low-angle follow camera

Replaces the "fixed top-down, locked" camera with a fixed-rotation follow camera at **pitch 36°, FOV 45°, yaw 0°**, derived offset `(0, 5.5, -7.57)`.

The decision specifies **pitch, distance, and FOV with measurable framing criteria** rather than an offset triple, because an offset hides the constraint `pitch > FOV/2` — below which the horizon enters frame and ground depth runs to infinity.

Two corrections of record: the real rig was `(7.879929, 11, -10)` at FOV 40, not the documented `(0, 12, -8)`; and it carried an undocumented **−38.2° yaw** that silently rotated the control mapping, since movement is camera-yaw-relative.

Rejected: `CinemachineDeoccluder` (camera collision becomes a machine-checked level constraint instead), and a dynamic or per-encounter rig (complexity a two-person team cannot debug live; deferred pending boss-room playtest).

### ADR-0002 — Full scene rebuild on extracted room data

All scenes rebuilt from scratch under the new camera. `LevelBuilder` + `WeaponDropTableSO` architecture preserved.

The material change: **promote `RoomData` to `RoomDataSO`** *before* rebuilding. Room definitions currently exist only as prefab-instance overrides with spawn points as `objectReference` fileIDs to scene-local GameObjects — they are destroyed by a rebuild. The environment layer is portable; the encounter layer is not.

Scenes become thin composition roots. Camera clearance (≥ 8 m rear, ≥ 6 m overhead, combat radius ≤ 9 m) is validated in the builder.

Existing scenes remain on disk; deletion is a separate decision.

### ADR-0003 — Occlusion-independent attack telegraph channel

Not requested; recorded because ADR-0001 cannot ship safely without it.

BoxForged has **no telegraph system** — every attack tell is a whole-body material tint, which works only because the current camera sees enemies separated and unoccluded. Parryable vs un-parryable is encoded **entirely in hue**, a standing accessibility defect.

Adds an occlusion-independent overhead indicator on the existing overlay camera stack, carries parryability on **shape**, adds per-class audio, keeps the tint as reinforcement. URP decal projectors rejected (mobile depth prepass cost; `m_RendererFeatures: []` today).

**ADR-0001 and ADR-0003 should be treated as one decision.**

### ADR-0004 — World 1 is one continuous scene, zoned by `RoomManager`

Owner directive: World 1 becomes **one scene, `CulDeSac_WildWestCity.unity`**, played as a single linear walk down one street, boss included. The random room-order draw is retired for this world.

No new mechanism is invented, and **no geometry changes.** `RoomManager` already models an ordered list of encounter zones inside a single scene, activated by `RoomTrigger` boundary crossings with no scene load. **Three zones**, in the street-local frame (note `[ENV - Static]` is rotated **yaw 45°** — the ADR gives the local↔world transform and every coordinate in both frames): The Arrival (Z −59.45…−44), Ambush Alley (Z −44…−36), The Showdown Circle (Z −36…−11.95). Boundaries sit at the two lines where both building rows have a wall. Zones 0 and 1 carry a **mixed roster** of WagonWheelRoller / SkepticGrunt / GnomeGrunt (5 and 7 spawns, `maxConcurrentEnemies` 3 and 4) — larger than the old encounters, which is how the drop from five encounters to three is absorbed. Zone 2 is **boss-only**.

**The boss-arena space question, measured:** with both covered wagons cleared the largest open circle in the street is **r 8.39 m** at street-local (0.80, −30.90); with them present it is **5.46 m**. That is enough for SpinCycle's 4.8 m spin charge, 4 m jump-back, and 3 m spin AoE. The fight therefore moves ~10 m **south** of the previous plan — the far north end past the buildings measures only r ≈ 4.9 m because `saloon_facade` stands in it. The saloon becomes the door the boss emerges from, which its runtime `_saloonNameContains` lookup already expects. **No ground expansion and no saloon move.**

**`_zoneSceneBindings` (the 2026-08-25 proposal) is withdrawn.** An empty-`spawnPoints` `RoomDataSO` with `bossOwnedWin` is already a safe no-op zone, and `RoomManager.OnRoomActivated` is a static event with zero subscribers. One small scene script (`WildWestCityZoneDirector`) clears the wagons, activates the pre-placed boss, and opens the gates. `RoomManager` gains only a read-only `HasZoneAfterCurrent` property; `GameManager.OnUpgradePicked` gains a two-line guard so the legacy scene-load path stays intact for World 2. The player's arena clamp needs no change — B87 already fixed it to 23 m.

Rejected: additive scene loading, distance-based streaming, `_zoneSceneBindings`, hand-authoring zones in the Inspector, a boss arena at the far north end, four-plus zones, and clearing only one wagon.

Performance position: **no new gating system**, and the `_useBakedNavMesh` opt-out is dropped (the runtime bake is collider-driven and correct here). Three risks recorded instead: texture residency (all 10 buildings resident for the whole run, likely breaching the < 150 MB/room budget), a stale NavMesh hole if the wagons are deactivated without a carving `NavMeshObstacle`, and boss/roller `NavMeshAgent` radii (1.0 / 0.95) exceeding the single baked agent type's 0.5.

> **Update from measurement (B112, 2026-08-27):** the texture-residency prediction **did not materialize** — a full on-device playthrough measured **41.2 MB / 52 textures** against the 150 MB budget. The real cost of a resident whole-world scene is **draw calls (205 vs < 100) and triangles (356.7k vs < 300k)**, with the SRP Batcher contributing **zero**. ADR-0005 §3 re-bases the budgets on this.

### ADR-0005 — World 2 is one continuous scene; single-scene becomes the default

World 2 (The Backyard/Dojo) is **one continuous scene, `Backyard_Dojo.unity`**, with **three** `RoomManager` zones — back gate/courtyard → garden gauntlet (containing the Koi Pond sub-space and the Skeptic beat) → the garden end/Blossom Court with the Grasscutter. Three zones consume `GameManager.ShowRoomClearScreenDelayed`'s existing index routing with **zero** code change.

**This also promotes single-continuous-scene from "a World 1 deviation" to the project default** for team-built worlds, retiring ADR-0002's "one scene per room" corollary as the default while **preserving** its `RoomDataSO`/`LevelBuilder` data-driven encounter layer, which is the load-bearing half.

> **Update, 2026-09-01:** ADR-0005 §6 item 1 ("author the ENV root at yaw 0") was reversed by explicit owner direction after acceptance — `Backyard_Dojo.unity`'s `[ENV - Static]` is now rotated 45°, matching World 1's diagonal composition, with the camera intentionally left un-rotated to produce the visible skew. The coordinate-transform tax that lesson existed to avoid did materialize (gates, triggers, spawn points, and drop-table coordinates all needed re-deriving). See ADR-0005 §6 item 1 for the full record.

Decided on facts about the project, not preference:

- **The scene-per-room path is currently dead code with no valid targets.** `RandomRoomPool` is empty (`GameManager.cs:52`), `LoadNextRoom()`'s exhausted branch logs an error instead of loading (`:607-615`), every per-room scene was deleted (commit `84a3a44e`), and `ZoneStartScene[1] = "TownSquare_Room1"` names a scene that never existed. Choosing the "documented default" would mean reviving a path with no working reference implementation.
- **The shop screen is unreachable under scene-per-room.** Routing keys off in-scene zone index, and a per-room scene's `RoomManager` always has one room at index 0 — so every clear routes to the upgrade screen.
- **Two canon story beats require the zones to coexist.** The post-boss victory beat is *"I look back at the engawa. The chair is still there"* — impossible once the Koi Pond scene has unloaded. And the Koi Pond is one of three *random* rooms in the v4 GDD, so a CANON Skeptic appearance would fire in ~1 run of 3.
- **A backyard is one contiguous space in the fiction**, its bamboo stockade is a diegetic boundary (World 1 needed retrofit invisible walls), and a long linear yard validates better against ADR-0001's no-deoccluder camera clearance than a set of small walled rooms.
- **Consistency is load-bearing here** — TDD §1: the two team-built worlds are the reference implementation for an audience-built World 3, and "two ways of doing the same thing is worse than one mediocre way."

The scene-load path (`RandomRoomPool`, `LoadNextRoom`, `s_roomQueue`, `CaptureLoadoutForTransition`) **stays alive, not deleted** — but must be re-qualified before any future world uses it.

Also decided: **whole-scene** performance budgets replacing per-room ones (< 100 draw calls, < 300k tris, < 150 MB textures, ≤ 20 distinct ENV materials, ≤ 500 ms scene-start hitch, combat radius ≤ 9 m); the boss arena is **budgeted before `GrasscutterAI` is written** (clear circle r ≥ 8.5 m, Spin-Dash travel ≤ 8 m, mandatory `NavMesh.SamplePosition` clamp on the dash landing point) rather than measured after, correcting the blueprint's 36 m court which violates TDD §6.4 two-fold; and `WildWestCityZoneDirector` is **renamed to a reusable `ZoneDirector`** rather than copied, since all four of its serialized fields are already scene-agnostic data.

Rejected: scene-per-room, a second one-off deviation, a hybrid boss-only scene, additive per-zone loading, five zones per the v4 GDD, a streaming/world-partition system, and copying the zone director.

Blocking prerequisites recorded: the **large-agent NavMesh** decision (the Grasscutter is the second boss to exceed the single baked agent type's 0.5 radius) and the **texture import policy** before any new dojo art.

**Amended 2026-09-01 by ADR-0006** — the two acceptance metrics quoted above ("combat radius ≤ 9 m", "clear circle r ≥ 8.5 m") were both measuring states that cannot occur, and the Spin-Dash cap of 8 m was undefended slack. See below.

### ADR-0006 — World 2 zone scale, and the two budget metrics that were measuring the wrong thing

The owner playtested World 2's built Stage A geometry and reported **zone 1 and zone 2 both too small** (zone 0 not flagged). Investigating that found two acceptance criteria in ADR-0005 §3 that could not be satisfied as written, and one boss constraint that had never been derived.

**New dimensions.** Zone 1 → **20.0 m × 28.0 m** (from 16.5 × 22.0; the 20 m width is `BACKLOG` B107's playtested World 1 figure for the identical complaint, not a fresh derivation). Zone 2 → **r = 10.0 m, 20.0 m across, centre `(0, 0, 55.0)`** (from r = 8.5 / 17.0). Zone 0 **unchanged** — it is the calibration point, at ~294 m² of free floor. Grasscutter Phase-2 Spin-Dash **≤ 8 m → ≤ 6.5 m**. Cherry tree stays at the arena centre and may now be 8.0 m tall instead of 7.0 m.

**Two metrics restated, both preserving the playtested numbers rather than discarding them:**

- **M1 — combat radius** (`TECHNICAL_DESIGN.md` §6.4.1, project-wide). Still ≤ 9 m, but measured over each contiguous `maxConcurrentEnemies` window of `spawnPoints[]` — the only live set `RoomManager` can produce — counting **closing** enemies only. The whole-roster reading describes a state the runtime cannot produce and World 1's 59.5 m street does not satisfy it.
- **M2 — boss-arena fight floor** (§6.4.2). Outer radius ≥ authored value, **radial fight band ≥ 8.5 m** (what World 1's measured 8.44 m actually described), interior obstruction ≤ 2% of floor and ≤ 1.0 m wide, boss traversal ≤ **0.35 × diameter** (World 1's playtested 0.284). The retired "largest inscribed obstacle-free circle" reading evaluates to `(R − r_t)/2` and so demands a **34.7 m** arena for a 0.70 m trunk. **World 1 passes M2 unchanged.**

**Diagnosis, for the record.** Zone 1's problem was fragmentation, not area: its total free floor already matched zone 0's, but the Crane Duelist fight had ~50 m² of usable floor. Zone 2's problem was the boss, not the metres: at an 8 m dash in a 17 m court the boss erased 47% of the arena per commitment against World 1's playtested 28%. And *"17 m is already at the camera's 16.8 m limit"* equated a diameter with a **follower** camera's per-position visible width — at r = 8.5 the boss at the opposite rim was already off-frame, so the growth ban rested on a limit already passed.

**Budget.** ~122.6k tris of 300k. Worst-case wall draw calls **fall by 9** versus the built scene, because the enlargement is paid for by an 8.0 m BD-01 variant on straight runs, and a 16-gon at r = 10.0 has 3.978 m sides — so the bigger arena uses the **same 14 modules** at native scale. Prop counts are **frozen** while area grows. Draw calls remain gated on B112's SRP-Batcher-at-zero investigation, which this decision neither improves nor worsens.

Granted **on** one condition: the ground-plane dash-lane telegraph (ADR-0003 channel) must exist before the arena is accepted at 20 m, because it replaces simultaneous on-frame visibility as the fairness mechanism.

Rejected: pure redefinition with no dimension change (the current arena fails a well-formed M2 at 8.15 m of 8.5 m); moving the tree off-centre (the victory beat's bloom shot cannot be framed at 20 m against F = 15.3 m); enlarging without cutting the dash (ratio-preservation would demand 28.6 m); uniform scale-up of all three zones; prop-thinning alone; widening the engawa; and adding a wave affordance to `RoomDataSO`.

Implementation is `docs/BACKLOG.md` **B116** (`unity-gameplay-engineer`) — a re-layout, not World 1's rigid translation. **M1 and M2 are not yet implemented in `LevelBuilder`'s validation**, and both were violated silently for a full design pass.

---

## Engine and pipeline

| Decision | Value | Notes |
|---|---|---|
| Engine | Unity 6 LTS `6000.5.3f1` | |
| Render pipeline | URP 17.5.0, mobile quality tier | `Mobile_RPAsset`, `Mobile_Renderer` |
| SRP Batcher | Enabled (`m_UseSRPBatcher: 1`) | Per-instance `Material` copies still batch; **`MaterialPropertyBlock` breaks batching** |
| MSAA | Disabled (`m_MSAA: 1`) | Revisit — aliasing is more visible at the closer camera |
| Shadow atlas | 256×256 | Very tight; see `docs/BACKLOG.md` B17 |
| Platforms | iOS + Android, landscape only | Owner performs all final builds |
| Language | C#, `Boxhead.*` namespaces | Legacy root; do not rename opportunistically |
| Assemblies | One (`Assembly-CSharp`) + trivial `StatSystem.asmdef` | No test assemblies exist |

---

## Approved packages

From `Packages/manifest.json`, reconciled against `docs/PROJECT_CONTEXT.md`.

| Package | Version | Status |
|---|---|---|
| `com.unity.cinemachine` | 3.1.7 | In use — camera rig |
| `com.unity.render-pipelines.universal` | 17.5.0 | In use |
| `com.unity.inputsystem` | 1.19.0 | In use — `PlayerInput` + on-screen controls |
| `com.unity.ai.navigation` | 2.0.14 | In use — runtime NavMesh bake |
| `com.unity.test-framework` | 1.7.0 | **Installed, unused** |
| `com.unity.timeline` | 1.8.12 | Installed; cutscenes use video playback, not Timeline |
| `com.unity.visualscripting` | 1.9.12 | No known use |
| `com.coplaydev.unity-mcp` | local file ref | Editor automation |

**No new third-party Unity or Asset Store package may be installed without explicit owner approval.**

### Asset sources

| Source | Status |
|---|---|
| Meshy — characters, weapons | Approved (`PROJECT_CONTEXT.md`) |
| Low Poly Mega Pack / Polyworks (Off Axis Studios) | Approved (`PROJECT_CONTEXT.md`) |
| SimpleTown | **Present in tree, in no approval record** |
| ExplosiveLLC (RPG Character Mecanim, SuperCharacterController) | **Present in tree, in no approval record** |
| Polylised — Medieval Desert City | **Present in tree, in no approval record** |

Reconciliation is `docs/BACKLOG.md` D6.

---

## Performance budget

Recorded in full in `docs/TECHNICAL_DESIGN.md` §3. Target: stable 60 FPS on 3–4-year-old iOS/Android, with **sustained thermal behaviour over a full 10–15 minute run** as the real acceptance criterion.

Retained: < 100 draw calls · < 300k scene triangles · zero steady-state GC allocation (currently honoured — no LINQ anywhere in the codebase).

Added: **< 150 MB texture memory per room** and **< 200 MB download size** — both currently at risk. Texture import settings cap essentially everything at 2048 with no platform overrides, and `StreamingAssets/Cutscenes/` ships 326 MB of video for a feature now scoped to boss intros only.
