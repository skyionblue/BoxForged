---
name: git-workflow
description: Git workflow for Claude-driven Unity feature development. Use when starting/completing tasks, creating branches, committing changes, reviewing diffs, or managing repo hygiene.
---

# Git Workflow

- Start from a clean understanding of current status; never discard unrelated user changes.
- Branch naming: `feature/<ticket>-<slug>`, `fix/<ticket>-<slug>`, `chore/<ticket>-<slug>`.
- Keep commits coherent and descriptive. Include the task ID when one exists.
- Before commit: inspect diff, run relevant tests/validators, perform code review for meaningful changes, update required docs, and verify no generated/cache/secrets are accidentally staged.
- Unity projects must keep `.meta` files paired with assets and must not commit Library/Temp/Logs/Obj or local IDE caches.
- Do not merge, force-push, rewrite shared history, or delete remote branches without explicit owner approval.
- Stop after a branch is ready and committed; report exact manual verification steps and merge readiness.
