# Unboxed Heroes — PixVerse Video Asset Document

**Version:** 1.0
**Date:** 2026-07-16
**Tool:** PixVerse (AI video generation — text-to-video and image-to-video)
**Output format:** MP4, 4–8 seconds per clip
**Target use:** Unity VideoPlayer component, RawImage UI panel, or sprite sheet source

---

## Summary Table

| Asset Name | Use | Duration | Loop |
|---|---|---|---|
| `vid_loading_bg_loop` | Loading screen animated background | 8 s | Yes |
| `vid_menu_bg_loop` | Main menu atmospheric background | 8 s | Yes |
| `vid_boss_spincycle_intro` | SpinCycle entrance cinematic before Room 3 | 6 s | No |
| `vid_win_imagination_restore` | Imagination Restore — world shifts from grey to vivid | 6 s | No |
| `vid_defeat_screen_loop` | Game-over atmosphere | 6 s | Yes |
| `vid_worldtree_ambient_loop` | World Tree ambient breathing loop (future hub/overworld) | 8 s | Yes |
| `vid_transition_room_portal` | Room-to-room transition wipe | 4 s | No |

---

## Asset Details

---

### 1. `vid_loading_bg_loop`

**Where it is used**
Animated background on the loading screen, playing behind the static UI overlay (title lockup, loading bar, progress text). Replaces or augments the static `ui_loading_bg_drained.png` from `docs/loading-screen-art-direction.md`. The video loops seamlessly while scene data loads.

**Duration:** 8 seconds
**Loop:** Yes — must cut seamlessly. Design the motion so the end frame matches the opening frame (slow drift never reaches a hard edge).

**PixVerse prompt**

```
Slow cinematic drift across a desolate post-apocalyptic landscape built entirely from corrugated cardboard, kraft paper, and marker ink. Drained desaturated world — pale warm grey sky (#D4CFC9), cracked cardboard earth, bare skeletal branches. Camera drifts imperceptibly rightward at 1 cm per second, barely moving. Scattered in the foreground: crumpled grey paper balls, an overturned tin can with marker-drawn ridges, a curling strip of kraft tape on the cracked ground. In the mid-distance a massive World Tree made of stacked compressed corrugated cardboard rings rises toward the sky, its bark showing kraft tape repair strips and hand-drawn marker growth lines. Desaturated dust motes drift slowly upward from the tree base. Thick black marker outlines on all surfaces. Flat cel-shaded surfaces with visible corrugation grain. No characters. Mood: vast quiet and determination. Tearaway game art style. No bright colors. No UI. No text.
```

**Negative prompt:** photorealism, smooth plastic, neon, bright colors, lush green nature, glowing effects, fast motion, camera shake, modern architecture, cheerful tone

**Aspect ratio:** 16:9
**Mode:** Text-to-video (no single source image — the background should span the full frame without being anchored to one portrait render)
**Source image option:** If image-to-video is preferred, use `images/Meshy_AI_pause-cardboard-bg-landscape-sm.png` as the starting frame and prompt for slow rightward drift and falling paper motes.

**Unity integration**
- File lands in `Assets/_Project/Video/LoadingScreen/vid_loading_bg_loop.mp4`
- Attach a `VideoPlayer` component to the loading screen canvas background GameObject
- Set VideoPlayer to `Loop = true`, `Render Mode = Render Texture`
- Assign a `RenderTexture` to a `RawImage` UI element sized to fill the canvas
- Play the video in `Awake` of the `LoadingScreenController`; pause or stop on scene-load complete
- The static UI overlay (title, bar, progress text) sits on a higher canvas sort order in front of the video

---

### 2. `vid_menu_bg_loop`

**Where it is used**
Animated background for the main menu screen. No main menu exists yet as a shipping scene, but this asset is the atmospheric layer for when it is built. The main menu will use the same drained-world framing as the loading screen — the player hasn't reclaimed anything yet.

**Duration:** 8 seconds
**Loop:** Yes

**PixVerse prompt**

```
Slowly breathing atmospheric landscape made entirely of corrugated cardboard and marker ink. The massive World Tree at center-frame, built from stacked cardboard rings, sways imperceptibly in a still wind — branches shift by 2–3 degrees and return. Camera is locked, no movement. Desaturated warm grey palette. Pale grey overcast sky. Cracked corrugated cardboard earth. Subtle paper fiber particles drift upward from the tree base, very slow, very sparse — fewer than ten visible at any time. Thick black marker outlines on all edges. Flat cel-shaded cardboard surfaces. The tree trunk shows kraft tape repair strips and marker-drawn growth rings. A faint radial fog at the tree base, grey-white at 35% opacity. Deep quiet mood — like the world is holding its breath. Tearaway game art style. No characters. No text. No UI.
```

**Negative prompt:** photorealism, green trees, bright colors, fast wind, camera pan, lens flare, glowing, neon, happy, modern setting, smooth plastic surfaces

**Aspect ratio:** 16:9
**Mode:** Text-to-video
**Source image option:** Use `images/Meshy_AI_pause-cardboard-bg-landscape-sm.png` as the starting frame if image-to-video gives a stronger result for a static camera with breathing tree motion.

**Unity integration**
- File lands in `Assets/_Project/Video/MainMenu/vid_menu_bg_loop.mp4`
- VideoPlayer on a `RawImage` filling the menu canvas background layer
- `Loop = true`, begin playback in `Start` on the main menu scene
- Menu UI elements (buttons, title) sit on a higher canvas sort order

---

### 3. `vid_boss_spincycle_intro`

**Where it is used**
Boss entrance cinematic that plays when the player enters Room 3 (the SpinCycle boss room) for the first time. This was deferred during Sprint 8. The clip plays once, then gameplay control is returned to the player. Unity pauses player input for the clip duration.

**Duration:** 6 seconds
**Loop:** No

**PixVerse prompt**

```
Dramatic cinematic boss entrance sequence. Camera begins low, ground level, looking down a cracked corrugated cardboard corridor. Steam vents from below the ground. The camera slowly tilts upward to reveal an enormous anthropomorphic washing machine — 4 meters tall — lurching into frame. Its drum door is its face: a porthole with hand-drawn angry marker eyes and a scowling mouth. Its body is corrugated cardboard panels bolted with brass brads, its sides wrapped in crinkled aluminum foil that catches cold grey light. The machine's drum spins slowly behind the porthole glass. It slams one corrugated cardboard fist into the ground as it enters, sending a shockwave of crumpled paper scraps outward. Cold grey-blue ambient lighting. Single warm point light from the machine's drum interior casting an orange-amber glow outward through the porthole. Thick black marker outlines on all surfaces. Flat cel-shaded cardboard and foil textures. Drained desaturated world. Heavy dramatic mood. No player character visible. Tearaway and Scott Pilgrim art style. No text. No UI.
```

**Negative prompt:** photorealism, friendly expression, bright cheerful colors, small boss, smooth plastic machine, clean laundry room, neon lighting, modern setting, happy tone, camera shake excessive

**Aspect ratio:** 16:9
**Mode:** Image-to-video. Use `images/SpinCycle.png` as the source image to anchor the boss character design, then prompt for the camera reveal motion described above.

**Unity integration**
- File lands in `Assets/_Project/Video/Cinematics/vid_boss_spincycle_intro.mp4`
- Trigger in `RoomManager.OnRoomActivated` when room index matches the SpinCycle room
- Use a full-screen `RawImage` canvas (sort order above HUD) with a `VideoPlayer` in `Play On Awake = false`
- Disable player input (`PlayerInput.enabled = false`) at clip start; re-enable in `VideoPlayer.loopPointReached` callback
- Hide HUD during playback; restore after
- `Loop = false`; destroy or deactivate the canvas object after playback ends

---

### 4. `vid_win_imagination_restore`

**Where it is used**
Plays immediately after the SpinCycle is defeated. Replaces (or plays before) the current real-time URP post-process color grade lerp. The cinematic version gives this moment weight as the most important visual event in the game — the world coming back to life. Afterwards, control returns to the player and the reclaimed color grade is locked in.

**Duration:** 6 seconds
**Loop:** No

**PixVerse prompt**

```
The world springs back to life from grey to vivid color, spreading outward like watercolor ink bleeding across wet paper. The transformation begins at the center of the frame where the defeated washing machine boss stood — a burst of warm amber-orange light, like a match being struck. Color bleeds outward from that point: first the cracked cardboard earth turns from grey-brown to warm kraft tan (#E8C97A), grass marker-green blades spring upward, the sky floods with saturated marker blue (#4A90D9). The World Tree in the mid-distance transforms in sequence — its corrugated trunk lightens from grey to rich brown cardboard, new vivid green cardboard-leaf clusters unfurl on its bare branches. Paper confetti in marker orange and marker purple bursts upward from the ground. The color spread moves like ink diffusing outward — not a sharp edge, a soft bloom. Cel-shaded flat surfaces, thick marker outlines on everything. Warm joyful mood building from quiet to euphoric over 6 seconds. Tearaway game art style. Watercolor-on-cardboard transition effect. No text. No UI.
```

**Negative prompt:** photorealism, dark tone, grey remaining, slow fade, linear wipe, digital glitch, neon, smooth plastic, no detail in tree

**Aspect ratio:** 16:9
**Mode:** Text-to-video (the transformation spans the whole environment, not a single character — no single source image captures it)
**Source image option:** For a grounded starting frame, use `images/Meshy_AI_pause-cardboard-bg-landscape-sm.png` as the opening drained-world frame and prompt for the color burst transformation to spread outward from center.

**Unity integration**
- File lands in `Assets/_Project/Video/Cinematics/vid_win_imagination_restore.mp4`
- Triggered in `GameManager` when boss death event fires, before `GameState = Won`
- Full-screen `RawImage` canvas over the scene, `VideoPlayer` plays once
- Keep the current URP post-process lerp code as a fallback; skip it when the video clip is assigned and plays successfully
- After `loopPointReached`: deactivate the video canvas, apply the reclaimed URP volume weight to 1.0 to lock in the color grade permanently, then show the win UI
- `Loop = false`

---

### 5. `vid_defeat_screen_loop`

**Where it is used**
Animated atmospheric background for the game-over / defeat screen. Plays behind the "You Lost" text, restart button, and any defeat UI. The current `GameOverUI` just pauses `Time.timeScale` and shows a panel — this adds atmosphere to that moment.

**Duration:** 6 seconds
**Loop:** Yes

**PixVerse prompt**

```
Slow, melancholy atmosphere over a drained desaturated cardboard world. The frame holds still — no camera movement. Foreground: cracked grey corrugated cardboard earth, overturned tin can, crumpled paper debris. Mid-distance: the base of the World Tree, bare and grey. Grey paper scraps drift slowly downward from above, like ash falling, at a near-imperceptible pace — three or four scraps visible at once. The tree's bare branches sway very slightly. Cold, still, heavy mood — not violent, not dramatic, just quiet defeat. Pale warm grey sky. Thick black marker outlines. Flat cel-shaded cardboard surfaces. Corrugated texture throughout. Tearaway art style. No characters. No UI. No text.
```

**Negative prompt:** photorealism, bright colors, action, camera shake, fast motion, happy tone, neon, glowing, smooth plastic, cheerful, explosions

**Aspect ratio:** 16:9
**Mode:** Text-to-video
**Source image option:** Use `images/Meshy_AI_pause-cardboard-bg-landscape-sm.png` as the starting frame if image-to-video produces better texture fidelity.

**Unity integration**
- File lands in `Assets/_Project/Video/UI/vid_defeat_screen_loop.mp4`
- In `GameOverUI.Show(bool won)`: when `won == false`, activate a `RawImage` background panel with a `VideoPlayer` behind the defeat text and buttons
- `Loop = true`; `AudioListener.pause = true` is already set in `GameOverUI` — VideoPlayer audio should be muted (defeat clip is atmosphere only; the `AudioManager` handles defeat audio)
- Stop and hide on restart (`OnRestartClicked`)

---

### 6. `vid_worldtree_ambient_loop`

**Where it is used**
Ambient loop for a future hub world, overworld map, or any scene where the World Tree appears as a living, breathing background element in its reclaimed (vivid) state. Not needed in the current build scope but produced now while the aesthetic is fresh. File in project so it is available when the overworld is designed.

**Duration:** 8 seconds
**Loop:** Yes

**PixVerse prompt**

```
The World Tree in its reclaimed, vivid state — fully alive. Massive trunk of rich brown corrugated cardboard with visible kraft tape repairs and marker-drawn growth rings. Cardboard leaf clusters rendered in vivid marker green (#5CB85C) unfurl and sway gently in a warm breeze. The sky is saturated marker blue (#4A90D9). Warm afternoon directional light from the right catches the tree's right-facing surfaces in amber-gold (#E8C97A). Paper butterflies — flat cardboard cutout shapes in marker orange and marker purple — drift lazily past the mid-frame. The camera is locked, no movement. Leaves and paper confetti pieces settle very slowly. Thick black marker outlines on all elements. Flat cel-shaded surfaces, corrugated cardboard texture on the trunk. Warm, hopeful, calm mood — the world restored. Tearaway and Little Big Planet art style. No characters. No UI. No text.
```

**Negative prompt:** photorealism, grey, drained palette, fast motion, lens flare, neon, smooth plastic, dark mood, camera shake, modern architecture

**Aspect ratio:** 16:9
**Mode:** Text-to-video

**Unity integration**
- File lands in `Assets/_Project/Video/Ambient/vid_worldtree_ambient_loop.mp4`
- Reserved for future overworld or hub scene
- VideoPlayer on a `RawImage` background panel; `Loop = true`

---

### 7. `vid_transition_room_portal`

**Where it is used**
Room-to-room transition wipe that plays when the player moves from Room 1 (Grunt arena) to Room 2 (transition corridor) and from Room 2 to Room 3 (SpinCycle boss room). Currently the transition is likely an instant scene load. This clip covers the cut with a craft-material wipe that feels deliberate and on-brand. 4 seconds is enough to mask a fast async load; for slower loads, the loading screen handles it separately.

**Duration:** 4 seconds
**Loop:** No

**PixVerse prompt**

```
A hand-crafted page-turn or cardboard flap transition wipe across the full frame. A large flat sheet of corrugated cardboard sweeps from left to right across a dark background, its surface showing kraft paper grain and faint corrugation lines, the leading edge curling slightly as it moves. As the cardboard sheet exits the right side of frame, it reveals the destination environment behind it — a different cracked-earth cardboard landscape. The sweep takes 3 seconds; the last second holds on the revealed environment. Movement is smooth but slightly hand-made — not perfectly mechanical. Thick marker-black outlines on the cardboard edge. Flat cel-shaded. Kraft tan (#C4893A) and dark brown (#3D2B1F) as the dominant colors of the sweeping sheet. No characters. No text. No UI.
```

**Negative prompt:** photorealism, digital wipe effect, glitch, neon, bright colors, smooth transition, fade to black only, modern graphic design

**Aspect ratio:** 16:9
**Mode:** Text-to-video

**Unity integration**
- File lands in `Assets/_Project/Video/Transitions/vid_transition_room_portal.mp4`
- Full-screen `RawImage` canvas at the highest sort order
- Trigger play at the start of `SceneManager.LoadSceneAsync` or the equivalent room-activation event in `RoomManager`
- `Loop = false`; hide canvas in `loopPointReached`
- If async load finishes before the clip ends, hold the canvas visible until `loopPointReached` — do not cut early

---

## Production Notes

### PixVerse Generation Settings

All clips should be generated at the highest available resolution PixVerse offers (1920×1080 minimum). If PixVerse outputs at a lower resolution, upscale before import.

**Style consistency across all clips:** Every prompt uses the same style vocabulary — corrugated cardboard texture, thick marker outlines, flat cel-shaded surfaces, Tearaway art style. When regenerating a clip that doesn't match, add the phrase "matching the visual style of all other clips in the series" to reinforce consistency.

**Negative prompt to include on every clip:**

```
photorealism, smooth plastic, neon colors, glowing, lens flare, modern design, text, UI elements, watermark
```

### Unity Import Settings

Apply these import settings to every video asset via the Unity Editor import inspector:

| Setting | Value |
|---|---|
| Transcode | Enabled |
| Codec | H.264 (broad device support) |
| Bitrate mode | High (for quality) |
| Spatial Quality | High |
| Import Audio | Disabled — all audio handled by `AudioManager` |
| Keep Alpha | Disabled (no alpha channel needed) |

### Loop Point Matching

For every looping clip (`vid_loading_bg_loop`, `vid_menu_bg_loop`, `vid_defeat_screen_loop`, `vid_worldtree_ambient_loop`): visually inspect the loop point before committing. In PixVerse, request that the last frame of motion matches the first frame — slow drift assets achieve this naturally; any clip with a particle burst or directional sweep needs a crossfade or re-generation.

### VideoPlayer Render Mode

All clips use `Render Mode = Render Texture` in Unity's VideoPlayer component, not `Camera Near Plane` or `Camera Far Plane`. This keeps the video in the UI layer and avoids z-fighting with scene geometry.

### File Naming Convention

Consistent with the project naming convention from `docs/art-style-guide.md` (section 10), all video assets use the `vid_` prefix:

```
vid_[subject]_[variant]_[state].mp4
```

No equivalent type prefix existed in the guide — `vid_` is the addition for this asset category.

### Priority Order for Production

Given Sprint 10 is the mobile build sprint and the loading screen is in progress:

1. `vid_loading_bg_loop` — loading screen is active Sprint 10 work; unblocks visual polish now
2. `vid_boss_spincycle_intro` — highest narrative impact; was explicitly deferred
3. `vid_win_imagination_restore` — second most important narrative moment
4. `vid_defeat_screen_loop` — small lift; adds polish to an existing scene
5. `vid_transition_room_portal` — nice-to-have for Sprint 10 mobile build feel
6. `vid_menu_bg_loop` — blocked until main menu scene exists
7. `vid_worldtree_ambient_loop` — blocked until overworld/hub is designed

---

_PixVerse Video Asset Document v1.0 — 2026-07-16_
_Consistent with Art Style Guide v1.0 and Loading Screen Art Direction v1.0._
