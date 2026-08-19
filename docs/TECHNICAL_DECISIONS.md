# BoxForged — Technical Decisions

Index and summary of accepted technical decisions and package approvals. Full context lives in `docs/adr/`.

---

## Architecture Decision Records

| ADR | Title | Status | Date |
|---|---|---|---|
| [0001](adr/0001-fixed-low-follow-camera.md) | Fixed low-angle follow camera (no rotation) | **Proposed** | 2026-08-19 |
| [0002](adr/0002-full-scene-rebuild.md) | Full scene rebuild on extracted room data | **Proposed** | 2026-08-19 |
| [0003](adr/0003-attack-telegraph-channel.md) | Occlusion-independent attack telegraph channel | **Proposed** | 2026-08-19 |

All three await owner approval at the pre-production gate. **None authorizes implementation.**

### ADR-0001 — Fixed low-angle follow camera

Replaces the "fixed top-down, locked" camera with a fixed-rotation follow camera at **pitch 36°, FOV 45°, yaw 0°**, derived offset `(0, 5.5, -7.57)`.

The decision specifies **pitch, distance, and FOV with measurable framing criteria** rather than an offset triple, because an offset hides the constraint `pitch > FOV/2` — below which the horizon enters frame and ground depth runs to infinity.

Two corrections of record: the real rig was `(7.879929, 11, -10)` at FOV 40, not the documented `(0, 12, -8)`; and it carried an undocumented **−38.2° yaw** that silently rotated the control mapping, since movement is camera-yaw-relative.

Rejected: `CinemachineDeoccluder` (camera collision becomes a machine-checked level constraint instead), and a dynamic or per-encounter rig (complexity a two-person team cannot debug live; deferred pending boss-room playtest).

### ADR-0002 — Full scene rebuild on extracted room data

All scenes rebuilt from scratch under the new camera. `LevelBuilder` + `WeaponDropTableSO` architecture preserved.

The material change: **promote `RoomData` to `RoomDataSO`** *before* rebuilding. Room definitions currently exist only as prefab-instance overrides with spawn points as `objectReference` fileIDs to scene-local GameObjects — they are destroyed by a rebuild. The environment layer is portable; the encounter layer is not.

Scenes become thin composition roots. Camera clearance (≥ 8 m rear, ≥ 6 m overhead, combat radius ≤ 9 m) is validated in the builder.

Existing scenes remain on disk; deletion is a separate decision.

### ADR-0003 — Occlusion-independent attack telegraph channel

Not requested; recorded because ADR-0001 cannot ship safely without it.

BoxForged has **no telegraph system** — every attack tell is a whole-body material tint, which works only because the current camera sees enemies separated and unoccluded. Parryable vs un-parryable is encoded **entirely in hue**, a standing accessibility defect.

Adds an occlusion-independent overhead indicator on the existing overlay camera stack, carries parryability on **shape**, adds per-class audio, keeps the tint as reinforcement. URP decal projectors rejected (mobile depth prepass cost; `m_RendererFeatures: []` today).

**ADR-0001 and ADR-0003 should be treated as one decision.**

---

## Engine and pipeline

| Decision | Value | Notes |
|---|---|---|
| Engine | Unity 6 LTS `6000.5.3f1` | |
| Render pipeline | URP 17.5.0, mobile quality tier | `Mobile_RPAsset`, `Mobile_Renderer` |
| SRP Batcher | Enabled (`m_UseSRPBatcher: 1`) | Per-instance `Material` copies still batch; **`MaterialPropertyBlock` breaks batching** |
| MSAA | Disabled (`m_MSAA: 1`) | Revisit — aliasing is more visible at the closer camera |
| Shadow atlas | 256×256 | Very tight; see `docs/BACKLOG.md` B17 |
| Platforms | iOS + Android, landscape only | Owner performs all final builds |
| Language | C#, `Boxhead.*` namespaces | Legacy root; do not rename opportunistically |
| Assemblies | One (`Assembly-CSharp`) + trivial `StatSystem.asmdef` | No test assemblies exist |

---

## Approved packages

From `Packages/manifest.json`, reconciled against `docs/PROJECT_CONTEXT.md`.

| Package | Version | Status |
|---|---|---|
| `com.unity.cinemachine` | 3.1.7 | In use — camera rig |
| `com.unity.render-pipelines.universal` | 17.5.0 | In use |
| `com.unity.inputsystem` | 1.19.0 | In use — `PlayerInput` + on-screen controls |
| `com.unity.ai.navigation` | 2.0.14 | In use — runtime NavMesh bake |
| `com.unity.test-framework` | 1.7.0 | **Installed, unused** |
| `com.unity.timeline` | 1.8.12 | Installed; cutscenes use video playback, not Timeline |
| `com.unity.visualscripting` | 1.9.12 | No known use |
| `com.coplaydev.unity-mcp` | local file ref | Editor automation |

**No new third-party Unity or Asset Store package may be installed without explicit owner approval.**

### Asset sources

| Source | Status |
|---|---|
| Meshy — characters, weapons | Approved (`PROJECT_CONTEXT.md`) |
| Low Poly Mega Pack / Polyworks (Off Axis Studios) | Approved (`PROJECT_CONTEXT.md`) |
| SimpleTown | **Present in tree, in no approval record** |
| ExplosiveLLC (RPG Character Mecanim, SuperCharacterController) | **Present in tree, in no approval record** |
| Polylised — Medieval Desert City | **Present in tree, in no approval record** |

Reconciliation is `docs/BACKLOG.md` D6.

---

## Performance budget

Recorded in full in `docs/TECHNICAL_DESIGN.md` §3. Target: stable 60 FPS on 3–4-year-old iOS/Android, with **sustained thermal behaviour over a full 10–15 minute run** as the real acceptance criterion.

Retained: < 100 draw calls · < 300k scene triangles · zero steady-state GC allocation (currently honoured — no LINQ anywhere in the codebase).

Added: **< 150 MB texture memory per room** and **< 200 MB download size** — both currently at risk. Texture import settings cap essentially everything at 2048 with no platform overrides, and `StreamingAssets/Cutscenes/` ships 326 MB of video for a feature now scoped to boss intros only.
