# BoxForged Project Context

This document preserves BoxForged-specific technical and workflow knowledge that remains useful after retiring the old `.claude-ORIG` agents and skills. It is project context, not reusable studio behavior.

## Repository and project layout

- Unity project: `BoxForged/BoxForged/`
- Raw incoming assets are staged outside the Unity project under `boxhead/models/` before being processed into `Assets/_Project/...`.
- Existing C# namespaces retain the legacy `Boxhead.*` name for now.

Namespace mapping currently recorded:

| Folder | Namespace |
|---|---|
| Core/ | `Boxhead.Core` |
| Player/ | `Boxhead.Player` |
| Enemy/ | `Boxhead.Enemy` |
| Systems/ | `Boxhead.Systems` |
| UI/ | `Boxhead.UI` |
| Editor/ | `Boxhead.Editor` |

## Asset sources

**Updated 2026-08-19 — owner reconciled third-party content directly in the project.**

- Characters: Meshy
- Weapons: Meshy
- Environment props: **Cartoon City** (publisher: Hayq Art) — `Assets/Hayq Art/Cartoon City/`. Replaces Low Poly Mega Pack - Polyworks.
- Character animation: RPG Character Mecanim Animation Pack FREE (ExplosiveLLC) — `Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/`

**Removed 2026-08-19:** Low Poly Mega Pack - Polyworks (`Off Axis Studios/`), SimpleTown, Polylised - Medieval Desert City. Confirmed via GUID cross-reference against every scene and prefab under `Assets/_Project/` that none of these three packs were actually referenced anywhere in the shipped project — safe removal, nothing broken. Note: many older planning documents (`docs/v4/levels/World1/...`, `docs/design/GDD-V2.md`, older sprint docs) still describe Polyworks as the env-prop source for World 1 — those are now stale and need reconciliation whenever World 1 is actually rebuilt under the new pack. Tracked in `docs/BACKLOG.md`.

**Status unresolved:** `ExplosiveLLC/SuperCharacterController` is still present in the project, unreferenced by anything under `_Project/`, and was not explicitly addressed by the owner's cleanup pass. Not removed — owner did not ask for its removal, only listed what to keep. Flag for a future pass if it should go too.

Third-party packages/assets that are not already approved require owner approval before being added.

## Asset import requirements

Treat raw model orientation, units, axes, pivots, root transforms, and armature transforms as untrusted until inspected.

Known project expectations that remain useful:

- Target imported model folders: `Assets/_Project/Models/<category>/<ModelName>/`.
- Extract embedded textures into `Assets/_Project/Models/<category>/<ModelName>/Textures/` when extraction is appropriate.
- Remap materials to project URP materials in `Assets/_Project/Materials/`.
- For Meshy FBX imports, start with `ModelImporter.globalScale = 1` and `ModelImporter.useFileScale = true`; verify actual dimensions in Unity rather than applying a global scale workaround.
- Weapons may use runtime holder/configuration scale for gameplay fit; do not repair source-unit problems with arbitrary model import scale.
- Verify human characters against a known-size reference in Unity; approximately 2 Unity units is a useful sanity check for human-sized characters, not a universal rule for all assets.

### Required model validation

For every new or suspect model:

1. Preserve the raw source file.
2. Inspect dimensions, transforms, visual forward/up, pivot/origin, armature/root transforms, materials, and triangle count before changing anything.
3. Define the intended Unity contract: real-world size, gameplay forward, up, feet/base location, pivot, root motion, sockets, collider expectations, and animation needs.
4. Make only the Blender normalization changes that inspection proves are needed.
5. Export a controlled candidate.
6. Validate in Unity using world axes, a ground plane, known-size reference geometry, animation/root motion, collider alignment, sockets, and representative gameplay movement.
7. If incorrect, identify whether the error is source, Blender transform, exporter conversion, Unity importer, root bone/armature, animation, or prefab hierarchy.
8. Fix the earliest correct layer. Do not add compensating rotations by habit.
9. Record any verified asset-specific exception; do not generalize it to all Meshy or all FBX assets.

### Historical orientation rules that are explicitly rejected

Do not reuse these retired assumptions from `.claude-ORIG`:

- “Meshy models always face `-Z`.”
- “CharacterModel must always have local rotation `(0, 180, 0)`.”
- “All Meshy FBX uses one fixed source up-axis.”
- “A single FBX `axis_forward`/`axis_up` pair proves visual/gameplay forward for every source.”

Any prior document containing those claims is historical only.

## Mobile performance budget

The current BoxForged budget was created for modern mobile hardware. Revisit it during pre-production if the target-device strategy changes.

- Draw calls: `< 100`
- Total scene triangles: `< 300k`
- Player character: roughly `20k` triangles
- Standard enemies: roughly `10-12k` triangles each
- Weapons: `< 1,000` triangles each
- Environment props: roughly `200-600` triangles each
- Gameplay GC allocation target: zero per frame

Runtime practices currently expected:

- no LINQ in hot paths such as `Update` or frequently-running coroutines;
- avoid boxing in hot paths;
- use `CompareTag` rather than string tag equality;
- avoid repeated `renderer.material` access in hot paths; cache owned material instances and destroy them appropriately;
- avoid repeated allocation of identical yield instructions in hot recurring coroutines when a cached instance is semantically safe;
- validate with profiling rather than assuming a micro-optimization is useful.

## Existing architecture contracts

These contracts describe planned/current BoxForged behavior and should not be silently changed.

### Communication

Use event-driven communication where appropriate. Existing code uses `System.Action` events. Subscribers must unsubscribe reliably during lifecycle teardown.

### Data

Game configuration is primarily ScriptableObject-driven where that data benefits from designer editing/reuse. Existing examples include `BoxData` and `SoundData`.

### Existing singleton usage

`GameManager` and `AudioManager` currently use singleton-style access. `AudioManager` persists across scene reloads; `GameManager` is scene-scoped. Do not introduce more global singleton managers by default.

### Component requirements

Use `[RequireComponent]` when a MonoBehaviour unconditionally requires another component obtained on the same GameObject.

## Key script contracts

These were recorded as the planned V4 architecture to preserve across the migration unless explicitly redesigned.

### `PlayerStats` — `Boxhead.Player`

- `int CurrentHealth`, `int MaxHealth`, `bool IsDead`
- `event Action<int, int> OnHealthChanged`
- `event Action OnDeath`
- `event Action<BoxData> OnBoxChanged`

### `CombatController` — `Boxhead.Player`

- `CombatState State`: `Idle`, `Attacking`, `Dodging`, `Parrying`, `Countering`, `Staggered`
- `TryReceiveAttack(int damage, bool parryable = true, GameObject attacker = null)` returns `AttackResult` (`Hit`, `Dodged`, `Parried`)
- callers should use the method contract rather than inspecting internal state to decide hit outcome
- relevant events include dodge, parry success, counter-window open/close, counter strike, and stagger

### `EnemyStats` — `Boxhead.Enemy`

- `int AttackDamage`, `bool IsDead`
- `event Action OnDeath`

### `BasicEnemyAI` — `Boxhead.Enemy`

- state progression currently planned around `Idle -> Chase -> WindUp -> Attacking -> Staggered -> Dead`
- counter-hit response is tied to the counter-strike event, not merely counter-window closure

### `GameManager` — `Boxhead.Core`

- states: `Playing`, `Won`, `Lost`
- scene-scoped manager
- restart reloads the active scene
- win handling currently depends on enemy completion and/or room flow

### `AudioManager` — `Boxhead.Core`

- persistent across scene reloads
- currently planned around a small pooled set of AudioSources and event-driven playback

### UI

- Game-over flow pauses timescale and audio and must restore both before reload.
- Keep the game-over panel inactive in the scene rather than creating activation/deactivation loops during initialization.

## Input

Uses Unity New Input System with `PlayerInput` and mobile on-screen controls.

Current action mapping intent:

| Action | Gamepad | Mobile |
|---|---|---|
| Move | left stick | virtual joystick |
| Attack | south button | on-screen button |
| Dodge | east button | on-screen button |
| Parry | west button | on-screen button |
| Jump | north button | on-screen button |

## Camera

**Corrected 2026-08-19 (was previously wrong for an entire project phase — see `docs/BACKLOG.md` B21).** Accepted spec going forward (ADR-0001; **implemented 2026-08-19, Sprint 0 complete** — ADR-0001's own on-device validation checklist is still pending): fixed-rotation follow, no aim/look-at component, pitch 36°, vertical FOV 45°, height 5.5m, `FollowOffset (0, 5.5, -7.57)`, yaw locked to 0°, `CinemachineHardLookAt` removed. Check `docs/SPRINT.md` for current implementation status before assuming this has landed in the prefab.

The previously documented `(0, 12, -8)` offset with a `CinemachineHardLookAt` was never actually what the prefab contained; the real prior values were `(7.879929, 11, -10)`, FOV 40, with an undocumented ~−38.2° yaw that silently rotated player controls. Prefer citing the live `pfb_CM_FollowCam.prefab` values over prose in this document going forward.

This camera change is coupled to a new attack-telegraph system (ADR-0003) — see `docs/adr/` and `docs/TECHNICAL_DESIGN.md` §2 and §4.

## Level generation

Levels are data-driven rather than hand-built as unique scenes.

- Level configuration is stored in ScriptableObject assets.
- A `LevelBuilder` instantiates enemy spawn points, props, wave configuration, and zone content at runtime.
- New level content should primarily be new data assets plus approved reusable builder behavior.
- NavMesh, lighting, overlap/spawn validation, and deterministic generation concerns belong in the builder architecture.

## Unity MCP workflow

The project uses MCP for Unity (`mcpforunityserver`). Tool names may evolve; inspect the connected MCP server rather than assuming unavailable names.

Known workflow that has been useful:

1. Confirm the Unity editor/MCP server is connected and ready before scene automation.
2. Prefer batch operations for coherent multi-step scene changes when the server supports them.
3. After script changes, wait for Unity compilation to finish and inspect console errors before continuing.
4. Capture a Unity screenshot/scene view after meaningful visual changes when supported.
5. For prefab wiring, use prefab-safe editing APIs/workflows rather than mutating an unloaded asset as if it were a live scene object.
6. Prefer serialized-property assignment for serialized fields during editor automation.

Historical `execute_code` quirks recorded in the old project should be revalidated against the currently connected MCP version before being treated as universal behavior.

## Blender MCP workflow

The project uses the `blender-studio` MCP server.

General practices worth preserving:

- inspect first, modify second;
- prefer named MCP tools when a suitable tool exists;
- use arbitrary Python only when the named tool surface does not cover the operation;
- use transactions/checkpoints for risky multi-step modifications when supported;
- inspect object/mesh/scene statistics before optimization decisions;
- recalculate normals, merge duplicate vertices, apply modifiers/transforms, unwrap/pack UVs, rig, bake, and export only when required by the current asset;
- validate results visually before export and again in Unity.

Do not preserve old Blender axis/orientation recipes as global rules. Export settings must be validated with the current asset and current Unity importer behavior.

## Branching history

The prior V4 workflow used `main` as the integration branch and `feature/v4-*` sprint branches. V4 Sprint 1 was completed before the project returned to Discovery. Do not automatically resume the old Sprint 2 plan.

**Updated 2026-09-02:** the "reevaluate after Discovery is locked" condition has been satisfied. Discovery was locked 2026-08-18 (`docs/CREATIVE_STATE.md` §Discovery lock status) and production authorized 2026-08-19; roadmap and sprint work was replanned accordingly and lives in `docs/ROADMAP.md` and `docs/SPRINT.md`. The "Sprint 0" numbering used there is a **new series** and is unrelated to the retired V4 Sprint 1/Sprint 2 numbering above. Note that `CLAUDE.md`'s "Current lifecycle state" section still describes the pre-2026-08-18 Discovery state and conflicts with every other record — flagged for the owner in `docs/SPRINT.md` §Open for owner decision.
