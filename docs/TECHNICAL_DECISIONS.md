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

ADR-0001–0003 await owner approval. ADR-0004 was accepted 2026-08-26 once the owner resolved every question it had left open; it is the implementation spec for the city scene but does not by itself authorize a commit.

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
