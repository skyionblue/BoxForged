# Meshy Prompts: Cul-de-Sac New Buildings

**Status:** Ready for generation — 6 new buildings extending the Wild West main street set
**Date:** 2026-08-05
**Existing buildings (do not duplicate):** saloon facade, two-story western house tall, porch cabin, shed with crate

---

## Global Settings — Cul-de-Sac Building Assets

| Setting | Value |
|---|---|
| **Palette** | Burnt amber `#C47A2B`, dusty tan `#C4A46A`, weathered wood brown `#6B4226`, terracotta `#8B5A2B`, ochre `#B8860B`. Warm shadows throughout. No cool greys. |
| **Texture** | 512×512 diffuse only. No normal maps. No roughness maps. No metallic. |
| **Grain** | Corrugated cardboard grain on all wood plank and flat panel surfaces. Stone surfaces get marker-drawn crack lines. |
| **Outlines** | Bold marker-drawn lines on all surface edges. Slightly uneven line weight — hand-drawn, not uniform. |
| **Style** | Low Poly. Hard faceted geometry, no smooth shading. |
| **Symmetry** | ON for all six buildings. |
| **Poly budget** | 400–800 tris per building. |
| **Delivery path** | `Assets/_Project/Models/Environment/CulDeSac/` |

> Meshy character limit: 800 characters per prompt. All prompts verified under limit.

---

## Generation Order (HIGH priority first)

| Order | Asset | Priority |
|---|---|---|
| 1 | Sheriff's Office | HIGH |
| 2 | Bank | HIGH |
| 3 | General Store | HIGH |
| 4 | Blacksmith Forge | MEDIUM |
| 5 | Barber Shop | MEDIUM |
| 6 | Stables | MEDIUM |

---

### Sheriff's Office

**Unity asset name:** `env_culdesac_bld_sheriffs_office.fbx`
**Poly budget:** ~500 tris
**Priority:** HIGH

#### Meshy Text Prompt

```
A small Western sheriff's office building, stylized low-poly game prop. Single-story brick and wood structure, wide flat facade. Front wall is rough stone brick lower half, weathered plank boards upper half. A barred window on each side of the door — two vertical iron bars per window, chunky square-section bars. Front door is solid dark wood, centered. Above the door a horizontal wooden sign board with a large five-pointed star shape painted in bold marker. Small wooden overhang above the door only — no full porch. Warm terracotta brick, saddle brown planks, dark iron bars. Corrugated cardboard grain on wood surfaces. Hard faceted geometry. Bold marker outlines on all edges. Chunky proportions. Stylized game prop, cardboard-and-marker aesthetic, Wild West frontier town.
```

#### Art Direction Notes

- The barred windows are the identity feature — two thick vertical bars per window, clearly readable from top-down camera
- If bars look thin: "thick chunky square-section iron bars, bold and wide, clearly visible"
- Narrower than the saloon — small civic building, not a wide storefront
- No full wraparound porch — only a small door overhang

---

### Bank

**Unity asset name:** `env_culdesac_bld_bank.fbx`
**Poly budget:** ~600 tris
**Priority:** HIGH

#### Meshy Text Prompt

```
A Western frontier bank building, stylized low-poly game prop. Single-story wide building with a flat-topped facade parapet that rises above the actual roofline — a false front wall. Two thick square stone columns flank a wide double-door entrance, columns slightly protruding from the facade. Double doors are wide and heavy-looking — two tall rectangular door panels side by side, dark wood, with chunky square door handles. Facade surface is rough stone blocks, marker-drawn mortar lines forming a grid pattern. The word BANK carved into the facade above the doors as thick block letters recessed slightly into the stone. Flat parapet top. Warm terracotta stone, dark wood doors. Hard faceted low-poly geometry. Bold marker outlines. Stylized game prop, cardboard-and-marker aesthetic, Wild West frontier town.
```

#### Art Direction Notes

- False-front parapet (tall flat wall rising above roofline) is the iconic Western bank silhouette — must land
- If Meshy generates a normal flat/pitched roof: "parapet wall extending above roofline, facade taller than building behind it, flat-top parapet, Western false front style"
- Two flanking stone columns must be thick and chunky — the top-down silhouette anchor
- "BANK" text: thick block letter shapes baked in texture, does not need to be fully legible

---

### General Store

**Unity asset name:** `env_culdesac_bld_general_store.fbx`
**Poly budget:** ~700 tris
**Priority:** HIGH

#### Meshy Text Prompt

```
A wide Western general store building, stylized low-poly game prop. Single-story wide storefront with a full-width covered porch. Porch roof is a shallow lean-to overhang supported by two square wooden posts at the front corners. Six wooden barrels stacked in two groups of three on the porch — three stacked vertically, flanking the entrance. Plank board facade, weathered tan paint, corrugated cardboard grain running vertically. A wide central door flanked by two plain rectangular windows. A horizontal sign board above the porch roofline. Warm saddle brown and dusty tan palette. Hard faceted low-poly geometry. Bold marker outlines. Chunky proportions, slightly wider than tall. Stylized game prop, cardboard-and-marker aesthetic, Wild West frontier town.
```

#### Art Direction Notes

- Widest building in the set — horizontal emphasis, wider than it is tall
- If Meshy generates narrow result: "very wide single-story building, horizontal emphasis, full-width porch across the entire facade"
- Barrel stacks are the identity feature — chunky cylinders grouped on the porch, visible in silhouette
- Two porch support posts must be clearly visible — simple square-section columns

---

### Blacksmith Forge

**Unity asset name:** `env_culdesac_bld_blacksmith_forge.fbx`
**Poly budget:** ~650 tris
**Priority:** MEDIUM

#### Meshy Text Prompt

```
A Western blacksmith forge workshop, stylized low-poly game prop. Open-front shed structure — no front wall, exposed interior. Wide rectangular shed footprint, three timber posts at the front edge holding up a shallow pitched roof. Roof is corrugated metal panels, dark grey-rust, marker-drawn horizontal ridges. Inside visible from front: a stone forge hearth block in the center-rear, roughly cube-shaped with a wide chimney stack rising from behind it. The chimney is a thick square-section column of stacked brick, protruding above the roofline. A dark shadow anvil shape on the forge block top — flat geometric anvil silhouette baked in texture. Warm wood brown posts, dark rust-grey roof, terracotta brick forge. Hard faceted geometry. Bold marker outlines. Stylized game prop, cardboard-and-marker aesthetic, Wild West frontier town.
```

#### Art Direction Notes

- **Highest rejection risk** — open-front generation is unreliable in Meshy
- If Meshy closes the front: "open workshop, no front wall, interior exposed, three timber columns define front edge only"
- Chimney rising above roofline is the top-down camera cue — if missing, add in Blender (simple square extrusion)
- Anvil is texture-only shadow shape, not a freestanding model

---

### Barber Shop

**Unity asset name:** `env_culdesac_bld_barber_shop.fbx`
**Poly budget:** ~600 tris
**Priority:** MEDIUM

#### Meshy Text Prompt

```
A narrow two-story Western barber shop building, stylized low-poly game prop. Tall narrow facade, vertical emphasis. First floor has a single large rectangular storefront window nearly the full width of the facade — thick wooden frame. A barber pole mounted on the front wall beside the window: a cylindrical pole with bold red and white diagonal stripes, a small round cap at the top and a cylinder base at the bottom. Second floor is a flat plank wall with one small centered window. A wooden sign board above the first floor window. Plank board construction, corrugated cardboard grain. Warm saddle brown and ochre palette, bold red on the barber pole stripes. Hard faceted low-poly geometry. Bold marker outlines. Stylized game prop, cardboard-and-marker aesthetic, Wild West frontier town.
```

#### Art Direction Notes

- Barber pole must be a three-dimensional cylinder protruding from the wall, not painted on
- If pole looks flat: "cylindrical pole protruding from wall surface, round cross-section, clearly three-dimensional"
- Narrow and tall — vertical contrast to the wide low buildings on the street
- Large first-floor window must occupy most of the first-floor wall width — not a small slit

---

### Stables

**Unity asset name:** `env_culdesac_bld_stables.fbx`
**Poly budget:** ~750 tris
**Priority:** MEDIUM

#### Meshy Text Prompt

```
A Western horse stables building, stylized low-poly game prop. Long low barn structure, horizontal emphasis. Wide pitched roof — gently angled, plank board construction. Three open stall bays across the front: three equal rectangular openings in the front wall, each opening is a dark interior shadow with a low horizontal bar across the lower third suggesting a stall gate. Two hay bales in front of the center stall — rectangular block bales, straw-textured tops, marker-drawn bale twine straps. Plank board walls, corrugated cardboard grain running horizontally. A small hay loft window in the gable above center. Warm saddle brown planks, dark ochre straw, pale tan canvas-patch roof. Hard faceted low-poly geometry. Bold marker outlines. Stylized game prop, cardboard-and-marker aesthetic, Wild West frontier town.
```

#### Art Direction Notes

- Long and low — widest building in the set, lowest roofline
- If Meshy generates tall barn: "long low horizontal barn, wide footprint, low roofline, horizontal emphasis"
- Three open stall bays are the identity feature — dark interior voids, not paneled doors
- Two hay bales will be used as minor cover props — ensure distinct block shapes from top-down view
- Hay loft gable window is optional — remove in post if it crowds the gable

---

## Delivery Paths

| Asset | Unity Path |
|---|---|
| `env_culdesac_bld_sheriffs_office.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_culdesac_bld_general_store.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_culdesac_bld_barber_shop.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_culdesac_bld_blacksmith_forge.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_culdesac_bld_bank.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_culdesac_bld_stables.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |

Raw downloads → `boxhead/models/V4/zips/Environment/CulDeSac/` → process through `/asset-pipeline` skill before Unity import.

---

*Created: 2026-08-05 | Art direction: art-direction-agent | Sprint 3 Phase 2*
