# Unboxed Heroes — Town Square Asset Brief

**Zone:** The Town Square (Phase 2, Zone 2 — V3 Sprint 07)
**Prepared:** 2026-07-28
**For:** Art partner — Meshy asset generation

---

## Open Design Questions

These need your input before art pass begins. Anything marked **BLOCKED** cannot be finalized until answered.

| # | Question | Why It Matters |
|---|---|---|
| Q1 | **Notice Pusher indicator light** — is the enemy's warning light visible at all angles, or only when the player is inside the attack arc? | Affects whether the Notice Pusher model needs a clearly front-facing indicator vs. an all-round one |
| Q2 | **Permit Pulper boss sash** — in the Imagination Restore win screen, does the boss's official sash appear restored and bright, or simply absent? | Determines what the final boss model needs — an additional bright sash mesh, or nothing |
| Q3 | **Quest content** — what are the 3 specific Phase 2 quests? | Determines whether the Notice Board prop needs interactive flavor text assigned or can stay generic |

---

## Asset Gap Analysis

Cross-referenced against the Town Square concept art prop key and our existing Unity packs (SimpleTown + Polyworks Mega Pack — all 8,977 prefabs searched).

| # | Prop | Status | Action |
|---|---|---|---|
| 1 | Community Hall / Boss Door building | ⚠️ Assemble from existing | Polyworks `Building_Castle_*` + `Building_Facade_Modular_*` + Dark Fantasy `Prop_Building_Heavy_Stone_Door_Fixed_01_Atlased` for the iron doors — no Meshy order needed |
| 2 | Notice Board / Quest Board (×3 placements) | ❌ Missing | **Order from Meshy — see Asset TS-01 below** |
| 3 | Stone Pavilion / Canopy | ✅ Available | Polyworks Dark Fantasy `Prop_Building_Canopy_Canvas_01/02/03_Atlased` — three canvas canopy variants, try before ordering |
| 4 | Filing Cabinet Stack | ❌ Missing | **Order from Meshy — see Asset TS-02 below** |
| 5 | Administrative Service Counter | ⚠️ Check first | Polyworks SciFi `SciFi_Modular_Table_Long_Grey_01_Atlased` — flat grey long table; institutional enough for Unimaginative aesthetic. Try before ordering. If it reads too sci-fi, fall back to TS-03 |
| 7 | Iron Bench (×2 placements) | ⚠️ Check first | Polyworks Druid Home `Prop_Furniture_Tribal_Stone_Seat_01_Atlased` — heavy stone seat. Try before ordering. Backup: TS-04 |
| 8 | Decorative Stone Urn | ✅ Available | Polyworks `Prop_Urn_01/02/03_Atlased` — three variants, pick whichest reads best at top-down camera |
| 9 | Take-a-Number Dispenser | ❌ Missing | **Order from Meshy — see Asset TS-05 below** |
| 10 | Delivery / Supply Cart | ⚠️ Approximate | Polyworks `Asian_Prop_Vehicle_Cart_01` — usable stand-in |
| 12 | Trash Bin | ✅ Available | SimpleTown `bin_mesh` |
| 13 | Entrance Arch | ✅ Available | Polyworks Ruins `Prop_Ruins_Archway_Stone_Full_01_Atlased` — full stone archway, better fit than Eastern arch |
| 15 | Flagpole | ✅ Available | SimpleTown `flag_mesh` or Polyworks `Abstract_Modular_Flag_Beige_01_Atlased` |
| 16 | **Dry Fountain** | ⚠️ Check first | Polyworks Dungeon has **`Prop_Gothic_Pool_Marble_01_Atlased`** and **`Prop_Statue_Gothic_Basin_01_Atlased`** — gothic marble pool and statue basin. Try these first. If scale or aesthetic is wrong, fall back to TS-06 Meshy order |
| 17 | Lantern Post (×4) | ✅ Available | Polyworks `Prop_Lantern_Post_01_Atlased` or `Prop_Lamp_Post_Metal_01_Atlased` |

**Summary: 3 confirmed Meshy orders (Notice Board, Filing Cabinets, Dispenser). 3 "check existing first" (Fountain, Bench, Counter). 7 confirmed available from existing packs.**

**Recommended workflow:** Drop `Prop_Gothic_Pool_Marble_01_Atlased` into a test scene first — if the scale and look works as the central fountain, that saves the highest-priority Meshy order. Then check the stone seat and long grey table. Only order TS-06/04/03 from Meshy if the existing props don't fit.

---

## Global Settings — Town Square Assets

The Town Square palette is distinct from the Cul-de-Sac. Where the Cul-de-Sac was warm amber and Western, the Town Square is civic, cool, and institutional — but still cardboard-and-marker.

| Setting | Value |
|---|---|
| **Palette** | Cool civic grey, stone slate, institutional beige, faded municipal blue. Cool shadows. No warm amber or ochre — that is Cul-de-Sac territory. |
| **Texture** | 512×512 diffuse only. No normal or roughness maps. |
| **Grain** | Corrugated cardboard texture on all flat panel surfaces. Stone surfaces get subtle crack lines instead of grain. |
| **Outlines** | Bold marker-drawn lines on all surface edges. |
| **Style** | Low Poly for all ENV props. |
| **Symmetry** | ON for all props in this list. |
| **Delivery path** | `Assets/_Project/Models/Environment/TownSquare/` |

> **Meshy character limit:** 800 characters per prompt. All prompts below are written under this limit.

---

## Meshy Prompts — Town Square ENV Props

**3 confirmed orders:** TS-01, TS-02, TS-05 — no existing substitutes found.
**3 backup prompts:** TS-03, TS-04, TS-06 — only order if the "Check first" existing assets don't work.

Order confirmed assets first: **TS-01 → TS-02 → TS-05**, then evaluate existing pack assets before ordering TS-03/04/06.

---

### Asset TS-06: Dry Fountain (BACKUP — check existing first)

> **Try before ordering:** Drop `Prop_Gothic_Pool_Marble_01_Atlased` and `Prop_Statue_Gothic_Basin_01_Atlased` (both in Polyworks/Atlased/Dungeon/) into a test scene. The gothic marble pool may be exactly right. Only order from Meshy if scale is too small, too tall, or the aesthetic is too ornate.

### Asset TS-06: Dry Fountain

**Unity asset name:** `env_ts_fountain_dry.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~400 tris

**This is the central combat obstacle for Room 1. It must provide solid cover for a character crouched behind it and read clearly from a top-down camera.**

#### Meshy Text Prompt

```
A large dry stone fountain, stylized low-poly game prop. Circular raised stone basin approximately
3 meters diameter, thick wide stone rim. Center column pedestal rising from the basin with a small
upper tier — three-tier stepped dry fountain, no water. Stone is worn civic grey with subtle crack
lines on the basin floor painted in marker. Corrugated cardboard grain texture on flat stone faces.
Cool grey and slate palette, no warm tones. Basin rim is wide and thick — knee height on a standing
character, solid enough to crouch behind. Hard faceted low-poly geometry. Chunky proportions.
Stylized game prop, cardboard-and-marker aesthetic, civic plaza theme.
```

#### Art Direction Notes

- The basin rim height and width are the gameplay-critical dimensions. From the top-down camera, the basin ring should form a clear doughnut silhouette — the player reads it as an obstacle to navigate around. If Meshy generates a thin rim, refine with "very wide thick stone rim, substantial wall height, clearly usable as cover."
- The center pedestal can have a simple upper basin or just a column — either reads correctly. The important thing is that it breaks line of sight across the fountain's center.
- No water in the basin — the fountain is dry and cracked. If Meshy adds a water surface, note it for removal.
- Cool grey throughout. No warm stone tones — this is civic stone, not desert sandstone.

#### Post-Processing Notes

- No rig. Snap base to Y=0.
- Target 400 tris. The circular basin geometry will need decimation — most of the budget should go to the rim cross-section.
- The fountain must be tagged `isStatic = true` in Unity and included in the NavMesh bake as an obstacle. Note for the scene setup step.

---

### Asset TS-01: Notice Board / Quest Board

**Unity asset name:** `env_ts_notice_board.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~200 tris
**Used at:** 3 locations in the zone (1 model, placed multiple times)

#### Meshy Text Prompt

```
A civic community notice board, stylized low-poly game prop. Square wooden board mounted on a single
thick square-section wooden post. Board surface covered with overlapping layers of papers and notices
— multiple sheets pinned at angles, some curling at corners, some torn. Papers have marker-drawn line
marks suggesting text and notices. Simple wooden frame borders the board. Post is square-section wood.
Muted institutional beige board, grey-brown post, aged papers with marker text. Corrugated cardboard
grain on the post and frame. Hard faceted low-poly geometry. Marker outlines on all edges. Stylized
game prop, cardboard-and-marker aesthetic, civic theme.
```

#### Art Direction Notes

- The stacked overlapping papers are the defining detail — they sell the "bureaucratic notice board" read immediately. If Meshy generates a clean empty board, refine with "many overlapping paper notices pinned to board, papers at angles, some torn, layered chaos of documents."
- The papers do not need legible text — marker-drawn horizontal lines implying text are sufficient and better for the aesthetic.
- This prop is placed at 3 locations in the zone. It should be distinct from the Cul-de-Sac's Wanted Poster (Asset 18) — wider board, post-mounted, covered in many documents rather than one.

#### Post-Processing Notes

- No rig. Snap post base to Y=0.
- Target 200 tris. The paper layers should be flat quads stacked at slight angles — minimal geometry, maximum texture work.

---

### Asset TS-02: Filing Cabinet Stack

**Unity asset name:** `env_ts_filing_cabinets.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~300 tris

#### Meshy Text Prompt

```
Three grey metal filing cabinets stacked vertically, stylized low-poly game prop. Three identical
rectangular steel cabinet units stacked in a single column, approximately 1.5 meters tall total.
Each cabinet has two large rectangular drawer fronts with simple bar handles. The top drawer of the
uppermost cabinet is open and overflowing with papers — document sheets spilling out. Steel surfaces
are flat medium grey, matte, no warm tones. Marker-drawn handle details and paper edges. Corrugated
cardboard grain texture on cabinet sides. Hard faceted low-poly geometry. Deliberately bland and
institutional. Stylized game prop, cardboard-and-marker aesthetic, bureaucratic.
```

#### Art Direction Notes

- The Unimaginative use filing cabinets as their primary storage prop — these should feel deliberately grey and boring, contrasting with the cardboard-and-marker warmth of the imaginative props nearby.
- The overflowing open top drawer is the personality detail. Documents spilling out read as "bureaucratic overload."
- Three cabinets stacked (not side by side) — this creates a tall, narrow cover obstacle appropriate for a character to hide behind.

#### Post-Processing Notes

- No rig. Snap base to Y=0.
- Target 300 tris. The stacked geometry is very regular — aggressive decimation is fine on the flat cabinet faces.

---

### Asset TS-03: Administrative Service Counter (BACKUP — check existing first)

> **Try before ordering:** `SciFi_Modular_Table_Long_Grey_01_Atlased` (Polyworks/Atlased/SciFi/) is a long flat grey table. Place it at counter height (scale Y up slightly) — the grey institutional look matches the Unimaginative aesthetic. Only order from Meshy if it reads too futuristic.

### Asset TS-03: Administrative Service Counter

**Unity asset name:** `env_ts_service_counter.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~350 tris

**Note: This prop is used in Room 3's Warden mechanic — the enemy stands behind it and cannot be hit from the front. The counter must have a clear "front" face and an open "back" area.**

#### Meshy Text Prompt

```
A government service counter, stylized low-poly game prop. A wide chest-high counter with a flat
clear top surface. Counter front face is solid flat panels. A low raised divider wall runs along the
back edge of the counter top. A small document tray and a desk bell sit on the counter surface.
A service window gap above the counter top — open space suggesting a service window. Grey institutional
surfaces, flat medium grey with slight beige tones on the counter top. Marker-drawn edge details.
Corrugated cardboard grain on flat panels. Hard faceted low-poly geometry. Deliberately dull
bureaucratic object. Stylized game prop, cardboard-and-marker aesthetic.
```

#### Art Direction Notes

- Width is important — the counter should be wide enough to span a meaningful section of a room (approximately 3–4 units wide at game scale). If Meshy generates a narrow counter, refine with "very wide service counter, spans the full width of a service window, wide desk area."
- The back divider wall behind the counter top is what makes the Warden mechanic work — the counter should visually telegraph "you can only approach this from the sides." Flag this for the NavMesh obstruction setup in Unity.
- Flat grey institutional. This is an Unimaginative prop — no warmth, no charm.

#### Post-Processing Notes

- No rig. Snap base to Y=0.
- Target 350 tris. Most budget goes to the counter body and the service window gap geometry.
- In Unity: mark as static obstacle and include in NavMesh bake. The Warden AI uses the counter's open end as its position anchor.

---

### Asset TS-04: Iron Bench (BACKUP — check existing first)

> **Try before ordering:** `Prop_Furniture_Tribal_Stone_Seat_01_Atlased` (Polyworks/Atlased/Druid Home/) is a heavy stone seat. Also check `Tribal_Plains_Log_Bench_Wood_Timber_01_Atlased` (Polyworks/Atlased/Tribal/). Drop into a test scene — if either reads as a park bench at game camera distance, no Meshy order needed.

### Asset TS-04: Iron Bench

**Unity asset name:** `env_ts_bench.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~200 tris
**Used at:** 2 locations (same model placed twice)

#### Meshy Text Prompt

```
A civic park bench, stylized low-poly game prop. Classic park bench: two thick chunky stone end
supports and three flat stone slat planks forming the seat, two slat planks forming the back rest.
Stone construction, heavy and solid. End supports are slightly ornamental — tapered at top with
simple rounded profiles. Cool medium grey stone, slightly weathered. Marker-drawn edge outlines on
all stone faces. Subtle corrugated cardboard grain texture on flat surfaces. Hard faceted low-poly
geometry. Bench seat approximately knee height. Chunky proportions. Stylized game prop,
cardboard-and-marker aesthetic, civic plaza theme.
```

#### Art Direction Notes

- Stone bench, not metal — the civic-castle theme leans toward heavy masonry over wrought iron. If Meshy generates a metal bench, refine with "stone block bench, heavy stone end supports, stone slab seat, masonry construction."
- The bench is both decoration and minor cover — a character can crouch behind it. Make sure the back rest is tall enough to partially obscure a crouching character from a top-down camera.

#### Post-Processing Notes

- No rig. Snap base to Y=0.
- Target 200 tris. The stone slab slats are flat geometry — decimate aggressively.

---

### Asset TS-05: Take-a-Number Dispenser

**Unity asset name:** `env_ts_number_dispenser.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON
**Poly budget:** ~100 tris
**Priority:** LOW — flavor prop only, generates last

#### Meshy Text Prompt

```
A take-a-number ticket dispenser on a post, stylized low-poly game prop. Free-standing thin square
post with a wide flat dispenser head at the top. The dispenser head is rectangular — a slot at the
bottom front dispenses paper tickets. A number display on the front face shows a number rendered in
thick marker-drawn digits. A short curl of paper tickets hangs from the dispenser slot. Post is thin
square metal section. Flat medium grey body, marker-drawn number on the display face. Corrugated
cardboard grain on flat surfaces. Hard faceted low-poly geometry. Approximately waist height.
Stylized game prop, cardboard-and-marker aesthetic, bureaucratic.
```

#### Art Direction Notes

- This is a pure flavor prop — it does not interact with gameplay. Generate it last and spend minimal refinement time on it.
- The number display on the front face is the key readable detail. It can display any number — "47" or "108" are good options that suggest a long queue.
- Small and narrow — this prop should not obstruct movement or look like a combat obstacle.

#### Post-Processing Notes

- No rig. Snap post base to Y=0.
- Target 100 tris. This is a very simple form.

---

## Delivery Paths

| Asset | Unity Filename | Path |
|---|---|---|
| Dry Fountain | `env_ts_fountain_dry.fbx` | `Assets/_Project/Models/Environment/TownSquare/` |
| Notice Board | `env_ts_notice_board.fbx` | `Assets/_Project/Models/Environment/TownSquare/` |
| Filing Cabinet Stack | `env_ts_filing_cabinets.fbx` | `Assets/_Project/Models/Environment/TownSquare/` |
| Service Counter | `env_ts_service_counter.fbx` | `Assets/_Project/Models/Environment/TownSquare/` |
| Iron Bench | `env_ts_bench.fbx` | `Assets/_Project/Models/Environment/TownSquare/` |
| Take-a-Number Dispenser | `env_ts_number_dispenser.fbx` | `Assets/_Project/Models/Environment/TownSquare/` |

Raw downloads go to `boxhead/models/V3/zips/Environment/TownSquare/` before processing.

**Generation order:** TS-06 (Fountain) → TS-01 (Notice Board) → TS-04 (Bench) → TS-02 (Filing Cabinets) → TS-03 (Counter) → TS-05 (Dispenser)

---

## Existing Pack Assets — No Order Needed

All confirmed available in the project. The scene builder pulls them directly from Unity.

**Confirmed available:**

| Prop | Source | Unity Path |
|---|---|---|
| Decorative Stone Urn | Polyworks | `Prefabs/Atlased/Props General/Prop_Urn_01_Atlased.prefab` |
| Trash Bin | SimpleTown | `SimpleTown/Prefabs/Props/bin_mesh.prefab` |
| Entrance Arch (stone) | Polyworks | `Prefabs/Atlased/Ruins/Prop_Ruins_Archway_Stone_Full_01_Atlased.prefab` |
| Flagpole | SimpleTown | `SimpleTown/Prefabs/Props/flag_mesh.prefab` |
| Lantern Post | Polyworks | `Prefabs/Atlased/Props General/Prop_Lantern_Post_01_Atlased.prefab` |
| Stone Pavilion / Canopy | Polyworks | `Prefabs/Atlased/Dark Fantasy/Prop_Building_Canopy_Canvas_01_Atlased.prefab` (try all 3 variants) |
| Community Hall iron doors | Polyworks | `Prefabs/Atlased/Dark Fantasy/Prop_Building_Heavy_Stone_Door_Fixed_01_Atlased.prefab` |
| Delivery Cart (stand-in) | Polyworks | `Prefabs/MaterialsOnly/Asian Additional/Asian_Prop_Vehicle_Cart_01.prefab` |

**Check these first before ordering (may save 3 Meshy orders):**

| Prop | Source | Unity Path | Notes |
|---|---|---|---|
| Dry Fountain (option A) | Polyworks | `Prefabs/Atlased/Dungeon/Prop_Gothic_Pool_Marble_01_Atlased.prefab` | Gothic marble pool — likely right scale and aesthetic |
| Dry Fountain (option B) | Polyworks | `Prefabs/Atlased/Dungeon/Prop_Statue_Gothic_Basin_01_Atlased.prefab` | Statue with basin — more vertical, could be the center pedestal |
| Iron Bench (option A) | Polyworks | `Prefabs/Atlased/Druid Home/Prop_Furniture_Tribal_Stone_Seat_01_Atlased.prefab` | Heavy stone seat |
| Iron Bench (option B) | Polyworks | `Prefabs/Atlased/Tribal/Tribal_Plains_Log_Bench_Wood_Timber_01_Atlased.prefab` | Timber log bench |
| Service Counter | Polyworks | `Prefabs/Atlased/SciFi/SciFi_Modular_Table_Long_Grey_01_Atlased.prefab` | Long flat grey table — matches Unimaginative bureaucratic tone |

---

_Town Square Asset Brief — Created 2026-07-28 | V3 Sprint 07 prep | See `docs/design/GDD-v2-town-square.md` for full zone design_
