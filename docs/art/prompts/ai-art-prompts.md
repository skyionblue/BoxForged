# BoxForged — Meshy AI Prompt Reference
## Sprint 6 Art Pass — 3D Model Generation

**Version:** 2.0
**Date:** 2026-07-14
**Tool:** Meshy AI (meshy.ai) — Text to 3D
**Status:** Active

---

## 1. How to Use This Document

Meshy AI generates game-ready 3D models from plain text descriptions. The workflow from prompt to Unity import is:

1. **Open meshy.ai** and select "Text to 3D."
2. **Paste the prompt** from this document into the text field.
3. **Set Style and Symmetry** as specified per asset (see the settings column in each entry).
4. **Generate.** Meshy produces several preview variants — pick the one closest to the art direction notes.
5. **Check the model** against the acceptance criteria in Section 3 before proceeding.
6. **Refine if needed.** Use Meshy's "Refine" mode to improve the winning variant. You can type additional direction into the refine prompt field.
7. **Export FBX or GLB.** FBX is preferred for Unity. Meshy bundles textures automatically — download the full package (mesh + textures).
8. **Apply post-processing** notes from the asset entry before importing.
9. **Import to Unity.** Drop the FBX into `Assets/_Project/Art/Characters/` or the appropriate subfolder. Apply the material settings from the art style guide.

### Character Limit

Meshy AI enforces an **800-character limit** on text prompts. All prompts in this document are kept under 800 characters. If you customise a prompt, paste it into a character counter before submitting — every character beyond 800 is silently cut off.

---

### What Meshy Generates

Meshy produces a full 3D mesh with auto-generated UV maps and PBR textures (albedo, roughness, metallic, normal). For this project:

- Accept the albedo/diffuse texture as a starting point; plan on painting over it in a tool like Substance Painter or directly in Unity's material editor to match the palette.
- Discard the roughness, metallic, and normal maps in Phase 1 — the art style guide calls for flat cardboard shading with Smoothness 0.05–0.15 and Metallic 0. These maps will be overkill and fight the aesthetic.
- The normal map may be useful in Phase 3 when polish passes begin; keep it in the delivery folder but do not assign it to the material yet.

### Priority Order for Generation

Generate assets in this order — higher entries unblock development and provide proportion reference for later entries:

1. Player character (The Ninja) — establishes the chibi proportion reference all other characters must match
2. The Skeptic — most important enemy; Phase 1 boss
3. Environment props (World Tree, fence section, dead tree) — needed before level art dressing begins
4. Weapons (broomstick, cardboard tube) — needed before weapon pickup system is visually tested
5. Gnome Soldier — Phase 1 enemy; can use a primitive placeholder until Skeptic is done

---

## 2. Meshy Style Settings Reference

### Style Preset

Meshy offers three Style presets in the generation interface:

| Preset | When to use for this project |
|---|---|
| **Cartoon** | All characters and most props. Produces softer geometry, rounder forms, and painterly auto-textures that are closer to the cardboard-and-marker aesthetic. Use this as the default. |
| **Realistic** | Never. Realistic produces PBR surfaces that fight the hand-crafted look and will need to be fully retextured. |
| **Low Poly** | Flat environment props only (fence sections, ground patches, simple structures). Produces hard-faceted geometry that reads as craft-made cardboard. Do not use for characters — it makes faces too blocky to recognize expressions. |

### Symmetry Toggle

| Setting | When to use |
|---|---|
| **Symmetry ON** | All biped characters and most props. Symmetry ensures the left and right halves of the model mirror each other, which is required for clean rigging and animation in Unity. Always on for characters. |
| **Symmetry OFF** | Organic props where asymmetry is part of the form — the dead tree, broken fence sections, collapsed structures. Turn off when you want the model to look like it was built or grown unevenly. |

### Topology Notes

Meshy produces variable polygon density depending on the prompt complexity and the style preset. For this project's performance budget (300k total scene triangles, targeting iPhone 14+ / flagship Android 2022+):

- After export, check triangle count in Unity's Model Import settings or use a tool like Blender to inspect before import.
- **Player character:** target ~20,000 triangles. Auto-retopologize in Blender (QuadRemesher or Remesh + cleanup) if Meshy output topology is unsuitable for animation deformation.
- **Standard enemies:** target 10,000–12,000 triangles each. Auto-retopologize if joint deformation is poor.
- **Boss (SpinCycle):** target ~25,000 triangles — boss appears alone and warrants the most detail.
- **Environment props:** target 200–600 triangles each. Meshy often over-triangulates flat surfaces — decimating props aggressively is fine.
- Do NOT use simple Decimate on animated characters — it creates chaotic triangle topology that deforms poorly at joints. Use auto-retopology tools instead.
- The auto-UV from Meshy is adequate for Phase 1; repacking UVs is a Phase 3 task.

---

## 3. Acceptance Criteria — What to Check Before Accepting a Model

Run every generated model through these checks before exporting. Reject and regenerate (or refine) if any check fails.

### Silhouette Check (Mandatory — Apply to Every Asset)

The silhouette is the primary read on a mobile screen. Players identify characters and enemies at less than 10% of screen size — shape is all they have.

**The 10% silhouette test:** In Meshy's preview, screenshot the model and scale it down until it occupies roughly 10% of your screen height. View it as a solid black shape (screenshot it, desaturate, flood-fill the model solid black in any image editor). The character must still be immediately identifiable — not "probably a character," but specifically THIS character. If it is ambiguous, reject it and strengthen the prompt before regenerating.

View the model in Meshy's 3D preview. Rotate to a side view and check:
- Does the silhouette match the described proportions? For characters: is the head visibly large relative to the torso? Are the legs short and stocky?
- Is the box head reading as a box — a clear rectangular cube — not a helmet, visor, or rounded mask?
- Does each character have a unique shape that distinguishes it from every other character at a glance? No two characters should share the same silhouette profile.
- Is the overall form readable at a small size? Apply the 10% test described above. The character must be clearly identifiable by shape alone.

**Character silhouette signatures (must be present and distinct):**
- Ninja: tall narrow box head, no hat — box extends high above the shoulders
- Cowgirl: wide-brimmed hat wider than shoulders — the dominant horizontal element
- Skeptic: plain box head, identical height to Ninja — same box, zero decoration (deliberate)
- Gnome Soldier: short round body, small box helmet — overall form is squat and circular
- Dirty Laundry Grunt: drooping sock hat hanging to one side — the only character with a clearly asymmetric head
- SpinCycle Boss: circular drum head wider than the body — the only character with a round head

### Proportion Check (Characters)

The art style guide mandates chibi-adjacent proportions:
- Head (the box): approximately 35% of total character height
- Torso: approximately 30% of total character height
- Legs: approximately 35% of total character height
- Body width: approximately 60% of height — stocky, not thin

If the character is too tall and thin (a common Meshy tendency on biped prompts), add "very stocky compact body, very short legs, oversized head" to the refine prompt and regenerate.

### Box Head Check (Characters Only)

The box on the character's head must:
- Be a recognizable rectangular cube or box shape
- Sit on or slightly over the head, not merged into it as a helmet
- Have flat sides — not curved, not rounded, not organic
- Be proportionally large — oversized relative to the torso

If Meshy generates a rounded helmet or a face-mounted visor instead of a box, the model is not usable. This is the most common failure mode. Strengthen the prompt with "large cardboard box completely enclosing the head, square flat sides, visible box edges and corners" and regenerate.

### Pose Check (Biped Characters)

Characters must be in T-pose or A-pose for Unity rigging:
- Arms extended outward from the body
- No crossed arms, no action poses, no weapon-holding poses
- Legs straight and slightly apart
- If Meshy generates an action pose, use the refine prompt: "T-pose, arms extended horizontally, no action pose, rigging reference pose."

### Poly Count Check

After export, inspect in Blender or Unity:
- Player character: target ~20,000 triangles; auto-retopologize if topology is unsuitable for animation
- Standard enemy characters: target 10,000–12,000 triangles; auto-retopologize if needed
- Boss character (SpinCycle): target ~25,000 triangles
- Props: accept up to 800 triangles (decimate aggressively if needed)

### Texture Check

Inspect the albedo/diffuse texture Meshy generates:
- Does the color roughly match the target palette for this asset?
- Is there visible surface detail (grain, hand-drawn lines) or is it a flat gradient?
- Meshy textures will always need repainting to match the cardboard-and-marker style. The check here is only: is the underlying UV and form usable as a base?

---

## 4. Category 1 — Player Character

### Asset 1: Player Character — The Ninja (Unboxed Hero)

**Unity asset name:** `chr_player_ninja.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON

#### Meshy Text Prompt

```
Stocky chibi humanoid in T-pose, arms extended horizontally, game-ready biped. Strong distinct silhouette: tall rectangular box head rising high above shoulders — clear readable shape at any size. Large dark navy blue cardboard box completely enclosing the head — flat-sided cube with visible square edges and corners. Purple hand-drawn marker lines on the box front form a ninja mask: angular eye cutouts and radiating lines. Box is oversized, one third of total character height. Body is very short and wide: thick short legs, compact torso, wide shoulders, no visible neck. Dark cloth wrappings. Hands OPEN — fingers naturally extended, all five individually modeled, not gripping. Stylized cartoon game character, hand-painted texture, chibi proportions, exaggerated proportions.
```

#### Art Direction Notes

- The box head is the most important element. It must read as a box immediately — four flat sides, visible edges and corners. If it looks like a rounded helmet or a visor, reject it.
- Marker lines on the box face should look drawn-on, not extruded as 3D geometry.
- The body should be visibly stocky — arms and legs short relative to the torso width. Meshy tends toward taller, thinner humanoids by default; if proportions are off, add "extremely short legs, very wide body, head larger than torso" to the refine prompt.
- Dark navy (`#1A1A2E`) is the target box color. Meshy may interpret this as black — if the box looks pure black with no blue, note it for the texturing pass.
- T-pose is required. Do not accept an action pose or a crossed-arm rest pose.
- Hands must be open with separated fingers — closed fists cannot be rigged for weapon-hold animations. This is a hard reject condition.

#### Post-Processing Notes

- In Blender: verify the armature Meshy generates has hip, spine, shoulder, upper arm, forearm, hand, thigh, shin, and foot bones at minimum. Delete any extra bones Meshy added around the box head — the box does not need to be rigged separately.
- Rename the root bone to `Root` and the hips bone to `Hips` before importing to Unity for Humanoid rig mapping.
- The Meshy albedo texture will be approximately correct in color but will lack the corrugation lines and marker details. Replace the box face texture with a hand-painted version referencing the art style guide before the Sprint 6 art pass is considered complete.

---

## 5. Category 2 — Enemy Characters

### Asset 2: The Skeptic (Priority — Phase 1 Boss)

**Unity asset name:** `chr_skeptic.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON

#### Meshy Text Prompt

```
Stocky chibi humanoid in T-pose, arms extended horizontally, game-ready biped. Strong distinct silhouette: same tall rectangular box head as the ninja but entirely plain — no hat, no decoration, clear readable shape. Very short legs, wide stocky torso, large head relative to body. Wears a plain undecorated grey cardboard box completely enclosing the head — flat grey cube with visible square edges and corners, no marker lines, no decoration of any kind. Plain grey jacket and grey trousers — entirely flat grey, no patterns, no warm tones. No accessories, no weapons. Deliberately boring and visually dull. Hands OPEN — fingers naturally extended, all five individually modeled, not gripping. Stylized cartoon game character, hand-painted texture, chibi proportions, exaggerated proportions.
```

#### Art Direction Notes

The Skeptic's defining characteristic is the deliberate absence of personality. Review every generated variant for this:

- The box head must have zero decoration — no lines, no marks, no color variation. If Meshy adds any face lines or texture details to the box, those must be removed during the texturing pass.
- The grey palette must be flat and institutional — not stylish "charcoal" grey, not "cool blue-grey," not "metallic silver." Flat warm grey (#787878 target).
- Clothing must be completely grey with no warm tones. Meshy often adds subtle warm undertones to fabric — flag this for correction in the texture pass.
- The body proportions must match Asset 1 (the player character). Generate the two models back to back and compare in Meshy's preview. If the Skeptic appears noticeably taller or thinner, use the refine prompt: "same extremely stocky chibi proportions, identical height to reference character, very short legs."
- The existing player model (`Meshy_AI_biped_Character_output.fbx`) is the proportion reference — it is a white/light-colored biped in roughly T-pose. The Skeptic must be the same height and build, just grey.
- Hands must be open with separated fingers — closed fists cannot be rigged for weapon-hold animations. This is a hard reject condition.

#### Post-Processing Notes

- Remove any warm color tones from the albedo texture before import. In any image editor, desaturate the texture fully, then add a very slight warm grey tint to avoid the texture reading as cool concrete-grey.
- The target albedo color for all surfaces is approximately #787878. The box head in particular must be this color — flat, no variation.
- Do not add a normal map to this character in Phase 1. The box head in particular should look flat and dimensionless — any surface detail on the box face undermines the "no imagination" visual message.
- Bone structure and naming: apply the same convention as Asset 1 (`Root`, `Hips`, etc.) so the same Animator Controller can be retargeted to both characters.

---

### Asset 3: Gnome Soldier (Phase 1 Standard Enemy)

**Unity asset name:** `chr_gnome_soldier.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON

#### Meshy Text Prompt

```
Stocky chibi humanoid in T-pose, arms extended horizontally, game-ready biped. Strong distinct silhouette: short squat round body, small box helmet sitting low on a wide circular torso — clearly squat and circular compared to taller box-headed characters. Short round body resembling a garden gnome reimagined as a soldier. Simple grey-toned armor made of cardboard plates strapped with tape. A small dull grey cardboard box worn as a rough helmet. Flat grey outfit underneath, no vivid colors — entirely muted grey and grey-brown. No decoration on armor or helmet. Hands OPEN — fingers naturally extended, all five individually modeled, not gripping. Stylized cartoon game character, hand-painted texture, chibi proportions, exaggerated proportions, clear readable shape.
```

#### Art Direction Notes

- Gnome Soldiers are generic Unimaginative foot soldiers — they should feel like variations on the same grey theme as The Skeptic, just slightly more armored.
- The "cardboard armor" should look like plates cut from cardboard boxes and strapped on — flat surfaces, visible tape strips, no metallic sheen.
- These will appear in groups, so the silhouette must be clearly different from The Skeptic at a glance. The rounded gnome-ish form helps with this.
- Smoothness: 0 on the armor material. Flat matte cardboard only.
- Hands must be open with separated fingers — closed fists cannot be rigged for weapon-hold animations. This is a hard reject condition.

#### Post-Processing Notes

- Same bone naming convention as Assets 1 and 2.
- Target 10,000–12,000 triangles. Gnome Soldiers appear in groups so target the lower end (~10k); auto-retopologize if joint deformation is poor.

---

### Asset 4: Dirty Laundry Grunt (Zone Enemy)

**Unity asset name:** `chr_dirty_laundry_grunt.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON

#### Meshy Text Prompt

```
Stocky chibi humanoid in T-pose, arms extended horizontally, game-ready biped. Strong distinct silhouette: drooping sock hat sagging far to one side — the only asymmetric head shape among all characters. Very short legs, wide round torso, large head. Made entirely of dirty laundry: grey sock worn as a helmet drooping heavily to one side, mismatched wrinkled shirt, crumpled stained trousers. All fabric visibly wrinkled and stained. Muted grey-brown and dingy off-white palette — no vivid colors. Flat cardboard box face inside the sock helmet. Hands OPEN — fingers naturally extended, all five individually modeled, not gripping. Stylized cartoon game character, hand-painted texture, chibi proportions, exaggerated proportions, clear readable shape.
```

#### Art Direction Notes

- This enemy is an Unimaginative soldier variant — the same grey dullness as the Gnome Soldier but expressed through discarded laundry rather than cardboard armor. No vivid colors anywhere on this character.
- The drooping sock helmet is the signature silhouette element. It must read as a sock — soft, floppy, drooping to one side — not a rigid helmet. If Meshy produces a stiff sock, refine with "soft drooping fabric sock hat, hanging limply to one side."
- The sullen expression on the cardboard box face should be minimal — a downward-curved marker line for a mouth, small flat eyes. Less expression, not more.
- Wrinkles and creases are the primary texture detail. The fabric must look unlaundered and neglected — not stylishly distressed, just dirty and forgotten.
- Palette: muted grey-brown (#7A7268), dingy off-white (#C8C4BE), with very faint stain marks in slightly darker grey. No warm tones, no color contrast.
- Hands must be open with separated fingers — closed fists cannot be rigged for weapon-hold animations. This is a hard reject condition.

#### Post-Processing Notes

- Same bone naming convention as Assets 1, 2, and 3 (Root, Hips, etc.).
- Target 10,000–12,000 triangles. Target the lower end (~10k) — these appear in groups alongside Gnome Soldiers.
- The albedo texture will need a stain pass: add subtle darker-grey blotch marks in an image editor to sell the "dirty laundry" read that Meshy's auto-texture may not capture.

---

### Asset 5: SpinCycle Boss (Zone 2 Boss)

**Unity asset name:** `chr_spincycle_boss.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON

#### Meshy Text Prompt

```
Large muscular cartoon brawler in T-pose, game-ready biped, boss character. Strong distinct silhouette: massive circular drum head wider than the body — the only character with a round head, immediately distinguishable from all box-headed characters. Cylindrical front-loading washing machine drum head with circular glass porthole centered on the front, chrome ring border. Head large and imposing relative to body. Thick muscular body: wide barrel chest, chunky arms, heavy legs. Torn grey shorts, ripped grey sleeveless vest. Mismatched sneakers — one blue, one red. Hands OPEN — thick fingers extended, all five individually modeled, not gripping. Larger than standard chibi, imposing and wide. Cartoon stylized game character, hand-painted texture, exaggerated proportions, clear readable shape.
```

#### Art Direction Notes

- The washing machine drum head is the central design element and must be unmistakable. It must read as a front-loading washing machine drum: cylindrical, with a clearly circular porthole window on the front, and a visible chrome ring border around the porthole. If Meshy generates any other head shape — a box, a sphere, a helmet — reject it.
- The body should feel like a brawler: wide, thick, powerful. Much larger and more imposing than the standard Gnome Soldier or Dirty Laundry Grunt. This is a boss character and must fill significantly more screen space.
- The mismatched sneakers (one blue, one red) is a texture variant, not a geometry difference. If Meshy generates both sneakers as the same color, note it for the texturing pass.
- The porthole window face: Meshy may add facial features inside the porthole (eyes, mouth). This is acceptable — the porthole can serve as the face. If Meshy generates eyes and expression lines inside the circular porthole, that reads correctly for the character.
- Symmetry ON for the body geometry. The sneaker color difference is handled at texture, not mesh level.
- Hands must be open with thick separated fingers — closed fists cannot be rigged for weapon-hold animations. This is a hard reject condition.

#### Post-Processing Notes

- CRITICAL: The drum head must be a SEPARATE MESH OBJECT from the body in the exported FBX. In Unity, `transform.Rotate` is called on the drum head independently to simulate a spinning drum. If Meshy merges the drum head and body into one mesh, the head must be separated in Blender before import (Edit Mode > select drum head faces > P > Separate by Selection). Verify this separation before import.
- Same bone naming convention as Assets 1–4. The drum head does NOT need its own bone — rotation is driven by script on the Transform directly, not via the rig.
- Target ~25,000 triangles (boss character, highest detail allocation). Auto-retopologize and preserve the porthole rim geometry — the circular porthole must hold its shape cleanly.
- The porthole glass should use the project's emissive material variant in Unity to give it a subtle inner glow — flag this for the material assignment step.

---

### Asset 6: Cowboy Character (Player Class Variant)

**Unity asset name:** `chr_cowboy.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON

#### Meshy Text Prompt

```
Stocky chibi humanoid in T-pose, arms extended horizontally, game-ready biped. Strong distinct silhouette: very wide cardboard cowboy hat brim extending beyond the shoulders — the dominant horizontal element, distinguishable from all other characters by hat width alone. Very short legs, wide stocky torso, large head. Cardboard cowboy hat with extra-wide brim, marker-drawn brim lines, tan cardboard color. Denim jacket with marker-drawn fringe lines on sleeves. Tall boots with marker spur details. Sheriff star badge on chest. Warm tan and denim blue palette. Hands OPEN — fingers naturally extended, all five individually modeled, not gripping. Stylized cartoon game character, hand-painted texture, chibi proportions, exaggerated proportions, clear readable shape.
```

#### Art Direction Notes

- This is a player class variant — same build and proportions as Asset 1 (The Ninja). Generate back-to-back with Asset 1 and compare height and width in Meshy's preview before accepting. If the Cowboy reads as taller or thinner, refine with "identical extremely stocky chibi build, same height as ninja character, very short legs, very wide torso."
- The cardboard cowboy hat is the primary silhouette marker. The wide brim must be clearly readable from the game's top-down camera angle — from above, the hat brim should be the widest element on the character. If the brim is too narrow, refine with "very wide cowboy hat brim, wider than the character's shoulders."
- All Western details (fringe, spurs, sheriff star) should look marker-drawn on the surface, not modeled as 3D geometry. Meshy may extrude some of these as geometry — note for the texture correction pass if so.
- Palette: warm tan (`#C8A96E`) for the hat and boots, denim blue (`#5B7FA6`) for the jacket, medium brown for the cardboard face. The star badge is a marker-yellow drawn shape on the jacket chest.
- Hands must be open with separated fingers — closed fists cannot be rigged for weapon-hold animations. This is a hard reject condition.

#### Post-Processing Notes

- Same bone naming convention as Assets 1–5. The Animator Controller from the Ninja should be retargetable to this character — verify Humanoid rig mapping matches.
- Target 10,000–12,000 triangles.
- The hat brim may cause UV stretching at the underside — flag for a UV unwrap pass in Blender if the brim texture looks heavily distorted.

---

## 6. Category 3 — Environment Props

### Asset 4: World Tree (Hero Prop — Long-Range Goal)

**Unity asset name:** `env_world_tree.fbx`
**Meshy Style:** Cartoon
**Symmetry:** OFF

#### Meshy Text Prompt

```
A massive ancient tree with a thick gnarled trunk and wide-spreading branches, stylized game prop.
The tree should look enormous — large enough to dwarf a small human figure. The trunk is deeply
textured with visible bark ridges and roots at the base spreading outward. The bark surface looks
like corrugated cardboard — layered ridges running vertically, warm tan and brown tones. Branches
spread wide and upward, thick near the trunk and tapering to thinner ends. The tree has no leaves
in its drained state — bare branches only, silhouetted against empty sky. Slightly asymmetric and
organic in form, not perfectly symmetrical. Low polygon faceting visible on the bark — stylized
not photorealistic. Tearaway game aesthetic, hand-painted texture, cartoon stylized tree prop.
```

#### Art Direction Notes

- This is the most important environment prop in the entire game. It will appear at a distance in Phase 2 zones as a narrative horizon element. Its silhouette must be immediately readable and iconic — wide, ancient, enormous.
- The corrugated-cardboard bark texture is the key visual link between this tree and the rest of the game's craft aesthetic. The bark should feel like it is made of stacked corrugated cardboard layers.
- For Phase 1, the World Tree does not appear in gameplay — generate this now for narrative reference art and to establish the visual benchmark. It will be placed in the Backyard as a distant silhouette in Phase 2.
- Symmetry is OFF — the tree should have organic asymmetry. If Meshy generates a perfectly mirrored tree, use the refine prompt: "asymmetric gnarled branches, irregular trunk shape, not mirrored."

#### Post-Processing Notes

- This prop does not need a rig. Delete any armature Meshy adds.
- UV scale: the bark texture should tile across the trunk surface. If Meshy generates a stretched texture on the trunk, plan a UV unwrap pass in Blender.
- For Phase 1 placeholder use: a single low-res version at 600 triangles max. The full-quality version is a Phase 3 asset.

---

### Asset 5: Dead Apple Tree (Backyard Prop — Drained State)

**Unity asset name:** `env_tree_dead_drained.fbx`
**Meshy Style:** Low Poly
**Symmetry:** OFF

#### Meshy Text Prompt

```
A dead bare apple tree prop, game-ready low polygon 3D model. No leaves, bare branches only.
The trunk is thin to medium width, slightly gnarled and twisted. Branches reach upward and outward
from the trunk, forking into smaller branches. The overall form is tall and reaching, silhouetted
shape. Surface texture is grey-brown weathered bark, dry and lifeless. No leaves, no buds, no fruit.
The tree looks like it has been dead for years — bark is cracked and dry. Overall height suggests a
tree that was once a full apple tree, now reduced to bare form. Faceted low-poly geometry on the
trunk and branches — stylized not photorealistic. Cartoon stylized dead tree prop.
```

#### Art Direction Notes

- Silhouette is everything for this prop. It needs to read clearly at game camera distance as "dead tree."
- Branches should have generous negative space between them — this tree sits in the corner of the backyard arena and doubles as a visual landmark. If the branching is too dense, the silhouette becomes unreadable.
- The slightly twisted trunk adds character without needing high polygon count.
- In the reclaimed state (Phase 3), this tree transforms into a cherry blossom — the base mesh will be repurposed. Keep the trunk and major branch structure clean enough to serve as a base for adding blossom geometry later.

#### Post-Processing Notes

- No rig needed. Delete any armature.
- Target 400 triangles after decimation. Low Poly preset on Meshy should get close.
- The grey-brown bark texture from Meshy will need its warmth reduced to match the drained palette (#A89F94 to #6B6560 range).

---

### Asset 6: Wooden Fence Section (Modular Arena Boundary)

**Unity asset name:** `env_fence_section_drained.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON

#### Meshy Text Prompt

```
A modular wooden fence section, game-ready low polygon 3D prop. Three or four vertical wooden
planks attached to two horizontal rails. The wood is weathered and grey — sun-bleached, drained of
color. Plank heights are slightly uneven. The planks have visible grain lines running vertically.
Small nail or screw heads visible at the joints where planks meet rails. The wood surface is rough
and dry — no paint, no varnish. Slightly warped plank edges. Muted grey tones, no warm brown.
Hard-edged faceted geometry, flat shading, stylized prop. The fence section is approximately the
width of two plank widths combined, designed to tile as a repeating modular unit. Low poly cartoon
stylized fence prop, hand-painted texture.
```

#### Art Direction Notes

- This prop tiles to form the entire arena boundary. Ensure the left and right sides of the mesh are clean vertical edges — any overhang or taper will create visible gaps when multiple sections are placed end to end.
- The warped plank tops add visual variety but should not break the tiling. Check in Meshy's preview that the top profile is irregular but the base is flat.
- Grey-brown wood only. If Meshy generates warm brown wood, note it for the texture correction pass.

#### Post-Processing Notes

- No rig needed.
- Target 150–200 triangles. Low Poly preset should achieve this.
- Snap the bottom vertices to Y=0 in Blender before import so the fence section sits flush on the ground plane without requiring manual placement adjustment in Unity.

---

### Asset 7: Collapsed Garden Shed (Background Prop)

**Unity asset name:** `env_shed_collapsed_drained.fbx`
**Meshy Style:** Low Poly
**Symmetry:** OFF

#### Meshy Text Prompt

```
A collapsed small wooden garden shed, game-ready low polygon 3D prop. The shed has partially
fallen over — one wall leaning inward, the roof sagging. Grey weathered wooden walls, corrugated
metal roof panels that are grey and slightly rusted. The structure looks like it was once a small
tool shed, now abandoned and partially collapsed. Door is hanging open or missing. Grey muted tones
throughout — grey wood walls, grey corrugated metal roof. No warm colors. Hard-edged faceted
geometry, flat shading. Background prop for a game environment — not the main focal point. Low poly
stylized cartoon prop.
```

#### Art Direction Notes

- This is a background prop — it sits behind the play area and frames the scene. It does not need high detail.
- The corrugated metal roof is a visual callback to the corrugated cardboard aesthetic — same ridged pattern, different material. Meshy should produce this naturally from the "corrugated metal" description.
- Asymmetry is intentional — Symmetry is OFF. The collapse should look organic and one-sided.

#### Post-Processing Notes

- No rig. Target 300–500 triangles.
- This prop should be placed behind the fence boundary and will rarely be seen up close. Aggressive decimation is fine.

---

## 7. Category 4 — Weapons and Items

### Asset 8: Broomstick (Bo Staff / Tier 1 Weapon)

**Unity asset name:** `env_weapon_broomstick.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON

#### Meshy Text Prompt

```
A wooden broomstick, game-ready 3D prop. Long thin wooden handle, slightly worn and scratched.
Broom head attached at one end with bound bristles — the bristles form a rough fan shape, slightly
spread. The wood is warm tan and brown. Simple rounded cylinder handle, smooth but with slight grain
texture. The broom end bristles are made of straw or dried grass, bound with a string wrap. Overall
length is approximately 1.5 meters. Clean simple prop shape. Stylized cartoon game item, hand-painted
texture. Symmetric about its long axis.
```

#### Art Direction Notes

- This doubles as a bo staff when the player picks it up — the form should look like it could be wielded as a weapon, not just a cleaning implement.
- The bristle end gives it visual interest at one tip. The balance of the handle should feel weapon-like — long, straight, held in the middle.
- Warm natural wood tones are correct here. This is a real-world object before it gets "imagined" into a weapon — its mundane appearance is part of the game's design.

#### Post-Processing Notes

- No rig. Ensure the pivot point is at the center of the handle (middle of the length) for correct rotation in Unity when the player holds it.
- Target 200 triangles.

---

### Asset 9: Cardboard Tube (Katana / Tier 2 Weapon)

**Unity asset name:** `env_weapon_cardboard_tube.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON

#### Meshy Text Prompt

```
A cardboard tube, game-ready 3D prop. A hollow cylindrical tube made from rolled cardboard, similar
to a paper towel inner roll but longer — approximately 80 centimeters long. The tube is light tan
cardboard color, slightly compressed at one end from handling. Visible cardboard layering at the cut
ends — the spiral winding of the cardboard construction is visible. Surface texture is rough brown
cardboard. Slight bend or compression along the length — not perfectly rigid. Simple prop shape.
Stylized cartoon game item, hand-painted texture.
```

#### Art Direction Notes

- This is the Tier 2 melee weapon — when imagined with the Ninja Box, it becomes a katana. The real-world form should look like a kid's cardboard tube weapon, which is exactly right.
- The spiral cardboard layering at the ends is a key visual detail — it establishes that this is a cardboard tube, not a wooden stick or plastic pipe.
- Keep the form simple. This object is picked up in the world and its identity needs to be instantly readable.

#### Post-Processing Notes

- No rig. Set pivot at the center along the length.
- Target 100–150 triangles. A tube is a very simple form and Meshy should not over-triangulate it.

---

### Asset 10: Ruler (Shuriken / Tier 1 Ranged Weapon)

**Unity asset name:** `env_weapon_ruler.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON

#### Meshy Text Prompt

```
A wooden school ruler, game-ready 3D prop. Flat rectangular plank approximately 30 centimeters long,
1 centimeter thick, and 3 centimeters wide. Light tan wood color. Measurement markings and numbers
printed along the length. Rounded corners. A small hole at one end. Flat clean surface. Simple
geometric prop. Low poly hard-edged shape. Stylized game item.
```

#### Art Direction Notes

- This is both a pickup item and the visual seed of the shuriken ranged attack. The measurement markings on the surface sell the "school ruler" identity.
- The flat rectangular form is critical — it needs to look like something a kid would throw, which a ruler naturally does.

#### Post-Processing Notes

- No rig. Pivot at the center.
- Target 50–80 triangles. This is a very simple flat shape.

---

## 8. Appendix A — Acceptance Checklist

Run this checklist before exporting any model from Meshy. Do not import to Unity until all applicable checks pass.

| Check | Characters | Props | Pass Condition |
|---|---|---|---|
| 10% silhouette test — solid black shape | Required | Required | Screenshot the model, scale to 10% screen height, flood-fill solid black. Asset must be immediately identifiable by shape alone. Hard reject if ambiguous. |
| Unique silhouette — distinct from all other assets | Required | N/A | Each character must have a different silhouette profile. No two characters share the same head shape or overall outline. Compare against all previously accepted characters before passing. |
| Box head reads as a box | Required | N/A | Four flat sides, visible square edges, not a helmet or visor |
| Chibi proportions | Required | N/A | Head ~35% height, stocky body, short legs |
| T-pose or A-pose | Required | N/A | Arms extended, no action pose |
| Symmetry on characters | Required | N/A | Left/right halves mirror correctly |
| Poly count within budget | Required | Required | Player ~20k tri; standard enemies 10–12k tri; SpinCycle ~25k tri; props under 800 tri |
| Correct color family | Required | Required | Skeptic grey; player navy; props natural/grey-brown |
| No photorealistic textures | Required | Required | Cartoon or hand-painted quality, not photo-render |
| Pivot point correct | N/A | Required | Props: center or base as appropriate for placement |
| Hands open (biped characters only) | Required | N/A | All five fingers separated, extended, not gripping — hard reject if closed fist |

---

## 9. Appendix B — Unity Asset Delivery Paths

When handing off exported files for Unity import, place them at these paths:

| Asset | Delivery Path |
|---|---|
| `chr_player_ninja.fbx` | `Assets/_Project/Art/Characters/` |
| `chr_skeptic.fbx` | `Assets/_Project/Art/Characters/` |
| `chr_gnome_soldier.fbx` | `Assets/_Project/Art/Characters/` |
| `env_world_tree.fbx` | `Assets/_Project/Art/Environment/` |
| `env_tree_dead_drained.fbx` | `Assets/_Project/Art/Environment/` |
| `env_fence_section_drained.fbx` | `Assets/_Project/Art/Environment/` |
| `env_shed_collapsed_drained.fbx` | `Assets/_Project/Art/Environment/` |
| `env_weapon_broomstick.fbx` | `Assets/_Project/Art/Props/` |
| `env_weapon_cardboard_tube.fbx` | `Assets/_Project/Art/Props/` |
| `env_weapon_ruler.fbx` | `Assets/_Project/Art/Props/` |

Meshy texture packages (the ZIP download) should be extracted alongside the FBX and renamed to match the FBX using the `tex_chr_` or `tex_env_` naming convention from the art style guide.

---

## 10. Appendix C — Common Meshy Failure Modes and Fixes

| Problem | Symptom | Fix |
|---|---|---|
| Silhouette fails 10% test | Asset unrecognizable as a solid black shape at small size | Strengthen the prompt's signature shape element: wider hat brim, taller box, droopier sock. Add "strong distinct silhouette, exaggerated proportions, clear readable shape" to the refine prompt. |
| Two characters have identical silhouettes | Cannot tell characters apart from shape alone | Identify the signature element that should differ (hat width, head shape, body bulk) and push it further in the refine prompt. Compare accepted models side by side in Meshy before finalizing. |
| Box head becomes a helmet | Rounded, visor-like head covering | Add to refine: "cardboard box completely enclosing head, square flat sides, visible box corners and edges, not a helmet" |
| Character too tall and thin | Proportions look like a normal humanoid | Add to refine: "extremely short stocky legs, very wide compact torso, oversized head, chibi proportions" |
| Character in action pose | Not T-pose | Add to refine: "T-pose, arms extended horizontally at shoulder height, standing straight, rigging reference pose" |
| Skeptic too interesting | Box has lines or texture details | Remove texture details in post; add to refine: "completely plain grey box, zero decoration, no lines on box surface" |
| Prop over poly budget | Triangle count too high | Decimate in Blender using Decimate modifier at ratio 0.4–0.6, check for face merging artifacts afterward |
| Meshy adds warm tones to Skeptic | Grey clothes look tan or brown | Desaturate albedo texture in Photoshop/GIMP; apply grey color correction |
| Asymmetric character | One arm longer than the other | Enable Symmetry in Meshy settings; or fix in Blender with mirror modifier |

---

_Meshy AI Prompt Reference v2.0 — 2026-07-14_
_Replaces v1.0 Midjourney/Stable Diffusion document. All prompts in this document are written for Meshy AI Text to 3D only._

---

> **V2 Cul-de-Sac content** (enemies 11–13, ENV props 14–25, animation pipeline) has been moved to `docs/meshy-prompts-v2.md`.


_Meshy AI Prompt Reference v2.0 — 2026-07-14_
_V1 Backyard Zone prompts only. See `docs/meshy-prompts-v2.md` for Cul-de-Sac (V2) content._
