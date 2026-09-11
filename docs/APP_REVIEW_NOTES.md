# App Review Notes — BoxForged

**Purpose:** Apple's App Review asked for structured information about the app (a standard "Guideline 2.1 — Information Needed" request) after the first submission, 2026-09-10. They asked for it as a *reply in App Store Connect* **and** as permanent content in the **App Review Information → Notes** field, so future submissions don't re-trigger the same request.

**How to use this file:** §2–§6 below are written to be pasted verbatim. Every factual claim was verified against the codebase on 2026-09-11 — see §Verification record at the bottom for what was checked and how, so this can be re-audited rather than trusted blindly. **Two items need your confirmation before sending — see §Needs your confirmation.**

**Re-check this file before every resubmission.** It is only true as of the audit date. If the app later adds accounts, ads, IAP, analytics, or any network call, §4 and §5 become false and the Notes field becomes a misrepresentation to Apple.

---

## Why they probably asked

Nothing here suggests a rejection on content. This request is routine when a reviewer cannot quickly tell what the app does or cannot reach its content. Two likely contributors worth addressing directly in the reply:

- **A run takes 10–15 minutes and the game has no tutorial gate.** A reviewer who opens the app, taps around a menu, and quits after two minutes may never see combat, the forge, or a boss.
- **The store metadata mentions progression, weapons and bosses** — features a reviewer would want to see demonstrated and might not reach unaided.

The fix for both is §1's recording plus §3's instructions. Keep the tone factual; don't argue.

---

## 1. Screen recording — what to capture

This is the one item nobody can do for you: it must be a real capture on a **physical device running the latest iOS**, not the Simulator and not Editor footage.

**Good news on scope.** Apple lists three things to include *if the app has them*. BoxForged has **none** of them, all verified in code:

| Apple's requirement | Applies? | Why |
|---|---|---|
| Account registration / login / **account deletion** | **No** | There is no account system of any kind. Saves are a local JSON file. |
| User-generated content + reporting/blocking | **No** | No text input, no chat, no multiplayer, no sharing. |
| Accessing paid content or features | **No** | No in-app purchases, no ads, no paywalled content. |

So the recording is simply: **launch the app, and play through a normal session.**

**Suggested shot list** (aim for 3–5 minutes, one continuous take, device audio on):

1. **Start from the home screen and tap the app icon** — Apple explicitly requires the recording to begin with launch. Don't start mid-session.
2. Let the splash and loading screens play through without cutting.
3. Show the run-start flow: character/style selection (Ninja vs Cowboy), then starting a run.
4. **Play World 1 (the Cul-de-Sac) for a minute or two.** Show the core loop clearly and unhurriedly: moving, attacking, **dodging**, **parrying**, picking up a weapon.
5. **Use the forge bench** — walk up to it and open the forge UI. This is a signature mechanic and worth showing deliberately.
6. **Fight and beat the SpinCycle boss**, then show the win screen with its run stats.
7. Tap **Continue** to advance to World 2 (the Backyard/Dojo) so the reviewer sees progression works.
8. Open the **World Map** and the **pause menu / settings** briefly, so the full navigation surface is visible.

If a full boss kill makes the video too long, it is fine to demonstrate combat thoroughly and then show the win screen and progression. Showing that progression *works* matters more than showing a flawless fight.

---

## 2. Purpose and target audience — paste this

> **Purpose.** BoxForged is a single-player action game for mobile. The player controls a child who uses a cardboard box to reimagine ordinary backyard and neighborhood objects as weapons and equipment, then clears short, self-contained levels against enemies built from the same everyday objects. A typical session lasts 10–15 minutes from start to finish.
>
> **The experience it provides.** The game is built around skill-based, readable combat rather than statistics or waiting: every enemy attack has a visible tell, and the player responds by dodging or parrying at the right moment. Between fights, the player collects objects and uses an in-game forge bench to turn them into usable gear. It is designed for short sessions on a phone, works entirely offline, and has no ads, no in-app purchases, and no pressure mechanics of any kind.
>
> **Target audience.** General audience, all ages, with an emphasis on players who enjoy action games in short sessions. The game is rated for a general audience: it contains only cartoon/fantasy violence against inanimate reimagined household objects, with no blood, no gore, no realistic weapons, no profanity, no gambling, no controlled substances, and no sexual content. It is not directed primarily at children under 13 and is not enrolled in the Kids Category, but it is designed to be appropriate for family play.

---

## 3. Setup and access instructions — paste this

> **No setup, credentials, or sample files are required.** The app has no accounts, no login, no registration, and no server. It runs fully offline from first launch. There is nothing for the reviewer to configure and nothing gated behind a purchase, a code, or a region.
>
> **To reach all main features from a cold launch:**
>
> 1. Launch the app and wait through the splash and loading screens.
> 2. At the run-start screen, choose a character and a fighting style (Ninja or Cowboy), then begin a run. Either choice reaches the same content.
> 3. **Movement and combat use on-screen touch controls**: a virtual stick on the left to move, and action buttons on the right to attack, dodge, and parry. The game is landscape-orientation only.
> 4. Defeat the enemies in an area to open the gate to the next area. Enemies must be cleared before progression continues — if the player appears to be blocked by an invisible barrier, there are still enemies remaining in that area.
> 5. **Weapon pickups** are on the ground in each area; walk over one to pick it up.
> 6. **The forge bench** is a workbench prop placed in each area. Walk up to it to open the forge interface and convert collected material into gear.
> 7. Clearing an area's final boss ends the level and shows a run-summary screen. From there, **Continue** advances to the next zone and the **World Map** shows overall progress.
>
> **Time to reach content:** combat begins within roughly 30 seconds of starting a run. A full level including its boss takes approximately 10–15 minutes. There is no tutorial to complete and no waiting period.

---

## 4. External services, tools, and platforms — paste this

> **The app uses no external services of any kind to deliver its functionality. It is entirely self-contained and works with no network connection.**
>
> Specifically, the app contains:
>
> - **No authentication or account services.** There are no user accounts.
> - **No payment processors.** There are no in-app purchases and no ads.
> - **No analytics, telemetry, crash-reporting, or attribution SDKs.** Unity's analytics, crash-reporting, and performance-reporting services are all explicitly disabled in the project configuration, and the app is not linked to a Unity cloud project.
> - **No data providers, backends, or remote APIs.** The app makes no network requests. All game content ships inside the app bundle.
> - **No AI services at runtime.** The app does not call any AI or machine-learning service.
> - **No advertising, tracking, or third-party SDKs of any kind.**
>
> **Data handling:** the app's only persisted data is a local save file containing game progress, written to the app's own container on the device. Nothing is collected, transmitted, or shared with anyone, and no personal information is requested or stored at any point. This matches the published privacy policy at https://boxforged.com/privacy/.
>
> **For completeness regarding development tooling** (none of which is a service the app contacts at runtime): the app is built with the Unity engine, and some 3D art assets were generated during development using Meshy, an AI 3D-model generation tool. These assets are static files baked into the app bundle; the shipped app does not connect to Meshy or to any other service.

---

## 5. Regional differences — paste this

> **The app functions identically in all regions.** There is one single build with no region-specific features, content, pricing, or restrictions.
>
> - No geographic gating, region locks, or location-based behavior of any kind. The app does not request or use location data.
> - No remote configuration or feature flags — behavior cannot differ by region because nothing is fetched at runtime.
> - No regional pricing differences: the app is free everywhere with no in-app purchases.
> - The app is currently available in **English only** and presents the same English content in every region.

---

## 6. Regulated industries and protected third-party material — paste this

> **The app does not operate in a regulated industry.** It is a single-player action game. It involves no financial services, healthcare, gambling or real-money gaming, cryptocurrency, telecommunications, government services, alcohol, tobacco, cannabis, firearms sales, or dating, and it handles no regulated or sensitive personal data.
>
> **It contains no protected third-party material requiring authorization.** There is no licensed music, no third-party brand, trademark, likeness, or franchise content, and no real-world entity is depicted. All characters, settings, story, and world content are original works created by the developer.
>
> All art, audio, and code assets in the app are either created by the developer, generated by the developer using commercially licensed tools, or licensed for commercial use through the Unity Asset Store and Unity's standard engine and package licensing. The developer holds the necessary rights for all material included in the app.

---

## Needs your confirmation before sending

Two things I could not verify from the repository. Both are almost certainly fine, but the Notes field is a statement to Apple, so confirm rather than assume:

1. **Asset licensing (affects §6's final paragraph).** The project ships third-party asset packages — `Assets/ExplosiveLLC` (SuperCharacterController) and `Assets/Hayq Art` (Cartoon City) — plus 3D models generated with **Meshy**, and Unity's TextMesh Pro. I can see the files but not your purchase receipts or your Meshy plan tier. Confirm you bought the Asset Store packages under a normal commercial license and that your Meshy plan grants commercial rights to generated models. If any were free-for-personal-use only, §6's last paragraph needs rewording before you send it.

2. **The new build.** Everything here was audited against the current repository on 2026-09-11. If the build you're about to upload changed anything about accounts, networking, purchases, or analytics, re-check §4 before sending. Nothing in this session's work touched any of those.

---

## Verification record (2026-09-11)

How each claim above was established, so this can be re-audited instead of re-trusted:

| Claim | How verified |
|---|---|
| No accounts / login / account deletion | `SaveSystem.cs` persists a single `save.json` to `Application.persistentDataPath` via `File.WriteAllText`. No auth code, no credential storage, no server anywhere in `Assets/_Project/Scripts`. |
| No user-generated content | Zero matches across all game scripts for `TMP_InputField`, `InputField`, `TouchScreenKeyboard`, or on-screen keyboard invocation. No multiplayer or chat code. |
| No IAP / ads / paid content | No Unity Purchasing, Advertisement, or monetization package in `Packages/manifest.json`. `docs/STORE_LISTING.md` records the locked owner decision (2026-09-08): ship monetization-free. |
| No analytics / telemetry / crash reporting | `ProjectSettings/UnityConnectSettings.asset` — every service block reads `m_Enabled: 0` (Analytics, CrashReporting, Performance Reporting). `ProjectSettings.asset` has an empty `cloudProjectId` and empty `organizationId`, so the app is not linked to a Unity cloud project. |
| No network calls | Grep for `UnityWebRequest`, `HttpClient`, `WebClient`, `Socket`, `UnityServices`, `Firebase` across all game scripts returns exactly one file, `CutscenePlayer.cs`, and that match is `using UnityEngine.Video` — a local `VideoPlayer` reading a cutscene file from `StreamingAssets`, not a network call. |
| No AI services at runtime | Meshy is a development-time asset generation tool; no runtime reference exists. The `com.coplaydev.unity-mcp` package's runtime assembly contains only compatibility shims, serialization converters and a screenshot utility — verified to contain no `Socket`, `TcpClient`, `UnityWebRequest`, `HttpListener` or `NetworkStream` code. |
| No regional variation | No remote config or network fetch exists, so runtime behavior cannot vary by region. No location permission or API use. Single build, English only. |
| Content rating claims in §2 | `docs/STORE_LISTING.md` §3's rating table, itself sourced to `docs/STORY_BIBLE.md` and `docs/CREATIVE_STATE.md`. |

**Housekeeping note, unrelated to Apple:** the `com.coplaydev.unity-mcp` development package has an `autoReferenced` runtime assembly with no platform exclusions, so it compiles into shipping player builds. It is inert (no networking, no `MonoBehaviour` that auto-runs) and is not a review concern, but a development tool should not really be in a store build. Worth excluding from release builds as future cleanup — tracked as `docs/BACKLOG.md` B150.
