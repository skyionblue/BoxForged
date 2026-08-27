# BoxForged — On-Device Performance Profiling Checklist (iOS)

- **Status:** Operational checklist. Executable by the owner without further engineering support.
- **Date:** 2026-08-27
- **Scope:** `CulDeSac_WildWestCity` (World 1, all three zones) on a physical iPhone.
- **Authority:** This document **invents no budgets and changes none**. Every budget referenced here is quoted from `docs/TECHNICAL_DESIGN.md` §3.2 / §3.3 / §3.1 or `docs/adr/0004-world1-single-continuous-scene.md` §8, and the source is named on every line. `docs/TECHNICAL_DESIGN.md` §3.7 remains the authoritative statement of *the protocol*; this document is the step-by-step *execution* of that protocol.

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

The iOS profile uses the global scene list (`m_OverrideGlobalSceneList: 0`), which currently contains **two** enabled scenes:

| Index | Scene | Enabled |
|---|---|---|
| 0 | `Assets/_Project/Scenes/CulDeSac_WildWestCity.unity` | ☑ |
| 1 | `Assets/_Project/Scenes/WeaponGripTest.unity` | ☑ |

`WeaponGripTest` is a validation scene, not shipped content. It does not affect frame time (it is never loaded), but **it and its referenced assets are included in the build**, so it inflates any download-size measurement. When you measure download size in §5.3, record whether it was still in the list. Do not remove it as part of this profiling pass — that is a separate decision.

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

### 4.2 Enemy HUD draw calls → §3.3

Budget (TDD §3.3): **≤ 2 draw calls per enemy, ≤ 20 total.**

This one needs a small bit of arithmetic rather than a single counter:

1. Capture the Rendering module's **Draw Calls Count** at a moment in S2 when **4 enemies are alive and all their health bars are visible**.
2. Capture **Draw Calls Count** at a moment in the same run with **0 enemies alive** (right after the zone clears, before you move much — keep the camera pointed at roughly the same view).
3. The difference, divided by 4, is roughly the per-enemy HUD + character cost. Write down all three numbers, not just the result.

Pass if the total attributable to enemy HUD stays ≤ 20 draw calls. `Enemy/EnemyHealthBar.cs:181,196` creates two `new Material` instances per enemy at runtime, which is exactly the cost this budget was written to bound.

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

### 4.6 Scene-start hitch → ADR-0004 §8

Budget (ADR-0004 §8): **≤ 500 ms scene-start hitch, including the runtime NavMesh bake.**

1. Start recording in the Profiler **before** the scene loads (tap into the level with the Profiler already recording).
2. Find the single enormous frame at the start of the graph. Click it.
3. Read its total frame time in ms.

Pass if ≤ 500 ms. Look in the **Hierarchy** view of that frame for NavMesh-related rows to see how much of it is the bake.

### 4.7 Telegraph indicators → §3.3

Budget (TDD §3.3, from ADR-0003): **≤ 12 concurrent, pooled. Per-wind-up instantiation is forbidden.**

This is checked structurally, not by a counter:

1. During S2 (4 enemies) and S3 (boss), watch the **Memory** module's graph and the **GC Alloc** column for allocation spikes that coincide with attack wind-ups.
2. Pass if telegraph wind-ups produce **no repeated instantiation allocations** — the pool should be created once.

**Configuration note to record, not a budget change:** `AttackTelegraphService._poolSize` currently defaults to **8**, below the ≤ 12 budget ceiling. That is legal (the budget is a maximum, not a requirement) and the service recycles the oldest indicator when the pool is exhausted rather than allocating. But if you see telegraph indicators visibly *disappearing early* during a busy S2 fight, the pool size is why — note it and it can be raised toward 12 without breaking the budget.

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
3. Click the red **record** button. Play scenario **S2**, then **S3**.
4. Stop recording.

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

## 6. Step 4 — The thermal run (S4). This is the acceptance criterion.

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

---

## 7. Results template — record here

Copy this block for each profiling session and fill it in. Append new sessions below; do not overwrite old ones — the point is to be able to compare a later run against an earlier one.

### Session record

```
Session date:            ____________________
Build:                   git commit ____________  branch ____________
Device model:            ____________________  (iOS version: __________)
Device class vs budget:  ☐ 3–4-year-old (target class)  ☐ NEWER than target — results are a LOWER BOUND only (Rule 2)
Build type:              ☐ Development Build (Pass A)   ☐ Release, non-development (Pass B)
Orientation:             landscape          Screen brightness: ______   Charging: ☐ no ☐ yes (invalidates thermal)
Scene:                   CulDeSac_WildWestCity
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

## 8. If something fails — what to do, and what NOT to do

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

---

## 9. Honesty checklist before reporting a result

Per the standard TDD §3.4 sets on itself (*"This has not been verified on device and should not be treated as measured"*):

- ☐ Every number has its scenario (S1–S4) and device model attached.
- ☐ Millisecond figures are labelled with which build they came from (development vs release).
- ☐ Any "pass" on a device newer than the 3–4-year-old target class is labelled a **lower bound**, not a pass.
- ☐ Derived numbers (like the enemy-HUD subtraction in §4.2) are shown with their inputs, not just the result.
- ☐ Anything not actually measured is left blank, not estimated.
- ☐ Discrepancies found between a document and a live asset are recorded as discrepancies, not silently fixed.
