# BoxForged — Claude Code Project Guide

BoxForged uses the reusable Unity AI Game Studio in `.claude/`. The reusable studio lifecycle and agent/skill behavior are loaded from `.claude/rules/studio-core.md`. This file contains only BoxForged-specific rules that must survive across sessions.

## Project identity

- **Game:** BoxForged
- **Engine:** Unity 6 LTS (`6000.5.3f1` currently recorded)
- **Render pipeline:** URP, mobile quality tier
- **Language:** C# with namespaces
- **Git remote:** `git@ghtm:skyionblue/BoxForged.git`
- **Unity project path:** `BoxForged/BoxForged/` relative to repository root
- **Legacy namespace:** existing C# uses `Boxhead.*`; do not rename opportunistically

Read `docs/PROJECT_CONTEXT.md` before significant implementation, architecture, asset, level, input, camera, or MCP work.

## Current lifecycle state

BoxForged is in **Production**. Discovery was locked 2026-08-18 ("Lock discovery and begin pre-production"); production was authorized 2026-08-19 ("Start Sprint 0"). See `docs/CREATIVE_STATE.md` §Discovery lock status and `docs/SPRINT.md` §Authorization record. The three Phase 2+ narrative beats listed as open in `CREATIVE_STATE.md` do not block production. Preserve completed work unless an accepted decision explicitly supersedes it. "V4 Sprint 1"/"Sprint 2" refer to the retired pre-discovery V4 numbering and should not be resumed.

- Maintain creative decisions in `docs/CREATIVE_STATE.md` as CANON / WORKING / OPEN / REJECTED.
- For narrative work, use the current `story-room` and `narrative-discovery` skill. Do not let prose generation silently establish canon.

## Critical Git approval rule

**Never create a git commit without explicit owner approval.**

Before any `git commit`:
1. Show what changed and why.
2. Show the proposed commit scope/message.
3. Wait for an explicit instruction such as `commit` or `yes, commit`.
4. Only then commit.

This overrides generic studio commit behavior. Branch creation is allowed. Never merge, force-push, or delete remote branches without explicit approval.

## Owner interaction

The owner is still learning Unity. When manual Editor steps are required:
- use plain language;
- give concrete click/drag/menu steps;
- do not assume familiarity with Unity-specific concepts;
- prefer MCP automation when it is safe and available, but explain what was changed.

## Asset authority

Use the **current** reusable `asset-pipeline` skill and `asset-engineer` agent. Old `.claude-ORIG` agents/skills are retired and must not be invoked or treated as authoritative.

### Model orientation correction

Legacy orientation assumptions are invalid. In particular:
- never assume all Meshy models face `-Z`, `+Z`, or any universal forward axis;
- never apply a universal `(0, 180, 0)` prefab/model-child rotation;
- never assume one source up-axis or one FBX axis recipe proves gameplay orientation;
- never stack Blender, FBX, importer, and prefab rotations as compensating fixes without diagnosing the source.

For every new or suspect model, inspect and validate the complete source -> Blender -> export -> Unity -> animation/gameplay chain. Fix the earliest layer that is actually wrong. Record only verified asset-specific exceptions.

Project-specific asset sources, staging paths, scale/import expectations, and MCP workflow are documented in `docs/PROJECT_CONTEXT.md`.

## Architecture authority

Existing architecture contracts in `docs/PROJECT_CONTEXT.md` are preserved unless Discovery/Pre-production explicitly approves a change. Do not redesign stable systems merely because a different generic pattern is possible.

For new architecture or a material change to an existing contract:
1. use `technical-director`;
2. record the decision in `docs/TECHNICAL_DECISIONS.md` / ADR when appropriate;
3. implement with `unity-gameplay-engineer` only after the design is accepted;
4. run `code-reviewer`, QA, and performance review as appropriate.

## Level generation

BoxForged levels are data-driven, not hand-authored as independent scenes. `LevelBuilder` consumes ScriptableObject level data for spawn points, props, waves, and zone configuration. New level-generation architecture must go through `technical-director` and then `unity-gameplay-engineer`; do not use retired `Plan` or `unity-senior-developer` agents.

## Builds and releases

The owner performs final builds and deployments. Studio agents may prepare release-readiness checks but must stop before final Android/iOS build or store deployment unless explicitly instructed otherwise.
