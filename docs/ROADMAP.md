# BoxForged Roadmap

**Status:** In production. Discovery locked 2026-08-18. Production authorized 2026-08-19 ("Start Sprint 0"). Sprint 0 (camera, telegraph, forge) complete, committed, and pushed on `feature/sprint-0-foundation-rebuild`. Phase 2 (World 1) **replanned 2026-08-25**: World 1 is now one continuous scene, `CulDeSac_WildWestCity.unity`, rather than separate room scenes. City geometry and core scaffolding are built; the zone architecture is specified in **[ADR-0004](adr/0004-world1-single-continuous-scene.md)**, **accepted 2026-08-26**. **Zone wiring is implemented and has been through four `code-reviewer` fix passes plus an owner-reported geometry revision (B107)** (all 2026-08-26) — combat/`RoomManager`/`LevelBuilder` wiring is in the scene and Play-Mode-verified; not yet committed, pending owner approval.

This roadmap reflects the podcast production model: BoxForged is being built live to demonstrate AI-assisted game development by two non-professional-developer creators. Scope is deliberately small for the team-built portion — the audience builds everything beyond it.

---

## Scope Model

| Layer | Who builds it | What it covers |
|---|---|---|
| **Core systems** | Team (owner + AI studio) | Combat, camera, forge, progression, save, UI, audio — the reusable foundation everything else depends on |
| **World 1 — The Cul-de-Sac (Western)** | Team | **One continuous scene** (`CulDeSac_WildWestCity.unity`) played start to finish, zoned by `RoomManager` — not separate room scenes. Replanned 2026-08-25, see [ADR-0004](adr/0004-world1-single-continuous-scene.md). |
| **World 2 — The Backyard (Dojo)** | Team | New build. Grasscutter boss. |
| **World 3+** | Audience (podcast contribution) | Framework and tools must exist so non-experts can extend the game; team does not pre-build these worlds |

The team's job in pre-production and production is to make Worlds 1 and 2 excellent, and to make sure the systems underneath them (LevelBuilder, forge, combat, camera) are solid enough that an audience-built World 3 doesn't require re-architecting anything.

---

## Phase Sequence

### Phase 0 — Pre-production (current phase)
- Convert locked creative decisions into GDD — **done (GDD v1.2)**
- Technical Design Document, Architecture reference, ADRs for camera + scene rebuild — in progress (technical-director)
- This roadmap, backlog, Sprint 0 — in progress
- Stop at pre-production approval gate. Owner explicitly authorizes production before implementation begins.

### Phase 1 — Foundation Rebuild (Production, pending authorization)
The two most urgent production tasks identified in discovery, in order:

1. **Camera overhaul** — replace the fixed top-down camera with the new fixed-follow, lower-angle, no-rotation camera. This changes how every subsequent room needs to be built, so it comes first.
2. **Forge transformation feel** — the core imagination mechanic (household object + cardboard → weapon) currently has no visual, audio, or narrative payoff. This is the single most important missing feeling in the game and the thing a podcast audience will judge the game by on first look.

Both are scoped in `docs/TECHNICAL_DESIGN.md` (technical-director, in progress).

### Phase 2 — World 1 Rebuild (Cul-de-Sac) — **replanned 2026-08-25 as a single continuous scene**

**Owner directive (2026-08-25):** World 1 is no longer a set of room scenes connected by scene loads. It is **one continuous scene, `CulDeSac_WildWestCity.unity`, that IS World 1** — a single linear walk down one street, start to finish, with the SpinCycle boss fight at the north end of that same scene. The random room-order draw is retired for this world. This intentionally suspends the project's "no room reuses another room's layout" rule **for this scene only**; it still applies everywhere else.

**ADR-0004 was accepted 2026-08-26** once the owner resolved every question it had left open (three zones, mixed roster in zones 0–1, boss-only zone 2, no ground expansion). It is now the implementation spec. **Implementation is complete** — the three `RoomDataSO`s and merged drop table, `LevelBuilder`/`RoomManager`/triggers/gates, the zone-director script, and the `GameManager` change are all built, went through five `code-reviewer` rounds (each found and fixed a real bug — see `docs/BACKLOG.md` B78–B108), were playtested for real by the owner (which surfaced and got fixed a separate `SpinCycleAI` win-condition bug, B103), and are **committed** as `b80953ca` on `feature/sprint-0-foundation-rebuild`. **Not yet pushed** — the commit carries ~57 MB of new LFS binaries and the owner wants the GitHub LFS cap confirmed first.

**Design of record:** [ADR-0004](adr/0004-world1-single-continuous-scene.md) — coordinate convention, zone layout, measured boss-arena analysis, encounter spec, wagon-clear mechanic, `GameManager` fix, performance position, alternatives, and remaining questions.

#### Status

- **City scene geometry — done, but revised once since the first build.** `CulDeSac_WildWestCity.unity`: 10 buildings in two facing rows, 34 street props, ground plane **40 × 59.5 m** (lengthened from 40 × 47.5 m in the second geometry revision below), warm directional sun. Hand-tuned by the owner over several iterations. **Not to be casually redesigned further** — ADR-0004 changes none of the original layout, but the owner's own 2026-08-26 playtest triggered a second, documented revision (see below).
- **Scene rotated.** `[ENV - Static]` is at **yaw 45°**. All design coordinates are given in the street-local frame with the world transform stated — see ADR-0004 §0 before authoring anything.
- **Core scaffolding — done** (B86–B90): `GameManager`, `AudioManager`, `ProgressionSystem`, `SaveSystem`, `AttackTelegraphService`, HUD, minimap camera, and the player's arena clamp fix (B87, re-derived after the second geometry revision: radius **24.5** on centre world `(−24.22, 0, −24.22)`, street-local `(0, −34.25)` — up from the original radius 23).
- **Zone wiring — implemented.** `LevelBuilder` + `RoomManager` wired with the three `RoomDataSO`s and the merged drop table; two `RoomTrigger`/`RoomGate` pairs; `WildWestCityZoneDirector` (wagon clear, gate open, boss activation); `pfb_enemy_spincycle` pre-placed inactive; `ImaginationRestore_Volume`/`PostProcess_Volume` copied from `CulDeSac_BossArena`; scene registered in `EditorBuildSettings.asset` (index 11) and `GameManager.ZoneIndexByScene`.
- **Zone architecture — accepted, ADR-0004.**
- **Second geometry revision, 2026-08-26 (owner-reported, B107).** The owner played a full run and found zone 1 ("Ambush Alley") only 8 m deep — its two forge workbenches sat ~8 m apart, barely separated, with no room for the zone's 7-enemy encounter. Owner direction: *"Buildings can move, do what it takes."* Fix: the entire north building cluster (`bank`, `shedwithcrate`, `sheriffs_office`, `blacksmith_forge`, `twostoryhouse`, `saloon_facade`, both covered wagons, 18 adjacent street props, `RoomGate_Zone1`, `RoomTrigger_Zone2`) rigidly translated +12 m along street-local Z; `Ground` lengthened to match (47.5 m → 59.5 m, south/spawn edge unchanged). Zone 1 depth 8 m → 20 m; workbench separation 8.0 m → 17.9 m. Player arena clamp and the boss-arena floor were both re-derived and empirically re-verified against the new layout, not just translated on paper (ADR-0004 §0/§2). Full detail in `docs/BACKLOG.md` B107 and ADR-0004 §0/§1.
- **Four `code-reviewer` fix passes complete, 2026-08-26** (the first two predate the second geometry revision above; all four have been re-verified as still holding against the current layout). First pass: missing upgrade/shop screens (B1), stale NavMesh hole left by closed-but-uncarved gates (B2), camera-yaw doc drift (H3), a ~1.5 s exploit window between gate-open and the reward screen freezing the player (H4), silent zone-director failures now logged (H5), and gates widened from 20 m to 26 m to seal flush with the building colliders (M9). Second pass: the 26 m gates still left an open flank between the outermost building colliders and the ground's true edge that let the player walk around the entire building row, gates included — closed with two full-length invisible `StreetBoundary_West`/`StreetBoundary_East` `BoxCollider`s, currently at street-local **X ±16…20** (widened once more from an initial ±17…20 after a third fix pass found two buildings' outer faces fell short of that line — see `docs/BACKLOG.md` B99's correction-of-record note); and two of zone 1's seven spawn points sat inside gate 1's `NavMeshObstacle` carve volume while the gate is closed for the whole zone-1 fight — both nudged south, re-verified against the live NavMesh. Third pass: the two safe-pockets left by the ±17…20 wall placement, closed as above. Fourth pass: both `RoomTrigger`s (`size.x = 22`) were narrower than their paired `RoomGate`s (`size.x = 26`) — widened to match exactly, removing a silent, permanent zone-2/boss-activation failure class regardless of future building placement. All fixes verified with real physics/NavMesh queries in Play Mode, not static inspection alone.

#### Zone layout (ADR-0004 §1) — street-local Z

| Idx | Zone | Local Z span | Depth | Encounter | Activation |
|---|---|---|---|---|---|
| 0 | The Arrival | −59.45 → −44 | 15.5 m | 5 enemies (2 Roller, 2 Gnome, 1 Skeptic), max 3 concurrent | Auto, on NavMesh ready |
| 1 | Ambush Alley | −44 → −24.5 | 20.0 m (widened from 8 m — second geometry revision, B107) | 7 enemies (2 Skeptic, 3 Gnome, 2 Roller), max 4 concurrent | `RoomTrigger` |
| 2 | The Showdown Circle | −24.5 → +0.05 | 24.55 m (unchanged; the zone shifted north with the rest of the cluster, its own depth didn't change) | SpinCycle only, `bossOwnedWin` | `RoomTrigger` |

Boundaries sit at the two lines where both building rows have a wall. `RoomGate` barricades divide the zones and open on clear. Three zones maps onto `GameManager`'s existing upgrade-screen/shop-screen routing with **no change to it**.

Zones 0 and 1 are deliberately larger encounters than the rooms they replace — that is how the drop from five encounters to three is absorbed, without a fourth zone that would break the screen routing.

#### The boss arena — measured, no geometry change (ADR-0004 §2)

The boss fight uses the **existing** street footprint. Clearing the two covered-wagon props as part of the SpinCycle intro beat opens the largest circle in the street from **r 5.46 m to r 8.39 m** (16.8 m across). The arena's location moved north with the rest of the north cluster in the second geometry revision — currently street-local **(0.80, −18.90)** (world `(−12.80, 0, −13.93)`), up from the original `(0.80, −30.90)`. Re-measured empirically post-shift (not just translated on paper, since one nearby prop did not move with the group): minimum clear radius **8.44 m**, essentially unchanged from the original 8.39 m target. That holds SpinCycle's 4.8 m spin charge, 4 m jump-back, and 3 m spin AoE.

The consequence is that **the fight sits mid-street, not at the far north end** — that stretch measures only r ≈ 4.9 m because the saloon facade stands in it. The saloon instead becomes the door the boss walks out of, which `SpinCycleAI`'s runtime saloon lookup already expects. No ground rescale, no saloon move.

#### Known prerequisites (ADR-0004 §Required changes) — all implemented

Four blockers, all done: scene registered in build settings and in `ZoneIndexByScene`/`ZoneStartScene[0]`, the two-line guard added to `GameManager.OnUpgradePicked` so it resumes play in place instead of loading a scene, and `LevelBuilder` + `RoomManager` added with all serialized dependencies wired (`_spawnPointMarkerPrefab` included).

The two changes that fail **silently** if missed are both done: the covered wagons carry a carving `NavMeshObstacle` plus ignore-from-build `NavMeshModifier` (deactivating them heals the NavMesh instantly, verified live), and `SpinCycleAI._introWalkTarget_X/_Z` is retargeted to this scene.

#### Consequences to note

- **Run length drops from five encounters to three**, mitigated by larger zone-0/zone-1 encounters rather than a fourth zone.
- **Texture residency roughly triples** — all 10 buildings resident for the whole run rather than 3–4 per scene. This makes the texture import-policy work in `docs/TECHNICAL_DESIGN.md` §3.4 a prerequisite for shipping this scene rather than deferred debt. See ADR-0004 §8.
- **Three pre-existing defects this scene exposes**, all logged and out of scope for the zone work: building colliders are far wider than their meshes (the player can walk into porches); the boss and wagon-wheel-roller `NavMeshAgent` radii (1.0 / 0.95) exceed the project's single baked agent type (0.5); and `SpinCycleAI`'s jump-back has no NavMesh clamp on its landing point.
- Asset defects **B78–B85** (baked building rotations, broken `western_house_tall` mesh, ~1 cm desert-scatter props, stale duplicate prefabs, inconsistent building scale) remain open and are explicitly out of scope.
- **Both ADR-0004 open questions are resolved.** The camera was nudged from yaw 35° to yaw 45° to match the street (`LevelBuilder._cameraYawDegrees = 45`); the `RoomGate` barricades are invisible colliders only, per explicit owner decision — no barricade prop. One doc-only item remains open: `docs/TECHNICAL_DECISIONS.md` still lists ADR-0001/0002/0003 as Proposed despite Sprint 0 having shipped against them.

#### Retired from this phase

- Per-room scenes for Saloon Front, Mailbox Row, and Town Square — **no longer planned as separate scenes.**
- `CulDeSac_BossArena` as a separate boss scene — the boss moves into the city's north plaza.
- The random room-order draw for World 1 (`GameManager.RandomRoomPool` / `InitRoomQueue`) — inert for this world; the code path stays alive for World 2.

#### Still in this phase

- Boss intro cutscene for SpinCycle (boss intros only, per locked cutscene scope). Its trigger moves from scene-entry to zone-entry — see ADR-0004 §Required changes #8.

#### Old scenes

`CulDeSac_Room1`, `CulDeSac_Room1_v2`, `CulDeSac_AmbushAlley`, `CulDeSac_AmbushAlley_v2`, `CulDeSac_SaloonFront`, `CulDeSac_MailboxRow`, and `CulDeSac_BossArena` are **kept on disk as reference, not deleted, not edited.** `Room1_v2` and `AmbushAlley_v2` hold the encounter tuning being migrated and must stay verifiable against it. Deletion and build-settings pruning remain separate, explicit owner decisions (ADR-0002 §5, ADR-0004 §7).

### Phase 3 — World 2 Build (Backyard/Dojo)
- Room 1 (Dojo Courtyard), Room 2 (Garden Gauntlet), Room 3 (The Garden End — Grasscutter boss)
- Grasscutter boss AI (currently has full lore, no implementation)
- Crane Duelist enemy (locked as World 2 enemy, no implementation)
- Boss intro cutscene for Grasscutter

### Phase 4 — Podcast Launch Readiness
- Both worlds playable end-to-end
- Release-readiness checklist run
- Audience contribution framework documented (how someone extends a World 3 without professional Unity experience)

### Phase 5+ — Audience-Driven Expansion (Post-launch)
- World 3+ per podcast/audience direction
- Co-op (Cowgirl, Female Ninja) — requires the reunion-zone story beat to be resolved first
- Meta-progression (Spark), monetization integration
- HKW real-world skill unlock system (Phase 3 per GDD Section 10.1)

---

## Explicitly Not Yet Scheduled

- Elder BoxHead / World Tree scene — needed only when a late-game zone is actually built
- Versus mode, Cardboard Mill boss — no owner priority yet
- Full grey-world Imagination Overlay (beyond the forge transformation) — good future podcast episode, not urgent for Worlds 1–2 launch
