# GDD: Cul-de-Sac — Room 4 "Mailbox Row"

**Zone:** The Cul-de-Sac (Zone 1)
**Room index:** 4 of 5 (random-draw pool)
**Version:** 1.0
**Date:** 2026-08-05
**Status:** Locked — ready for implementation

---

## 1. Room Overview

**Room name:** Mailbox Row
**Purpose:** Marshal mastery test. The player has met MilepostMarshals in Saloon Front. This room makes them the primary threat while introducing a new positional wrinkle: the WagonWheelRoller spawns south of the player — behind them — and charges north. The Marshals are at the far north blocking the exit gate. The player is caught between two threats from the first second.

**Size:** 32m wide × 34m long
**Layout:** Long and residential. Two rows of mailboxes line the sides. Marshals at the far north near the gate. Roller starts south of player spawn. Little room to flank — the layout funnels players toward the Marshals while the Roller pressures from behind.

---

## 2. Narrative Setup

The back stretch of the cul-de-sac. The residential stretch. Mailboxes line both curbs — each marked with a small grey flag that wasn't there before. The Unimaginative mark their territory with bureaucratic efficiency.

Two MilepostMarshal units stand at the far end, not moving. They're waiting for Kid to come to them. The Roller is behind Kid's starting position — already spinning up when the room activates.

Kid sees: the gunslingers are at the end of the road, blocking the only exit. The deputy with the wheel is already behind. There's no backing out. The only path is through.

---

## 3. Visual Tone

| Element | Description |
|---|---|
| Sky | Burnt amber — same zone palette |
| Ground | Cracked asphalt with more dead grass patches — residential neglect |
| Lighting | Same warm golden-hour. Shadows longer — narrower street between closer houses |
| Shadow color | Warm burnt sienna |
| Atmosphere | Quieter than the other rooms. The mailboxes give it a mundane residential quality — imagination overlay feels thinnest here |
| Imagination state | Awakening — the imagination is working harder to transform the ordinary |

---

## 4. Special Mechanic: Flanked from Behind

The WagonWheelRoller spawns at (0, 0, −16) — south of the player spawn at (0, 0, −12) — and charges north toward the Marshals. This is the only room where an enemy starts behind the player.

**The design tension:** The player's instinct is to back away from the Marshals. Mailbox Row punishes that instinct — retreating walks them directly into the Roller's charge. The correct play is to advance on one Marshal aggressively, parry, and clear the front before the Roller arrives.

**Aggro delay:** `2f` — the longest delay in any room. The player needs time to register that the Roller is behind them.

---

## 5. Forge Workbench

**Position:** (−10, 0, −14) — south-west corner behind the first row of mailboxes.

---

## 6. Enemy Encounter

| Enemy | Spawn Position | Behavior |
|---|---|---|
| Marshal_West | (−4, 0, +10) | Walks south. Wide swing. Parryable. |
| Marshal_East | (+4, 0, +10) | Same. 8m spacing. |
| Roller_South | (0, 0, −16) | Behind player. Charges north. |

**Aggro delay:** `2f`
**Max concurrent:** 3
**Room clear condition:** All 3 dead → `RoomManager.OnRoomCleared` → exit gate opens → `UpgradeScreen`.

---

## 7. Cardboard Drops

| Enemy | Drop |
|---|---|
| MilepostMarshal | 2–3 per kill |
| WagonWheelRoller | 1–2 per kill |

2 cardboard piles at (−8, 0, 0) and (8, 0, 0) — mid-room flanks.

---

## 8. ENV Props List

All from existing Polyworks pack.

### Primary Cover
| Prop | Count | Purpose |
|---|---|---|
| `pfb_env_mailbox_telegraph` | 6 | Two rows of 3 lining east and west curbs — room's defining visual |
| `pfb_env_rain_barrel` | 4 | Near mailboxes — low cover |
| `pfb_env_covered_wagon` | 1 | North end near Marshals — only heavy cover |

### Street Dressing
| Prop | Count | Purpose |
|---|---|---|
| `pfb_env_bld_twostoryhouse` | 2 | West flank buildings |
| `pfb_env_bld_porchcabin` | 2 | East flank buildings |
| `pfb_env_hitching_post` | 2 | Near north wagon |
| `pfb_env_lamp_post_western` | 2 | Mid-street center |
| `pfb_env_wanted_poster_blank` | 2 | South section building walls |
| `pfb_env_broken_wagon_wheel` | 3 | Scatter throughout — most wheel-heavy room |
| `pfb_env_rope_coil` | 2 | Near south mailboxes |
| `pfb_env_tumbleweed_static` | 2 | North section near gate — scale (0.25, 0.25, 0.25) |

### Craftsmanship ENV Dressing

1. **A child's drawing** taped to the inside of a mailbox door, visible when the door hangs open. Red crayon. A figure with a box on its head. It's been there a while.

2. **A lawn sprinkler** lying on its side in a patch of dead grass, hose still attached and coiled. The head is aimed up at nothing.

3. **A birthday balloon** — deflated, tied to a mailbox post with a ribbon. The ribbon is the only thing still moving when Kid enters. Just barely.

4. **A doormat** in front of the nearest house that reads "WELCOME" face-down. Someone flipped it deliberately. Or it was never the right way up.

---

## 9. Zone Lore Hook

The grey flags on the mailboxes are not decorations. They're markers — the same system The Unimaginative uses to tag structures for extraction. This street has already been processed. Whatever was here before is gone.

The child's drawing in the mailbox is the only thing they left behind. They didn't see it as relevant.

---

## 10. Difficulty Scaling

**From Room 3:** Two Marshals + front Roller → two Marshals + rear Roller. Enemy count identical, positional reversal changes everything. Hardest room in the random pool by design.
**Marshal pressure:** Player must advance into melee range while managing a rear threat. No safe retreat exists.

---

## 11. Implementation Notes for Unity

- `EnemySpawner.aggroDelay = 2f` — longest delay in any room. Critical given the rear-spawn surprise.
- Roller_South SpawnPoint at z=−16, south of player spawn at z=−12.
- Only 1 covered wagon — at the north end. Room is intentionally sparse on heavy cover.
- Mailbox rows at x=±10. Place 3 per side along east and west curbs.
- Scene name: `CulDeSac_MailboxRow.unity`

---

## 12. Open Questions

| # | Question | Impact | Status |
|---|---|---|---|
| 1 | Roller_South at z=−16 — close enough to feel like an immediate rear threat after 2s delay, or should it start at z=−20? | First-encounter fairness | ❓ Open |
| 2 | Covered wagon near Marshal spawns — does it interfere with Marshal walk-toward-player AI? | AI pathfinding | ❓ Open |
| 3 | Mailboxes at ~1m tall — enough geometry for the player to break enemy sightlines, or too sparse mid-combat? | Cover balance | ❓ Open |

---

*Room design owner: Louie Celli | Created: 2026-08-05 | Sprint 3 Phase 2*
