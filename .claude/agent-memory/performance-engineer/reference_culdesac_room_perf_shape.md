---
name: reference-culdesac-room-perf-shape
description: How Cul-de-Sac _v2 rooms are actually built (runtime-spawned props, inert static-batching flags, SRP Batcher is the real mechanism) and where per-enemy cost really sits.
metadata:
  type: reference
---

Structural facts about the `CulDeSac_*_v2` rooms — verified 2026-08-24 reviewing `CulDeSac_AmbushAlley_v2`. Re-check before quoting, but these are architectural, not incidental.

**Rooms are near-empty scenes.** A `_v2` room scene contains ~15 GameObjects and exactly **one renderer** (`Ground`, MeshFilter+MeshRenderer). The 4 `Boundary_*` objects are **BoxCollider-only, no renderers** — invisible walls, zero render cost. Exactly **one** light (realtime directional `Sun`, `m_Lightmapping: 4`). Everything else — every prop, pickup, workbench, cardboard pile, and enemy spawn marker — is `Instantiate`d at runtime by `LevelBuilder` under `_spawnRoot` from a `WeaponDropTableSO` + `RoomDataSO`. So a room diff between two `_v2` scenes is usually only the two SO references; **audit the data assets, not the scene**.

**The static-batching flags are inert — this is the trap.** `Assets/_Project/Prefabs/ENV/pfb_env_*.prefab` all carry `m_StaticEditorFlags: 12` (BatchingStatic + NavigationStatic), so the Inspector looks correct. But they are runtime-instantiated and `LevelBuilder` never calls `StaticBatchingUtility.Combine`, and static batching is a build-time operation for scene objects — so **zero static batching actually happens**. `Mobile_RPAsset.asset` also has `m_SupportsDynamicBatching: 0`. Do not report this as an emergency: `m_UseSRPBatcher: 1` and every ENV prop is single-renderer/single-material on the **same URP Lit shader**, so the SRP Batcher batches them well. SRP Batcher cuts per-draw CPU setup but does **not** reduce draw-call count. Net: draw calls ≈ renderer count, each cheap. `StaticBatchingUtility.Combine` on an env-prop-only subroot is the real lever *if* measurement shows draw calls are the bottleneck — scope it away from pickups/piles/spawn markers, which move or die.

**Per-enemy cost is wildly asymmetric, and the approved asset is the expensive one.** `pfb_enemy_wagonwheel_roller` (Room 1's only enemy, already perf-approved) has **no Animator and 2 transforms** — but its 3 textures are `maxTextureSize: 2048` on *every* platform with `overridden: 0`, and its Metallic map is wrongly `sRGBTexture: 1`. `pfb_enemy_skeptic_grunt` is a 27-transform Humanoid skinned character, but its 2 textures carry real Android/iOS overrides down to 1024 and correct sRGB. So Room 1's approved baseline dominates texture memory, while animated enemies dominate CPU. Never assume "already reviewed" means "cheap" — check which axis. Texture-import policy generally is tracked as BACKLOG B1; don't re-file it as new.

**Animator defaults to watch:** every animated enemy prefab in the project has `m_CullingMode: 0` (AlwaysAnimate) — project-wide, pre-existing. `m_ApplyRootMotion` is `0` on every animated enemy **except** `pfb_enemy_skeptic_grunt` (`1`), which is agent-driven and imports with `importAnimation: 0`. `GameObject.FindWithTag("Player")` in `Start()` is the established project-wide pattern in every enemy AI — not a per-enemy finding.

See [[reference-perf-budgets]] and [[feedback-evidence-standard]].
