---
name: measuring-city-scene
description: How to measure CulDeSac_WildWestCity correctly — its ENV root is rotated, so renderer AABBs and world coords both lie about the street layout.
metadata:
  type: reference
---

`CulDeSac_WildWestCity.unity` has its `[ENV - Static]` root **rotated in yaw** (45° as of 2026-08-26). Two measurement traps follow, and both produced wrong numbers before being caught:

1. **`Renderer.bounds` is a world AABB.** Transforming its corners back into the rotated street-local frame inflates every footprint badly (the 40 × 47.5 m ground measured as 87.5 × 87.5). Use `MeshFilter.sharedMesh.bounds` through `envRoot.worldToLocalMatrix * meshTransform.localToWorldMatrix` instead.
2. **Raw world X/Z say nothing about the street.** Work in the street-local frame (local +Z = up the street) and convert only at the end. The transform and anchor points are recorded in `docs/adr/0004-world1-single-continuous-scene.md` §0.

Two facts about this scene that are easy to assume wrongly:

- **Building colliders are far wider than their meshes** — single `BoxCollider`s giving a collision corridor of about X ±12.9 against a visual corridor of X −8.8…+10. Clearance computed from colliders is optimistic; compute from meshes.
- **Several props have no collider at all** (broken wagon wheels, rope coils), and short ones (rain barrels 0.9 m, water troughs 0.5 m) are missed entirely by a `Physics.CheckSphere` sampled at chest height. Sample at ~0.45 m, just above the NavMesh climb height.

**How to apply:** any question about how much room something has in this scene — arena size, spawn placement, camera sightlines — should be answered by rasterizing a clearance map from mesh footprints via `execute_code`, not by eyeballing transforms or trusting a prior doc's numbers. The exact scripts and the resulting arena figures are in ADR-0004 §2.

Related: [[project-roommanager-zone-mechanism]], [[project-docs-drift-from-code]]
