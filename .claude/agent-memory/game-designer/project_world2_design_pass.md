---
name: world2-design-pass-state
description: World 2 design pass completed 2026-09-01 — spec written and handed to stage-A implementation; four unresolved conflicts are waiting on the owner/technical-director and none of them are safe to decide unilaterally
metadata:
  type: project
---

World 2 (Backyard/Dojo) level design pass is **done**; implementation has not started. Spec at `docs/v4/levels/World2/backyard-dojo/zone-layout-spec.md`.

**Why:** ADR-0005 locked the scene architecture (one continuous scene, 3 zones) and scaffolded `Backyard_Dojo.unity` with empty `RoomManager` zones, but explicitly deferred "zone geometry, dimensions, prop placement, spawn coordinates, and encounter composition" to a separate task. That task was this one.

**How to apply:** the four findings below are the load-bearing output. Do not let a future session quietly resolve any of them — each was surfaced deliberately rather than fudged, and each has a recommended answer that still needs owner or `technical-director` sign-off. They are also listed as Q1–Q3 and Q11 in the spec's §8.

1. **ADR-0005 §3's "boss arena clear circle r ≥ 8.5 m" is literally incompatible with §4's "cherry tree at centre."** With a central obstacle of radius `r_t`, the largest inscribed clear circle is `(R − r_t)/2`, so satisfying the metric literally needs a ~35 m arena — the exact number ADR-0005 corrected. Recommended amendment: restate as *"outer walkable radius ≥ 8.5 m, no interior obstruction exceeding 0.8 m diameter."* The arena size (17.0 m across) must not change; it is pinned to the camera's 16.8 m visible width.
2. **The CANON Assembly Beat imagery cannot be delivered on the gameplay camera.** "The shed's roof lifts" and "a tree at the far end shaking pink" both fail ADR-0001's tall-prop frame limit from the spawn point. Recommended: a 3 s scripted intro camera (mechanism already exists as the boss-intro cam). This is unbudgeted scope relative to ADR-0005 §7.
3. **The Polyworks Asian prop set that ADR-0005 Fact 3's whole draw-call argument rests on is not in the project.** Zero files match `Asian_Prop*`, `*tatami*`, `*zen*`, `*sand*`, `*fountain*`, `*paper*`. Only 8 dojo ENV prefabs actually exist. The spec is built entirely from those 8 plus 7 new small meshes; the shared-atlas premise has to be re-earned by consolidating materials, and installing the package needs explicit owner approval.
4. **`street_pond_a` does not exist** (the old blueprint's `⚠️ check-first` item) but **`pfb_env_koi_pond_basin` does** — so ADR-0005's "possibly the pond basin is new" is resolved as *not new*.

**Recommended next step at time of writing:** hand **stage A** (spec §9) to `unity-gameplay-engineer` — ~70% of the level, fully unblocked, produces a walkable end-to-end yard with a real zone-0 fight and lets the flood-fill / camera-clearance / NavMesh validation run early. Stages B–F are blocked on the `ZoneDirector` rename, new ENV art, enemy AI (Crane/Lurker/Grasscutter prefabs are all art-only with no `EnemyStats`), and owner decisions respectively.

**Related:** [[world2-layout-spec-location]], [[boxforged-camera-level-design-rules]].
