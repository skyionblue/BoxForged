# Meshy Prompts: The Backyard (Dojo) — World 2

**Zone:** The Backyard / Dojo (World 2)
**Prepared:** 2026-08-07
**New ENV assets needed:** 1 confirmed (bamboo stockade wall) + 1 check-first backup (koi pond)

> The dojo already has a rich prop library in-project (cherry tree, stone lantern, torii gate, stepping stones, training dummy, weapon rack, shed) plus the Polyworks **Asian Additional** pack (paper lanterns, bamboo fountain, zen gravel, tatami, bamboo stalks). Only the bamboo boundary wall is genuinely missing.

---

## ENV Asset Gap Summary

| Prop | Status | Source |
|---|---|---|
| Cherry blossom tree | ✅ In project | `pfb_env_cherry_blossom_tree` |
| Stone lantern | ✅ In project | `pfb_env_stone_lantern` |
| Torii gate | ✅ In project | `pfb_env_torii_gate` |
| Stepping stones | ✅ In project | `pfb_env_stepping_stone_tile` |
| Training dummy (makiwara) | ✅ In project | `pfb_env_target_dummy` |
| Weapon rack | ✅ In project | `pfb_env_weapon_rack` |
| Training hall (shed) | ✅ In project | `pfb_env_bld_shedwithcrate` |
| Treehouse platform | ✅ In project | `pfb_env_treehouse_platform` |
| Paper lanterns | ✅ Polyworks | `Asian_Prop_Paper_Lantern_01/02`, `Asian_Prop_Bamboo_Paper_Lantern_01` |
| Bamboo fountain (shishi-odoshi) | ✅ Polyworks | `Asian_Prop_Bamboo_Dried_Water_Fountain_01` |
| Zen raked-gravel | ✅ Polyworks | `Asian_Prop_Zen_Garden_Sand_01` |
| Zen rocks | ✅ Polyworks | `Rock_*` |
| Tatami mats | ✅ Polyworks | `Asian_Prop_Tatami_Mat_*` |
| Bamboo stalk clusters | ✅ Polyworks | `Asian_Prop_Bamboo_Dried_Large_01` |
| **Koi pond basin** | ⚠️ Check first | `street_pond_a` (Polylised pack) — try first; Meshy only if it doesn't read |
| **Bamboo stockade wall** | ❌ Missing | Meshy order — see BD-01 |

---

## Global Settings — Backyard/Dojo Assets

| Setting | Value |
|---|---|
| **Palette** | Jade green, mossy stone-grey, warm bamboo tan, cherry-blossom pink, lacquer red accents. Cool overcast light. No warm amber (that's World 1). |
| **Texture** | 512×512 diffuse only. No normal or roughness maps. |
| **Grain** | Corrugated cardboard grain on flat panels; bamboo shows vertical stalk segments; stone gets marker-drawn crack lines. |
| **Outlines** | Bold marker-drawn lines on all surface edges. |
| **Style** | Low Poly |
| **Symmetry** | ON |
| **Delivery path** | `Assets/_Project/Models/ENV/Backyard/` |

> **Meshy character limit:** 800 characters per prompt. Prompt below is under this limit.

---

## Asset BD-01: Bamboo Stockade Wall

**Unity asset name:** `env_backyard_bamboo_wall.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~400 tris per segment
**Priority:** HIGH — arena boundary + primary dojo identity; used in every room.

**What it is:** A modular bamboo stockade wall segment — a row of thick vertical bamboo poles lashed together with rope cross-bindings, standing about player-height. Tiled edge-to-edge to form arena boundaries.

### Meshy Text Prompt

```
A modular bamboo stockade wall segment, stylized low-poly game prop. A row of 6-8 thick vertical bamboo poles standing side by side, lashed together by two horizontal rope binding bands (upper and lower). Each pole has visible segmented nodes along its length and a flat-cut top. Warm tan-gold bamboo with subtle green tint near the nodes, dark hemp-brown rope bindings. Corrugated cardboard grain on the pole surfaces, visible rope braid on the bindings. Marker-drawn outlines on all edges. Hard faceted low-poly geometry. Chunky proportions. Flat base at Y=0, roughly 3m wide and 2m tall, tileable left-to-right. Stylized game prop, cardboard-and-marker aesthetic, feudal Japanese dojo backyard.
```

### Art Direction Notes

- Must tile seamlessly left-to-right — keep left and right edges flat and identical so segments butt together into a continuous wall.
- The rope binding bands are the identity read — two clear horizontal wraps, chunky.
- **Cover/collision note:** this is an arena boundary wall (~2m tall) — full-height blocker, NOT low cover. Solid box collider along the segment; NavMesh obstacle (carve). The player never sees over it from the top-down camera, so the top edge can be simple.

### Post-Processing Notes

- No rig. Snap base to Y=0.
- Target ~400 tris per segment. Aggressive decimation fine — it repeats many times.
- Mark Static in Unity. Add BoxCollider + NavMeshObstacle (carve=true).

---

## Asset BD-02 (BACKUP — only if `street_pond_a` fails): Koi Pond Basin

**Unity asset name:** `env_backyard_koi_pond.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~350 tris
**Priority:** MEDIUM — only order if the Polyworks `street_pond_a` prefab does not read as a koi pond in-scene.

**What it is:** A low circular stone-rimmed koi pond basin, wide and shallow, with a flat water plane inset below the rim. Sits flush on the ground.

### Meshy Text Prompt

```
A low circular koi pond basin, stylized low-poly game prop. Wide shallow round basin with a chunky stacked-stone rim about knee height, flat interior floor. A separate flat water surface plane sits just below the rim, calm and still. A few smooth pebbles along the inner edge. Mossy grey stone rim with green moss patches, warm sandy interior, pale jade-green water. Marker-drawn crack lines on the stones, marker outlines on all edges. Hard faceted low-poly geometry. Chunky proportions, clearly wider than tall. Flat base at Y=0, roughly 3m diameter. Stylized game prop, cardboard-and-marker aesthetic, feudal Japanese dojo backyard.
```

### Art Direction Notes

- Read from the top-down game camera is a ring (stone rim) with a flat colored disc (water) inside — keep the rim chunky and the water plane distinct in color.
- Keep it low — knee-height rim. The player fights on narrow boards around it (engawa).
- **Collision note:** the water interior is a trigger volume (brief move-speed slow), the stone rim is a low collider. NavMesh: carve the whole footprint (enemies path around it).

### Post-Processing Notes

- No rig. Snap base to Y=0. Water plane as a separate submesh so it can take a scrolling/tinted material.
- Target ~350 tris. Mark rim Static; water plane keeps its own material slot.

---

## ⚠️ Separate Track — NEW Enemy / Boss / Weapon Models (NOT ENV — route to art-direction-agent + /asset-pipeline)

These are characters/weapons, not ENV props, so they are **out of scope for this ENV Meshy file** and do not count against the 8k ENV tri budget. Listed here so they aren't forgotten. Prompts to be authored by **art-direction-agent**; import via `/asset-pipeline` (weapons/props) or `/unity-character-importer` (rigged enemies/boss).

> ✅ **The three rigged characters below now have authored Meshy prompts in the sibling file: `meshy-enemies.md` (same folder).** Water Whip (weapon) remains unauthored — see note in that file.

| Asset | Type | Notes |
|---|---|---|
| Crane Duelist | Enemy (rigged) | Lawn flamingo → one-legged crane-stance spear duelist. New model. Tri budget ~10–12k (standard enemy). → `meshy-enemies.md` BD-E1 |
| Grasscutter | Boss (rigged) | Push reel-mower → tengu blade-master. New model. Tri budget ~25k (boss). → `meshy-enemies.md` BD-B1 |
| Leaf Pile Lurker | Enemy (rigged) | Designed since old Zone 1, never modeled. Ambusher. ~10–12k. → `meshy-enemies.md` BD-E2 |
| Water Whip | Weapon | Garden hose → water dragon-whip. Model + inventory icon still pending from prior sprint. Not yet authored. |

---

## Generation Order (ENV)

1. **Bamboo Stockade Wall (BD-01)** — HIGH, used in every room as boundary.
2. **Koi Pond (BD-02)** — only if `street_pond_a` check-first substitute fails.

## Delivery Paths

| Asset | Unity Filename | Raw Download Path |
|---|---|---|
| Bamboo Stockade Wall | `env_backyard_bamboo_wall.fbx` | `models/V4/env/Backyard/` |
| Koi Pond (if needed) | `env_backyard_koi_pond.fbx` | `models/V4/env/Backyard/` |

**After download:** place zips in `models/zips/` and run `/asset-pipeline` to process through Blender and import to Unity.

---

*Prompts prepared: 2026-08-07 | For art direction review: art-direction-agent*
