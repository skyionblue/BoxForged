---
name: project-bundle-identity-branding
description: Bundle ID history (com.theunboxedheroes → com.boxforged) and status of the 2026-09-08 release-readiness audit's open owner decisions.
metadata:
  type: project
---

`PlayerSettings.applicationIdentifier` **was** `com.theunboxedheroes` for both Android and iPhone, while `productName` is `BoxForged` and `companyName` is `SkyionBlue`. That was never a stale template leftover — `docs/media/*` confirms `theunboxedheroes.com` is the podcast's own public brand/website (the show BoxForged is built live on, see [[project_podcast]] in the main memory index).

**RESOLVED 2026-09-08:** owner decided to change it. Bundle ID is now `com.boxforged` (both platforms), changed before any store upload since it becomes permanent afterward. The local Android release keystore alias (`unboxedheroes`, at `/Users/jcelli/unboxed-heroes-release.keystore`) was **not** changed and doesn't need to match the bundle ID — keystore identity and application identifier are independent. Decision recorded in `docs/TECHNICAL_DECISIONS.md` §Engine and pipeline and `docs/SPRINT.md` Sprint 2.

**How to apply:** the podcast-branding context above still explains *why* `com.theunboxedheroes` existed historically — useful if it resurfaces in git history or old docs — but do not report it as the current bundle ID. Verify the live value in `ProjectSettings/ProjectSettings.asset` rather than trusting this note's date.

**Other open decisions from the 2026-09-08 audit — status as of that date, verify current state before reusing:**
- `defaultScreenOrientation` — **RESOLVED 2026-09-08.** Was fixed `LandscapeRight` (3) with both landscape autorotate flags inertly enabled; owner chose auto-rotate, now `AutoRotation` (4) so the existing flags take effect.
- `GameManager.cs`'s `Application.targetFrameRate` — **RESOLVED 2026-09-08.** Was hardcoded `30`, contradicting the documented 60 FPS target (`TECHNICAL_DESIGN.md` §3.1). Owner confirmed 60 FPS is the real target; cap changed to `60`. This makes the 2026-08-27 on-device profiling numbers stale (captured under the 30 FPS cap) — a fresh Pass A/B, extended to cover World 2 (`Backyard_Dojo`), is still needed and is the next open item.
- `WeaponGripTest.unity` in `EditorBuildSettings` — **still open, unresolved.** Deliberately kept (zone index 99, dev-only) per B66/ADR-0005; inflates shipped build size. Owner hasn't decided whether to strip it before store submission.
- Android adaptive/round icon slots empty, iOS 180×180 icon slot's source-file mismatch, Apple Developer Team membership confirmation, privacy policy/store data-safety prep, no automated test coverage, missing `KNOWN_ISSUES.md`/`CHANGELOG.md`/`AI_CONTEXT.md` — all **still open** per `docs/SPRINT.md` Sprint 2 as of 2026-09-08.
