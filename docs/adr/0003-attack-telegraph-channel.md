# ADR-0003: Occlusion-independent attack telegraph channel

- **Status:** **Accepted** — owner authorized 2026-08-19, explicitly choosing to build this alongside ADR-0001 rather than deferring the camera change
- **Date:** 2026-08-19
- **Related:** [ADR-0001](0001-fixed-low-follow-camera.md) (camera)

> This ADR was not requested. It is recorded because ADR-0001 cannot be safely implemented without it: the camera change invalidates the mechanism the game currently uses to communicate every attack.

## Context

### There is no telegraph system

A tree-wide search for `telegraph`, `indicator`, `aoe`, `decal`, `Projector`, `DecalProjector`, `LineRenderer`, and ground-marker terms returns **zero** gameplay hits. The ground-indicator AOEs described in planning material do not exist in code.

Every attack tell in BoxForged is a **whole-body material tint**. `Enemy/SpinCycleAI.cs:962-967`:

```csharp
private IEnumerator WindUp(Color color)
{
    _state = BossState.WindUp;
    SetColor(color);
    yield return _waitWindUp;
```

with `SetColor` at `:1209-1212` assigning `_material.color`. The hue *is* the attack identity:

| Attack | Tell | Parryable |
|---|---|---|
| DrumSlam (`:667-677`) | `WindUp(Color.red)` | **No** — `parryable: false` |
| Haymaker (`:690-699`) | `WindUp(Color.yellow)` | Only when the drum window faces the player |
| SpinCharge (`:719-749`) | `WindUp(orange)` | No |
| Clothes Toss (`:809`) | `WindUp(Color.magenta)` | No |
| Jump (`:972-975`) | `SetColor(Color.cyan)` | No — landing hit `:951-954` |
| Phase 2 combo (`:920`) | `WindUp(purple)` | Mixed |

`Enemy/BasicEnemyAI.cs:225,257` and the other AI classes follow the same pattern.

### Why the camera change breaks it

Whole-body tint is a **silhouette-fill** signal. It works at the current 40.8° pitch because every enemy is separated in screen space, seen against the ground plane, and largely unoccluded — you are looking down at the top of the arena.

At ADR-0001's 36° pitch with half the camera height and distance, three things change at once:

1. **Enemies occlude each other.** A tinted enemy standing behind another enemy communicates nothing. At a steep angle they were separated; at a shallow angle they stack.
2. **Props occlude at body height.** Fences, mailboxes, bushes, and workbenches previously sat below the sightline. Now they cover torsos — and the two occlusion systems that would fade them are both mistuned for the new angle (see ADR-0001 consequences), with `BuildingOcclusionFader.cs:83` aiming its only ray at the player's *feet*.
3. **Less enemy surface area is on screen** for near enemies, so less of the body carries the hue at exactly the moment the attack matters most.

The tint also carries **no spatial information** — no direction, no range, no landing point. At a top-down view the player could infer all of that from the arena; at kid-height they cannot. SpinCycle's airborne slam is the sharp case: the landing hit is un-parryable (`:951-954`), so the only defence is spacing, which requires seeing where it will land — and at 36° the boss can leave frame vertically mid-arc.

### An independent problem the camera change exposes

Parryable versus un-parryable is encoded **entirely in hue** — red DrumSlam versus yellow Haymaker. This is the single most consequential bit of information in a Sekiro-influenced combat system, and it is carried on the one channel that:

- is unavailable to red-green colour-blind players (~8% of males);
- degrades on small mobile screens and shifts on OLED panels viewed off-axis;
- is destroyed by any occlusion, however partial.

This is a pre-existing accessibility defect. The camera change does not cause it, but it removes the compensating context that was masking it.

## Decision

Introduce a **second telegraph channel that does not depend on the enemy's body being visible**, and make parryability legible on a non-colour channel.

1. **Overhead billboard telegraph.** A small indicator above each winding-up enemy, rendered so it is never occluded by world geometry. The project already has an overlay camera stack (`Core/CameraStackWirer.cs`, `UI/HUDCameraInjector.cs`) — this rides that existing infrastructure rather than adding rendering machinery.
2. **Parryable versus un-parryable is carried by shape, not hue.** Two clearly distinct glyphs. Colour stays as a redundant reinforcement, never as the sole carrier.
3. **Distinct audio cue per class.** Audio is occlusion-proof and screen-position-proof, and `AudioManager` already exists as a persistent event-driven service.
4. **Keep the existing body tint.** It reads well when unoccluded and costs nothing. This is an addition, not a replacement.
5. **Drive it from the existing state machine.** `WindUp(Color)` already centralises every tell in every AI class. The telegraph should be raised there — one seam, already present, rather than per-attack call sites.

### Explicitly rejected: URP decal projectors

Ground-decal AOE markers are the conventional answer and are rejected on mobile cost. `Assets/Settings/Mobile_Renderer.asset` has `m_RendererFeatures: []` — no decal support exists today, and the URP Decal Renderer Feature requires a depth prepass on mobile. Adding a full-screen depth prepass to gain telegraph markers is a large, permanent frame-time cost imposed on every scene, against a budget already under pressure from texture memory. An overhead billboard achieves occlusion-independence at a fraction of the cost.

## Alternatives considered

**1. Do nothing; rely on the body tint at the new angle.** Zero cost. Rejected: the failure is not cosmetic — an un-parryable attack that the player cannot distinguish from a parryable one converts a skill-expression moment into an unfair hit, and the game's combat pillar is "read and react."

**2. Raise the camera back up until tint readability is restored.** Genuinely solves it, and cheaply. Rejected because it defeats ADR-0001's purpose — but it is the correct fallback if the telegraph work cannot be funded. **These two decisions are coupled: approving the low camera without the telegraph channel is the one combination that should not ship.**

**3. Animation-based tells instead of indicators (wind-up poses, weapon arcs).** The highest-quality answer and what a large studio would do. Rejected for this milestone on asset-pipeline cost: it requires authored per-attack animation on every enemy, which is exactly the kind of art-heavy work a two-person team building live cannot absorb. Worth revisiting per-boss, where the animation count is small and the payoff highest.

**4. Screen-edge arrows for off-screen threats only.** Narrower, cheaper, and also needed — ADR-0001 reduces rear visibility from 6.6 m to 4.2 m. This is complementary rather than alternative, and should be scoped alongside.

## Consequences

### Positive

- Attack readability stops depending on camera angle, so future camera tuning cannot silently break combat fairness.
- Resolves a pre-existing colour-only encoding defect.
- Reuses the existing overlay camera stack and `WindUp` seam; no new rendering features, no new packages.
- Audio cues benefit sighted players in crowded rooms too, where tint stacking was already marginal.

### Negative

- New runtime system to build, test, and pool. Indicators must be pooled — per-wind-up instantiation in a room-clear brawler is exactly the allocation pattern the mobile budget forbids.
- Adds draw calls. Overlay-rendered billboards do not batch with world geometry; budget them explicitly (see `docs/TECHNICAL_DESIGN.md`).
- Screen clutter risk with many simultaneous enemies. Needs a cap and a distance/priority rule.
- Not authorized by this ADR; it is production work.

## Validation

1. Colour-blind simulation (protanopia/deuteranopia) — parryable and un-parryable must remain distinguishable.
2. Greyscale screenshot test — the distinction must survive total desaturation.
3. Worst-case occlusion: enemy fully behind a wall, mid wind-up, with the telegraph still readable.
4. Crowd test at `maxConcurrentEnemies` with all enemies winding up simultaneously — verify the cap and legibility.
5. Audio-only test: mute the display and confirm parryable/un-parryable is still distinguishable.
