# Owner Overview Layer — Plan

## Problem

Documentation under `docs/` has grown large and technical (BACKLOG.md 1800+ lines,
PERFORMANCE_PROFILING.md 800+ lines, ADRs, story canon, v4 legacy naming, etc.).
That depth is necessary for AI agents to work correctly session to session, but it's
overwhelming for the two human owners to read directly, and it mixes current state
with historical/superseded material without always saying which is which.

Owner-confirmed pain points (2026-09-09):
- Too many files/folders to know where to look.
- Conflicting or stale info mixed with current info.
- Docs are too long/detailed to read at the owner level.

Owner-confirmed audience: the two owners (podcast hosts), not the agents and not
(yet) the audience.

## Non-goals

- Not moving, renaming, or deleting any existing agent-facing doc (`docs/*.md`
  root files, `docs/adr/`, `docs/story/`, `docs/v4/`, `docs/media/`, `docs/art/`).
  Cross-reference audit (2026-09-09) confirmed these are live and cited by exact
  path from dozens of docs and from persistent memory of 4 different agents
  (technical-director, unity-gameplay-engineer, game-designer, release-engineer).
  Moving them without a full reference-rewrite pass would break agent context.
- Not a replacement for `docs/SPRINT.md`, `docs/BACKLOG.md`, `docs/CREATIVE_STATE.md`,
  etc. — those remain the authoritative source agents read from. This layer is a
  curated *summary* pointing back at them, not a fork of them.

## Proposed structure

New directory: **`docs/overview/`** (lowercase, to visually match the existing
owner convention that ALL-CAPS root docs = agent-operational; this new folder is
explicitly the "read this first, human" set).

1. **`docs/overview/where-things-stand.md`**
   Plain-language current status: what's built and working, what's being tested,
   what's next. Sourced from `SPRINT.md`'s "WHERE WE LEFT OFF" plus any open
   BACKLOG items that matter at owner level. Updated whenever sprint status
   changes materially — this is the one file meant to answer "where are we"
   without opening anything else.

2. **`docs/overview/the-game.md`**
   Plain-language snapshot of the game as it stands: premise, world, cast,
   current levels/worlds. Pulled **only from CANON** entries in
   `CREATIVE_STATE.md` / `STORY_BIBLE.md` — WORKING/OPEN debate and internal
   design rationale left out, since that's for creative sessions, not a status
   read.

3. **`docs/overview/docs-map.md`**
   One-page guide to the rest of `docs/`: what each root file and subfolder is
   for, and which are current vs. historical-but-kept (e.g. `docs/v4/sprints/`
   is retired numbering; `docs/v4/levels/` is current room blueprints;
   `docs/design/GDD-V2.md` and `combat-system-design.md` are confirmed
   superseded/historical). Solves the "too many files, don't know where to
   look" problem without moving anything.

## Confirmed-stale files (separate, small action)

`docs/design/GDD-V2.md` and `docs/design/combat-system-design.md` are
confirmed historical by the project's own docs (`PROJECT_CONTEXT.md` and
`CREATIVE_STATE.md` respectively say so explicitly). Nothing else references
them by path except `docs/BACKLOG.md`/`PROJECT_CONTEXT.md` mentioning them as
stale. Proposal: move both into `docs/archive/`, leave a one-line pointer in
`docs-map.md`. `docs/design/fighting-style-specials.md` — leaving in place
until confirmed either way.

## Steps

- [ ] Draft `docs/overview/where-things-stand.md`
- [ ] Draft `docs/overview/the-game.md`
- [ ] Draft `docs/overview/docs-map.md`
- [ ] Move `GDD-V2.md` + `combat-system-design.md` to `docs/archive/`
- [ ] Owner review of all drafts before treating any of it as durable

## Maintenance

This is a living layer. `where-things-stand.md` should be refreshed at the end
of sessions where sprint/status changes materially, the same way `SPRINT.md`
already is.
