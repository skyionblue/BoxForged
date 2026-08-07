# GDD: Cul-de-Sac — Room 1 "The Arrival"

**Zone:** The Cul-de-Sac (Zone 1)
**Room index:** 1 of 5 (fixed — always the first room)
**Version:** 1.0
**Date:** 2026-08-05
**Status:** Locked — ready for implementation

---

## 1. Room Overview

**Room name:** The Arrival
**Purpose:** Orientation. The player's first moment in the Cul-de-Sac. Combat is secondary — the environment is the star. This room teaches navigation and dodge timing without overwhelming the player.

**Size:** 40m wide × 30m long
**Layout:** Mostly open street with soft cover on the flanks. One central corridor runs the full length. Player enters from the south end; the exit gate is at the north end, visible from spawn.

---

## 2. Narrative Setup

Kid rounds the corner and the cul-de-sac opens up wide and amber and enormous. The minivans are covered wagons. The houses are two-story saloons. The birdbath at the far end is a water trough. The sky is the color of old pennies.

He's never been to a real Main Street. He doesn't need to have been.

The room introduces Kid to the zone before it introduces danger. Three seconds of pure atmosphere before anything moves toward him. That three seconds is the design.

---

## 3. Visual Tone

| Element | Description |
|---|---|
| Sky | Burnt amber. Golden hour. No clouds. |
| Ground | Cracked asphalt. Dusty terracotta texture. Dirt patches and dead grass circles. |
| Lighting | Permanent warm directional light (golden hour). Long shadows pointing north. No cool tones anywhere in the room. |
| Shadow color | Warm burnt sienna — never grey or blue. |
| Atmosphere | Tumbleweed drifts across the street during the 3-second delay. |
| Imagination state | Awakening — warm saturation, outlines visible, everything has begun to take its imagined form. |

---

## 4. Special Mechanic: The 3-Second Delay

When the player enters the room, a 3-second timer starts before WagonWheelRollers aggro. During this window:
- All enemies patrol slowly in their spawn zones
- The camera shows the full street ahead
- Tumbleweeds drift (particle or animated static props)
- No combat UI elements appear yet

**Purpose:** Let the player absorb the visual transformation before the fight begins. The environment makes the first impression — combat is confirmation, not introduction.

**Implementation:** `EnemySpawner` waits `aggroDelay = 3f` seconds after `RoomManager.OnRoomActivated` before enabling enemy AI aggro.

---

## 5. Forge Workbench

A `pfb_workbench` is placed at the room entrance (south end), in the safe zone behind the spawn point. The player can interact with it before advancing into the street.

**Position:** (0, 0, -12) — just south of the player spawn at (0, 0, -10).
**Purpose:** Lets the player forge any raw objects they carried from a previous session. In a fresh run the bag is empty, but the workbench teaches the mechanic.
**Trigger radius:** 3m.

---

## 6. Enemy Encounter

**Enemy type:** WagonWheelRoller (2–3)
**Spawn positions:** See Unity Blueprint.
**Aggro delay:** 3 seconds after room activation.
**Behavior:** Patrol slowly in their zones → aggro on player enter range → wind-up spin → charge.

**No ranged enemies. No ambushes. No elite enemies.**

This encounter exists to teach movement and dodge timing. The WagonWheelRoller's charge is parryable and has a clear telegraph. Room 1 is where the player learns to read it.

**Room clear condition:** All WagonWheelRollers dead → `RoomManager.OnRoomCleared` fires → exit gate opens → `UpgradeScreen` shown.

---

## 7. Cardboard Drops

| Enemy | Cardboard drop |
|---|---|
| WagonWheelRoller | 1–2 per kill |

**Cardboard piles (pre-placed by LevelBuilder):**
- 2 cardboard piles (`pfb_pickup_cardboard`) — placed behind barrels mid-street
- Each pile: 5 cardboard

**Total cardboard available from Room 1:** ~13–18 (kill drops + piles). Enough for one Standard forge (2 cardboard) with plenty left over.

---

## 8. ENV Props List

All props are available from existing project assets. No new Meshy orders required for the structural props.

### V3 Migrated Props (already in project)

| Prop | Count | Purpose |
|---|---|---|
| `pfb_env_saloon_facade` | 4 | Building facades on east and west flanks |
| `pfb_env_covered_wagon` | 4 | Soft cover, mid-street and flanks |
| `pfb_env_hitching_post` | 4 | Tied to wagon positions |
| `pfb_env_lamp_post_western` | 2 | Flanking the central street, north section |
| `pfb_env_rain_barrel` | 3 | Scatter near saloon facades |
| `pfb_env_mailbox_telegraph` | 2 | East flank, between saloons |
| `pfb_env_tumbleweed_static` | 4 | Mid-street scatter + north edge |
| `pfb_env_wanted_poster_blank` | 3 | On saloon walls |
| `pfb_env_saloon_sign_board` | 2 | Above saloon facades |
| `pfb_env_gallows_frame` | 1 | Far north-east corner — atmosphere |
| `pfb_env_water_trough` | 1 | South of exit gate |

### Polyworks Props (in project via Off Axis Studios pack)

| Prefab | Count | Purpose |
|---|---|---|
| `Prop_Barrel_Closed_01` | 3 | Scatter near wagons and saloons |
| `Prop_Barrel_Water_01` | 2 | Near hitching posts |
| `Rock_Small_Dirt_01–04` | 12 | Street edge scatter |
| `Rock_Medium_Dirt_01–02` | 4 | Slightly larger scatter, flanks |
| `Vegetation_Bush_Small_01–03` | 6 | Dead/dry bushes at street edges |
| `Vegetation_Cactus_01–15` | 5 | Wild West atmosphere, outer edges |
| `Prop_Fence_Wooden_Small_01–04` | 8 sections | Boundary between yards, east + west |
| `Prop_Junk_Cardboard_Box_01–05` | 6 | Scattered near barrels (world theming + loot signal) |
| `Prop_Sign_Wooden_Blank_01` | 3 | On or near saloon facades |

### Missing — Meshy Order Required

| Prop | Count | Details |
|---|---|---|
| Rope Coils | 2 | Atmospheric western dressing near wagons |
| Broken Wagon Wheel | 2 | Near covered wagons, scatter |

See `meshy-prompts.md` for generation details.

---

## 9. Craftsmanship ENV Dressing

Four background props that tell a human story without labels. These are non-interactive. They exist in the outer edges of the room and communicate what the neighborhood was before The Great Hush.

1. **A child's bike** leaning against the side of a saloon facade, kickstand down. The kickstand has left a permanent scuff mark on the cracked asphalt. Nobody moved it.

2. **A garden hose** coiled on a hook beside a house, still connected to the spigot. The spigot handle is rusted slightly from a direction nobody turned it. The hose end points nowhere in particular.

3. **A basketball** wedged against a fence post, half-deflated. There are scuff marks on the nearby asphalt from a driveway court that no longer functions as one.

4. **A porch chair** visible through an open gate at the edge of the room. The chair faces the street. The cushion has taken the shape of whoever sat there. Nobody is sitting there now.

These props are not in the active combat zone. They are on the perimeter — seen, not interacted with.

---

## 10. Performance Notes

- All non-moving props marked as **Static** in the Inspector
- GPU Instancing enabled on Polyworks atlas material (shared by all Polyworks props)
- Lighting baked via Unity lightmapper
- **Triangle budget (Room 1):** Target under 150k total. Current estimate: ~80k with all props listed above.
- NavMesh obstacles: covered wagons, saloon facades, barrels (medium/large), fence sections
- Enemy NavMesh: bake on the flat ground plane only — wagons + fences should be NavMesh obstacles

---

## 11. Open Questions

| # | Question | Impact | Status |
|---|---|---|---|
| 1 | Does `RoomGate` show a physical door/gate object or is the exit purely a trigger zone? The V3 `RoomGate.cs` opens a door — does Room 1 have visible gate geometry? | Visual only — no gameplay impact | ❓ Open |
| 2 | Should the forge workbench auto-show a prompt UI when the player spawns, or only on proximity approach? | UX — first-time player instruction | ❓ Open |
| 3 | The 3-second aggro delay is purely time-based. Should it also check if the player has moved forward into the street, so a player who stands at spawn forever doesn't trigger combat unexpectedly? | Edge case — new player behavior | ❓ Open |
| 4 | WagonWheelRoller count: 2 or 3? At 2 the room may feel thin. At 3 it may feel crowded for new players. Recommend playtesting both and deciding during Phase 1 tuning. | Difficulty tuning | ❓ Open |

---

*Room design owner: Louie Celli | Created: 2026-08-05 | Based on docs/art/cul-de-sac-room1.md and docs/story/zones/the-cul-de-sac.md*
