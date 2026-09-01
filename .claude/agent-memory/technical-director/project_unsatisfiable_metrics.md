---
name: project-unsatisfiable-metrics
description: BoxForged has twice written acceptance metrics into accepted ADRs that measure states the runtime cannot produce — check a metric is satisfiable and mechanism-accurate before it becomes a gate.
metadata:
  type: project
---

**Before writing a numeric acceptance criterion into an ADR, prove it is satisfiable and that it measures a state the runtime can actually produce.** This project has shipped two that were not, both inside ADR-0005 §3, both caught only after the geometry was built and playtested (resolved 2026-09-01 by ADR-0006).

**Case 1 — "boss arena minimum clear circle r ≥ 8.5 m."** Written as the *largest inscribed obstacle-free circle*, generalized from World 1's measured 8.44 m. But that measurement was taken with the arena **empty** (both wagons cleared). For radius `R` and a central obstacle of radius `r_t` the quantity is `(R − r_t)/2`, so the rule demanded `R ≥ 17.35 m` — a **34.7 m arena** — to accommodate a 0.70 m tree trunk, while the same ADR mandated a tree at the centre and rejected 36 m as failing the camera 2×. **Unsatisfiable by any arena with a central feature of any size.** A measurement's preconditions travel with it; dropping them turns a fact into an impossible requirement.

**Case 2 — "combat radius per zone ≤ 9 m."** Read as the enclosing circle of a zone's *whole spawn roster*. But `Systems/RoomManager.cs` `TrySpawnNext` advances a **monotonic** `_nextSpawnPointIndex` and `OnSpawnedEnemyDied` refills exactly **one** slot per death, so the live set is only ever a **contiguous window of ≤ `maxConcurrentEnemies` entries in array order**. The whole roster is never alive. World 1's shipped 59.5 m street does not satisfy the literal rule either — it was documented but unenforced. Worse, the false reading was *actively harmful*: it left World 2's zone 1 with 0.04 m of headroom (8.96 m of 9.0) and was the stated reason the zone could not grow. Measured correctly the worst window was 7.79 m, and the zone then grew 54% in area and still passed.

**The tell in both cases:** the metric was an *aggregate over authored data* rather than a property of a state the game can be in. Both fixes came from reading the mechanism (`RoomManager`'s spawn loop; how a clear circle is actually computed) rather than from re-deriving the number.

**Also check the derivation still applies, not just the arithmetic.** ADR-0005 justified a 17 m arena as "matching the camera's 16.8 m visible lateral width." That equates a *diameter* with a **follower** camera's *per-position* visible width — only meaningful if the player stands at the centre, which an 8 m boss dash guarantees they do not. The limit had already been exceeded, so "the arena must not grow" was defending a wall that was not there. Separately, the number that actually predicted how an arena *feels* was not any radius but **boss longest-traversal ÷ arena diameter** (World 1's playtested SpinCycle: 0.284; World 2's budgeted Grasscutter: 0.471 in a same-sized arena).

**Why:** on this project an accepted ADR's metric becomes a gate that other agents refuse to override on their own authority — correctly. `zone-layout-spec.md` §4.6 hit Case 1, did the algebra, and escalated rather than fudging, which cost a full design pass and a build. A bad metric therefore does not just misinform; it blocks.

**How to apply:** for every numeric criterion you write, (a) construct one concrete configuration that satisfies it, (b) name the runtime mechanism it is measured over and check that state is reachable, (c) carry the preconditions of any measurement you generalize from, and (d) re-check shipped work against it — if World 1 does not satisfy your new rule, either the rule or the claim that World 1 shipped correctly is wrong. Prefer metrics computable from `RoomDataSO` + scene geometry so they can go into `LevelBuilder` validation; ADR-0006's M1/M2 are specified for that and are **not yet implemented**.

Related: [[project-docs-drift-from-code]], [[reference-room-scale-calibration]], [[project-roommanager-zone-mechanism]]
