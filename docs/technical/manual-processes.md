# Manual Processes — BoxForged

Processes that require manual steps in the Unity Editor and cannot be fully automated.

---

## 1. Baking a NavMesh for a New Room

Every room scene needs its own baked NavMesh. Without it, all NavMeshAgent-based enemies stand still (`isOnNavMesh` returns false).

### Steps

1. Open the room scene in Unity Editor.
2. Select every static floor, wall, and obstacle GameObject that enemies should walk on or around. In the Inspector, enable **Navigation Static** via the Static dropdown (top-right of Inspector, click the arrow → Navigation Static).
   - ✅ Mark: floor planes, walls, buildings, large props (barrels, crates, fences, rocks)
   - ❌ Skip: player, enemies, spawners, cameras, lights, trigger zones, projectiles
3. Open **Window → AI → Navigation** (or **Window → AI → Navigation (Obsolete)** in Unity 6).
4. Go to the **Bake** tab. Confirm agent settings:
   - Agent Radius: `0.5`
   - Agent Height: `2.0`
   - Max Slope: `30`
   - Step Height: `0.4`
5. Click **Bake**.
6. Blue overlay appears on walkable surfaces — confirm it covers the floor and cuts holes around obstacles.
7. Save the scene (**Ctrl+S / Cmd+S**).

### Verification

Hit Play. Enemies should now path around obstacles rather than walking through them. If an enemy stands still, check:
- Is the enemy prefab on the NavMesh surface (not floating above it)?
- Does the prefab have a `NavMeshAgent` component?
- Is the NavMesh baked for this scene (blue overlay visible in Scene view with NavMesh display on)?

### Notes

- NavMesh data is stored in a folder next to the scene file: `Scenes/RoomName/NavMesh.asset`
- The SpinCycle boss disables its NavMeshAgent during SpinCharge, JumpCharge, and JumpBack — this is intentional. It re-enables and warps back to the mesh after each direct-movement attack.
- SprinklerSentinel does **not** use NavMesh by design (burrowing jump locomotion).

---

## 2. Building a New Room Scene from Scratch

### Prefab drop-in list

Drag these prefabs in — GameManager self-wires all UI at runtime:

| Prefab | Path | Notes |
|---|---|---|
| `pfb_player` | Prefabs/Player/ | Tag: Player |
| `pfb_GameManager` | Prefabs/Core/ | Self-finds all UI refs in Start() |
| `pfb_AudioManager` | Prefabs/Core/ | DontDestroyOnLoad |
| `pfb_SaveSystem` | Prefabs/Core/ | |
| `pfb_ProgressionSystem` | Prefabs/Core/ | |
| `pfb_DifficultyManager` | Prefabs/Core/ | |
| `pfb_RoomManager` | Prefabs/Core/ | Configure _rooms list per scene |
| `pfb_EnemySpawner` | Prefabs/Core/ | One per enemy type; set enemy prefab ref |
| `pfb_Main_Camera` | Prefabs/Core/ | |
| `pfb_MinimapCamera` | Prefabs/Core/ | |
| `pfb_CM_FollowCam` | Prefabs/Core/ | **Manual step — see below** |
| `pfb_hud_v2` | Prefabs/UI/ | HUD + pause + RunStartUI embedded |
| `pfb_GameOverUI` | Prefabs/UI/ | |
| Enemies as needed | Prefabs/Enemies/ | Tag each: Enemy |

### Manual wiring required after drop-in

**CM_FollowCam tracking target:**
1. Select `pfb_CM_FollowCam` in the hierarchy.
2. Find the `CinemachineCamera` component.
3. Set `Tracking Target` → the Player's `CameraLookTarget` child transform.
4. Set `Look At` → same `CameraLookTarget`.

**RoomManager rooms:**
1. Select the `RoomManager` GameObject.
2. In the `_rooms` list, add one `RoomData` entry per room/wave.
3. Each entry: set `enemies` list, `spawnPoints`, `maxConcurrentEnemies`, `exitGate`, `bossOwnedWin`.

**EnemySpawner configuration:**
1. For each `pfb_EnemySpawner` instance, set `enemyPrefab` to the desired enemy.
2. Set `spawnPoints` array to spawn point Transforms in the scene.
3. Tune `maxActiveEnemies` and `maxTotalSpawns`.

**Build Settings:**
1. **File → Build Settings → Add Open Scenes** to register the new scene.
2. Scene 0 must remain `LoadingScreen.unity`. New gameplay scenes go at index 2+.

### After setup

Bake NavMesh (see Section 1), then hit Play to verify enemies spawn and path correctly.

---

## 3. Adding a New Character (Meshy → Blender → Unity)

See `docs/technical/asset-pipeline-plan.md` for the full pipeline. Key manual steps:

1. **Blender** — open the raw Meshy FBX, manually position rig bones, verify weight painting.
2. **Blender FBX export** — use confirmed settings: `axis_forward='-Z'`, `axis_up='Y'`, `apply_scale_options='FBX_SCALE_ALL'`, `bake_space_transform=True`.
3. **Unity ModelImporter** — set Humanoid rig, configure Avatar bone mappings manually if auto-detect misses any. Check for 74m float bug in Play mode (if present, review scale settings).
4. **Animator culling** — if the character's mesh is a sibling of Armature (not a child), set `Animator.cullingMode = AlwaysAnimate` on the prefab to prevent silent bone write skips.
5. **WeaponGripPoint** — add an empty child to the `LeftHand` bone with `localScale` set so `lossyScale = (1,1,1)`. This normalises weapon attachment across characters with different armature scales.

---

## 4. Configuring the Boss Intro Camera (CM_BossIntroCam)

The `CM_BossIntroCam` Cinemachine camera must be positioned manually per room to frame the Saloon door (or equivalent boss entrance).

1. In the room scene, find `CM_BossIntroCam`.
2. Position it to frame the boss entrance from a dramatic angle — typically slightly elevated and offset to one side.
3. Ensure it is **disabled** by default (enabled only by `BossIntroSequence` at runtime).
4. The `BossIntroSequence` component on the SpinCycle boss handles switching between `CM_BossIntroCam` and `CM_FollowCam` automatically.

---

## 5. Setting Up an Enemy Spawner Wave

`EnemySpawner` supports wave-based spawning with respawn limits.

Key fields to configure per spawner instance:
- `enemyPrefab` — which enemy to spawn
- `skepticPrefab` — alternate enemy spawned every N kills (Grunt spawner only)
- `spawnPoints` — array of Transform positions; enemies spawn at these randomly
- `maxActiveEnemies` — max alive at any time (controls density)
- `maxTotalSpawns` — kill this many and the spawner stops (controls total wave size)
- `initialDelay` — seconds before first spawn after room activates
- `respawnCheckInterval` — how often the spawner checks if it should spawn more

Production values in CulDeSac_Room1 for reference:
| Spawner | maxActive | maxTotal |
|---|---|---|
| EnemySpawner_Grunts | 3 | 12 |
| EnemySpawner_WheelRoller | 2 | 5 |
| EnemySpawner_Sentinel | 1 | 3 |

---

## 6. Wiring a New Weapon's Grip Offset

Each `WeaponData` ScriptableObject stores per-character grip offsets. After importing a new weapon:

1. Create a `WeaponData` SO in `Assets/_Project/ScriptableObjects/Weapons/`.
2. Assign `weaponPrefab`, `weaponName`, `weaponIcon`.
3. In Play mode with the weapon equipped, use **WeaponHolder → right-click → Reapply Grip (Live Tuning)** to tweak `gripPositionOffset`, `gripRotationOffset`, `gripScale` until the weapon sits correctly in the hand.
4. Copy the values out of Play mode into the ScriptableObject before stopping (Play mode changes are lost).

---

## 7. Adding a New Scene to Build Settings

1. Open the scene in Unity.
2. **File → Build Settings**.
3. Click **Add Open Scenes**.
4. Confirm scene order: 0 = LoadingScreen, 1 = CulDeSac_Room1, 2+ = new scenes.
5. If the new scene is a gameplay room, update `LoadingScreenController.cs` or whichever script decides which scene to load next.
