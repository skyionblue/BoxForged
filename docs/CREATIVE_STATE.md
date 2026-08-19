# Creative State

This file records the current decision state during concept and narrative discovery. Do not treat WORKING or OPEN material as accepted design.

## CANON
Owner-approved creative facts and constraints that should not be casually changed.

### Protagonist
- Kid is a **girl** (she/her/hers). This is not ambiguous. All story content, assets, and UI must use she/her.
- Kid's parents never gave her a phone — they just redirected her: outside, kitchen table, here's some tape.
- Kid's interior motivation is Mr. Wen — her neighbor who went grey after The Hush. She never says his name in-game. He is not a quest marker. He is the reason everything matters.
- Mr. Wen was a **martial arts instructor** — had a small dojo nearby, taught kids after school. On Sunday mornings he sat in the roots of the World Tree. He knew where it was before Kid did. He knew what it was for. The Lens cannot reach him — he is not somewhere imagination can simply fix.
- The **Unimaginative escalate through management failures, not ideology** — they notice Kid's reclamation when grey zones go vivid, when their systems report anomalies. They are bureaucrats responding to broken metrics, not crusaders opposing imagination.
- The box is a lens, not a creator. It reveals imagination that was always there. Kid does not pretend. She sees correctly.

### Kid's Arc — CANON
**Certainty → Courage.**
- She begins: forward momentum, goes because she *knows*. The certainty of a kid who never doubted.
- Journey: the Skeptic's words land heavier as the zones get harder. Certainty thins. She actively recruits other kids — shows them something through the box. They see it or they don't.
- Resolution: at the World Tree, she finds Mr. Wen's tomatoes in the roots. She knows. That is her private moment. She does not need to see him restored. She's done what she came to do.
- What happens to Mr. Wen after belongs to the companion anime — it is not the game's story to tell.

### Kid's Fear — CANON
She is afraid she will **lose the sight** — not the fight. That one day she'll look through the box and it will be just cardboard. That the Skeptic will be right: *"It was always just a backyard."* This is why she recruits: every kid who sees is proof the sight is real.

### Kid's Mission — CANON
Kid actively recruits other kids. The invitation is the box — she shows them something through it. She does not make speeches. The more kids who see, the stronger the World Tree grows — visible in how the world looks: more vivid, more reclaimed, more alive.

**Hard Knocks Workshop:** Real-world kids completing HKW skills feed this same coalition. Future feature: real HKW kids named in the World Tree (Phase 3+, confirmed as planned).

### Transmedia
- A companion anime show is in development (owner's partner).
- **Intentionally different universes.** The game and anime are not the same continuity and must not be reconciled.
  - Game world: The Great Hush happened. Internet shut off. Imagination faded into silence.
  - Anime world: The Great Hush never happened. The Source (social media company) controls society. Imagination is drowned out by too much noise.
- Same themes from opposite directions. Both valid. Both intentional.
- Mr. Wen's recovery arc belongs to the anime. The game ends at the tomatoes.
- The Friend concept, adult relationship arcs, and the anime's cast (Janez, Benji, Sol, etc.) are anime territory — not the game's to carry.

### The World
- The inciting event is **The Great Hush**: the Internet was deliberately shut off by two infrastructure engineers.
- **The Elders** are exactly two people — lifelong friends who met in junior high, worked multiple IT jobs together. Not executives, not founders. The people who kept websites running. Their names never appear. They are not villains. They ran the math. They were not certain. The game does not resolve whether they were right.
- **Elder BoxHead** is The Two Elders. This is the late-game revelation.
- The Unimaginative are not cruel. They are suspended — people who came home from The Hush and waited, and nothing grew in the silence. They are the wound the cure accidentally made worse.
- The World Tree is three hundred years old — the last living tree, the last source of cardboard.

### Characters
- The **Cowgirl** is looking for her best friend — a specific person whose number she lost when The Hush took her phone.
- The **Female Ninja** is the Cowgirl's missing best friend. Their reunion is a story beat (Phase 2+).

### The Cowgirl / Female Ninja Reunion — Shape CANON
- The Female Ninja moves first — toward a place, not toward the Cowgirl. She needs help with something specific (what, exactly, is OPEN).
- The Cowgirl arrives at the same place at the same moment, still searching.
- Recognition is **simultaneous** — neither initiates it. Neither says "I was looking for you" first.
- The reunion happens **separately from Kid's story**. Kid is not present. When Kid meets them they are already together — an existing fact she walks into.
- Phase 2+ story beat. Zone TBD.
- **The Friend concept is not in the game.** The only friendship in the story belongs to The Elders — two people who knew each other since junior high and made a world-changing decision together. Kid does not have a named childhood best friend in the game. The Cowgirl joins Kid's cause because she notices what Kid is doing and wants to be part of it. This is covered in the companion anime, not the game.
- **The Skeptic** is a nameless metaphysical presence — what happens to the space where imagination used to be, given a shape. It is never given a wound or a backstory.
- **Skeptic Grunts** are a recurring enemy type — kids who never learned to see imaginatively, now working for the Unimaginative. They appear across all zones.
- **Gnome Soldiers** are a recurring enemy type across all zones (like Koopa Troopas) — not only World 2.

### Game Structure
- Roguelite run structure. Each run is a single session (~10–15 min), starting fresh.
- Boxes are **cosmetic only**. Fighting Styles are the gameplay layer.
- **Cutscenes: boss intros only.** No full cutscene system. Story is told environmentally. The only held cinematic moment is a brief boss intro before each zone boss — establishes who you're fighting. All other story beats (Mr. Wen's tomatoes, Imagination Restore, zone reclamation) are environmental, not cutscene-driven.
- Two default Fighting Styles: Ninja Style, Cowboy Style.
- Combat system: dodge-parry-jump, Sekiro-influenced (read and react, not stamina management).
- Camera: **fixed follow, lower angle, no rotation.** Behind Kid, follows her position, never rotates. Closer and lower than the original rig — world seen closer to kid-height. Think Hades pulled in closer. **Technically validated spec (ADR-0001, 2026-08-19):** pitch 36°, vertical FOV 45°, height 5.5m, `FollowOffset (0, 5.5, -7.57)` — supersedes the earlier conversational placeholder `(0, 4, -6)`, which failed a rear-visibility safety check. **Coupled decision:** this camera change requires a new attack-telegraph system (ADR-0003) because the game's only current attack-tell — whole-body color tint — degrades at the new angle. Shipping the camera without the telegraph work is not an acceptable combination; the fallback is holding the camera closer to its original pitch. Overrides the previous "fixed top-down" lock. Pre-production/production task — see `docs/TECHNICAL_DESIGN.md` and `docs/adr/`.
- Platform: iOS + Android, landscape only.
- Co-op architecture designed in from day one; Phase 1 is single-player.

### World 2 Enemies
- **Crane Duelist**: Pink lawn flamingo imagined as a one-legged spear duelist. Waits for range, patient and still until it isn't. Counter window when it wobbles after a full thrust. Room placement TBD. Full lore: `docs/story/enemies/crane-duelist.md`.
- **Gnome Soldiers** and **Skeptic Grunts** also appear (recurring types — see below).
- **Leaf Pile Lurkers**, **Sprinkler Sentinel**: World 2 zone-specific enemies.

### World Order
- **World 1 — The Cul-de-Sac (Western):** Boss is SpinCycle. Already built.
- **World 2 — The Backyard / Dojo:** Boss is **The Grasscutter** — a rusted push reel-mower imagined as a tengu blade-master. Two-phase fight: Kata (deliberate, honest tells, teaches you to read it) → Rev (the machine, cutting lanes, no more honor). Cherry blossoms on defeat. Full lore: `docs/story/enemies/grasscutter-boss.md`.

### Monetization
- Free-to-play: ads + optional IAP. No pay-to-win. All styles and boxes earnable through Spark.

---

## WORKING
Current preferred directions still open to refinement.

- **PermitPulper, NoticePusher, LaundryTumbler**: Candidate enemy thoughts. May be used if they make sense narratively/mechanically. Not canon.
- Hard Knocks Workshop real-world skill mapping (Compass Dash, Splitting Maul, Ember Charge, Craftsman Skin Tier) — conceptually locked for Phase 2/3. Implementation details pending.
- Two-pool IP / Spark system structure.
- Combo Imagination (specific box + style visual pairings).

---

## OPEN
Important unresolved creative questions.

- Grasscutter: **LOCKED** — World 2 boss (moved to CANON)
- Crane Duelist: **LOCKED** — World 2 enemy (moved to CANON)
- World 2 room structure: where does the Crane Duelist appear — Room 1, Room 2, or both?
- The Cowgirl/Female Ninja reunion: zone TBD. Trigger: **recruitment** — the Female Ninja has seen what Kid is doing and comes to join. She seeks out the Cowgirl to be part of the effort. The Cowgirl arrives at the same place still searching. Recognition is simultaneous. Neither one gets to say "I found you."
- The Friend: when does The Friend appear? Which zone? What is the story beat?
- Elder BoxHead / The Two Elders: what do they want from Kid when she reaches the World Tree? Are they allies, obstacles, or both?
- The Cardboard Mill boss: undefined.
- Versus mode rules: undefined.
- Spark conversion rate: playtesting dependent.
- IAP final pricing: estimates only.

---

## REJECTED
Directions intentionally ruled out. Record a short reason so future sessions do not revive them.

- **Kid as a boy**: Changed during narrative development. She is a girl. Do not revisit.
- **"The Friend" concept (Kid's named childhood best friend)**: Not in the game. Covered in the companion anime. The only friendship the game story carries belongs to The Elders.
- **SpinCycle as World 2 boss**: SpinCycle belongs to the Western/Cul-de-Sac level. World 2 needs its own boss (Grasscutter is the leading candidate).
- **The Elders as a large collective**: Exactly two people. Do not expand this to a committee or organization.
- **Stamina-based combat**: The `docs/design/combat-system-design.md` file is a legacy artifact describing a different combat system. It is superseded by the dodge-parry-jump design. Treat it as historical only.
- **Old `.claude-ORIG` agents and skills**: Retired. Do not invoke.
- **Universal Meshy orientation assumptions**: Rejected per asset pipeline overhaul.
- **Tumbleweed Roller as an enemy**: Removed from roster in GDD v0.8.
- **Deforestation as the world-breaking event**: Replaced by The Great Hush.

---

## Discovery lock status
- Concept discovery: **LOCKED** (2026-08-18) — core loop, visual style, camera direction, forge mechanic, cutscene scope, monetization, and world structure all settled.
- Narrative discovery: **LOCKED** (2026-08-18) — complete for Worlds 1 and 2 scope. Three Phase 2+ beats remain open (Elder BoxHead scene, Skeptic final moment, Cowgirl/Female Ninja reunion zone) but do not block production.
- **Pre-production authorized by owner: YES (2026-08-18)** — "Lock discovery and begin pre-production."
- **Scene rebuild directive:** Owner does not want to reuse the existing Unity scenes (`CulDeSac_Room1`, `CulDeSac_AmbushAlley`, `CulDeSac_SaloonFront`, `CulDeSac_MailboxRow`, `CulDeSac_BossArena`, `CharacterTest`, `ForgeLoop_Test`). All room layouts are to be rebuilt from scratch under the new camera and current creative canon. Existing scenes are not to be deleted without separate explicit approval — they remain on disk as reference/fallback until new scenes are verified, then a deletion decision is made explicitly.
