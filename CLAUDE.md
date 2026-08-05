# BoxForged — Claude Code Project Guide

## Project Overview

**BoxForged** is a mobile-first (iOS + Android) action game built in Unity 6 LTS with URP.
The premise: cardboard boxes become ninja masks and cowboy hats, household objects become weapons.
Setting: post-apocalyptic — the World Tree is the last source of cardboard; The Unimaginative have seized it.

**Engine:** Unity 6 LTS (6000.5.3f1)
**Render Pipeline:** URP (mobile quality tier)
**Language:** C# with namespaces
**Git remote:** `git@ghtm:skyionblue/BoxForged.git`
**Unity project path:** `BoxForged/BoxForged/` (double-nested inside the repo root)

> **Setup status:** This directory is a greenfield — the Unity project has not been created here yet. The plan is to set up a fresh Unity 6 project and port scripts and assets from the prior work preserved on the `v3/main` branch.

> **Name note:** The game was renamed from *BoxHead Ninjas* to *BoxForged*. The C# namespaces (`Boxhead.*`) retain the legacy name — namespace renaming is deferred to a future sprint. All folder names, the GitHub repo, and Unity ProjectSettings now reflect *BoxForged*.

---

## CRITICAL: Commit Approval Required

**Never create a git commit without explicit approval from the project owner.**

Before running any `git commit` command:
1. Show the user what changed and why
2. Wait for the user to say "commit" or "yes, commit" or similar explicit approval
3. Only then run `git commit`

This rule overrides any default behavior. Do not auto-commit at the end of tasks, sessions, or after completing a feature. The user will request a commit when they are ready.

---

## Unity Developer Notes

The project owner has **no prior Unity experience**. When explaining Unity concepts:
- Use plain language; avoid assumed knowledge of the Editor UI
- Prefer concrete steps ("click X, drag Y") over abstract descriptions
- Relate concepts to things a programmer already knows where possible

---

## Asset Import Workflow

New assets (models, textures, audio) are placed in `boxhead/models/` — **above** the Unity project directory — not directly into `Assets/`. Claude is responsible for extracting archives, moving files to the correct Unity folder, and configuring import settings before use.

### Asset Sources

**Characters:** All character models come from [Meshy](https://meshy.ai).

**ENV Props:** All environment props come from **Low Poly Mega Pack - Polyworks** (Unity Asset Store).

**Weapons:** Weapon models come from [Meshy](https://meshy.ai).

### Meshy Models (Characters + Weapons)

Character and weapon models from Meshy share these import requirements:

- **Scale factor:** Set `ModelImporter.globalScale = 1` and `ModelImporter.useFileScale = true` on import. The FBX file's own unit scale is read automatically and converts the model to Unity's 1-unit-per-meter convention. **Exception — weapons:** also leave `globalScale = 1, useFileScale = true` but `WeaponHolder.weaponScale` controls their runtime size independently. Never hard-code `globalScale = 100` — different Meshy exports use different internal units and will be wildly oversized.
- **Target folder:** `Assets/_Project/Models/<category>/<ModelName>/` (e.g. `Models/Characters/GnomeSoldier/`)
- **Textures:** Extract embedded textures to `Assets/_Project/Models/<category>/<ModelName>/Textures/` using `ModelImporter.ExtractTextures(path)`.
- **Materials:** Remap to the project's URP materials in `Assets/_Project/Materials/`.

### Import checklist (run for every new Meshy asset)

1. Copy/extract the raw files from `boxhead/models/` into the correct `Assets/_Project/Models/` subfolder.
2. Set `globalScale = 1` and `useFileScale = true` on the `ModelImporter` and re-import.
3. Extract embedded textures.
4. Assign extracted textures to the material's `_BaseMap` and `_EmissionMap` slots.
5. Verify scale in the scene — the model root should be approximately human-sized (~2 units tall).

---

## Namespaces

| Folder | Namespace |
|---|---|
| Core/ | `Boxhead.Core` |
| Player/ | `Boxhead.Player` |
| Enemy/ | `Boxhead.Enemy` |
| Systems/ | `Boxhead.Systems` |
| UI/ | `Boxhead.UI` |
| Editor/ | `Boxhead.Editor` |

---

## Branching Strategy

```
main          ← primary integration branch; all sprint work merges here
feature/v4-*  ← one branch per V4 sprint (e.g. feature/v4-sprint-02-abilities)
```

**Current status:** V4 Sprint 1 complete. Sprint 2 (Epic/Legendary abilities) is next.

**Rule:** Always branch new sprints from `main`. PRs target `main`.

---

## Mobile Performance Budget

These limits target modern mobile hardware (iPhone 14+ / flagship Android 2022+):

- **Draw calls:** < 100
- **Scene triangles (total):** < 300k
- **Player character:** ~20k triangles
- **Standard enemies:** ~10–12k triangles each
- **Weapons:** < 1,000 triangles each
- **ENV props:** 200–600 triangles each
- **GC alloc in gameplay:** 0 (zero per frame)

**Enforced patterns:**
- Cache all `WaitForSeconds` / `WaitForSecondsRealtime` in `Awake` as private fields — never `new` them in coroutines
- Cache `renderer.material` in `Awake`, call `Destroy(_material)` in `OnDestroy` — never access `.material` in a hot path
- No LINQ in Update/coroutines
- No boxing (avoid passing value types as `object`)
- Use `CompareTag` not `== tag`

---

## Architecture Patterns

### Event-Driven Communication

Scripts communicate via `System.Action` events. Never poll state; subscribe in `OnEnable`/`Start`, unsubscribe in `OnDestroy`.

```csharp
// Publisher
public event Action<int, int> OnHealthChanged;
public event Action OnDeath;

// Subscriber
private void Start()   { _stats.OnHealthChanged += UpdateHealth; }
private void OnDestroy() { _stats.OnHealthChanged -= UpdateHealth; }
```

### ScriptableObject Data

Game data lives in ScriptableObjects, not MonoBehaviour fields:
- `BoxData` — stats (maxHealth, moveSpeed), box color, counter ability flag
- `SoundData` — AudioClip, volume, pitchMin/pitchMax per `SoundEvent`

### Singleton Pattern

`GameManager` and `AudioManager` are singletons. Pattern used:

```csharp
public static T Instance { get; private set; }

private void Awake()
{
    if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject); // AudioManager only
}

private void OnDestroy() { if (Instance == this) Instance = null; }
```

`AudioManager` calls `DontDestroyOnLoad` — it survives scene reload.
`GameManager` does NOT — it is re-created per scene and clears `Instance` in `OnDestroy`.

### RequireComponent

Always add `[RequireComponent(typeof(X))]` when a script unconditionally calls `GetComponent<X>()` in `Awake`.

---

## Key Scripts Reference

> These scripts are the **planned architecture**, to be ported from `v3/main`. They define the design contract — agents should write new code to this specification.

### `PlayerStats` (Boxhead.Player)
- `int CurrentHealth`, `int MaxHealth`, `bool IsDead`
- `event Action<int, int> OnHealthChanged` — fires `(current, max)` after any health change
- `event Action OnDeath` — fires once when health reaches 0
- `event Action<BoxData> OnBoxChanged`

### `CombatController` (Boxhead.Player)
- `CombatState State` — `{ Idle, Attacking, Dodging, Parrying, Countering, Staggered }`
- `AttackResult TryReceiveAttack(int damage, bool parryable = true, GameObject attacker = null)` — returns `{ Hit, Dodged, Parried }`; callers never inspect `State` directly; always pass `attacker: gameObject`
- Events: `OnDodgeStarted`, `OnParrySuccess`, `OnCounterWindowOpened`, `OnCounterStrike` (carries `GameObject` target — the attacker who was parried), `OnCounterWindowClosed`, `OnPlayerStaggered`

### `EnemyStats` (Boxhead.Enemy)
- `int AttackDamage`, `bool IsDead`
- `event Action OnDeath`

### `BasicEnemyAI` (Boxhead.Enemy)
- States: `Idle → Chase → WindUp → Attacking → Staggered → Dead`
- Subscribes to `CombatController.OnCounterStrike` (NOT `OnCounterWindowClosed`) to detect counter hits
- Material cached in `Awake`, destroyed in `OnDestroy`

### `GameManager` (Boxhead.Core)
- `GameState State` — `{ Playing, Won, Lost }`
- Wires to player via `FindWithTag("Player")` in `Start`; tracks all enemies via `FindGameObjectsWithTag("Enemy")` — subscribes `OnDeath` per `EnemyStats`; `_livingEnemyCount` decrements per death
- `TriggerWin()` — fires when `_livingEnemyCount` reaches 0 (or called directly by `RoomManager`); guarded by `State == Playing`
- `Restart()` calls `SceneManager.LoadScene(activeScene.buildIndex)`

### `AudioManager` (Boxhead.Core)
- `Play(SoundEvent)` — round-robin pool of 8 `AudioSource`s; prefers idle sources
- Pool sources are children of the AudioManager GameObject
- `DontDestroyOnLoad` — persists across restarts

### `HUDController` (Boxhead.UI)
- Health slider: green (`#33CC33`) → red (`#CC3333`) via `Color.Lerp`
- Counter window indicator: `WaitForSecondsRealtime` pulse (survives `timeScale = 0`)
- `_counterWindowImage` cached in `Start` — no `GetComponent` per frame

### `GameOverUI` (Boxhead.UI)
- `Show(bool won)` activates panel, sets TMP text, pauses via `Time.timeScale = 0` + `AudioListener.pause = true`
- Panel starts **inactive in the scene** — do NOT set `panel.SetActive(false)` in `Awake` (it triggers a reactivation loop)
- `OnRestartClicked` restores `timeScale = 1` and `AudioListener.pause = false` before loading

---

## Input System

Uses **Unity New Input System** with `PlayerInput` component (SendMessages behavior).

| Action | Gamepad | Touch (Mobile) |
|---|---|---|
| Move | `<Gamepad>/leftStick` | Virtual joystick (left side) |
| Attack | `<Gamepad>/buttonSouth` | On-screen button |
| Dodge | `<Gamepad>/buttonEast` | On-screen button |
| Parry | `<Gamepad>/buttonWest` | On-screen button |
| Jump | `<Gamepad>/buttonNorth` | On-screen button |

On-screen buttons use `OnScreenButton` components.

---

## Camera

Cinemachine 3 (Unity 6). `CinemachineCamera` with:
- Body: `CinemachineFollow` — offset `(0, 12, -8)`
- Aim: `CinemachineHardLookAt`
- Result: fixed top-down perspective following the player

---

## Unity MCP

The project uses **MCP for Unity** (`mcpforunityserver`) for scene automation.

**To start:** Window → MCP for Unity → Start Local HTTP Server (inside Unity Editor)

Key tools: `execute_code`, `batch_execute`, `manage_gameobject`, `manage_components`, `manage_scene`, `manage_camera`, `read_console`, `manage_editor`, `manage_prefabs`, `manage_material`, `manage_asset`.

**Workflow:**
1. Read `mcpforunity://editor/state` — confirm `ready_for_tools: true`
2. Use `batch_execute` for multi-step scene operations
3. After script changes: wait for `is_compiling: false`, then `read_console(types=["error"])`
4. Use `manage_camera(action="screenshot", include_image=true)` to verify visual results

**`execute_code` rules:**
- No `using` directives — Unity editor namespaces are pre-imported
- Use `UnityEngine.Object.DestroyImmediate()` not `Object.DestroyImmediate()` (ambiguous)
- `foreach` silently fails to assign Transform locals — use `for` + array index instead
- Use `PrefabUtility.LoadPrefabContents` / `SaveAsPrefabAsset` / `UnloadPrefabContents` for headless prefab wiring
- Use `SerializedObject` + `FindProperty` for serialized field assignment (not reflection)

---

## Blender MCP

The project uses the **blender-studio** MCP server. It exposes **200 named tools** plus `execute_python` for arbitrary bpy code.

**To start:** The blender-studio MCP must be running and connected to a live Blender session.

**Tool-first rule:** Always use a named tool when one exists — fall back to `execute_python` only for operations not covered. Use `mcp__blender__search_tools` to discover tools by keyword.

### Key Tools for This Project's Workflow

**Inspection (before any changes)**
- `get_object_info` — location, rotation, scale, materials, modifiers, poly count
- `get_mesh_stats` — vertex / edge / face / triangle counts
- `get_scene_info` — all objects, render engine, frame range
- `get_scene_stats` — total triangle count, texture memory, estimated render time
- `ping` — health check; returns Blender version

**Import / Export**
- `import_file` — import OBJ, FBX, GLTF/GLB, USD, PLY, STL into the current scene
- `import_files` — batch import multiple files; returns success/failure per file
- `export_file` — export scene or selection to OBJ, FBX, GLB, USD, STL, or PLY

**Mesh Normalization (pre-Unity pass)**
- `apply_transforms` — apply location / rotation / scale to identity (always run before export)
- `recalculate_normals` — fix inside-out normals from imports
- `flip_normals` — invert all face normals
- `merge_by_distance` — weld duplicate verts within a threshold
- `apply_all_modifiers` — bake all modifiers to real geometry before export
- `apply_modifier` — bake a single named modifier

**UV**
- `mark_seam` — mark edges as UV seams (provide `edge_indices` or omit to mark all)
- `pack_uvs` — pack UV islands into 0–1 space, minimising wasted area

**Materials / Textures**
- `create_material` — new Principled BSDF material
- `assign_material` — assign an existing material to an object slot
- `add_image_texture` — load an image and wire it to a material's Principled BSDF (BASE_COLOR, ROUGHNESS, METALLIC, NORMAL, EMISSION, ALPHA)
- `bake_texture` — bake AO, DIFFUSE, NORMAL, ROUGHNESS, or SHADOW to an image (Cycles required)
- `pack_textures` — embed all external images into the .blend file

**Rigging / Animation export prep**
- `add_armature_modifier` — link a mesh to an armature for skeletal deformation
- `add_bone` — add a bone to an armature (enters/exits Edit mode automatically)
- `bake_action` — bake constraints and driven values to direct keyframes (required before Unity FBX export)
- `apply_pose_as_rest` — bake current pose as new rest pose
- `reset_pose` — reset all bones to rest position

**Object management**
- `create_object` — create a new 3D object
- `delete_object` — permanently delete an object
- `duplicate_object` — duplicate with optional linked duplicate
- `join_objects` — join multiple meshes into one
- `rename_object` — rename object and optionally its data block
- `set_3d_cursor` — place cursor at world position or snap to object origin
- `move_to_collection` — organise objects into collections

**Scene persistence**
- `save_blend` — save the current .blend file
- `reload_image` — reload an image from disk after external editing

**Batch / Safety**
- `execute_batch` — run multiple named tool calls in one round-trip (failures captured per-command, does not stop batch)
- `execute_tools_safe` — dry-run validation before committing changes
- `begin_transaction` / `commit` / `rollback` — wrap multi-step operations; `rollback` restores from auto-checkpoint
- `create_checkpoint` / `restore_checkpoint` — manual snapshots before risky operations
- `list_checkpoints` — see all available checkpoints

**Arbitrary Python (fallback only)**
- `execute_python` — run bpy code in the live session; assign `__result__ = value` to return data; stdout captured
- `execute_python_headless` — run bpy code in a `blender --background` subprocess (batch ops that don't need the live session)

### FBX Export for Unity (confirmed working settings)

Use `export_file` for standard exports. When fine-grained control is needed, use `execute_python` with these confirmed settings:

```python
bpy.ops.export_scene.fbx(
    filepath=out_path,
    use_selection=True,
    axis_forward='-Z',
    axis_up='Y',
    apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_ALL',   # required — FBX_SCALE_NONE causes 1/100 scale in Unity
    bake_space_transform=True,             # required — bakes axis conversion into vertices
    mesh_smooth_type='FACE',
    use_mesh_modifiers=True,
    add_leaf_bones=False,
    path_mode='COPY',
    embed_textures=False,
)
```

### Standard Pre-Export Checklist (run for every Meshy model)

1. `get_object_info` — confirm object is in scene and check current scale/rotation
2. `apply_transforms` — zero out location/rotation/scale
3. `recalculate_normals` — fix any inside-out normals
4. `merge_by_distance` — weld duplicate vertices (threshold: 0.0001)
5. `apply_all_modifiers` — bake to real geometry
6. `get_mesh_stats` — confirm triangle count is within mobile budget
7. `export_file` (FBX) — export with Unity axis settings

---

## Custom Agents

Project-specific Claude Code agents live in `.claude/agents/`. **Always delegate to the most specific agent — do not perform agent-domain work inline.**

### Shared Foundational Context

All agents read shared context from `.claude/foundational/` before starting any task:

| File | Contents |
|---|---|
| `project-context.md` | Game premise, tech stack, repo layout, branch strategy, sprint state, Unity MCP workflow |
| `tech-standards.md` | Performance budgets, namespaces, architecture patterns, key scripts API, execute_code patterns |
| `art-and-style.md` | Visual style, color palette, triangle budgets, import pipeline, URP constraints |
| `game-world.md` | Characters, enemies, zones, weapons, combat system, tone, terminology |

### Agent Directory

| Agent | Trigger — invoke when the request involves… |
|---|---|
| `unity-senior-developer` | Implementing any Unity feature, system, mechanic, or new script |
| `unity-code-reviewer` | Reviewing C# scripts for correctness, GC, architecture, naming |
| `unity-code-assistant` | Quick script edits, debug sessions, or refactors of existing code |
| `gameplay-designer` | Evaluating fun, pacing, difficulty curve, or reward structure — NO code |
| `performance-optimizer` | Profiling GC allocations, draw calls, Update() abuse, mobile bottlenecks |
| `art-direction-agent` | Visual style decisions, asset lists, AI art prompts (Meshy, Midjourney, PixVerse) |
| `game-design-doc-writer` | Drafting or updating the GDD, designing mechanics, planning progression |
| `storyteller` | Any in-game text — lore, dialogue, quest text, zone descriptions, item flavor text, tutorial hints, victory/defeat moments |
| `storyboard-artist` | Cutscenes, boss intros, zone transitions, UI flows — shot-by-shot sequences |
| `project-manager` | Sprint planning, milestone tracking, phase breakdowns, sprint doc updates |
| `blender-specialist` | Blender pipeline work — model inspection, pivot/orientation/scale fixes, FBX/OBJ export, Blender Python via MCP |
| `3d-artist` | Creative Blender work — retopology, UV unwrapping, materials, renders, visual quality passes |
| `social-media-manager` | All public-facing content — posts, announcements, devlogs, content calendars |
| `n8n-agent` | Building or modifying n8n automation workflows, connecting services via n8n |
| `Explore` | Finding files, grepping symbols, answering "where is X defined?" |
| `Plan` | Designing implementation plans and architecture before any coding starts |

### Skills

| Skill | Invoke with | What it does |
|---|---|---|
| `level-design` | `/level-design [zone name] [optional context]` | Full level design pipeline — GDD, Meshy prompts, LevelData ScriptableObject spec. Four approval gates. Writes to `docs/levels/[zone-slug]/`. |
| `unity-character-importer` | `/unity-character-importer [Name] [zipPath]` | Full Meshy → Blender → Unity character import pipeline |
| `profiling-workflow` | `/profiling-workflow` | Frame budget setup and Unity Profiler baseline |
| `asset-pipeline` | `/asset-pipeline` | Routes non-character assets through Blender and into Unity |

### Mandatory Routing Rules

These rules are non-negotiable — route before acting, not after:

1. **Any Unity C# implementation** → `unity-senior-developer`. Do not write scripts inline.
2. **Before committing any new or modified script** → `unity-code-reviewer`.
3. **Any Meshy, PixVerse, or AI art tool prompt** → `art-direction-agent`. Never write prompts ad hoc.
4. **Any sprint plan file** (`sprints/*.md`) → `project-manager` to write or update it.
5. **Any in-game text, dialogue, lore, or story content** → `storyteller`. Never write story content inline.
6. **Any Blender pipeline operation** (import, pivot, scale, axis fix, FBX/OBJ export) → `blender-specialist`.
7. **Any creative Blender work** (modeling, retopology, UV, materials, renders) → `3d-artist`.
8. **Any public-facing post, announcement, or social content** → `social-media-manager`. Never write posts inline.
9. **Any implementation plan or architecture design** → `Plan` agent before writing code.
10. **Performance concerns on any feature** → `performance-optimizer` proactively, not only when the game is slow.
11. **Designing a new zone or level** → `/level-design` skill. Never design a level ad hoc without the skill.

---

## Level Generation

Levels are **never hand-built as scenes**. Instead, each level is a ScriptableObject data asset (enemy spawn points, prop placements, wave configs, zone theme) and a `LevelBuilder` MonoBehaviour reads that data and instantiates everything at runtime.

- Adding a new level = creating a new ScriptableObject and adding it to the world's level list — no scene editing.
- Each world has its own `LevelBuilder` variant (or a shared builder configured by the ScriptableObject).
- Nav mesh, lighting, and spawn overlap edge cases must be handled in the builder script — plan these before implementation.

**Routing rule:** Any level generation architecture or new `LevelBuilder` script → `Plan` agent first, then `unity-senior-developer`.

---