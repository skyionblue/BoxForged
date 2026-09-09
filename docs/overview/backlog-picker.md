# Backlog — Pick Your Next Task

*Last updated: 2026-09-09. A curated, plain-language view of what's still open
in `docs/BACKLOG.md` (1800+ lines), grouped by what kind of work it actually
is. Full technical detail for any item lives at its `B##` entry there — search
for the number. Nine items (B17, B20, B95, B81, B16, B94, B97, B19) are fully
fixed as of 2026-09-09 and are not listed below. B125 and B126 were decided
(not fixed, but resolved — see their `BACKLOG.md` entries) and are also
dropped from this list.*

Check items off as they get done, or just tell me the B-number(s) you want to
tackle next.

## Held for TestFlight (owner decision 2026-09-09 — known issues, not blockers)

- **B4 / B44 / B45** — Certain Epic/Legendary weapons (Bo Staff, Pressure Cannon, Magic Wand, Shuriken) either freeze combat on Special or double-fire it. Root cause is two overlapping ability systems needing consolidation (~2-3 days of real design + implementation work). Owner chose to ship TestFlight with this as a known issue rather than delay or patch around it — revisit after initial tester feedback.

## Real bugs, not yet fixed

Actual defects players could hit. Some are small, a couple need real investigation time.

- [ ] **B91** — Building colliders are wider than their meshes; you can walk into porches/facades in the Cul-de-Sac.
- [ ] **B92 / B93** — Boss and enemy NavMesh sizes exceed the project's baked settings; some boss attacks have no landing-point safety clamp.
- [ ] **B106** — Occasionally the win screen doesn't show after beating a boss. Intermittent, logging is now in place to help catch it next time it happens.
- [ ] **B139** — A recurring error in the enemy health bar code, seen on a real device. Not yet root-caused.
- [ ] **B117** — A boss dash move validates where it lands but not the path it takes to get there.
- [ ] **B124** — One planned enemy behavior (grass/petals kicking up when the boss is dormant) was never implemented.

## Needs a `technical-director` scoping pass (architecture-level)

Bigger picture items — not something to fix in isolation.

- [ ] **B127 / B128 / B133 / B134** — World 2's NavMesh setup has a cluster of related issues (inert modifiers, no bounds, runtime-bake questions).
- [ ] **B132** — World 2's rendering performance (SRP Batcher) has never been verified on a real device; one repeated wall piece is the single biggest draw-call cost in the scene.
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
