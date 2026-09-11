# Where Things Stand

*Last updated: 2026-09-11. This is the one file to read to answer "where are we."
Full detail always lives in `docs/SPRINT.md` and `docs/BACKLOG.md` — this just
summarizes it in plain language.*

## The short version

**The iOS app has been submitted to Apple for App Store review** (2026-09-10) —
this went further than the original TestFlight-only plan. An Android build is
still being tested informally by a second tester, with no formal Play Store
submission yet. The support page and privacy policy are both live on the real
website. A two-week-old performance finding turned out to be a false alarm and
got corrected. One intermittent bug (occasional missing win screen) is still
being chased.

## Latest change (2026-09-11)

**World 2's fences are gone.** The Backyard/Dojo used to be ringed by a visible
bamboo stockade. You asked for it to work the way World 1 does — nothing to look
at, but still something that stops you walking off the edge — so the walls'
*visuals* were deleted and their *collision* was left completely untouched. You
can't tell the difference by walking around; you just can't see a fence any more.

Two things worth knowing:

- **The zone gates are now invisible too.** In World 2 the barrier that holds you
  in zone 1 until you've cleared it never had any artwork of its own — the fence
  was doing that job. With the fence gone, you'll walk into an invisible wall with
  nothing on screen to explain it. If that feels bad in play, the fix is to give
  the *gate* a visual (a shimmer, a rope, something), not to bring the fence back.
- **Also fixed:** one of World 2's weapon pickups was being dropped inside the
  forge bench. The bench has been moved 12 m further up the yard, into the
  emptiest part of zone 0 — it's now near the gate at the far end rather than
  sitting next to where you start, and it keeps at least 3 m of clearance from
  every enemy spawn point and prop around it.
- **Agility upgrades now do something you can feel.** Your partner was right —
  picking Agility changed almost nothing. It was wired up correctly, but it only
  ever affected one thing: how far the dodge roll travels, by half a metre per
  level on a 3 m roll. A level is now worth +0.75 m (a 25% longer, faster roll),
  and the two in-run Agility cards had magnitudes that were 10-20x too large
  (one would have given a 13 m dodge) — those are re-scaled, with a hard ceiling
  so it can't run away. One card claimed "Movement speed increased," which was
  flatly untrue; its text and name are corrected.
- **Parry can't be upgraded by anything, and that's worth a decision.** He was
  also trying to parry and expected Agility to help. Nothing in the game improves
  the parry window — the only way to get a wider one is to pick the Cowboy
  fighting style at the start of a run. Logged as B149.
- **Luck does nothing at all** (B148). Spark spent on the Luck stat, and the
  "Lucky Break" card, are both completely inert — nothing in the game reads that
  value. Needs your call: wire it to drop quality, or take it off the menu.
- **A real bug turned up while measuring this** (`B146`, not caused by the change):
  the zone-0 gate is 10.5 m wide but the gap it's meant to plug is 15 m, so there
  are two walkable slots either side of it. A player can currently slip past into
  zone 1 without clearing zone 0. Needs your call on the fix — see `docs/BACKLOG.md`.

## What's working

- World 1 → World 2, the whole loop: beat the SpinCycle boss in the
  Cul-de-Sac, see the win screen with correct stats, hit Continue, land in
  the Backyard/Dojo, beat the Grasscutter boss, see the World Map with both
  zones showing correctly.
- **The app has been submitted to Apple for review.** App Store Connect app
  record created, build archived and uploaded via Xcode, TestFlight testers
  already had it, and now it's gone further — into the actual App Store
  review queue. Store listing screenshots (both iPhone and 13" iPad sizes,
  since Apple flagged the app as iPad-capable) were captured directly in the
  Unity Editor at the exact required pixel sizes — no physical iPad needed.
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
  not the game — the batching system is on and working fine.
- **The real performance cost that was found instead is now fixed.** One
  small decorative prop (a stepping stone, repeated 32 times) was far more
  detailed than it needed to be. Reducing its detail turned out not to be
  possible without it visually breaking, so it — along with a few unrelated
  placeholder objects — was removed from the Backyard/Dojo level entirely.
  Measured result: World 2's triangle count dropped 45% and its draw calls
  dropped 40%, comfortably inside budget now.
- World 2's pathfinding setup (which enemies use to navigate) was checked
  live and confirmed working correctly — an earlier concern about it turned
  out to be a non-issue.
- App icon, splash screen, and loading art all show the real BoxForged
  branding (not leftover podcast/placeholder art).
- Store-listing prep (screenshots, copy, privacy policy) is done for iOS —
  it's what got submitted. A preview video is the one still-missing piece,
  and Apple didn't block submission on it.

## What's next

1. **Wait on Apple's review decision.** Typically takes anywhere from under
   24 hours to a few days. Nothing to do here but check App Store Connect.
2. **Catch the intermittent missing-win-screen bug.** Sometimes after
   beating SpinCycle the win screen doesn't show and the character freezes,
   even though the rest of the HUD keeps working. It only happens
   occasionally and has never been caught in the act — logging is in place
   waiting for the next time it happens on a real device, and a device
   console capture at that exact moment is what's needed to pin it down.
3. **One more performance check-off.** A single reading from the phone's
   performance profiler (something you'd already have open) would confirm
   the corrected performance finding above holds true on a real device, not
   just in the Editor.
4. **Set up a real Android release track.** Right now Android testing is
   informal (a build handed directly to a tester) — there's no Google Play
   Console app record or proper internal-testing track yet.
5. **Capture a preview video** for a future store-listing update — not
   required, would round things out.

## Known rough edges (none of these block TestFlight)

- The intermittent missing-win-screen bug above — the main thing still
  being chased.
- Pressing Special on a few specific Epic/Legendary weapons (Bo Staff,
  Pressure Cannon, Magic Wand, Shuriken) either freezes combat briefly or
  double-fires the special — a known, pre-existing issue (owner decision:
  ship TestFlight with it, fix later rather than delay for it).
- No automated tests exist yet.

## Where this comes from

This summary is drawn from `docs/SPRINT.md` (current sprint detail),
`docs/BACKLOG.md` (full bug/issue history), and `docs/KNOWN_ISSUES.md`
(the short technical list this file is the plain-language version of).
Update this file when sprint status changes materially — it should never
say something SPRINT.md contradicts.
