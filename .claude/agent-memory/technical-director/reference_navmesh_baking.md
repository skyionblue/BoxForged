---
name: navmesh-baking
description: How this project bakes NavMesh (legacy Navigation window, NOT NavMeshSurface), the silent-wrong-bake trap when Ground is not NavigationStatic, which agent settings are authoritative, and why NavMeshModifier components here are inert.
metadata:
  type: reference
---

**This project bakes with the LEGACY Navigation window**, not the `NavMeshSurface` workflow. The tell is `Scenes/<SceneName>/NavMesh.asset` referenced from the scene's `NavMeshSettings.m_NavMeshData`. In code: `UnityEditor.AI.NavMeshBuilder.BuildNavMesh()` (plus `ClearAllNavMeshes()` first), then save the scene. Confirmed for both `CulDeSac_WildWestCity` (World 1) and `Backyard_Dojo` (World 2, baked 2026-09-01).

**Do not "modernise" one scene to `NavMeshSurface` while the other uses the legacy path.** That divergence is worse than either choice on its own.

## The trap: a bake with no floor SUCCEEDS

`Backyard_Dojo` had `m_NavMeshData: {fileID: 0}` for its whole life (`docs/BACKLOG.md` B122). The cause was **not** that nobody pressed Bake — `Ground`, the single 66.6 × 66.6 m plane that is the scene's *only* walkable surface, carried `m_StaticEditorFlags: 0`. World 1's `Ground` carries `4294967295` (Everything).

Baking in that state **reports success** and produces a plausible 67.6 m² / 38-triangle asset made of wall-tops, shed roof and koi-pond lid, with no floor at all and bounds that stop short of the boss arena. Nothing errors. **Always check the bake's `area` and world bounds against the playable region**, not just that an asset appeared:

```
NavMesh.CalculateTriangulation()  -> verts, indices
area = Σ |cross(b-a, c-a)|/2      -> compare to expected playable m²
```
Correct result for `Backyard_Dojo`: **1 139 verts / 485 tris / 1 216.1 m²**. Then `NavMesh.SamplePosition` at the spots that matter and `NavMesh.CalculatePath` for `PathComplete`.

**When fixing the flag, set the `NavigationStatic` bit only (`| StaticEditorFlags.NavigationStatic`, 0 → 8).** Do not copy World 1's "Everything" — the GI, batching and occlusion bits are separate rendering decisions with their own consequences.

## Which agent settings win

Per-scene `NavMeshSettings.m_BuildSettings` is what the bake uses, and it **overrides** the project agent-type defaults from `NavMesh.GetSettingsByIndex(0)`. Both scenes carry identical per-scene settings: **`agentRadius 0.5`, `agentHeight 2`, `agentSlope 45`, `agentClimb 0.4`**. The `agentClimb 0.75` quoted in B116 is the *project* default and is not what bakes. 0.4 is authoritative.

Note the standing gap (B114): boss `NavMeshAgent.radius = 1` against a mesh baked at 0.5, so bosses path closer to walls than their agent radius claims. True for both bosses; consistent, not new.

## `NavMeshModifier` components here are inert

`Backyard_Dojo` has 8 `NavMeshModifier`s, all `m_IgnoreFromBuild: true`. `NavMeshModifier` belongs to `Unity.AI.Navigation` (the `NavMeshSurface` workflow) and **the legacy bake ignores it completely**. Six of those objects are `NavigationStatic` court props, so they carve holes their author explicitly asked them not to — and `ZoneDirector._clearOnBossZone` deactivates all six before the boss fight, so the holes outlive the props. `docs/BACKLOG.md` B127.

## The navmesh is bigger than the play area

Because `Ground` is one big plane and the dojo walls are only 2.4 m tall obstacles standing on it, the bake legitimately produces walkable floor **all around the outside** of the arena (~318 m² of court inside 1 216 m² of total). This made an unclipped camera-occlusion sweep report 7 344 false in-frame positions. Clip spatial sweeps to the actual play region, and see `docs/BACKLOG.md` B128 for bounding the bake.

`NavMesh.asset` is **not** LFS-bound here (`filter: unspecified`, ~34 KB straight to git) — no quota impact.

Related: [[boss-intro-camera]], [[project-preproduction-gate]], [[project-docs-drift-from-code]]
