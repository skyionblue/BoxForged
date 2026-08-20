---
name: feedback-dontdestroyonload-root
description: DontDestroyOnLoad singleton prefabs (AudioManager, AttackTelegraphService) must stay scene-root GameObjects, never nested under a tidy "[Managers]" group
metadata:
  type: feedback
---

When composing a new BoxForged scene, `pfb_AudioManager` and `pfb_AttackTelegraphService` (and any other prefab whose script calls `DontDestroyOnLoad(gameObject)` in `Awake()` — check with `grep -rl DontDestroyOnLoad Assets/_Project/Scripts/` before placing new persistent-singleton prefabs) must be placed as scene **root** GameObjects, not nested under a `[Managers]` container for hierarchy tidiness.

**Why:** `DontDestroyOnLoad` only works on root GameObjects. Nesting these prefabs under a parent group makes Unity print a console warning (`"X: DontDestroyOnLoad only works for root GameObjects..."`) and the object does NOT persist across scene loads the way the singleton pattern assumes. Discovered while building `CulDeSac_Room1_v2.unity` (World 1 Room 1 rebuild, ADR-0002) — grouping every manager prefab under one `[Managers]` object for a clean hierarchy silently broke both of these.

**How to apply:** `pfb_GameManager` and `pfb_RoomManager` do NOT call `DontDestroyOnLoad` (safe to nest under `[Managers]`), but always grep the actual script before assuming — don't rely on this list staying accurate. See [[project_room1_v2_build]] for the specific incident.
