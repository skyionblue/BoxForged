# Unity Blueprint: Cul-de-Sac Room 1 "The Arrival"

**Scene file:** `Assets/_Project/Scenes/CulDeSac_Room1.unity`
**Asset delivery path:** `Assets/_Project/Models/ENV/CulDeSac/`
**Scene structure:** Scene-per-room (loaded via `SceneManager.LoadScene` from run loop)

---

## Layout Overview

40m wide × 30m long open street. One central corridor running south (spawn) to north (exit). Soft cover on the flanks provided by covered wagons and saloon facades. The ground is cracked asphalt with dirt patches. The exit gate is visible from spawn at the far north end.

**Coordinate system:** Player spawns at approximately (0, 0, -10). Exit gate at (0, 0, +12). Positive Z is north (toward exit).

---

## ASCII Layout (top-down, 1 cell ≈ 4m)

```
W W W W W W W W W W   ← north boundary
W . . . [G] . . . W   ← exit gate at center north
W . . . . . . . . W
W b . . . . . . b W   ← barrels near saloon (cover)
W S . W . . W . S W   ← Saloon | Wagon | Wagon | Saloon
W . . . . . . . . W   ← open combat zone
W . E . . . . E . W   ← Enemy spawn zones (2 WagonWheelRollers flanking)
W . . . . . . . . W
W S . W . E . W . W   ← optional 3rd enemy center, more wagons
W s . . . . . . s W   ← small rocks / cacti scatter
W . . [WB] . . . . W  ← workbench at south-center
W . . . P . . . . W   ← player spawn
W W W W W W W W W W   ← south boundary

Legend:
  P  = Player spawn (0, 0, -10)
  WB = Forge Workbench (0, 0, -12)
  G  = Exit Gate (0, 0, +12)
  S  = Saloon Facade
  W  = Covered Wagon (cover object)
  E  = Enemy spawn zone
  b  = Barrel cluster
  s  = Rock/cactus scatter
  .  = Open ground
  W (outer) = Room boundary wall
```

---

## Scene Hierarchy (recommended structure)

```
CulDeSac_Room1
├── [Managers]
│   ├── pfb_GameManager
│   ├── pfb_AudioManager
│   ├── pfb_ProgressionSystem
│   ├── pfb_SaveSystem
│   └── pfb_RoomManager  (configure with 1 RoomData entry, no exit gate yet)
│
├── [Player]
│   ├── pfb_player          position: (0, 0, -10)
│   ├── pfb_Main_Camera
│   └── pfb_CM_FollowCam
│
├── [LevelProps]            (spawned at runtime by LevelBuilder)
│
├── [ENV — Static]
│   ├── Ground              (Plane, scale 10×1×7.5 = 40×30m, position (0,0,1))
│   ├── [Buildings]
│   │   ├── pfb_env_saloon_facade × 4
│   │   └── pfb_env_saloon_sign_board × 2
│   ├── [Wagons]
│   │   └── pfb_env_covered_wagon × 4
│   ├── [Street Props]
│   │   ├── pfb_env_hitching_post × 4
│   │   ├── pfb_env_lamp_post_western × 2
│   │   ├── pfb_env_rain_barrel × 3
│   │   ├── pfb_env_mailbox_telegraph × 2
│   │   ├── pfb_env_tumbleweed_static × 4
│   │   ├── pfb_env_wanted_poster_blank × 3
│   │   ├── pfb_env_gallows_frame × 1
│   │   └── pfb_env_water_trough × 1
│   ├── [Polyworks Props]
│   │   ├── Prop_Barrel_Closed_01 × 3
│   │   ├── Prop_Barrel_Water_01 × 2
│   │   ├── Rock_Small_Dirt_01–04 × 12
│   │   ├── Rock_Medium_Dirt_01–02 × 4
│   │   ├── Vegetation_Bush_Small_01–03 × 6
│   │   ├── Vegetation_Cactus_01–15 × 5
│   │   ├── Prop_Fence_Wooden_Small_01–04 × 8
│   │   ├── Prop_Junk_Cardboard_Box_01–05 × 6
│   │   └── Prop_Sign_Wooden_Blank_01 × 3
│   └── [Craftsmanship Dressing]  (4 background story props — see GDD §9)
│
├── [Lighting]
│   ├── Sun (Directional Light — warm amber, rotation 50°X, -30°Y, intensity 1.2)
│   └── Ambient (Lighting Settings — warm fill, no skybox HDRI)
│
├── [Boundaries]
│   ├── RoomBoundary_North  (invisible wall at Z=+15)
│   ├── RoomBoundary_South  (invisible wall at Z=-15)
│   ├── RoomBoundary_East   (invisible wall at X=+21)
│   └── RoomBoundary_West   (invisible wall at X=-21)
│
├── [Gate]
│   └── RoomGate_Exit       position: (0, 0, +12) — initially CLOSED
│
└── [HUD]
    └── pfb_hud_v4
```

---

## Enemy Placement

| Enemy | Count | Spawn Position | Behavior Notes |
|---|---|---|---|
| WagonWheelRoller | 1 | (-6, 0, +2) — west flank, near wagon | Patrols west flank. Aggros when player crosses Z=0. |
| WagonWheelRoller | 1 | (+6, 0, +2) — east flank, near wagon | Patrols east flank. Aggros when player crosses Z=0. |
| WagonWheelRoller (optional) | 0–1 | (0, 0, +5) — center north | Add for 3-enemy variant. Patrols between wagons. |

**Aggro delay:** 3 seconds after `RoomManager.OnRoomActivated`. Set `EnemySpawner.aggroDelay = 3f`.
**Concurrent cap:** All 2–3 enemies active simultaneously (this is an introductory room, no wave logic needed).

---

## LevelBuilder Configuration

Create `WeaponDropTableSO_CulDeSac_Room1.asset` at `Assets/_Project/ScriptableObjects/Levels/`.

| Type | Entries | Positions |
|---|---|---|
| Weapon pickups (scattered) | 3 props | (-5, 0, -5), (0, 0, -2), (5, 0, -5) — south half of room |
| Cardboard piles | 2 | (-3, 0, +2), (3, 0, +2) — mid-street behind barrels |
| Workbench | 1 | (0, 0, -12) — south of spawn |

The 3 weapon pickups should use weapons relevant to the run's active weapon pool. Recommended starting weapons for Room 1: Broomstick, Ruler, Cardboard Tube.

---

## Props Placement Detail

### Saloon Facades (4 total)

| Instance | Position | Rotation | Notes |
|---|---|---|---|
| Saloon_A | (-12, 0, +6) | Y=90° | West flank, facing street |
| Saloon_B | (-12, 0, -2) | Y=90° | West flank, south of A |
| Saloon_C | (+12, 0, +6) | Y=-90° | East flank, facing street |
| Saloon_D | (+12, 0, -2) | Y=-90° | East flank, south of C |

Attach `pfb_env_saloon_sign_board` to Saloon_A and Saloon_C (above facades).
Attach `pfb_env_wanted_poster_blank` to 3 of the 4 facades (side walls).

### Covered Wagons (4 total — primary cover objects)

| Instance | Position | Rotation | Notes |
|---|---|---|---|
| Wagon_W1 | (-7, 0, +3) | Y=15° | West flank, near enemy spawn, slight angle |
| Wagon_W2 | (-8, 0, -3) | Y=-10° | West flank, south of W1 |
| Wagon_E1 | (+7, 0, +3) | Y=-15° | East flank, near enemy spawn |
| Wagon_E2 | (+8, 0, -3) | Y=10° | East flank, south of E1 |

Wagons are **NavMesh Obstacles** — add `NavMeshObstacle` component, carve=true, shape=Box.
Wagons are **Static** for rendering but obstacle carving must be dynamic — use `NavMeshObstacle` not static geometry.

Place `pfb_env_hitching_post` × 1 beside each wagon.

### Gallows Frame

Position: (+14, 0, +8) — far north-east background. Rotated slightly toward center. This is atmosphere only — outside the main combat zone.

### Exit Gate

- GameObject: `RoomGate_Exit` at position (0, 0, +12)
- Has `RoomGate.cs` component (from V3 migration)
- Initially: blocked (visible gate geometry or trigger zone)
- Opens on: `RoomManager.OnRoomCleared` event

---

## NavMesh Setup

1. Set the Ground plane as **Static → Navigation Static**
2. Add `NavMeshObstacle` (carve=true) to: covered wagons, saloon facades, large barrel clusters, fence sections
3. Open the Navigation window → Agents tab: confirm default agent radius = 0.4m, height = 2m
4. Bake NavMesh
5. Verify: spawn all 3 WagonWheelRollers → confirm they can path to player start position from each spawn

**Performance note:** Room 1 has no complex geometry. NavMesh bake should complete in under 30 seconds.

---

## Lighting Setup

1. **Directional Light (Sun):**
   - Color: `#FFB347` (warm amber-orange)
   - Intensity: 1.2
   - Rotation: X=50°, Y=-30°, Z=0° (long shadows pointing north-east)
   - Shadow type: Soft shadows
   - Mark as Baked Lighting contributor

2. **Ambient:**
   - Open Lighting Settings → Environment
   - Source: Color
   - Ambient color: `#4A2800` (dark warm brown — fills shadow areas with warmth, not blue)

3. **Bake lighting** after ENV is dressed — Static props only. Player, enemies, and gate are excluded.

---

## Post-Process Setup (Imagination Restore prep)

Add a **Global Volume** to the scene with a **Color Adjustments** override:
- Saturation: +15 (slightly vivid — Awakening state, not full Reclaimed)
- Color Filter: `#FFF0D8` (warm cream tint)

This will be the **before** state. When `ImaginationRestore.cs` fires after the boss in Room 5, it lerps this volume to full saturation/vividness.

---

## Unity Notes

- **Scene loading:** This scene is loaded by `GameManager` when Run starts. Set it as the initial scene in `GameManager.ZoneStartScene["CulDeSac"]`.
- **ProgressionSystem:** Already DontDestroyOnLoad from the run start — no second instance needed. Remove `pfb_ProgressionSystem` from this scene if it comes from the previous scene.
- **Camera:** `pfb_CM_FollowCam` `CameraFollowTargetInjector` auto-finds the Player by tag at runtime.
- **RoomManager config:** Set `_rooms[0].roomName = "The Arrival"`, `_rooms[0].maxConcurrentEnemies = 3`, `_rooms[0].exitGate = RoomGate_Exit`.
- **EnemySpawner config:** `aggroDelay = 3f`, `spawnOnRoomActivation = true`.
- **UpgradeScreen:** After `RoomManager.OnRoomCleared` fires → `GameManager.HandleRoomCleared()` → shows `UpgradeScreen`. Wire this in `GameManager`.

---

## Performance Budget

| Category | Target | Notes |
|---|---|---|
| Total triangles | < 150,000 | Well under 300k scene limit |
| Draw calls | < 60 | GPU instancing on Polyworks atlas covers most props |
| Shadow casters | Buildings + wagons only | Disable shadows on small scatter props |
| Dynamic objects | Player + 3 enemies | Everything else is Static |

---

*Blueprint owner: Louie Celli | Created: 2026-08-05 | Hand to `unity-senior-developer` after `/level-design` review.*
