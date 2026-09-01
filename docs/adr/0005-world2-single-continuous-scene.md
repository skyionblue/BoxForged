# ADR-0005: World 2 (The Backyard/Dojo) is one continuous scene — and single-scene becomes the default for team-built worlds

- **Status:** **Accepted (architecture) — 2026-08-31.** The owner explicitly delegated this choice to `technical-director` rather than picking a pattern. The scene-architecture decision below is made and is the spec for scaffolding. The content questions in §Open Questions are owner-owned and do **not** block scaffolding. This ADR does not by itself authorize a commit.
- **Date:** 2026-08-31
- **Supersedes:** [ADR-0002](0002-full-scene-rebuild.md) §Decision step 3's "one scene per room" corollary, **as a default** — not only for World 1. ADR-0002's `RoomDataSO` / `LevelBuilder` data-driven encounter layer is **preserved and is the load-bearing half of that ADR**; only its scene-granularity corollary is retired.
- **Extends:** [ADR-0004](0004-world1-single-continuous-scene.md) — World 1 declared the single-scene pattern "a deviation for World 1's specific needs, not a default replacement of ADR-0002." This ADR revisits that on evidence and promotes it.
- **Related:** [ADR-0001](0001-fixed-low-follow-camera.md) (camera clearance is a level constraint), [ADR-0003](0003-attack-telegraph-channel.md)

---

## Context

### The question

ROADMAP Phase 3 authorizes World 2, The Backyard (Dojo): three rooms plus the Grasscutter boss and the Crane Duelist. Two patterns exist in this codebase and both are precedented:

1. **ADR-0002** — per-room Unity scenes, each room a `RoomDataSO`-driven data asset in its own scene, chained by `GameManager.LoadNextRoom()` → `SceneManager.LoadScene`. Still the documented default.
2. **ADR-0004** — one continuous scene with multiple `RoomManager` zones inside it, no scene loads between zones. Shipped for World 1, explicitly scoped as a World-1-only deviation.

World 2 does not inherit ADR-0004 automatically. This ADR decides which pattern World 2 uses, and — because the answer turns out to rest on facts about the whole project rather than about the dojo — whether the default itself should change.

### Fact 1: the scene-per-room path is currently dead code with no valid targets

This is the single most decisive input, and it was **not** true when ADR-0004 was written. Verified in `HEAD` today:

| Evidence | Location |
|---|---|
| `RandomRoomPool` is **empty** — `private static readonly string[] RandomRoomPool = { };` | `Core/GameManager.cs:52` |
| `LoadNextRoom()`'s exhausted-queue branch **logs an error instead of loading**, because there is no target scene | `Core/GameManager.cs:607-615` |
| Every per-room scene was **deleted from disk** | commit `84a3a44e` "Remove legacy per-room World-1 scenes" — `CulDeSac_Room1`, `Room1_v2`, `AmbushAlley`, `AmbushAlley_v2`, `SaloonFront`, `MailboxRow`, `BossArena` |
| `Assets/_Project/Scenes/` holds **one** gameplay scene | `CulDeSac_WildWestCity.unity` (+ `CharacterTest`, `ForgeLoop_Test`, `WeaponGripTest`) |
| Build settings hold **two** entries, one of them a dev scene | `EditorBuildSettings.asset` — `CulDeSac_WildWestCity`, `WeaponGripTest` |
| `ZoneStartScene[1] = "TownSquare_Room1"` points at a scene that **has never existed** | `Core/GameManager.cs:35`; `docs/ARCHITECTURE.md` §5 divergence 11 |

So "use the documented default" is not the conservative option. It means **reviving a path with zero working reference implementation** and doing new, unbudgeted work first: populate `RandomRoomPool` (or replace the random draw with a fixed sequence), give `LoadNextRoom()` a real terminal target, and re-qualify `CaptureLoadoutForTransition()` / `ProgressionSystem` run-loadout restore across a scene boundary — a transition that has not executed in this project since the World 1 rebuild. The single-scene path, by contrast, is implemented, playtested by the owner, and has survived five `code-reviewer` fix passes plus an owner-driven geometry revision (`docs/BACKLOG.md` B99–B108).

### Fact 2: the reward-screen routing only produces variety with in-scene zones

`GameManager.ShowRoomClearScreenDelayed` (`:544-556`) routes by **in-scene zone index**: index 0 → `UpgradeScreen`, index 1 → `ShopScreen`, index ≥ 2 → no screen (the boss zone; `OnRoomClearScreenShown` still fires unconditionally so a gate subscriber can never stall).

Under scene-per-room, each scene's `RoomManager` holds exactly one room, always at index 0. Every room clear therefore routes to the upgrade screen and **the shop screen is unreachable** — a real functional loss, not a stylistic one, and one that would have to be fixed (routing keyed off run progress rather than room index) before a per-room World 2 could offer a shop. A three-zone single scene consumes the existing routing exactly as written, with **zero** `GameManager` change. This is the same reason ADR-0004 fixed World 2 at three zones, and it applies identically here.

### Fact 3: the measured cost of a resident whole-world scene is not what ADR-0004 feared

ADR-0004 §8 named texture residency as the headline risk and predicted the single-scene pivot "is likely to breach the per-room texture budget on its own." The first real on-device capture (`docs/BACKLOG.md` B112, `docs/PERFORMANCE_PROFILING.md` session 2026-08-27, full `CulDeSac_WildWestCity` playthrough) says otherwise:

| Metric | Budget | World 1, measured on device | Verdict |
|---|---|---|---|
| Texture memory, steady state | < 150 MB (TDD §3.3) | **41.2 MB / 52 textures** | **Comfortably inside.** The prediction did not materialize |
| Draw calls | < 100 (TDD §3.2) | **205** | **Over by 2×** |
| Triangles | < 300k (TDD §3.2) | **356.7k** | **Over by ~19%** |
| SRP Batcher contribution | assumed active | **0** (Standard 204, SRP Batcher 0) | Not engaging at all — separate open defect |

The real cost of holding a whole world resident is **draw calls and triangles**, not texture memory. That reframes the World 2 decision rather than settling it against a single scene, because the dojo prop set is structurally cheaper on exactly the axis that failed: World 1 renders **ten unique Meshy buildings**, each with its own 27–31 MB BaseColor and its own material. The dojo set is one building (the shed), one cherry tree, and a repeated kit — stone lanterns, stepping stones, tatami, gravel, zen rocks, paper lanterns, bamboo wall — drawn largely from the shared Polyworks Asian atlas, i.e. **many instances of few materials**, which is the case GPU instancing and the SRP Batcher exist to serve. The blueprint's own budget for genuinely new dojo geometry is **< 8k triangles** (`docs/v4/levels/World2/backyard-dojo/unity-blueprint.md` §Performance Budget).

The honest position is therefore: a resident dojo is *plausibly* cheaper than a resident street, and the way to keep it that way is a hard whole-scene budget enforced during construction (§3 below) — not a scene boundary used as an accidental memory-partitioning device, which is what per-room scenes were doing for World 1 without anyone deciding it.

### Fact 4: the dojo reads as one space more strongly than the street did

World 1's justification was an owner directive ("one continuous street"). World 2's is the fiction itself. The restored zone lore (`docs/story/zones/backyard-dojo.md`) is written as a single walk through one enclosed yard: *"I push through the back gate and the weeds go quiet… There's a tree at the far end shaking pink all over the ground."* A real backyard is one contiguous, walled space. Splitting it into loaded scenes means inventing boundaries the fiction does not have; splitting a *street* at least had blocks and buildings to hide the seams behind.

Two structural bonuses follow from the yard being enclosed rather than open:

- **The perimeter is diegetic.** World 1 needed `StreetBoundary_West`/`_East` — two invisible retrofit colliders added after a physics flood-fill found the player could walk around the outside of the entire building row and skip the gates (ADR-0004 §1, second and third fix passes, `docs/BACKLOG.md` B99). The dojo's bamboo stockade **is** the boundary, authored from the start, visible, and part of the art.
- **Camera clearance improves with length, not with room count.** ADR-0001 ships **no deoccluder**; instead every walkable point wants ≥ 8 m clear behind and ≥ 6 m above along the camera axis, machine-checked by `LevelBuilder.ValidateCameraClearance` (`Systems/LevelBuilder.cs:143`). A set of small walled boxes (the blueprint's 34×28, 32×26, 28×24, 30×26 rooms) puts the player near a wall for much of every fight. A long linear yard puts the 8 m "behind" volume over floor the player just walked through. This is the same reason World 1's street validates acceptably.

### Fact 5: two canon story beats require the zones to coexist

This is the argument I find hardest to answer any other way.

The Koi Pond beat is not self-contained. The Skeptic sets a folded lawn chair flat on the engawa and leaves; *"the lawn chair stays flat on the boards, open and empty, and Kid has to step around it, and keeps half-looking at it the rest of the room."* Then, in the **post-boss victory beat**, after the Grasscutter falls and the cherry tree blooms:

> *I look back at the engawa. The chair is still there.*
> **KID:** *"Somebody planted you."*

Under scene-per-room, the Koi Pond scene is **unloaded** before the boss fight begins. "I look back at the engawa" is then not a camera turn — it is impossible, and the beat degrades to a voice line about an object the player cannot see. In one continuous scene it is exactly what it is written as: the player turns and the chair is still on the boards.

Second: under the v4 GDD the Koi Pond is one of **three random rooms**, so the Skeptic appearance — a CANON recurring character whose recognition is explicitly designed to *"build slowly — by the third zone, it begins to land"* (`docs/story/zones/backyard-dojo.md`, `docs/CREATIVE_STATE.md` §World 2 Enemies) — fires in roughly one run out of three. A linear single scene makes it guaranteed. ROADMAP Phase 3's current three-room scope has already retired the random draw; this decision is consistent with that, not a new imposition on it.

### Fact 6: consistency has unusual weight on this project

`docs/TECHNICAL_DESIGN.md` §1 states the constraint plainly: the two team-built worlds *"are documentation as much as they are levels,"* the audience builds World 3+, and *"two ways of doing the same thing is worse than one mediocre way."* Shipping World 1 as a single scene and World 2 as chained scenes means the reference implementation teaches two patterns, one of which currently does not work. An audience contributor with AI assistance will copy whichever they find. There is only one honest thing to point them at.

### What is not a factor

- **`RoomManager` needing work.** It does not. It has always modelled an ordered list of encounter zones inside one scene and never loaded scenes; the scene-per-room assumption lives entirely in `GameManager`. `HasZoneAfterCurrent` (`Systems/RoomManager.cs:203`) already exists.
- **Implementation cost of the mechanism.** `RoomDataSO`, `LevelBuilder.BuildSpawnPoints`, `RoomTrigger`, `RoomGate`, and the `OnRoomActivated`/`OnRoomClearScreenShown` seams are all proven either way. The delta between the two patterns is now almost entirely in `GameManager` and in what has to be resurrected.

---

## Decision

### §1 World 2 is one continuous scene, `Backyard_Dojo.unity`, with three `RoomManager` zones

One scene is World 2, played start to finish: back gate → courtyard → garden → the garden end, with the Grasscutter fought in that same space. No scene load inside a run. Zone activation is `RoomTrigger` → `RoomManager.OnRoomEntered(index)`, exactly as World 1.

**Three zones, not four or five.** Three is what `ShowRoomClearScreenDelayed` routes with no code change (index 0 → upgrade, 1 → shop, 2 → boss), and it matches ROADMAP Phase 3's authorized scope. Zone names are provisional pending §Open Questions 1:

| Idx | Provisional zone | Role | Reward screen |
|---|---|---|---|
| 0 | The Back Gate / Dojo Courtyard | Assembly Beat, gnome pack rhythm | Upgrade |
| 1 | Garden Gauntlet (containing the Koi Pond / engawa sub-space and the Skeptic beat) | Crane Duelist debut, constrained footing, the Skeptic appearance | Shop |
| 2 | The Garden End — Blossom Court | Grasscutter, `bossOwnedWin: true`, empty `spawnPoints` | none (boss owns the win) |

**The Koi Pond is a sub-space of zone 1, not a fourth zone.** That preserves the Skeptic beat, keeps the engawa and the lawn chair resident for the post-boss callback, and needs no `GameManager` change. If the owner wants it as its own combat zone, `ShowRoomClearScreenDelayed`'s index routing must be generalized **first** — small, but a prerequisite, not an afterthought.

### §2 Single-continuous-scene becomes the **default** for team-built worlds

ADR-0002's "one scene per room" corollary is retired as the default, project-wide. `docs/ARCHITECTURE.md` §4.2.1's sentence *"the room-per-scene model in 4.2 above is still the default"* is corrected by this ADR.

Rationale: leaving a non-functional path documented as the default is precisely this project's dominant failure mode — the camera was documented wrong for an entire phase and it changed how work was planned (`.claude/agent-memory/technical-director/project_docs_drift_from_code.md`, `docs/ARCHITECTURE.md` §4.1). A "default" that has no scenes, an empty room pool, and an error log where the scene load should be is worse than no default at all.

**The scene-load path stays alive and is not deleted.** `RandomRoomPool`, `InitRoomQueue`, `s_roomQueue`, `CaptureLoadoutForTransition`, `LoadNextRoom`, and `ZoneStartScene` remain, per ADR-0004 §6. A future world may genuinely need it — a world too large to hold resident, a world with a real interior, or one with a distinct lighting environment per room. The rule is: **it must be re-qualified before use** (a real target scene, a working loadout hand-off across the boundary, and reward routing that does not key off in-scene index), and the world proposing it needs an ADR saying why residency is the wrong trade there.

### §3 Whole-scene performance budgets — the condition this decision is granted on

The budget unit changes from "per room" to **per scene**, because there is now only one scene. These are the numbers World 2 is built against, and B112 makes them evidence-based rather than aspirational. Measured on a representative 3–4-year-old device per TDD §3.7/§3.8 (two passes: Unity Profiler for counts, non-development build + Instruments for the frame-time verdict).

| Budget | Target | Basis / note |
|---|---|---|
| Draw calls, whole yard, peak | **< 100** | TDD §3.2. World 1 measured **205** — World 2 must not repeat this. The dojo kit is many instances of few materials, which is the winnable case |
| Triangles, whole yard, peak | **< 300k** | TDD §3.2. World 1 measured **356.7k** |
| Texture memory, whole scene, steady state | **< 150 MB** | TDD §3.3. World 1 measured **41.2 MB** — real headroom, do not spend it carelessly |
| Distinct ENV materials in the scene | **≤ 20** | New. World 1's SRP Batcher contributed **0** (B112); the dojo's shared-atlas kit is the chance to get instancing/batching actually working. Verify a non-zero SRP Batcher or Instanced draw-call count on device |
| New (non-atlas) ENV geometry for the whole zone | **< 8k tris** | Blueprint §Performance Budget — only the bamboo wall (BD-01) and possibly the pond basin are new |
| Any new ENV texture | **≤ 512** on Android + iOS, with explicit platform overrides | TDD §3.4. The import-policy pass is a **prerequisite** for new dojo art, not follow-up work |
| Scene-start hitch, incl. the runtime NavMesh bake | **≤ 500 ms** | ADR-0004 §8. One bake per run instead of three-to-five is a straight win of this decision |
| Live enemies, peak | **≤ 4** | GDD §8 mixing rules — gnome pack max 4, max 1 Crane Duelist, max 2 risen Leaf Lurkers |
| ~~Combat radius, per zone~~ | ~~**≤ 9 m**~~ | ~~TDD §6.4, derived from the camera's 16.8 m visible width~~ **Restated by [ADR-0006](0006-world2-zone-scale-and-arena-metric.md) §2.1 (2026-09-01) — metric M1.** Still ≤ 9 m, but measured over each contiguous `maxConcurrentEnemies` window of `spawnPoints[]` (the only live set `RoomManager` can produce), counting **closing** enemies only. The whole-roster reading is retired: it describes a state the runtime cannot produce, and World 1's shipped 59.5 m street does not satisfy it |
| ~~Boss arena minimum clear circle~~ | ~~**r ≥ 8.5 m**~~ | ~~World 1's measured 8.44 m holds SpinCycle. See §4 — this one runs the other way round for World 2~~ **Replaced by [ADR-0006](0006-world2-zone-scale-and-arena-metric.md) §2.2 (2026-09-01) — metric M2.** The largest-inscribed-obstacle-free-circle reading is unsatisfiable for any arena with a central feature: it evaluates to `(R − r_t)/2`, demanding a **34.7 m** arena for a 0.70 m trunk. M2 replaces it with outer radius ≥ 10.0 m, **radial fight band ≥ 8.5 m** (which is what World 1's 8.44 m actually measured), interior obstruction ≤ 2% of floor and ≤ 1.0 m wide, and boss traversal ≤ 0.35 × diameter |
| Thermal | No sustained frame-time regression, minute 1 vs minute 12 | TDD §3.1. This is the real acceptance criterion, and it is the one a podcast audience will see |

**Explicitly rejected again: a distance-based prop-streaming or per-zone renderer-activation system.** ADR-0004 §8 rejected it for an open street where everything is visible from everywhere. A walled yard with a shed genuinely occludes, so the argument is weaker here — but the correct first moves are Unity's own occlusion culling, GPU instancing on the shared atlas, a static-batching subroot, and finding out why the SRP Batcher reports zero. Hand-rolled activation gating is more code than the encounter layer it would serve, and it buys pop-in. Revisit only if the budgets above fail *after* those four.

### §4 The boss arena is budgeted before the boss is written, not measured after

ADR-0004 could measure World 1's arena against `SpinCycleAI`'s real authored constants because the boss already existed. `GrasscutterAI` does not exist. The dependency therefore inverts, and the arena must not be retrofitted to whatever the AI turns out to do.

This ADR fixes the contract; `GrasscutterAI` is authored **to** it:

> **Amended by [ADR-0006](0006-world2-zone-scale-and-arena-metric.md), 2026-09-01, after the owner playtested the built Stage A geometry and reported the arena too small.** The arena is now **r = 10.0 m (20.0 m across), centre `(0, 0, 55.0)`**, and the Spin-Dash cap is **≤ 6.5 m**. Two premises below did not survive review: the clear-circle metric is unsatisfiable with a central tree (ADR-0006 §2.2), and *"17 m matches the camera's 16.8 m visible width"* equates a diameter with a follower camera's per-position visible width — at r = 8.5 the boss at the opposite rim was **already** off-frame, so "the arena must not grow" was resting on a limit already passed (ADR-0006 Fact 4). Rows below are superseded where struck through; everything not struck through still binds.

| Constraint | Value | Why |
|---|---|---|
| ~~Boss arena minimum clear circle~~ | ~~**r ≥ 8.5 m** (~17 m across)~~ | ~~Matches the camera's 16.8 m visible lateral width and World 1's measured, playtested 8.44 m~~ → **ADR-0006 §1: r = 10.0 m, 20.0 m across, metric M2** |
| ~~Phase-2 Spin-Dash travel~~ | ~~**≤ 8 m**~~ | ~~Must start and end inside the arena. SpinCycle's charge is 4.8 m; 8 m is generous~~ → **ADR-0006 §1.2: ≤ 6.5 m.** The 8 m figure was undefended slack. At 8 m in a 17 m court the dash erases 47% of the arena per commitment, against World 1's playtested 28% — this, not the arena's metres, is the likeliest cause of the "too small" report. `GrasscutterAI` does not exist, so the cut is free |
| Phase-1 AoE / Petal Toss reach | **≤ 4 m** | SpinCycle's `fullSpinRadius` is 3 m |
| Dash landing point | **must be `NavMesh.SamplePosition`-clamped before the move commits** | Not optional. `SpinCycleAI`'s `JumpBack`/`SpinCharge`/`JumpCharge` disable the agent and move by raw `transform.position` with no bounds check, which can put the boss ~3 m inside a building (ADR-0004 §2). Shipping the same hole twice is not acceptable |
| Cut-Grass Trail hazards | pooled, zero per-frame allocation | Blueprint §Unity Notes; TDD §3.2 steady-state GC budget |

**Correction of record:** the blueprint's *"~36 m circular sparring court"* (`unity-blueprint.md` §Boss Room) predates ADR-0001 and violates TDD §6.4's ≤ 9 m combat radius by a factor of two. Over half that fight would happen off screen. The Blossom Court is ~17–20 m across with the cherry tree at centre, not 36 m. **Settled by [ADR-0006](0006-world2-zone-scale-and-arena-metric.md) §1 at the top of that range: 20.0 m across, r = 10.0 m, tree still at centre.** The rejection of 36 m stands.

**Blocking prerequisite: the large-agent NavMesh problem, now on its second boss.** The project bakes exactly **one** NavMesh agent type (radius 0.5, height 2, climb 0.75). `pfb_enemy_spincycle` is radius 1.0 / height 4 and `pfb_enemy_wagonwheel_roller` is radius 0.95 — both pathing on a mesh carved for an agent half their width, so any 1–2 m gap reads as walkable and is not (ADR-0004 §8). The Grasscutter is a drum-chested boss on iron-sandal wheels and will land in the same class. **Decide before authoring it:** add a second agent type for large agents, or cap boss agent radius at 0.5 and let the collider carry the bulk. The second is cheaper and almost certainly correct. This is a prerequisite for the Grasscutter, not a backlog item to carry.

### §5 `WildWestCityZoneDirector` is generalized to `ZoneDirector` and reused — not copied

Read the script (`Systems/WildWestCityZoneDirector.cs`): it has four serialized fields — `_clearOnBossZone`, `_boss`, `_gateByZone`, `_bossZoneIndex` — and **nothing in it is street-specific except the comments and the class name.** ADR-0004 §4 was right to keep scene composition out of `RoomManager`, and right that "which props clear, which GameObject is the boss" are scene facts; what it got wrong is that those facts are already expressed as *data*, so the component is generic and only its name is not.

Decision: **rename the class and its file to `ZoneDirector`, behaviour-identical, and re-point the World 1 scene at it.** Rename file and class together so the `.cs.meta` GUID is preserved and `CulDeSac_WildWestCity.unity`'s component reference survives; keep every existing guard and diagnostic (the `Awake` forced-inactive boss, `OnEnable`-not-`Start` subscription, the `GameManager.OnRoomClearScreenShown`-not-`RoomManager.OnRoomCleared` gate timing for H4, and all five loud-failure logs — those encode five review passes of hard-won knowledge and none of them are optional). Then World 2 adds an *instance*, not a class.

Because this touches a shipped, five-times-reviewed scene, the rename is only done with a World 1 zone walkthrough re-verified afterwards (§Validation 1).

World 2's genuinely unique staging — the 3-second Assembly Beat, the Skeptic's scripted arrival and departure, the koi-pond water-slow trigger, the cherry tree's full-bloom on Imagination Restore — goes in **one** scene-local script (`BackyardDojoBeats` or similar) subscribing to the same seams. That is the ADR-0004 §4 pattern applied correctly: reusable mechanism as a shared component, one-off staging as a scene script.

Rejected: authoring a second `BackyardDojoZoneDirector` by copy. This codebase already carries two HUD controllers, two forge UIs, two ability systems, two enemy-spawn systems, and two boss-intro implementations (`docs/ARCHITECTURE.md` §5). Adding a sixth duplicated pair to save a rename would be indefensible.

### §6 What World 2 must not repeat from World 1

Every item below cost World 1 a review pass or a geometry revision. All are free to get right at authoring time and expensive to retrofit.

1. ~~**Author the ENV root at yaw 0.**~~ **Superseded by explicit owner direction, 2026-09-01.** World 1's `[ENV - Static]` is rotated **45°**, so every design coordinate needs a local↔world transform, `Renderer.bounds` inflates footprints badly, and raw world X/Z say nothing about the level (ADR-0004 §0; `.claude/agent-memory/technical-director/reference_measuring_city_scene.md`). This ADR originally built the yard axis-aligned per that lesson — but the owner, looking at the built level live, asked for the same 45° diagonal composition World 1 uses, for visual consistency between the two worlds. `[ENV - Static]` is now rotated 45° in `Backyard_Dojo.unity`, `LevelBuilder._cameraYawDegrees` stays **0** (camera intentionally NOT co-rotated — rotating both together is a no-op on screen, since it just relabels which compass direction is "forward"; only rotating the geometry alone produces the visible skew the owner wanted). The predicted tax **did materialize exactly as warned**: `RoomGate`/`RoomTrigger` (scene-root siblings, not children of the rotated hierarchy), `RoomData_Backyard_Dojo_Zone0`'s spawn points, and `WeaponDropTableSO_Backyard_Dojo`'s cardboard-pile/workbench coordinates all needed re-deriving by hand after the rotation, since none of that data lives under `[ENV - Static]` and a parent rotation doesn't touch it. Full fix record: `.claude/agent-memory/unity-gameplay-engineer/project_backyard_dojo_build.md` and the project-state memory's item 19. **Standing note for World 3+:** this is a real, recurring cost of the diagonal-composition choice, not a one-time World 1 mistake — budget for it explicitly if a future team-built world also wants this look, rather than treating axis-aligned authoring as a solved problem.
2. **Prop colliders must match their visual mesh footprints.** World 1's buildings carry single `BoxCollider`s far wider than their meshes (collision corridor X ±12.9 against a visual corridor X −8.8…+10), so the player walks into porches and every clearance number had to be computed against meshes instead (ADR-0004 §0). Author per-prop colliders to the silhouette.
3. **The perimeter is sealed and physically continuous from day one.** Bamboo stockade walls with real colliders, closing the ring. Prove it with the physics flood-fill methodology B99 established (`Physics.OverlapCapsule` at the player's real `CharacterController` radius on a 0.5 m grid, BFS from spawn, **plus a downward ground-support raycast per cell** — B107 found the flood-fill otherwise routes through off-mesh void and produces false positives), run as a three-configuration control: gates closed with boundaries off (must reproduce a bypass), gates closed with boundaries on (must not), gates open (must, as a positive control that the test detects connectivity at all).
4. **Derive each `RoomTrigger`'s width and its paired `RoomGate`'s width from one authored value.** They drifted apart in World 1 (22 vs 26) and a `RoomTrigger_Zone2` bypass is a silent, permanently-unwinnable run with zero diagnostic output (`docs/BACKLOG.md` B108).
5. **Any prop or enemy deactivated at runtime needs a carving `NavMeshObstacle` + an ignore-from-build `NavMeshModifier`.** `LevelBuilder.BuildNavMeshDeferred` calls `NavMesh.RemoveAllNavMeshData()` and re-bakes from **physics colliders** at `Start()`, so a prop deactivated later leaves a permanent hole. World 2 has more of this pattern than World 1 did, not less: the Grasscutter's tall grass, and especially the **Leaf Pile Lurkers**, which are authored as dormant props that rise.
6. **One `WeaponDropTableSO` per scene, and the whole table spawns at `Start()`.** `LevelBuilder` holds exactly one `_dropTable`. Merge World 2's loot into a single `WeaponDropTableSO_Backyard_Dojo`, leave `envProps` empty (the yard is hand-dressed), and place zone-2 loot so it is ≥ 1.5 m clear **with the boss zone's pre-boss props still present**.
7. **The walled perimeter is the primary boundary; `PlayerController._arenaBoundaryRadius` is a backstop.** World 1 re-derived its single circular clamp twice because an open street had no real edges. Size the clamp generously around the yard once, and let the stockade do the work.
8. **The "no room reuses another room's layout" rule is NOT suspended for World 2.** ADR-0004 suspended it for the city scene only, on owner direction, because a street is repetitive by nature. A yard's zones are naturally distinct — a gravel garden, a pond veranda, an open blossom court — and the blueprint already provides three genuinely different shapes. The standing rule (owner memory: *"No room may reuse another room's shape/prop layout, ever"*) applies here in full.

### §7 What stays exactly as it is

`RoomManager` is not modified. `RoomDataSO`, `LevelBuilder`, `RoomTrigger`, `RoomGate`, the `OnRoomActivated` / `OnRoomClearScreenShown` seams, `HasZoneAfterCurrent`, and `GameManager.OnUpgradePicked`'s guard all work as shipped and need no change for a three-zone World 2. New assets only: `Backyard_Dojo.unity`, three `RoomDataSO`s, one `WeaponDropTableSO`, and one scene-beats script. `GameManager` additions are two dictionary entries (`ZoneIndexByScene["Backyard_Dojo"] = 1`, `ZoneStartScene[1] = "Backyard_Dojo"`, replacing the never-existent `TownSquare_Room1`) plus a build-settings entry.

That `ZoneStartScene[1]` correction incidentally closes `docs/ARCHITECTURE.md` §5 divergence 11 and the long-standing unreachable-zone-1 note in `docs/BACKLOG.md` line 167.

---

## Alternatives considered

**1. Scene-per-room per ADR-0002 — the documented default.** The main alternative, and the one the owner deliberately left open. Rejected on six grounds, in descending order of weight: (a) the path is currently dead with no target scenes, an empty room pool, and an error log where the load should be, so choosing it means new work and new risk rather than the safe option; (b) the post-boss engawa callback — *"I look back at the engawa. The chair is still there"* — is impossible once the Koi Pond scene is unloaded; (c) the shop screen is unreachable when every `RoomManager` has one room at index 0; (d) a backyard is one contiguous space in the fiction, so scene seams would be invented rather than found; (e) three-to-five synchronous scene loads per run, each carrying a full runtime NavMesh re-bake, against a 500 ms hitch budget and with no loading-screen UX in the project; (f) it would make the two reference worlds teach two patterns, one broken, to an audience expected to build World 3.

**2. Keep ADR-0002 as the default and treat World 2 as a second one-off deviation.** Tempting as the minimum-claims option, and it would produce the same `Backyard_Dojo.unity`. Rejected because after two worlds it is no longer a deviation, and a documented default with zero shipping users is exactly the doc-drift failure this project has already paid for once. Two worlds out of two is the pattern; say so.

**3. Hybrid — one continuous scene for zones 0–1, a separate `Backyard_BossCourt` scene.** Superficially attractive: it caps residency at the moment the boss's VFX and hazard pools arrive. Rejected — it reintroduces the entire dead scene-load path for a single transition, still breaks the engawa callback (the pond unloads before the boss), still loses the shop routing, and puts a loading hitch immediately before the zone's emotional peak.

**4. Additive scene loading, one additive scene per zone.** Rejected for ADR-0004 §Alternatives-1's reasons, which all still hold: coordinate conflicts between overlapping zone scenes, duplicated managers, a lighting and NavMesh seam at every boundary, and an async hitch in a scene small enough not to need one — plus there is no loading UX (`LoadingScreen.unity` does not exist and was removed from build settings).

**5. Single scene with five zones, matching the v4 GDD (Room 1 + three random + boss).** Rejected: it breaks `ShowRoomClearScreenDelayed`'s routing, the random draw is retired for linear worlds, a canon Skeptic beat firing in one run out of three undermines the character's designed slow-build recognition, and five walled zones resident at once is precisely where the draw-call budget fails. The v4 GDD/blueprint predate ADR-0001, ADR-0002, and ADR-0004 and are superseded on structure — see §Open Questions 1.

**6. Build a proper streaming / world-partition level system now, so World 3+ scales.** The generalizing move. Rejected as premature by a wide margin: two team-built worlds, a two-person non-expert team, measured texture memory at 27% of budget, and a stated project value of legibility over cleverness (TDD §1). It would also be substantially more code than the encounter layer it serves.

**7. Copy `WildWestCityZoneDirector` to `BackyardDojoZoneDirector`.** Rejected — see §5. The component is already fully data-driven; only its name is scene-specific.

---

## Consequences

### Positive

- One working level-architecture pattern in the project, with a shipped, playtested, five-times-reviewed reference implementation, pointed at the audience who will build World 3.
- The Koi Pond and the post-boss engawa callback are shippable as written, and the Skeptic beat is guaranteed rather than 1-in-3.
- Zero `GameManager` change beyond two dictionary entries. `RoomManager` untouched. The shop screen works.
- One runtime NavMesh bake per run instead of three-to-five, and no mid-run loading hitch.
- `ZoneStartScene[1]` finally points at a scene that exists, closing ARCHITECTURE §5 divergence 11.
- The zone director becomes a reusable component instead of the project's sixth duplicated pair.
- World 1's eight authoring lessons (§6) are paid forward instead of re-learned. **Correction (2026-09-01): lesson 1 (yaw-0 ENV root) was reversed by owner direction after this ADR was accepted** — see §6 item 1's strikethrough. World 2 ended up paying the exact coordinate-transform tax this lesson was meant to avoid, as a deliberate trade for visual consistency with World 1, not as a repeated mistake.

### Negative / risks

- **Whole-yard draw calls and triangles are the live risk.** World 1 measured 205 draw calls and 356.7k triangles against < 100 / < 300k. World 2 has a structurally cheaper prop set, but "cheaper" is a hypothesis until profiled. §3 is a condition, not a hope, and it must be measured *during* construction rather than after the yard is dressed.
- **The SRP Batcher measured at zero on World 1** and nobody yet knows why. If the cause is project-wide (shader/material variant incompatibility) it will hit World 2 identically, and the dojo's shared-atlas kit — the main reason to expect a better draw-call outcome — depends on batching or instancing actually engaging.
- **The boss arena is budgeted before the boss exists**, which inverts World 1's much safer measure-then-place order. If `GrasscutterAI` cannot be authored inside §4's envelope, the arena has to grow, and at ~17 m across it is already at the camera's visible width. Surface that early, not during boss tuning. **This risk fired within 24 hours (2026-09-01):** the owner playtested the built geometry and reported the arena too small, before `GrasscutterAI` existed at all. Resolved by [ADR-0006](0006-world2-zone-scale-and-arena-metric.md) — arena to r = 10.0 m, dash cut to ≤ 6.5 m, and the "already at the camera's visible width" claim retired as a false limit (ADR-0006 Fact 4). **Lesson for World 3+: budgeting an arena before its boss is workable, but "the camera's visible width" is not the constraint it looks like for a follower camera, and a boss's traversal-to-diameter ratio is the number that predicts how an arena feels.**
- **The Training Hall (blueprint Room B) does not fit this decision, or the camera.** It is a roofed interior; ADR-0001's ≥ 6 m overhead clearance makes a roofed room unbuildable under the current rig regardless of scene architecture, and a single continuous outdoor scene also cannot give it its own lighting environment. ROADMAP Phase 3's three-room scope already drops it. If it returns, it must be a roofless or open-sided pavilion.
- **`GameManager` still carries two progression paths** — in-scene and scene-load. Now that both worlds are single-scene, the second has zero users. It stays (§2), but it is unexercised code that will rot; anyone reviving it should expect to re-qualify it from scratch.
- **Three zones is a ceiling until the reward routing is generalized.** A fourth zone needs `ShowRoomClearScreenDelayed` changed first.
- **One scene means one lighting environment** for the whole yard. The GDD's cool overcast key is uniform, so this costs nothing today — but the interior/exterior contrast the blueprint wanted for the Training Hall is not available.
- **Renaming the zone director touches a shipped scene.** Low risk, non-zero; gated on §Validation 1.

### Out of scope / explicitly deferred

- Zone geometry, dimensions, prop placement, spawn coordinates, and encounter composition. This ADR sets the pattern and the budgets; the layout is the next task and must be built to the blueprint plus §6, not invented (owner memory: *build to spec, don't invent layouts*).
- `GrasscutterAI` and `CraneDuelistAI` implementation, and the Grasscutter boss intro.
- The Leaf Pile Lurker prefab, the bamboo wall (BD-01), and the pond basin decision (`street_pond_a` vs new) — all asset-pipeline work, and all gated on the texture import policy (§3).
- The SRP-Batcher-at-zero investigation (B112) and the `StaticBatchingUtility` question — performance-engineer work, but a dependency of §3.
- The 30 FPS cap at `GameManager.cs:101-102`, which contradicts TDD §3.1's 60 FPS target — owner decision, B112.
- Whether the `RoomDataSO` set should eventually carry zone bounds so `RoomTrigger`/`RoomGate` placement is derived rather than hand-placed. Attractive after two worlds of hand-placement; not now.

---

## Open questions for the owner

None of these block scaffolding the scene. All are content or tuning decisions.

1. ~~**Which room structure is canon?**~~ **Resolved by the owner, 2026-08-31: three rooms, ROADMAP's scope wins.** The Koi Pond is a sub-space of zone 1 (Garden Gauntlet), not a fourth/fifth zone. `docs/v4/levels/World2/backyard-dojo/gdd.md`/`unity-blueprint.md`'s five-room random-draw structure is superseded; that doc should be corrected or marked historical the next time someone is in it, but doing so is not blocking.
2. ~~**Does the Koi Pond need to be its own combat zone?**~~ **Resolved by Q1's answer: no.** `ShowRoomClearScreenDelayed`'s index routing does not need to be generalized.
3. ~~**Large-agent NavMesh:** second agent type, or cap boss agent radius at 0.5?~~ **Resolved by the owner, 2026-08-31, revised same day: match Grasscutter's `NavMeshAgent.radius` to SpinCycle's (1), not the baked 0.5.** First call was to cap at 0.5; owner reconsidered and chose boss-to-boss consistency instead — SpinCycle already ships at radius `1` with no reported pathing symptom, so Grasscutter gets the same value rather than being the odd one out at a different radius. No second baked NavMesh agent type; both bosses' physical `Collider` carries the size, same as today. WagonWheelRoller (radius `0.95`) is unrelated to this call — it's not a boss. Tracked as `docs/BACKLOG.md` B114.
4. **The 30 FPS cap** (B112) — raise to 60 and re-test, or correct TDD §3.1's documented target? Affects how §3's thermal criterion is judged.
5. **Water Whip** ships with World 2 or is dropped from the dojo weapon pool until art lands? (v4 GDD §12 Q4, still open.)
6. **Carried forward, still unresolved:** `docs/TECHNICAL_DECISIONS.md` lists ADR-0001/0002/0003 as **Proposed** while ADR-0002's own header says **Accepted** and Sprint 0 shipped against 0001/0003. This ADR supersedes part of ADR-0002 and would prefer to know it is superseding an accepted decision.
7. **Zone scale, resolved by [ADR-0006](0006-world2-zone-scale-and-arena-metric.md) 2026-09-01 after live playtest:** zone 1 → **20.0 m × 28.0 m** (from 16.5 × 22.0), zone 2 → **r = 10.0 m / 20.0 m across** (from 8.5 / 17.0), zone 0 unchanged. Both of this ADR's §3 acceptance metrics were found to be measuring states the runtime cannot produce and were restated. Implementation is `docs/BACKLOG.md` B116; ADR-0006 §1 Open Question 1 asks the owner to confirm the arena's feel by playing it rather than re-deriving it.

---

## Validation before World 2's scene architecture is called done

1. **The `ZoneDirector` rename does not regress World 1.** Full `CulDeSac_WildWestCity` walkthrough: each zone activates exactly once in order, wagons clear before the boss activates, both gates open on their zone clear, SpinCycle's intro plays and `DefeatSequence` triggers the win exactly once. Console clean.
2. **Walk `Backyard_Dojo` start to finish.** Each zone activates once, in order; no zone is reachable past a closed gate; re-entering a cleared zone triggers nothing; zone 0's upgrade screen and zone 1's shop screen both appear and return control **without a scene load**, with the player resuming where they stood.
3. **Gate-bypass flood-fill**, three-configuration control with the ground-support raycast, per §6.3.
4. **Every spawn point, workbench, pickup, and cardboard pile is on the runtime NavMesh and reachable** — `NavMesh.CalculatePath` returns `PathComplete` from each zone entry, checked with each zone's gate in its real closed state (World 1's B100 found two spawns inside a closed gate's carve volume).
5. **`LevelBuilder.ValidateCameraClearance` run with `_cameraYawDegrees = 0`**, vertex counts recorded, violations bucketed per zone as B107 did.
6. **The engawa callback works:** after the Grasscutter falls, the lawn chair is still on the boards and is visible from the victory-beat camera.
7. **Profile a full run on a representative 3–4-year-old device against §3**, both passes (Unity Profiler for counts, non-development build for the frame-time and thermal verdict), recording frame time at minute 1 versus minute 12 and the scene-start hitch including the NavMesh bake. Report the SRP Batcher / Instanced draw-call split explicitly. A figure without its scenario is not evidence.
