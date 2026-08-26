---
name: feedback-idle-fx-visual-child-only
description: Idle-motion scripts (bob/spin/etc.) on a world pickup must live on a visual-only child, never on the interaction root that also holds the trigger Collider/Rigidbody.
metadata:
  type: feedback
---

Never attach a per-frame idle-animation component (bob, spin, breathing scale, etc.) directly
to a pickup/interactable's root GameObject if that root also carries the trigger `Collider`
and/or `Rigidbody` used for pickup detection. Create a dedicated visual-only child (e.g.
`Visuals`) under the root, reparent the mesh renderers onto it, and attach the idle-motion
component to that child instead. The interaction root's Transform should stay completely
static.

**Why:** `Transform.localPosition`/`Transform.Rotate` calls run on whatever GameObject the
component sits on. If that's the same object as the `BoxCollider`/`Rigidbody`, the trigger
volume itself gets bobbed and rotated every frame — unnecessary kinematic-Rigidbody physics
cost (broadphase has to re-evaluate a "moving" collider every frame for no gameplay reason),
and a reliability risk for pickup detection (the trigger's world bounds are no longer stable).
Found and fixed live while adding `PickupIdleFX` to `pfb_pickup_cardboard.prefab` — the first
draft added the component to the prefab root (same object as `CardboardPickup`, `BoxCollider`,
kinematic `Rigidbody`); verified via `BoxCollider.bounds` sampled across many stepped frames
that the trigger center was silently drifting/rotating with the visual bob. Fix: added a
`Visuals` child, reparented the 5 mesh children onto it (identity local transform, so their
local positions/rotations numerically carry over unchanged), moved `PickupIdleFX` there. Re-
verified: `BoxCollider.bounds.center` stayed bit-for-bit identical across 40+ stepped frames
while the `Visuals` child's local Y and rotation visibly animated.

**How to apply:** Whenever adding "look at me" motion (bob/spin/pulse/etc.) to any prefab that
is also a physics interactable (pickup, hazard, lever, anything with a `Collider` used for
gameplay detection), check whether the target GameObject also carries that Collider/Rigidbody
before attaching the animation component. If so, split into `Visuals` (animated, no physics)
+ root (static, holds Collider/Rigidbody/interaction script). This generalizes beyond
cardboard pickups to any future world-pickup/interactable idle-FX work in BoxForged.
