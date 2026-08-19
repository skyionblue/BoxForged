---
name: project-preproduction-gate
description: BoxForged lifecycle state — pre-production authorized 2026-08-18; TDD/ARCHITECTURE/ADRs 0001-0003 produced 2026-08-19 and all still Proposed; production NOT authorized.
metadata:
  type: project
---

BoxForged left Discovery on 2026-08-18 ("Lock discovery and begin pre-production"). On 2026-08-19 the pre-production technical deliverable was produced: `docs/TECHNICAL_DESIGN.md`, `docs/ARCHITECTURE.md`, `docs/TECHNICAL_DECISIONS.md`, `docs/BACKLOG.md`, and three ADRs.

All three ADRs are **Proposed, not Accepted**. Production is not authorized — no C# or scene work should begin until the owner approves.

- **ADR-0001** fixed low follow camera: pitch 36°, FOV 45°, yaw 0°, offset `(0, 5.5, -7.57)`
- **ADR-0002** full scene rebuild, gated on extracting `RoomData` → `RoomDataSO` **before** old scenes are abandoned
- **ADR-0003** attack telegraph channel (not requested — recorded because ADR-0001 is unsafe without it)

**Why:** the owner locked two decisions (camera override, full scene rebuild) that are material architecture changes, and project rules require `technical-director` design + ADR before implementation.

**How to apply:** before doing any camera, scene, level, or forge implementation work, check whether these ADRs have moved to Accepted. If they are still Proposed, the work is not authorized. Six owner decisions (D1–D6 in `docs/BACKLOG.md`) also gate downstream work — notably the ability-system choice and whether existing room encounter data is preserved.

ADR-0001 and ADR-0003 should be approved or rejected **together**; the low camera without the telegraph work is the one combination that should not ship.

Related: [[project-asset-weight-risk]], [[project-docs-drift-from-code]]
