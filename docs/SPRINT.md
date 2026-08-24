# Sprint 0 — Foundation Rebuild

**Status:** **AUTHORIZED — production in progress.** Owner said "Start Sprint 0" (2026-08-19). Implementation underway on the branch below.
**Branch:** `feature/sprint-0-foundation-rebuild` (created 2026-08-19)
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

## Prerequisite — must happen before old scenes are touched

Per **[ADR-0002](adr/0002-full-scene-rebuild.md)**: `RoomManager`'s room/spawn-point data is **not portable** — it exists only as scene-local prefab-instance overrides and will be lost the moment old scenes are abandoned. Before any scene is rebuilt or removed:
1. Promote `RoomData` to a `RoomDataSO` (spawn points as `Vector3` data, not scene references)
2. Extend `LevelBuilder` to read spawn points from that data the way it already reads props/pickups/cardboard/workbenches
3. Extract every existing room's encounter data into that format while the old scenes still exist to read from

This is what makes the audience-contribution goal (Phase 5+) actually reachable — a contributor authors a room as a data asset, not by wiring scene-local references correctly.

---

## Owner-verified items outside sprint scope

- **HUD pause button** — owner asked about adding a pause button to the HUD; investigation found the plumbing already exists in `pfb_hud_v4` (the prefab every scene actually uses): `PauseMenu.cs`, the `PausePanel`, and a `PauseButton_Shield` button already wired to `PauseMenu.TogglePause`. The one visual gap found — `PauseButton_Shield`'s Image has alpha 0, so it's an invisible tap zone with no icon, unlike every other HUD button — was flagged to the owner with a choice (quick UI placeholder vs. full matching Meshy cardboard-icon art). Owner manually verified in Play Mode that the pause button works as-is and asked to leave it alone. No code or prefab changes made (2026-08-24).

---

## Explicitly Out of Scope for Sprint 0

- Any new room/level content (that's Phase 2 — World 1 rebuild)
- The full grey-world Imagination Overlay (separate, larger scope — future episode)
- Grasscutter, Crane Duelist, Gnome Soldier, or Leaf Pile Lurker implementation (World 2, later sprint)
- Save system, difficulty system, or ability-system reconciliation (see `docs/BACKLOG.md`)
- Texture import policy / thermal budget work — tracked separately in `docs/BACKLOG.md` (B1) as the single largest technical risk, but not part of this sprint's two features

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
