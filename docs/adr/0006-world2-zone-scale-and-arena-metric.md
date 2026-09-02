# ADR-0006: World 2 zone scale — and the two budget metrics that were measuring the wrong thing

- **Status:** **Accepted (architecture) — 2026-09-01.** Amends two acceptance criteria in [ADR-0005](0005-world2-single-continuous-scene.md) §3/§4 and restates `docs/TECHNICAL_DESIGN.md` §6.4's combat-radius rule project-wide. Implementation of the new dimensions is a follow-up task (`docs/BACKLOG.md` B116, `unity-gameplay-engineer`). This ADR does not by itself authorize a commit, and nothing in the scene is changed by it.
- **Date:** 2026-09-01
- **Amends:** [ADR-0005](0005-world2-single-continuous-scene.md) §3 (two budget rows), §4 (arena contract table and the "must not grow" correction of record), §Open Questions (adds 7).
- **Restates:** `docs/TECHNICAL_DESIGN.md` §6.4 and §6.3 step 4 — the combat-radius metric. **This half is project-wide, not World-2-local**, which is why this is a separate ADR rather than an inline amendment.
- **Supersedes in part:** `docs/v4/levels/World2/backyard-dojo/zone-layout-spec.md` §0, §1.1, §1.2, §1.6, §1.7, §3 (footprint), §3.2 (shed size, `agentClimb` assumption), §4 (all), §5 table, §4.6, §8 Q1. See §6 below for the exact list.
- **Resolves, from `docs/BACKLOG.md` B115:** the shed's spec-vs-prefab size question (§3.2), the engawa NavMesh-connection question routed to `technical-director` (§3.3), and zone 2's 57.7% camera-clearance diagnostic (§Consequences).
- **Related:** [ADR-0001](0001-fixed-low-follow-camera.md) (camera framing is the source of both metrics), [ADR-0004](0004-world1-single-continuous-scene.md) (World 1's measured 8.44 m and the B107 de-cramping precedent), [ADR-0003](0003-attack-telegraph-channel.md) (the telegraph channel this decision leans on).
- **Followed by [ADR-0007](0007-ground-plane-lane-telegraph.md), 2026-09-01:** builds the ground-plane dash-lane telegraph that §1.3 makes a **condition** of the 20.0 m arena, after a code review found `GrasscutterAI` telegraphing the Spin-Dash with a body-anchored tell. §1.3's fallback (shrink the arena and dash) was examined and found to have no legal landing site. §1.3, §Validation 10, and one arithmetic erratum in §2.2 are annotated below.

---

## Context

### The trigger

The owner played the built Stage A World 2 geometry (`Backyard_Dojo.unity`, `docs/BACKLOG.md` B115) on 2026-09-01 and reported, unprompted, that **zone 1 (Garden Gauntlet) and zone 2 (Blossom Court) both feel too small.** Zone 0 was not flagged.

That is live-play evidence against dimensions this project accepted on paper eight hours earlier, and it arrives on top of a conflict the design pass had already flagged and escalated: `zone-layout-spec.md` §4.6 found that ADR-0005 §3's *"boss arena minimum clear circle r ≥ 8.5 m"* and ADR-0005 §4's *"cherry tree at centre"* are literally incompatible, and asked `technical-director` to choose between restating the criterion and moving the tree.

Two separate things are therefore on the table, and they turn out to have the same root cause.

### Fact 1: this exact complaint has a precedent, and the owner's accepted answer to it was large

`docs/BACKLOG.md` **B107**: *"Zone 1 widened 8 m → 20 m (owner-reported: workbenches nearly overlapping, no room for the fight) — fixed and re-verified 2026-08-26."* The same pass lengthened World 1's `Ground` from 47.5 m to 59.5 m and translated the entire north cluster by `(+8.485, 0, +8.485)` (street-local +12 m on Z).

So World 1's zone 1 was **widened 2.5× and the world lengthened 12 m** in response to the identical report, and the result playtested well enough that the owner then de-prioritized a separate workbench bug as *"not a concern after the board was lengthened/Zone 1 de-cramped"* (`docs/BACKLOG.md` B104).

**This is the single most useful calibration input available**, and it is far better evidence than any derivation from the camera: it is a dimension this owner played and accepted. World 1's playtested zone 1 is **20 m wide**. World 2's is 16.5 m. The complaint is not surprising; it is the same complaint, and the answer should be at least as generous.

### Fact 2: zone 1's problem is not its total area — it is that one of its two sub-spaces has ~50 m² of usable floor

Zone 0 is the control. It was **not** flagged, and it is the leanest zone in the yard: 15.0 × 20.0 = 300 m² gross, essentially unobstructed (11 collider-less stepping stones, 4 lanterns, 2 makiwara, 1 rack ≈ 6 m² of footprint) → **~294 m² of free floor** for 4 concurrent enemies plus the player.

Zone 1 gross is *larger* than zone 0 — 16.5 × 22.0 = 363 m² — so "total area" does not explain the complaint. Free floor:

| | Gross | Minus | Free |
|---|---|---|---|
| Zone 1 whole | 363.0 | shed 24.0, pond 30.0, engawa agent-erosion 8.0, posts/lanterns/chair ≈ 5.2 | **295.8 m²** |

**295.8 against zone 0's 294 — statistically identical.** The aggregate is a dead end. The zone is two sub-spaces, and splitting it is what shows the problem:

| Sub-space | Extent | Gross | Free | Role |
|---|---|---|---|---|
| 1A Gravel Lanes | Z 17.0–30.0 | 214.5 m² | ~209 m² | gnome + Lurker fight, channelled by 6 makiwara into ~2.5 m gates |
| 1B Koi Pond / Engawa | Z 30.0–39.0 | 148.5 m² | ~86.5 m² | **the Crane Duelist fight** |

Of 1B's 86.5 m², the engawa contributes a **2.0 m usable band** (3.0 m platform, agent-radius erosion) ≈ 16 m², and the east corridor 4.0 × 8.5 ≈ 34 m². **The Crane Duelist — a duellist-class enemy that strafes to stay facing the player — is fought in roughly 50 m² of usable floor, against zone 0's 294 m² for four gnomes.**

That is the complaint, located and quantified. It is a *fragmentation* problem, not a footprint problem, and it correctly predicts that a naive uniform scale-up would be the wrong fix.

### Fact 3: zone 2's problem is the boss's authored dash, not the arena's dimensions in isolation

Zone 2 is 227 m² (r = 8.5) against World 1's playtested arena at 224 m² (r = 8.44). Nearly identical. So why does one feel adequate and the other cramped?

Because the boss is different. The ratio that matters is **longest committed traversal move ÷ arena diameter** — how much of the fight space the boss erases per commitment:

| | Move | Arena across | Ratio |
|---|---|---|---|
| World 1, SpinCycle, playtested | spin charge `spinChargeSpeed 8 × spinChargeDuration 0.6` = **4.8 m** (ADR-0004 §2) | 16.88 m | **0.284** |
| World 2, Grasscutter, as budgeted | Phase-2 Spin-Dash **≤ 8 m** (ADR-0005 §4) | 17.0 m | **0.471** |

**The Grasscutter's dash is 1.67× SpinCycle's in an arena of the same size.** Nearly half the court per dash means the player dodges 2–3 m laterally and the next dash re-covers the space immediately — there is nowhere to dodge *to*. That is a small-arena feeling produced by the boss's envelope, not by the arena's metres.

And the 8 m figure was never derived. ADR-0005 §4 introduced it as slack: *"SpinCycle's charge is 4.8 m; 8 m is generous."* Generous against nothing in particular. `GrasscutterAI` does not exist, so tightening it costs zero rework — this is the cheapest lever in the whole decision and it should have been the first one questioned.

### Fact 4: "17.0 m is already at the camera's limit" is a coincidence, not a derivation — and the limit was already exceeded

`zone-layout-spec.md` §4.1 says: *"This arena is 17.0 m across, matching the camera's 16.8 m visible lateral width… **The arena must not grow.**"* ADR-0005 §4 says the same. This is the load-bearing reason the spec's recommended fix was a pure redefinition with **no dimension change**, and it does not survive inspection.

The camera (ADR-0001) is a **follower** at `(0, 5.5, −7.57)`, yaw 0. W = 16.8 m is the visible lateral width **at the player's depth**, and it travels with the player. Equating a 17.0 m *diameter* with a 16.8 m *visible width* is only meaningful if the player stands at the arena centre. The player does not: an 8 m dash drives them to the rim.

Measured against what the frustum actually shows — F = 15.3 m ahead, R = 4.2 m behind, ±8.4 m lateral:

| Player at | Boss at | Separation | On screen? |
|---|---|---|---|
| south rim | north rim, r = 8.5 | 17.0 m | **No** — 1.7 m past F = 15.3 m |
| west rim | east rim, r = 8.5 | 17.0 m | **No** — 0.2 m past ±8.4 m |

**The current 17.0 m arena is already past rim-to-rim visibility.** The spec half-notices this in §4.4 and specifies the correct mitigation — *"Dash telegraph is a ground-plane lane, not a body pose… a wind-up read from the boss's body is unfair at that separation; a ground lane is readable from anywhere"* — routed through ADR-0003's channel.

That mitigation is the important part. **Once far-rim readability is carried by a ground-plane telegraph rather than by both combatants being in frame simultaneously, arena radius stops being camera-bound.** It becomes bound by how far the boss can go off-frame before the fight stops reading at all, which is a graded cost, not a wall. "Must not grow" was resting on a number that had already been passed.

### Fact 5: the retired clear-circle metric is not strict — it is a different measurement

`zone-layout-spec.md` §4.6's algebra is correct and worth restating because it generalizes past World 2. For an arena of radius `R` with a central obstacle of radius `r_t`, the largest inscribed obstacle-free circle is `(R − r_t) / 2`. So "minimum clear circle ≥ 8.5 m" demands `R ≥ 2 × 8.5 + r_t = 17.35 m` — **a 34.7 m arena to accommodate a 0.70 m tree trunk.** ADR-0005 §4 explicitly rejected 36 m as failing the camera by 2×.

The metric is therefore not a strict version of the right rule. It is unsatisfiable by **any** arena with a central feature of any size, and it coincides with a sensible rule only when the arena is empty — which is the condition World 1's 8.44 m happened to be measured under, with both wagons cleared. It was generalized from one measurement into a requirement without anyone noticing that the measurement's precondition was "nothing in the middle."

### Fact 6: the ≤ 9 m combat-radius rule is also being measured against a state the runtime cannot produce

Verified in `Systems/RoomManager.cs`: `TrySpawnNext` (`:359-411`) advances a **monotonic** `_nextSpawnPointIndex`, and `OnSpawnedEnemyDied` (`:418-424`) refills exactly **one** slot per death. Therefore the live enemy set at any instant is exactly a **contiguous window of at most `maxConcurrentEnemies` entries in array order** — never the whole roster.

`zone-layout-spec.md` §3.4 records zone 1's whole-roster enclosing circle as **8.96 m against a 9.0 m budget** and calls it *"the tightest number in the spec"*, adding that *"the 8.96 m figure is the whole roster, a state that never occurs — but it is the number TDD §6.4 asks for."* The spec is right on both counts, and the consequence is that **a false constraint with 0.04 m of headroom is currently the thing forbidding zone 1 from growing.**

Computed properly, over every 4-wide window of zone 1's 7 spawns, the worst window is **7.79 m** (window `{2,3,4,5}`, limited by the diagonal from the Lurker at `(−1.8, 19.5)` to the corridor gnome at `(7.5, 32.0)`). There is 1.21 m of genuine headroom, not 0.04 m.

The rule also cannot mean what it literally says at the zone level: no zone longer than roughly 18 m can satisfy a whole-roster 9 m circle, and **World 1's shipped, playtested 59.5 m street does not satisfy it** for any reading that spans a zone. It has been quietly unenforced rather than met.

Finally, the 9 m figure's own derivation (TDD §2.4) is about **approach**: `(R − 1.5)/3 ≥ 0.6`, i.e. the player must see a *closing* grunt's entire wind-up before it reaches attack range. An enemy that never closes cannot violate that. The Crane Duelist is canon-stationary — *"It does not chase. Chasing is for things that haven't decided what they are"* (`docs/story/enemies/crane-duelist.md`) — and `zone-layout-spec.md` §3.4 makes that behaviour load-bearing for the Skeptic beat. A held position is a landmark the player chooses to approach, not an off-screen ambush.

### Fact 7: the build report already measured three things that make zone 1 worse than the design pass thought

`docs/BACKLOG.md` B115 recorded these during the Stage A build. All three were verified against the built scene and none of them were available to `zone-layout-spec.md`.

**(a) The shed is roughly twice its spec envelope.** Measured at **~7.4 × 9.5 × 6.4 m** after the orientation fix, against the spec's assumed 3.0 × 8.0 × 4.2 m. B115 left it at native scale rather than non-uniformly rescaling a detailed building mesh, and flagged "spec table or prefab authoritative?" to `art-director`. This makes Fact 2's arithmetic optimistic:

| Zone 1 free floor | With the spec's 3.0 × 8.0 shed | **With the measured 7.4 × 9.5 shed** |
|---|---|---|
| Gross 363 m², minus shed / pond 30 / engawa erosion 8 / props 5.2 | ~295.8 m² | **~249.5 m²** |
| vs zone 0's accepted ~294 m² | parity | **−15%** |

So zone 1 is not merely fragmented, it is also **genuinely smaller in free floor than the zone the owner did not complain about.** The complaint is better founded than the design-pass numbers suggested.

**(b) The engawa does not connect to the NavMesh.** `NavMesh.CalculatePath` onto the boards returns `PathPartial`. The spec's §3.2 derived the 0.35 m board height from an assumed `agentClimb = 0.75`; the project's actual `NavMeshSettings` is **`agentClimb: 0.40`** with `cellSize: 0.16667`, which is nominally above 0.35 m but inside the voxel-quantization margin. B115 explicitly routed this to `technical-director` as a cross-cutting NavMesh-bake setting. Resolved in §3.3.

**(c) Zone 2's camera-clearance diagnostic is 57.7%, not "a small cluster."** Measured per zone: 21.2% / 28.8% / **57.7%** "behind" violations, against World 1's shipped 8%. B115's explanation is correct and is a scale argument: zone 2 is only 17 m deep and carries two `Building`-layer occluders casting 8 m diagnostic shadows from opposite ends — the shed's north face at the seam and the tree trunk across the arena. A 17 m-deep zone cannot absorb two of those.

This is the third independent indicator pointing at zone 2's depth, and unlike the others it was already recorded as a measurement before anyone played the level.

### What is not a factor

- **Zone 0.** Not flagged, and it is this decision's calibration point. Untouched (§2.4).
- **Total scene triangles.** The layout estimate is ~117k against a 300k budget (`zone-layout-spec.md` §5.2). Growth is affordable on this axis by a wide margin; it is not what constrains the answer.
- **Texture memory.** Nothing here adds a texture. World 1 measured 41.2 MB of a 150 MB budget (B112).

---

## Decision

### §1 Zone 2 — Blossom Court grows to r = 10.0 m, the tree stays at centre, and the boss's dash is cut

Of the three options the design pass put forward, **(a) redefine only** is rejected, **(c) move the tree** is rejected, and **(b) enlarge** is accepted — but only together with a corrected metric and a tightened boss envelope, because enlargement alone would not fix Fact 3.

| | Old | **New** |
|---|---|---|
| Plan form | regular 16-gon | unchanged |
| Walkable radius (apothem, wall inner face) | 8.5 m | **10.0 m** |
| Across | 17.0 m | **20.0 m** |
| Centre | `(0, 0, 47.5)` | **`(0, 0, 55.0)`** |
| Z span | 39.0 → 56.0 | **45.0 → 65.0** |
| Circumradius / X extent | 8.85 / ±8.85 | **10.196 / ±10.20** |
| Floor area | 227 m² | **314 m²** |
| Side length | 3.38 m (BD-01 X-scaled 0.845) | **3.978 m (BD-01 at native 4.0 m, X-scale 0.995)** |
| Wall modules after the south opening | 14 | **14 — unchanged** |
| South opening (vertex at due south, 2 segments omitted) | 6.76 m | **7.80 m** |
| `ZONE2_BOUNDARY_WIDTH` | 8.8 | **9.8** |

**The enlargement is free on the module count.** A 16-gon at apothem 10.0 has 3.978 m sides, so BD-01 is used at essentially native 4.0 m scale instead of X-scaled to 0.845. Fourteen modules either way. This is the rare case where the bigger geometry is also the cleaner authoring.

**Why 10.0 m and not more.** Ratio-preservation against World 1 would demand more: a 6.5 m dash at World 1's playtested 0.284 wants a 22.9 m court. 20.0 m across is deliberately **short of** what the ratio asks for, with the remainder covered by the ground-plane dash telegraph (§1.3). Three independent checks converge here rather than one being extrapolated:

| Check | At r = 10.0 |
|---|---|
| Free floor vs zone 0's accepted 294 m² | **314 m²** — lands on the calibration point |
| Dash ÷ diameter, with the §1.2 cut | **0.325** vs World 1's playtested 0.284 and the current 0.471 |
| Frustum exceedance, player rim to boss opposite rim | 20.0 m vs F = 15.3 m → **4.7 m off-frame**, up from 1.7 m — same category, not a new one, and telegraph-covered |

**Why not larger:** past ~20 m the off-frame margin stops being recoverable by a telegraph and the fight genuinely loses its opponent. This is the cost of enlargement and it is real; it is accepted, not waved away.

#### §1.1 The cherry tree stays at `(0, 0, 55.0)` — option (c) is rejected, and the reason is a camera, not a preference

> **SUPERSEDED FOR THIS PLACEMENT — 2026-09-01, by explicit owner decision.** The cherry tree is built at design `(0, ·, 62.530)`, the north-rim position this section rejects, and the owner has confirmed that is intentional: *"I moved the tree to the back of the area because it made more sense back there."* Per `.claude/rules/studio-core.md` §Creative decision discipline that is CANON and is not re-litigated. See [ADR-0008](0008-boss-intro-camera-authored-vantage.md) §Amendment.
>
> **The four grounds below are deliberately left intact**, both as a record of what was traded away and because they may still be sound general guidance for arena dressing. What they are no longer is a reason to move this tree. Their live consequences are tracked, not argued: ground 1 (the victory beat's bloom shot) is ADR-0008 §Validation 8; ground 3 (the orbital movement grammar) is `docs/BACKLOG.md` **B125**; ground 4 (the boss's dormancy spot) is resolved by ADR-0008 §2 moving the **boss**, not the tree. Ground 2 (canon) is a story question for `story-room`, not an architecture one.
>
> Two of this section's own numbers were also measured against the built asset on 2026-09-01 and do not hold: the canopy is an **ellipse of half-axes 4.4 × 2.5 m** against the "r ≤ 3.5" in the table below, its underside starts at **y ≈ 1.9** against "≥ 4.0", and **only the trunk capsule has a collider** — the canopy has none, so every physics-based clearance sweep in this ADR's §Validation silently passes through it. See `docs/BACKLOG.md` **B126**.

The design pass offered moving the tree to the north rim as the alternative if the metric amendment were refused. Rejected on four grounds, the first of which is decisive and appears not to have been checked:

1. **It breaks the victory beat's first shot.** `zone-layout-spec.md` §4.5 Shot A frames the bloom from `(0, 3.0, 40.5)` looking north and up at the canopy. With the tree at the north rim, the canopy is ~20 m ahead of a camera whose forward ground reach is F = 15.3 m, and its crown is above the top ray. **The bloom — the zone's emotional payoff and the Imagination Restore trigger — cannot be framed.** A centred tree is 7.5 m from Shot A's new station and frames cleanly.
2. **Canon places it at the centre.** *"The cherry tree at the center of the court is younger than everything around it."*
3. **It is zone 2's shape identity.** `zone-layout-spec.md` §1.3 distinguishes the three zones partly by movement grammar — free / channelled / **orbital**. Orbital requires something to orbit. Without it zone 2 collapses toward zone 0's read, and the standing "no room reuses another room's shape/prop layout" rule (owner memory, ADR-0005 §6.8) is not suspended for World 2.
4. **It collides with the boss's dormancy spot** at the north rim, and it is Phase 1's only cover.

With the enlargement, the trunk's claim on the floor becomes negligible anyway: **0.385 m² of 314 m², or 0.12%.**

Tree constraints, re-derived for r = 10.0 (all from ADR-0001's `tan(36°) = 0.7265`):

| Constraint | Old | **New** | Derivation |
|---|---|---|---|
| Trunk collider (capsule, layer `Building`) | r ≤ 0.35 m | **unchanged** | §2.2's ceiling is 1.0 m diameter; there is no reason to spend the margin, and the dash-chord clearance uses it |
| Canopy underside | ≥ 4.0 m | **unchanged** | camera ray at 4.0 m behind the player is at 3.91 m. Now clear for players ≥ 4.0 m from the trunk = **84%** of the ring (was 87%) |
| Canopy radius | ≤ 3.5 m | **unchanged** | keeps the intro walk target outside the canopy footprint. **Do not grow the one genuine residual occluder** while the two-occlusion-system defect is open |
| Total height | ≤ 7.0 m | **≤ 8.0 m** | rim occlusion threshold rises from `1.0 + 0.7265 × 8.5 = 7.18` to `1.0 + 0.7265 × 10.0 = 8.27` m |

The taller allowance is a dividend, not slack: a slenderer, higher-crowned tree serves *"too young to be this tall"* and the World Tree breadcrumb better than a 7 m one. **Canopy radius and underside do not move** — `Systems/CameraOcclusion.cs` vs `Systems/BuildingOcclusionFader.cs` is still unresolved (ADR-0001 §2.8) and `zone-layout-spec.md` §4.3 correctly makes a working fader a **prerequisite** for zone 2. That prerequisite stands.

#### §1.2 The Grasscutter's Phase-2 Spin-Dash is cut from ≤ 8 m to ≤ 6.5 m

ADR-0005 §4's boss contract is amended. The 8 m figure was undefended slack (Fact 3); 6.5 m is still 1.35× SpinCycle's playtested 4.8 m, which suits a heavier boss. `GrasscutterAI` does not exist, so **this costs nothing to change now and would cost a re-tune later.**

| Constraint | Old | **New** |
|---|---|---|
| Phase-2 Spin-Dash travel | ≤ 8.0 m | **≤ 6.5 m** |
| Phase-1 AoE / Petal Toss reach | ≤ 4.0 m | **unchanged** (4.0 / 20.0 = 0.20 vs SpinCycle's 3.0 / 16.88 = 0.178) |
| Dash lane perpendicular offset from centre | ≥ 2.5 m | **unchanged** → chord `2 × √(10² − 2.5²)` = **19.36 m**; a 6.5 m dash leaves ~6.4 m of chord at each end |
| Dash landing `NavMesh.SamplePosition`-clamped | required | **unchanged, still not optional** |
| `NavMeshAgent.radius` | 1 | **unchanged** (owner call, B114) |
| Cut-Grass Trail hazards pooled, zero per-frame alloc | required | **unchanged** |

#### §1.3 The ground-plane dash telegraph is promoted from a spec addition to a condition of this decision

`zone-layout-spec.md` §4.4 introduced it as a layout-derived addition. Fact 4 makes it the thing that **replaces** simultaneous on-frame visibility as the fairness mechanism, so it is no longer optional dressing:

> A full-chord ground-plane lane indicator is drawn before the dash commits, routed through ADR-0003's channel. It is authored and verified **before** the arena is accepted at 20.0 m across. The minimap is disabled in boss arenas (World 1 precedent), so this is the only channel available.

If this cannot be built, the arena must come back down and the dash down with it — the enlargement is granted **on** it.

> **Resolved in design 2026-09-01 by [ADR-0007](0007-ground-plane-lane-telegraph.md) — the telegraph is being built, not the fallback.** A `code-reviewer` pass on the newly written `GrasscutterAI` found `SpinDash` telegraphing through the ADR-0003 **overhead billboard** (`AttackTelegraphService.Show(transform, …)`, body-anchored 2.6 m above the boss), not this lane — and further, that the dash's heading is computed *after* the wind-up, so during the whole 0.9 s rev there was no committed lane to draw at all. ADR-0007 adds a world-space ground-lane geometry to the ADR-0003 channel and commits the heading at rev start. Implementation is `docs/BACKLOG.md` **B118**.
>
> **The fallback in the paragraph above was examined and is not available**, which is worth recording because this ADR offered it in good faith. M2.2 with a 0.35 m central trunk requires `R ≥ 8.85 m`; a body-anchored tell readable rim-to-rim requires `2R ≤ F = 15.3 m`, i.e. `R ≤ 7.65 m`. **The two intervals are disjoint by 1.2 m of radius** — there is no arena size at which a body tell is readable rim-to-rim *and* M2 is satisfied, and the pre-B116 r = 8.5 m arena fails M2.2 at 8.15 m regardless. Coming back down would have required revising M2 as well, discarding World 1's playtested 8.44 m radial band. See ADR-0007 Fact 4.
>
> **The condition itself is not yet discharged.** §Validation 10 below still has to pass on device.

#### §1.4 Derived zone-2 positions

Re-derived from the new centre and radius. Court dressing moves out to keep the inner court open; nothing changes in count.

| Element | Old | **New** |
|---|---|---|
| Court dressing radius (4 makiwara + 2 lanterns, `ZoneDirector._clearOnBossZone`) | r = 6.8 | **r = 8.0** |
| Boss dormancy, pre-placed inactive | `(0, 0, 54.5)`, r = 7.0 | **`(0, 0, 63.0)`, r = 8.0** |
| Intro walk target | `(0, 0, 51.5)`, r = 4.0 | **`(0, 0, 59.5)`, r = 4.5** (outside the 3.5 m canopy) |
| Tall grass (BD-04 ×6, collider-less) | X ±3.0, Z 53.0–56.0 | **X ±3.0, Z 60.5–65.0** |
| Loot `scatteredObjects[4]` / `cardboardPiles[2]` | `(±5.0, 0, 41.5)`, r = 7.81 | **`(±5.0, 0, 47.5)`, r = 9.0** — outside `RoomTrigger_Zone2`'s X ±4.9 |
| Victory-beat Shot A | `(0, 3.0, 40.5)` → `(0, 5.0, 47.5)` | **`(0, 3.0, 46.5)` → `(0, 5.5, 55.0)`** |
| Victory-beat Shot B | `(−0.5, 3.6, 43.0)` → chair | **re-derive against the chair's final position; the 9.2 m / ~17° framing is the target, not the coordinates** |

All six court-dressing props are still deactivated at runtime and still need ADR-0005 §6.5's **carving `NavMeshObstacle` + `ignoreFromBuild` `NavMeshModifier`** pairing. Six props, six pairs, no exceptions. The enlargement changes their positions, not that rule.

### §2 The two metrics, restated

These replace the criteria the old numbers were checked against. Both are machine-checkable and both should end up in `LevelBuilder`'s validation rather than in a document (§5.3).

#### §2.1 M1 — Combat radius (replaces `docs/TECHNICAL_DESIGN.md` §6.4's per-zone reading)

> **The concurrency window.** The live enemy set a `RoomManager` zone can produce is exactly a contiguous window of at most `maxConcurrentEnemies` entries in `RoomDataSO.spawnPoints[]`, because `TrySpawnNext` advances a monotonic `_nextSpawnPointIndex` and `OnSpawnedEnemyDied` refills one slot per death (`Systems/RoomManager.cs:359-424`).
>
> **The criterion.** A zone satisfies the combat-radius budget when, for **every** such window, the minimum enclosing circle of that window's **closing** spawns — those whose AI pursues the player — has radius **≤ 9 m**.
>
> **Position-holding spawns are excluded** from the circle: canon-stationary duellists, dormant risers before they rise, pre-placed inactive bosses. Each excluded spawn must sit **≥ 5 m outside** the window's closing circle, so the player meets it as a distinct engagement rather than simultaneously.
>
> **Why.** TDD §2.4 derives the 9 m figure from `R ≥ 3.3 m` — the requirement that the player see a *closing* grunt's entire wind-up before it reaches attack range. An enemy that never closes cannot violate that; the player chooses when to approach it. The whole-roster reading is retired: it describes a state the runtime cannot produce, and no zone longer than ~18 m can satisfy it — World 1's shipped 59.5 m street does not.

**Effect on World 1:** M1 is looser than the retired reading on its first two clauses (any window ⊆ the whole roster; closing spawns ⊆ all spawns), so nothing that passed can newly fail on those. The third clause is new, but World 1's rosters are WagonWheelRoller / SkepticGrunt / GnomeGrunt — all closing classes — and its zone 2 is boss-only, so the clause is vacuous there. **World 1 is not re-validated by this ADR**; re-running M1 over its three `RoomDataSO`s is cheap and worth doing when someone is next in that data.

#### §2.2 M2 — Boss-arena fight floor (replaces ADR-0005 §3/§4's "minimum clear circle r ≥ 8.5 m")

> An arena satisfies the fight-floor budget when all four hold, measured against **visual mesh footprints** — not renderer AABBs and not colliders, both of which mislead on this project (ADR-0004 §0/§2, `.claude/agent-memory/technical-director/reference_measuring_city_scene.md`):
>
> 1. **Outer walkable radius** about the arena centre ≥ the world's authored value, to the wall's inner face. *(World 2: **10.0 m**.)*
> 2. **Radial fight band ≥ 8.5 m** everywhere: the continuous obstruction-free distance from the interior-obstruction envelope to the outer wall.
> 3. **Interior obstruction budget:** total interior obstruction footprint ≤ **2%** of arena floor area, and no single obstruction wider than **1.0 m** in any horizontal direction.
> 4. **Boss traversal ratio:** the boss's longest committed traversal move ≤ **0.35 × arena diameter**.
>
> **Why each.** Clause 2 is the quantity World 1's measured 8.44 m actually described, and it is what the retired "clear circle" was protecting — it inherits the playtested number rather than discarding it. Clause 3 keeps clause 2 honest by capping what may sit in the middle. Clause 4 is Fact 3: the ratio, not the radius, is what made two same-sized arenas feel different, and World 1's playtested 0.284 sets the reference.
>
> **What was wrong with the old metric.** "Largest inscribed obstacle-free circle ≥ 8.5 m" evaluates to `(R − r_t)/2` for a central obstacle, so it demands `R ≥ 17.35 m` — a **34.7 m** arena — to accommodate a 0.70 m trunk. It is not a stricter version of the right rule; it is a different rule that coincides with one only when the arena is empty, which is the condition World 1's 8.44 m was measured under.

**The current built arena fails M2.2**, at `8.5 − 0.35` → **8.15 m against 8.5 m**. *(Erratum corrected 2026-09-01 per [ADR-0007](0007-ground-plane-lane-telegraph.md) §6: this originally read `10.0 − 0.35`, a typo in the first operand — the arena being judged here is the **pre-enlargement** r = 8.5 one. The subtraction, the 8.15 m result, and the conclusion were all correct; only the quoted radius was wrong, and it would have confused anyone reconciling it with the 9.65 m figure two paragraphs below.)* The corrected metric independently says the arena is too small, which corroborates the playtest from a direction that had nothing to do with it. This is also why **option (a) — pure redefinition, no dimension change — was not actually available**: the spec's proposed wording ("outer radius ≥ 8.5 m, no obstruction > 0.8 m wide") passes only because it drops the radial-band requirement entirely, and with it the one number World 1 had actually measured.

**World 1 re-checked against M2:** outer radius 8.44, radial band 8.44 (empty after the wagons clear) ✓, obstruction 0% ✓, ratio 4.8 / 16.88 = 0.284 ≤ 0.35 ✓. **World 1 passes. No retroactive change.**

At r = 10.0, World 2 reads: outer radius **10.0** ✓, radial band **9.65** ✓, obstruction **0.12%** and 0.70 m wide ✓, ratio **0.325** ✓.

### §3 Zone 1 — Garden Gauntlet grows to 20.0 m × 28.0 m, and prop counts do **not** grow with it

| | Old | **New** | Factor |
|---|---|---|---|
| X span | −7.0 … +9.5 | **−8.0 … +12.0** | — |
| Width | 16.5 m | **20.0 m** | ×1.21 |
| Z span | 17.0 … 39.0 | **17.0 … 45.0** | — |
| Depth | 22.0 m | **28.0 m** | ×1.27 |
| Gross area | 363 m² | **560 m²** | ×1.54 |
| Free floor, with the **measured** 7.4 × 9.5 m shed (Fact 7a) | ~249.5 m² | **~446 m²** | ×1.79 |
| Free floor, if the shed is rescaled to its 3.0 × 8.0 m spec envelope | ~296 m² | **~493 m²** | ×1.67 |

Width **20.0 m** is not derived from the camera; it is B107's playtested World 1 figure (Fact 1) and the asymmetric dog-leg shape is preserved and strengthened (west −8.0 for the shed, east +12.0 for the corridor). The 16.8 m visible width is not a wall: World 1's own shipped visual corridor is X −8.8…+10 = **18.8 m** (ADR-0005 §6.2), so 20.0 m is inside precedent, not beyond it.

The split matters more than the totals, because Fact 2 located the problem in 1B:

| Sub-space | Old extent / free | **New extent / free** | Factor |
|---|---|---|---|
| 1A Gravel Lanes | Z 17.0–30.0, ~209 m² | **Z 17.0–32.0, ~295 m²** | ×1.41 |
| 1B Koi Pond / Engawa | Z 30.0–39.0, ~86.5 m² *(spec shed)* / **does not close** *(real shed — see below)* | **Z 32.0–45.0, ~182 m²** | **×2.10** |
| East corridor | 4.0 m wide, ~34 m² | **6.3 m wide, ~82 m²** | ×2.41 |

1A now lands on zone 0's accepted ~294 m². 1B — the Crane Duelist fight — roughly doubles.

**Zone 1B's width is arithmetic, not taste.** Sum the spec's own §3.2 elements across X, using the shed's **measured** 7.4 m (Fact 7a) instead of its assumed 3.0 m:

| Element | Width |
|---|---|
| Shed | **7.4 m** (measured; spec assumed 3.0) |
| Engawa boards | 3.0 m |
| Pond–engawa gap (the dunk) | 0.5 m |
| Koi pond | 6.0 m |
| East corridor | 4.0 m |
| **Required** | **20.9 m** |
| **Available in the built zone** | **16.5 m** |

**The built zone 1 is over-subscribed by 4.4 m.** It cannot hold its own specified elements at the shed's real size — the spec's X budget closes to exactly 16.5 m only because it assumed a 3.0 m shed. Something had to give, and what gave was the fight floor. This is very likely the largest single contributor to "feels too small," and it means the width increase is a **correctness fix** that happens to also be the requested scale-up.

Even 20.0 m does not close with the shed fully inside the yard (20.9 needed). §3.2 resolves that.

1A now lands on zone 0's accepted 294 m² almost exactly. 1B — the Crane Duelist fight — roughly doubles.

**The engawa stays 3.0 m wide (2.0 m usable) and 0.35 m high.** Both numbers are derived and load-bearing (`zone-layout-spec.md` §3.2: agent-radius erosion, and climb 0.75). **The tightrope is the design.** Do not widen the boards to relieve crowding; the relief comes from the apron and corridor around them.

#### §3.1 Prop counts are held. This is half the fix.

The crowding is caused by **movement-blocking props per unit of free floor**, not by prop count as such. So:

| Rule | Value |
|---|---|
| Movement-blocking props in zone 1 (6 makiwara, 2 lanterns, shed, pond, engawa, 1 rack) | **held exactly at current counts** |
| Collider-less floor dressing (stepping stones, leaf-pile mounds, craftsmanship) | may scale with lane length; costs no fight floor |
| Zone 1 total prop count | **≤ 66** (from ~62, allowing ~3 extra stepping stones to span the longer lanes) |
| Resulting blocker density | **1 per 5.9 m² → 1 per 9.0 m²** |
| Prop-count gradient across zones (`zone-layout-spec.md` §1.3) | **34 / ≤66 / 28 — shape preserved** |

Zone 2 holds at 28 props over 314 m² (1 per 11.2 m², from 1 per 8.1). **No prop count anywhere increases to fill the new space.** That is the direct answer to "don't just scale everything up naively": the area grows, the obstruction does not, and the difference is fight floor.

#### §3.2 The shed stays at native scale and straddles the west stockade line

This resolves B115's open question to `art-director` ("spec table or prefab authoritative?") in favour of **the prefab**, without a rescale.

`zone-layout-spec.md` §3.3 already establishes the shed as *"a solid exterior prop the player never enters. Its doorway is a framing device, not a portal."* Only its **east face** is play-relevant. So place it **through** the west boundary rather than inside the yard:

| | Value |
|---|---|
| Footprint | **native 7.4 × 9.5 m, no rescale** — long axis north–south (B115's +90° world-Y correction stands) |
| X extent | **−11.2 … −3.8** — 4.2 m inside the yard, 3.2 m outside |
| East face / doorway plane | **X = −3.8** |
| West stockade | **interrupted for the shed's 9.5 m of Z — the shed body *is* the wall there** |

Zone 1B's X budget then closes exactly:

`4.2 (shed, inside portion) + 3.0 (engawa) + 0.5 (gap) + 6.0 (pond) + 6.3 (east corridor) = 20.0 m` ✓

This is how a shed actually sits in a backyard — against the fence, not in the middle of the lawn — so it is diegetically better than the spec's arrangement, not a compromise. It also **saves 2–3 stockade modules** (the shed replaces that run of wall) and avoids non-uniformly rescaling a detailed building mesh, which was the thing B115 correctly declined to do.

Two consequences to carry:

- **The shed is 6.4 m tall, not 4.2 m.** By the spec's Derived rule A it occludes the player within `(6.4 − 1.0) / 0.7265 =` **7.43 m**, not 4.4 m. It is on the yard's west flank rather than south of the fight, so it occludes laterally rather than along the camera axis — but this is part of zone 1's 28.8% clearance diagnostic and it will not improve. Record it; do not chase it. It also strengthens, not weakens, `zone-layout-spec.md` §1.4's finding that the shed roof cannot appear in the Assembly Beat.
- **The engawa-callback sightline is unaffected by the shed moving west** — it moves *away* from the ray. §3.3's north–south long-axis rule remains binding for the original reason.

#### §3.3 The engawa's NavMesh connection: a local step, not a project-wide `agentClimb` change

B115 (Fact 7b) routed this to `technical-director`. Decision: **build a ramp; do not raise `agentClimb`.**

| Option | Verdict |
|---|---|
| Raise project `NavMeshSettings.agentClimb` 0.40 → 0.75 to match the spec's assumption | **Rejected.** It is a project-wide bake setting. Raising it silently changes enemy traversal in `CulDeSac_WildWestCity` — a shipped, five-times-reviewed scene — letting agents step onto props and ledges nobody has evaluated. Changing a global to fix one platform in one zone is the wrong direction of fix, and it is untestable without re-walking World 1. |
| Lower the engawa below 0.35 m | **Rejected.** `zone-layout-spec.md` §3.2 caps it at ~0.25 m before it stops reading as a veranda, and 0.25 against climb 0.40 at `cellSize 0.16667` is ~1.5 voxels of margin — the same quantization band that already failed. |
| **Add a low ramp/step wedge at the engawa's south step, with a collider, inside the bake** | **Accepted.** Deterministic, local, touches no shipped scene and no global setting. A stepping stone at a veranda's entrance (*kutsunugi-ishi*) is canon-appropriate dressing rather than a hack, and it reads as an invitation onto the boards — which is exactly the traversal the Skeptic beat and the Crane fight both depend on. |
| `NavMeshLink` / off-mesh link | **Rejected for now.** No agent in this project handles off-mesh links, so it would need runtime work, and it gives a teleport-feel traversal where a ramp gives a walk. |

**Acceptance:** `NavMesh.CalculatePath` from each of zone 1's spawn entries and from the zone-1 entry point to a point on the boards returns **`PathComplete`**, not `PathPartial`, on the runtime bake with gates in their real closed state. This joins ADR-0005 §Validation 4.

**Raise `agentClimb` only as a fallback**, and then to **0.50** rather than 0.75 — enough margin over a 0.35 m step, with the smallest project-wide blast radius — and only with a full World 1 zone walkthrough re-verified afterwards, on the same gate as ADR-0005 §Validation 1's `ZoneDirector` rename.

#### §3.4 M1 re-verified against the new footprint — this is what bounds the growth

Zone 1's growth is bounded by M1's beat-gap window, not by area or budget. Worked, with an indicative roster (exact coordinates are the implementer's to derive; these demonstrate the envelope is satisfiable and fix the two constraints that actually bind):

| Idx | Prefab | Indicative position | Closes? |
|---|---|---|---|
| 0 | gnome grunt | `(−5.5, 0, 21.5)` | yes |
| 1 | gnome grunt | `(8.0, 0, 21.5)` | yes |
| 2 | leaf-pile Lurker | `(−2.0, 0, 20.0)` | yes, once risen |
| 3 | gnome grunt | `(1.5, 0, 27.0)` | yes |
| 4 | leaf-pile Lurker | `(5.0, 0, 25.5)` | yes, once risen |
| 5 | gnome grunt | `(9.0, 0, 33.5)` | yes |
| 6 | Crane Duelist | `(−3.0, 0.35, 41.5)` | **no — held, excluded** |

| Window (4-wide) | Closing members | Enclosing radius | vs 9 m |
|---|---|---|---|
| `{0,1,2,3}` | all 4 | **6.75 m** | ✓ |
| `{1,2,3,4}` | all 4 | **5.20 m** | ✓ |
| `{2,3,4,5}` | all 4 | **8.71 m** | ✓ **binding** |
| `{3,4,5,6}` | 3 (Crane excluded) | **~5.0 m**; Crane sits 9.0 m outside it (≥ 5 required) | ✓ |

**Two derived constraints the implementer must respect**, both from the binding window `{2,3,4,5}` — the one straddling the gravel-beat / pond-beat gap, where a surviving gravel straggler can be alive when the corridor gnome spawns:

1. **Spawn 5 (east-corridor gnome) no further north than Z ≈ 33.5 and no further east than X ≈ +9.0.** At `(9.5, 34.0)` the window measures 9.06 m and **fails**. This is a 0.5 m-sensitive constraint; re-compute, do not eyeball.
2. **If the Crane moves, the southern spawns move north with it** — `zone-layout-spec.md` §3.4's existing rule, still binding.

Note the shape of this result: under the retired whole-roster metric zone 1 had 0.04 m of headroom and could not grow at all. Under M1 it grows 54% in area and still lands at 8.71 m of 9.0 m. **The metric was the constraint, not the geometry.**

### §4 Whole-yard consequences

| Element | Old | **New** |
|---|---|---|
| Zone 0 | X ±7.5, Z −3.0…17.0 | **unchanged** |
| Yard Z extent | −3.0 … 56.0 (59.0 m) | **−3.0 … 65.0 (68.0 m)** |
| `Ground` (built-in Plane) | `pos (0, 0, 26.0)`, `scale (2.1, 1, 6.1)` → 21.0 × 61.0 | **`pos (0.9, 0, 31.0)`, `scale (2.42, 1, 7.0)` → 24.2 × 70.0**, X −11.2…+13.0, Z −4.0…+66.0 |
| `ZONE1_BOUNDARY_WIDTH` (Z 17.0 chokepoint) | 10.5 | **unchanged** |
| `ZONE2_BOUNDARY_WIDTH` | 8.8 | **9.8** |
| `RoomGate_Zone1` | Z 39.0, width 8.8 | **Z 45.0, width 9.8** |
| `RoomTrigger_Zone2` | Z 41.0, width 8.8, depth 3 | **Z 47.0, width 9.8, depth 3** |
| `PlayerController._arenaCenter` / `_arenaBoundaryRadius` (backstop only) | `(0, 0, 26.5)` / 30.0 | **`(0.5, 0, 31.0)` / 35.0** |
| Stockade linear metres | ~162.5 m | **~187.4 m** (+15.3%) |

**The dead strips outside the stockade grow** — widest ~4 m, east of the arena, up from ~3 m. `zone-layout-spec.md` §1.2 flagged these as a re-creation of World 1's B99 walkable-outside-the-boundary problem and said the flood-fill must **prove** they are unreachable rather than assume it. That obligation gets stronger, not weaker: the three-configuration flood-fill with the B107 ground-support raycast (ADR-0005 §6.3) is a **blocking** acceptance item. The spec's alternative — trimming `Ground` into per-zone quads instead of one oversized plane — becomes materially more attractive at this size and is now the recommended fallback if the flood-fill finds anything or if NavMesh vertex count comes in high.

### §5 Budget re-check — and how the growth is paid for

#### §5.1 The numbers

| Budget | Target | Old layout | **New layout** | Verdict |
|---|---|---|---|---|
| Triangles, whole yard, peak | < 300k | ~117,000 | **~122,600** | ✓ 41% of budget |
| — of which stockade | — | ~36,570 (162.5 m × 225 tris/m) | **~42,165** (187.4 m) | +5,597 |
| BD-01 module count = worst-case draw calls with no batching | — | 44 (counted in the built scene) | 47 naive → **35 with §5.2** | **−9 vs built** |
| Distinct ENV materials | ≤ 20 | 13 | **13** | ✓ unchanged |
| New unique (non-atlas) ENV tris | < 8,000 | ~4,550 | **~6,350** (adds BD-01-Long at 1,800) | ✓ 1,650 headroom |
| Texture memory, steady state | < 150 MB | — | **unchanged** — no new texture | ✓ |
| Live enemies, peak | ≤ 4 | 4 | **4** | ✓ |
| Combat radius (M1) | ≤ 9 m per window | 7.79 m worst | **8.71 m worst** | ✓ 0.29 m margin |
| Boss-arena radial band (M2.2) | ≥ 8.5 m | **8.15 m ✗** | **9.65 m** | ✓ |
| Boss traversal ratio (M2.4) | ≤ 0.35 | **0.471 ✗** | **0.325** | ✓ |
| NavMesh vertices (est., flat ground) | — | ~1,300 | **~1,700** (+32% ground area) | **measure** |
| Scene-start hitch incl. NavMesh bake | ≤ 500 ms | untested | untested | **measure — §7.5** |
| Draw calls, whole yard, peak | < 100 | not guaranteed by layout | **not guaranteed by layout** | **gated on B112** |

#### §5.2 How the growth is paid for: BD-01 gets a long variant

Enlargement adds ~24.9 m of stockade. Rather than assert that is affordable, it is **refunded**:

> **Author a second BD-01 variant at 8.0 m nominal length (≤ 1,800 tris), same material, for straight runs.** Zone 0's two 20 m side walls and zone 1's two 28 m side walls total 96 m of straight run: **12 × 8.0 m modules instead of 24 × 4.0 m.** Keep the 4.0 m module for the arena's 16-gon and the short returns.

Triangles are neutral (225 tris/m either way). Worst-case draw calls fall from 47 to **35 — nine fewer than the 44 in the built scene today.** Cost: one extra 1,800-tri asset against §5.5's 3,450-tri headroom, leaving 1,650. Non-uniform **X-scale remains permitted** (same mesh, same material — instancing unaffected); **Y-scale remains forbidden**, because 2.4 m wall height is load-bearing in the spec's Derived rule A.

#### §5.3 What is honestly unresolved

**The draw-call budget is not decided by zone dimensions and never was.** The layout has 130+ ENV objects; with the SRP Batcher at zero (B112: `Standard: 204, SRP Batcher: 0, BRG: 0, Standard Instanced: 0`) it exceeds < 100 regardless of how big the zones are. Enlargement moves the worst case by +3 modules before §5.2 and −9 after. **B112's SRP-Batcher-at-zero investigation remains the prerequisite `zone-layout-spec.md` §5.3 says it is** — this ADR does not improve that position and does not claim to.

**Camera clearance improves.** `ValidateCameraClearance` wants ≥ 8 m clear behind every walkable point; more open floor behind the player is strictly better. Vertex count rises ~32% and the violation *fraction* should fall. Record the bucketed counts per ADR-0005 §Validation 5; do not chase them.

### §6 What this means for already-built geometry

`Backyard_Dojo.unity` currently holds Stage A geometry at the old dimensions (B115). **Nothing in this ADR is implemented.** The follow-up is `docs/BACKLOG.md` **B116** for `unity-gameplay-engineer`, and it is a re-layout, not a translation — unlike World 1's B107, where a uniform rigid `(+8.485, 0, +8.485)` translation preserved the north cluster's internal geometry by construction. Here the zone envelopes themselves change, so interior positions must be re-derived.

**A live coordinate-frame conflict the implementer must know about before touching anything.** Verified in the scene file today:

| Object | State |
|---|---|
| `[ENV - Static]` | **rotation yaw 45°**, `m_LocalEulerAnglesHint (0, 0, 0)` — the hint is stale and the Inspector may read 0 while the quaternion is 45° |
| `[Zone Boundaries]`, `[Player]`, `[Managers]`, `[HUD]`, `[Lighting]`, `[Spawned]` | yaw 0 |
| `RoomTrigger_Zone1` | `(13.612, 1, 13.258)` — i.e. design `(0.25, 1, 19.0)` **baked into rotated world coordinates** |
| `RoomTrigger_Zone2` | `(28.99, 1, 28.99)` — design `(0, 1, 41.0)` rotated |
| `RoomGate_Zone1` | `(27.577, 0, 27.577)` — design `(0, 0, 39.0)` rotated |
| `Ground`, `CherryTree_TrunkCollider`, `Stockade` | design coordinates, as children of the rotated root |
| `BD01_WallModule` instances | **44** |

So: **`zone-layout-spec.md` §0 is now false.** It states `[ENV - Static]` is at identity and *"every coordinate in this document is a world coordinate. There is no local↔world transform."* The owner reversed that on 2026-09-01 for visual consistency with World 1, recorded in ADR-0005 §6 item 1's strikethrough. Every number in this ADR and in the spec is an **`[ENV - Static]`-local** coordinate; anything parented outside that root — `RoomGate_*`, `RoomTrigger_*`, `RoomDataSO` spawn points, `WeaponDropTableSO` positions, `PlayerController._arenaCenter`, the victory-beat camera keys — needs the 45° transform applied by hand. ADR-0005 §6 item 1 records that this tax *"materialized exactly as warned"* the first time. It will again.

Sections of `zone-layout-spec.md` superseded by this ADR: **§0** (coordinate frame), **§1.1** (zone table), **§1.2** (`Ground`), **§1.6** (zone-2 boundary width and Z positions), **§1.7** (`_arenaBoundaryRadius`), **§3** footprint header, **§3.2**'s shed dimensions and its `agentClimb = 0.75` assumption, **§4** in full, **§5**'s budget table and the §4.6 finding. Everything else — the encounter design and array-order pacing, the Skeptic beat, §3.3's shed-as-solid-exterior resolution and its north–south long-axis rule, the prop kit, the asset findings in §5.1, the material allocation in §5.4, the new-geometry list in §5.5 — **stands and is not reopened.**

**Three prefab-level gotchas from B115 that will bite the re-layout.** All verified during the Stage A build and all still true:

1. **Seven of the eight dojo ENV prefabs bake a 270° local X rotation at their root**, and `pfb_env_stepping_stone_tile` / `pfb_env_cherry_blossom_tree` additionally bake a **100× uniform scale**. Anything added as a *child* of these inherits the distortion — B115's trunk `CapsuleCollider` landed at world `(0, 0, −152.5)` with 35×35×200 extents before being re-parented as a sibling. Add colliders and `NavMeshObstacle`s as **siblings placed in world space**, or compose the transform properly. Never assume identity.
2. **`pfb_env_koi_pond_basin` renders ~1.9 × 1.9 m at native scale** and was fitted to the spec's 6.0 × 5.0 m by scaling to `(3.175, 1, 2.66)`. It has no baked rotation, so that is a plain per-axis fit — but the numbers change with the pond's new position and must be re-fitted, not copied.
3. **The wall-tiling helper's axis convention.** B115 found and fixed a 90° axis swap: `yaw = Atan2(dir.x, dir.z)` aligns local **+Z** with `dir`, so wall **length must go on local Z** and thickness on X. The first pass had them reversed and every rotated segment — both chokepoint returns and all 14 sides of the arena ring — was thin-side-on, leaving physical bypasses that only the flood-fill caught. The arena ring is being rebuilt at a new radius, so this is live again.

---

## Alternatives considered

**1. Option (a) alone — restate the criterion, change no dimension** (the design pass's recommendation). Rejected. It was the right call on the *metric* and this ADR adopts a corrected form of it, but it cannot be the whole answer for two reasons that did not exist when it was written: the owner has now played the arena and reported it small, and a well-formed metric (M2) shows the current arena **fails** at 8.15 m of a 8.5 m radial band. The spec's proposed wording passes only by dropping the radial-band requirement, which discards the one figure World 1 had actually measured. It also leaves Fact 3 — the 0.471 dash ratio — entirely untouched, and that is what the playtest was most likely reacting to.

**2. Option (c) — move the tree to the north rim.** Rejected; see §1.1. The decisive objection is that the victory beat's bloom shot cannot be framed at 20 m against F = 15.3 m, which appears not to have been checked when the alternative was offered. Canon, the orbital movement grammar, the dormancy-spot collision, and Phase 1's only cover all point the same way.

**3. Enlarge zone 2 without cutting the dash.** Ratio-preservation at 8 m of dash and World 1's playtested 0.284 demands a **28.6 m** arena — worse than the 36 m ADR-0005 §4 rejected, and hopeless against the camera. Cutting an undefended number on a boss that does not exist yet is free; growing the arena to accommodate it is not. Both levers move, and the cheap one moves first.

**4. Scale all three zones by a uniform factor.** Rejected, and the brief for this decision warned against it. Zone 0 was not flagged and is the calibration point; zone 1's problem is fragmentation in one sub-space, not uniform tightness; zone 2's is the boss envelope. A uniform ×1.3 would over-grow 1A, under-grow 1B, spend the stockade budget on zone 0 for no reported benefit, and leave the dash ratio at 0.471.

**5. Fix zone 1 by thinning props alone, with no dimension change.** Genuinely attractive — it is free on every budget and §3.1 adopts half of it. Rejected as sufficient: 1B's ~86.5 m² of usable floor is consumed by the shed, the pond, and the engawa's authored 2.0 m band, none of which are removable dressing. Thinning cannot produce a Crane fight space; only the apron and corridor can. Prop discipline plus enlargement is the answer, and the discipline is what keeps the enlargement from being spent.

**6. Widen the engawa boards to give the Crane room.** Rejected. 3.0 m → 2.0 m usable is derived from agent-radius erosion and is deliberately a tightrope (`zone-layout-spec.md` §3.2). Widening it removes the zone's signature constrained-footing beat, which is the retired Rock Garden's surviving mechanic. Relieve the pressure around the boards, not on them.

**7. Keep the whole-roster combat-radius reading and accept that zone 1 cannot grow.** Rejected. It describes a state `RoomManager` cannot produce (Fact 6), World 1's shipped street does not satisfy it, and a documented-but-unenforced rule with 0.04 m of false headroom blocking a playtest-driven fix is exactly this project's dominant failure mode — the same one ADR-0005 §2 named when it retired a "default" that had no working implementation.

**8. Add a wave/gate affordance to `RoomDataSO` so the two beats cannot overlap.** This would dissolve §3.4's binding window outright and let zone 1 grow further. Rejected as out of scope and premature: `zone-layout-spec.md` §1.5 already found there is no aggro-delay or wave-timer field anywhere in the data layer and that `RoomManager`'s B49 auto-activation makes even a 3-second deferral non-trivial. M1's held-spawn exclusion gets zone 1 to 8.71 m without touching runtime code. If a later world needs longer zones, this is the right thing to build and it deserves its own ADR.

**9. Grow zone 2 past 20 m across.** Ratio-preservation asks for 22.9 m. Rejected: at 20 m the boss is already 4.7 m off-frame at rim-to-rim separation, and past that the telegraph is covering for a fight whose opponent is simply absent. 20 m is the point where the three convergent checks in §1 agree; further is extrapolation from one of them.

---

## Consequences

### Positive

- The Crane Duelist fight floor roughly **doubles** (86.5 → ~198 m²), and the corridor route widens 4.0 → 6.5 m. This is where the complaint actually lived.
- Zone 1A's free floor lands on **zone 0's accepted 294 m²**, so the yard's pacing is calibrated against a zone the owner played and did not flag, rather than against a derivation.
- The boss arena grows 17.0 → 20.0 m across at **zero additional wall modules**, and BD-01 is used at native 4.0 m scale instead of X-scaled 0.845.
- With §5.2's long wall module, worst-case wall draw calls fall to **35, nine below the 44 in the built scene** — the enlargement is net-negative on the budget that is actually at risk.
- Two metrics that were measuring states the runtime cannot produce are replaced with mechanism-accurate, machine-checkable ones, and both **preserve** the playtested numbers (8.44 m radial band, 0.284 traversal ratio) rather than discarding them. World 1 passes both without change.
- The cherry tree can now be **8.0 m** tall instead of 7.0 m, which serves *"too young to be this tall"* better than the constraint it replaces.
- Prop counts are frozen while area grows, so the fix cannot be spent on dressing — the standing failure mode for exactly this kind of change.
- **Zone 1B's X budget actually closes**, which it did not in the built scene: 20.9 m of specified elements were being fitted into 16.5 m (§3, Fact 7a). This was a latent correctness defect, not a taste question, and it is fixed as a side effect.
- **Zone 2's camera-clearance diagnostic should improve materially** from B115's measured 57.7% — the zone gains 3 m of depth and the shed's north face moves ~6 m further from the seam, so the two 8 m diagnostic shadows that were covering a 17 m-deep zone no longer overlap the same floor.
- **B115's two open questions to other disciplines are resolved as a by-product**, both without a rescale or a global change: the shed stays at its native measured size (§3.2) and the engawa gets a local ramp instead of a project-wide `agentClimb` change (§3.3).

### Negative / risks

- **The boss goes further off-frame.** At 20 m across, rim-to-rim separation exceeds F = 15.3 m by 4.7 m, up from 1.7 m. This is a real cost, accepted deliberately, and the ground-plane dash telegraph (§1.3) is the mitigation the enlargement is **granted on**. If the telegraph is not built, the arena and the dash both come back down.
- **The dead strips outside the stockade grow to ~4 m.** The three-configuration flood-fill is now a blocking acceptance item, not a formality, and per-zone `Ground` quads are the recommended fallback.
- **NavMesh bake area grows ~32%** against a 500 ms scene-start hitch budget that has never been measured for this scene. Measured, not assumed — this is precisely the mistake ADR-0004 §8 made about texture memory and ADR-0005 Fact 3 had to correct.
- **The engawa callback sightline must be re-verified, not translated.** The arena centre moves Z 47.5 → 55.0 and the chair moves north with zone 1, so the ray from arena centre to chair changes length (13.7 → ~18.5 m) and angle. It still clears on the indicative numbers (at Z = 45.0 the ray is at X ≈ −1.46, inside the ±3.9 opening), but this is ADR-0005 §Validation 6 and the whole reason ADR-0005 chose a single scene. **`zone-layout-spec.md` §3.3's "shed long axis runs north–south" rule stays binding.**
- **The 45° `[ENV - Static]` rotation tax is paid a second time.** Every boundary object, `RoomDataSO` spawn point, drop-table position, arena-clamp centre, and camera key needs the transform applied by hand (§6). ADR-0005 §6 item 1 already documented this cost as recurring; this is the recurrence.
- **`GrasscutterAI`'s envelope is tightened before the boss is written**, which is the same inverted dependency ADR-0005 §4 accepted knowingly. 6.5 m is defended by ratio, not by playtest. If the boss cannot be authored inside it, the escalation is the AI **or** another look at the arena — with M2.4 as the shared currency, which is the point of having the ratio.
- **The draw-call budget is not improved by this decision**, only not worsened. B112 remains the gate.
- **World 1 is not re-validated against M1**, only argued to be unaffected (§2.1).

### Out of scope / explicitly deferred

- Implementation. B116, `unity-gameplay-engineer`. Nothing in the scene is touched by this ADR.
- Exact interior coordinates for zone 1's props, spawns, pond, engawa, and loot, and zone 2's court dressing. §3.2 fixes the shed and §3.4 the two spawn constraints that bind; the rest is derivation, and it must be re-derived rather than scaled.
- Zone 0's dimensions. Watch item: 1A is now calibrated *to* zone 0's 294 m², so if zone 0 is later reported small, the whole calibration moves and that is a new decision, not an adjustment.
- BD-01-Long as an asset (§5.2) — `asset-engineer` / `art-director`, and gated on the same texture-import-policy prerequisite as BD-01…BD-07 (`zone-layout-spec.md` §5.6, commit `94ad911b`'s `AssetPostprocessor` still disabled by default).
- The dash-lane telegraph's visual design — `ui-ux-designer` / ADR-0003.
- B112's SRP-Batcher-at-zero investigation. Unchanged prerequisite.
- Adding M1 and M2 to `LevelBuilder`'s Editor validation. Both are computable from `RoomDataSO` plus scene geometry and both were violated silently for a full design pass, which is the argument for coding them. Recommended, not scoped here.
- The `RoomDataSO` wave-affordance question (Alternative 8).

---

## Open questions for the owner

None block the follow-up implementation.

1. **Is 20.0 m across the right feel for the Blossom Court?** The three convergent checks in §1 agree on it, but the only real test is the owner walking it. The arena is a 16-gon defined by one apothem value, so re-scaling it is cheap: 10.0 m gives 3.978 m sides (native BD-01), 10.5 m gives 4.177 m. **Build it, play it, and report — do not re-derive it on paper.**
2. **Is the Grasscutter's 6.5 m dash acceptable as a design constraint** before the boss is designed? It is ratio-derived, not playtested, and it is the number most likely to be argued with during boss authoring.
3. **Carried forward from ADR-0005, still unresolved:** `docs/TECHNICAL_DECISIONS.md` lists ADR-0001/0002/0003 as **Proposed** while ADR-0002's own header says **Accepted** and Sprint 0 shipped against 0001/0003. This ADR restates a criterion in TDD §6.4 that traces to ADR-0002 and would prefer to know it is amending an accepted decision. **Third ADR to ask.**
4. **Carried forward:** the 30 FPS cap at `GameManager.cs:101-102` vs TDD §3.1's 60 FPS target (B112). Still affects how the thermal criterion is judged.

---

## Validation before the new dimensions are called done

Additive to ADR-0005 §Validation, which still applies in full.

1. **M1 re-run over all three `RoomDataSO`s** with final coordinates: every contiguous `maxConcurrentEnemies`-wide window's closing-set enclosing circle ≤ 9 m, and every held spawn ≥ 5 m outside its window's circle. Report the worst window per zone with its member indices. **The binding one is zone 1's beat-gap window and it is 0.5 m-sensitive** — compute it, do not eyeball it.
2. **M2 re-measured on the built arena** by the physics-sweep method ADR-0004 §Validation 4 used (fine angular raycast sweep at multiple heights from the arena centre, with the court dressing cleared and `RoomGate_Zone1` open — the real zone-2 state), against **visual mesh footprints**: outer radius ≥ 10.0, radial band ≥ 8.5, obstruction ≤ 2% and ≤ 1.0 m wide. A NavMesh ring-sample is **not** a substitute — ADR-0004 §Validation 4 found it produces a lower on-mesh fraction near gates for reasons unrelated to fight floor.
3. **Free-floor audit** against the §3 and §1 tables: zone 1A ≥ 290 m², zone 1B ≥ 180 m², zone 2 = 314 m², zone 0 unchanged at ~294 m². **Measure against visual mesh footprints and report measured, not designed, values** — Fact 7a is what happens when a design number stands in for a prefab's real size.
4. **Zone 1B's X budget closes at 20.0 m** with the shed at native scale straddling the west line per §3.2: shed-inside 4.2 + engawa 3.0 + gap 0.5 + pond 6.0 + corridor 6.3. Re-measure the shed's real footprint after placement rather than trusting 7.4 × 9.5.
5. **`NavMesh.CalculatePath` returns `PathComplete` onto the engawa boards** from each zone-1 spawn and from the zone-1 entry, on the runtime bake with gates in their real closed state — §3.3's acceptance criterion. `PathPartial` means the ramp did not work and the fallback (`agentClimb` → 0.50, plus a World 1 re-walk) is required.
6. **Prop-count audit:** movement-blocking props in zone 1 unchanged in count; zone 1 total ≤ 66; zone 2 total 28. A count that grew is a regression even if it looks good.
7. **Scene-start hitch measured on device, including the runtime NavMesh bake, against ≤ 500 ms**, with the NavMesh vertex count recorded. First measurement for this scene; the +32% ground area makes it worth having a number rather than an inheritance from World 1.
8. **Three-configuration gate-bypass flood-fill** per ADR-0005 §6.3, with the B107 ground-support raycast **and B115's plausible-ground-height band** (B115 found the first version of that check treated the tops of gates and walls as valid footing), run against the enlarged dead strips: gates closed + boundaries off (must reproduce a bypass), closed + on (must not), open (positive control). **Blocking.** The shed now forms part of the west boundary (§3.2), so it must seal — a 3.2 m-deep prop standing in for 9.5 m of wall is a new bypass surface if its collider does not match its mesh.
9. **The engawa callback re-verified geometrically and on camera** with final coordinates — the ray from the arena centre to the chair clears the Z-45 opening and the shed, and victory-beat Shot B frames the chair, the boards, and the empty doorway. ADR-0005 §Validation 6.
10. **The dash-lane ground telegraph exists and is readable from the far rim** before the arena is accepted at 20.0 m. §1.3 is a condition, not a follow-up. **Still open. Designed 2026-09-01 by [ADR-0007](0007-ground-plane-lane-telegraph.md)** — the shape, timing, width, chord measurement, and escape arithmetic are all fixed there; implementation is `docs/BACKLOG.md` B118. The specific case to test is the one this criterion exists for: **player at the south rim, boss at the north rim, 4.7 m off-frame.** ADR-0007 §Validation 1–10 is additive to this item, not a substitute for it.
11. **`ValidateCameraClearance` re-run** with `_cameraYawDegrees = 0`, violations bucketed per zone, vertex counts recorded, **compared against B115's measured 21.2% / 28.8% / 57.7%**. Zone 2 should improve materially (17 → 20 m of depth, and the shed's north face moves 6 m further from the seam); zone 1 should improve on depth but not on the shed, which is 6.4 m tall rather than the spec's assumed 4.2 m. Record either way; do not chase.
12. **Draw calls and triangles re-profiled** on a representative device against §5.1, reporting the Standard / SRP Batcher / Instanced split explicitly. A figure without its scenario is not evidence.
