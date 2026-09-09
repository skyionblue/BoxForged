# Grasscutter Full Model Replacement — Plan

## Goal

Replace the current, already-shipped Grasscutter character model/rig with
`raw-assets/models/zips/Grasscutter_part-segmentation.fbx`, carrying over the
weight-painting quality already achieved on the current model (pivot fix,
torso/belt rigidity), so the new mesh's separated parts (notably the hands)
can later become swappable — motivating example: hands as rockets.

## What we know about the source file

- 8 separate mesh parts (`model_part0`–`model_part7`), generic names — no
  semantic labels. Identified by geometry:
  - `part0`/`part5` — hands (symmetric pair, smallest, isolated from arms)
  - `part1`/`part4` — arms (symmetric pair)
  - `part2`/`part3` — legs (symmetric pair, largest of the limb pairs)
  - `part6` — head (centered, small)
  - `part7` — torso + blade/reel assembly (**367,698 verts alone** — over
    half the whole file)
- **Zero armature, zero skin weights.** This is raw segmented geometry, not
  a rigged character.
- **~567,500 vertices total**, at a tiny raw export scale (needs
  normalizing to the ~4.25 m boss height, per B119/ADR-0008).
- Current shipped model: 32,546 vertices, one mesh, already rigged and
  weight-tuned today.

## The real scope, step by step

1. **Define the spec explicitly before touching geometry**: target real-world
   height (4.25 m, matching the shipped boss), target poly budget, bone
   naming (must match the current 36-bone skeleton — `Hips`, `Spine`...
   `Reel_Root`, `Reel_Body`, `Reel_Blade_Front/Right/Left`(+`_Tip`), arms,
   legs — exactly, so `GrasscutterAI._reelRoot`, `AC_Grasscutter.controller`,
   and `Anim_Grasscutter_Idle.anim` don't all need separate rewiring/retargeting).
2. **Decimation is the biggest open risk, not a formality.** No boss-specific
   budget is written down (`docs/TECHNICAL_DESIGN.md` §3.2 has ~20k tris for
   the *player* and ~10–12k for standard enemies; nothing bigger for
   bosses), but even a generous 20–25k tri target means **reducing this
   source by more than 95%**. This project's whole visual style is
   deliberate, crisp low-poly faceting (confirmed today — the ribbed reel
   drum's angular look is intentional art, not a defect). A >95% automatic
   decimation of a dense organic sculpt risks melting exactly that faceted
   look, not just "looking a bit rougher." This needs an `art-director`
   judgment call on whether the result still matches style, not just a
   mechanical decimate-and-check-the-triangle-count pass.
3. **Rig it** — new armature matching the existing bone set/hierarchy, or the
   Unity-side wiring (AI script fields, Animator, existing animation clips)
   all need rework too.
4. **Weight it** — either automatic (Blender bone heat) as a baseline plus a
   real manual pass, or surface-transfer from the current mesh's now-good
   weights. Either way, expect to **rediscover new versions of today's exact
   bugs** on the new topology (misaligned seams at part boundaries, a
   mispositioned rotation pivot) — today's diagnostic techniques (distance-
   from-bone, weight-discontinuity-across-edges) apply directly and should
   make this faster the second time, but it is not a copy-paste of today's
   fixes.
5. **Unity side is not a simple reimport.** We learned today that
   `pfb_enemy_grasscutter.prefab` has the model **fully unpacked** —
   independent of the FBX, with every bone baked in directly. A new
   skeleton means rebuilding this prefab's character hierarchy (re-parenting
   NavMeshAgent/colliders/`GrasscutterAI`'s bone references), not just
   dropping in a new FBX and reimporting.
6. **Re-verify everything from today, on the new model**: pivot centering,
   torso/belt rigidity, arm independence — using the same quantitative
   checks (distance-from-pivot under rotation, weight-discontinuity scan),
   not just visual screenshots (which already produced one false positive
   today).

## Recommended sequencing (de-risked)

Do all of steps 1–4 **in complete isolation** — a separate staging `.blend`,
never touching the committed/shipped `Grasscutter.fbx` or
`pfb_enemy_grasscutter.prefab` — until the new model is fully validated on
its own (correct scale, correct bone names, weights verified in Blender).
Only then do a single, clean swap into Unity, following the same
backup-before-overwrite discipline used today. This keeps the currently
shipped, working boss completely safe for the whole middle of this process.

## Recommendation

This is genuinely a multi-session task (asset budget/decimation judgment,
rigging, a full reweighting pass, and a real prefab rebuild in Unity) — not
a continuation of today's fixes. Worth treating as its own dedicated piece
of work, likely through `art-director` (style/decimation call) and
`asset-engineer` (pipeline execution), rather than more ad-hoc Blender MCP
work in an already very long session.

## Open decision for the owner

Given the scope above: proceed now as a dedicated task, or hold this for
later and consider the cheaper "swap just the hands as separate rocket
props on the current, already-fixed model" path instead?
