# Unity Blueprint: Cul-de-Sac Room 2 "Ambush Alley"

**Scene file:** `Assets/_Project/Scenes/CulDeSac_AmbushAlley.unity`
**WeaponDropTableSO:** `Assets/_Project/ScriptableObjects/Levels/WeaponDropTableSO_CulDeSac_AmbushAlley.asset`
**Template:** Copy structure from `CulDeSac_Room1.unity` — same manager stack, same lighting setup, same boundary system.

---

## Layout Overview

28m wide × 32m long. Narrower than Room 1 by 12m. Two dense prop clusters at x=±10 divide the room into three vertical zones: west gutter, 8m central corridor, east gutter. The central corridor is the combat spine. The gutters are danger zones, not safe zones.

**Coordinate system:** Player spawns at (0, 0, −12). Exit gate at (0, 0, +14). Positive Z is north (toward exit).

---

## ASCII Layout (top-down, 1 cell ≈ 4m)

```
W W W W W W W W   ← north boundary (z=+16)
W . . [G] . . W   ← exit gate center-north
W . L . . L . W   ← lamp posts flanking gate
W . . . . . . W
W b . E . . b W   ← barrels, Roller_East right, Roller_West left
W [C] . . . [C] W  ← wagon cluster west & east (cover)
W . . E . . . W   ← Roller_North center
W [C] . . . [C] W  ← second wagon pair, mid-room
W . . . . . . W
W s . . . . s W   ← crate scatter mid-room
W . . . . . . W
W WB . . . . . W  ← workbench SW corner
W . . . P . . W   ← player spawn (0, 0, -12)
W W W W W W W W   ← south boundary (z=-16)

Legend:
  P  = Player spawn
  WB = Forge Workbench (-9, 0, -13)
  G  = Exit Gate (0, 0, +14)
  E  = Enemy spawn
  [C] = Covered wagon cluster (cover object)
  b  = Barrel
  s  = Stacked crates
  L  = Lamp post
  W (outer) = Boundary
```

---

## Scene Hierarchy

Copy `CulDeSac_Room1.unity` hierarchy exactly. Change:
- Scene name → `CulDeSac_AmbushAlley`
- `LevelBuilder._dropTable` → `WeaponDropTableSO_CulDeSac_AmbushAlley`
- `EnemySpawnPoints` positions (see below)
- `EnemySpawner.aggroDelay = 0f` (no delay — key difference from Room 1)

---

## Enemy Placement

| Enemy | Count | Spawn Position | Behavior |
|---|---|---|---|
| Roller_North | 1 | (0, 0, +8) | Center-north. Direct south charge. |
| Roller_West | 1 | (−8, 0, +2) | West flank. Patrols west cluster. |
| Roller_East | 1 | (+8, 0, +2) | East flank. Patrols east cluster. |

**EnemySpawnPoints:** 3 entries in `RoomManager._rooms[0].spawnPoints`
**aggroDelay:** `0f` — all activate immediately on room start
**maxConcurrentEnemies:** `3`

---

## WeaponDropTableSO Configuration

Create `WeaponDropTableSO_CulDeSac_AmbushAlley.asset` by duplicating `WeaponDropTableSO_CulDeSac_Room1.asset`.

### envProps entries

**Buildings (4) — boundary walls**
| Prefab | Position | Euler Y |
|---|---|---|
| `pfb_env_bld_twostoryhouse` | (−14, 0, 4) | 90 |
| `pfb_env_bld_twostoryhouse` | (−14, 0, −5) | 90 |
| `pfb_env_bld_shedwithcrate` | (14, 0, 4) | −90 |
| `pfb_env_bld_shedwithcrate` | (14, 0, −5) | −90 |

**Covered Wagons (4) — primary cover, NavMeshObstacle**
| Prefab | Position | Euler Y |
|---|---|---|
| `pfb_env_covered_wagon` | (−10, 0, 5) | 20 |
| `pfb_env_covered_wagon` | (−10, 0, −2) | −15 |
| `pfb_env_covered_wagon` | (10, 0, 5) | −20 |
| `pfb_env_covered_wagon` | (10, 0, −2) | 15 |

**Barrels (4)**
| Prefab | Position |
|---|---|
| `pfb_env_rain_barrel` | (−12, 0, 3) |
| `pfb_env_rain_barrel` | (−12, 0, −1) |
| `pfb_env_rain_barrel` | (12, 0, 3) |
| `pfb_env_rain_barrel` | (12, 0, −1) |

**Stacked Crates (2) — mid-room center obstruction**
| Prefab | Position |
|---|---|
| `pfb_env_stacked_crates` | (−3, 0, 0) |
| `pfb_env_stacked_crates` | (3, 0, 0) |

**Hitching Posts (2)**
- (−11, 0, 5.5) and (−11, 0, −1.5)

**Mailboxes (2)**
- (13, 0, 2) and (13, 0, −4)

**Lamp Posts (2)**
- (−5, 0, 10) and (5, 0, 10)

**Wanted Posters (2)**
- (−13.5, 1.5, 3) Y=90 and (13.5, 1.5, 3) Y=−90

**Broken Wagon Wheels (2)**
- (−11, 0, 0) and (11, 0, −1)

**Rope Coils (2)**
- (−9, 0, 3) and (−8.5, 0, −3)

### workbenchPositions
- `(−9, 0, −13)`

### cardboardPiles
- `(−4, 0, 3)` amount: 5
- `(4, 0, 3)` amount: 5

### scatteredObjects (weapon pickups)
- Same 3 weapons as Room 1 (Broomstick, FoamSword, CardboardTube) at positions (−5, 0, −7), (5, 0, −7), (0, 0, −4)

---

## Lighting

Identical to `CulDeSac_Room1.unity`:
- Directional Light: `#FFB347`, intensity 0.9, rotation (50, −30, 0)
- Ambient: flat `#4A2800`
- Global Volume: Saturation +15, Color Filter `#FFF0D8`

No lighting changes between Cul-de-Sac rooms — same zone, same time of day.

---

## NavMesh Setup

1. Ground plane: Navigation Static
2. All 4 covered wagons: `NavMeshObstacle` carve=true, shape=Box
3. Stacked crates: Static geometry only — small enough to not block NavMesh significantly
4. Buildings: Navigation Static
5. Bake NavMesh after all env props are spawned in play mode test

---

## Performance Budget

| Category | Target |
|---|---|
| Total triangles | < 120,000 |
| Draw calls | < 50 |
| Dynamic objects | Player + 3 enemies |
| Static | Ground + all ENV |

---

## Unity Notes

- **aggroDelay must be 0** — this is not a mistake. Do not add a delay for "feel" without design approval.
- The stacked crates at (±3, 0, 0) should NOT have NavMeshObstacle — enemies need to path through the center. If they cause pathfinding issues in testing, remove them or move to the room flanks.
- The workbench is outside the gate's combat zone by design. Do not move it north of z=−10.
- After NavMesh bake, test all 3 enemy spawn points to confirm they can path to player spawn (0, 0, −12) without getting stuck on wagons.

---

*Blueprint owner: Louie Celli | Created: 2026-08-05 | Sprint 3 Phase 2*
