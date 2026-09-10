---
name: srp-batcher-counter-is-broken
description: Unity 6000.5.3f1's "SRP Batcher Draw Calls Count" reads 0 whether the batcher is on or off — it produced three wrong "independent on-device confirmations". Use SetPass Calls instead, and know that SRP Batcher preempts GPU instancing so <100 draw calls is unreachable.
metadata:
  type: reference
---

## Never trust `SRP Batcher Draw Calls Count` on this project

Unity `6000.5.3f1`'s Rendering-category counter **`SRP Batcher Draw Calls Count` reads 0 whether the SRP Batcher is enabled or disabled.** A 0 reading carries no information. The aggregate `Draw Calls Count` in the same category returns `_valid: false` — this counter family is only partly wired in this Unity version. `UnityEditor.UnityStats.srpBatcherDrawCalls` is the same broken underlying counter.

This cost the project real time: B112 (2026-08-27), a World-1 recapture and B132 (2026-09-08) each read it on device, each got 0, and B132 wrote up **"ADR-0005 §3's central performance premise is confirmed false … the third independent on-device confirmation."** They were **one measurement error repeated three times**. Independence was illusory — same counter, same bug.

**Use `SetPass Calls Count` instead.** That is the quantity the SRP Batcher actually governs and it is right next to the broken one in the Profiler's Rendering module.

## The A/B that settles it, reusable

Same scene, same camera, frame content held identical (163 draw calls, 255 359 tris in every row):

| `GraphicsSettings.useScriptableRenderPipelineBatching` | SetPass | Static Batches | Draw Calls | SRP counter |
|---|---|---|---|---|
| **True** (shipped) | **44** | 6 | 163 | 0 |
| **False** | **85** | 22 | 163 | 0 |

Toggling it in Play Mode is safe and instant. **Only ever set it while `EditorApplication.isPlaying` is true** — in Edit mode it touches project graphics state; guard the call and restore explicitly (I tripped this once when Play Mode had silently exited).

## The architectural fact behind it

Unity's batching mechanisms are **mutually exclusive per renderer**, in priority order: **static batching → SRP Batcher → GPU instancing**. Only **GPU instancing reduces the draw-call *count***; the other two reduce per-draw CPU setup. Measured with 50 synthetic cubes: SRP Batcher on + instancing on → +50 draws, 0 instanced batches; SRP Batcher off + instancing on → +0 draws, the 50 collapse into 1.

Therefore **TDD §3.2's `< 100 draw calls` budget is unreachable while the SRP Batcher is enabled**, and it is not a content problem. Raised as **ADR-0009 (Proposed, needs owner decision)**: keep the batcher and budget SetPass calls (recommended, zero change) vs disable it project-wide and chase the literal count via instancing (SetPass roughly doubles).

Two `docs/PERFORMANCE_PROFILING.md` §8/§8.1 levers are **withdrawn** as a result: *"SRP Batcher: 0 → investigate shader/material incompatibility"* (dead end — all 28 materials in `Backyard_Dojo` are stock URP shaders, all report `ShaderUtil.GetSRPBatcherCompatibilityCode == 0`, and **zero** renderers use `MaterialPropertyBlock`), and *"draw calls > 100 → `StaticBatchingUtility.Combine`"* (static batching is already on for 51 objects, saves no draw calls, costs memory, and preempts the other two).

## Where the cost actually is — measure by ablation, don't reason from instance counts

Disabling renderers in Play Mode and re-reading counters is fast and decisive. In `Backyard_Dojo`:

| Ablation | Draw calls | Triangles |
|---|---|---|
| 45 active `BD01_WallModule` | **−10** | **−444** |
| 32 `SteppingStone*` | **−23** | **−84 000** |

**The 47-module stockade that three documents call "the single biggest draw-call risk" costs 10 draw calls** — each module is a 12-triangle `Cube` and most are frustum-culled. The real cost is decorative geometry density: `pfb_env_stepping_stone_tile` is **1 750 triangles** for a flat ground decal, ×32 = a third of the whole triangle budget. No dojo ENV prefab has a `LODGroup`, and B1's `AssetPostprocessor` covers textures only — there is **no geometry policy at all**. B144.

Roughly half of all draws are the shadow pass (50 casters vs 87 visible renderers) with shadow distance already at its 25 m target — so the lever is per-renderer `Cast Shadows`, not distance.

## Practical gotchas when doing this in the Editor

- `Application.runInBackground` is **false** here: Play Mode does not tick while the Editor is unfocused. Set it true at runtime and print `Time.frameCount` to prove frames moved, or your A/B measures nothing.
- `manage_profiler get_counters` sometimes returns all-zero when the Game view has stopped repainting. Zeros across *every* counter means "no frame rendered", not "no draws" — retry, don't record it.
- **`frame_debugger_enable` kicked the Editor out of Play Mode** and `frame_debugger_get_events` returned nothing. Don't build a plan around it; the counter A/B is more informative anyway.
- Fingerprint the scene before Play Mode (sorted `name|position|scale|activeSelf` → MD5) and re-check after. The scene here is often already `isDirty` with the owner's unsaved edits, and this project has a documented history of Play-Mode state leaking.

Related: [[navmesh-baking]], [[asset-weight-risk]], [[project-unsatisfiable-metrics]], [[project-docs-drift-from-code]]
