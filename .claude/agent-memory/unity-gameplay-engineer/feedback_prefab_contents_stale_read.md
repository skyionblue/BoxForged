---
name: feedback-prefab-contents-stale-read
description: PrefabUtility.LoadPrefabContents() + GetComponentsInChildren() immediately after can read stale/wrong transform values on the first call of a session — verify with git diff after the real edit, don't trust the first live read alone.
metadata:
  type: feedback
---

Discovered 2026-08-21 during the B66 `WeaponGripPoint` clean-slate reset (`docs/BACKLOG.md`, [[project_weapon_grip_socket]]). Before touching any of the 4 `_childsafe` character prefabs, ran one `execute_code` call per prefab: `PrefabUtility.LoadPrefabContents(path)` → find the `WeaponGripPoint` child by name → read `localPosition`/`localEulerAngles` → `UnloadPrefabContents`. This reported a plausible-looking **mixed** picture (Cowboy disputed, Cowgirl/Ninja Male already clean, Ninja Female at the old seed value) that matched the task's own stated assumption closely enough to not look suspicious.

**It was wrong for 3 of the 4 prefabs.** After unconditionally setting all four to identity and saving, `git diff` showed the *actual* prior committed value: all four prefabs carried the exact same non-identity rotation quaternion, not the mixed set the first read reported. The write pass (immediately followed by an independent fresh re-load-and-verify, not the same in-memory instance) was correct; only the very first exploratory read was stale/wrong.

**How to apply:** a `LoadPrefabContents` → read → `UnloadPrefabContents` cycle run early in a session (especially the *first* one) is not fully trustworthy on its own for values that matter to a decision — don't report or act on it as ground truth. Prefer: (1) do the intended edit unconditionally based on what the task actually needs, not contingent on trusting the diagnostic read, then (2) verify via `git diff` against the real committed file, or a second independent fresh load, after saving. If a pre-edit read and a post-edit `git diff` disagree about what "before" was, trust the `git diff` — it reflects the real file, not whatever in-memory prefab-content instance the read call happened to produce.
