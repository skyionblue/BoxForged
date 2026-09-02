# Memory Index

- [Lifecycle & ADR status](project_preproduction_gate.md) — World 1 + most of World 2 shipped; SPRINT.md lagged 2 phases; CLAUDE.md's "Discovery" line is stale — never agent-edit it.
- [Boss-intro camera contract](reference_boss_intro_camera.md) — ADR-0008's three invariants (I2 horizontal + burial term, I3 = bury the subject), the 63–68% fill band, and 5 measurement traps.
- [NavMesh baking](reference_navmesh_baking.md) — legacy Navigation-window bake, not NavMeshSurface. A floorless bake SUCCEEDS silently; check area, not just that an asset appeared.
- [Cherry tree position is CANON](project_cherry_tree_position_is_canon.md) — owner put it on the north rim deliberately; never propose re-centring. A doc/diff contradiction proves the records wrong, not the code.
- [Metrics that measure impossible states](project_unsatisfiable_metrics.md) — 3 unsatisfiable clauses in accepted ADRs, incl. a conditional grant's fallback. Prove the escape hatch too.
- [Room-scale calibration](reference_room_scale_calibration.md) — playtested dimensions the owner accepted, plus the 3 diagnostics that explain "feels too small" (free floor per sub-space, dash÷diameter, narrow-axis sum).
- [RoomManager is not a scene loader](project_roommanager_zone_mechanism.md) — multi-zone-in-one-scene works and is now the default; the scene-load half is dead code with no targets.
- [Measuring the city scene](reference_measuring_city_scene.md) — ENV root is rotated: renderer AABBs and raw world coords both mislead. Rasterize from mesh footprints.
- [Asset weight — CORRECTED by device measurement](project_asset_weight_risk.md) — texture-residency fear disproved (41.2MB); real cost is draw calls/tris and SRP Batcher at zero.
- [Docs drift from code](project_docs_drift_from_code.md) — camera was documented wrong for a whole phase. Verify against prefab/asset YAML, not PROJECT_CONTEXT.
