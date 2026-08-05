# Unboxed Heroes V4 — Weapon Model Status

**Last updated:** 2026-08-04  
**Purpose:** Track which 3D models exist and which need to be created in Meshy for the weapon creation system.

Each weapon requires **two distinct models**:
1. **Raw Object Prop** — the household object lying on the ground in the level (before forging)
2. **Forged Weapon** — what the player holds in their hand after forging at the workbench

---

## Raw Object Props (World Pickups)

These appear in the level as household objects the player walks over to add to their material bag.

| # | Raw Object | Becomes | Status | Notes |
|---|---|---|---|---|
| 1 | Broomstick | Bo Staff | ✅ **In project** | `obj_broomstick_pickup.fbx` |
| 2 | Ruler | Shurikens | ✅ **In project** | `obj_ruler_pickup.fbx` |
| 3 | Foam Sword | Foam Sword | ✅ **In project** | `obj_foamsword_equipped.fbx` (raw = forged for this weapon) |
| 4 | Cardboard Tube | Katana | ✅ **In project** | `obj_cardboardtube_pickup.fbx` |
| 5 | Flashlight | Lightsaber | ⚠️ **Rebuild needed** | `obj_flashlight_pickup.fbx` exists but has a hole in the geometry and exceeds the 1,000 triangle budget |
| 6 | Garden Hose | Water Whip | ✅ **In project** | `obj_gardenhose_pickup.fbx` |
| 7 | Garden Trowel | Quickdraw Blade | ❌ **Missing — needs Meshy** | Small hand garden trowel/shovel |
| 8 | Jump Rope | Lasso | ❌ **Missing — needs Meshy** | Jump rope coiled on the ground with handles |
| 9 | Bicycle Pump | Pressure Cannon | ❌ **Missing — needs Meshy** | Floor-standing bicycle pump |
| 10 | Remote Control | Magic Wand | ❌ **Missing — needs Meshy** | TV/universal remote control lying flat |
| 11 | Lunchbox | Shield | ⚠️ **Using forged model** | Currently using the lunchbox shield weapon model — may be acceptable since the lunchbox IS the shield |
| 12 | Bike Horn | Dynamite Bundle | ❌ **Missing — needs Meshy** | Rubber squeeze bicycle horn |

---

## Forged Weapons (Player Hand)

These appear in the player's hand after the weapon is forged at the workbench.

| # | Raw Object | Forged Weapon | Status | Notes |
|---|---|---|---|---|
| 1 | Broomstick | Bo Staff | ✅ **In project** | `obj_bostaff_equipped.fbx` |
| 2 | Ruler | Shurikens | ✅ **In project** | `obj_shuriken_equipped.fbx` |
| 3 | Foam Sword | Foam Sword | ✅ **In project** | `obj_foamsword_equipped.fbx` |
| 4 | Garden Trowel | Quickdraw Blade | ✅ **In project** | `obj_quickdrawblade_equipped.fbx` |
| 5 | Cardboard Tube | Katana | ✅ **In project** | `obj_katana_equipped.fbx` |
| 6 | Flashlight | Lightsaber | ✅ **In project** | `obj_lightsaber_equipped.fbx` |
| 7 | Garden Hose | Water Whip | ✅ **In project** | `obj_gardenhose_pickup.fbx` (hose is both raw and equipped) |
| 8 | Jump Rope | Lasso | ✅ **In project** | `obj_lasso_equipped.fbx` |
| 9 | Bicycle Pump | Pressure Cannon | ✅ **In project** | `obj_pressurecannon_equipped.fbx` |
| 10 | Remote Control | Magic Wand | ✅ **In project** | `obj_magicwand_equipped.fbx` |
| 11 | Lunchbox | Shield | ✅ **In project** | `obj_lunchboxshield_equipped.fbx` |
| 12 | Bike Horn | Dynamite Bundle | ✅ **In project** | `obj_dynamitebundle_equipped.fbx` |

---

## Meshy Generation Queue

The following models need to be generated in Meshy and processed through the asset pipeline.

**Triangle budget:** All props must be under **1,000 triangles** (weapon pickup budget). Use Meshy's Simplify tool before export if over budget.

**Import path:** Download as `.glb` or `.fbx` → place in `boxhead/models/` → run `/asset-pipeline` skill.

**Final Unity destination:** `Assets/_Project/Models/Weapons/[ObjectName]/`

### Priority 1 — Missing Raw Props (5 models)

Full Meshy prompts for all of these already exist — see:
- `docs/art/prompts/meshy-v2.md` — Garden Trowel, Jump Rope, Bike Horn
- `docs/art/prompts/meshy-weapons.md` — Bicycle Pump, Remote Control, Flashlight

| Model | Unity Filename | Prompt Location |
|---|---|---|
| **Garden Trowel** | `obj_gardentrowel_pickup.fbx` | `meshy-v2.md` → Weapon 13 |
| **Jump Rope** | `obj_jumprope_pickup.fbx` | `meshy-v2.md` → Weapon 11 |
| **Bicycle Pump** | `obj_bicyclepump_pickup.fbx` | `meshy-weapons.md` → Weapon 8 |
| **Remote Control** | `obj_remotecontrol_pickup.fbx` | `meshy-weapons.md` → Weapon 9 |
| **Bike Horn** | `obj_bikehorn_pickup.fbx` | `meshy-v2.md` → Weapon 12 |

### Priority 2 — Rebuilds (1 model)

| Model | Issue | Prompt Location |
|---|---|---|
| **Flashlight** | Geometry hole + 1,032 triangles (over 1,000 budget) | `meshy-weapons.md` → Weapon 6 |

---

## Style Guide Reminder

All models must match the game's visual language:
- **Low-poly, stylized** — hard faceted geometry, not smooth/realistic
- **Warm palette** — browns, tans, wood tones, aged metals
- **Marker outlines** — black outlined edges visible in texture
- **Corrugated cardboard grain** on any cardboard surfaces
- **Worn and used** — these are household objects in a post-apocalyptic neighborhood

Reference: `docs/v4/art/prompts/meshy-env-forge-and-cardboard.md` for full Meshy prompt style examples.

---

_Document owner: Louie Celli | Created: 2026-08-04_
