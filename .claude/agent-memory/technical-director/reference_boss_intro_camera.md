---
name: boss-intro-camera
description: The boss-intro cinematic camera contract (ADR-0008, amended twice) — authored vantage, three invariants (I2 stated horizontally, with the buriedYOffset coupling), the 63-68% framing band, and the measured Blossom Court geometry at the rescaled 4.250 m Grasscutter.
metadata:
  type: reference
---

Boss intros on this project are **hard-cut cinematic shots from an authored vantage**, not runtime-computed camera offsets. Recorded as `docs/adr/0008-boss-intro-camera-authored-vantage.md` (accepted 2026-09-01, **amended the same day** — see [[cherry-tree-position-is-canon]]).

**The three invariants.** Each closes a failure class by proof, so none needs a per-approach-angle or per-device sweep to trust:

- **I1 (walls).** Camera and every look target must be interior points of the arena polygon. Boss arenas here are convex (World 2's is a 16-gon), so no segment between interior points can cross the boundary. The old design put the camera *outside* the hull, which is why no distance value could fix it.
- **I2 (the player).** Compare **horizontally**, and include the burial term:
  `activation_h > √(tickOver² − b²) > √(trigger² − b²) > distance(camera, boss)`, where `b = |_buriedYOffset|`.
  Every point that can occlude the boss lies within `distance(camera, boss)` of the boss, so a proximity trigger firing at a *larger* radius leaves the player provably behind or beside the camera at every bearing. Get it backwards — as `_introTriggerRange = 9` vs a 14 m camera did — and the player stands exactly where the camera does.
  **The burial term is the trap (see trap 5).** Also measure `activation_h` to the **nearest touchable point of the trigger volume**, not its centre — these are box colliders several metres across; using the centre overstated it by 0.7 m in World 2.
- **I3 (the reveal).** During the pre-reveal shot the boss must be **entirely below the opaque ground plane**. Then it cannot be spoiled in that shot at any aspect ratio. This replaced an angular-separation margin that, with the tree pinned, came down to **0.1° at 20:9** — chasing a 2° margin across a 16:9-to-21:9 device population is the same "correctness depends on the sampled configuration" failure the ADR exists to end. Bury the subject instead. It is also usually what the GDD already asks for.

**Framing, and the band that holds.** Aim at ~50% of the subject's **measured** height, never at `transform.position` (the feet). `SpinCycleAI` has always had `_introCamLookHeight = 2.0f`; `GrasscutterAI` dropped it in the port — check for it in any new boss. Target fill is **63–68% of frame height** (SpinCycle's playtested reveal is 63%). `Lens.FieldOfView` is **vertical**, so vertical framing is aspect-independent; only horizontal margins vary — check 4:3, it is always the tight one. For a *tree/landmark* look point, aim at the mesh's **triangle-area-weighted centroid**, not `Renderer.bounds.center.y` (for the cherry tree those are 4.46 vs 3.81, and the bounds midpoint sits in the trunk because the box floor is the root flare).

**Match the gameplay camera where you can.** `pfb_CM_FollowCam` is FoV **45**, world euler **(36, 0, 0)** — a *fixed* world yaw of 0, plus `AspectAdaptiveCameraFraming` (whose policy is: lock vertical FoV and pitch, adapt distance; stated target aspect 16:9). Placing the intro anchor so the camera→boss axis lies along **world +Z**, with `_introCamFoV = 45`, makes the handoff to gameplay a pure pitch-and-position cut — zero yaw rotation, zero focal-length change. That killed two of the three components of the ADR-0001 handoff pop, and it is what decided which side of the arena the Grasscutter goes on.

**Measured Blossom Court geometry** (`Backyard_Dojo.unity`, Edit Mode, 2026-09-01). `[ENV - Static]` is at **yaw 45°**; `m_LocalEulerAnglesHint` reads `(0,0,0)` and lies. `world = ((x + z)/√2, y, (z − x)/√2)`.

| | design | world |
|---|---|---|
| Arena centre | `(0, 55.0)` | `(38.891, 38.891)` |
| Wall inner face (apothem, 16-gon) | **10.0001 m** (3600-ray sweep) | — |
| Wall circumradius | 10.196 m | — |
| South gate opening | bearings 158.7°–201.3° | — |
| `RoomTrigger_Zone2` (activation line) | `(0, 47.0)` | `(33.234, 33.234)` |
| Cherry tree trunk axis — **CANON, do not move** | `(0, 62.530)` | `(44.215, 44.215)` |
| Canopy: max radius **4.503** about the trunk axis, foliage from y ≈ **2.0**, southern edge z **59.26** | `x ±4.359, z 59.262…65.798` | — |
| `RoomTrigger_Zone2` — a **9.8 × 3.0 m box**, not a point | `x −4.90…4.90, z 45.50…48.50` | centre `(33.234, 33.234)` |
| Boss dormancy per ADR-0008 **Amendment 2** | `(−6.10, 57.50)` | `(36.3453, 44.9720)` |
| Intro cam anchor per ADR-0008 **Amendment 2** | `(−0.5139, 1.80, 51.9139)` | `(36.3453, 1.800, 37.0720)` |

**Grasscutter is 4.250001 m** (root scale `2.5`); SpinCycle 4.250760. Silhouette radius 2.275, footprint 4.452 × 1.966.
Chosen field values: FoV 45, bossLookHeight **2.13**, treeLookHeight 4.50, triggerRange **9.6**, tickOverRange **11.5**, camDistance (fallback) **7.9**, buriedYOffset **−4.45**.
Chain (horizontal): 12.060 (activation) > 10.604 > 8.506 > 7.900 (anchor→boss). Boss fills **64.8 %**, tree 80.9 % whole / 59.9 % foliage.

*(Superseded: the 3.40 m-boss staging was boss `(−6.5, 58.0)`, anchor `(−2.039, 53.539)`, values 1.70 / 8.5 / 11.0 / 6.5 / −3.60. Do not mix the two sets.)*

**Five traps specific to this shot, all of which produced a wrong published number at least once:**

1. **`Renderer.bounds` is not a size measurement here.** It is a world AABB. On skinned meshes Unity derives it through the **root bone**, so a rig with a non-yaw `Hips` rotation inflates it; on environment meshes the 45° ENV yaw inflates it. B119 published the Grasscutter as "4.70 m tall, matching SpinCycle to the centimetre" — measured from mesh vertices it is **3.400 m** and SpinCycle is **4.251 m**. Measure from `sharedMesh.vertices` / `BakeMesh`. See [[project-docs-drift-from-code]].
2. **The cherry tree's canopy has no collider.** Only the 0.35 m trunk capsule is on the `Building` layer, so a `Physics.Linecast` clearance sweep passes through 8.8 m of foliage and reports the shot clean. Voxelise the mesh triangles (0.2 m cells) or ray-triangle test. The built canopy is an **ellipse of half-axes 4.4 × 2.5 m** with foliage from y ≈ 1.9 — not ADR-0006 §1.1's "r ≤ 3.5, underside ≥ 4.0". Its southern edge is design **z = 59.2**; the floor south of that is clear.
3. **A prefab's written serialized value shadows the C# field default.** `pfb_enemy_grasscutter.prefab` carries `_introCamDistance: 14`; editing `= 9f` in the script changed nothing that ships. Re-read the YAML after saving — a clean compile proves nothing. The same prefab also bakes a **world position** into its own root (`design (0, 0, 63.0)`, now inside the tree), masked by the scene instance's override.
4. **The six court dressing props are in `ZoneDirector._clearOnBossZone`** and are deactivated *before* the boss activates. Any Edit-Mode clearance sweep must exclude or deactivate them, or it reports clips that cannot happen. Prefer excluding their colliders from the raycast results over mutating the scene (Edit-Mode state leaks — see the project's Unity-editor-state-leak note).

5. **`_buriedYOffset` silently shrinks `_introTriggerRange`.** `BossIntro` sets `transform.position = buriedPos` *before* the Phase A wait loop, and that loop compares a **3-D** `Vector3.Distance` to a player whose pivot is at its feet. So the serialized range is a **slant range** and the floor radius is `√(range² − buried²)`. Burying deeper to satisfy I3 *tightens* I2 — two fields that look independent, coupled in the direction nobody guesses. ADR-0008's first staging passed only by luck (`√(8.5²−3.6²)=7.70 > 6.309`, term never examined); at the rescaled boss the naive choice fails outright (`√(9.0²−4.45²)=7.82 < 7.90`). Always state I2 horizontally. Code fix proposed as `docs/BACKLOG.md` B129.

**When a sweep says the shot is dirty, check the clip before believing it.** World 2's `Ground` is a 66.6 m plane and the court walls are only 2.4 m tall obstacles on it, so the navmesh covers the whole backyard *outside* the arena. An unclipped trigger sweep reported 7 344 in-frame player positions — all of them outside the wall, unreachable, and wall-occluded. Clip to the arena polygon **and** the navmesh, and add a line-of-sight test.

**Known blockers/gaps recorded, not fixed:** No boss intro locks player movement — B120. The GDD's "tall grass" and "grass and petals kick up" have no implementation — B124. *(B122's missing NavMesh and B123's size question are both CLOSED — see [[project-preproduction-gate]].)*

Related: [[cherry-tree-position-is-canon]], [[project-unsatisfiable-metrics]], [[project-docs-drift-from-code]], [[reference-room-scale-calibration]], [[measuring-city-scene]]
