# Changelog

User- and project-visible completed changes, organized by sprint/session rather than by individual commit. This is a curated summary, not a commit log — see `git log` for the full mechanical history, and `docs/SPRINT.md`/`docs/BACKLOG.md` for full investigation detail behind any entry here. Newest first.

---

## Sprint 2 — Mobile Release Readiness (2026-09-08 → in progress)

**2026-09-11**
- **World 2's bamboo stockade is now invisible** (owner request: "have the walls in the second level removed and only invisible walls to stop the player from falling off the edge. Same way the first level is"). All 47 `BD01_WallModule` objects had their `MeshRenderer` and `MeshFilter` stripped; every `BoxCollider` was left exactly as authored, so the sealed perimeter, enemy pathing and camera-clearance behaviour are unchanged (`LevelBuilder`'s runtime bake uses `PhysicsColliders`, not render meshes, so the navmesh is bit-for-bit the same geometry source). This brings World 2 in line with World 1's `StreetBoundary_West`/`_East` pattern — Transform + `BoxCollider` on the `Building` layer, no renderer. The 7 short return walls at the two zone boundaries (z = 17 and z = 45) were initially kept visible so the `RoomGate`s (which have no visual mesh of their own) would still read as gates, but the owner reviewed them and asked for those to go too — "I don't want the choke points they look odd" — so the perimeter is now entirely invisible. **Consequence to be aware of: World 2's zone gates now have no visual cue at all.** A player who has not yet cleared a zone walks into an invisible barrier with nothing on screen explaining it. World 1 does not have this problem because its gates are flanked by visible buildings. If that reads badly in play, the fix is a gate visual (a shimmer, a rope, a torii) rather than restoring the walls. Supersedes ADR-0005 §6's "the perimeter is diegetic … visible, and part of the art" rationale — see that ADR's amendment note.
- **Agility upgrades now actually do something noticeable — and the cards no longer lie about what they do.** Player report (owner's partner, via owner): picking Agility on the post-level screen felt like nothing changed, and he expected it to improve his parry. Traced it: Agility was wired correctly end to end, but `agilityBonus` has exactly one consumer in the whole codebase (`CombatController.DodgeRoutine`), where it adds raw metres to a 3 m dodge. The meta stat gave **+0.5 m per level** — a 17% longer roll, only visible while dodging. Three changes: (1) `ProgressionSystem.RebuildOverlay` agility scale `0.1 → 0.15`, so a level is **+0.75 m / +25%** instead of +0.5 m / +17%; (2) `CombatController` now clamps the total to 2.5× base (7.5 m) — needed because the in-run cards stack into the same additive bonus and could previously send the `CharacterController` clean through geometry; (3) the two `AgilityUp` cards were re-pointed from `DodgeSpeedUp` at some earlier stage but kept magnitudes authored as if they were percentages — `Swift Feet` was adding **+10 m** to a 3 m dodge (a 13 m roll). Re-scaled to +1.5 m, and `AgileWarrior` to +0.75 m. Card text corrected: `AgileWarrior` read *"Movement speed increased"*, which was simply false (movement speed comes from `BoxData.moveSpeed` and no upgrade touches it) — it is now "Light Footwork / Dodge roll travels further and faster."
- **Confirmed while investigating the above: nothing in the game improves parry.** The 0.4 s `parryActiveWindow` is only ever widened by the Cowboy fighting style's `WiderParryWindow` passive, chosen at run start — no stat, card or upgrade affects it. Logged as B149, since a player reasonably expected otherwise.
- **World 2's zone-0 forge bench moved 12 m deeper into the yard** (owner report: a weapon pickup was being dropped inside it). `WeaponDropTableSO_Backyard_Dojo.workbenchPositions[0]`: `(-1.7678, 0, 4.5962)` → `(8.132, 0, 11.667)`. The bench is 2.43 m wide and the pickup sat 1.0 m off its centre along that axis, so they genuinely intersected. New spot was picked by a grid search over zone 0 maximising depth subject to a hard 3 m clearance floor against every enemy spawn point, dressing prop, pickup, cardboard pile, the player spawn, the walls and the zone gate — it is the deepest position in zone 0 that holds that floor (tightest constraint is the gate line at exactly 3.0 m). Verified in-Editor: sits on `Ground` at y=0, no solid overlap, navmesh-reachable (0.10 m), and now 12.73 m from the pickup that was inside it.
- Found while measuring the above: **B146**, a pre-existing zone-gating bypass at World 2's zone-0 boundary — `RoomGate_Zone0` is 10.5 m wide across a 15 m chokepoint, leaving 1.8 m and 1.2 m walkable gaps either side of the closed gate. Not introduced by the wall change (no colliders were touched); open for an owner/designer call on the fix.

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
