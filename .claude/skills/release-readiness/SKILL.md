---
name: release-readiness
description: Pre-build and pre-release validation for Unity Android/iOS projects. Use before the owner performs builds, TestFlight/Play testing, or store submission.
---

# Release Readiness

Do not perform the final distributable build or deployment. Prepare the project so the owner can do so safely.

Validate: clean source state; target scenes/order; bundle/application identifiers; semantic version/build number policy; orientation; graphics APIs; scripting backend/architecture; stripping/linker risks; permissions; privacy/data collection implications; icons/splash/store assets; localization; accessibility settings; save migration; analytics/online-service environment; test results; known issues; performance budgets; device smoke-test matrix; release notes; rollback/recovery notes.

Produce owner-run Android and iOS build checklists using the project's actual settings. Never invent signing identities, keystores, certificates, provisioning profiles, or store credentials.
