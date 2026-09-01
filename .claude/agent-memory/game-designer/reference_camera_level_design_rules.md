---
name: boxforged-camera-level-design-rules
description: The two derived ADR-0001 camera formulas that govern every BoxForged level layout — prop-occlusion distance and the tall-prop off-top-of-frame limit; both bite hard and neither is in the ADR as a formula
metadata:
  type: reference
---

BoxForged's camera (ADR-0001) is fixed: pitch 36°, yaw 0, FOV 45, height 5.5 m. Framing: **F = 15.3 m ahead, R = 4.2 m behind, W = 16.8 m lateral.** Camera clearance is a *level-design* constraint (no deoccluder ships): ≥ 8 m clear behind and ≥ 6 m above every walkable point, diagnostically checked by `LevelBuilder.ValidateCameraClearance`.

Two derived rules do the real work in layout. They are worked out in full in `docs/v4/levels/World2/backyard-dojo/zone-layout-spec.md` §0 — read that rather than re-deriving:

**Rule A — a prop occludes the player** when it stands `d` metres *behind* (camera-side of) the player and `d < (h_prop − 1.0) / 0.7265`. Consequence: a 2.4 m wall only occludes within 1.93 m, a 4.2 m shed within 4.4 m, a 6 m prop within 6.9 m. **So tall props go north of where the player fights, never south of it**, and low perimeter walls are nearly free.

**Rule B — a tall prop's top leaves frame** unless its distance *ahead of the player* is `≤ (6.35 − h) / 0.2401 − 7.57`. Consequence: a 4.0 m point is in frame only within ~2.2 m ahead; **anything above ~4.5 m is effectively never in frame at any distance.** This is why "distant backdrop" placement does not work on this rig (ADR-0001 §Consequences, BACKLOG B57) and why any narrative beat that asks the player to *see* something tall or far needs a scripted camera.

Also load-bearing: `LevelBuilder._cameraClearanceMask` is layer 8 (`Building`) **only** — props not on that layer are invisible to the validator. Put walls/buildings/tall trunks on `Building`; keep low dressing off it so the diagnostic stays meaningful.

**Related:** [[world2-layout-spec-location]], [[boxforged-arena-radius-vs-central-obstacle-conflict]].
