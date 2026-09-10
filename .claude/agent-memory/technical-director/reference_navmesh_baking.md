---
name: navmesh-baking
description: THREE different navmeshes exist here (Editor legacy asset, Editor Play Mode runtime bake, device runtime bake) and only the runtime one ships — plus the silent-wrong-bake trap, which agent settings are authoritative, and the Editor/device divergence.
metadata:
  type: reference
---

## The single most important fact: the bake you can see is not the bake that ships

**Corrected 2026-09-10 — the earlier version of this memory said "this project bakes with the LEGACY Navigation window, not NavMeshSurface." That is wrong and it misled B127 for nine days.**

There are **three** navmeshes, and they are not the same:

| # | Which | Produced by | Ships? |
|---|---|---|---|
| 1 | `Scenes/<Scene>/NavMesh.asset` | `UnityEditor.AI.NavMeshBuilder.BuildNavMesh()`, Navigation window | **No** — discarded at runtime |
| 2 | Editor Play Mode runtime bake | `LevelBuilder` | Almost — but see B145 |
| 3 | Device runtime bake | `LevelBuilder` | **Yes. This is the only one that matters.** |

`LevelBuilder.BuildNavMeshDeferred()` (`Assets/_Project/Scripts/Systems/LevelBuilder.cs`) runs in **every** scene with a `LevelBuilder` — both worlds — and does: create a `RuntimeNavMeshSurface` GameObject → `AddComponent<NavMeshSurface>()` → `collectObjects = CollectObjects.All`, `useGeometry = PhysicsColliders` → **`NavMesh.RemoveAllNavMeshData()`** → `BuildNavMesh()`. Everything else stays at `NavMeshSurface` defaults: `layerMask = ~0`, `agentTypeID = 0`, `ignoreNavMeshAgent = true`, `ignoreNavMeshObstacle = true`.

Consequences to apply every time:

- **`NavigationStatic` flags do nothing at runtime.** The runtime path reads colliders, never static editor flags. Verified: `RoomGate_Zone0/1` have `staticFlags = 0` and are still collected. Any fix phrased as "set/clear NavigationStatic" is a no-op on the shipped navmesh.
- **`NavMesh.asset` never loads at runtime.** Keep it for Edit-mode tooling; do not cite it as evidence about gameplay.
- **State which bake you measured.** An Edit-mode `SamplePosition`/`CalculatePath` check is a valid Edit-mode check and nothing more. `docs/BACKLOG.md` B143.
- **Editor Play Mode ≠ device.** A non-convex `MeshCollider` whose mesh lacks Read/Write works in Editor Play Mode and is **silently dropped in a player build** — Unity says so in a warning that is easy to scroll past (`RuntimeNavMeshBuilder: Source mesh … does not allow read access`). `KoiPond` is one such case. `PhysicsColliders` was chosen to dodge the Read/Write problem but only covers `MeshRenderer`s, not `MeshCollider`s. B145.

## `NavMeshModifier` DOES work here (was recorded as inert — wrong)

`Backyard_Dojo` has 8 `NavMeshModifier`s, all `m_IgnoreFromBuild: true`, `m_ApplyToChildren: 1`, `m_AffectedAgents: ffffffff`. Under the runtime `NavMeshSurface` bake they are **honoured**. Proven twice on 2026-09-10:

1. **Collection A/B** — replicating `NavMeshSurface.CollectSources()`'s markup construction and calling the same runtime `NavMeshBuilder.CollectSources` overload: **79 sources without markups → 71 with**, each of the 8 objects dropping 1 → 0.
2. **Navmesh A/B** — with the six court props' `NavMeshObstacle`s disabled and frames ticking, the floor under all six is **continuous** (area 1 220.9 → 1 264.8 m²). So the holes you see are *dynamic carving*, not baked geometry.

The props therefore get both states for free: carved while they are dressing, solid for the boss fight (because `ZoneDirector._clearOnBossZone` `SetActive(false)`s all six, which disables their obstacles). B127's "decide, don't just fix" dilemma never arises — closed as moot, no owner decision.

**Package source is the fastest way to settle questions like this**: `Library/PackageCache/com.unity.ai.navigation@*/Runtime/NavMeshSurface.cs`, `CollectSources()` and `CollectSourcesInHierarchy()`. The latter's `#if UNITY_EDITOR && !isPlaying` branch is exactly why Edit-mode and Play-mode bakes can differ.

## The trap: a bake with no floor SUCCEEDS (still true, Editor bake only)

`Backyard_Dojo`'s `Ground` carried `m_StaticEditorFlags: 0`, so the **Editor** bake produced a plausible 67.6 m² of wall-tops and koi-pond lid with no floor and no error (B122). Always check `area` and world bounds, not just that an asset appeared:

```
NavMesh.CalculateTriangulation() -> verts, indices
area = Σ |cross(b-a, c-a)|/2     -> compare to expected playable m²
```

Reference values for `Backyard_Dojo`: Editor bake **1 139 verts / 485 tris / 1 216.1 m²**; runtime bake **849 verts / 363 tris / 1 220.9 m²** (they differ — expected, different collection rules).

Note `CalculateTriangulation()` reflects carving, so an obstacle A/B needs **frames to tick between the change and the read**. In this Editor `Application.runInBackground` is **false**, so Play Mode does not advance while the Editor is unfocused — `Time.frameCount` sits still and your A/B silently measures nothing. Set `Application.runInBackground = true` at runtime first, and always print `Time.frameCount` to prove frames moved.

## Which agent settings win

Per-scene `NavMeshSettings.m_BuildSettings` overrides the project agent-type defaults. Both scenes: **`agentRadius 0.5`, `agentHeight 2`, `agentSlope 45`, `agentClimb 0.4`**. The `agentClimb 0.75` in B116 is the project default and is not what bakes. Standing gap (B114): boss `NavMeshAgent.radius = 1` against a mesh baked at 0.5.

## Bake size and cost

`Ground` is one 66.6 × 66.6 m plane and the dojo walls are 2.4 m obstacles on it, so the bake legitimately covers ~1 216 m² against ~318 m² of playable court, including walkable floor **outside** the arena. Clip spatial sweeps to the play region (an unclipped occlusion sweep once reported 7 344 false in-frame positions).

**Measured cost, so nobody re-panics about it:** the full-scene runtime bake is **≈ 5.6–6.7 ms** in the Editor; a 48 × 48 m bounds-constrained bake is **≈ 3.0–3.7 ms**. The entire prize from B128's bounds volume is ~3 ms against ADR-0004 §8's 500 ms scene-start budget. **It is not a hitch risk** — B134 downgraded P1 → P3. And `LevelBuilder` has no bounds concept, is shared by both worlds, and would need a new serialized field, so it is not a drive-by fix either.

`NavMesh.asset` is not LFS-bound (~34 KB straight to git).

Related: [[srp-batcher-counter-is-broken]], [[boss-intro-camera]], [[project-docs-drift-from-code]], [[project-unsatisfiable-metrics]]
