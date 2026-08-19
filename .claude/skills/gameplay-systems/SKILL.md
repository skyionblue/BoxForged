---
name: gameplay-systems
description: Reusable Unity gameplay-system design guidance. Use for characters, interactions, abilities, state machines, checkpoints, hazards, triggers, objectives, cameras, input, or puzzle runtime logic.
---

# Gameplay Systems

Define behavior as states, inputs/events, outputs, invariants, interruption rules, and reset behavior before implementation.

- Keep input intent separate from character actions so touch/gamepad/keyboard can map to the same gameplay commands.
- Make character states explicit when transitions matter; reject illegal transitions deterministically.
- Build interactions around capabilities/interfaces rather than type checks when multiple actors/objects may participate.
- Design hazards/checkpoints/restarts so they cannot leave stale references, duplicated subscriptions, or partially reset puzzle state.
- Treat win/objective conditions as authoritative state, not UI state.
- Keep camera logic separate from player motor logic.
- Use configuration assets/data for tunable speeds, forces, cooldowns, ranges, and puzzle parameters.
- Every feature needs tests for normal flow, interruption, reset/retry, scene reload, and invalid/edge state where applicable.
