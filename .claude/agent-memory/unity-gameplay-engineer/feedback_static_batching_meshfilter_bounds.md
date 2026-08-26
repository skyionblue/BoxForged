---
name: feedback-static-batching-meshfilter-bounds
description: MeshFilter.sharedMesh.bounds is unreliable at runtime for a GameObject marked Batching Static — Unity replaces its mesh reference with a combined-batch mesh. Read MeshCollider.sharedMesh.bounds instead. Found fixing pfb_MinimapCamera's edge-clamp in CulDeSac_WildWestCity (B110).
metadata:
  type: feedback
---

Found 2026-08-26 building the minimap follow-camera clamp in `CulDeSac_WildWestCity` (`docs/BACKLOG.md` B110, see [[project_wildwestcity_build]]).

**The bug:** a script cached `Ground.GetComponent<MeshFilter>().sharedMesh.bounds` once in `Start()`, expecting the flat 5×5 default-Plane bounds (`Center (0,0,0)`, `Extents (5,0,5)`) the Editor shows for that object. At runtime the read came back as `Center (-21, 2.49, -21)`, `Extents (35.18, 2.49, 35.18)` — a completely different, non-flat shape with real height, roughly 7× larger. `Ground` is marked fully static (`m_StaticEditorFlags: 4294967295`, includes Batching Static). Direct diagnosis confirmed it: `meshFilter.sharedMesh.name` at runtime read `"Combined Mesh (root: scene) 3"`, not `"Plane"` — Unity's static-batching pass, which runs at the start of Play Mode (not just in a real build), rewrites a batched renderer's `MeshFilter.sharedMesh` to point at one shared combined mesh covering the *entire static batch* it was folded into, with bounds spanning every object in that batch, not just the one GameObject being queried. This is silent — no error, no warning — and only shows up at runtime; the same read in Edit Mode still returns the real, un-batched mesh, so a naive Edit-Mode sanity check would not have caught it.

**The fix:** read `GetComponent<MeshCollider>().sharedMesh.bounds` instead. Static batching only touches renderers/`MeshFilter`s for draw-call merging — it never touches colliders. `MeshCollider.sharedMesh` reliably stays the real, unmodified source mesh asset in both Edit and Play mode. (This requires the object to actually have a `MeshCollider` referencing the same mesh, which is common for ground/level geometry but not universal — check first.)

**How to apply:**
- Never trust `MeshFilter.sharedMesh` (or anything derived from it — `.bounds`, vertex/triangle counts, etc.) for a *static* GameObject's true local geometry at runtime. If you need the real per-object mesh data at runtime, prefer `MeshCollider.sharedMesh` (if present), or capture the value once in Edit Mode / at build time and serialize it, rather than reading it live.
- This is easy to miss because it's silent and staging-dependent: it will pass a quick Edit-Mode check, and even in Play Mode a screenshot alone can look plausible until you check whether it's void-safe at the level's true boundary. What actually caught it here was comparing the *numeric* cached bounds against the known-correct authored values (`Extents (5,0,5)`) via a live reflection read on the running component — not eyeballing a screenshot.
- Any other script in this project reading `MeshFilter.sharedMesh` off a `Batching Static` object at runtime should be treated as suspect until checked the same way — this is a general Unity behavior, not scoped to `Ground` or to this scene.
