# Memory Index

- [Lifecycle & ADR status](project_preproduction_gate.md) — Sprint 0 shipped; ADR-0001/2/3 status contradicts across docs; ADR-0004 Accepted but unimplemented.
- [RoomManager is not a scene loader](project_roommanager_zone_mechanism.md) — multi-zone-in-one-scene already works; scene-specific staging belongs in an OnRoomActivated subscriber.
- [Measuring the city scene](reference_measuring_city_scene.md) — ENV root is rotated: renderer AABBs and raw world coords both mislead. Rasterize from mesh footprints.
- [Asset weight is the dominant risk](project_asset_weight_risk.md) — 2.6GB textures at 2048 w/ no platform overrides + 326MB retired video. Optimize assets before code.
- [Docs drift from code](project_docs_drift_from_code.md) — camera was documented wrong for a whole phase. Verify against prefab/asset YAML, not PROJECT_CONTEXT.
