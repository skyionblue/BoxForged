# Unity Blueprint: Cul-de-Sac Room 3 "Saloon Front"

**Scene file:** `Assets/_Project/Scenes/CulDeSac_SaloonFront.unity`
**WeaponDropTableSO:** `Assets/_Project/ScriptableObjects/Levels/WeaponDropTableSO_CulDeSac_SaloonFront.asset`
**Template:** Duplicate `CulDeSac_Room1.unity` hierarchy. Adjust ground scale, enemy spawns, and drop table.

---

## Layout Overview

36m wide × 30m long — slightly wider than Room 1, same depth. The west flank is defined by the prominent saloon facade. Center is open plaza. East flank has porch cabins. Two Marshals anchor the mid-room on either side of the central axis. The Roller starts at the far north.

**Coordinate system:** Player spawns at (0, 0, −12). Exit gate at (0, 0, +13). Positive Z = north.

---

## ASCII Layout (top-down, 1 cell ≈ 4m)

```
W W W W W W W W W W   ← north boundary
W . . . [G] . . . . W  ← exit gate center
W . L . . . . L . . W  ← lamp posts
W . . . . E . . . . W  ← Roller_North (0, 0, +10)
W S . b . . . b . . W  ← saloon west, barrel east
W S . M . . M . C . W  ← Marshal_West (-5), Marshal_East (+5), wagon east
W . . . . . . . C . W  ← wagon east lower
W b . . . . . . b . W  ← barrel scatter
W . . . . . . . . . W
W . . . WB . . . . W   ← workbench SE (9, 0, -12)
W . . . . P . . . . W  ← player spawn
W W W W W W W W W W   ← south boundary

Legend:
  P  = Player spawn (0, 0, -12)
  WB = Workbench (9, 0, -12)
  G  = Exit gate (0, 0, +13)
  M  = Marshal spawn
  E  = Roller spawn
  S  = Saloon facade (west, x=-15)
  C  = Covered wagon (east, x=+10)
  b  = Barrel
  L  = Lamp post
```

---

## Enemy Placement

| Enemy | Count | Spawn Position | Behavior |
|---|---|---|---|
| Marshal_West | 1 | (−5, 0, +4) | Walks south, wide swing, parryable |
| Marshal_East | 1 | (+5, 0, +4) | Same. 10m spacing from Marshal_West |
| Roller_North | 1 | (0, 0, +10) | Charges south at speed |

**aggroDelay:** `1f`
**maxConcurrentEnemies:** `3`

---

## WeaponDropTableSO Configuration

Duplicate `WeaponDropTableSO_CulDeSac_Room1.asset` → rename to `WeaponDropTableSO_CulDeSac_SaloonFront.asset`.

### envProps entries

**Buildings**
| Prefab | Position | Euler Y |
|---|---|---|
| `pfb_env_saloon_facade` | (−15, 0, 5) | 90 |
| `pfb_env_saloon_facade` | (−15, 0, −3) | 90 |
| `pfb_env_bld_porchcabin` | (15, 0, 5) | −90 |
| `pfb_env_bld_porchcabin` | (15, 0, −3) | −90 |

**Sign board**
| Prefab | Position | Euler Y |
|---|---|---|
| `pfb_env_saloon_sign_board` | (−15, 3, 5) | 90 |

**Covered Wagons (2) — east flank only; saloon facade serves as west cover**
| Prefab | Position | Euler Y |
|---|---|---|
| `pfb_env_covered_wagon` | (11, 0, 3) | −20 |
| `pfb_env_covered_wagon` | (11, 0, −3) | 15 |

**Barrels (4)**
- (−13, 0, 3), (−13, 0, −1), (12, 0, 4), (12, 0, −2)

**Hitching Posts (3)**
- (−13, 0, 5), (−13, 0, 1), (12, 0, 1)

**Water Trough (1)**
- (−11, 0, 2) — in front of saloon, verify clear of Marshal_West patrol path

**Lamp Posts (2)**
- (−5, 0, 9), (5, 0, 9)

**Wanted Posters (3)**
- (−14.5, 1.5, 4) Y=90, (−14.5, 1.5, −2) Y=90, (14.5, 1.5, 4) Y=−90

**Gallows (1)**
- (16, 0, 9) Y=160

**Tumbleweeds (2)**
- (−7, 0, −5), (6, 0, −8) — scale (0.25, 0.25, 0.25)

**Rope Coil (1)**
- (−12, 0, 0)

### workbenchPositions
- `(9, 0, −12)`

### cardboardPiles
- `(−6, 0, −2)` amount: 5
- `(6, 0, −2)` amount: 5

### scatteredObjects
- Broomstick at (−5, 0, −7)
- FoamSword at (5, 0, −7)
- CardboardTube at (0, 0, −4)

---

## NavMesh Setup

- Ground: Navigation Static
- Both covered wagons: `NavMeshObstacle` carve=true
- Saloon facades: Navigation Static (buildings act as walls)
- Water trough: Static only — small, verify doesn't block Marshal path
- Bake after all props spawn; test Marshal patrol paths

---

## Performance Budget

| Category | Target |
|---|---|
| Total triangles | < 130,000 |
| Draw calls | < 55 |
| Dynamic objects | Player + 3 enemies |

---

## Unity Notes

- `aggroDelay = 1f` — do not set to 0. The one-second pause is the only moment the player gets to register MilepostMarshal before combat.
- Marshal spawns are intentionally close to mid-room (z=+4), not near the exit gate. This limits the Roller's acceleration runway to ~6m before it reaches the player.
- Saloon facade at x=−15 acts as the west boundary wall — no separate invisible boundary wall needed on the west side if the saloon geometry fills the gap.
- Scene name: `CulDeSac_SaloonFront.unity`

---

*Blueprint owner: Louie Celli | Created: 2026-08-05 | Sprint 3 Phase 2*
