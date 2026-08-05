# BoxForged — Meshy Prompts V2: Cul-de-Sac Zone

**Version:** 2.0
**Zone:** The Cul-de-Sac (Phase 2, Zone 1)
**Tool:** Meshy AI (meshy.ai) — Text to 3D
**Art direction:** Warm amber/terracotta/ochre palette. Corrugated cardboard grain throughout. Wild West main street layered over cracked suburban cul-de-sac. Marker-drawn details on all surfaces.

> **V1 prompts** (Backyard Zone — characters, Backyard ENV props, Phase 1 weapons) live in:
> - `docs/ai-art-prompts.md` — characters (Assets 1–6) and Backyard ENV (Assets 4–7)
> - `docs/meshy-weapon-prompts.md` — Phase 1 weapons (1–10)

---

## Global Settings — Cul-de-Sac Assets

| Setting | Value |
|---|---|
| **Palette** | Burnt amber, terracotta, ochre, saddle brown, faded teal, weathered tan. Warm shadows. |
| **Texture** | 512×512 diffuse only. No normal or roughness maps in Phase 2. |
| **Grain** | Corrugated cardboard texture on all wood and plank surfaces. |
| **Outlines** | Marker-drawn lines on all surface edges and details. |
| **Character delivery** | `Assets/_Project/Models/Characters/` |
| **ENV delivery** | `Assets/_Project/Models/Environment/CulDeSac/` |
| **Weapon delivery** | `Assets/_Project/Models/Weapons/` |

---

## Part 1 — Enemy Characters

### Asset 11: Wagon Wheel Roller (Zone 2 Introductory Enemy)

> **Design note:** Originally Tumbleweed Roller. Replaced in sprint 11 — the tumbleweed's organic branch tangle could not be generated reliably by Meshy. Wagon Wheel Roller fills the same gameplay role (no-limbs rolling charge enemy) with a shape Meshy can produce consistently.

**Unity asset name:** `chr_wagon_wheel_roller.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON

#### Meshy Text Prompt

```
A large wagon wheel creature, game-ready static mesh, upright standing position. Diameter
approximately 0.6 meters. Classic wooden spoked wheel: thick outer iron rim, eight wooden spokes
radiating from a central hub, wide hub cap at center. The wood surfaces show visible corrugated
cardboard grain texture — layered ridges running along each spoke and plank of the rim. Iron rim
has a dark gunmetal sheen with marker-drawn edge lines. Two small round eyes set into the front
face of the hub cap, amber-yellow irises, simple dot pupils — the only personality marker on an
otherwise utilitarian object. The wood palette is warm saddle brown and ochre; iron rim is dark
grey-black. All surface edges have bold marker-drawn outlines. Hard faceted low-poly geometry.
No limbs, no face features beyond the hub eyes. Wheel rests flat on the bottom rim as if parked
upright. Stylized game prop, cardboard-and-marker Western aesthetic, cartoon stylized.
```

#### Art Direction Notes

- The wagon wheel silhouette must read immediately and clearly from the game's top-down camera angle — the spokes radiating from a central hub are the key readable element. If Meshy produces a wheel that looks like a disc without spoke gaps, reject and add "eight visible spokes with open gaps between them, negative space visible through wheel."
- The hub eyes are the only personality detail. They should be subtle — a player notices them on second look, not at a glance. If Meshy makes the eyes prominent, note for the texture correction pass.
- Symmetry ON — the wheel is geometrically symmetric and rolling contact points must be even.
- Palette matches the Cul-de-Sac warm zone: saddle brown, burnt ochre, dark gunmetal. No cool greys on the wood surfaces.
- No hand rigging — this enemy has no limbs. Rolling animation is driven procedurally in Unity.
- The iron rim should be visibly darker than the wooden spokes to give a strong silhouette ring around the outside. If the rim blends into the spokes, add "dark iron rim clearly contrasting against lighter wooden spokes" to the refine prompt.

#### Post-Processing Notes

- No armature needed. Delete any rig Meshy generates.
- Target 300–400 triangles after decimation. The wheel is a simple geometric shape — Low Poly preset should produce a clean result near budget.
- Pivot at the geometric center of the wheel (hub center) — this is the rotation axis for the rolling animation. Verify in Blender before export.
- The hub eyes should be painted into the texture, not modeled as geometry. If Meshy extrudes them, flatten in the texture pass.
- The amber glow in the eyes is added in Unity as an emissive material property, not baked into Meshy's texture.

---

### Asset 12: Hitching Hound (Zone 2 Flanking Enemy)

**Unity asset name:** `chr_hitching_hound.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON

#### Meshy Text Prompt

```
A dog-shaped creature made entirely of braided rope and cord, game-ready biped-quad in standing
four-legged pose, all four legs on the ground, T-pose equivalent for rigging. Dog proportions but
constructed of thick twisted rope: the head is a large loop of rope tightened into a rounded snout
shape, two small knot-eyes, no visible fur — only braided cord texture. Body is thick coiled rope
forming haunches, barrel chest, and shoulders. Legs are rigid rope columns. Tail is a large lasso
loop curling upward. Color palette: natural manila rope tan, saddle brown cord, with faded teal
accent rope woven through. Marker-drawn lines emphasize the braid pattern. Chunky stylized cartoon
game character, hand-painted texture, cardboard-and-marker aesthetic.
```

#### Art Direction Notes

- The rope construction must be immediately readable as rope, not fur or skin. The key details: visible braid lines on every surface, a knot-snout rather than a sculpted muzzle, and the lasso-loop tail. If any of these read as organic animal anatomy instead of rope, reject the variant.
- Four-legged standing pose for the static mesh — this is the closest equivalent to a T-pose for a quadruped. All four legs planted, tail loop raised, head level.
- The faded teal accent cord is the subtle Cul-de-Sac palette tie-in — Meshy may not reproduce this accurately, but flag it for the texture pass.
- Body should feel low-slung and compact, like a medium-sized dog. Not a Great Dane silhouette (too tall) and not a Chihuahua silhouette (too small). If proportions drift, add "medium dog size, wide haunches, short legs, compact body" to the refine prompt.
- Symmetry ON — the flanking AI requires symmetric animation, and paired Hounds must match.
- Paws only — no finger rigging. Simple paw mesh is sufficient.

#### Post-Processing Notes

- This model requires a full quadruped rig. Minimum bones: hips, spine, chest, neck, head, four legs with upper/lower/foot, and a tail bone for the lasso loop.
- If Meshy generates an internal armature, inspect it — quadruped auto-rigs from Meshy are often incorrect. Plan a manual rig pass in Blender using the Auto-Rig Pro quadruped template.
- Target 1,200 triangles (higher than most enemies because the rope braid requires additional geometry to read correctly). Decimate cautiously — losing the braid pattern kills the design.

---

### Asset 13: Milepost Marshal (Zone 2 Ranged Enemy)

**Unity asset name:** `chr_milepost_marshal.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON

#### Meshy Text Prompt

```
A tall thin biped creature in arms-extended active pose, game-ready. Extremely elongated thin body —
pole-like torso, very long thin legs, minimal visible hips. No visible neck. The head is an
octagonal stop-sign shape: bright red octagon face with a bold white border stripe and a gold star
badge at the center — flat-sided, no facial features, no mouth, no eyes. Two thin hexagonal-rod
arms extend straight out horizontally from the mid-torso, ending in small flared cone tips like
revolver barrels. Arms fully extended as if active/firing pose. Muted tan and ochre body with
rust-red accent at the badge. Hard faceted low-poly geometry. Marker outlines on all surfaces.
Stylized game enemy, cardboard-and-marker aesthetic, Western sheriff theme.
```

#### Art Direction Notes

- The Milepost Marshal is modeled in its active/arms-extended pose, not T-pose, because it never walks — it stands and rotates in place. The arms should be modeled extended at 90 degrees from the torso, representing the attack position.
- The stop-sign head is the design's central joke and must land immediately: flat octagon, no facial features, gold star. If Meshy rounds the octagon into a circle or adds face details, reject it.
- The body must be visibly pole-thin — when the arms are folded in, this character should read as an ordinary stop sign from a distance. The thin torso and long legs reinforce the "it IS a stop sign" visual gag.
- Palette: the body should be the dull weathered metal of an actual sign post — terracotta and ochre tones, not bright silver. Only the octagon face is red. If Meshy makes the whole body red, note for the texture pass.
- Symmetry ON. The arms must be perfectly symmetric — the attack AI relies on symmetric arm rotation.
- Arms are mechanical rods — no hand rigging. Arms fold/unfold via animation state, not finger bones.

#### Post-Processing Notes

- Rig required: spine, hips, two shoulder joints, upper arm, forearm (the hexagonal rod is the entire arm). The arm fold/unfold animation is the primary animation state — it must rotate cleanly from 90-degree-extended to folded-against-body.
- The octagon face should be a flat-shaded polygon with no depth — any thickness Meshy adds to the sign face should be reduced to near-zero in Blender.
- Target 600 triangles. Low Poly preset should produce fewer — accept up to 800 if the sign face geometry needs extra edge loops.

---

## Part 2 — Environment Props: Cul-de-Sac

All props below share the global Cul-de-Sac settings listed at the top unless noted otherwise.

---

### Asset 14: Covered Wagon

**Unity asset name:** `env_cs_covered_wagon.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON
**Poly budget:** ~600 tris

#### Meshy Text Prompt

```
A pioneer covered wagon, stylized cartoon game prop. Wooden wagon body with flat plank sides,
visible marker-drawn wood grain lines running horizontally. White canvas roof stretched over four
barrel-hoop ribs arching from side to side. Canvas has wrinkled folds and slight marker-drawn
stitching lines. Four large spoked wooden wheels, oversized and slightly cartoony. Dust-caked
chassis. Warm saddle brown and tan palette. Corrugated cardboard texture on all wood surfaces.
A buckboard seat at the front. No horses. Clean silhouette, chunky proportions. Stylized 3D game
prop, cardboard-and-marker aesthetic. Symmetric about the center axis.
```

#### Art Direction Notes

- This is the zone's primary cover object and will appear repeatedly in groups. The silhouette must be instantly readable as "covered wagon" at game camera distance.
- The canvas roof is the signature shape element — the barrel-hoop arch must be clearly visible in profile. If the roof looks flat or tent-like rather than arched, refine with "canvas stretched over curved barrel-hoop ribs, rounded arch profile, not a flat roof."
- Wheels should be visibly oversized relative to the body — this is a cartoon-adjacent prop, not a replica. Four spokes minimum per wheel to read as a wheel at small size.
- No horses or hitching hardware on this model.

#### Post-Processing Notes

- No rig. Snap base to Y=0.
- Wheels can be separate child meshes in the hierarchy if Meshy generates them separately — useful for future rotation animation.
- Target 600 tris total (wagon body + wheels combined). Decimate aggressively on the canvas roof — it is a smooth surface.

---

### Asset 15: Saloon Front Facade

**Unity asset name:** `env_cs_saloon_facade.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON
**Poly budget:** ~800 tris

#### Meshy Text Prompt

```
A Western saloon storefront facade, stylized cartoon game prop. Flat-front building panel, wide
wooden plank boards running vertically, weathered tan paint flaking at edges. A raised front porch
with plank flooring and a simple rail running the full width. Bat-wing saloon doors centered on
the facade — two half-height hinged panels, dark wood, slightly open. A second-floor balcony railing
at the top of the facade — railing only, no floor geometry behind it. Hand-painted sign board above
the doors with thick marker lettering. Corrugated cardboard grain on all wood surfaces. Warm tan
and ochre palette. Chunky stylized proportions. Stylized 3D game prop, cardboard-and-marker
aesthetic. No side walls, flat-panel construction only.
```

#### Art Direction Notes

- This is a facade panel placed against a house front — it has no side walls or interior. The flat-panel construction is intentional and correct. The visible edges should look like cut plywood or cardboard.
- The bat-wing doors are the most important detail. They must read as bat-wing (split down the center, each half-door independently swinging) rather than a full door. If Meshy generates a solid door, reject it and refine with "two separate half-height swing doors split at center, each door panel independent."
- The balcony railing at the top is a suggestion of height — it does not need a floor behind it.
- Sign text: leave the sign board blank or with generic "SALOON" text — sign name variants are applied by texture swap in Unity.

#### Post-Processing Notes

- No rig. This is a static facade prop.
- 800 tris budget is higher than other props because of the bat-wing door geometry. If Meshy over-generates, decimate the plank wall surfaces first.
- The bat-wing doors should be separate child meshes so Unity can animate them opening independently via a simple rotation tween.

---

### Asset 16: Hitching Post

**Unity asset name:** `env_cs_hitching_post.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~150 tris per post

#### Meshy Text Prompt

```
A single Western hitching post, stylized game prop. One vertical wooden post approximately 1.5
meters tall with a horizontal crossbar near the top. Worn and chipped wood, slightly weathered.
Corrugated cardboard grain texture on all surfaces. A rope loop tied loosely to the crossbar,
hanging down. Base is slightly buried — no visible footing hardware. Warm saddle brown and ochre
palette. Hard-edged faceted geometry. Marker-drawn wood grain lines. Stylized low-poly game prop,
cardboard-and-marker aesthetic. Single post only — no paired posts.
```

#### Art Direction Notes

- This is the single-post unit. In Unity, three-post groups are assembled from instances — model only one post with crossbar.
- The crossbar must be clearly visible in silhouette from the game's top-down camera angle.
- The rope loop is a visual detail — it should hang loosely and read as rope, not cable or wire.
- Snap base to Y=0 so posts can be placed directly without vertical offset adjustment in Unity.

#### Post-Processing Notes

- No rig. Target 150 tris per post — Low Poly preset should achieve this easily.
- The rope loop geometry can be as few as 6–8 triangles.

---

### Asset 17: Water Trough

**Unity asset name:** `env_cs_water_trough.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~250 tris

#### Meshy Text Prompt

```
A Western wooden water trough, stylized game prop. Low wide rectangular basin, rough-hewn plank
construction. Planks run horizontally along the long sides. A single wide metal band wraps around
the middle. Slightly weathered, with moss on one short end. Still water visible in the basin — flat
surface, slightly reflective, no animation. Warm saddle brown wood, rust-orange metal band, grey-
green moss patch. Corrugated cardboard grain on the wood surfaces. Marker outlines on edges. Hard-
edged low-poly faceted geometry. Chunky proportions, slightly wider and shorter than realistic.
Stylized game prop, cardboard-and-marker aesthetic.
```

#### Art Direction Notes

- The trough is a wide, low shape — it should read clearly from above (the game's top-down camera). If Meshy generates a tall or narrow trough, refine with "very wide and low, short sides, long rectangular basin, broad footprint."
- The still water surface in the basin should be flat geometry — a single slightly blue-tinted quad. Not an animated water shader (Phase 2 constraint).
- The moss patch is intentional weathering detail — one end only, visually asymmetric without needing Symmetry OFF.

#### Post-Processing Notes

- No rig. Snap base to Y=0.
- Target 250 tris. The basin interior (flat water surface) can be a single quad.

---

### Asset 18: Wanted Poster

**Unity asset name:** `env_cs_wanted_poster.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~80 tris (flat quad with detail texture)

#### Meshy Text Prompt

```
A Western wanted poster mounted on a wood backing board, stylized game prop. Tall rectangular
parchment nailed to a rough plank backing. Torn and frayed edges on the parchment. Aged yellowed
paper texture with visible grain. Large bold "WANTED" text in thick block marker letters at the top.
A bold black silhouette figure drawn at center — humanoid, generic, marker-drawn. A reward amount
in bold text at the bottom. Two nail heads visible where the poster meets the backing board. Warm
aged paper tones, tan and sepia. Slightly curling at the lower corners. Flat quad construction,
minimal depth. Stylized low-poly game prop, cardboard-and-marker aesthetic.
```

#### Art Direction Notes

- This is a flat prop — near-zero depth. The silhouette figure at center should be generic and bold enough to read at small size. It is intentionally ambiguous (it will be the Milepost Marshal silhouette per the GDD, but Meshy does not need to know the specific character).
- Torn edges on the parchment are important for the aged look. If Meshy generates clean straight edges, add "torn and ragged parchment edges, frayed corners" to the refine prompt.
- This prop mounts against a surface (telegraph post, wall). Ensure the back face is clean — no floating geometry.

#### Post-Processing Notes

- No rig. The texture carries all the detail — invest time on the albedo texture pass for this one.
- 80 tris is the ceiling. A flat quad with slight edge bevel is correct and sufficient.

---

### Asset 19: Mailbox Telegraph Office

**Unity asset name:** `env_cs_mailbox_telegraph.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON
**Poly budget:** ~300 tris

#### Meshy Text Prompt

```
A Western telegraph office shack on a post, stylized game prop. A mailbox-sized wooden building
front mounted on a single wooden post at curb height. The building face has a small arched window,
a letter-slot opening at the bottom, and a tiny hand-painted sign reading "TELEGRAPH" in marker
lettering above the window. A small Western-Union-style signal flag on a thin arm extends from one
side — a small rectangular flag in rust red. The post is wood-textured with corrugated cardboard
grain. Warm saddle brown and ochre palette, rust red flag. Chunky stylized proportions, slightly
oversized building head relative to the post. Stylized cartoon game prop, cardboard-and-marker
aesthetic. Symmetric.
```

#### Art Direction Notes

- This is a mailbox reimagined as a tiny telegraph shack — the proportions should feel like a mailbox on a post, but all the surface details are Western. The building-head should be clearly larger than the post (top-heavy, charming).
- The signal flag is a small detail that sells the Western read. If Meshy omits it, note for a geometry add in Blender — it is only a flat rectangle on a thin rod.
- The "TELEGRAPH" text on the sign does not need to be legible at game camera distance. It can be represented as horizontal marker lines that imply text.

#### Post-Processing Notes

- No rig. Snap post base to Y=0.
- Target 300 tris. The building face carries most of the detail in the texture, not the geometry.

---

### Asset 20: Tumbleweed (Static Prop)

**Unity asset name:** `env_cs_tumbleweed_static.fbx`
**Meshy Style:** Low Poly
**Symmetry:** OFF
**Poly budget:** ~400 tris

#### Meshy Text Prompt

```
A dry tumbleweed ball, stylized low-poly game prop, static environmental decoration. Roughly
spherical, approximately 0.5 meters diameter. Constructed of interlocking dry stick branches woven
loosely — gaps and negative space visible throughout the silhouette, not a solid ball. Individual
branches cross and overlap, slightly see-through when viewed against light. No eyes, no creature
features — purely a plant prop. Color palette: dry ochre, bleached tan, burnt sienna. Corrugated
cardboard grain texture on branch surfaces. Marker outlines on branch shapes. Hard faceted low-poly
geometry. Slightly flattened on the bottom from resting on ground. Stylized game prop,
cardboard-and-marker aesthetic.
```

#### Art Direction Notes

- This is the purely decorative static version — no eye sockets, no creature features. Tumbleweeds appear as ambient drifting props in rooms; only the Tumbleweed Roller enemy version (Asset 11) has eyes.
- The see-through silhouette is critical. If Meshy fills in the interior and produces a solid ball, reject it and refine with "loosely woven branches, open gaps between branches, not solid, see-through interior."
- Asymmetry is intentional (Symmetry OFF) — real tumbleweeds are irregular.

#### Post-Processing Notes

- No rig. This is a drifting ambient prop driven by a simple path animation in Unity.
- Target 400 tris. Do not decimate too aggressively — some branch geometry is needed for the see-through silhouette.

---

### Asset 21: Gallows Frame

**Unity asset name:** `env_cs_gallows_frame.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~350 tris

#### Meshy Text Prompt

```
A Western gallows scaffold frame, stylized game prop. T-shaped timber construction approximately
3 meters tall. Thick square-section vertical post, thick horizontal crossbar at the top extending
to one side. Both timbers are chunky and oversized — cartoon-adjacent proportions. A single rope
hangs from the end of the crossbar — loosely coiled at the end, no noose. Corrugated cardboard
grain texture on all wood surfaces. Warm saddle brown and dark ochre palette. Marker-drawn grain
lines on all faces. Hard-edged faceted low-poly geometry. Chunky stylized 3D game prop,
cardboard-and-marker aesthetic. Symmetric about the vertical axis.
```

#### Art Direction Notes

- The gallows frame is placed over the Command Node birdbath in Room 5 — it functions as a navigation obstacle in the boss fight. The silhouette must be readable from above (game camera).
- No noose — the GDD specifies a coiled rope end only. If Meshy generates a noose, remove it in post.
- The "chunky oversized timbers" note is important — this should feel like a cartoon scaffold, not a realistic execution platform.

#### Post-Processing Notes

- No rig. Solid static obstacle.
- Target 350 tris. Square-section timber geometry is efficient — most budget goes to the rope detail.

---

### Asset 22: Rain Barrel / Saloon Barrel

**Unity asset name:** `env_cs_barrel.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~200 tris

#### Meshy Text Prompt

```
A wooden saloon barrel, stylized game prop. Short wide cylindrical barrel with slightly convex
stave sides. Three dark metal bands wrapping horizontally — top, middle, and bottom. Wood staves
running vertically with visible gaps between them suggesting construction. Dark stained warm brown
wood, dark rust-brown metal bands. Corrugated cardboard grain texture on the stave surfaces.
Marker-drawn outlines on the bands and stave edges. Hard-edged low-poly faceted geometry. Can be
placed upright or on its side — model in upright orientation, pivot at center base. Chunky
proportions, slightly wider than tall. Stylized low-poly game prop, cardboard-and-marker aesthetic.
```

#### Art Direction Notes

- This barrel is multipurpose — it appears upright as a container and on its side as a cover object. Model it upright; Unity handles the rotation for on-its-side placement.
- The convex stave sides (the classic barrel bulge) should be readable even at low poly count. If Meshy generates a straight-sided cylinder, add "barrel-shaped bulge in the middle, convex stave sides, classic barrel silhouette" to the refine prompt.
- Metal bands should be clearly distinct from the wood — darker tone, slight sheen suggestion in the texture.

#### Post-Processing Notes

- No rig. Pivot at center of base (Y=0 at the bottom).
- Target 200 tris. Aggressive decimation is fine on the stave cylinder.

---

### Asset 23: Lamp Post (Western)

**Unity asset name:** `env_cs_lamp_post_western.fbx`
**Meshy Style:** Cartoon
**Symmetry:** ON
**Poly budget:** ~250 tris

#### Meshy Text Prompt

```
A tall Western wooden lamp post, stylized game prop. Wooden post approximately 3.5 meters tall,
slightly tapered toward the top. A wrought-iron-style curved bracket extends from near the top,
holding a glass lantern box. The lantern is square-sided with glass panel faces — a hand-drawn
flame texture visible inside through the glass (not animated, just texture). The iron bracket has a
slight S-curve shape. Corrugated cardboard grain texture on the wooden post. The bracket has
hand-drawn marker lines suggesting iron work. Warm saddle brown post, dark grey bracket, amber
lantern glass. Chunky cartoon proportions. Stylized cartoon game prop, cardboard-and-marker
aesthetic. Symmetric.
```

#### Art Direction Notes

- The lantern at the top is the focal detail. It must read as a lantern (square box with glass sides) rather than an electric streetlight globe. If Meshy generates a round globe, reject and refine with "square glass lantern box, four flat glass panel sides, not a round globe."
- The S-curve bracket is what distinguishes this from a modern streetlamp. If Meshy generates a straight horizontal bracket, add "S-curve ornamental iron bracket, curved decorative arm" to the refine prompt.
- The flame inside is texture-only — no VFX geometry needed from Meshy.

#### Post-Processing Notes

- No rig. Snap post base to Y=0.
- Target 250 tris. Most budget goes to the lantern box geometry (8 faces minimum for the box frame).

---

### Asset 24: Command Node Birdbath — Active State

**Unity asset name:** `env_cs_command_node_active.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~400 tris

#### Meshy Text Prompt

```
A cracked stone birdbath on a post, reimagined as a grey signal antenna, stylized game prop. Stone
basin on a pedestal post, basin showing visible crack lines. Flat grey stone — no warm tones, no
cardboard texture (this is an Unimaginative object — deliberately grey and flat). Three angular
geometric panel fins attached to the post below the basin — flat rectangular panels arranged
radially, like antenna fins. Panels are matte grey with faint angular line markings in slightly
darker grey. A faint grey static-residue texture in the basin — ashy, not wet. Desaturated grey
throughout. Hard-edged low-poly faceted geometry. Stylized game prop. No cardboard grain — the
grey panels are smooth and characterless by design.
```

#### Art Direction Notes

- This is the one asset in this zone that deliberately breaks the cardboard-and-marker aesthetic. The Unimaginative's technology has no corrugation, no warmth, no grain. The grey panels should feel alien and wrong in the context of the warm Wild West scene around it.
- The stone basin should still read as a birdbath — familiar shape, but cracked and grey.
- The antenna fin panels (3 total, radiating from the post) are the visual signal that this is enemy infrastructure.
- This is the Active state. The Destroyed state is handled by deactivating the fin panel child objects in Unity — model the fins as separate children in the hierarchy.

#### Post-Processing Notes

- No rig. Fins should be separate child meshes so Unity can toggle them off on the Destroyed state trigger.
- Target 400 tris total (basin + post + 3 fin panels). Fins are flat quads — they add minimal tri count.
- Apply a flat grey material (no corrugation normal, no grain overlay) — this asset intentionally uses the Unimaginative material variant.

---

### Asset 25: Saloon Sign Board

**Unity asset name:** `env_cs_saloon_sign.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~100 tris

#### Meshy Text Prompt

```
A horizontal rectangular wooden sign board, stylized game prop. Rough-cut plank edges, slightly
uneven and aged. Board surface is weathered tan wood with visible grain. Thick marker lettering
fills the face — bold block letters, hand-painted look, slightly imperfect letter spacing. Two rope
segments hang from holes at the top corners, suggesting the sign hangs from a bracket above.
Slightly tilted — not perfectly level. Corrugated cardboard grain texture on the wood face. Warm
tan and ochre palette, dark marker text color. Hard-edged low-poly faceted geometry. Chunky
proportions. Stylized low-poly game prop, cardboard-and-marker aesthetic. Symmetric about center.
```

#### Art Direction Notes

- The sign text does not need to be a specific name — Meshy can generate any placeholder text in block letters. The actual sign name variants ("The Last Counter," "The Dusty Parry," etc.) are applied by texture swap in Unity, so the geometry only needs to have a legible letter-style pattern on it.
- The slight tilt is an important charm detail. If Meshy generates a perfectly level sign, rotate it 3–5 degrees in Blender before import.
- Rope hangs should be simple — two short rope segments at the top corners.

#### Post-Processing Notes

- No rig. Pivot at the center of the sign board (not the rope tops).
- Target 100 tris. This is a near-flat prop; the geometry is trivial, the texture does all the work.

---

## Part 3 — Weapons: Cul-de-Sac Zone

**Art direction:** Cardboard-and-marker aesthetic. Warm Western palette — saddle brown, tan, rust red, ochre. All weapons follow the dual-prompt system: Pickup Prop (real-world object in the scene) + Imagined Weapon (held during combat).

**Poly budget:** 300–500 tris per weapon model (mobile target). Texture: 512×512 diffuse only.

---

### Weapon 11: Jump Rope → The Lasso

**Tier:** 2 | **Type:** Melee, reach-based | **Priority:** MEDIUM

**Pickup Prop — Jump Rope**

```
A child's jump rope, stylized chunky 3D, cardboard-and-marker art style. Braided cord in red and
white stripes coiled loosely on the ground. Two chunky wooden handles at each end, rounded cylinder
shape, warm tan wood. Slight marker-drawn detail lines on the handles. Rope has visible braid
texture, slightly hand-drawn. Warm red and white palette with tan handles. Chunky cartoon
proportions. Cardboard-and-marker aesthetic, game-ready prop. Not photorealistic, keep braid bold
and graphic.
```

**Imagined Weapon — The Lasso**

```
A full cowboy lasso weapon, stylized chunky 3D, cardboard-and-marker art style. A wide overhead
loop of braided rope frozen mid-swing above the wielder's hand. The loop is large — occupying the
upper half of the composition. Braided rope texture, hand-drawn marker lines emphasizing the braid.
A grip knot handle at the bottom where the rope meets the hand. Rope is natural manila tan with a
saddle-brown shading. The loop has a slight squash-and-stretch in the swing direction. Small
cardboard corrugation detail on the grip wrap. Chunky cartoon proportions, bold graphic silhouette.
Cardboard-and-marker aesthetic. Not realistic rope physics — stylized and frozen like a graphic.
```

**Unity filename:** `obj_lasso_equipped.fbx` / `obj_jumprope_pickup.fbx`

**Art Direction Notes:**
- The lasso equipped model shows the rope mid-swing, loop extended overhead. This is a static mesh — the swing animation is handled by the animator in Unity.
- The loop diameter should be visibly larger than the character's body to communicate the wide melee reach this weapon has.
- If Meshy collapses the loop flat, refine with "wide open lasso loop, rope circle clearly open and horizontal, loop held above head."

**Post-Processing Notes:**
- No rig. Pivot at the grip point (bottom of the rope, where the hand would hold it).
- The rope loop should be modeled as a tube cross-section, not a flat ribbon — rope is round in cross-section.
- Target 350 tris for equipped version, 200 tris for pickup.

---

### Weapon 12: Bike Horn → The Dynamite Bundle

**Tier:** 3 | **Type:** Ranged, AoE | **Priority:** LOW (model last)

**Pickup Prop — Bike Horn**

```
A rubber squeeze bicycle horn, stylized chunky 3D, cardboard-and-marker art style. The classic
bulb-type horn: a round rubber squeeze bulb at one end, a short curved metal tube, a flared bell
opening at the other end. Bright yellow rubber bulb, dark grey tube, marker-drawn detail lines.
Chunky and slightly oversized proportions. Cardboard-and-marker aesthetic, game-ready prop. Not
photorealistic rubber — chunky cartoon proportions, bold outlines.
```

**Imagined Weapon — The Dynamite Bundle**

```
A cartoon dynamite bundle weapon, stylized chunky 3D, cardboard-and-marker art style. Five red
cylindrical dynamite sticks bundled and bound together with black tape stripes. The bundle is
chunky and slightly oversized — clearly a cartoon prop, not realistic. A fuse made of twisted rope
exits from the top — the rope end is the rubber horn bulb, slightly visible. Fuse has a hand-drawn
spark at the tip drawn in marker. Bold red sticks with hand-drawn label marks. Black tape bands
with marker-drawn crosshatch texture. Yellow and orange spark detail at the fuse tip. Chunky
cartoon proportions. Cardboard-and-marker aesthetic. Not realistic or dangerous-looking — cheerful
cartoon dynamite.
```

**Unity filename:** `obj_dynamitebundle_equipped.fbx` / `obj_bikehorn_pickup.fbx`

**Art Direction Notes:**
- The dynamite bundle is the zone's Tier 3 AoE weapon — it should look visibly powerful, which means large and chunky.
- The horn-bulb fuse connection is the design's conceptual payoff — the rubber bulb from the bike horn becomes the squeeze-to-throw mechanism. If the connection reads, great; if Meshy does not capture it, the texture pass can reinforce it with a visible rubber texture on the fuse end.
- The cheerful cartoon aesthetic is intentional — the dynamite should look like something from a Saturday morning cartoon, not a thriller.

**Post-Processing Notes:**
- No rig. Pivot at the grip point (one side of the bundle, where the hand would hold while throwing).
- The five sticks should be visible as individual cylinders in the bundle — do not decimate so aggressively that they merge into a single lump.
- Target 400 tris for equipped version, 150 tris for pickup.

---

### Weapon 13: Garden Trowel → The Quickdraw Blade

**Tier:** 1 | **Type:** Melee, fast | **Priority:** HIGH (model first among new weapons)

**Pickup Prop — Garden Trowel**

```
A short-handled garden trowel, stylized chunky 3D, cardboard-and-marker art style. Classic hand
trowel: a short wooden handle and a flat metal blade head that is roughly triangular with a slightly
curved scoop shape. Warm tan wooden handle with visible grain. Silvery-grey trowel head with
marker-drawn edge lines suggesting the blade edge. Slightly worn and dusty — gardening tool used in
actual soil. Chunky proportions, slightly oversized handle grip. Cardboard-and-marker aesthetic,
game-ready prop.
```

**Imagined Weapon — The Quickdraw Blade**

```
A bowie knife weapon, stylized chunky 3D, cardboard-and-marker art style. The trowel head
transformed into a wide flat bowie blade — the scoop curve of the trowel becomes the blade's
false edge, the flat face is the blade body. Held in a reverse grip. The blade has a visible
corrugated cardboard texture running the length — it is clearly a trowel-blade, not steel. The
edge has a silvery marker-drawn sheen. Wooden handle with leather-strip wrapping in saddle brown.
A slight curve to the blade tip — clip-point bowie silhouette. Bold marker outlines on all edges.
Compact and fast-looking — short overall length. Cardboard-and-marker aesthetic, Psychonauts style.
Not realistic metal — cardboard construction visible.
```

**Unity filename:** `obj_quickdrawblade_equipped.fbx` / `obj_gardentrowel_pickup.fbx`

**Art Direction Notes:**
- The Quickdraw Blade is the zone's fastest melee weapon — the model should communicate speed through its compact proportions and sharp silhouette.
- The corrugated cardboard texture on the blade face is the key visual callback. If Meshy generates a smooth metal blade, add "visible corrugated cardboard texture on blade surface, horizontal corrugation lines" to the refine prompt.
- Reverse grip pose: model the blade pointing downward from the grip if Meshy allows a pose. If Meshy cannot produce a reverse-grip hold, model the weapon standalone with the blade angled at 45 degrees.

**Post-Processing Notes:**
- No rig. Pivot at the grip center (the handle).
- This is a compact weapon — the equipped model should be noticeably smaller than the katana or bo staff.
- Target 250 tris for equipped version, 120 tris for pickup.

---

### Weapon 14: Watering Can → The Six-Shooter

**Tier:** 2 | **Type:** Ranged, burst-fire | **Priority:** MEDIUM-HIGH

**Pickup Prop — Watering Can**

```
A metal watering can, stylized chunky 3D, cardboard-and-marker art style. Classic oval body with a
long curved spout and a rose nozzle head at the tip. Rounded handle arching over the top. Dull
grey metal body with a faint warmth to the tone — not polished. Visible seam lines where the metal
is joined, marker-drawn to look like rivet rows. A slight dent on one side. Corrugated cardboard
grain texture on the flat body surfaces. Chunky proportions — slightly oversized. Cardboard-and-
marker aesthetic, game-ready prop.
```

**Imagined Weapon — The Six-Shooter**

```
A long-barreled revolver weapon, stylized chunky 3D, cardboard-and-marker art style. The watering
can body becomes the grip — the oval watering can shape is clearly visible as the grip/cylinder.
The long curved spout becomes the revolver barrel — long, slightly curved, with the rose nozzle
transformed into a flared barrel muzzle. The handle arch becomes a trigger guard. A visible
cylinder drum at the grip shows six chambers. Warm grey metal with saddle brown grip wrapping.
Gold marker-drawn details on the cylinder chambers. Muzzle has a small flared opening. Chunky
cartoon proportions — oversized barrel, wide grip. Water droplets frozen at the muzzle tip glinting
gold. Bold marker outlines. Cardboard-and-marker aesthetic. Not realistic revolver — cartoonish and
toy-like.
```

**Unity filename:** `obj_sixshooter_equipped.fbx` / `obj_wateringcan_pickup.fbx`

**Art Direction Notes:**
- The watering can DNA must survive the transformation — the viewer should be able to look at the Six-Shooter and still recognize the watering can underneath it. The long curved spout-barrel and the oval body-grip are the two elements that carry this read.
- The gold water droplets at the muzzle tip communicate the "water = bullets" theme.
- The reload animation in gameplay is Kid tilting the can downward (pouring to reload). The model should have a natural center of gravity that suggests this.
- If Meshy generates a generic pistol with no watering can shape remaining, reject it entirely. The transformation concept is load-bearing.

**Post-Processing Notes:**
- No rig. Pivot at the grip center (where the hand holds it).
- The cylinder drum should be modeled with 6 visible chamber holes — even if they are small at this poly budget, they matter for the "six-shooter" read.
- Target 450 tris for equipped version (highest of the new weapons due to the cylinder detail), 180 tris for pickup.

---

## Part 4 — Animation Pipeline

Animations are NOT sourced from Meshy. Use the following pipeline for every character.

### Tool Selection

| Tool | Use for | Avoid for |
|---|---|---|
| **Mixamo** | Locomotion — idle, walk, run, strafe | Quadrupeds, combat-specific timing |
| **Blender** | All combat animations — attack, parry, dodge, stagger, death, boss attacks | Nothing; Blender can do everything |
| **Meshy Animate** | Nothing — do not use | All game animations |

**Why not Meshy Animate:** Text-prompted animation generation offers no control over frame timing. This game's parry window is 0.3–0.35s and attack telegraphs must sync to exact gameplay constants. Meshy Animate cannot meet that precision requirement.

**Why Mixamo for locomotion:** Adobe Mixamo (free with Adobe account) provides a large library of high-quality biped animations. Upload the character FBX, auto-rig, apply any clip from the library, export as FBX for Unity. Fastest path for walk/run/idle.

**Why Blender for combat:** Full keyframe control over every frame, easing curve, and root motion strip. Required for attack wind-ups, parry window alignment, stagger recovery, and boss phase transitions.

### Blender → Mixamo FBX Export Settings

When exporting a character FBX from Blender for upload to Mixamo auto-rigger:

| Setting | Value | Note |
|---|---|---|
| `axis_forward` | `'Z'` | **NOT** `'-Z'` — using `-Z` causes a 180° wrist twist in Mixamo |
| `axis_up` | `'Y'` | Standard |
| `global_scale` | `1.0` | |
| `object_types` | `{'MESH'}` | Mesh only for auto-rigger upload; no armature |
| `bake_space_transform` | `False` | |
| `bake_anim` | `False` | |

> **Why `Z` not `-Z`:** Confirmed on Ninja character (2026-07-17) — `Ninja_mixamo_v3.fbx`. Exporting with `-Z` (the Unity standard) produced a 180° wrist twist when uploaded to Mixamo auto-rigger. Switching to `+Z` resolved it.

After Mixamo auto-rigs the character, download as **FBX for Unity** and place in `models/processed/characters/<Name>/`.

### Per-Character Animation Plan

| Character | Locomotion Source | Combat Source | Notes |
|---|---|---|---|
| Player Ninja | Mixamo | Blender | All attack/dodge/parry authored manually |
| Player Cowboy | Mixamo | Blender | Can share combat clips with Ninja where appropriate |
| Skeptic | Mixamo | Blender | |
| Gnome Soldier | Mixamo | Blender | |
| Dirty Laundry Grunt | Mixamo | Blender | |
| SpinCycle Boss | Blender | Blender | Boss movement too specific for Mixamo; Meshy embedded clips usable as rough reference only |
| Hitching Hound | Blender | Blender | Quadruped — Mixamo is biped only |
| Milepost Marshal | None (stationary) | Blender | Arm fold/unfold states only; no locomotion |
| Tumbleweed Roller | Script-driven (physics sphere) | None | No animation rig; motion via code |
| Sprinkler Sentinel | Script-driven (transform.Rotate) | None | No skeleton; rotation via script |

### Mixamo Workflow

1. Export the processed character FBX from Blender (mesh only, T-pose, `axis_forward='Z', axis_up='Y'`)
2. Upload to mixamo.com → Auto-Rig (confirm joint placement)
3. Choose **Standard Skeleton (65 bones)** if available — includes finger bones. If Mixamo returns fewer bones (e.g. 24-bone basic skeleton), proceed to the finger rig step below.
4. Browse animations → select clip → download as **FBX for Unity** (with skeleton, 30fps, no keyframe reduction)
5. Place downloaded ZIPs/FBXs in `models/processed/characters/<Name>/Animations/`
6. **Add finger rig in Blender** (see below) before importing the character mesh FBX into Unity
7. Import into Unity per the settings table below — do not overwrite the character mesh FBX

### Adding the Finger Rig in Blender (post-Mixamo)

If the Mixamo auto-rigger returned a skeleton without finger bones (LeftHand/RightHand as terminal bones only), add the finger rig in Blender before Unity import. Confirmed on `Cowboy_mixamo_v1.fbx` — 2026-07-17.

**Bone naming:** Use standard Mixamo finger names so animations from Mixamo's library apply correctly:
```
LeftHandThumb1/2/3   LeftHandIndex1/2/3   LeftHandMiddle1/2/3
LeftHandRing1/2/3    LeftHandPinky1/2/3
RightHandThumb1/2/3  RightHandIndex1/2/3  RightHandMiddle1/2/3
RightHandRing1/2/3   RightHandPinky1/2/3
```

**Steps:**
1. Import the Mixamo character FBX into Blender
2. In armature Edit Mode, add 5 × 3-bone chains per hand, parented to `LeftHand`/`RightHand`
   - Segment length: ~0.065m (middle/proximal), ~0.04m (tip)
   - Lateral spacing: ~0.03m between fingers; thumb offset ~+0.05m toward palm center
   - Direction: extend in the same direction as the hand bone
3. Exit Edit Mode → select mesh then armature → `Set Parent → Armature Deform → With Automatic Weights`
4. Export with `axis_forward='Z', axis_up='Y'`, `object_types={'ARMATURE','MESH'}`, `bake_anim=False`
5. Copy exported FBX to `Assets/_Project/Models/Characters/<Name>/`

### Unity Import Settings — Blender→Mixamo FBX Pipeline

> **Confirmed on:** `Ninja_mixamo_v3.fbx` — 2026-07-17. Apply these exact settings to all biped characters going through the Blender→Mixamo pipeline.

#### Character Mesh FBX

| ModelImporter Property | Value | Why |
|---|---|---|
| `animationType` | `Generic` | **Not Humanoid.** Humanoid ignores the `char1` child's `localScale=(0.01,0.01,0.01)` in play mode, causing the character to float 74m above the ground. Generic rig respects the full transform stack consistently. |
| `globalScale` | `0.01` (Mixamo-original FBX) **or** `0.0001` (Blender re-export) | Mixamo exports FBX in cm; Blender re-exports in meters. Both give ~2m character but need different globalScale to compensate. **If the character was processed through Blender for finger rigging, use `0.0001`.** |
| `useFileScale` | `false` | Disabling prevents Unity from double-applying the FBX file's own unit scale on top of `globalScale`. |
| `importAnimation` | `false` | Character FBX contains no animation data; skip to avoid confusion. |

#### Animation FBXs (one per clip, downloaded from Mixamo)

| ModelImporter Property | Value | Why |
|---|---|---|
| `animationType` | `Generic` | Must match the character rig type. |
| `avatarSetup` | `NoAvatar` | Generic rigs don't use the Avatar system. |
| `globalScale` | `0.01` | Same scale as character mesh — animation curves are in cm space. |
| `useFileScale` | `false` | Same reason as character mesh. |
| `importAnimation` | `true` | |

**Clip rename (required):** Every FBX downloaded from Mixamo names its clip `"mixamo.com"` by default. Rename via `ModelImporter.clipAnimations` before use:

```csharp
// Run once after import for each animation FBX
var imp = AssetImporter.GetAtPath(path) as ModelImporter;
var clips = imp.clipAnimations;
clips[0].name = "Walk";   // replace with the proper clip name
imp.clipAnimations = clips;
imp.SaveAndReimport();
```

#### What NOT to try (known failures)

| Attempted | Result | Why it fails |
|---|---|---|
| `Humanoid` + `globalScale=1` + `useFileScale=false` | Edit: 1.87m ✓ / Play: character 74m above ground | Humanoid Avatar drives bones bypassing `char1`'s `localScale=(0.01,0.01,0.01)` |
| `Humanoid` + `globalScale=100` + `useFileScale=true` | Edit: 1.87m / Play: Y=74m (center), height ~2m | `globalScale` multiplies Avatar bone world positions by 100 |
| `Humanoid` + root `scale=(100,100,100)` | Play: still 74m center | Root scale magnifies Avatar-driven bone positions the same way |
| `Generic` + `globalScale=1` + `useFileScale=false` | Edit: 186.8m | No child scale correction; raw cm geometry |
| `Generic` + `globalScale=1` + `useFileScale=true` | Edit: 0.019m / Play: 0.019m | Consistent but 100× too small |

#### Scene setup

```
Ninja_V2 (root)
  localPosition = (0, 0, 0)
  localScale    = (1, 1, 1)     ← never scale the root — globalScale=0.01 handles it
  Animator: controller=AC_<CharName>, avatar=<CharName>Avatar, applyRootMotion=false
```

### Blender Combat Animation Workflow

1. Open the processed character FBX in Blender
2. Author keyframes in the NLA Editor or Action Editor
3. Strip root motion from the Hips bone (CharacterController owns world position)
4. Export each animation as a separate FBX Action or bake to NLA strip
5. Import into Unity, set loop/non-loop per clip, assign to Animator Controller

### SpinCycle Embedded Animations (from Meshy FBX)

The Meshy-generated SpinCycle FBX includes embedded generic animation clips. These can be used as non-combat reference states:

| Meshy Clip | Boss Use | Author replacement? |
|---|---|---|
| Idle_5 | Between attacks, drum slowly rotating | No — keep as-is |
| Walking | Slow approach Phase 1 | No — keep |
| Running | Aggressive chase Phase 2 | No — keep |
| Regular_Jump | Drum Slam (un-parryable) | Yes — re-author in Blender for exact timing |
| Run_and_Jump | Jump Charge Phase 2 | Yes — re-author for Gallows Run variant |
| Weapon_Combo_2 | Haymaker combo | Yes — re-author parry window frame-precise |
| Hit_Reaction | Stagger on parry counter | Yes — re-author |

---

## Unity Asset Delivery Paths — Cul-de-Sac

| Asset | Unity Filename | Delivery Path |
|---|---|---|
| `chr_tumbleweed_roller.fbx` | `chr_tumbleweed_roller.fbx` | `Assets/_Project/Models/Characters/` |
| `chr_hitching_hound.fbx` | `chr_hitching_hound.fbx` | `Assets/_Project/Models/Characters/` |
| `chr_milepost_marshal.fbx` | `chr_milepost_marshal.fbx` | `Assets/_Project/Models/Characters/` |
| `env_cs_covered_wagon.fbx` | `env_cs_covered_wagon.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_saloon_facade.fbx` | `env_cs_saloon_facade.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_hitching_post.fbx` | `env_cs_hitching_post.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_water_trough.fbx` | `env_cs_water_trough.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_wanted_poster.fbx` | `env_cs_wanted_poster.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_mailbox_telegraph.fbx` | `env_cs_mailbox_telegraph.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_tumbleweed_static.fbx` | `env_cs_tumbleweed_static.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_gallows_frame.fbx` | `env_cs_gallows_frame.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_barrel.fbx` | `env_cs_barrel.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_lamp_post_western.fbx` | `env_cs_lamp_post_western.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_command_node_active.fbx` | `env_cs_command_node_active.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `env_cs_saloon_sign.fbx` | `env_cs_saloon_sign.fbx` | `Assets/_Project/Models/Environment/CulDeSac/` |
| `obj_lasso_equipped.fbx` / `obj_jumprope_pickup.fbx` | — | `Assets/_Project/Models/Weapons/` |
| `obj_dynamitebundle_equipped.fbx` / `obj_bikehorn_pickup.fbx` | — | `Assets/_Project/Models/Weapons/` |
| `obj_quickdrawblade_equipped.fbx` / `obj_gardentrowel_pickup.fbx` | — | `Assets/_Project/Models/Weapons/` |
| `obj_sixshooter_equipped.fbx` / `obj_wateringcan_pickup.fbx` | — | `Assets/_Project/Models/Weapons/` |

---

*Meshy V2 Prompt Reference — Cul-de-Sac Zone — Created 2026-07-17*
*V1 prompts (Backyard Zone) remain in `docs/ai-art-prompts.md` and `docs/meshy-weapon-prompts.md`.*
