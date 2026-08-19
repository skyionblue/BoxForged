# ADR-0001: Fixed low-angle follow camera (no rotation)

- **Status:** **Accepted** — owner authorized 2026-08-19 ("Start Sprint 0," following explicit confirmation to build the camera and telegraph work together)
- **Date:** 2026-08-19
- **Supersedes:** GDD v0.6 item 22 ("fixed top-down, no rotation"); the Camera section of `docs/PROJECT_CONTEXT.md`
- **Related:** [ADR-0002](0002-full-scene-rebuild.md) (scene rebuild), [ADR-0003](0003-attack-telegraph-channel.md) (telegraph channel)

## Context

### The documented camera was never the real camera

Both `docs/PROJECT_CONTEXT.md` and the GDD record the rig as Cinemachine with offset `(0, 12, -8)` and a hard look-at. The authoritative values in
`BoxForged/BoxForged/Assets/_Project/Prefabs/Core/pfb_CM_FollowCam.prefab` are different:

```
BindingMode: 4                                   # WorldSpace
FollowOffset: {x: 7.879929, y: 11, z: -10}
FieldOfView: 40
```
plus a `CinemachineHardLookAt` with zero offset. No scene overrides `FollowOffset`.

Two consequences follow, and both were invisible in the docs:

1. **The real pitch is ~40.8°, not a steep top-down.** Horizontal distance is `sqrt(7.88² + 10²) = 12.73`, height 11, so `atan(11 / 12.73) = 40.8°`.
2. **The rig carries a yaw of about −38.2°** (`atan2(-7.88, 10)`). The camera does not look down +Z; it looks down a diagonal.

The yaw matters more than it looks, because movement is camera-relative
(`Player/PlayerController.cs:192-200`):

```csharp
Vector3 camForward = _mainCamera.transform.forward;
Vector3 camRight = _mainCamera.transform.right;
camForward.y = 0f;
camRight.y = 0f;
camForward.Normalize();
camRight.Normalize();
Vector3 moveDir = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
```

Because `.y` is zeroed, **camera pitch does not affect controls at all — only yaw does.** Today, joystick-up drives the player along world `(-0.619, 0, 0.786)`. Any new rig with yaw 0 rotates the entire control mapping by +38°.

So the decision splits into two independent parts that the placeholder offset `(0, 4, -6)` silently bundled together: a **pitch/height change** (free with respect to controls) and a **yaw change** (a real feel change).

### What the owner asked for

Fixed follow camera, lower angle, no rotation. Behind Kid, follows position, never rotates, sits lower and closer so the world reads at kid-height. "Hades pulled in closer," explicitly not over-the-shoulder. The conversational figure `(0, 4, -6)` was a placeholder, not a spec.

### Why an offset is the wrong thing to specify

An offset triple couples height, distance, and yaw into three numbers that hide the three quantities that actually govern readability: **pitch angle, camera-to-player distance, and vertical FOV**. Two rigs with the same "lowness" can frame completely different amounts of ground.

For a camera at height `h` with pitch `θ` and vertical FOV `f`, the ground plane is visible between:

- near edge: `h / tan(θ + f/2)` (horizontal distance from camera)
- far edge: `h / tan(θ − f/2)`

and the player sits at `h / tan(θ)`. This yields the three numbers that decide whether the game is readable:

- **F** — metres of ground visible *ahead* of Kid
- **R** — metres visible *behind* Kid
- **W** — lateral width visible at Kid's depth

The hard constraint is `θ > f/2`. If pitch drops below half the vertical FOV the horizon enters frame, the far edge runs to infinity, and the renderer draws unbounded distant geometry.

**This is exactly what breaks the naive reading of `(0, 4, -6)`.** At Unity's default 60° FOV that rig gives a top ray of `33.7 − 30 = 3.7°`, putting the ground far edge at `4 / tan(3.7°) ≈ 62 m` — roughly 2.5× the ground depth the current rig draws. The placeholder is not wrong because it is low; it is only safe because the project already runs a 40° FOV. That relationship is not written down anywhere, which is how the project ended up with a documented offset that does not match the prefab.

### The constraint that actually binds: rear visibility

Rear visibility is governed almost entirely by camera height, so lowering the camera costs it directly. This is measurable against real combat numbers:

| Source | Value |
|---|---|
| `Enemy/SkepticGruntAI.cs:16` | `moveSpeed = 3f` |
| `Enemy/SkepticGruntAI.cs:19` | `attackRange = 1.5f` |
| `Enemy/SkepticGruntAI.cs:23` | `windUpDuration = 0.6f` |
| `Enemy/BasicEnemyAI.cs:14,16,19` | `2.5 m/s`, `1.5 m`, `0.8 s` |

A grunt entering frame at distance `R` reaches attack range after `(R − 1.5) / 3` seconds. For the player to see the *entire* 0.6 s wind-up on screen, we need `(R − 1.5)/3 ≥ 0.6`, i.e. **R ≥ 3.3 m** as a floor, and comfortably more to leave reaction time.

At the current rig R ≈ 6.6 m (1.7 s of warning). At a literal `(0, 4, -6)` with FOV 40, R ≈ 3.1 m — **below the floor**. An enemy can cross from off-screen into attack range in less time than its own tell lasts. That is the single failure the camera change can cause, and it is quantifiable rather than a matter of taste.

## Decision

Adopt a fixed-rotation follow camera specified by **pitch, distance, and FOV**, not by a raw offset.

### Recommended rig

| Parameter | Value |
|---|---|
| Pitch (θ) | **36°** below horizontal |
| Vertical FOV | **45°** |
| Yaw / roll | **0°** — fixed, never driven at runtime |
| Camera height (h) | **5.5 m** |
| Derived `FollowOffset` | **`(0, 5.5, -7.57)`** |
| Derived camera distance | 9.36 m |
| Camera transform rotation | Euler `(36, 0, 0)` |

Derivation: `d_h = 5.5 / tan(36°) = 7.57`, so the offset is `(0, h, −d_h)`. The camera transform's own rotation supplies the pitch; nothing aims it at runtime.

### Resulting framing, against the current rig

| | Current (11 m, 40.8°, FOV 40) | **Recommended (5.5 m, 36°, FOV 45)** | Literal `(0,4,-6)` @ FOV 40 |
|---|---|---|---|
| Height | 11.0 m | **5.5 m** | 4.0 m |
| Camera distance | 16.8 m | **9.4 m** | 7.2 m |
| Ground ahead of Kid (F) | 16.2 m | **15.3 m** | 10.4 m |
| Ground behind Kid (R) | 6.6 m | **4.2 m** | 3.1 m ✗ below floor |
| Lateral width at Kid (W, 19.5:9) | 26.5 m | **16.8 m** | 11.4 m |
| Top ray above ground | 20.8° | **13.5°** | 13.7° |

The recommendation **halves camera height and distance** — delivering the requested kid-height, pulled-in framing — while giving up only 0.9 m of forward sightline. It spends its budget on the rear (6.6 → 4.2 m) and the sides (26.5 → 16.8 m), which are the two places the loss can be designed around.

### Cinemachine 3 rig specification

| Component | Setting | Rationale |
|---|---|---|
| `CinemachineCamera` | Lens FOV 45, vertical axis | See aspect rule below |
| `CinemachineFollow` | `BindingMode: WorldSpace` (already `4`) | **Preserve.** Any `LockToTarget*` mode orbits the camera as Kid turns — fatal for a no-rotation design |
| `CinemachineFollow` | `FollowOffset (0, 5.5, -7.57)` | Derived above |
| `CinemachineFollow` | Damping `(0.25, 0.20, 0.25)` | See damping note |
| `CinemachineHardLookAt` | **Remove** | A look-at re-pitches the camera as Kid moves toward/away, so the horizon subtly rolls. A truly fixed camera takes its rotation from its transform |
| Aim component | **None** | Rotation is authored, never computed |
| `CinemachineDeoccluder` | **Do not add** | See below |

**Damping.** Slightly lower on Z than X. Dodge is a fast lateral burst; if lateral damping lags, the dodge resolves before the camera does and the parry window reads late. Keep all values ≤ 0.25 s and set rotational damping to zero — there is no rotation to damp.

**Vertical framing.** Kid should sit at roughly 40% of screen height from the bottom rather than centred, so the forward sightline is spent where threats come from. With no aim component this is achieved by authoring the pitch slightly steeper than the geometric camera→player line, not by a runtime composer.

**Aspect rule.** Unity's `fieldOfView` is vertical, so horizontal coverage varies with aspect: at FOV 45 a 19.5:9 phone sees 83.9° horizontally, a 4:3 iPad only 57.8°. Naively locking the *horizontal* axis instead inverts the problem — on 4:3 the vertical FOV would rise to ~68°, pushing the top ray to ~2° and putting the horizon back on screen. **Therefore: lock vertical FOV (it protects the `θ > f/2` constraint, which is the safety-critical one) and recover lateral coverage on narrow aspects by increasing camera distance along the existing view axis.** Pitch must never change with aspect.

### Acceptance criteria (measurable, aspect-independent)

The rig is correct when, on every supported aspect from 4:3 to 21:9:

| Metric | Target |
|---|---|
| Ground visible ahead of Kid (F) | ≥ 12 m |
| Ground visible behind Kid (R) | ≥ 4 m (hard floor 3.3 m) |
| Lateral width at Kid's depth (W) | ≥ 16 m |
| Top ray above ground plane | ≥ 10° (horizon never in frame) |
| Camera pitch | identical on all aspects |

These, not the offset triple, are the spec. Any offset that satisfies them is acceptable.

### Camera collision: no deoccluder

`CinemachineDeoccluder` is explicitly rejected. A fixed no-rotation camera earns its keep through absolute predictability; a deoccluder that pulls in near walls introduces distance pops precisely during the wall-adjacent fights where readability matters most, and it does so non-deterministically.

Instead, **camera clearance becomes a level-design constraint**: every walkable point in a room must have a clear volume of ≥ 8 m behind and ≥ 6 m above it along the camera axis. This is checkable in the level builder rather than patched at runtime, and it is affordable only because every scene is being rebuilt anyway (ADR-0002). Residual cases are handled by fading occluders, never by moving the camera.

### Yaw: adopt 0°, deliberately

Set yaw to 0. This rotates the control mapping by 38° relative to today — a real change, but one whose cost is entirely relative to existing level geometry, and **every scene is being rebuilt** (ADR-0002). Making this change at any other time would mean re-tuning five rooms of layout against rotated controls. Doing the camera change and the scene rebuild together is materially cheaper than doing either alone.

## Alternatives considered

**1. Keep the current rig (11 m, 40.8°, yaw −38.2°).** Zero cost, and the current pitch is already less top-down than the docs claim. Rejected: it does not deliver the kid-height framing the owner asked for, and the undocumented diagonal yaw is a standing trap — the next person to author a room will lay it out on world axes and find the controls skewed.

**2. Take `(0, 4, -6)` literally.** Simplest possible reading of the brief. Rejected on the rear-visibility floor: R ≈ 3.1 m gives less warning than a grunt's own wind-up, and W ≈ 11.4 m makes any room wider than a corridor unreadable. It is close to viable and could be revisited if off-screen threat indicators prove strong enough in playtest.

**3. Widen the FOV to recover rear visibility while staying at 4 m.** Attractive because it keeps the low height. Rejected: at h = 5 m, θ = 34°, FOV 60 the rear reaches 4.97 m, but the top ray falls to 4° and the horizon enters frame — trading a readability problem for a rendering one. **The three goals (low camera, adequate rear visibility, bounded horizon) cannot be satisfied simultaneously by a single static rig**; something must give, and the recommendation gives up rear metres and buys them back with off-screen indicators.

**4. Dynamic rig — lower while exploring, pull back in combat.** Solves the tension honestly. Rejected for this team and this milestone: blend states, hysteresis, and per-encounter authoring are exactly the kind of complexity a two-person non-expert team cannot debug on a livestream, and a camera that moves on its own contradicts the "never surprises you" premise. Revisit only if boss arenas prove unreadable.

**5. Per-encounter camera profiles (boss rooms use a higher pitch).** A narrower version of (4), and the most likely future concession — `SpinCycleAI` has an airborne slam whose landing is un-parryable (`SpinCycleAI.cs:951-954`), and at 36° a jumping boss can leave frame vertically. Deferred rather than rejected: it is recorded in the backlog and gated on boss-room playtest evidence.

## Consequences

### Positive

- Delivers the requested framing with a quantified justification instead of a placeholder.
- Removes a documentation/reality mismatch that would have misled every future session.
- Improves one latent hazard: at extreme pitch `camForward` degenerates when `.y` is zeroed; a lower camera is strictly safer.
- Bounds ground depth (top ray 13.5° vs a naive 3.7°), keeping distant-geometry cost predictable.
- Combat aiming is unaffected — `CombatController.cs:365` and `DynamiteBundleAbilityData.cs:70-71` use character facing, not camera forward.

### Negative / required follow-up work

These are consequences of the decision and must be scheduled with it, not discovered later:

| Item | Location | Impact |
|---|---|---|
| **Two occlusion systems both mistuned** | `Systems/CameraOcclusion.cs`, `Systems/BuildingOcclusionFader.cs` | `CameraOcclusion.cs:102` rejects occluders by bounds-*centre* distance and `:105-131` tests the full projected AABB — at a low angle a nearby building's rect covers the player almost unconditionally, causing mass over-fading. `BuildingOcclusionFader.cs:83` casts a single ray at the player's *feet*, which at a near-level angle misses walls covering the torso entirely. **Pick one system and delete the other**; they even use different selection mechanisms (LayerMask vs tag) |
| **Enemy health bars** | `Enemy/EnemyHealthBar.cs:113` | Billboard refresh is gated on *camera* movement only, so bars never re-orient when the enemy moves and the camera does not. Invisible at 40°; visibly skewed at 36° where per-enemy view angle varies across screen. `_offset (0, 2.5, 0)`, `_barWidth 1.4`, `_barHeight 0.3` are world-space and roughly double in apparent size at half the distance |
| **Hardcoded FOV duplicate** | `Enemy/SpinCycleAI.cs:88` `_normalCameraFoV = 40f` | Duplicates the rig's FOV. Must move to 45 or the boss-intro → gameplay handoff pops |
| **Arena radius incompatible** | `Player/PlayerController.cs:21` `_arenaBoundaryRadius = 18f` | A 36 m-diameter arena against 16.8 m of visible width means over half of any arena fight happens off screen. Combat arenas must shrink to roughly 8–9 m radius (ADR-0002) |
| **Rear visibility deficit** | — | R drops 6.6 → 4.2 m. Requires off-screen threat indicators (ADR-0003) |
| **Inert look-at wiring** | `Core/CameraFollowTargetInjector.cs:32` | Sets `cam.LookAt`; with `HardLookAt` removed this becomes a no-op. Harmless, but note it rather than debug it later |
| **Stale comments** | `CameraOcclusion.cs:11-12`, `SpinCycleAI.cs:542`, `CameraStackWirer.cs:11` | All describe the old camera or a `pfb_camera_rig` that does not exist |

### Explicitly unaffected

`CameraStackWirer`, `HUDCameraInjector`, `HUD3DPositioner`, `HealthBar3D`, `BonusHealthBar3D`, `ChargeMeter3D`, `BossHealthBar` — none reference the gameplay camera transform. `MinimapIndicator` is bound to a separate minimap camera at `y = 40` (overridden to 60 in `CulDeSac_Room1`) on its own layer. There are no `WorldToScreenPoint` / `ScreenPointToRay` call sites anywhere in the tree, so there is no touch-to-world picking to re-tune.

## Validation

Cannot be settled by inspection. Before this ADR moves to Accepted:

1. Build a grey-box room at the recommended rig and measure F, R, W against the acceptance table on both a 19.5:9 phone and a 4:3 tablet.
2. Verify a Skeptic Grunt approaching from directly behind is on screen for its full 0.6 s wind-up.
3. Verify SpinCycle's airborne slam (`SpinCycleAI.cs:951`) stays in frame through its whole arc; if not, escalate to per-encounter profiles (alternative 5).
4. Verify parry timing feel is unchanged — the parry read must not depend on camera distance.
5. Profile ground-geometry draw calls before and after; confirm the bounded top ray holds.
