# BoxForged — Technical Design Document

- **Status:** Pre-production draft — awaiting owner approval at the pre-production gate
- **Date:** 2026-08-19
- **Engine:** Unity 6 LTS `6000.5.3f1` · URP 17.5.0 · Cinemachine 3.1.7 · Input System 1.19.0
- **Platforms:** iOS + Android, landscape only
- **Authority:** `docs/PROJECT_CONTEXT.md` (existing contracts), `docs/CREATIVE_STATE.md` (CANON), `docs/adr/` (decisions)

> **This document alone does not authorize implementation** — see §11 for the current authorization status. As of 2026-08-19, the owner has authorized production for this sprint's scope; check §11 or `docs/SPRINT.md` before assuming otherwise.

---

## 1. Purpose and scope

This TDD covers the technical work implied by the two decisions locked this session — the camera override and the full scene rebuild — plus the technical shape of the forge "transformation moment," a revised mobile performance budget, and the risks specific to how this project is actually built.

**Production context that constrains every decision here:** BoxForged is built live on a podcast by two non-professional developers with AI assistance. Only two worlds are built by the core team (World 1 Cul-de-Sac, World 2 Backyard/Dojo); all later worlds are intended to come from audience contribution. This makes two properties load-bearing that would otherwise be nice-to-haves:

- **Legibility over cleverness.** A contributor with AI assistance must be able to tell what to copy. Two ways of doing the same thing is worse than one mediocre way.
- **Data over scene surgery.** Content authored as assets can be reviewed, diffed, and generated. Content wired into scenes cannot.

The two team-built worlds are the reference implementation. They are documentation as much as they are levels.

---

## 2. Camera system architecture

Full rationale, derivations, and alternatives are in **[ADR-0001](adr/0001-fixed-low-follow-camera.md)**. Summary of the technical design:

### 2.1 The documented camera was never the real camera

`docs/PROJECT_CONTEXT.md` and the GDD both record offset `(0, 12, -8)` with a hard look-at. The authoritative prefab
(`Assets/_Project/Prefabs/Core/pfb_CM_FollowCam.prefab`) has `FollowOffset: (7.879929, 11, -10)`, `FieldOfView: 40`, `BindingMode: 4` (WorldSpace). No scene overrides it.

The real rig is therefore **pitch ≈ 40.8°, yaw ≈ −38.2°** — not a top-down camera, and not axis-aligned. Correcting this record is a prerequisite for any camera work; the placeholder `(0, 4, -6)` was being compared against a rig that does not exist.

### 2.2 Specify pitch, distance, and FOV — not an offset

An offset triple hides the three quantities that govern readability. For height `h`, pitch `θ`, vertical FOV `f`, the visible ground runs from `h / tan(θ + f/2)` to `h / tan(θ − f/2)`, with the player at `h / tan(θ)`.

The hard constraint is **`θ > f/2`**. Below that the horizon enters frame and ground depth runs to infinity. This is why the placeholder needs qualification: at Unity's default 60° FOV, `(0, 4, -6)` puts the ground far edge at ~62 m — about 2.5× what the current rig draws. It is only safe because the project already runs FOV 40. That relationship was written down nowhere.

### 2.3 Recommended rig

| Parameter | Value |
|---|---|
| Pitch | **36°** |
| Vertical FOV | **45°** |
| Yaw / roll | **0°**, fixed |
| Height | **5.5 m** |
| `FollowOffset` | **`(0, 5.5, -7.57)`** |
| Camera distance | 9.36 m |
| Transform rotation | Euler `(36, 0, 0)` |

| Framing | Current | **Recommended** | Literal `(0,4,-6)` @ FOV 40 |
|---|---|---|---|
| Height / distance | 11.0 m / 16.8 m | **5.5 m / 9.4 m** | 4.0 m / 7.2 m |
| Ground ahead (F) | 16.2 m | **15.3 m** | 10.4 m |
| Ground behind (R) | 6.6 m | **4.2 m** | 3.1 m ✗ |
| Lateral width (W) | 26.5 m | **16.8 m** | 11.4 m |
| Top ray | 20.8° | **13.5°** | 13.7° |

This halves camera height and distance — the requested kid-height framing — at a cost of 0.9 m forward sightline, spending the rest on rear and lateral coverage where it can be designed around.

### 2.4 The binding constraint is rear visibility

Rear visibility is governed almost entirely by camera height, so lowering the camera costs it directly — and it is the one quantity with a hard floor derived from combat data:

| Source | Value |
|---|---|
| `Enemy/SkepticGruntAI.cs:16,19,23` | `moveSpeed 3`, `attackRange 1.5`, `windUpDuration 0.6` |
| `Enemy/BasicEnemyAI.cs:14,16,19` | `2.5 m/s`, `1.5 m`, `0.8 s` |

For the player to see a grunt's entire wind-up on screen: `(R − 1.5)/3 ≥ 0.6`, so **R ≥ 3.3 m**. A literal `(0, 4, -6)` yields R ≈ 3.1 m — an enemy can cross from off-screen into attack range in less time than its own tell lasts.

### 2.5 Acceptance criteria

The rig is correct when, on every aspect from 4:3 to 21:9:

| Metric | Target |
|---|---|
| Ground ahead (F) | ≥ 12 m |
| Ground behind (R) | ≥ 4 m (hard floor 3.3 m) |
| Lateral width (W) | ≥ 16 m |
| Top ray above ground | ≥ 10° |
| Pitch | identical on all aspects |

**These are the spec, not the offset.** Any offset satisfying them is acceptable.

### 2.6 Rig composition

| Component | Setting |
|---|---|
| `CinemachineFollow` | `BindingMode: WorldSpace` — **preserve.** Any `LockToTarget*` mode orbits with the player, fatal for a no-rotation design |
| `CinemachineFollow` | Damping `(0.25, 0.20, 0.25)`; slightly lower on Z. Dodge is a fast lateral burst — lateral lag makes the parry window read late |
| `CinemachineHardLookAt` | **Remove.** A look-at re-pitches as Kid moves toward/away, rolling the horizon. Rotation comes from the transform |
| Aim component | **None** |
| `CinemachineDeoccluder` | **Do not add** — see 2.7 |

**Aspect rule.** Lock *vertical* FOV; it protects the `θ > f/2` constraint. Recover lateral coverage on narrow aspects by increasing camera distance along the view axis. Never vary pitch with aspect. (Locking horizontal FOV instead inverts the problem: on 4:3 the vertical FOV would rise to ~68°, pushing the top ray to ~2° and putting the horizon back on screen.)

### 2.7 Camera collision: no deoccluder, a level constraint instead

`CinemachineDeoccluder` is rejected. A fixed no-rotation camera earns its value through absolute predictability; a deoccluder introduces non-deterministic distance pops exactly during wall-adjacent fights.

Instead, **camera clearance is a level-design constraint, machine-checked in the builder**: every walkable point needs ≥ 8 m clear behind and ≥ 6 m above along the camera axis. This is affordable only because all scenes are being rebuilt (ADR-0002). Residual cases fade occluders; they never move the camera.

### 2.8 Required follow-up work

| Item | Location | Impact |
|---|---|---|
| **Two occlusion systems, both mistuned** | `Systems/CameraOcclusion.cs`, `Systems/BuildingOcclusionFader.cs` | `CameraOcclusion.cs:102` rejects by bounds-*centre* depth; `:105-131` tests the full projected AABB — at low angle a near building's rect covers the player almost unconditionally → mass over-fading. `BuildingOcclusionFader.cs:83` casts one ray at the player's *feet*, missing walls that cover the torso. **Pick one, delete the other** — they even select differently (LayerMask vs tag) |
| **Enemy health bars** | `Enemy/EnemyHealthBar.cs:113` | Billboard refresh gated on *camera* movement only — bars never re-orient when the enemy moves and the camera doesn't. Invisible at 40°, visibly skewed at 36°. `_offset (0,2.5,0)`, `_barWidth 1.4`, `_barHeight 0.3` are world-space and roughly double in apparent size |
| **Hardcoded FOV duplicate** | `Enemy/SpinCycleAI.cs:88` `_normalCameraFoV = 40f` | Must track the rig or the boss-intro handoff pops |
| **Arena radius** | `Player/PlayerController.cs:21` `_arenaBoundaryRadius = 18f` | 36 m diameter vs 16.8 m visible width — over half of an arena fight would be off screen. Must shrink to ~8–9 m radius |
| **Telegraph channel** | See [ADR-0003](adr/0003-attack-telegraph-channel.md) | Blocking — see §4 |

**Unaffected:** `CameraStackWirer`, `HUDCameraInjector`, `HUD3DPositioner`, `HealthBar3D`, `BonusHealthBar3D`, `ChargeMeter3D`, `BossHealthBar` — none reference the gameplay camera transform. No `WorldToScreenPoint` / `ScreenPointToRay` exists anywhere in the tree, so there is no touch-to-world picking to retune.

### 2.9 Controls: the yaw change is the real player-facing consequence

Movement is camera-relative and yaw-projected (`Player/PlayerController.cs:192-200`); `.y` is zeroed, so **pitch does not affect controls at all — only yaw does.** Moving from yaw −38.2° to 0 rotates the entire control mapping by 38°.

Adopt yaw 0 deliberately. The cost is entirely relative to existing level geometry, and every scene is being rebuilt — **doing the camera change and the scene rebuild together is materially cheaper than doing either alone.**

Two secondary notes: `PlayerController.cs:184` disables movement entirely if `_mainCamera` is null, and `_mainCamera` is cached once in `Awake` and never re-resolved — any future rig that *replaces* the Main Camera GameObject at runtime will silently freeze the player. Separately, the degenerate-pitch normalize hazard at extreme angles is strictly *improved* by a lower camera.

---

## 3. Performance budget

### 3.1 Confirmed target

Stable **60 FPS on representative 3–4-year-old iOS/Android hardware**, landscape, with graceful degradation. Run length 10–15 minutes, which makes **sustained thermal behaviour**, not peak frame time, the real acceptance criterion.

### 3.2 The existing budget is retained, with the camera caveat

| Budget | Existing | Status |
|---|---|---|
| Draw calls | < 100 | **Retained**, with per-system allocation below |
| Scene triangles | < 300k | **Retained**, but unverified against actual assets |
| Player character | ~20k tris | Retained |
| Standard enemies | ~10–12k tris | Retained |
| Weapons | < 1,000 tris | Retained |
| Environment props | ~200–600 tris | Retained |
| Steady-state GC | zero per frame | **Retained — and currently honoured.** Zero LINQ in the codebase; `OverlapSphereNonAlloc` into pre-allocated buffers; cached `WaitForSeconds`; cached delegates |

The camera change does **not** require loosening the geometry budget. The naive fear — that a closer camera renders more — is backwards here: the recommended rig's bounded 13.5° top ray gives ~16 m of ground depth versus the current ~24 m. Where the camera *does* cost is **composition**, not count: a low camera makes far walls, backdrops, and prop *sides* visible that a 40° camera never showed. Rooms that were open-topped boxes now need something beyond the far wall. Budget that as scene-dressing geometry, not as a budget increase.

### 3.3 New budgets this project does not yet have

| Budget | Target | Rationale |
|---|---|---|
| **Texture memory, steady state** | **< 150 MB per room** | The dominant unbudgeted cost — see §3.4 |
| **Download size** | **< 200 MB** | Practical over-cellular threshold on both stores. Currently exceeded by StreamingAssets alone — see §3.5 |
| **Enemy HUD draw calls** | ≤ 2 per enemy, ≤ 20 total | `Enemy/EnemyHealthBar.cs:181,196` creates two `new Material` instances per enemy at runtime |
| **Telegraph indicators** | ≤ 12 concurrent, pooled | ADR-0003; per-wind-up instantiation is forbidden |
| **Shadow distance** | 25 m (from 40) | `QualitySettings` shadowDistance 40 against a 16 m visible ground depth wastes most of a **256×256** shadow atlas (`Mobile_RPAsset`). Halving distance roughly doubles effective resolution at zero cost |
| **Thermal** | No sustained frame-time regression across a full 15-min run | Run length makes this the real test |

### 3.4 Texture memory is the dominant risk

Measured from the repository:

- **2.6 GB of source textures**, 353 files. Individual BaseColor maps run 27–31 MB (`Grasscutter_BaseColor.png` 30.9 MB, `SpinCycle_BaseColor.png` 29.3 MB, `Cowgirl_BaseColor.png` 28.3 MB).
- **3.1 GB across 124 FBX** — ~25 MB average per model.
- Import settings: **essentially every texture imports at `maxTextureSize: 2048`**, with only a `DefaultTexturePlatform` entry — **no Android or iPhone platform overrides**, `textureFormat: -1` (Auto), `textureCompression: 1` (Normal). Only 34 textures are capped at 1024 and 15 at 512.

At 2048² a compressed texture with mipmaps costs roughly 2.5 MB (ASTC 6×6) to 5.6 MB (ASTC 4×4 / ETC2 RGBA). A room drawing 40–60 distinct textures therefore lands at **100–150 MB of texture memory for a single room** — and that is before audio, meshes, and the 326 MB video payload.

The failure mode is not a crash. It is **thermal**: sampling 2048² maps for props that occupy 40 screen pixels destroys cache coherency and burns memory bandwidth continuously, which on a 3–4-year-old device means throttling partway through a 10–15 minute run. This is precisely the scenario the run length is designed around.

**Recommended remedy (production work):** a texture import-policy pass driven by an `AssetPostprocessor`, with per-category caps — characters and bosses 1024, weapons 512, environment props 512, UI 256 — plus explicit Android/iOS ASTC overrides. This is high value, low risk, scriptable, and exactly the kind of task a non-expert team can execute confidently with AI assistance. It should precede any further asset import.

**This has not been verified on device and should not be treated as measured.** The numbers above are derived from file inspection; the budget is a hypothesis to profile against, not a finding.

### 3.5 Package size: 326 MB of video for a feature that was cut

`Assets/StreamingAssets/Cutscenes/` holds **10 `.mp4` files totalling 326 MB**, and StreamingAssets ships verbatim — Unity does not compress or strip it.

| File | Size | Still in scope? |
|---|---|---|
| `unboxed_intro.mp4` | 112.3 MB | No |
| `cowboy_ninja_skills.mp4` | 34.9 MB | No |
| `ninja_skills.mp4` | 29.4 MB | No |
| **`spincycle_standoff.mp4`** | **26.8 MB** | **Yes — the only boss intro** |
| `boy_putting_box_on.mp4` + `_v2` | 46.3 MB | No |
| `wild_west_transform` / `_change_phone` | 41.1 MB | No |
| `forge_whip_craft` / `forge_whip_2` | 35.1 MB | No |

`docs/CREATIVE_STATE.md` locks cutscenes to **boss intros only**. Of ten videos, exactly one is a boss intro. Retiring the rest recovers roughly **300 MB of download size** — the single largest and cheapest technical win available, and it is a content decision rather than an engineering one.

### 3.6 A correction to the batching guidance

`docs/PROJECT_CONTEXT.md` correctly says to cache owned material instances. One nuance should be recorded, because the codebase currently contains advice pointing the wrong way.

`Mobile_RPAsset` has `m_UseSRPBatcher: 1`. **Under the SRP Batcher, per-instance `Material` copies still batch** as long as they share a shader variant — so the widespread `renderer.material` pattern used for colour-flash telegraphs is *not* the batching problem that general Unity folklore suggests. Conversely, **`MaterialPropertyBlock` breaks SRP batching.** The comment at `Enemy/SpinCycleAI.cs:1123` endorsing MPB is therefore a performance trap on this render pipeline.

Material *lifetime* hygiene in this codebase is genuinely good — every `renderer.material` and `new Material` site has a matching `Destroy`. Preserve that.

### 3.7 Profiling protocol

No optimization is accepted without a before/after measurement on device.

1. Representative scenario: one full room clear at `maxConcurrentEnemies`, plus one full boss fight.
2. Capture on a real 3–4-year-old device, not the Editor and not a flagship.
3. Record CPU main/render thread, GPU frame time, draw calls, SetPass calls, texture memory, GC allocation per frame, and **frame time at minute 1 versus minute 12** for thermal.
4. Record the scenario and device alongside the numbers. A figure without its scenario is not evidence.

### 3.8 Executing the protocol — see `docs/PERFORMANCE_PROFILING.md`

§3.7 states *what* to measure. **[`docs/PERFORMANCE_PROFILING.md`](PERFORMANCE_PROFILING.md)** is the step-by-step execution of it for `CulDeSac_WildWestCity` on a physical iPhone, written to be followed by a non-expert without further engineering support: exact Unity and Xcode menu paths, which Profiler modules to open and what to read in each, which Instruments template to use, and a **results template that is the recording home for every device measurement** — results go there, not into this document.

That checklist introduces **no new budgets**. Every pass/fail line in it cites §3.1, §3.2, §3.3, or [ADR-0004](adr/0004-world1-single-continuous-scene.md) §8 as its source.

Three things it establishes that §3.7 leaves open, and which matter for whether the numbers are trustworthy:

- **Two passes are required, not one.** A Development Build (needed for the Unity Profiler) inflates CPU frame time. So the Unity Profiler pass owns *counts and structure* — draw calls, SetPass calls, triangles, GC allocation, texture memory — and a **non-development** build profiled with Xcode Instruments owns the *frame-time and thermal verdict*. Reporting a Development Build millisecond figure as the answer to the 60 FPS budget is wrong.
- **The minute-1-vs-minute-12 comparison cannot be one Unity Profiler recording.** The Profiler frame buffer caps at 2000 frames (~33 s at 60 FPS). The 15-minute thermal run is measured with Xcode's continuous gauges, optionally supplemented by two short Profiler captures at each end.
- **Device class is part of the result.** A pass on a device newer than the §3.1 target class is a lower bound, not a pass — consistent with the standard §3.4 sets on itself.

**One correction this document needs.** §3.3's shadow-distance row describes the current setting as 40 m against a **256×256** atlas. The live `Assets/Settings/Mobile_RPAsset.asset` actually reads `m_ShadowDistance: 50`, `m_MainLightShadowmapResolution: 1024`, `m_ShadowCascadeCount: 1`. The **25 m target is unchanged and still correct**; only the description of the current state is stale, and the gap is larger than §3.3 implies (50 → 25, not 40 → 25). Flagged here rather than edited in place, since correcting a budget table's prose is a separate reviewed change.

---

## 4. Attack readability — a blocking dependency of the camera change

Full analysis in **[ADR-0003](adr/0003-attack-telegraph-channel.md)**. Stated here because it gates §2.

**There is no telegraph system in BoxForged.** A tree-wide search for telegraph, indicator, AOE, decal, and projector terms returns zero gameplay hits. The ground-indicator AOEs referenced in planning material do not exist in code.

Every attack tell is a **whole-body material tint** (`SpinCycleAI.cs:962-967`, `BasicEnemyAI.cs:225,257`), and hue *is* the attack identity — red DrumSlam is un-parryable (`:677`), yellow Haymaker is conditionally parryable (`:699`).

Whole-body tint is a silhouette-fill signal that works because the current 40.8° camera sees every enemy separated and unoccluded against the ground. At 36° with half the height and distance, enemies occlude each other, props occlude at torso height, and less body surface carries the hue. The tint also carries no spatial information — no direction, no landing point — which the top-down view previously supplied from context.

Separately, encoding parryable versus un-parryable **entirely in hue** is a pre-existing accessibility defect (~8% of males, plus small-screen and off-axis OLED shift). The camera change does not cause it; it removes the context that masked it.

**Decision:** add an occlusion-independent overhead telegraph riding the existing overlay camera stack, carry parryability on **shape** rather than hue, add distinct audio per class, keep the body tint as reinforcement, and raise it from the existing `WindUp(Color)` seam. URP decal projectors are rejected — `Mobile_Renderer.asset` has `m_RendererFeatures: []` and decals require a mobile depth prepass, a permanent full-frame cost against an already-pressured budget.

**Approving the low camera without the telegraph channel is the one combination that should not ship.** If the telegraph work cannot be funded, the correct response is to raise the camera back toward the current pitch, not to proceed.

---

## 5. Forge system — the transformation moment

### 5.1 Current implementation

The forge domain logic is in good shape and is the model the rest of the project should follow. `Systems/ForgeController.cs:22-46` is close to pure rules with a deliberately correct ordering — slot availability is checked *before* resources are spent:

```csharp
var instance = new WeaponInstance(weaponObject, WeaponTier.Standard);
if (!_weaponInventory.AddToWeaponSlot(instance)) return false;  // check slots FIRST
_cardboardResource.Spend(weaponObject.forgeCost);               // spend AFTER success
_weaponInventory.RemoveFromMaterialBag(bagIndex);
OnWeaponForged?.Invoke(instance);
```

Flow: `WorkbenchProp` polls proximity (`:59-85`, `_interactRadius = 3f`) → `ForgePanel` opens → `ForgeController.TryForge(bagIndex)` → `WeaponInventory.AddToWeaponSlot` auto-equips if the filled slot is active (`:41-42`) → `WeaponHolder.Attach` destroys and re-instantiates the weapon on the hand bone (`:164-167`, with a standing pooling TODO).

### 5.2 The presentation layer is an empty socket

This is the most useful finding for this work: **there is nothing to unpick.**

- `Systems/WeaponForgeAnimation.cs` exists (95 lines) but its GUID appears in **zero prefabs and zero scenes**. It is not on `pfb_player.prefab`. It has never run.
- What it would do is minimal: reposition one existing `ParticleSystem` to `WeaponHolder.MuzzlePosition` and `PlayOneShot` a clip (`:63-77`). **No duration, no tween, no scaling, no colour work, no camera work, no text.** Forge and upgrade route to the identical handler (`:45-46`), so an upgrade currently looks exactly like a forge.
- `OnWeaponForged` and `OnWeaponUpgraded` (`ForgeController.cs:10-11`, `Action<WeaponInstance>`) have **zero live subscribers**.

So today the only forge feedback is the panel text refreshing, plus a one-shot video on the first-ever forge (`ForgeController.cs:38-43`).

### 5.3 Available seams

| Seam | Location | Notes |
|---|---|---|
| `OnWeaponForged` / `OnWeaponUpgraded` | `ForgeController.cs:10-11` | The natural hook. Payload carries `Data` (icons, prefabs, rarity, abilities) and `Tier`, so a presenter can branch forge-vs-upgrade and read rarity for the colour bloom |
| `WeaponHolder.MuzzlePosition` | `WeaponHolder.cs:38-46` | Public world anchor, already the intended spawn point |
| Tier weapon prefabs | `WeaponObjectSO.cs:36-37` | `epicWeaponPrefab` / `legendaryWeaponPrefab` are **declared but never read** by `ForgeController` or `WeaponHolder`. The tier-visual path is a data slot already waiting for the weapon-glow feature |
| `HitStopManager` | `Core/HitStopManager.cs` | Existing juice service — freezes animators *without* touching `timeScale`, fires `CinemachineImpulseSource.GenerateImpulse` |
| `VolumeProfile_ImaginationRestore.asset` | already instanced as `ImaginationRestore_Volume` in `CulDeSac_Room1` | Existing post-process volume; the natural driver for the colour bloom |
| `RarityIndicator.cs` | — | Existing tier-VFX pattern (`_rareVFX` / `_legendaryVFX` child toggles) worth matching |

### 5.4 Two constraints that will otherwise cost a debugging session

The live forge panel is a **hard modal pause** (`UI/ForgePanel.cs:83-86`):

```csharp
if (_panel != null) _panel.SetActive(true);
Time.timeScale      = 0f;
AudioListener.pause = true;
```

1. **Any transformation coroutine must use unscaled time** (`WaitForSecondsRealtime` / `Time.unscaledDeltaTime`) or it will never advance. `GameManager.cs:77,82` already uses this pattern.
2. **The forge sound will be silent** unless its `AudioSource` sets `ignoreListenerPause = true`, because `AudioListener.pause` is true for the whole panel lifetime. `WeaponForgeAnimation`'s `PlayOneShot` does *not* set this — attaching it as-is produces a silent forge. `CutscenePlayer.cs:520-523` is the existing precedent for the flag.

Also note `WorkbenchProp.cs:70` early-returns when `Time.timeScale == 0`, so `OnPlayerExited` cannot fire while the panel is open; the panel closes only via its close button.

### 5.5 Recommended technical shape

Add a **`ForgePresenter`** — a separate component subscribing to `OnWeaponForged` / `OnWeaponUpgraded`, running an unscaled coroutine sequence. Keeping presentation out of `ForgeController` preserves the one system in this codebase that is already close to testable domain logic, and lets the moment be re-authored without touching forge rules.

Sequence (anim → bloom → sound → text → glow), all unscaled:

1. **Anticipation** — object rises from the muzzle anchor, cardboard gathers.
2. **Transformation** — scale/rotation tween on the swap from raw object to named weapon.
3. **Colour bloom** — drive the existing `ImaginationRestore` volume; intensity keyed to `WeaponInstance.Tier`.
4. **Impact** — `HitStopManager` beat plus impulse.
5. **Imaginative text** — the named-weapon reveal ("a broomstick" → "**Bo Staff**"). No dedicated field exists; `ForgePanel._statusText` is a single TMP blob. Needs a new element.
6. **Weapon glow** — implement the `epicWeaponPrefab` / `legendaryWeaponPrefab` path.

**Decided (owner, 2026-08-19): in-world, at the workbench.** The transformation plays in the 3D world rather than inside the paused modal panel — this reads considerably better at the new low, close camera and reinforces both changes together. The panel's role narrows to slot selection; the forge moment itself becomes a world-space event. This is a UX change, not just a visual one — `ForgePresenter` should drive the world-space sequence (§5.5 above), and `WorkbenchProp`/`ForgePanel` need to hand off to it rather than playing the moment inside the panel itself.

---

## 6. Scene and level architecture

Full analysis in **[ADR-0002](adr/0002-full-scene-rebuild.md)**.

### 6.1 What is preserved

`LevelBuilder` is genuinely data-driven and is preserved. It runs four spawn passes then a deferred runtime NavMesh bake (`Systems/LevelBuilder.cs:23-31`), reading exactly one ScriptableObject type — `WeaponDropTableSO`, which carries scattered objects, loot-zone objects, cardboard piles, workbench positions, and env props.

Baked NavMesh is already irrelevant: `LevelBuilder.cs:65` calls `NavMesh.RemoveAllNavMeshData()` and rebakes at runtime from physics colliders. The five `Scenes/<name>/NavMesh.asset` files are vestigial — props need colliders, not read/write meshes.

Also portable: all prefabs, all weapon/ability/upgrade/difficulty/character SOs, and zone routing (hardcoded in `GameManager.cs:18-43`, not scene data).

### 6.2 What the rebuild destroys

**`RoomManager._rooms` is the single biggest loss.** `RoomData` is a plain `[Serializable]` class (`Systems/RoomManager.cs:9-22`), **not** a ScriptableObject. `pfb_RoomManager.prefab` ships with `_rooms: []`, so every room definition exists only as prefab-instance overrides inside each scene, with spawn points stored as `objectReference` fileIDs pointing at **scene-local GameObjects**:

```
propertyPath: _rooms.Array.data[0].roomName             value: The Arrival
propertyPath: _rooms.Array.data[0].maxConcurrentEnemies value: 3
propertyPath: '_rooms.Array.data[0].spawnPoints.Array.data[0]'  objectReference: {fileID: 816146668}
```

Room names, concurrency caps, spawn wiring, `exitGate`, `propsGroup`, `bossOwnedWin` — all destroyed with the scene. Also lost: `EnemySpawnPoint` placement and per-point config, `Ground`, the four boundary colliders, lighting/RenderSettings, and 88–114 `pfb_hud_v4` instance overrides per scene.

So: **the environment layer is portable; the encounter layer is not.**

### 6.3 Decision — extract room data before rebuilding

1. Promote `RoomData` to **`RoomDataSO`**, with spawn points as `Vector3` data rather than scene references.
2. Extend `LevelBuilder` to spawn enemy spawn points from that data, as it already does for props, pickups, cardboard, and workbenches.
3. Rebuild scenes as **thin composition roots** — ground, boundaries, lighting, volumes, manager/player/HUD prefabs. No gameplay content authored in-scene.
4. Add **camera-clearance validation** to the builder: assert ≥ 8 m rear / ≥ 6 m overhead clear volume over walkable area, and combat radius ≤ 9 m. Fail loudly in the Editor. *(Combat radius is measured per §6.4.1's metric M1, not over the whole spawn roster — restated 2026-09-01 by [ADR-0006](adr/0006-world2-zone-scale-and-arena-metric.md). M1 and M2 are both computable from `RoomDataSO` plus scene geometry and are **not yet implemented in `LevelBuilder`**; both were violated silently for a full design pass, which is the argument for coding them.)*

Extraction must happen **before** the scenes are abandoned, or the data is lost. Rebuilding first and extracting later means authoring every room twice.

The payoff beyond this milestone: a contributor authors a room by creating a data asset, not by opening a scene and wiring scene-local references correctly. That is the difference between the audience-contribution goal being reachable and not.

### 6.4 Room dimensions are now constrained by the camera

`Player/PlayerController.cs:21` sets `_arenaBoundaryRadius = 18f` — a 36 m-diameter arena against 16.8 m of visible width. Over half of an arena fight would happen off screen. **Combat space must shrink to roughly 8–9 m radius**, independent of any creative decision.

#### 6.4.1 How the ≤ 9 m combat radius is measured — restated 2026-09-01

**Restated by [ADR-0006](adr/0006-world2-zone-scale-and-arena-metric.md) §2.1.** The 9 m figure is retained; what changed is what it is measured over. It had been read as *"the enclosing circle of a zone's whole spawn roster"*, which describes a state the runtime cannot produce and which World 1's shipped 59.5 m street does not satisfy — so it was documented but unenforced, and it blocked a playtest-driven fix to World 2 with 0.04 m of false headroom.

> **Metric M1.** The live enemy set a `RoomManager` zone can produce is exactly a contiguous window of at most `maxConcurrentEnemies` entries in `RoomDataSO.spawnPoints[]`, because `TrySpawnNext` advances a monotonic `_nextSpawnPointIndex` and `OnSpawnedEnemyDied` refills one slot per death (`Systems/RoomManager.cs:359-424`).
>
> A zone passes when, for **every** such window, the minimum enclosing circle of the window's **closing** spawns — those whose AI pursues the player — has radius **≤ 9 m**.
>
> **Position-holding spawns are excluded** from that circle (canon-stationary duellists, dormant risers before they rise, pre-placed inactive bosses), and each must sit **≥ 5 m outside** it so the player meets it as a distinct engagement.

The exclusion follows from §2.4, which is where the 9 m figure comes from: it derives from `R ≥ 3.3 m`, the requirement that the player see a **closing** grunt's entire wind-up before it reaches attack range. An enemy that never closes cannot violate that — the player chooses when to approach it.

#### 6.4.2 Boss arenas — metric M2

Boss arenas are measured separately, by **[ADR-0006](adr/0006-world2-zone-scale-and-arena-metric.md) §2.2**, against visual mesh footprints (not renderer AABBs, not colliders): outer walkable radius ≥ the world's authored value, **radial fight band ≥ 8.5 m** everywhere, interior obstruction ≤ 2% of floor area and ≤ 1.0 m wide, and **boss longest committed traversal ≤ 0.35 × arena diameter** (World 1's playtested reference is 0.284). The retired *"largest inscribed obstacle-free circle ≥ 8.5 m"* reading is unsatisfiable for any arena with a central feature — it evaluates to `(R − r_t)/2`, demanding a 34.7 m arena for a 0.70 m tree trunk.

### 6.5 Debt to clear during the rebuild, not carry forward

| Item | Evidence |
|---|---|
| ~~Three Build Settings scenes do not exist on disk~~ — **FIXED 2026-08-27**, entries removed from `EditorBuildSettings.asset` | `LoadingScreen.unity`, `TownSquare_Room1.unity`, `TownSquare_BossHall.unity` are gone from Build Settings. `GameManager.cs:34` `ZoneStartScene[1]` is still unreachable (World 2 doesn't exist yet) — unrelated to the Build Settings fix |
| Two parallel enemy-spawn systems run simultaneously | `RoomManager`'s `EnemySpawnPoint` path and `Enemy/EnemySpawner.cs`'s `Transform[]` coroutine, both active in CulDeSac scenes |
| Two forge UIs, the better one dead | `UI/ForgeUI.cs` (514 lines, zero scene/prefab references) vs live `UI/ForgePanel.cs` (193 lines) |
| `_spawnRoot` null | `{fileID: 0}` in `CulDeSac_Room1` — builder content lands at scene root |
| Dead data fields | `WeaponSpawnEntry.useRarityOverride` / `rarityOverride` never read (`LevelBuilder.cs:128-143`) |
| `Editor/Sprint4SceneSetup.cs` superseded | Wires `HUDController`; live `GameManager.cs:55` field is `HUDController_V2`. Do not use for the rebuild |
| Orphaned boss content | `BossHallDoor._bossSceneName = "TownSquare_BossHall"` (`:19`) — scene does not exist. `BossRoomWeaponSpawner` in no scene or prefab |

---

## 7. Systems reconciliation

Findings that are architecturally relevant but were not part of the two locked decisions. **All are backlog candidates, not authorized work.**

### 7.1 Two parallel ability systems — highest contributor-confusion risk

Two complete, independent ability stacks coexist:

| Layer | V3 | V4 |
|---|---|---|
| Weapon data | `WeaponData` (81 assets) | `WeaponObjectSO : WeaponData` (12 assets) |
| Ability data | `WeaponAbilityData` (3 assets) | `AbilitySO` (24 assets) |
| Ability logic | 5 SO subclasses, `IEnumerator Activate` | 16 `AbilityBehaviour` SOs, `void Execute` |
| Context struct | `AbilityActivationContext` | `AbilityExecutionContext` |
| Driver | `CombatController` | `AbilityExecutor` |

They negotiate at runtime — `CombatController.cs:764` checks `_abilityExecutor.HasActiveSpecialAbility` so a V4 ability can suppress the V3 path. `LassoAbilityData` and `QuickdrawBladeAbilityData` are code with no assets — dead.

For a project whose thesis is "non-experts can extend this with AI assistance," two ways to author an ability is the single most damaging piece of debt in the codebase. Neither a contributor nor an AI assistant can reliably infer which to use.

`AbilityExecutor` itself is well built — pre-allocated `Collider[]` / `EnemyStats[]` buffers, cached delegates, a `readonly struct` context, a three-line `Update`. The problem is duplication, not quality.

### 7.2 Cutscene scope — the system is not over-built, it is the wrong tool

`Core/CutscenePlayer.cs` (610 lines) is a **full-screen H.264 video player**, not a Timeline or camera-move system. It implements Android `jar://` StreamingAssets extraction, per-clip RenderTexture rebuild, letterboxing, a skip button with reveal delay, a loading-screen overlay with min-hold and fade, input gating, and a code-built canvas.

Of its five call sites, **only one is a boss intro** (`SpinCycleAI.cs:352-355`). The others are the game intro, zone entry, character showcase, and the first-forge tutorial — all outside the locked "boss intros only" scope.

And **the other boss intro does not use it at all.** `Enemy/PermitPulperBossIntro.cs` (259 lines) is a separate hand-rolled in-engine cinematic that disables the `CinemachineBrain`, drives `Camera.main` directly, freezes the boss animator mid-clip, and fires a Cinemachine impulse **via reflection**. It has no skip.

So cutting scope to boss intros deletes the *callers*, not the player, leaving a 610-line video player serving one call site — with the Android extraction, loading-screen hold, and RT rebuild all dead weight, plus ~300 MB of retired video (§3.5). The genuinely reusable boss-intro asset is `PermitPulperBossIntro`, which is currently the *least* general of the two and hardcoded to one boss.

**Recommendation:** treat "boss intro" as one small in-engine system generalized from `PermitPulperBossIntro`, and scope `CutscenePlayer` down or retire it with its unused video payload.

### 7.3 Orphaned content

| Script | Status |
|---|---|
| `PermitPulperBossAI.cs` (757 lines) + `PermitPulperBossIntro.cs` (259) | **In no scene and no prefab.** `GameManager.cs:140` and `BossHealthBar.cs:37` reach for it with `FindAnyObjectByType` and always get null. 1,016 lines that cannot currently run |
| `PermitPulperAI.cs`, `NoticePusherAI.cs` | Each on one prefab; **neither prefab is referenced anywhere** |
| `NoticePusherPatrol.cs` | Fully orphaned |
| `LaundryTumbler.cs` | **Live** — 15-line `transform.Rotate` on `pfb_enemy_spincycle`, which is in `CulDeSac_BossArena` |

`docs/CREATIVE_STATE.md` lists PermitPulper, NoticePusher, and LaundryTumbler as **WORKING**, not CANON — "candidate enemy thoughts." Per the task constraints these go to `docs/BACKLOG.md` as an explicit keep-or-cut decision rather than being silently resolved. Note the creative-state entry and the code disagree: `LaundryTumbler` is shipping today.

### 7.4 Save system has a version field but no migration

`Core/SaveSystem.cs:26` writes JSON to `Application.persistentDataPath/save.json`, synchronously on the main thread. `version` is used purely as a corruption sentinel (`:70-78`), never as a migration key — there is no `if (loaded.version < N) Migrate(...)`. Bumping it would silently do nothing, and a v1 save read by future v2 code is simply misread.

`Data` is exposed as a mutable reference with the header explicitly endorsing external mutation, and `ProgressionSystem.cs:252` does exactly that without saving — per-kill Spark flushes only on stat upgrade, IP conversion, or `OnApplicationPause`.

This is low-severity today (10 fields, single player, pre-release) and becomes expensive the moment real players have saves. Backlog before first release, not now.

### 7.5 Five persistent singletons

`AudioManager`, `SaveSystem`, `ProgressionSystem`, `DifficultyManager`, and `CutscenePlayer` are all `DontDestroyOnLoad` static-instance MonoBehaviours. `PROJECT_CONTEXT.md` records two and says not to add more by default. Recording the drift rather than proposing a rewrite — a service-locator refactor is not worth its risk at this stage, but the count should stop growing.

---

## 8. Test strategy

### 8.1 Current state: there is no test infrastructure

- **Zero** test folders, EditMode/PlayMode assemblies, or `[Test]` / `[UnityTest]` methods anywhere under `Assets/`.
- `com.unity.test-framework: 1.7.0` is installed and unused.
- **Exactly one `.asmdef` exists in the whole project**, and it is not a test assembly. Its complete contents:
  ```json
  { "name": "StatSystem" }
  ```
  Every other script — all gameplay, save, ability, and cutscene code — compiles into the default `Assembly-CSharp`.

The absence of asmdefs is the practical reason no tests exist: a test assembly must reference `Assembly-CSharp`, which is awkward. The nearest thing to a test is `Core/SaveTester.cs`, a manual IMGUI debug panel that **must not ship in a build**.

The project's own Definition of Done requires "affected tests pass." **There is currently no mechanism to satisfy that clause.**

### 8.2 Proposed minimum

Deliberately small. The goal is a working ratchet, not coverage.

1. **One assembly boundary for testable logic.** Move genuinely pure rules — forge rules, durability, inventory, stat math — behind an asmdef with a matching EditMode test assembly. Do not attempt to asmdef the whole project; that is a large, high-friction refactor with little near-term payoff.
2. **EditMode tests for the forge rules first.** `ForgeController.TryForge` is the best-shaped logic in the codebase (check-then-spend ordering is exactly the kind of invariant a regression would silently break) and it is about to gain a presentation layer. Highest value per unit of effort.
3. **`RoomDataSO` validation tests** alongside the ADR-0002 extraction — spawn points inside bounds, concurrency caps sane, camera clearance satisfied. These protect audience-contributed content, which is the whole point of the extraction.
4. **Manual test protocol for camera and readability** (§2.5, ADR-0003 validation). These are perceptual and cannot be automated; they need a written, repeatable procedure instead.

### 8.3 Explicitly out of scope

PlayMode integration tests over the full run loop. The scene rebuild is about to invalidate every scene they would target. Revisit once new scenes are verified.

---

## 9. Risks specific to a two-person non-expert team building live

| # | Risk | Why it bites *this* team | Mitigation |
|---|---|---|---|
| 1 | **Texture memory / thermal throttling** (§3.4) | Invisible in the Editor and on a modern phone. Surfaces as gradual slowdown 10 minutes into a run — exactly the moment a livestream demo has an audience. Hardest class of bug for a non-expert to diagnose | Import-policy `AssetPostprocessor`; profile a full 15-min run on a real 3–4-year-old device before any public demo |
| 2 | **Two ability systems** (§7.1) | A contributor or AI assistant cannot infer which to use, and will pick wrong roughly half the time | Choose one, migrate, delete the other, document the survivor |
| 3 | **No tests, no asmdefs** (§8.1) | AI-assisted changes are fast and confident; without a ratchet, silent regressions accumulate faster than a two-person team can notice | Minimum viable test boundary around forge and room-data rules |
| 4 | **Room data trapped in scenes** (§6.2) | Directly blocks the audience-contribution premise. Also means camera re-tuning costs a full re-author | `RoomDataSO` extraction before rebuild (ADR-0002) |
| 5 | **326 MB of retired video shipping** (§3.5) | Exceeds the entire practical download budget before any game content | Retire out-of-scope videos; a content decision |
| 6 | **6.8 GB `.git`, 3.2 GB working tree** | Clone and branch times become a real tax on a two-person team; no Git LFS configured. Vendor demo content adds ~1.2 GB (`Off Axis Studios` 1.1 GB, plus Polylised, SimpleTown, ExplosiveLLC, and a 60 MB `Screenshots/` folder inside `Assets/`) | Audit vendor content; move `Screenshots/` out of `Assets/`; evaluate LFS |
| 7 | **Unapproved third-party content in the tree** | `PROJECT_CONTEXT.md` approves Meshy and Polyworks. `SimpleTown`, `ExplosiveLLC` (RPG Character Mecanim, SuperCharacterController), and `Polylised` are present but unlisted | Reconcile against `docs/TECHNICAL_DECISIONS.md`; no new packages without owner approval |
| 8 | **Camera change without telegraph work** (§4) | Would ship a combat system whose central read is unreliable — and it would present as "the game feels unfair," which is very hard to trace back to a camera decision | Treat ADR-0001 and ADR-0003 as one decision |
| 9 | **Doc/reality drift** (§2.1) | The camera was documented wrong for an entire phase and nobody caught it. On a project where docs *are* the AI's context, a wrong doc actively propagates errors | Correct `PROJECT_CONTEXT.md` and the GDD; prefer citing prefab/asset values over prose |
| 10 | **73 scene-search call sites** | `FindObjectOfType` / `FindWithTag` across the codebase. Architecturally impure — **but load-bearing**: it is what lets a rebuilt scene work without manual re-wiring, which is exactly what protects a non-expert team | **Do not remove.** Formalize it: keep lookups in `Awake`/`Start`, never in hot paths, and document it as the intended scene-composition pattern |

Item 10 deserves emphasis, because the textbook advice is wrong here. Self-wiring by tag lookup is normally a smell; on this project it is the mechanism that makes a full scene rebuild survivable by two non-experts. It should be made explicit and consistent rather than replaced with dependency injection that nobody on the team can debug at 11pm on a stream.

---

## 10. Owner decisions

1. ~~**Forge transformation staging**~~ (§5.5) — **DECIDED 2026-08-19: in-world, at the workbench.**
2. ~~**Cutscene retirement**~~ (§3.5) — **DECIDED 2026-08-19: retired.** All nine non-boss-intro videos deleted; ~300 MB recovered.
3. ~~**PermitPulper / NoticePusher / LaundryTumbler**~~ (§7.3) — **DECIDED 2026-08-19: leave dormant.** No deletion; not fit into Worlds 1–2; revisit for World 3+.
4. ~~**Ability system consolidation**~~ (§7.1) — **DECIDED 2026-08-19: keep V4.** See `docs/BACKLOG.md` D3. The investigation corrected the scope substantially: `WeaponData` (V3) is V4's own equipped-data layer and stays permanently, so this is not an 81-asset migration. Only 3 ability assets (Shuriken, SixShooter, DynamiteBundle) need real work, and that work is blocked on extending V4's architecture — tracked as B4, not as a data conversion.
5. ~~**Preserve existing room encounter data?**~~ — **RESOLVED BY EXECUTION.** `RoomData` was promoted to `RoomDataSO` and every room's encounter data extracted per ADR-0002 *before* the legacy per-room scenes were deleted in `84a3a44e`. Both continuous worlds (`CulDeSac_WildWestCity`, `Backyard_Dojo`) now author encounters exclusively as `RoomDataSO` assets. Nothing time-sensitive remains here.
6. **Boss-room camera** — still open. Accept a possible per-encounter profile if SpinCycle's airborne slam proves unreadable at 36°? Decide after playtest evidence, per ADR-0001 alternative 5.

---

## 11. Approval gate

This document, `docs/ARCHITECTURE.md`, and ADRs 0001–0003 constitute the pre-production technical deliverable.

**PRODUCTION AUTHORIZED 2026-08-19** — owner said "Start Sprint 0." ADRs 0001–0003 are approved. Implementation is underway on `feature/sprint-0-foundation-rebuild` per `docs/SPRINT.md`. This line is the authoritative authorization record for this document; if it is out of date, check `docs/SPRINT.md`'s authorization record or the branch's git log before assuming production is not authorized.
