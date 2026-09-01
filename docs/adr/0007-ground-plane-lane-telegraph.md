# ADR-0007: A ground-plane lane geometry for the ADR-0003 telegraph channel

- **Status:** **Accepted (architecture) — 2026-09-01.** Extends [ADR-0003](0003-attack-telegraph-channel.md) with a second telegraph *geometry* and satisfies [ADR-0006](0006-world2-zone-scale-and-arena-metric.md) §1.3's condition **in design**. The condition is not discharged until ADR-0006 §Validation 10 passes on device. Implementation is `docs/BACKLOG.md` **B118** (`unity-gameplay-engineer`); the visual treatment is `ui-ux-designer`. This ADR does not authorize a commit and changes no runtime code.
- **Date:** 2026-09-01
- **Extends:** [ADR-0003](0003-attack-telegraph-channel.md) — adds decision 6 (a ground-plane lane geometry) and narrows the scope of its "Explicitly rejected: URP decal projectors" section, which is currently read as banning ground telegraphs outright.
- **Clarifies:** [ADR-0006](0006-world2-zone-scale-and-arena-metric.md) §1.2's "dash lane perpendicular offset from centre ≥ 2.5 m" row (§5 below) and corrects one arithmetic erratum in its §2.2.
- **Trigger:** a `code-reviewer` pass on the newly built `GrasscutterAI` found that the Spin-Dash's implemented telegraph is body-anchored, not the ground-plane lane ADR-0006 §1.3 made a **condition** of the 20.0 m arena. The owner routed the conflict here rather than having it patched silently.
- **Related:** [ADR-0001](0001-fixed-low-follow-camera.md) (the frustum numbers both ADRs rest on), `docs/BACKLOG.md` B116 (the 20 m arena, already built), B117 (`SpinDash`'s endpoint-only NavMesh clamp — adjacent, see §6).

---

## Context

### What ADR-0006 actually required

§1.3, quoted in full, is unambiguous that this is a grant condition and not dressing:

> A full-chord ground-plane lane indicator is drawn before the dash commits, routed through ADR-0003's channel. It is authored and verified **before** the arena is accepted at 20.0 m across. The minimap is disabled in boss arenas (World 1 precedent), so this is the only channel available.
>
> If this cannot be built, the arena must come back down and the dash down with it — the enlargement is granted **on** it.

Repeated as §Validation 10: *"The dash-lane ground telegraph exists and is readable from the far rim before the arena is accepted at 20.0 m. §1.3 is a condition, not a follow-up."*

### What was built

`Enemy/GrasscutterAI.cs:746-748`:

```csharp
private IEnumerator SpinDash()
{
    yield return StartCoroutine(WindUp(Color.red, AttackTelegraphKind.MeleeUnparryable, _waitSpinDashRev));
```

`WindUp` (`:877-881`) raises `AttackTelegraphService.Show(transform, kind, WaitDuration(cachedWait), _telegraphHeightOffset)` — the ADR-0003 overhead billboard, tracking the boss's `Transform` 2.6 m above its root. The code review's finding is correct: this is a body-anchored tell.

Three further facts make the gap larger than "the wrong shape was used."

**Fact 1: the ADR-0003 API cannot express a lane.** `Core/AttackTelegraphService.cs` exposes exactly one entry point, `Show(Transform target, AttackTelegraphKind kind, float duration, float heightOffset)`. It takes no direction, no length, no width, and no world anchor. `AttackTelegraphKind`'s five members — `MeleeParryable`, `MeleeUnparryable`, `AreaUnparryable`, `ProjectileParryable`, `ProjectileUnparryable` — classify *parryability*, not geometry, and `AttackTelegraphIndicator.Activate` maps them onto exactly two meshes (a circle for the two parryable kinds, a triangle for everything else), then billboards to the camera every frame. **There is no ground-plane lane option to select.** No amount of care at the `GrasscutterAI` call site could have produced one; the channel does not have the shape.

**Fact 2: during the wind-up there is no lane to draw.** The dash's direction is computed at `:754-777`, *after* `WindUp` has already returned. For the whole 0.9 s rev the attack has no committed heading: it re-aims to `_player.position` at the instant of commitment. So the line-743 comment — *"Never parryable — dodge perpendicular to the lane"* — describes something the player cannot do. Moving during the rev moves the lane with you. At the player's 5 m/s (`BoxData_Ninja.asset`, `moveSpeed: 5`) against the dash's 13 m/s, a late-aimed dash is not dodgeable by position at all.

This matters for the decision: the ADR-0006 fairness mechanism is not merely a missing visual. **Committing the heading before the telegraph is the load-bearing half**, and a lane indicator is how that commitment becomes visible. Building the indicator without committing the aim would draw an honest-looking lane over a dishonest attack, which is worse than the body tell it replaced.

**Fact 3: the overhead billboard is occlusion-independent but not *frustum*-independent, and elevation makes it worse.** ADR-0003 solved occlusion with `ZTest Always` (`Shaders/TelegraphOverlayUnlit.shader:36`). That does nothing for an indicator that is off-screen. ADR-0001's forward ground reach is F = 15.3 m; the camera looks *down*, so ground points beyond 15.3 m project past the top edge, and a point raised 2.6 m at that distance is higher still — further off-frame, not nearer. For a boss at the far rim of a 20 m arena (4.7 m past F), the overhead indicator is **strictly less visible than the boss's own body.**

### Fact 4: the fallback ADR-0006 named does not reduce to a legal configuration

ADR-0006 §1.3's escape hatch — *"the arena must come back down and the dash down with it"* — is what makes option 2 look available. It is not, and the reason is inside ADR-0006's own metric.

M2 clause 2 requires a **radial fight band ≥ 8.5 m**: the obstruction-free distance from the interior-obstruction envelope to the outer wall. With the trunk collider at the centre at r ≤ 0.35 m (§1.1, unchanged), the band is `R − 0.35`, so:

| Requirement | Bound on arena radius |
|---|---|
| M2.2 radial fight band ≥ 8.5 m, with a 0.35 m central trunk | **R ≥ 8.85 m** (17.7 m across) |
| Rim-to-rim separation on frame, so a body-anchored tell is readable (2R ≤ F = 15.3 m) | **R ≤ 7.65 m** (15.3 m across) |

**The two intervals are disjoint by 1.2 m of radius.** There is no arena size at which a body-anchored telegraph is readable rim-to-rim *and* M2 is satisfied. The smallest arena M2 permits is already 2.4 m past the frustum. Retreating to the pre-B116 r = 8.5 m does not restore fairness either — that arena **fails M2.2 at 8.15 m of 8.5 m**, which is the measurement that corroborated the owner's playtest in the first place.

So option 2 is not a cheaper version of the fix. It is a return to a configuration already measured as failing, and paying for it would require revising M2 as well — discarding World 1's playtested 8.44 m radial band, the one number in this whole decision chain that came from someone actually playing the game. That is a considerably larger act of vandalism than adding a quad.

### Fact 5: the cost of option 2 has risen since ADR-0006 was written

When §1.3 was drafted, the arena was a paper dimension. `docs/BACKLOG.md` **B116 is DONE as of 2026-09-01**: the 16-gon has been rebuilt at r = 10.0 with wall centrelines offset 0.15 m for the inner-face measurement, the arena centre moved to `(0, 0, 55.0)`, and every derived position in §1.4 — court dressing, dormancy spot, intro walk target, tall grass, loot, gates, triggers, `PlayerController._arenaCenter`, victory-beat camera keys — re-derived by hand through the 45° `[ENV - Static]` transform. Two real bugs were found and fixed on the way (a tree mesh 7 m from its own collider; the wall centreline-vs-inner-face error).

Reverting the arena means re-deriving all of that a second time and **paying the 45° coordinate tax a third time** (ADR-0005 §6 item 1 records it materializing twice already). Against that, the telegraph is one quad mesh, one shader property, one material, one pooled component, and roughly twenty lines at the `GrasscutterAI` call site.

### Fact 6: a ground-plane lane is not the thing ADR-0003 rejected

ADR-0003's "Explicitly rejected: URP decal projectors" section is the reason someone might believe ground telegraphs are banned on this project. Read precisely, it rejects **`DecalRendererFeature` and the depth prepass it requires on mobile** — the machinery that conforms a decal to arbitrary geometry. It does not reject the *idea* of a marking on the floor.

The boss arena floor is a **single flat plane at y = 0** (`Ground`, a built-in Plane; ADR-0006 §4). A marking on a flat floor needs no projection: it is a flat quad — two triangles — with the existing unlit telegraph material. No renderer feature, no prepass, no package. `Mobile_Renderer.asset`'s `m_RendererFeatures: []` stays empty.

This distinction should be written down, because the next person to want a ground indicator will read that section and stop.

### What is not a factor

- **Draw calls.** One lane is +1 draw call, 2 triangles, `ZWrite Off`, no shadows, for ~1.4 s per dash. Against B112's measured 204 this is noise. Recorded anyway, per ADR-0003's instruction to budget indicator draw calls explicitly.
- **New packages or assets.** None. One shader property, one material asset, one runtime-built shared mesh.
- **The minimap.** Disabled in boss arenas (World 1 precedent), which is why §1.3 called this the only channel available. Unchanged.

---

## Decision

**Option 1: build the telegraph.** A ground-plane lane geometry is added to the ADR-0003 channel as a first-class, reusable variant, and `GrasscutterAI.SpinDash` commits its heading before the wind-up so there is a real lane to draw. ADR-0006's arena stays at r = 10.0 m and its dash at 6.5 m.

### §1 The channel gains a second geometry, selected by method, not by a new enum member

`AttackTelegraphService` gains one static entry point alongside `Show`:

```csharp
public static AttackTelegraphHandle ShowGroundLane(
    Vector3 start, Vector3 direction, float length, float width,
    AttackTelegraphKind kind, float duration, float groundY = 0f)
```

- **World-space anchored. It does not take, or track, a `Transform`.** That is the point: the lane is committed geometry. An indicator that followed the boss would reproduce Fact 2's dishonesty in a new shape.
- `direction` is normalized on the XZ plane by the service; a degenerate direction returns `AttackTelegraphHandle.None` and logs once.
- `kind` continues to carry **parryability and the audio class only**. Call it with `AttackTelegraphKind.AreaUnparryable`, which already maps to `SoundEvent.TelegraphAreaUnparryable` in `MapAudioCue`.

**`AttackTelegraphKind` does not grow.** Its five members all classify parry behaviour; a `GroundLane…` member would mean "geometry" while its siblings mean "class", and would force a decision about `GroundLaneParryable` that no authored attack needs. Geometry is chosen by which method you call. This leaves the enum, `MapAudioCue`, and all twelve existing `Show` call sites untouched.

Recorded as known debt rather than fixed here: the enum *already* conflates shape with class for the billboard path (`MeleeParryable` → circle, everything else → triangle). If a third geometry ever appears, the right refactor is two orthogonal parameters — geometry and class — and it should be done then, across all call sites, not smuggled in for one variant now.

### §2 Rendering: a flat quad, depth-tested, on the existing shader

| Property | Value |
|---|---|
| Mesh | one **static shared quad** built once, XZ plane, `x ∈ [−0.5, 0.5]`, `z ∈ [0, 1]`, normal +Y. Same `EnsureSharedMeshes` pattern and the same Editor `playModeStateChanged` `DestroyImmediate` cleanup as `AttackTelegraphIndicator` |
| Transform | position `start + Vector3.up * (groundY + 0.02f)`; rotation `Quaternion.LookRotation(direction, Vector3.up)`; scale `(width, 1, length)` |
| Shader | **the existing `BoxForged/TelegraphOverlayUnlit`**, with `ZTest Always` promoted to a material property: `[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 8` and `ZTest [_ZTest]` in the pass. `8` is `Always`, so `mat_TelegraphOverlay.mat` behaves **exactly** as it does today |
| Material | new `Assets/_Project/Materials/mat_TelegraphLane.mat`, same shader, `_ZTest = 4` (`LEqual`) |
| Renderer | `shadowCastingMode Off`, `receiveShadows false`, as the billboard already does |

**Why depth-tested rather than `ZTest Always`.** A 19 m band drawn over the top of the player and the boss reads as a UI overlay pasted on the screen, not as a marking on the ground — and the ground read is the whole spatial payload. The queue is `Transparent+100`, so opaque depth is already written when the lane draws: `LEqual` means the player and the boss correctly stand *on* the lane. The 0.02 m lift plus `ZWrite Off` keeps it off the ground plane without z-fighting.

Occlusion-independence — ADR-0003's actual requirement — is satisfied here by **extent, not by depth state**. You cannot hide a 19 m band behind a 1.0 m obstruction, and M2 clause 3 caps interior obstructions at ≤ 2% of floor area and ≤ 1.0 m wide, so there is nothing in the fight band capable of hiding it. This is a genuinely different mechanism from the billboard's, and it is why the billboard keeps `Always` and the lane does not.

### §3 A separate pooled component and a separate pool

New `Core/AttackTelegraphLane.cs`, pooled by the same service with its **own array of 2**, warmed in `Awake` alongside the billboard pool.

**Not the existing pool, and this is the decisive reason.** `FindSlot` (`:197-212`) evicts at the round-robin cursor when every slot is busy. A boss lane is raised *first* and held for ~1.4 s while ordinary wind-ups come and go — so in a crowded room it is exactly the kind of entry that gets recycled out from under itself. Zone 2 is boss-only today so it would not bite, but this channel is meant to be reusable and a fairness-critical indicator must not be evictable by a grunt. Two separate pools, two separate caps.

Secondary reason: `AttackTelegraphIndicator`'s entire `Update` is target-tracking plus `ApplyBillboard`. A lane does neither. Folding both into one component means a mode branch in every method and the class's stated invariant — *"billboards to face the main camera every frame it is active"* — stops holding.

**Handle disambiguation.** `AttackTelegraphHandle` gains an `internal readonly byte PoolId` (0 = billboard, 1 = lane) so `Hide` routes to the right array. The struct is only ever constructed inside the service, so this is source- and behaviour-compatible.

**Material wiring.** A `[SerializeField] private Material _laneSourceMaterial` on the service, tinted once in `CreateSharedMaterials` with the existing `UnparryableColor` and destroyed in `OnDestroy`, following B32's lesson exactly — reference the material asset from the Inspector; **do not `Shader.Find`**, or the shader is stripped from real builds while working fine in the Editor. Assign `mat_TelegraphLane.mat` on `Assets/_Project/Prefabs/Core/pfb_AttackTelegraphService.prefab`, which is already placed in `Backyard_Dojo.unity` (verified). A null `_laneSourceMaterial` must log an error naming the asset path, as `_overlaySourceMaterial` already does.

### §4 The Spin-Dash's numbers

| Quantity | Value | Derivation |
|---|---|---|
| **Heading committed** | at rev start, **before** `WindUp` is entered | Fact 2. The aim block (`:754-777`) moves above the `WindUp` call; the dash then travels the heading it advertised |
| **Lane raised** | at rev start, same frame as the heading commit | §1.3's "drawn before the dash commits" |
| **Lane duration** | `spinDashRevDuration + spinDashMaxDistance / spinDashSpeed` = 0.9 + 0.5 = **1.4 s** | held through the travel too, so the band stays visible while the player is dodging out of it. `Hide(handle)` on death, stagger, or the `travelDist < 0.05` abort |
| **Lane extent** | **full chord**, wall inner face to wall inner face, through the committed heading | §1.3's "full-chord". See below |
| **Lane width** | **3.0 m** = `2 × _dashContactRadius` | the dash's own hit test is `IsPlayerWithinRange(1.5f)` (`:800`). Promote that magic literal to a serialized `_dashContactRadius = 1.5f` and derive **both** the hit test and the lane width from it. An indicator narrower than the hitbox is a fairness lie in the direction that costs the player health |
| **Rev-duration floor** | **≥ 0.75 s** while the lane is the fairness mechanism | see the escape arithmetic below. 0.9 s stands, with slack |
| **Overhead billboard** | **kept**, unchanged | ADR-0003 decision 4's philosophy: this is additive. The billboard says *an unparryable attack is coming, from that enemy*; the lane says *and it goes here*. Two channels, neither sufficient alone |

**Why full-chord, and not the 6.5 m of travel.** This is the part an implementer will otherwise reasonably get wrong. A 6.5 m segment starting at a north-rim boss is entirely off-frame for a south-rim player — the exact failure the indicator exists to fix. A chord extended to both walls always has its **near end close to the player**, inside the frustum, wherever on that chord the player stands. Frustum-independence is a property of the extent, not of the shader.

The lane therefore **over-states how far the boss travels and never under-states the width or the heading.** That asymmetry is the safety principle: over-warning is fair, under-warning is not. It also means B117's pending path-clamp fix (which may shorten the actual travel) cannot make the lane a lie.

**Measuring the chord.** Two `Physics.RaycastNonAlloc` calls into a preallocated buffer, from `start + Vector3.up * 0.5f` along `±direction`, on the `Building` layer, capped at 24 m, taking the **farthest** hit in each direction. Taking the farthest rather than the first is what makes the trunk irrelevant without a name or tag hack — the wall is always beyond it. Zero allocation, twice per dash. If a direction returns no hit, fall back to half of a serialized `_dashLaneFallbackLength = 20.0f` on that side and log once: a missing wall must not silently collapse the lane to zero length.

**The escape arithmetic — the number that decides whether any of this works.** The player must clear 1.5 m of half-width plus roughly 0.4 m of body radius = **1.9 m of lateral displacement**, and has two routes:

| Route | Time to clear 1.9 m | Slack in a 0.9 s rev |
|---|---|---|
| Walk perpendicular, `moveSpeed 5` (`BoxData_Ninja.asset`) | **0.38 s** | 0.52 s for recognition |
| One dodge: `dodgeMovementDelay 0.2` + `dodgeDuration 0.5`, covering `dodgeDistance 3.0 m` (`Player/CombatController.cs:20-23`) | **0.70 s** | 0.20 s |

Both close, and the more forgiving route is the plain one — walking beats dodging here, because `dodgeDistance / dodgeDuration` = 6 m/s is barely above `moveSpeed`. **0.75 s is the floor** (0.38 s escape + ~0.35 s recognition); 0.9 s holds it with margin. Reducing `spinDashRevDuration` below 0.75 s, or widening the band without lengthening the rev, breaks the grant and is not a tuning decision.

This is stated as arithmetic rather than asserted because two accepted acceptance criteria on this project have already turned out to be unsatisfiable as written (ADR-0006 Facts 5 and 6). A telegraph that cannot be escaped inside its own wind-up is the same class of error.

### §5 ADR-0006 §1.2's "perpendicular offset from centre ≥ 2.5 m" is a chord-arithmetic assumption, not an AI constraint

That row exists to show the dash always fits: at a 2.5 m offset the chord of a r = 10 arena is 19.36 m, leaving ~6.4 m at each end of a 6.5 m dash. `SpinDash` aims at the live player, so nothing enforces the offset, and a player standing at the centre yields a lane straight through the trunk.

**Do not enforce it in the AI.** Aiming past the player to satisfy a geometry rule would read as the boss deliberately missing, which is a worse defect than the one it fixes. The row is hereby read as a **worst-case bound used to prove the dash fits inside the arena**, not as a runtime invariant. The two things it was protecting are covered elsewhere: the trunk-in-lane case by §4's farthest-hit chord measurement, and the dash-fits-inside-the-arena case by the NavMesh clamp (`ClampToNavMesh`, and B117's pending strengthening of it).

### §6 Erratum in ADR-0006 §2.2

*"The current built arena fails M2.2, at `10.0 − 0.35` → **8.15 m** against 8.5 m"* — the subtraction is right, the first operand is a typo. The *current built* arena at the time was r = **8.5**, so the calculation is `8.5 − 0.35 = 8.15`. The conclusion is unaffected. Corrected inline in ADR-0006, because whoever runs §Validation 2 will otherwise try to reconcile 10.0 − 0.35 with 9.65 two paragraphs later.

---

## Alternatives considered

**1. Option 2 — revise the condition and shrink the arena and dash back down.** The path ADR-0006 §1.3 itself named, and the reason the owner was offered a genuine choice. Rejected, and not on cost: **it has no legal landing site.** M2.2 with a central 0.35 m trunk demands R ≥ 8.85 m; a body-anchored tell readable rim-to-rim demands R ≤ 7.65 m; the intervals are disjoint (Fact 4). The pre-B116 r = 8.5 m arena fails M2.2 at 8.15 m — it is the configuration the playtest complained about and the corrected metric independently rejected. Taking this path would additionally require revising M2, whose radial-band clause is World 1's playtested 8.44 m, and re-deriving every position B116 has just finished placing while paying the 45° coordinate tax a third time (Fact 5). It trades one quad for the loss of the only playtested number in the chain.

**2. Add a `GroundLaneUnparryable` member to `AttackTelegraphKind`.** The obvious move, and the one the brief anticipated. Rejected as the *smaller* change it appears to be: the enum's five members classify parryability, a sixth classifying geometry makes the type mean two things, and it immediately raises `GroundLaneParryable` — a variant no authored attack wants. Selecting geometry by method leaves the enum, `MapAudioCue`, and twelve call sites untouched. §1 records the pre-existing shape/class conflation as debt with the condition under which it should actually be paid.

**3. Reuse the existing 8-slot indicator pool with a mode flag on `AttackTelegraphIndicator`.** Fewer files and no new component. Rejected: `FindSlot`'s eviction makes a long-lived boss lane recyclable by ordinary grunt wind-ups (§3), and the component's billboard-and-track invariant would have to be abandoned. Two small pools are simpler than one pool with two personalities.

**4. `ZTest Always` for the lane, reusing `mat_TelegraphOverlay.mat` unchanged.** Zero new assets — no shader property, no material. Rejected: a 19 m band painted over the player and the boss reads as a screen overlay rather than a floor marking, and the floor read is the entire spatial payload. The lane's occlusion-independence comes from its extent (§2), so it does not need the depth trick the billboard needs.

**5. A URP decal projector.** The conventional answer, and already rejected by ADR-0003 on mobile cost — `DecalRendererFeature` requires a depth prepass, a permanent full-screen cost in every scene. Still rejected, and it is not needed: the arena floor is one flat plane, so a flat quad is exactly equivalent visually and free. §Fact 6 narrows ADR-0003's rejection so this reasoning is available to the next person instead of reading as a blanket ban.

**6. Animation-based tells — a visible wind-up pose that reads the heading (ADR-0003 Alternative 3).** Still the highest-quality answer and still rejected for the same reason: authored per-attack animation on a boss this team has not finished modelling. It also cannot solve Fact 3 — an off-frame boss's pose is not visible at any quality level. A lane and an animated wind-up are complementary, and this is the right thing to revisit per-boss later.

**7. Do nothing; treat §1.3 as an aspiration and ship the body tell.** Rejected. The condition was written as a condition, in two places, precisely so this could not happen quietly, and Fact 2 means the attack currently advertises a dodge the player cannot perform. The owner routing the conflict here rather than accepting a silent patch is the same instinct.

**8. Screen-edge arrows for the off-frame boss (ADR-0003 Alternative 4).** Complementary rather than alternative, and now more clearly wanted: the lane fixes *where the attack goes*, not *where the boss is*, and ADR-0006 accepted 4.7 m of off-frame boss at rim-to-rim. Out of scope here; recorded as a follow-up in B118's "not in scope".

---

## Consequences

### Positive

- ADR-0006's arena survives at r = 10.0 m on the mechanism it was granted on, and B116's completed re-layout is not re-done.
- **The Spin-Dash becomes the attack its own code comment claims it is.** Committing the heading before the rev turns "dodge perpendicular to the lane" from a false comment into the actual counterplay, and the arithmetic in §4 shows there are two escape routes with slack.
- The channel gains a reusable geometry, not a one-boss special case. `SpinCycleAI.SpinCharge`, `WagonWheelRollerAI`'s charge, and any future dash or beam attack can call `ShowGroundLane` without touching the service again.
- ADR-0003's decal rejection stops reading as a ban on ground telegraphs, so the next person wanting one is not blocked by a misread (Fact 6).
- The lane's width is derived from the dash's own hit-test constant instead of being a second independent number, so the two cannot drift apart.
- The escape window is proved by arithmetic against measured project values rather than asserted — the discipline ADR-0006 Facts 5 and 6 exist to enforce.
- Cost is one shader property, one material, one shared quad, one 60-line pooled component, and +1 draw call for ~1.4 s per dash.

### Negative / risks

- **The lane over-states how far the boss travels** — a full chord against a 6.5 m dash. Accepted deliberately (§4): the alternative is an indicator that is off-frame exactly when it is needed. If it reads badly in play, the fix belongs to `ui-ux-designer` as a visual gradient distinguishing the committed travel from the remaining chord, **not** as a shortened lane.
- **The boss body is still 4.7 m off-frame at rim-to-rim.** ADR-0006 accepted this and this ADR does not improve it. The lane covers the attack, not the opponent. Alternative 8 is the complementary piece.
- **`SpinDash` gains a behaviour change**, not just an indicator: the heading commits 0.9 s earlier. The attack becomes genuinely dodgeable, which means it will land less often and the fight's difficulty moves. That is the intent, but it wants a play pass, and it interacts with B117's path-clamp work on the same code path.
- **A new serialized dependency that fails silently if unwired.** `_laneSourceMaterial` unassigned means no lane, which means the arena's grant condition is unmet while the game still runs. Hence the loud error and hence §Validation 4's prefab check.
- **`_ZTest` is added to a shader the shipped billboard path uses.** The default is `Always`, so behaviour is unchanged, but it is a shipped-path edit. A separate shader file is the zero-risk alternative if the implementer prefers it; one shader with a property is cleaner and one fewer asset.
- **Verified only in the boss arena.** The lane assumes a flat floor at a known `groundY`. On sloped or multi-level ground a flat quad will clip. Zone 2's floor is a plane; any future use on non-flat ground needs its own look, and that is the point at which ADR-0003's decal rejection genuinely deserves re-litigating.
- **The condition is not discharged by this ADR.** §Validation 10 of ADR-0006 still has to pass on device.

### Out of scope / explicitly deferred

- Implementation. `docs/BACKLOG.md` **B118**, `unity-gameplay-engineer`.
- The lane's visual treatment — colour, edge, animation, whether the committed travel segment is distinguished from the rest of the chord. `ui-ux-designer`.
- Screen-edge indicators for the off-frame boss (Alternative 8).
- Retrofitting `ShowGroundLane` to `SpinCycleAI.SpinCharge` or `WagonWheelRollerAI`. World 1 is shipped and playtested; changing its boss's telegraph is a separate decision with its own play pass.
- B117's endpoint-only NavMesh clamp. Adjacent and on the same method, but a distinct correctness fix with its own verification.
- The orthogonal geometry/class refactor of `AttackTelegraphKind` (§1).
- Authoring `SoundData` clips for any `SoundEvent.Telegraph*` — none exist yet for any telegraph kind (`Core/SoundData.cs:21`), which is a pre-existing gap this ADR neither creates nor fixes.

---

## Open questions for the owner

None block B118.

1. **Does the full-chord lane read well, or does it feel like the boss threatens more ground than it takes?** The honest answer needs play, not paper. The dial is the visual treatment, not the length — shortening the lane re-breaks frustum-independence.
2. **Does the Spin-Dash still feel dangerous once its heading is committed 0.9 s early?** It will land less often by design. If it now feels toothless, the levers are `spinDashSpeed`, the attack's frequency in the Phase-2 pool, or the trail hazard's persistence — **not** the rev duration below 0.75 s and not the lane.
3. **Carried forward, fourth ADR to ask:** `docs/TECHNICAL_DECISIONS.md` still lists ADR-0001/0002/0003 as **Proposed** while ADR-0002's own header says **Accepted**, Sprint 0 shipped against 0001/0003, and this ADR extends 0003. Extending a decision recorded as un-approved is uncomfortable and the discrepancy is now three ADRs old.

---

## Validation before the telegraph is called done

1. **ADR-0006 §Validation 10, on device:** the lane is visible and readable with the player at the south rim and the boss dormant/active at the north rim — the 4.7 m-off-frame case. This is the condition the arena is granted on; nothing else in this list substitutes for it.
2. **The heading is honest.** Record the committed direction at rev start and the actual travel vector at dash end; the angle between them must be **0** for a dash that is not NavMesh-clamped, and the clamped case must only ever *shorten* travel along that heading, never rotate it.
3. **The band matches the hitbox.** With the lane up, a player standing 1.4 m off the centreline is hit and one standing 1.6 m off is not — i.e. the drawn width and `_dashContactRadius` agree. A drawn band narrower than the hitbox fails.
4. **`_laneSourceMaterial` is assigned on `pfb_AttackTelegraphService.prefab`**, and the prefab instance is present in `Backyard_Dojo.unity`. Also confirm the error path: with the field cleared, the service logs the named error rather than rendering nothing quietly.
5. **The escape window holds in play, not just in arithmetic.** From standing still on the centreline at rev start, both a perpendicular walk and a single perpendicular dodge clear the band before contact. If either fails, `spinDashRevDuration` rises; it does not fall.
6. **Zero steady-state allocation.** Profile a Phase-2 sequence of at least six dashes: no per-dash `Mesh`, `Material`, `GameObject`, or `RaycastHit[]` allocation. The chord measurement uses `RaycastNonAlloc` into a preallocated buffer.
7. **Both lane slots and both pools survive exhaustion.** Force two concurrent lanes plus eight concurrent billboards and confirm neither pool evicts the other's entries, and that a lane is never recycled by a billboard request.
8. **Greyscale and colour-blind passes**, per ADR-0003 §Validation 1–2. The lane's meaning is carried by its position and extent, so it should pass trivially — confirm rather than assume, and confirm it is distinguishable from the Cut-Grass Trail hazard quads it will overlap.
9. **Draw calls and frame time** with a lane active, against ADR-0006 §5.1. Expect +1 call; report the measured number with its scenario.
10. **`mat_TelegraphOverlay.mat` is unchanged in behaviour** after `_ZTest` is added to the shader: the billboard still draws through walls (ADR-0003 §Validation 3's worst-case occlusion test still passes).
