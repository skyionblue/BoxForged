# ADR-0009: The SRP Batcher is engaged, it preempts GPU instancing, and TDD §3.2's `< 100 draw calls` budget is therefore not reachable as written

- **Status:** **Accepted (Option A) — 2026-09-10, pending one required on-device validation reading (see §7).** Owner chose Option A: keep the SRP Batcher, re-express the budget in SetPass calls/render-thread ms rather than raw draw-call count. No code/asset/setting change required by this decision itself — see §6 for the doc updates this discharges.
- **Date:** 2026-09-10
- **Author:** `technical-director`, during Sprint 2 (Mobile Release Readiness), while scoping `docs/BACKLOG.md` B132.
- **Scope:** project-wide rendering budget and batching strategy. Both worlds. Affects how every future scene is judged "in budget".
- **Related:** [ADR-0004](0004-world1-single-continuous-scene.md) §8 (World 1 budgets), [ADR-0005](0005-world2-single-continuous-scene.md) §3 (the SRP-Batcher grant condition this ADR discharges), [ADR-0006](0006-world2-zone-scale-and-arena-metric.md) §5.2 (`BD-01-Long`), `docs/TECHNICAL_DESIGN.md` §3.2 and §3.6, `docs/PERFORMANCE_PROFILING.md` §8/§8.1, `docs/BACKLOG.md` B112 / B132 / B144.

---

## 1. Context — a measurement error that has been driving decisions for two weeks

Three separate profiling captures (B112 on 2026-08-27, a World-1 recapture on 2026-09-08, and B132's World-2 capture the same day) all reported **`SRP Batcher Draw Calls Count: 0`** on device, with the SRP Batcher enabled project-wide. B132 concluded from this that ADR-0005 §3's central performance premise — that the dojo kit is "many instances of few materials … the case GPU instancing and the SRP Batcher exist to serve" — was **confirmed false**, and queued a shader/material-incompatibility investigation plus a `StaticBatchingUtility.Combine` pass.

That investigation was carried out on 2026-09-10. **The premise is not false. The counter is broken.**

## 2. Decision drivers — what was actually measured

All figures: `Backyard_Dojo.unity`, macOS Editor Play Mode, `PC_RPAsset` tier, Metal, development configuration. These are **not** device numbers (see §7). They are used for causal conclusions and relative magnitudes, both of which transfer.

**a. No shader or material incompatibility exists.** All 132 renderers in the scene resolve to **28 materials across 3 shaders** — `Universal Render Pipeline/Lit`, `Universal Render Pipeline/Unlit`, `Universal Render Pipeline/2D/Sprite-Unlit-Default`. `UnityEditor.ShaderUtil.GetSRPBatcherCompatibilityCode(shader, 0)` returns **0 (COMPATIBLE) for every one**. `Renderer.HasPropertyBlock()` is **false for all 132** — no `MaterialPropertyBlock` is in use anywhere, so the "MPB defeats batching" hypothesis has nothing to act on. A Constant Buffer layout mismatch is a per-shader property and all three shaders are stock URP shaders reporting compatible.

**b. The SRP Batcher is engaged and doing its job.** Toggling `GraphicsSettings.useScriptableRenderPipelineBatching` in Play Mode, with frame content held identical (163 draw calls, 255,359 triangles in every row):

| SRP Batcher | SetPass Calls | Static Batches | Standard Draw Calls | `SRP Batcher Draw Calls Count` |
|---|---|---|---|---|
| **On** (as shipped) | **44** | 6 | 163 | **0** |
| **Off** | **85–86** | 22 | 163 | **0** |
| **On** again (reproduced) | **44** | 6 | 163 | **0** |

Disabling the batcher **nearly doubles SetPass calls**. The counter reads 0 in both states, so a 0 reading carries no information. Corroborating: the aggregate `Draw Calls Count` counter returns `_valid: false` on `6000.5.3f1` — this counter family is only partly wired in this Unity version.

**c. The SRP Batcher preempts GPU instancing.** 50 synthetic cubes (`Cube` primitive + a runtime copy of the stockade's `Lit` material) spawned in front of the camera:

| Configuration | Draw calls | Instanced Batched Draw Calls | Instanced Batches | SetPass |
|---|---|---|---|---|
| SRP Batcher **on**, instancing **off** | 213 (+50) | 0 | 0 | 44 |
| SRP Batcher **on**, instancing **on** | **213 (+50)** | **0** | **0** | 44 |
| SRP Batcher **off**, instancing **on** | **163 (+0)** | **61** | **12** | 86 |

Enabling instancing changed nothing while the batcher was on. Turning the batcher off collapsed the same 50 cubes into one instanced draw.

**d. Static batching preempts both, saves no draw calls, and costs memory.** 51 scene objects in `Backyard_Dojo` carry the Batching Static bit, producing **6 static batches and 106 of the 163 draws**. Static batching folds meshes into a shared buffer but still issues one draw per instance — it reduces per-draw setup, not the count — and the combined meshes measure 33k–35k triangles each.

**e. The things blamed for the overruns are not causing them.** Ablation, same frame:

| Ablation | Draw calls | Triangles |
|---|---|---|
| Baseline | 163 | 255,359 |
| 45 active `BD01_WallModule` renderers disabled | 153 (**−10**) | 254,915 (**−444**) |
| 32 `SteppingStone*` renderers disabled | 140 (**−23**) | 171,359 (**−84,000**) |

The 47-module stockade — B132's named "single biggest draw-call risk", and the thing ADR-0006 §5.2 budgeted `BD-01-Long` to refund — costs **10 draw calls**. Each module is a **12-triangle `Cube`**, and most are frustum-culled at any moment. The real triangle cost is decorative floor tiles at **1,750 triangles each × 32** (see `docs/BACKLOG.md` B144). Roughly half of all draws are the shadow pass: 50 shadow casters against 87 visible renderers, with URP shadow distance already at TDD §3.3's 25 m target.

## 3. The actual architectural problem

Unity's batching mechanisms are **mutually exclusive per renderer**, in priority order: static batching → SRP Batcher → GPU instancing. Only **GPU instancing reduces the draw-call count**. The other two reduce per-draw CPU setup.

This project has static batching on for 51 objects, the SRP Batcher on globally, and GPU instancing consequently never engaging. So:

> **TDD §3.2's `< 100 draw calls` budget cannot be met on this content while the SRP Batcher is enabled, because the SRP Batcher blocks the only mechanism that reduces the count.**

This is not a content problem that more art optimisation fixes. It is a mismatch between the budget and the pipeline. It has been invisible until now because the broken counter made it look like the SRP Batcher was doing nothing, which pointed every previous investigation at "make the SRP Batcher work" rather than "the SRP Batcher working is *why* the count is high."

## 4. A consequence for ADR-0005 §3 that must be recorded either way

ADR-0005 §3 granted the single-continuous-scene architecture on the condition of *"verify a non-zero SRP Batcher or Instanced draw-call count on device."* **That condition is unsatisfiable as written** — the counter it names cannot report non-zero on this Unity version, and the instanced counter cannot report non-zero while the SRP Batcher is on. The condition should be restated, regardless of which option below is chosen:

> **Proposed restatement of ADR-0005 §3's condition:** *verify on device that `SetPass Calls Count` is materially below the renderer count — i.e. that per-draw state setup is being amortised. A reading in the 40s against ~87 visible renderers satisfies this; a reading approaching the draw-call count does not.*

This is the same property ADR-0005 §3 was trying to assert. It is merely expressed in a quantity this Unity version actually reports.

## 5. Options — **the owner decision**

### Option A (recommended) — keep the SRP Batcher; re-express the budget in what it governs

Treat **SetPass calls and CPU render-thread milliseconds** as the budgeted rendering quantities, and raw draw-call count as recorded-only. Current SetPass count is **44**, which is healthy for the content.

- **Cost:** zero code, zero setting, zero asset change. A documentation change to TDD §3.2, ADR-0004 §8 and ADR-0005 §3.
- **Benefit:** budgets the thing that actually costs CPU on mobile, and the thing the pipeline is designed around. The measured SetPass halving is a real win the project is already getting for free.
- **Risk:** the project loses a simple, familiar number. Draw-call count is still worth *recording* — an unexplained jump is still a signal.
- **What still has to happen:** the actual overruns are content, and remain P1. B144 (geometry density — ~75,000 triangles recoverable from one asset) and the shadow-pass pass (turn off `Cast Shadows` on flat ground decoration) are the real work, and neither depends on this decision.

### Option B — disable the SRP Batcher project-wide; chase a literal `< 100` draw-call count via GPU instancing

- **Requires:** `useSRPBatcher: 0` on both RP assets; clearing Batching Static from the 51 scene objects (instancing and static batching are also mutually exclusive); enabling `Enable GPU Instancing` on the `Lit` material (currently **off**) and auditing the other 27.
- **Cost:** SetPass calls roughly double (44 → ~85, measured). Meaningful churn across scenes and materials, and a per-scene authoring burden thereafter.
- **Benefit:** the draw-call count becomes reducible. Measured effect on the synthetic probe was 50 draws → 1.
- **Risk:** trades a **measured** CPU win for an **unmeasured** one. On tile-based mobile GPUs, state changes are typically the more expensive of the two, so this may be a net regression on the target device class. Would need an on-device A/B before adoption, not a blind toggle — the same standard `docs/PERFORMANCE_PROFILING.md` §8 already applies to `m_SupportsHDR` (B38).

### Simpler alternative considered and rejected

**"Do nothing, keep chasing `< 100` with the SRP Batcher on."** Rejected because it is not reachable — §3. Continuing to hold scenes to it means every future scene fails a budget it structurally cannot pass, which trains the team to ignore the budget. The point of a budget is that passing and failing both mean something.

## 6. Consequences of accepting Option A

- `docs/TECHNICAL_DESIGN.md` §3.2 gains SetPass and render-thread-ms budgets; the draw-call line becomes record-only with a note on why.
- ADR-0005 §3's condition is restated per §4 and is then **discharged as satisfied**, closing the last open condition on the World 2 architecture grant.
- `docs/PERFORMANCE_PROFILING.md` §8/§8.1 lose the "SRP Batcher: 0 → investigate" and "draw calls > 100 → `StaticBatchingUtility.Combine`" rows (already annotated as withdrawn on 2026-09-10) and gain "SetPass calls approaching renderer count → check the SRP Batcher is on".
- ADR-0006 §5.2's `BD-01-Long` refund loses its performance justification (measured: 10 draw calls) and must be re-justified on art merit or dropped. It stays listed under *Not built*.
- B132 closes as diagnosed; B112 is annotated as superseded; B144 becomes the live P1 rendering item.

## 7. Validation and honesty note

Per `docs/PERFORMANCE_PROFILING.md` §9: **every number in this ADR is macOS-Editor Play Mode, not device, and not from a release build.** They support causal conclusions (does toggling X change Y?) and relative magnitudes (which asset costs most) — both of which transfer — and no absolute device figure is claimed. The Editor faithfully reproduced the on-device symptom (`SRP Batcher Draw Calls Count: 0`), which is what made the A/B possible in the first place.

**Required before this ADR is accepted:** one on-device reading of **`SetPass Calls Count`** in both worlds. In the 40s confirms the batcher is engaged on the shipping build and validates Option A's premise; in the 80s would mean the Editor and device differ and this whole analysis needs redoing on device. The counter is already visible in the Profiler's Rendering module next to the ones the owner has been capturing, so this needs no new build.

**Play Mode hygiene for the investigation itself:** the SRP batching toggle, six `NavMeshObstacle`s, 45 wall renderers, 32 stepping-stone renderers, the directional light's shadow mode and three temporary GameObjects were all mutated and all restored. Scene fingerprint (753 transforms; name + position + scale + activeSelf; MD5) verified identical before and after; `PC_RPAsset` and `Mobile_RPAsset` verified non-dirty; `git status` shows no Unity asset changes from the session.
