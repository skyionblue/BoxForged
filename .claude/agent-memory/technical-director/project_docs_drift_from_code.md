---
name: project-docs-drift-from-code
description: BoxForged docs have drifted from code in load-bearing ways — verify architecture claims against prefab/asset YAML, never from PROJECT_CONTEXT or the GDD alone.
metadata:
  type: project
---

BoxForged's documentation has drifted from the codebase in ways that actively mislead. Verified 2026-08-19.

The sharpest example: both `docs/PROJECT_CONTEXT.md` and the GDD recorded the camera as Cinemachine offset `(0, 12, -8)` with a hard look-at. The actual prefab (`Assets/_Project/Prefabs/Core/pfb_CM_FollowCam.prefab`) has `FollowOffset: (7.879929, 11, -10)`, `FieldOfView: 40` — and an **undocumented −38.2° yaw**. Because `PlayerController` derives movement from camera yaw, that yaw silently rotated the entire control mapping. It was wrong for a whole phase and nobody caught it.

Other confirmed drift: planning material references "ground-indicator AOEs" that do not exist in code (there is no telegraph system at all); three scenes listed in Build Settings do not exist on disk; `CREATIVE_STATE.md` lists `LaundryTumbler` as a non-canon "thought" while it is live in a shipping scene.

A second sharp example (2026-08-19, B4/D3): the backlog described the two weapon-ability systems as duplicate stacks to be consolidated, implying ~81 `WeaponData` assets needed converting. Reading the assets showed the opposite — they are **stacked layers, not competitors**. `WeaponObjectSO.baseEquippedData` points *at* a `WeaponData`, so V3 is a live dependency of V4, and V4 supplies Epic/Legendary abilities only (`AbilityExecutor` maps Standard tier to `null`). The scary asset count was an artifact of the wrong mental model. **Beware summaries that count assets by type without checking which layer references which.**

**Why:** on this project the docs *are* the AI's primary context, so a wrong doc propagates errors into every future session rather than just misinforming a human reader once.

**How to apply:** for any claim about architecture, camera, scene contents, or system wiring, verify against the actual prefab/scene/asset YAML or the C# before relying on it. Prefer citing file:line or asset values over prose in anything written back into the docs. Treat `PROJECT_CONTEXT.md` as intent, not as ground truth.

Related: [[project-preproduction-gate]]
