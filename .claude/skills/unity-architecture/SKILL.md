---
name: unity-architecture
description: Unity architecture patterns and decision rules for Unity 6 LTS. Use when designing or changing runtime systems, dependencies, scenes, prefabs, ScriptableObjects, events, services, save/platform boundaries, or cross-platform structure.
---

# Unity Architecture

Use Unity 6 LTS and inspect existing structure before introducing patterns.

- Keep core rules in plain C# where practical; let MonoBehaviours adapt Unity lifecycle/input/physics/rendering to domain logic.
- Prefer explicit references or constructor/factory injection for pure C# objects. Avoid service locator patterns and uncontrolled singletons.
- Use ScriptableObjects for authored configuration/shared immutable-ish data, not as hidden mutable global state.
- Prefer small cohesive components and composition over deep inheritance.
- Treat scenes as composition roots. Avoid runtime `Find*` calls except editor/debug tooling.
- Use events/interfaces to decouple systems only when ownership boundaries are real; avoid event soup.
- Separate persistence, platform services, analytics, input, audio, and UI behind replaceable interfaces when those domains exist.
- Make reset/reload behavior explicit for gameplay systems.
- Record major cross-cutting decisions in `docs/adr/` and summarize them in `docs/TECHNICAL_DECISIONS.md`.

Before approval, compare at least one simpler alternative and state why the chosen design fits current scope.
