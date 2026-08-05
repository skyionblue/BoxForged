# BoxForged — Cutscene Production Document

**For:** PixVerse + Gemini AI video generation  
**Platform:** Mobile (16:9 landscape, 10–20s per cutscene)  
**Art Style:** Stylized 3D with cardboard-and-marker aesthetic. Chunky proportions, hand-drawn marker details, craft-material textures. Dual-layer world: drained (grey, muted) → reclaimed (vivid, saturated, warm). See `art-style-guide.md` for color codes and visual reference.

---

## Art Style Summary

**The Two-Layer World:**  
Every location in BoxForged exists in two visual states. The **Drained state** (controlled by The Unimaginative) uses desaturated grey-browns: warm grey `#C8BFB0`, darker grey-brown `#A89F94`, pale grey sky `#D4CFC9`. Everything feels abandoned, flat, lifeless. The **Reclaimed state** (restored by the player's imagination) uses vivid marker colors: craft tan `#E8C97A`, marker blue sky `#4A90D9`, marker green grass `#5CB85C`, marker red `#E05A4E`, marker orange `#F5A623`, marker purple `#9B59B6`, marker gold `#F7C244`. The transition between these states is the emotional core of the game.

**Cardboard & Marker Aesthetic:**  
Surfaces look like corrugated cardboard or kraft paper with visible grain. Outlines and details are hand-drawn marker strokes (slightly uneven, dark brown `#3D2B1F`). Bright colors mimic Crayola markers on cardboard — vivid but warm, never neon. Characters are chunky (head ~35% of height, stocky proportions). The box on the player's head is a perfect cube, oversized, with marker-drawn details (ninja: dark navy `#1A1A2E` + purple lines; cowboy: leather brown `#8B5E2A` + gold lines). Lighting in URP: drained state uses flat diffuse (no highlights, fog-like ambient); reclaimed state uses warm directional light with soft shadows and vivid saturation. Visual references: Tearaway (craft materials), A Short Hike (warm simplicity), Little Big Planet (craft construction).

---

## Cutscene List

| # | Title | Phase | Priority | Target Duration | Trigger | Location |
|---|---|---|---|---|---|---|
| 1 | Opening: Kid Finds the Box | Intro | **P0** | 15–18s | Game start | Backyard (drained) |
| 2 | SpinCycle Boss Intro | 1 | **P0** | 8–10s | Enter boss room | Spin Arena (Backyard) |
| 3 | Backyard Reclaimed | 1 | **P0** | 12–15s | SpinCycle defeated | Full Backyard zone |
| 4 | Cul-de-Sac Arrival | 2 | **P1** | 10–12s | First entry to Cul-de-Sac zone | Dead-end street |
| 5 | The Skeptic — First Encounter | 2 | **P1** | 8–10s | Room 3 or 4, Cul-de-Sac | Mid-street |
| 6 | Command Node Revealed | 2 | P2 | 6–8s | Enter Room 5 | Town Square (Cul-de-Sac) |
| 7 | World Tree Glimpse | 2 (late) | P2 | 10–12s | Cardboard Mill boss defeated | Rooftop or high vantage point |
| 8 | The Friend Appears | Mid-game | P3 | *Reserved* | TBD — locked for later | *TBD* |
| 9 | Ending: The World Tree Blooms | 3 | P3 | 15–20s | Final boss defeated | World Tree base |

**Priority key:**
- **P0** = Phase 1 prototype — must ship
- **P1** = Phase 2 priority — Cul-de-Sac launch
- **P2** = Nice-to-have for Phase 2
- **P3** = Phase 3+ content

---

## Cutscene 1: Opening — Kid Finds the Box

**Narrative Beat:**  
Kid is in his backyard. The world is grey, quiet, abandoned. He finds a cardboard box half-buried near the fence. The moment he puts it on his head, the world flickers — just for a heartbeat — into color. He sees what was always there. The adventure begins.

**Tone:** Quiet → discovery → wonder. No dialogue. Let the visual transformation carry the moment.

---

### Shot 1 — The Grey Backyard
**Duration:** 4 seconds  
**Camera:** Wide, static, low angle (waist height) — looking across the overgrown backyard toward the dead apple tree  
**What happens:** Kid stands in the center of the frame, back to camera, looking at the grey yard. Overgrown grass, yellowed and flat. Broken wooden fence, warped planks. Dead apple tree in the distance — bare branches, no leaves. Everything is muted grey-brown. Kid's shoulders slump slightly — the posture of a kid who's used to boredom.  
**Visual prompt for PixVerse/Gemini:**  
"A wide shot of an abandoned suburban backyard. An 8-year-old boy in jeans and t-shirt stands with his back to camera, looking at the yard. Overgrown grass: yellowed, hex `#C8BFB0` warm grey. Dead apple tree with bare branches (dark grey-brown `#A89F94`) in the background. Broken wooden fence: grey warped planks (`#A89F94`), corrugated cardboard texture. Sky: pale grey `#D4CFC9`, fog-like, no blue. Lighting: flat diffuse, no shadows, fog-like ambient. Surfaces: corrugated cardboard grain, kraft paper texture. Outlines: dark brown marker strokes `#3D2B1F`, slightly uneven. Art style: stylized 3D, cardboard and kraft paper materials, hand-drawn marker outlines, craft aesthetic. Visual ref: Tearaway craft materials, post-apocalyptic suburban. Camera: static, low angle (waist height). **PixVerse note:** Keep motion minimal — boy's shoulders may slump slightly, but no walking. Static shot preferred for opening. Mood: quiet, abandoned, drained."  
**Dialogue:** None.

---

### Shot 2 — Kid Finds the Box
**Duration:** 3 seconds  
**Camera:** Medium close-up, side angle — Kid crouches, reaches toward the fence  
**What happens:** Kid notices something near the base of the fence — a cardboard box, half-buried under dead leaves. He walks over (camera follows), crouches, brushes the leaves away. The box is plain, unmarked, slightly scuffed. He picks it up, turns it over in his hands. Curious, not excited yet.  
**Visual prompt for PixVerse/Gemini:**  
"A medium close-up of an 8-year-old boy crouching near a broken wooden fence. He picks up a plain cardboard box from a pile of dead leaves (leaves: dark grey-brown `#A89F94`). The box: unmarked, slightly scuffed, craft-tan cardboard `#E8C97A` with visible corrugation lines. Boy's expression: curious but calm. Background: grey overgrown grass `#C8BFB0`, muted fence `#A89F94` (cardboard texture, hand-drawn wood grain marker lines `#3D2B1F`). Lighting: flat diffuse, no highlights. Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines (dark brown `#3D2B1F`), craft materials. Visual ref: Tearaway. Camera: side angle, slow follow (smooth motion, not jerky). **PixVerse note:** Subtle motion only — boy crouches and reaches. Avoid fast hand movements (causes blur). Mood: discovery, quiet."  
**Dialogue:** None.

---

### Shot 3 — Kid Puts on the Box
**Duration:** 3 seconds  
**Camera:** Medium shot, frontal — Kid lifts the box, places it on his head  
**What happens:** Kid stands, holds the box at waist level for a beat, then lifts it over his head. The moment it settles on his head, the camera holds on his silhouette — box covering his face, arms still raised slightly. A single heartbeat of stillness. Then the box begins to glow faintly from within — warm orange light leaking from the bottom edge.  
**Visual prompt for PixVerse/Gemini:**  
"A medium frontal shot of an 8-year-old boy placing a cardboard box over his head. The box: plain, unmarked craft-tan cardboard `#E8C97A` with visible corrugation lines. As it settles, a faint warm orange glow `#F5A623` begins to leak from the bottom edge where it meets his shoulders (subtle emission, soft glow, not harsh). Background: grey suburban backyard `#C8BFB0`, muted fence `#A89F94`. Sky: pale grey `#D4CFC9`. Lighting: flat diffuse transitioning to faint warm glow from below. Boy's posture: arms slightly raised, box covering face. Surfaces: corrugated cardboard texture. Art style: stylized 3D, craft materials, hand-drawn outlines. Camera: static, frontal. **PixVerse note:** Keep motion slow and deliberate — arms lift box, settle it on head. The glow should appear gradually (1–2 seconds). Avoid fast movements. Mood: anticipation, magic just beginning."  
**Dialogue:** None.

---

### Shot 4 — The World Flickers into Color
**Duration:** 5 seconds  
**Camera:** Wide shot, rotating 180° around Kid (or fast cut: grey → vivid)  
**What happens:** The world flickers. Grey bleeds into color — not all at once, but in waves. The dead apple tree blooms pink cherry blossoms. The grass shifts from yellow-grey to vivid marker green. The fence straightens, wood grain details appear drawn in marker. The sky shifts from pale grey to warm afternoon blue. Kid lowers his arms, turns slowly, looking around. The box on his head now has marker-drawn details — purple ninja mask lines appearing stroke by stroke, as if drawn in real-time.  
**Visual prompt for PixVerse/Gemini:**  
"A wide shot of a suburban backyard transforming from drained grey to vivid color. Initial state: grey grass `#C8BFB0`, dead apple tree (bare branches `#A89F94`), pale grey sky `#D4CFC9`, flat diffuse lighting. Transformation: dead apple tree blooms vivid pink cherry blossoms (marker pink, vivid). Grass shifts from yellowed grey `#C8BFB0` to vivid marker green `#5CB85C` in a wave spreading outward. Wooden fence straightens, marker-drawn wood grain details (dark brown `#3D2B1F` strokes) appear stroke by stroke. Sky transitions from pale grey `#D4CFC9` to warm afternoon blue `#4A90D9`. An 8-year-old boy stands center frame, cardboard box on head — purple marker lines `#9B59B6` drawing themselves onto the box (ninja mask design, hand-drawn strokes appearing in real-time). Boy turns slowly, looking around. Lighting shifts from flat diffuse to warm directional with soft shadows. Surfaces: corrugated cardboard, kraft paper, hand-drawn marker details. Art style: stylized 3D, cardboard-and-marker aesthetic, color-bloom transformation effect (like watercolor spreading). Visual ref: Tearaway, Hollow Knight zone reclaim. Camera: wide, slow 180° rotation around boy (1–2 RPM max) OR fast cut from grey to color (safer for AI generation). **PixVerse note:** Prefer fast cut over rotation — 180° camera moves often generate warping artifacts. If rotation used, VERY slow. Keep boy's motion minimal (head turn only). Transformation should feel magical, not chaotic. Mood: transformation, wonder, imagination awakening."  
**Dialogue (optional, Kid's voice — soft, certain):**  
KID: "I can see it."

---

## Cutscene 2: SpinCycle Boss Intro

**Narrative Beat:**  
Kid enters the boss arena — a circular colosseum of stacked washing machine drums. The ground is a spinning mosaic floor. SpinCycle stands at the center: a heavyweight brawler with a washing machine drum for a head, muscular build, torn shorts, mismatched sneakers. The drum rotates slowly. SpinCycle cracks his knuckles. Kid draws his weapon. The fight begins.

**Tone:** Intimidation → Kid's resolve. Brief, punchy. No long villain monologue — SpinCycle doesn't talk, he *looms*.

---

### Shot 1 — Kid Enters the Arena
**Duration:** 2.5 seconds  
**Camera:** Low angle, wide — Kid walks through an archway into the circular arena  
**What happens:** Kid steps through a doorway (stacked washing machine drums form an archway). Camera is low, looking up at him as he enters. Behind him: the vivid reclaimed backyard (green, pink blossoms). Ahead of him: the Spin Arena — circular colosseum, drum-stack walls, spinning mosaic floor in the center. Light streams down from above. Kid pauses at the threshold, weapon in hand (katana or bo staff).  
**Visual prompt for PixVerse/Gemini:**  
"A low-angle wide shot of an 8-year-old boy with a cardboard box (ninja mask design: dark navy `#1A1A2E` box with hand-drawn purple marker lines `#9B59B6`, perfect cube, oversized) walking through an archway into a circular arena. The archway: stacked washing machine drums (metallic grey, corrugated cardboard texture on drum shells). Boy holds a cardboard katana (craft-tan `#E8C97A`, marker-drawn blade details `#3D2B1F`). Boy proportions: chunky (head 35% height, stocky build). Behind him: vivid green grass `#5CB85C`, pink cherry blossoms (marker pink), warm directional light. Ahead: circular colosseum with drum-stack walls, spinning mosaic floor (colorful cardboard tiles). Light streams down from above (warm golden `#F5A623`). Lighting: warm directional with soft shadows. Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`, chunky proportions. Visual ref: Tearaway. Camera: low angle (looking up), static. **PixVerse note:** Slow walk only — fast walking generates foot-slide artifacts. Keep camera static. Mood: threshold moment, stepping into danger."  
**Dialogue:** None.

---

### Shot 2 — SpinCycle Reveal
**Duration:** 3 seconds  
**Camera:** Medium shot, slow push-in on SpinCycle  
**What happens:** SpinCycle stands at the center of the arena, facing away. Camera pushes in slowly. His silhouette: broad shoulders, torn vest, torn shorts, mismatched sneakers (one blue, one red). The washing machine drum on his head rotates slowly — a porthole window visible, spinning into view then away. He cracks his knuckles (audible). Does not turn yet.  
**Visual prompt for PixVerse/Gemini:**  
"A medium shot of a heavyweight brawler character (SpinCycle boss) standing in the center of a circular arena. His head: washing machine drum (front-loader, porthole window visible, rotating slowly — metallic grey with corrugated cardboard texture on drum shell). Muscular build (exaggerated chunky proportions), torn vest (craft paper texture, grey-brown `#A89F94`), torn shorts, mismatched sneakers (one marker blue `#4A90D9`, one marker red `#E05A4E`). Hands crack knuckles (subtle motion). Back to camera. Background: spinning mosaic floor (colorful cardboard tiles), drum-stack colosseum walls (metallic grey with cardboard texture). Lighting: warm directional from above, soft shadows. Art style: stylized 3D, corrugated cardboard and kraft paper materials, hand-drawn marker outlines `#3D2B1F`, chunky exaggerated proportions. Visual ref: Tearaway, Psychonauts chunky design. Camera: slow push-in (1–2 seconds). **PixVerse note:** Minimal motion — drum rotation and knuckle crack only. Slow camera push preferred over fast zoom. Mood: intimidation, silent threat."  
**Dialogue:** None (sound: knuckle crack, drum rotation hum).

---

### Shot 3 — Kid's Resolve
**Duration:** 2.5 seconds  
**Camera:** Close-up, frontal on Kid's box — then pull back to medium shot  
**What happens:** Close-up on Kid's cardboard ninja mask (purple marker lines, eye cutouts). Camera holds for one beat, then pulls back to reveal Kid in fighting stance — katana raised, feet planted. His posture: ready, not afraid. SpinCycle turns (off-camera) — the drum rotation sound accelerates.  
**Visual prompt for PixVerse/Gemini:**  
"A close-up of a cardboard box with hand-drawn purple marker lines `#9B59B6` (ninja mask design, eye cutouts — dark voids or faint glow). Box: dark navy `#1A1A2E`, perfect cube, corrugated cardboard texture, marker strokes slightly uneven (hand-drawn feel). Camera pulls back to reveal an 8-year-old boy in fighting stance — cardboard katana (craft-tan `#E8C97A`) raised, feet planted wide. Boy: chunky proportions (head 35% height), jeans, sneakers, box on head. Background: circular arena, spinning mosaic floor (blurred motion), drum-stack walls. Lighting: warm directional with soft shadows. Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`. Visual ref: Tearaway. Camera: close-up on box → smooth pullback to medium shot (2 seconds). **PixVerse note:** Pullback motion only — boy should be static in stance. Avoid repositioning feet during pullback (causes sliding). Mood: determination, readiness."  
**Dialogue (optional, Kid's voice — short, certain):**  
KID: "Let's go."

---

## Cutscene 3: Backyard Reclaimed

**Narrative Beat:**  
SpinCycle is defeated. The drum slows, grinds, stops. He collapses. For a moment, silence. Then pink cherry blossom petals begin to drift in — first one, then dozens. Color washes across the entire Backyard zone. The Imagination Restore fires. Grey bleeds into vivid warmth. The dead tree blooms fully. The fence straightens. Kid stands in the center, breathing hard, then looks up — he sees what he's won.

**Tone:** Quiet triumph → blooming wonder. Let the visual transformation be the hero of this moment.

---

### Shot 1 — SpinCycle Falls
**Duration:** 3 seconds  
**Camera:** Wide, static — SpinCycle collapses in the center of the arena  
**What happens:** SpinCycle staggers, the drum on his head slows its rotation, grinds, then stops. He drops to one knee, then collapses forward onto the arena floor. The spinning mosaic floor beneath him slows and stops. Silence. Kid stands in the background, weapon lowered, breathing hard.  
**Visual prompt for PixVerse/Gemini:**  
"A wide shot of a heavyweight brawler character (SpinCycle boss: washing machine drum for head — metallic grey with cardboard texture, porthole visible; muscular build, torn vest grey-brown `#A89F94`, torn shorts, mismatched sneakers) collapsing forward onto a circular arena floor. The drum on his head slows rotation and stops (porthole window settles facing down). The spinning mosaic floor (colorful cardboard tiles) beneath him grinds to a halt. Background: circular colosseum of drum-stack walls (metallic grey, corrugated cardboard texture). An 8-year-old boy with cardboard ninja mask (dark navy `#1A1A2E`, purple marker lines `#9B59B6`) stands in background, cardboard katana lowered. Lighting: warm directional with soft shadows. Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`, chunky exaggerated proportions. Camera: wide, static. **PixVerse note:** Slow-motion collapse preferred — fast fall generates motion blur. Keep boy static in background. Mood: defeat, moment of silence."  
**Dialogue:** None (sound: drum grinding to a stop, heavy breathing).

---

### Shot 2 — First Cherry Blossom
**Duration:** 3 seconds  
**Camera:** Close-up, slow motion — a single pink cherry blossom petal drifts down into frame  
**What happens:** Camera focuses on empty air above the arena. A single pink cherry blossom petal drifts down in slow motion. It floats gently, catching the light. Behind it, out of focus: the grey backyard visible through the arena archway. The petal lands on SpinCycle's collapsed drum-head.  
**Visual prompt for PixVerse/Gemini:**  
"A close-up slow-motion shot of a single pink cherry blossom petal (vivid marker pink, hand-drawn marker strokes `#3D2B1F` on edges) drifting downward through the air. Soft focus background: grey suburban backyard `#C8BFB0` visible through an archway. The petal floats gently, catching warm afternoon light (golden `#F5A623` rim lighting). It lands on the surface of a washing machine drum (out of focus, foreground — metallic grey with cardboard texture). Lighting: warm soft directional from above, gentle glow. Art style: stylized 3D, cardboard-and-marker aesthetic, craft paper texture on petal. Visual ref: Tearaway. Camera: close-up, slow motion (0.5x speed). **PixVerse note:** Simple motion only — single petal falling straight down. Avoid complex tumbling (generates artifacts). Soft focus background critical to hide detail. Mood: quiet, the calm before transformation."  
**Dialogue:** None (sound: soft wind chime or single bell tone).

---

### Shot 3 — The Bloom Begins
**Duration:** 4 seconds  
**Camera:** Medium shot, Kid's POV — looking out through the arena archway at the backyard  
**What happens:** Kid turns toward the archway (camera is his POV). Through the archway, the backyard is still grey. Then color begins to bleed in — like watercolor spreading across wet paper. The dead apple tree's branches glow faintly, then *explode* into pink cherry blossoms. The grass shifts from yellow-grey to vivid marker green in a wave spreading outward from the tree. The fence straightens, wood grain details appearing stroke by stroke.  
**Visual prompt for PixVerse/Gemini:**  
"A medium shot from character's point of view (Kid's POV), looking through an archway into a suburban backyard. Initial state: yard grey and muted — grass `#C8BFB0`, dead apple tree (bare branches `#A89F94`), fence `#A89F94`, sky `#D4CFC9`, flat diffuse lighting. Transformation begins: color bleeds in like watercolor spreading on wet paper. Dead apple tree in center glows faintly (warm orange `#F5A623` from within), then blooms vivid pink cherry blossoms (marker pink, hundreds of blossoms bursting outward). Grass shifts from yellowed grey `#C8BFB0` to vivid marker green `#5CB85C` in a wave spreading outward from tree (ripple effect). Wooden fence straightens, hand-drawn wood grain details (dark brown marker strokes `#3D2B1F`) appearing stroke by stroke in real-time. Sky transitions from pale grey `#D4CFC9` to warm afternoon blue `#4A90D9`. Lighting shifts from flat diffuse to warm directional with soft shadows. Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker details, color-bloom transformation (watercolor bleed effect). Visual ref: Tearaway, Hollow Knight zone reclaim. Camera: static, medium shot POV (no movement). **PixVerse note:** Complex transformation — may benefit from splitting into two shorter clips (tree bloom, then grass/fence). Keep camera locked (POV shots with motion cause nausea). Transformation should spread outward, not pop instantly. Mood: wonder, imagination restoring the world."  
**Dialogue:** None (sound: rising musical swell, wind chimes, soft rustling).

---

### Shot 4 — Kid Sees the Reclaimed World
**Duration:** 5 seconds  
**Camera:** Wide, high angle — looking down at Kid standing in the center of the now-vivid arena, with the reclaimed backyard visible beyond  
**What happens:** Camera pulls back and rises to a high angle. Kid stands in the center of the arena, weapon at his side, looking out at the transformed backyard. The arena itself shifts from grey drum-stacks to warm wood and paper-lantern decorations. Cherry blossoms drift everywhere. The backyard is now a feudal Japanese dojo courtyard — stone walls, training dummies, vivid green grass, pink blossoms, warm afternoon light. Kid tilts his head back slightly, taking it all in.  
**Visual prompt for PixVerse/Gemini:**  
"A wide high-angle shot of a circular arena transforming into a feudal Japanese dojo courtyard. An 8-year-old boy with cardboard ninja mask (dark navy `#1A1A2E`, purple marker lines `#9B59B6`, perfect cube) stands in center, looking out at vivid suburban backyard now reimagined as training ground. Boy: chunky proportions, katana at side. Cherry blossom petals (vivid marker pink) drift through air everywhere. Grass: vivid marker green `#5CB85C`. Wooden fence: warm wood tone with hand-drawn grain details (dark brown marker strokes `#3D2B1F`). Stone walls (grey cardboard with marker-drawn stone lines) and paper lanterns (warm orange `#F5A623` glow) appear. Sky: warm afternoon blue `#4A90D9` with golden sunlight (warm directional lighting, soft shadows). Arena floor shifts from grey mosaic to warm wood planks with marker grain. Training dummies appear (cardboard tubes, rope). Lighting: warm directional with vivid saturation. Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker details, vivid saturated colors (Crayola marker on cardboard — vivid but warm, not neon). Visual ref: Tearaway, A Short Hike warm palette. Camera: wide, high angle (30–45° above, looking down). **PixVerse note:** Complex scene — static camera strongly recommended. Transformation should be near-complete at start, with final details (lanterns, petals) appearing. Avoid camera motion during transformation (causes disorientation). Mood: triumph, wonder, imagination fully restored."  
**Dialogue (optional, Kid's voice — soft, certain):**  
KID: "It's real."

---

## Cutscene 4: Cul-de-Sac Arrival

**Narrative Beat:**  
Kid steps out of the Backyard zone and into a new location: the Cul-de-Sac. It's a curved dead-end street. Everything is grey — cracked asphalt, dark-windowed houses, empty mailboxes. Kid stops, looks around. Then he adjusts the box on his head. The Wild West overlay begins to shimmer into view — faint at first, then stronger. The minivans become covered wagons. The houses grow saloon facades. Kid nods once. A new fight.

**Tone:** Drained → Kid recognizes the potential → overlay awakens. No dialogue — Kid's action (adjusting the box) is the signal.

---

### Shot 1 — Transition: Kid Walks Toward the Street
**Duration:** 2.5 seconds  
**Camera:** Medium shot, following Kid from behind as he walks down a residential sidewalk  
**What happens:** Kid walks along a cracked sidewalk toward a curved dead-end street (the Cul-de-Sac). Camera follows from behind. The Backyard is behind him (vivid, blooming). Ahead: grey. The street curves into view — cracked asphalt, empty mailboxes, dark-windowed houses with for-sale signs, abandoned minivans. Everything is muted grey-brown. Kid's pace slows as he approaches.  
**Visual prompt for PixVerse/Gemini:**  
"A medium shot following an 8-year-old boy with a cardboard box (cowboy design: leather brown `#8B5E2A`, gold marker lines `#F7C244` drawing brim, perfect cube) from behind as he walks along a cracked sidewalk toward a curved dead-end street. Boy: chunky proportions. Behind him: vivid green grass `#5CB85C` and pink cherry blossoms (reclaimed Backyard zone), warm directional lighting. Ahead: grey suburban cul-de-sac — cracked asphalt `#A89F94`, dark-windowed houses (grey-brown `#C8BFB0`), empty mailboxes, abandoned minivans (grey), muted grey-brown palette `#C8BFB0` base, flat diffuse lighting ahead. Clear visual boundary between vivid reclaimed (behind) and drained grey (ahead). Sidewalk: cracked concrete with cardboard texture. Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`. Visual ref: Tearaway. Camera: medium shot, following from behind (slow tracking, 1–2 fps movement). **PixVerse note:** Slow walk only — fast walking causes foot-slide. Camera should track smoothly, not shake. Clear color gradient from vivid (reclaimed) to grey (drained) is critical visual beat. Mood: leaving safety, entering drained territory."  
**Dialogue:** None.

---

### Shot 2 — Kid Stops, Looks Around
**Duration:** 3 seconds  
**Camera:** Wide, static — Kid stands at the edge of the cul-de-sac, looking left and right  
**What happens:** Kid stops at the curb. Camera widens to show the full cul-de-sac: a curved street with houses on both sides, a center island with a cracked stone birdbath, abandoned minivans parked at angles. Everything is grey. No movement. No sound except wind. Kid turns his head slowly, scanning the street. His posture: wary but not afraid.  
**Visual prompt for PixVerse/Gemini:**  
"A wide static shot of a curved suburban dead-end street (cul-de-sac). Cracked asphalt: dark grey-brown `#A89F94` with cardboard texture. Dark-windowed houses: grey-brown `#C8BFB0`, sun-bleached with for-sale signs (faded cardboard signs). Empty mailboxes: grey metal with corrugated texture. Abandoned minivans: grey, dusty (cardboard vehicle shells). Center island: cracked stone birdbath (grey stone `#A89F94`, dry basin). Muted drained-state palette: warm grey `#C8BFB0` base, darker grey-brown `#A89F94` shadows, pale grey sky `#D4CFC9`. Flat diffuse lighting, no shadows, fog-like ambient. An 8-year-old boy with cardboard box (cowboy: leather brown `#8B5E2A`, gold marker lines `#F7C244`) stands at curb in foreground, head turning left and right (slow scan motion). Boy: chunky proportions. Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`. Visual ref: Tearaway, post-apocalyptic suburban. Camera: wide, static (no movement). **PixVerse note:** Minimal motion — boy's head turn only. Static environment. Keep lighting flat (no dramatic shadows). Mood: abandoned, quiet tension."  
**Dialogue:** None (sound: distant wind, faint static hum from the birdbath).

---

### Shot 3 — Kid Adjusts the Box
**Duration:** 2 seconds  
**Camera:** Close-up, side angle — Kid's hands reach up and adjust the box on his head  
**What happens:** Close-up on Kid's profile. His hands reach up, grasp the sides of the cardboard box, and adjust it — tilting it slightly, then settling it firmly back in place. The box glows faintly from within (warm orange light). Kid's expression (visible through the eye cutout): focused, determined.  
**Visual prompt for PixVerse/Gemini:**  
"A close-up side-angle shot of an 8-year-old boy adjusting a cardboard box on his head. His hands grasp the sides of the box (cowboy design: leather brown `#8B5E2A`, gold marker-drawn brim `#F7C244`, perfect cube, corrugated cardboard texture) and tilt it slightly (1–2° tilt), then settle it firmly back in place. The box glows faintly from within — warm orange light `#F5A623` leaking from bottom edge (subtle emission, soft glow). Boy's face partially visible through eye cutout: focused, determined expression. Background: blurred grey street `#C8BFB0` (depth-of-field blur). Lighting: faint warm glow from below (box emission), otherwise flat diffuse. Hands: simple geometry, cardboard-like skin tone. Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`. Visual ref: Tearaway. Camera: close-up, side angle (45° from profile). **PixVerse note:** Subtle hand motion only — grasp, tilt 1–2°, settle. Avoid fast hand movements (blur). Glow should pulse gently (not strobe). Mood: resolve, activating the lens."  
**Dialogue:** None.

---

### Shot 4 — The Wild West Overlay Awakens
**Duration:** 4.5 seconds  
**Camera:** Wide, slow push-in on the cul-de-sac as the overlay shimmers into view  
**What happens:** The grey cul-de-sac begins to transform — but not fully. The Wild West overlay shimmers into existence like a heat mirage. The minivans flicker and partially become covered wagons (canvas tops visible, wooden wheels). The house facades shimmer with painted saloon fronts. Hitching posts appear at the curb. The sky begins to shift from pale grey to faint golden hour. The transformation is incomplete — The Unimaginative still hold this place. Kid steps forward into the street.  
**Visual prompt for PixVerse/Gemini:**  
"A wide shot of a curved suburban cul-de-sac transforming partially into a Wild West main street. Initial state: grey street `#C8BFB0`, abandoned minivans (grey), houses (grey-brown), pale grey sky `#D4CFC9`, flat diffuse lighting. Transformation (partial, incomplete): The street shimmers (heat mirage effect). Abandoned minivans flicker and partially become covered wagons — canvas tops (faded canvas `#E8D6A0`), wooden wheels (weathered brown `#8B6914`) visible overlaid translucently on grey vehicles (50% opacity overlay, grey still visible underneath). House facades shimmer with painted saloon fronts (sun-bleached wood `#C4A46A`, marker-drawn signage `#3D2B1F`) appearing as translucent overlays (60% opacity, grey house visible underneath). Hitching posts (knotted rope brown `#8B5E2A`, marker grain lines) appear at curb. Sky transitions from pale grey `#D4CFC9` to faint golden hour (warm amber `#F5A623` gradient, but muted — 50% saturation, grey bleeds through). Lighting shifts from flat diffuse to faint warm directional (soft shadows starting to appear). The transformation is incomplete — grey drained state bleeds through warm tones everywhere (layered reality: 50% drained, 50% reclaimed). An 8-year-old boy with cardboard box (cowboy: leather brown `#8B5E2A`, gold marker brim `#F7C244`) steps forward into street (slow walk). Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`, layered dual-state reality effect (translucent warm overlay on grey base). Visual ref: Tearaway, A Short Hike. Camera: wide, slow push-in (1–2 seconds). **PixVerse note:** Complex layered effect — may benefit from gradual fade-in rather than instant overlay. Keep boy's walk slow. Translucent overlays are critical visual beat (shows incomplete reclamation). Avoid fast shimmering (causes flicker artifacts). Mood: imagination awakening, incomplete reclamation, challenge ahead."  
**Dialogue (optional, Kid's voice — one word, certain):**  
KID: "Okay."

---

## Cutscene 5: The Skeptic — First Encounter

**Narrative Beat:**  
Mid-run in the Cul-de-Sac, Kid clears a room. A figure appears at the far end of the street. The Skeptic. A crushed, grey, undecorated box on his head. Flat grey clothes. He walks forward slowly. Kid readies his weapon. The Skeptic stops ten feet away. Speaks one flat line. Then turns and walks away, fading into grey static. Kid is left unsettled — not afraid, but shaken. The world desaturates slightly for a moment, then returns.

**Tone:** Unease. The Skeptic doesn't attack — his presence is the attack. Leave silence after his line.

---

### Shot 1 — The Skeptic Appears
**Duration:** 3 seconds  
**Camera:** Wide, static — Kid stands in the street; The Skeptic appears in the distance  
**What happens:** Kid stands in the center of the Cul-de-Sac street, weapon lowered (just finished a fight). The street is partially reclaimed — warm tones bleeding into the grey. At the far end of the street, a figure fades into view from grey static. The Skeptic: a boy the same age as Kid, wearing a crushed grey cardboard box (no marker lines, no decoration — just raw grey cardboard). Flat grey clothes. He walks forward slowly, hands at his sides.  
**Visual prompt for PixVerse/Gemini:**  
"A wide static shot of a partially reclaimed Wild West cul-de-sac street. An 8-year-old boy with cardboard box (cowboy: leather brown `#8B5E2A`, gold marker brim `#F7C244`, chunky proportions) stands center frame, cardboard weapon lowered. Background: partially transformed street — warm amber tones `#F5A623` bleeding into grey `#C8BFB0` (translucent overlay, 50% reclaimed). At far end of street, a second boy (The Skeptic) fades into view from grey static (particle fade-in effect, grey `#787878` static noise). The Skeptic: same age as player, wears a crushed, undecorated grey cardboard box on head (flat grey `#787878`, NO marker lines, NO decoration, raw grey cardboard, corrugated texture visible, crushed/dented). Flat grey clothes (grey t-shirt, grey jeans — no warm tones anywhere). He walks forward slowly (deliberate pace, 1 fps), hands at sides (no weapon). Lighting: warm directional on player side, flat diffuse grey on Skeptic side (visual contrast). Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F` (only on player — Skeptic has NO marker details). Visual contrast critical: vivid player vs. grey Skeptic. Dual-layer world (reclaimed warm + drained grey). Visual ref: Tearaway, Hollow Knight Shade aesthetic. Camera: wide, static (no movement). **PixVerse note:** Slow fade-in for Skeptic (2 seconds from static to solid). Slow walk only. Visual contrast between player (warm, decorated) and Skeptic (grey, undecorated) is THE key beat. Keep lighting divided (warm left, flat right). Mood: unease, something wrong."  
**Dialogue:** None (sound: static hum rising, wind stops).

---

### Shot 2 — Kid Readies, The Skeptic Stops
**Duration:** 2.5 seconds  
**Camera:** Medium two-shot — Kid in foreground, The Skeptic in background, ten feet apart  
**What happens:** Kid raises his weapon (katana, lasso, or quickdraw blade) into a ready stance. The Skeptic stops walking ten feet away. Does not raise a weapon. Does not move. Just stands. The camera holds the two-shot — Kid's colorful box vs. The Skeptic's grey box. Silence.  
**Visual prompt for PixVerse/Gemini:**  
"A medium two-shot of two 8-year-old boys facing each other in a Wild West street, ten feet apart. Foreground: player boy with colorful cardboard box (cowboy: leather brown `#8B5E2A`, gold marker brim `#F7C244`, hand-drawn marker details, chunky proportions) raises cardboard weapon (lasso or quickdraw blade) into ready stance. Background: The Skeptic with crushed, undecorated grey cardboard box (flat grey `#787878`, NO marker lines, NO decoration, raw grey cardboard, corrugated texture, dented/crushed shape — NOT a perfect cube). Flat grey clothes (grey t-shirt, grey jeans — no warm tones). Hands at sides, no weapon. Visual contrast critical: player (vivid, warm, decorated) vs. Skeptic (flat grey, cold, undecorated). Background: partially reclaimed cul-de-sac street — warm amber tones `#F5A623` bleeding into grey `#C8BFB0` (translucent overlay). Saloon facades partially visible. Lighting: warm directional on player (soft shadows), flat diffuse grey on Skeptic (no shadows). Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F` (only on player side). Visual ref: Tearaway. Camera: medium two-shot, static (no movement). **PixVerse note:** Minimal motion — player raises weapon slowly, Skeptic completely still (statue-like). Visual contrast between warm/vivid player and cold/grey Skeptic is THE critical beat. Keep lighting divided. Mood: tension, standoff, wrongness."  
**Dialogue:** None (sound: static hum, silence).

---

### Shot 3 — The Skeptic Speaks
**Duration:** 2.5 seconds  
**Camera:** Close-up on The Skeptic's grey box, then pull back to medium shot  
**What happens:** Close-up on The Skeptic's crushed grey box. No eye cutouts visible (or if visible, just dark voids). Camera holds for one beat. Then The Skeptic speaks — one flat line, no emotion. His voice is quiet, not cruel. Just… empty. Camera pulls back to medium shot as he speaks.  
**Visual prompt for PixVerse/Gemini:**  
"A close-up of a crushed, undecorated grey cardboard box worn by a child (The Skeptic's box). The box: raw grey cardboard `#787878`, corrugated texture visible, crushed/dented (NOT a perfect cube — irregular shape). NO marker lines, NO decoration, NO color — pure flat grey. Eye cutouts: dark voids (black or very dark grey) — no glow, no light. Camera holds (2 seconds), then pulls back to medium shot of an 8-year-old boy wearing the grey box, standing in a Wild West street. Boy: flat grey clothes (grey t-shirt, grey jeans), no warm tones anywhere, chunky proportions. Background: partially reclaimed cul-de-sac — warm amber tones `#F5A623` fading slightly toward grey `#C8BFB0` (desaturation effect spreading from Skeptic outward). Saloon facades dimming. Lighting: flat diffuse grey (no directional light on Skeptic). Art style: stylized 3D, corrugated cardboard texture, NO marker details on Skeptic. Visual contrast: grey lifeless box vs. warm decorated environment. Visual ref: Tearaway, Hollow Knight Shade. Camera: close-up on box → smooth pullback to medium shot (2 seconds). **PixVerse note:** Skeptic completely still (no motion except breathing if any). Pullback motion only. Grey box texture and lack of decoration is critical visual beat — shows emptiness, absence of imagination. Mood: flatness, emptiness, disbelief made flesh."  
**Dialogue (The Skeptic's voice — flat, quiet, no emotion):**  
THE SKEPTIC: "None of this is real."

---

### Shot 4 — The Skeptic Fades; World Desaturates
**Duration:** 3 seconds  
**Camera:** Wide, static — The Skeptic turns and walks away; the world desaturates as he fades  
**What happens:** The Skeptic turns slowly and walks back toward the far end of the street. As he walks, he fades into grey static — his silhouette dissolving into the air. The moment he fades completely, the street's colors drain slightly — the warm amber sky mutes toward grey, the saloon facades flicker and dim. Kid lowers his weapon, watching. The desaturation holds for two beats, then slowly the colors begin to return. Kid shakes his head once, then steps forward.  
**Visual prompt for PixVerse/Gemini:**  
"A wide static shot of a boy (The Skeptic) with crushed grey cardboard box (flat grey `#787878`, no decoration, dented shape) turning slowly (1-second turn) and walking away down a Wild West street (slow deliberate walk, 1 fps). As he walks, his silhouette dissolves into grey static (particle fade-out effect — grey static noise `#787878` replacing solid form, 2-second fade). The moment he fades completely (disappears into grey static), the street's colors drain: warm amber sky `#F5A623` mutes toward pale grey `#D4CFC9` (desaturation effect, 1 second), saloon facades dim (opacity drops 30%), marker-drawn details fade (become less vivid). Foreground: an 8-year-old boy (player) with colorful cardboard box (cowboy: leather brown `#8B5E2A`, gold marker brim `#F7C244`) lowers cardboard weapon, watching (head tracks Skeptic). Lighting shifts from warm directional to flatter diffuse. Desaturation holds for two seconds, then colors begin to slowly return (re-saturation, 2-second fade back to warm amber). Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`, color-drain transformation effect (desaturation wave). Visual ref: Tearaway, Hollow Knight curse effect. Camera: wide, static (no movement). **PixVerse note:** Complex fade and color-drain sequence — may benefit from slow-motion (0.75x speed). Skeptic's dissolve should be gradual (particle effect, not instant pop). Color drain should spread outward from where he disappeared. Player motion minimal (weapon lower, head turn). Mood: unsettled, doubt seeded, but not victorious."  
**Dialogue:** None (sound: static fades, wind returns, Kid's breath).

---

## Cutscene 6: Command Node Revealed

**Narrative Beat:**  
Kid enters the final combat room of the Cul-de-Sac (Room 5: Town Square). At the center of the square is a cracked stone birdbath — but it's glowing faintly with grey static light. The Command Node. The source of The Unimaginative's control over this zone. Kid stops, looks at it. A SprinklerSentinel guards it. Kid nods. One more fight before the boss.

**Tone:** Recognition. This is the objective. Brief, punchy — get the player ready for the Room 5 fight.

---

### Shot 1 — Kid Enters the Town Square
**Duration:** 2 seconds  
**Camera:** Wide, low angle — Kid walks through a saloon-facade doorway into an open square  
**What happens:** Kid walks through a doorway (saloon facade) into the Town Square — the center of the Cul-de-Sac. The square is circular, open sky above. At the center: a cracked stone birdbath on a small island. The birdbath glows faintly with grey static light — unnatural, wrong. Kid stops at the threshold.  
**Visual prompt for PixVerse/Gemini:**  
"A wide low-angle shot of an 8-year-old boy with cardboard box (cowboy: leather brown `#8B5E2A`, gold marker brim `#F7C244`, chunky proportions) walking through a saloon facade doorway into an open circular town square. Saloon doorway: sun-bleached wood `#C4A46A`, marker-drawn signage `#3D2B1F`. At center of square: cracked stone birdbath (grey stone `#A89F94`, dry basin) on small circular island, glowing faintly with grey static light (cold blue-grey, unnatural pulsing glow — NOT warm). Background: partially reclaimed Wild West street — warm amber tones `#F5A623`, saloon facades, hitching posts (knotted rope brown), covered wagons. Sky: warm amber `#F5A623` gradient to blue `#4A90D9`. Lighting: warm directional on street, cold flat diffuse grey light radiating from birdbath (visual contrast: warm street vs. cold grey center). Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`. Visual ref: Tearaway. Camera: wide, low angle (looking up slightly). **PixVerse note:** Slow walk through doorway. Birdbath glow should pulse slowly (1Hz, cold grey-blue — contrast to warm environment). Static camera. Mood: objective revealed, wrongness at the center."  
**Dialogue:** None (sound: static hum from the birdbath, low and constant).

---

### Shot 2 — The Command Node Pulses
**Duration:** 2.5 seconds  
**Camera:** Medium shot, slow push-in on the birdbath  
**What happens:** Camera pushes in on the birdbath. It's cracked, weathered stone. The basin is dry except for a faint pool of grey static light that pulses slowly — like a heartbeat. The light flickers and spreads faint grey tendrils across the ground. A SprinklerSentinel (gold badge-eye variant) stands beside it, rotating slowly, scanning the square.  
**Visual prompt for PixVerse/Gemini:**  
"A medium shot of a cracked stone birdbath in the center of a town square. Birdbath: cracked grey stone `#A89F94` with cardboard texture, weathered. Basin is dry except for a faint pool of glowing grey static light (cold blue-grey, pulsing slowly like a heartbeat at ~1Hz — NOT warm). The light flickers and spreads faint grey tendrils (wispy particle tendrils, grey `#787878`) across the ground (creeping outward 1–2 feet). Beside the birdbath: SprinklerSentinel guardian (brass body `#C0A050`, rotating sprinkler head, glowing gold badge-eye `#F7C244` — cardboard and craft materials, mechanical but craft-like). Background: open circular square, warm amber sky `#F5A623`, saloon facades visible. Lighting: cold flat diffuse grey radiating from birdbath (grey light pool), warm directional on surroundings (visual contrast: cold grey center vs. warm environment). Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`, craft materials (brass is cardboard with metallic sheen). Visual ref: Tearaway, Little Big Planet craft props. Camera: medium shot, slow push-in (1–2 seconds). **PixVerse note:** Slow pulsing glow only (1Hz heartbeat). Tendrils should creep slowly (not fast flicker). SprinklerSentinel rotates slowly (1 RPM). Keep motion subtle. Cold grey light vs. warm environment is critical visual contrast. Mood: wrongness, the source of the grey, objective target."  
**Dialogue:** None (sound: static pulse, mechanical rotation hum).

---

### Shot 3 — Kid Draws Weapon
**Duration:** 1.5 seconds  
**Camera:** Close-up, side angle — Kid's hand draws his weapon (lasso, six-shooter, or quickdraw blade)  
**What happens:** Close-up on Kid's hand as he draws his weapon smoothly — lasso uncoiling, six-shooter sliding from holster, or quickdraw blade drawn from sheath. Camera holds on the weapon for one beat. Then Kid's face (box eye cutout visible) — determined.  
**Visual prompt for PixVerse/Gemini:**  
"A close-up side-angle shot of an 8-year-old boy's hand drawing a weapon — coiled rope lasso (brown rope `#8B5E2A`, marker-drawn coil texture `#3D2B1F`) OR cardboard six-shooter pistol (craft-tan `#E8C97A`, gold marker details `#F7C244`) OR quickdraw blade (cardboard with metallic foil overlay, marker edge lines). The weapon: corrugated cardboard and craft materials, hand-drawn marker details `#3D2B1F`. Hand: simple chunky geometry, cardboard-like skin tone. Camera holds on weapon (1 second), then cuts to boy's face — cardboard cowboy box (leather brown `#8B5E2A`, gold marker brim `#F7C244`, eye cutout visible — dark void or faint glow). Expression visible through cutout: determined (eyebrows angled down). Background: blurred town square (depth-of-field blur, warm amber tones `#F5A623`). Lighting: warm directional with soft shadows. Art style: stylized 3D, corrugated cardboard and kraft paper textures, hand-drawn marker outlines `#3D2B1F`. Visual ref: Tearaway. Camera: close-up side angle (weapon draw), then cut to close-up frontal (face). **PixVerse note:** Slow weapon draw (1–2 seconds). Avoid fast hand motion (blur). Cut between shots preferred over camera move (cleaner). Mood: readiness, one more fight."  
**Dialogue (optional, Kid's voice — short, certain):**  
KID: "One more."

---

## Cutscene 7: World Tree Glimpse

**Narrative Beat:**  
After defeating the Cardboard Mill boss (late Phase 2), Kid climbs to a rooftop or high vantage point. For the first time, he sees the World Tree in the distance. It's enormous — but dying. The trunk is grey, the branches bare. Faint grey static crackles around its base. The Unimaginative's mills surround it. Kid stares. The stakes become real. He's been fighting for this all along. No dialogue — the visual speaks.

**Tone:** Awe → dread → resolve. The first time the player sees the end goal. Make it breathtaking and heartbreaking at once.

---

### Shot 1 — Kid Climbs to the Vantage Point
**Duration:** 2.5 seconds  
**Camera:** Medium shot, following Kid as he climbs onto a rooftop  
**What happens:** Kid climbs up a ladder or stack of crates onto a factory rooftop. Camera follows from below. He reaches the top, stands, turns toward the horizon. Wind blows his shirt. The cardboard box on his head tilts slightly in the breeze.  
**Visual prompt for PixVerse/Gemini:**  
"A medium shot of an 8-year-old boy with a cardboard box on his head climbing onto a factory rooftop. He reaches the top, stands, and turns toward the horizon. Wind blows his shirt and tilts the cardboard box slightly. Background: industrial ruins (cardboard mill, grey concrete, rusted metal). Sky: muted grey-blue. Art style: stylized 3D, cardboard-and-marker aesthetic. Camera: medium shot, following. Mood: transition, reaching a high place."  
**Dialogue:** None (sound: wind, distant static hum).

---

### Shot 2 — The World Tree Revealed
**Duration:** 5 seconds  
**Camera:** Wide, slow pan from Kid → the distant World Tree on the horizon  
**What happens:** Camera starts on Kid (medium shot, profile), then slowly pans across the landscape toward the horizon. In the distance: the World Tree. Enormous — its trunk is thicker than a building, its branches reach toward the sky like grasping hands. But the tree is dying. The trunk is grey, cracked, leaking faint static light. The branches are bare — no leaves, no blossoms. Around its base: The Unimaginative's mills — grey factories, smokestacks, chain-link fences. The sky above the tree is darker, stormier. The camera holds on the tree.  
**Visual prompt for PixVerse/Gemini:**  
"A wide slow panning shot from a boy on a rooftop to a distant enormous tree on the horizon. The tree is the World Tree — trunk thicker than a building, branches reaching skyward like grasping hands. The tree is dying: grey cracked trunk, bare branches with no leaves, faint grey static light leaking from cracks. Around the base: grey industrial factories, smokestacks, chain-link fences. Sky above the tree: dark grey storm clouds. Foreground: industrial ruins (cardboard mill, muted colors). Art style: stylized 3D, cardboard-and-marker aesthetic, epic scale. Camera: wide, slow pan. Mood: awe, dread, the stakes made real."  
**Dialogue:** None (sound: wind, distant static crackle from the tree, low ominous hum).

---

### Shot 3 — Kid's Reaction
**Duration:** 2.5 seconds  
**Camera:** Close-up on Kid's box (eye cutout visible), then slow zoom out to medium shot  
**What happens:** Close-up on Kid's cardboard box — the eye cutout visible. Camera holds as Kid stares at the tree. His posture: still, absorbing what he's seeing. Then the camera slowly zooms out to medium shot — Kid standing on the rooftop, small against the vast horizon, the dying World Tree in the distance. He clenches one fist at his side.  
**Visual prompt for PixVerse/Gemini:**  
"A close-up of a cardboard box (ninja or cowboy design) with eye cutout visible, staring toward the horizon. Camera holds, then slowly zooms out to reveal an 8-year-old boy standing alone on a factory rooftop. Background: distant dying World Tree (enormous, grey, bare branches, surrounded by grey factories). Boy clenches one fist at his side. Art style: stylized 3D, cardboard-and-marker aesthetic. Camera: close-up → medium zoom-out. Mood: resolve forming, the journey's purpose revealed."  
**Dialogue (optional, Kid's voice — soft, determined):**  
KID: "I'm coming."

---

## Cutscene 8: The Friend Appears

**Status:** Reserved for mid-game story beat. Locked until Phase 3 design.

**Placeholder narrative beat:**  
Kid is in a difficult fight. About to be overwhelmed. A second figure appears — another kid, another box. The Friend. He joins the fight without a word. After the fight, they stand side by side. Kid nods. The Friend nods back. No dialogue. The journey continues — but now, together.

---

## Cutscene 9: Ending — The World Tree Blooms

**Narrative Beat:**  
Final boss defeated. The Unimaginative's hold is broken. Kid stands at the base of the World Tree. The grey static cracks and shatters. Color floods into the tree — the trunk shifts from grey to warm brown, the branches grow vivid green leaves, pink and gold blossoms bloom in seconds. The tree glows. The sky clears. Kid reaches up, takes off the box, holds it in his hands. He looks at it, then at the blooming tree. He smiles — a real, full, earned smile. The box was always a lens. What he saw was always real.

**Tone:** Catharsis. The journey is complete. Let the bloom take its time. No rush. Earn the smile.

---

### Shot 1 — Final Boss Falls
**Duration:** 3 seconds  
**Camera:** Wide, static — the final boss collapses at the base of the World Tree  
**What happens:** The final boss (design TBD — Phase 3) collapses to the ground at the base of the World Tree. The grey static around the tree flickers and dims. Kid stands in the foreground, weapon lowered, breathing hard. The tree looms behind — still grey, still dying. Silence.  
**Visual prompt for PixVerse/Gemini:**  
"A wide static shot of a large defeated enemy collapsing at the base of an enormous dying tree. The tree is the World Tree — grey cracked trunk, bare branches, faint static light flickering and dimming. Foreground: an 8-year-old boy with a cardboard box on his head, weapon lowered, breathing hard. Background: grey factories, chain-link fences. Art style: stylized 3D, cardboard-and-marker aesthetic, epic scale. Camera: wide, static. Mood: victory, but not yet triumph — the tree is still dying."  
**Dialogue:** None (sound: enemy collapse, static dimming, Kid's breath).

---

### Shot 2 — The Grey Cracks
**Duration:** 3 seconds  
**Camera:** Medium shot, slow push-in on the World Tree trunk  
**What happens:** Camera pushes in on the base of the World Tree's trunk. The grey cracked bark glows faintly. Then cracks of warm golden light appear — spreading like lightning through the bark. The static light flickers, then shatters like glass. The cracks widen. Warm light pours out.  
**Visual prompt for PixVerse/Gemini:**  
"A medium shot of the base of an enormous tree trunk. The bark is grey, cracked, leaking faint grey static light. Suddenly, cracks of warm golden light appear and spread through the bark like lightning. The static light flickers and shatters like glass. The cracks widen, warm light pouring out. Art style: stylized 3D, cardboard-and-marker aesthetic, transformation effect. Camera: medium shot, slow push-in. Mood: breaking, the grey shattering, hope emerging."  
**Dialogue:** None (sound: glass-crack sound effect, static shattering, rising musical swell).

---

### Shot 3 — The Tree Blooms
**Duration:** 6 seconds  
**Camera:** Wide, low angle looking up the trunk as the bloom spreads upward  
**What happens:** The grey bark shifts to warm rich brown. Green leaves burst from the branches — hundreds, then thousands. Pink and gold blossoms bloom in waves spreading upward from the base to the crown. The tree glows with warm light. The sky above clears from storm-grey to vivid blue. Cherry blossom petals and golden leaves drift down like snow. The factories around the tree's base begin to shift — grey concrete becomes warm wood, chain-link fences become garden trellises. The camera follows the bloom upward to the crown of the tree — fully alive, fully vivid.  
**Visual prompt for PixVerse/Gemini:**  
"A wide low-angle shot looking up the trunk of an enormous tree as it transforms from dying grey to fully alive. Grey cracked bark shifts to warm rich brown. Green leaves burst from branches in waves. Pink and gold blossoms bloom in seconds, spreading upward from base to crown. The tree glows with warm light. Sky clears from storm-grey to vivid blue. Cherry blossom petals and golden leaves drift down like snow. Background: grey factories transforming into warm wooden structures, chain-link fences becoming garden trellises. Art style: stylized 3D, cardboard-and-marker aesthetic, bloom transformation, vivid saturated colors. Camera: wide, low angle, following the bloom upward. Mood: catharsis, life restored, imagination victorious."  
**Dialogue:** None (sound: rising musical crescendo, rustling leaves, wind chimes, distant laughter).

---

### Shot 4 — Kid Takes Off the Box
**Duration:** 4 seconds  
**Camera:** Medium close-up, frontal — Kid reaches up and takes off the box  
**What happens:** Kid stands at the base of the now-blooming World Tree. He reaches up with both hands, grasps the cardboard box, and lifts it off his head. Camera holds as he lowers the box to chest level, holding it in both hands. His face is visible for the first time — a boy, ~10 years old, scruffy hair, real. He looks at the box in his hands — the marker-drawn details (ninja or cowboy design) are vivid, hand-made. Then he looks up at the blooming tree.  
**Visual prompt for PixVerse/Gemini:**  
"A medium close-up frontal shot of a 10-year-old boy lifting a cardboard box off his head and holding it at chest level in both hands. His face is visible for the first time — scruffy hair, real, earnest expression. The box is vivid with hand-drawn marker details (ninja or cowboy design). Background: the base of an enormous blooming tree — vivid green leaves, pink and gold blossoms, warm light. Cherry blossom petals drift through the air. Art style: stylized 3D, cardboard-and-marker aesthetic. Camera: medium close-up, frontal. Mood: quiet wonder, the journey complete."  
**Dialogue:** None (sound: soft rustling, petals falling, wind).

---

### Shot 5 — Kid Smiles
**Duration:** 3 seconds  
**Camera:** Close-up on Kid's face, then pull back to wide shot of the blooming tree with Kid in foreground  
**What happens:** Close-up on Kid's face. He looks at the box, then up at the tree. A slow smile spreads across his face — real, full, earned. Not triumphant — grateful. Joyful. The camera holds for one beat, then pulls back to a wide shot: Kid standing at the base of the enormous blooming World Tree, box held in his hands, surrounded by falling petals and warm light. The world is vivid, alive, reclaimed.  
**Visual prompt for PixVerse/Gemini:**  
"A close-up of a 10-year-old boy's face as a slow smile spreads across his face — real, full, earned, joyful. Then the camera pulls back to a wide shot: the boy stands at the base of an enormous blooming tree (World Tree), holding a cardboard box in his hands. The tree is fully alive — vivid green leaves, pink and gold blossoms, warm light glowing from within. Cherry blossom petals and golden leaves fall like snow. Background: transformed landscape — warm wooden structures, garden trellises, vivid colors. Sky: warm afternoon blue. Art style: stylized 3D, cardboard-and-marker aesthetic, vivid saturated colors. Camera: close-up → wide pullback. Mood: catharsis, joy, imagination victorious, the journey complete."  
**Dialogue (optional, Kid's voice — soft, certain, final line of the game):**  
KID: "It was real. It always was."

---

## Production Notes

### General PixVerse/Gemini Prompt Structure
For every shot, include:
1. **Shot type + camera motion** (wide, medium, close-up; static, push-in, pan, rotate)
2. **Subject description** (character age, clothing, box design, posture, action)
3. **Setting description** (location, background, lighting, color palette state)
4. **Art style anchor** ("stylized 3D, cardboard-and-marker aesthetic, hand-drawn outlines, craft textures, chunky proportions")
5. **Mood/tone** (one-word or short phrase — guides the AI's emotional framing)

### Color Palette References for Prompts

**Drained State (Exact Hex Codes):**
- Base: warm grey `#C8BFB0`
- Shadows/depth: darker grey-brown `#A89F94`
- Dark accents: dark grey `#6B6560`
- Sky/ambient: pale grey `#D4CFC9`
- Lighting: flat diffuse, no highlights, fog-like ambient
- Texture: corrugated cardboard grain visible

**Reclaimed State (Exact Hex Codes):**
- Cardboard base: craft tan `#E8C97A`
- Sky: marker blue `#4A90D9`
- Grass/nature: marker green `#5CB85C`
- Danger/energy: marker red `#E05A4E`
- Rewards/accents: marker orange `#F5A623`
- Ninja box: dark navy `#1A1A2E` with purple accents `#9B59B6`
- Cowboy box: leather brown `#8B5E2A` with gold accents `#F7C244`
- Outlines: dark brown marker `#3D2B1F`
- Lighting: warm directional with soft shadows, vivid saturation

**Cul-de-Sac Reclaimed (Wild West):**
- Sky: warm amber `#F5A623` fading to marker blue `#4A90D9`
- Dirt road: warm sandy brown `#C4A46A`
- Weathered wood: sun-bleached `#C4A46A` with darker grain `#8B6914`
- Faded canvas: wagon tops `#E8D6A0`
- Lighting: golden hour, warm directional

**The Skeptic (Always Grey):**
- Box and clothes: flat grey `#787878`
- NO warm tones, NO marker decoration, NO color
- Eye cutouts: dark voids (black or very dark grey)
- Visual contrast with player is critical

### Mobile Constraints & AI Video Tool Best Practices

**Duration & Format:**
- Total per cutscene: 10–20 seconds (mobile attention span)
- Per shot: 3–6 seconds (sweet spot for AI video generation)
- Aspect ratio: 16:9 landscape (always)
- Target resolution: 1080p minimum for mobile display

**PixVerse/Gemini Technical Constraints:**
- **Camera motion:** Prefer static shots or slow push-in/pullback (max 1–2 fps camera movement). Fast camera moves (pans, rotations >1 RPM) generate warping artifacts.
- **Character motion:** Slow deliberate actions only (1 fps walk speed, slow hand movements). Fast actions cause motion blur and deformation.
- **Transformations:** Gradual color bleeds and fades work best. Instant pops or fast shimmering cause flicker artifacts.
- **Complex scenes:** Split complex transformations into multiple shorter clips rather than one long clip with many simultaneous changes.
- **Particle effects:** Simple, slow-moving particles only. Avoid complex particle systems (render poorly at AI video resolution).
- **Depth of field:** Use background blur to hide low-detail areas. AI video struggles with fine detail at distance.
- **Lighting changes:** Gradual shifts (2+ seconds) preferred over instant lighting flips.

**Visual Storytelling for Mobile:**
- Dialogue: Optional and minimal (players may have sound off)
- Silhouettes: Keep character silhouettes clear and bold (readable at small screen size)
- Color contrast: Use vivid color shifts to carry emotional beats (grey → vivid reclaim)
- Transitions: Fast cuts preferred over slow fades (mobile players want to return to gameplay quickly)

**URP Mobile Lighting Specs (for reference/consistency):**
- Drained: flat diffuse, no shadows, fog-like ambient occlusion
- Reclaimed: single warm directional light (sun), soft shadows (512px shadow map), vivid color saturation via post-process volume

### Material & Texture Descriptions (Use in Every Prompt)

**Cardboard/Box Materials:**
- "Corrugated cardboard texture with visible corrugation lines running horizontally"
- "Kraft paper surface, slightly rough, not smooth"
- "Box edges slightly worn, not pristine"
- For The Skeptic: "Crushed/dented cardboard, NOT a perfect cube, irregular shape"

**Marker Details:**
- "Hand-drawn marker strokes, slightly uneven (not computer-perfect)"
- "Dark brown marker ink `#3D2B1F` for outlines and details"
- "Purple marker lines `#9B59B6` for ninja design (mask outline, eye shapes)"
- "Gold marker lines `#F7C244` for cowboy design (hat brim on sides of box)"

**Character Proportions:**
- "Chunky proportions: head (box) ~35% of total height, stocky build (width ~60% of height)"
- "The box is a perfect cube (unless The Skeptic's crushed box), oversized relative to body"
- "Limbs simple geometry, not detailed muscle definition"

**Environment Materials:**
- Wood: "Sun-bleached wood grain with hand-drawn marker grain lines `#3D2B1F`"
- Stone: "Grey stone with cardboard texture, not realistic rock"
- Metal (drums, sprinklers): "Metallic grey with corrugated cardboard texture overlay — craft-material metal, not photorealistic"
- Fabric (wagon canvas): "Faded canvas with kraft paper texture"

**Lighting Quality:**
- Drained: "Flat diffuse lighting, no highlights, fog-like ambient, grey color cast"
- Reclaimed: "Warm directional light (sun angle 45° above horizon), soft shadows, vivid color saturation, warm golden cast"
- URP mobile: "Single directional light + ambient, no complex multi-light setups"

### Next Steps
1. ~~**Art-direction-agent pass:** Refine visual prompts with exact hex color codes, URP lighting specs, and PixVerse/Gemini-specific technical art direction from `art-style-guide.md`~~ **COMPLETE**
2. **PixVerse/Gemini generation:** Use refined prompts to generate video clips for P0 cutscenes (1–3) first
3. **Storyboard-artist pass (optional):** Expand any cutscene into frame-by-frame panel breakdowns for animatic/storyboard review
4. **Audio cues:** Define SFX and music cues per shot (not included here — sound design is a separate pass)

---

_Cutscene Production Document v1.0 — 2026-07-21_  
_9 cutscenes defined. P0 (Phase 1 prototype: cutscenes 1–3), P1 (Phase 2 Cul-de-Sac: cutscenes 4–6), P2–P3 (future content: cutscenes 7–9)._  
_All prompts are starting drafts — refine with art-direction-agent for final PixVerse/Gemini generation._
