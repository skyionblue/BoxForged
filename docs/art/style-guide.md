# Unboxed Heroes — Art Style Guide

**Version:** 1.1
**Date:** 2026-07-19
**Engine:** Unity 6 LTS + URP (Mobile quality tier)
**Platform:** iOS + Android (primary), PC (later)

---

## 1. Visual Identity in One Sentence

> Unboxed Heroes looks like a child drew a post-apocalyptic world on cardboard with markers — chunky characters, hand-crafted textures, and a world that gets more colorful the more imagination wins.

---

## 2. Core Art Direction

### The Two-Layer World

Every location in the game exists in two visual states. The art must support both:

| State | When | Look |
|---|---|---|
| **Drained** (Unimaginative-controlled) | Start of each zone | Muted, grey-brown, desaturated. Feels like an abandoned parking lot. |
| **Reclaimed** (player-controlled) | After boss is defeated | Vivid, saturated, warm. Hand-drawn details appear. The world blooms. |

The transition between these two states is the most important visual moment in the game. Everything in the art pipeline should be designed with this shift in mind.

### The Cardboard & Marker Aesthetic

The world is made of **craft materials** — cardboard, marker ink, crayon, tape, and foil. This is not photorealistic. It is not flat cartoon. It sits in between:

- Surfaces look like **corrugated cardboard or kraft paper** — visible grain, slightly rough
- Outlines and details look like **marker strokes** — slightly uneven, hand-drawn
- Bright colors look like **Crayola markers** on cardboard — vivid but warm, not neon
- Metallic things (foil armor, tin can helmets) look like **aluminum foil scrunched and flattened**
- Shiny things look like they were wrapped in **gift wrap or cellophane**

**What this is NOT:**
- Photorealistic textures
- Smooth plastic-looking surfaces
- Flat 2D cartoon (it's 3D with depth and lighting)
- Gritty post-apocalyptic brown (the drained state is grey, not grimy)

---

## 3. Color Palette

### Drained State (Unimaginative)

Used for zone before player reclaims it. Applied via URP post-process volume.

| Role | Color | Notes |
|---|---|---|
| Primary | `#C8BFB0` — warm grey | Base tone for everything |
| Secondary | `#A89F94` — darker grey-brown | Shadows, depth |
| Accent | `#6B6560` — dark grey | Edges, outlines |
| Sky/ambient | `#D4CFC9` — pale grey | Fog-like, no blue |

> In Unity: implement as a URP Global Volume with Color Adjustments (Saturation: -80, Contrast: +10, Color Filter: warm grey tint).

### Reclaimed State (Imagination)

Used after zone boss is defeated. Full vivid color.

| Role | Color | Hex | Notes |
|---|---|---|---|
| Primary warm | Craft tan | `#E8C97A` | Cardboard base color |
| Sky blue | Marker blue | `#4A90D9` | Clean, saturated sky |
| Grass / nature | Marker green | `#5CB85C` | Vivid, slightly warm |
| Accent 1 | Marker red | `#E05A4E` | Enemies, danger, energy |
| Accent 2 | Marker orange | `#F5A623` | IP points, rewards |
| Accent 3 | Marker purple | `#9B59B6` | Ninja Box color |
| Accent 4 | Marker gold | `#F7C244` | Cowboy Box color |
| Shadow | Dark brown | `#3D2B1F` | Outlines, deep shadows |

### Character-Specific Colors

**Phase 1 (Backyard)**

| Character | Box Color | Accent |
|---|---|---|
| Kid — Ninja Box | `#1A1A2E` dark navy | Purple marker lines |
| Kid — Cowboy Box | `#8B5E2A` leather brown | Gold marker lines |
| The Skeptic | `#787878` flat grey | No decoration — just cardboard |

**Phase 2 (Cul-de-Sac)**

| Character | Box Color | Accent |
|---|---|---|
| NinjaFemale | `#1A1A2E` dark navy | Purple marker lines — same box design, different silhouette |
| Cowgirl | `#8B5E2A` leather brown | Gold marker lines, brim drawn on sides |

**V2 Enemy Colors**

| Enemy | Primary | Accent |
|---|---|---|
| WagonWheelRoller | `#8B6914` weathered wood brown | `#C0A050` worn brass hub ring |
| HitchingHound | `#5C4A2A` dark leather | `#C8B098` chain links — warm silver |
| MilepostMarshal | `#7A7A6E` aged iron grey | `#D4A832` faded yellow road paint stripes |
| SprinklerSentinel (V2) | Same as Phase 1 — brass with glowing blue eye | — |

---

## 4. Characters

### Proportions

Characters are **"chibi-adjacent"** — not full chibi, but chunky and readable at small mobile screen sizes:

```
Head (box): ~35% of total height
Torso:      ~30% of total height
Legs:       ~35% of total height
Width:      ~60% of height (stocky, not tall and thin)
```

The box on the head is a **perfect cube** — slightly oversized relative to the body. This is intentional. The box should read immediately as a box, not a helmet.

### Texture Style — Characters

- **Base material:** Cardboard texture baked into the diffuse — visible corrugation lines running horizontally
- **Marker drawings:** Drawn directly onto the box texture — the ninja mask lines, cowboy hat brim, etc. Look hand-drawn with slight wobble, not perfect
- **Clothing:** Flat color with a very subtle fabric grain — like construction paper
- **Skin (if visible):** Warm, simple — one or two tones, no photorealistic shading
- **Poly count target:** 800–1,500 triangles per character (mobile budget)

### The Skeptic — Visual Rules

The Skeptic must **visually contrast** with the player at all times:
- Box is unpainted — raw grey cardboard, no marker lines
- Clothes are grey-washed versions of normal colors
- No warm tones anywhere on the character
- When they take damage, a brief color flash of grey-white (not the warm flash other enemies get)

---

## 5. Environments

### Environment Structure

Each zone has two full art passes — the **drained version** and the **reclaimed version**. In Phase 1, only the Backyard zone exists. The drained → reclaimed transition is a URP post-process color grade shift, not a full asset swap (that comes in Phase 3).

### The Backyard (Phase 1)

**Drained:**
- Overgrown grass, yellowed and flat
- Broken wooden fence — grey, warped planks
- Garden shed — collapsed, corrugated metal roof (grey)
- Dead apple tree — bare branches, no leaves
- All objects: desaturated, grey-brown

**Reclaimed:**
- Grass: vivid marker green, slightly stylized blades
- Fence: wood tone restored, with marker-drawn details (nails, grain lines)
- Apple tree transforms into a cherry blossom — pink marker blossoms
- Training dummies appear (imagined) — made of what looks like cardboard tubes and rope
- Stone garden path appears — flat grey stones with marker-drawn moss

### The Cul-de-Sac (Phase 2)

**Drained:**
- Cracked asphalt — grey, weed-broken, oil-stained
- Dead-end street curb — concrete grey
- Empty house facades — sun-bleached, peeling, grey-brown
- All props: desaturated dust tones

**Reclaimed:**
- Asphalt becomes packed dirt main street — warm sandy brown (`#C4A46A`)
- Sky: warm afternoon — `#F5A623` orange fading to `#4A90D9` blue
- Saloon facade: sun-bleached wood, marker-drawn weathervane and signage
- Hitching post: knotted rope in warm brown, marker-etched grain lines
- Covered wagon: faded canvas (`#E8D6A0`), wood wheels in the same weathered brown as WagonWheelRoller
- Lamp posts: cast iron grey with a warm orange lantern glow
- Tumbleweed: pale straw, marker-line detail on individual branches
- Command Node Birdbath (objective): Reclaimed state adds glowing blue water and surrounding wildflowers — signals zone completion approaching

**ENV Prop Texture Rules:**
- Saloon and larger props: 512×512 diffuse
- Small props (barrel, tumbleweed, wanted poster): 256×256 diffuse
- Weathering baked in — sun-bleach gradient from top to bottom on all wooden surfaces
- No normal maps in Phase 2 (same rule as Phase 1 — Phase 3 polish pass)

### Environment Asset Source

**Low Poly Mega Pack - Polyworks** (Unity Asset Store) — all ENV props are sourced from this pack. The pack's low-poly aesthetic matches the game's cardboard-and-craft visual style perfectly.

### Environment Texture Rules

- **Ground:** Tiling texture, 512×512 max on mobile. Slight hand-drawn grass blade pattern.
- **Walls/structures:** 512×512 diffuse. Cardboard grain on flat surfaces, visible construction lines.
- **Props (from Polyworks):** Already textured and optimized for mobile; use as-is where possible. Retexture only when the pack's default materials don't match the cardboard aesthetic.
- **Draw calls:** Target **<100 draw calls** per scene on mobile. Use static batching for environment props.
- **No normal maps** on environment in Phase 1 — keep it flat and craft-like (normal maps can be added in Phase 3 polish).

### URP Shader — The Cardboard Look

Use the **URP Lit shader** with these settings for the cardboard aesthetic:
- Smoothness: `0.05–0.15` (very matte — cardboard is not shiny)
- Metallic: `0` (except foil items — set to `0.7`)
- Emission: `0` (no glowing surfaces in base state)

For the hand-drawn outline effect: use a **Post-Process Outline** pass in URP (edge detection on depth/normals buffer). Outline color: `#3D2B1F`. Width: 1.5–2px at 1080p, scaled for device resolution.

---

## 6. UI Style

The UI looks like it was **drawn by a kid on a piece of cardboard** using markers and crayons.

### UI Design Rules

- **Fonts:** Hand-written, slightly irregular. Recommended free options: "Permanent Marker" (Google Fonts) or "Schoolbell". Never use a clean sans-serif.
- **Buttons:** Rounded rectangles with a marker-drawn border — slightly uneven edges. Fill with flat color. Drop shadow: hand-drawn, offset slightly.
- **Icons:** Simple, thick-line marker drawings. Not detailed. Recognizable at 48×48px minimum.
- **Health bar:** Drawn as a series of **crayon-filled rectangles** — not a smooth bar. Each rectangle represents a health chunk. When damaged, one rectangle gets a big red X drawn over it.
- **IP counter:** Tally marks — groups of five vertical strokes. Feels like a kid counting on paper.
- **Active box indicator:** Small icon of the current box with its marker decoration. Sits top-center of screen.

### On-Screen Controls — Joystick & Buttons

Controls are **2D sprites rendered on the Canvas** — no 3D meshes. All assets are PNG sprites, ASTC/ETC2 compressed.

#### Virtual Joystick

| Asset | Size | Description |
|---|---|---|
| `ui_joystick_base.png` | 256×256 | A hand-drawn circle — slightly uneven, marker-stroke style. Background: aged paper `#F5EDD6` at 70% opacity. Stroke: dark brown `#3D2B1F`, 3px hand-drawn feel. |
| `ui_joystick_thumb.png` | 128×128 | Filled circle. Color matches active box: purple `#9B59B6` for Ninja, gold `#F7C244` for Cowboy. Slightly worn, marker-filled look. |

**Behavior:** Base appears where the left thumb touches — fades to invisible (alpha 0) within 1s of releasing. Thumb moves within the base radius.

#### Action Buttons (Diamond Layout)

All four buttons share the same base shape: **rounded rectangle, 120×120pt touch target, marker-drawn border**.

| Asset | Fill Color | Label | States |
|---|---|---|---|
| `ui_btn_attack` | `#E05A4E` marker red | **ATTACK** | normal, glow (Counter Window) |
| `ui_btn_dodge` | `#4A90D9` marker blue | **DODGE** | normal |
| `ui_btn_parry` | `#5CB85C` marker green | **PARRY** | normal, pulse (enemy wind-up cue) |
| `ui_btn_jump` | `#F5A623` marker orange | **JUMP** | normal |

**Per-button sprite files:**
- `ui_btn_attack_normal.png`, `ui_btn_attack_glow.png`
- `ui_btn_dodge_normal.png`
- `ui_btn_parry_normal.png`, `ui_btn_parry_pulse.png`
- `ui_btn_jump_normal.png`
- All at **256×256px**

**Visual rules:**
- Border: dark brown `#3D2B1F`, hand-drawn (slightly uneven thickness, not a perfect rect)
- Fill: flat color, no gradient — Crayola marker on cardboard feel
- Label: hand-written font ("Permanent Marker" or "Schoolbell"), white, centered
- Drop shadow: hand-drawn, offset 2–3px down-right, 40% opacity
- Normal state: 75% opacity so gameplay is visible underneath
- Pressed state: darken fill by 15%; border thickens slightly (simulates pressing cardboard)
- Glow state (ATTACK during Counter Window): add a pulse animation — opacity cycles 75%→100%→75% at ~2Hz in the player's box color
- Pulse state (PARRY during enemy wind-up): slow pulse — opacity cycles 75%→95%→75% at ~1Hz

### Mobile UI Layout

All UI must respect:
- **Top safe area:** 44pt minimum clearance from top edge (Dynamic Island / notch)
- **Bottom safe area:** 34pt minimum clearance (home indicator)
- **Minimum touch target:** 48×48pt for any interactive element
- **HUD opacity:** Semi-transparent (70–80%) so gameplay is visible underneath

### UI Color Usage

- Backgrounds: `#F5EDD6` — aged paper / cardboard color
- Primary text: `#3D2B1F` — dark brown marker
- Highlight / active: Accent colors from the palette (orange for IP, purple for ninja, gold for cowboy)
- Danger / low health: `#E05A4E` — marker red

---

## 7. VFX Style

All VFX should feel **paper and craft-made**:

| Effect | Style |
|---|---|
| Hit spark | Burst of marker-star shapes, flat 2D sprites |
| Dodge trail | Brief afterimage — same character silhouette, 40% opacity |
| Parry success | Gold ring expanding outward — drawn circle, hand-made feel |
| IP pickup | Orange tally mark floats upward and fades |
| Imagination Restore | Color washes across the scene — like watercolor bleeding outward from the player |
| Enemy defeat | Cardboard explosion — flat cardboard-shaped pieces scatter and fade |

**VFX technical rules:**
- Use **Shader Graph particle shaders** — no expensive VFX Graph on mobile in Phase 1
- Max **50 particles** per emitter on mobile
- All VFX sprites: 256×256 max, use sprite sheets where possible

---

## 8. Mobile-Specific Constraints

These are hard limits. Exceeding them causes frame drops on mid-range devices (iPhone 12 / Samsung A-series).

| Constraint | Limit |
|---|---|
| Draw calls per frame | < 100 |
| Triangles per frame | < 150,000 |
| Texture size (characters) | 512×512 max |
| Texture size (environment) | 512×512 max (tiling OK) |
| Texture size (UI) | 256×256 per element |
| Texture format | ASTC (iOS) / ETC2 (Android) — set in Unity Texture Importer |
| Real-time lights | Max 2 per scene (1 directional + 1 optional point) |
| Shadows | Directional light only, low resolution (512px shadow map) |
| Particle systems active | Max 5 simultaneously |
| Target frame rate | 60fps on iPhone 12 / equivalent Android |

---

## 9. Placeholder Asset Strategy

Development must never be blocked waiting for final art. Use these placeholders:

| Asset Type | Placeholder | Source |
|---|---|---|
| Player character | Unity capsule primitive + colored box on top | Built-in |
| Enemy characters | Unity capsule primitive, different color per type | Built-in |
| Environment geometry | Unity cube/plane primitives, colored materials | Built-in |
| UI elements | Unity default UI with colored panels and Arial text | Built-in |
| VFX | Simple particle system with default sphere particles | Built-in |
| Icons | Colored squares with text label | Built-in |

**Placeholder color conventions (internal only):**

| Object | Placeholder Color |
|---|---|
| Player (Ninja Box) | Purple |
| Player (Cowboy Box / NinjaFemale) | Purple |
| Player (Cowgirl) | Gold |
| Gnome Soldier | Red |
| Leaf Pile Lurker | Dark green |
| The Skeptic | Grey |
| WagonWheelRoller | Orange-brown |
| HitchingHound | Dark brown |
| MilepostMarshal | Grey-green |
| SprinklerSentinel | Teal |
| Collectible / IP | Orange |
| Safe zone | Blue |
| Trigger zone | Yellow |

---

## 10. Asset Naming Convention

All assets follow this pattern: `[type]_[subject]_[variant]_[state]`

| Type Prefix | Used For |
|---|---|
| `chr_` | Character meshes and textures |
| `env_` | Environment meshes and textures |
| `ui_` | UI sprites and elements |
| `vfx_` | VFX sprites and prefabs |
| `sfx_` | Audio files |
| `mat_` | Materials |
| `tex_` | Textures |
| `pfb_` | Prefabs |
| `so_` | ScriptableObjects |
| `anim_` | Animation clips |

**Examples:**
```
chr_player_ninja_idle.fbx
chr_player_ninja_idle.anim
tex_chr_player_ninja_diffuse.png
mat_chr_player_ninja.mat
pfb_chr_gnomesoldier.prefab
env_backyard_fence_drained.fbx
ui_hud_healthbar.png
vfx_hit_spark.prefab
sfx_combat_parry_success.wav
so_enemy_gnomesoldier.asset
```

---

## 11. Reference Images & Mood

The following publicly available games and media capture aspects of the Unboxed Heroes art direction. Use these as conversation anchors with artists, not as targets to copy:

| Reference | What to borrow |
|---|---|
| **Tearaway** (Media Molecule) | Cardboard/paper texture aesthetic, craft material world |
| **A Short Hike** | Simple, clean character proportions, warm color palette |
| **Psychonauts** | Chunky, expressive character design |
| **Hollow Knight** (early zones) | Drained-to-bright visual progression as you reclaim areas |
| **Little Big Planet** | Craft material props and environment construction |
| **Scott Pilgrim vs. The World (game)** | Marker-line outlines, flat vivid colors, pop energy |

---

_Art Style Guide v1.1 — 2026-07-19_
_v1.0: Phase 1 Backyard. v1.1: V2 Cul-de-Sac environment, V2 character/enemy colors, on-screen controls art spec (joystick + 4-button diamond)._
_All decisions subject to revision after Phase 1 playtesting._
