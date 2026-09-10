# Where Things Stand

*Last updated: 2026-09-10. This is the one file to read to answer "where are we."
Full detail always lives in `docs/SPRINT.md` and `docs/BACKLOG.md` — this just
summarizes it in plain language.*

## The short version

The game is in testers' hands. TestFlight is live on iOS (app record created,
build uploaded, invites sent), and an Android build is being tested informally
by a second tester. The support page and privacy policy are both live on the
real website. A two-week-old performance finding turned out to be a false
alarm and got corrected. One intermittent bug (occasional missing win screen)
is still being chased.

## What's working

- World 1 → World 2, the whole loop: beat the SpinCycle boss in the
  Cul-de-Sac, see the win screen with correct stats, hit Continue, land in
  the Backyard/Dojo, beat the Grasscutter boss, see the World Map with both
  zones showing correctly.
- **TestFlight is live.** App Store Connect app record created, build
  archived and uploaded via Xcode, tester invites sent.
- **The marketing website is current**: hero and enemy character art
  updated to the latest concept art, a new support page is live at
  `boxforged.com/support`, and the privacy policy remains live and accurate.
- If a player also dies right as they land the killing blow on a boss (in
  the few seconds before the win screen appears), the win now correctly
  counts — this used to silently turn into a loss instead.
- **A two-week-old performance concern turned out to be wrong, and got
  corrected rather than left on the books.** World 2 looked like it was
  getting no benefit from Unity's rendering-batching system (three separate
  measurements said so). It turned out the *measurement itself* was broken,
  not the game — the batching system is on and working fine. The real
  performance cost was found instead: one small decorative prop (a stepping
  stone) is far more detailed than it needs to be and is repeated 32 times,
  eating about a third of the triangle budget by itself.
- World 2's pathfinding setup (which enemies use to navigate) was checked
  live and confirmed working correctly — an earlier concern about it turned
  out to be a non-issue.
- App icon, splash screen, and loading art all show the real BoxForged
  branding (not leftover podcast/placeholder art).
- Store-listing prep (screenshots plan, copy, privacy policy) is essentially
  done except real screenshots/video, which can be captured any time now.

## What's next

1. **Catch the intermittent missing-win-screen bug.** Sometimes after
   beating SpinCycle the win screen doesn't show and the character freezes,
   even though the rest of the HUD keeps working. It only happens
   occasionally and has never been caught in the act — logging is in place
   waiting for the next time it happens on a real device, and a device
   console capture at that exact moment is what's needed to pin it down.
2. **One more performance check-off.** A single reading from the phone's
   performance profiler (something you'd already have open) would confirm
   the corrected performance finding above holds true on a real device, not
   just in the Editor.
3. **Fix the real performance cost** found above (the over-detailed
   stepping stone prop) — an art/asset task, not urgent, not blocking
   anything.
4. **Set up a real Android release track.** Right now Android testing is
   informal (a build handed directly to a tester) — there's no Google Play
   Console app record or proper internal-testing track yet.
5. **Capture real screenshots and a preview video** for the eventual store
   listing, now that the full game loop works end-to-end on a real device.

## Known rough edges (none of these block TestFlight)

- The intermittent missing-win-screen bug above — the main thing still
  being chased.
- Pressing Special on a few specific Epic/Legendary weapons (Bo Staff,
  Pressure Cannon, Magic Wand, Shuriken) either freezes combat briefly or
  double-fires the special — a known, pre-existing issue (owner decision:
  ship TestFlight with it, fix later rather than delay for it).
- No automated tests exist yet.
- A navmesh detail (an enemy's ability to path around a pond) can silently
  behave slightly differently between the Unity Editor and a real device
  build — found this session, not yet fixed, low priority.

## Where this comes from

This summary is drawn from `docs/SPRINT.md` (current sprint detail),
`docs/BACKLOG.md` (full bug/issue history), and `docs/KNOWN_ISSUES.md`
(the short technical list this file is the plain-language version of).
Update this file when sprint status changes materially — it should never
say something SPRINT.md contradicts.
