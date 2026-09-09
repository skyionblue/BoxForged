# What's In `docs/` — A Map

*This folder has grown a lot because AI agents write detailed, cross-checked
notes as they work — that's what keeps them consistent session to session.
It's not meant for a human to read end to end. This page tells you what's
where and, importantly, what's current vs. just kept for the record.*

## Start here (this folder)

- `where-things-stand.md` — current status, plain language.
- `the-game.md` — the settled story/world, plain language.
- `backlog-picker.md` — every open backlog item, grouped by what kind of work it is, so you can pick what to tackle next.

## The "operating" docs (root of `docs/`, ALL CAPS names)

These are what the AI agents read before doing any work. They're current and
actively maintained — think of them as the project's own memory, not
something that goes stale on its own:

- `SPRINT.md` — what's being worked on right now, session by session.
- `BACKLOG.md` — every open bug/issue, with history (this one's long: 1800+
  lines, because it doubles as a change log).
- `CREATIVE_STATE.md` — the actual source of story/design canon (CANON /
  WORKING / OPEN / REJECTED). `the-game.md` above is a plain-language summary
  of just the CANON parts of this file.
- `STORY_BIBLE.md` — deeper narrative detail (themes, character arcs).
- `ARCHITECTURE.md`, `TECHNICAL_DESIGN.md`, `TECHNICAL_DECISIONS.md` — how
  the game is built and why.
- `ROADMAP.md` — phase/world sequencing.
- `PERFORMANCE_PROFILING.md` — mobile performance measurements.
- `STORE_LISTING.md` — app store copy and checklist.

**Don't move or rename these** — they're referenced by exact file path from
inside the AI agents' own persistent memory. Moving them would make an agent
lose track of things mid-project.

## `adr/` — Architecture Decision Records

Numbered, permanent decision records (camera design, scene structure, etc.),
e.g. `0004-world1-single-continuous-scene.md`. These read like an old
engineering log, not a status doc — but they're live, cited constantly, and
not to be archived just because they're numbered like "old" documents.

## `story/` — the actual lore content

Full character, zone, and enemy lore that `CREATIVE_STATE.md`/`STORY_BIBLE.md`
point to for detail (e.g. `story/enemies/grasscutter-boss.md`,
`story/zones/backyard-dojo.md`). Current, not legacy.

## `v4/` — a confusing name, mixed status

This folder's name is misleading — it doesn't mean "old version 4," it's a
mix:
- `v4/levels/World1/`, `World2/`, `World3/` — **current**, detailed room-by-room
  blueprints (Unity setup, Meshy prompts, GDD per room). Read before building
  or changing any room.
- `v4/sprints/` (sprint-01/02/03) — **genuinely retired.** This was an old
  sprint-numbering scheme from before the current Sprint 0/1/2 system.
  Historical only.
- `v4/art/`, `v4/design/` — mostly still relevant to the (currently paused)
  weapon system; check `docs/BACKLOG.md` before assuming something here is
  dead.

## `media/` — marketing and outreach, not game design

Podcast/show branding, press outreach drafts, the social launch checklist.
Live and actively used for release prep (`STORE_LISTING.md` references it),
just a different topic from the game-design docs above.

## `archive/` — confirmed historical, kept for reference only

- `GDD-V2.md` — an earlier full game design doc. Superseded; kept because
  `PROJECT_CONTEXT.md` still points back at it as the source of some now-stale
  facts that needed correcting.
- `combat-system-design.md` — described a stamina-based combat system that
  was replaced by the current dodge-parry-jump design. `CREATIVE_STATE.md`
  calls this out explicitly as historical only.

`design/fighting-style-specials.md` was left in `docs/design/` rather than
archived — nothing currently confirms whether it's still relevant, so it
wasn't moved without checking first.

## `adr/`, `art/style-guide.md` — one more note

`art/style-guide.md` (cardboard-and-marker art direction) is referenced by
exact path from level-design docs. Current, not legacy, despite living
outside the ALL-CAPS root set.
