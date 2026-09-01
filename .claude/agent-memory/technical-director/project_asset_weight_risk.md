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
| SRP Batcher contribution | assumed active | **0** (Standard 204, SRP Batcher 0, Instanced 0) | Not engaging at all |

Also found: an undocumented `Application.targetFrameRate = 30` at `GameManager.cs:101-102`, contradicting the documented 60 FPS target, with only ~4.34 ms CPU against the resulting 33 ms budget. Owner decision still open.

**So the real cost of holding a whole world resident is draw calls and triangles, not texture residency.** And the SRP-Batcher-at-zero finding is more urgent than any batching *strategy* — something is preventing it engaging at all (likely shader/material variant incompatibility). Investigate that before scoping `StaticBatchingUtility.Combine`.

### What still stands from the original inspection

- ~353 source textures totalling 2.6 GB; individual Meshy BaseColor maps 27–31 MB; essentially everything imports at `maxTextureSize: 2048` with only a `DefaultTexturePlatform` entry and **no Android or iPhone override**. The import-policy pass (`AssetPostprocessor`, per-category caps, ASTC) is still the right work — it is now about *download size and headroom*, not about a measured memory breach.
- `Assets/StreamingAssets/Cutscenes/` — 10 `.mp4`, 326 MB, shipping verbatim for a feature scoped to boss-intros-only. Exactly one is a boss intro. Retiring the rest recovers ~300 MB of download size. Still the cheapest single win available, and it is a content decision.
- Gameplay code hygiene is genuinely good (zero LINQ anywhere, pre-allocated physics buffers, consistent material `Destroy`), so optimization attention still belongs on assets and rendering, not on the C#.
- Thermal, not peak frame time, is still the real acceptance criterion — 10–15 minute runs on 3–4-year-old hardware. The minute-1-vs-minute-12 run has **not** been done yet, nor has Pass B (Xcode Instruments, non-development build).

**Why this correction matters:** the wrong half of this hypothesis was about to be used as an argument against World 2's scene architecture (ADR-0005). Presenting the file-inspection estimate as a finding would have driven a real architecture decision off a number that turned out to be 3.6× too pessimistic.

**How to apply:** for World 2 and any future world, budget **per scene, not per room** — < 100 draw calls, < 300k tris, < 150 MB textures, ≤ 20 distinct ENV materials (ADR-0005 §3). Prefer many instances of few shared-atlas materials over unique per-prop Meshy textures; that is the axis World 1 actually failed on. Never quote a file-inspection estimate as measured.

Related: [[project-preproduction-gate]], [[project-docs-drift-from-code]]
