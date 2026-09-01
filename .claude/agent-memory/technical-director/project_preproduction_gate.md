---
name: project-preproduction-gate
description: BoxForged lifecycle state — production authorized, Sprint 0 and World 1 (ADR-0004) shipped and committed, World 2 authorized as Phase 3 with ADR-0005 accepted; ADR-0001/2/3 status is still contradictory across docs.
metadata:
  type: project
---

BoxForged left Discovery 2026-08-18; the owner authorized production 2026-08-19 ("Start Sprint 0").

- **Sprint 0** (camera ADR-0001, telegraph channel ADR-0003, forge feel) — implemented and pushed on `feature/sprint-0-foundation-rebuild`.
- **World 1 / ADR-0004** — no longer just a spec. Built, five `code-reviewer` fix passes, an owner-reported geometry revision (B107), owner-playtested, and **committed** (`b80953ca`, plus `84a3a44e` which deleted every legacy per-room scene). Push was gated on the owner confirming the GitHub LFS cap.
- **World 2 / Phase 3** — production explicitly authorized. `docs/adr/0005-world2-single-continuous-scene.md` **Accepted 2026-08-31**: one continuous scene `Backyard_Dojo.unity`, three zones, and single-continuous-scene promoted from a World-1 deviation to the **project default**. The owner delegated that architecture choice to `technical-director` rather than picking a pattern.
- **World 2 Stage A built** (B115) and **playtested by the owner 2026-09-01**, who reported zone 1 and zone 2 both too small. `docs/adr/0006-world2-zone-scale-and-arena-metric.md` **Accepted 2026-09-01** amends ADR-0005 §3/§4 and restates TDD §6.4 project-wide: zone 1 → 20.0 × 28.0 m, arena → r = 10.0 m, Grasscutter dash ≤ 6.5 m, zone 0 unchanged. Implementation is **B116**, not started. Note the pattern — the owner playtests fast and reports scale by feel, so expect an accepted ADR's dimensions to be revisited within a day of the geometry existing; write the metrics so they survive that (see [[project-unsatisfiable-metrics]]).

**ADR status is still genuinely contradictory — do not assume.** `docs/TECHNICAL_DECISIONS.md` lists ADR-0001/0002/0003 as **Proposed**; `docs/adr/0002-full-scene-rebuild.md`'s own header says **Accepted**; `docs/ROADMAP.md` says production was authorized. Code shipped against 0001/0003 regardless, and ADR-0005 now supersedes part of 0002. Flagged for the owner in TECHNICAL_DECISIONS and in ADR-0005 §Open Questions 6; still unresolved.

**World 2's room structure is contradicted by three sources — surface it, do not silently pick one.** `docs/ROADMAP.md` Phase 3 says three rooms (Dojo Courtyard / Garden Gauntlet / Garden End) and names no Koi Pond; `docs/v4/levels/World2/backyard-dojo/gdd.md` + `unity-blueprint.md` (2026-08-07, marked "Approved") say five rooms with a random draw of Rock Garden / Training Hall / Koi Pond, and predate ADR-0001/0002/0004; `docs/story/zones/backyard-dojo.md` (canon lore) has Back Gate + a mid-zone Koi Pond carrying the Skeptic beat + the Grasscutter at the end. ADR-0005 assumes ROADMAP's three zones with the Koi Pond as a sub-space of zone 1 — the only reading satisfying all three — and lists the conflict as an owner question.

**Why:** the owner pivots direction mid-flight (room-by-room → single scene) and expects a `technical-director` design + ADR pass before implementation, per `CLAUDE.md`. They also delegate architecture calls outright when both options are precedented, so expect to decide and justify rather than present a menu.

**How to apply:** check current doc state rather than trusting any single source. Six older owner decisions (D1–D6 in `docs/BACKLOG.md`) still gate downstream work, notably the V3/V4 ability-system choice. Two ADR-0005 prerequisites are live blockers for the Grasscutter: the large-agent NavMesh decision (single baked agent type radius 0.5, bosses run 0.95–1.0) and the texture import policy before any new dojo art.

Related: [[project-asset-weight-risk]], [[project-docs-drift-from-code]], [[project-roommanager-zone-mechanism]]
