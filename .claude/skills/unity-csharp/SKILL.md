---
name: unity-csharp
description: Production C# conventions for Unity projects. Use when writing, refactoring, or reviewing C# scripts, tests, editor tooling, runtime components, or APIs.
---

# Unity C# Standards

- Enable nullable reference types where project/tooling permits and make null expectations explicit.
- Use clear names and small methods. Prefer immutable data and `readonly` where practical.
- Use `[SerializeField] private` instead of public fields for inspector wiring unless a public API is required.
- Validate required serialized dependencies early (`OnValidate`, initialization guards, or tests) rather than failing deep in gameplay.
- Cache component references. Never use `GetComponent`, LINQ, string formatting, reflection, or allocations repeatedly in hot `Update` paths without evidence it is acceptable.
- Pair event subscription/unsubscription correctly with Unity lifecycle.
- Avoid `async void` except event-handler boundaries. Define cancellation/lifetime behavior for async work.
- Do not hide errors with broad catch blocks. Log actionable context without flooding per-frame logs.
- Keep editor-only code in Editor assemblies/folders and guard platform-specific code.
- Add XML documentation to public reusable APIs when intent/constraints are not obvious; do not comment trivial syntax.
- Keep tests deterministic; abstract time/randomness where needed.
