# Orientation and Scale Validation

Use this workflow instead of relying on generator-specific axis assumptions.

## 1. Inspect before changing

Record:
- raw file format and source tool/export type;
- Blender object location/rotation/scale;
- mesh dimensions and bounding box;
- which local direction the model visually faces;
- which axis represents height;
- armature/root-bone transforms for rigged assets;
- current unit scale.

Do not infer forward from file format alone.

## 2. Establish the gameplay contract

Define what Unity expects for this asset in the actual prefab/controller:
- prefab root rotation;
- movement/controller forward direction;
- camera orientation;
- socket/weapon forward conventions;
- root-motion expectations;
- ground plane and expected real-world dimensions.

The gameplay contract is the truth; a visually correct model that moves backward is not validated.

## 3. Export one controlled candidate

Create a reversible checkpoint/source copy. Export one candidate without adding compensating prefab rotations. Use the project's currently approved FBX/glTF export settings, but treat them as a starting configuration rather than proof of correctness.

## 4. Validate in Unity

Place the imported model in a clean validation scene with:
- world-axis gizmos/reference arrows;
- a ground plane;
- known-size reference geometry;
- a forward-facing marker;
- an Animator test for rigged characters;
- representative socket/weapon attachment when applicable.

Verify:
1. correct height/size;
2. upright orientation;
3. visual forward matches controller/gameplay forward;
4. feet/base contact ground at expected origin;
5. animations move/turn in expected direction;
6. root motion does not introduce unintended rotation/scale;
7. sockets and colliders align.

Capture a screenshot when MCP/editor tooling supports it.

## 5. Diagnose instead of compensating

If incorrect, identify the layer causing it:
- source object transform;
- armature/root bone;
- Blender object transform;
- export axis conversion;
- Unity importer;
- prefab hierarchy;
- controller/model-forward contract.

Fix one layer at a time and revalidate. Avoid "it looks right" child rotations that conceal an upstream problem.

## 6. Record verified exceptions

If a particular generator/export preset consistently requires an exception, record the exact source preset/version and the validation evidence in project-specific documentation. Do not promote an asset-specific exception into the reusable studio standard.
