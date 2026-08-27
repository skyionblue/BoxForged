# BoxForged — Architecture

- **Status:** Living reference · pre-production snapshot
- **Date:** 2026-08-19
- **Engine:** Unity 6 LTS `6000.5.3f1` · URP 17.5.0 · Cinemachine 3.1.7 · Input System 1.19.0 · AI Navigation 2.0.14
- **Unity project root:** `BoxForged/BoxForged/`
- **Scripts root:** `Assets/_Project/Scripts/`

This document describes what exists in code **today**, which contracts are staying, and which are changing under [ADR-0001](adr/0001-fixed-low-follow-camera.md), [ADR-0002](adr/0002-full-scene-rebuild.md), [ADR-0003](adr/0003-attack-telegraph-channel.md), and [ADR-0004](adr/0004-world1-single-continuous-scene.md).

It is descriptive, not aspirational. Where the codebase diverges from the documented intent, the divergence is recorded rather than smoothed over — that divergence is itself the most useful thing in here.

---

## 1. Assembly and namespace structure

**There is effectively one assembly.** Exactly one `.asmdef` exists in the entire project, and it is not a test assembly:

```json
{ "name": "StatSystem" }
```

Everything else — all gameplay, save, ability, UI, and cutscene code — compiles into the default `Assembly-CSharp`. There are no EditMode or PlayMode test assemblies, and `com.unity.test-framework` is installed but unused. Consequences and the proposed minimum boundary are in `docs/TECHNICAL_DESIGN.md` §8.

Namespaces follow folders, retaining the legacy `Boxhead.*` root (not to be renamed opportunistically):

| Folder | Namespace | Files |
|---|---|---|
| `Core/` | `Boxhead.Core` | 17 |
| `Player/` | `Boxhead.Player` | 13 |
| `Enemy/` | `Boxhead.Enemy` | 22 |
| `Systems/` | `Boxhead.Systems` | 44 |
| `Systems/Abilities/` | `Boxhead.Systems` | 18 |
| `Systems/StatSystem/` | (own assembly) | 7 |
| `UI/` | `Boxhead.UI` | 29 |
| `Editor/` | `Boxhead.Editor` | 4 |

`Combat/` exists as an empty folder.

---

## 2. System map

### 2.1 Core — `Boxhead.Core`

| System | Role | Lifetime |
|---|---|---|
| `GameManager` | Win/lose state, zone→scene maps, shuffled room queue, cross-scene loadout capture/restore, cutscene gating, between-room screen routing | Scene-scoped singleton |
| `AudioManager` | Event-driven pooled playback | `DontDestroyOnLoad` |
| `SaveSystem` | JSON persistence to `persistentDataPath/save.json` | `DontDestroyOnLoad` |
| `ProgressionSystem` | XP/level, IP/combo, run selection, run loadout snapshot | `DontDestroyOnLoad` |
| `DifficultyManager` | Holds `DifficultyData`, applies multipliers on spawn | `DontDestroyOnLoad` |
| `CutscenePlayer` | Full-screen H.264 video playback from StreamingAssets | `DontDestroyOnLoad` |
| `HitStopManager` | Impact juice — freezes animators without touching `timeScale`, fires Cinemachine impulse | Scene |
| `CutsceneFlags` | PlayerPrefs seen-flags + `CutsceneCatalog` constants | Static |
| `CameraStackWirer`, `CameraFollowTargetInjector` | URP camera-stack and Cinemachine follow-target wiring | Scene |
| `SaveTester`, `TestSceneStarter` | Dev-only harnesses | Scene |

**Five persistent singletons.** `docs/PROJECT_CONTEXT.md` records two (`GameManager`, `AudioManager`) and says not to add more by default; three more have accumulated. Recorded as drift — a service-locator refactor is not worth its risk at this stage, but the count should stop growing.

### 2.2 Player — `Boxhead.Player`

| System | Role |
|---|---|
| `PlayerStats` | Health, death, box changes |
| `CombatController` | 965 lines. Combat state machine, `TryReceiveAttack`, parry/counter windows, V3 special-ability driver |
| `PlayerController` | Movement, jump, arena bounds, auto-facing |
| `WeaponHolder` | Equip/attach to hand bone, material application, muzzle anchor |
| `WeaponCycler` | Per-character weapon variant resolution (`_nm` / `_nf`) |
| `WeaponEquipController` | **Not on the player prefab** — `WeaponHolder.cs:251` calls it through a null-conditional, so its two events never fire |
| `CharacterStatsSO`, `FightingStyleData` | Authored configuration |

### 2.3 Enemy — `Boxhead.Enemy`

Ten AI classes plus projectiles, health bars, and spawners. `BasicEnemyAI` and `SkepticGruntAI` are the general types; `SpinCycleAI` (1,256 lines) is the World 1 boss.

All AI share a state progression around `Idle → Chase → WindUp → Attacking → Staggered → Dead` and communicate attacks through `CombatController.TryReceiveAttack(damage, parryable, attacker)`.

**Every attack tell in the game is a whole-body material tint** driven from a `WindUp(Color)` coroutine. There is no telegraph, indicator, decal, or AOE-marker system anywhere in the codebase. See ADR-0003.

### 2.4 Systems — `Boxhead.Systems`

Four clusters:

- **Forge/weapons** — `ForgeController`, `WeaponInstance`, `WeaponInventory`, `WeaponDurability`, `CardboardResource`, `CardboardPickup`, `WeaponObjectSO`, `WeaponDropTableSO`, `WorkbenchProp`, `WeaponForgeAnimation`
- **Level/rooms** — `LevelBuilder`, `RoomManager`, `EnemySpawnPoint`, `RoomGate`, `RoomTrigger`, `SafeZone`, `BossHallDoor`, `BossRoomWeaponSpawner`
- **Abilities** — `AbilityExecutor`, `AbilitySO`, `Abilities/*Behaviour` (V4) alongside `WeaponAbilityData` + `*AbilityData` (V3)
- **Camera support** — `CameraOcclusion`, `BuildingOcclusionFader`

### 2.5 UI — `Boxhead.UI`

29 files. Screen-space menus (`RunStartUI`, `ShopScreen`, `UpgradeScreen`, `WorldMapScreen`, `MetaScreen`, `InventoryScreen`), world/overlay HUD (`HUDController_V2`, `WeaponHUDSlots`, `HealthBar3D`, `ChargeMeter3D`), and a separate HUD overlay camera stack.

Two generations coexist in several places: `HUDController` vs `HUDController_V2`, and `ForgeUI` vs `ForgePanel`.

---

## 3. Contracts that are staying

Preserved from `docs/PROJECT_CONTEXT.md` and confirmed against code.

### 3.1 Combat

`CombatController` remains the single authority on damage resolution.

- States: `Idle`, `Attacking`, `Dodging`, `Parrying`, `Countering`, `Staggered`
- `TryReceiveAttack(int damage, bool parryable = true, GameObject attacker = null)` → `AttackResult` (`Hit` / `Dodged` / `Parried`)
- **Callers use the return value; they never inspect internal state to decide the outcome.** Every enemy AI honours this today.

`PlayerStats`, `EnemyStats`, and `BasicEnemyAI` contracts are unchanged.

### 3.2 Communication

Event-driven via `System.Action`, with subscription paired to Unity lifecycle. `AbilityExecutor` is the reference implementation — delegates are cached as fields in `Awake` specifically so unsubscribe works and no allocation occurs per subscription.

`RoomManager` exposes **static** events (`OnRoomActivated`, `OnRoomCleared`); everything else uses instance events.

### 3.3 Room / win ownership

A deliberate and correctly enforced boundary:

- `RoomManager` owns room activation, the `maxConcurrentEnemies` refill cap, the `_roomClearedDelay` beat, and gate opening. It **never** triggers win — explicitly documented at `RoomManager.cs:258-259`.
- Win is triggered exclusively by boss AI via its own defeat sequence.
- `GameManager` defers: `CheckWinCondition` returns early when a `RoomManager` is present (`GameManager.cs:344-348`).

**Preserve this.** It is one of the cleanest boundaries in the codebase.

### 3.4 Data

ScriptableObjects for authored configuration — 175 `.asset` files across weapons, abilities, upgrades, difficulty, characters, drop tables, sound, and volumes. `WeaponObjectSO` extends `WeaponData`; `AbilitySO` and `AbilityBehaviour` are SO-based.

### 3.5 Hot-path discipline

Genuinely honoured, and worth stating so it is not eroded:

- **Zero `using System.Linq` in the entire codebase.**
- Pre-allocated physics buffers (`AbilityExecutor.cs:62-64`), cached `WaitForSeconds`, cached delegates, `readonly struct` contexts, squared-distance comparisons.
- Every `renderer.material` / `new Material` site has a matching `Destroy`.

### 3.6 Scene self-wiring — formalized, not removed

73 `FindObjectOfType` / `FindWithTag` / `GameObject.Find` call sites exist, concentrated in `Awake`/`Start`.

Textbook advice says replace these with explicit injection. **That advice is wrong for this project.** Self-wiring by tag lookup is what allows a rebuilt scene to work without manual re-wiring — which is exactly what protects a two-person non-expert team through a full scene rebuild, and what makes audience-contributed rooms viable.

The contract is therefore made explicit rather than removed:

- Lookups happen in `Awake`/`Start` only, **never** in `Update` or any hot path.
- Every lookup null-checks and logs actionable context (the codebase already does this consistently).
- Scenes remain composition roots; prefabs self-register.

`WorkbenchProp` shows the better version of this pattern — static `OnSpawned` / `OnRemoved` events let it announce itself instead of being searched for.

---

## 4. Contracts that are changing

### 4.1 Camera — ADR-0001

| | Before | After |
|---|---|---|
| Documented offset | `(0, 12, -8)` | — |
| **Actual** offset | `(7.879929, 11, -10)`, FOV 40 | `(0, 5.5, -7.57)`, FOV 45 |
| Pitch | 40.8° | **36°** |
| Yaw | **−38.2°** | **0°** |
| Aim | `CinemachineHardLookAt` | **none** — rotation from transform |
| Binding mode | `WorldSpace` | `WorldSpace` (preserved) |
| Collision | none | none — deoccluder explicitly rejected |

**The documented camera was never the real camera.** The prefab has a −38.2° yaw that appears in no document. Because `PlayerController.cs:192-200` derives movement from camera yaw (pitch is zeroed out), this rig silently rotated the control mapping by 38° relative to world axes.

**New contract:** the camera is specified by **pitch, distance, and FOV** with measurable framing acceptance criteria (ground ahead ≥ 12 m, behind ≥ 4 m, lateral ≥ 16 m, top ray ≥ 10°) — not by an offset triple. Any offset meeting those criteria is valid.

**Rationale:** an offset hides the quantities that govern readability and cannot be validated across aspect ratios. The hard constraint `pitch > FOV/2` — below which the horizon enters frame and ground depth runs to infinity — is invisible in an offset and is exactly what makes the placeholder `(0, 4, -6)` unsafe at Unity's default FOV.

### 4.2 Level data — ADR-0002

| | Before | After |
|---|---|---|
| Env props, pickups, cardboard, workbenches | `WeaponDropTableSO` assets | unchanged |
| **Room definitions** | plain `[Serializable] RoomData` stored as **prefab-instance overrides inside each scene**, with spawn points as `objectReference` fileIDs to scene-local GameObjects | **`RoomDataSO` assets**, spawn points as `Vector3` data |
| Enemy spawn points | hand-placed scene GameObjects | spawned by `LevelBuilder` from data |
| Scenes | ground + boundaries + lighting + managers + hand-placed encounter content | **thin composition roots only** |
| Camera clearance | not modelled | **validated by the builder** (≥ 8 m rear, ≥ 6 m overhead, combat radius ≤ 9 m) |

**Rationale:** the environment layer is already portable; the encounter layer is not, and is destroyed by a scene rebuild. Extraction must precede the rebuild or every room is authored twice. Beyond this milestone it is what makes a contributed room a reviewable data asset rather than correct scene surgery.

`LevelBuilder`'s existing responsibilities and its runtime NavMesh bake (`LevelBuilder.cs:65`, which discards all baked scene NavMesh data) are unchanged.

### 4.2.1 World 1 is one continuous scene — ADR-0004

ADR-0002's "one scene per room" corollary is superseded **for World 1 only**. `CulDeSac_WildWestCity.unity` is one continuous street played start to finish, not a series of scene loads. This did not require replacing `RoomManager`/`RoomDataSO` — `RoomManager` already models an ordered list of encounter zones inside a single scene (`_rooms`, `RoomTrigger.OnTriggerEnter` → `OnRoomEntered(index)`), it had just never been used that way. Three `RoomDataSO` zones (Arrival, Ambush Alley, boss-only Showdown Circle) are appended via `LevelBuilder.RoomData` as before; what changed is `GameManager` — `RoomManager.HasZoneAfterCurrent` now lets a room-clear advance to the next zone in-scene instead of always calling `LoadNextRoom()`/`SceneManager.LoadScene`. A scene-local `WildWestCityZoneDirector` handles the one piece of behavior specific to this layout (clearing two covered-wagon props to open the boss's fight space on zone-2 entry). See ADR-0004 for the full zone geometry, the gate/NavMesh-carving pattern (`RoomGate` now also toggles a `NavMeshObstacle`, not just its `Collider`/`Renderer`), and the boundary-wall pattern that seals the street against flanking the gates. World 2 and any future world are not required to follow this pattern — the room-per-scene model in 4.2 above is still the default; ADR-0004 documents when and why to deviate from it.

### 4.3 Attack telegraphs — ADR-0003

| | Before | After |
|---|---|---|
| Channel | whole-body material tint only | tint **plus** occlusion-independent overhead indicator |
| Parryable encoding | **hue alone** (red vs yellow) | **shape**, with hue as redundant reinforcement |
| Audio | none per class | distinct cue per class |
| Raised from | per-attack `WindUp(Color)` calls | same seam, one place |

**Rationale:** silhouette-fill tint works only while enemies are unoccluded and separated — a property of the *current* camera, not of the game. Hue-only encoding of the single most consequential combat bit is also a standing accessibility defect. URP decal projectors are rejected: `Mobile_Renderer.asset` has `m_RendererFeatures: []`, and decals require a mobile depth prepass.

---

## 5. Known architectural divergences

Recorded, not resolved. Backlog candidates — see `docs/BACKLOG.md`.

| # | Divergence | Evidence |
|---|---|---|
| 1 | **Two complete ability systems** — V3 (`WeaponAbilityData`, `IEnumerator Activate`, driven by `CombatController`) and V4 (`AbilitySO`/`AbilityBehaviour`, `void Execute`, driven by `AbilityExecutor`), with separate context structs. They negotiate at runtime via `CombatController.cs:764` | Highest contributor-confusion risk in the codebase |
| 2 | **Two occlusion systems**, both mistuned for the new camera, using different selection mechanisms (LayerMask vs tag) | `CameraOcclusion.cs`, `BuildingOcclusionFader.cs` |
| 3 | **Two forge UIs** — the better-built `ForgeUI` (514 lines) is referenced by no scene or prefab; the live `ForgePanel` (193 lines) is a hard modal pause | |
| 4 | **Two HUD controllers** — `HUDController` and `HUDController_V2` | |
| 5 | **Two enemy-spawn systems** running simultaneously — `RoomManager`'s spawn-point path and `EnemySpawner`'s `Transform[]` coroutine | |
| 6 | **Two boss-intro implementations** — `CutscenePlayer` (video, used by SpinCycle) and `PermitPulperBossIntro` (in-engine, hand-rolled, uses reflection for the Cinemachine impulse) | |
| 7 | **Save has a version field but no migration path** — `version` is a corruption sentinel only (`SaveSystem.cs:70-78`); `Data` is exposed as a mutable reference | |
| 8 | **Persistence split across two backends** — JSON for `SaveData`, PlayerPrefs for `CutsceneFlags` | |
| 9 | **Mutable state on shared ScriptableObject assets** — e.g. `TheFirstStrikeBehaviour._firstHitReady` | Single-player-only assumption; would break co-op, which CANON says is designed for from day one |
| 10 | **1,016 lines of unreachable boss code** — `PermitPulperBossAI` + `PermitPulperBossIntro` are in no scene or prefab; runtime lookups always return null | |
| 11 | ~~**Three Build Settings scenes do not exist on disk**~~ — **FIXED 2026-08-27**, dead entries removed from `EditorBuildSettings.asset` (see `docs/BACKLOG.md` B11) | `GameManager.cs:34` zone 1 (`TownSquare_Room1`) is still unreachable — that scene simply doesn't exist yet (World 2, Phase 3), independent of the Build Settings cleanup |
| 12 | **No test infrastructure** — one trivial asmdef, zero tests, DoD unsatisfiable | |

Divergence 9 is worth flagging beyond its size: `docs/CREATIVE_STATE.md` records co-op as designed-in from day one, and per-asset mutable ability state is directly incompatible with that.

---

## 6. Repository organization

```
BoxForged/BoxForged/
  Assets/
    _Project/
      Scripts/       Core · Player · Enemy · Systems{Abilities,StatSystem} · UI · Editor
      ScriptableObjects/  Weapons · Abilities · Upgrades · Difficulty · Characters
                          Levels · SoundData · Volumes · Rooms (proposed, ADR-0002)
      Prefabs/       Core · Player · Enemies · UI
      Models/        Characters · Enemies · Weapons · Props · ENV · HUD   (3.1 GB)
      Scenes/        7 project scenes — all superseded by ADR-0002
      Materials/ · Fonts/ · Animations/
    Settings/        Mobile_RPAsset · Mobile_Renderer · PC_* · volume profiles
    StreamingAssets/Cutscenes/   10 .mp4, 326 MB
    Screenshots/                 60 MB — does not belong under Assets/
    <vendor>/        Off Axis Studios (1.1 GB) · Polylised · SimpleTown · ExplosiveLLC · TextMesh Pro
```

**Third-party reconciliation needed.** `docs/PROJECT_CONTEXT.md` approves Meshy and Polyworks. `SimpleTown`, `ExplosiveLLC` (RPG Character Mecanim, SuperCharacterController), and `Polylised` are present but unlisted in any approval record. No new packages without owner approval.

Working tree is 3.2 GB; `.git` is 6.8 GB with no LFS configured.

---

## 7. Data flow — a run

```
App start
  └─ RuntimeInitializeOnLoad: CutscenePlayer bootstraps
  └─ DontDestroyOnLoad services: AudioManager, SaveSystem, ProgressionSystem, DifficultyManager

Scene load  (composition root)
  ├─ LevelBuilder.Start()
  │     SpawnEnvProps → SpawnWeaponPickups → SpawnCardboardPiles → SpawnWorkbenches
  │     ⤷ ADR-0002 adds: SpawnEnemySpawnPoints (from RoomDataSO)
  │     └─ BuildNavMeshDeferred  (discards baked NavMesh, rebakes from colliders)
  ├─ GameManager.Start()  → restores run loadout LAST (order matters: earlier equips onto an inactive model)
  └─ RoomManager.Activate(room 0)

Combat loop
  Input (New Input System) → PlayerController  [movement is camera-yaw-relative]
                           → CombatController  [aiming uses character facing, NOT camera]
  Enemy AI → WindUp(Color) → TryReceiveAttack(dmg, parryable, attacker) → AttackResult
                           ⤷ ADR-0003 adds: telegraph raised here
  Enemy death → RoomManager refill (up to maxConcurrentEnemies)
  Room cleared → RoomManager.OnRoomCleared (static) → GameManager routes screen

Forge loop
  WorkbenchProp proximity poll → ForgePanel opens  [Time.timeScale = 0, AudioListener.pause = true]
    → ForgeController.TryForge: check slot → spend → remove material → OnWeaponForged
    → WeaponInventory auto-equips if the filled slot is active
    → WeaponHolder.Attach (Destroy + Instantiate on hand bone; pooling TODO)
    ⤷ proposed: ForgePresenter subscribes to OnWeaponForged  [must use UNSCALED time]

Boss defeat → boss AI DefeatSequence → GameManager win  (RoomManager never triggers win)
```

Two ordering facts that are load-bearing and easy to break:

- **Loadout restore must be last in `GameManager.Start()`** — restoring earlier equips onto an inactive model's hand bone.
- **The forge runs at `timeScale = 0` with the AudioListener paused** — any presentation coroutine must use unscaled time, and any audio source must set `ignoreListenerPause = true` or be silent.

---

## 8. Change process

Per `CLAUDE.md`:

1. New architecture or a material change to an existing contract goes through `technical-director`.
2. The decision is recorded in `docs/TECHNICAL_DECISIONS.md` and, when cross-cutting, as a numbered ADR in `docs/adr/`.
3. Implementation follows with `unity-gameplay-engineer` **only after the design is accepted**.
4. `code-reviewer`, QA, and performance review as appropriate.

Existing contracts in `docs/PROJECT_CONTEXT.md` are preserved unless Discovery or Pre-production explicitly approves a change. Stable systems are not redesigned merely because a different generic pattern is possible.
