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

Verified 2026-08-26, correcting two earlier errors in this memory: `AppendRoomsFromLevelBuilder()` calls `_rooms.Add(...)`, so `RoomDataSO` zones are **appended after** Inspector-authored ones (not prepended). And a `RoomDataSO` with an **empty `spawnPoints` array plus `bossOwnedWin: true` is already a safe no-op zone** — `ActivateRoom` falls through to the legacy path, finds nothing, and `RoomCleared` returns on the boss guard. That plus `RoomManager.OnRoomActivated` (a `static event Action<int>` with zero subscribers project-wide) means scene-specific staging — activating a pre-placed boss, clearing props, opening gates — belongs in a small scene script, **not** in new serialized fields on `RoomManager`. Subscribe such a script in `OnEnable`, not `Start`: `RoomManager.Start()` can fire `ActivateRoom(0)` synchronously.

It remains true that a `RoomDataSO` cannot hold scene references, and that `LevelBuilder.BuildNavMeshDeferred` calls `NavMesh.RemoveAllNavMeshData()` and re-bakes synchronously from **PhysicsColliders** at `Start()` — so any Editor-baked NavMesh is discarded, a prop with no collider never carves, and a prop deactivated after `Start()` leaves a permanent hole unless it carries a carving `NavMeshObstacle` plus an ignore-from-build `NavMeshModifier`. The old 8.5 m `PlayerController._arenaBoundaryRadius` blocker is **fixed** (B87).

Related: [[project-preproduction-gate]], [[project-docs-drift-from-code]]
