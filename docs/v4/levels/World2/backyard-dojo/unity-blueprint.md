> # ⛔ SUPERSEDED — HISTORICAL ONLY. DO NOT BUILD FROM THIS FILE.
>
> **Superseded 2026-09-01 by [`zone-layout-spec.md`](zone-layout-spec.md)**, which is the live, buildable spec for World 2. Build to that document.
>
> This blueprint predates [ADR-0001](../../../../adr/0001-fixed-low-follow-camera.md) (fixed low camera), [ADR-0004](../../../../adr/0004-world1-single-continuous-scene.md), and [ADR-0005](../../../../adr/0005-world2-single-continuous-scene.md). Three things in it are now wrong in ways that would break the build:
>
> 1. **Scene-per-room is retired.** ADR-0005 §1/§2: World 2 is **one continuous scene**, `Backyard_Dojo.unity`, with three in-scene `RoomManager` zones. The five scene files named below will never exist, and the `s_roomQueue` / `RandomRoomPool` path they rely on is dead code with no valid targets.
> 2. **The 5-room random-draw structure is retired.** Owner-resolved 2026-08-31 (ADR-0005 §OQ1): **exactly three zones.** Rooms A/B/C do not exist as separate rooms. The Rock Garden folds into zone 1A, the Koi Pond becomes zone 1's sub-space, and the Training Hall is retired as a space (a roofed interior is unbuildable under ADR-0001's ≥ 6 m overhead clearance).
> 3. **The "~36 m circular sparring court" is wrong by 2×.** It violates TDD §6.4's ≤ 9 m combat radius; over half that fight would be off screen at this project's camera. The Blossom Court is **17.0 m across** (ADR-0005 §4).
>
> Nothing of value here is discarded — `zone-layout-spec.md` §7 records exactly where each piece of this document went. Kept on disk for that trace, and because its per-room lore, mechanic intents, and craftsmanship dressing were the source material for the live spec.

---

# Unity Blueprint: The Backyard (Dojo) — World 2

**Scene files:**
- Combat rooms: `Backyard_Room1.unity`, `Backyard_RockGarden.unity`, `Backyard_TrainingHall.unity`, `Backyard_KoiPond.unity`
- Boss room: `Backyard_BossCourt.unity`

**Asset delivery path:** `Assets/_Project/Models/ENV/Backyard/`
**Scene structure:** Scene-per-room, loaded via the World 1 run-loop system (static room queue).

---

## Build Dependencies (must exist before scenes are playable)

| Asset | Status | Blocks |
|---|---|---|
| `CraneDuelistAI` + Crane Duelist model/prefab | ❌ New | Rooms B, C |
| `GrasscutterAI` + Grasscutter model/prefab | ❌ New | Boss Court |
| Leaf Pile Lurker model/prefab | ❌ Never built | Rooms A, C |
| `env_backyard_bamboo_wall` | ❌ Meshy (BD-01) | All rooms (boundary) — placeholder: existing fence prefab |
| Koi pond | ⚠️ `street_pond_a` check-first | Room C |
| Water Whip model/icon | ❌ Art pending | Weapon pool (can drop if unbuilt) |

Route enemy/boss models through `/asset-pipeline` + art-direction-agent first. Gnome Soldier (`pfb_enemy_gnome_grunt`) already exists.

---

## Room 1 — The Back Gate

### Layout Overview
A ~34×28m walled courtyard. Player enters through a torii gate at the south; the sparring ground opens ahead with the cherry tree offset to one side and the shed (training hall) closed at the north. Bamboo stockade walls ring the space.

### ASCII Layout (1 cell ≈ 4m)
```
W W W W W W W W W    ← north bamboo wall (shed facade center)
W  .  .  [T] . .  W   ← cherry Tree offset NE
W  .  E  .  .  E  .  W  ← Gnome patrol zones
W  L  .  .  .  .  L  W   ← stone Lanterns flanking
W  .  .  E  .  .  .  W   ← 3rd gnome center
W  s  .  .  .  .  s  W   ← stepping-stone scatter
W  .  .  [WB] .  .  W   ← forge Workbench
W  .  .  .  P  .  .  W   ← Player spawn (torii gate)
W W W W [G] W W W W   ← south wall, torii entrance

P=player spawn  G=torii gate  T=cherry tree  E=gnome spawn
L=stone lantern  s=stepping stones  WB=workbench  W=bamboo wall
```

### Enemy Placement
| Enemy | Count | Spawn | Behavior |
|---|---|---|---|
| Gnome Soldier | 3 | (-6,0,4), (6,0,4), (0,0,8) | Patrol; knock-charge in staggered wave after 3s Assembly Beat |

### Props
| Prop | Placement | Source | Notes |
|---|---|---|---|
| Cherry tree | NE, (7,0,10) | `pfb_env_cherry_blossom_tree` | NavMesh obstacle |
| Stone lanterns | flanks (±9,0,2) | `pfb_env_stone_lantern` | Low cover |
| Torii gate | south spawn (0,0,-13) | `pfb_env_torii_gate` | Entrance frame |
| Bamboo wall | perimeter | BD-01 (placeholder: fence) | Boundary, carve |
| Forge workbench | (0,0,-11) | Backyard workbench | Safe-zone forge |
| Stepping stones | scatter | `pfb_env_stepping_stone_tile` | Decoration |
| Craftsmanship ×4 | perimeter | see GDD §11 | Non-interactive |

### Unity Notes
- Assembly Beat: `EnemySpawner.aggroDelay = 3f`.
- RoomManager: `roomName = "The Back Gate"`, `maxConcurrentEnemies = 3`, `bossOwnedWin = false`.
- Warm-cool lighting flip from World 1 — see Lighting.

---

## Room A — The Rock Garden (random)

### Layout Overview
~32×26m. Raked-gravel zen garden crossed by stepping-stone lanes; gravel borders hide Leaf Pile Lurkers. Movement is channeled along the stone paths.

### ASCII Layout
```
W W W W W W W W
W  z  z  [l] z  W   ← gravel with buried Leaf lurker
W  s  s  s  s  W    ← stepping-stone lane
W  E  z  z  E  W    ← Gnomes on gravel
W  s  s  s  s  W
W  z  [l] z  z  W   ← 2nd leaf lurker
W  .  .  P  .  W    ← player enters
W W W W W W W W

z=raked gravel  s=stepping stones  l=leaf lurker (buried)
E=gnome  P=player entry
```

### Enemy Placement
| Enemy | Count | Spawn | Behavior |
|---|---|---|---|
| Gnome Soldier | 2 | mid-gravel | Charge along open gravel |
| Leaf Pile Lurker | 2 | gravel borders | Dormant; rise when player funneled past |

### Props
| Prop | Placement | Source | Notes |
|---|---|---|---|
| Raked gravel | floor fill | `Asian_Prop_Zen_Garden_Sand_01` | Flat, walkable |
| Zen rocks | garden accents | Polyworks `Rock_*` | Low cover / NavMesh carve |
| Stepping stones | lanes | `pfb_env_stepping_stone_tile` | Movement channels |
| Bamboo fountain | corner | `Asian_Prop_Bamboo_Dried_Water_Fountain_01` | Decoration |
| Bamboo wall | perimeter | BD-01 | Boundary |

### Unity Notes
- Leaf Lurkers spawn dormant; trigger rise on player proximity (reuse ambush pattern).
- Zen rocks are NavMesh obstacles; keep gravel lanes clear for pathing.

---

## Room B — The Training Hall (random)

### Layout Overview
~28×24m interior dojo hall (shed). Pillars and weapon racks break sightlines; tighter than the outdoor rooms — forces the player back into the Crane Duelist's line.

### ASCII Layout
```
W W W W W W W
W  r  .  [C] .  W   ← Crane Duelist far end, weapon Racks
W  |  .  .  .  |  W   ← pillars (cover)
W  .  E  .  E  .  W   ← Gnomes
W  |  .  .  .  |  W
W  t  t  P  t  W    ← tatami, player entry
W W W W W W W

C=Crane Duelist  |=pillar  r=weapon rack  t=tatami  E=gnome  P=player
```

### Enemy Placement
| Enemy | Count | Spawn | Behavior |
|---|---|---|---|
| Crane Duelist | 1 | far end (0,0,9) | Strafes to hold duel; Beak Thrust down the hall's long axis |
| Gnome Soldier | 2 | mid (±4,0,3) | Pressure player off the duel line |

### Props
| Prop | Placement | Source | Notes |
|---|---|---|---|
| Weapon racks | back wall | `pfb_env_weapon_rack` | Decoration + cover |
| Pillars | 4, mid-room | primitive/beam or shed pillars | Sightline breaks; NavMesh carve |
| Tatami | floor | `Asian_Prop_Tatami_Mat_*` | Floor dressing |
| Shed shell | room structure | `pfb_env_bld_shedwithcrate` | Interior walls |
| Paper lanterns | ceiling line | `Asian_Prop_Paper_Lantern_01` | Warm interior light |

### Unity Notes
- Crane Duelist debut — ensure `CraneDuelistAI` exists.
- Long axis (south→north) gives the Crane its thrust lane; pillars let the player break line but gnomes punish camping.
- Interior: dimmer, warmer point lights (paper lanterns).

---

## Room C — The Koi Pond (random, Skeptic room)

### Layout Overview
~30×26m. A koi pond fills the center as a no-stand zone; the player fights on narrow engawa (veranda) boards around it. Knockback risks a dunk (brief slow).

### ASCII Layout
```
W W W W W W W W
W  d  .  [C] .  d  W   ← engawa boards, Crane at far board
W  =  =  =  =  =  W    ← board (walkway)
W  =  [ KOI ]  =  W    ← pond (no-stand, water hazard)
W  =  =  =  =  =  W
W  l  .  E  .  l  W    ← Leaf lurkers + gnome on outer ring
W  .  .  P  .  .  W    ← player entry
W W W W W W W W

KOI=pond (trigger slow)  ==engawa board  C=Crane  l=leaf lurker
E=gnome  d=craftsmanship dressing  P=player
```

### Enemy Placement
| Enemy | Count | Spawn | Behavior |
|---|---|---|---|
| Crane Duelist | 1 | far board (0,0,8) | Thrust threatens to knock player into pond |
| Leaf Pile Lurker | 2 | outer ring corners | Rise to break spacing on the narrow boards |
| Gnome Soldier | 1 | outer ring center | Charges across boards |

### Props
| Prop | Placement | Source | Notes |
|---|---|---|---|
| Koi pond | center (0,0,2) | `street_pond_a` (⚠️) or BD-02 | Water = trigger slow; carve NavMesh |
| Engawa boards | ring around pond | plank meshes / stepping tiles | Walkways; keep narrow |
| Stone lanterns | board corners | `pfb_env_stone_lantern` | Low cover |
| Cherry tree | edge | `pfb_env_cherry_blossom_tree` | Petals over water |
| Craftsmanship ×4 | engawa edges | see GDD §11 | Non-interactive |

### Unity Notes
- **Skeptic appearance:** scripted moment in the shed doorway (see GDD §9) — brief, non-combat; reuse cutscene/trigger pattern.
- Water hazard: trigger volume, `moveSpeed` slow only (no damage/death).
- Keep boards genuinely narrow so knockback matters.

---

## Boss Room — The Blossom Court

### Arena Shape + Dimensions
~36m circular sparring court, cherry tree at dead center as the only fixed obstacle. Bamboo stockade ring. Petals drift constantly.

### Phase 1 Safe Zones ("Kata")
The Grasscutter positions deliberately; the player is safe circling the tree at mid-radius, using it to break the Petal Toss ranged fan. Cover = the central tree.

### Phase 2 Safe Zones + New Obstacles ("Rev")
The reel becomes a whirlwind and launches straight-line Spin-Dashes across the court — **moving hazard lanes**. Safe ground = perpendicular to the current dash lane; cut-grass petal trails shrink standable space over time. The central tree still blocks line-of-sight but no longer offers a static safe circle (dashes cross it). Player must keep rotating.

### Boss Spawn + Intro Sequence
Grasscutter dormant in tall grass at the north far edge. Trigger on room activation: camera cuts to the cherry tree, then to the mower reel spinning up; it rises and advances. Reuse the SpinCycle boss-intro camera pattern and timing.

### Unity Notes
- `GrasscutterAI`, two-phase (mirror `SpinCycleAI` structure). Phase transition at 50% HP with visual + audio cue (reel pitch rises).
- Phase 2 Spin-Dash: NavMesh-off straight-line lerp along a telegraphed lane; cut-grass trail = pooled trail-hazard volumes (cache waits, zero per-frame alloc).
- Parry targets: Phase 1 Blade Combo beats are the parryable hitboxes; Reel Guard-Break is NOT parryable.
- Defeat: reuse SpinCycle defeat sequence (stumble → wobble → burst → shrink) + Imagination Restore volume; trigger the cherry tree full-bloom on restore. `RoomManager.bossOwnedWin = true`.
- Minimap disabled in boss arena (as World 1).

---

## Cross-Room Technical Notes

### Level Generator Integration
Mirror World 1: `Backyard_Room1` (fixed) → random draw of {RockGarden, TrainingHall, KoiPond} → `Backyard_BossCourt` (fixed). Reuse the static room-queue fields (`s_roomQueue`, `s_roomQueueIndex`) so selection persists across scene loads. Restart returns to the world's first scene.

### Shared Prefabs Across Rooms
Bamboo wall (BD-01), stone lantern, cherry tree, stepping stones, forge workbench, `pfb_GameManager`, `pfb_AudioManager`, `pfb_ProgressionSystem`, `pfb_RoomManager`, `pfb_hud_v4`, HUD, CM follow cam.

### NavMesh Setup
Bake on flat ground per scene. NavMesh obstacles (carve): bamboo walls, cherry tree, stone lanterns, pillars (Training Hall), koi pond footprint, zen rocks. Agent radius 0.4m, height 2m. Verify all enemy spawns can path to player spawn; verify pathing around the central tree in the boss court.

### Performance Budget
| Category | Target |
|---|---|
| Triangles / room | < 160k |
| Draw calls | < 70 (GPU instancing on Polyworks atlas) |
| Dynamic objects | Player + enemies + boss VFX only; rest Static |
| New ENV geometry (whole zone) | < 8k tris (only BD-01, optionally BD-02) |

### Lighting
Cool overcast key light (opposite of World 1's warm amber): directional, pale jade-white `#DCE6DA`, softer intensity (~1.0), higher angle (diffuse midday-overcast), soft shadows tinted sage-grey. Interior (Training Hall) uses warm paper-lantern point lights for contrast. Ambient: cool green-grey fill. Bake after ENV dressing. Global Volume with Color Adjustments prepped for Imagination Restore (Awakening → full saturation on boss defeat).

---

*Blueprint owner: Louie Celli | Created: 2026-08-07 | Hand to `unity-senior-developer` after enemy/boss models land via `/asset-pipeline`.*
