# BoxForged — Game Design Document V2

**Version:** 0.9
**Date:** 2026-07-23
**Status:** Active — prototype design locked; Phase 2 Cul-de-Sac design locked; lore and character canon locked; HKW skill mapping added

---

## Changelog

| Version | Date | Changes |
|---|---|---|
| 0.1 | 2026-07-13 | Initial draft |
| 0.2 | 2026-07-13 | Locked all open questions. Added full World Tree lore and post-apocalyptic setting. Updated combat to dodge-parry. Added co-op architecture direction. Updated Skeptic to permanent antagonist. |
| 0.3 | 2026-07-13 | Locked all Phase 2 decisions. Separate co-op IP pools. Skeptic confirmed nameless archetype. Cardboard Mill moved to late Phase 2. Elder BoxHead confirmed as late-game mystery. NGO confirmed as netcode library. |
| 0.4 | 2026-07-16 | Revised core premise: screen addiction + internet collapse replaces deforestation as the world-breaking event. Kid is now a boy whose parents kept him phone-free — the box is a lens for his imagination. Added The Friend (locked, future introduction). Updated elevator pitch and lore accordingly. |
| 0.5 | 2026-07-16 | Combat update: enemies attack constantly; Option C stagger adopted. Backyard redesigned as 3-room layout. SpinCycle confirmed as Phase 1 boss (2-phase fight). Sprinkler Sentinel added as new Phase 1 enemy type. Sprint plan 7–10 locked. |
| 0.6 | 2026-07-18 | Roguelite run structure added. Jump + aerial attacks locked. Box System restructured — boxes are cosmetic-only; Fighting Styles system introduced as the gameplay layer. Two-pool IP system defined (in-run IP + meta Spark, phased rollout). Monetization model locked (ads + IAP). Control layout updated with Jump button (diamond arrangement). Phase 1 prototype run structure defined. Camera confirmed fixed top-down — no rotation. Section numbering corrected. |
| 0.7 | 2026-07-19 | Phase 2 Cul-de-Sac design locked. V2 enemy roster confirmed: WagonWheelRoller, HitchingHound, MilepostMarshal (melee brawler), SprinklerSentinel V2. V2 weapons added: Jump Rope (Lasso), Bike Horn (Dynamite Bundle), Garden Trowel (Quickdraw Blade), Watering Can (Six-Shooter). Cul-de-Sac zone detail added. Button layout positions finalized: JUMP=North, PARRY=West (corrects v0.6). |
| 0.8 | 2026-07-22 | Lore and character canon locked. Inciting event named **The Great Hush** (present-day; engineers coordinated Internet shutdown). Elders confirmed as infrastructure/DevOps people — not villains; morally complex. Full character lore added for Kid, Cowgirl, Female Ninja, The Skeptic, The Elders. Enemy lore entries added for all Phase 1 + Phase 2 enemies. Zone lore, weapon flavor text, UI copy, World Tree lore, and ending narrative authored. All story content in `docs/story/`. Tumbleweed Roller removed from enemy roster (not in game). |
| 0.9 | 2026-07-23 | Cross-project integration: Section 10.1 added — Hard Knocks Workshop real-world skill mapping (Navigation → Compass Dash, Axe-work → Splitting Maul, Fire-starting → Ember Charge, Woodworking → Craftsman Skin Tier). Abilities designed for Phase 3 QR code unlock system. |

---

## 1. Game Overview

**Title:** BoxForged
**Genre:** Action Roguelite Brawler (incremental scope — prototype → full RPG)
**Platform:** iOS + Android (Phase 1 release); PC in a later phase
**Engine:** Unity 6 LTS + URP (Mobile quality tier)
**Target Audience:** Ages 10–35. Kids who want fun action; adults who remember pretending.

### Elevator Pitch

> Everyone was addicted to screens. Then the Internet broke — everywhere, all at once. The world didn't know what to do with its hands. But one kid's parents never allowed a phone in the first place. So when everything went quiet, he went outside, found a cardboard box, put it on his head — and saw everything. The box doesn't create the world. It lets him see what was always there: the backyard is a dojo, the shed is a fortress, the cracked cul-de-sac is a Wild West showdown waiting to happen. The Unimaginative want to end it. They've seized the World Tree — the last source of cardboard on Earth — because without boxes, the lens goes dark forever. But they forgot about the kids.

### Tone

Whimsical on the surface, quietly urgent underneath. The game plays the imagination completely straight — a cardboard sword is *genuinely* a sword, the backyard is *genuinely* a feudal courtyard. But the stakes are real: the world is losing its ability to imagine, and children are the last ones who still can.

Think: **Scott Pilgrim** meets **Calvin and Hobbes** meets **Shovel Knight** — with the emotional undertow of **Wall-E**.

The post-apocalyptic setting is never bleak. It is seen entirely through a child's eyes: ruins are dungeons, dead factories are castles, a dying world is just a bigger playground to reclaim.

---

## 2. Core Fantasy / Player Experience

**What does it feel like to play?**

The player should feel like the last imaginative kid in a world that forgot how. Every upgrade isn't "you got a better sword" — it's "you *imagined* a better sword." The stakes are real (the World Tree is dying), but the power to fight back comes from pure creative belief.

**Core emotions to evoke:**
- **Nostalgia** — "I remember doing exactly this as a kid."
- **Urgency with wonder** — "The world needs us, and we're doing it *our* way."
- **Empowerment** — "My weird idea is the right idea."
- **Hope** — "Imagination wins. It always wins."

**The game earns these by:**
- Letting the player mix and match boxes and fighting styles in unexpected ways
- Making the world visibly brighter and more vivid as the player pushes back The Unimaginative
- Rewarding creative combinations with unique effects
- Never letting The Unimaginative feel unbeatable — they're powerful, but they have no imagination

---

## 3. Visual Style Direction

**Confirmed: Stylized 3D with a "cardboard and marker" aesthetic.**

The world is rendered in 3D with a deliberately hand-crafted look — as if everything was made from cardboard, marker drawings, and craft supplies.

- Characters have slightly boxy, chunky proportions (deliberate)
- Textures have a hand-drawn, marker-on-cardboard quality
- The post-apocalyptic world is grey and washed out in its base state — reclaiming areas restores color and vibrancy

### Imagination Overlay

The world has a **dual-layer look** that reflects the state of imagination in each zone:

- **Drained State (Unimaginative-controlled):** Muted greys and browns. Everything looks like an abandoned parking lot. The imagination layer is almost invisible.
- **Awakening State (in-progress):** Warm, slightly saturated. Outlines appear. Objects start to glow with their imagined identity.
- **Reclaimed State (player-controlled):** Full vivid, saturated color. The imagination layer bleeds fully into reality — the garage IS a dungeon, the backyard IS a dojo.

**Phase 1:** URP post-process color grading shift tied to the boss defeat event. Full dual-layer rendering is Phase 3.

### Reference Touchstones

- **Characters:** Psychonauts (chunky, expressive), A Short Hike (clean, simple shapes)
- **World:** Tearaway (paper/cardboard textures), Little Big Planet (crafted materials), early areas of Hollow Knight (muted tones brightening as you progress)
- **UI:** Hand-drawn, marker-font style. Health bar looks like a crayon drawing. IP counter looks like tally marks.

---

## 4. Core Gameplay Loop

### Run Structure (Roguelite)

Each play session is a **run** — the player starts fresh, builds their combat style room by room, and either clears the boss or is defeated. Runs are the core loop.

```
START RUN
  → Choose Fighting Style
  → Enter Room 1 (combat)
  → Earn IP via combat — longer combos = higher IP multiplier
  → Between rooms: choose 1 of 3 upgrades (Hades-style)
  → Enter Room 2 (combat)
  → Safe Zone: shop (spend IP on power-ups, health, one-time moves)
  → Enter Room 3 (combat)
  → Boss Fight
  → RUN END
      → IP scored + displayed
      → % of IP converts to Spark (meta-currency)
      → Meta screen: spend Spark on permanent unlocks
```

**Session length target:** 10–15 minutes per run. Designed for mobile — completable in a single sitting.

### Moment-to-Moment

The player moves through rooms of a post-apocalyptic location reimagined through a child's lens. Combat uses a dodge-parry-jump rhythm — attacks are telegraphed, well-timed parries stagger enemies and open counter-attack windows, and jump creates aerial opportunities. Between rooms, the player chooses upgrades that shape how the rest of the run plays out.

### Long-Term Loop (Phase 2+)

Permanent progression layers build on top of the run loop: new box cosmetics, new fighting styles, and permanent minor upgrades unlock between runs using Spark. The player's visual style grows as they invest in the game over time.

**Co-op layer (Phase 2+):** The loop is designed for 2-player co-op from the ground up. Phase 1 is single-player but all systems must be architected to support a second player without rework.

---

## 5. Combat System — Dodge-Parry-Jump

Combat is timing-based, not twitch-reflex. The player has a meaningful choice on every enemy attack: dodge, parry, or jump.

### Dodge

- A directional roll with brief invincibility frames
- Cancels any animation the player is in
- Short cooldown (~0.8s) to prevent spam
- Direction matters — dodging into an attack still connects

### Parry

- A timed block triggered by pressing parry just before an enemy attack lands (~0.2–0.3s window; ~0.3–0.35s on touchscreen)
- On successful parry: enemy is staggered, opening a **Counter Window** (1.5–2s)
- Counter Window allows a free hit or special ability use at no resource cost
- Failed parry: player takes full damage and is briefly staggered themselves (punishing but not lethal)
- Parry has no cooldown — it is purely timing-skill

### Jump + Aerial Attack

- A dedicated **JUMP** button launches the player airborne (short arc, mobile-friendly)
- Pressing **ATTACK** while airborne triggers an **aerial strike** — animation and effect are determined by the active Fighting Style
- Certain enemy attacks telegraph ground-only AOE with a floor indicator — jumping avoids these entirely
- Aerial attacks can hit staggered enemies for additional damage

### Enemy Aggression — Always Attacking

Enemies attack constantly — the player is always in reactive mode. There is no baiting system. Breathing room is created by the player through aggression, not by waiting.

**Option C — Stagger as Breathing Room (locked):**
- Landing hits on enemies staggers them briefly, interrupting their next attack
- A successful parry staggers the enemy longer than a regular hit
- The player earns pauses by staying aggressive — a passive player gets overwhelmed
- Enemies attack in short bursts (2–3 hits) followed by a visible recovery animation

### Combat Feel Goals

- Enemies telegraph attacks clearly (wind-up animation + audio cue)
- SpinCycle's Regular Jump slam is un-parryable — dodge only. All other Phase 1 attacks are parryable.
- SpinCycle boss has a 2-phase attack pattern requiring dodge, parry, and aerial awareness
- Combat should feel more like **Sekiro** (read and react) than **Dark Souls** (stamina management)

### Object Interaction in Combat

- Objects determine attack range, speed, and animation
- Fighting Style determines the aerial attack and the special ability available during the Counter Window

---

## 6. The Box System — Cosmetic Identity

The cardboard box on the player's head is their **visual identity**. Boxes are purely cosmetic — they change how the player looks, not how they play.

> **Key distinction:** The box determines appearance. The **Fighting Style** (Section 7) determines gameplay. Any box can be paired with any Fighting Style.

### How It Works

- The player chooses their box at run start or on the meta screen between runs
- Boxes are unlocked through Spark (earned by playing) or purchased via IAP
- No gameplay stats are attached to boxes

### Starter Boxes (Phase 1 — 2 boxes, both default-unlocked)

| Box | Imagined As | Visual Theme |
|---|---|---|
| **Ninja Box** | Ninja Mask (black marker lines) | Dark ink strokes, sharp angles, eye cutouts |
| **Cowboy Box** | Cowboy Hat (brim drawn on sides) | Wide brim sketch, star badge, rope details |

### Expanded Boxes (Phase 2+)

| Box | Imagined As | Visual Theme |
|---|---|---|
| **Knight Box** | Knight Helmet (foil-wrapped) | Silver crayon, visor slit, crest drawn on top |
| **Wizard Box** | Wizard Hat (cone taped on top) | Purple marker stars, moon cutout, tissue paper trim |
| **Astronaut Box** | Space Helmet (porthole cut out) | Silver paint, circular porthole window, antenna |
| **Pirate Box** | Pirate Captain's Hat | Skull and crossbones marker, tricorn shape cut from flaps |

---

## 7. Fighting Styles — Gameplay Class System

The **Fighting Style** is the player's gameplay class for a run. It determines the aerial attack, special move, and passive bonus. Styles are fully independent of box appearance.

### How It Works

- Chosen at the start of each run — fixed for the run's duration
- Unlocked permanently through Spark or IAP
- Mid-run upgrades can enhance style abilities (aerial hits more targets, special costs less, etc.)

### Phase 1 Fighting Styles

**Ninja Style** *(default-unlocked)*
| Element | Description |
|---|---|
| **Aerial Attack** | Spinning dive kick — fast, narrow hit zone, knocks enemies back |
| **Special Move** | Shadow Dash — dash through an enemy dealing damage; usable in Counter Window or on full charge |
| **Passive** | Dodge has invincibility frames — clean dodges avoid all damage |

**Cowboy Style** *(default-unlocked)*
| Element | Description |
|---|---|
| **Aerial Attack** | Lasso Slam — descends with a lasso arc that pulls nearby enemies together on landing |
| **Special Move** | Tumbleshot — fires a burst of ranged shots; usable in Counter Window or on full charge |
| **Passive** | Parry window is wider (~0.4s vs. 0.3s) — more forgiving timing on every parry |

### Phase 2+ Fighting Styles *(Spark or IAP)*

**Samurai Style**
| Element | Description |
|---|---|
| **Aerial Attack** | Overhead Blade Drop — slow descent, large AOE on landing, stuns hit enemies |
| **Special Move** | Focused Counter Burst — point-blank explosion of force; massive damage, short range |
| **Passive** | Parry counter deals 2× damage during Counter Window |

**Knight Style**
| Element | Description |
|---|---|
| **Aerial Attack** | Shield Slam — lands shield-first, creates a brief stun field |
| **Special Move** | Shield Bash — launches the nearest enemy backward into others |
| **Passive** | Takes 25% reduced damage while standing still |

**Wizard Style**
| Element | Description |
|---|---|
| **Aerial Attack** | Arcane Drop — releases an AOE pulse from the landing point |
| **Special Move** | Chaos Wand — fires a random elemental shot (fire, lightning, or shadow) |
| **Passive** | Counter Window duration extended to 3s |

**Pirate Style**
| Element | Description |
|---|---|
| **Aerial Attack** | Cannonball — rockets straight down at high speed, massive impact damage |
| **Special Move** | Call the Crew — an imagined crewmate joins briefly, attacking nearby enemies |
| **Passive** | IP earned per kill increased by 25% |

### Combo Imagination (Box + Style Pairings)

Specific Box + Style pairings trigger unique **Combo Imagination** visual effects — cosmetic flourishes that reward mixing and matching:

| Box | Style | Combo Name | Visual Effect |
|---|---|---|---|
| Ninja Box | Ninja Style | Shadow Ninja | Attacks leave lingering ink-stroke afterimages |
| Cowboy Box | Cowboy Style | Quick Draw Kid | Lasso glows gold; tumbleshots leave star trails |
| Knight Box | Samurai Style | Steel Sentinel | Blade drop leaves a glowing impact crater |
| Wizard Box | Wizard Style | Arcane Scholar | Elemental shots leave color-coded sparkle trails |

---

## 8. Weapon / Object System

Everyday objects found in the environment become weapons through imagination. The player picks them up in the world. *(Future system — placeholder behavior in Phase 1; full implementation in Phase 2+.)*

### Object Tiers

| Real Object | Imagined Weapon | Tier | Type |
|---|---|---|---|
| Broomstick | Bo Staff | 1 | Melee, sweeping |
| Ruler | Throwing shuriken | 1 | Ranged, multi-hit |
| Pool noodle | Foam sword | 1 | Melee, fast |
| Cardboard tube | Katana | 2 | Melee, precise |
| Flashlight | Lightsaber / torch | 2 | Melee + light AoE |
| Spatula | Short sword / paddle | 2 | Melee, knockback |
| Garden hose | Water whip / chain | 2 | Melee, reach |
| Bicycle pump | Pressure cannon | 3 | Ranged, AoE |
| Remote control | Magic wand | 3 | Ranged, elemental |
| Lunchbox | Shield / throwing weapon | 3 | Defensive + ranged |

### Phase 2 Objects — The Cul-de-Sac

| Real Object | Imagined Weapon | Tier | Type |
|---|---|---|---|
| Jump Rope | Lasso | 2 | Melee, reach — wide arc swing; roots enemies briefly on connect |
| Bike Horn | Dynamite Bundle | 3 | Ranged, AoE — long wind-up throw; bounces then detonates |
| Garden Trowel | Quickdraw Blade | 1 | Melee, fast — three-hit combo; extra stagger on pommel strike |
| Watering Can | Six-Shooter | 2 | Ranged, burst — 6 shots; 1.2s stationary reload after each burst |

**Combo Imagination (Cowboy Style + Cul-de-Sac weapons):**
- Cowboy Style + Lasso → **Round-Up:** Counter Window throws the lasso forward at range, pulling the nearest enemy toward Kid for a follow-up hit
- Cowboy Style + Six-Shooter → **Fan the Hammer:** Counter Window fires all 6 shots simultaneously in a spread (no reload; full reload after)

---

## 9. Characters

### Player Character — The Kid

**Default Name:** Kid (player-named at start)
**Appearance:** A boy (~8–12), wearing a cardboard box on his head. Jeans, sneakers, a t-shirt — slightly scruffy. The box is marked up with the active cosmetic's design using crayon and marker.

**Backstory:** Before the box, he was already the kind of kid who made things. His parents never bought him a tablet — they'd redirect him outside, to the kitchen table, here's some tape. The day The Great Hush happened, he was in the backyard building a trebuchet from PVC pipe. He didn't fully understand what had happened. He went back to the trebuchet. Two days later he found the box in the recycling, put it on his head, and saw the backyard differently. He ran inside for markers. He's been running somewhere ever since.

**Personality:** The Kid does not perform enthusiasm — he is genuinely, fundamentally inside whatever he's doing. The box does not make him imaginative. It reveals that he always was. He has conclusions, stated simply, then acts on them. He will not be talked out of anything by someone who hasn't tried it. The Skeptic's words land, but they do not stop him.

**What drives him:** He wants the apple tree to have its blossoms back. He doesn't articulate it that way. But that's what he's moving toward — everything that's gone grey, he wants it back. He knows it's still there underneath.

Full character lore: `docs/story/characters/the-kid.md`

---

### The Cowgirl

**Appearance:** Girl, same age range. Cowboy box — brim drawn on the sides, rope details, star badge. Braid visible, boots, vest.

**Backstory:** She had a phone — got it for her ninth birthday, loved it. Used it to watch videos of horses, to text her best friend things that were easier to type than to say. The Great Hush took it all at once. First three days she cried. On the fourth day she found a hat box in the garage, cut it down, drew a brim, drew a star, put it on. She picked up the clothesline rope already in the garage and walked outside. She still grieves the phone sometimes — not the phone exactly, but her best friend's number, which she no longer has. She's looking for her. That's not the whole reason she's fighting, but it's in there.

**Personality:** Where the Kid is forward momentum, the Cowgirl is deliberate weight. She takes in a situation before moving through it. She is the one who notices when something's been reclaimed — color returning to a wall, a plant straightening. The Kid never doubted this world. The Cowgirl chose it. That gives her a gravity he doesn't have yet.

Full character lore: `docs/story/characters/the-cowgirl.md`

---

### The Female Ninja

**Appearance:** Girl, same age range. Ninja box — black marker lines, ninja mask aesthetic. Minimal decoration. Three lines. That was enough.

**Backstory:** She was training before The Great Hush — forms in the backyard at five in the morning, practicing what she'd seen in videos until her knees hurt. She did not fully know why. No teacher, no dojo, no tournament. When The Great Hush came, the phone went with it, but she barely noticed. She'd already absorbed what she needed. What The Hush clarified: she'd been preparing for something. Now she knows what it is.

**Personality:** Quiet, not shy. Speaks when she has something specific to say. Notices the structural truth of every situation. Moves correctly. She admires the Kid — he just goes — though she would never use that word. The Cowgirl she trusts.

Full character lore: `docs/story/characters/the-female-ninja.md`

---

### SpinCycle (Phase 1 Boss)

**Reality:** A heavy-duty garden washing machine — a front-loader drum on a spike base.
**What Kid sees:** A heavyweight brawler with a washing machine drum for a head. Muscular, torn shorts, ripped vest, mismatched sneakers (one blue, one red). The drum rotates slowly at all times.

**The drum window is his weak point.** Parry is only rewarded when the porthole window faces the player AND the drum is at full speed.

**Model:** `Assets/_Project/Models/Characters/SpinCycle/`

**Phase 1 — Rinse Cycle (100% → 50% health):**
| Attack | Telegraph | Response |
|---|---|---|
| Drum Slam | Leans back, drum spins to full speed, window faces up | Dodge sideways — un-parryable |
| Haymaker | Winds up one arm, drum window faces player | Parryable — stagger + counter window |
| Spin Charge | Crouches, rushes in straight line | Dodge perpendicular |
| Clothes Toss | Reaches into drum, pulls out balled laundry | Dodge projectile; closing distance opens parry window |

**Phase 2 — Spin Cycle (50% → 0):**
| Attack | Telegraph | Response |
|---|---|---|
| Full Spin | Spreads arms, rotates whole body | Move to outer ring |
| Suds Burst | Drum flashes white — 3 ground-hugging foam blobs | Dodge through gaps OR jump over them |
| Double Haymaker | Both arms — two-part timing | Parry first, dodge second |
| Jump Charge | Run and Jump across arena | Dodge hard lateral |

**Defeat moment:** The drum slows, grinds, stops. Cherry blossoms drift in. Imagination Restore fires — full color blooms across the Backyard.

---

### The Friend (Locked — Future Introduction)

Kid's best friend since kindergarten. Equal partner, not a sidekick. Do NOT name in Phase 1 or Phase 2 — his name is a story beat.

### The Skeptic (Recurring Antagonist)

Nameless by design. The Skeptic is not a person who lost their imagination — it is what happens to the space where imagination used to be, given a shape and a box and a direction. There is no wound in the Skeptic, no tragedy. The Skeptic never had imagination to lose.

The box is grey — unpainted, undecorated, the original corrugated brown bleached entirely out. Not because someone painted it grey, but because nothing was ever added. The Skeptic is what cardboard looks like when no one picks it up.

The Skeptic does not pursue. Does not raise its voice. Arrives, observes, speaks one flat sentence, and waits. When it attacks, it is less like aggression and more like a correction being applied. Its presence drains the Imagination Overlay — taking hits temporarily desaturates the environment and weakens style special abilities. This is not intentional on its part. The Skeptic is simply there. Its presence is enough.

**What the Skeptic says:** Flat. One sentence. No follow-up. The silence after is the second blow.
- "That's a stick."
- "You'll stop eventually."
- "I've seen this before."
- "None of that happened."
- "It was always just a backyard."

*(Lines escalate across the game from most concrete to most sweeping.)*

Full character lore: `docs/story/characters/the-skeptic.md`

---

### The Elders

Not a single character — a collective. Engineers. Network Engineers, DevOps Engineers, Support Engineers — the infrastructure people who made sure things were working. The ones who knew where the actual wires went and had spent twenty to thirty years watching something go wrong. They found each other through the professional networks only people in their positions had access to. They ran the thought experiment. They stress-tested the models. The math said: survivable. Hard. Not clean. But survivable.

They set a date. They did the work. They didn't announce themselves.

Not one of them believed they were definitely right. That's the weight they carry. They calculated that the margin of error was acceptable. They understood that acceptable is not the same as good. Some are still alive, somewhere, watching. Most are just trying to rebuild something useful with their hands.

The game does not resolve whether they were right.

Full character lore: `docs/story/characters/the-elders.md`

### Enemy Archetypes

#### Phase 1 Enemies

| Name | Concept | Behavior | Room |
|---|---|---|---|
| **Gnome Soldiers** | Garden gnomes imagined as stocky armored soldiers in terracotta plate mail | Patrol, charge on sight, simple melee — never alone; always 3+ | Room 1 |
| **Leaf Pile Lurkers** | Dead leaves given form — hunched figures with no face, just shadow | Hide in piles, ambush, retreat into cover | Room 2 |
| **Sprinkler Sentinel** | Garden pop-up sprinkler imagined as a rotating turret with a glowing blue eye | Holds ground, sweeping water bursts, Overheated weak-point window | Room 2 |
| **SpinCycle** (boss) | Washing machine drum-headed heavyweight brawler — porthole is the eye and weak point | 2-phase fight | Room 3 |

**Sprinkler Sentinel:** Sweeping water attacks are ground-level — the player can jump over them. Parrying a burst during Locked state triggers Overheated early (the only mid-fight way to open the weak-point window ahead of schedule).

**SpinCycle weapon — Suds Blob:** In Phase 2 of the fight, SpinCycle vents laundry foam through the drum seals. Blobs pool across the arena and expand toward the player. When struck, each blob divides into two smaller, slightly faster ones. The fight requires managing both SpinCycle and the foam field simultaneously. See `docs/story/enemies/spincycle-boss.md`.

Full enemy lore (voice, audio cues, creature descriptions): `docs/story/enemies/`

#### Phase 2 Enemies — The Cul-de-Sac

| Name | Concept | Behavior |
|---|---|---|
| **WagonWheelRoller** | A wagon wheel rolling on its own, imagined as a charging brawler | Patrols; aggros at range; spin wind-up → AddForce charge; stunned on wall hit or after distance |
| **HitchingHound** | A low, fast creature — braided-rope dog, part dog, part lasso | Circles player; attacks exposed flanks; Ankle Wrap (brief root if connects) + Body Slam |
| **MilepostMarshal** | A road signpost with two rod arms — an iron lawman | Patrols waypoints; guard arm blocks frontal melee (0 damage); Slam cone + Retreat + 180° Sweep; counter staggers with directional knockback |
| **SprinklerSentinel (V2)** | Phase 1 Sentinel recontextualized as a saloon-bouncer guardian | Same mechanics as Phase 1; NavMeshObstacle so enemies path around it; gold badge-eye texture variant |

**MilepostMarshal state machine:** `Patrol → Alert → WindUp → Slam → Retreat → SweepWindUp → Sweep → Stunned → Dead`
- Guard arm (right): raised in Alert+; melee from guarded arc = 0 damage; parry and counter always bypass
- Slam: ground indicator cone (shadow disc) before firing — required for mobile readability
- Retreat telegraph: both arms raise; player closing in triggers Sweep; holding position lets it whiff → teaches patience vs. aggression read

#### Phase 2+ Later Enemy Archetypes

| Name | Concept |
|---|---|
| **Factory Drones** | Unimaginative workers in cardboard-mill ruins |
| **Grey Architects** | Unimaginative planners demolishing reclaimed zones |
| **Attic Golems** | Unclaimed junk animated by residual imagination energy |
| **The Foreman** | Mid-tier boss — runs a seized cardboard mill |

---

## 10. World / Settings — The Post-Apocalyptic Imagination War

### Lore

**The Great Hush** is what adults call it, when they think children aren't listening. They say it quietly. They don't elaborate.

The Internet didn't break. It was turned off — deliberately, by engineers. Infrastructure people: Network Engineers, DevOps Engineers, Support Engineers — the people who made sure things were working. The ones who knew where the actual wires went and had spent years watching something go wrong that they couldn't explain to anyone who hadn't already seen it. Neighbors who shared a wall and had never spoken. Children sitting in the same room as their parents, somewhere else entirely. Whole communities that had outsourced the act of being present to a device that fit in a pocket.

They talked about it among themselves for a long time. Then they stopped talking and did something.

**The Great Hush happened in the present day.** No countdown, no announcement. One morning the connection was simply gone. Adults stood in yards and parking lots, holding phones that had become rectangles of glass. Some of them cried. Some of them looked at the sky as if noticing it for the first time.

The Elders were right about the diagnosis. People had forgotten how to build things, fix things, maintain things — objects, homes, and relationships with other people. Whether the Elders were right about the cure is the question the game lives inside. The game does not resolve it.

**The Unimaginative** are not villains. They are the wound the cure accidentally made worse — people who had outsourced everything to the network and had it taken away with nothing to fill the gap. The grey settled in. Some neighborhoods never came back.

Kid's parents had never allowed a phone. When the world went quiet, he went outside, found a cardboard box, put it on his head — and saw everything. The box doesn't create the world. It is a lens: a way of seeing what was always there.

**The World Tree** is three hundred years old — the last living tree on Earth, the last source of cardboard. The Unimaginative didn't cut it down; they built mills around it and left it standing because a dead tree produces dead cardboard, and a dead box is just packaging. They needed it alive. The player's journey is a march toward the World Tree — zone by zone, restoring imagination to a world that forgot. Full World Tree lore in `docs/story/narrative/world-tree.md`.

### Phase 1 Zone — The Backyard

**Room 1 — The Dojo Courtyard**
- Feudal Japanese training ground. Stone walls, training dummies, cherry blossom apple tree, paper lanterns.
- Enemies: 2–3 Gnome Soldiers
- Objects: Broomstick (Bo Staff), Ruler (Shurikens)

**Room 2 — The Garden Gauntlet**
- Zen rock garden. Animated water features, paper cranes, elevated stone pathways.
- Enemies: 1 Sprinkler Sentinel + Leaf Pile Lurkers + 1–2 Gnome Soldiers
- Objects: Garden Hose (Water Whip), Cardboard Tube (Katana)
- Safe Zone: box cosmetic switch + IP shop before boss

**Room 3 — The Spin Arena (Boss)**
- Circular colosseum of stacked washing machine drums. Spinning mosaic floor. Raised center platform + lower outer ring.
- Boss: SpinCycle (2-phase)
- Win: defeat SpinCycle → Imagination Restore → full color blooms

### Phase 2 Zone 1 — The Cul-de-Sac

**Reality:** A curved suburban dead-end. Cracked asphalt, dark-windowed houses with for-sale signs, abandoned minivans, empty mailboxes, a cracked stone birdbath on the center island — the Unimaginative's Command Node, broadcasting grey static across the block.

**What Kid sees:** A Wild West main street. The minivans are covered wagons. The houses have saloon fronts painted over their vinyl siding. Hitching posts stand at the curb. The birdbath is a water trough. The sky is perpetually mid-afternoon golden hour; The Unimaginative's presence drains it grey and Kid's presence pushes the gold back in.

**Color palette:** Burnt amber sky, cracked terracotta ground, weathered-tan saloon facades, rust red / saddle brown accents. Warm shadows (deep burnt sienna, not black). Full ENV spec in `docs/art-style-guide.md`. Color table in `docs/GDD-v2-cul-de-sac.md` Section 1.

**Room structure** (5 combat rooms + boss — full design in `docs/GDD-v2-cul-de-sac.md`):
- Room 1 (fixed): The Arrival — 2–3 WagonWheelRollers; orientation room
- Rooms 2–4 (random draw): Ambush Alley / Saloon Front / Mailbox Row — escalating enemy mix
- Room 5 (fixed): The Town Square — all enemy types; Sprinkler Sentinel guards Command Node
- Boss Room: The Showdown Circle — SpinCycle V2 (same move set, 15% faster Phase 1 timing, new Dust Devil + Gallows Run attacks)

**Enemies:** WagonWheelRoller, HitchingHound, MilepostMarshal, SprinklerSentinel (V2 gold-badge-eye texture variant)

**ENV props:** All props sourced from **Low Poly Mega Pack - Polyworks** (Unity Asset Store). Covered Wagon, Saloon Front Facade, Hitching Post, Water Trough, Wanted Poster, Mailbox Telegraph Office, Gallows Frame, Rain Barrel, Lamp Post (Western), Command Node Birdbath, Tumbleweed (static prop), Saloon Sign Board

**Win condition:** Defeat all enemies in Room 5 → Command Node birdbath shatters → boss trigger → defeat SpinCycle V2 → Imagination Restore fires (warm gold bloom across the full street, then vivid full color)

---

### Phase 2+ Zones

| Zone | Phase | Reality | Imagination | Story Beat |
|---|---|---|---|---|
| **The Garage** | 2 | Collapsed workshop | Dungeon with forge weapons | First Unimaginative factory outpost |
| **The School Hallway** | 2 | Crumbling school corridor | Castle gauntlet hall | The Unimaginative tried to end education here |
| **The Basement** | 2 | Flooded ruins | Underground dungeon / cave | Hidden cache of old cardboard |
| **The Attic** | 2 | Collapsed attic, junk | Wizard's tower | Clues about the Elder BoxHead |
| **The Cul-de-Sac** | 2 | Cracked streets | Wild West main street | Mid-Phase 2 showdown climax |
| **The Cardboard Mill** | 2 (late) | Seized factory | Industrial dungeon | Phase 2 climax; first glimpse of World Tree |
| **The World Tree** | 3 | Last living tree | Enormous living fortress | Final zone — Elder BoxHead mystery resolved |

---

## 10.1 Hard Knocks Workshop — Real-World Skill Mapping

The world of BoxForged was built by people who made things. The skills Kid and his allies use in the game are not invented for convenience — they are the real skills The Great Hush forced children to rediscover: how to navigate without a phone, how to use an axe, how to start a fire, how to build something by hand.

**Hard Knocks Workshop** (Lehi, Utah) teaches these exact skills to kids ages 10–15. The mapping below is intentional, not decorative. A kid who completes axe-work at HKW and then unlocks the Splitting Maul in the game should feel that the game recognized what they did in the real world — because it did. These four abilities are designed from the start to serve as Phase 3 real-world unlock rewards (QR codes issued by HKW instructors on skill completion).

| HKW Real-World Skill | In-Game Ability | Phase Available | Unlock Method (Phase 3) |
|---|---|---|---|
| **Navigation** | **Compass Dash** — a dodge upgrade that locks Kid's movement to precise cardinal directions, bypassing terrain geometry and enemy positioning. Where a standard dodge goes roughly where the stick points, the Compass Dash snaps to the nearest cardinal angle. In Kid's imagination: the compass his grandfather kept in a drawer, finally in use. | Phase 2 | HKW wilderness navigation certification → QR code |
| **Axe-work** | **Splitting Maul** — a charged overhead slam. Kid winds up fully (held ATTACK), then drives straight down. Staggers all enemies in a wide cone in front of him. The animation is deliberate, committed, and heavy — it rewards setting up the moment rather than spamming. Un-interruptible once the downswing starts. | Phase 2+ | HKW axe-work certification → QR code |
| **Fire-starting** | **Ember Charge** — a weapon special mode available on any melee weapon after a successful parry. The parry "strikes the flint." One follow-up hit within the Counter Window deals fire damage and leaves a brief burning effect on the struck enemy. Available on Cowboy Style first; extends to other styles in Phase 3. | Phase 2+ | HKW fire-starting certification → QR code |
| **Woodworking / building** | **Craftsman Skin Tier** — a cosmetic weapon skin layer applied to any weapon, making it look hand-carved and personalized: rough tool marks on the handle, a wrap of cord at the grip, initials scratched into the flat. Each HKW student's skin is unique (generated from their enrollment ID). No gameplay effect. The skin persists across runs. | Phase 3 | HKW woodworking project completion → QR code |

### Design Intent

These are not Easter eggs. They are design bridges — each one built so that the game experience reinforces what the real skill taught, and the real skill makes the game feel earned. The Splitting Maul is a heavy, deliberate action because axe-work is a heavy, deliberate skill. The Compass Dash rewards directional precision because navigation is precision. The connection should be obvious to a kid who has done both.

Phase 3 implementation will require QR code issuance infrastructure on the HKW side and a redemption flow inside the game (likely a code-entry screen on the meta menu). The ability designs above are finalized so that implementation does not require mechanics redesign when Phase 3 begins.

---

## 11. Mobile Controls & Touch UI

### Screen Orientation
**Landscape only.**

### Camera
**Fixed top-down. No rotation.** Cinemachine, offset `(0, 12, -8)`, hard look-at player. Final.

### Control Layout — Diamond Button Arrangement

```
┌─────────────────────────────────────────────────────┐
│                                                     │
│                   [game world]                      │
│                                                     │
│  ┌──────────┐                    [JUMP]             │
│  │          │               [PARRY]   [DODGE]       │
│  │ joystick │                    [ATTACK]           │
│  │  (move)  │                                       │
│  └──────────┘                                       │
└─────────────────────────────────────────────────────┘
```

| Button | Action |
|---|---|
| ATTACK (South / bottom) | Primary strike; aerial strike when airborne |
| DODGE (East / right) | Directional roll; dodges backward if joystick neutral |
| PARRY (West / left) | Timed parry stance |
| JUMP (North / top) | Launch airborne; press ATTACK while airborne for aerial strike |

### Parry on Touchscreen
Window: **0.3–0.35s** (vs. 0.2s on gamepad). Parry button pulses when an enemy is mid-wind-up.

### Counter Window on Mobile
- Screen edge flash in box color
- ATTACK button glows and pulses for ~1.5s
- Short haptic vibration on parry success

### Safe Zone UI
Combat buttons replaced by:
- **SWITCH BOX** — opens box cosmetic selector
- **SHOP** — opens IP shop (power-ups, health restore, one-time moves)

### HUD Elements (Phase 1)

```
┌─────────────────────────────────────────────────────┐
│ [HEALTH BAR]      [BOX ICON | STYLE ICON]  [IP ///] │
│                   [CHARGE METER      ]              │
│                                                     │
│                   [game world]                      │
│                                                     │
└─────────────────────────────────────────────────────┘
```

- **Health bar** — top-left, crayon-drawn style, green → red
- **IP counter** — top-right, tally mark style
- **Box + Style icons** — top-center, small
- **Charge meter** — below style icon; fills with combo hits; full = special move available
- All elements respect iOS safe area and Android cutout areas

### Design Constraints
- Minimum touch target: **48×48 points**
- No UI in bottom ~60pt (home indicator area)
- Legible at 375pt width (iPhone SE)

---

## 12. Multiplayer Architecture — Co-op First

Phase 1 is single-player. All systems must be architected for 2 players from day one.

### Architecture Rules

- All player-facing systems support 2 independent instances
- Enemy AI handles 2-player targeting (threat/aggro splitting)
- No singleton patterns that break with a second player

### Phase 2 (Local Co-op)
- Both players choose boxes and styles independently
- Separate IP pools and Spark pools per player

### Phase 3+ (Online)
- Unity Netcode for GameObjects (NGO)
- Versus mode as a separate mode — competitive scoring

---

## 13. Progression System

### Two-Pool IP System (Phased)

#### Phase 1 — In-Run IP

**Imagination Points (IP)** are earned during a run. Combo length multiplies IP per kill. Spent during the run only — does not carry between runs.

**How IP is spent:**
- **Mid-room upgrades (Hades-style):** After each combat room, choose 1 of 3 upgrades. Run-specific enhancements (e.g., "aerial hits 2 enemies," "counter window lasts 3s").
- **Safe Zone shop:** Spend IP on health restore, single-use special moves, temporary boosts.
- IP remaining at run end is scored.

#### Phase 2 — Meta-Progression Spark

At run end, a % of earned IP converts to **Spark** — persistent meta-currency that carries between runs.

**Spark is spent on:**
- Box cosmetics (if not purchased via IAP)
- Fighting Styles (if not purchased via IAP)
- Permanent minor upgrades (small; skill matters more than grind)

Conversion rate to be tuned through playtesting.

### Progression Philosophy

No stat numbers shown to players. Upgrades are expressed imaginatively: "Your broomstick is now a *legendary* staff." The player never grinds — they discover.

---

## 14. Monetization Model

Free to download. Revenue from ads and optional IAP. No purchase required to experience all gameplay.

### Ad Formats

| Format | When | Notes |
|---|---|---|
| Interstitial | Between runs, on the meta screen | Frequency-capped |
| Rewarded (optional) | In-run, after death or at shop | Player opts in for: one revival OR bonus IP |

Remove Ads purchase eliminates interstitials. Rewarded ads remain opt-in.

### In-App Purchases

| Item | Type | Description | Price Range |
|---|---|---|---|
| Box Skin Packs | Cosmetic — permanent | New box appearance; no gameplay effect | $0.99–$2.99 |
| Fighting Style Packs | Gameplay — permanent | Unique aerial, special move, and passive | $1.99–$3.99 |
| Power-Up Starter Bundles | Consumable — run-start | Applied at run start; single-use | $0.99 packs |
| Remove Ads | Permanent | Removes interstitial ads; rewarded ads remain opt-in | $4.99 one-time |

### Balance Principles

- No pay-to-win: purchased styles are not stronger, just different
- All styles and boxes earnable through Spark — IAP is a shortcut, not a gate
- Power-up bundles are the only IAP that cannot be earned through normal play

---

## 15. Phase 1 Scope — Minimum Viable Fun

**The one thing Phase 1 must nail:** Different Fighting Styles genuinely change how you play — and dodge-parry-jump combat is readable, fair, and satisfying.

### Prototype Run Structure

```
Run start → Choose Fighting Style (Ninja or Cowboy)
         → Room 1: Dojo Courtyard (Gnome Soldiers)
         → Between rooms: 3-upgrade choice (Hades-style)
         → Room 2: Garden Gauntlet (Sprinkler Sentinel + Leaf Pile Lurkers + Gnomes)
         → Safe Zone: box cosmetic switch + IP shop
         → Room 3: Spin Arena (SpinCycle boss fight)
         → Run End: IP scored → Spark conversion → meta screen
```

### Locked Scope

**In:**
- One location: The Backyard (3 rooms)
- Two box cosmetics: Ninja Box, Cowboy Box (default-unlocked)
- Two fighting styles: Ninja Style, Cowboy Style (default-unlocked)
- Jump + aerial attacks (style-dependent)
- Three objects: Broomstick, Ruler, Garden Hose (placeholder behavior)
- Three enemy types: Gnome Soldiers, Leaf Pile Lurkers, Sprinkler Sentinel
- One boss: SpinCycle (2-phase)
- In-run IP system: mid-room upgrades + Safe Zone shop
- Imagination Restore on boss defeat
- Player naming screen (default: Kid)
- Win condition + Spark conversion screen

**Out (Phase 2+):**
- Meta-progression Spark system
- IAP / ad integration (architecture only)
- Co-op
- Multiple locations, save/load, inventory UI, dialogue system, visible stat numbers
- Full weapon/object synergy system

**Target play time:** 10–15 minutes per run.

---

## 16. Open Questions

| # | Question | Why It Matters |
|---|---|---|
| 1 | **Elder BoxHead identity** | Late-game twist — needed before World Tree zone design |
| 2 | **Versus mode rules** | Competitive scoring structure undefined |
| 3 | **Cardboard Mill boss** | Phase 2 climax boss undefined |
| 4 | **Full quest system** | Phase 2 quest structure undefined |
| 5 | **Spark conversion rate** | Needs playtesting to tune — how much Spark per run? |
| 6 | **IAP final pricing** | Estimates listed; confirm before App Store submission |

### Locked Decisions Log

| # | Question | Answer | Version |
|---|---|---|---|
| 1 | Player character name | Player-named; default "Kid" | 0.2 |
| 2 | Combat system | Dodge-Parry (Sekiro-influenced) | 0.2 |
| 3 | Box switching | Safe zones only, no cooldown | 0.2 |
| 4 | Multiplayer direction | Co-op first; versus in Phase 3+ | 0.2 |
| 5 | The Skeptic | Permanent antagonist; nameless archetype | 0.2 / 0.3 |
| 6 | Long-term goal | Reclaim the World Tree from The Unimaginative | 0.2 |
| 7 | Co-op IP pool | Separate pools per player | 0.3 |
| 8 | Cardboard Mill phase | Late Phase 2 climax zone | 0.3 |
| 9 | Elder BoxHead | Late-game mystery / plot twist | 0.3 |
| 10 | Netcode library | Unity Netcode for GameObjects (NGO) | 0.3 |
| 11 | Phase 1 boss | SpinCycle (washing machine brawler) | 0.5 |
| 12 | Enemy aggression | Always attacking; Option C stagger | 0.5 |
| 13 | Backyard layout | 3-room progression | 0.5 |
| 14 | New Phase 1 enemy | Sprinkler Sentinel | 0.5 |
| 15 | Sprint plan | Sprints 7–10 locked | 0.5 |
| 16 | Game structure | Roguelite — each run starts fresh | 0.6 |
| 17 | Jump mechanic | Jump button; ATTACK while airborne = aerial (style-dependent) | 0.6 |
| 18 | Box system | Boxes are cosmetic-only | 0.6 |
| 19 | Fighting Styles | Separate gameplay class — aerial, special, passive per style | 0.6 |
| 20 | IP system | Two-pool: in-run IP + meta Spark (phased) | 0.6 |
| 21 | Monetization | Free-to-play: ads + IAP (box skins, styles, power-up bundles, remove ads) | 0.6 |
| 22 | Camera | Fixed top-down, no rotation. Final. | 0.6 |
| 23 | Control layout | Diamond button arrangement (ATTACK / DODGE / PARRY / JUMP) | 0.6 |
| 24 | Control layout positions | ATTACK=South, DODGE=East, PARRY=West, JUMP=North (overrides v0.6 diagram which had PARRY=top/JUMP=left) | 0.7 |
| 25 | V2 Cul-de-Sac enemy roster | WagonWheelRoller, HitchingHound, MilepostMarshal (melee brawler), SprinklerSentinel V2 | 0.7 |
| 26 | MilepostMarshal design | Melee brawler — Patrol→Alert→Slam→Retreat→Sweep; guard arm blocks frontal melee; NOT the stationary ranged stop-sign from prior zone doc | 0.7 |
| 27 | V2 Cul-de-Sac weapons | Jump Rope→Lasso (T2), Bike Horn→Dynamite Bundle (T3), Garden Trowel→Quickdraw Blade (T1), Watering Can→Six-Shooter (T2) | 0.7 |

---

## Appendix: Terminology Reference

| Term | Meaning |
|---|---|
| The Great Hush | The inciting event — the day the Elders killed the Internet. Present day. Adults whisper it. Full lore: `docs/story/great-hush.md` and `docs/silence-naming-options.md` |
| The Elders | The engineers and infrastructure people who coordinated The Great Hush. Not villains — morally complex. Full lore: `docs/story/characters/the-elders.md` |
| Box | Cosmetic identity — the cardboard box on the player's head; visual only (v0.6+) |
| Fighting Style | Gameplay class for a run — determines aerial attack, special move, passive bonus |
| Object | Real-world item reimagined as a weapon or tool |
| Imagination Points (IP) | In-run currency earned from combat; spent on mid-run upgrades and shop items |
| Spark | Meta-currency; % of run IP converts to Spark at run end; persists between runs |
| Imagination Tier | Power level of an object's transformation (1–3) |
| Combo Imagination | Visual effect from a specific Box + Style pairing |
| The Skeptic | Recurring antagonist; nameless; disrupts imagination on hit |
| The Unimaginative | Overarching antagonist organization; seized the World Tree |
| The World Tree | Last living tree on Earth; source of all remaining cardboard |
| Imagination Restore | Visual event when a zone is reclaimed — color and vibrancy return |
| Imagination Overlay | Dual-layer visual system blending reality and imagination states |
| Safe Zone | Between-encounter area: box cosmetic switching + IP shop |
| Counter Window | ~1.5–2s window after a successful parry — free hit or special move |
| Charge Meter | Fills with combo hits; full charge enables the Fighting Style's special move |
| Run | A single play session from style selection to boss defeat or death |
| Aerial Attack | ATTACK while airborne; animation and effect are style-dependent |
| Kid | Default player character name |

---

_GDD-V2 v0.8 — Prototype design locked. Phase 2 Cul-de-Sac design locked. Lore and character canon locked. See GDD.md for Phase 1 detail and legacy content. See GDD-v2-cul-de-sac.md for full Cul-de-Sac room/enemy/weapon/ENV spec. See docs/story/ for all narrative content._
