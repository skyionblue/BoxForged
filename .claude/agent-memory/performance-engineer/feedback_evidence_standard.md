---
name: feedback-evidence-standard
description: Every performance finding must be explicitly labeled as inspected/certain vs needs-on-device-profiling; never present an inferred number as measured.
metadata:
  type: feedback
---

Every performance finding must state whether it is (a) certain from code/asset inspection or (b) a hypothesis that needs on-device profiling to confirm. Never present a derived number as a measurement.

**Why:** The project's own TDD sets this standard on itself — `docs/TECHNICAL_DESIGN.md` §3.4 ends its texture-memory analysis with "This has not been verified on device and should not be treated as measured." The owner explicitly asked for findings held to the same bar. BoxForged is built live on a podcast by two non-professional developers, so a confident-sounding but unverified claim can send a whole stream session chasing a non-problem.

**How to apply:** Split findings into "inspected — this is definitely true" (e.g. a shader with zero material references will be stripped; a `GetComponentInChildren` inside `Update` definitely runs per frame) and "plausible — profile it" (e.g. how many ms that actually costs). Also write findings so a non-Unity-expert can act on them without reading a profiler capture. See [[reference-perf-budgets]].
