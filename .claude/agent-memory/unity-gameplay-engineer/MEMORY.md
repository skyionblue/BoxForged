# Memory Index

- [Sprint 0 authorization gate](project_sprint0_gate.md) — Sprint 0 is AUTHORIZED (2026-08-19); always verify SPRINT.md's status line fresh rather than trusting cached belief. Includes B27/B59 camera offset history and a real AspectAdaptiveCameraFraming distance bug found 2026-08-20.
- [Room1_v2 build status](project_room1_v2_build.md) — ADR-0002 RoomDataSO architecture + CulDeSac_Room1_v2 scene: what's built, saloon sign removal, forge wiring/testing, RunStartUI+run-end-screen blocking screenshots, reload techniques.
- [DontDestroyOnLoad root rule](feedback_dontdestroyonload_root.md) — AudioManager/AttackTelegraphService (and similar) must be scene-root GameObjects, never nested in a tidy "[Managers]" group.
- [Unity MCP scene save safety](feedback_unity_mcp_scene_save_safety.md) — manage_scene "save" always targets the active scene (no save-as); no discard override for close/load; disable roots instead.
