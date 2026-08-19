# BoxForged — Backlog

Technical debt and deferred work discovered during pre-production technical review (2026-08-19).

**Nothing here is authorized.** Items are recorded rather than silently resolved, per the studio change-discipline rule. Priority is impact-ordered, not schedule-ordered.

Severity: **P1** blocks or endangers the two team-built reference worlds · **P2** costs real time or quality soon · **P3** worth doing, not urgent.

---

## Owner decisions required

These are not engineering calls. They gate work below. Decided items are struck through with the resolution; still-open items remain live.

| # | Decision | Context | Resolution |
|---|---|---|---|
| D1 | ~~**PermitPulper / NoticePusher / LaundryTumbler — keep or cut?**~~ | `docs/CREATIVE_STATE.md` lists all three as WORKING ("candidate enemy thoughts", not canon). Code disagrees: `LaundryTumbler` is **live** on `pfb_enemy_spincycle` in `CulDeSac_BossArena` (a 15-line drum rotator). `PermitPulperAI` and `NoticePusherAI` each sit on one prefab that nothing references. `PermitPulperBossAI` (757 lines) + `PermitPulperBossIntro` (259) are in **no scene or prefab** — 1,016 lines that cannot run | **DECIDED (2026-08-19): leave dormant.** No deletion, no time spent fitting them into Worlds 1–2 (already fully cast). Revisit for World 3+ only. `LaundryTumbler` stays as-is (already part of shipped SpinCycle). |
| D2 | ~~**Retire the nine non-boss-intro cutscene videos?**~~ | Cutscenes are locked to boss intros only. 10 `.mp4` ship in StreamingAssets (326 MB); exactly one (`spincycle_standoff.mp4`, 26.8 MB) is a boss intro. Recovers ~300 MB of download size | **DECIDED (2026-08-19): retired.** All 9 out-of-scope videos + `.meta` files deleted from `StreamingAssets/Cutscenes/`. Only `spincycle_standoff.mp4` remains. |
| D3 | ~~**Ability system: V3 or V4?**~~ | Two complete stacks coexist. Needed before any new weapon authoring | **DECIDED (2026-08-19): keep V4.** Investigation corrected the scope dramatically — this is NOT an 81-asset migration, `WeaponData` (V3) is V4's own equipped-data layer and stays permanently. Only 3 ability assets (Shuriken, SixShooter, DynamiteBundle) need real work, and that work is blocked on extending V4's architecture (see B4) — not a data-conversion task. 2 confirmed-dead classes deleted now; the real 3-asset port is its own ~2-3 day task after Sprint 0 commits. |
| D4 | ~~**Preserve existing room encounter data, or discard?**~~ | Deferred in the pre-production brief. Must be transcribed **before** old scenes are abandoned or it is lost — so it needs an early answer even if the answer is "discard" | **DECIDED (2026-08-19): discard.** Owner: the existing scenes/rooms were built while figuring out the podcast process and other systems, not intended as final content worth preserving. No `RoomDataSO` extraction needed — B3 closed, ADR-0002 updated. |
| D5 | ~~**Forge transformation staging — modal panel or in-world?**~~ | Affects UX, not just visuals. World-space reads considerably better at the new camera | **DECIDED (2026-08-19): in-world, at the workbench.** Not the paused modal panel. `docs/TECHNICAL_DESIGN.md` §5.5 and `docs/SPRINT.md` updated accordingly. |
| D6 | ~~**Third-party content reconciliation**~~ | `SimpleTown`, `ExplosiveLLC` (RPG Character Mecanim, SuperCharacterController), `Polylised` are in the tree but listed in no approval record. `PROJECT_CONTEXT.md` approves only Meshy and Polyworks | **DECIDED (2026-08-19): owner reconciled directly.** Removed: Polyworks (`Off Axis Studios/`), SimpleTown, Polylised. Added: Cartoon City (Hayq Art) as the new environment pack. Kept: RPG Character Mecanim Animation Pack FREE. Confirmed via GUID cross-check that none of the three removed packs were referenced anywhere in `_Project/` — safe removal. `SuperCharacterController` still present, unaddressed — not removed, owner didn't ask for it. See `docs/PROJECT_CONTEXT.md`. |

---

## P1 — Endangers the reference worlds

### B1. Texture import policy pass
**Impact:** thermal throttling mid-run on target hardware; likely the project's dominant performance risk.

2.6 GB of source textures across 353 files; individual BaseColor maps run 27–31 MB. **Essentially every texture imports at `maxTextureSize: 2048` with only a `DefaultTexturePlatform` entry — no Android or iPhone override, `textureFormat: -1` (Auto).** Only 34 are capped at 1024, 15 at 512.

Estimated 100–150 MB of texture memory for a single room. The failure mode is not a crash but gradual slowdown ~10 minutes into a 10–15 minute run — the exact scenario a livestream demo hits.

**Proposed:** `AssetPostprocessor` enforcing per-category caps (characters/bosses 1024, weapons 512, env props 512, UI 256) with explicit ASTC overrides per platform. Should precede any further asset import.
**Note:** derived from file inspection, **not measured on device**. Profile before and after.

### B2. Telegraph channel — blocking dependency of the camera change
**Implemented 2026-08-19 — but see B32, a critical bug found in review that means it currently renders nothing on a real device.**

See [ADR-0003](adr/0003-attack-telegraph-channel.md). Approving the low camera *without* this is the one combination that should not ship; the fallback is raising the camera back toward current pitch. Design itself (pooling, shape-coded parryability, hooks into the `WindUp` seam) was verified clean by performance review — the only defect is the build-stripping issue in B32.

Includes fixing the standing accessibility defect: parryable vs un-parryable is encoded **entirely in hue** (red DrumSlam vs yellow Haymaker) — resolved via shape-coding, pending B32's fix to actually be visible.

### B3. `RoomDataSO` extraction before the scene rebuild — **CLOSED 2026-08-19, not needed**
**Superseded by the D4 decision to discard existing room data.** The existing rooms were built while figuring out process, not as content worth preserving — so there is nothing to extract. `RoomDataSO` as an *architecture pattern* (data-driven room authoring, so a future contributor can build a room as a data asset rather than wiring scene-local references) may still be worth building when World 1/2 rooms are actually designed — but that's a fresh design task at that point, not a transcription of the current scenes.

### B4. Consolidate the two ability systems — **scope corrected 2026-08-19, much smaller than believed, but blocked on real design work**

**The original framing of this item was wrong in a load-bearing way.** V3 and V4 are not two competing systems needing a large migration — they're stacked layers, and V4 *depends on* V3. `WeaponObjectSO` (V4) has a field, `baseEquippedData`, pointing at a `WeaponData` (V3) asset — this is the live per-character grip/visual-variant resolution path (`WeaponInventory.ResolveEquipData` → `WeaponCycler.ResolveWeapon` → `WeaponHolder.EquipWeapon`). **The 81 `WeaponData` assets are not migration debt — they are V4's own equipped-data layer and are not going anywhere.**

**Real scope, verified directly against the assets:** of 81 `WeaponData` assets, 69 have no ability at all and need nothing; the other 12 are just character-variant copies of 3 weapon families. **Only 3 `WeaponAbilityData` assets actually need retiring: Shuriken, SixShooter, DynamiteBundle** (plus their 2 already-dead sibling classes, `LassoAbilityData`/`QuickdrawBladeAbilityData` — **deleted 2026-08-19**, confirmed dead via GUID sweep, superseded by shipping V4 behaviours).

**Why it's still not a quick fix:** V4's `AbilitySO`/`AbilityExecutor` architecturally cannot express what those 3 weapons need — an attack-button trigger (V4 only has OnHit/OnSpecial/OnDodge/OnBlock/Passive, no equivalent of V3's `FiresOnAttackButton`), per-equip ammo/reload state (Six Shooter's 6-shot/1.2s-reload/counter-window "Fan the Hammer" behavior — `AbilitySO` has one flat `cooldown` float), counter-window context (`AbilityExecutionContext` doesn't carry it, `AbilityActivationContext` does), and a HUD readiness surface (`CombatController.SpecialAbilityProgress` reads a per-ability `ProgressFraction` that V4 doesn't supply). This is genuine design/architecture work, not asset conversion — extending `AbilitySO`, `AbilityExecutor`, and `AbilityExecutionContext`, and doing it without reproducing V3's existing SO-instance-state smell (ammo count living on the shared asset instead of a per-equip component).

**Recommendation: own dedicated task after Sprint 0 commits, ~2-3 days** (half a day design/ADR for the attack-trigger + ammo-state contract, ~1 day porting the 3 behaviours, ~1 day rewiring + regression testing ranged weapons). Not bundled into Sprint 0.

Gated on D3 (decided: keep V4, migrate what's left — now correctly scoped to 3 assets, not 81).

### B27. Camera fails its own lateral-width requirement — worse than first thought, affects common phones too
**Surfaced 2026-08-19, expanded 2026-08-19 by performance review — real acceptance-criteria gap, needs an owner decision, not just an engineering fix.** ADR-0001's acceptance table requires lateral width ≥16m at Kid's depth on every supported aspect ratio (4:3 to 21:9). Initial finding: fails at 4:3 (tablet, ≈10.3m). **Performance review recalculated per-aspect and found the arena itself (`_arenaBoundaryRadius = 8.5`, 17m diameter) overflows the visible frame on far more than just tablets:**

| Aspect | Visible width at player depth | 17m arena |
|---|---|---|
| 20:9 (modern phone) | ~17.2m | just fits |
| **16:9 (common 3–4-year-old Android — the actual target hardware)** | **~13.8m** | **~3m overflows off-screen** |
| 4:3 (iPad) | ~10.3m | ~7m overflows |

The 16.8m figure in the ADR only holds at roughly 20:9. **This means the arena edges are off-screen on the specific hardware class the mobile performance budget already targets** (3–4-year-old devices, which skew 16:9 rather than the newer 20:9 aspect), not just on tablets. This is arithmetic from the prefab values, not yet measured on an actual device — verify in a build on a 16:9 device before treating it as certain, but the math is straightforward enough to trust as a strong signal.

**FIXED 2026-08-19.** New component `Core/AspectAdaptiveCameraFraming.cs` on `pfb_CM_FollowCam.prefab`: scales `FollowOffset` along its existing direction (pitch and vertical FOV never touched) to guarantee ≥16m lateral width at every aspect ratio, only pulling back further than the authored base distance when narrower aspects would otherwise fall short — 20:9/21:9 are unchanged. Verified by invoking the actual compiled method against all four target aspects:

| Aspect | F (≥12) | R (≥4, floor 3.3) | W (≥16) | Top ray (≥10°) |
|---|---|---|---|---|
| 4:3 | 23.74m | 6.50m | 16.00m | 13.5° |
| 16:9 | 17.81m | 4.88m | 16.00m | 13.5° |
| 20:9 | 15.34m | 4.20m | 17.23m | 13.5° |
| 21:9 | 15.34m | 4.20m | 18.09m | 13.5° |

All four floors clear at every aspect. Top-ray angle is exactly 13.5° regardless of distance (it only depends on pitch and FOV, both fixed), so this fix structurally cannot bring the horizon into frame. **One manual check still recommended:** confirm visually in a normal Editor session (not the headless MCP session used to build this) that switching the Game View aspect preset during Play Mode visibly changes `FollowOffset` in the Inspector — the automated check exercised the real method directly but couldn't do a literal visual resize due to the session's non-standard Game View size.

### B32. CRITICAL — the telegraph shader will be stripped from the real build; the whole system is currently invisible on-device — **FIXED 2026-08-19**
Fixed in the Sprint 0 bug-fix pass: created `Assets/_Project/Materials/mat_TelegraphOverlay.mat` (using `BoxForged/TelegraphOverlayUnlit`), added a `[SerializeField] private Material _overlaySourceMaterial` to `AttackTelegraphService` referencing it, and removed the `Shader.Find` runtime lookup entirely from `AttackTelegraphIndicator`. Also collapsed the 8 per-instance runtime materials down to 2 shared ones (parryable/unparryable), owned and destroyed by the service. **Critically, the serialized-field fix alone does nothing unless something in the build actually assigns it** — the service was purely self-bootstrapping (never placed in any scene), so a new `pfb_AttackTelegraphService.prefab` (with the material wired) was created and placed in all 6 scenes that already carry `pfb_AudioManager` (`ForgeLoop_Test`, `CulDeSac_Room1`, `CulDeSac_AmbushAlley`, `CulDeSac_SaloonFront`, `CulDeSac_MailboxRow`, `CulDeSac_BossArena`), mirroring that existing precedent. Verified via a Play Mode smoke test in `ForgeLoop_Test` — zero console errors, service resolves the material without the "not assigned" fallback error path firing.

**Surfaced 2026-08-19 by performance review.** `Assets/_Project/Shaders/TelegraphOverlayUnlit.shader` is referenced by zero materials, prefabs, or scenes, and is not in `ProjectSettings/GraphicsSettings.asset`'s Always Included Shaders. Unity only includes a shader in a build if something references it that way — a runtime `Shader.Find("BoxForged/TelegraphOverlayUnlit")` string lookup (what `AttackTelegraphService` currently does) does not count. On a real Android/iOS build, `Shader.Find` returns null, an error logs, and every telegraph indicator silently renders nothing. **This works perfectly in the Editor and fails silently in a build — the worst possible failure shape for a livestream demo.** Compounded by B29 (no audio clips authored yet either) — on a real build today, there is currently no attack telegraph at all, visual or audio.

**Fix:** create a material asset (`mat_TelegraphOverlay.mat`) using this shader, and have `AttackTelegraphService` reference it via a serialized field rather than a runtime string lookup. This also lets the 8 per-instance runtime materials collapse to 2 shared ones. Queued for the Sprint 0 fix pass.

### B33. `LevelBuilder.ValidateCameraClearance` will hang Play Mode entry with a flood of false-positive errors — **FIXED 2026-08-19**
Fixed in the Sprint 0 bug-fix pass: `_cameraClearanceMask` now defaults to the Building layer only (matching `CameraOcclusion._wallMask`) instead of `~0`; the field was moved outside the `#if UNITY_EDITOR` guard (only `ValidateCameraClearance`'s logic stays editor-only) to avoid a prefab/scene serialization mismatch across build configurations; and per-vertex `Debug.LogError` calls (up to ~40, each with a stack trace) were replaced with a single aggregated `Debug.LogWarning` per validation run (e.g. "4 of 16 NavMesh vertices have < 6.0m clear above"). Verified via a Play Mode smoke test in `ForgeLoop_Test`: bake completed instantly (16 verts) and produced exactly one warning line, correctly scoped to real geometry rather than flagging props/pickups/enemies. The deeper "is 8m clearance achievable in an 8.5m arena" question is unchanged and remains tracked in B27 — this fix only makes the validator report sanely.

**Surfaced 2026-08-19 by performance review.** Confirmed genuinely editor-only (correctly wrapped in `#if UNITY_EDITOR`, cannot run in a player build) — but as written it: (1) uses an unrestricted `~0` layer mask, so it flags props, pickups, enemies, and workbenches as "camera clearance violations," not just building geometry; (2) runs two raycasts per NavMesh triangulation vertex, synchronously, in one frame — thousands to tens of thousands of raycasts on a room-sized mesh; (3) can emit up to ~40 `Debug.LogError` calls, each capturing a stack trace. Expected result: a multi-second hang entering Play Mode and a console flooded with mostly-false-positive errors, which trains everyone to ignore the check — defeating its purpose.

**Fix:** restrict the mask to the actual building/geometry layer, move the check to an on-demand menu item rather than running on every Play Mode entry, and stride/dedupe the vertex list. Queued for the Sprint 0 fix pass.

### B42. Telegraph indicators are never explicitly hidden — ghost warnings linger over dead/stunned enemies
**Surfaced 2026-08-19 by code review.** `AttackTelegraphService.Show()` returns a handle specifically so callers can later call `Hide()` — but no call site across any of the 8 modified AI scripts actually captures that handle or calls `Hide()`. An indicator only clears itself on natural duration expiry or if its target GameObject is destroyed. **Failure scenario:** the player interrupts an enemy's wind-up by killing it (many enemies play a death animation before actually destroying the GameObject) or staggering it — the "attack incoming" icon keeps floating over the corpse/stunned enemy for the rest of the original wind-up duration. Not fixed in the Sprint 0 fix pass (deferred per reviewer's own recommendation, since it's a visual polish issue, not a safety/correctness one) — tracked here so it isn't forgotten. Fix suggestion: a small shared per-enemy telegraph helper (`Raise`/`Clear`) called from each AI's existing stagger/death state-entry methods, which would also deduplicate the identical one-line `Show()` call currently repeated across all 8 scripts.

### B43. Camera occlusion fade loses partial-occlusion detection and hysteresis with the single-raycast retune
**Surfaced 2026-08-19 by code review.** The single torso-height raycast (B5/B35's fix) is a large net performance win, but trades away two things the old 8-corner AABB test had: (1) no dead-band, so strafing along a wall edge can cause the wall to flicker between faded and solid frame-to-frame; (2) no partial-occlusion detection — a pillar or low wall covering everything except the exact torso sample point won't trigger a fade at all, where the old test would have caught it. Not fixed in the Sprint 0 fix pass (deferred per reviewer's recommendation) — worth a playtest pass at the new camera angle specifically. Candidate fix if it proves to be a real problem: add a short fade-hold after the ray clears, or sample a small fan of 3 rays instead of 1.

### B34. `ForgePresenter` can permanently zero the equipped weapon's visible scale — **FIXED 2026-08-19**
Fixed in the Sprint 0 bug-fix pass: added a `CancelActiveSequence()` cleanup path (called from both `Dispatch()`, when a second forge/upgrade interrupts an in-flight one, and `OnDisable()`) that restores the tracked weapon transform's `localScale`/`localRotation` to its correct end-state and explicitly stops + zeroes the colour-bloom coroutine's volume weight, rather than relying on the coroutine reaching its own happy-path end. The bloom coroutine is now tracked in a field (`_colorBloomSequence`) instead of fire-and-forget.

**Surfaced 2026-08-19 by performance review — correctness bug, not a performance one.** The transformation sequence sets the weapon's scale to zero at the start and only restores it at the natural end of the sequence. If the sequence is interrupted (forging twice in quick succession, or the player object being disabled mid-sequence), nothing restores the scale — the equipped weapon becomes permanently invisible until the player re-equips something. A related fire-and-forget color-bloom coroutine has the same problem: two overlapping forges can leave the imagination-restore visual effect stuck on.

**Fix:** restore scale/rotation in a guaranteed cleanup path (not just the happy-path end of the sequence), and track the bloom coroutine so it can be stopped alongside the rest of the sequence. Queued for the Sprint 0 fix pass.

---

## P2 — Costs real time or quality soon

### B5. Resolve the two occlusion systems — **DONE 2026-08-19**
Kept `CameraOcclusion.cs` (deleted `BuildingOcclusionFader.cs` — see B28, it had a real bug beyond being redundant). Retuned to a single torso-height raycast. **New finding from review:** the retuned method now calls `GetComponentInChildren<Renderer>()` on every ray hit, every frame, unfiltered — see B35.

### B6. `EnemyHealthBar` billboard and scale — **DONE 2026-08-19, verified no regression**
Billboard refresh now also triggers on enemy movement, not just camera movement. Performance review confirmed the added cost is negligible (~0.01ms at 20 enemies) and that the guard it modifies was already firing almost every frame anyway, since the camera follows the player continuously in combat — net cost delta ≈ zero. Bar dimensions scaled down by the camera-distance ratio as a documented estimate, not yet visually re-checked in-engine. Two `new Material` instances per enemy at runtime confirmed unchanged, cleanup confirmed still correct — this remains tracked debt (draw-call budget, not a regression from this change).

### B47. `AspectAdaptiveCameraFraming.Apply()` deserves a permanent EditMode test once test infrastructure exists
Validated 2026-08-19 via a one-off reflection-based check against the four target aspect ratios (see B27) rather than a real test, since no EditMode/PlayMode test assembly exists in the project yet (see B7). Once B7's minimum test boundary exists, this is a clean, pure-math candidate to cover permanently — asserting F/R/W/top-ray against the same four aspect ratios.

### B7. Minimum test infrastructure
Zero tests, zero test assemblies; exactly one `.asmdef` in the project (`{"name": "StatSystem"}`). `com.unity.test-framework` installed and unused. **The Definition of Done clause "affected tests pass" is currently unsatisfiable.**

Proposed minimum: one assembly boundary around pure rules (forge, durability, inventory, stat math) + EditMode tests, starting with `ForgeController.TryForge` (its check-then-spend ordering is exactly the invariant a regression would silently break). Not a whole-project asmdef refactor.

### B8. Arena radius vs camera framing — **CORRECTION 2026-08-19: NOT actually done, despite being reported as done — FIXED 2026-08-19 (for real this time)**
C# default changed 18f → 8.5f, but **code review found the value on `pfb_player.prefab` (line 140) is still the old serialized `18`** — Unity does not retroactively apply a new script default to a component instance that already has its own saved value on a prefab. The prefab itself needs the value changed directly, not just the C# initializer. Also superseded in target by B27 (8.5 isn't even enough at real target-hardware aspect ratios) — but right now, on disk, the arena is still 18. Queued for the Sprint 0 fix pass.

Fixed via Unity MCP `manage_prefabs.modify_contents` directly on `pfb_player.prefab`'s `Boxhead.Player.PlayerController` component, and verified by reading the serialized value back from disk afterward (`_arenaBoundaryRadius: 8.5`). B27's separate concern (whether 8.5 is even enough at target aspect ratios) is untouched — that's an owner decision, not this fix's scope.

### B9. Hardcoded FOV duplicate — **CORRECTION 2026-08-19: NOT actually done, despite being reported as done — FIXED 2026-08-19 (for real this time)**
Same class of bug as B8. `SpinCycleAI._normalCameraFoV` C# default changed to `45f` with a comment claiming it now matches the rig and fixes the boss-intro→gameplay handoff pop — but **`pfb_enemy_spincycle.prefab` (line 1053) still has the old serialized value `40`.** The comment asserting the fix is now actively misleading. The pop this was meant to fix still happens. Queued for the Sprint 0 fix pass.

Fixed via Unity MCP `manage_prefabs.modify_contents` directly on `pfb_enemy_spincycle.prefab`'s `Boxhead.Enemy.SpinCycleAI` component, and verified by reading the serialized value back from disk afterward (`_normalCameraFoV: 45`).

### B39. Same stale-prefab-serialization bug affects the EnemyHealthBar retune — **FIXED 2026-08-19**
`EnemyHealthBar._barWidth`/`_barHeight` C# defaults were shrunk (0.8/0.17) to compensate for the closer camera, but all 6 enemy prefabs (`pfb_enemy_milepost_marshal`, `pfb_enemy_wagonwheel_roller`, `pfb_enemy_skeptic_grunt`, `pfb_enemy_gnome_grunt`, `pfb_enemy_spincycle`, `pfb_enemy_sprinkler_sentinel`) still carry the old serialized `1.4`/`0.3`. Health bars remain oversized at the new camera distance until the prefabs themselves are edited. Queued for the Sprint 0 fix pass, alongside B8/B9 — same root cause, same fix pattern (edit the prefab's serialized value directly, not just the C# default).

Fixed via Unity MCP `manage_prefabs.modify_contents` on all 6 prefabs' `Boxhead.Enemy.EnemyHealthBar` components, verified by reading each serialized value back from disk afterward (`_barWidth: 0.8`, `_barHeight: 0.17` on every one).

### B40. `ForgePanel.Close()` unpauses the game underneath the first-forge cutscene — the most-demoed moment in the game — **FIXED 2026-08-19**
Fixed in the Sprint 0 bug-fix pass: `ForgePanel.Forge()`/`Upgrade()` now route success through a new `HandleForgeOrUpgradeSuccess()`, which checks `CutscenePlayer.Instance.IsPlaying` immediately after `TryForge()`/`TryUpgrade()` return (reliable because `CutscenePlayer.Play()` sets `IsPlaying = true` synchronously before its coroutine even starts). If a cutscene just started, the panel hides its own UI but deliberately leaves `Time.timeScale`/`AudioListener.pause` untouched, and starts `WaitForCutsceneThenClose()` — a coroutine that polls with a bare `yield return null` (so it keeps advancing even at `timeScale = 0`) until the cutscene ends, then performs the normal `Close()`. Traced through both the first-forge case (cutscene fires → world stays frozen behind it → resumes correctly on cutscene end) and the normal subsequent-forge case (no cutscene → `Close()` fires immediately, unchanged behaviour). `Close()` itself now also cancels any pending wait coroutine, so whichever path reaches it, state stays consistent.

**Surfaced 2026-08-19 by code review — severe, podcast-relevant.** The very first successful forge in a save fires a cutscene (`CutscenePlayer`) that disables player input but does NOT touch `Time.timeScale`. Previously the forge panel stayed open (keeping `timeScale = 0`) until the player closed it, so the frozen panel kept the world frozen behind the cutscene too. Now `ForgePanel.Close()` runs immediately after a successful forge and sets `timeScale = 1`. **Failure scenario: player forges their first weapon ever — a full-screen video covers the screen, their controls are disabled, and the game world runs at full speed behind it. Any enemy in the room can freely attack them while they can't see or move.** This is specifically the first-forge moment — one of the most likely moments to be shown live on the podcast. Queued for the Sprint 0 fix pass as a must-fix, not a backlog item.

### B41. Legacy V3 `Inventory` path silently resets forged weapon tier to Standard — **FIXED 2026-08-19**
Fixed in the Sprint 0 bug-fix pass: `WeaponHolder` now caches a sibling `WeaponInventory` reference and, in `EquipWeapon(WeaponData, WeaponTier)`, reasserts `tier` from `WeaponInventory.ActiveWeapon.Tier` whenever that's non-null — before the passed-in `tier` is used for anything. Confirmed via code trace that `WeaponPickup` (the only live world-pickup path) exclusively calls into `WeaponInventory.AddToMaterialBag`, never the V3 `Inventory`, so there is no live path where a genuinely different, non-forge-tracked weapon needs to be equipped at Standard tier while a forged weapon is also active — meaning the reassertion is safe everywhere it fires. This resolves the `BossRoomWeaponSpawner.ClearPlayerWeapons()` → `Inventory.Drop()` path directly, and also fixes the same regression via `WeaponCycler.Start()`'s unconditional `Inventory.SetEquipped()` call (which runs on every scene load, including boss rooms, and was an equally live source of the same bug). WeaponInventory's own calls (which already pass the correct tier) are unaffected — the reassignment is a no-op on that path.

**Surfaced 2026-08-19 by code review.** `Boxhead.Systems.Inventory` (the older V3 system, still on the player prefab alongside V4's `WeaponInventory`) calls an equip overload that resolves to `WeaponTier.Standard` regardless of the weapon's actual tier. Live call sites include `BossRoomWeaponSpawner`, which calls this **on every boss-room entry** — meaning a player carrying a forged Legendary weapon into a boss fight has their tier silently reset the moment they arrive. This directly undermines the tier-glow feature this same sprint just built (B24/B25): the two systems disagree about the weapon's tier, and the visual glow would show the wrong tier or none at all. Queued for the Sprint 0 fix pass — the fix is to have `WeaponHolder` reassert tier from `WeaponInventory` (the source of truth) whenever it re-attaches a weapon, rather than trusting whichever system called last.

### B35. `CameraOcclusion` calls `GetComponentInChildren<Renderer>()` every frame, per hit, unfiltered — **FIXED 2026-08-19**
Fixed as a bundled low-risk item in the Sprint 0 bug-fix pass: added a `Dictionary<Collider, Renderer> _rendererCache`, resolved once per collider (including caching a `null` result for colliders with no Renderer) via a new `ResolveRenderer()` helper, cleared in `OnDestroy()` alongside the existing per-instance material cleanup.

**Surfaced 2026-08-19 by performance review.** The occlusion-system retune (B5) is a large net win overall (one raycast replacing up to 256 per-frame matrix projections), but the `Renderer` lookup that now runs on every ray hit, every frame, is unfiltered — in the old code it sat behind a viewport-rect rejection test that made it rare; now nothing filters it. This directly violates the project's own "avoid per-frame `GetComponent`" rule. **Fix:** cache the `Collider → Renderer` mapping in a dictionary once, since it never changes at runtime. Small, low-risk fix.

### B10. Boss-intro system consolidation
Two unrelated implementations: `CutscenePlayer` (610-line video player, used by SpinCycle) and `PermitPulperBossIntro` (259-line in-engine cinematic that disables the `CinemachineBrain`, drives `Camera.main` directly, and fires a Cinemachine impulse **via reflection**, with no skip).

Under boss-intros-only scope, the reusable asset is the in-engine one — currently the *less* general of the two and hardcoded to one boss. Cutting scope deletes `CutscenePlayer`'s *callers*, leaving a large video player serving one call site. Gated on D2.

### B11. Dead and superseded scene/level code
- Three Build Settings scenes do not exist on disk (`LoadingScreen`, `TownSquare_Room1`, `TownSquare_BossHall`), making `GameManager.cs:34` zone 1 unreachable
- Two enemy-spawn systems run simultaneously (`RoomManager` spawn points + `EnemySpawner`'s `Transform[]` coroutine)
- `BossHallDoor._bossSceneName = "TownSquare_BossHall"` (`:19`) — scene does not exist; `BossRoomWeaponSpawner` in no scene or prefab
- `Editor/Sprint4SceneSetup.cs` wires `HUDController` but live `GameManager.cs:55` uses `HUDController_V2`; hardcodes `"You Win!"` copy. **Do not use for the rebuild**
- `WeaponSpawnEntry.useRarityOverride` / `rarityOverride` declared but never read (`LevelBuilder.cs:128-143`)
- `_spawnRoot` is `{fileID: 0}` in `CulDeSac_Room1` — builder content lands at scene root

### B12. Duplicate UI generations
`ForgeUI` (514 lines, better built, **referenced by no scene or prefab**) vs live `ForgePanel` (193 lines). `HUDController` vs `HUDController_V2`. Resolve before rebuilding HUD wiring. Also: `pfb_hud_v4` carries 88–114 instance overrides per scene — reconcile into the prefab rather than re-creating the divergence.

### B13. Repository weight
Working tree 3.2 GB; `.git` 6.8 GB; no Git LFS. Vendor demo content ~1.2 GB (`Off Axis Studios` 1.1 GB, plus Polylised, SimpleTown, ExplosiveLLC). `Assets/Screenshots/` (60 MB) is inside the Unity project and gets imported — it does not belong there. Clone and branch cost is a real tax on a two-person team. Gated partly on D6.

---

## P1 — Endangers the reference worlds (continued — found during D3 investigation, pre-existing, not caused by Sprint 0)

### B44. Live bug: a null-reference exception soft-locks combat on specific Epic/Legendary weapons
**Surfaced 2026-08-19 while scoping B4/D3 — pre-existing bug, not introduced by Sprint 0.** With a fighting style active (always true from run start) and an equipped Epic/Legendary weapon whose ability trigger is `OnSpecial` but has no V3 fallback ability, pressing Special throws a null-reference exception mid-coroutine, which leaves `CombatController.State` stuck at `SpecialAttacking` — **the player cannot attack or use Special again until they dodge.** Reproduces with Bo Staff Epic, Pressure Cannon Epic/Legendary, and Magic Wand Legendary. Root cause: `CombatController.cs:786` falls through to calling `_currentAbility.Activate(...)` when `_currentAbility` is null, because the V3/V4 negotiation logic assumes a V3 ability exists as a fallback whenever the style special path isn't taken. Not fixed — the correct behavior (should the style special fire instead? should V4 alone handle it?) is a design decision, not a null-guard, and this bug structurally disappears once B4's ability-system work lands. Worth knowing this exists on specific weapons right now, independent of when B4 gets scheduled.

### B45. Live bug: Shurikens Legendary double-fires its special (V3 and V4 both trigger)
**Surfaced 2026-08-19, same investigation as B44 — pre-existing.** Shuriken is the one weapon that still has a real V3 ability, so instead of the null-reference in B44, both systems fire: V4's "Three at Once" via `OnSpecialActivated`, then the V3 shuriken throw via `_currentAbility.Activate`, back to back. Same root cause as B44 (the V3/V4 negotiation), same resolution path (disappears once B4 lands). Not fixed now.

### B46. `WeaponCycler` is documented as an editor-only debug helper but is a hard production dependency
**Surfaced 2026-08-19, same investigation.** `WeaponCycler` is disabled (`m_Enabled: 0`) on `pfb_player.prefab` and documented as an editor/play-mode helper, but its `ResolveWeapon` method is actually called by `WeaponInventory.ResolveEquipData` and `WeaponPickup` — meaning per-character weapon-variant resolution runs through a component that's marked disabled and labeled as a debug tool. Not a bug (it evidently still runs when called directly, regardless of the `enabled` flag, since Unity only skips `Update`/`MonoBehaviour` messages on disabled components, not direct method calls) — but worth relabeling so a future contributor doesn't disable/remove it thinking it's safe to.

## P3 — Worth doing, not urgent

### B14. Save migration path
`Core/SaveSystem.cs` has a `version` field used purely as a corruption sentinel (`:70-78`) — no migration branch. Bumping it would silently do nothing. `Data` is exposed as a mutable reference and `ProgressionSystem.cs:252` mutates without saving (per-kill Spark flushes only on upgrade, conversion, or `OnApplicationPause`). Persistence is also split across two backends — JSON for `SaveData`, PlayerPrefs for `CutsceneFlags`.

Low severity pre-release; expensive the moment real players have saves. **Do before first release.**

### B15. Mutable state on shared ScriptableObject assets
e.g. `TheFirstStrikeBehaviour._firstHitReady`, `_cachedCombat`. A single-player-only assumption — and `docs/CREATIVE_STATE.md` records co-op as designed-in from day one. Directly incompatible.

### B16. `MaterialPropertyBlock` guidance is inverted for URP
`Mobile_RPAsset` has `m_UseSRPBatcher: 1`. Under the SRP Batcher, per-instance `Material` copies still batch (same shader variant), while **`MaterialPropertyBlock` breaks SRP batching**. The comment at `Enemy/SpinCycleAI.cs:1123` endorsing MPB is a performance trap on this pipeline.

### B17. Shadow distance vs camera framing
**Corrected 2026-08-19 — the original numbers here were slightly off.** `Mobile_RPAsset.asset` actually has `m_ShadowDistance: 50` (main light) with a **1024×1024** main-light shadowmap (256×256 was the additional-lights low tier, not the main light) and 1 cascade. Against the new camera's actual relevant ground depth (~25m, computed from the implemented rig: 5.5m height, 7.57m back, 36° pitch, 45° vertical FOV), one 1024² cascade stretched over 50m wastes roughly half the atlas in each axis. **Dropping `m_ShadowDistance` to 25 roughly doubles effective shadow texel density at zero cost, and culls shadow casters beyond the camera's reach — which also reduces draw calls.** Directly thermal-relevant given the 10–15 minute run length. Not yet done.

### B18. Weapon attach pooling
`Player/WeaponHolder.cs:164-167` does `Destroy` + `Instantiate` on every equip, with a standing `// TODO: replace Destroy/Instantiate with an object pool`. Pool only if profiling shows it matters.

### B19. Unreferenced components that were written and never attached
**Partially resolved 2026-08-19.** `Systems/WeaponForgeAnimation.cs` — GUID was in zero prefabs and zero scenes; `OnWeaponForged` / `OnWeaponUpgraded` now have a live subscriber (`Systems/ForgePresenter.cs`, added in Sprint 0), which supersedes what `WeaponForgeAnimation` attempted. `WeaponForgeAnimation.cs` is now confirmed fully redundant — candidate for deletion, not attachment. `Player/WeaponEquipController.cs` is still not on the player prefab, so `WeaponHolder.cs:251` still calls it through a null-conditional and its two events still never fire — that half is still open. Attach or remove.

### B20. `SaveTester` must not ship
`Core/SaveTester.cs` is a 149-line IMGUI `OnGUI` debug panel. Strip from release builds.

### B21. Documentation/reality drift — **DONE 2026-08-19**
The camera was documented as `(0, 12, -8)` with a hard look-at for an entire phase; the prefab had `(7.879929, 11, -10)` with a **−38.2° yaw** appearing in no document. On a project where docs are the AI's primary context, a wrong doc actively propagates errors. `docs/PROJECT_CONTEXT.md` and GDD items 22/41 corrected now that ADR-0001 is Accepted and implemented. Prefer citing prefab/asset values over prose going forward.

### B22. `PlayerController` camera caching
`Player/PlayerController.cs:184` disables movement entirely when `_mainCamera` is null, and `_mainCamera` is cached once in `Awake` (`:69`) and never re-resolved. Any future rig that *replaces* the Main Camera GameObject at runtime silently freezes the player.

### B24. Forge presentation — art/prefab wiring needed before the moment is visible
**Surfaced 2026-08-19, implementation complete, art pass outstanding.** `Systems/ForgePresenter.cs` and `Systems/WeaponTierGlow.cs` are implemented, compiled clean, and validated in a live Play Mode smoke test (see `docs/SPRINT.md`) — but the moment is currently invisible in practice because no art assets are wired to it yet:
- No `TextMeshPro` reveal prefab is assigned to `ForgePresenter._revealTextPrefab` (sequence runs correctly and no-ops gracefully without it, same convention as `RarityIndicator`)
- No `WeaponTierGlow` component or glow-VFX children exist on any weapon prefab yet
- `ForgePresenter` is not yet added to `pfb_player` (it requires `ForgeController`, already present, but was added as a fresh component, not yet placed)

None of this is code work — it's exactly the kind of asset-wiring pass a non-expert owner can do directly in the Unity Editor once ready. See `docs/SPRINT.md` for the four manual steps.

### B25. Epic/legendary weapon-visual prefabs don't reach most weapons yet
The new `epicWeaponPrefab`/`legendaryWeaponPrefab` read path (`WeaponHolder.ResolveTierPrefab`) works, but most authored weapons resolve through a `baseEquippedData` override (a V3 `WeaponData` asset) before `WeaponHolder` ever sees the `WeaponObjectSO` — confirmed live against `WeaponObject_Broomstick.asset`. So the new tier-prefab path only takes effect for weapons *without* that override. This is downstream of the existing V3/V4 ability-system split (D3, still open) — full coverage needs that reconciliation, not a fix in isolation. The persistent glow (B24) is unaffected by this and applies regardless of which data path resolved the weapon.

### B26. Forge panel no longer reopens automatically after a successful forge
`UI/ForgePanel.cs` now closes on every successful forge/upgrade to hand off to the in-world moment (the owner's locked staging decision). If the owner wants faster repeat-forging without walking away and back to the workbench, that's a small follow-up — not implemented, not requested, just noting the behavior changed.

### B28. `BuildingOcclusionFader` deleted — it had a real bug beyond the reason it was removed
**Resolved 2026-08-19.** Per ADR-0001's required occlusion-system consolidation, `CameraOcclusion.cs` was kept and `Systems/BuildingOcclusionFader.cs` was deleted. Reasons: it wasn't attached to anything in any scene/prefab (dead code), **and** it mutated shared `Material` assets in place (`rend.sharedMaterials[m].SetFloat(...)`) rather than instancing them — meaning if it had ever run, it would have permanently converted every building sharing that material to Transparent, project-wide, with no way to undo short of re-importing the asset. Worth remembering as a general lesson: dead code doing an unsafe thing is still worth removing, not just ignoring. `CameraOcclusion.cs` was retuned to a single torso-height raycast (replacing an 8-corner viewport-AABB test) — correct for the new low camera pitch and cheaper.

### B29. No audio assets exist yet for the new attack-telegraph sound cues
`Core/SoundData.cs` gained four new `SoundEvent` entries (`TelegraphMeleeParryable`, `TelegraphMeleeUnparryable`, `TelegraphAreaUnparryable`, `TelegraphProjectile`) as part of the new telegraph system (ADR-0003). No actual audio clips are authored or wired to a `SoundData` asset yet — `AudioManager.Play()` is a safe no-op until they exist, so nothing is broken, but the telegraph's audio channel (explicitly required by the ADR as occlusion-proof reinforcement) is currently silent. Needs an audio-authoring pass.

### B30. Dynamically-resolved parryability collapsed to the conservative shape
SpinCycle's Haymaker (parryable only when the drum window faces the player) and PermitPulperBossAI's ShredSpin (parryable based on player-facing at hit time) both resolve parryability *after* the wind-up telegraph would need to appear — a static icon can't honestly represent a state that hasn't resolved yet. Both were classified as their un-parryable shape (the safe choice) with the real dynamic tell (pendulum facing / facing check) left untouched as the actual skill-expression mechanism. Not a bug, but worth knowing: the shape/icon on these two specific attacks is deliberately conservative, not a precise readout.

### B31. `PermitPulperAI` / `NoticePusherAI` cannot receive a telegraph without a balance change
Both have zero wind-up window today — the hit lands the same frame the attack starts. Adding a telegraph would require giving them one, which is a balance/design decision, not something folded into the Sprint 0 telegraph work. Connects to the still-open D1 decision (both are currently dormant per owner decision, not used in Worlds 1–2) — relevant only if either is ever activated.

### B23. Persistent singleton count
Five `DontDestroyOnLoad` static-instance MonoBehaviours (`AudioManager`, `SaveSystem`, `ProgressionSystem`, `DifficultyManager`, `CutscenePlayer`). `PROJECT_CONTEXT.md` records two and says not to add more by default. A service-locator refactor is not worth its risk now — but the count should stop growing. (`AttackTelegraphService` adds a sixth — self-bootstrapping, same pattern, noting the count moved rather than treating it as new debt.)

### B36. Forge reveal text is instantiate-and-destroy, not pooled
**Surfaced 2026-08-19 by performance review.** Frequency is low enough (only on forge/upgrade) that this isn't a throughput concern, but `TextMeshPro` instantiation is expensive per-call (mesh allocation, material resolution, font atlas binding), and it happens at exactly the moment the forge transformation's emotional peak plays — a single-frame hitch there is the most noticeable place in the whole game for one to happen. **Fix:** instantiate one label once, keep it deactivated, reuse it. Cheap fix, worth doing whenever this file is next touched.

### B37. `GameObject.Find` retried every forge if the Imagination volume is missing from a scene — **FIXED 2026-08-19**
Fixed as a bundled low-risk item in the Sprint 0 bug-fix pass: added a `_imaginationVolumeSearchAttempted` bool guard so `ColorBloomBeat` only ever calls `GameObject.Find(ImaginationVolumeName)` once per `ForgePresenter` instance, regardless of how many forges/upgrades happen with the volume still absent.

**Surfaced 2026-08-19 by performance review.** `ForgePresenter` caches the Imagination-restore volume reference on success, but on failure (volume absent from the scene) the cache never populates, so the scene-wide `Find` call repeats on every subsequent forge — a standing violation of the project's own no-runtime-`Find`-in-hot-paths convention, low frequency but easy to fix with a `bool` guard.

### B38. HDR is enabled on the mobile render pipeline asset — bandwidth cost, not part of Sprint 0
**Surfaced 2026-08-19 by performance review as an adjacent finding, not part of the reviewed diff.** `Mobile_RPAsset.asset` has `m_SupportsHDR: 1`, which forces FP16 render targets — roughly doubling color bandwidth on tile-based mobile GPUs. Memory bandwidth is the primary driver of sustained thermal throttling, which is exactly the failure mode the 10–15 minute run length is designed to catch (see B1). `m_RenderScale` is already 0.8, suggesting the team is already bandwidth-aware — this is a natural next lever. Needs an on-device A/B test, not a blind toggle.
