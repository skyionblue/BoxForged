# Unboxed Heroes — Figma Weave AI Image Prompts

**Purpose:** 2D illustration and concept art for loading screens, menus, zone mood boards, character references, cutscene panels, and marketing. Weave handles all 2D AI generation — Meshy handles 3D.
**Tool:** Figma Weave (weave.figma.com) — node-based AI image generation and compositing
**Art direction:** Cardboard-and-marker aesthetic. Everything looks handmade from craft materials. Chunky stylized look — not photorealistic, not flat 2D cartoon. Sits between. Think Tearaway meets Psychonauts. Every prompt enforces strong silhouettes, warm palettes per zone state, and visible corrugation and marker-line detail.
**Two world states:** Every scene exists in Drained (grey, muted, `#C8BFB0` primary) or Reclaimed (vivid, warm, `#E8C97A` primary). Know which state you are illustrating before picking a prompt.

---

## 1. How to Use Weave

### Workflow

1. Open weave.figma.com. Create a new canvas or open your project canvas.
2. Add a **Generate Image** node. Paste the prompt from this document into the text field.
3. Select the model backend (see model recommendations per prompt below).
4. Set the output dimensions to match the spec for that asset (16:9, 1:1, etc.).
5. Generate 4 variants. Pick the strongest composition — assess for silhouette, color palette match, and cardboard texture fidelity before accepting.
6. For further editing: pipe the output into an **Inpaint** node to fix specific regions, an **Upscale** node before final export, or a **Relight** node to swap between Drained and Reclaimed world states.
7. Export as PNG. Follow the file-naming convention: `ui_[subject]_[variant]_[state].png`.

### Model Selection Guide

| Model | Best for |
|---|---|
| **Recraft V3** | Flat illustration, UI art, character concept sheets — clean, graphic, stylized. Preferred for menu backgrounds, character references, and HUD concept art. |
| **Stable Diffusion XL** | Texture work, environmental wide-angles, mood boards — handles grain and material detail better than Recraft. Preferred for zone mood boards and loading screen variants. |
| **Runway Gen-3** | Painterly atmospheric pieces — soft edges, blended backgrounds. Use when a scene needs more ambient depth than graphic precision. Good for cutscene panels. |
| **Google Imagen 3** | High compositional fidelity — respects layout descriptions closely. Use when the composition spec is precise and you need the AI to honor it (e.g. "player left-center, tree right-center, sky top 20% clear"). |
| **OpenAI GPT-Image** | Quick concept iteration — good at following mixed-style prompts. Use early in a process to test concept directions before committing to a longer Recraft or SDXL run. |

### Weave-Specific Workflow Tips

**Inpainting for iteration:** After a base image is generated, use the Inpaint node to fix specific regions without regenerating the whole image. Example: if the World Tree silhouette reads correctly but the foreground debris looks wrong, inpaint only the bottom 20% of the frame. Preserve everything that works.

**Upscaling:** All final-output images should pass through the Upscale node before export. Generate at native resolution, then upscale to the delivery spec. Do not upscale more than 2x or texture grain becomes blurry.

**Relighting for world states:** The Relight node is the fastest way to produce the Drained and Reclaimed variants of the same composition. Generate the Reclaimed (vivid, warm) version first — the world in full color is easier for AI to generate well. Then use Relight with a cool, desaturated light setup and grey-tone override to produce the Drained variant from the same composition.

**Feeding Meshy renders into Weave:** Screenshot a Meshy 3D model in Unity or in the Meshy preview (any angle, white or grey background). Feed that screenshot into Weave as a reference image input. Generate a 2D concept illustration from it — this workflow is the fastest way to produce character reference sheets without building a separate 2D illustration pipeline. The AI uses the 3D model's silhouette and proportion as a structural anchor while applying the cardboard-and-marker style.

**Negative prompt — use on every prompt in this document:**
```
photorealism, smooth plastic, neon colors, glowing effects, lens flare, watermark, text overlay, UI elements, modern graphic design, clean vector art, flat 2D cartoon, dark grimy tone, excessive detail that reads as noise at small sizes
```

---

## 2. Loading Screen

The loading screen is approved for production. Full compositional spec is in `/Users/jcelli/Documents/personal/boxhead/docs/art/loading-screen.md`. Prompts here supplement the three Meshy prompts in that doc with Weave-native variants, plus a Reclaimed-world alternative.

---

### Loading Screen — Drained World (Primary)

**Use:** Main loading screen background — `ui_loading_bg_drained.png`
**Model recommendation:** Stable Diffusion XL (best texture grain fidelity for the cardboard earth)
**Output dimensions:** 2732 × 1536 px — scale down to target device in Unity

**Prompt**
```
Wide cinematic landscape illustration, 16:9 format. A child warrior in a chibi-proportioned
cardboard-box ninja outfit stands in the left-center foreground, seen from behind, looking
toward a massive withered World Tree in the mid-distance. The tree is built from stacked
compressed corrugated cardboard rings, its bark marked with hand-drawn marker growth lines
and kraft tape repair strips, its branches bare and skeletal, reaching outward. Low three-quarter
camera angle looking slightly upward from waist height — the tree looms, the player looks small.
Drained desaturated world: pale warm grey sky (#D4CFC9), cracked corrugated cardboard earth
(#A89F94), no green, no color saturation. Single warm directional light from the right at low angle
catches the player's right shoulder and the tree's right face in faint amber-grey (#C9BC9A).
Scattered foreground debris: overturned tin can with marker-drawn ridges, crumpled grey paper
balls, a curling strip of kraft tape on the cracked ground. Atmospheric dust motes drift upward
at the tree base. Thick dark brown marker outlines (#3D2B1F) on all edges. Flat cel-shaded surfaces
with visible corrugation grain throughout. Top 20% of frame is clear sky — no branches cross it.
Bottom 15% of frame trends dark (vignette toward #6B6560) — loading bar will overlay here.
Craft material world aesthetic. Tearaway game art style. No text. No UI.
```

**Negative prompt:** photorealism, bright colors, green vegetation, smooth surfaces, neon, branches in top 20% of frame, happy tone, modern architecture, lens flare, watermark

**Acceptance criteria:**
- Player silhouette reads clearly against the grey ground at thumbnail size
- World Tree is the dominant vertical element in the center-right third of the frame
- Top 20% is clear sky — no obstructions for the title treatment that will overlay here
- Bottom 15% is dark enough that a kraft-brown loading bar will read against it
- No warm colors except the faint directional light hit on the player's right shoulder and the tree's right face

---

### Loading Screen — Reclaimed World (Alt / Victory Variant)

**Use:** Alternate loading screen for post-boss play sessions — `ui_loading_bg_reclaimed.png`. Used when the zone is already cleared.
**Model recommendation:** Recraft V3 (vivid colors read more cleanly with Recraft's graphic engine)
**Output dimensions:** 2732 × 1536 px

**Prompt**
```
Wide cinematic landscape illustration, 16:9 format. A child warrior in a chibi-proportioned
cardboard-box ninja outfit stands in the left-center foreground, seen from behind, looking toward
a massive living World Tree in the mid-distance. The tree is built from rich warm brown corrugated
cardboard rings, vivid marker-green cardboard leaf clusters unfurl on its branches, paper butterflies
in marker orange and purple drift lazily past the mid-frame. Low three-quarter camera angle
looking slightly upward. Reclaimed vivid world: saturated marker-blue sky (#4A90D9), warm kraft
tan ground (#E8C97A), marker-green grass blade details along the ground plane. Warm afternoon
directional light from the right, amber-gold (#F5A623) catching the player's shoulder and the tree's
bark. Player outfit retains dark navy box with purple marker lines. Paper confetti scraps float near
the ground — orange (#F5A623), purple (#9B59B6), gold (#F7C244). Thick dark brown marker outlines
(#3D2B1F) on all edges. Flat cel-shaded surfaces, visible corrugation grain. Top 20% is clear blue sky.
Warm hopeful mood — imagination restored, the world blooms. Tearaway and Little Big Planet style.
No text. No UI.
```

**Negative prompt:** grey, desaturated, drained world, photorealism, neon, smooth plastic, gloomy tone, dark sky, watermark

**Acceptance criteria:**
- World feels visibly, joyfully alive — the color contrast to the Drained variant should be immediately apparent
- Marker-green leaf clusters on the tree read as cardboard cutouts, not photorealistic foliage
- Paper confetti elements feel hand-cut, not digital particle effects
- Top 20% remains clear for the title treatment overlay

---

## 3. Menu Backgrounds

---

### Main Menu Background

**Use:** Atmospheric background for the main menu scene (not yet built — produce now for when it ships)
**Model recommendation:** Stable Diffusion XL (atmosphere and grain) or Runway Gen-3 (if a more painterly, softer result is preferred)
**Output dimensions:** 2732 × 1536 px

**Prompt**
```
Wide cinematic atmospheric landscape illustration, 16:9 format. The massive World Tree stands
at center frame, built from stacked compressed corrugated cardboard rings, its bark showing hand-drawn
marker growth lines and kraft tape repair strips. The tree's branches are bare and skeletal in the
drained world state. Camera locked, no motion implied. Pale warm grey overcast sky (#D4CFC9),
cracked corrugated cardboard earth (#A89F94). Subtle radial fog at the tree base — grey-white at
35% opacity, softening the tree's connection to the ground. Desaturated dust motes drifting upward,
very sparse. The tree top reaches near the top of the frame. Deep quiet mood — world holding its
breath. Thick dark brown marker outlines (#3D2B1F). Flat cel-shaded surfaces. No characters.
Center of frame held clear vertically for title and menu button overlay. Bottom 20% darker (vignette)
for button contrast. Tearaway art style. No text. No UI.
```

**Negative prompt:** photorealism, bright colors, green trees, characters in frame, camera motion implied, neon, cheerful, fast-moving elements, watermark

**Model recommendation:** Stable Diffusion XL
**Acceptance criteria:**
- World Tree is centered and occupies most of the frame height
- Center column is compositionally clean — title and menu buttons will overlay here
- Mood is quiet and vast, not threatening or grim

---

### Character Select Background

**Use:** Background for the character selection screen — `ui_character_select_bg.png`
**Model recommendation:** Recraft V3 (graphic clarity for a UI context)
**Output dimensions:** 2732 × 1536 px

**Prompt**
```
Wide landscape illustration, 16:9 format, styled for a character selection screen background.
The setting is a stylized Cul-de-Sac Wild West main street in its Reclaimed vivid state. Packed
sandy-brown dirt main street (#C4A46A) recedes into the mid-distance. Warm afternoon sky:
amber-orange (#F5A623) fading upward to marker blue (#4A90D9) at the top of frame. Flanking
structures: a sun-bleached wooden saloon facade on the left, a hitching post with knotted rope on
the right, lamp posts with warm orange lantern glow. Background: a covered wagon in faded canvas,
tumbleweed in pale straw, wildflowers around a birdbath. All props have thick dark brown marker
outlines (#3D2B1F) and hand-drawn grain detail lines. Visible corrugated cardboard texture on
wooden surfaces. Flat cel-shaded, no photorealism. The center one-third of the frame is compositionally
open — no tall props cross this band — leaving space for character portrait cards. Warm, energetic,
game-ready mood. Tearaway and Scott Pilgrim visual style. No characters. No text. No UI.
```

**Negative prompt:** photorealism, dark tone, drained grey world, smooth plastic, neon, characters in frame, cluttered center composition, watermark

**Acceptance criteria:**
- Center vertical band is visually clear to let character portrait cards overlay without competing
- Warm amber and terracotta tones dominate — this is the Cul-de-Sac Reclaimed palette
- Corrugated cardboard texture is visible on wooden surfaces, not smooth painted wood

---

### Game Over — Defeat Screen Background

**Use:** Atmospheric background for the defeat / game-over state — `ui_defeat_bg.png`
**Model recommendation:** Runway Gen-3 (painterly, soft edges reinforce the deflated mood)
**Output dimensions:** 2732 × 1536 px

**Prompt**
```
Wide melancholy atmospheric landscape illustration, 16:9 format. Drained desaturated cardboard
world. The frame holds still — quiet, heavy, defeated. Foreground: cracked grey corrugated cardboard
earth, an overturned tin can, three crumpled grey paper balls. Mid-distance: the base of the World
Tree, bare and grey, trunk showing kraft tape repair strips. Grey paper scraps drift slowly downward
from the top of the frame, like ash falling — four or five scraps visible, sparse. The tree's bare
branches are slightly bowed. Cold still mood — not violent, just quiet defeat. Pale warm grey sky.
Thick dark brown marker outlines. Flat cel-shaded corrugated cardboard surfaces throughout.
Dark vignette on all four edges, heaviest at the top to allow "You Lost" text to overlay legibly.
Tearaway art style. No characters. No text. No UI.
```

**Negative prompt:** photorealism, bright colors, action, cheerful tone, glowing elements, characters, neon, victory imagery, watermark

**Acceptance criteria:**
- Mood communicates defeat without being threatening or violent
- Edge vignette is heavy enough that white or kraft-brown text will read over it
- Paper scraps falling reads as gentle sadness, not dramatic VFX

---

### Victory Screen Background

**Use:** Atmospheric background for the win / zone-cleared state — `ui_victory_bg.png`
**Model recommendation:** Recraft V3 (vivid color graphic)
**Output dimensions:** 2732 × 1536 px

**Prompt**
```
Wide joyful landscape illustration, 16:9 format. The moment of Imagination Restore — the world
springs back to vivid life. Warm amber-orange light burst radiates from the center of the frame,
like a match being struck in a dark room. Color bleeds outward: kraft tan ground (#E8C97A), marker-
green grass blades spring up, saturated marker-blue sky (#4A90D9) fills the upper frame. The World
Tree at center-right glows with new life — its corrugated bark warms from grey to rich brown,
cardboard leaf clusters in vivid marker green unfurl on its branches. Paper confetti in marker orange
(#F5A623), purple (#9B59B6), and gold (#F7C244) bursts upward from the ground in arcs. The color
spread has a watercolor-bleed quality — soft diffusing edges, not a sharp graphic boundary.
Thick dark brown marker outlines on all elements. Flat cel-shaded surfaces. Warm, euphoric, earned.
Heavy vignette on all edges — lightest at center where the burst originates. Tearaway art style.
No characters. No text. No UI.
```

**Negative prompt:** photorealism, grey, drained world, dark tone, digital glow, neon, smooth plastic, watermark

**Acceptance criteria:**
- The center color burst reads as a clear visual event — something happened here
- Paper confetti shapes read as hand-cut flat pieces, not round particles or digital sparkles
- The color transition from center outward is visible — edges still have some grey-tinged residue while the center is fully vivid

---

## 4. Zone Mood Boards

Mood boards are wide-angle atmospheric renders used as creative reference — not final game assets. They establish the feel of a zone before any ENV prop placement happens. Resolution can be lower than UI output assets (1920×1080 is sufficient). Use freely for reference; these are not imported into Unity.

---

### Zone 0 — The Backyard (Reclaimed)

**Use:** Creative reference for the Backyard zone in its Reclaimed state. Feudal Japanese dojo overlaid on a suburban backyard.
**Model recommendation:** Stable Diffusion XL
**Output dimensions:** 1920 × 1080 px (reference only)

**Prompt**
```
Atmospheric wide-angle illustration. A suburban backyard transformed by a child's imagination
into a feudal Japanese dojo — the two realities overlaid. The backyard is real and slightly visible:
wooden fence planks, apple tree branches, garden shed outline. But the imagination layer dominates:
the fence becomes dojo wall panels with marker-drawn kanji symbols, the apple tree is in vivid pink
cherry blossom bloom with flat cardboard-cutout petals in marker pink (#E05A4E tinted lighter),
the shed becomes a small dojo training hall with a marker-drawn roof eave. Ground: vivid marker-green
grass with individual blade lines drawn in dark marker. Stone garden path in flat grey stones with
marker-drawn moss lines. Training dummies made of cardboard tubes and rope stand in the mid-ground.
Warm golden afternoon light. Saturated, warm, full-color Reclaimed palette: marker green, craft tan,
marker blue sky. Thick dark brown marker outlines (#3D2B1F) on everything. Flat cel-shaded corrugated
surfaces. Children's creative energy — handmade, earnest, joyful. Tearaway and A Short Hike aesthetic.
No characters. No text.
```

**Negative prompt:** photorealism, grey drained world, smooth surfaces, adult architectural detail, dark tone, neon, grim post-apocalyptic mood, watermark

**Acceptance criteria:**
- The dual-reality layer reads — you can see both the suburban backyard structure and the dojo imagination layer simultaneously
- Cherry blossom petals read as flat cardboard cutout shapes, not photorealistic organic petals
- Overall mood is warm and hopeful — a kid's best-day-ever energy

---

### Zone 0 — The Backyard (Drained)

**Use:** Creative reference for the Backyard zone before the player reclaims it.
**Model recommendation:** Stable Diffusion XL
**Output dimensions:** 1920 × 1080 px (reference only)

**Prompt**
```
Atmospheric wide-angle illustration. An overgrown abandoned suburban backyard in a drained
desaturated post-apocalyptic world. Yellowed flat grass, dead and grey. Broken wooden fence —
grey warped planks with gaps. A dead apple tree — bare branches, no leaves, silhouetted against
pale grey sky. A collapsed garden shed with a corrugated metal roof, grey and bowed. Ground texture:
dry cracked soil, like corrugated cardboard that has absorbed too much rain and dried flat.
Overcast pale warm grey sky (#D4CFC9). No living color anywhere. All surfaces: desaturated grey-brown.
Visible corrugated grain on wooden surfaces. Thick dark brown marker outlines (#3D2B1F). Flat cel-shaded.
Scattered debris: dead leaves, a broken garden gnome tipped sideways, a rusted watering can.
Mood: abandoned, quiet, grey — but not threatening. The world is grey, not evil.
Tearaway art style. No characters. No text.
```

**Negative prompt:** photorealism, bright colors, green vegetation, warm mood, neon, glowing, characters in frame, watermark

**Acceptance criteria:**
- Zero warm color anywhere in the frame — the only warm note allowed is the very faint corrugated cardboard material tone on wooden surfaces (not saturated)
- The broken garden gnome reads as a specific easter egg — its silhouette should be recognizable
- Mood is melancholy but not hopeless

---

### Zone 1 — The Cul-de-Sac (Reclaimed)

**Use:** Creative reference for the Cul-de-Sac in its Reclaimed Wild West state.
**Model recommendation:** Stable Diffusion XL or Recraft V3
**Output dimensions:** 1920 × 1080 px (reference only)

**Prompt**
```
Atmospheric wide-angle illustration. A suburban cul-de-sac dead-end street transformed by
imagination into a Wild West main street — the two realities overlaid. Packed sandy-brown dirt
main street (#C4A46A) replaces the cracked asphalt. Warm afternoon sky: amber-orange (#F5A623)
at the horizon fading to saturated marker blue (#4A90D9) overhead. House facades become saloon
frontages: sun-bleached wood, marker-drawn weathervanes and hanging signage, wooden awnings.
A hitching post in the left mid-ground: knotted rope in warm brown, marker-etched wood grain.
A covered wagon to the right: faded canvas (#E8D6A0), weathered brown wood wheels that echo
the WagonWheelRoller enemy design. Cast iron lamp posts with warm orange lantern glow catching
the dusty air. Tumbleweed in pale straw in the far background. A birdbath Command Node at center-
mid-distance glows faintly blue, surrounded by small wildflowers — the zone objective. Thick dark
brown marker outlines (#3D2B1F) on all surfaces. Visible corrugated cardboard texture on wood.
Flat cel-shaded, no photorealism. Warm, dusty, adventurous. Scott Pilgrim meets Tearaway.
No characters. No text.
```

**Negative prompt:** photorealism, grey drained world, neon, smooth plastic, dark mood, modern street furniture, watermark

**Acceptance criteria:**
- The dual-reality layer reads — you see a suburban street structure under the Wild West imagination overlay
- The birdbath Command Node at center is the clear mid-ground focal point
- Amber-terracotta-dusty orange dominates — the Cul-de-Sac warm palette is firmly established

---

### Zone 1 — The Cul-de-Sac (Drained)

**Use:** Creative reference for the Cul-de-Sac before the player reclaims it.
**Model recommendation:** Stable Diffusion XL
**Output dimensions:** 1920 × 1080 px (reference only)

**Prompt**
```
Atmospheric wide-angle illustration. A suburban dead-end street in a drained desaturated
post-apocalyptic world. Cracked asphalt — grey, weed-broken, with fine cracks like a dried
cardboard sheet. Dead-end curb: concrete grey, slightly crumbling. House facades along the sides:
sun-bleached, peeling paint, window frames bare and grey-brown. Pale grey sky — no blue, no
warmth. All props desaturated dust tones: a leaning lamp post, a rusted fire hydrant, a toppled
mailbox. Corrugated cardboard grain visible on wooden surfaces. Thick dark brown marker outlines
(#3D2B1F). Flat cel-shaded. No green, no warm color. The street feels empty and stopped — like a
photograph of a world mid-exhale. Grey concrete birdbath in the mid-distance — glowing element
is absent in this state, just a plain stone basin. Tearaway art style. No characters. No text.
```

**Negative prompt:** photorealism, warm colors, green vegetation, vivid palette, neon, glowing birdbath, wild west elements, watermark

**Acceptance criteria:**
- Atmosphere communicates the zone is unreclaimed — grey, still, no imagination present
- The birdbath reads as a plain stone object with no glow, no wildflowers — the absence is felt
- The cracked asphalt texture reads as corrugated or dried-cardboard material, not photorealistic concrete

---

## 5. Character Concept Renders

These are 2D front-facing illustration prompts used as creative reference alongside Meshy 3D generation. Feed Meshy screenshots into Weave as reference images when available — the 3D silhouette anchors the AI's interpretation of proportions.

---

### The Kid — Ninja Box

**Use:** Character reference sheet — front-facing illustration. `ref_chr_kid_ninja_front.png`
**Model recommendation:** Recraft V3 (clean graphic character illustration)
**Output dimensions:** 1024 × 1024 px (square reference)

**Prompt**
```
Front-facing character concept illustration, square format. A child warrior — roughly 8-12 years
old — wearing a cardboard box as a ninja mask and head covering. Chibi-adjacent proportions:
box head roughly 35% of total height, compact torso 30%, short legs 35%. The box is a perfect cube,
slightly oversized relative to the body — clearly a cardboard box, not a helmet. Hand-drawn navy
marker ninja eye-slits on the front face, slightly wobbly imperfect lines. Corrugated cardboard grain
on the box surface, kraft-brown base tone with dark navy (#1A1A2E) paint wash. Purple marker accent
lines on the box corners. Wrapped cloth outfit: dark navy and purple, marker-drawn forearm wrappings
in dark brown, simple flat-soled sneakers. Slight dynamic lean — weight on one foot, relaxed confident
stance. Warm craft tan background (#F5EDD6) — flat, no gradient. Thick dark brown marker outlines
(#3D2B1F) on all edges. Flat cel-shaded. No weapons. The box head is the identity — it should be
visually dominant and read immediately as a box at any scale. Tearaway meets Psychonauts style.
```

**Negative prompt:** photorealism, smooth helmet, realistic fabric, realistic skin, thin proportions, tall adult anatomy, weapons, background clutter, neon, watermark

**Model recommendation:** Recraft V3
**Acceptance criteria:**
- Box head reads immediately as a cardboard box — cubic, not rounded or helmet-like
- Chibi proportions are clearly exaggerated vs. realistic anatomy — this is not a tall character
- Marker-line details on the box face are visibly hand-drawn (slight wobble, not geometric perfection)
- Character passes the silhouette test: shown as a solid black shape, the cube-on-compact-body signature is instant and unique

---

### The Kid — Cowboy Box

**Use:** Character reference sheet — Kid in Cowboy Box form. `ref_chr_kid_cowboy_front.png`
**Model recommendation:** Recraft V3
**Output dimensions:** 1024 × 1024 px

**Prompt**
```
Front-facing character concept illustration, square format. A child warrior — roughly 8-12 years
old — wearing a cardboard box decorated as a cowboy hat as a head covering. The box is a cube with
an exaggerated wide brim drawn on all four sides in thick marker — the brim extends significantly
beyond the box's cube footprint, making the hat brim the dominant silhouette feature. The top of
the box is flat. Marker-drawn star badge on the front face. Leather brown (#8B5E2A) paint wash with
gold marker accent lines (#F7C244). Corrugated cardboard grain visible. Same chibi-adjacent body
proportions as the Ninja Box form: compact torso, short legs. Western outfit: denim jacket with
marker-drawn seam lines, simple boots, a rope loop hanging at the hip. No gun. Warm craft tan
background (#F5EDD6), flat. Thick dark brown marker outlines (#3D2B1F). Flat cel-shaded. Earnest
confident stance — same kid, different box. Tearaway and Psychonauts style.
```

**Negative prompt:** photorealism, gun or holster with gun, realistic leather, smooth surfaces, tall adult anatomy, realistic hat, thin proportions, neon, watermark

**Acceptance criteria:**
- The wide hat brim must be visibly dominant — if shown as a solid black shape, the brim makes this character's silhouette clearly different from the Ninja Box form
- Brim extends past the cube on all visible sides — not just drawn on the front face
- Same body proportions as the Ninja Box reference — these are clearly the same character in different headgear

---

### Female Ninja (NinjaFemale)

**Use:** Character reference sheet — the Phase 2 playable character. `ref_chr_ninjaF_front.png`
**Model recommendation:** Recraft V3
**Output dimensions:** 1024 × 1024 px

**Prompt**
```
Front-facing character concept illustration, square format. A female child warrior — roughly 8-12
years old — wearing a taller, slightly narrower cardboard box as a ninja mask. The box is taller
than wide (rectangular, not a perfect cube), making her silhouette taller and more vertical than
the Kid's square box. Hand-drawn mask slits in dark navy marker. Dark navy box (#1A1A2E) with purple
marker accent lines. Slightly slimmer chibi proportions than the Kid — same age, different build.
Ninja outfit: dark navy with purple accents, marker-drawn belt and forearm wrappings. Her posture is
alert and precise — slightly more upright and measured than the Kid's relaxed lean. Matching sneakers.
No weapons. Warm craft tan background (#F5EDD6), flat. Thick dark brown marker outlines (#3D2B1F).
Flat cel-shaded corrugated cardboard grain on the box. Her silhouette must be distinct from the Kid:
taller rectangular box vs. square box — the two should be identifiable as different characters
when shown as solid black shapes. Tearaway and Psychonauts style.
```

**Negative prompt:** photorealism, adult female anatomy, tall adult proportions, realistic fabric, smooth surfaces, weapons in hand, neon, watermark

**Acceptance criteria:**
- Box is visibly taller than wide (rectangular, not cubic) — this is the key silhouette differentiator from the Kid
- Posture reads as more precise and controlled than the Kid's relaxed stance
- Shown as a solid black shape, this character is immediately distinct from the Ninja Box Kid form

---

### Cowgirl

**Use:** Character reference sheet — the Cowgirl playable character. `ref_chr_cowgirl_front.png`
**Model recommendation:** Recraft V3
**Output dimensions:** 1024 × 1024 px

**Prompt**
```
Front-facing character concept illustration, square format. A female child warrior — roughly 8-12
years old — wearing a cardboard box decorated as a wide-brimmed cowgirl hat. The brim is exaggerated
and wide — extends significantly beyond the cube's footprint on both sides. A hand-drawn ribbon line
runs around the hat at brim-level in marker gold (#F7C244). Leather brown box (#8B5E2A) with gold
marker accent lines and a small marker-drawn flower on the front face. The brim droops very slightly
on both sides — not rigid, slightly soft like real hat brim — giving her a gentler energy than the
Kid's flat-topped cowboy brim. Chibi-adjacent proportions. Western outfit: vest in warm tan,
marker-drawn fringe lines on the jacket hem, simple boots with marker-drawn stitching. A lasso loop
coiled at her hip. Warm craft tan background (#F5EDD6), flat. Thick dark brown marker outlines
(#3D2B1F). Flat cel-shaded. Confident, warm stance — feet apart, lasso hand relaxed at side.
Tearaway and Psychonauts style. No gun.
```

**Negative prompt:** photorealism, adult female anatomy, gun, realistic leather, smooth surfaces, thin anime proportions, neon, watermark

**Acceptance criteria:**
- Wide drooping brim is the dominant silhouette feature — unmistakable as a solid black shape
- The slight brim droop distinguishes her from the Kid's stiffer cowboy brim — same hat archetype, different character energy
- Lasso coil at the hip adds an extra silhouette attachment that breaks the simple humanoid outline

---

## 6. Cutscene Panels

Static 2D panels for key story moments. These are used as storyboard reference and optionally as in-engine static cutscene frames. Runway Gen-3 is recommended for cinematic panels — its painterly quality adds weight to story moments. Each panel is composed for 16:9 landscape.

---

### Panel 1 — The Great Hush (Inciting Event)

**Story moment:** The day the Elders cut the Internet. The world goes quiet. Screens go dark. People stop creating.
**Use:** Opening cinematic / title sequence panel — `cut_panel_thegreat_hush.png`
**Model recommendation:** Runway Gen-3
**Output dimensions:** 1920 × 1080 px

**Prompt**
```
Cinematic wide illustration panel, 16:9. The moment the world went silent. A suburban street scene
at dusk — warm orange streetlight beginning to glow. Multiple houses visible in a row, each with
lit windows. At the same instant in every window: the warm blue-white glow of screens flickers and
goes dark — replaced by cold black rectangles. On the street, people mid-step stop and look up or
look at their now-dark phone screens. The mood shift is visible in the color: the left side of the
frame holds the last warm screen glow (#F5A623 amber), the right side has gone cold and grey
(#A89F94). This is not violent. It is simply — quiet. The cardboard-and-marker aesthetic applies:
people have slightly simplified chibi-adjacent proportions, marker outlines on all edges, flat
cel-shaded surfaces. Street details: marker-drawn pavement cracks, lamp post with thick outlines.
The frame is wide and cinematic — small figures against big quiet street. Tearaway art style,
painterly atmosphere. No text. No UI.
```

**Negative prompt:** photorealism, explosion, violence, dark threatening mood, neon, smooth surfaces, realistic faces, watermark

**Acceptance criteria:**
- The screen-going-dark moment is clear — visible bright-to-dark transition in the windows
- The color temperature shift from left (warm) to right (cold) is legible as a compositional idea
- People read as slightly simplified, craft-aesthetic figures — not photorealistic humans

---

### Panel 2 — Kid Puts on the Box

**Story moment:** The Kid finds a cardboard box in the backyard, draws a ninja mask on it, and puts it on their head. The world doesn't change. They decide it does anyway.
**Use:** Act 1 story panel — `cut_panel_box_on_head.png`
**Model recommendation:** Runway Gen-3
**Output dimensions:** 1920 × 1080 px

**Prompt**
```
Cinematic wide illustration panel, 16:9. A child in a drained grey suburban backyard kneels
beside an overturned cardboard box. The child — chibi-adjacent proportions, scruffy jeans and
sneakers — has just drawn crude ninja eye-slits on the box in dark navy marker. The marker pen
is still in their hand. They look at the box with total, absolute conviction — no irony, no hesitation.
The grey backyard surrounds them: dead grass, broken fence, bare apple tree. But a small warm glow
emanates from the box itself — craft tan (#E8C97A) warm light spilling onto the ground and the child's
face, the only warmth in an otherwise grey frame. This is the moment imagination enters the world.
Low camera angle — shot from ground level, looking slightly up. The box is slightly larger than
life-proportioned to the child's body. Thick dark brown marker outlines. Flat cel-shaded. Small
human figure, vast quiet grey world around them. Tearaway art style. No text. No UI.
```

**Negative prompt:** photorealism, ironic expression, multiple characters, happy colorful world (the world is still grey here), neon, smooth surfaces, realistic child anatomy, watermark

**Acceptance criteria:**
- The warm glow from the box is the only saturated warm tone in the frame — everything else is the drained palette
- The child's expression reads as conviction, not surprise or humor
- The box is visibly the same craft-material aesthetic as the rest of the game's world — corrugated cardboard, marker-drawn mask

---

### Panel 3 — World Tree Reveal

**Story moment:** The Kid first sees the World Tree from a distance. It is vast, withered, surrounded by The Unimaginative's structures. The destination is set.
**Use:** Act 1 story panel — `cut_panel_worldtree_reveal.png`
**Model recommendation:** Runway Gen-3 or Google Imagen 3 (for precise composition control)
**Output dimensions:** 1920 × 1080 px

**Prompt**
```
Cinematic wide illustration panel, 16:9. A child warrior in a cardboard-box ninja outfit stands
at the crest of a hill, seen from behind, looking down at a vast grey landscape. In the far distance,
the World Tree — a massive structure made of stacked compressed corrugated cardboard rings, its bare
branches reaching across a quarter of the upper frame, its base obscured by low grey haze. Around
the tree's base: blocky uniform grey structures belonging to The Unimaginative — featureless concrete-
grey buildings, no marker-drawn detail, deliberately blank and architectural. The child is small —
occupies no more than 10% of frame height — silhouetted against the grey sky at left-center of frame.
The Tree is the destination. Low horizon, wide sky, the World Tree is the vertical hero element.
Drained desaturated palette throughout. Thick dark brown marker outlines on the player figure and
the Tree. The Unimaginative structures have clean architectural lines — marker-less, plain, dull by
design. Vast scale, quiet determination. Tearaway art style, painterly atmosphere. No text. No UI.
```

**Negative prompt:** photorealism, bright colors, vivid sky, neon, characters facing forward, happy mood, watermark

**Acceptance criteria:**
- Scale is legible — the World Tree is enormous, the player figure is tiny
- The Unimaginative structures are deliberately blank and architecturally uniform — they contrast with the craft-material texture of everything else
- The player's silhouette is recognizable as the box-ninja form even at that small size

---

### Panel 4 — Ending (Reclaimed World)

**Story moment:** The World Tree restored. The world blooms. The Kid removes the box and looks at it — then puts it back on.
**Use:** Ending sequence panel — `cut_panel_ending_restored.png`
**Model recommendation:** Recraft V3 (vivid color clarity for the payoff moment)
**Output dimensions:** 1920 × 1080 px

**Prompt**
```
Cinematic wide illustration panel, 16:9. The World Tree in full vivid restoration — massive, alive,
commanding the frame. Its corrugated cardboard trunk is warm rich brown, cardboard leaf clusters
in vivid marker green (#5CB85C) fill the branches. The sky is full saturated marker blue (#4A90D9).
The ground is warm kraft tan (#E8C97A). A child warrior stands at the tree's base, this time facing
the viewer — small against the enormous tree, box on their head, one hand raised slightly. Paper
butterflies in marker orange and purple drift past the mid-frame. Confetti of gold, orange, and purple
paper pieces settles in arcs around the player. The tree's bark shows kraft tape repair strips —
it was mended as much as restored, the damage still legible in the material. The mood is earned joy —
not triumphant fanfare, but a deep warm quiet satisfaction. Thick dark brown marker outlines.
Flat cel-shaded corrugated cardboard surfaces. Tearaway meets Scott Pilgrim. No text. No UI.
```

**Negative prompt:** photorealism, grey, drained palette, neon, dark mood, sad expression, watermark

**Acceptance criteria:**
- The kraft tape repair strips are visible on the tree's trunk — the tree is restored, not perfect
- Paper butterflies read as flat cardboard cutout shapes, not photorealistic insects
- The player facing the viewer for the first time in the cutscene sequence feels like a choice — their posture reads as settled and calm, not triumphant

---

## 7. Marketing and Social Media Art

Wide cinematic prompts for promotional use. These images are output at high resolution and do not need to match UI safe zones or leave overlay space. They stand alone as images. Recraft V3 or Stable Diffusion XL recommended for sharpness. Export at 4K (3840 × 2160 px) for print and scaled-down for social.

---

### Hero Image / Key Art

**Use:** App Store banner, Play Store feature graphic, primary marketing image — `mkt_key_art_hero.png`
**Model recommendation:** Recraft V3 (graphic clarity, saturated colors for store thumbnails)
**Output dimensions:** 3840 × 2160 px

**Prompt**
```
Cinematic wide key art illustration, 16:9. The hero image for Unboxed Heroes. Center-frame: the
Kid in cardboard-box ninja outfit stands in a confident power pose — chest forward, weight on one foot,
one arm slightly raised. The World Tree fills the background, alive and vivid in its Reclaimed state:
warm rich brown corrugated trunk, vivid marker-green cardboard leaf clusters, rising to the top of
frame. The sky is full saturated marker blue (#4A90D9). Ground: warm kraft tan (#E8C97A). The player
character is in full Reclaimed world vivid color: dark navy box head with purple marker lines, purple-
navy outfit. Flanking the player in the background, partially visible at the edges of frame: the Cowgirl
(leather brown wide-brimmed box, gold marker lines) on the left, the Female Ninja (dark navy taller
rectangular box) on the right — both slightly out of focus to keep the Kid as the clear hero.
Paper confetti of gold, orange, and purple floats in arcs. Thick dark brown marker outlines (#3D2B1F)
on all characters and major elements. Flat cel-shaded surfaces. Corrugated cardboard texture on all
boxes. Strong silhouettes — every character reads as a solid black shape from distance. Vivid, energetic,
handcrafted. Tearaway meets Psychonauts meets Scott Pilgrim. No text. No UI.
```

**Negative prompt:** photorealism, grey drained world, dark tone, neon, smooth plastic, realistic anatomy, watermark, enemy characters visible, abstract background

**Acceptance criteria:**
- The Kid is the clear primary subject — centered and the largest figure
- All three character silhouettes (Kid, Cowgirl, NinjaFemale) pass the silhouette test — recognizable as separate characters as black shapes
- The World Tree establishes the game world without overwhelming the characters
- Vivid palette reads immediately in thumbnail — this is an App Store image that must pop at 250px wide

---

### Boss Encounter Key Art

**Use:** Social media, announcement art, devlog header — `mkt_key_art_boss.png`
**Model recommendation:** Stable Diffusion XL (dramatic tension, atmospheric depth)
**Output dimensions:** 3840 × 2160 px

**Prompt**
```
Dramatic cinematic illustration, 16:9. A tense standoff: the Kid in cardboard-box ninja outfit
faces an enormous anthropomorphic washing machine boss — SpinCycle. The player figure stands
in the left foreground, small, in fighting stance, feet planted, weight forward. SpinCycle looms
in the right center and background — 4 meters tall, front-loading drum as its head (porthole
with hand-drawn angry marker eyes), corrugated cardboard panel body, scrunched aluminum foil
trim catching cold grey light, impossibly wide shoulders. The machine's drum porthole glows a
warm amber-orange from within — the only warm light source. The world is drained and grey:
cracked grey cardboard earth, pale overcast sky. Cold grey ambient light everywhere except the
drum glow. The size contrast between the small ninja warrior and the enormous machine is the
point of the image — the hero is outnumbered by scale. Thick dark brown marker outlines on all
edges. Flat cel-shaded corrugated and foil textures. Drained desaturated palette except the drum's
warm interior glow. Tearaway meets Scott Pilgrim. Heavy tension. No text. No UI.
```

**Negative prompt:** photorealism, friendly boss expression, bright world, small boss, smooth plastic machine, clean laundry room, equal character sizes, neon, watermark

**Acceptance criteria:**
- Scale contrast between player and boss is extreme and immediately legible
- The drum's amber glow is the only warm tone in the frame — it draws the eye to the boss's "face"
- SpinCycle's silhouette passes the test: circular drum head on wide shoulders is a unique profile that would be recognizable as a solid black shape

---

### App Store Icon Background (Square Variant)

**Use:** App icon background layer — `mkt_icon_bg.png`. The Kid character renders on top of this.
**Model recommendation:** Recraft V3
**Output dimensions:** 1024 × 1024 px (square)

**Prompt**
```
Square format illustration for a mobile game app icon background. A vivid flat graphic backdrop.
Dominant color: saturated marker blue (#4A90D9) in the upper two-thirds — clean, graphic, no
gradient. Bottom third: warm kraft tan (#E8C97A) ground plane, with a very subtle corrugated
cardboard grain texture. A small segment of the World Tree trunk visible at the right edge —
just the warm brown corrugated bark and a few marker-green leaf clusters, partially cropped.
Paper confetti pieces — orange, purple, gold — scatter in arcs across the upper field. Thick
dark brown marker outlines (#3D2B1F) on all elements. Flat cel-shaded. The center of the square
is visually open — the character portrait will be placed here; do not put any foreground elements
in the center 40% of the square. Vivid, graphic, immediately readable at very small sizes (60x60pt).
Tearaway style graphic.
```

**Negative prompt:** photorealism, grey, drained palette, dark background, cluttered center, neon, text, watermark

**Acceptance criteria:**
- Center 40% of the square is visually clear — no strong color blocking or foreground elements
- Reads as a coherent graphic image at 60×60pt (phone home screen size)
- The blue and kraft tan zones are strongly differentiated — a clean horizon line between them

---

## 8. Weave-Specific Workflow Tips

### Iteration with Inpainting

Inpainting is the most powerful Weave tool for controlled iteration. Use it when a generated image is mostly right but has a specific failure region. Workflow:

1. Generate the base image at the full resolution spec.
2. In Weave, pipe the output to an Inpaint node.
3. Mask only the failing region — be conservative, mask as little as possible.
4. Write a focused inpaint prompt describing what should be in that region (not the full image prompt — just the local area).
5. Run 2–3 inpaint variations and composite the best one back into the base image.

Common inpaint use cases for Unboxed Heroes:
- Fixing the World Tree silhouette without regenerating the whole landscape
- Replacing a flat or photorealistic texture region with correct corrugated cardboard grain
- Swapping a foreground element (wrong debris, wrong prop type)
- Cleaning up the protected overlay zones (top 20% sky, bottom 15% vignette) without affecting the main illustration

### Relighting for World State Variants

When you have a strong composition in the Reclaimed (vivid) state and need the Drained (grey) variant:

1. Pipe the Reclaimed image into the Relight node.
2. Set the light color to cool grey-white (`#D4CFC9`) — no warm tones.
3. Lower the light intensity to flatten the mood.
4. Apply a desaturation pass via the Color Grading node: Saturation -80, Contrast +10, Color Temperature toward grey.
5. Review: the corrugated texture grain and marker outlines should still be visible — if they are lost, pull back the desaturation to -65.

This produces a Drained variant that is compositionally identical to the Reclaimed version — useful for before/after zone reveal comparisons and for any two-panel marketing layout.

### Feeding Meshy Renders into Weave

This is the fastest concept art pipeline:

1. Generate a Meshy 3D model (character or enemy).
2. Take a screenshot from Meshy's preview or from Unity's scene view (front-facing or 3/4 angle, grey or white background).
3. In Weave, use the screenshot as a Reference Image input on a Generate Image node.
4. Write a 2D illustration prompt describing the character in the Tearaway-meets-Psychonauts style.
5. The AI uses the 3D model's silhouette and proportions as a structural anchor while generating a 2D illustration pass.

This workflow avoids the silhouette drift that happens when 2D character reference art is generated independently from the 3D model. The Meshy screenshot pins the shape, Weave applies the style. Particularly effective for enemy concept sheets where the Meshy model already defines a strong silhouette (SpinCycle, Sprinkler Sentinel).

### Upscaling Before Final Export

All images that will be imported into Unity or delivered as marketing assets should pass through the Upscale node before export:

1. Generate at the native prompt resolution (typically 2x smaller than the delivery spec).
2. Pipe into Upscale — use the 2x mode.
3. Review the upscaled output: check that corrugated cardboard grain has not become blurry (the most common failure). If it has, use Sharpen post-process at 15–20% to restore grain.
4. Export the final upscaled PNG.

Do not upscale more than 2x in a single pass — beyond 2x, AI upscalers generate speculative detail that diverges from the base image's material aesthetic and introduces texture inconsistencies.

### Consistency Across a Session

When generating multiple related images in one session (e.g., all four cutscene panels), keep the style vocabulary identical across all prompts. Use the same phrase block at the end of every prompt:

```
Thick dark brown marker outlines (#3D2B1F). Flat cel-shaded. Corrugated cardboard texture.
Tearaway art style. No text. No UI.
```

If a generated image drifts from the aesthetic (too photorealistic, too flat-cartoon, wrong palette), use this diagnosis sequence before regenerating:

1. Is the model backend the same as the other images? Switch back if not.
2. Is the style phrase block present and identical? Paste it in if missing.
3. Add the phrase "consistent with Tearaway game art style" explicitly at the start of the prompt.
4. Reduce the resolution by 50% and generate a quick test first — iterate faster, then upscale the accepted result.

---

*Weave AI Prompts v1.0 — 2026-07-23*
*Consistent with Art Style Guide v1.1, Loading Screen Art Direction v1.0, and PixVerse Asset Document v1.0.*
*Weave is for 2D output only — all 3D model generation routes through Meshy.*
