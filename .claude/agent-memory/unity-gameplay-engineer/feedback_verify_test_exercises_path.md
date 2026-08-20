---
name: feedback-verify-test-exercises-path
description: Before trusting a "verification passed" result for a resolution/lookup path (per-character variant, per-platform override, feature flag branch, etc.), confirm the test case actually goes through that path — not just that the end result looks correct.
metadata:
  type: feedback
---

Discovered 2026-08-20 during [[project_weapon_grip_socket]] (B63, docs/BACKLOG.md). Round 1 of that task tuned `WeaponGripPoint` on Cowboy/Ninja Male and "verified" it didn't regress the per-character `WeaponCycler.characterWeaponSets` duplication by equipping `WeaponObject_BikeHorn` and screenshotting the result. It looked fine — but `WeaponObject_BikeHorn` has no `_cb`/`_nm` variant asset, so `WeaponCycler.ResolveWeapon` silently fell back to the shared/default `WeaponData` for that test. The verification never actually ran the per-character-variant resolution branch it was meant to validate. The real bug (variant assets double-offsetting against the newly-tuned socket) shipped anyway and the owner caught it testing for real.

**Why this matters:** a resolver/lookup function with a fallback path (`ResolveWeapon` falls back to `baseWeapon` when no variant matches) can make a test case that *should* exercise branch A silently exercise branch B instead, with no visible difference in the passing result — the fallback is often deliberately designed to look seamless.

**How to apply:**
- When validating any per-variant/per-platform/per-character resolution path, pick (or construct) a test case you've confirmed actually has a variant for every branch being tested — don't assume a "representative" test case does.
- Better: log or inspect which branch actually fired (e.g. compare the resolved object's identity against the fallback's identity — `resolved == baseWeapon` — not just eyeball the visual result) so a silent fallback is visible in the test output itself, not just discoverable by someone re-deriving it later.
- If a "verification" only tests one instance of a multi-instance mechanism (one weapon out of 19, one character out of N), say so explicitly in the writeup rather than implying full coverage — this is what let the false "Cowboy looks correct" read stand unchallenged.
