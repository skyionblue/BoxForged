---
name: feedback-codedom-no-local-functions
description: The Unity MCP execute_code tool's default CodeDom compiler cannot parse C# local functions — use Func<>/Action<> lambda variables instead, or the whole script fails to compile with confusing errors.
metadata:
  type: feedback
---

Discovered 2026-08-26 implementing ADR-0004 in `CulDeSac_WildWestCity` (see [[project_wildwestcity_build]]). This project's `execute_code` MCP tool defaults to the CodeDom compiler (C# 6 grammar), not Roslyn. **C# local functions — `void Foo(...) { ... }` or `T Foo<T>(...) { ... }` declared inside a method body — are a C# 7 syntax feature CodeDom's parser does not recognize at all.** Writing one anywhere in an `execute_code` script produces a wall of misleading "unexpected symbol `(`... expecting `,`, `;`, or `='" errors starting at the local function's declaration line and cascading through the rest of the method — the actual cause (a local function) is not named anywhere in the error output, so it reads like a cascading syntax typo rather than an unsupported-language-version issue.

**How to apply:** if `execute_code` fails to compile with a long cascade of "unexpected symbol" errors starting partway through an otherwise-plain script, check first whether the script declares a local function. Replace it with a `Func<TArgs..., TReturn>` or `Action<TArgs...>` variable assigned a lambda — e.g. `Func<string, int, Vector3, GameObject> MakeThing = (name, idx, pos) => { ...; return go; };` — since lambda-to-delegate assignment is a C# 3 feature CodeDom handles fine. This is purely a rewrite of the same logic, not a capability loss.

Generalizes to any future `execute_code` call in this project (and any other project using MCPForUnity's default CodeDom path) that needs a helper subroutine inside a single script — always reach for `Func`/`Action` lambdas from the start rather than local functions, to avoid the debugging round-trip.
