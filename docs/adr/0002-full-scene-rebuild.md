# ADR-0002: Full scene rebuild on extracted room data

- **Status:** **Accepted, extraction requirement withdrawn 2026-08-19.** Owner authorized production 2026-08-19 ("Start Sprint 0"). On the same date, owner decided (D4, `docs/BACKLOG.md`) to **discard** existing room encounter data rather than extract it — the current rooms were built while figuring out process/systems, not intended as content worth preserving. The extraction requirement below (§Decision, step 1) is therefore withdrawn. The `RoomDataSO` *pattern* — data-driven room authoring so a future contributor builds a room as a data asset rather than wiring scene references — remains good architecture and should still be built when World 1/2 rooms are actually designed; it's just no longer a transcription task against the current scenes.
- **Date:** 2026-08-19
- **Related:** [ADR-0001](0001-fixed-low-follow-camera.md) (camera), [ADR-0003](0003-attack-telegraph-channel.md)

## Context

The owner has directed that no existing Unity scene be reused. All room layouts are to be rebuilt from scratch under the new camera (ADR-0001) and current creative canon. Existing scenes stay on disk as reference until new scenes are verified; deletion is a separate, explicit decision.

Room content and layout are treated as fully open. The `LevelBuilder` + ScriptableObject level architecture recorded in `docs/PROJECT_CONTEXT.md` is to be preserved unless it specifically conflicts with the new camera.

### What is actually in the scenes

Inspection shows the scenes are remarkably thin, and the level architecture is genuinely data-driven — but **not uniformly so**. What survives a rebuild and what does not falls into a sharp split.

`CulDeSac_Room1.unity` — the reference room — is 20 root GameObjects and 12 prefab instances. The only baked geometry in the entire scene is a single `Ground` object.

**Portable (lives in assets, survives the rebuild):**

- Every environment prop, weapon pickup, cardboard pile, and workbench position — all six `WeaponDropTableSO` assets in `ScriptableObjects/Levels/`. `LevelBuilder` reads exactly one ScriptableObject type and spawns all of it at runtime (`Systems/LevelBuilder.cs:23-31`).
- All prefabs: `pfb_GameManager`, `pfb_RoomManager`, `pfb_EnemySpawner`, `pfb_hud_v4`, `pfb_player`, `pfb_workbench`.
- All weapon / ability / upgrade / difficulty / character ScriptableObjects.
- Zone routing and room shuffling — hardcoded in `Core/GameManager.cs:18-43`, not scene data.
- Baked NavMesh is *already* irrelevant: `LevelBuilder.cs:65` calls `NavMesh.RemoveAllNavMeshData()` and rebakes at runtime from physics colliders. The five `Scenes/<name>/NavMesh.asset` files are vestigial.

**Scene-baked (destroyed by the rebuild):**

1. **`RoomManager._rooms` — the single biggest loss.** `RoomData` is a plain `[Serializable]` class (`Systems/RoomManager.cs:9-22`), **not** a ScriptableObject. `pfb_RoomManager.prefab` ships with `_rooms: []`, so every room definition exists only as prefab-instance property overrides inside each scene:

   ```
   propertyPath: _rooms.Array.data[0].roomName             value: The Arrival
   propertyPath: _rooms.Array.data[0].maxConcurrentEnemies value: 3
   propertyPath: _rooms.Array.data[0].spawnPoints.Array.size value: 4
   propertyPath: '_rooms.Array.data[0].spawnPoints.Array.data[0]'  objectReference: {fileID: 816146668}
   ```

   Those `objectReference` fileIDs point at **scene-local `EnemySpawnPoint` GameObjects**. Room name, enemy concurrency cap, spawn-point wiring, `exitGate`, `propsGroup`, and `bossOwnedWin` all vanish with the scene.

2. **`EnemySpawnPoint` placement and per-point config** — 4–5 scene GameObjects per scene, each carrying `_enemyPrefab` and `_spawnCount` (`Systems/EnemySpawnPoint.cs:12-13`). Positions are transforms, not data.
3. **`Ground`** — the one baked mesh.
4. **`Boundary_North/South/East/West`** — invisible collider walls.
5. **Lighting and RenderSettings**, plus Sun and the two post-process `Volume` placements.
6. **`pfb_hud_v4` instance overrides** — 88 to 114 per scene, a substantial amount of per-scene UI divergence that nobody has reconciled.

So the honest position is: **the environment layer is data-driven and portable; the encounter layer is not.** Rebuilding scenes as-is would mean re-authoring every room's enemy structure by hand, in the Editor, five times — precisely the work a two-person non-expert team is least equipped to redo, and precisely the work most likely to be redone again after camera playtests.

### Why the camera forces layout change anyway

ADR-0001 reduces lateral visible width from 26.5 m to 16.8 m and rear visibility from 6.6 m to 4.2 m. Meanwhile `Player/PlayerController.cs:21` sets `_arenaBoundaryRadius = 18f` — a 36 m-diameter arena. **Over half of any arena fight would happen off screen.** Combat space must shrink to roughly 8–9 m radius regardless of any creative decision, so no existing room layout survives the camera change on its merits.

The camera also introduces a constraint no current room was authored against: with no deoccluder (ADR-0001), every walkable point needs a clear volume of ≥ 8 m behind and ≥ 6 m above along the camera axis.

## Decision

**Rebuild all scenes, but extract room structure into ScriptableObject data *first*.**

1. **Introduce `RoomDataSO`** — promote the existing `RoomData` class to a ScriptableObject asset. Room name, `maxConcurrentEnemies`, enemy composition, and spawn-point *positions* (as `Vector3` data, not scene object references) move into assets under `ScriptableObjects/Rooms/`. `RoomManager` consumes a `RoomDataSO[]`.
2. **Extend `LevelBuilder` to spawn enemy spawn points** from that data, exactly as it already spawns props, pickups, cardboard, and workbenches. This makes the encounter layer as portable as the environment layer already is, and it removes the last reason for a room to contain hand-placed gameplay objects.
3. **Rebuild scenes as thin composition roots only** — ground, boundaries, lighting, volumes, and the manager/player/HUD prefab instances. No gameplay content authored in-scene.
4. **Add a camera-clearance validation pass** to the builder: assert the ≥ 8 m rear / ≥ 6 m overhead clear volume over the walkable area, and assert combat radius ≤ 9 m. Fail loudly in the Editor rather than silently shipping an unreadable room.
5. **Keep the existing scene files untouched on disk** as reference until new scenes are verified, per the owner's directive. Deletion remains a separate decision.

### Why extraction before rebuild, rather than after

This is the crux of the ADR. Rebuilding first and extracting later means authoring every room twice. Extracting first means:

- room content becomes reviewable in a diff (a `.asset` file) instead of buried in a 80 KB scene YAML as `objectReference` fileIDs;
- the audience-contribution goal becomes reachable — a contributor authors a room by creating a data asset, not by opening a scene and wiring scene-local references correctly;
- re-tuning rooms after camera playtest becomes a data edit, not a scene rebuild — which matters because ADR-0001 explicitly expects tuning iterations;
- it makes the *next* rebuild cheap too.

The cost is real: it is a code change (`RoomManager`, `LevelBuilder`, a new SO type) that must precede level work, and it is production work not yet authorized.

## Alternatives considered

**1. Rebuild scenes as they are, keeping `RoomData` scene-embedded.** Lowest immediate cost and no code change; matches the current shipped pattern. Rejected: it re-creates by hand the exact structure that will need re-tuning after camera playtests, and it leaves the encounter layer un-diffable and un-contributable. For a project whose stated goal is audience-authored worlds, hand-wiring scene-local spawn references is the wrong long-term shape.

**2. Extract room data *and* move ground/boundaries/lighting into data too — fully generated scenes.** Maximally consistent with "levels are data, not scenes." Rejected as over-reach for this milestone: lighting and ground geometry are exactly the things a designer wants to see and nudge in the Editor, generated scenes are far harder for a non-expert to debug when they go wrong, and it would make the two reference worlds *less* legible as examples. Scenes remain composition roots — the studio rule already says so.

**3. Abandon `LevelBuilder` and hand-author rooms as ordinary scenes.** Simplest possible mental model, and the easiest thing to teach on a livestream. Rejected: it discards a working, genuinely data-driven system, contradicts the project's level-generation contract, and scales worst in exactly the direction the project is heading (many audience-built worlds).

**4. Defer the whole rebuild until after camera tuning is finished.** Tempting, since layout depends on final camera values. Rejected only in sequencing terms: the *data extraction* has no dependency on final camera numbers and should proceed first; the *layout authoring* should indeed wait for the camera to be validated. These are separated in the recommended order below.

## Consequences

### Positive

- Room content becomes portable, diffable, and authorable without Editor scene surgery — directly serving the audience-contribution goal.
- Camera re-tuning after playtest becomes a data edit rather than a rebuild.
- Scenes shrink to composition roots, matching the studio architecture rule.
- Clearance and arena-radius constraints become machine-checked instead of relying on a non-expert noticing an unreadable room.

### Negative / risks

- Requires production code changes before level authoring can start; this ADR does not authorize them.
- One-time migration cost: the five existing rooms' encounter data must be transcribed into assets before the scenes are abandoned, or it is lost. **This transcription must happen while the old scenes still exist.**
- `LevelBuilder` gains responsibility; it must not become a god object. Spawn passes stay separate and independently testable.

### Debt this rebuild should clear rather than carry forward

Discovered during inspection; these should not be re-created in new scenes:

| Item | Evidence | Action |
|---|---|---|
| **Three Build Settings scenes do not exist on disk** | `LoadingScreen.unity`, `TownSquare_Room1.unity`, `TownSquare_BossHall.unity` are in `EditorBuildSettings.asset` but absent | `GameManager.cs:34` `ZoneStartScene[1] = "TownSquare_Room1"` is unreachable. Clean up during rebuild |
| **Two parallel enemy-spawn systems run simultaneously** | `RoomManager`'s `EnemySpawnPoint` path and `Enemy/EnemySpawner.cs`'s self-driving `Transform[]` coroutine both active in CulDeSac scenes | Choose one. `GameManager:165-169` depends on `EnemySpawner` for `_totalEnemyCount` |
| **Two forge UIs, the better one dead** | `UI/ForgeUI.cs` (514 lines, zero references in any scene or prefab) vs live `UI/ForgePanel.cs` (193 lines) | Decide before rebuilding HUD wiring |
| **`_spawnRoot` is null** | `{fileID: 0}` in `CulDeSac_Room1` | All builder-spawned content lands at scene root. Assign a container in new scenes |
| **Dead data fields** | `WeaponSpawnEntry.useRarityOverride` / `rarityOverride` declared but never read by `LevelBuilder.cs:128-143` | Implement or remove |
| **`Editor/Sprint4SceneSetup.cs` is superseded** | Wires `HUDController`, but live `GameManager.cs:55` field is `HUDController_V2`; hardcodes `"You Win!"` copy | Do not use for the rebuild |
| **Orphaned boss content** | `BossHallDoor` hardcodes `_bossSceneName = "TownSquare_BossHall"` (`:19`), a scene that does not exist; `BossRoomWeaponSpawner` in no scene or prefab | Resolve or remove |
| **HUD prefab overrides** | 88–114 per scene | Reconcile into the prefab; do not re-create divergence |

### Recommended sequencing

1. Transcribe existing room encounter data into `RoomDataSO` assets — **while the old scenes still exist.**
2. Land the `RoomDataSO` / `LevelBuilder` code change and the clearance validation.
3. Validate the camera on one grey-box room (ADR-0001 validation steps).
4. Only then author World 1 and World 2 layouts against the confirmed camera.
5. Verify new scenes; then raise scene deletion as a separate decision.

Steps 1–2 are production work and are **not authorized by this ADR.**
