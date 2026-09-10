# AI Context

Fast orientation for a session picking up this project cold. This file summarizes; it does not replace the documents it points to — read those fresh, this just tells you which ones and why.

*Last updated: 2026-09-10.*

## Read this first, in order

1. **`CLAUDE.md`** (repo root) — BoxForged-specific rules that override generic studio behavior. Most important: **never commit without explicit owner approval**, and the current lifecycle-state line (see below).
2. **`.claude/rules/studio-core.md`** — the reusable studio operating contract: lifecycle gates, agent workflow, Unity MCP concurrency rule (only one agent may hold live Unity Editor access at a time — real, confirmed bug history behind this, not a theoretical caution), creative-decision discipline.
3. **`docs/SPRINT.md`** — current sprint status and "where we left off." Read this fresh every session; it is rewritten often and is the single most current operational document.
4. **`docs/BACKLOG.md`** — every bug/issue/decision with full investigation detail. `docs/KNOWN_ISSUES.md` is the short current-state summary; this is the full history.
5. **`docs/CREATIVE_STATE.md`** — CANON/WORKING/OPEN/REJECTED creative decisions. Treat CANON as settled; never silently promote WORKING/OPEN to CANON.

## Current lifecycle state

**Production.** Discovery locked 2026-08-18, production authorized 2026-08-19 ("Start Sprint 0"). Currently in **Sprint 2 — Mobile Release Readiness**, branch `feature/mobile-release-readiness`. TestFlight is live (iOS); Android has no formal release track yet, only informal tester distribution. See `docs/SPRINT.md` for exact current status — it changes session to session, don't trust this file for that level of detail.

## What this project actually is

BoxForged is a mobile action roguelite: a girl (Kid, always she/her — personal to the owner, do not revisit) wears a cardboard box and sees the ordinary world as it "really" is — household objects as weapons, kids in boxes as a growing coalition. Built **live on a podcast** by two non-professional-developer co-hosts using this AI studio; only Worlds 1 (Cul-de-Sac/Western, SpinCycle boss) and 2 (Backyard/Dojo, Grasscutter boss) are team-pre-built, World 3+ comes from audience-submitted ideas the team builds live on-air. Full detail: `docs/PROJECT_CONTEXT.md`, `docs/GAME_DESIGN.md`, `docs/STORY_BIBLE.md`.

## Owner working style — things that will save you a correction

- **The owner is a DevOps engineer, not a game developer.** Explain Unity/game-dev concepts via infra analogies (ADR ≈ RFC, `BACKLOG.md` ≈ issue tracker, CANON/WORKING/OPEN/REJECTED ≈ merged/in-review/undecided/won't-fix). Don't assume Unity-specific or narrative-design familiarity.
- **Never commit without explicit approval** — show the diff and proposed message, wait for an explicit "commit"/"yes, commit." This overrides generic studio commit behavior.
- **The owner performs all final builds and store deployments** (Xcode archive/upload, Play Console). Agents prepare release-readiness, never execute the final build/submit step.
- **Unity MCP concurrency is a hard rule, not a suggestion** — a real bug already happened from two agents touching the Editor at once (one agent's prefab edit left a transient in-memory duplicate a second, concurrently-running agent investigated as if persistent). Never run two Unity-touching agents in parallel.
- **This project's Editor does not reliably revert scene/prefab state after Play Mode** in every circumstance — always `git diff` a scene file after Play Mode work before trusting "it reverted cleanly," including your own.
- **Blender-side verification is not sufficient for a skin-weight/rig fix** — always get a real Unity-rendered check (screenshot or live view) before calling an asset fix done; a prior fix passed every Blender-side check and still corrupted the mesh in actual Unity.
- **This class-of-bug lesson recurs often: an agent's own self-report is not proof of what it did.** Verify via `git diff`, live reflection, or console state directly — several real incidents in `docs/BACKLOG.md` involved a subagent's summary being wrong or incomplete about its own work.

## Common gotchas specific to this codebase

- Legacy C# namespaces are still `Boxhead.*` — do not rename opportunistically, see `CLAUDE.md`.
- Intermittent bugs in the win/death/screen-transition path (`GameManager`, `SpinCycleAI`) have a long history of **never being root-caused from static code review alone** — every real close required an actual on-device console capture at the moment of failure. Don't guess-fix this class of bug; get a device log first.
- A scene's `NavMeshModifier` components can silently be inert depending on whether the scene bakes via the legacy Navigation window or a runtime `NavMeshSurface` — check which bake path applies before reasoning about navmesh behavior (see `docs/BACKLOG.md` B127/B133).
- Prefabs with a fully-unpacked FBX hierarchy (e.g. `pfb_enemy_grasscutter`) are fragile against future re-exports of the source FBX — Blender's exporter does not reproducibly bake the same axis/scale convention across separate export sessions, which can silently desync frozen prefab bone transforms. See `docs/BACKLOG.md` B130 before touching any similarly-structured character prefab.
- Every new/suspect 3D model needs independent orientation/axis validation — there is no universal Meshy orientation assumption that holds project-wide (`CLAUDE.md` §Model orientation correction).

## Where to look for what

| Question | Document |
|---|---|
| What's the current sprint status / what's left? | `docs/SPRINT.md` |
| What's broken right now? | `docs/KNOWN_ISSUES.md` (short) / `docs/BACKLOG.md` (full detail) |
| What's already been decided creatively? | `docs/CREATIVE_STATE.md` |
| What's the accepted architecture? | `docs/ARCHITECTURE.md`, `docs/TECHNICAL_DECISIONS.md`, `docs/adr/` |
| What changed recently, in plain terms? | `docs/CHANGELOG.md` (curated) or `docs/overview/where-things-stand.md` (plain-language, owner-facing) |
| What's next on the roadmap? | `docs/ROADMAP.md` |
| Asset pipeline / import conventions? | `docs/PROJECT_CONTEXT.md`, the `asset-pipeline` skill |
