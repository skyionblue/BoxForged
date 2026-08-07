# Meshy Prompts: The Backyard (Dojo) — World 2 — Enemies / Boss

**Zone:** The Backyard / Dojo (World 2)
**Prepared:** 2026-08-07
**Scope:** Rigged characters only (2 enemies + 1 boss). ENV props live in the sibling `meshy-prompts.md`.

> These are **characters**, not ENV props. They do **not** count against the <8k ENV tri budget.
> All three are rigged → import via **`/unity-character-importer`** (Meshy → Blender weight/orientation pass → Unity with RPG Character Mecanim animations). Do **not** send these through `/asset-pipeline` (that is for static props/weapons).

---

## Global Settings — Backyard/Dojo Characters

| Setting | Value |
|---|---|
| **Palette** | Jade green, mossy stone-grey, warm bamboo/straw tan, cherry-blossom pink `#F4B8C4`, lacquer red `#A62B1F`. Cool overcast light. **No warm amber / terracotta / western tones — those belong to World 1.** |
| **Texture** | 512×512 diffuse only. No normal or roughness maps. |
| **Grain** | Corrugated cardboard grain on armor plates and flat panels. |
| **Outlines** | Bold marker-drawn lines on all surface edges. |
| **Style** | Low Poly |
| **Symmetry** | See per-asset (Grasscutter ON; Crane/Leaf OFF — asymmetric poses). |
| **Pose** | T-pose (or A-pose) on export — required for rigging. |
| **Delivery path** | `Assets/_Project/Models/Characters/<Name>/` |

> **Meshy character limit:** 800 characters per prompt. All three prompts below are under this limit (verified: 737 / 763 / 716).

---

## Asset BD-E1: Crane Duelist  *(enemy, rigged)*

**Unity asset name:** `char_crane_duelist.fbx`
**Meshy Style:** Low Poly
**Symmetry:** OFF (asymmetric one-legged stance; generate on one leg, rig will pose it)
**Poly budget:** ~10–12k tris (standard enemy tier)
**Import route:** `/unity-character-importer`
**Priority:** HIGH — debut enemy, the World 2 skill-check read.

**What it is:** A plastic pink lawn flamingo reimagined as a one-legged crane-stance spear duelist — conical straw hat, pink-lacquered armor, long thin beak-spear. Still, elegant, patient. (GDD §3.)

### Meshy Text Prompt

```
A one-legged crane-stance spear duelist, stylized low-poly character, T-pose for rigging. Tall and thin, balanced on a single stork-like leg with a raised bent second leg tucked up. Wide conical straw hat shading a narrow beaked face. Pink-lacquered segmented plate armor over a slim body, high collar. Holds a long thin beak-spear in both hands, angled forward. Exaggerated elongated proportions, elegant and poised. Cherry-blossom pink lacquer, warm straw-tan hat, jade-green sash, dark spear shaft. Corrugated cardboard grain on the armor plates, bold marker-drawn outlines on all edges. Hard faceted low-poly geometry. Strong distinct silhouette, clear readable one-legged shape. Cardboard-and-marker aesthetic, feudal Japanese dojo.
```

### Silhouette Check (10% test)

- As a solid black shape it must read as: **one thin leg + tall body + wide conical hat + long straight spear line.** That vertical-line-with-a-cone-hat read is unmistakable next to the **stocky, wide, round-shouldered gnomes** and the **low lumpy leaf piles**. Passes.
- The two hard identity marks: the **conical hat** (widest point at the top) and the **single leg**. If Meshy gives it two planted legs, refine: "balanced on ONE leg only, second leg lifted and tucked, no ground contact on the second foot."

### Rigging / Silhouette Guidance

- **Meshy exports rig on two legs by default.** Generate T-pose; in the Blender pass, the raised-crane-stance is achieved via animation/pose, not baked geometry — keep both legs riggable. Weight the tucked leg so the crane-stance idle can be authored in Mecanim.
- The **beak-spear** is a long thin prop held in both hands — keep it as part of the mesh or a clearly separable submesh so grip can be calibrated (do not fight the established grip-scale calibration; see project memory on weapon grips).
- Elongate proportions vertically — the Crane should be visibly **taller** than the player and the gnomes. Height is part of the read.

---

## Asset BD-B1: The Grasscutter  *(boss, rigged)*

**Unity asset name:** `char_grasscutter.fbx`
**Meshy Style:** Low Poly
**Symmetry:** ON (drum-chest and folded wings are bilaterally symmetric)
**Poly budget:** ~25k tris (boss tier)
**Import route:** `/unity-character-importer`
**Priority:** HIGH — zone boss.

**What it is:** A rusted push reel-mower reimagined as a drum-chested tengu blade-master — the reel of curved blades is its spinning heart, the push-handle folds into war-wings, the wheels are heavy sandals. (GDD §5.)

### Meshy Text Prompt

```
A drum-chested tengu blade-master, stylized low-poly boss character, T-pose for rigging. A wide barrel torso built from a horizontal cylindrical reel of curved overlapping blades across the chest, the spinning heart. Two large folded angular wings rising off the back from a push-handle frame, hinged like war-fans. Stubby powerful arms. Squat legs ending in two big flat round wheel-sandals. A stern beaked tengu mask face on a small head above the drum. Rusted iron-grey metal, lacquer-red trim, jade-green cloth wraps, cherry-pink petal flecks. Corrugated cardboard grain on plates, bold marker outlines on all edges. Hard faceted low-poly. Broad heavy top-heavy silhouette, clearly winged and drum-bodied. Cardboard-and-marker aesthetic, feudal Japanese dojo.
```

### Silhouette Check vs. SpinCycle (World 1 boss)

This is the load-bearing differentiation. **SpinCycle** (washing machine) reads as a **smooth boxy monolith — a tall dented cube with a single round porthole eye, no limbs of note, front-heavy and blank.** The Grasscutter must NOT read as another box:

- **Grasscutter is winged and radial, not boxy.** Its silhouette is **top-heavy with two big angular wings fanning off the back** and a **horizontal cylindrical blade-reel** across the chest — a wide "X / winged-drum" shape, not a monolithic square.
- **Wheels-as-sandals** give it two clear round feet; SpinCycle sits flat with no foot read.
- **Beaked tengu mask** on a small high head vs. SpinCycle's **fogged porthole eye** centered on a faceless drum.
- Solid-black test: Grasscutter = winged drum on round feet; SpinCycle = a plain tall block. Distinct. Passes.
- If the reel reads as a flat shield rather than a segmented blade cylinder, refine: "horizontal cylinder of many curved blades like a mower reel, blades visible edge-on."

### Rigging / Silhouette Guidance

- Rig the **wings (push-handle) as posable** — Phase 1 keeps them folded; the fantasy is they flare open in Phase 2 ("Rev"). Keep the handle-wing joints clean in the skeleton so the animator can open them.
- The **blade-reel** should be a separable spinning submesh (its own material slot) so the "spinning heart" can rotate as a driven part in Phase 2 without deforming the torso — mirrors the SpinCycle drum-spin pattern.
- Keep it **broad and low-and-heavy at the base** (wheel-sandals wide apart) so the ground-shake / spin-dash reads as weighty.
- Petal flecks are diffuse-texture detail only — do not model petals as geometry (tri budget + it "shakes petals loose" as a VFX, not mesh).

---

## Asset BD-E2: Leaf Pile Lurker  *(enemy, rigged)*

**Unity asset name:** `char_leaf_pile_lurker.fbx`
**Meshy Style:** Low Poly
**Symmetry:** OFF (organic, uneven leaf clumping)
**Poly budget:** ~10–12k tris (standard enemy tier)
**Import route:** `/unity-character-importer`
**Priority:** HIGH — no prefab exists yet (GDD §4 build dependency); needed for Rock Garden + Koi Pond rooms.

**What it is:** A pile of dead leaves reimagined as a humanoid ambusher — dormant as a leaf pile until the player is close, then it rises. Tone: **"sad more than menacing"** — weary, slumped, a little collapsed.

### Meshy Text Prompt

```
A humanoid ambusher made of dead autumn leaves, stylized low-poly character, T-pose for rigging. A slumped hunched figure whose whole body is clustered dry curled leaves packed into a rough person shape, thin drooping arms, a lowered head, sagging shoulders. Reads sad and weary more than menacing, a little collapsed. Muted brown, dusty tan, and faded jade-green leaves with a few cherry-pink petals caught in it, dark hollow eye gaps. Corrugated cardboard grain on the leaf clumps, bold marker-drawn outlines on all edges. Hard faceted low-poly geometry. Soft rounded lumpy silhouette, clearly a pile-shaped stooped figure, distinct from stocky armored gnomes. Cardboard-and-marker aesthetic, feudal Japanese dojo.
```

### Silhouette Check (10% test)

- As a solid black shape: a **low, rounded, lumpy, stooped mound with a bowed head and drooping arms** — reads as a sad slouch. This is distinct from the **gnome's stocky-but-upright armored blockiness** and from the **Crane's tall thin vertical line**. Passes.
- The "sad, not menacing" read comes from posture: **head down, shoulders sagging, arms hanging.** Do not let Meshy generate an aggressive lunging pose — refine if needed: "stooped and drooping, head bowed, defeated posture, not aggressive."
- Keep it **shorter than the player** — a small collapsed figure. Height reinforces the "pile" read.

### Rigging / Silhouette Guidance

- Rig with a **standard humanoid skeleton** despite the lumpy shape — the ambush "rise from dormant pile" is a played animation (crumple down into a flat pile → rise into the stooped figure). Ensure spine/arm bones can collapse it convincingly; weight the leaf clumps to the nearest bone.
- The **dormant leaf-pile form** can be a separate low pose in the same rig (or a swapped simple pile mesh) — decide in the character-importer pass; flag to `unity-senior-developer` that the AI needs a dormant→rise state (GDD §2 Rock Garden mechanic).
- Leaf clumps are lumpy but must stay **low-poly** — cluster leaves into faceted clumps, do not model individual leaves.

---

## Weapon — Water Whip (NOT authored here)

The **Water Whip** (garden hose → water dragon-whip, GDD §6) is a **weapon**, not a rigged character. It routes through `/asset-pipeline`, not `/unity-character-importer`, and its prompt belongs with weapon prompts — intentionally out of scope for this character file. It is design-complete but art-pending (GDD Open Question #4). Author its Meshy prompt + inventory icon separately when it is greenlit for World 2.

---

## Generation / Build Order

1. **Leaf Pile Lurker (BD-E2)** — hard build dependency (no prefab exists); needed in two rooms before scene construction can proceed.
2. **Crane Duelist (BD-E1)** — debut enemy, needed for Training Hall + Koi Pond rooms.
3. **The Grasscutter (BD-B1)** — boss; can build in parallel with the boss-room scene.

## Delivery Paths

| Asset | Unity Filename | Raw Download Path | Import Route |
|---|---|---|---|
| Crane Duelist | `char_crane_duelist.fbx` | `models/zips/` | `/unity-character-importer` |
| The Grasscutter | `char_grasscutter.fbx` | `models/zips/` | `/unity-character-importer` |
| Leaf Pile Lurker | `char_leaf_pile_lurker.fbx` | `models/zips/` | `/unity-character-importer` |

**After download:** place the Meshy zips in `models/zips/` and run `/unity-character-importer [Name] [zipPath]` for each.

---

*Prompts prepared: 2026-08-07 | By: art-direction-agent | Companion to `meshy-prompts.md` (ENV) in this folder.*
