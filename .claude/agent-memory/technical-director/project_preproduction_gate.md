---
name: project-preproduction-gate
description: BoxForged lifecycle state — production authorized 2026-08-19, Sprint 0 shipped; ADR-0001/2/3 status is contradictory across docs; ADR-0004 (single-scene World 1) is Accepted but unimplemented.
metadata:
  type: project
---

BoxForged left Discovery 2026-08-18 and the owner authorized production 2026-08-19 ("Start Sprint 0"). **Sprint 0 is implemented and pushed** on `feature/sprint-0-foundation-rebuild` — camera (ADR-0001), telegraph channel (ADR-0003), and forge feel all shipped. The earlier note that "production is not authorized" is obsolete.

**ADR status is genuinely contradictory across sources — do not assume.** `docs/TECHNICAL_DECISIONS.md` lists ADR-0001/0002/0003 as **Proposed**; `docs/adr/0002-full-scene-rebuild.md`'s own header says **Accepted**; `docs/ROADMAP.md` says production was authorized. Code shipped against 0001/0003 regardless. Flagged in TECHNICAL_DECISIONS for the owner; check current state rather than trusting any single doc.

- **ADR-0004** (`docs/adr/0004-world1-single-continuous-scene.md`): World 1 becomes one continuous scene `CulDeSac_WildWestCity.unity`, zoned by `RoomManager`, boss included. **Accepted 2026-08-26** — the owner resolved all five original open questions. It is now an implementation spec, still unimplemented, and a commit still needs the owner's normal approval. Two questions remain open: barricade art for the `RoomGate`s, and the measured 10° street-vs-camera yaw mismatch.

**Why:** the owner pivots direction mid-flight (room-by-room → single scene) and expects a `technical-director` design + ADR pass before implementation, per `CLAUDE.md`.

**How to apply:** the ADR-0001/2/3 status conflict is still unresolved — check current state rather than trusting any single doc. Six older owner decisions (D1–D6 in `docs/BACKLOG.md`) still gate downstream work, notably the V3/V4 ability-system choice.

Related: [[project-asset-weight-risk]], [[project-docs-drift-from-code]], [[project-roommanager-zone-mechanism]]
