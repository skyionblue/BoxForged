# GDD: Cul-de-Sac — Room 3 "Saloon Front"

**Zone:** The Cul-de-Sac (Zone 1)
**Room index:** 3 of 5 (random-draw pool)
**Version:** 1.0
**Date:** 2026-08-05
**Status:** Locked — ready for implementation

---

## 1. Room Overview

**Room name:** Saloon Front
**Purpose:** Enemy type introduction. First encounter with the MilepostMarshal. The player must adapt to a grounded melee fighter that rewards parrying — while simultaneously managing a mobile WagonWheelRoller that punishes standing still. The room teaches two-threat prioritization across different enemy archetypes.

**Size:** 36m wide × 30m long
**Layout:** Wide central plaza with saloon facades prominent on the west flank. Two Marshals positioned at mid-room with spacing designed to prevent the player from engaging both simultaneously. The Roller starts at the north end with room to build speed before reaching the player.

---

## 2. Narrative Setup

The street opens into a wider plaza — the cul-de-sac's closest thing to a town square. Two MilepostMarshal units stand in front of the saloon facades like deputies who never received the order to stand down.

The Unimaginative assigned them here because this is a convergence point — where the side branches of the cul-de-sac meet the main street. Control the intersection, control the zone.

Kid sees: a saloon showdown. Two gunslingers blocking the entrance. A third shape — the Roller — already in motion at the far end of the street. Classic High Noon geometry. The Marshals hold the center. The Roller covers the exit.

---

## 3. Visual Tone

| Element | Description |
|---|---|
| Sky | Burnt amber — same as all Cul-de-Sac rooms |
| Ground | Cracked asphalt with more terracotta dust in the wider plaza |
| Lighting | Warm golden-hour light. Long shadows from the saloon facade on the west flank |
| Shadow color | Warm burnt sienna |
| Atmosphere | Wide and exposed. The openness after Ambush Alley should feel like relief — then the Marshals move |
| Imagination state | Awakening — same zone, not yet reclaimed |

---

## 4. Special Mechanic: Dual Archetype Pressure

The MilepostMarshal is a grounded melee brawler — it walks toward the player, telegraphs a wide swing, and rewards the parry with a clean counter window. The WagonWheelRoller charges at speed from a distance.

**The design tension:** Parrying a Marshal requires staying close. The Roller punishes staying in one spot. The player must pick an order, isolate one threat, and manage the other with movement.

**Teaching objective:** The player learns to read two enemy types at once. Killing the Roller first removes the mobile threat but leaves two melee fighters. Killing a Marshal first removes parry pressure but gives the Roller more room to accelerate.

**Implementation:** `EnemySpawner.aggroDelay = 1f` — one second to let the player register the new enemy type before the fight begins.

---

## 5. Forge Workbench

**Position:** (9, 0, −12) — south-east corner, offset from center so the southern safe zone stays clear.

---

## 6. Enemy Encounter

| Enemy | Spawn Position | Behavior |
|---|---|---|
| Marshal_West | (−5, 0, +4) | Walks toward player. Wide swing. Parryable. |
| Marshal_East | (+5, 0, +4) | Same behavior. Spaced 10m from Marshal_West. |
| Roller_North | (0, 0, +10) | Charges south. Circulates after impact. |

**Aggro delay:** `1f` second.
**Max concurrent:** 3.
**Room clear condition:** All 3 dead → `RoomManager.OnRoomCleared` → exit gate opens → `UpgradeScreen`.

---

## 7. Cardboard Drops

| Enemy | Drop |
|---|---|
| MilepostMarshal | 2–3 per kill |
| WagonWheelRoller | 1–2 per kill |

2 cardboard piles at (−6, 0, −2) and (6, 0, −2). Each: 5 cardboard.

---

## 8. ENV Props List

All from existing Polyworks pack.

### Primary Cover
| Prop | Count | Purpose |
|---|---|---|
| `pfb_env_saloon_facade` | 2 | West flank — prominent landmark |
| `pfb_env_covered_wagon` | 2 | Mid-room flanks — cover, NavMesh obstacle |
| `pfb_env_rain_barrel` | 4 | Near saloon and wagon — secondary cover |

### Street Dressing
| Prop | Count | Purpose |
|---|---|---|
| `pfb_env_bld_porchcabin` | 2 | East flank buildings |
| `pfb_env_saloon_sign_board` | 1 | Above west saloon |
| `pfb_env_hitching_post` | 3 | Beside wagons and saloon |
| `pfb_env_water_trough` | 1 | In front of saloon |
| `pfb_env_lamp_post_western` | 2 | North section, flanking exit gate |
| `pfb_env_wanted_poster_blank` | 3 | On saloon walls and east building |
| `pfb_env_gallows_frame` | 1 | Far north-east corner — atmosphere |
| `pfb_env_tumbleweed_static` | 2 | South section scatter |
| `pfb_env_rope_coil` | 1 | Near saloon entrance |

### Craftsmanship ENV Dressing

1. **A rocking chair** on the saloon porch, still moving slightly when Kid arrives. Nobody pushed it. The boards creak.

2. **A sheriff's star** lying in the dust at the base of a hitching post — the pin-back still attached. It caught the light when it fell. No one picked it up.

3. **A half-eaten apple** on the water trough edge, browning. It was placed there intentionally. Someone got interrupted mid-bite.

4. **A music box** visible through a cracked saloon window, lid open. It isn't playing. Whatever wound it had has run out.

---

## 9. Zone Lore Hook

The Marshals were posted at the intersection before Kid arrived. They didn't react to movement, noise, or light — only to the specific shape of a child with a cardboard box on their head. The Unimaginative didn't send them for the zone. They sent them for Kid.

This room confirms what Ambush Alley hinted at: Kid is being tracked.

---

## 10. Difficulty Scaling

**From Room 2:** Three simultaneous Rollers → two Marshals + one Roller. Enemy count drops but complexity increases — two archetypes requiring different responses simultaneously.
**New mechanic:** MilepostMarshal's telegraphed wide swing. Players who spam dodge are punished. Parry is the correct response; this room teaches it under pressure.

---

## 11. Implementation Notes for Unity

- `EnemySpawner.aggroDelay = 1f`
- 3 EnemySpawnPoints in `RoomManager._rooms[0].spawnPoints`
- Marshal spawn positions spaced 10m apart — prevents simultaneous aggro from both
- Saloon facade: `pfb_env_saloon_facade` at x=−15, Y=90 rotation
- Water trough in front of saloon at approximately (−12, 0, +3) — verify doesn't block Marshal AI
- Scene name: `CulDeSac_SaloonFront.unity`

---

## 12. Open Questions

| # | Question | Impact | Status |
|---|---|---|---|
| 1 | Should Marshal_West and Marshal_East aggro simultaneously or stagger by 0.5s? Staggered gives the player a moment to focus on one first. | First-encounter difficulty | ❓ Open |
| 2 | Water trough near Marshal_West patrol path — does it obstruct enemy AI pathfinding? | AI pathfinding | ❓ Open |
| 3 | Saloon facade at x=−15 — does it block minimap camera view of the west arena half? | Minimap visibility | ❓ Open |

---

*Room design owner: Louie Celli | Created: 2026-08-05 | Sprint 3 Phase 2*
