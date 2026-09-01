---
name: reference-room-scale-calibration
description: Playtested BoxForged room-scale reference values — the owner has twice reported a zone "too small"; these are the dimensions they accepted, and the free-floor / dash-ratio numbers to size new rooms against.
metadata:
  type: reference
---

The owner reports zone scale by feel, twice now, and both times the useful answer came from **what they had already accepted** rather than from re-deriving against the camera. Size new rooms against these, not against ADR-0001's 16.8 m visible width.

**Accepted, playtested dimensions:**

| Reference | Value | Source |
|---|---|---|
| World 1 zone 1, after the owner said "no room for the fight" | widened **8 m → 20 m**; `Ground` lengthened 47.5 → 59.5 m | `docs/BACKLOG.md` **B107** (2026-08-26). Owner then de-prioritized a separate bug as "not a concern after the board was lengthened/Zone 1 de-cramped" (B104) |
| World 1 visual street corridor | X −8.8…+10 = **18.8 m** wide | ADR-0005 §6.2 — so 16.8 m is not a ceiling; ~20 m is precedented |
| World 1 boss arena | measured clear radius **8.44 m** (~16.9 m across), physics-sweep verified | ADR-0004 §Validation 4 |
| SpinCycle longest traversal | **4.8 m** (`spinChargeSpeed 8 × spinChargeDuration 0.6`) → dash ÷ diameter = **0.284** | ADR-0004 §2 |
| World 2 zone 0 free floor — **the zone the owner did NOT complain about** | ~**294 m²**, near-unobstructed, 4 concurrent enemies | derived 2026-09-01, ADR-0006 Fact 2 |
| World 2 after the 2026-09-01 report | zone 1 → **20.0 × 28.0 m**; arena → **r = 10.0 m / 20.0 m across**; zone 0 unchanged | ADR-0006 §1, §3 |

**The three diagnostics that actually explained "too small"** — none of which is total area:

1. **Free floor, not gross area, and per sub-space.** World 2 zone 1's *total* free floor matched zone 0's, but it is two sub-spaces and the northern one (the Crane Duelist fight) had ~50 m² of usable floor against zone 0's 294. Always split a multi-beat zone before believing its aggregate.
2. **Boss longest-traversal ÷ arena diameter.** Two arenas of near-identical size felt different because the Grasscutter's budgeted 8 m dash in a 17 m court erases 47% of the arena per commitment, against SpinCycle's playtested 28%. **This ratio, not the radius, predicts feel.** Keep ≤ 0.35.
3. **Sum the zone's elements across its narrow axis.** World 2 zone 1B's spec'd elements (shed + engawa + gap + pond + corridor) summed to **20.9 m of width in a 16.5 m zone** — over-subscribed by 4.4 m — because the spec assumed a 3.0 m shed and the real prefab is **7.4 m**. That arithmetic check is fast and would have caught it at design time.

**Corollary on props:** the fix is area up, **movement-blocking prop count held**. Collider-less floor dressing (flat stones, leaf mounds) costs no fight floor and may scale; posts, buildings, water bodies and platforms may not. World 2 zone 1's blocker density went 1 per 5.9 m² → 1 per 9.0 m² with no prop removed.

**How to apply:** when sizing or resizing a room, state the free floor per sub-space, the blocker density, and (for a boss room) the traversal ratio, and compare each to the table above. When the owner says a zone feels small, look for fragmentation and for an over-subscribed narrow axis before adding metres uniformly — and check whether a prefab's real measured footprint matches the number the spec used.

Related: [[reference-measuring-city-scene]], [[project-unsatisfiable-metrics]], [[project-docs-drift-from-code]]
