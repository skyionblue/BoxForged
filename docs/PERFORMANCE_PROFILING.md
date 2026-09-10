# BoxForged — On-Device Performance Profiling Checklist (iOS)

- **Status:** Operational checklist. Executable by the owner without further engineering support.
- **Date:** 2026-08-27 · World 2 scenarios added 2026-09-08
- **Scope:** **Both team-built worlds** on a physical iPhone — `CulDeSac_WildWestCity` (World 1, three zones, scenarios **S1–S4**) and `Backyard_Dojo` (World 2, three zones, Crane Duelist and the Grasscutter boss, scenarios **S5–S9**).

**Stale as of 2026-09-08 — re-run before trusting any number below.** Two things changed since the 2026-08-27 session:

1. **The undocumented 30 FPS cap is gone.** §7's "Deliberate 30 FPS cap" finding was acted on — `Application.targetFrameRate` is now `60` (`Core/GameManager.cs:105`, with `QualitySettings.vSyncCount = 0` at `:104`). **Every frame-time number captured under the old cap is not comparable to anything measured now**, and the ~33 ms figure in the 2026-08-27 table is the cap, not a budget failure. Treat that whole session as a World-1, 30-FPS-cap baseline, not a current result. See `docs/BACKLOG.md` B112 and `docs/TECHNICAL_DECISIONS.md` §Engine and pipeline.
2. **World 2 is now covered, but has never been measured.** `Backyard_Dojo` (three zones, `CraneDuelistAI`, `GrasscutterAI`) shipped after this checklist was written. Scenarios **S5–S9** (§1.1) and results sections **H–O** (§7.1) were added 2026-09-08 to cover it. **They are all blank — no World 2 number of any kind has ever been captured on a device.** The World 1 rows above them are stale; the World 2 rows below them are empty. Neither is a current result.

**Read §1.1 before scheduling a World 2 session.** There is currently no working in-game route from World 1 to World 2 in a device build, which changes how S5–S9 have to be reached and makes S9 unrunnable as written.

- **Authority:** This document **invents no budgets and changes none**. Every budget referenced here is quoted from `docs/TECHNICAL_DESIGN.md` §3.2 / §3.3 / §3.1, `docs/adr/0004-world1-single-continuous-scene.md` §8, or — for World 2 — `docs/adr/0005-world2-single-continuous-scene.md` §3 and `docs/adr/0006-world2-zone-scale-and-arena-metric.md` §5.1, and the source is named on every line. `docs/TECHNICAL_DESIGN.md` §3.7 remains the authoritative statement of *the protocol*; this document is the step-by-step *execution* of that protocol.

---

## 0. Read this first — the three rules that make the numbers mean something

**Rule 1 — a number without its scenario and device is not evidence.** This is TDD §3.7 item 4. Every table in §7 has a scenario column and a device column. Fill them in, always.

**Rule 2 — a pass on a newer phone is not a pass.** The budget in TDD §3.1 is *stable 60 FPS on representative 3–4-year-old hardware*. If the test iPhone is newer than that (an iPhone 14 or later, roughly), then a result inside budget is a **lower bound only** — it means "not obviously broken on good hardware", not "passes". Record the exact model and note it. A result *outside* budget on a fast phone, however, is a real failure and can be trusted immediately.

**Rule 3 — two passes, because neither tool sees everything.**

| Pass | Build | Tool | What only this pass can tell you |
|---|---|---|---|
| **A** | Development Build | Unity Profiler | Draw calls, SetPass calls, triangles, GC allocation per frame, texture memory, which C# function costs what |
| **B** | Non-development (release) build | Xcode Instruments + Xcode debug gauges | True GPU frame time on Metal, thermal state over a long run, energy impact, real (uninstrumented) frame time |

A Development Build adds profiler instrumentation to every frame, so **Pass A's absolute millisecond numbers run high**. Use Pass A for *counts and structure* (draw calls, allocations, memory) and Pass B for *the frame-time and thermal verdict*. Do not report a Pass A millisecond figure as the pass/fail answer for the 60 FPS budget.

---

## 1. Scenarios to profile

Zone structure is fixed and already recorded in `docs/adr/0004-world1-single-continuous-scene.md` §1 and §5 — do not re-derive it.

| ID | Scenario | What it exercises | Zone data asset |
|---|---|---|---|
| **S1** | Full clear of **Zone 0 "The Arrival"** — 5 spawns, `maxConcurrentEnemies` **3**, mixed roster | Mid-load combat, first NavMesh use, first enemy HUD spawns | `RoomData_CulDeSac_WildWestCity_Zone0.asset` |
| **S2** | Full clear of **Zone 1 "Ambush Alley"** — 7 spawns, `maxConcurrentEnemies` **4**, mixed roster | **The CPU/enemy peak of the whole game.** 4 live enemies is the recorded peak-live-enemy budget (ADR-0004 §8) | `RoomData_CulDeSac_WildWestCity_Zone1.asset` |
| **S3** | Full **boss fight — Zone 2 "The Showdown Circle"**, SpinCycle only, no regular enemies | Boss intro camera + cutscene, boss attack telegraphs, largest single character | `RoomData_CulDeSac_WildWestCity_Zone2.asset` |
| **S4** | **One continuous 15-minute run**, app never backgrounded: Zone 0 → Zone 1 → Zone 2 → death or win, then keep playing/idling until 15 minutes have elapsed | Thermal throttling. This is the real acceptance criterion per TDD §3.1 | — |

**S2 is the scenario that matters most for CPU and draw calls.** TDD §3.7 says "one full room clear at `maxConcurrentEnemies`" — for this scene that is specifically Zone 1, because it is the only zone with a cap of 4. If you only have time for one combat capture, capture S2.

S1 and S2 both happen inside one playthrough, so a single run can produce both. S4 is a separate, longer run with a different tool (Pass B).

**Hold the phone in landscape the whole time** (TDD §3.1 — landscape only) and note screen brightness, because brightness affects thermal results.

---

## 1.1 World 2 scenarios (`Backyard_Dojo`) — read this whole subsection before building

Zone structure is fixed by `docs/adr/0005-world2-single-continuous-scene.md` §1 and the dimensions by `docs/adr/0006-world2-zone-scale-and-arena-metric.md` §1/§3. Rosters below are read from the shipped `RoomDataSO` assets, **not** from either ADR — ADR-0006 §3.4's indicative roster is a worked example that predates the built data and does not match it (it lists 7 spawns including two Leaf Pile Lurkers; the Lurker was cut from World 2 on 2026-09-02, `docs/SPRINT.md` §Sprint 1 scope). Trust the assets.

| ID | Scenario | What it exercises | Zone data asset |
|---|---|---|---|
| **S5** | Full clear of **Zone 0 "The Back Gate / Dojo Courtyard"** — 5 spawns, all Gnome Grunt, `maxConcurrentEnemies` **4** | First combat, first NavMesh use after the runtime bake, whole-yard residency at its cheapest character load | `RoomData_Backyard_Dojo_Zone0.asset` |
| **S6** | Full clear of **Zone 1 "Garden Gauntlet"** — 6 spawns, `maxConcurrentEnemies` **4**, mixed roster: 4 Gnome Grunt + 1 Skeptic Grunt + 1 **Crane Duelist** | **The CPU/character peak of World 2.** Three distinct enemy types, four live at once, in the yard's most heavily dressed zone (shed, koi pond, engawa) | `RoomData_Backyard_Dojo_Zone1.asset` |
| **S7** | **Boss fight — Zone 2 "The Garden End — Blossom Court"** vs the **Grasscutter**. `spawnPoints` is empty, `maxConcurrentEnemies` **1**, `bossOwnedWin: 1` — boss only, no regular enemies | Boss intro cinematic (authored vantage camera, ADR-0008), the ground-plane Spin-Dash lane telegraph (ADR-0007/B118), Cut-Grass Trail hazard pool, largest single character in the scene | `RoomData_Backyard_Dojo_Zone2.asset` |
| **S8** | **One continuous 15-minute World 2 run**, app never backgrounded: Zone 0 → Zone 1 → Zone 2 → death or win, then keep playing/idling to 15 minutes | Thermal throttling in World 2. Same acceptance criterion as S4, different scene | — |
| **S9** | **Both worlds in one continuous session** — World 1 start to finish, then World 2 start to finish, without relaunching the app | Cross-scene residency: whether unloading `CulDeSac_WildWestCity` actually returns its memory before `Backyard_Dojo` loads, and whether a second scene load's runtime NavMesh bake behaves like the first | — **currently unrunnable, see below** |

**S6 is the scenario that matters most for CPU and draw calls in World 2**, and it is the direct analogue of World 1's S2. Note that **World 2's zone 0 also caps at 4** (unlike World 1's zone 0, which caps at 3), so S5 is not a "warm-up" scenario the way S1 is — it hits the same peak-live-enemy budget. S6 still wins on cost because it adds two enemy types and the yard's densest dressing. If you only have time for one World 2 combat capture, capture S6.

S5, S6 and S7 all happen inside one playthrough, so a single run can produce all three. S8 is a separate, longer run with a different tool (Pass B).

### S9 is blocked: there is no in-game route from World 1 to World 2 in a device build

This was found while writing this section, on 2026-09-08, and it is **not** currently in `docs/BACKLOG.md` or `docs/SPRINT.md`. It is a functional defect, not a performance one, but it directly gates this checklist so it is recorded here.

`GameManager.ZoneStartScene[1]` was corrected to `"Backyard_Dojo"` by ADR-0005 §7 (`Core/GameManager.cs:38`), and `docs/ARCHITECTURE.md` §5 divergence 11, `docs/BACKLOG.md` line 167 and `docs/TECHNICAL_DESIGN.md` §Divergences all record zone 1 as "reachable now" on the strength of that. **Nothing reads it.** The two paths that could:

| Path | What it actually does |
|---|---|
| Beat the World 1 boss → run-end → **Continue** | `MetaScreen.OnContinue()` (`UI/MetaScreen.cs:78-87`) calls `GameManager.Restart()`, which loads `ZoneStartScene[0]` — deliberately, per its own comment. It never advances a zone |
| Beat the World 1 boss → **World Map** → zone-1 node | `WorldMapScreen.OnTownSquareSelected()` (`UI/WorldMapScreen.cs:117`) is hardcoded to `OnZoneSelected("TownSquare_Room1")` — a scene that has never existed and was removed from `EditorBuildSettings.asset` on 2026-08-27. The adjacent comment at `:114-115` explicitly says "Do not hardcode a scene name here" |

The node *does* unlock — `GameManager` sets `highestZoneReached = 1` on a zone-0 win (`:494-502`) and `WorldMapScreen:101` enables the button at `>= 1` — so on device the button becomes live and then throws `Scene 'TownSquare_Room1' couldn't be loaded` on tap, stranding the player. A one-line change to `ZoneStartScene[1]` would fix it, but **that is a code change and is out of scope for this document.** Route it to `unity-gameplay-engineer`.

**Consequences for this checklist:**

- **S9 cannot be run** until that is fixed. Leave §7 section **N** blank and record "blocked — no in-game route" rather than substituting a relaunch, which would measure a different thing (a relaunch resets the whole process; the question S9 asks is specifically whether a *scene* unload returns its memory).
- **S5–S8 need `Backyard_Dojo` to launch directly.** The practical way to do that without touching code is to make it the first enabled entry in the build's scene list — it is currently index **2** of three (`ProjectSettings/EditorBuildSettings.asset:15`, enabled). Reordering it to index 0 changes what the app boots into, so **note it in the session record and put it back afterwards**; a release build must launch into `CulDeSac_WildWestCity`. Do not delete any entry.
- Because the app then boots straight into World 2, every S5–S8 capture is a **cold-start-into-World-2** measurement. That is the right thing to measure for S5–S8 and it is *not* the same as arriving in World 2 after a World 1 run — which is the gap S9 exists to close, and which stays unmeasured.

---

## 2. Step 0 — Prepare the build (5 minutes, in Unity)

### 2.1 Development Build and Autoconnect Profiler are already ON — but the change is not committed

As of 2026-08-27, `Assets/Settings/Build Profiles/iOS.asset` records `m_Development: 1` and `m_ConnectProfiler: 1` **in the working copy only**. The committed version of that file still has both at `0`. So the two settings Pass A needs are already correct *on this machine right now*, but they are an uncommitted Editor change and will vanish on a `git checkout` of that file, or for anyone who pulls the repo.

Practical consequence: **always verify the checkboxes in the Editor rather than assuming**, and if you want the profiling-enabled state to survive, it needs to be committed deliberately (owner approval required per the project's git rule) — bearing in mind that a Development Build should *not* be the state a release build ships from.

Confirm visually before you build:

1. In Unity: **File → Build Profiles**.
2. In the left-hand list, click the **iOS** profile (the one under "Platforms", with the scene list showing `CulDeSac_WildWestCity`).
3. Scroll the right-hand panel to the **Build Settings** section (below "Scene List" and "Player Settings Overrides").
4. Confirm these checkboxes:
   - ☑ **Development Build** — must be checked for Pass A.
   - ☑ **Autoconnect Profiler** — must be checked for Pass A.
   - ☐ **Deep Profiling Support** — leave **unchecked**. It makes the game far slower and would ruin the frame-time numbers. Only turn it on if §5.4 tells you to.
   - ☐ **Script Debugging** — leave unchecked.

> If your Unity version shows **File → Build Settings** instead of **Build Profiles**, the same four checkboxes are in that window at the bottom. Either window is fine.

### 2.2 Note what is in the scene list

The iOS profile uses the global scene list (`m_OverrideGlobalSceneList: 0`), which as of 2026-09-08 contains **three** enabled scenes (`ProjectSettings/EditorBuildSettings.asset:8-16`):

| Index | Scene | Enabled | Note |
|---|---|---|---|
| 0 | `Assets/_Project/Scenes/CulDeSac_WildWestCity.unity` | ☑ | World 1. Launches by default |
| 1 | `Assets/_Project/Scenes/WeaponGripTest.unity` | ☑ | dev-only validation scene |
| 2 | `Assets/_Project/Scenes/Backyard_Dojo.unity` | ☑ | World 2 |

`WeaponGripTest` is a validation scene, not shipped content. It does not affect frame time (it is never loaded), but **it and its referenced assets are included in the build**, so it inflates any download-size measurement. When you measure download size in §5.3, record whether it was still in the list. Do not remove it as part of this profiling pass — that is a separate decision.

**`Backyard_Dojo` is at index 2, and nothing in the game loads it (§1.1).** For a World 2 session you will need to drag it to the top of this list so the app boots into it. That is a temporary profiling change: record it in the session record, and drag `CulDeSac_WildWestCity` back to index 0 when you are done. Note also that World 2's presence in the list is new since the 2026-08-27 download-size context — its assets are in the build now and were not then, so **the download-size number is expected to have grown independently of anything else**.

### 2.3 Raise the Profiler frame buffer

The Unity Profiler only keeps a limited number of recent frames, and the default (300) is about 5 seconds at 60 FPS. Raise it so a whole zone clear fits:

1. **Unity → Settings…** (or **Edit → Preferences** on Windows).
2. Left sidebar: **Analysis → Profiler**.
3. Set **Frame Count** to **2000** (the maximum). That is roughly 33 seconds at 60 FPS.

**This is why S4 (15 minutes) cannot be one continuous Unity Profiler recording.** 15 minutes is ~54,000 frames and will not fit. S4 is measured in Pass B with Xcode, plus two short Unity Profiler captures (minute 1 and minute 12) if you want the allocation/draw-call detail at both ends. See §6.

### 2.4 Build and install

Build to Xcode and run on the device exactly as you did for the first successful device build. Nothing about the iOS/IL2CPP/ARM64/Metal configuration needs to change — that was validated in the release-engineering pass on 2026-08-27.

### 2.5 Connect the Profiler

1. Keep the iPhone connected by USB, **unlocked**, with the app in the **foreground**. A locked screen or a backgrounded app stops the data.
2. In Unity: **Window → Analysis → Profiler**.
3. At the top of the Profiler window, open the **attach dropdown** (it says "Playmode" or "Editor" by default) and choose the entry that looks like **`iPhone Player (<your device name>)`**.
4. Make sure the red **Record** button (top-left of the Profiler) is on.

> If the device never appears in that dropdown: put the Mac and the iPhone on the **same Wi-Fi network** (Unity's player connection uses network discovery, not just the cable), then relaunch the app on the device. Waiting 10–20 seconds after app launch is normal.

---

## 3. Step 1 — Choose which Profiler modules to show

At the top-left of the Profiler window there is a **Profiler Modules** dropdown. Enable exactly these and turn the rest off — fewer modules means less overhead and a readable window:

- ☑ **CPU Usage**
- ☑ **Rendering**
- ☑ **Memory**
- ☑ **Highlights** (optional but beginner-friendly — it draws your frame time against a target line)
- ☐ **GPU Usage** — enable it, but **expect it to be blank or unreliable on iOS/Metal**. Unity's GPU profiler does not report properly on most mobile GPUs. This is not a bug in your build; it is the specific reason Pass B exists.

Click a module's name in the left column to make its detail pane appear at the bottom of the window.

---

## 4. Step 2 — Pass A: the Unity Profiler measurements

Play the scenario, then **stop recording** (click the red Record button off) before you start reading numbers. Trying to read a live-scrolling graph is the most common beginner mistake.

To read a specific moment: click on the frame-time graph at the point you care about — a vertical white line marks the selected frame, and the bottom pane then describes **that one frame**.

For each measurement below, take the value at the **worst frame in the scenario** (the tallest spike), not the average. Budgets are ceilings.

### 4.1 Draw calls, SetPass calls, batches, triangles → §3.2 and ADR-0004 §8

1. Click the **Rendering** module name.
2. The bottom pane lists counters for the selected frame.

| Read this counter | Budget | Source | Pass if |
|---|---|---|---|
| **Draw Calls Count** | **< 100** | TDD §3.2, restated ADR-0004 §8 | < 100 at the worst frame of S2 |
| **SetPass Calls Count** | *no numeric budget recorded* | TDD §3.7 says record it | Record it. It is the material/shader-switch count — if it is close to Draw Calls, batching is not helping |
| **Batches Count** | *no numeric budget recorded* | — | Record it. Compare to Draw Calls to see how much the SRP Batcher is actually merging |
| **Total Triangles** | **< 300k** | TDD §3.2 | < 300k at the worst frame of S2 |
| **Total Vertices** | *no numeric budget recorded* | — | Record it |

**Where to expect the peak:** S2 (Zone 1, 4 live enemies) for the combat peak, and S3 (boss) for the largest single character. Take both.

**Interpretation note, already recorded in TDD §3.6:** this project's `Mobile_RPAsset` has `m_UseSRPBatcher: 1` and `m_SupportsDynamicBatching: 0`. The SRP Batcher makes each draw call *cheaper on the CPU* but does **not reduce the draw-call count**. So if Draw Calls is over 100, the fix is fewer renderers (or static batching), not "turn on batching" — it is already on.

#### World 2 — the flagged headline draw-call risk: 47 identical stockade wall modules

**Take the Draw Calls Breakdown sub-panel for S5, S6 and S7, not just the total.** The Rendering module's breakdown splits the count into `SRP Batcher / BRG / Standard Instanced / Standard`, and for World 2 that split is the whole question.

`Backyard_Dojo.unity` contains **47 `BD01_WallModule` prefab instances** (counted in the scene file: 45 named `BD01_WallModule` plus `(1)` and `(2)`). They are the bamboo stockade, they are the same prefab drawn from the same shared atlas material, and they are present in every one of S5–S8 regardless of which zone you are standing in, because the yard is one continuous scene (ADR-0005 §1).

Why this is the number to watch:

- **Against a whole-scene budget of < 100 draw calls (TDD §3.2, restated ADR-0005 §3), 47 identical wall modules is ~47% of the budget spent on one prop** — before a single character, pickup, prop or HUD element is drawn.
- **ADR-0006 §5.1 predicted exactly this and budgeted a fix that was not built.** Its table reads *"BD-01 module count = worst-case draw calls with no batching: 47 naive → 35 with §5.2"*, where §5.2 is a long `BD-01-Long` wall variant that refunds the module count. `docs/BACKLOG.md` B116's own completion note lists **"BD-01-Long as finished art"** under *Not built* — the walls are tiled with standard X-scaled BD-01 placeholders instead. So the built scene is at the 47 the ADR called the worst case, not the 35 it budgeted.
- **This is simultaneously the best instancing candidate in the project.** ADR-0005 §3 granted World 2's single-scene architecture partly on the argument that the dojo kit is *"many instances of few materials, which is the case GPU instancing and the SRP Batcher exist to serve"*, and made *"verify a non-zero SRP Batcher or Instanced draw-call count on device"* an explicit condition. World 1 measured `SRP Batcher: 0, Standard: 204` (§7, 2026-08-27). **If World 2's breakdown also reads `SRP Batcher: 0, Standard Instanced: 0`, then the central performance premise of ADR-0005 §3 is false and the ADR's own condition has failed** — that is a much more important finding than the total, and it is only visible in the breakdown.

Record the breakdown verbatim for each of S5/S6/S7, and record the number separately with the player standing where the fewest wall modules are on screen versus the most — frustum culling should move this number a lot, and if it does not, culling is the finding.

**Also record `Batches Count` against `Draw Calls Count` for World 2 specifically.** ADR-0006 §5.1 budgets **≤ 20 distinct ENV materials** for the whole yard and estimates the built layout at 13. That budget exists to make batching possible; if SetPass Calls comes back near the draw-call count anyway, the material count is not the thing blocking it.

### 4.2 Enemy HUD draw calls → §3.3

Budget (TDD §3.3): **≤ 2 draw calls per enemy, ≤ 20 total.**

This one needs a small bit of arithmetic rather than a single counter:

1. Capture the Rendering module's **Draw Calls Count** at a moment in S2 when **4 enemies are alive and all their health bars are visible**.
2. Capture **Draw Calls Count** at a moment in the same run with **0 enemies alive** (right after the zone clears, before you move much — keep the camera pointed at roughly the same view).
3. The difference, divided by 4, is roughly the per-enemy HUD + character cost. Write down all three numbers, not just the result.

Pass if the total attributable to enemy HUD stays ≤ 20 draw calls. `Enemy/EnemyHealthBar.cs:181,196` creates two `new Material` instances per enemy at runtime, which is exactly the cost this budget was written to bound.

**World 2 (S6):** do the same subtraction in Zone 1 with 4 enemies alive. Zone 1's roster is **4 Gnome Grunt + 1 Skeptic Grunt + 1 Crane Duelist** (`RoomData_Backyard_Dojo_Zone1.asset`), and because `RoomManager` spawns a contiguous window of the array in order (ADR-0006 §2.1), the live set that includes the Crane Duelist is the window `{1,2,3,4}` — one Gnome, the Skeptic, one Gnome, the Crane. Take the 4-alive sample from that window if you can, because it is the most *distinct* four characters the zone can put on screen at once and therefore the worst case for per-enemy material instancing. Note in the record which four were alive; that is the "scenario" half of Rule 1 for this measurement.

### 4.3 CPU main thread and render thread → §3.1

1. Click the **CPU Usage** module name.
2. In the bottom pane, switch the view dropdown (bottom-left, says "Timeline") to **Timeline** for the thread breakdown.
3. The rows are labelled **Main Thread** and **Render Thread**. Hover a block to see its duration in ms.

| Read this | Reference | Source |
|---|---|---|
| **Main Thread** total ms, worst frame | 60 FPS = **16.6 ms total budget per frame** | TDD §3.1 |
| **Render Thread** total ms, worst frame | Same 16.6 ms wall clock | TDD §3.1 |

**Remember Rule 3:** these numbers are inflated by the Development Build. Treat them as *"which thread is the bottleneck and which function is expensive"*, and let Pass B decide whether 60 FPS is actually met. If Main Thread is the tall one, the problem is C#/gameplay/animation. If Render Thread is the tall one, it is draw submission — cross-check §4.1.

To find *what* is expensive: switch the same dropdown to **Hierarchy**, click the **Time ms** column header to sort, and read the top few rows for the worst frame.

### 4.4 GC allocation per frame → §3.2

Budget (TDD §3.2): **zero managed allocation per frame in steady state.** TDD §3.2 records that this is currently honoured — this step verifies it still is, on device, under real combat.

1. **CPU Usage** module → bottom pane view dropdown → **Hierarchy**.
2. Click the **GC Alloc** column header to sort by it, descending.
3. Do this for the worst frame during S2 combat, and again for a frame while just walking with nothing happening.

| Read this | Budget | Pass if |
|---|---|---|
| **GC Alloc**, top row, steady-state walking frame | zero per frame (TDD §3.2) | 0 B. Any recurring non-zero row is a regression — write down the function name in the top row |
| **GC Alloc**, worst combat frame in S2 | zero per frame steady state | Spawn/death frames legitimately allocate; a *sustained* per-frame allocation while nothing spawns does not |
| **GC.Collect / GC.Alloc** spikes in the frame-time graph | — | Note whether frame-time spikes line up with GC rows |

### 4.5 Texture memory → §3.3 (the flagged headline risk)

Budget (TDD §3.3): **< 150 MB per room, steady state.** TDD §3.4 flags this as the dominant unverified risk and states plainly that the 100–150 MB estimate is derived from file inspection and **has never been measured on device**. ADR-0004 §8 goes further: the single-continuous-scene pivot makes all 10 buildings plus 34 props resident for the whole run, and is *"likely to breach the per-room texture budget on its own."*

**This measurement is the single most valuable number in this whole checklist.** It either confirms or kills the project's largest recorded performance risk.

1. Click the **Memory** module name.
2. In the bottom pane, the simple view shows category totals. Note **Graphics & Graphics Driver** — this is the closest single number to "GPU-side memory including textures".
3. For the detail: set the dropdown to **Detailed**, then click **Take Sample: iPhone Player**. The app freezes for a moment while it captures.
4. In the resulting tree, expand **Assets → Texture2D**. Sort by size. Read the **total** for Texture2D.

| Read this | Budget | Source | Pass if |
|---|---|---|---|
| **Texture2D total**, taken while standing in Zone 1 mid-combat | **< 150 MB** | TDD §3.3 | < 150 MB |
| **Graphics & Graphics Driver** total | — | TDD §3.7 says record texture memory | Record as a cross-check |
| **Top 10 individual textures by size** | — | supports B1 | Write these down — they are the exact work list for BACKLOG **B1** (texture import policy) |

> **Caveat to record with the result:** the standalone **Memory Profiler package** (`com.unity.memoryprofiler`) is **not installed** in this project, so the built-in Memory module's detailed sample is the coarsest of the available tools. It is good enough to answer "are we over 150 MB", which is the question that matters. Installing the Memory Profiler package would give a much better breakdown but is a new package dependency and needs owner approval per the studio rules — do not install it as part of this pass.

Take this sample **once per zone** (Zone 0, Zone 1, Zone 2) if you can. Because this is one continuous scene, the number should barely change between zones — if it does change a lot, that is itself an interesting finding worth writing down.

#### World 2 — take the same samples, and expect the boss to be resident the whole time

Same procedure, same < 150 MB budget (TDD §3.3, restated per-scene by ADR-0005 §3), sampled in each of Zone 0, Zone 1 and Zone 2.

Two things to know before you read the number:

- **The Grasscutter is loaded from the moment the scene loads, in all three zones.** `pfb_enemy_grasscutter` is a pre-placed scene instance (ADR-0006 §1.4 "Boss dormancy, pre-placed inactive"; the prefab is referenced in `Backyard_Dojo.unity`), and `ZoneDirector` only forces it *inactive* — an inactive `GameObject` still has its meshes and textures loaded. So the boss's texture cost is in the Zone 0 sample too, and the three per-zone numbers should be close. TDD §3.4 records `Grasscutter_BaseColor.png` at **30.9 MB** on disk, one of the three largest source textures in the project, so this is not a rounding error. If Zone 0's number is much *lower* than Zone 2's, something is streaming that the architecture does not think is streaming — write it down.
- **Only one boss is resident, not two.** World 1 and World 2 are separate top-level scenes and are never loaded together (§1.1), so `pfb_enemy_spincycle` does not appear in `Backyard_Dojo.unity` — confirmed, zero references. Do not budget World 2 as "two bosses' worth of assets"; it is one, and `SpinCycle_BaseColor.png` (29.3 MB, TDD §3.4) is not part of World 2's residency. The cross-scene question — whether World 1's textures are actually *released* when `Backyard_Dojo` loads after it — is what S9 was written to answer, and S9 is blocked (§1.1).

ADR-0006 §5.1 records texture memory as **unchanged** by World 2's layout work on the grounds that it adds no new texture, and World 1 measured 41.2 MB against the 150 MB budget. That is real headroom and the expectation is a pass — which is exactly why a **fail** here would be worth a great deal. It would mean the headroom ADR-0005 §3 spent World 2's whole-scene budget against was not there.

### 4.6 Scene-start hitch → ADR-0004 §8

Budget (ADR-0004 §8): **≤ 500 ms scene-start hitch, including the runtime NavMesh bake.**

1. Start recording in the Profiler **before** the scene loads (tap into the level with the Profiler already recording).
2. Find the single enormous frame at the start of the graph. Click it.
3. Read its total frame time in ms.

Pass if ≤ 500 ms. Look in the **Hierarchy** view of that frame for NavMesh-related rows to see how much of it is the bake.

#### World 2 — this is where the hitch is most likely to fail, and the reason is already tracked

Same budget, same procedure, run on the `Backyard_Dojo` cold start. Three facts make World 2's bake structurally more expensive than World 1's, and all three are inspected-certain from the code and the backlog rather than estimated:

1. **The Editor-baked NavMesh is thrown away at runtime.** `Systems/LevelBuilder.cs:73-96` defers one frame, then calls `NavMesh.RemoveAllNavMeshData()` and re-bakes with a runtime `NavMeshSurface` using `collectObjects = All` and `useGeometry = PhysicsColliders`. So `docs/BACKLOG.md` B122's Editor bake (fixed 2026-09-01, 1 139 verts / 485 tris / 1 216.1 m²) is **not what runs on device** — it is discarded on the second frame of every run. The B122 figures are a good *prediction* of the runtime bake's size, not a measurement of it.
2. **The bake surface has no bounds and is far larger than the playable area — `docs/BACKLOG.md` B128.** `Ground` is a single 66.6 × 66.6 m plane and the court walls are only 2.4 m tall obstacles standing on it, so the bake produces 1 216 m² of walkable surface against roughly 318 m² of playable Blossom Court, including a continuous walkable ring outside the arena wall. B128 files this as a pathing and minor-memory issue at P3. **It is also a scene-start-hitch input, and that is not currently recorded anywhere** — bake cost scales with the area being voxelised, and World 2 is voxelising an area several times its playable footprint. If the ≤ 500 ms hitch budget fails in World 2 and passes in World 1, B128 is the first place to look, and its priority is probably wrong.
3. **ADR-0005 claimed this as a win.** §Consequences lists *"One runtime NavMesh bake per run instead of three-to-five"* as a positive of the single-scene decision. That is true of the *count*; it says nothing about the cost of the one bake that remains, which has never been measured in either world.

**Record the NavMesh rows separately from the total frame time** for World 2, so the bake's share is attributable. In a Development Build the bake also logs its own result — `[LevelBuilder] Runtime NavMesh baked: N verts, M tris` (`LevelBuilder.cs:108`, `UNITY_EDITOR || DEVELOPMENT_BUILD` only). **Copy that line into the session record.** Comparing its vert/tri count against B122's Editor-bake figures is the cheapest available check on whether the runtime bake is covering the same geometry, and it costs you nothing but reading the device console.

**One measurement-interpretation caveat, flagged not resolved.** B127 records that the scene's 8 `NavMeshModifier` components are inert *"because the scene uses the legacy bake"* and concludes six court props carve permanent navmesh holes. That analysis is about the **Editor** bake. The runtime bake described above is a `NavMeshSurface`, which is the workflow `NavMeshModifier` belongs to — so the two bakes may not agree about those six props, and the runtime one is the one that ships. This does not change any number you are asked to record and it is **not** a performance finding; it is a note that B127's conclusion may be scoped to a bake that never runs on device. Route it to `technical-director` alongside B127; do not act on it here.

### 4.7 Telegraph indicators → §3.3

Budget (TDD §3.3, from ADR-0003): **≤ 12 concurrent, pooled. Per-wind-up instantiation is forbidden.**

This is checked structurally, not by a counter:

1. During S2 (4 enemies) and S3 (boss), watch the **Memory** module's graph and the **GC Alloc** column for allocation spikes that coincide with attack wind-ups.
2. Pass if telegraph wind-ups produce **no repeated instantiation allocations** — the pool should be created once.

**Configuration note to record, not a budget change:** `AttackTelegraphService._poolSize` currently defaults to **8**, below the ≤ 12 budget ceiling. That is legal (the budget is a maximum, not a requirement) and the service recycles the oldest indicator when the pool is exhausted rather than allocating. But if you see telegraph indicators visibly *disappearing early* during a busy S2 fight, the pool size is why — note it and it can be raised toward 12 without breaking the budget.

#### World 2 — two telegraph geometries now share the ADR-0003 channel, plus the Cut-Grass Trail

Run the same check during **S6** (4 enemies, and the Crane Duelist is a telegraph-driven duellist with a counter window, so it is telegraph-heavy by design) and **S7** (boss).

Three World-2-specific things to watch for, all against the same "pooled, no per-wind-up instantiation" criterion:

| Watch for | Source | Note |
|---|---|---|
| **Ground-plane Spin-Dash lane** allocations during the boss's 0.9 s rev | `docs/adr/0007-ground-plane-lane-telegraph.md`; `docs/BACKLOG.md` B118 | ADR-0007 added a *second* telegraph geometry (a world-space ground lane) to ADR-0003's channel, alongside the existing overhead billboard. B118 reports it as zero-allocation **by construction** — `RaycastNonAlloc` into a preallocated buffer, no per-dash `new`/`Instantiate` — and explicitly labels that a code-review-level claim, not a measured one, because the live scene's Profiler reading was too noisy to attribute. **This capture is the measurement B118 says it does not have.** |
| **Cut-Grass Trail hazard** allocations | ADR-0005 §4; ADR-0006 §1.2 | Both ADRs require these pooled with zero per-frame allocation. Never profiled |
| Whether the boss's overhead billboard and the ground lane are both drawn | ADR-0007 | Two geometries, one channel — count them against the ≤ 12 concurrent ceiling together, not separately |

**Do not treat a S7 pass here as discharging ADR-0006 §Validation 10.** That validation is about whether the lane telegraph is *readable* from the far rim of a 20 m arena by a human — the condition ADR-0006 §1.3 granted the arena size on, still open in `docs/SPRINT.md` §Sprint 1. This checklist measures whether the telegraph *allocates*. Those are different questions and passing one says nothing about the other. Note both verdicts separately in the record so nobody later reads a green allocation row as the arena being accepted.

### 4.8 Physics and animation (no numeric budget — record only)

TDD §3 records no explicit physics budget, so do not invent a pass/fail here. Simply note, in the CPU **Hierarchy** view of the worst S2 frame, the ms cost of any row containing `Physics`, `Animator`, or `NavMesh`. These are the three most likely CPU offenders in this scene and knowing their share now makes any future regression obvious.

---

## 5. Step 3 — Pass B: Xcode Instruments (the frame-time and thermal verdict)

**Rebuild first, with Development Build OFF.** In **File → Build Profiles → iOS → Build Settings**, uncheck **Development Build** and **Autoconnect Profiler**, then rebuild to Xcode. This is the build whose numbers count for the 60 FPS and thermal budgets.

> Remember to re-check both boxes afterwards if you want to run Pass A again. Unchecking them returns `iOS.asset` to its committed state (§2.1), which is also the correct state for any real release build — but it does silently disable the Unity Profiler next time.

The Unity-generated Xcode project's **Profile** action already builds in a release configuration (`m_iOSXcodeBuildConfig: 1` in the build profile), so no scheme editing is needed.

### 5.1 GPU frame time and Metal work

1. In Xcode, with `Unity-iPhone.xcodeproj` open and the device selected as the run destination: **Product → Profile** (⌘I).
2. Instruments opens with a template chooser. Choose **Game Performance**. (If your Xcode does not offer it, choose **Metal System Trace**.)
3. Click the red **record** button. Play scenario **S2**, then **S3**. For a World 2 build, play **S6**, then **S7**.
4. Stop recording.

> **The 60 FPS target is new since this section was written.** Under the old 30 FPS cap a GPU frame time of 20 ms still hit the cap; it no longer does. Everything below is now judged against 16.6 ms for real, in both worlds. This is the single biggest reason the 2026-08-27 numbers cannot be carried forward.

| Read this track | Budget | Source | Pass if |
|---|---|---|---|
| **GPU frame time / GPU encoder duration**, worst frame | must fit inside 16.6 ms alongside CPU | TDD §3.1 (60 FPS) | Well under 16.6 ms — if GPU time alone approaches 16.6 ms you are GPU-bound |
| **Displayed FPS / frame interval** | **stable 60 FPS** | TDD §3.1 | Flat at 60. Look for dips and count them |
| **Thermal State** | no sustained regression | TDD §3.3 | See §6 |

### 5.2 Where the GPU time goes (optional, very informative)

For a per-draw-call GPU breakdown — including exactly what the shadow pass costs:

1. Run the app from Xcode normally (**Product → Run**, ⌘R).
2. While it is running, in the debug bar at the bottom of the Xcode window click the **camera icon** (Capture GPU Frame / Metal Frame Capture).
3. Xcode captures one frame and shows every Metal render encoder and draw call with timings.

| Read this | Relevant budget | Source |
|---|---|---|
| Cost of the **shadow map render encoder** | Shadow distance target **25 m** | TDD §3.3 |
| Draw call count per encoder | Draw calls **< 100** | TDD §3.2 — a good independent cross-check on §4.1 |

**Discrepancy to record, not to fix:** TDD §3.3 sets the shadow-distance target at **25 m** and describes the current value as "from 40" against a **256×256** atlas. The live `Assets/Settings/Mobile_RPAsset.asset` actually reads `m_ShadowDistance: 50`, `m_MainLightShadowmapResolution: 1024`, `m_ShadowCascadeCount: 1`. So the *target* is still 25 m as recorded, but the gap from the current setting is larger than §3.3's prose describes (50 → 25, not 40 → 25), and the atlas is bigger than §3.3 says. Measure against the live asset values, quote 25 m as the target, and flag the §3.3 prose as needing a correction pass — **do not silently change the budget.**

### 5.3 Energy, memory ceiling, and download size

**Energy and memory, live:** run from Xcode with **Product → Run**, then open the **Debug navigator** (left sidebar, the gauge icon). It shows live **CPU**, **Memory**, **Energy Impact**, and **FPS** gauges with no Instruments session at all. This is the easiest continuous view of the whole run and is what §6 uses.

| Read this gauge | Budget | Source |
|---|---|---|
| **Memory**, peak | no explicit runtime-memory budget recorded in TDD §3 — record only | TDD §3.7 says record memory |
| **Energy Impact** | no numeric budget recorded — record the qualitative level (Low/High/Very High) | TDD §3.1 (battery/thermal intent) |

**Download size → §3.3 (< 200 MB):** this is not a device measurement. Read it from the Xcode build:

1. In Xcode, **Product → Archive**, then in the Organizer choose the archive → **Distribute App → ... → App Thinning: All compatible device variants** to generate an App Thinning Size Report; *or*
2. Simply note the size of the built `.ipa` / the `Payload` app bundle.

| Read this | Budget | Source | Pass if |
|---|---|---|---|
| Install/download size | **< 200 MB** | TDD §3.3 | < 200 MB. TDD §3.5 records that `Assets/StreamingAssets/Cutscenes/` alone holds **326 MB** of `.mp4`, of which only `spincycle_standoff.mp4` (26.8 MB) is still in scope — so **expect this budget to fail today**, and expect it to pass after that content decision |

Also note whether `WeaponGripTest` was still in the scene list (§2.2) when this size was measured.

### 5.4 If and only if a CPU number fails: deep profiling

If Pass A showed a CPU cost you cannot attribute to a function, rebuild once with **Deep Profiling Support** checked (§2.1) and repeat §4.3. It instruments every method call, so **frame times become meaningless** — use it purely to find the guilty function name, then turn it back off.

---

## 6. Step 4 — The thermal run (S4 for World 1, S8 for World 2). This is the acceptance criterion.

TDD §3.1: *"Run length 10–15 minutes, which makes sustained thermal behaviour, not peak frame time, the real acceptance criterion."* TDD §3.7 item 3: record **frame time at minute 1 versus minute 12**.

### Procedure

1. Use the **non-development build** (§5) — a Development Build's own overhead would contaminate a thermal test.
2. Let the phone sit at **room temperature, not charging, not in a case**, for a few minutes first. Charging heats the phone and will produce a false failure. Note screen brightness and set it consistently.
3. Run the app from Xcode (**Product → Run**) and open the **Debug navigator** gauges (§5.3) so FPS, CPU, and Energy are visible for the whole run. Optionally also run the **Game Performance** Instruments template for its **Thermal State** track.
4. Start a timer. Play continuously for **at least 15 minutes**, never backgrounding the app. Zone 0 → Zone 1 → Zone 2. If you win or die before 15 minutes, restart and keep playing — the requirement is 15 minutes of continuous foreground GPU/CPU load, not one completed run.
5. Write down the frame time / FPS at these marks: **minute 1, 3, 6, 9, 12, 15.**
6. Optional detail: run two short Unity Profiler captures on a Development Build — one at minute 1 and one at minute 12 — and diff the §4.1 and §4.3 numbers. Because the Profiler frame buffer maxes out around 33 seconds (§2.3), two short captures is the correct technique; one long recording is not possible.

### What a FAIL looks like

| Pattern | Verdict |
|---|---|
| Frame time **gradually creeps upward** — e.g. 16 ms at minute 1, 18 ms at minute 6, 21 ms at minute 12, and it never recovers while you keep playing | **FAIL. This is the thermal failure the budget exists to catch.** The device is throttling. Frame time gets worse the longer you play and only recovers after you stop and the phone cools |
| Frame time flat at minute 1 and minute 12, within noise of each other | **PASS** on the thermal budget (TDD §3.3 "no sustained frame-time regression across a full 15-min run") |
| One **sudden** drop that then recovers | **Not thermal.** That is a hitch — a GC spike, an asset load, a scene event. Chase it with §4.4 / §4.6, not here |
| A single dip every time a specific thing happens (boss intro, forge, zone transition) | **Not thermal.** Event-driven hitch |
| Frame time already bad at minute 1 and equally bad at minute 12 | **Not thermal** — a plain frame-time budget failure. §4.1/§4.3 own that |

The distinguishing feature is **gradual and non-recovering while under load**. Write the six timestamped numbers down; the shape of that list *is* the verdict.

**If it fails:** TDD §3.4 names the most likely cause and it is already on the backlog — sampling 2048² textures for props that occupy 40 screen pixels destroys cache coherency and burns memory bandwidth continuously. Memory bandwidth is the primary driver of sustained throttling. Go to §8 before changing anything.

### S8 — the same run in World 2

Identical procedure, on a build whose scene list boots into `Backyard_Dojo` (§2.2). Zone 0 → Zone 1 → Zone 2, 15 minutes continuous foreground, same six timestamped marks, same fail patterns.

**Run S8 as a separate session from S4 and record it separately.** Two reasons, both about not contaminating the one measurement that decides the release:

- **Thermal state carries across scenes but not across a cool-down.** If you play World 1 for 15 minutes and then immediately start World 2, minute 1 of S8 begins on an already-hot phone and its whole curve is displaced. That would not measure World 2; it would measure a warm start. Let the device return to room temperature between S4 and S8, and note the gap.
- **A relaunch between them is not S9.** Running S4, quitting, relaunching into World 2 and running S8 is two clean single-world runs, which is what you want. It is *not* the both-worlds-in-one-session test — that is S9, and S9 is blocked (§1.1). Do not record a S4-then-relaunch-then-S8 pair as if it answered S9.

**What is different about World 2's thermal risk, and what is the same.** The same texture-bandwidth mechanism from TDD §3.4 applies unchanged. What is genuinely different is that ADR-0005 §3 accepted World 2's single-scene architecture on a *hypothesis* — that the dojo's shared-atlas kit would be cheaper on draw calls and triangles than World 1's ten unique buildings — and ADR-0005 §Negative states it plainly: *"'cheaper' is a hypothesis until profiled."* S8's minute-1-vs-minute-12 pair is the first evidence either way, and if World 2 throttles where World 1 does not, the §4.1 wall-module breakdown is where the explanation will be.

---

## 7. Results template — record here

Copy this block for each profiling session and fill it in. Append new sessions below; do not overwrite old ones — the point is to be able to compare a later run against an earlier one.

The **Session record** block below is used for either world. **Sections A–G are World 1 (`CulDeSac_WildWestCity`, S1–S4); sections H–O are World 2 (`Backyard_Dojo`, S5–S9) and live in §7.1.** Fill in the set that matches the world you actually played, and leave the other set alone — a half-filled table from the wrong world is worse than an empty one.

### Session record

```
Session date:            ____________________
Build:                   git commit ____________  branch ____________
Device model:            ____________________  (iOS version: __________)
Device class vs budget:  ☐ 3–4-year-old (target class)  ☐ NEWER than target — results are a LOWER BOUND only (Rule 2)
Build type:              ☐ Development Build (Pass A)   ☐ Release, non-development (Pass B)
Orientation:             landscape          Screen brightness: ______   Charging: ☐ no ☐ yes (invalidates thermal)
World / scene:           ☐ World 1 — CulDeSac_WildWestCity (S1–S4)
                         ☐ World 2 — Backyard_Dojo (S5–S8)
                         ☐ Both in one session (S9 — blocked, see §1.1; do not tick unless the route was fixed)
Target frame rate:       ☐ 60 (GameManager.cs:105, current)   ☐ 30 (pre-2026-09-08 cap — say so, numbers are not comparable)
Scene list at build:     index 0 = ______________  · WeaponGripTest still enabled? ☐ yes ☐ no
                         (World 2 sessions need Backyard_Dojo dragged to index 0 — §2.2. Put it back afterwards)
Tool(s):                 ☐ Unity Profiler  ☐ Xcode Instruments (template: ____________)  ☐ Xcode debug gauges
Notes / anything unusual:
```

### Session 2026-08-27 — first on-device Pass A capture

```
Session date:            2026-08-27
Build:                   commit 12a40904, branch feature/sprint-0-foundation-rebuild
Device model:            iPhone 15 Pro Max  (iOS version: __________)
Device class vs budget:  ☐ 3–4-year-old (target class)  ☑ NEWER than target (Rule 2 — "iPhone 14 or later, roughly")
Build type:              ☑ Development Build (Pass A). Pass B (Xcode Instruments, thermal run) not yet done.
Orientation:             landscape          Screen brightness: unknown   Charging: unknown
Scene:                   CulDeSac_WildWestCity — one continuous playthrough, Zone 0 → Zone 1 → Zone 2, through defeating SpinCycle
Tool(s):                 ☑ Unity Profiler (screenshots read by Claude, not a live MCP data pull — see note)
Notes / anything unusual: Captured via screenshots of the Profiler window rather than an automated data pull — MCP profiler
  tool calls (get_frame_timing, get_counters, memory_take_snapshot) returned idle/local-Editor data even with the device
  selected as the Profiler connection target; only the native Profiler window UI showed real device data. Frame numbers below
  come from one frame (11825/13122) within the full playthrough, exact zone/enemy-count at that instant not confirmed.
```

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Draw Calls Count | mid-run, full playthrough (zone unconfirmed) | < 100 | TDD §3.2 | **205** (204 Standard, 1 Null Geometry) | ☐ **FAIL** |
| SetPass Calls Count | same frame | — record | TDD §3.7 | **38** | recorded |
| Total Triangles | same frame | < 300k | TDD §3.2 | **356.7k** | ☐ **FAIL** (~19% over) |
| Total Vertices | same frame | — record | — | **464.6k** | recorded |
| Used Textures (this frame) | same frame | — cross-check | TDD §3.7 | **52 textures / 41.2 MB** | recorded — see Memory row below |
| Texture2D total (detailed sample) | not yet taken this session | < 150 MB | TDD §3.3 | **41.2 MB used-this-frame is a strong signal, but not the §4.5 detailed-sample procedure** | ☐ pass (provisional) |
| CPU frame time (Development Build, inflated per Rule 3) | same frame | — dev-build inflated | TDD §3.7 | **33.32 ms** (matches the 30 FPS cap below, not a budget failure by itself) | record only |

**Rule 2 applies: test device is an iPhone 15 Pro Max, newer than the 3–4-year-old target class.** Per this doc's own Rule 2, the two measured budget failures (draw calls, triangles) are real and trustworthy as-is — a failure on fast hardware is a genuine failure. Texture memory (bytes of loaded `Texture2D` data) is not meaningfully device-speed-dependent, so the 41.2 MB reading is likely a fair measurement rather than a lower bound. Any future frame-time/thermal **pass** verdict from this same device, however, must be labeled a lower bound only, not a confirmed pass, until re-run on target-class hardware or shown to fail even here.

**New finding, not anticipated by §4.1's interpretation note:** the Draw Calls Breakdown for this frame reads `SRP Batcher: 0, BRG: 0, Standard Instanced: 0, Standard: 204`. `Mobile_RPAsset` has the SRP Batcher enabled and TDD §3.6 / BACKLOG (line ~671) both assumed it "still applies" to these renderers — but in this captured frame it is contributing **zero** batched draws, not just failing to reduce the draw-call count. Before reaching for `StaticBatchingUtility.Combine` (BACKLOG's recorded lever), it is worth finding out *why* the SRP Batcher isn't engaging at all here — a shader/material incompatibility would also explain the elevated draw-call count and may be the more direct fix.

**Deliberate 30 FPS cap found, not a performance failure:** `Application.targetFrameRate = 30` is set explicitly at `GameManager.cs:102` (with `QualitySettings.vSyncCount = 0` at line 101 so it takes effect on iOS). The CPU frame in this capture used only ~4–6 ms of a 33 ms budget in an earlier, quieter sample — there is substantial headroom below even the 60 FPS (16.6 ms) budget. This directly contradicts TDD §3.1's stated "stable 60 FPS" target and needs an owner decision: raise the cap to 60 and re-test, or correct the documented target to 30. Not changed here — flagged only.

### Session 2026-09-08 — second on-device Pass A capture, first fully-automated data pull

```
Session date:            2026-09-08
Build:                   feature/mobile-release-readiness, exact commit not confirmed — owner built via
                         Unity's Build & Run before this session's d3e4195f commit; whether it includes the
                         60 FPS change / WorldMap fix / boss-intro fix at the binary level is not independently
                         verified, only inferred from when in the conversation the build was made. Flagged as
                         a discrepancy, not resolved here.
Device model:            iPhone 15 Pro Max (same device as 2026-08-27 session, per Profiler console tag)
Device class vs budget:  ☐ 3–4-year-old (target class)  ☑ NEWER than target (Rule 2 applies again)
Build type:              ☑ Development Build (Pass A) — confirmed by the fact a profiler connection was
                         possible at all. Pass B not done.
Orientation:             landscape (assumed, not explicitly confirmed this session)
Scene:                   CulDeSac_WildWestCity (World 1) — owner-confirmed "playing the first level."
                         Exact zone/enemy-count during the captured frames not confirmed — see note below.
Tool(s):                 ☑ Unity Profiler, data pulled via `UnityEditorInternal.ProfilerDriver
                         .GetFormattedCounterValue(frameIndex, category, name)` through Unity MCP's
                         `execute_code`, scanning the entire captured frame buffer (2000 frames,
                         indices 13147–15146) for the worst (highest) value per counter — not read off
                         screenshots, and not a single spot-check frame.
Notes / anything unusual: The live "current frame" MCP profiler tools (`get_frame_timing`, `get_counters`)
  reproduced the exact same idle/stale-data limitation the 2026-08-27 session found — confirmed again,
  not a fluke. But `ProfilerDriver.GetFormattedCounterValue` against a specific historical frame index
  DOES return real device data, even after the live connection had already dropped — this is a real
  workaround for that limitation, worth reusing for future sessions instead of screenshot-reading.
  The device disconnected mid-session at least once; data below comes from whatever was in the buffer
  after reconnection, spanning an uncertain mix of gameplay moments within World 1 (not a single
  clean "start of Zone 1 to end of Zone 1" bracket the way the checklist's S1/S2 ask for).
```

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Draw Calls Count (Standard), worst in buffer | World 1, zone/moment unconfirmed | < 100 | TDD §3.2 | **166** @ frame 14898 | ☐ **FAIL** |
| SRP Batcher Draw Calls Count, same frame | — | — cross-check | TDD §3.6 | **0** | Same finding as 2026-08-27 — still contributing nothing |
| Static Batched Draw Calls Count, same frame | — | — record | — | **51** | recorded |
| SetPass Calls Count, same frame | — | — record | TDD §3.7 | **53** | recorded |
| Static Batches Count, same frame | — | — record | — | **6** | recorded |
| Total Triangles, worst in buffer | World 1, zone/moment unconfirmed | < 300k | TDD §3.2 | **329.65k** @ frame 15001 (311.27k at the draw-call-worst frame above) | ☐ **FAIL** (~10% over) |
| Total Vertices, worst-triangle frame | — | — record | — | **419.60k** | recorded |
| Total Used Memory, worst-triangle frame | — | — record | TDD §3.7 | **301.9 MB** | recorded |
| Texture Memory, worst-triangle frame | — | < 150 MB | TDD §3.3 | **111.5 MB** | ☑ **pass** |
| GC Used Memory, worst-triangle frame | — | — this is cumulative heap, not per-frame GC Alloc — TDD §3.2's zero-alloc budget needs the "GC Alloc" CPU-Usage counter instead, not queried this session | — | **6.0 MB** | not a budget check, record only |

**Consistent with 2026-08-27, not an improvement or regression either way (different frame, same failure shape):** draw calls and triangles are both over budget again, and the SRP Batcher is again contributing exactly zero despite being enabled — this is now confirmed across two independent sessions, on two different days, so it is not a one-off capture artifact. Texture memory again comes in comfortably under budget.

**CPU/thread timing not captured this session** — `ProfilerDriver.GetFormattedCounterValue` does not expose Main/Render Thread ms under any counter name tried (`Main Thread`, `Render Thread`, `CPU Main Thread`, `CPU Render Thread`, `Frame Time`, all under a `CPU Usage` category — all returned empty). That data likely requires the richer `HierarchyFrameDataView`/`ProfilerFrameDataIterator` API, not the simple named-counter API used here. Left blank in §A below rather than estimated.

### A. Frame time and threads

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Frame time, worst (release build) | S2 | ≤ 16.6 ms (60 FPS) | TDD §3.1 | | ☐ pass ☐ fail |
| Frame time, worst (release build) | S3 boss | ≤ 16.6 ms (60 FPS) | TDD §3.1 | | ☐ pass ☐ fail |
| CPU Main Thread, worst | S2 | — (dev-build inflated) | TDD §3.7 | | record |
| CPU Render Thread, worst | S2 | — (dev-build inflated) | TDD §3.7 | | record |
| GPU frame time, worst | S2 | must fit in 16.6 ms | TDD §3.1 | | ☐ pass ☐ fail |
| GPU frame time, worst | S3 boss | must fit in 16.6 ms | TDD §3.1 | | ☐ pass ☐ fail |
| Top CPU function, worst frame | S2 | — | — | | record name + ms |

### B. Rendering

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Draw Calls Count, worst | S2 (4 enemies) | < 100 | TDD §3.2 | | ☐ pass ☐ fail |
| Draw Calls Count, worst | S3 boss | < 100 | TDD §3.2 | | ☐ pass ☐ fail |
| SetPass Calls Count, worst | S2 | — record | TDD §3.7 | | record |
| Batches Count, worst | S2 | — record | — | | record |
| Total Triangles, worst | S2 | < 300k | TDD §3.2 | | ☐ pass ☐ fail |
| Total Triangles, worst | S3 boss | < 300k | TDD §3.2 | | ☐ pass ☐ fail |
| Total Vertices, worst | S2 | — record | — | | record |
| Draw calls, 4 enemies alive | S2 | — | — | | (a) |
| Draw calls, 0 enemies alive | S2 | — | — | | (b) |
| Enemy HUD draw calls = (a−b) | S2 | ≤ 2/enemy, ≤ 20 total | TDD §3.3 | | ☐ pass ☐ fail |
| Shadow-pass GPU cost | S2 | target shadow distance 25 m | TDD §3.3 | | record |

### C. Memory

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Texture2D total (detailed sample) | Zone 0 | < 150 MB | TDD §3.3 | | ☐ pass ☐ fail |
| Texture2D total (detailed sample) | Zone 1 | < 150 MB | TDD §3.3 | | ☐ pass ☐ fail |
| Texture2D total (detailed sample) | Zone 2 boss | < 150 MB | TDD §3.3 | | ☐ pass ☐ fail |
| Graphics & Graphics Driver total | Zone 1 | — cross-check | TDD §3.7 | | record |
| Top 10 textures by size | any | — feeds BACKLOG B1 | TDD §3.4 | | list separately |
| Peak process memory (Xcode gauge) | S4 | — record | TDD §3.7 | | record |

### D. Allocation

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| GC Alloc / frame, walking, nothing happening | S1/S2 idle | **zero** | TDD §3.2 | | ☐ pass ☐ fail |
| GC Alloc / frame, worst combat frame | S2 | zero steady state | TDD §3.2 | | ☐ pass ☐ fail |
| Highest-allocating function (if any) | S2 | — | — | | record name + bytes |
| Telegraph wind-up allocations | S2 + S3 | pooled, no per-wind-up instantiation | TDD §3.3 / ADR-0003 | | ☐ pass ☐ fail |

### E. Loading and packaging

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Scene-start hitch (incl. NavMesh bake) | scene load | ≤ 500 ms | ADR-0004 §8 | | ☐ pass ☐ fail |
| Install / download size | build | < 200 MB | TDD §3.3 | | ☐ pass ☐ fail |
| `WeaponGripTest` still in scene list? | build | — context for size | §2.2 | ☐ yes ☐ no | note |

### F. Thermal — the acceptance criterion

| Mark | Frame time / FPS | CPU % | Thermal state | Notes |
|---|---|---|---|---|
| Minute 1 | | | | |
| Minute 3 | | | | |
| Minute 6 | | | | |
| Minute 9 | | | | |
| **Minute 12** | | | | |
| Minute 15 | | | | |

```
Thermal verdict (TDD §3.3 — no sustained frame-time regression across a full 15-min run):
  ☐ PASS — minute 12 within noise of minute 1
  ☐ FAIL — gradual, non-recovering frame-time creep (state the numbers): ____________________
  ☐ INCONCLUSIVE — run interrupted / device was charging / device is newer than target class
```

### G. Live enemy count sanity check

| Measurement | Budget | Source | Measured | Verdict |
|---|---|---|---|---|
| Peak simultaneous live enemies observed | ≤ 4 | ADR-0004 §8 (Zone 1 `maxConcurrentEnemies`) | | ☐ pass ☐ fail |

---

## 7.1 Results template — World 2 (`Backyard_Dojo`, S5–S9)

**Nothing below has ever been measured.** Sections A–G above are World 1 and carry one stale 2026-08-27 capture; sections H–N are World 2 and are empty. Same rules apply — Rule 1 (scenario + device), Rule 2 (a newer phone gives a lower bound, not a pass), Rule 3 (Pass A for counts, Pass B for the frame-time and thermal verdict).

Budget sources differ slightly from World 1's: TDD §3.1/§3.2/§3.3 still own the frame-time, draw-call, triangle, texture and download budgets, but the **whole-scene** restatement and World 2's two extra rows come from `docs/adr/0005-world2-single-continuous-scene.md` §3 and `docs/adr/0006-world2-zone-scale-and-arena-metric.md` §5.1. Each row names its own source, as always.

### H. Frame time and threads — World 2

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Frame time, worst (release build) | S5 Zone 0 | ≤ 16.6 ms (60 FPS) | TDD §3.1 | | ☐ pass ☐ fail |
| Frame time, worst (release build) | S6 Zone 1 | ≤ 16.6 ms (60 FPS) | TDD §3.1 | | ☐ pass ☐ fail |
| Frame time, worst (release build) | S7 boss | ≤ 16.6 ms (60 FPS) | TDD §3.1 | | ☐ pass ☐ fail |
| CPU Main Thread, worst | S6 | — (dev-build inflated) | TDD §3.7 | | record |
| CPU Render Thread, worst | S6 | — (dev-build inflated) | TDD §3.7 | | record |
| GPU frame time, worst | S6 | must fit in 16.6 ms | TDD §3.1 | | ☐ pass ☐ fail |
| GPU frame time, worst | S7 boss | must fit in 16.6 ms | TDD §3.1 | | ☐ pass ☐ fail |
| Top CPU function, worst frame | S6 | — | — | | record name + ms |
| Top CPU function, worst frame | S7 boss | — | — | | record name + ms |
| `Physics` / `Animator` / `NavMesh` row cost, worst frame | S6 | — record only (§4.8) | — | | record |

### I. Rendering — World 2

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Draw Calls Count, worst | S5 Zone 0 (4 enemies) | record-only, per ADR-0009 (2026-09-10) | TDD §3.2 / ADR-0009 | | record |
| Draw Calls Count, worst | S6 Zone 1 (4 enemies) | record-only, per ADR-0009 (2026-09-10) | TDD §3.2 / ADR-0009 | | record |
| Draw Calls Count, worst | S7 boss | record-only, per ADR-0009 (2026-09-10) | TDD §3.2 / ADR-0009 | | record |
| **SetPass Calls Count** | S6 | in the 40s = batcher engaged; approaching 80s = it is not | ADR-0009 §7 (supersedes the `SRP Batcher` breakdown row below, which reads 0 whether the batcher is on or off) | | ☐ pass ☐ **fail** |
| ~~**Draw Calls Breakdown** — `SRP Batcher`~~ | S6 | **withdrawn as a pass/fail check — always reads 0, uninformative** | ADR-0009 (2026-09-10 correction of ADR-0005 §3) | | do not use |
| **Draw Calls Breakdown** — `Standard Instanced` | S6 | — record | ADR-0005 §3 | | record |
| **Draw Calls Breakdown** — `Standard` | S6 | — record | ADR-0005 §3 | | record |
| Draw calls, most wall modules on screen | S5/S6 | — record (§4.1) | — | | (a) |
| Draw calls, fewest wall modules on screen | S5/S6 | — record (§4.1) | — | | (b) |
| Frustum-culling delta = (a−b) | — | — | — | | record — near-zero means culling is the finding |
| SetPass Calls Count, worst | S6 | — record | TDD §3.7 | | record |
| Batches Count, worst | S6 | — record | — | | record |
| Distinct ENV materials in scene | any | **≤ 20** | ADR-0006 §5.1 (est. 13) | | ☐ pass ☐ fail |
| Total Triangles, worst | S6 | < 300k whole scene | TDD §3.2 / ADR-0005 §3 | | ☐ pass ☐ fail |
| Total Triangles, worst | S7 boss | < 300k whole scene | TDD §3.2 / ADR-0005 §3 | | ☐ pass ☐ fail |
| Total Vertices, worst | S6 | — record | — | | record |
| Draw calls, 4 enemies alive (window `{1,2,3,4}`) | S6 | — | — | | (c) |
| Draw calls, 0 enemies alive | S6 | — | — | | (d) |
| Enemy HUD draw calls = (c−d) | S6 | ≤ 2/enemy, ≤ 20 total | TDD §3.3 | | ☐ pass ☐ fail |
| Which 4 enemies were alive for (c) | S6 | — | — | | record names |
| Shadow-pass GPU cost | S6 | target shadow distance 25 m | TDD §3.3 | | record |

> ADR-0006 §5.1's own estimate for the built layout is **~122,600 triangles**, 41% of the 300k budget, and it flags draw calls as *"not guaranteed by layout — gated on B112."* If the measured triangle count is far above ~122,600, the layout estimate and the built scene have diverged and that is worth a note of its own.

### J. Memory — World 2

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Texture2D total (detailed sample) | Zone 0 | < 150 MB whole scene | TDD §3.3 / ADR-0005 §3 | | ☐ pass ☐ fail |
| Texture2D total (detailed sample) | Zone 1 | < 150 MB whole scene | TDD §3.3 / ADR-0005 §3 | | ☐ pass ☐ fail |
| Texture2D total (detailed sample) | Zone 2 boss | < 150 MB whole scene | TDD §3.3 / ADR-0005 §3 | | ☐ pass ☐ fail |
| Zone 2 minus Zone 0 delta | — | expected ≈ 0 (boss is resident from load, §4.5) | ADR-0006 §1.4 | | record — a large delta is a finding |
| Graphics & Graphics Driver total | Zone 1 | — cross-check | TDD §3.7 | | record |
| Top 10 textures by size | any | — feeds BACKLOG B1 | TDD §3.4 | | list separately |
| Peak process memory (Xcode gauge) | S8 | — record | TDD §3.7 | | record |

### K. Allocation — World 2

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| GC Alloc / frame, walking, nothing happening | S5/S6 idle | **zero** | TDD §3.2 | | ☐ pass ☐ fail |
| GC Alloc / frame, worst combat frame | S6 | zero steady state | TDD §3.2 | | ☐ pass ☐ fail |
| Highest-allocating function (if any) | S6 | — | — | | record name + bytes |
| Telegraph wind-up allocations, Crane Duelist | S6 | pooled, no per-wind-up instantiation | TDD §3.3 / ADR-0003 | | ☐ pass ☐ fail |
| **Spin-Dash ground-lane telegraph allocations** | S7 boss | zero per dash | ADR-0007; B118 (claimed, never measured) | | ☐ pass ☐ fail |
| **Cut-Grass Trail hazard allocations** | S7 boss | pooled, zero per-frame | ADR-0005 §4; ADR-0006 §1.2 | | ☐ pass ☐ fail |
| Concurrent telegraph indicators, both geometries | S6 + S7 | ≤ 12 | TDD §3.3 / ADR-0003 | | ☐ pass ☐ fail |

### L. Loading and packaging — World 2

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Scene-start hitch (incl. runtime NavMesh bake) | `Backyard_Dojo` cold load | ≤ 500 ms | ADR-0004 §8, restated ADR-0005 §3 | | ☐ pass ☐ fail |
| — of which NavMesh rows | same frame | — record (§4.6) | — | | record ms |
| `[LevelBuilder] Runtime NavMesh baked: N verts, M tris` console line | same load | — record | `LevelBuilder.cs:108` | | record verbatim |
| Same, compared to B122's Editor bake (1 139 verts / 485 tris / 1 216.1 m²) | — | — | B122 | | ☐ similar ☐ **very different** |
| Install / download size, **with World 2 in the build** | build | < 200 MB | TDD §3.3 | | ☐ pass ☐ fail |
| `WeaponGripTest` still in scene list? | build | — context for size | §2.2 | ☐ yes ☐ no | note |

### M. Thermal — World 2 (S8). This is the acceptance criterion.

Run separately from S4, on a phone returned to room temperature. See §6's S8 subsection.

| Mark | Frame time / FPS | CPU % | Thermal state | Notes |
|---|---|---|---|---|
| Minute 1 | | | | |
| Minute 3 | | | | |
| Minute 6 | | | | |
| Minute 9 | | | | |
| **Minute 12** | | | | |
| Minute 15 | | | | |

```
Gap since the S4 run ended, and device temperature at start: ____________________

Thermal verdict (TDD §3.3 — no sustained frame-time regression across a full 15-min run):
  ☐ PASS — minute 12 within noise of minute 1
  ☐ FAIL — gradual, non-recovering frame-time creep (state the numbers): ____________________
  ☐ INCONCLUSIVE — run interrupted / device was charging / warm start / device is newer than target class
```

### N. Cross-scene residency (S9) — blocked

```
☐ BLOCKED — no in-game route from World 1 to World 2 (§1.1: WorldMapScreen.cs:117 hardcodes
  "TownSquare_Room1"; MetaScreen.OnContinue() restarts zone 0). Not measured.
☐ Route fixed — commit ____________ — S9 run and recorded below.

Texture2D total, standing in World 1 zone 2 before the transition:   __________ MB
Texture2D total, standing in World 2 zone 0 after the transition:    __________ MB
Peak process memory across the transition (Xcode gauge):             __________ MB
Scene-start hitch for Backyard_Dojo loaded AFTER a World 1 run:      __________ ms   (vs cold-start: ______ ms)
```

> **What this section exists to catch.** Both worlds are single continuous scenes holding their whole world resident (ADR-0004, ADR-0005). Nothing has ever verified that World 1's residency is actually released when World 2 loads over it. If it is not, the peak is the *sum* of two worlds, not the max — and every per-scene budget in both ADRs is measuring the wrong quantity. This is cheap to check and has never been checked; it is only blocked because the player cannot get there.

### O. Live enemy count sanity check — World 2

| Measurement | Scenario | Budget | Source | Measured | Verdict |
|---|---|---|---|---|---|
| Peak simultaneous live enemies observed | S5 Zone 0 | ≤ 4 | `RoomData_Backyard_Dojo_Zone0.asset` `maxConcurrentEnemies: 4`; ADR-0005 §3 | | ☐ pass ☐ fail |
| Peak simultaneous live enemies observed | S6 Zone 1 | ≤ 4 | `RoomData_Backyard_Dojo_Zone1.asset` `maxConcurrentEnemies: 4`; ADR-0005 §3 | | ☐ pass ☐ fail |
| Peak simultaneous live enemies observed | S7 Zone 2 | ≤ 1 (boss only) | `RoomData_Backyard_Dojo_Zone2.asset` `maxConcurrentEnemies: 1`, `spawnPoints: []`, `bossOwnedWin: 1` | | ☐ pass ☐ fail |

---

## 8. If something fails — what to do, and what NOT to do

> **⚠ Two rows in the tables below were corrected on 2026-09-10 (`technical-director`, B132 retraction). Read this before using either table.**
>
> 1. **Never conclude anything from `SRP Batcher Draw Calls Count` / the breakdown panel's "SRP Batcher" row.** It is broken in Unity `6000.5.3f1` — it reads **0 whether the SRP Batcher is on or off**. The SRP Batcher **is** engaged on this project (proven by A/B: SetPass calls 44 with it on, 85 with it off, identical frame). The §8.1 row *"`SRP Batcher: 0` in the World 2 breakdown → investigate a shader/material incompatibility"* is a **dead end and is withdrawn** — that investigation was done and found all 28 materials on stock URP shaders, all SRP-Batcher-compatible, and zero `MaterialPropertyBlock` users. **To check the batcher is alive, read `SetPass Calls Count`, not the SRP row:** ~40s means engaged, ~80s means it is not.
> 2. **`StaticBatchingUtility.Combine` is the wrong lever for World 2 and is withdrawn as the "Draw calls > 100" answer there.** Static batching is *already on* in `Backyard_Dojo` (51 objects, 6 batches, 106 of 163 draws). It does **not** reduce the draw-call count, it costs memory and build size, and it **preempts both GPU instancing and the SRP Batcher** on every object it touches. Adding more is a net loss. Note also that the SRP Batcher preempts GPU instancing project-wide, so **on this pipeline you get low SetPass counts or a low draw-call count, not both** — which is why TDD §3.2's `< 100 draw calls` was the wrong budget to hold. **Decided 2026-09-10 (owner, ADR-0009, Option A):** keep the SRP Batcher; SetPass Calls Count and render-thread ms are now the budgeted rendering quantities; raw draw-call count is recorded-only, not a pass/fail gate. See `docs/adr/0009-srp-batcher-and-the-draw-call-budget.md` and B132.
>
> The measured actual causes of World 2's overruns are **geometry density** (`pfb_env_stepping_stone_tile` at 1,750 tris × 32 = 84,000 tris, a third of the frame — B144) and the **shadow pass** (50 casters against 87 visible renderers), not batching. The 47-module stockade this document flags as the headline draw-call risk measured **10 draw calls and 444 triangles**.

**Do not change a setting before you have the measurement.** The whole point of §7 is that any later "this is faster now" claim can be checked against a recorded before-number. TDD §3.7 opens with the rule: *no optimization is accepted without a before/after measurement on device.*

Each of these is an **already-recorded** backlog item with an already-recorded rationale. None of them is a new idea, and none should be applied blind.

| If this fails | Most likely lever | Already tracked as | Nature of the fix |
|---|---|---|---|
| **Texture memory > 150 MB** *or* **thermal creep** | Texture import policy pass — `AssetPostprocessor`, per-category caps (characters/bosses 1024, weapons 512, env props 512, UI 256) plus explicit Android/iOS ASTC overrides | **BACKLOG B1**; TDD §3.4; ADR-0004 §8 calls it *"a prerequisite for this scene shipping"* | Scriptable, low risk, high value. The §4.5 top-10-textures list is the work order |
| **Thermal creep** with texture memory in budget | `m_SupportsHDR: 1` on `Mobile_RPAsset` forces FP16 render targets, roughly doubling colour bandwidth on tile-based mobile GPUs | **BACKLOG B38** | Needs an **on-device A/B test**, explicitly *not* a blind toggle. `m_RenderScale` is already 0.8 |
| **GPU-bound** (GPU time near/over 16.6 ms) | Shadow distance — target 25 m per TDD §3.3; live asset is currently 50 m over a 40 × 59.5 m street with one realtime directional light | TDD §3.3; ADR-0004 §8 (*"the cheap first move if GPU time is over"*) | Cheapest single GPU lever. Also see §5.2's discrepancy note |
| **Draw calls > 100** | `StaticBatchingUtility.Combine` on an environment-prop-only subroot. Note the `BatchingStatic` flags on `pfb_env_*` prefabs are **inert** — props are instantiated at runtime by `LevelBuilder`, and static batching is build-time for scene objects | BACKLOG (recorded under the B1-adjacent notes) | Only worth doing *if a real device profile shows draw calls are the bottleneck* — which is what §4.1 determines. Must be scoped away from pickups, piles, and spawn markers, which move or die |
| **Download size > 200 MB** | Retire the 9 out-of-scope cutscene `.mp4`s; keep only `spincycle_standoff.mp4`. Recovers ~300 MB | TDD §3.5 | A **content decision**, not engineering. Owner call |
| **GC allocation per frame > 0** | Find the function name from §4.4 and treat it as a regression against TDD §3.2's recorded "currently honoured" state | new finding if it happens — file it | Fix at the source; do not add pooling speculatively |
| **Any per-frame cost you cannot explain** | Re-read TDD §3.6 before reaching for `MaterialPropertyBlock`. Under this project's SRP Batcher, per-instance `renderer.material` copies **do** batch and **MPB breaks SRP batching** — the comment at `Enemy/SpinCycleAI.cs:1123` recommending MPB is a trap on this pipeline | TDD §3.6 | Do not follow generic Unity folklore here |

### 8.1 World 2 — additional levers, and one World 1 assumption that does not carry over

| If this fails | Most likely lever | Already tracked as | Nature of the fix |
|---|---|---|---|
| **Draw calls > 100 in `Backyard_Dojo`** | **`BD-01-Long`, the long wall variant.** ADR-0006 §5.2 already designed and budgeted it precisely to refund the module count: *"47 naive → 35 with §5.2"*, with the extra geometry costed at ~1,800 tris against 1,650 of remaining headroom in the < 8k new-ENV-geometry budget | **ADR-0006 §5.2**; `docs/BACKLOG.md` **B116** lists it under *Not built* | Art task, already specified. Cheapest structural lever in World 2 and it is a refund the ADR already accounted for, not new scope |
| **`SRP Batcher: 0` in the World 2 breakdown too** | This makes B112's SRP-Batcher-at-zero **project-wide, not a World 1 quirk** — a shader or material variant incompatibility rather than anything about the city scene | **B112**; ADR-0005 §Negative flags exactly this risk | Investigate the cause before spending anything else. It is also the failure of ADR-0005 §3's explicit condition and should be reported to `technical-director` as such, not just logged |
| **Scene-start hitch > 500 ms in World 2** | **Constrain the NavMesh bake area.** `Ground` is a 66.6 × 66.6 m plane baking 1 216 m² against ~318 m² of playable court | **B128** (currently P3) | If this is what fails, B128 is mis-prioritised — it is filed as a pathing/memory nicety and would in fact be a load-time budget breach. Say so when you file the result |
| **Texture memory > 150 MB in World 2** | Same B1 import-policy lever as World 1. Check the top-10 list for `Grasscutter_*` first — TDD §3.4 records `Grasscutter_BaseColor.png` at 30.9 MB on disk, and the boss is resident in all three zones | **BACKLOG B1**; TDD §3.4 | Same scriptable, low-risk fix. The `AssetPostprocessor` landed 2026-08-31 and was applied retroactively 2026-09-01, so a failure here would mean the policy is not covering these assets |

**The World 1 static-batching caveat does not apply to World 2 — check this before repeating it.** The §8 table above records that `BatchingStatic` flags on `pfb_env_*` prefabs are **inert in World 1**, because `LevelBuilder` instantiates those props at runtime and static batching is a build-time step for scene objects. That reasoning is sound and specific to World 1. World 2 is built the other way round:

| | World 1 (`CulDeSac_WildWestCity`) | World 2 (`Backyard_Dojo`) |
|---|---|---|
| Env props | instantiated at runtime by `LevelBuilder` from `WeaponDropTableSO.envProps` | **hand-dressed as scene objects.** `WeaponDropTableSO_Backyard_Dojo.envProps` is **empty** (0 entries), per ADR-0005 §6.6 |
| Scene objects carrying the `BatchingStatic` bit | 1 (`Ground`, flags `4294967295`) | **51** (flags `14`, `20`, `30` — all include bit 4 = Batching Static) |
| Static batching enabled for the platform | `m_StaticBatching: 1` for iPhone and Android (`ProjectSettings.asset:565-571`) | same |

So **World 2's stockade and dressing are genuinely eligible for build-time static batching and World 1's props are not.** That is a structural reason to expect World 2's draw-call number to come in better than World 1's 205 — and equally, if it does *not*, the batching is failing for a reason worth finding rather than a reason already understood. Record the Profiler's batching-savings figures alongside the raw count so this is answerable either way.

**Two things this changes in how you read a World 2 result**, neither of which is a budget change:

- A World 2 draw-call pass is **not** evidence that World 1's problem is solved or shrinking. The two scenes fail or pass for different structural reasons.
- Static batching trades memory for draw calls — it duplicates mesh data into combined buffers. If draw calls pass but section **J**'s memory numbers are higher than expected, those two results are related, not independent.

---

## 9. Honesty checklist before reporting a result

Per the standard TDD §3.4 sets on itself (*"This has not been verified on device and should not be treated as measured"*):

- ☐ Every number has its scenario (S1–S9) **and its world** and device model attached. A bare "draw calls: 140" is ambiguous now that two scenes are in scope.
- ☐ Millisecond figures are labelled with which build they came from (development vs release).
- ☐ Millisecond figures are labelled **60 FPS target or the old 30 FPS cap**. Numbers from either side of 2026-09-08 are not comparable.
- ☐ Any "pass" on a device newer than the 3–4-year-old target class is labelled a **lower bound**, not a pass.
- ☐ Derived numbers (like the enemy-HUD subtraction in §4.2) are shown with their inputs, not just the result.
- ☐ Anything not actually measured is left blank, not estimated.
- ☐ Discrepancies found between a document and a live asset are recorded as discrepancies, not silently fixed.
- ☐ A World 2 result is not reported as covering World 1, or vice versa. They are separate scenes with separate residency and separate structural failure modes (§8.1).
- ☐ If the scene list was reordered to launch World 2 (§2.2), the session record says so **and the list was put back**.
- ☐ S9 is recorded as **blocked**, not as passed or skipped, until the routing defect in §1.1 is fixed.
- ☐ An allocation pass on the Spin-Dash telegraph is not reported as discharging ADR-0006 §Validation 10, which is a human readability check (§4.7).
