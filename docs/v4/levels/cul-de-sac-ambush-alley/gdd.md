# GDD: Cul-de-Sac — Room 2 "Ambush Alley"

**Zone:** The Cul-de-Sac (Zone 1)
**Room index:** 2 of 5 (random-draw pool — one of three rooms drawn after Room 1)
**Version:** 1.0
**Date:** 2026-08-05
**Status:** Locked — ready for implementation

---

## 1. Room Overview

**Room name:** Ambush Alley
**Purpose:** First escalation. Room 1 gave the player open space and a gentle aggro delay. Ambush Alley removes both — all three Rollers are active at spawn, and the narrower layout prevents the open-field kiting that kept Room 1 safe. The player must think in three directions simultaneously for the first time.

**Size:** 28m wide × 32m long
**Layout:** Two wagon/barrel clusters at x=±10 create a forced central corridor 8m wide. Flanks offer cover but not safety — each cover piece blocks one enemy's sightline while exposing the player to another. The only truly safe space is the workbench zone at the south end.

---

## 2. Narrative Setup

Kid turns down a side branch of the cul-de-sac. The houses crowd closer here. Someone has pushed wagons and crates against the buildings in patterns no person would choose — deliberate, geometric, wrong.

The Unimaginative don't set traps out of cruelty. They set them out of efficiency. The WagonWheelRollers were positioned here the same way traffic cones are positioned on a highway — to control the flow of things they don't want.

Kid doesn't know that. What Kid sees is a canyon ambush — boulder piles flanking a narrow pass, mine-cart barricades sealing the sides, three shapes waiting in the amber haze ahead. The only way through is through.

---

## 3. Visual Tone

| Element | Description |
|---|---|
| Sky | Same burnt amber as Room 1 — continuous zone palette |
| Ground | Cracked asphalt, slightly more dirt accumulation in the narrower alley |
| Lighting | Warm golden directional light, more shadow from closer buildings |
| Shadow color | Warm burnt sienna — same as Room 1 |
| Atmosphere | No tumbleweed drift — this room is static, waiting. The stillness is the tell. |
| Imagination state | Awakening (same as Room 1) — the zone is not yet reclaimed |

---

## 4. Special Mechanic: Simultaneous Spawn, No Delay

When the player enters the room, all 3 WagonWheelRollers activate immediately — no patrol phase, no aggro delay. This is the defining departure from Room 1.

**Why this works:** Room 1 gave the player 3 seconds to read the space. By Room 2 the player knows what a WagonWheelRoller looks like. The pedagogical purpose of the delay is complete. Removing it raises the stakes without introducing anything mechanically new.

**The geometry trap:** The two flank wagon clusters at x=±10 are positioned so that dodge-rolling west puts the player between Wagon_W and the west roller, and dodge-rolling east does the same on the opposite side. The "safe" play is to attack the center roller first and use the central corridor to manage spacing. Players who try to kite to a flank get caught in a two-on-one pinch.

**Implementation:** `EnemySpawner.aggroDelay = 0f`. All three EnemySpawnPoints active at room start.

---

## 5. Forge Workbench

**Position:** (-9, 0, -13) — south-west corner behind the first wagon cluster, outside the combat zone.

**Purpose:** Player carries weapons from Room 1. This workbench lets them upgrade an existing weapon or forge a new raw object before committing to the fight.

---

## 6. Enemy Encounter

**Enemy type:** WagonWheelRoller × 3 — all spawn simultaneously on room activation. No wave logic.

| Enemy | Spawn Position | Behavior |
|---|---|---|
| Roller_North | (0, 0, +8) | Center-north. Charges straight south toward player. |
| Roller_West | (−8, 0, +2) | West-flank. Patrols west cover cluster, aggros on sight. |
| Roller_East | (+8, 0, +2) | East-flank. Patrols east cover cluster, aggros on sight. |

**Room clear condition:** All 3 WagonWheelRollers dead → `RoomManager.OnRoomCleared` → exit gate opens → `UpgradeScreen` shown.

---

## 7. Cardboard Drops

| Enemy | Drop |
|---|---|
| WagonWheelRoller | 1–2 per kill |

2 cardboard piles pre-placed behind barrels at (−4, 0, +3) and (+4, 0, +3). Each pile: 5 cardboard.

---

## 8. ENV Props List

All props from the existing Polyworks pack. No Meshy orders required.

### Primary Cover (NavMesh Obstacles)
| Prop | Count | Purpose |
|---|---|---|
| `pfb_env_covered_wagon` | 4 | Two clusters (x=±10), create east and west cover corridors |
| `pfb_env_rain_barrel` | 4 | Scattered near wagons — secondary cover, small profile |
| `pfb_env_stacked_crates` | 2 | Mid-room center, subtle obstruction to force player positioning |

### Street Dressing (Static)
| Prop | Count | Purpose |
|---|---|---|
| `pfb_env_bld_twostoryhouse` | 2 | West flank building face (atmosphere, boundary) |
| `pfb_env_bld_shedwithcrate` | 2 | East flank building face |
| `pfb_env_hitching_post` | 2 | Beside west wagons |
| `pfb_env_mailbox_telegraph` | 2 | East side, between buildings |
| `pfb_env_lamp_post_western` | 2 | Mid-street north, flanking exit gate |
| `pfb_env_wanted_poster_blank` | 2 | On building walls |
| `pfb_env_broken_wagon_wheel` | 2 | Ground scatter near wagons |
| `pfb_env_rope_coil` | 2 | Ground scatter, west side |

### Craftsmanship ENV Dressing

Four background props that tell a human story without labels. All at room perimeter — non-interactive.

1. **A child's bicycle** leaning against the east building, rear wheel missing. The front wheel still has a playing card clothes-pinned to the spoke. The card is a joker.

2. **A garden hose** wound around a rusted wall bracket on the west building. The nozzle end is aimed at a dead flower bed that never got watered on the last day.

3. **A pair of muddy boots** left outside a door on the east side. One boot is tipped over. Nobody came back for them.

4. **A chalkboard** sitting just inside an open window on the west building. The writing is partially erased — only a single line of numbers survives. They look like a phone number.

---

## 9. Zone Lore Hook

Ambush Alley is where Kid first understands that The Unimaginative planned ahead. Room 1 felt like a frontier town mid-siege. Ambush Alley feels like a trap that was set before Kid arrived — and probably for Kid specifically. The Unimaginative don't improvise. They anticipated.

This room plants the seed: someone has been watching.

---

## 10. Difficulty Scaling

**HP tuning:** Same as Room 1 (Zone 1 baseline — no scaling between rooms within the same zone).
**Pressure increase from Room 1:** From 2 sequential Rollers → 3 simultaneous Rollers. The increase is in cognitive load, not raw stats.
**Fail state probability:** Higher than Room 1, but survivable with the parry timing learned in Room 1.

---

## 11. Implementation Notes for Unity

- `EnemySpawner.aggroDelay = 0f` — critical difference from Room 1
- All 3 EnemySpawnPoints in `RoomManager._rooms[0].spawnPoints`
- Covered wagons: `NavMeshObstacle` with `carve=true`
- Stacked crates: Static geometry (small — no NavMeshObstacle needed)
- Workbench at (−9, 0, −13), south-west corner
- Scene name: `CulDeSac_AmbushAlley.unity`

---

## 12. Open Questions

| # | Question | Impact | Status |
|---|---|---|---|
| 1 | Should Roller_North spawn already moving or use the standard windup-then-charge? Instant movement is more ambush-appropriate but may feel cheap on first encounter. | Difficulty / fairness | ❓ Open |
| 2 | The stacked crates obstruct the center corridor — do they need NavMesh obstacles? If enemy AI can't path around them, the center roller may get stuck. | AI pathfinding | ❓ Open |
| 3 | Should mid-combat cardboard piles reward aggressive play (dangerous pickup mid-fight) or be pre-collected before the fight? Current placement in the mid-street pushes them into the danger zone. | Economy design | ❓ Open |

---

*Room design owner: Louie Celli | Created: 2026-08-05 | Sprint 3 Phase 2*
