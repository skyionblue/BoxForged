---
name: puzzle-level-design
description: Cooperative puzzle and level-design procedure for games where multiple actors have complementary abilities. Use for puzzle specs, level sequencing, solvability, cooperation enforcement, hazards, checkpoints, or anti-soft-lock review.
---

# Puzzle and Level Design

For every puzzle specify:
1. Learning objective and mechanics exercised.
2. Initial world state and available information.
3. Required capabilities/actors.
4. State transitions and gating conditions.
5. Intended solution sequence.
6. Valid alternate solutions.
7. Exploits/bypass routes to prevent or intentionally allow.
8. Fail states, hazards, checkpoint/reset behavior, and soft-lock recovery.
9. Success condition and feedback.
10. Accessibility and readability requirements.
11. Automated/manual test cases.

When cooperation is a core pillar, prove that no single character/actor can satisfy all required state transitions alone. Introduce one new concept at a time before combining it with previous mechanics. Increase reasoning complexity before increasing timing precision unless the design explicitly calls for dexterity.
