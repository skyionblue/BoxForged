# World 2 — The Backyard (Dojo): Zone Layout & Encounter Spec

**Status:** Design spec, ready for implementation handoff. Not yet built.
**Date:** 2026-09-01
**Scene:** `Assets/_Project/Scenes/Backyard_Dojo.unity` (scaffolded, empty)
**Binding architecture:** [ADR-0005](../../../../adr/0005-world2-single-continuous-scene.md) — one continuous scene, three `RoomManager` zones
**Supersedes:** `unity-blueprint.md` (entirely) and `gdd.md` §2 "Room Structure" + §10 "Run integration". The rest of `gdd.md` (§1 tone/palette, §3 Crane Duelist, §4 returning enemies, §5 Grasscutter, §6 weapons, §7 ENV props, §8 difficulty scaling, §9 lore hook, §11 craftsmanship dressing, §12 open questions) **remains live reference and is not superseded.**
**Story canon:** `docs/story/zones/backyard-dojo.md`, `docs/story/enemies/crane-duelist.md`, `docs/story/enemies/grasscutter-boss.md` — CANON, not modified by this document.

---

## 0. Coordinate convention — read this first

`[ENV - Static]` in the scaffolded scene is confirmed at **position (0,0,0), rotation identity, `m_LocalEulerAnglesHint` (0,0,0)**, and `LevelBuilder._cameraYawDegrees = 0`. This is ADR-0005 §6.1 honoured at authoring time.

**Therefore: every coordinate in this document is a world coordinate. There is no local↔world transform.** Unlike World 1 (ADR-0004 §0, `[ENV - Static]` rotated 45°), there is a single coordinate space and design numbers are Inspector numbers.

- **+Z is north** and is the direction of travel. The player walks from Z ≈ 0 to Z ≈ 55.
- **+X is east.**
- The camera sits behind the player at **−Z** (`pfb_CM_FollowCam` verified at pitch 36°, yaw 0, FOV 45).
- **Player spawn is (0, 0, 0)** — already authored in the scaffold as `[Player]/pfb_player`. Do not move it.

### Camera framing numbers this layout is designed against (ADR-0001 §Resulting framing)

| Quantity | Value | Consequence for this layout |
|---|---|---|
| Ground visible **ahead** of Kid (F) | **15.3 m** | Nothing the player must read may sit more than 15.3 m north of them |
| Ground visible **behind** Kid (R) | **4.2 m** | The south stockade must be within ~4 m of spawn or it is off frame |
| Lateral width at Kid's depth (W) | **16.8 m** | **This is why the boss arena is 17.0 m across, not 36 m** |
| Camera pitch | 36° | `tan(36°) = 0.7265` — used in the occlusion math below |
| Top view-frustum ray | 13.5° below horizontal | `tan(13.5°) = 0.2401` — used in the tall-prop math below |

**Derived rule A — wall/prop occlusion of the player.** A prop of height `h_p` standing `d` metres *behind* (south of) the player occludes the player when `d < (h_p − 1.0) / 0.7265` (1.0 m ≈ player centre height). This is why the stockade is specified at **2.4 m**: it only occludes within **1.93 m** of it, so a player standing anywhere on the yard floor with ≥2 m of clearance to the south wall is never hidden. A 4.2 m shed occludes within 4.4 m; a 6 m prop within 6.9 m. Tall props therefore go **north** of where the player fights, never south of it.

**Derived rule B — tall props leaving the top of frame** (ADR-0001 §Consequences, B57). A point at height `h` stays in frame only while its distance *ahead of the player* is `≤ (6.35 − h) / 0.2401 − 7.57`. Concretely: **a point at 4.0 m is visible only within 2.2 m ahead of the player; a point above ~4.5 m is effectively never in frame.** This constraint is the reason for §1.4's finding about the Assembly Beat, and for the cherry tree's authored canopy height in §4.

---

## 1. Whole-yard structure

### 1.1 Zone table

| Idx | Zone | Z span | Depth | X span | Width | Activation | Reward screen |
|---|---|---|---|---|---|---|---|
| 0 | **The Back Gate / Dojo Courtyard** | −3.0 → 17.0 | 20.0 m | −7.5 … +7.5 | 15.0 m | Auto on `LevelBuilder.OnNavMeshReady` (B49) — no trigger | Upgrade |
| 1 | **Garden Gauntlet** (contains the Koi Pond / engawa sub-space) | 17.0 → 39.0 | 22.0 m | −7.0 … +9.5 | 16.5 m | `RoomTrigger` `roomIndex = 1` | Shop |
| 2 | **The Garden End — Blossom Court** | 39.0 → 56.0 | 17.0 m | −8.5 … +8.5 | 17.0 m | `RoomTrigger` `roomIndex = 2` | none — `bossOwnedWin: true` |

`roomName` values are already correct in the three scaffolded `RoomDataSO` assets and match ADR-0005 §1's table. Do not rename them.

Zone widths are all ≤ 17.0 m so both stockade walls stay inside the camera's **16.8 m** visible lateral width (ADR-0001) at any player position near the centreline. Zone 1 is the widest non-arena zone at 16.5 m and is deliberately **asymmetric** (west wall at −7.0, east at +9.5) — see §1.3.

### 1.2 Ground

The scaffold's `[ENV - Static]/Ground` is a built-in Plane at position **(0, 0, 25)**, scale **(4, 1, 7)** → 40 m × 70 m, spanning X −20…+20, Z −10…+60. That is far larger than the yard and re-creates World 1's B99 problem: walkable ground outside the boundary.

**Resize it** to the yard's bounding box plus a 1.0 m band under the stockade:

| Field | Value | Result |
|---|---|---|
| `localPosition` | `(0, 0, 26.0)` | — |
| `localScale` | `(2.1, 1, 6.1)` | 21.0 m × 61.0 m → X −10.5…+10.5, Z −4.5…+56.5 |

This leaves four dead strips outside the stockade (widest ~3.0 m, west of zone 1). They are unreachable if the stockade seals, which §6's flood-fill must **prove**, not assume. Trimming Ground into three per-zone quads is a valid alternative if NavMesh vertex count or `ValidateCameraClearance` violation counts come in high; do not do it pre-emptively — one Ground plane is one collider and World 1's entire 59.5 m street baked to 1,303 NavMesh verts.

### 1.3 The three zones must not reuse each other's shape (ADR-0005 §6.8)

The "no room reuses another room's shape/prop layout, ever" rule is **not** suspended for World 2. Evidence that it is satisfied:

| | Zone 0 | Zone 1 | Zone 2 |
|---|---|---|---|
| Plan form | Wide shallow **rectangle**, bilaterally symmetric | Long asymmetric **dog-leg** (open lanes → raised L around water) | **16-gon ring** with one central obstruction |
| Elevation | Single flat plane | Two levels: gravel at 0.0, engawa boards at **+0.35 m** | Single flat plane |
| Cover | **None** — deliberately bare | Heavy: 12 posts, a shed, a no-stand water body | None *after* the court clears (§4.3) |
| Movement grammar | **Free / omnidirectional** | **Channelled** — three N–S lanes, then a 2.0 m-usable board run | **Orbital** — circling one centre |
| Prop count | ~34 | ~62 | ~28 |
| Player's read | "This place has rules" | "This place was lived in" | "This place has been swept for a duel" |

The prop-count gradient (34 → 62 → 28) is deliberate and is also a performance decision: peak dressing density lands in the middle zone, **not** in the boss zone where the Grasscutter's VFX and pooled hazard volumes arrive.

### 1.4 Finding: the Assembly Beat's CANON imagery does not survive the gameplay camera

`docs/story/zones/backyard-dojo.md` §Zone Intro (CANON):

> *"The grass stops leaning. It stands up straight, all of it, in rows. **The shed's roof lifts.** Stones find the path like they'd been waiting to. **There's a tree at the far end shaking pink all over the ground.**"*

Both bolded images are undeliverable on the ADR-0001 rig from the spawn point:

- **The shed's roof** is at ~4.2 m. By Derived rule B a 4.2 m point is in frame only within ~1.6 m ahead of the player. The shed is at Z 30–38; the player is at Z 0.
- **The tree at the far end** is at Z 47.5, i.e. **47.5 m** ahead against F = **15.3 m**, and its crown is above 4.5 m, so it is never in frame at any distance.

This is a camera/narrative conflict, not a story problem, and it is not resolvable by moving geometry — moving the shed or the tree south breaks §4's arena and §1.1's zone spans.

**Recommendation (owner call — do not implement without it): the 3-second Assembly Beat is a scripted camera beat, not a gameplay-camera moment.** The project already has the mechanism (`SpinCycleAI`'s boss-intro camera; ADR-0001 permits authored non-gameplay cameras and rejects only *dynamic gameplay* rigs). A single 3 s dolly from behind the torii, raised to ~9 m and pitched ~28°, looking north up the full 60 m of yard, shows the whole thing assemble — roof, stones, rows, and the tree at the far end — and then hands back to the fixed rig. This also pays off ADR-0005's central claim: the yard is one continuous space, and the intro is the one shot that proves it.

**Scope note:** this is a scripted camera that ADR-0005 §7 did not budget ("new assets only: the scene, three `RoomDataSO`s, one `WeaponDropTableSO`, one scene-beats script"). Flagged, not absorbed.

**Fallback if the owner declines the scripted camera:** the beat is reduced to what is genuinely in frame from (0,0,0) — the gravel rows straightening, the stepping-stone spine snapping into place, the four stone lanterns rising, the torii's lacquer saturating. The shed roof and the tree are cut from the *visible* beat and survive only in the voice-over line. This is a real loss of CANON imagery and should be an owner decision, not an implementer's.

### 1.5 The 3-second aggro delay has no data affordance — mechanism decision required

`RoomDataSO` has exactly four fields: `roomName`, `maxConcurrentEnemies`, `spawnPoints[]`, `bossOwnedWin`. `RoomSpawnPointEntry` has `enemyPrefab`, `position`, `facingY`, `spawnCount`. **There is no aggro-delay, wave-timer, or activation-delay field anywhere in the data layer**, and the old blueprint's `EnemySpawner.aggroDelay = 3f` refers to the *other*, parallel spawn system (`Enemy/EnemySpawner.cs`) which the `RoomManager`/`RoomDataSO` path does not use (ADR-0004 §6.5 records the two-system divergence).

**Recommended mechanism:** `BackyardDojoBeats` (the ADR-0005 §5 scene-local script) subscribes to `LevelBuilder.OnNavMeshReady`, plays the 3.0 s assembly VFX (and the §1.4 camera), and **only then** lets zone 0 activate. Because zone-0 enemies are spawned *by* activation, nothing needs an aggro hold — the delay is a deferral of `RoomManager`'s index-0 activation, not enemy suppression.

**Open implementation question for `unity-gameplay-engineer`:** `RoomManager` currently auto-activates room 0 the moment `OnNavMeshReady` fires (B49). Deferring it requires either a small seam on `RoomManager` or a delayed `OnNavMeshReady` raise. Do not guess — read `RoomManager.cs` and pick the change that does not race B49. Whatever the mechanism, the player must retain movement control during the 3 s (they are walking in through the gate).

### 1.6 Zone boundaries — gates and triggers derived from one authored value each

ADR-0005 §6.4 requires each `RoomTrigger`'s width and its paired `RoomGate`'s width to come from **one** authored value (World 1 drifted 22 vs 26; a `RoomTrigger_Zone2` bypass is a silent, permanently-unwinnable run).

Two authored constants:

```
ZONE1_BOUNDARY_WIDTH = 10.5   // zone 0 → zone 1
ZONE2_BOUNDARY_WIDTH =  8.8   // zone 1 → zone 2
```

| Object | Position | Size | Notes |
|---|---|---|---|
| `RoomGate_Zone0` (opens on zone-0 clear) | `(0.25, 0, 17.0)` | `(10.5, 4, 1)` | Spans X −5.0…+5.5, Z 16.5…17.5. Overlaps each stockade return by 1.0 m |
| `RoomTrigger_Zone1` (`roomIndex = 1`) | `(0.25, 1, 19.0)` | `(10.5, 3, 3)` | Spans Z 17.5…20.5 — **adjacent to, not overlapping, the gate** |
| `RoomGate_Zone1` (opens on zone-1 clear) | `(0, 0, 39.0)` | `(8.8, 4, 1)` | Spans X −4.4…+4.4, Z 38.5…39.5. Overlaps each return by ~1.0 m |
| `RoomTrigger_Zone2` (`roomIndex = 2`) | `(0, 1, 41.0)` | `(8.8, 3, 3)` | Spans Z 39.5…42.5 |

**Changes from the scaffold:** the scaffolded `RoomTrigger_Zone1` is at `(0,1,20)` and `RoomTrigger_Zone2` at `(0,1,40)`, both `size (30, 4, 2)`. Move Zone1 to Z 19.0 and Zone2 to Z 41.0, and **narrow both from 30 m to the values above** — a 30 m trigger extends past the stockade into the dead strips and past the arena wall. Trigger depth goes **2 → 3 m**: World 1 authored 3 m explicitly "so a sprinting `CharacterController` cannot tunnel through them in one physics step" (ADR-0004 §1). Keep `position.y = 1`, `size.y = 3` or 4 (either spans the player capsule).

Order along the player's path is **gate → trigger**, so the trigger is unreachable until the gate opens. Neither gate exists in the scaffold; both must be added under `[Zone Boundaries]`, and `RoomGate.Open()` must disable child `Collider`, `Renderer`, **and `NavMeshObstacle`** (World 1's M9 fix — a closed-but-uncarved obstacle leaves a stale NavMesh hole after opening).

Both boundaries sit at a **real chokepoint**, per the World 1 pattern (a barricade must read as spanning a gap, not floating):

- **Z = 17.0** — a bamboo return from each side wall: west X −7.5…−4.0, east X +4.5…+10.5 (the east return also carries the 2.0 m jog out to zone 1's wider east wall). Walkable gap **X −4.0…+4.5 = 8.5 m**.
- **Z = 39.0** — the shed's north face (X −7.0…−4.0) plus returns X −4.0…−3.38 and X +3.38…+9.5. Walkable gap **X −3.38…+3.38 = 6.76 m**. This is the arena's own southern wall segment pair; you pass *through* the stockade into the court.

### 1.7 Perimeter — the boundary is diegetic and sealed from day one (ADR-0005 §6.3)

The bamboo stockade **is** the boundary. There are no invisible retrofit colliders (World 1 needed `StreetBoundary_West`/`_East` after a flood-fill found the bypass).

- **Height 2.4 m** — chosen from Derived rule A: it occludes the player only within 1.93 m of it, so it never hides a fight. **Do not change wall height to fill a span.** X-scaling a wall module is fine; Y-scaling is not.
- **Wall band 0.3 m thick**, collider matched to the visual mesh (ADR-0005 §6.2 — World 1's building colliders were up to 4 m wider than their meshes and every clearance number had to be recomputed).
- **Layer: `Building` (layer 8).** `LevelBuilder._cameraClearanceMask` is `m_Bits: 256` = `Building` only, so anything not on that layer is invisible to `ValidateCameraClearance`. Put the **stockade, the shed, and the cherry-tree trunk** on `Building`. Keep every low prop (lanterns, stepping stones, makiwara, grass, leaf piles, craftsmanship props) **off** it — they do not occlude and would only generate noise.
- **Continuous ring**, closing at the south wall (Z = −3.0, X −7.5…+7.5) with a 3.0 m torii opening at X −1.5…+1.5.

`PlayerController._arenaBoundaryRadius` is a **backstop only** (ADR-0005 §6.7). Size it once, generously, as a `pfb_player` scene-instance override: `_arenaCenter = (0, 0, 26.5)`, `_arenaBoundaryRadius = 30.0` — reaches Z −3.5 (south wall) and Z +56.5 (arena north rim + 0.5 m). Do not re-derive it per zone.

---

## 2. Zone 0 — The Back Gate / Dojo Courtyard

**Footprint:** X −7.5…+7.5 (15.0 m) × Z −3.0…17.0 (20.0 m). Flat, single plane.
**Role:** the Assembly Beat, and the World 2 gnome pack rhythm ("they no longer come one at a time" — `gdd.md` §2).
**Reward:** Upgrade screen (`GameManager.ShowRoomClearScreenDelayed` index 0).

### 2.1 Why it is bare

Zone 0 is the **only** zone with no cover, and that is the design. The lesson is a staggered four-gnome pack charge; cover would let the player solve it positionally instead of rhythmically. The formality — bilateral symmetry, a single stone spine, four lanterns in two matched pairs — is the Assembly Beat made permanent: *"The grass stops leaning. It stands up straight, all of it, in rows."*

### 2.2 Encounter — `RoomData_Backyard_Dojo_Zone0`

| Field | Current (scaffold) | **Spec** |
|---|---|---|
| `roomName` | `The Back Gate / Dojo Courtyard` | unchanged |
| `maxConcurrentEnemies` | `2` | **`4`** |
| `bossOwnedWin` | `false` | unchanged |
| `spawnPoints` | `[]` | **5 entries below** |

`maxConcurrentEnemies = 4` sits exactly on ADR-0005 §3's `≤ 4` live-enemy budget and on `gdd.md` §8's "concurrent gnome pack: max 4". That is deliberate: four-at-once *is* the zone's teaching goal. **Flagged as the tuning value most likely to need a playtest revision** — if 4 reads as unfair as the world's first encounter, drop to 3 and keep 5 spawns; do not add a sixth spawn.

Five spawns rather than `gdd.md` §2's three, for the same reason ADR-0004 §1 gave World 1 ("the old model was five encounters; this is three… §5 makes zones 0 and 1 substantially meatier"). World 2 lost two rooms to the 3-zone lock; zones 0 and 1 absorb the pacing.

**`RoomManager.TrySpawnNext` walks `spawnPoints` in array order** — array order is spawn order. Indices 0–3 are the seeded opening wave; index 4 refills on the first death.

| # | `enemyPrefab` | `position` | `facingY` | `spawnCount` | Reads as |
|---|---|---|---|---|---|
| 0 | `pfb_enemy_gnome_grunt` | `(−4.5, 0, 7.5)` | 180 | 1 | West flanker |
| 1 | `pfb_enemy_gnome_grunt` | `(4.5, 0, 7.5)` | 180 | 1 | East flanker — symmetric pair opens |
| 2 | `pfb_enemy_gnome_grunt` | `(0.0, 0, 12.0)` | 180 | 1 | Down the stone spine |
| 3 | `pfb_enemy_gnome_grunt` | `(−5.5, 0, 13.0)` | 175 | 1 | Deep west, arrives late in the seed |
| 4 | `pfb_enemy_gnome_grunt` | `(6.0, 0, 5.0)` | 195 | 1 | The refill that comes in *behind* the player's advance |

**Geometry checks (do not re-derive; verify these):**

| Check | Value | Constraint | Source |
|---|---|---|---|
| Whole-roster enclosing circle | centre `(0.25, 0, 9.0)`, **r = 7.00 m** | ≤ 9 m | TDD §6.4 / ADR-0005 §3 |
| Seed-wave (0–3) enclosing circle | centre `(−0.5, 0, 10.25)`, **r = 5.71 m** | ≤ 8.4 m (W/2) so all 4 fit on screen | ADR-0001 |
| Minimum pairwise spawn clearance | **2.92 m** | ≥ 1.5 m | ADR-0004 §5 authoring rule |
| Closest spawn to player spawn `(0,0,0)` | **7.81 m** (index 4) | ≥ 5 m | avoids spawn-on-player |

### 2.3 Props (~34)

Floor is **raked gravel** as a *ground material*, not a prop — the old blueprint's `Asian_Prop_Zen_Garden_Sand_01` does not exist in this project (§5.1) and a material costs zero draw calls.

| Prop | Prefab | Count | Positions |
|---|---|---|---|
| Torii gate | `pfb_env_torii_gate` | 1 | `(0, 0, −3.0)`, yaw 0 — set in the south wall opening, frames the shot |
| Stepping stones | `pfb_env_stepping_stone_tile` | 11 | X = 0, Z = 1.0 … 15.0 at 1.4 m spacing. Flat, walkable, no collider |
| Stone lanterns | `pfb_env_stone_lantern` | 4 | `(±6.5, 0, 3.0)`, `(±6.5, 0, 15.0)` — two matched pairs |
| Makiwara posts | `pfb_env_target_dummy` | 2 | `(−2.5, 0, 15.5)`, `(2.5, 0, 15.5)` — flank the exit gap; the dojo's threshold |
| Weapon rack | `pfb_env_weapon_rack` | 1 | `(−6.8, 0, 8.5)`, yaw 90 — against the west wall |
| Bamboo stockade (BD-01) | new | ~14 modules | Perimeter per §1.7 |

**Craftsmanship dressing** (`gdd.md` §11, non-interactive, all on the shared craftsmanship material):

| Prop | Position | Note |
|---|---|---|
| Child's practice sword, grip worn smooth | `(−1.2, 0, −2.7)` | Leaning on the torii's west upright |
| Chalk hopscotch grid, numbers half rained away | `(0, 0.01, 3.5)` | **A thin quad, not a decal** — see §5.4 |
| Single garden glove, fingers curled | `(7.2, 1.2, 6.0)` | On an east stockade post |
| Birdhouse, perch snapped off, still nailed level | `(−7.2, 1.9, 11.0)` | West stockade |

### 2.4 Loot — `WeaponDropTableSO_Backyard_Dojo`, zone-0 entries

| Entry | Position | Nearest-neighbour clearance |
|---|---|---|
| `workbenchPositions[0]` (forge) | `(−4.5, 0, 2.0)` | 4.0 m to spawn 0 |
| `cardboardPiles[0]` | `(4.5, 0, 1.5)` | 3.85 m to spawn 4 |
| `scatteredObjects[0]` | `(−6.0, 0, 8.5)` | 2.06 m to spawn 3 |
| `scatteredObjects[1]` | `(6.0, 0, 11.0)` | 6.18 m to spawn 2 |

### 2.5 Camera-clearance expectation

`ValidateCameraClearance` will report a violation band along the **south wall**: NavMesh vertices within 8 m north of Z = −3.0 have < 8 m clear behind. That band is ~8 m deep and unavoidable — every zone-0 layout has a south wall. It is a *diagnostic*, not a blocker (the script logs one summarised warning and does not stop Play Mode), and Derived rule A shows the 2.4 m wall does not actually hide the player beyond 1.93 m. Record the count; do not chase it. World 1's shipped figure was 104 / 1,303 vertices (8%).

---

## 3. Zone 1 — Garden Gauntlet (with the Koi Pond / engawa sub-space)

**Footprint:** X −7.0…+9.5 (16.5 m) × Z 17.0…39.0 (22.0 m). Two elevations: gravel at Y 0.0, engawa boards at **Y +0.35**.
**Role:** Crane Duelist debut, constrained footing, and the CANON Skeptic beat.
**Reward:** Shop screen (index 1).

Zone 1 absorbs three retired rooms. What survives from each is recorded in §7.

### 3.1 Sub-space 1A — The Gravel Lanes (Z 17.0 … 30.0)

Raked gravel crossed by **three north–south stepping-stone lanes** at X = −4.5, +1.0, +6.5, each 1.4 m wide. Movement is channelled by **makiwara posts** (`pfb_env_target_dummy`) set in facing pairs that form ~2.5 m gates across the lanes, and by two stone lanterns.

This is the retired Rock Garden's *Constrained Footing* mechanic rebuilt from prefabs that exist. It is also better thematically: a gauntlet of training posts in the yard where kids learned is a stronger read than a zen gravel garden, and it costs **zero new art** (§5.1's finding is that the entire Polyworks Asian prop set the old blueprint assumed is absent from this project).

| Prop | Prefab | Count | Positions |
|---|---|---|---|
| Makiwara posts | `pfb_env_target_dummy` | 6 | `(−2.8, 0, 22.5)`, `(−0.6, 0, 22.5)`, `(3.0, 0, 26.5)`, `(5.0, 0, 26.5)`, `(−5.8, 0, 27.5)`, `(8.5, 0, 20.5)` |
| Stone lanterns | `pfb_env_stone_lantern` | 2 | `(−6.5, 0, 19.5)`, `(9.0, 0, 25.0)` |
| Stepping stones | `pfb_env_stepping_stone_tile` | 18 | 6 per lane at 2.0 m spacing, Z 18…28 |
| Leaf-pile mounds (BD-03) | new | 4 | Two live at the Lurker spawns `(−1.8, 0, 19.5)` and `(4.0, 0, 24.0)`; two decoys at `(6.8, 0, 18.5)` and `(−3.5, 0, 28.5)` |

**Leaf-pile mounds carry no collider and are ≤ 0.30 m tall.** This deliberately sidesteps ADR-0005 §6.5 — the trap there is a prop that *carves* NavMesh at bake and is *deactivated later*, leaving a permanent hole, and the ADR names Leaf Pile Lurkers as the case most likely to hit it. A collider-less flat mound never enters the bake, never needs a carving `NavMeshObstacle`, and never needs deactivating: the Lurker rises *through* it. **Do not give the leaf piles colliders.**

Two decoy piles exist so the ambush is a read, not a certainty. Anti-frustration rule: the rise must have a **≥ 0.2 s telegraph frame** (leaves lift before the hitbox exists) and route through ADR-0003's overhead telegraph channel like every other wind-up.

### 3.2 Sub-space 1B — The Koi Pond & Engawa (Z 30.0 … 39.0)

This is the zone's emotional centre, and per CANON *"the dojo is at its most finished here."*

| Element | Extent | Notes |
|---|---|---|
| **Shed** (`pfb_env_bld_shedwithcrate`) | X −7.0…−4.0, Z 30.0…38.0, height ~4.2 m | Long axis **north–south**, along the west wall. **Solid exterior prop with a doorway — no interior.** See §3.3 |
| Shed doorway | East face, centred `(−4.0, 0.35, 34.0)` | Opens east onto the boards; the Skeptic stands framed in it, looking at the pond |
| **Engawa** (veranda boards, BD-02) | X −4.0…−1.0, Z 30.0…38.0, top at **Y +0.35** | 3.0 m wide, 8.0 m long. Step at the south end (Z 29.8…30.0) |
| **Koi pond** (`pfb_env_koi_pond_basin`) | X −0.5…+5.5, Z 30.5…35.5 | 6.0 m × 5.0 m. Water surface at Y −0.35. Basin carves NavMesh |
| Pond–engawa gap | **0.5 m** | The dunk. Knockback off the boards puts you in the water |
| East corridor | X +5.5…+9.5, Z 30.5…39.0 | 4.0 m wide. The route to the gate for a player who skips the boards |
| North apron | X −1.0…+9.5, Z 35.5…39.0 | Connects the corridor and the boards to the gate |

**The engawa is 0.35 m high for a reason.** The baked NavMesh agent type is radius 0.5 / height 2 / **climb 0.75**. A 0.35 m step is below climb height, so the boards bake as connected walkable surface with no ramp, no jump, and no off-mesh link. Do not raise it above 0.75 m and do not lower it below ~0.25 m (below that it stops reading as a veranda). This also satisfies the accessibility rule that no traversal requires precise input.

**The engawa is 3.0 m wide for a reason.** Agent-radius erosion removes 0.5 m from each edge, so a 3.0 m platform bakes to a **2.0 m usable band**. The Crane Duelist "strafes to stay facing" the player (`gdd.md` §3) and needs *some* lateral room; 2.0 m gives it about 1.0 m of strafe, which suits an enemy that "barely shifts on that one leg" while still making the boards feel like a tightrope. **Dependency: the Crane Duelist prefab's `NavMeshAgent.radius` must be ≤ 0.5.** At 0.95–1.0 (SpinCycle's and WagonWheelRoller's values) it cannot path the boards at all. Flag to whoever builds `CraneDuelistAI`; this is not the B114 boss-radius decision (that covers bosses only, and the Crane is not a boss).

**Craftsmanship dressing** (`gdd.md` §11, all CANON, all load-bearing for the Skeptic's line):

| Prop | Position | Why it matters |
|---|---|---|
| Toy boat run aground, sail a scrap of napkin | `(1.2, −0.30, 30.8)` | Pond's south edge |
| **Two pairs of flip-flops, one adult, one child** | `(−3.4, 0.35, 33.2)` and `(−3.4, 0.35, 34.6)` | **The Skeptic's line points at these.** They flank the doorway. Do not move them out of the doorway's frame |
| Wind chime, one tube missing, still turning | `(−1.2, 2.0, 32.0)` | On the engawa's east rail. Keeps turning through the whole beat |
| Coffee mug, dried ring inside | `(−1.3, 0.95, 35.4)` | On the rail |

Additional dressing: `pfb_env_weapon_rack` ×1 at `(−4.1, 0.35, 31.2)` against the shed's east wall (yaw 90); `pfb_env_stone_lantern` ×2 at `(−0.8, 0, 36.6)` and `(6.2, 0, 29.4)`.

### 3.3 How the retired Training Hall's shed survives

ADR-0005 §Consequences records that the Training Hall "does not fit this decision, or the camera" — a roofed interior is unbuildable under ADR-0001's ≥ 6 m overhead-clearance rule, and a single continuous scene cannot give it its own lighting environment.

**Resolution: the shed is a solid exterior prop the player never enters.** Its doorway is a framing device, not a portal. This costs nothing (the room was already out of scope in ROADMAP Phase 3), keeps the CANON stage direction — the Skeptic "standing in the open shed doorway" — exactly as written, and means the shed needs no interior geometry, no interior lighting, and no roof clearance. Its `pfb_env_weapon_rack` dressing moves outside onto the east wall.

**The shed's long axis must run north–south along the west wall.** This is not a style choice — see §4.5. An east–west shed at the north end of zone 1 blocks the sightline that the post-boss engawa callback requires.

### 3.4 Encounter — `RoomData_Backyard_Dojo_Zone1`

| Field | Current (scaffold) | **Spec** |
|---|---|---|
| `roomName` | `Garden Gauntlet` | unchanged |
| `maxConcurrentEnemies` | `2` | **`4`** |
| `bossOwnedWin` | `false` | unchanged |
| `spawnPoints` | `[]` | **7 entries below** |

| # | `enemyPrefab` | `position` | `facingY` | `spawnCount` | Reads as |
|---|---|---|---|---|---|
| 0 | `pfb_enemy_gnome_grunt` | `(−4.5, 0, 21.0)` | 180 | 1 | West lane |
| 1 | `pfb_enemy_gnome_grunt` | `(6.5, 0, 21.0)` | 180 | 1 | East lane |
| 2 | `pfb_leaf_pile_lurker` | `(−1.8, 0, 19.5)` | 180 | 1 | Rises out of the gravel border, *behind* the lane the player took |
| 3 | `pfb_enemy_gnome_grunt` | `(1.0, 0, 25.5)` | 180 | 1 | Centre lane, deeper |
| 4 | `pfb_leaf_pile_lurker` | `(4.0, 0, 24.0)` | 180 | 1 | Second Lurker — breaks spacing mid-advance |
| 5 | `pfb_enemy_gnome_grunt` | `(7.5, 0, 32.0)` | 200 | 1 | The east corridor, past the pond |
| 6 | `pfb_crane_duelist` | `(−2.5, 0.35, 36.0)` | 180 | 1 | **The closer.** On the boards' north end, holding the lane |

**Enemy-mixing compliance** (`gdd.md` §8, ADR-0005 §3):

- Peak live enemies: **4** ✓ (`maxConcurrentEnemies = 4`)
- Concurrent gnome pack: max reachable **3** ✓ (≤ 4)
- Concurrent risen Leaf Lurkers: max reachable **2** ✓ (exactly 2 in the roster)
- Concurrent Crane Duelists: max reachable **1** ✓ (exactly 1 in the roster)
- Seed (indices 0–3) = gnome, gnome, Lurker, gnome — a legal opening

| Check | Value | Constraint |
|---|---|---|
| Whole-roster enclosing circle | centre `(1.0, 0, 27.75)`, **r = 8.96 m** | ≤ 9 m — **this is the tightest number in the spec** |
| Seed-wave (0–3) enclosing circle | centre `(1.0, 0, 22.5)`, **r = 5.70 m** | ≤ 8.4 m (W/2) ✓ |
| Minimum pairwise spawn clearance | **3.09 m** | ≥ 1.5 m ✓ |

**The Crane's position at Z = 36.0 is load-bearing.** The whole-roster circle is 8.96 m against a 9.0 m budget; moving the Crane 0.5 m further north breaks TDD §6.4. If the Crane must move, the *southern* spawns (0, 1, 2) move north with it.

**Why the roster spans 16.5 m of depth without breaking on-screen readability:** array order stages the zone as two beats. Indices 0–3 are the gravel-lane fight (all within a 5.70 m circle); indices 5–6 are the pond/boards fight. The player advances between them. The 8.96 m figure is the *whole roster*, a state that never occurs — but it is the number TDD §6.4 asks for and it is inside budget.

**The Crane does not chase** (`docs/story/enemies/crane-duelist.md`: *"It does not chase. Chasing is for things that haven't decided what they are."*). It spawns on the boards and waits. That means the player **must** come to the engawa to clear zone 1 — which is exactly what guarantees the Skeptic beat fires (§3.5). The story behaviour and the level's beat delivery are the same mechanism; keep them coupled.

### 3.5 The Skeptic beat — spatial and trigger spec

CANON (`docs/story/zones/backyard-dojo.md`): placement is *"after the room's fight settles, as the player crosses the engawa boards toward the shed."* The chair then stays flat on the boards, *"and Kid has to step around it, and keeps half-looking at it the rest of the room"* — so **the beat fires mid-zone, before the zone clears, and combat continues afterwards.** It must not be moved behind the shop screen.

| Element | Spec |
|---|---|
| Trigger volume | `BackyardDojoBeats`, box at `(−2.5, 1.0, 30.4)`, size `(3.0, 3, 1.6)` — the engawa's south step. Fires **once** |
| Gate condition | Player enters the volume **and** zone-1 live enemy count **≤ 1**. If the player is standing in the volume when the count drops to ≤ 1, fire then |
| Why ≤ 1 works | Only the Crane remains at that point (it never advanced). The beat lands as the player steps onto the boards to fight it — exactly as written |
| Skeptic instance | Pre-placed inactive at `(−4.0, 0.35, 34.0)` (the doorway), yaw 90 (facing east, at the pond). **No collider, no `NavMeshAgent`, no `NavMeshObstacle`** — spawned after the bake, contributes nothing to navigation |
| Motion | Scripted 2-node move only: doorway → set chair → doorway. Not pathfound |
| Chair placement | `(−2.2, 0.35, 34.0)` — 1.8 m east of the doorway, flat on the boards, in the player's crossing line |
| Chair collider | **Yes**, plus a **carving `NavMeshObstacle`**. This is the *inverse* of ADR-0005 §6.5's case: §6.5 is about props removed *after* the bake leaving holes; the chair is *added* after the bake, which is precisely what runtime carving is for. It needs **no** `NavMeshModifier` because it was never in the bake |
| Chair material | `MAT_unimaginative_grey` — a **dedicated** material that must be excluded from the Imagination Restore colour ramp. The chair does not come back |
| Departure | Skeptic deactivates when the player is within 3.0 m of the doorway, or 6.0 s after the line finishes, whichever is first — *"The doorway's empty by the time Kid crosses to it"* |
| Non-blocking | **No input lock, no camera takeover.** The Crane may still be live. The beat plays over gameplay |
| Local VFX | Petal-fall mask over the chair's footprint (`petals stop landing on that one spot`); wind chime keeps turning; pond reflection drops for ~1 s then returns |

**Accessibility note.** The Skeptic beat's meaning is carried by *greyness*, which would be a colour-only channel. The CANON staging already supplies three redundant channels and they are not optional dressing: **motion** (petals visibly stop landing on the chair's footprint), **audio** (the chime's tone going dead, then the silence after the line), and **silhouette** (a folded lawn chair is unlike any other shape in the yard). Implement all three. Do not reduce the beat to "the object turns grey."

### 3.6 Loot — zone-1 entries

| Entry | Position | Purpose | Clearance |
|---|---|---|---|
| `workbenchPositions[1]` (forge) | `(2.0, 0, 28.5)` | Pond's south apron — safe, before the boards | 4.03 m to nearest loot; 4.92 m to spawn 4 |
| `cardboardPiles[1]` | `(−2.0, 0, 28.0)` | Gravel, just south of the engawa step | 4.03 m |
| `scatteredObjects[2]` | `(−2.5, 0.35, 32.0)` | **On the boards** — bait, and it puts the player on the engawa | 2.02 m to the chair; 4.0 m to the Crane spawn |
| `scatteredObjects[3]` | `(7.8, 0, 35.5)` | East corridor — the corridor route also pays | 3.51 m to spawn 5 |

`LevelBuilder` spawns the whole table at `Start()`, so all of this exists from frame 0 and is simply gated off by the closed gates. Leave `envProps` **empty** — the yard is hand-dressed under `[ENV - Static]` per ADR-0005 §6.6.

### 3.7 Water hazard — OPEN, do not decide

`gdd.md` §12 Q3 is still open: *koi pond water — brief slow only, or also a small damage tick?* This spec assumes **slow only** (a trigger volume applying a `moveSpeed` multiplier, no damage, no death) because that is what `gdd.md` §2 Room C states as designed, but **the open question is the owner's and is not resolved here.**

Regardless of that answer, the pond's edge must be readable **without relying on colour** (accessibility): a raised stone coping ~0.25 m around the basin rim, an animated ripple surface, and a distinct water-entry audio cue. "It looks blue" is not a hazard read.

---

## 4. Zone 2 — The Garden End: Blossom Court

**Footprint:** a 16-sided ring, walkable radius **8.5 m**, centre **(0, 0, 47.5)** → X −8.5…+8.5, Z 39.0…56.0.
**Role:** the Grasscutter. `bossOwnedWin: true`, no reward screen, no regular enemies.

### 4.1 Correction of record: 36 m → 17.0 m

The old blueprint's *"~36 m circular sparring court"* predates ADR-0001 and violates TDD §6.4's ≤ 9 m combat radius by a factor of two — over half that fight would happen off screen (ADR-0001 §Consequences: `_arenaBoundaryRadius = 18f` is explicitly called out as incompatible).

**This arena is 17.0 m across**, matching the camera's **16.8 m** visible lateral width (ADR-0001) and ADR-0005 §3/§4's `r ≥ 8.5 m`, which is itself calibrated to World 1's measured, playtested **8.44 m** SpinCycle circle. **The arena must not grow.** If `GrasscutterAI` cannot be authored inside §4.4's envelope, the correct escalation is to change the AI or the boss's move set — not the arena, because at 17.0 m across it is already at the camera's limit.

### 4.2 Geometry

| Element | Spec |
|---|---|
| Stockade ring | Regular **16-gon**, inner face (apothem) at **r = 8.5 m** from `(0,0,47.5)`. Side length **3.38 m**. Height 2.4 m, band 0.3 m, layer `Building` |
| South opening | The two segments straddling due south are omitted → a **6.76 m** gap centred on X = 0 at Z = 39.0. `ZONE2_BOUNDARY_WIDTH = 8.8` covers it with ~1 m overlap each side |
| Wall modules | 14 BD-01 modules, X-scaled 0.845 (4.0 m nominal → 3.38 m). **X-scale only** |
| Floor | Jade grass material — the only zone whose floor is grass, not gravel. This is the "living" zone |
| Cherry tree | `pfb_env_cherry_blossom_tree` at **(0, 0, 47.5)**. See §4.3 for its authored constraints |
| Tall grass (boss dormancy) | BD-04 cards ×6, X −3.0…+3.0, Z 53.0…56.0. **Collider-less** — no NavMesh carve, nothing to deactivate |

**No raised mound, no root collar, no stone base under the tree.** Dressing at the tree's foot is a petal/moss *texture* on the ground material only, r ≤ 2.0 m, with no geometry and no collider. This is a deliberate trade of dressing for fight floor — see §4.6.

### 4.3 The cherry tree's authored constraints

The tree is simultaneously: the CANON World Tree breadcrumb (*"younger than everything around it. Too young to be this tall"*), the zone's Imagination Restore trigger, Phase 1's only cover, and — if authored carelessly — the thing that occludes the entire boss fight.

| Constraint | Value | Derivation |
|---|---|---|
| Trunk collider | Capsule, **radius ≤ 0.35 m**, height 4.0 m, layer `Building` | §4.6 — the clear-circle budget |
| Canopy underside | **≥ 4.0 m** | Derived rule A: the camera ray behind a player at 4.0 m is at height `1.0 + 0.7265 × 4.0 = 3.91 m`. A canopy starting at 4.0 m never crosses the camera ray for any player standing ≥ 4.0 m from the trunk — i.e. across 87% of the fight ring |
| Canopy radius | **≤ 3.5 m** | Keeps the boss's intro walk target (§4.4) outside the canopy footprint |
| Total height | **≤ 7.0 m** | At the rim (r = 8.5) the occlusion threshold is `1.0 + 0.7265 × 8.5 = 7.18 m`. A ≤ 7.0 m tree never occludes a player standing at the rim |
| Canopy renderer | **No collider. Not on the `Building` layer.** Routed to the project's occlusion-fader path | The canopy is the one genuine residual occluder in the yard, for players inside r < 4.0 m |

**Flagged dependency:** ADR-0001 §Consequences records that the project has **two** occlusion systems, both mistuned (`Systems/CameraOcclusion.cs` rejects occluders by bounds-centre distance; `Systems/BuildingOcclusionFader.cs` casts a single ray at the player's feet), with the instruction *"Pick one system and delete the other."* That has not happened. The cherry tree canopy is the first case in this project that genuinely needs a working fader. **This is a prerequisite for zone 2, not follow-up work.**

The tree's silhouette should be authored to match the canon: a slender, high-crowned young tree — a thin trunk with the crown lifted clear. The 0.35 m trunk and 4.0 m canopy underside are not compromises against the story; they are the story.

### 4.4 The Grasscutter's arena contract (ADR-0005 §4)

`GrasscutterAI` does not exist. ADR-0005 inverted World 1's order deliberately: the arena is budgeted first and the AI is authored **to** it. These are the numbers, restated exactly, plus the two layout-derived constraints this spec adds.

| Constraint | Value | Source |
|---|---|---|
| Arena minimum clear circle | **r ≥ 8.5 m** | ADR-0005 §3/§4 — see §4.6 for the amendment this spec requests |
| Phase-2 Spin-Dash travel | **≤ 8 m** | ADR-0005 §4. Must start and end inside the arena. (SpinCycle's charge is 4.8 m) |
| Phase-1 AoE / Petal Toss reach | **≤ 4 m** | ADR-0005 §4. (SpinCycle's `fullSpinRadius` is 3 m) |
| Dash landing point | **`NavMesh.SamplePosition`-clamped before the move commits** | ADR-0005 §4 — **not optional.** `SpinCycleAI`'s `JumpBack`/`SpinCharge`/`JumpCharge` move by raw `transform.position` with no bounds check and can put the boss ~3 m inside a building. Shipping that hole twice is not acceptable |
| Cut-Grass Trail hazards | Pooled, **zero per-frame allocation** | ADR-0005 §4, TDD §3.2 |
| `NavMeshAgent.radius` | **1** (matching SpinCycle) | ADR-0005 §OQ3, resolved by the owner 2026-08-31; BACKLOG B114. The physical `Collider` carries the bulk; there is no second baked agent type |
| **Dash lanes are chords, never diameters** *(added by this spec)* | Perpendicular distance from arena centre **≥ 2.5 m** | The tree trunk is at the centre. A 2.5 m offset gives a chord of `2 × √(8.5² − 2.5²) = 16.24 m`, so an 8 m dash from either rim end terminates ~0.1 m short of the chord midpoint — comfortably inside. This also keeps the boss's 1 m agent radius ≥ 1.15 m clear of the 0.35 m trunk |
| **Dash telegraph is a ground-plane lane, not a body pose** *(added by this spec)* | Full-chord ground indicator drawn before the dash commits | Anti-frustration. In a 17.0 m arena against 16.8 m of visible width, a boss at the far rim is at the *frame edge* when the player is at the near rim. A wind-up read from the boss's body is unfair at that separation; a ground lane is readable from anywhere. The minimap is disabled in boss arenas (World 1 precedent), so this is the only channel available. Route it through ADR-0003 |

### 4.5 Boss placement and the victory beat

| Element | Position | Note |
|---|---|---|
| `pfb_grasscutter`, pre-placed **inactive** | `(0, 0, 54.5)`, yaw 180 | r = 7.0 from centre, dormant in the tall grass at the far end. Pre-place at the point the intro teleports to, so the Editor view matches runtime (World 1 precedent) |
| Intro walk target | `(0, 0, 51.5)` | r = 4.0 from centre — just outside the 3.5 m canopy radius, 3.0 m south of dormancy |
| `_imaginationVolume` | scene `ImaginationRestore_Volume` | Already present in the scaffold. Without it `DefeatSequence` skips the imagination ramp and calls `TriggerWin()` directly |

**The post-boss engawa callback — the sightline that ADR-0005 was decided on.** CANON:

> *I look back at the engawa. The chair is still there.*
> **KID:** *"Somebody planted you."*

ADR-0005 Fact 5 names this as the argument it found hardest to answer any other way: under scene-per-room the pond is unloaded and the beat degrades to a voice line about an invisible object. In one continuous scene it must literally work. **This spec verifies it geometrically:**

- Chair at `(−2.2, 0.85, 34.0)` (top surface), arena centre at `(0, 0, 47.5)` → **13.7 m** apart.
- Ray from arena centre to the chair: at Z = 39.0 (the zone boundary) it is at X = **−1.39**, inside the **−3.38 … +3.38** stockade opening ✓.
- At Z = 38.0 (the shed's north face) it is at X = **−1.55**; the shed spans X −7.0…−4.0 ✓ clear.
- **This is why §3.3 requires the shed's long axis to run north–south.** An east–west shed at zone 1's north end sits directly on this ray and the beat becomes impossible. Do not rotate the shed.

**Victory-beat camera keys** (two shots, in CANON order):

| Shot | Camera | Look target | Frames |
|---|---|---|---|
| A — the bloom | `(0, 3.0, 40.5)` | `(0, 5.0, 47.5)` | Looking north and up at the canopy going off all at once |
| B — the callback | `(−0.5, 3.6, 43.0)` | `(−2.2, 0.85, 34.0)` | 9.2 m, ~17° pitch — the chair, the boards, and the empty doorway behind it |

Shot B must be authored and verified against the real chair instance, not assumed. It is ADR-0005 §Validation 6.

### 4.6 Finding: §3's "clear circle r ≥ 8.5 m" and §4's "cherry tree at centre" are literally incompatible

This is the hardest constraint in the brief and it does not resolve cleanly. Stating it plainly rather than fudging a number.

ADR-0005 §3 requires a **minimum clear circle of r ≥ 8.5 m**, measured the way World 1 measured its 8.44 m: the *largest inscribed obstacle-free circle*, computed against visual mesh footprints (ADR-0004 §2). ADR-0005 §4 simultaneously specifies the cherry tree **at the arena's centre**, and the story canon requires it there (*"The cherry tree at the center of the court is younger than everything around it"*).

For an arena of radius `R` with a central obstacle of radius `r_t`, the largest obstacle-free inscribed circle is `(R − r_t) / 2`. With `R = 8.5` and a 0.35 m trunk, that is **4.08 m** — less than half the required 8.5 m. To satisfy the metric *literally*, `R` would have to be `2 × 8.5 + r_t = 17.35 m`, i.e. **a 34.7 m arena** — which is the 36 m number ADR-0005 §4 explicitly corrected, and which fails the camera by 2×.

**The two constraints cannot both hold. One of them must be restated, and it should not be the arena size** — 8.5 m is derived from the camera's 16.8 m visible width and is the harder physical limit.

**Recommended amendment (needs `technical-director` sign-off; this spec does not have the authority to change an accepted ADR's acceptance criterion):**

> ADR-0005 §3, boss-arena row, restated: **"Arena outer walkable radius ≥ 8.5 m about the arena centre, with no interior obstruction exceeding 0.8 m in diameter."**

Under that reading the Blossom Court passes: outer walkable radius exactly 8.5 m, sole interior obstruction a 0.70 m trunk, giving a **continuous fight ring 8.15 m wide** (r 0.35 → 8.5) with **~31 m of circumference at mid-radius** — ample tangential escape from a 4 m Phase-1 AoE, which is the space the constraint exists to protect.

**Alternative if the amendment is refused:** move the tree to the arena's north rim at `(0, 0, 54.0)`, restoring a solid 8.5 m clear disc and satisfying §3 as literally written. Costs: Phase 1 loses its only cover ("the player is safe circling the tree at mid-radius" — old blueprint §Phase 1 Safe Zones), the orbital movement grammar in §1.3 collapses toward zone 0's, the tall-grass dormancy spot and the tree collide spatially, and the tree-at-centre framing that carries the World Tree breadcrumb is weakened. **Recommend the amendment, not the move** — but this is the owner's and technical-director's call, not this spec's.

**Also note:** the trunk *will* produce a small `ValidateCameraClearance` violation cluster in the 8 m band immediately north of `(0,0,47.5)`. That is expected, is a diagnostic only, and is the correct trade — a 0.70 m pole in a 17 m court occludes a sliver of frame, unlike World 1's water trough and stacked crates.

### 4.7 The court clears for the duel — `ZoneDirector._clearOnBossZone`

ADR-0005 §5 keeps `ZoneDirector` (the generalized `WildWestCityZoneDirector`) as a reusable component; World 1 used `_clearOnBossZone` to remove two covered wagons and open SpinCycle's fight space. World 2's equivalent is better than a workaround — it is the zone's thesis:

Pre-fight, the Blossom Court is dressed as a **sparring court**: four makiwara posts and two stone lanterns standing in a ring. On zone-2 activation they fold away and the court becomes open ground. *This is where you learn it* — and then the lesson ends and the machine arrives.

| Prop | Prefab | Position | Polar (r, from centre) |
|---|---|---|---|
| Makiwara A | `pfb_env_target_dummy` | `(−3.4, 0, 53.4)` | 6.8 m |
| Makiwara B | `pfb_env_target_dummy` | `(3.4, 0, 53.4)` | 6.8 m |
| Makiwara C | `pfb_env_target_dummy` | `(−5.9, 0, 44.1)` | 6.8 m |
| Makiwara D | `pfb_env_target_dummy` | `(5.9, 0, 44.1)` | 6.8 m |
| Lantern E | `pfb_env_stone_lantern` | `(−6.8, 0, 47.5)` | 6.8 m |
| Lantern F | `pfb_env_stone_lantern` | `(6.8, 0, 47.5)` | 6.8 m |

All six are grouped under `[ENV - Static]/Zone2_CourtDressing` and wired to `ZoneDirector._clearOnBossZone`.

**All six are deactivated at runtime, so all six need ADR-0005 §6.5's treatment: a carving `NavMeshObstacle` plus a `NavMeshModifier` with `ignoreFromBuild`.** `LevelBuilder.BuildNavMeshDeferred` calls `NavMesh.RemoveAllNavMeshData()` and re-bakes from physics colliders at `Start()`; a prop deactivated later without this pairing leaves a permanent hole in the mesh. This is the single most likely §6 lesson to be forgotten — six props, six pairs of components, no exceptions.

### 4.8 Loot — zone-2 entries

ADR-0005 §6.6: zone-2 loot must be ≥ 1.5 m clear **with the pre-boss props still present**, because `LevelBuilder` spawns the whole table at `Start()`.

| Entry | Position | Clearance with court dressing present |
|---|---|---|
| `scatteredObjects[4]` | `(−5.0, 0, 41.5)` | 2.75 m to Makiwara C; 6.26 m to Lantern E; 7.81 m to the trunk |
| `cardboardPiles[2]` | `(5.0, 0, 41.5)` | 2.75 m to Makiwara D |

Both sit at r = 7.81 m from centre (inside 8.5 ✓) and at X = ±5.0, outside `RoomTrigger_Zone2`'s X ±4.4 volume. **No workbench in zone 2** — the boss owns the win, there is no reward screen, and World 1's zone 2 had none either.

### 4.9 Weapon pool — OPEN, not decided here

`WeaponDropTableSO_Backyard_Dojo` exists and is completely empty. The dojo-native pool per `gdd.md` §6 is **Bo Staff, Katana, Shurikens, Water Whip**, all four already design-complete and in-project except one.

**ADR-0005 §OQ5 and `gdd.md` §12 Q4 are still open: does Water Whip ship with World 2, or is it dropped from the dojo pool until its model and icon land?** This spec **does not decide that.** Author the table with Bo Staff / Katana / Shurikens and leave a clearly-labelled fourth slot. The four `scatteredObjects` positions above are pool-agnostic — the geometry does not change either way. Owner call.

---

## 5. Performance — the budgets this layout is built against

ADR-0005 §3 made the whole-scene budgets **the condition the single-scene decision was granted on.** They are restated with exact figures and then answered.

| Budget | Target | Basis | This layout |
|---|---|---|---|
| Draw calls, whole yard, peak | **< 100** | TDD §3.2. World 1 measured **205** | Plausible but **not guaranteed by layout** — see §5.3 |
| Triangles, whole yard, peak | **< 300k** | TDD §3.2. World 1 measured **356.7k** | Est. **~118k** — see §5.2 |
| Texture memory, steady state | **< 150 MB** | TDD §3.3. World 1 measured **41.2 MB** | Comfortable; the kit is 8 existing props + 7 new meshes |
| Distinct ENV materials | **≤ 20** | ADR-0005 §3 (new) | **13 allocated, 7 reserved** — see §5.4 |
| New (non-atlas) ENV geometry | **< 8k tris** | ADR-0005 §3 | Est. **~4,550** — see §5.5 |
| Any new ENV texture | **≤ 512** on Android + iOS, explicit platform overrides | TDD §3.4 | **Prerequisite** — see §5.6 |
| Scene-start hitch incl. NavMesh bake | **≤ 500 ms** | ADR-0004 §8 | One bake per run instead of three-to-five |
| Live enemies, peak | **≤ 4** | `gdd.md` §8 | ✓ §2.2, §3.4 |
| Combat radius, per zone | **≤ 9 m** | TDD §6.4 | 7.00 / 8.96 / 8.5 — all inside, zone 1 by 0.04 m |
| Boss arena clear circle | **r ≥ 8.5 m** | ADR-0005 §3 | **Conflict — see §4.6** |
| Thermal | No sustained frame-time regression, min 1 vs min 12 | TDD §3.1 | The real acceptance criterion, and the one a podcast audience will see |

### 5.1 Finding: the Polyworks Asian prop set the ADR's draw-call argument depends on is not in this project

ADR-0005 Fact 3 is the load-bearing performance argument for the whole decision:

> *"The dojo set is one building (the shed), one cherry tree, and a repeated kit — stone lanterns, stepping stones, tatami, gravel, zen rocks, paper lanterns, bamboo wall — **drawn largely from the shared Polyworks Asian atlas**, i.e. many instances of few materials, which is the case GPU instancing and the SRP Batcher exist to serve."*

**Verified in the project today: none of those Polyworks assets exist.** Zero files match `Asian_Prop*`, `*tatami*`, `*zen*`, `*sand*`, `*fountain*`, or `*paper*`. There is no zen-garden sand, no tatami, no paper lantern, no bamboo fountain, and no rock set.

What *does* exist and is fully wired: `pfb_env_cherry_blossom_tree`, `pfb_env_stone_lantern`, `pfb_env_torii_gate`, `pfb_env_stepping_stone_tile`, `pfb_env_target_dummy`, `pfb_env_weapon_rack`, `pfb_env_bld_shedwithcrate`, and `pfb_env_koi_pond_basin` — **eight** props.

**Consequences, all of which this spec already absorbs:**

1. **The shared-atlas premise is unavailable.** The draw-call argument must be re-earned by consolidating the eight existing prefabs' materials down to §5.4's allocation, not by an atlas that was assumed to be present. That consolidation is real work and it is the actual draw-call lever.
2. **Raked gravel and the stone path become ground *materials*, not props** (§2.3). This is strictly cheaper and is a genuine improvement over the old blueprint.
3. **Zone 1A's cover is makiwara posts, not zen rocks** (§3.1). Better thematically and costs nothing.
4. **No tatami, no paper lanterns, no bamboo fountain** anywhere in this spec.
5. **`street_pond_a` does not exist either** — the old blueprint's `⚠️ check-first` dependency. **`pfb_env_koi_pond_basin` does exist** (model `Models/ENV/Koi_pond_basin/Koi_pond_basin.fbx`). ADR-0005's "possibly the pond basin are new" is **resolved: not new.**
6. **Installing the Polyworks Asian package is an owner decision, not an implementer's.** `studio-core.md`: *"Do not install a new third-party Unity/Asset Store package without explicit owner approval."* Flagged, not assumed.

**Second, smaller asset flag:** unused v2 model generations exist — `Models/Props/prop_cherryblossom_v2`, `prop_steppingstone_v2`, `prop_targetdummy_v2`, with matching `Materials/Props/MAT_prop_*_v2`. Three of the four prop families this spec uses most heavily have two generations in the project and no prefab points at the newer one. **Which generation World 2 builds against directly determines the material count in §5.4** and is an `asset-engineer` / `art-director` question. Do not mix generations.

### 5.2 Triangle estimate

The good news, and it is the payoff of the leaner kit: **the dojo has no analogue of World 1's ten unique Meshy buildings**, each with its own 27–31 MB BaseColor and its own material. ADR-0005 Fact 3's hypothesis holds on the triangle axis even though the atlas premise (§5.1) does not.

| Group | Instances | Est. tris each | Est. total |
|---|---|---|---|
| BD-01 stockade modules | ~40 | 900 | 36,000 |
| Cherry tree | 1 | 4,000 | 4,000 |
| Shed | 1 | 2,500 | 2,500 |
| Stone lanterns | 8 | 700 | 5,600 |
| Makiwara posts | 12 | 400 | 4,800 |
| Stepping stones | ~40 | 120 | 4,800 |
| Engawa modules (BD-02) | 6 | 300 | 1,800 |
| Weapon racks | 2 | 900 | 1,800 |
| Torii | 1 | 800 | 800 |
| Pond basin | 1 | 1,200 | 1,200 |
| Tall-grass cards (BD-04) | 6 | 200 | 1,200 |
| Leaf-pile mounds (BD-03) | 4 | 250 | 1,000 |
| Craftsmanship props | 8 | 200 | 1,600 |
| Ground | 1 | 2 | 2 |
| **ENV subtotal** | | | **~67,100** |
| Player + 4 enemies + boss | | | ~40,000 |
| HUD, VFX, pooled hazards | | | ~10,000 |
| **Total, peak** | | | **~117,000** |

Against **< 300k**, with World 1 measured at 356.7k. **Caveat: the per-instance figures for the eight existing prefabs are estimates, not measurements.** Measure them before trusting this table — that is exactly the mistake ADR-0004 §8 made about texture memory and ADR-0005 Fact 3 had to correct.

### 5.3 Draw calls — the one budget layout cannot guarantee

This is the honest gap. Layout can hold the material count to 13 and make every repeated prop instanceable, but it cannot make batching *engage*.

ADR-0005 §3 and BACKLOG **B112** record that World 1's Draw Calls Breakdown measured `Standard: 204, SRP Batcher: 0, BRG: 0, Standard Instanced: 0`. The SRP Batcher contributed **nothing**, not merely failed to reduce the count, and nobody yet knows why (shader/material variant incompatibility is the leading candidate). If the cause is project-wide it will hit World 2 identically.

**Therefore: the B112 SRP-Batcher-at-zero investigation is a prerequisite of this spec's draw-call budget, not follow-up work.** It is a `performance-engineer` task and it should land before the yard is dressed, not after.

What this layout does to help, all of which must actually be authored:

- **13 distinct ENV materials** (§5.4), consolidated across the eight existing prefabs.
- **`Enable GPU Instancing` on every repeated-prop material** — stockade, stepping stones, lanterns, makiwara, engawa, grass cards.
- **All static ENV under `[ENV - Static]`, marked Static**, so static batching has a subroot to work with (the `StaticBatchingUtility.Combine` question is open in B112).
- **Zero per-instance material tweaks.** One instance with a modified material breaks the whole batch. This is the single easiest way to lose the budget while dressing.
- **Non-uniform X-scale on wall modules is fine** (same mesh, same material — instancing is unaffected). **Y-scale is forbidden**, because wall height is load-bearing in Derived rule A.

### 5.4 ENV material allocation — 13 of 20

| # | Material | Consumers |
|---|---|---|
| 1 | `MAT_ground_gravel` | Zone 0 floor, zone 1A floor (raked gravel + baked stone-path markings) |
| 2 | `MAT_ground_grass` | Zone 2 court floor, zone-1 borders |
| 3 | `MAT_prop_stone` | Stepping stones, stone lanterns, pond coping |
| 4 | `MAT_prop_wood_weathered` | Shed, engawa boards, makiwara, weapon racks |
| 5 | `MAT_prop_lacquer_red` | Torii |
| 6 | `MAT_env_bamboo` | BD-01 stockade, all ~40 modules |
| 7 | `MAT_tree_bark` | Cherry-tree trunk |
| 8 | `MAT_tree_blossom` | Cherry-tree canopy (alpha-clipped) |
| 9 | `MAT_water_pond` | Koi pond surface |
| 10 | `MAT_grass_card` | Tall grass (BD-04) + leaf-pile mounds (BD-03), alpha-clipped |
| 11 | `MAT_props_craftsmanship` | All 8 craftsmanship micro-props, one atlas |
| 12 | `MAT_unimaginative_grey` | **The lawn chair only.** Must be excluded from the Imagination Restore colour ramp |
| 13 | `MAT_petals_vfx` | Drifting-petal particles + the pond ripple |

**7 slots reserved** against ADR-0005 §3's `≤ 20`. Spend them deliberately.

**No decals are available.** `Mobile_Renderer.asset` has `m_RendererFeatures: []` and ADR-0003 rejected URP decal projectors outright (they require a mobile depth prepass). Every "decal" in the old blueprint — the chalk hopscotch grid, the raked-gravel rake lines, the path markings — must be either **baked into a ground texture** or authored as a **thin geometry quad**. §2.3 specifies the hopscotch as a quad on material 11; the rake lines belong in material 1's texture.

**No second cherry tree anywhere in the yard.** Canon: the tree is *"the only thing in the yard nobody could have raked, mowed, or bagged into submission. It's the one living thing."* Zone 1's petal fall (CANON: *"The petals keep falling — except on the one grey spot"*) is drifting-petal VFX carried south on the wind from the single tree at the far end. This is a small dividend of the single-scene decision: the yard's one living thing genuinely reaches every zone, and its trunk and the pink drift on the ground are visible from zone 1's north end at ~11 m (inside F = 15.3 m), even though its crown is above frame per Derived rule B.

### 5.5 New geometry — ~4,550 of 8,000 tris

ADR-0005 §3 budgets **< 8k tris** of new non-atlas ENV geometry for the whole zone, noting *"only the bamboo wall (BD-01) and possibly the pond basin are new."* The pond basin turns out to exist (§5.1), but §5.1's other findings add several small meshes.

| Asset | Description | Budget (unique tris) |
|---|---|---|
| **BD-01** | Bamboo stockade module, 4.0 × 2.4 × 0.3 m, tileable, X-scalable | ≤ 900 |
| **BD-02** | Engawa board module, 3.0 × 4.0 × 0.35 m | ≤ 300 |
| **BD-03** | Leaf-pile mound, flat, ≤ 0.30 m, **collider-less** | ≤ 250 |
| **BD-04** | Tall-grass card cluster, **collider-less** | ≤ 200 |
| **BD-05** | Pond coping ring (only if not part of `pfb_env_koi_pond_basin` — check first) | ≤ 900 |
| **BD-06** | Folded lawn chair, flat, ~0.12 m tall | ≤ 400 |
| **BD-07** | Craftsmanship micro-props ×8, one atlas: practice sword, hopscotch quad, garden glove, birdhouse, toy boat, two flip-flop pairs, wind chime, coffee mug | ≤ 1,600 |
| | **Total** | **≤ 4,550** |

**3,450 tris of headroom** against the 8k budget. Do not spend it on the stockade — a 900-tri wall module instanced 40 times is 36k triangles, which is a third of the whole scene's estimated cost.

### 5.6 Texture import policy is a prerequisite, not follow-up

ADR-0005 §3: *"Any new ENV texture ≤ 512 on Android + iOS, with explicit platform overrides. **The import-policy pass is a prerequisite for new dojo art, not follow-up work.**"*

Commit `94ad911b` added a texture-import-policy `AssetPostprocessor` **disabled by default**. It must be enabled and verified before BD-01 … BD-07 art lands, or the seven new meshes arrive with unconstrained textures and §3's texture-memory headroom (World 1 measured 41.2 MB of a 150 MB budget) gets spent by accident.

### 5.7 Lighting — one environment for the whole yard

One scene means one lighting environment (ADR-0005 §Consequences). `gdd.md` §1's cool overcast key is uniform, so this costs nothing today — the interior/exterior contrast the retired Training Hall wanted is simply not available, and §3.3 removes the need for it.

| Setting | Spec | Scaffold today |
|---|---|---|
| Directional key | Pale jade-white `#DCE6DA`, intensity ~1.0, high angle (diffuse midday overcast), soft shadows tinted sage-grey `#4A554A` | `Sun` at colour `(0.85, 0.9, 1.0)`, intensity 1 — **bluish, not jade. Retune** |
| Ambient | Cool green-grey fill | — |
| `PostProcess_Volume` | Present in the scaffold (global, priority 1) | ✓ |
| `ImaginationRestore_Volume` | Present in the scaffold (global, priority 0). Colour Adjustments prepped for the Awakening → full-saturation ramp on boss defeat | ✓ |
| Imagination Restore exclusion | `MAT_unimaginative_grey` (the lawn chair) must **not** receive the ramp | — |
| Bake | After ENV dressing is final, not before | — |

---

## 6. Validation — mapped to ADR-0005 §Validation

Nothing below is optional; each item exists because World 1 paid for it.

1. **`ZoneDirector` rename does not regress World 1** (ADR-0005 §5, §Validation 1). `WildWestCityZoneDirector` → `ZoneDirector`, file and class renamed together so the `.cs.meta` GUID survives and `CulDeSac_WildWestCity.unity`'s component reference holds. Full World 1 walkthrough afterwards. **This is a prerequisite for zone 2's `ZoneDirector` instance and is not part of this spec's scope.**
2. **Walk `Backyard_Dojo` start to finish.** Each zone activates once, in order; no zone reachable past a closed gate; re-entering a cleared zone triggers nothing; zone 0's upgrade screen and zone 1's shop screen both appear and return control **without a scene load**, with the player resuming where they stood.
3. **Gate-bypass flood-fill, three-configuration control, with the ground-support raycast** (ADR-0005 §6.3). `Physics.OverlapCapsule` at the player's real `CharacterController` radius on a 0.5 m grid, BFS from `(0,0,0)`, **plus a downward ground-support raycast per cell** — B107 found the flood-fill otherwise routes through off-mesh void and produces false positives. Three configurations: (a) gates closed, stockade colliders disabled → **must** reproduce a bypass; (b) gates closed, stockade enabled → **must not**; (c) gates open → **must**, as a positive control that the test detects connectivity at all. Pay specific attention to the four dead strips outside the stockade (§1.2).
4. **Every spawn point, workbench, pickup, and cardboard pile is on the runtime NavMesh and reachable.** `NavMesh.SamplePosition` for on-mesh, `NavMesh.CalculatePath` returning `PathComplete` from each zone entry, **checked with each gate in its real closed state** — World 1's B100 found two spawns inside a closed gate's carve volume. Note the four engawa positions at Y = 0.35 specifically.
5. **`ValidateCameraClearance` with `_cameraYawDegrees = 0`.** Record NavMesh vertex count and both violation counts, bucketed per zone as B107 did. Expected clusters: the south-wall band in zone 0 (§2.5), the shed's north side in zone 1, and a small cluster north of the tree trunk in zone 2 (§4.6). It is a diagnostic and does not block Play Mode; record it, do not chase it.
6. **The engawa callback works** (ADR-0005 §Validation 6). After the Grasscutter falls, the lawn chair is still on the boards at `(−2.2, 0.35, 34.0)` and is visible from victory-camera shot B (§4.5). Verify against the real instance with `Camera.WorldToViewportPoint`, not by eye from an editor angle — B57's lesson.
7. **Profile a full run on a representative 3–4-year-old device against §5**, both passes (Unity Profiler for counts; non-development build + Instruments for the frame-time and thermal verdict). Record frame time at minute 1 vs minute 12, the scene-start hitch including the NavMesh bake, and **the SRP Batcher / Instanced draw-call split explicitly.** A figure without its scenario is not evidence.
8. **Tall-prop composition check** (ADR-0001 §Consequences B57, not caught by the clearance validator). Verify with `Camera.main.WorldToViewportPoint` at realistic combat positions that the shed's doorway, the torii's crossbeam, and the tree's trunk read in frame where the player will actually be standing when they need to see them.

---

## 7. What survives from the superseded documents

`unity-blueprint.md` is superseded in full; `gdd.md` §2 and §10 are superseded. Nothing of value in them is discarded — this is where each piece went.

| Superseded material | Where it lives now |
|---|---|
| **Room 1 — The Back Gate:** Assembly Beat, gnome pack rhythm, torii entrance, 4 craftsmanship props | **Zone 0** (§2). Enemy count 3 → 5, `maxConcurrentEnemies` → 4 |
| **Room A — The Rock Garden:** Constrained Footing, channelled movement, Leaf Pile Lurkers in the borders | **Zone 1A** (§3.1). Channelling is makiwara posts, not zen rocks (§5.1) |
| **Room B — The Training Hall:** roofed interior, pillars, tatami, the Duel Floor | **Retired as a space** (ADR-0005: a roofed interior is unbuildable at ≥ 6 m overhead clearance). Its shed survives as a solid exterior prop with a doorway (§3.3); its weapon racks move outside; its "tighter space forces you back into the Crane's line" mechanic is replaced by the engawa's 2.0 m usable board run (§3.2) |
| **Room C — The Koi Pond:** narrow walkways, water hazard, engawa, the Skeptic beat, 4 craftsmanship props | **Zone 1B** (§3.2, §3.5). All CANON staging preserved verbatim |
| **Boss Room — The Blossom Court:** central cherry tree, Kata/Rev phases, moving hazard lanes, Cut-Grass Trail | **Zone 2** (§4). Arena corrected 36 m → **17.0 m** (§4.1) |
| **Random draw of three rooms** | **Retired.** ROADMAP Phase 3's three-room scope, owner-resolved 2026-08-31 (ADR-0005 §OQ1). The Skeptic beat is now guaranteed instead of firing in ~1 run of 3 |
| **Scene-per-room run integration** (`s_roomQueue`, `RandomRoomPool`) | **Retired** by ADR-0005 §1/§2. One scene, three in-scene zones |
| **`street_pond_a ⚠️ check-first`** | **Resolved:** does not exist. `pfb_env_koi_pond_basin` does (§5.1) |
| **Polyworks Asian props** (tatami, paper lanterns, bamboo fountain, zen garden sand, zen rocks) | **Not in the project** (§5.1). Not used by this spec. Installing the package is an owner decision |
| **`gdd.md` §1, §3–§9, §11, §12** | **Still live reference.** Tone/palette, Crane Duelist, returning-enemy tuning, Grasscutter phases, weapon pool, ENV prop list, difficulty scaling, lore hook, craftsmanship dressing, open questions |

---

## 8. Open questions — flagged, not resolved

| # | Question | Owner | Blocks |
|---|---|---|---|
| 1 | **§4.6: `r ≥ 8.5 m` clear circle vs the cherry tree at centre.** Amend the metric to "outer walkable radius ≥ 8.5 m, no interior obstruction > 0.8 m diameter" (recommended), or move the tree to the north rim? | `technical-director` + owner | Zone 2 acceptance criteria |
| 2 | **§1.4: the Assembly Beat's CANON imagery** ("the shed's roof lifts", "a tree at the far end") is undeliverable on the gameplay rig. Authorize a 3 s scripted intro camera (recommended, but unbudgeted scope), or cut the imagery from the visible beat? | Owner | Zone 0 intro |
| 3 | **§5.1: the Polyworks Asian prop set is absent.** Install the package (needs explicit owner approval per `studio-core.md`), or ship the 8-prop kit this spec is built on (recommended)? | Owner | Nothing — this spec assumes the 8-prop kit |
| 4 | **§5.1: v1 ENV prefabs vs unused v2 models** (`prop_cherryblossom_v2`, `prop_steppingstone_v2`, `prop_targetdummy_v2`). Which generation does World 2 build against? Do not mix | `asset-engineer` / `art-director` | §5.4's material count |
| 5 | **Water Whip** — ships with World 2, or dropped from the dojo weapon pool until art lands? (ADR-0005 §OQ5, `gdd.md` §12 Q4) | Owner | `WeaponDropTableSO_Backyard_Dojo` contents only; geometry is pool-agnostic |
| 6 | **Koi pond water** — brief `moveSpeed` slow only, or also a small damage tick? (`gdd.md` §12 Q3) | Owner | §3.7 |
| 7 | **Crane Duelist Beak Thrust parry window** — how tight before it is frustrating rather than satisfying? (`gdd.md` §12 Q1) | Playtest | `CraneDuelistAI` tuning |
| 8 | **Grasscutter Phase-2 dash** — fixed telegraphed lanes or player-aimed? (`gdd.md` §12 Q2). **This spec partially pre-empts it:** §4.4 requires chord lanes with a ground-plane telegraph for readability at 17 m. If player-aimed dashes are chosen, the chord constraint and the telegraph still apply | Owner | `GrasscutterAI` |
| 9 | **§1.5: how the 3 s activation delay is implemented** without racing B49's auto-activate of room 0 | `unity-gameplay-engineer` | Zone 0 |
| 10 | **§2.2: zone-0 `maxConcurrentEnemies` 4 vs 3.** 4 is specified because four-at-once is the zone's teaching goal; it is also the world's first encounter | Playtest | Tuning only |
| 11 | **§4.3: the two mistuned occlusion systems** (ADR-0001: *"Pick one system and delete the other"*) are still both present. The cherry-tree canopy is the first case in this project that genuinely needs a working fader | `technical-director` | Zone 2 |
| 12 | **§5.3 / B112: SRP Batcher measured at zero.** Prerequisite for the < 100 draw-call budget, not follow-up | `performance-engineer` | §5's draw-call budget |
| 13 | **§5.6: texture import policy** `AssetPostprocessor` (commit `94ad911b`) is disabled by default and must be enabled before BD-01 … BD-07 art | `asset-engineer` | New dojo art |
| 14 | **The 30 FPS cap** at `GameManager.cs:101-102` contradicts TDD §3.1's 60 FPS target. Affects how §5's thermal criterion is judged (ADR-0005 §OQ4, B112) | Owner | §5 thermal verdict |

---

## 9. Build order — what is buildable today

Three of the four enemy prefabs are **art-only**. `pfb_crane_duelist` has zero scripts; `pfb_grasscutter` has one (`MovementBanking`); `pfb_leaf_pile_lurker` has zero. None has `EnemyStats`, and **`RoomManager` destroys any spawned prefab lacking `EnemyStats`, with an error, to avoid a room-clear deadlock.** No `CraneDuelistAI`, `GrasscutterAI`, or Leaf-Pile-Lurker AI script exists.

Consequence for sequencing:

| Stage | Work | Blocked on |
|---|---|---|
| **A — buildable now** | Ground resize (§1.2); full stockade perimeter (§1.7); both `RoomGate`s and both `RoomTrigger`s (§1.6); all three zones' geometry and dressing from the 8 existing prefabs (§2.3, §3.1, §3.2, §4.2, §4.7); the whole `WeaponDropTableSO` **minus** the Water Whip slot (§4.9); `RoomData_..._Zone0` complete (§2.2); lighting retune (§5.7); flood-fill, camera-clearance, and NavMesh validation (§6.3–§6.5) | Nothing |
| **B — needs the `ZoneDirector` rename** | `ZoneDirector` instance, `_clearOnBossZone` wiring, `_bossZoneIndex = 2` (§4.7) | ADR-0005 §5 rename + World 1 re-verification (§6.1) |
| **C — needs new ENV art** | BD-01 … BD-07 (§5.5), currently placeholdered by primitives | §5.6 texture import policy |
| **D — needs enemy implementation** | `RoomData_..._Zone1`'s spawn array (§3.4) — authoring it before Crane/Lurker have `EnemyStats` produces runtime errors and a zone that cannot clear | `CraneDuelistAI`, Leaf-Pile-Lurker AI, `EnemyStats` on both prefabs |
| **E — needs the boss** | Boss placement, intro, victory cameras (§4.5), hazard pools (§4.4) | `GrasscutterAI`, authored **to** §4.4's contract |
| **F — needs an owner decision** | The zone-0 intro camera (§1.4 / Q2); the Water Whip slot (Q5); the §4.6 amendment (Q1) | Owner |

**Recommendation: stage A is the implementation handoff.** It is roughly 70% of the level, is unblocked, produces a walkable end-to-end yard with a real zone-0 fight, and lets §6's flood-fill / clearance / NavMesh validation run early — which is exactly when World 1's expensive lessons were learned late.

---

*Spec owner: `game-designer`, 2026-09-01. Built against ADR-0005 (Accepted 2026-08-31), ADR-0001, ADR-0003, ADR-0004, TDD §3 and §6.4, and the CANON zone lore in `docs/story/zones/backyard-dojo.md`. Hand to `unity-gameplay-engineer` for stage A.*
