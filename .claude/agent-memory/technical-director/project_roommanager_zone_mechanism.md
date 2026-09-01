---
name: project-roommanager-zone-mechanism
description: RoomManager already supports multiple sequential encounter zones in ONE scene — it is not a scene loader. GameManager is the part that assumes scene-per-room.
metadata:
  type: project
---

`RoomManager` models an **ordered list of encounter zones inside a single scene**, activated by `RoomTrigger` boundary colliders calling `OnRoomEntered(index)`. It never loads scenes. `AppendRoomsFromLevelBuilder()` builds zones from `LevelBuilder.RoomData` (`RoomDataSO[]`) at `Start()`.

The scene-per-room assumption lives in **`GameManager`**, not `RoomManager`: `OnUpgradePicked` → `LoadNextRoom()` → `SceneManager.LoadScene`, plus `RandomRoomPool` / `InitRoomQueue` / `ZoneStartScene`.

**Why:** this was mis-scoped once already — a single-scene World 1 looked like it needed a `RoomManager` redesign when the real work was in `GameManager` and scene composition. Knowing the split prevents proposing a rewrite of the one system that is already correct.

**How to apply:** when asked about multi-room-in-one-scene, streaming, or level flow, build on `RoomManager`/`RoomDataSO`/`RoomTrigger` and look at `GameManager` for what breaks.

**Update 2026-08-31 — the scene-load half is now dead code, and single-scene is the default (ADR-0005).** `RandomRoomPool` is empty (`GameManager.cs:52`), `LoadNextRoom()`'s exhausted-queue branch logs an error instead of loading (`:607-615`), commit `84a3a44e` deleted every per-room scene, `Assets/_Project/Scenes/` holds one gameplay scene, and `ZoneStartScene[1] = "TownSquare_Room1"` names a scene that has never existed. So "use ADR-0002's documented per-room default" is **not** the conservative option — it means reviving a path with no working reference implementation. The machinery is kept, not deleted, but must be re-qualified before any future world uses it.

Two more things that only work in a single scene: `GameManager.ShowRoomClearScreenDelayed` routes rewards by **in-scene zone index** (0 → upgrade, 1 → shop, ≥2 → boss), so under scene-per-room every `RoomManager` has one room at index 0 and **the shop screen is unreachable**; and a fourth zone would need that routing generalized first. `WildWestCityZoneDirector`'s four serialized fields (`_clearOnBossZone`, `_boss`, `_gateByZone`, `_bossZoneIndex`) are all scene-agnostic data — only the class name is scene-specific — so ADR-0005 §5 renames it to a reusable `ZoneDirector` rather than copying it per world.

Verified 2026-08-26, correcting two earlier errors in this memory: `AppendRoomsFromLevelBuilder()` calls `_rooms.Add(...)`, so `RoomDataSO` zones are **appended after** Inspector-authored ones (not prepended). And a `RoomDataSO` with an **empty `spawnPoints` array plus `bossOwnedWin: true` is already a safe no-op zone** — `ActivateRoom` falls through to the legacy path, finds nothing, and `RoomCleared` returns on the boss guard. That plus `RoomManager.OnRoomActivated` (a `static event Action<int>` with zero subscribers project-wide) means scene-specific staging — activating a pre-placed boss, clearing props, opening gates — belongs in a small scene script, **not** in new serialized fields on `RoomManager`. Subscribe such a script in `OnEnable`, not `Start`: `RoomManager.Start()` can fire `ActivateRoom(0)` synchronously.

It remains true that a `RoomDataSO` cannot hold scene references, and that `LevelBuilder.BuildNavMeshDeferred` calls `NavMesh.RemoveAllNavMeshData()` and re-bakes synchronously from **PhysicsColliders** at `Start()` — so any Editor-baked NavMesh is discarded, a prop with no collider never carves, and a prop deactivated after `Start()` leaves a permanent hole unless it carries a carving `NavMeshObstacle` plus an ignore-from-build `NavMeshModifier`. The old 8.5 m `PlayerController._arenaBoundaryRadius` blocker is **fixed** (B87).

Related: [[project-preproduction-gate]], [[project-docs-drift-from-code]]
