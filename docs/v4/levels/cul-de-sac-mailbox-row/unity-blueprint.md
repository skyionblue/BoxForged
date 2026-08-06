# Unity Blueprint: Cul-de-Sac Room 4 "Mailbox Row"

**Scene file:** `Assets/_Project/Scenes/CulDeSac_MailboxRow.unity`
**WeaponDropTableSO:** `Assets/_Project/ScriptableObjects/Levels/WeaponDropTableSO_CulDeSac_MailboxRow.asset`
**Template:** Duplicate `CulDeSac_Room1.unity`. Adjust ground scale to 32×34m, update enemy spawns, drop table.

---

## Layout Overview

32m wide × 34m long. Long residential street. Marshals at the far north. Roller behind the player at the south. Two rows of mailboxes define the mid-room corridor. The single covered wagon at the north end is the only heavy cover — everything else is mailboxes and barrels.

**Coordinate system:** Player spawns at (0, 0, −12). Exit gate at (0, 0, +15). Roller_South spawns at (0, 0, −16) — behind player.

---

## ASCII Layout (top-down, 1 cell ≈ 4m)

```
W W W W W W W W W   ← north boundary (z=+17)
W . . [G] . . . W   ← exit gate center
W . T . . . T . W   ← tumbleweeds
W . . M . M . . W   ← Marshal_West (-4, +10), Marshal_East (+4, +10)
W . [C] . . . . W   ← covered wagon north-west
W m . . . . . m W   ← mailbox row mid
W m . . . . . m W   ← mailbox row mid
W m . . . . . m W   ← mailbox row south
W . b . . . b . W   ← barrels near mailboxes
W . . . . . . . W
W . . . P . . . W   ← player spawn (0, 0, -12)
W WB . . . . . . W  ← workbench SW (-10, 0, -14)
W . . . R . . . W   ← Roller_South (0, 0, -16) — behind player
W W W W W W W W W   ← south boundary (z=-17)

Legend:
  P  = Player spawn (0, 0, -12)
  WB = Workbench (-10, 0, -14)
  G  = Exit gate (0, 0, +15)
  M  = Marshal spawn
  R  = Roller_South spawn — BEHIND player
  [C] = Covered wagon
  m  = Mailbox
  b  = Barrel
  T  = Tumbleweed
```

---

## Enemy Placement

| Enemy | Count | Spawn Position | Notes |
|---|---|---|---|
| Marshal_West | 1 | (−4, 0, +10) | North end, walks south |
| Marshal_East | 1 | (+4, 0, +10) | North end, 8m from Marshal_West |
| Roller_South | 1 | (0, 0, −16) | **Behind player spawn** — charges north |

**aggroDelay:** `2f` — critical. Player must see the Roller is behind them.
**maxConcurrentEnemies:** `3`

---

## WeaponDropTableSO Configuration

Duplicate `WeaponDropTableSO_CulDeSac_Room1.asset` → rename to `WeaponDropTableSO_CulDeSac_MailboxRow.asset`.

### envProps entries

**Buildings (4)**
| Prefab | Position | Euler Y |
|---|---|---|
| `pfb_env_bld_twostoryhouse` | (−14, 0, 5) | 90 |
| `pfb_env_bld_twostoryhouse` | (−14, 0, −4) | 90 |
| `pfb_env_bld_porchcabin` | (14, 0, 5) | −90 |
| `pfb_env_bld_porchcabin` | (14, 0, −4) | −90 |

**Covered Wagon (1) — north end only**
| Prefab | Position | Euler Y |
|---|---|---|
| `pfb_env_covered_wagon` | (−9, 0, 8) | 20 |

**Mailboxes (6) — 3 per side, defining the room**

West row (x=−10):
- (−10, 0, 4), (−10, 0, 0), (−10, 0, −4)

East row (x=+10):
- (10, 0, 4), (10, 0, 0), (10, 0, −4)

**Barrels (4)**
- (−11, 0, 3), (−11, 0, −1), (11, 0, 3), (11, 0, −1)

**Hitching Posts (2)**
- (−10, 0, 9), (−8, 0, 7)

**Lamp Posts (2)**
- (−5, 0, 2), (5, 0, 2)

**Wanted Posters (2)**
- (−13.5, 1.5, −3) Y=90, (13.5, 1.5, −3) Y=−90

**Broken Wagon Wheels (3)**
- (−8, 0, −2), (7, 0, 3), (−6, 0, 6)

**Rope Coils (2)**
- (−9, 0, −5), (−9.5, 0, −9)

**Tumbleweeds (2)**
- (−5, 0, 12), (4, 0, 11) — scale (0.25, 0.25, 0.25)

### workbenchPositions
- `(−10, 0, −14)`

### cardboardPiles
- `(−8, 0, 0)` amount: 5
- `(8, 0, 0)` amount: 5

### scatteredObjects
- Broomstick at (−5, 0, −7)
- FoamSword at (5, 0, −7)
- CardboardTube at (0, 0, −5)

---

## NavMesh Setup

- Ground: Navigation Static
- Covered wagon (north): `NavMeshObstacle` carve=true
- Mailboxes: Static only — 1m tall, treat as minor obstacles, no NavMeshObstacle
- Buildings: Navigation Static
- After bake: verify Roller_South at z=−16 can path north through the mailbox corridor to reach Marshals and player

---

## Performance Budget

| Category | Target |
|---|---|
| Total triangles | < 110,000 |
| Draw calls | < 45 |
| Dynamic objects | Player + 3 enemies |

---

## Unity Notes

- **Roller_South SpawnPoint must be at z=−16** — south of player spawn (z=−12). This is intentional and correct. Do not move it north.
- The 2-second aggro delay is the longest in any room. Do not reduce without design approval.
- Only one covered wagon in this room. If NavMesh testing shows the Marshals cannot path around it, move it to the north edge (z=+13) out of the combat zone.
- Ground plane scale: (3.2, 1, 3.4) to give 32×34m coverage.
- Scene name: `CulDeSac_MailboxRow.unity`

---

*Blueprint owner: Louie Celli | Created: 2026-08-05 | Sprint 3 Phase 2*
