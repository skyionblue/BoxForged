---
name: project-documentation
description: Maintains durable game-project memory and architecture documentation. Use when creating or updating creative-state tracking, story bible, GDD, TDD, architecture, ADRs, roadmap, backlog, sprint, known issues, changelog, AI context, or project context.
---

# Project Documentation

Documentation is operational memory, not a diary. Update only what future sessions need and preserve lifecycle state.

- `PROJECT_CONTEXT.md`: product identity, target platforms, owner constraints, approved dependencies/tools.
- `CREATIVE_STATE.md`: CANON, WORKING, OPEN, REJECTED creative decisions plus discovery lock status. Treat this as authoritative for decision state.
- `STORY_BIBLE.md`: living narrative material for narrative-bearing games; characters, world rules, themes, motifs, dramatic structure, intentional ambiguity, and story-gameplay relationships.
- `GAME_DESIGN.md`: accepted player-facing rules, pillars, loop, mechanics, progression, UX, failure/win conditions. Do not use it to disguise unresolved discovery as final design.
- `TECHNICAL_DESIGN.md`: technical requirements, performance budgets, platform constraints, systems, test strategy.
- `ARCHITECTURE.md`: current component boundaries/data flows and repository/scene organization.
- `TECHNICAL_DECISIONS.md`: index/summary of accepted technical decisions and package approvals.
- `adr/NNNN-title.md`: context, decision, alternatives, consequences, status.
- `ROADMAP.md`, `BACKLOG.md`, `SPRINT.md`: planning state after the relevant work is authorized.
- `KNOWN_ISSUES.md`: reproducible unresolved issues and impact.
- `CHANGELOG.md`: user/project-visible completed changes.
- `AI_CONTEXT.md`: concise handoff state, current lifecycle gate, current focus, important gotchas, and next work.

During Discovery, prefer `CREATIVE_STATE.md` and `STORY_BIBLE.md` over filling the GDD with guesses. Remove stale statements when reality changes; do not let docs contradict code or each other without flagging it.
