---
name: feedback-check-docs-before-flagging-gaps
description: This project already tracks release-readiness minutiae exhaustively in BACKLOG.md/PERFORMANCE_PROFILING.md/SPRINT.md — grep those before reporting something as a fresh, undiscovered gap.
metadata:
  type: feedback
---

During the 2026-09-08 mobile release-readiness audit, nearly everything that looked like a surprising discovery (first on-device profiling results and their over-budget draw calls/triangles, the undocumented 30 FPS cap, the intentional dev scene in Build Settings, a custom iOS post-build code-signing script, a prior Build-Settings GUID corruption incident) turned out to already be known, measured, and written up in detail in `docs/BACKLOG.md` (search by "B" + number, e.g. B112, B66, B98) and `docs/PERFORMANCE_PROFILING.md`.

**Why:** this project's docs are operational memory maintained by prior sessions, not a diary — they record exactly the kind of thing a release-readiness pass would otherwise have to re-discover from scratch (signing setup, known perf numbers, deliberate scene-list choices).

**How to apply:** before reporting any finding as a "gap," grep `docs/BACKLOG.md`, `docs/PERFORMANCE_PROFILING.md`, `docs/SPRINT.md`, and `docs/adr/` for related keywords first. If it's already there, report it as "known, already tracked, still open as of [date]" rather than as a new discovery — cite the existing backlog item number. Only genuinely new findings (not already logged anywhere) should be presented as new gaps.
