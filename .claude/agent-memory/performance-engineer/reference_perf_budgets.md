---
name: reference-perf-budgets
description: Where BoxForged's performance budgets, profiling protocol, and known perf debt live — TECHNICAL_DESIGN.md §3, the on-device checklist in PERFORMANCE_PROFILING.md, plus the URP asset settings worth re-checking.
metadata:
  type: reference
---

BoxForged performance authority:

- `docs/TECHNICAL_DESIGN.md` §3 — the budget table, new budgets (texture memory, download size, enemy HUD draw calls, telegraph indicators, shadow distance, thermal), and §3.7 the profiling protocol (representative scenario = one full room clear at `maxConcurrentEnemies` + one boss fight; record frame time at minute 1 vs minute 12 for thermal).
- `docs/PERFORMANCE_PROFILING.md` (added 2026-08-27) — the **operational** side: beginner-executable iOS device checklist, per-budget pass/fail mapping, and the results-recording template. Device measurements go **there**, not into the TDD. It cites budgets only; it never defines them. Two more recorded budgets live in ADR-0004 §8 and not in the TDD: **scene-start hitch ≤ 500 ms** (incl. runtime NavMesh bake) and **peak live enemies ≤ 4** (zone 1 `maxConcurrentEnemies`).
- `docs/PROJECT_CONTEXT.md` — the original budget (<100 draw calls, <300k tris, zero per-frame GC).
- `docs/BACKLOG.md` — B1 tracks the texture-memory / import-policy work. Treat as known; do not re-flag it as a new finding.
- `BoxForged/BoxForged/Assets/Settings/Mobile_RPAsset.asset` — the live URP settings. Note the TDD's §3.3 numbers for this file are stale: it says shadow distance 40 and a 256x256 atlas; the asset actually reads `m_ShadowDistance: 50`, `m_MainLightShadowmapResolution: 1024`, `m_ShadowCascadeCount: 1`, plus `m_RenderScale: 0.8` and `m_SupportsHDR: 1`. Read the asset, don't quote the TDD.

**Tooling constraints for any on-device pass (verified 2026-08-27):**

- The standalone **Memory Profiler package (`com.unity.memoryprofiler`) is NOT installed** — `Packages/manifest.json` has URP, Cinemachine, Input System, AI Navigation, Timeline, uGUI, Visual Scripting, Test Framework and module stubs, nothing else. Texture-memory measurement therefore has to come from the built-in Profiler Memory module's detailed "Take Sample", which is coarse but enough to answer "over or under 150 MB". Installing the package is a new dependency and needs owner approval.
- **Unity's GPU Usage profiler module does not report reliably on iOS/Metal.** GPU frame time has to come from Xcode (Game Performance / Metal System Trace template, or Capture GPU Frame). Don't promise GPU numbers from the Unity Profiler alone.
- The **Profiler frame buffer caps at 2000 frames (~33 s at 60 FPS)**, so the TDD's minute-1-vs-minute-12 thermal comparison can never be one continuous Unity Profiler recording — it must be Xcode's continuous gauges, or two short captures at each end. Preference lives at Settings → Analysis → Profiler → Frame Count.
- **A Development Build inflates CPU frame time.** Split every device pass: Unity Profiler for counts/structure, non-development build + Xcode for the frame-time and thermal verdict. Never report a dev-build millisecond figure as the 60 FPS answer.
- `Assets/Settings/Build Profiles/iOS.asset` is where `m_Development` / `m_ConnectProfiler` / `m_BuildWithDeepProfilingSupport` live (Unity 6 Build Profiles, `m_OverrideGlobalSceneList: 0` so it uses the global scene list). The committed state has Development/ConnectProfiler **off**; the owner toggles them in the Editor, which shows up as an uncommitted asset diff. **Check `git diff` on this file before claiming what the build's profiling state is** — see [[feedback-unity-editor-state-leak]] for the general shape of this hazard. Also note `WeaponGripTest.unity` is currently **enabled** in the global scene list, so it ships in device builds and inflates download-size measurements.

**Note on `Shader.Find`:** this codebase uses it in several places (`EnemyHealthBar`, `MinimapIndicator`, `BossHeadBounce`, `AttackTelegraphIndicator`). It is safe for URP package shaders but NOT for project-authored shaders — a custom shader referenced by no material asset and absent from Graphics Settings' Always Included Shaders is stripped from player builds and works only in the Editor. Check GUID references before trusting any `Shader.Find` of a `BoxForged/*` shader. See [[feedback-evidence-standard]].
