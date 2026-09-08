---
name: reference-world2-perf-shape
description: Backyard_Dojo (World 2) is built the OPPOSITE way to World 1 — scene-authored static geometry, 47 stockade wall modules, one resident boss — so World 1's perf reasoning does not transfer. Plus how to reach World 2 for profiling at all.
metadata:
  type: reference
---

Verified 2026-09-08 against the built `Backyard_Dojo.unity` while extending `docs/PERFORMANCE_PROFILING.md`. **Do not carry World 1's perf reasoning across** — the two worlds fail for different structural reasons.

**The static-batching trap is World-1-only. World 2 inverts it.** [[reference-culdesac-room-perf-shape]] records that `BatchingStatic` flags are inert because `LevelBuilder` instantiates props at runtime. That is true for World 1 and **false for World 2**: `WeaponDropTableSO_Backyard_Dojo.envProps` is **empty** (the yard is hand-dressed, ADR-0005 §6.6), and **51 scene objects carry the Batching Static bit** (`m_StaticEditorFlags` 14 / 20 / 30, all include bit 4) against World 1's one. `ProjectSettings.asset:565-571` has `m_StaticBatching: 1` for iPhone and Android. So World 2 genuinely gets build-time static batching and World 1 structurally cannot. Expect World 2's draw-call count to beat World 1's 205 for that reason; if it doesn't, that's the finding.

**The headline draw-call risk is 47 identical `BD01_WallModule` instances** — the stockade, one prefab, one shared-atlas material, resident in all three zones (one continuous scene). Against a whole-scene < 100 draw-call budget that is ~47% spent on one prop. ADR-0006 §5.1 predicted exactly this ("47 naive → 35 with §5.2") and budgeted `BD-01-Long` as the refund; **B116 lists BD-01-Long under *Not built***, so the built scene sits at the ADR's own worst case. It is also the project's best instancing candidate, which makes it the cleanest test of B112's SRP-Batcher-at-zero.

**The Editor-baked NavMesh is discarded at runtime.** `Systems/LevelBuilder.cs:73-96` calls `NavMesh.RemoveAllNavMeshData()` then re-bakes with a runtime `NavMeshSurface` (`CollectObjects.All`, `PhysicsColliders`). So B122's Editor-bake figures (1 139 verts / 485 tris / 1 216.1 m²) are a *prediction* of the runtime bake, never a measurement of it. Two consequences: (1) **B128** (no bake bounds — 1 216 m² baked against ~318 m² playable) is filed P3 as pathing/memory but is also a **scene-start-hitch** input against the ≤ 500 ms budget, which nothing records; (2) **B127**'s "the legacy bake ignores `NavMeshModifier`" analysis is about the Editor bake — the runtime bake *is* a `NavMeshSurface`, so the two may disagree and the runtime one ships. Flag both to `technical-director`; neither is a performance-engineer call.

**One boss resident, not two.** `pfb_enemy_grasscutter` is a pre-placed scene instance forced inactive by `ZoneDirector`, so its textures are loaded in all three zones (`Grasscutter_BaseColor.png` = 30.9 MB on disk, TDD §3.4). `pfb_enemy_spincycle` has **zero** references in `Backyard_Dojo.unity` — the worlds are separate top-level scenes. Never budget World 2 as "two bosses' worth of assets".

**Zone rosters (read the assets, not the ADRs).** ADR-0006 §3.4's 7-spawn roster with Leaf Pile Lurkers is a worked example that predates the build; the Lurker was cut 2026-09-02. Actual: Zone 0 = 5 Gnome Grunt, cap **4**; Zone 1 = 4 Gnome + 1 Skeptic + 1 Crane Duelist (6 spawns), cap **4**; Zone 2 = `spawnPoints: []`, cap 1, `bossOwnedWin: 1`. Note World 2's **zone 0 also caps at 4**, unlike World 1's zone 0 (cap 3) — there is no cheap warm-up zone in World 2.

**Getting to World 2 at all is a blocker for any on-device pass.** Nothing reads `GameManager.ZoneStartScene[1]`: `WorldMapScreen.cs:117` hardcodes the never-existent `"TownSquare_Room1"`, and `MetaScreen.OnContinue()` deliberately restarts zone 0. Several docs claim zone 1 is "reachable now" on the strength of the `ZoneStartScene[1]` correction alone — they are wrong. To profile World 2 the owner must reorder `Backyard_Dojo` to build index 0; the both-worlds-in-one-session test is simply unrunnable until the routing is fixed. Full write-up in `docs/PERFORMANCE_PROFILING.md` §1.1.

See [[reference-perf-budgets]] and [[feedback-evidence-standard]].
