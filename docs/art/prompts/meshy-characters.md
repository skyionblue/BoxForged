# BoxForged — Meshy Character & Enemy Prompts

**Purpose:** AI-generated 3D models for character and enemy assets.
**Tool:** Meshy (text-to-3D)
**Art direction:** Chunky stylized 3D, cardboard-and-marker aesthetic. Think Tearaway meets Psychonauts — not photorealistic, not flat cartoon. Surfaces have visible corrugation or grain. Details look hand-drawn with marker strokes.
**Poly budget:** Characters 1,500–3,000 triangles. Enemies 800–1,500 triangles. (Mobile target.)
**Texture size:** 1024×1024 for characters, 512×512 for enemies. Diffuse + normal map.

---

## How to Use These Prompts

Each character has one or more prompts depending on whether Meshy generates the rig separately from animations.

**After generating in Meshy:**
1. Export as `.fbx` with textures (enable "Export with textures" before downloading)
2. Place zip in `boxhead/models/zips/`
3. Extract to `Assets/_Project/Models/Characters/<CharacterName>/`
4. Set `globalScale = 1`, `useFileScale = true` on all FBXes
5. Extract textures to `Assets/_Project/Models/Characters/<CharacterName>/Textures/`
6. Assign `_BaseMap`, `_BumpMap`, `_MetallicGlossMap` on the URP/Lit material

**For rigged bipeds:** Use Meshy's "Rig & Animate" feature — request Idle, Walk, Run, Hit Reaction at minimum. Export each animation as a separate FBX with skin.

**For non-biped enemies** (like Sprinkler Sentinel): Export as a static mesh only — Unity will drive rotation and state via script, no animation rig needed.

---

## Player Character

---

### Kid (App Icon Hero Shot)

**Concept:** The player character — a child warrior wearing a cardboard box as a ninja mask. This version is a hero pose optimized for the app icon, not the gameplay character.
**Type:** Static mesh — render-only, no rig or animation needed
**Priority:** ⭐⭐⭐ HIGH (required for App Store / Play Store submission)

**Silhouette signature:** Perfect-cube box head (roughly one-third of total height) on a compact chibi body. The square head is the identity marker — must read at any size.

**Meshy Prompt**
```
Heroic child warrior wearing a cardboard box as a ninja mask. Stylized chunky 3D game character.
URP mobile game asset. Strong distinct silhouette: perfect cube box head dominates upper third,
compact chibi body with short legs and wide torso. Clear readable shape at small size. Exaggerated
proportions. Confident power pose — chest forward, arms slightly out, weight on one foot. Box head
hand-drawn ninja eye-slits in dark navy marker, wobbly imperfect lines. Corrugated cardboard grain,
kraft-brown tone, dark navy paint, bold purple marker accent lines. Wrapped cloth outfit dark navy
and purple, marker-drawn forearm wrappings, simple footwear. No weapons in hand.
Tearaway, Psychonauts style. Thick marker outlines.
Not photorealistic. Not smooth plastic. Not flat cartoon.
```

**Negative prompt:** realistic skin, photorealistic textures, smooth PBR surfaces, thin proportions, tall anatomy, complex accessories, weapons, background clutter, realistic fabric folds

**Acceptance criteria:**
- Silhouette test: when shown as a solid black shape at 150px tall (10% of 1080p), the square box head is immediately identifiable and distinct from all enemies
- Box head must be visibly cubic — not rounded, not oval — at small render sizes
- Body reads as distinctly shorter and stockier than a normal human proportion

**Aspect ratio:** 1:1 | **Poly budget:** 1,500–2,500 triangles

**Render notes (for the icon shot):**
- **Camera:** 30–40° above eye level, slightly tilted — box head dominates the upper half of the frame
- **Framing:** Character fills 80% of canvas height; feet can be cropped at ankle
- **Key light:** Warm white `#FFF5E0` at 45° upper-left; soft fill at 20% from right, no rim light
- **Background:** Flat solid — marker blue `#4A90D9` or amber-orange `#F5A623`. No gradient, no texture
- **Outline pass:** 2–3px dark brown `#3D2B1F` — mandatory for 60×60pt home screen silhouette read
- **No depth of field** — entire character must be in sharp focus

**What NOT to include:** Secondary characters, weapons, props, background details, or text. Only the box head and heroic pose need to read at small sizes.

**Unity filename:** `chr_player_ninja_icon.fbx` (render-only — no colliders, rigs, or animation clips)
**Target folder:** `Assets/_Project/Models/Characters/PlayerNinja/`
**Output size:** Render to 1024×1024 PNG (no alpha) for App Store; 512×512 PNG for Play Store

---

## Logo / Brand Mascot

---

### Mascot — Full Hero Image

**Concept:** The BoxForged brand mascot. A compact chibi child warrior wearing a cardboard box as a helmet/mask, holding a cardboard sword raised in a triumphant hero pose. This is not the gameplay character — it is the logo-facing version, optimized for website hero images, social media headers, and marketing renders. Built once, used everywhere brand presence is needed.
**Type:** Static mesh — render-only, no rig or animation needed
**Priority:** HIGH (required before any public-facing web or social presence)

**Silhouette signature:** Perfect-cube box head (one-third of total height) on a wide, stocky chibi body. Raised cardboard sword extends above the head, adding vertical drama. The box-head + upward sword combo creates an unmistakable cross-shape silhouette — heroic, readable at any size.

**Color palette (required):**
- Box head: craft tan `#E8C97A` corrugated cardboard, dark brown `#3D2B1F` outlines
- Marker eye-slits: marker blue `#4A90D9`
- Clothing accents: marker red `#E05A4E`
- Sword: craft tan cardboard blade, gold `#F7C244` marker-drawn edge stripe
- Outline and shadow: dark brown `#3D2B1F`

**Meshy Prompt**
```
Heroic chibi child warrior, stylized chunky 3D game character, URP mobile game asset.
Strong distinct silhouette: perfect cube cardboard box worn as a helmet-mask, cube occupies
upper third of total height. Compact wide chibi body — short legs, broad torso, exaggerated
proportions. Clear readable shape at small size. Triumphant hero pose: chest forward, one arm
raised high holding a cardboard sword above the head, other arm planted confidently at side,
weight on one foot. Box helmet: corrugated cardboard grain, kraft tan surface, dark navy
marker-drawn eye-slits — wobbly imperfect hand-drawn lines. Cardboard sword: flat blade,
corrugated cardboard texture, gold marker stripe along the edge, dark brown marker outlines.
Clothing: simple outfit in muted tan and warm brown, marker-drawn detail lines on fabric.
Craft tan, marker blue, marker red, gold, dark brown outlines. Tearaway, Psychonauts style.
Thick marker outlines on all edges. Warm, hopeful, playful energy — not grimdark.
Not photorealistic. Not smooth plastic. Not flat cartoon. No background.
```

**Negative prompt:** realistic skin, photorealistic textures, smooth PBR surfaces, thin proportions, tall anatomy, complex armor, grimacing expression, dark lighting, horror tone, grimdark mood, background clutter, other characters

**Acceptance criteria:**
- Silhouette test: when shown as a solid black shape at 150px tall, the cube box head and raised sword are immediately identifiable and read as heroic
- Box head must be visibly cubic — not rounded, not oval — at small render sizes
- Sword must be clearly cardboard (corrugated texture), not metallic or realistic
- Overall mood: warm and triumphant, not aggressive or scary

**Aspect ratio:** 1:1 for renders | **Poly budget:** 1,500–2,500 triangles

**Render notes (for website and social):**
- **Camera:** 20–30° above eye level, centered — box head and raised sword both in frame
- **Framing:** Character fills 75–80% of canvas height; feet visible or cropped at ankles
- **Key light:** Warm white `#FFF5E0` at 45° upper-left; soft fill at 25% from right; optional warm rim light from below at 10% to lift the feet
- **Background options:**
  - Website hero: flat marker blue `#4A90D9` — solid, no gradient, no texture
  - Social media: flat craft tan `#E8C97A` — warm and on-brand
  - Transparent PNG: export with alpha for flexible placement
- **Outline pass:** 2–3px dark brown `#3D2B1F` — mandatory
- **No depth of field** — entire character sharp

**Unity filename:** `chr_brand_mascot_hero.fbx` (render-only — no colliders, rigs, or animation clips)
**Target folder:** `Assets/_Project/Models/Characters/BrandMascot/`
**Output sizes:** 2048×2048 PNG (transparent alpha) for web; 1200×1200 PNG for social

---

### Mascot — Icon Version

**Concept:** A simplified, high-contrast version of the brand mascot designed for small-format use: favicon (32×32px), navigation logo (48px height), app badge, and watermark. The design is stripped back to pure silhouette reads — the box head and sword are the only elements that need to survive at tiny sizes.
**Type:** Static mesh — render-only, optimized for square format small-size rendering
**Priority:** HIGH (favicon and nav logo needed before site launch)

**Design constraint:** At 32×32px, only two things can read: the cube box shape and the sword. Everything else — clothing detail, fabric folds, marker line work — disappears. The prompt must produce a model where those two elements have extreme shape contrast against each other and the background.

**Silhouette signature:** Near-symmetrical cube on a compact body. Sword held straight up, perfectly vertical, centered above the head. The result is a clean vertical stack — sword tip, box head, body — that reads as a single strong shape even when reduced to favicon size.

**Meshy Prompt**
```
Compact chibi warrior icon figure, stylized chunky 3D, URP mobile game asset. Strong distinct
silhouette optimized for small icon sizes. Perfect cube cardboard box as head-helmet, occupying
one-third of total height — cube face forward, flat sides clearly readable. Sword held straight
up in both hands, blade vertical, centered above the box head — creates a clean vertical
silhouette: sword tip, cube head, compact body stacked top to bottom. Body: wide and minimal,
no fussy detail, broad shoulders, short legs planted firmly. Near-symmetrical pose — both
hands on sword hilt, elbows slightly out. Corrugated cardboard grain on box and sword blade.
Dark brown marker outlines, bold and thick. Craft tan cardboard, gold sword edge stripe.
Silhouette must be instantly readable as a clean bold shape at 32 pixels tall.
Not photorealistic. Not smooth. No background. Minimal surface detail.
```

**Negative prompt:** asymmetric pose, raised single arm, fine surface detail, thin outlines, realistic textures, smooth plastic surfaces, complex clothing, background, other characters, accessories beyond sword

**Acceptance criteria:**
- Silhouette test: when shown as a solid black shape at 32px tall, the cube head and vertical sword read as a single clean bold shape — no detail required, shape alone is enough
- The sword must be strictly vertical — no diagonal angle that would complicate the favicon read
- Body width should be close to box head width — compact and blocky, not hourglass

**Aspect ratio:** 1:1 (square — required for favicon use) | **Poly budget:** 800–1,200 triangles

**Render notes (for favicon and nav logo):**
- **Camera:** Dead-on front view, no angle — perfectly orthographic or very low FOV (5–10°) to minimize perspective distortion
- **Framing:** Character fills 85–90% of canvas height; tight crop, no breathing room wasted
- **Key light:** Flat front lighting — no dramatic shadows that obscure the silhouette at small sizes
- **Background:**
  - Favicon: marker blue `#4A90D9` rounded square (apply in design tool after render)
  - Nav logo: transparent PNG — place on whatever background the site uses
- **Outline pass:** 3–4px dark brown `#3D2B1F` — heavier than the hero version to survive downscaling
- **Downscale test:** After rendering, manually scale the PNG to 64×64, 32×32, and 16×16 and verify the shape reads at all three sizes

**Unity filename:** `chr_brand_mascot_icon.fbx` (render-only)
**Target folder:** `Assets/_Project/Models/Characters/BrandMascot/`
**Output sizes:** 512×512 PNG (transparent alpha) — design tool handles downscaling to favicon sizes

---

### Mascot Silhouette Entry (Shape Signature Table Update)

Add to the shape signature table in the General Meshy Tips section:

| Mascot (Hero) | Cube box head + raised sword overhead — cross silhouette |
| Mascot (Icon) | Cube box head + vertical sword centered — stacked pillar silhouette |

---

## Phase 1 Enemies

---

### Sprinkler Sentinel

**Concept:** A garden sprinkler imagined as a rotating turret guardian. Holds ground, sweeps the area with water bursts. The center eye glows and is the weak point.
**Type:** Non-biped prop/enemy — static mesh, no rig needed
**Priority:** ⭐⭐⭐ HIGH (Phase 1, Room 2 enemy)

**Silhouette signature:** Four arms spread wide like a compass rose + squat cylindrical body on a spike base. Must read as a distinct cross-or-star shape at 10% screen size.

**Meshy Prompt — Base Model**
```
Low-poly stylized 3D enemy for a mobile action game. URP mobile game asset.
Garden sprinkler transformed into an armored turret sentinel. Strong distinct silhouette:
squat cylindrical brass body, single large glowing blue eye on front face, four chunky pipe
arms extending outward symmetrically like a compass rose, each curving down to a wide nozzle
tip. Thick conical spike base anchors it to the ground. Clear readable shape at small size —
arms spread wide, body compact. Exaggerated proportions. Worn brass and copper, green verdigris
patina, corrugated texture, hand-drawn marker panel lines. Brass gold, copper orange, verdigris
green, glowing blue eye. Psychonauts style. No background. Symmetrical rest pose.
Not photorealistic. Not smooth. No organic shapes. No legs or feet.
```

**Acceptance criteria:**
- Silhouette test: when shown as a solid black shape at 150px tall (10% of 1080p), the four-arm compass-rose outline is immediately recognizable and distinct from all biped enemies
- No arm can be hidden behind the body in the rest pose — spread must be fully visible from the front camera angle
- The spike base reads as a pointed anchor, not a flat disc

**Meshy Prompt — Overheated State (optional variant)**
```
Same low-poly stylized brass sprinkler sentinel as before.
In this variant: the glowing blue eye is now bright white-yellow, slightly pulsing.
Steam vents from the nozzle tips. Small orange heat glow around the base of each arm.
Everything else identical — same chunky brass body, same four arms, same ground spike.
```

**Unity filename:** `enemy_sprinkler_sentinel.fbx`
**Target folder:** `Assets/_Project/Models/Characters/SprinkerSentinel/`

---

## Phase 1 Boss

---

### SpinCycle

**Concept:** A washing machine drum-headed heavyweight brawler. The front-loader drum rotates constantly. The glass porthole window is the parry weak point.
**Type:** Rigged biped — request full animation set from Meshy
**Priority:** ⭐⭐⭐ HIGH (Phase 1 boss)
**Status:** Model already imported from `Meshy_AI_washer_brawler_rigged_biped.zip` ✓

If a re-generation is needed:

**Silhouette signature:** Massive circular drum head on an impossibly wide-shouldered body, no neck. The drum's circular bulk makes this character unmistakable at any size — distinct from all biped enemies who have humanoid heads.

**Meshy Prompt — Character Base**
```
Low-poly stylized 3D boss character for a mobile action game. URP mobile game asset.
Strong distinct silhouette: heavyweight brawler with a front-loading washing machine drum
as his head — cylindrical, gunmetal grey, circular glass porthole on front face, ribbed metal
bands ringing the drum vertically. Drum sits directly on massively broad shoulders with no neck.
Body: extremely muscular, exaggerated proportions, chunky cartoon bulk. Torn dark shorts.
Shredded vest hanging open. Mismatched sneakers: left bright blue high-top, right dark red
high-top. Warm tan skin, slightly grimy. Hand-drawn marker lines on clothing for wear.
Clear readable shape at small size — circular drum head reads instantly. Psychonauts style.
T-pose. Clean white background. Not photorealistic. Not smooth.
```

**Acceptance criteria:**
- Silhouette test: when shown as a solid black shape at 150px tall (10% of 1080p), the circular drum head on wide shoulders is instantly distinguishable from all human-headed enemies
- Drum diameter must be visibly wider than the character's skull would be — at least 1.5x head-width of a normal character
- No neck gap — drum sits flush on shoulder line, creating a distinctive flat-top-plus-barrel profile

**Meshy Prompt — Animation Requests (if re-generating)**
Request these animations individually in Meshy's Rig & Animate:
- `Idle` — standing, drum rotating slowly
- `Walking` — slow menacing walk
- `Running` — aggressive sprint
- `Regular Jump` — stationary jump straight up and down (Drum Slam attack)
- `Run and Jump` — running leap forward (Jump Charge attack)
- `Weapon Combo 2` — two-hit haymaker combo, left then right
- `Sword Parry` — arms cross in front, blocking stance
- `Roll Dodge` — lateral roll dodge
- `Hit Reaction` — stagger backward on taking damage

**Unity filename:** `Meshy_AI_washer_brawler_rigged_biped_Character_output.fbx` + per-animation FBXes
**Target folder:** `Assets/_Project/Models/Characters/SpinCycle/` ✓ (already imported)

---

## Future Characters (Phase 2+)

These are listed for reference — do not model until Phase 2 sprint begins.

---

### The Friend (Locked — No Name Yet)

**Concept:** Kid's brother from another mother, together since kindergarten. His own box, his own way of seeing the imagined world. Equal to Kid in every way — not a sidekick.
**Type:** Rigged biped — full animation set
**Priority:** LOCKED — do not generate until Phase 2 story introduction sprint

**Notes for when this is ready:**
- Silhouette must be distinct from Kid at 10% screen size — the two cannot read as the same shape. Kid's signature is a perfect-cube box head on a compact body. The Friend needs a different box shape (taller, wider, or angled differently) or a different body silhouette (taller, stockier, or with a standout accessory) so the two are never confused when shown as solid black shapes.
- His box decoration is his own — should feel like it came from a different imagination but the same world
- Do not make him a palette swap — he needs his own visual identity
- When writing the Meshy prompt: include "strong distinct silhouette", "clear readable shape", "exaggerated proportions", and describe the specific silhouette differentiator (e.g. "taller rectangular box head" or "wide-brimmed box hat shape") upfront in the prompt

---

## General Meshy Tips for Characters

### Silhouette — Non-Negotiable Rule

Every character and enemy prompt must produce a model with a strong, distinct silhouette recognizable at 10% screen size (approximately 150px tall on a 1080p display). Mobile screens are small — players read shapes first, details second.

**10% silhouette test:** After receiving a Meshy model, view it as a solid black shape. If you cannot instantly identify which character it is from shape alone, the model fails. Regenerate with stronger shape language before importing.

**Always include these terms in every character prompt:**
- "strong distinct silhouette"
- "clear readable shape"
- "exaggerated proportions"
- "URP mobile game asset"

**Each character's shape signature must be defined before writing the prompt:**

| Character | Shape signature |
|---|---|
| Brand Mascot (Hero) | Cube box head + raised sword overhead — cross silhouette |
| Brand Mascot (Icon) | Cube box head + vertical sword centered — stacked pillar silhouette |
| Kid (Ninja) | Cube box head, compact chibi body |
| SpinCycle | Circular drum head, no neck, massively wide shoulders |
| Sprinkler Sentinel | Four-arm compass rose on spike base, no legs |
| The Friend | TBD — must differ from Kid's cube-head silhouette |

**No two characters may share the same silhouette signature.** If a new character reads the same as an existing one when shown as a black shape, redesign before prompting.

---

### Art Style Keywords

- **Keywords that help:** "stylized 3D", "low-poly game character", "Psychonauts style", "chunky proportions", "hand-crafted look", "cartoon game enemy", "mobile game asset"
- **Keywords to avoid:** "photorealistic", "hyperdetailed", "PBR materials", "realistic anatomy"
- **If the model comes out too realistic:** Add "chunky and stocky", "exaggerated proportions", "toy-like", "cardboard texture on clothing"
- **If the silhouette is still weak after generation:** Regenerate — do not accept a weak silhouette. Add "bold shapes", "high contrast between body parts", name the specific shape more explicitly in the prompt (e.g. "perfectly square cube head, exactly as wide as shoulders").

### Technical

- **Poly count:** After importing, check Unity's mesh inspector. If over budget, use Meshy's Simplify before re-exporting.
- **Texture export:** Always use "Export with textures" and download the ZIP. Re-download if textures are missing.
- **Rigged bipeds:** Export each animation as a separate FBX with skin (`_withSkin`). This matches the import workflow already established for SpinCycle and the Skeptic.

---

*Last updated: 2026-07-24 — Added Logo / Brand Mascot section (Hero Image + Icon versions)*
