# BoxForged — Meshy ENV Prop Prompts: Forge Workbench & Cardboard Pile

**Zone:** The Backyard (Phase 1) — Safe Zone / Crafting Area
**Prepared:** 2026-08-04
**For:** Meshy text-to-3D generation

---

## How to Use These Prompts

Paste the **Meshy Text Prompt** directly into Meshy's text-to-3D input. Set **Meshy Style: Low Poly** and **Symmetry: OFF** for both props — they are hand-built, asymmetric objects.

After generating:
1. Download as `.glb` or `.fbx`
2. Place in `boxhead/models/` (above the Unity project root) for pipeline processing
3. Run `/asset-pipeline` to route through Blender validation and into Unity
4. Final Unity destination: `Assets/_Project/Models/Environment/Backyard/`

**Poly budget:** 300–500 triangles each. Check in Meshy's poly count display before exporting — use Meshy's Simplify tool if over 600.

**Texture:** 512×512 diffuse only. No normal maps.

---

## Global Settings — These Two Props

| Setting | Value |
|---|---|
| **Meshy Style** | Low Poly |
| **Symmetry** | OFF |
| **Texture size** | 512×512 diffuse only |
| **Palette** | Warm kraft brown, aged wood, tan/grey tape tones — no cool greys, no sci-fi |
| **Outlines** | Marker-drawn on all surface edges |
| **Grain** | Corrugated cardboard grain on flat cardboard surfaces; visible wood grain on wood |
| **Delivery path** | `Assets/_Project/Models/Environment/Backyard/` |

> **Meshy character limit:** 800 characters per prompt. Both prompts below are written under this limit.

---

## Asset BK-01: Forge Workbench

**Unity asset name:** `env_bk_forge_workbench.fbx`
**Meshy Style:** Low Poly
**Symmetry:** OFF
**Poly budget:** 400–500 tris
**Priority:** HIGH — this is the primary crafting station; appears in the Safe Zone

**What it is:** The crafting station where the player wraps household objects in cardboard to make weapons. A child built this from salvaged wood and cardboard. It should feel important — used, worn, deliberate. Like someone has made a hundred things here and plans to make a hundred more.

**Gameplay camera:** The game camera is top-down at roughly a 45-degree angle. The workbench must read clearly from above — the work surface, the cardboard backing wall, and the tools-implied details should all be visible from that angle.

### Meshy Text Prompt

```
A wooden crafting workbench, stylized low-poly game prop. Sturdy rectangular wood plank work surface
reinforced on the edges with strips of cardboard and duct tape. A short vertical cardboard wall rises
behind the work surface like a pegboard made of cardboard — flat rectangular cardboard panel with tape
seams. Small details on the wall: a cardboard tube holder on one side, a tape roll hanging from a
simple hook. Table legs are thick chunky square-section wood, reinforced at the joints with strips of
cardboard wrapped tightly with grey duct tape. Surface is worn and marked. Warm dark wood brown,
kraft cardboard brown, grey tape tones. Corrugated cardboard grain on cardboard panels. Marker-drawn
outlines on all edges. Chunky proportions. Hard faceted low-poly geometry. Stylized game prop,
cardboard-and-marker aesthetic.
```

### Style Keywords

- `low poly stylized 3D`
- `cardboard texture`
- `URP mobile game asset`
- `hand-crafted`
- `chunky proportions`
- `game-ready low poly`
- `craft material aesthetic`

### Negative Prompt

```
photorealistic, smooth plastic, metallic surfaces, sci-fi, fantasy, glowing, neon, realistic wood grain,
high poly, subdivision surface, HDRP, PBR metal, clean factory-made, modern design, glass, sharp thin legs
```

### Art Direction Notes

- **The cardboard backing wall is the key read.** It is what turns this from "a table" into "a crafting station." If Meshy generates just a flat table with no backing wall, refine with: "a short cardboard panel wall rising from the back of the table, waist height, flat rectangular panel made of cardboard."
- **Top-down readability is critical.** The pegboard-style backing wall must be visible from above. If it generates too short (flush with the table surface), refine with: "cardboard backing wall rises 40cm above the table surface, visible from above."
- The tape roll on a hook and the cardboard tube holder are texture/silhouette details — they do not need to be separate meshes. Implied is fine.
- This prop should feel loved, not abandoned. Worn and used, but deliberately set up. Not junk, not trash.
- **Asymmetry is correct.** The backing wall details (tube holder, tape roll) should sit off-center — it was built by a kid, not manufactured.

### Silhouette Check

From top-down: should read as a wide rectangle (the work surface) with a shorter rectangle rising behind it (the backing wall). The workbench silhouette should be immediately distinct from a barrel, a crate, or a bench — the upright backing wall is what does this.

Solid black shape test: the backing wall must be visible and read as a vertical element rising from the table's rear edge. If it merges flush with the table mass, the backing wall is too short.

### Post-Processing Notes

- No rig. Snap base to Y=0.
- Target 400–500 tris. Most of the budget goes to the work surface and the backing wall geometry — legs can be aggressively decimated.
- Tag as `isStatic = true` in Unity. Included in the Safe Zone setup — not a NavMesh obstacle (players do not need to navigate around it; it sits at the zone edge).
- The workbench is a **UI trigger zone** — the player walks up to it to open the crafting screen. Mark it with a trigger collider (Box Collider, `isTrigger = true`) in Unity after import. Size the collider to cover the front face of the work surface.

---

## Asset BK-02: Single Cardboard Piece (Pickup Unit)

**Unity asset name:** `env_bk_cardboard_piece.fbx`
**Meshy Style:** Low Poly
**Symmetry:** OFF
**Poly budget:** 50–80 tris
**Priority:** HIGH — this is the unit of the primary resource collectible; instanced 4–8× per pickup prop

**What it is:** A single flat corrugated cardboard sheet — one piece, not a pile. In Unity, the `pfb_pickup_cardboard` prefab clusters 4–8 instances of this single mesh at varied positions, rotations, and slight scale variation to create the pile appearance. Generating the pile as one model would waste the triangle budget and lose flexibility. The individual piece is what Meshy generates.

**Key insight:** In BoxForged, cardboard is the most precious resource in the world. A single piece of cardboard should feel like something worth picking up — not trash.

**Gameplay camera:** Top-down at roughly 45 degrees. The piece needs to read as a distinct physical object from above, not flat ground decal.

### Meshy Text Prompt

```
A single flat corrugated cardboard sheet, stylized low-poly game prop. Rectangular shape, slightly
larger than a sheet of paper, gently warped — not perfectly flat. Torn and slightly rough on two
edges, cut clean on the other two. Corrugated fluting visible on all torn edges showing the internal
wave structure. Surface has subtle texture — kraft brown corrugated cardboard grain. A faint marker
scribble on one face, like a label was written and crossed out. Slightly bent across the long axis,
giving it a gentle curve. Warm kraft brown colour, slightly aged. Hard faceted low-poly geometry.
Stylized game prop.
```

### Style Keywords

- `low poly stylized 3D`
- `corrugated cardboard`
- `URP mobile game asset`
- `game-ready low poly`
- `warm kraft brown`
- `single flat sheet`
- `stylized pickup prop`

### Negative Prompt

```
photorealistic, pile, stack, bundle, multiple pieces, trash, debris, dirty, grimy, smooth plastic,
metallic, sci-fi, high poly, subdivision surface, HDRP, box, container, folded box, cube
```

### Art Direction Notes

- **One piece only.** The prompt must not let Meshy interpret this as a bundle or stack. If it generates multiple pieces, add to prompt: "a single isolated cardboard sheet, alone, no other pieces."
- **The gentle warp is important.** A perfectly flat rectangle looks like a game tile or a UI element. The slight bow across the long axis gives it physical weight and makes it read as a real object.
- **Torn edges on two sides** are the most important silhouette detail. They break the rigid rectangle into something organic and immediately read as "real cardboard" from a distance.
- **The faint marker scribble** adds personality and connects to the game's cardboard-and-marker aesthetic. One or two characters or a crossed-out word — not a full label.
- Color should be warm kraft (#C49A5A to #A67C45 range). Do not let it go grey or cool.

### Silhouette Check

From top-down: should read as an irregular rectangle — mostly straight edges but with roughness and tear character on two sides. The gentle warp should give it a slight shadow gradient across the surface.

Solid black shape test: must be distinct from a floor tile (too perfect), a plank (too thin), and a leaf (too organic). The two straight cut edges + two torn edges is the key read.

### Unity Assembly Notes

This single mesh is the base for the `pfb_pickup_cardboard` prefab. In Unity:

1. Import `env_bk_cardboard_piece.fbx` to `Assets/_Project/Models/Environment/Backyard/`
2. Create an empty GameObject — this becomes `pfb_pickup_cardboard`
3. Add 4–6 child instances of the piece mesh, each at a slightly varied:
   - Y position (0 to 0.03m, layered)
   - Y rotation (randomised 0–360°)
   - Scale (0.85×–1.1× variation)
4. Add a **Box Collider** with `isTrigger = true` on the parent — sized to cover the pile's footprint
5. Add `CardboardPickup` component to the parent — awards resources and destroys the parent on player enter
6. Do NOT tag `isStatic = true` — this prop is destroyed on pickup

---

## Delivery Paths

| Asset | Unity Filename | Raw Download Path |
|---|---|---|
| Forge Workbench | `env_bk_forge_workbench.fbx` | `boxhead/models/env/backyard/` |
| Cardboard Piece (single unit) | `env_bk_cardboard_piece.fbx` | `boxhead/models/env/backyard/` |

**Generation order:** BK-01 (Forge Workbench) first — higher priority, more refinement passes expected. BK-02 (Cardboard Piece) second — simpler form. Remember: BK-02 is one piece; the pile is assembled in Unity by the developer from multiple instances.

---

## Acceptance Criteria

Every generated ENV prop must pass all of the following checks before being accepted into the pipeline:

**Silhouette (required)**
- Render against white background, convert to solid black shape, scale to 10% of viewport height
- The prop must be instantly identifiable as itself (workbench = table with upright backing; cardboard pile = low wide bundle)
- Must be distinguishable from other props in the scene (crate, barrel, fence post) by shape alone

**Poly count**
- Forge Workbench: 400–500 tris. Cardboard Piece (single): 50–80 tris.
- Check in Unity's mesh inspector after import. Use Meshy Simplify before export if over budget.

**Texture**
- 512×512 max. Diffuse only (no normal maps in Phase 1).
- Export with textures enabled.

**Scale**
- `globalScale = 1`, `useFileScale = true` on import.
- Forge Workbench: approximately 0.8–1.0m tall at game scale.
- Cardboard Piece: approximately 0.03–0.05m tall (flat sheet), 0.3–0.4m wide. The pile height (0.15–0.2m) is achieved by layering 4–6 instances in the prefab — not the model itself.

**Style**
- Warm cardboard-and-marker aesthetic visible in texture
- No photorealistic surfaces, no smooth PBR materials
- Corrugated cardboard grain visible on cardboard surfaces

---

_Backyard ENV Props — Created 2026-08-04 | V4 Phase 1 prep_
_See `docs/art/style-guide.md` for full art direction reference._
