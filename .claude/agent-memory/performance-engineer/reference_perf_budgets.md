---
name: reference-perf-budgets
description: Where BoxForged's performance budgets, profiling protocol, and known perf debt live — TECHNICAL_DESIGN.md §3, plus the URP asset settings worth re-checking.
metadata:
  type: reference
---

BoxForged performance authority:

- `docs/TECHNICAL_DESIGN.md` §3 — the budget table, new budgets (texture memory, download size, enemy HUD draw calls, telegraph indicators, shadow distance, thermal), and §3.7 the profiling protocol (representative scenario = one full room clear at `maxConcurrentEnemies` + one boss fight; record frame time at minute 1 vs minute 12 for thermal).
- `docs/PROJECT_CONTEXT.md` — the original budget (<100 draw calls, <300k tris, zero per-frame GC).
- `docs/BACKLOG.md` — B1 tracks the texture-memory / import-policy work. Treat as known; do not re-flag it as a new finding.
- `BoxForged/BoxForged/Assets/Settings/Mobile_RPAsset.asset` — the live URP settings. Note the TDD's §3.3 numbers for this file are stale: it says shadow distance 40 and a 256x256 atlas; the asset actually reads `m_ShadowDistance: 50`, `m_MainLightShadowmapResolution: 1024`, `m_ShadowCascadeCount: 1`, plus `m_RenderScale: 0.8` and `m_SupportsHDR: 1`. Read the asset, don't quote the TDD.

**Note on `Shader.Find`:** this codebase uses it in several places (`EnemyHealthBar`, `MinimapIndicator`, `BossHeadBounce`, `AttackTelegraphIndicator`). It is safe for URP package shaders but NOT for project-authored shaders — a custom shader referenced by no material asset and absent from Graphics Settings' Always Included Shaders is stripped from player builds and works only in the Editor. Check GUID references before trusting any `Shader.Find` of a `BoxForged/*` shader. See [[feedback-evidence-standard]].
