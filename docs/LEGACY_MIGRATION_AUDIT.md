# Legacy `.claude-ORIG` Migration Audit

The old `.claude-ORIG` tree is retired. None of its agents or skills should be invoked by the current studio.

## Retained as project knowledge

The migration intentionally preserved useful BoxForged-specific information such as:

- repository/Unity project layout;
- commit-approval policy;
- legacy namespaces that still exist in code;
- raw asset staging path and asset sources;
- project folder/material/texture import conventions;
- measured mobile performance targets;
- existing gameplay architecture/API contracts;
- New Input System and camera choices;
- data-driven `LevelBuilder` architecture;
- useful Unity MCP and Blender MCP operating practices;
- branch/sprint history as historical context.

These facts now live in `CLAUDE.md` or `docs/PROJECT_CONTEXT.md`, not in legacy agents/skills.

## Explicitly rejected

The migration does **not** carry forward old agent/skill behavior, including:

- old routing to `unity-senior-developer`, `Plan`, `storyteller`, `blender-specialist`, or other retired agents;
- the old `unity-character-importer`, `asset-pipeline`, `level-design`, or profiling skills;
- universal Meshy orientation/up-axis assumptions;
- mandatory `CharacterModel.localRotation = (0, 180, 0)`;
- any rule that treats one FBX export axis pair as correct for every asset;
- conflicting Blender/Unity orientation recipes found in the old material.

## Rule for future salvage

Historical files may be inspected only to recover facts that can be independently validated against the current project. A legacy instruction is never authoritative merely because it previously existed.
