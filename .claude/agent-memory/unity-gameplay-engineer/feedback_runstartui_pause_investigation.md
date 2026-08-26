---
name: feedback-runstartui-pause-investigation
description: RunStartUI/ForgePanel already correctly freeze the background (verified 2026-08-26); reflection-driven Show()/ClearRunSelection() calls out of GameManager's normal order produce misleading artifacts, not real bugs.
metadata:
  type: feedback
---

On 2026-08-26 the owner reported "when the user is choosing their character the game should
not be playing in the background." Investigated in `CulDeSac_WildWestCity`:

- `RunStartUI.Show()` (Assets/_Project/Scripts/UI/RunStartUI.cs) already sets
  `Time.timeScale = 0f`, disables `_playerInput`, and sets `_cachedPlayerAnimator.speed = 0f`.
- `ForgePanel.cs:83-86` (referenced from `ForgePresenter.cs`'s header comment) also sets
  `Time.timeScale = 0f` when the mid-run forge/upgrade panel opens.
- `WorkbenchProp.Update()` explicitly early-returns (`if (Time.timeScale == 0f) return;`)
  so proximity events don't fire while paused.
- A clean repro (stop Play Mode fully, re-enter, let `GameManager.Start()` drive the flow
  untouched, screenshot the "Choose Your Hero" screen, wait ~2 real seconds, screenshot again)
  produced pixel-identical frames — zero background motion during character select under
  normal conditions.

**Artifact encountered, not a real bug:** while probing this manually, reflection-invoking
`RunStartUI.Show()` and `ProgressionSystem.ClearRunSelection()` directly (bypassing
`GameManager`'s normal call order) after having earlier teleported the player near a
workbench via script (for unrelated cardboard-pickup testing, with `Time.timeScale` still 1
at that time) produced a leftover Forge panel visibly stacked behind the character-select
panel, plus a later unexplained camera jump. This does not reproduce under normal play — it's
an artifact of racing manual reflection calls against the game's own Start()-time cutscene/
RunStartUI sequencing, not a defect in the current pause logic.

**Why this matters:** don't re-litigate "is the background paused during character select"
from scratch — the mechanism is already correct as of this date. If the owner reports it
again, ask for a concrete repro first (what exactly is moving — camera, an NPC, a specific
prop?) rather than re-deriving it via scripted Play Mode teleports/reflection, which this
session showed can itself manufacture confusing, non-representative state in this project.

**How to apply:** treat this as the current baseline for RunStartUI/ForgePanel pause
correctness. If a future session finds a genuine repro, update this memory with the real
root cause once confirmed under untampered normal play.
