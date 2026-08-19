---
name: asset-pipeline
description: Meshy-to-Blender-to-Unity 3D asset pipeline and technical-art standards. Use when creating, cleaning, rigging, importing, optimizing, or validating characters, props, environments, animations, materials, colliders, LODs, scale, pivots, or orientation.
---

# Meshy -> Blender -> Unity Asset Pipeline

Treat generated/source asset coordinate systems as untrusted input. Never assume every Meshy export uses the same up axis, forward axis, unit scale, object rotation, or armature transform.

Before generation or import, define the asset spec: gameplay role/readability, real-world dimensions, polygon budget, material slots, texture sets/resolution, rig/animation requirement, pivot/origin, collision, LODs, and naming.

## Pipeline

1. Generate/source the asset without assuming generated topology, transforms, units, or orientation are game-ready.
2. Inspect the raw asset before changing it. Record dimensions, object transforms, mesh bounds, armature transforms, visual forward direction, and up direction.
3. In Blender, validate mesh cleanup, normals, topology/deformation, UVs, materials, rig weights, animation, pivot/origin, scale, and LOD candidates. Use configured Blender MCP when available.
4. Normalize only what inspection proves needs normalization. Do not apply a memorized 90/180-degree rotation or source-specific axis recipe globally.
5. Export to a staging path while preserving the source-art file separately.
6. In Unity, validate the imported asset in a dedicated validation scene/prefab before production use: dimensions, feet/base on ground, visual forward versus gameplay forward, animation root motion, collider alignment, weapon/socket alignment, normals, materials, and camera-facing readability.
7. If orientation is wrong, diagnose whether the error originates in source transforms, Blender transforms, FBX export conversion, Unity importer settings, armature/root bone orientation, or prefab hierarchy. Fix the earliest correct layer; do not stack compensating rotations blindly.
8. Capture before/after evidence with Blender/Unity screenshots when available and record any asset-specific exception in project documentation.
9. Configure Unity importer scale, mesh compression where safe, read/write flags, material remapping, texture compression/mipmaps, rig/avatar, clip import, LODGroup, colliders, prefab structure, and address/loading strategy.
10. Test in representative lighting/gameplay and profile on the target device class.

## Hard rules

- Never hard-code `CharacterModel.localRotation = (0, 180, 0)` as a universal fix.
- Never state that all Meshy FBX files face a specific axis unless the current asset was inspected and verified.
- Never stack a Blender rotation, FBX axis conversion, Unity importer correction, and prefab child rotation without proving each is required.
- Do not use a scale workaround to repair an axis/orientation problem.
- Prefer transforms of `(position=0, rotation=identity, scale=1)` for production prefab roots when achievable without damaging rig/animation data.
- For rigged characters, validate bind pose and animation after any transform/export change.
- Preserve raw source files so a bad normalization pass is reversible.

Read `references/orientation-validation.md` for the required diagnostic workflow when orientation, scale, root motion, sockets, or animation direction is in question.

Minimize material count and overdraw; size textures to screen-space need, not source-generator defaults.
