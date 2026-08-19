---
name: accessibility
description: Baseline accessibility rules for Unity games. Use for gameplay, UI, controls, feedback, puzzles, audio, haptics, menus, onboarding, and testing.
---

# Accessibility Baseline

Bake accessibility into design rather than adding it at release.

- Never make color the only carrier of required information; pair color with shape/icon/text/animation/audio as appropriate.
- Support scalable/readable text and UI, safe-area layouts, adequate contrast, and large touch targets.
- Provide independent music/SFX/voice controls where those channels exist; subtitles/captions for essential spoken information.
- Make haptics optional and avoid requiring haptic perception.
- Support input remapping where practical on platforms that expose configurable controls; keep gameplay actions abstracted from physical inputs.
- Avoid required rapid repetition, extremely precise timing, or simultaneous complex gestures unless central to the intended challenge; provide alternatives/assist options when reasonable.
- Ensure critical puzzle state and interactability have redundant feedback.
- Preserve settings persistently and test accessibility features as first-class behavior.
