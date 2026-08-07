# GDD: The Backyard (Dojo) — World 2

**World:** 2 (run order: after the Cul-de-Sac)
**Rooms:** 5 (Room 1 fixed → 3 random → boss)
**Version:** 1.0
**Date:** 2026-08-07
**Status:** Approved — ready for implementation
**Scope:** This zone only. Self-contained; does not modify World 1.

---

## Changelog

| Version | Date | Change |
|---|---|---|
| 1.0 | 2026-08-07 | Initial draft through /level-design pipeline (4 gates approved) |

---

## 1. Zone Overview

**Narrative Setup:** The backyard behind the last house on the block — where kids sparred with sticks and built forts before The Great Hush. The Unimaginative didn't demolish it; they flagged it for "lot clearance" and posted a groundskeeping detail to finish the job. The grass grew waist-high, the shed sagged, the swing set rusted mid-arc. Kid steps through the back gate and the overgrowth resolves into a feudal dojo courtyard he instinctively knows how to move through. He's never trained in a real dojo. He doesn't need to have. He knows exactly what this place is for.

**Imagination Transformation:**
- **Reality:** An overgrown suburban backyard — chest-high weeds, a collapsed garden shed, a rusted swing set, a chain-link fence, a coiled garden hose, a drained kiddie pool, a dying tree shedding the last of its blossoms.
- **What Kid sees:** A feudal Japanese dojo courtyard — a bamboo stockade wall, a wooden training hall (the shed), stone lanterns, a koi pond (the kiddie pool), a raked zen rock garden, and a single cherry tree raining pink petals over the sparring ground.

**Visual Tone & Palette:**

| Element | Color | Notes |
|---|---|---|
| Sky | Overcast pearl-jade `#DCE6DA` | Calm, misty spring morning — deliberately cool, the inverse of World 1's harsh amber noon |
| Ground | Jade grass + mossy stone `#6B8E4E` | Stepping-stone paths through raked gravel |
| Accent colors | Cherry-blossom pink `#F4B8C4`, lacquer red `#A62B1F` | Pink petals + red torii/lanterns |
| Shadow color | Cool sage-grey `#4A554A` | Cool shadows — hard contrast to World 1's warm sienna |
| Unimaginative presence | Flat grey | Drains the green to wet concrete, the petals to ash |
| Imagination Restore | Gold bloom + a gust of cherry blossoms | Gold constant across zones; the dojo's unique trigger is the tree bursting into full flower |

---

## 2. Room Structure

### Room 1 (fixed) — "The Back Gate"
Intro room. Player enters through the torii gate; the dojo assembles around them over a 3-second beat.
- **Enemies:** 3 Gnome Soldiers (pack).
- **Special mechanic — Assembly Beat:** a 3-second aggro delay while the transformation plays. Gnomes patrol, then knock-charge in a staggered wave. Teaches the World 2 gnome pack rhythm (they no longer come one at a time).
- **Craftsmanship dressing:** see §11.

### Random Room A — "The Rock Garden"
Raked gravel zen garden with stepping-stone paths.
- **Enemies:** 2 Gnome Soldiers + 2 Leaf Pile Lurkers.
- **Special mechanic — Constrained Footing:** raked gravel lanes and stepping stones channel movement; Leaf Pile Lurkers are buried in the gravel borders and rise when the player is funneled past them. Teaches footing awareness under ambush pressure.

### Random Room B — "The Training Hall"
Interior of the shed reimagined as a dojo hall — weapon racks, pillars, tatami.
- **Enemies:** 1 Crane Duelist (debut) + 2 Gnome Soldiers.
- **Special mechanic — The Duel Floor:** pillars provide cover but the tighter space keeps forcing the player back into the Crane's line; gnomes pressure the player into breaking sightline with the duelist. Teaches the Crane read under crossfire.

### Random Room C — "The Koi Pond" *(Skeptic room)*
A veranda (engawa) wrapping the kiddie-pool koi pond.
- **Enemies:** 1 Crane Duelist + 2 Leaf Pile Lurkers + 1 Gnome Soldier.
- **Special mechanic — Narrow Walkways:** the pond is a no-stand zone; the player fights on narrow engawa boards, so knockback and the Crane's thrust threaten to put them in the water (brief move-speed slow, not death).
- **The Skeptic appears here** (see §9). **Craftsmanship dressing:** see §11.

### Boss Room (fixed) — "The Blossom Court"
Open sparring court, cherry tree at center.
- **Boss:** The Grasscutter (see §6).

---

## 3. New Enemy — Crane Duelist

**Real-world object:** A plastic pink lawn flamingo on a single wire leg.
**What Kid sees:** A one-legged spear duelist in crane stance — pink-lacquered armor, a wide conical straw hat, balanced on one leg, wielding a long thin beak-spear. Still, elegant, patient.

**Behavior pattern:**
- Does **not** rush. Locks onto the player at mid-range and strafes to stay facing them — it wants a duel, not a brawl.
- **Beak Thrust:** long wind-up (rears back on its leg, hat tilts) → a single fast, long-reach lunging thrust. Parryable, but a **tight window**; a clean parry opens an unusually large counter. Miss → heavy player stagger.
- **Pivot Sweep:** if the player circles behind, it answers with a spinning low sweep — its anti-flank move. Short range.
- **Recovery:** after the thrust it is visibly off-balance on one leg for ~1s — the counter window.

**Attack type:** Melee (long reach).
**Parry rules:** Beak Thrust parryable (tight timing, high reward). Pivot Sweep **un-parryable — jump/dodge only**.
**Visual silhouette:** One leg, tall, conical hat, thin spear — unmistakable next to the stocky gnomes and low leaf piles.
**Lore hook:** It stood on one leg in the same spot for years. It has had a very long time to think about balance. It does not miss twice.
**Health / difficulty tier:** Tier 2 — duelist / skill-check. **New READ:** precise parry timing on a single long-reach tell (World 1 taught telegraphed charges and area-denial; this teaches patience and exact timing).

---

## 4. Returning Enemies (World 2 tuning)

**Gnome Soldiers** — +30% HP, +15% attack speed. Same pack-charger behavior and ceramic-knock tell, but arrive in larger packs (3–4) with staggered charges so the player can't parry the whole wave at once. Reference: `docs/story/enemies/gnome-soldier.md`.

**Leaf Pile Lurkers** — +30% HP. Dormant as leaf piles until the player is close, then rise. World 2 twist: seeded near the Crane Duelist so they rise mid-duel and break the player's spacing. *(No prefab exists yet — build dependency, see blueprint.)*

---

## 5. New Boss — The Grasscutter

**Real-world object:** A rusted push reel-mower.
**What Kid sees:** A drum-chested tengu blade-master — the reel of curved blades is its spinning heart, the push-handle folds like war-wings, the wheels are heavy sandals. It shakes petals loose when it moves.

**Intro sequence:** Dormant in the tall grass at the arena's far end. As Kid approaches, the reel ticks over, grass and petals kick up, and it rises. Camera cuts to the cherry tree, then to the mower spinning up — mirrors the World 1 boss-intro cadence (reuse the boss-intro camera pattern).

**Phase 1 — "Kata" (100% → 50%):** An honorable blade-master; deliberate positioning.

| Attack | Tell | Response |
|---|---|---|
| Blade Combo | Two-beat overhead reel swing | Parry both beats |
| Reel Guard-Break | Reel flares wide, slow | Dodge back — do NOT parry (breaks guard) |
| Petal Toss | Flicks a fan of cut blades (ranged) | Dodge lateral or block |

**Phase 2 — "Rev" (50% → 0%):** The reel becomes a continuous whirlwind. Qualitatively new mechanic = **moving hazard lanes**.

| Attack | Tell | Response |
|---|---|---|
| Spin-Dash | Revs, aims, then a straight-line charge across the arena | Leave the lane — dodge perpendicular |
| Whirlwind Pull | Sustained spin drags the player inward | Dodge outward, against the pull |
| Cut-Grass Trail | Leaves damaging petal trails behind each dash | Manage shrinking safe ground |

**Defeat moment:** The reel jams, grinds, and stops. A final gust of cherry blossoms drifts across the court. Gold Imagination Restore blooms and the cherry tree bursts into full flower.

*(No prefab exists yet — build dependency, see blueprint.)*

---

## 6. Weapons (reused — dojo-native, in project)

| Weapon | Real object | Imagined | Tier | Type |
|---|---|---|---|---|
| Bo Staff | Broomstick | Bo staff | 1 | Melee, wide arcs |
| Katana | Cardboard Tube | Katana | 2 | Melee, fast clean strikes |
| Shurikens | Ruler | Thrown stars | 1 | Ranged |
| Water Whip | Garden Hose | Water dragon-whip | 2 | Melee reach |

No new weapons this zone — all four are dojo-native and already in-project.
⚠️ **Water Whip** still needs its model + icon (pending asset work). Design-complete, art-pending — see Open Questions.

---

## 7. ENV Props List

*(Full gap analysis and Meshy details in `meshy-prompts.md`.)*

| Prop | Real basis | Imagined | Source |
|---|---|---|---|
| Cherry blossom tree | Dying tree | Cherry tree | `pfb_env_cherry_blossom_tree` |
| Stone lantern | Garden ornament | Ishidoro lantern | `pfb_env_stone_lantern` |
| Torii gate | Back gate/arbor | Torii | `pfb_env_torii_gate` |
| Stepping stones | Path pavers | Stepping stones | `pfb_env_stepping_stone_tile` |
| Training dummy | Scarecrow/post | Makiwara | `pfb_env_target_dummy` |
| Weapon rack | Tool rack | Weapon rack | `pfb_env_weapon_rack` |
| Training hall | Garden shed | Dojo hall | `pfb_env_bld_shedwithcrate` |
| Paper lanterns | String lights | Paper lanterns | Polyworks Asian Additional |
| Bamboo fountain | Hose spigot | Shishi-odoshi | `Asian_Prop_Bamboo_Dried_Water_Fountain_01` |
| Raked gravel | Sandbox/dirt | Zen garden | `Asian_Prop_Zen_Garden_Sand_01` |
| Zen rocks | Yard rocks | Zen stones | Polyworks Rocks |
| Tatami mats | Picnic mats | Tatami | Polyworks Asian Additional |
| Koi pond basin | Kiddie pool | Koi pond | ⚠️ `street_pond_a` (check-first) |
| Bamboo stockade wall | Chain-link fence | Bamboo wall | ❌ Meshy order |

**Global Meshy settings** for any new ENV props: see `meshy-prompts.md`. Total new ENV geometry budget: <8k tris (only 1–2 props ordered, so well under).

---

## 8. Difficulty Scaling

- **Baseline:** +30% HP, +15% attack speed vs. World 1 (Cul-de-Sac) equivalents.
- **New mechanics introduced:** tight-window parry (Crane), moving-hazard-lane boss phase (Grasscutter Rev), terrain-constrained footing (rock garden, engawa).
- **Enemy-mixing rules:**

| Rule | Value |
|---|---|
| Concurrent Crane Duelists | Max 1 (the duel read breaks with two) |
| Concurrent risen Leaf Lurkers | Max 2 |
| Concurrent Gnome pack | Max 4 |

- **Room restrictions:**

| Room | Restriction |
|---|---|
| Room 1 | Gnomes only — no Crane, no ambush |
| Crane Duelist | Debuts no earlier than Random Room B |
| Skeptic | Room C only |

- **Difficulty curve:**

| | Room 1 | Rock Garden | Training Hall | Koi Pond | Boss |
|---|---|---|---|---|---|
| Pressure | Low | Medium | Medium-High | High | Peak |
| Teaches | pack rhythm | footing/ambush | the duel | duel + terrain | rhythm → chaos |

---

## 9. Zone Lore Hook

The backyard is where kids *learned* — the first place imagination was practiced, not just used. Reclaiming it restores the idea that skill is made, not given.

**The Skeptic** appears in the Koi Pond room: standing in the shed doorway, they set a folded lawn chair down flat, say something flat and mundane ("It's just a yard."), and are gone by the time the player crosses to them.

**World Tree connection:** the cherry tree here is a seedling descendant of the World Tree — the first hint that living cardboard once grew wild in ordinary backyards. *(Confirm lore pacing with narrative canon — see Open Questions #5.)*

---

## 10. Implementation Notes for Unity

- **Run integration:** mirror World 1. Scenes `Backyard_Room1` → random draw of {`Backyard_RockGarden`, `Backyard_TrainingHall`, `Backyard_KoiPond`} → `Backyard_BossCourt`. Register in the same static room-queue system used by World 1 (`s_roomQueue`, `s_roomQueueIndex`).
- **Crane Duelist AI:** new `CraneDuelistAI` — strafe-to-face state, tight-parry-window flag on Beak Thrust, un-parryable flag on Pivot Sweep, off-balance counter window. Follows `BasicEnemyAI` state pattern; subscribes to `CombatController.OnCounterStrike`. Material cached in `Awake`, destroyed in `OnDestroy`.
- **Grasscutter:** new `GrasscutterAI`, two-phase like `SpinCycleAI`. Phase 2 spin-dash = NavMesh-off straight-line lerp along a telegraphed lane; cut-grass trail = pooled trail-hazard volumes (no per-frame alloc). Reuse the SpinCycle defeat-sequence pattern (stumble → wobble → burst → shrink) and the Imagination Restore volume.
- **Water hazard (Koi Pond):** trigger volume applies a brief move-speed slow, not damage/death.
- **Reuse:** boss-intro camera pattern, RoomManager clear→~0.7s delay flow, minimap, Imagination Restore.
- **Performance:** dojo props mostly Static; GPU instancing on Polyworks atlas; bake lighting. Per-room triangle target <160k.

---

## 11. Craftsmanship ENV Dressing

**Room 1 — The Back Gate** (4 background story props, non-interactive):
1. A child's wooden practice sword leaning on the gate, the grip worn smooth from one pair of hands.
2. A faded chalk hopscotch grid on the path stones, the numbers half rained-away.
3. A single garden glove on a fence post, the fingers curled from one last grip.
4. A birdhouse with the perch snapped off, still nailed perfectly level.

**Room C — The Koi Pond** (4 background story props, non-interactive):
1. A toy boat run aground at the pond edge, its sail a scrap of napkin.
2. Two pairs of flip-flops set neatly on the engawa — one adult-sized, one child-sized.
3. A wind chime with one tube missing, still turning, still trying to ring.
4. A coffee mug on the veranda rail with a dried ring inside, waiting for a morning that didn't come.

---

## 12. Open Questions

| # | Question | Impact | Status |
|---|---|---|---|
| 1 | Crane Duelist Beak Thrust parry window — how tight before it's frustrating vs. satisfying? Needs playtest tuning. | Core skill-check feel | ❓ Open |
| 2 | Grasscutter Phase 2 spin-dash: fixed telegraphed lanes (readable) or player-aimed dashes (scarier)? | Boss difficulty/readability | ❓ Open |
| 3 | Koi Pond water: brief slow only, or also a small damage tick? | Balance | ❓ Open |
| 4 | Water Whip art (model + icon) is unbuilt — ship World 2 with it, or hold it and drop it from the dojo weapon pool until art lands? | Scope / art dependency | ❓ Open |
| 5 | "Cherry tree = World Tree seedling" reveal — too big for World 2, or the right early breadcrumb? Confirm with narrative canon. | Lore pacing | ❓ Open |
| 6 | Leaf Pile Lurker has no prefab and the Crane/Grasscutter are new models. Build these before or in parallel with scene construction? | Build sequencing | ❓ Open |

---

*Room design owner: Louie Celli | Created: 2026-08-07 | Hand to `unity-senior-developer` after review; enemy/boss models route through `/asset-pipeline` + art-direction-agent first.*
