---
name: unity-testing
description: Unity testing and validation workflow. Use for feature completion, bug fixes, regressions, scene/prefab validation, EditMode/PlayMode tests, or release readiness.
---

# Unity Testing

Use a layered strategy:
- Plain C# unit tests for deterministic domain logic.
- EditMode tests for configuration, editor-safe integration, serialization, validators, and asset contracts.
- PlayMode tests for lifecycle, scene behavior, physics/trigger flows, character interactions, reset/retry, and end-to-end gameplay slices.
- Static/editor validators for missing references, duplicate IDs, invalid layers/tags, scene configuration, forbidden dependencies, and asset-budget violations where practical.
- Manual test checklist for feel, visuals, touch ergonomics, device performance, audio/haptics, and scenarios automation cannot reliably judge.

For bugs, capture a failing reproduction test when practical before the fix. Tests must be deterministic and clean up created objects/state. Report what ran and what could not be verified.
