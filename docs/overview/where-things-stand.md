# Where Things Stand

*Last updated: 2026-09-09. This is the one file to read to answer "where are we."
Full detail always lives in `docs/SPRINT.md` and `docs/BACKLOG.md` — this just
summarizes it in plain language.*

## The short version

Both levels play start to finish, on a real phone, right now. World 1 (the
Cul-de-Sac) leads into World 2 (the Backyard/Dojo), the win screen and World
Map both work, and the game is one step away from being in testers' hands via
TestFlight.

## What's working

- World 1 → World 2, the whole loop: beat the SpinCycle boss in the
  Cul-de-Sac, see the win screen with correct stats, hit Continue, land in
  the Backyard/Dojo, beat the Grasscutter boss, see the World Map with both
  zones showing correctly.
- App icon, splash screen, and loading art all show the real BoxForged
  branding (not leftover podcast/placeholder art).
- Store-listing prep (screenshots plan, copy, privacy policy) is essentially
  done.
- Eight small backlog cleanups landed 2026-09-09: the save-system debug panel
  no longer ships in release builds, shadow rendering is tuned to the
  camera's actual range (a real mobile performance win), the "enemies
  remaining" HUD counter can no longer read wrong, a chunk of confirmed-dead
  code and 3 stale duplicate environment prefabs were removed, a stale/
  misleading code comment was corrected, a defensive warning was added to
  catch a future zone-progression edge case, and a cosmetic Inspector data
  quirk on 4 scene objects was fixed.

## What's next — and it's not a coding task

The very next step is **App Store Connect setup**, which only the owner can
do (Apple requires it, and it's also this project's own rule — agents don't
touch final builds or store submissions):

1. Create the app record on App Store Connect.
2. Archive and upload the build from Xcode.
3. Fill in TestFlight info (one open decision: which feedback email to use).
4. Add testers and get real people playing it.

Full click-by-click steps are in `docs/SPRINT.md` under "Ready for TestFlight."

## Known rough edges (none of these block TestFlight)

- World 2 is heavier on draw calls/triangles than the project's own budget —
  worth a look before a *public* release, fine for tester feedback now.
- A couple of minor open bugs (an occasional null-reference error in an enemy
  health bar; a narrow win/death race condition) are tracked but not urgent.
- No automated tests exist yet.

## Where this comes from

This summary is drawn from `docs/SPRINT.md` (current sprint detail) and
`docs/BACKLOG.md` (full bug/issue history). Update this file when sprint
status changes materially — it should never say something SPRINT.md
contradicts.
