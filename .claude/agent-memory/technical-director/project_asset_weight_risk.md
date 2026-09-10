---
name: project-asset-weight-risk
description: Asset weight was BoxForged's presumed dominant risk, but the first on-device capture (2026-08-27) disproved the texture-memory half — the measured cost of a resident whole-world scene is draw calls and triangles, with the SRP Batcher contributing zero.
metadata:
  type: project
---

**Corrected 2026-08-31 against real device measurement. The original hypothesis was half wrong — read the correction before repeating it.**

### What was measured (B112, `docs/PERFORMANCE_PROFILING.md` session 2026-08-27)

A full on-device `CulDeSac_WildWestCity` playthrough — the whole of World 1 resident in one scene, which was the scenario ADR-0004 §8 predicted would breach the texture budget:

| Metric | Budget | Measured | Verdict |
|---|---|---|---|
| Texture memory, steady state | < 150 MB | **41.2 MB / 52 textures** | Comfortably inside — the prediction **did not materialize** |
| Draw calls | < 100 | **205** | Over by 2× |
| Triangles | < 300k | **356.7k** | Over ~19% |
| SRP Batcher contribution | assumed active | **0** (Standard 204, SRP Batcher 0, Instanced 0) | ~~Not engaging at all~~ **RETRACTED — broken counter, see below** |

Also found: an undocumented `Application.targetFrameRate = 30` at `GameManager.cs:101-102`, contradicting the documented 60 FPS target, with only ~4.34 ms CPU against the resulting 33 ms budget. Owner decision still open.

**So the real cost of holding a whole world resident is draw calls and triangles, not texture residency.** That half stands.

**SECOND CORRECTION, 2026-09-10 — the SRP-Batcher-at-zero row above is a measurement artifact, and the sentence that used to follow it here sent two later sessions down a dead end.** `SRP Batcher Draw Calls Count` reads 0 in this Unity version whether the batcher is on or off. The batcher **is** engaged (SetPass 44 with it on, 85 with it off). There is no shader/material incompatibility — every material is a stock URP shader reporting SRP-Batcher-compatible, and nothing uses `MaterialPropertyBlock`. Do **not** scope `StaticBatchingUtility.Combine` either: static batching is already on, saves no draw calls, and preempts both the SRP Batcher and GPU instancing. Full detail and the reusable A/B in [[srp-batcher-counter-is-broken]]; the budget question it opens is ADR-0009 (Proposed).

### What still stands from the original inspection

- ~353 source textures totalling 2.6 GB; individual Meshy BaseColor maps 27–31 MB; essentially everything imports at `maxTextureSize: 2048` with only a `DefaultTexturePlatform` entry and **no Android or iPhone override**. The import-policy pass (`AssetPostprocessor`, per-category caps, ASTC) is still the right work — it is now about *download size and headroom*, not about a measured memory breach.
- `Assets/StreamingAssets/Cutscenes/` — 10 `.mp4`, 326 MB, shipping verbatim for a feature scoped to boss-intros-only. Exactly one is a boss intro. Retiring the rest recovers ~300 MB of download size. Still the cheapest single win available, and it is a content decision.
- Gameplay code hygiene is genuinely good (zero LINQ anywhere, pre-allocated physics buffers, consistent material `Destroy`), so optimization attention still belongs on assets and rendering, not on the C#.
- Thermal, not peak frame time, is still the real acceptance criterion — 10–15 minute runs on 3–4-year-old hardware. The minute-1-vs-minute-12 run has **not** been done yet, nor has Pass B (Xcode Instruments, non-development build).

**Why this correction matters:** the wrong half of this hypothesis was about to be used as an argument against World 2's scene architecture (ADR-0005). Presenting the file-inspection estimate as a finding would have driven a real architecture decision off a number that turned out to be 3.6× too pessimistic.

**How to apply:** for World 2 and any future world, budget **per scene, not per room** — < 100 draw calls, < 300k tris, < 150 MB textures, ≤ 20 distinct ENV materials (ADR-0005 §3). Prefer many instances of few shared-atlas materials over unique per-prop Meshy textures; that is the axis World 1 actually failed on. Never quote a file-inspection estimate as measured.

**One more axis this memory missed entirely: geometry density.** Textures got a policy (B1's `AssetPostprocessor`) and it worked. Meshes got nothing — no per-category triangle cap, no LOD requirement, no import check. Measured 2026-09-10: `pfb_env_stepping_stone_tile` is **1 750 triangles for a flat ground decal**, placed 32 times, = a third of the whole 300k triangle budget in one frame. No dojo ENV prefab has a `LODGroup`. When budgeting a future world, budget triangles per *asset*, not just per scene. B144.

Related: [[project-preproduction-gate]], [[project-docs-drift-from-code]], [[srp-batcher-counter-is-broken]], [[navmesh-baking]]
