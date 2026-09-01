---
name: coroutine-external-stop-cleanup
description: Unity coroutines stopped externally (StopAllCoroutines/StopCoroutine) never resume to run pending cleanup — any resource a coroutine acquires must also be released at the external stop site, not just inside the coroutine.
metadata:
  type: feedback
---

A Unity coroutine that is stopped from outside itself (`StopAllCoroutines()` on the owning
MonoBehaviour, or `StopCoroutine(handle)`) is simply discarded — its `IEnumerator` is never
`MoveNext()`'d again. Any cleanup written *inside* the coroutine body, even one that looks like a
guard ("if state == Dead, Hide the handle, then yield break"), only runs if the coroutine itself
resumes on its own and hits that check. If death/interruption is detected and handled by some
other code path that kills the coroutine directly (the common pattern: `_state = Dead;
StopAllCoroutines();`), the in-coroutine check never fires — C# `finally` blocks don't help either,
since Unity's coroutine stop doesn't unwind the enumerator's stack, it just drops the reference.

**Why this matters here:** found while implementing [[project_backyard_dojo_build]]'s 15th pass
(B118/ADR-0007, the Grasscutter's ground-plane dash-lane telegraph). `GrasscutterAI.SpinDash`
raises a pooled `AttackTelegraphHandle` and had an in-coroutine `if (_state == BossState.Dead) {
Hide(handle); yield break; }` check — but the actual death path is `HandleDeath()` calling
`StopAllCoroutines()` immediately after setting `_state = Dead`, so that in-coroutine check is
dead code for every real death. Fixed by adding an explicit `AttackTelegraphService.Hide(handle)`
call in `HandleDeath()` itself, before `StopAllCoroutines()`.

**How to apply:** any time a coroutine in this codebase acquires a pooled/held resource (a
telegraph handle, a rented hazard slot, an input lock, anything with a `Hide`/`Release`/`Return`
counterpart) and the owning object can also be killed/interrupted from outside that coroutine
(death, stagger, scene teardown, `OnDisable`), check whether the kill path calls
`StopCoroutine`/`StopAllCoroutines` directly. If it does, an in-coroutine-only cleanup check is not
sufficient — add the release call at the external stop site too. This applies to every boss/enemy
AI in this project that follows the same `WindUp`/telegraph/`StopAllCoroutines`-on-death pattern
(`SpinCycleAI`, `PermitPulperBossAI`, and any future one), not just `GrasscutterAI`.
