# BoxForged — Meshy Weapon Prompts

**Purpose:** AI-generated 3D models for all weapon/object assets, ordered by priority.
**Tool:** Meshy (text-to-3D)
**Art direction:** Chunky stylized 3D, cardboard-and-marker aesthetic. Think Tearaway meets Psychonauts — not photorealistic, not flat cartoon. Surfaces have visible corrugation or grain. Details look hand-drawn with marker strokes.
**Poly budget:** 300–600 triangles per weapon model (mobile target).
**Texture size:** 512×512 max. Diffuse only — no normal maps in Phase 1.
**Silhouette rule (Principle #6):** Every weapon must be instantly distinguishable from every other weapon by shape alone at 10% of screen size. Each prompt below includes silhouette-first language to enforce this. See the acceptance criteria section at the bottom of this document before approving any generated model.

---

## How to Use These Prompts

Each weapon has two prompts:
- **Pickup Prop** — the real-world object lying on the ground in the Backyard (before the player picks it up)
- **Imagined Weapon** — the fantasy version the player holds and attacks with

For Phase 1, prioritize the **Imagined Weapon** model — that's what's visible during gameplay. The Pickup Prop can use a simple placeholder or be modeled afterward.

**After generating in Meshy:**
1. Export as `.glb` or `.fbx`
2. Import into Unity: `Assets/_Project/Models/Weapons/`
3. Set import scale, ensure no embedded textures need extracting
4. Name per convention: `obj_[weapon-name]_[variant].fbx`

---

## Phase 1 Weapons (Sprint 6 — Model These First)

These three weapons are locked for Phase 1 gameplay. The Cardboard Tube is also needed for the Shadow Katana combo (Ninja Box + Tube).

---

### 1. Broomstick → Bo Staff

**Tier:** 1 | **Type:** Melee, sweeping | **Priority:** ⭐⭐⭐ HIGH

**Pickup Prop — Broomstick**
```
A worn wooden broomstick, stylized and chunky, cartoon proportions, standing upright leaning against nothing.
Cardboard-and-marker art style. Slightly scuffed wood grain texture that looks hand-drawn.
Bristle head made of straw bundles, slightly askew. Warm brown tones.
Strong distinct silhouette — long thin shaft capped by a wide splayed bristle head, readable at small size.
Clean white background, game prop view, 3/4 angle.
Not photorealistic, not smooth plastic, not realistic straw detail.
```

**Imagined Weapon — Bo Staff**
```
A warrior's bo staff, stylized chunky 3D, cardboard-and-marker art style.
Smooth dark wooden staff with marker-drawn grip wrapping bands in black and white.
Slightly oversized proportions like a Psychonauts prop. Both tips have small metal-looking caps
that resemble crumpled aluminum foil. Warm brown and black color palette.
Strong distinct silhouette — long slender rod with capped tips, instantly readable as a staff at small size.
Clean white background, game weapon view, 3/4 angle.
Not photorealistic, not too thin or realistic, no complex detail.
```

**Unity filename:** `obj_bostaff_equipped.fbx` / `obj_broomstick_pickup.fbx`

---

### 2. Ruler → Throwing Shurikens

**Tier:** 1 | **Type:** Ranged, multi-hit | **Priority:** ⭐⭐⭐ HIGH

**Pickup Prop — Ruler**
```
A classic 30cm wooden school ruler, stylized chunky 3D, cardboard-and-marker art style.
Flat rectangular shape with hand-drawn measurement lines and numbers that look like crayon scrawls.
Warm tan wood color with slight grain. Small notch at one end.
Strong distinct silhouette — flat, wide, elongated rectangle, instantly readable as a ruler at small size.
Clean white background, flat prop view slightly angled, 3/4 above.
Not photorealistic, keep it simple and readable, no complex engraving.
```

**Imagined Weapon — Throwing Shuriken**
```
A set of three throwing shurikens (ninja stars), stylized 3D, cardboard-and-marker art style.
Each shuriken looks like it was cut from cardboard and reinforced with metallic tape — flat, angular, four-pointed.
Surface has a faint corrugated cardboard texture. Edges have a hand-drawn marker outline.
Color: dark grey with slight silver foil sheen on the flat faces. Small orange marker dot in the center.
Strong distinct silhouette — sharp four-pointed star shape, spiky and angular, unmistakable at small size.
Three shurikens fanned out, clean white background, top-down 3/4 view.
Not photorealistic metal, not too detailed, keep edges bold and graphic.
```

**Unity filename:** `obj_shuriken_equipped.fbx` / `obj_ruler_pickup.fbx`

---

### 3. Garden Hose → Water Whip

**Tier:** 2 | **Type:** Melee, reach | **Priority:** ⭐⭐⭐ HIGH

**Pickup Prop — Garden Hose**
```
A coiled garden hose, stylized chunky 3D, cardboard-and-marker art style.
Green rubber hose coiled in a loose circle. Surface has visible ridged texture like a real hose
but simplified and chunky. Small brass-colored nozzle attached at one end.
Strong distinct silhouette — tight circular coil with nozzle sticking out, readable as a hose at small size.
Clean white background, prop view, 3/4 angle slightly above.
Not photorealistic rubber, not perfectly smooth, chunky and cartoon-adjacent.
```

**Imagined Weapon — Water Whip**
```
A dynamic water whip weapon, stylized 3D, cardboard-and-marker art style.
A green hose that transforms into a flowing arc of water — the tip curls like a cracking whip
frozen mid-swing. The water stream is stylized: segmented, almost like paper cutout shapes,
with a hand-drawn marker outline. Blue-green gradient along the water arc with small white
star-shaped splash droplets at the tip. Handle end is chunky dark green rubber.
Strong distinct silhouette — long sweeping whip arc with a chunky handle base, clear readable shape at small size.
Clean white background, weapon action pose, 3/4 view.
Not realistic water simulation, not too detailed — keep shapes graphic and bold.
```

**Unity filename:** `obj_waterwhip_equipped.fbx` / `obj_gardenhose_pickup.fbx`

---

### 4. Cardboard Tube → Katana *(Needed for Shadow Katana Combo)*

**Tier:** 2 | **Type:** Melee, precise | **Priority:** ⭐⭐ MEDIUM-HIGH

**Pickup Prop — Cardboard Tube**
```
A cardboard mailing tube, stylized chunky 3D, cardboard-and-marker art style.
Cylindrical tube with visible corrugated cardboard end caps. Natural kraft brown color with
subtle grain texture that looks hand-drawn. Slightly dented and worn.
Strong distinct silhouette — long narrow cylinder with flat end caps, readable as a tube at small size.
Clean white background, prop view leaning at slight angle, 3/4 above.
Not photorealistic, keep the corrugation visible and stylized, chunky proportions.
```

**Imagined Weapon — Katana**
```
A katana sword, stylized chunky 3D, cardboard-and-marker art style.
The blade looks like a flattened cardboard tube pressed into a sword shape — visible corrugation lines
run along the length, giving it a hand-crafted look. The edge has a silvery foil sheen (crumpled foil look).
Handle wrapped with black paper strips tied with twine. Simple round guard made from a cardboard disc.
Small orange tape detail near the base. Overall dark navy and silver palette with black marker outlines.
Strong distinct silhouette — long diagonal blade with a small round guard, exaggerated proportions, clear readable shape.
Clean white background, weapon pose diagonal, 3/4 view.
Not smooth metal, not realistic blade — cardboard katana with strong graphic silhouette.
```

**Unity filename:** `obj_katana_equipped.fbx` / `obj_cardboardtube_pickup.fbx`

---

## Phase 2 Weapons (Model After Phase 1 Ships)

---

### 5. Pool Noodle → Foam Sword

**Tier:** 1 | **Type:** Melee, fast | **Priority:** ⭐ LOW (Phase 2)

**Imagined Weapon — Foam Sword**
```
A foam sword weapon, stylized chunky 3D, cardboard-and-marker art style.
Shaped like a classic fantasy short sword but made entirely from a pool noodle —
bright cyan foam with a slight squished-cylinder cross-section. Visible foam texture
that looks like tiny bubbles, slightly hand-drawn. Guard is a flat piece of cardboard
with marker-drawn crossguard lines. Handle is wrapped in orange tape.
Strong distinct silhouette — wide rounded blade with a flat T-shaped crossguard, exaggerated chunky proportions, clear readable shape at small size.
Clean white background, sword held upright, 3/4 view.
Not sharp or dangerous-looking — soft toy-like silhouette distinct from the katana's thin blade.
```

**Unity filename:** `obj_foamsword_equipped.fbx`

---

### 6. Flashlight → Lightsaber / Torch

**Tier:** 2 | **Type:** Melee + light AoE | **Priority:** ⭐ LOW (Phase 2)

**Pickup Prop — Flashlight**
```
A chunky handheld flashlight, stylized 3D, cardboard-and-marker art style.
Classic cylinder shape with a large round reflector head. Dark grey body with yellow marker accents.
The lens looks like crinkled cellophane. Visible screw lines drawn on with marker.
Strong distinct silhouette — short fat cylinder capped by a wide circular head, readable as a flashlight at small size.
Clean white background, prop view, 3/4 angle.
Not photorealistic, chunky proportions, cartoon-adjacent.
```

**Imagined Weapon — Lightsaber / Torch**
```
A glowing energy sword weapon, stylized chunky 3D, cardboard-and-marker art style.
Handle is a chunky flashlight body with grip tape stripes in black marker.
The blade is a bold cylinder of yellow-white light — stylized, not realistic glow —
looks like a glowing paper tube with hand-drawn light lines radiating outward.
Small star-burst shapes float near the base. Yellow and white color palette with dark outlines.
Strong distinct silhouette — fat handle with a long straight glowing blade, exaggerated proportions, clear readable shape at small size.
Clean white background, upright pose, 3/4 view.
Not realistic glow or smooth gradient — graphic and bold.
```

**Unity filename:** `obj_lightsaber_equipped.fbx` / `obj_flashlight_pickup.fbx`

---

### 7. Spatula → Short Sword / Paddle

**Tier:** 2 | **Type:** Melee, knockback | **Priority:** ⭐ LOW (Phase 2)

**Imagined Weapon — Short Sword / Paddle**
```
A battle paddle weapon, stylized chunky 3D, cardboard-and-marker art style.
A kitchen spatula transformed — the wide flat head becomes a sturdy shield-like blade
with hand-drawn impact lines and a zig-zag edge design in black marker.
Handle is dark wood-grain, slightly chunky. Flat head is bright orange with marker details.
Strong distinct silhouette — wide flat rectangular blade on a thin handle, exaggerated proportions, clear readable shape at small size. Unmistakably different from round or pointed weapons.
Clean white background, weapon raised pose, 3/4 view.
Not photorealistic kitchen tool, exaggerate the flat head to be more weapon-like and graphic.
```

**Unity filename:** `obj_paddle_equipped.fbx`

---

### 8. Bicycle Pump → Pressure Cannon

**Tier:** 3 | **Type:** Ranged, AoE | **Priority:** ⭐ LOW (Phase 2)

**Imagined Weapon — Pressure Cannon**
```
A handheld pressure cannon, stylized chunky 3D, cardboard-and-marker art style.
Based on a bicycle pump — the long cylinder becomes a barrel, the handle becomes the grip.
Body is a chunky tube with hand-drawn pressure gauge details, duct-tape seams, and warning arrows
drawn in orange marker. Barrel tip has a wide flared opening. Grey and orange color palette.
Strong distinct silhouette — long barrel with a wide flared muzzle and a perpendicular grip handle, exaggerated proportions, clear readable cannon shape at small size.
Clean white background, weapon held at angle, 3/4 view.
Not photorealistic metal, very chunky proportions, cobbled-together toy cannon.
```

**Unity filename:** `obj_pressurecannon_equipped.fbx`

---

### 9. Remote Control → Magic Wand

**Tier:** 3 | **Type:** Ranged, elemental | **Priority:** ⭐ LOW (Phase 2)

**Imagined Weapon — Magic Wand**
```
A magic wand weapon shaped like a TV remote control, stylized chunky 3D, cardboard-and-marker art style.
The remote becomes a wizard's wand — elongated, with glowing button symbols drawn in marker on the face.
A star-shaped burst at the tip glows with purple energy, stylized as a flat marker-drawn star shape.
Body is dark grey plastic with visible screw circles and marker-drawn runes where the channel buttons were.
Purple accent glow, black outlines, small floating star particles near the tip.
Strong distinct silhouette — rectangular wand body tapering to a large spiky star burst at the tip, clear readable shape at small size.
Clean white background, wand raised upward, 3/4 view.
Not photorealistic, chunky and graphic, remote DNA visible but magically transformed.
```

**Unity filename:** `obj_magicwand_equipped.fbx`

---

### 10. Lunchbox → Shield / Throwing Weapon

**Tier:** 3 | **Type:** Defensive + ranged | **Priority:** ⭐ LOW (Phase 2)

**Imagined Weapon — Lunchbox Shield**
```
A shield/throwing weapon shaped like a metal lunchbox, stylized chunky 3D, cardboard-and-marker art style.
Classic rectangular metal lunchbox with rounded corners and a handle on top.
Face is decorated with hand-drawn marker art — a star, the hero's box insignia, and bold graphic lines.
Edges have a crumpled aluminum foil look. Color: bright red body with yellow lid and black marker outlines.
Strong distinct silhouette — squat rectangular block with a small handle protruding from the top, exaggerated proportions, unmistakably a lunchbox at small size.
Clean white background, shield-hold pose, 3/4 view slightly above.
Not photorealistic metal, chunky and toy-like, marker decorations must look hand-drawn and slightly imperfect.
```

**Unity filename:** `obj_lunchboxshield_equipped.fbx`

---

## Acceptance Criteria

Every generated weapon model must pass all of the following checks before being accepted into the pipeline:

**Silhouette (required — Principle #6)**
- Render the model against a white background and convert it to a solid black shape
- Scale the silhouette to 10% of viewport height (~150px on a 1080p screen)
- The weapon must be instantly identifiable as itself (not just "a sword" — THIS specific sword)
- The weapon must be distinguishable from every other weapon in the game by shape alone, with no color or texture information
- Fail indicator: two weapons produce silhouettes that could be confused for each other — redesign or exaggerate proportions until they differ

**Poly count**
- 300–600 triangles. Check in Unity's mesh inspector after import.
- Use Meshy's Simplify tool before export if the count is too high.

**Texture**
- 512×512 max. Diffuse only (no normal maps in Phase 1).
- Export with textures enabled; re-download ZIP if textures are missing.

**Scale**
- `globalScale = 1`, `useFileScale = true` on import.
- Runtime size controlled by `WeaponHolder.weaponScale`. Never hard-code `globalScale = 100`.

**Style**
- Chunky proportions, cardboard-and-marker aesthetic visible in texture
- No photorealistic surfaces, no smooth PBR metal, no generic fantasy design

---

## General Meshy Tips

- **Art style keywords that help:** "stylized 3D", "cartoon game asset", "Psychonauts style", "chunky proportions", "hand-crafted look", "game-ready low poly"
- **Art style keywords to avoid in prompts:** "photorealistic", "hyperdetailed", "PBR materials", "realistic textures"
- **If the model comes out too smooth:** Add "cardboard texture", "paper texture", "hand-drawn lines on surface" to the prompt
- **If proportions are wrong:** Add "chunky and stocky", "exaggerated proportions", "toy-like scale"
- **Poly count:** After importing, check in Unity's mesh inspector. Target 300–600 tris. Use the Simplify tool in Meshy before export if needed.
- **Texture export:** Use "Export with textures" in Meshy. If textures don't export, re-download the ZIP with the option enabled (this was an issue with character imports — same fix applies).

---

> **V2 Cul-de-Sac weapons (11–14: Lasso, Dynamite Bundle, Quickdraw Blade, Six-Shooter) have been moved to `docs/meshy-prompts-v2.md`.**


*Last updated: Sprint 11 — 2026-07-17*
*Phase 1 priority weapons (1–4) and Phase 2 general weapons (5–10) in this file. Cul-de-Sac weapons (11–14) moved to `docs/meshy-prompts-v2.md`.*
