# Changelog

User- and project-visible completed changes, organized by sprint/session rather than by individual commit. This is a curated summary, not a commit log — see `git log` for the full mechanical history, and `docs/SPRINT.md`/`docs/BACKLOG.md` for full investigation detail behind any entry here. Newest first.

---

## Sprint 2 — Mobile Release Readiness (2026-09-08 → in progress)

**2026-09-10**
- Marketing website (`website/`): hero-card art (Ninja, Female Ninja, Cowgirl, Cowboy) and the Skeptic enemy card swapped from broken placeholder image paths to real concept art; added Crane Duelist and Grasscutter as new World 2 enemy cards. Support page built and deployed live at `https://boxforged.com/support/`.
- Fixed `EnemyHealthBar.BuildBar()` NullReferenceException (B139) — confirmed via live reproduction of the exact reported spawn chain, zero errors across 8 real enemy spawns.
- TestFlight is live: App Store Connect app record created, build archived/uploaded, tester invites sent. An Android build is being tested informally by a second tester (no formal Google Play track yet).
- Fixed a same-window win/death race (B105): if the player dies during SpinCycle's/Grasscutter's/PermitPulper's multi-second defeat animation after the boss is already dead, the win now correctly takes priority instead of silently being dropped (owner decision).
- World 2's `NavMeshModifier` exclusion behavior confirmed working correctly under the real runtime bake (B133); the runtime navmesh re-bake measured at ~6ms, negligible against the scene-start budget (B134).
- **Corrected the record on World 2 performance (B132):** the "SRP Batcher contributes zero draw calls" finding that drove two weeks of investigation was a broken profiler counter reading in this Unity version, not a real defect — the batcher is actually on and working. Found the real triangle-budget cost instead: one decorative prop (`pfb_env_stepping_stone_tile`, 1,750 tris × 32 instances) accounts for roughly a third of the whole-scene triangle budget with no LOD (new: B144). Owner decided ADR-0009: keep the SRP Batcher, budget SetPass calls/render-thread ms instead of raw draw-call count (zero cost, the project already gets the SetPass halving for free) — `docs/TECHNICAL_DECISIONS.md`, `docs/adr/0005-world2-single-continuous-scene.md` §3, and `docs/PERFORMANCE_PROFILING.md` §8/§8.1 updated to match. One on-device validation reading still needed before the ADR fully closes.
- Created `docs/KNOWN_ISSUES.md`, `docs/CHANGELOG.md`, `docs/AI_CONTEXT.md` — closing the last outstanding process-debt item from Sprint 2.

**2026-09-09**
- Nine backlog cleanups: save-system debug panel stripped from release builds, shadow distance tuned to the camera's actual range (real mobile perf win), "enemies remaining" HUD counter can no longer read wrong, two dead scripts and 3 duplicate environment prefabs removed, a misleading code comment fixed, a defensive warning added for a zone-progression edge case, a cosmetic Inspector data quirk fixed on 4 scene objects.
- Grasscutter boss's reel-drum rig fixed for real: rotation pivot re-centered on the blade cluster (was baked near the hips), torso/waist-belt housing corrected to stay rigid with the body instead of spinning with the blades. Verified quantitatively (distance-from-pivot under rotation), not just visually.
- Fixed a recurring HUD-drift bug: `HUD3DPositioner` was repositioning based on the Editor's arbitrary Game View panel size instead of gating to real Play/device time.
- `docs/BACKLOG.md` B106 (win-screen soft-lock) confirmed CLOSED via a real on-device console trace, then reopened as B142 after recurring with a new symptom (character freeze, not just missing win screen) — see Known Issues.

**2026-09-08 — Sprint 2 opened**
- Owner: "I would like to get this game deployed for Android, and iOS." Bundle ID set to `com.boxforged`, orientation set to auto-rotate, 60 FPS cap removed.
- App icon and splash screen fixed project-wide — both were still showing the podcast's "Unboxed Heroes" branding instead of BoxForged's own art.
- A full chain of "authored but never instantiated" UI screens found and fixed: `ShopScreen`, `UpgradeScreen`, and `WorldMapScreen` were all fully built but never actually placed in the HUD prefab, so every non-boss room-clear reward screen and the entire World Map were silently dead in any real build. Fixed by wiring all three into `pfb_hud_v4`.
- `MetaScreen`'s Continue button changed (owner decision) to advance to the next zone instead of always restarting World 1.
- Store-listing copy, privacy policy confirmation, and age-rating draft completed in `docs/STORE_LISTING.md`.
- The full World 1 → World 2 player loop confirmed working end-to-end on a physical iPhone for the first time: beat SpinCycle, win screen with correct stats, Continue into `Backyard_Dojo`, beat Grasscutter, World Map showing both zones correctly.

---

## Sprint 1 — World 2 Completion & Polish (2026-09-02 → 2026-09-08)

- World 2 (`Backyard_Dojo.unity`) substantially delivered: full 3-zone continuous scene, `GrasscutterAI` boss (two-phase Kata→Rev fight, Spin-Dash ground-plane telegraph), `CraneDuelistAI`, zone re-layout to fix undersized zones/boss arena (ADR-0006), boss-intro camera and staging overhaul (ADR-0008).
- Leaf Pile Lurker cut from World 2 (owner decision — "we have enough enemies currently"); remains planned as a returning enemy in a later zone per the story bible.
- `InventoryScreen` (the Bag) now correctly pauses the game while open, with a fix for a real bug the change introduced (closing the Bag could unpause the game out from under a still-open Pause menu).
- Sprint bookkeeping reconciled: Sprint 0 formally closed, `CLAUDE.md`'s stale "back in Discovery" lifecycle line corrected to reflect that the project has been in Production since 2026-08-19.

---

## Sprint 0 / Phases 2-3 — Foundation Rebuild, World 1, World 2 (2026-08-19 → 2026-09-02)

- **Camera overhaul:** replaced the old ~41°-pitch, undocumented-yaw camera with a fixed-follow, zero-yaw, low-angle rig (ADR-0001) — the single biggest feel change in the project's history, moving the game from "a fighter clearing rooms" toward "a kid whose imagination is changing the world."
- **Attack telegraph channel** (ADR-0003): occlusion-independent overhead indicators, parryable-vs-not carried by shape (not just hue), added as a direct consequence of the camera change degrading the old whole-body-tint tell.
- **Forge transformation feel:** the moment a household object becomes a weapon now has a real anticipation/reveal beat instead of being an instant inventory swap.
- **World 1 rebuilt** as one continuous scene (`CulDeSac_WildWestCity.unity`, ADR-0004) replacing the old per-room-scene model. SpinCycle boss.
- **World 2 built** as one continuous scene (`Backyard_Dojo.unity`, ADR-0005), establishing single-continuous-scene as the project-wide default for future worlds.

---

## Pre-production (2026-08-18 and earlier)

Concept and narrative discovery locked 2026-08-18. Everything before this point is concept/creative development, not shippable production work — see `docs/CREATIVE_STATE.md` for the decision record.
