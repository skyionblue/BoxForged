---
name: mobile-performance
description: Mobile performance budgeting, profiling, and optimization rules for Unity. Use when creating runtime systems/assets, reviewing rendering/physics/loading, investigating frame drops, or preparing Android/iOS performance targets.
---

# Mobile Performance

Default target: stable 60 FPS on representative 3-4-year-old iOS/Android hardware with graceful degradation.

During pre-production record project-specific budgets for: CPU main/render thread frame time, GPU frame time, memory peak/steady state, managed allocations per frame, draw calls/batches, triangles/vertices, texture memory/resolution, shader complexity, physics cost, scene/load time, package size, battery/thermal behavior.

Rules:
- Profile representative gameplay on device before and after meaningful optimization.
- Target effectively zero avoidable steady-state managed allocations in hot gameplay loops.
- Pool only objects proven frequent/expensive enough to benefit.
- Use LODs, occlusion/culling strategy, texture compression, mipmaps, batching/instancing, lighting/shadow budgets, and shader variants intentionally.
- Avoid unnecessary physics bodies/colliders and high-frequency queries.
- Load large content asynchronously and define transition/loading UX.
- Prefer quality tiers/dynamic degradation over hard failure on lower-end devices.
- Document profiling scenario and evidence; never call something 'optimized' without a measurable target.
