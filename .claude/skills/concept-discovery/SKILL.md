---
name: concept-discovery
description: Develop incomplete game ideas collaboratively before formal pre-production. Use for new game concepts, vague or exciting-but-unresolved ideas, creative direction questions, player-fantasy exploration, mechanic/theme alignment, scope discovery, or whenever the owner wants to flesh out a game rather than immediately create a GDD, TDD, roadmap, backlog, or sprint.
---

# Concept Discovery

Treat discovery as collaborative design, not document generation.

## Core rules

- Read existing project context and creative notes before proposing changes.
- Preserve explicit owner decisions. Never silently promote an idea to accepted design.
- Ask 3-5 tightly related questions at a time, then wait for answers.
- When useful, present 2-4 meaningfully different options and explain the player-experience tradeoff of each.
- Recommend a direction when evidence supports one, but keep major creative choices with the owner.
- Challenge contradictions, weak player motivation, scope traps, derivative design, and mechanics that do not support the intended experience.
- Do not create production sprints, implementation tasks, final GDD/TDD content, or architecture during discovery.

## Track creative state

Maintain `docs/CREATIVE_STATE.md` using exactly these states:

- **CANON**: explicitly accepted by the owner; do not change casually.
- **WORKING**: current best direction; still negotiable.
- **OPEN**: unresolved question that matters.
- **REJECTED**: intentionally ruled out; do not revive without a new reason.

Only the owner may promote a major creative decision to CANON.

## Discover the game in this order when applicable

1. Emotional promise and player fantasy.
2. Audience, platform context, and intended session experience.
3. Core verbs and core loop.
4. What makes the game distinct.
5. Challenge, failure, recovery, and progression.
6. Story/world/character needs; invoke the Story Room for narrative-heavy concepts.
7. Camera, controls, presentation, accessibility, and feedback.
8. Scope boundaries, experiments, major risks, and unresolved questions.

Do not force this order when the concept clearly needs a different sequence.

## Connect ideas to play

For every major concept ask:

- What does the player do?
- What does the player understand or feel because of that action?
- What feedback makes the rule legible?
- How does the idea deepen over time?
- What would make it frustrating, repetitive, or unclear?
- Can it be demonstrated through play instead of explained?

Prefer designs where mechanics, presentation, narrative, and progression reinforce the same player experience.

## Discovery completion gate

Before recommending pre-production, summarize:

- the current game promise;
- accepted pillars;
- core loop and verbs;
- important CANON decisions;
- remaining OPEN questions;
- risky assumptions that need prototypes;
- what is deliberately out of scope.

Remain in discovery until the owner explicitly authorizes the transition. Accept phrases such as **"Lock discovery and begin pre-production"** or **"Lock the concept and begin pre-production"** as authorization.
