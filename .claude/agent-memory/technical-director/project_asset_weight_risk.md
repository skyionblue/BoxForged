---
name: project-asset-weight-risk
description: BoxForged's dominant technical risk is asset weight — 2.6GB source textures all importing at 2048 with no platform overrides, plus 326MB of retired cutscene video shipping in StreamingAssets.
metadata:
  type: project
---

The largest technical risk in BoxForged is **asset weight**, not gameplay code. Two independent problems, both found by file inspection on 2026-08-19:

1. **Texture memory.** ~353 source textures totalling 2.6 GB; individual Meshy BaseColor maps run 27–31 MB. Essentially every texture imports at `maxTextureSize: 2048` with only a `DefaultTexturePlatform` entry — **no Android or iPhone override**. Estimated 100–150 MB of texture memory for a single room. The failure mode is thermal throttling ~10 minutes into a 10–15 minute run, which is exactly when a livestream demo has an audience.

2. **Package size.** `Assets/StreamingAssets/Cutscenes/` holds 10 `.mp4` totalling 326 MB and ships verbatim. Cutscenes are now locked to boss-intros-only; exactly one video (`spincycle_standoff.mp4`) is a boss intro. Retiring the rest recovers ~300 MB.

**Important caveat: neither figure has been measured on device.** They are derived from file inspection and are a hypothesis to profile against, not a finding. Do not present them as measured.

**Why:** the mobile target is 3–4-year-old hardware and runs are 10–15 minutes, so sustained thermal behaviour — not peak frame time — is the real acceptance criterion. Gameplay code hygiene is genuinely good (zero LINQ anywhere, pre-allocated physics buffers, consistent material `Destroy`), so optimization attention belongs on assets, not on the C#.

**How to apply:** when asked to optimize performance, start with the texture import policy (`AssetPostprocessor` with per-category caps) before touching gameplay code. Profile a full 15-minute run on a real mid-range device before any public demo. Recorded as B1/D2 in `docs/BACKLOG.md`.

Related: [[project-preproduction-gate]]
