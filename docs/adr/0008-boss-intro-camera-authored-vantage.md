# ADR-0008: Boss-intro cameras use an authored vantage point, and three invariants that make the shot provable

- **Status:** **Accepted (architecture) — 2026-09-01. Amended three times (2026-09-01 x2, 2026-09-02 x1).** *Amendment 1* followed the owner correcting a premise about the cherry tree. *Amendment 2* followed the owner deciding to rescale the Grasscutter to SpinCycle's true height; every measured number in §2, §4 and §5 was re-derived a third time and the 3.40 m-based figures are superseded. The camera/staging implementation from Amendment 2 was completed 2026-09-02. **Amendment 3 (2026-09-02) retires §4's I2/I3 invariant chain**: the trigger model changed from proximity-gated to activation-gated (owner decision, following a playtest — see `docs/BACKLOG.md` B120), so there is no more distance-gated trigger for I2/I3 to guarantee anything about. §2's boss position, §3's authored-vantage camera anchor, and §5's framing (look heights/FoV) are all unaffected and remain the shipped implementation — only the *when* of the cut changed, not the *where* the camera sits or *what* it frames.
- **Date:** 2026-09-01 (amended twice, same day)
- **Scope:** the boss-intro cinematic camera contract, project-wide. World 2's Grasscutter is the case that forced it; World 1's SpinCycle is re-read against it in §6 and is **not** retroactively changed.
- **Related:** [ADR-0001](0001-fixed-low-follow-camera.md) (the follow cam's 45° FoV, its fixed world yaw 0 / pitch 36°, and the handoff pop), [ADR-0005](0005-world2-single-continuous-scene.md) §4 (boss contract), [ADR-0006](0006-world2-zone-scale-and-arena-metric.md) §1.1/§1.4/§2.2 (arena centre, radius, dressing ring, M2).

---

## Amendment 2 (2026-09-01) — the boss was rescaled to 4.251 m, so every framing number here is the *third* derivation

**Read this before trusting any number below.** The measured figures in §2, §4 and §5 have now been derived three times against three different boss heights, and only the third is current:

| Pass | Boss height used | Status |
|---|---|---|
| B119's original | 4.70 m | **Wrong** — `SkinnedMeshRenderer.bounds` read through a rotated `Hips` root bone (Fact 4a) |
| This ADR's Amendment-1 pass | 3.40 m | **Correct measurement, superseded decision** — it was the true height at root scale `(2,2,2)` |
| **Current (Amendment 2)** | **4.250001 m** | **Authoritative** — the owner rescaled the prefab root to `(2.5, 2.5, 2.5)` |

**What the owner decided.** B123 asked whether the two bosses were meant to read as the same size. The answer is yes: the Grasscutter is rescaled to match SpinCycle's *corrected* true height of **4.250760 m**. B123 is thereby resolved, not merely deferred.

**What was actually done, and measured (this session, Edit Mode, live scene instance — not the prefab asset):**

- `pfb_enemy_grasscutter.prefab` root `m_LocalScale` **`(2,2,2)` → `(2.5, 2.5, 2.5)`**. The base unscaled model is exactly **1.700 m**, so 2.5 is the clean authorable factor; the arithmetic `4.251/1.70 ≈ 2.5004` was **not** trusted, it was checked by measurement at five candidate scales.
- Resulting height, measured on the **live scene instance**: **4.250001 m**. Delta from SpinCycle **−0.00076 m** against a stated tolerance of **±0.01 m**. (`Renderer.bounds.size.y` now reads 5.8699 — still not a height, still not to be quoted.)
- The scene instance carries **zero** `m_LocalScale` overrides, so the prefab value propagates. Verified via `PrefabUtility.GetPropertyModifications`.
- Silhouette radius **1.820 → 2.275 m**. Footprint **3.562 × 1.573 → 4.452 × 1.966 m**.

**Measurement method, and why it is trustworthy this time.** Heights are computed by evaluating the skin directly — `worldVertex = Σᵢ wᵢ · (bones[i].localToWorldMatrix · bindposes[i] · v)` over all 32 546 vertices — which never consults a bounding volume and so cannot inherit the root-bone rotation error of Fact 4a. Run against the *unchanged* prefab it reproduces the Amendment-1 numbers to five decimals (Grasscutter 3.40000, SpinCycle 4.25076), and it independently reproduces the tree's area-weighted centroid at **4.462**. Three cross-checks, so the method is validated rather than merely asserted.

**Consequences that changed the answer (not just the numbers):**

1. **The Amendment-1 boss position is no longer viable.** At design `(−6.5, 58.0)` the wall clearance past the blade tips falls from 1.149 m to **0.652 m** — exactly the collision B123 predicted ("a further 1.25× would … eat most of the 1.149 m wall clearance"). The boss moves to design **`(−6.10, 57.50)`**; §2 is re-derived.
2. **The camera must retreat**, because framing is set by subject height: the anchor→boss distance goes **6.309 → 7.900 m** to hold the 63–68 % fill band.
3. **That retreat exposed a coupling I2 did not model: `_buriedYOffset` shrinks the effective trigger radius.** `BossIntro` sets `transform.position = buriedPos` *before* the Phase A wait loop, and that loop compares a **3-D** `Vector3.Distance` to the player. With the boss buried 4.45 m the horizontal trigger radius is `√(trigger² − 4.45²)`, not `trigger`. At the Amendment-1 values this happened to still hold (`√(8.5² − 3.6²) = 7.70 > 6.309`) — by luck, unexamined. At the new distance it would **fail**: `√(9.0² − 4.45²) = 7.82 < 7.90`. **I2 is restated in horizontal terms in §4.** This is the single most important finding of this pass, because it is a trap every future buried-boss intro inherits.
4. **The "activation distance" in Amendment 1 was measured from the wrong thing.** It used `RoomTrigger_Zone2`'s *centre* (12.777 m). The trigger is a **9.8 × 3.0 m box**, and the player activates the zone on first contact with its surface. First contact is provably the **south face** (§4), giving **12.060 m**.
5. **B122 (bake the NavMesh) is DONE**, and its root cause was not "nobody pressed Bake" — see the Validation section.

**What survived unchanged, re-verified rather than assumed:** the tree does not move (Amendment 1); no computed `_introCamDistance` can work (Fact 2); the six `_clearOnBossZone` props are a false positive (Fact 5); the authored-anchor architecture (§3); and the anchor still places the camera→boss axis on **world +Z** for the yaw-match with `pfb_CM_FollowCam`.

**One geometric correction to Fact 0**, found while re-measuring: the canopy's design-Z extent is **59.262 … 65.798** (symmetric about the trunk at 62.53), not the "59.2 … 64.2" recorded in Amendment 1 — that figure was not symmetric about the trunk and cannot have been right. The canopy's **maximum radius about the trunk axis is 4.503 m** (in the y 4.0–4.5 band), and foliage begins at **y ≈ 2.0**. The southern edge at z = 59.26 is unchanged and is still the number the staging depends on.

---

## Amendment 1 (2026-09-01) — the cherry tree's position is an owner decision, not a defect

The first version of this ADR opened by asserting that `CherryTree_TrunkCollider` / `CherryTree_BlossomCourt` sitting at design `z = 62.53` — roughly 7.5 m north of the Blossom Court's `(0, 55.0)` centre — was an unintended B116 regression, and its §1 decided to move both objects back to the centre.

**That premise is wrong. The owner has stated: *"I moved the tree to the back of the area because it made more sense back there."*** The north-rim position is an intentional design decision.

Per `CLAUDE.md` ("Existing architecture contracts … are preserved unless Discovery/Pre-production explicitly approves a change") and `.claude/rules/studio-core.md` §Creative decision discipline, this is now the owner's explicit call. It is **CANON** and is not re-litigated here.

Consequences of the correction, recorded rather than argued:

- **[ADR-0006](0006-world2-zone-scale-and-arena-metric.md) §1.1's rejection of a north-rim tree is superseded for this specific placement, by owner authority.** §1.1's four grounds are **not deleted** — they remain on record and may still be sound general guidance for future arena dressing. Two of them have now been overridden knowingly, and their real consequences are recorded as backlog items rather than treated as bugs: the boss's dormancy spot must move off the north rim (this ADR, §2), and zone 2 no longer has a central obstruction to orbit (`docs/BACKLOG.md` B125).
- **The camera is now solved with the tree pinned as a fixed, non-negotiable constraint.** Every number in §2–§6 was re-derived from scratch against the tree's *measured* built position and *measured* built geometry, not against ADR-0006's intended envelope.
- **The two structural findings survive the correction intact**, and were re-verified rather than assumed: no fixed `_introCamDistance` that retreats away from the player can work in this arena (Fact 2), and the `CourtMakiwara_A/B` clip found in review is a false positive because those props are deactivated before the boss activates (Fact 5).
- **B121 (the process finding) partly stands and is re-scoped, not deleted.** B116's completion note is still inaccurate about *what it did* — it reports "re-centring both at the new centre `(0,0,55.0)`" while `git log -L` shows the trunk moved `z: 47.5` → `z: 62.53` — and its M2 figure is still a number the built layout cannot produce. That the resulting position turned out to be the one the owner wanted does not make the note accurate. See `docs/BACKLOG.md` B121.

---

## Context

`GrasscutterAI`'s boss intro is a two-shot hard cut: the camera cuts to the cherry tree, holds 1.4 s, then cuts to the Grasscutter rising out of the grass. Two consecutive attempts to make that shot work failed review, each by clipping into different geometry. This ADR records why neither could have worked, and replaces the mechanism rather than the numbers.

### Fact 0 — the built geometry, measured, and where it differs from what the docs say

All measurements below were taken on 2026-09-01 from `Backyard_Dojo.unity` open in the Editor, Edit Mode, scene not dirty. `[ENV - Static]` is at **yaw 45°**, so every figure marked *design* is ENV-local; `design → world` is `world = ((x + z)/√2, y, (z − x)/√2)`.

| Element | Measured (design) | Doc says | Verdict |
|---|---|---|---|
| Arena centre | `(0, 55.000)` | ADR-0006 §1.4 `(0, 55.0)` | ✓ |
| Wall inner face (apothem, 16-gon) | **10.0001 m** (3600-ray `Building` sweep, min at bearing 78.5°) | 10.0 m | ✓ re-verified |
| Wall circumradius | 10.196 m | — | ✓ consistent with a 16-gon |
| South gate opening | bearings **158.7° – 201.3°** | — | recorded |
| Court dressing ring | r = 8.000–8.011 m, 6 props | ADR-0006 §1.4 r = 8.0 | ✓ |
| `CherryTree_TrunkCollider` | `(0, 2.0, 62.530)`, capsule r 0.35 × h 4.0, layer 8 | — | **owner-decided position** |
| `CherryTree_BlossomCourt` visual | `(0, 3.810, 62.530)`, mesh spans y **−0.051 … 7.671** | height cap ≤ 8.0 m | ✓ under the cap |
| Canopy plan extent | **x −4.359 … +4.359, z 59.262 … 65.798** (symmetric about the trunk); **max radius about the trunk axis 4.503 m**, in the y 4.0–4.5 band | ADR-0006 §1.1 "canopy r ≤ 3.5" | **violated by 1.00 m. Amendment 2 corrects Amendment 1's "z 59.2 … 64.2 / half-axes 4.4 × 2.5", which was not symmetric about the trunk and understated the Z half-axis by ~0.8 m. The southern edge (z 59.26) — the figure the staging actually uses — is unchanged.** |
| Canopy underside | first foliage at y ≈ **2.0**; radius ≥ 4.2 from y ≈ 2.5 | ADR-0006 §1.1 "underside ≥ 4.0" | **violated by ~2.0 m** |
| Canopy visual centroid (triangle-area weighted) | `(−0.159, **4.462**, 62.163)` | — | this is the look point, see §5 |
| `Renderer.bounds.center.y` of the tree | 3.810 | — | see §5 for why it is the wrong number to aim at |
| Grasscutter **mesh** height | ~~3.400 m~~ → **4.250001 m** at root scale `(2.5,2.5,2.5)`, feet exactly at y = 0 | B119: "4.70 m tall exactly" | **B119's figure is an artifact — see Fact 4. The 3.400 m figure was correct at the old `(2,2,2)` scale and is superseded by Amendment 2's rescale.** |
| Grasscutter **mesh** footprint | ~~3.56 × 1.57 m~~ → **4.452 × 1.966 m**; max radius from own axis ~~1.820~~ → **2.275** | B119: "5.39 × 5.27 m" | same artifact; rescaled per Amendment 2 |
| SpinCycle **mesh** height | **4.251 m** | B119: "4.70 m" | same artifact |
| Baked NavMesh in this scene | ~~none~~ → **baked 2026-09-01: 1 139 verts / 485 tris / 1 216.1 m²**. Root cause of the absence was `Ground` carrying `m_StaticEditorFlags: 0` | B116 reports `PathComplete` validations | **resolved; `docs/BACKLOG.md` B122. B116's reported validations still cannot have been run against the scene as it was saved.** |
| "Tall grass at the far end" (GDD §5) | **no such object exists in the scene** (no GameObject whose name contains "grass") | GDD §5 | `docs/BACKLOG.md` B124 |
| `ZoneDirector._clearOnBossZone` | exactly the 6 court props, verified by reading the serialized array | — | ✓ Fact 5 holds |
| Gameplay camera | `pfb_CM_FollowCam`: **FoV 45**, world euler **(36, 0, 0)**, plus `AspectAdaptiveCameraFraming` | ADR-0006 §2.6 | ✓ — used as a design input in §3 |

### Fact 1 — the tree is a much larger occluder than any document says, and it has no collider above the trunk

This replaces the original Fact 1 (which claimed the tree's position was a defect; see *Amendment*).

The tree is **not** the "r ≤ 3.5, underside ≥ 4.0" cylinder ADR-0006 §1.1 specified. Measured from the mesh, it is an ellipse of half-axes 4.4 × 2.5 m in plan, with foliage starting at y ≈ 1.9 and topping out at 7.67 m. Its southern edge sits at design **z = 59.2**; the arena floor south of that line is completely free of tree geometry.

Two traps follow, and both would have produced a wrong answer if taken from the docs instead of the asset:

1. **A radius-3.5 approximation understates the canopy by 1.0 m** on its wide axis, and the canopy is elliptical rather than circular, so a single radius is wrong in both directions depending on bearing.
2. **Only the 0.35 m trunk capsule is on the `Building` layer.** The canopy has **no collider at all**. A `Physics.Linecast` clearance sweep — the method this ADR's original §Validation prescribed — passes straight through 8.8 m of foliage and reports the shot clean. Every canopy clearance figure in this ADR is therefore computed against a **voxelisation of the tree's actual triangles** (0.2 m cells, 130 128 surface samples, 8 122 occupied cells), not against raycasts.

### Fact 2 — no value of `_introCamDistance` could ever have worked, and that is provable

*(Unchanged, and re-verified against the measured arena.)*

`ComputeIntroCamPosition()` places the camera at `bossPos + normalize(bossPos − playerPos) × _introCamDistance` — it **retreats away from the player**. The arena is a convex 16-gon of inner radius 10.0 m about `(0, 55)`. So the camera's radius from the arena centre is at least `r_boss + D` whenever the player is on the inward side, which is every approach through the only gate.

- Original staging: `r_boss = 8.003`, `D = 14` → camera at **r = 22.0 m**, i.e. **12.0 m outside the wall**.
- Reducing to `D = 9` at the same boss position → r = 17.0, still 7.0 m outside.

The requirement set was unsatisfiable as posed: *far-end boss* + *away-from-player retreat* + *enough distance to frame the subject* cannot all hold inside a 10 m arena. Every fix so far has re-derived the same formula and moved the collision to different geometry.

**The retreat-along-the-boss↔player-axis rule was never a requirement.** The GDD asks for two hard cuts; it says nothing about where the vantage sits. It is an implementation detail that has now generated three bugs.

### Fact 3 — the boss cannot stay at the arena centre, and it cannot stay on the north rim either

The current work-in-progress moves the boss to design `(0.354, 0, 54.801)` — 0.41 m from the arena centre. That does put the camera inside the wall, but it abandons the GDD's *"dormant in the tall grass at the arena's far end"* with no creative decision on record.

The original staging — the north rim at design `(0, 0, 63.0)`, per ADR-0006 §1.4 — is now **unavailable**, because the tree occupies that spot by owner decision. The boss's dormancy point is the thing that moves. §2 re-derives it.

*(The original Fact 3 also cited "it occupies the spot the tree is supposed to hold" and "it destroys zone 2's orbital movement grammar." The first is void — the tree is not at the centre. The second is no longer an argument about the boss: with the tree at the rim, zone 2 has no central obstruction at all, which is a consequence of the owner's decision and is recorded as `docs/BACKLOG.md` B125, not as an objection here.)*

### Fact 4 — the boss is 3.40 m tall, not 4.70 m, and the vertical crop is a *look-point* bug

Two separate things were wrong, and they compound.

**(a) The height figure is a measurement artifact.** B119 reports the Grasscutter at "4.70 m tall exactly, matching SpinCycle to the centimeter." That number is `SkinnedMeshRenderer.bounds.size.y`. For a skinned mesh, Unity derives world bounds by transforming `localBounds` through the **root bone**, and this rig's `Hips` carries a non-yaw rotation, so the axis-aligned result inflates. Measured from the baked mesh vertices instead:

| | Mesh height | `Renderer.bounds` height | Max radius from own axis |
|---|---|---|---|
| `pfb_enemy_grasscutter` (root scale 2,2,2) | **3.400 m** | 4.696 m | 1.820 m |
| `pfb_enemy_spincycle` (root scale 2,2,2) | **4.251 m** | 3.809 m (body) + 1.490 (head), separate renderers | 1.924 m |

So the two bosses are **not** the same height — the Grasscutter is 0.85 m shorter, 80% of SpinCycle. B119's 2× root scale-up is not reverted here (the owner's complaint was "too small," and 3.40 m is a large boss), but the claim that it now matches SpinCycle is withdrawn, and whether it *should* match is a creative call recorded as `docs/BACKLOG.md` B123. **Every framing number in §5 is derived from the measured 3.400 m**, not from 4.70.

**(b) Phase C aims at the feet.** `FrameIntroCamera(standPos)` aims at `transform.position` — y = 0. With the camera at y = 1.8 the frame is pitched down, so the subject's own height is spent below frame. `SpinCycleAI` does **not** have this bug: it carries `_introCamLookHeight = 2.0f` and `PositionIntroCamera` sets `lookAt.y = _introCamLookHeight`. `GrasscutterAI` reused SpinCycle's cadence and dropped the field. So the fix is not new architecture — it is restoring the World 1 pattern that was lost in the port.

### Fact 5 — the Edit-Mode prop sweep measured a state the runtime never produces

*(Unchanged, and re-verified: `ZoneDirector._clearOnBossZone` was read from the serialized array and contains exactly `CourtMakiwara_A/B/C/D` and `CourtLantern_E/F`.)*

The review that rejected the second attempt found the camera wedging near `CourtMakiwara_A/B`. Those props are `SetActive(false)` in `HandleZoneActivated` **before** the boss activates — order the code calls out as load-bearing, citing World 1's wagons beside SpinCycle's dolly path. At intro time the Blossom Court's only interior obstacle is the tree. This is the same class of error ADR-0006 Fact 6 named: a metric evaluated against a configuration the runtime cannot reach.

Quantified this time: with the six props **excluded** from the sweep, the nearest `Building` hit from the chosen anchor is 7.493 m. Including them changes that figure by **0.000 m** at the chosen anchor — but that is luck, not a reason to measure the wrong state.

### Fact 6 — the trigger radius and the camera distance were on a collision course

`_introTriggerRange = 9` fires the intro the moment the player is 9 m from the boss, while the camera sat 14 m away. The player therefore stood, at trigger time, *closer to the boss than the camera was* — so some approach bearing always put the player inside or directly in front of the shot. That is an unforced coupling, and it is why the shot's correctness has kept depending on which way the player walked in.

### Fact 7 — a 0.6 m burial does not hide a 3.40 m boss (now a 4.250 m one), and there is no grass to hide it either

*(Amendment 2: at the rescaled boss the exposed height is **3.65 m of 4.250 m**, so this fact is strengthened, not weakened.)*

`_buriedYOffset = -0.6`. During Phase A and Phase B the boss therefore stands with **2.80 m of its 3.40 m body above ground, in plain sight**, for the entire approach and the entire tree shot. The GDD's *"dormant in the tall grass … and it rises"* is not what the scene does, and the tall grass that was supposed to cover the difference **does not exist as geometry** (no scene object's name contains "grass"). §4's invariant I3 is the response.

---

## Decision

### §1 The tree stays where the owner put it, and is treated as a fixed constraint

`CherryTree_TrunkCollider` and `CherryTree_BlossomCourt` **do not move**. Design `(0, ·, 62.530)` is CANON by explicit owner decision (see *Amendment*). No task may re-centre them without a new owner decision.

Everything downstream is derived against the tree's **measured** geometry (Fact 0/Fact 1), not against ADR-0006 §1.1's envelope, which the built asset does not satisfy on canopy radius or canopy underside. That envelope mismatch is recorded as `docs/BACKLOG.md` B126 for the art pipeline; it is not a blocker for this shot, because this shot was solved against the asset as built.

### §2 The boss is staged at the arena's north-west, design `(−6.10, 0, 57.50)` = world `(36.3453, 0, 44.9720)`

> **Superseded by Amendment 2.** The Amendment-1 value was design `(−6.5, 0, 58.0)` = world `(36.416, 0, 45.608)`, derived for a 3.40 m boss. At the 4.250 m boss that point yields only **0.652 m** of wall clearance past the blade tips, so it is withdrawn. The reasoning below is unchanged; only the point moved, and it moved *inward*, not sideways.

The GDD's *"far end"* is the north, and the tree owns the north centre. The boss goes to the north-**west** pocket: r = **6.60 m** of the 10.0 m radius — the far half, diametrically clear of the gate, and clear of the canopy.

Why this point and not another:

- **Wall.** Live 3600-ray `Building` sweep from the boss position (six `_clearOnBossZone` props and the trunk capsule excluded), run at y = 0.5/1.0/2.0: nearest wall **3.5295 m** at world bearing 326.2°. Against the measured **2.275 m** silhouette radius that is **1.254 m** of blade clearance; against `NavMeshAgent.radius = 1` it is 2.530 m. *(Above y = 2.4 the sweep finds nothing — the dojo walls are only 2.4 m tall, so a 4.25 m boss now stands head-and-shoulders above them. That is a silhouette gain, not a clearance problem, since the intro camera is interior.)*
- **Tree.** Plan distance from the trunk axis **8.070 m**, against a required `silhouette + canopy = 2.275 + 4.503 = 6.778 m`, i.e. **1.292 m** of margin with the boss's full 4.25 m height clear of foliage.
- **Far end.** r = 6.60 m, and the boss stands **13.9 m** from the gate mouth. The north *axis* remains unavailable: with the canopy's southern edge at design z = 59.26, a boss on x ≈ 0 is swallowed before it clears the arena centre — which is what Fact 3 rejects.
- **Why 6.60 and not further out.** The wall-clearance/far-end trade was enumerated rather than guessed. Requiring ≥ 0.75/1.00/1.25/1.50 m of blade clearance caps r at 7.10/6.85/6.60/6.32 respectively; ≥ 1.75 m is **infeasible** at any position satisfying the tree and anchor constraints. **1.25 m was chosen**: it costs only 0.25 m of r against the 1.00 m option, and this is a boss that spin-dashes and is knocked around, so blade tips 1 m from a wall is thin.

**Rotation is kept**: the existing world yaw 225° (= design 180°) faces the gate, and from the chosen anchor the boss's forward vector sits **45°** off the view axis — a three-quarter front view rather than a profile.

**Why the boss cannot go east.** The mirrored position design `(+6.10, 57.50)` puts its anchor (which must sit due world-−Z of the boss, §3) at design `(11.69, 51.92)` — **12.09 m from the arena centre, outside the 10 m wall**, violating I1 outright. The west pocket is not a preference; it is the only side on which the world-+Z camera axis stays inside the arena.

### §3 The intro vantage is an authored `Transform`, not a runtime computation

`GrasscutterAI` gains `[SerializeField] private Transform _introCamAnchor`. `ComputeIntroCamPosition()` returns `_introCamAnchor.position` when assigned; the existing player-axis computation is **retained as a fallback** (so the prefab still runs in a scene with no anchor) and both `OnValidate` and `Awake` warn when the anchor is null, per the `unity-csharp` early-validation rule.

Only the anchor's **position** is read. Rotation is still derived per phase from the look point, so the "one vantage, two look targets" structure is preserved — what changes is that the vantage is data, not a formula.

**Blossom Court anchor: world `(36.3453, 1.800, 37.0720)` = design `(−0.5139, 1.80, 51.9139)`.**

> **Superseded by Amendment 2.** The Amendment-1 anchor was world `(36.416, 1.800, 39.300)` at 6.309 m from the boss. The taller subject requires **7.900 m** to hold the fill band (§5), so the anchor retreats along the same world-+Z axis.

The anchor is chosen so the camera→boss axis lies along **world +Z** — measured world yaw **0.00°**, identical to `pfb_CM_FollowCam`'s locked yaw. Combined with §5's FoV of 45 (also identical), the Phase D handoff to gameplay becomes a pure change of pitch and position: **zero yaw rotation, zero focal-length change.** ADR-0001 §2.6 warns about that pop; this removes two of its three components rather than shrinking one. That is a derived property of the anchor, not a coincidence — the boss's bearing was picked from the gameplay camera's, and the west pocket is the side where the two agree.

**Why authored beats computed here.** The simpler alternative — keep the computation and re-anchor it on the arena centre instead of the player — was considered and is genuinely close: it also guarantees an interior camera. It was rejected because it still hides the shot inside arithmetic no one can look at, it needs an arena-centre reference the boss does not currently have, it cannot express a lateral offset, and it cannot be aimed at the gameplay camera's world yaw, which is where most of this staging's value now sits. An authored Transform is inspectable in the Scene view, matches the `_cherryTreeLookTarget` pattern already in this class, and is the project's stated preference for data-driven configuration over magic numbers. It is also *less* code than either computation.

### §4 Three invariants, each of which closes a failure class by proof rather than by sweep

> **I1 (walls).** The intro camera and every look target must be **interior points of the arena polygon**. The arena is convex, so no segment between interior points can cross the boundary — wall clipping becomes impossible by construction, at every approach bearing, with no raycast required.
>
> **I2 (the player).** *Restated in Amendment 2 — the Amendment-1 form is unsafe.* All four quantities must be compared **horizontally**:
>
> `activation_h  >  √(tickOver² − b²)  >  √(trigger² − b²)  >  distance(anchor, boss)`  where `b = |_buriedYOffset|`
>
> Every point that can occlude the boss on screen lies inside the cone from the camera through the boss's silhouette, and every point in that cone is within `distance(anchor, boss)` of the boss. The intro fires only while the player is within `_introTriggerRange` of the boss. If the trigger radius is the larger, the player is provably outside the occluding set at trigger time — behind or beside the camera — for **every** approach bearing.
>
> **The burial term is not optional.** `BossIntro` assigns `transform.position = buriedPos` *before* the Phase A wait loop, and that loop compares `Vector3.Distance(transform.position, _player.position)` — a **3-D** distance from a boss that is `b` metres underground to a player whose pivot is at its feet (`pfb_player`'s `CharacterController` is height 1.8, centre y 0.9, pivot y 0). The serialized `_introTriggerRange` is therefore a **slant range**, and the radius that actually matters on the floor is `√(trigger² − b²)`. Deepening the burial to satisfy I3 *tightens* I2. The two invariants are coupled, and nothing in the field names says so.
>
> **`activation_h`** is the distance from the boss to the nearest point of `RoomTrigger_Zone2` **that the player can touch first**, not to the trigger's centre. The trigger is a 9.8 × 3.0 m box (design x −4.90…4.90, z 45.50…48.50), and first contact is provably its **south face**: there is no navmesh inside the arena polygon west of design x = −4.90 or east of x = +4.90 at z < 45.50 (**0 points** on either side, swept at 0.05 m), so the player cannot flank around to a nearer face. Measured `activation_h` = **12.060 m**. *(Amendment 1's 12.777 m was measured to the box's centre and was too generous; the box's absolute nearest corner is 9.080 m, but it sits behind the activation surface and cannot be reached first.)*
>
> **I3 (the reveal).** During Phase A and Phase B the boss must be **entirely below the ground plane** (`_buriedYOffset ≤ −(mesh height)`), so it cannot appear in the tree shot. At the 4.250 m boss this means `_buriedYOffset ≤ −4.25`; the authored value is **−4.45**, keeping the 0.20 m margin the Amendment-1 staging used.
>
> The ground is an opaque plane at y = 0 covering design X −11.2…13.0 × Z −4.0…66.0, and the camera is above it, so a subject wholly below y = 0 is occluded from every interior vantage at every aspect ratio. This makes "the boss is not spoiled in the tree shot" a property of the staging rather than an angular margin that has to be re-measured per device. It is also what the GDD already asks for: *"dormant in the tall grass … and it rises."*

**Why I3 is new, and why it is the right shape of fix.** The first version of this ADR enforced the same goal with a *horizontal angular separation* between the two subjects (46.3° against a 34.3° half-frame at 16:9). With the tree pinned at the north rim, that constraint becomes both marginal and aspect-dependent: the best staging that satisfies everything else clears the frame edge by only **0.1° at 20:9**, and modern phones are 19.5:9–21:9. Chasing a 2° margin across an unknown device population is exactly the "correctness depends on a sampled configuration" failure mode Fact 5 names. Burying the subject removes the question.

Verified for the chosen staging — **all figures re-measured live this session at the 4.250 m boss** (Amendment 2). The Amendment-1 column is kept so the trail is legible.

| Check | Amendment 1 (3.40 m boss) | **Current (4.250 m boss)** |
|---|---|---|
| **I1** — anchor radius from arena centre | 2.509 m of 10.000 | **3.129 m of 10.000** |
| **I1** — live 3600-ray `Building` sweep from the anchor, six props excluded | 7.493 m | **7.1485 m** |
| **I1** — boss position, live 3600-ray `Building` sweep | 2.929 m (blades +1.149) | **3.5295 m (blades +1.254, agent radius +2.530)** |
| Boss-shot occlusion: **448** silhouette rays vs `Building` physics **and** exact ray-triangle over all 3 032 canopy triangles | 0 blocked (voxelised) | **0 blocked** |
| Min distance from tree geometry to the camera→boss segment | 4.097 m | **4.2685 m** |
| Anchor → tree look point, `Building` linecast | no hits | **no hits** |
| **I2** — horizontal chain (see the restated I2 above) | *(not evaluated in this form)* | **12.060 (activation) > 10.604 (tickOver_h) > 8.506 (trigger_h) > 7.900 (anchor→boss)** |
| **I2** — serialized 3-D values as the code compares them | 11.0 > 8.5 | **11.5 > 9.6** |
| **I2** — 7 200 bearings × 9 body heights on the trigger circle, clipped to the **baked NavMesh** *and* the arena polygon, with a `Building` line-of-sight test | 0 in frame, 0 occluding | **0 in frustum, 0 visible, 0 occluding — at 4:3, 16:9, 20:9 and 21:9** |
| **I2** — closest a reachable trigger position comes to the anchor | 2.191 m at −2.188 along-track | **0.606 m at −0.681 m along-track → behind the camera** |
| **I3** — boss top during Phase B | y = −0.20 m | **y = −0.20 m** (buried −4.45, height 4.250) |
| **I3** — `Ground` covers the boss's XZ | — | **yes** (plane at y = 0 spanning world X −10.75…55.86, Z −12.02…54.59) |

**A note on what "clipped to the arena" is doing, because it is load-bearing.** Without that clip the same sweep reports **7 344 in-frame body samples**. Every one of them is *beyond* the boss (**0 in front**), on navmesh that lies **outside the 2.4 m dojo wall** — the `Ground` plane is 66.6 × 66.6 m and the court walls are just short obstacles standing on it, so the bake legitimately produces walkable floor all round the outside. Those positions are unreachable during the fight and are additionally wall-occluded. The clip is not a convenience; omitting it turns a clean result into a false alarm, and quoting the unclipped number would be the same class of error as Fact 5.

### §5 Framing is set by two look heights and one FoV, restoring the World 1 pattern

`_introBossLookHeight = 2.13f` (50.1% of the **measured** 4.250 m — was 1.70 for the 3.40 m boss) and `_introTreeLookHeight = 4.50f`, both absolute above the court floor at y = 0. Phase C aims at `standPos + up × _introBossLookHeight`; Phase B aims at the tree's XZ raised to `_introTreeLookHeight`.

**Why 4.50 and not `Renderer.bounds.center.y`.** The original ADR's validation step said to use the tree's `bounds.center.y` if it differed from the assumed 5.5 by more than 0.5 m. It does — `bounds.center.y` is **3.810** — but that is the midpoint of a box whose bottom is the root flare at y ≈ 0 and is *not* where the visual mass is. The triangle-**area-weighted centroid** of the tree's 322 m² of surface sits at y = **4.462**, which is the number to aim at. Rounded to 4.50 for authoring.

**`_introCamFoV` moves 32 → 45**, matching `pfb_CM_FollowCam` and `_normalCameraFoV` exactly (§3). `Lens.FieldOfView` is *vertical*, so this framing is aspect-independent; only the horizontal margins in the table vary by device, and the project's aspect policy (`AspectAdaptiveCameraFraming`, ADR-0001 §2.6) is explicitly to lock vertical FoV and adapt distance, never the reverse.

Re-derived at the 4.250 m boss (Amendment 2). Camera pitch is derived, not authored: **+2.392°** for the boss shot, **+14.253°** for the tree shot.

| | Boss shot | Tree shot |
|---|---|---|
| Distance / vFoV | **7.907 m** / 45° | **10.966 m** / 45° |
| Frame covers | y **−1.092 … 5.466** (6.558 m) | y **0.260 … 9.738** (9.478 m) |
| Subject | boss 0 … 4.250 → **64.8%** of frame height | tree 0 … 7.671 → **80.9%**; foliage 2.0 … 7.671 → 59.9% |
| Headroom / margin | **+1.216 m** (18.5%), ground line visible 1.092 m ahead of the feet | **+2.067 m** of sky above the crown; trunk cropped 0.260 m above its base |
| Horizontal half-frame at 4:3 (tightest) | 4.367 m vs **2.275 m** half-radius → **+2.09 m** | 6.056 m vs 4.503 m canopy radius → **+1.55 m** |

*(Amendment 1's figures, for the trail: boss shot 6.309 m framing 3.40 m at 65.1 %; tree shot 9.227 m.)*

**Reference point:** SpinCycle's playtested reveal fills **63%** of frame height. **64.8%** is the same band, and it is the band `.claude/agent-memory/technical-director/reference_boss_intro_camera.md` records as this project's target. B119's independently-derived 59% used the pre-scale-up boss and the inflated-bounds height, so it is not a corroborating source and is not counted as one here.

**The tree shot got looser and that is accepted, not overlooked.** Because one anchor serves both look targets (§3), retreating for the taller boss also retreats from the tree: the foliage mass now fills 59.9% where it filled 75.5%. The *whole* tree is in frame at 80.9% with 2.07 m of sky, which reads as an establishing shot of a landmark rather than a subject reveal — which is what Phase B is. Alternative 5 (a second anchor) remains the escape hatch if a playtest disagrees; it is not taken pre-emptively.

### §6 The prefab's serialized value is what ships

`pfb_enemy_grasscutter.prefab` carries `_introCamDistance: 14`, and the scene instance reads 14 — confirmed this session by reading the live `SerializedObject`, not the file. Once a serialized field has been written, the prefab's explicit value **shadows the C# default** — the earlier change of `= 14f` to `= 9f` in the script therefore changed nothing that ships. Every value in §2 and §5 must be written into the prefab (or an explicit scene-level `PrefabInstance` override), and the prefab YAML must be re-read afterwards to confirm it carries the final number. Editing the C# default alone is not a fix, and a green compile is not evidence that it took.

**World 1 is re-read against this ADR and not changed.** SpinCycle satisfies I1 by construction (its dolly path is inside the street and `_clearOnBossZone` clears the wagons off it) and already has the look-height field. Its intro is activation-gated rather than proximity-gated, so I2 does not apply to it. I3 does not apply either — SpinCycle walks out of a doorway rather than rising from the ground. No retroactive work.

---

## Consequences

**Good.** All three failure classes close by proof rather than by sampling, so "which way did the player walk in" and "how wide is the player's phone" both stop being variables. The shot becomes inspectable in the Scene view. The GDD's far-end staging is restored *and* its "rises out of the grass" beat becomes real for the first time. The intro FoV and world yaw now match the gameplay camera exactly, so the Phase D handoff pop ADR-0001 warns about loses two of its three components.

**Cost.** One new scene object and one new serialized reference per boss intro that wants an authored shot — a designer must now place a camera anchor rather than getting a shot for free. Accepted: the free shot has cost three review cycles.

**Cost — I3 changes what the player sees during the approach.** With `_buriedYOffset = −4.45` the arena reads as empty until the intro fires. Today, a **3.65 m** mower is visible from the gate (a 4.250 m boss sunk 0.6 m) — the current read is *more* conspicuous than it was, not less, so this change is correspondingly more visible. This is a deliberate staging change in service of the GDD's own text, but it is a change the owner should see in play. It also raises the value of the *"grass and petals kick up"* beat, which has no implementation (`docs/BACKLOG.md` B124) — without it there is no diegetic cue before the cut. **§Alternatives 7 records a staging that needs no burial change, in case the owner prefers the current read.**

**Residual gap, recorded not fixed.** Neither boss intro locks player movement. I2 guarantees the frame is clean **at trigger time**; a player who holds forward through the ~5.7 s intro will walk into the boss shot. This is pre-existing in World 1, shipped, and a `PlayerController` change with project-wide blast radius — so it is `docs/BACKLOG.md` **B120**, not part of this decision.

**Not resolved here.** `Systems/CameraOcclusion.cs` vs `Systems/BuildingOcclusionFader.cs` (ADR-0001 §2.8) stays open. With the tree at the rim rather than the centre, the fader's urgency for zone 2's *gameplay* camera drops — the fight floor's centre is now clear — but the tree still occludes the north third and the fader is still unbuilt. The **intro** camera is unaffected: §4's table shows 4.097 m of clearance from any tree geometry to the camera→boss sightline.

---

## Alternatives considered

**1. Keep the computed vantage, tune `_introCamDistance` again.** Rejected — Fact 2 proves no value exists.

**2. Keep the boss at the arena centre (the current WIP).** Rejected — Fact 3. It buys an interior camera at the price of the GDD's staging, and it was already flagged in B119 as a call this agent had to make rather than a code change.

**3. Re-anchor the computation on the arena centre instead of the player.** Rejected, but it was close — see §3. It satisfies I1 and needs no new scene object. It loses the lateral offset and, decisively, cannot be aimed at the gameplay camera's world yaw.

**4. Add a wall-avoidance raycast clamp to the intro vcam** (B119's option (b), modelled on `CameraOcclusion.cs`). Rejected. It is strictly more code than an anchor, it makes the final shot depend on runtime physics results that cannot be previewed in the Scene view, and it treats the symptom. I1 removes the intent instead of the consequence. It would also have been *wrong* here: the canopy has no collider (Fact 1), so a raycast clamp would have driven the camera straight into 8.8 m of foliage while reporting itself clear.

**5. Two authored vantages, one per phase.** Rejected as unnecessary. One anchor frames both subjects well (§5), and I3 removes the only reason the two shots were fighting each other. Revisit only if a playtest says the tree shot wants a different lens.

**6. Move the tree back to the arena centre** (the original ADR's §1). **Withdrawn — the owner has decided the tree's position.** See *Amendment*.

**7. Keep `_buriedYOffset = −0.6` and solve the tree shot by angular separation instead of I3.** Not rejected — **held as the fallback** if the owner wants the dormant boss visible during the approach. **⚠ Amendment 2: the coordinates below were derived for the 3.40 m boss and are NOT valid at 4.250 m — they must be re-derived before use.** Two things move: the larger silhouette shrinks the angular margins further (they were already device-dependent), and dropping the burial to 0.6 m *relaxes* I2's horizontal budget, which changes the trigger/tick-over values too. The alternative's *shape* still stands; its numbers do not. Recorded as it was:

> Boss design `(−7.0, 0, 57.25)` = world `(35.532, 0, 45.432)`; anchor design `(−2.048, 1.80, 53.519)` = world `(36.396, 1.800, 39.292)`. All other field values as in §5. Verified: I1 anchor clearance 7.473 m, boss wall margin 0.916 m, 0/448 occlusion rays blocked, I2 0-in-frame / 0-occluding at 21:9, boss fill 66.2%, tree fill 75.3%.
>
> Its cost is the margin: the boss clears the tree-shot frame edge by **+17.5° at 4:3, +10.0° at 16:9, +3.8° at 20:9, +2.4° at 21:9**. That is adequate but device-dependent, and it gives up the world-yaw match (boss-shot yaw −8.00° instead of 0.00°). It is a worse shot bought to avoid a staging change, which is why it is the alternative and not the decision.

---

## Validation

Blocking, before this is called done:

1. ~~**Bake the NavMesh.**~~ **DONE 2026-09-01 (Amendment 2).** The blocker was not an unpressed button: `Ground` — the 66.6 × 66.6 m plane that is the scene's *only* walkable surface — carried `m_StaticEditorFlags: 0`, where World 1's `Ground` carries `4294967295`. A bake with it unflagged produces 67.6 m² of wall-tops and koi-pond lids and no floor at all, which is a *silently* wrong bake, not an obvious failure. Fixed by setting the `NavigationStatic` bit only (0 → 8) — deliberately **not** copying World 1's "Everything", because the GI/batching/occlusion bits have separate rendering consequences that are not this task's to decide. Re-baked with `UnityEditor.AI.NavMeshBuilder.BuildNavMesh()` (the legacy Navigation-window path, matching World 1's `Scenes/<Scene>/NavMesh.asset` mechanism). Result: **1 139 verts / 485 tris / 1 216.1 m²**, asset at `Assets/_Project/Scenes/Backyard_Dojo/NavMesh.asset`. Confirmed `NavMesh.SamplePosition` succeeds at the boss's new position (0.074 m) and `NavMesh.CalculatePath` from the gate returns **`PathComplete`**. See `docs/BACKLOG.md` B122.
2. **The prefab carries the numbers.** Re-read `pfb_enemy_grasscutter.prefab` YAML after saving and confirm `_introCamFoV: 45`, `_introTriggerRange: 9.6`, `_introTickOverRange: 11.5`, `_introCamDistance: 7.9`, `_introBossLookHeight: 2.13`, `_introTreeLookHeight: 4.5`, `_buriedYOffset: -4.45`. §6. *(The root `m_LocalScale: {2.5, 2.5, 2.5}` is already written and verified on disk.)*
3. **I1 by sweep, not by argument.** Omnidirectional `Building`-layer sweep from the anchor confirming ≥ 7.0 m to the nearest wall, and `Physics.Linecast` from the anchor to both look points — with the six `_clearOnBossZone` props **deactivated**, which is the state the intro actually runs in (Fact 5).
4. **Canopy clearance against the mesh, not the collider.** The canopy has no collider (Fact 1). Any re-verification of the boss shot must voxelise or ray-triangle-test the tree's actual mesh; a `Physics` sweep will report a clean shot through solid foliage.
5. **I2 by sweep — in the restated, horizontal form.** Confirm `activation_h > √(tickOver² − b²) > √(trigger² − b²) > distance(anchor, boss)` from the *serialized* values, **including the `_buriedYOffset` term** — comparing the raw serialized numbers is the trap this amendment exists to close. Then sample the trigger circle at 21:9, clipped to the baked NavMesh **and** the arena polygon, and confirm none lands in frame.
6. **I3 by screenshot.** Enter Play Mode, pause during Phase B, and confirm no part of the boss is visible in the tree shot — at 16:9 **and** at a 20:9 Game View aspect.
7. **The three-quarter read.** `VisualRoot` carries a 180° local yaw relative to the boss root (root world yaw 225°, mesh world yaw 45°). Per `CLAUDE.md`'s model-orientation rule this must not be assumed correct or incorrect: screenshot Phase C and confirm the reel/blade assembly faces the camera and the gate, and record the finding either way.
8. **The victory beat's bloom shot** (`zone-layout-spec.md` §4.5 Shot A) frames the canopy with the tree at its owner-decided position. That shot was derived for a centred tree and has never been re-checked against the built one.
9. **M2 re-measured** by ADR-0006 §Validation 2's method, against the tree's real position and real (elliptical, 4.4 × 2.5 m) footprint. B116's published 9.65 m radial band was computed for a centred 0.35 m trunk and has never been measured against any scene that could produce it (`docs/BACKLOG.md` B121).

Non-blocking, needs a human: does the cut from the tree to the boss read as a deliberate cut rather than a jump? And does the arena feel empty during the approach now that the boss is fully buried (§Consequences)? Those are the two things none of the arithmetic above can answer.
