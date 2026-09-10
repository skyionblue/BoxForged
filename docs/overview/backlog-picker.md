# Backlog — Pick Your Next Task

*Last updated: 2026-09-10. A curated, plain-language view of what's still open
in `docs/BACKLOG.md` (2000+ lines), grouped by what kind of work it actually
is. Full technical detail for any item lives at its `B##` entry there — search
for the number. Items fixed/resolved/decided as of 2026-09-10 are dropped from
this list: B105, B127, B132, B133, B134, B139, B143, B144, B145. (Sept 9's
nine items — B17, B20, B95, B81, B16, B94, B97, B19 — and B125/B126 were
already dropped as of the last update.)*

Check items off as they get done, or just tell me the B-number(s) you want to
tackle next.

## Held for TestFlight (owner decision 2026-09-09 — known issues, not blockers)

- **B4 / B44 / B45** — Certain Epic/Legendary weapons (Bo Staff, Pressure Cannon, Magic Wand, Shuriken) either freeze combat on Special or double-fire it. Root cause is two overlapping ability systems needing consolidation (~2-3 days of real design + implementation work). Owner chose to ship TestFlight with this as a known issue rather than delay or patch around it — revisit after initial tester feedback.

## Real bugs, not yet fixed

Actual defects players could hit. Some are small, a couple need real investigation time.

- [ ] **B106 / B142** — Occasionally the win screen doesn't show after beating SpinCycle, and the character freezes (HUD/joystick stay responsive). The most active bug right now — genuinely intermittent, never reproduced on demand. Logging is in place; needs a real device console capture at the moment it happens next.
- [ ] **B91** — Building colliders are wider than their meshes; you can walk into porches/facades in the Cul-de-Sac.
- [ ] **B92 / B93** — Boss and enemy NavMesh sizes exceed the project's baked settings; some boss attacks have no landing-point safety clamp.
- [ ] **B117** — A boss dash move validates where it lands but not the path it takes to get there.
- [ ] **B124** — One planned enemy behavior (grass/petals kicking up when the boss is dormant) was never implemented.

## Needs a decision, not just a fix

- [ ] **ADR-0009** — *New, decided 2026-09-10, one step left.* A two-week-old performance worry ("the rendering batching system isn't helping") turned out to be a broken measurement, not a real problem — see `where-things-stand.md`. Owner already chose how to re-measure it going forward; just needs one on-device reading to confirm and fully close out.

## Needs a `technical-director` scoping pass (architecture-level)

Bigger picture items — not something to fix in isolation.

- [ ] **B136** — Ten-plus places in the code independently pause/unpause the game with no shared system managing it — works today, fragile long-term.

## Worth doing before a public (non-TestFlight) release

- [ ] **B14** — No save-file version migration path yet. Flagged in the backlog itself as "do before first release."
- [ ] **B15** — Some shared data assets have state that won't work correctly once co-op is added (co-op is designed-in, just not built yet).
- [ ] **B18 / B36** — Two small performance patterns (destroy+recreate instead of reuse) — worth pooling if profiling ever shows it matters, not urgent otherwise.

## Asset / content gaps

- [ ] **B51** — One weapon (Six-Shooter) is missing its pickup-item art; only has the equipped version.
- [ ] **B79 / B80 / B84** — A few environment props are undersized, too small to see, or missing their model file entirely.
- [ ] **B29** — No sound effects exist yet for the attack-warning system.

## Testing

- [ ] **B7** — No automated tests exist anywhere in the project yet (also called out as a release-readiness gap in `docs/SPRINT.md`).

## Old items worth double-checking before acting on

These were written early on, before the camera and attack-warning systems were
actually built and shipped. They might already be resolved by later work —
worth a quick "is this still true?" check before treating them as real tasks:
**B1, B2, B10, B11, B12, B13, B27**.
