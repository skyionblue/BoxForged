# Known Issues

Reproducible, unresolved issues in the current build, and their actual player-facing impact. This is a short, current summary for anyone (owner, tester, future session) asking "what's actually broken right now" — it does not replace `docs/BACKLOG.md`, which carries full investigation detail, root-cause writeups, and closed/historical items. When an issue here is resolved, move it to `docs/CHANGELOG.md` and delete it from this file rather than marking it struck through — this file should only ever list what's *currently* true.

*Last updated: 2026-09-10.*

---

## Release-blocking (P0)

### Intermittent missing win screen after beating SpinCycle (World 1 boss)
Sometimes, after defeating SpinCycle, the win screen never appears. The HUD, buttons, and joystick stay fully responsive, but the player character stops moving on screen. Reported on a physical iPhone via TestFlight, 2026-09-10 — "tried to replicate it and it doesn't happen all the time." Genuinely intermittent; has never been reproduced on demand in the Editor. Two leading candidates (`Time.timeScale` stuck non-1, or the player's combat state stuck mid-parry/stagger at the moment of the kill) are indistinguishable from the symptom alone. Permanent diagnostic logging (tagged `[GameManager]`/`[SpinCycleAI]`, including `Time.timeScale=` and `playerCombatState=`) is already live in the TestFlight build, waiting on a real device console capture at the moment of failure. Full history: `docs/BACKLOG.md` B106/B142.

---

## Not release-blocking, real

### World 2 is well over its triangle budget — one decorative prop is the real cause
`Backyard_Dojo.unity` measured 236 draw calls / 465k triangles on-device against budgets of <100 / <300k. The draw-call side is mostly explained by a broken engine counter that made the SRP Batcher look inactive when it isn't (see the pending decision below) — the real, confirmed cost driver is a single decorative ground-tile prop at 1,750 triangles × 32 instances, about a third of the whole-scene triangle budget, with no LOD. See `docs/BACKLOG.md` B144.

### Rendering budget decided, one on-device reading still needed to close it out
Owner decided (2026-09-10, ADR-0009): keep the SRP Batcher, budget SetPass calls/render-thread ms instead of raw draw-call count. Before the ADR is fully closed, one on-device `SetPass Calls Count` reading is needed (in the 40s confirms the batcher is engaged on the real shipping build, matching the Editor finding; in the 80s would mean the analysis needs redoing on-device). The counter is already visible in the Profiler's Rendering module next to the ones already being captured — no new build needed. See `docs/adr/0009-srp-batcher-and-the-draw-call-budget.md` §7.

### No automated test coverage
There is no EditMode or PlayMode test suite anywhere in the project. All verification to date has been manual Play Mode testing and on-device playtesting.

### No measured on-device frame time
Every performance capture so far measures draw calls and triangle counts (a proxy), not actual frame time or thermal behavior. A real Xcode Instruments capture has never been done.

---

## Platform / release-process gaps (not code bugs)

- **Android has no formal release track yet.** An Android build is being tested informally (direct distribution to a tester), but there is no Google Play Console app record, signed AAB, or internal testing track set up.
- **No store screenshots or app preview video captured yet**, even though the full game loop now works end-to-end on-device. See `docs/STORE_LISTING.md` §7.
