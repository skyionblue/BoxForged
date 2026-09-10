# Sprint 2 — Mobile Release Readiness

**Status: AUTHORIZED, in progress — aiming for TestFlight (internal + external), not full store submission yet.** Owner said "I would like to get this game deployed for Android, and iOS" (2026-09-08), immediately after closing out Sprint 1's crash-recovery cleanup commit (`ac1fa96e`) on `feature/sprint-0-foundation-rebuild`. New branch: `feature/mobile-release-readiness`. Sprint 1 (below) remains open in parallel — additive scope, not a supersession.

**Branch:** `feature/mobile-release-readiness`, HEAD `02509e37` (pushed).

**WHERE WE LEFT OFF (2026-09-08, end of session):** the whole World 1 → World 2 player loop is now confirmed working end-to-end on a physical iPhone — beat SpinCycle, win screen shows with correct stats, Continue advances to `Backyard_Dojo`, beat Grasscutter, World Map opens correctly and shows both zones with a corrected "Dojo" label. Store-listing prep is essentially done except two small items. **Next session should start at "Ready for TestFlight" below.**

**WHERE WE LEFT OFF (2026-09-10, end of session):** owner asked to close out B132, B133/B134, and B105 before ending this sprint, plus the remaining process-debt item. All four resolved: B105 fixed (owner decided win-over-death priority when both happen in the same window); B133/B134 resolved (NavMesh modifiers confirmed working under the runtime bake, B127 closed as moot, B134 downgraded — negligible cost); B132 **retracted**, not just fixed — the "SRP Batcher contributes zero" finding that drove two weeks of investigation turned out to be a broken profiler counter, not a real defect (see ADR-0009). `docs/KNOWN_ISSUES.md`, `docs/CHANGELOG.md`, `docs/AI_CONTEXT.md` created, closing the last process-debt item. **New open items from this session:** ADR-0009 needs an owner decision (SRP Batcher rendering-budget strategy — see below), and B144 (geometry density, not B132's wall-module theory) is now the real live rendering item. B142 (intermittent no-win-screen) recurred again via owner report, still blocked on a device console capture.

**WHERE WE LEFT OFF (2026-09-09, end of session):** a large batch of backlog fixes landed (see `docs/BACKLOG.md` for B17/B20/B95/B19/B16/B81/B94/B97, all fixed; B4/B44/B45 explicitly held for TestFlight by owner decision; B125/B126 decided, no World 1/2 content changed). Separately, the Grasscutter boss's reel got a real technical-art pass: its rotation pivot was baked near the hips instead of the blade cluster (fixed directly in `pfb_enemy_grasscutter.prefab`, since that prefab has the model fully unpacked/disconnected from `Grasscutter.fbx` — re-exporting the FBX alone cannot fix bone *positions* there, only mesh/weight data), and the torso housing plus a waist belt/buckle detail were incorrectly rigid-bound to the spinning reel instead of the body (fixed via weight reassignment in Blender, re-exported). Both fixes verified quantitatively (distance-from-pivot under rotation), not just visually — a visual-only read led to one false-positive "fix" that was correctly caught and reverted. Also root-caused and fixed a recurring "HUD elements silently drift position" issue: `HUD3DPositioner.cs` was `[ExecuteAlways]` and repositioned based on `Screen.width/height` even in the Editor, where that reflects the Game View panel's arbitrary size, not a real device — gated to `Application.isPlaying`. **A full Grasscutter model replacement (using `raw-assets/models/zips/Grasscutter_part-segmentation.fbx`, a raw unrigged ~567k-vertex part-segmented export) was scoped but explicitly held** — see `grasscutter-model-replacement-plan.md` at the repo root for the full writeup; the source file has no armature/weights and would need ~95%+ decimation, fresh rigging, a full reweighting pass, and a `pfb_enemy_grasscutter.prefab` rebuild. **TestFlight setup itself is unchanged and still the next real-world action — see "Ready for TestFlight" below.**

## Ready for TestFlight — what to actually do next

**Status 2026-09-10: DONE — owner created the App Store Connect app record, archived/uploaded the build, and sent tester invites.** TestFlight is live. Next actions are in **[§Now that TestFlight is live](#now-that-testflight-is-live)** below. Steps 1–4 below are kept for reference/reruns (e.g. the next build upload).

All of the steps below are owner-performed (App Store Connect / Apple Developer portal / Xcode) — not something an agent does, per this project's own rule that final builds and deployments are the owner's.

### Step-by-step walkthrough

**1. Create the App Store Connect app record** (only needs doing once, since the bundle ID changed this session).
- Go to [appstoreconnect.apple.com](https://appstoreconnect.apple.com) → **My Apps** → **+** → **New App**.
- Platform: iOS. Name: BoxForged. Primary language: English. **Bundle ID**: select `com.boxforged` from the dropdown.
  - If `com.boxforged` isn't in that dropdown yet, register it first at [developer.apple.com/account](https://developer.apple.com/account) → **Certificates, IDs & Profiles** → **Identifiers** → **+** → **App IDs** → register `com.boxforged`. Then it'll show up back in App Store Connect.
- SKU: any unique internal label (e.g. `boxforged001`, never shown to users). Click **Create**.

**2. Archive and upload the build from Xcode.**
- Open the Xcode project Unity generated from the iOS build.
- Device/scheme selector at the top must say **"Any iOS Device"**, not a simulator.
- **Product → Archive** (takes a few minutes).
- In the **Organizer** window that opens: select the archive → **Distribute App** → **App Store Connect** → **Upload** → accept the defaults (automatic signing is already configured via the post-build script, Team `V62D5FT8F5`).

**3. Fill in TestFlight info** (back in App Store Connect, once the build finishes processing — usually 10–60 min, appears under the app's **TestFlight** tab).
- **Test Information**: Beta App Description, **Feedback Email** → `boxforged@gmail.com` (owner decision, 2026-09-09), **Privacy Policy URL** → `https://boxforged.com/privacy/` (already live and confirmed accurate, 2026-09-08).

**4. Add testers.**
- **Internal Testing** — your own App Store Connect team, no Apple review, available almost immediately.
- **External Testing** — a group + public link, goes through Apple's Beta App Review (lighter than full App Store review, typically under 48 hours).

### Still-open decisions blocking full completion (not TestFlight itself)
- ~~Support page (`https://boxforged.com/support/`) not deployed~~ — **RESOLVED 2026-09-10.** Built (`website/support/index.html`, commit `a2b574f9`), pushed to `origin`, and owner-deployed to the live server. Live for the store listing now.
5. Do the archive/upload in Xcode yourself (per project rule — this isn't something an agent does). Once a build is in TestFlight, this sprint's remaining items (screenshots, video, the performance question) can happen in parallel with real tester feedback.

## Now that TestFlight is live

**Status 2026-09-10:** app record created, build archived/uploaded, tester invites sent (owner-performed, this session). None of this is store submission — TestFlight is a beta channel, not a public release.

1. **If any invited testers are External Testing group members** (not just your own App Store Connect team), the build goes through Apple's Beta App Review first — typically under 48 hours. Internal testers can install immediately with no review. Nothing to do here but wait/check App Store Connect's TestFlight tab for review status.
2. **Collect and triage tester feedback** as it comes in (TestFlight's built-in feedback, or the `boxforged@gmail.com` Feedback Email set in step 3 above). Log anything actionable in `docs/BACKLOG.md`.
3. **Android — in progress, not yet a formal Play Console track.** Owner's partner is testing an Android build as of 2026-09-10 (informal/direct distribution, not confirmed as a Google Play internal testing track). Still needed for a real release: an app record in Google Play Console, a signed AAB upload, and an internal testing track/tester list if not already using one — same owner-only build/deploy boundary as iOS.
4. **Independent of tester feedback, in parallel:**
   - ~~Deploy the support page~~ — **DONE 2026-09-10**, see the resolved item just above.
   - Capture real screenshots and an app preview video now that the full World 1 → World 2 loop works on-device (`docs/STORE_LISTING.md` §7).
   - ~~`docs/BACKLOG.md` B139 (recurring `NullReferenceException` in `EnemyHealthBar.BuildBar()`)~~ — **FIXED 2026-09-10**, see BACKLOG for the live Play Mode repro that confirmed it.
   - B132 (World 2 over its draw-call/triangle budget) is safe to leave for TestFlight but should get a `technical-director` scoping pass before a public release.

## Confirmed fixed and tested this session (2026-09-08), full detail in `docs/BACKLOG.md`

- **B112/B131/B137/B138/B140/B141 — the whole win/progression loop.** In order of discovery: 60 FPS cap removed; `WorldMapScreen` hardcoded a dead scene name (fixed, but the screen turned out to have zero live instances anywhere — see B141); `ShopScreen`/`UpgradeScreen` were fully built but never instantiated (fixed); `UpgradeScreen.OnDestroy()` was missing a guard its siblings had, causing a silent un-pause (fixed); `MetaScreen`'s Continue button always restarted zone 0 by design until the owner asked for it to advance to the next zone instead (changed, `GameManager.ContinueToNextZone()`); `WorldMapScreen`/`pfb_world_map` had zero live instances at all — same class of bug as `ShopScreen`/`UpgradeScreen`, plus two more bugs only visible once it was finally instantiated (auto-showed at game start; zone-1 label read "Town Square" instead of "Dojo") — all fixed and verified via a real on-device console trace, twice (the win-screen chain proved intermittent once between two "confirmed working" tests — see B106's addendum — diagnostic logging was made **permanent**, not stripped, because of this).
- **B131's earlier fix was real but was unreachable** until B141 landed — worth remembering if a future session sees WorldMap-related code that looks already-fixed but isn't observably working; check whether the screen has a live instance at all before assuming the bug is elsewhere.
- App icon consistency (iOS + Android, all slots now `AppIcon_BoxForged.png`), splash screen logo (was showing the podcast's "Unboxed Heroes" wordmark, now the correct BoxForged art), loading screen art swap — all done and device-verified.
- `WeaponGripTest.unity` stripped from the build's scene list (owner decision).

## Still open — real, but none block TestFlight

- ~~**B132** — World 2's SRP Batcher contributes zero on-device~~ — **RETRACTED 2026-09-10.** `technical-director` found this was a measurement artifact, not a real defect: `SRP Batcher Draw Calls Count` reads 0 in Unity `6000.5.3f1` whether the batcher is on or off, so the reading carried no information. The batcher is actually engaged and working (44 SetPass calls with it on vs. 85 with it off, measured live via A/B toggle). The real architectural finding: TDD §3.2's `<100 draw calls` budget is structurally unreachable while the SRP Batcher is on, because Unity's batching mechanisms are mutually exclusive per-renderer and only GPU instancing reduces the raw count. **New owner decision needed — see ADR-0009** (`docs/adr/0009-srp-batcher-and-the-draw-call-budget.md`): keep the SRP Batcher and re-budget on SetPass calls/render-thread ms instead (recommended, zero cost), or disable it project-wide and chase the literal draw-call number via instancing (real churn, unmeasured mobile tradeoff). The actual triangle-budget culprit was found along the way and is unrelated to this decision — see B144 below.
- ~~**B133/B134**~~ — World 2's NavMesh findings — **RESOLVED 2026-09-10.** B133: verified live that `NavMeshModifier` exclusions DO work correctly under World 2's actual runtime bake (unlike the Editor's legacy bake) — B127 closes as moot, no owner decision needed, the system already does what its author intended. B134: the runtime bake costs ~6ms against a 500ms scene-start budget — negligible, downgraded P1→P3, B128's bounds-volume fix deliberately not implemented (the justification for it is gone, and it turned out to be a bigger shared-system change than assumed). Two new real defects found along the way: **B145** (`KoiPond`'s navmesh contribution silently differs between Editor and a real device build — a Read/Write flag gap) and **B143** (the committed Editor `NavMesh.asset`/`NavigationStatic` flags are dead weight at runtime in both worlds).
- ~~**B139**~~ — recurring `NullReferenceException` in `EnemyHealthBar.BuildBar()` — **FIXED 2026-09-10**, confirmed via live Play Mode repro of the exact reported call chain (`RoomManager.ActivateRoom` → `EnemySpawnPoint.SpawnNext` → `Instantiate` → `EnemyHealthBar.Awake`/`BuildBar`), zero errors across 8 real enemy spawns.
- ~~**B105**~~ — a known, narrow race between `TriggerWin()`/`HandlePlayerDeath()` sharing one state guard — **FIXED 2026-09-10.** Owner decision: a boss defeat already in flight now wins the race over a same-window player death. Compiles clean; not yet verified against a clean on-device repro of the original race (which was only ever reproduced as a test-methodology side effect, not a real player scenario).
- **NEW — B144 (P1, replaces B132 as the live rendering item):** Dojo ENV props are geometry-dense with no LODs. One decorative stepping-stone prop is 1,750 triangles and 32 instances of it cost a third of the whole-scene triangle budget — the actual fix target, not the wall-module stockade B132 originally blamed (measured cost: only 10 draw calls).
- ~~**NEW — owner decision pending: ADR-0009**~~ — **DECIDED 2026-09-10.** Owner chose Option A: keep the SRP Batcher, budget SetPass calls/render-thread ms instead of raw draw-call count. `docs/adr/0005-world2-single-continuous-scene.md` §3, `docs/PERFORMANCE_PROFILING.md` §8/§8.1, `docs/TECHNICAL_DECISIONS.md` all updated to match. **Still needs:** one on-device `SetPass Calls Count` reading (40s range expected) to close out ADR-0009's own validation requirement.
- No automated EditMode/PlayMode test coverage exists anywhere in the project.
- ~~`docs/KNOWN_ISSUES.md`, `docs/CHANGELOG.md`, `docs/AI_CONTEXT.md` don't exist~~ — **CREATED 2026-09-10**, all three now exist and are current as of that date.
- Screenshots and an app preview video for the store listing — safe to capture now that the full loop works, per `docs/STORE_LISTING.md` §7.
- A real Xcode Instruments "Pass B" (frame-time/thermal) capture has still never been done — everything measured so far is draw-call/triangle counts (Pass A equivalent via the profiler buffer), not actual FPS.

---

# Sprint 1 — World 2 Completion & Polish

**Status: AUTHORIZED.** Owner closed Sprint 0 and opened this sprint 2026-09-02, in the same decision that authorized committing Sprint 0's outstanding work and correcting `CLAUDE.md`'s stale lifecycle-state line (see `CLAUDE.md` §Current lifecycle state). Same branch, same production authorization (2026-08-19) — this is a bookkeeping/scope split, not a new authorization gate.

**Branch:** `feature/sprint-0-foundation-rebuild` (unchanged — no new branch cut)

**Scope:** exactly the items already carried in Sprint 0's **§Still open for World 2** (below, now historical), minus Leaf Pile Lurker — **REJECTED for World 2** (owner decision 2026-09-02, "we have enough enemies currently"; assets deleted, not just left unbuilt). This is a World-2-scope cut only, not a cut of the enemy concept from the game — see `docs/CREATIVE_STATE.md`, which still carries it as a planned *returning* enemy in a later zone. Nothing else new invented here — this section exists so the still-open list has a current home instead of living inside a closed sprint's retrospective.

Validation not yet discharged:
- **ADR-0006 §Validation 10** — the 20m arena's dash-lane telegraph is Play-Mode-verified only; not yet confirmed on-device or with a human playtest of that specific mechanic. The arena size is conditionally granted on this passing.
- **ADR-0001's validation checklist** — still pending a real-device measurement pass (Skeptic Grunt wind-up on-screen from behind, SpinCycle's slam staying in frame, parry-timing feel), now applying to World 2's camera work too.

Correctness items found and deliberately not fixed:
- **B117** — `GrasscutterAI.SpinDash`'s NavMesh clamp validates the landing point, not the path to it.
- **B121** — B116's own completion note misreports what it changed; its M2 clearance figure was computed from the spec rather than measured.
- **B120 (World 1 half)** — `SpinCycleAI`'s boss intro still doesn't lock player movement, and still has the same 2-second-blend "hard cut" bug Grasscutter had before today's fix. Not backported.

Decisions the owner or a designer owns (logged as "decide, don't just fix" — do not resolve these by unilaterally picking an option):
- **B125** — zone 2 has no central obstruction now that the cherry tree sits at the north rim (CANON owner placement); the "orbital" combat-movement grammar both ADR-0006 §1.1 and the zone spec assumed no longer has anything to orbit. Needs `game-designer` + `technical-director`. **Not** a reason to move the tree.
- **B126** — the built cherry tree's canopy exceeds ADR-0006 §1.1's envelope and has **no collider at all** — any `Physics.Raycast` clearance check passes straight through it. Either amend the envelope to the built asset or re-author the asset.
- **B127** — the scene's 8 `NavMeshModifier` components are inert under the legacy bake, so six court props carve permanent navmesh holes their author asked to exclude. Two mutually-exclusive fixes exist; do not install `NavMeshSurface` in this one scene while World 1 uses the legacy path.
- **B128** — the bake has no bounds volume: 1,216 m² walkable against ~318 m² of playable Blossom Court.
- **B124** — the GDD's "dormant in the tall grass … grass and petals kick up" beat has no implementation — no grass GameObject, no kick-up VFX.

Not started, not in this sprint's scope:
- `docs/v4/levels/World2/backyard-dojo/gdd.md` §12 open questions Q1–Q4, including the Crane Duelist's Beak Thrust parry window (explicitly playtest-tuning work).

---

# Sprint 0 — Foundation Rebuild (CLOSED 2026-09-02)

**Status:** **CLOSED.** Sprint 0's own three scope items were COMPLETE and committed as of 2026-08-19 (camera overhaul, attack telegraph channel, forge transformation feel — acceptance-criteria gaps noted per item below, still open, now tracked under Sprint 1 above where relevant). Formally closed 2026-09-02 — see **[§Open for owner decision](#open-for-owner-decision)** for why it stayed open this long.

**Sprint 0 was never formally closed at the time, and no successor sprint document was opened until now.** The branch it was created on carried all of ROADMAP Phase 2 (World 1 — the continuous city scene) and most of Phase 3 (World 2 — the Backyard/Dojo, the Grasscutter boss, and the Crane Duelist) without the tracking document advancing. That work is recorded in **[§Work delivered on this branch after Sprint 0's scope](#work-delivered-on-this-branch-after-sprint-0s-scope)** below, which is why the "Explicitly Out of Scope for Sprint 0" section carries a correction of record rather than being deleted.

**Authorization:** Owner said "Start Sprint 0" (2026-08-19), following the discovery lock and pre-production authorization of 2026-08-18 recorded in `docs/CREATIVE_STATE.md` §Discovery lock status. See §Authorization record at the foot of this document.
**Branch:** `feature/sprint-0-foundation-rebuild` (created 2026-08-19; HEAD `497e0031` as of 2026-09-02, fully pushed, no divergence from upstream)
**Last reconciled against git history:** 2026-09-02
**Depends on:** `docs/TECHNICAL_DESIGN.md` and `docs/ARCHITECTURE.md` (technical-director, in progress) for exact Cinemachine rig and forge-feedback implementation specifics.

---

## Why this sprint exists

Discovery surfaced one diagnosis that matters more than any single feature: **the game currently feels like a fighter clearing rooms, not a kid whose imagination is changing the world.** Two root causes were identified:

1. The camera sits higher and further back than the owner wants (technical-director found the real rig is pitched ~41° with an undocumented diagonal yaw — not the flat top-down the docs claimed), and needs replacing before any more rooms are built under it.
2. The forge — the actual imagination mechanic — has no visual, audio, or narrative payoff. Weapons currently just appear in inventory.

Everything downstream (the World 1 rebuild, the World 2 build) depends on these two things being right first. Building rooms under the old camera, or shipping more silent forge transformations, would mean redoing work later.

---

## Scope

### 1. Camera overhaul — **IMPLEMENTED 2026-08-19, one acceptance-criteria gap open (see below)**
- Replace the real existing rig — `FollowOffset (7.88, 11, -10)`, FOV 40, pitch ~40.8°, and an **undocumented yaw of ~−38.2°** (the camera has never looked straight down +Z) — with the new fixed-follow, lower-angle, zero-yaw camera per **[ADR-0001](adr/0001-fixed-low-follow-camera.md)**.
- Recommended rig: pitch 36°, vertical FOV 45°, height 5.5 m, `FollowOffset (0, 5.5, -7.57)`. These are derived values, not a placeholder — see the ADR for the acceptance-criteria table (ground ahead/behind/lateral width/top ray) that any implementation must satisfy on every supported aspect ratio.
- **This item is coupled to item 3 below (telegraph channel) — see [ADR-0003](adr/0003-attack-telegraph-channel.md).** The lower camera degrades the game's only current attack-tell mechanism (whole-body color tint). Shipping the camera change without the telegraph channel is the one outcome that should not happen; the documented fallback is holding the camera closer to its current pitch.
- **Acceptance criteria:**
  - Camera never rotates (mobile joystick control stays predictable); yaw fixed at 0°
  - Ground ahead ≥ 12 m, ground behind ≥ 4 m (hard floor 3.3 m), lateral width ≥ 16 m, top ray ≥ 10° above ground — on every supported aspect ratio (4:3 to 21:9)
  - A Skeptic Grunt approaching from directly behind stays on screen for its full 0.6s wind-up
  - SpinCycle's airborne slam stays in frame through its full arc (escalate to a per-boss-room camera profile if not — see ADR-0001 alternative 5)
  - Parry timing feel is unchanged — parry read must not depend on camera distance
  - Camera does not clip through walls/props; level builder enforces ≥8m rear / ≥6m overhead clearance along the camera axis (no deoccluder — see ADR-0001 §2.7)

**Implementation status:** rig built exactly to spec (pitch 36°, FOV 45°, height 5.5m, offset `(0, 5.5, -7.57)`, `CinemachineHardLookAt` removed, yaw fixed at 0°). Follow-up fixes done: `SpinCycleAI` FOV constant, `PlayerController` arena radius, occlusion system consolidation (kept `CameraOcclusion`, deleted `BuildingOcclusionFader` — which had a latent bug beyond the reason for removal, see B28), `EnemyHealthBar` billboard fix, stale comments corrected, a new editor-only `LevelBuilder.ValidateCameraClearance()` check.

**Aspect-ratio gap fixed 2026-08-19 (B27):** the static rig only cleared its ≥16m lateral-width floor at ~20:9 (modern phones); 16:9 (the actual target hardware) and 4:3 fell short. New `Core/AspectAdaptiveCameraFraming.cs` component scales camera distance per-aspect (pitch/FOV untouched) to guarantee the floor everywhere. Verified against all four target aspects — see B27 for the full table. All acceptance criteria below are now met on every supported aspect ratio, not just the one originally measured.

**Bug-fix pass 2026-08-19:** code review and performance review found the FOV constant, arena radius, and `EnemyHealthBar` dimensions above were changed in C# but never actually took effect — the prefabs on disk still had the old serialized values (Unity does not retroactively apply a new script default to an already-serialized field). Fixed directly on the prefabs via Unity MCP and verified by reading the values back (see `docs/BACKLOG.md` B8, B9, B39). `LevelBuilder.ValidateCameraClearance` was also found to hang Play Mode entry and flood the console (unrestricted `~0` layer mask, up to ~40 `Debug.LogError` calls with stack traces) — fixed to mask on the Building layer only and aggregate into one `Debug.LogWarning` per run (see B33). `CameraOcclusion`'s per-frame, per-hit `GetComponentInChildren<Renderer>()` call was also cached (B35).

**ADR-0001's own validation checklist is still pending — "Accepted" means the design decision is approved, not that on-device validation has happened.** Still needed: a real-device measurement pass, confirming a Skeptic Grunt's full wind-up stays on screen from directly behind, confirming SpinCycle's airborne slam stays in frame through its arc, and confirming parry-timing feel is unchanged. This is `qa-engineer` work, not yet performed.

### 2. Attack telegraph channel — **IMPLEMENTED 2026-08-19** (new — blocking dependency of item 1)
- Per **[ADR-0003](adr/0003-attack-telegraph-channel.md)**: there is currently no telegraph system in BoxForged. Every attack tell is a whole-body color tint, and parryable-vs-unparryable is encoded **entirely in hue** (this is also a pre-existing colorblind-accessibility gap the camera change exposes rather than causes).
- Add an occlusion-independent overhead indicator, carry parryability on shape rather than hue, add distinct audio per attack class, keep the existing body tint as reinforcement.
- **Acceptance criteria:**
  - ✅ Parryable vs. un-parryable carried by shape (filled circle = parryable, filled triangle = un-parryable) — not yet confirmed in an actual colorblind simulation or desaturated screenshot (manual QA step, not yet performed)
  - ✅ Telegraph remains readable through occlusion — achieved via a `ZTest Always` unlit overlay shader rather than the originally-suggested overlay-camera-stack pattern (that pattern turned out to be screen-fixed HUD elements, not world-anchored trackers — documented deviation, see below)
  - ✅ Pooled (8 indicators, oldest-active eviction) — no per-wind-up instantiation
  - Audio per attack class: code path exists (`SoundData.cs` gained 4 new `SoundEvent`s) but **no actual audio clips are authored yet** — currently a safe silent no-op. See `docs/BACKLOG.md` B29.

**Implementation notes:** built a self-bootstrapping `AttackTelegraphService` (same singleton pattern as `AudioManager`) plus a pooled `AttackTelegraphIndicator` using procedurally-built circle/triangle meshes. Wired into every enemy that actually has a real wind-up window: `BasicEnemyAI`, `SkepticGruntAI`, `HitchingHoundAI`, `WagonWheelRollerAI`, `MilepostMarshalAI`, `SprinklerSentinelAI`, `SpinCycleAI`, and `PermitPulperBossAI` (the last one dormant per the D1 decision, but already had the same `WindUp` seam, so it's now consistent if ever reactivated). Two attacks with dynamically-resolved parryability (SpinCycle's Haymaker, PermitPulper's ShredSpin) were conservatively classified as their un-parryable shape rather than guessed — see B30. `PermitPulperAI`/`NoticePusherAI` were excluded — they have no wind-up window at all, which is a balance change outside this task's scope (B31).

**Bug-fix pass 2026-08-19:** performance review found the shader was referenced only via a runtime `Shader.Find` string lookup, which Unity strips from real builds — the whole system rendered nothing on-device despite working in the Editor (**B32, critical**). Fixed by creating `mat_TelegraphOverlay.mat`, adding a serialized `_overlaySourceMaterial` field to `AttackTelegraphService`, and placing a new `pfb_AttackTelegraphService` prefab in every scene that already carries `pfb_AudioManager` (the service was previously never placed anywhere, so the serialized field alone would not have created a real build reference). Also collapsed 8 per-instance runtime materials down to 2 shared ones. Code review separately found indicators flashed once at the world origin on activation before jumping to the correct position (`transform.position` was only ever set in `Update()`, which does not run until the frame after a coroutine-driven `Activate()` call) — fixed by setting position explicitly inside `Activate()`.

**Note on scope:** the earlier task briefs for this sprint explicitly said not to touch the existing Cul-de-Sac scenes (they're superseded, not edited, per the owner's scene-rebuild directive). The fix-pass agent placed the new `pfb_AttackTelegraphService` instance into all 6 existing scenes (the 5 Cul-de-Sac rooms + `ForgeLoop_Test`) because that reference is required for the shader fix to actually be testable/functional at all right now, and it wasn't carried forward as a constraint into the fix-pass briefing. This is additive only — no room/encounter content was touched, and `RoomManager`'s scene-local data (the thing ADR-0002 says must be extracted before scenes are touched) is unaffected. Judged low-risk, but flagged here since it's a deviation from an explicit earlier instruction, not a silent one.

### 3. Forge transformation feel — **IMPLEMENTED 2026-08-19, awaiting review + art wiring**
- When a household object + cardboard becomes a weapon at the Forge Workbench, the moment must read as imagination happening, not a crafting transaction.
- **Acceptance criteria:**
  - ✅ Transformation is not an instant inventory swap — includes a brief visible transformation beat (`ForgePresenter`: anticipation ghost → hide/grow tween)
  - ✅ A color bloom fires — drives `ImaginationRestore_Volume` intensity, keyed to weapon tier (box-palette-specific tinting not yet threaded through — flag for follow-up if that specificity matters, currently tier-keyed only)
  - ✅ Sound design hook exists (`ForgePresenter` SFX beat) — actual resonant/papery sound asset not yet authored, code path is ready
  - ✅ World-space TMP reveal text beat exists in-sequence — no-ops gracefully until a text prefab is assigned (placeholder copy only, not final flavor text — that's a writing decision, not an engineering one)
  - ✅ Persistent tier glow implemented (`WeaponTierGlow`) — no-ops gracefully until glow VFX prefabs are wired to weapon prefabs
  - ✅ Built on existing `ForgeController` / `WeaponInstance` / `WeaponDurability` systems via `OnWeaponForged` / `OnWeaponUpgraded` — `ForgeController`'s core logic untouched
  - ✅ **Plays in-world, at the workbench** — `ForgePanel` now closes (unpausing) on successful forge/upgrade and hands off to the world-space sequence, per the owner's locked decision

**Remaining before this is player-visible (asset wiring, not code — see `docs/BACKLOG.md` B24):**
1. Create a 3D `TextMeshPro` prefab for the reveal text, assign to `ForgePresenter._revealTextPrefab` on the player prefab
2. Add small particle-effect children to weapon prefabs, add `WeaponTierGlow` to the weapon root, wire the children into `_epicGlowVFX`/`_legendaryGlowVFX`
3. Add the `ForgePresenter` component to `pfb_player` (not yet placed on any prefab)
4. Manual test: approach a workbench, forge an item, confirm the panel closes and the weapon grows into place with bloom/impulse/reveal-text; upgrade and confirm the same beat plays faster/smaller

**Bug-fix pass 2026-08-19:** code review found `ForgePanel.Close()` unpausing immediately after every successful forge would unpause the game underneath the first-forge tutorial cutscene — `CutscenePlayer` disables input but never touches `Time.timeScale`, so the player would be unable to see or move while every enemy in the room kept attacking in real time (**B40**, the most-demoed moment in the game). Fixed by having `ForgePanel` check `CutscenePlayer.Instance.IsPlaying` immediately after a successful forge/upgrade and, if a cutscene just started, defer the real `Close()` until it finishes rather than unpausing underneath it. Performance review separately found `ForgePresenter`'s transformation sequence could permanently zero the equipped weapon's visible scale if interrupted by a second forge, and the colour-bloom effect could get stuck on the same way (**B34**) — fixed with a guaranteed cleanup path that restores scale/rotation and resets the bloom weight on interruption, not just at the coroutine's happy-path end.

**Known scope boundary (not a bug — see BACKLOG B25):** the new tier-prefab visual path only takes effect for weapons without a `baseEquippedData` override; most authored weapons currently have one, routing through the V3 weapon-data system instead. Full coverage is downstream of the V3/V4 ability-system reconciliation (still open, D3). The persistent glow is unaffected by this and works regardless.

---

## Prerequisite — must happen before old scenes are touched — **SATISFIED**

**Status, 2026-09-02:** all three steps are done. `RoomDataSO` exists, `LevelBuilder` reads spawn points from it, and every legacy room's encounter data was extracted **before** the per-room scenes were deleted in `84a3a44e`. Both continuous worlds now author encounters exclusively as `RoomDataSO` assets (`RoomData_CulDeSac_WildWestCity_Zone0-2`, `RoomData_Backyard_Dojo_Zone0-2`), so the data-portability risk this section guards against is closed. Note that ADR-0005 later retired ADR-0002's *scene-granularity* corollary as a default while explicitly preserving this `RoomDataSO`/`LevelBuilder` half as the load-bearing part of that ADR.

Original text, kept for the record. Per **[ADR-0002](adr/0002-full-scene-rebuild.md)**: `RoomManager`'s room/spawn-point data is **not portable** — it exists only as scene-local prefab-instance overrides and will be lost the moment old scenes are abandoned. Before any scene is rebuilt or removed:
1. Promote `RoomData` to a `RoomDataSO` (spawn points as `Vector3` data, not scene references)
2. Extend `LevelBuilder` to read spawn points from that data the way it already reads props/pickups/cardboard/workbenches
3. Extract every existing room's encounter data into that format while the old scenes still exist to read from

This is what makes the audience-contribution goal (Phase 5+) actually reachable — a contributor authors a room as a data asset, not by wiring scene-local references correctly.

---

## Owner-verified items outside sprint scope

- **HUD pause button** — owner asked about adding a pause button to the HUD; investigation found the plumbing already exists in `pfb_hud_v4` (the prefab every scene actually uses): `PauseMenu.cs`, the `PausePanel`, and a `PauseButton_Shield` button already wired to `PauseMenu.TogglePause`. The one visual gap found — `PauseButton_Shield`'s Image has alpha 0, so it's an invisible tap zone with no icon, unlike every other HUD button — was flagged to the owner with a choice (quick UI placeholder vs. full matching Meshy cardboard-icon art). Owner manually verified in Play Mode that the pause button works as-is and asked to leave it alone. No code or prefab changes made (2026-08-24).

---

## Explicitly Out of Scope for Sprint 0

**Correction of record, 2026-09-02:** three of the five exclusions below were written for Sprint 0's own scope and were true when written, but the project moved on to ROADMAP Phase 2 and Phase 3 on this same branch **without Sprint 0 ever being closed or a successor sprint opened**, so this list has been read as current far longer than it was accurate. Each line is annotated with what actually happened. The exclusions are kept rather than deleted so the original scope boundary stays legible.

- ~~Any new room/level content (that's Phase 2 — World 1 rebuild)~~ — **no longer true.** Phase 2 (`CulDeSac_WildWestCity.unity`) and Phase 3 (`Backyard_Dojo.unity`) were both built on this branch. See §Work delivered on this branch after Sprint 0's scope.
- The full grey-world Imagination Overlay (separate, larger scope — future episode) — **still out of scope, unchanged.**
- ~~Grasscutter, Crane Duelist, Gnome Soldier, or Leaf Pile Lurker implementation (World 2, later sprint)~~ — **no longer true for three of the four.** `GrasscutterAI` (full boss: combat, Spin-Dash lane telegraph, activation-gated intro cinematic and authored camera staging) and `CraneDuelistAI` (stationary telegraph-driven duellist with a counter window) are both implemented, code-reviewed, and committed. The Gnome Soldier (`pfb_enemy_gnome_grunt`) is a returning World 1 enemy and is placed in World 2 zones 0 and 1. **The Leaf Pile Lurker was genuinely unimplemented at the time** — `pfb_leaf_pile_lurker.prefab` existed as an art-only prefab with no script components and zero references anywhere in the project; there was no `LeafPileLurkerAI`. **Since deleted** (owner decision 2026-09-02, REJECTED for World 2 — see `docs/CREATIVE_STATE.md`; not cut from the game overall). See §Work delivered on this branch after Sprint 0's scope.
- Save system, difficulty system, or ability-system reconciliation (see `docs/BACKLOG.md`) — **still out of scope, unchanged.** Ability-system reconciliation remains D3/B4.
- ~~Texture import policy / thermal budget work — tracked separately in `docs/BACKLOG.md` (B1)~~ — **no longer true.** ADR-0004 §8 made the texture import policy a shipping prerequisite for the continuous city scene rather than deferred debt; the `AssetPostprocessor` landed 2026-08-31 (`94ad911b`) and was applied retroactively project-wide 2026-09-01 (`9c1d6637`). First on-device profiling capture is B112.

---

## Work delivered on this branch after Sprint 0's scope

Everything below post-dates Sprint 0's three scope items and was implemented on `feature/sprint-0-foundation-rebuild`. It is authorized by the same 2026-08-19 production authorization and scheduled by `docs/ROADMAP.md` Phase 2 / Phase 3 — it is **not** unscheduled work — but it was never reflected in this document, which is the staleness this section closes.

### Phase 2 — World 1, the continuous city scene — **DELIVERED, committed `b80953ca`**

`CulDeSac_WildWestCity.unity` replaced the five per-room Cul-de-Sac scenes: one continuous street, three `RoomManager` zones, `SpinCycle` at the north end. Design of record is **[ADR-0004](adr/0004-world1-single-continuous-scene.md)** (accepted 2026-08-26). Five `code-reviewer` fix passes plus one owner-playtest geometry revision (zone 1 widened 8 m → 20 m, B107). The legacy per-room scenes were subsequently deleted (`84a3a44e`). Full status, zone table, and arena measurements live in `docs/ROADMAP.md` Phase 2 — not duplicated here.

**Implementation status:** complete, committed, pushed. Open follow-ups: B105 (win/death `GameState` arbitration, not fixed), B106 (`_runEndScreen` null after `TriggerWin()`, unresolved, needs a manual owner check), B120's World 1 half (SpinCycle's boss intro still does not lock player movement — fixed for World 2 only).

### Phase 3 — World 2, the Backyard/Dojo — **SUBSTANTIALLY DELIVERED, open items below**

`Backyard_Dojo.unity` is one continuous scene with three `ZoneDirector`-driven zones, per **[ADR-0005](adr/0005-world2-single-continuous-scene.md)** (accepted 2026-08-31), which also **promoted single-scene-per-world to the project default** and retired ADR-0002's one-scene-per-room corollary (the `RoomDataSO`/`LevelBuilder` half of ADR-0002 is preserved and load-bearing).

- **Scene scaffold** — `9ab107bc` (B113). `WildWestCityZoneDirector` generalized to `ZoneDirector` via `git mv` with GUID preserved, so World 1's scene reference survived with no scene-file change; `ZoneStartScene[1]` corrected.
- **Stage A geometry and dressing** — `b2ae0b00` / `6a8c5d1b` (B115), authored against `docs/v4/levels/World2/backyard-dojo/zone-layout-spec.md`.
- **Zone rescale** — **[ADR-0006](adr/0006-world2-zone-scale-and-arena-metric.md)** (accepted 2026-09-01, `4f04cc7b`), triggered by the owner playtesting Stage A and reporting zones 1 and 2 as too small. Zone 1 → 20.0 × 28.0 m, boss arena → r = 10.0 m. Implemented in `06123dc6` (B116). ADR-0006 also **restated `docs/TECHNICAL_DESIGN.md` §6.4's combat-radius metric project-wide** after finding two accepted budget metrics were measuring the wrong thing.
- **Encounters** — three `RoomDataSO`s authored: `RoomData_Backyard_Dojo_Zone0` ("The Back Gate / Dojo Courtyard", 5 Gnome spawns, max 4 concurrent), `RoomData_Backyard_Dojo_Zone1` ("Garden Gauntlet", 4 Gnome + 1 Skeptic Grunt + 1 Crane Duelist, max 4 concurrent), `RoomData_Backyard_Dojo_Zone2` ("The Garden End — Blossom Court", max 1 — the boss is pre-placed inactive and activated by `ZoneDirector`, not spawned from the table).
- **`GrasscutterAI`** — `816405f2` (2026-09-01), with `code-reviewer` fixes folded in. Boss combat, phases, and the Spin-Dash.
- **Spin-Dash ground-plane lane telegraph** — **[ADR-0007](adr/0007-ground-plane-lane-telegraph.md)** (accepted 2026-09-01, `7a20deca`) added a second telegraph *geometry* to ADR-0003's channel, after a code review found the implemented Spin-Dash tell was body-anchored rather than the ground lane ADR-0006 §1.3 made a **condition** of the 20 m arena. Implemented in `8677467e` (B118).
- **Boss intro camera and staging** — **[ADR-0008](adr/0008-boss-intro-camera-authored-vantage.md)** (accepted 2026-09-01, **amended three times**) replaced player-relative retreat math with an authored vantage `Transform` and independent look-heights, and established a project-wide boss-intro camera contract. Implemented `418e2ab7` (2026-09-02). Amendment 3 retired the ADR's I2/I3 invariant chain when the owner's playtest moved the trigger from proximity-gated to **activation-gated** (fires on zone activation, matching `SpinCycleAI`) — see B120.
- **`CraneDuelistAI`** — `418e2ab7` (2026-09-02), with a counter-window timing fix and a coroutine-cleanup fix from code review. `pfb_crane_duelist`'s `NavMeshAgent.radius` is **0.35**, which satisfies `zone-layout-spec.md` §3.4's ≤ 0.5 engawa-pathing dependency (checked 2026-09-02) — this enemy is *not* affected by B114's boss-radius problem.
- **Boss scale** — root scale `(2,2,2)` → `(2.5,2.5,2.5)`, measured height **4.250001 m** against SpinCycle's corrected 4.250760 m, per owner decision (B119/B123). Heights were derived by evaluating the skin directly, not from `Renderer.bounds` — see ADR-0008 Amendment 2 before quoting any figure.
- **NavMesh** — `Backyard_Dojo.unity` had never been baked at all; baked 2026-09-01 (B122). It uses the **legacy Navigation-window bake**, matching World 1 — not `NavMeshSurface`.
- **Grasscutter reel-drum rig defect** — B130. First fix corrupted the mesh in Unity and was reverted (`e9505ddb` → `5ea7d5e3`); root-caused on the second pass to the prefab's 38 frozen model-hierarchy transforms desyncing from the re-exported FBX's bindposes, and re-fixed in `497e0031`. Owner-confirmed by playtest.

**Implementation status:** World 2 is playable end-to-end in the Editor with all three zones, both new enemy/boss AIs, the boss intro, and a baked NavMesh. What remains is validation and a set of deliberately-deferred decisions, below.

#### Still open for World 2

Validation not yet discharged:
- **ADR-0006 §Validation 10 has not passed on device or with a human.** The 20 m arena was granted *on the condition* that the dash-lane telegraph works; B118 is Play-Mode-verified only. Until this passes, the arena size is conditionally granted, not accepted.
- **ADR-0001's own validation checklist is still pending** and now applies to World 2's camera work as well (see item 1's note above).

Correctness items found and deliberately not fixed:
- **B117** — `GrasscutterAI.SpinDash`'s NavMesh clamp validates the landing point, not the path to it. Not fixed, flagged.
- **B121** — B116's own completion note misreports what it changed, and its M2 clearance figure was computed from the spec rather than measured. Not corrected.
- **B120 (World 1 half)** — boss intros still do not lock player movement in World 1.

Decisions the owner or a designer owns (logged as "decide, don't just fix"):
- **B125** — with the cherry tree at the north rim (CANON owner placement), zone 2 has no central obstruction, so the "orbital" movement grammar ADR-0006 §1.1 and `zone-layout-spec.md` §1.3 both assume no longer has anything to orbit. Needs `game-designer` + `technical-director`; **not** a reason to move the tree.
- **B126** — the built cherry tree exceeds ADR-0006 §1.1's canopy envelope (canopy radius 4.4 m vs ≤ 3.5 m; underside ≈ 1.9 m vs ≥ 4.0 m), and **the canopy has no collider at all**, so any `Physics.Raycast`-based clearance check passes straight through 8.8 m of foliage and reports clear. Either amend the envelope to the built asset or re-author the asset.
- **B127** — the scene's 8 `NavMeshModifier` components are inert under the legacy bake, so six court props carve permanent navmesh holes their author explicitly asked to exclude. Two mutually-exclusive options, both documented; do **not** resolve it by installing `NavMeshSurface` in one scene while World 1 uses the legacy path.
- **B128** — the bake has no bounds volume: 1 216 m² walkable against ~318 m² of playable Blossom Court, including a walkable ring outside the arena wall.
- **B124** — the GDD's "dormant in the tall grass … grass and petals kick up" beat has no implementation; no GameObject in the scene has "grass" in its name, and there is no kick-up VFX.

Not started:
- ~~**Leaf Pile Lurker** — art prefab only, no AI, no placement, no references.~~ **REJECTED for World 2, assets deleted 2026-09-02** — see `docs/CREATIVE_STATE.md`. Not cut from the game; still planned as a returning enemy in a later zone per the story bible.
- `docs/v4/levels/World2/backyard-dojo/gdd.md` §12 open questions Q1–Q4, including the Crane Duelist's Beak Thrust parry window, which is explicitly playtest-tuning work.

---

## Open for owner decision

Two items in this document cannot be resolved by an agent, and both are recorded here rather than silently settled.

### 1. Sprint bookkeeping — should Sprint 0 be closed and a new sprint opened?

Sprint 0's three scope items shipped 2026-08-19. Everything since — Phase 2, Phase 3, ADRs 0004–0008, roughly forty commits — has run under a document titled "Sprint 0" whose scope section excluded most of it. Nothing was built without authorization; the tracking artifact simply never advanced. The choice of whether to close Sprint 0 and open a successor sprint document (and what to number it) is owner bookkeeping, not an agent decision, so this document has been corrected in place rather than split.

### 2. `CLAUDE.md`'s lifecycle-state line contradicts every other record — and it is the root guide

`CLAUDE.md` §"Current lifecycle state" says *"BoxForged is back in **Discovery** while the concept/story is being developed further"* and instructs agents not to *"create new production sprints until the owner explicitly locks discovery and authorizes pre-production/production."*

The evidence says that line describes the state **before 2026-08-18** and is stale, not a live directive:

| Source | Record |
|---|---|
| `docs/CREATIVE_STATE.md` §Discovery lock status | Concept discovery **LOCKED** 2026-08-18; narrative discovery **LOCKED** 2026-08-18; *"Pre-production authorized by owner: YES (2026-08-18) — 'Lock discovery and begin pre-production.'"* — the exact phrase `.claude/rules/studio-core.md` Gate 0 requires |
| This document, §Authorization record | Production authorized 2026-08-19 ("Start Sprint 0") |
| `docs/TECHNICAL_DESIGN.md` line 508 | *"PRODUCTION AUTHORIZED 2026-08-19"* |
| `docs/ROADMAP.md` line 3 | *"In production. Discovery locked 2026-08-18. Production authorized 2026-08-19."* |
| `docs/PROJECT_CONTEXT.md` line 225 | *"V4 Sprint 1 was completed before the project returned to Discovery"* — confirming `CLAUDE.md`'s "V4 Sprint 1"/"Sprint 2" numbering refers to the **pre-discovery-lock** V4 workflow, a different numbering series from this document's "Sprint 0" |

So the two documents are describing two different points on the same timeline, and `CLAUDE.md`'s is the earlier one. **`CLAUDE.md` has deliberately not been edited by an agent** — it is the root project guide, and correcting an authorization statement in it is an owner action. Proposed replacement for the owner to apply or reject:

> BoxForged is in **Production**. Discovery was locked 2026-08-18 ("Lock discovery and begin pre-production"); production was authorized 2026-08-19 ("Start Sprint 0"). See `docs/CREATIVE_STATE.md` §Discovery lock status and `docs/SPRINT.md` §Authorization record. The three Phase 2+ narrative beats listed as open in `CREATIVE_STATE.md` do not block production. Preserve completed work unless an accepted decision explicitly supersedes it. "V4 Sprint 1"/"Sprint 2" refer to the retired pre-discovery V4 numbering and should not be resumed.

Until the owner rules, an agent reading `CLAUDE.md` in isolation will conclude production is not authorized and may refuse in-flight Phase 3 work.

---

## Required workflow (per `.claude/rules/studio-core.md`)

1. `technical-director` review complete — see `docs/TECHNICAL_DESIGN.md`, `docs/ARCHITECTURE.md`, and ADRs 0001–0003
2. `unity-gameplay-engineer` implements camera rig, telegraph channel, `RoomDataSO` extraction, and forge feedback layer once design is accepted
3. `code-reviewer` pass on all changes
4. `performance-engineer` review — confirm the camera change doesn't regress draw calls/GC given the mobile performance budget, the telegraph indicators stay pooled and within their draw-call budget, and the forge VFX/particle work stays within budget
5. `qa-engineer` — manual verification steps for combat readability at the new camera angle and telegraph legibility (critical — this is the thing most likely to silently break); run the ADR-0003 validation checklist (colorblind sim, greyscale, occlusion, crowd, audio-only)
6. Update `docs/KNOWN_ISSUES.md` and `docs/CHANGELOG.md` if either exists by sprint end

## Authorization record

Owner authorized production with "Start Sprint 0" on 2026-08-19. This satisfies the pre-production stop condition below — `unity-gameplay-engineer` and other production agents are cleared to implement the scope above on `feature/sprint-0-foundation-rebuild`. Do not re-request authorization for this sprint's scope; if you are an agent reading this document and unsure whether this line is current, check the git log on this branch or ask the coordinating session rather than refusing outright.

**Original stop condition (satisfied, kept for record):** This plan does not authorize implementation on its own. Per studio lifecycle rules, pre-production stops at the approval gate. The owner must explicitly authorize production (e.g., "start Sprint 0" or "begin production") before `unity-gameplay-engineer` writes any code. — **Done, see above.**
