---
name: feedback-reflection-invoke-event-handlers
description: To verify a script that reacts to a static event (e.g. RoomManager.OnRoomCleared) by toggling scene state, directly reflection-invoke its private handler method during Play Mode instead of waiting on the real coroutine-timed sequence that would normally raise the event. Also covers reflection-invoking a generic method with a `ref` parameter.
metadata:
  type: feedback
---

Learned 2026-08-26 verifying `WildWestCityZoneDirector` (see [[project_wildwestcity_build]]) against ADR-0004's validation checklist, in a session where Play Mode also proved unreliable across separate MCP tool calls (see [[feedback_unity_mcp_play_mode_capture]] — this adds a data point: Play Mode ended silently between calls **twice**, with no compile happening at the time, so it isn't only a "code just recompiled" trigger).

**The problem:** fully exercising a zone-clear/zone-activate reaction end-to-end requires either real combat (killing every spawned enemy) or waiting through `RoomManager`'s real `WaitForSeconds(0.7f)` room-clear delay coroutine — both need many real Play Mode frames to tick, which this environment does not reliably deliver across tool-call round trips.

**The fix:** directly invoke the reactive script's private handler method via reflection while a live Play Mode session is running, then read the resulting state back through the same reflection / `SerializedObject` machinery:

```csharp
var director = ...; // live component reference
var method = typeof(WildWestCityZoneDirector).GetMethod("HandleZoneCleared",
    BindingFlags.NonPublic | BindingFlags.Instance);
method.Invoke(director, new object[] { 0 });
// then check gate.enabled, wagon.activeSelf, etc. directly
```

This is deterministic, needs zero real-time waiting, and tests exactly the logic under test (does the script correctly react to the event firing) without needing the *cause* of that event (a real enemy death, a real 0.7s coroutine tick) to actually occur. It is not a substitute for verifying the event actually fires in real gameplay (RoomManager's own `OnRoomActivated`/`OnRoomCleared` dispatch still needs separate confirmation, e.g. via a genuine `_currentRoom` read after zone-0 auto-activated at scene start) — be explicit in any report about which parts were verified via real gameplay versus this direct-invocation technique.

**A second, related finding from the same session:** runtime-instantiated objects (spawn markers, spawned enemy clones) were correctly destroyed when Play Mode silently ended, and direct-reflection `SetActive`/`Collider.enabled` toggles made through live component references *during* an actual running Play Mode session were also correctly reverted on Stop, both times. [[feedback_play_mode_no_revert]]'s "Play Mode doesn't revert" finding was specifically about Inspector/Editor-driven mutations left after a session — it does not necessarily generalize to every kind of Play-Mode-time state change. Don't assume the worst case (permanent pollution) applies without checking; read the actual post-Stop state before deciding whether cleanup is needed.

**Third finding (2026-08-26, B108/`GameManager.ShowScreenOrLogMissing`): reflection-invoking a method with a `ref T` parameter does not write back into the field/local you conceptually bound it to — only into the `object[] args` array slot.** `MethodInfo.Invoke(target, args)` does copy the by-ref parameter's final value back into `args[i]` after the call (so `args[i]` correctly reflects what the method assigned), but if `args[i]` started as a plain `null` rather than something bound to a real field's storage, the real field is untouched — even though a normal compiled call site (`Foo(ref myField, ...)`) would have updated `myField` directly. Concretely: calling a generic `ShowScreenOrLogMissing<T>(ref T screen, ...)` via `method.MakeGenericMethod(typeof(ShopScreen)).Invoke(gm, new object[]{null, ...})` left `GameManager`'s real `_shopScreen` field still reading `null` afterward via `field.GetValue(gm)`, even though the method internally found and used the real `ShopScreen` instance correctly (confirmed via `Time.timeScale` changing and the show callback firing). **Fix for testing:** read `args[0]` after `Invoke` (not the original field) to see what the method actually did, and if you want the real field updated too (to fully restore state before continuing), write `args[0]` back into it manually via `field.SetValue(gm, args[0])`. This is a reflection-testing artifact, not a bug in the method under test — don't misdiagnose it as one.

**Fourth finding, same session: to test a "log an error if this dependency is genuinely still missing" branch without any risk of leaving Play Mode state unrevertable (per [[feedback_play_mode_no_revert]]'s caution about pre-existing-object mutations), pick an unrelated real Component type that has zero instances in the current scene as a stand-in, rather than `Destroy()`-ing a real pre-existing scene object to simulate absence.** E.g. verifying `GameManager.ShowScreenOrLogMissing<T>` correctly logs when `T` can't be found used `PermitPulperBossAI` (a real, already-referenced type in `GameManager.cs`, but one with zero instances in `CulDeSac_WildWestCity` since it's a different scene's boss) instead of destroying the real `RunEndScreen`/`ShopScreen` instance that does exist. Exercises the exact same `FindAnyObjectByType<T>() == null` code path with zero cleanup risk.
