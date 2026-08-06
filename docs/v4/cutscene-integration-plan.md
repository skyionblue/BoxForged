# Cutscene Integration Plan

Play 9 pre-rendered H.264 MP4 cutscenes at key game moments. Game is **landscape-only**; portrait clips are **pillarboxed** for now (confirmed). Skippable (confirmed). Unity Video module already installed.

## Storage
- Copy MP4s to `Assets/StreamingAssets/Cutscenes/` (clean names below). Play via `VideoPlayer.url = Path.Combine(Application.streamingAssetsPath, "Cutscenes", "<file>.mp4")`.
- H.264 is hardware-decoded on iOS/Android — no Unity transcode.
- **Android gotcha:** StreamingAssets live compressed inside the APK (`jar:file://…`), which `VideoPlayer` may not read directly. Handle robustly: on first play, if the direct URL fails / on Android, copy the file from StreamingAssets to `Application.persistentDataPath` (via `UnityWebRequest`) and play from there. Editor + iOS play from StreamingAssets directly.
- Track `*.mp4` in git-LFS (add to `.gitattributes` if not already).

## CutscenePlayer (reusable, DontDestroyOnLoad singleton)
- Full-screen overlay Canvas (top sort order): black background Image + RawImage (RenderTexture target) + Skip button (fades in after ~1s).
- `VideoPlayer` → RenderTexture → RawImage. Audio via VideoPlayer (AudioSource, unscaled).
- API: `Play(string fileName, System.Action onFinished, bool skippable = true)`.
- Aspect-fit via `AspectRatioFitter` using the video's width/height (read after `Prepare`) → portrait pillarboxes, landscape fills.
- While playing: raise the overlay, disable player input; restore + `onFinished` on `loopPointReached` OR Skip.
- Pausing gameplay during in-scene cutscenes optional (video plays on its own clock); overlay hides gameplay anyway.

## Trigger map (primary clip → moment, + play-once policy)
| Moment | Primary clip | Alt | Play policy |
|---|---|---|---|
| Game intro / first launch | `boy_putting_box_on.mp4` (landscape 10s) | `boy_putting_box_on_v2.mp4` (portrait 30s) | once ever |
| Enter Cul-de-Sac zone | `wild_west_transform.mp4` | `wild_west_change_phone.mp4` | once per zone |
| Character select — Ninja | `ninja_skills.mp4` | — | once per character |
| Character select — showcase | `cowboy_ninja_skills.mp4` | — | once |
| SpinCycle boss intro | `spincycle_standoff.mp4` | — | each boss encounter (skippable) |
| First forge at workbench | `forge_whip_craft.mp4` | `forge_whip_2.mp4` | once (tutorial) |

Play-once flags persisted (PlayerPrefs or SaveData). All alternates shipped so clips are swappable by config.

## Clean filenames (StreamingAssets/Cutscenes/)
boy_putting_box_on.mp4, boy_putting_box_on_v2.mp4, wild_west_transform.mp4, wild_west_change_phone.mp4, ninja_skills.mp4, cowboy_ninja_skills.mp4, spincycle_standoff.mp4, forge_whip_craft.mp4, forge_whip_2.mp4

## Hook points (existing code)
- Intro: a boot/first-scene entry (GameManager or a new bootstrap) — no menu scene exists yet.
- Zone entry: GameManager scene-load into `CulDeSac_Room1` (first room of zone).
- Character select: `RunStartUI.OnStartClicked` (after gender/style chosen).
- Boss intro: `SpinCycleAI` — play before/at the start of its intro sequence in `CulDeSac_BossArena`.
- First forge: `ForgePanel`/`ForgeController.TryForge` first success.

## Open items
- Character-select showcase repetition — defaulting to once-per-character to avoid replaying every run.
- ~200 MB total video (git-LFS) — compress/trim later if build size matters.
