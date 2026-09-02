---
name: cherry-tree-position-is-canon
description: The Blossom Court cherry tree's off-centre north-rim position (design z=62.53) is an explicit owner decision, not the B116 regression ADR-0008 first called it. Never propose re-centring it.
metadata:
  type: project
---

The cherry tree in `Backyard_Dojo.unity` sits at design `(0, ·, 62.530)` — about 7.5 m north of the Blossom Court's `(0, 55.0)` centre, hard against the north rim. **That position is intentional and is CANON.**

Owner, 2026-09-01, verbatim: *"I moved the tree to the back of the area because it made more sense back there."*

**Why:** an earlier `technical-director` pass (ADR-0008's first version) built a full derivation on the premise that the off-centre position was an unintended B116 regression, and specified moving both the trunk collider and the visual back to the centre. It had three plausible-looking lines of evidence — ADR-0006 §1.1 explicitly rejects a north-rim tree on four grounds; B116's completion note claims it *re-centred* the tree while `git log -L` shows `z: 47.5` → `z: 62.53`; and B116's reported M2 figure is arithmetically only producible from a centred trunk. All three are real findings about the *documentation*. None of them was evidence about the owner's intent, and the whole derivation was wrong because no record connected the move to a decision.

**How to apply:**

- Do not propose re-centring the tree, and do not treat its position as a defect, in any task. If a metric or shot fails because of it, the tree is the fixed constraint and the other thing moves. ADR-0008 §2 moved the *boss* for exactly this reason.
- `docs/adr/0006-world2-zone-scale-and-arena-metric.md` §1.1 is **superseded for this placement by owner authority**, and carries an in-place banner saying so. Its four grounds are deliberately preserved — they may still be sound general guidance, and two of them named real consequences that are now tracked as `docs/BACKLOG.md` B125 (zone 2 has no central obstruction to orbit any more) and ADR-0008 §Validation 8 (the victory beat's bloom shot was framed for a centred tree).
- **The general lesson, which is the reusable part:** a documented decision plus a diff that contradicts it is evidence of a *records* problem, not proof of a code defect. Where the diff is a deliberate-looking placement change on an authored asset, ask the owner before deriving a fix from the doc. Cheaper than a full ADR.
- The mirror-image finding still stands and should not be dropped in sympathy: B116's completion note is inaccurate about **what it did** regardless of whether the result was wanted, and its M2 number was computed from the spec rather than measured. Re-scoped as `docs/BACKLOG.md` B121, which now also asks that any completion note describing a *move* quote before/after coordinates.

Related: [[boss-intro-camera]], [[project-docs-drift-from-code]], [[project-unsatisfiable-metrics]]
