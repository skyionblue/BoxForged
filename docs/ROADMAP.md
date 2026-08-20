# BoxForged Roadmap

**Status:** In production. Discovery locked 2026-08-18. Production authorized 2026-08-19 ("Start Sprint 0"). Sprint 0 (camera, telegraph, forge) complete, committed, and pushed on `feature/sprint-0-foundation-rebuild`. Phase 2 (World 1 rebuild) underway — Room 1 in progress.

This roadmap reflects the podcast production model: BoxForged is being built live to demonstrate AI-assisted game development by two non-professional-developer creators. Scope is deliberately small for the team-built portion — the audience builds everything beyond it.

---

## Scope Model

| Layer | Who builds it | What it covers |
|---|---|---|
| **Core systems** | Team (owner + AI studio) | Combat, camera, forge, progression, save, UI, audio — the reusable foundation everything else depends on |
| **World 1 — The Cul-de-Sac (Western)** | Team | Full rebuild under new camera/canon. Previously built under old camera; scenes to be recreated from scratch, not reused. |
| **World 2 — The Backyard (Dojo)** | Team | New build. Grasscutter boss. |
| **World 3+** | Audience (podcast contribution) | Framework and tools must exist so non-experts can extend the game; team does not pre-build these worlds |

The team's job in pre-production and production is to make Worlds 1 and 2 excellent, and to make sure the systems underneath them (LevelBuilder, forge, combat, camera) are solid enough that an audience-built World 3 doesn't require re-architecting anything.

---

## Phase Sequence

### Phase 0 — Pre-production (current phase)
- Convert locked creative decisions into GDD — **done (GDD v1.2)**
- Technical Design Document, Architecture reference, ADRs for camera + scene rebuild — in progress (technical-director)
- This roadmap, backlog, Sprint 0 — in progress
- Stop at pre-production approval gate. Owner explicitly authorizes production before implementation begins.

### Phase 1 — Foundation Rebuild (Production, pending authorization)
The two most urgent production tasks identified in discovery, in order:

1. **Camera overhaul** — replace the fixed top-down camera with the new fixed-follow, lower-angle, no-rotation camera. This changes how every subsequent room needs to be built, so it comes first.
2. **Forge transformation feel** — the core imagination mechanic (household object + cardboard → weapon) currently has no visual, audio, or narrative payoff. This is the single most important missing feeling in the game and the thing a podcast audience will judge the game by on first look.

Both are scoped in `docs/TECHNICAL_DESIGN.md` (technical-director, in progress).

### Phase 2 — World 1 Rebuild (Cul-de-Sac)
Rebuild all Cul-de-Sac scenes from scratch under the new camera:
- Room 1 (The Arrival), Rooms 2–4 (Ambush Alley / Saloon Front / Mailbox Row, random draw), Room 5 (The Town Square), Boss Room (The Showdown Circle — SpinCycle)
- Existing scenes (`CulDeSac_Room1`, `CulDeSac_AmbushAlley`, `CulDeSac_SaloonFront`, `CulDeSac_MailboxRow`, `CulDeSac_BossArena`) are reference only — not to be reused directly. New scenes built under current design.
- Boss intro cutscene for SpinCycle (new cutscene scope — boss intros only, locked this session)

### Phase 3 — World 2 Build (Backyard/Dojo)
- Room 1 (Dojo Courtyard), Room 2 (Garden Gauntlet), Room 3 (The Garden End — Grasscutter boss)
- Grasscutter boss AI (currently has full lore, no implementation)
- Crane Duelist enemy (locked as World 2 enemy, no implementation)
- Boss intro cutscene for Grasscutter

### Phase 4 — Podcast Launch Readiness
- Both worlds playable end-to-end
- Release-readiness checklist run
- Audience contribution framework documented (how someone extends a World 3 without professional Unity experience)

### Phase 5+ — Audience-Driven Expansion (Post-launch)
- World 3+ per podcast/audience direction
- Co-op (Cowgirl, Female Ninja) — requires the reunion-zone story beat to be resolved first
- Meta-progression (Spark), monetization integration
- HKW real-world skill unlock system (Phase 3 per GDD Section 10.1)

---

## Explicitly Not Yet Scheduled

- Elder BoxHead / World Tree scene — needed only when a late-game zone is actually built
- Versus mode, Cardboard Mill boss — no owner priority yet
- Full grey-world Imagination Overlay (beyond the forge transformation) — good future podcast episode, not urgent for Worlds 1–2 launch
