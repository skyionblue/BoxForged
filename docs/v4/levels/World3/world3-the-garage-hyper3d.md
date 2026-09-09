# World 3 — The Garage — Hyper3D.ai Pilot Prompt

**Status:** Experimental. Part of the Hyper3D.ai whole-world-generation pipeline pilot for World 3 (see `docs/ROADMAP.md` and `docs/CREATIVE_STATE.md`). Not yet evaluated against `LevelBuilder`/`RoomDataSO` architecture.

Source material: `docs/story/zones/the-garage.md` (zone lore) and `docs/art/style-guide.md` (cardboard-and-marker art direction).

## Prompt (Imagination-state / playable forge-dungeon)

A deep dungeon forge carved into living dark stone, built entirely from a child's craft materials — cardboard, marker ink, foil, and tape rendered as real architecture. Style: stylized low-poly 3D, chunky and hand-crafted, NOT photorealistic — corrugated-cardboard-textured stone walls with visible grain, marker-stroke outlines on every edge, warm ember-orange and cinder-red lighting from a central black-iron forge glowing at the room's heart. Wide establishing view down a long stone corridor that opens into a cavernous workshop bay: to the left, a battered pegboard eight feet wide covering the far wall, densely covered in glowing marker-colored tool outlines (wrench, hammer, tongs) traced in vivid crayon color against dark stone; one real antique hand-saw hangs in its slot, handle forward. Center-frame, a massive stone forge-altar workbench with tool-rack silhouettes glowing above it. In the mid-ground, a hulking humanoid construct assembled from filing-cabinet plating and sheaves of grey paper, standing at rigid attention like a knight built of bureaucracy. Near it, a rusted welding cart with an oxygen tank and torch arm, faintly glowing blue at its joints like a small fire elemental. Ceiling drops low and close, ten feet then lower, lit by torches where bare bulbs used to be. Deep shadows in warm dark brown, not black. Overall palette: black iron, ember orange, cinder red, warm amber torchlight, with scattered vivid marker colors (red, orange, purple, gold) on the glowing pegboard outlines as the only saturated color pops against the dark stone. Mobile-game low-poly asset style, clean readable silhouettes, no photorealistic textures, no smooth plastic surfaces.

## Notes

- Deliberately depicts the **Imagination/forge state**, not the grey "Drained" reality-garage state — this is the version meant to become playable geometry. A companion prompt for the drained reality-garage reference has not been written yet; write one if the two-layer transition needs its own asset pass.
- Deliberately excludes rigged characters (Pegboard Warden boss, the Cowgirl) — those are character rigs, not environment geometry, and mixing them into a whole-world generation pass risks them being baked into static scenery.
- One dense wide shot by design — a whole-world generator needs multiple distinct, readable props/materials in-frame to have something to extract. Revisit this approach once real Hyper3D output has been inspected; it may want single-subject reference shots instead.
- Not yet evaluated for how (or whether) Hyper3D's output format maps onto `LevelBuilder`/`RoomDataSO`. That assessment belongs to `technical-director` before committing to a full Garage build through this pipeline.
