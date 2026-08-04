# Unboxed Heroes — Meshy HUD Element Prompts

**Purpose:** AI-generated 3D models for all HUD/UI asset components.
**Tool:** Meshy (text-to-3D)
**Art direction:** Chunky stylized 3D, cardboard-and-marker aesthetic. Think Tearaway meets Psychonauts — not photorealistic, not flat cartoon. HUD elements look like a kid built a scoreboard out of craft supplies and stuck it to the top of the screen.
**Poly budget:** 200–400 triangles per HUD element (these are background frames — keep them simple).
**Texture size:** 512×512 max. Diffuse only — no normal maps in Phase 1.
**Render setup in Unity:** After import, render each element face-on from a top-down orthographic camera. The rendered sprite is what gets used in the UI.

---

## How to Use These Prompts

Each HUD element is a 3D prop modeled in Meshy, imported into Unity, and rendered orthographically to produce a sprite. This keeps the HUD visually consistent with the 3D world.

**After generating in Meshy:**
1. Export as `.glb` or `.fbx`
2. Import into Unity: `Assets/_Project/Models/UI/HUD/`
3. Set import scale, extract textures to `Assets/_Project/Models/UI/HUD/Textures/`
4. Name per convention: `ui_hud_[element].fbx`
5. Place in a dedicated UI render camera scene, screenshot orthographically, export as PNG sprite

---

## HUD Layout Reference

```
┌─────────────────────────────────────────────────────┐
│ [HEALTH BAR]      [BOX ICON | STYLE ICON]  [IP ///] │
│                   [CHARGE METER      ]              │
└─────────────────────────────────────────────────────┘
```

The five elements below make up the full HUD. Model each separately — they are composited in Unity UI.

---

## 1. Health Bar Frame

**Position:** Top-left | **Priority:** ⭐⭐⭐ HIGH

This is the outer frame/shell of the health bar. The colored fill (green → red gradient) is handled in Unity via a UI Image component — the Meshy model provides only the cardboard container.

```
A horizontal health bar frame made from a strip of torn cardboard, stylized chunky 3D,
cardboard-and-marker art style. Rectangular shape, slightly wider than tall, with rough
torn edges on the left and right sides as if ripped from a larger piece. A small red heart
shape drawn in crayon sits on the far left end. The surface has visible corrugated cardboard
texture. Color: natural kraft brown with slight warm tones. A thin black marker outline traces
the inner opening where the health fill bar will show through.
Clean white background, face-on orthographic view, slightly angled for depth.
Strong distinct silhouette, bold graphic form readable at thumbnail size.
Not photorealistic, not perfectly rectangular — imperfect hand-made edges, chunky proportions.
```

**Unity filename:** `ui_hud_healthbar_frame.fbx`

---

## 2. IP Tally Counter Frame

**Position:** Top-right | **Priority:** ⭐⭐⭐ HIGH

The container for the Imagination Points counter. Tally marks are rendered on top of this frame via Unity TextMeshPro using a custom tally-mark font, or drawn via sprite overlay.

```
A small rectangular notepad or sticky note, stylized chunky 3D, cardboard-and-marker art style.
Slightly crumpled at the corners as if pulled from a pocket. Surface has faint ruled lines in
light pencil — like graph paper or notebook paper drawn on cardboard. Five tally marks are
scratched into the surface in dark crayon: four vertical strokes with a diagonal cross-stroke.
A small star drawn in orange marker sits in the top-left corner as a label icon.
Color: pale cream or light tan with brown kraft undertone. Dark marker outlines.
Clean white background, face-on orthographic view with slight angle.
Clear readable shape, strong distinct silhouette distinct from other HUD frames.
Not photorealistic paper, not perfectly flat — slightly puffy and chunky, hand-crafted feel.
```

**Unity filename:** `ui_hud_ipcounter_frame.fbx`

---

## 3. Box Icon Frame

**Position:** Top-center (left of style icon) | **Priority:** ⭐⭐ MEDIUM

This is the frame that displays the player's active box cosmetic icon. The box icon itself (Ninja, Cowboy, etc.) is a separate sprite swapped in at runtime.

```
A small square frame made from a piece of cardboard, stylized chunky 3D, cardboard-and-marker
art style. Square shape with slightly rounded corners, thick cardboard walls visible on all edges.
Looks like a tiny picture frame a kid made — edges have hand-drawn decorative dashes in black marker.
A small folded paper tab sticks up from the top center as a label grip, with the letter "B"
written on it in crayon. Surface texture shows corrugated cardboard grain.
Color: natural kraft brown with warm yellow-brown tones.
Clean white background, face-on orthographic view, slight angle showing frame depth.
Strong distinct silhouette, square outline with protruding tab — bold graphic form.
Not photorealistic, chunky and thick-walled, marker decorations look hand-drawn and imperfect.
```

**Unity filename:** `ui_hud_boxicon_frame.fbx`

---

## 4. Style Icon Frame

**Position:** Top-center (right of box icon) | **Priority:** ⭐⭐ MEDIUM

Holds the active Fighting Style icon. Visually similar to the Box Icon Frame but slightly different shape to distinguish them at a glance.

```
A small pentagon or shield-shaped frame made from cardboard, stylized chunky 3D,
cardboard-and-marker art style. Shield outline with a flat top and pointed bottom — like a
tiny coat-of-arms. Thick cardboard edges with corrugated texture visible on the sides.
Decorative star drawn in orange marker in each top corner. A small paper tab at the top
has the letter "S" written in crayon. The inner face is slightly recessed.
Color: natural kraft brown with a faint blue marker border line inside the opening.
Clean white background, face-on orthographic view, slight angle showing depth.
Strong distinct silhouette — pointed shield outline must differ clearly from square box icon frame.
Not photorealistic, chunky and graphic, shield shape bold and readable at small sizes.
```

**Unity filename:** `ui_hud_styleicon_frame.fbx`

---

## 5. Charge Meter Frame

**Position:** Top-center, below box/style icons | **Priority:** ⭐⭐ MEDIUM

Container for the charge meter — a horizontal bar that fills as the player builds combos. The fill itself is handled in Unity (a color-animated UI Image). This model provides the frame only.

```
A narrow horizontal bar frame made from a folded strip of cardboard, stylized chunky 3D,
cardboard-and-marker art style. Long and thin — roughly 3:1 width-to-height ratio.
Looks like a cardboard ruler with the measuring markings replaced by lightning bolt symbols
drawn in yellow marker at even intervals. Both ends are capped with small squares of
crumpled foil, as if reinforced by a kid with craft supplies. A small lightning bolt drawn
in yellow crayon sits above the center.
Color: dark kraft brown frame, yellow marker accents.
Clean white background, face-on orthographic view, slight angle.
Clear readable shape — extreme horizontal proportion is its key silhouette distinction.
Not photorealistic, keep it narrow and graphic, lightning bolt symbols bold and clearly readable.
```

**Unity filename:** `ui_hud_chargemeter_frame.fbx`

---

## 6. HUD Background Panel (Optional — Full Top Strip)

**Priority:** ⭐ LOW (use only if individual elements need a unifying backdrop)

If the individual elements look too disconnected in the final UI, this unified panel can sit behind all HUD elements as a background strip.

```
A horizontal strip of torn cardboard running full-width, stylized chunky 3D,
cardboard-and-marker art style. Slightly wider than tall — meant to span the entire top of
a mobile screen. Top edge is straight (screen edge), bottom edge is torn and uneven,
like cardboard ripped by hand. Visible corrugated texture across the entire surface.
Faint marker scribbles and doodles in the background — a small star, a dot pattern, an arrow —
as if a kid was decorating the border of a notebook page. Warm kraft brown color.
Clean white background, face-on orthographic view.
Bold graphic form — straight top with ragged torn bottom silhouette, instantly readable full-width band.
Not photorealistic, not perfectly rectangular on the bottom — torn and imperfect is the goal.
```

**Unity filename:** `ui_hud_background_strip.fbx`

---

## Acceptance Criteria

Before any generated HUD asset moves to Unity import, it must pass all of the following checks:

**Silhouette readability (Principle #6 — mandatory)**
- Convert the rendered sprite to a solid black shape. Every element must be identifiable at that silhouette alone, at thumbnail size (~64px wide).
- Each element must be distinguishable from all others by shape alone, without color or texture cues:
  - Health bar: wide flat horizontal band with heart bump on left
  - IP Counter: small upright rectangle (taller than wide)
  - Box Icon frame: square with protruding tab on top
  - Style Icon frame: pentagon/shield with pointed bottom (visually different from the square above)
  - Charge Meter: very long thin horizontal strip (clearly narrower and longer than the health bar)
  - Background Panel: full-width band with ragged torn bottom edge
- If two elements could be confused as silhouettes, adjust the asset before import — do not rely on color to differentiate them.

**General quality checks**
- Face-on surface (not back or sides) is fully detailed and clean.
- Cardboard texture is visible and consistent with other HUD elements.
- No photorealistic, glass, metal, or smooth-plastic surfaces.
- Proportions match the target aspect ratios: health bar ~4:1, charge meter ~3:1, icon frames ~1:1.
- Poly count: 200–400 triangles.
- Texture: 512×512 max, diffuse only.

---

## General Meshy Tips (HUD-Specific)

- **The inner face matters most** — HUD frames are viewed almost perfectly face-on. Make sure the prompt emphasizes face detail over side/back detail.
- **Keep it readable at small sizes** — mobile HUD elements are small. Bold outlines and simple shapes read better than fine detail.
- **If the model is too ornate:** Add "simple shape", "minimal detail", "bold graphic lines" to the prompt
- **If the corrugation isn't visible:** Add "visible corrugated cardboard texture", "ridged surface"
- **If proportions are wrong:** The health bar frame should be ~4:1 (wide); the charge meter should be ~3:1 (wide); the icon frames should be roughly square (1:1)
- **Art style keywords that help:** "stylized 3D", "game UI asset", "cardboard craft aesthetic", "hand-made look", "marker outlines", "Tearaway art style"
- **Art style keywords to avoid:** "photorealistic", "smooth plastic", "glass", "metal", "hyperdetailed"

---

*Last updated: Sprint 11 — 2026-07-18*
*HUD prompts correspond to the layout defined in GDD-V2.md Section 11.*
