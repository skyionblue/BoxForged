# Meshy Prompts: Cul-de-Sac Room 1 "The Arrival"

**Zone:** The Cul-de-Sac (Zone 1)
**Prepared:** 2026-08-05
**Assets needed:** 2 (all other Room 1 props exist in project)

---

## Asset Gap Summary

| Prop | Status | Source |
|---|---|---|
| Saloon facades | ✅ In project | `pfb_env_saloon_facade` |
| Covered wagons | ✅ In project | `pfb_env_covered_wagon` |
| Hitching posts | ✅ In project | `pfb_env_hitching_post` |
| Western lamp posts | ✅ In project | `pfb_env_lamp_post_western` |
| Rain barrels | ✅ In project | `pfb_env_rain_barrel` |
| Mailbox telegraph | ✅ In project | `pfb_env_mailbox_telegraph` |
| Tumbleweed (static) | ✅ In project | `pfb_env_tumbleweed_static` |
| Wanted poster | ✅ In project | `pfb_env_wanted_poster_blank` |
| Saloon sign board | ✅ In project | `pfb_env_saloon_sign_board` |
| Gallows frame | ✅ In project | `pfb_env_gallows_frame` |
| Water trough | ✅ In project | `pfb_env_water_trough` |
| Barrels | ✅ Polyworks | `Prop_Barrel_Closed_01`, `Prop_Barrel_Water_01` |
| Rocks (scatter) | ✅ Polyworks | `Rock_Small_Dirt_01–04` |
| Dead bushes | ✅ Polyworks | `Vegetation_Bush_Small_01–03` |
| Cacti | ✅ Polyworks | `Vegetation_Cactus_01–15` |
| Wooden fence sections | ✅ Polyworks | `Prop_Fence_Wooden_Small_01–04` |
| Cardboard boxes | ✅ Polyworks | `Prop_Junk_Cardboard_Box_01–05` |
| Wooden signs | ✅ Polyworks | `Prop_Sign_Wooden_Blank_01` |
| **Rope Coils** | ❌ **Missing** | Meshy — see below |
| **Broken Wagon Wheel** | ❌ **Missing** | Meshy — see below |

---

## Global Settings — Cul-de-Sac Assets

| Setting | Value |
|---|---|
| **Palette** | Warm tan, dusty orange, worn brown, weathered wood — no cool colours, no modern materials |
| **Texture** | 512×512 diffuse only. No normal or roughness maps. |
| **Grain** | Corrugated cardboard grain on flat surfaces. Wood has visible grain lines. Rope has visible braid texture. |
| **Outlines** | Bold marker-drawn lines on all surface edges. |
| **Style** | Low Poly |
| **Symmetry** | OFF for both assets — handmade, organic shapes |
| **Delivery path** | `Assets/_Project/Models/ENV/Backyard/` → actually `Assets/_Project/Models/ENV/CulDeSac/` |

> **Meshy character limit:** 800 characters per prompt. Both prompts below are under this limit.

---

## Asset CD-01: Rope Coils

**Unity asset name:** `env_cds_rope_coil.fbx`
**Meshy Style:** Low Poly
**Symmetry:** OFF
**Poly budget:** 200–300 tris
**Priority:** LOW — atmospheric dressing only, no gameplay function

**What it is:** A coil of thick braided rope lying on the ground near a covered wagon or hitching post. The rope is looped in 2–3 loose circles, not perfectly neat — like it was thrown down in a hurry. One end trails slightly outward.

### Meshy Text Prompt

```
A coil of thick braided rope lying flat on the ground, stylized low-poly game prop. Braided rope in 2–3 loose overlapping loops, slightly uneven, not perfectly circular. One rope end trails away from the coil. Warm tan-brown colour with visible braid lines on the rope surface. Marker-drawn outlines on the rope edges. Hard faceted low-poly geometry. Chunky proportions. Base at Y=0. Stylized game prop, cardboard-and-marker aesthetic, Wild West frontier.
```

### Art Direction Notes

- The braid texture is the single most important detail. Without visible braid lines it reads as a garden hose, not rope.
- Keep it flat — this is a ground prop, should barely extend above Y=0.2m.
- Slightly irregular loop shape. Too perfect reads as a barrel hoop.

### Post-Processing Notes

- No rig. Snap base to Y=0.
- Target 200–300 tris. Aggressive decimation is fine — this is a background scatter prop.
- NOT isStatic — set static=true in Unity (non-interactive).
- No collider needed.

---

## Asset CD-02: Broken Wagon Wheel

**Unity asset name:** `env_cds_broken_wagon_wheel.fbx`
**Meshy Style:** Low Poly
**Symmetry:** OFF
**Poly budget:** 300–450 tris
**Priority:** MEDIUM — supports the covered wagon narrative; appears 1–2 times near wagons

**What it is:** A wooden wagon wheel lying on its side on the ground, broken — 2–3 spokes snapped, the rim cracked and split in one section. The wood is weathered, dry, slightly bleached by the sun. Looks like it was removed from a wagon and left here.

### Meshy Text Prompt

```
A broken wooden wagon wheel lying flat on the ground, stylized low-poly game prop. Large wheel with hub, spokes, and outer rim. 2–3 spokes broken or missing. One section of the outer rim split and cracked. Weathered dry wood texture with visible grain lines. Warm pale brown, slightly bleached. Marker-drawn outlines on all edges. Hard faceted low-poly geometry. Base at Y=0. Stylized game prop, cardboard-and-marker aesthetic, Wild West frontier.
```

### Art Direction Notes

- The broken spokes are the read — if Meshy generates a complete wheel, refine with: "2 spokes snapped off, visible break points where spokes were."
- The wheel should be large: approximately 1.2–1.5m diameter (the covered wagon wheels in the project are this size for reference).
- Lying flat on the ground — horizontal, not standing upright.
- If Meshy generates it standing: it should lie flat. Rotate -90° on X in Blender before export.

### Silhouette Check

From top-down (game camera angle): should read as a circle with spokes and a hub, with 2–3 gaps where spokes are broken. Distinct from a whole wheel or a barrel.

### Post-Processing Notes

- No rig. Ensure the wheel is horizontal (lying flat) before export.
- Target 300–450 tris.
- Mark as Static in Unity.
- Add a thin BoxCollider (`isTrigger=false`) if it will be a NavMesh obstacle near wagons.

---

## Generation Order

1. **Broken Wagon Wheel** — more visible, closer to combat space, more recognisable prop
2. **Rope Coils** — atmospheric scatter, lower visual priority

---

## Delivery Paths

| Asset | Unity Filename | Raw Download Path |
|---|---|---|
| Rope Coils | `env_cds_rope_coil.fbx` | `models/V4/env/CulDeSac/` |
| Broken Wagon Wheel | `env_cds_broken_wagon_wheel.fbx` | `models/V4/env/CulDeSac/` |

**After download:** place zips in `models/zips/` and run `/asset-pipeline` to process through Blender and import to Unity.

---

*Prompts prepared: 2026-08-05 | For art direction review contact art-direction-agent*
