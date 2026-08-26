---
name: feedback-editor-step-frozen-playmode
description: Play Mode simulation frames can stay frozen across many MCP tool calls when the Editor window is unfocused; EditorApplication.Step() reliably forces progress regardless of Time.timeScale
metadata:
  type: feedback
---

In this Unity MCP setup, a running Play Mode session does not reliably advance simulation frames between separate `execute_code`/tool calls when the Editor window isn't focused. Observed directly: `Time.frameCount` and `Time.realtimeSinceStartup` both stayed completely frozen across ~130 seconds of real wall-clock time and multiple round-trip tool calls, with `Time.timeScale` at both 0 and 1 — simple polling (just calling `execute_code` again and again) does not make the game loop tick.

**Fix that reliably works: call `UnityEditor.EditorApplication.Step()` (one or more times) from inside `execute_code`.** Each call processes a burst of whatever real elapsed wall-clock time has accumulated since the Editor last ticked — a single `Step()` after a long idle gap has been observed to jump `Time.frameCount` by ~800 and `Time.realtimeSinceStartup` by double-digit seconds. It works regardless of `Time.timeScale`, since it operates at the Editor/player-loop level, not the gameplay-time level — this makes it the right tool specifically for waiting out a `WaitForSecondsRealtime` coroutine delay while `Time.timeScale == 0` (e.g. behind a frozen character-picker screen). `Step()` also sets `EditorApplication.isPaused = true` as a side effect; that's fine, it doesn't block further `Step()` calls.

**Do not use `Thread.Sleep`/a bash `sleep` to try to "wait for frames"** — that blocks (or idles) the calling thread/process, not Unity's own Editor loop, and does not make Play Mode advance.

**How to apply:** whenever a Play Mode test needs real unscaled time to elapse (a `WaitForSecondsRealtime` delay, an animation, a Cinemachine damped move) and a plain re-query isn't advancing `Time.frameCount`/`Time.realtimeSinceStartup`, wrap the wait in a small loop of `EditorApplication.Step()` calls instead of polling. See [[project_wildwestcity_build]]'s seventh-pass entry (2026-08-26, H4 verification) for a worked example — this technique is what made it possible to observe a `WaitForSecondsRealtime(1.5f)`-gated UI screen actually appear and prove it opened a NavMesh gate in the same frame, rather than only being able to check state immediately-before and never immediately-after.

Related but distinct from [[feedback_unity_mcp_play_mode_capture]] (Play Mode silently *ending* mid-session) and [[feedback_rigid_rotation_technique]] point 4 (an earlier, milder version of "frames don't tick reliably between calls") — this is the strongest form of the symptom observed so far (fully frozen, not just unreliable), and `Step()` is the confirmed fix.
