using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Boxhead.Core
{
    /// <summary>
    /// DontDestroyOnLoad singleton that plays a full-screen H.264 cutscene from
    /// StreamingAssets and invokes a callback when the clip ends (or is skipped).
    ///
    /// The entire overlay (Canvas → black background → RawImage → Skip button) is built
    /// in code by <see cref="Bootstrap"/> so any scene can call <see cref="Play"/> without
    /// needing a prefab wired into the scene. A matching prefab is also shipped at
    /// Assets/_Project/Prefabs/UI/pfb_cutscene_player.prefab for manual instantiation.
    ///
    /// Landscape clips fill the screen; portrait clips are pillarboxed (black bars on the
    /// sides) via an AspectRatioFitter driven by the video's real width/height.
    ///
    /// Concurrency policy: if <see cref="Play"/> is called while a cutscene is already
    /// playing, the new request is ignored (the in-flight callback is not fired). Cutscene
    /// triggers in this project are gated by one-shot flags or distinct scene loads, so
    /// overlapping requests do not occur in practice.
    /// </summary>
    public class CutscenePlayer : MonoBehaviour
    {
        public static CutscenePlayer Instance { get; private set; }

        /// <summary>True while a cutscene overlay is visible and the clip is playing.</summary>
        public bool IsPlaying { get; private set; }

        private const string CutsceneSubfolder = "Cutscenes";
        private const float  SkipRevealDelay    = 1f; // seconds (unscaled) before Skip becomes usable
        private const float  VideoVolume        = 0.3f; // cutscene audio level (0-1); source clips are loud, keep low

        // Loading screen tuning (unscaled seconds).
        private const string LoadingSpriteName  = "loading_screen"; // Resources.Load<Sprite>(...)
        private const float  LoadingAspect      = 1376f / 768f;     // native aspect of the loading art
        private const float  LoadingMinHold     = 1.5f;             // min time the loading art stays fully opaque
        private const float  LoadingFadeDuration = 1f;              // fade-out duration once the hold elapses

        // Built-in-code UI
        private Canvas               _canvas;
        private RawImage             _rawImage;
        private AspectRatioFitter    _aspectFitter;
        private Button               _skipButton;
        private CanvasGroup          _skipGroup;

        // Loading screen (shown over the video during Prepare for cutscenes that request it).
        private Image                _loadingImage;
        private CanvasGroup          _loadingGroup;
        private AspectRatioFitter    _loadingFitter;

        // Video
        private VideoPlayer          _videoPlayer;
        private AudioSource          _audioSource;
        private RenderTexture        _renderTexture;

        // Playback state
        private Action               _onFinished;
        private PlayerInput          _playerInput;         // disabled during playback, restored after
        private bool                 _playerInputWasEnabled;
        private Coroutine            _skipRevealRoutine;
        private Coroutine            _prepareRoutine;
        private Coroutine            _loadingRoutine;
        private float                _loadingShownUnscaledTime; // unscaled timestamp the loading art became visible
        private readonly WaitForSecondsRealtime _skipRevealWait = new WaitForSecondsRealtime(SkipRevealDelay);

        // ── Bootstrap ────────────────────────────────────────────────────────────

        /// <summary>
        /// Auto-spawns the singleton before the first scene loads so any scene can call
        /// CutscenePlayer.Instance.Play(...) without pre-wiring a prefab.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("CutscenePlayer");
            go.AddComponent<CutscenePlayer>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildOverlay();
            BuildVideoPlayer();
            HideOverlay();
        }

        private void OnDestroy()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.loopPointReached -= OnLoopPointReached;
                _videoPlayer.errorReceived    -= OnVideoError;
            }
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
            if (Instance == this) Instance = null;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Plays the named cutscene full-screen. <paramref name="fileName"/> is a file inside
        /// StreamingAssets/Cutscenes/ (e.g. "spincycle_standoff.mp4"). <paramref name="onFinished"/>
        /// fires exactly once when the clip completes or is skipped. When
        /// <paramref name="skippable"/> is false the Skip button never appears.
        /// If already playing, the call is ignored (see class remarks).
        /// </summary>
        public void Play(string fileName, Action onFinished = null, bool skippable = true)
        {
            Play(fileName, onFinished, skippable, showLoadingScreen: false);
        }

        /// <summary>
        /// As <see cref="Play(string, Action, bool)"/>, but when <paramref name="showLoadingScreen"/>
        /// is true a full-screen loading image covers the video during Prepare() and for a minimum
        /// hold afterward, then fades out (unscaled). The Skip button is only revealed once the
        /// loading art has fully faded. Used for the game-intro cutscene on first boot.
        /// </summary>
        public void Play(string fileName, Action onFinished, bool skippable, bool showLoadingScreen)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                onFinished?.Invoke();
                return;
            }

            if (IsPlaying)
            {
                Debug.LogWarning($"[CutscenePlayer] Play('{fileName}') ignored — a cutscene is already playing.");
                return;
            }

            IsPlaying   = true;
            _onFinished = onFinished;

            ShowOverlay();
            SetSkipInteractable(false, skippable);

            // Bring up the loading art immediately so it covers the black frame during Prepare().
            if (showLoadingScreen) ShowLoadingScreen();

            // Disable player input so screen taps only reach the Skip button.
            DisablePlayerInput();

            if (_prepareRoutine != null) StopCoroutine(_prepareRoutine);
            _prepareRoutine = StartCoroutine(PrepareAndPlay(fileName, skippable, showLoadingScreen));
        }

        // ── Playback pipeline ───────────────────────────────────────────────────────

        private IEnumerator PrepareAndPlay(string fileName, bool skippable, bool showLoadingScreen)
        {
            // Resolve the source URL. On Android, StreamingAssets are packed inside the APK
            // (jar:file://...) and VideoPlayer cannot read them directly, so copy to a readable
            // path first. In the Editor and on iOS the StreamingAssets path is a plain file path.
            string url = null;
            yield return ResolvePlayableUrl(fileName, resolved => url = resolved);

            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError($"[CutscenePlayer] Could not resolve a playable URL for '{fileName}'. Ending cutscene.");
                EndPlayback();
                yield break;
            }

            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url    = url;
            _videoPlayer.Prepare();

            // Wait for prepare (with a timeout so a broken clip never hangs the game).
            float waited = 0f;
            while (!_videoPlayer.isPrepared && waited < 8f)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!_videoPlayer.isPrepared)
            {
                Debug.LogError($"[CutscenePlayer] VideoPlayer failed to prepare '{fileName}' within timeout. Ending cutscene.");
                EndPlayback();
                yield break;
            }

            // Match the RawImage aspect to the real video dimensions so portrait pillarboxes
            // and landscape fills, regardless of the overlay's own aspect ratio.
            int w = (int)_videoPlayer.width;
            int h = (int)_videoPlayer.height;
            if (w > 0 && h > 0)
            {
                _aspectFitter.aspectRatio = w / (float)h;

                // CRITICAL: the RenderTexture must match the video's NATIVE resolution. A fixed
                // (e.g. 1920x1080) RT makes the VideoPlayer squash a portrait frame to fill it,
                // and the AspectRatioFitter then just bars an already-distorted image. Rebuild the
                // RT per clip at w x h so the frame stays undistorted; the fitter pillarboxes it.
                if (_renderTexture == null || _renderTexture.width != w || _renderTexture.height != h)
                {
                    if (_renderTexture != null) { _renderTexture.Release(); Destroy(_renderTexture); }
                    _renderTexture = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32) { name = "CutsceneRT" };
                    _renderTexture.Create();
                    _videoPlayer.targetTexture = _renderTexture;
                    _rawImage.texture          = _renderTexture;
                }
            }

            _videoPlayer.Play();

            if (showLoadingScreen)
            {
                // The loading art covers the video; hold it for a minimum time (so it's actually
                // seen even when Prepare was instant in-editor), fade it out, THEN reveal Skip.
                if (_loadingRoutine != null) StopCoroutine(_loadingRoutine);
                _loadingRoutine = StartCoroutine(HoldFadeLoadingThenRevealSkip(skippable));
            }
            else if (skippable)
            {
                // Normal path: fade the Skip button in ~1s after playback starts (unscaled).
                if (_skipRevealRoutine != null) StopCoroutine(_skipRevealRoutine);
                _skipRevealRoutine = StartCoroutine(RevealSkipAfterDelay());
            }

            _prepareRoutine = null;
        }

        /// <summary>
        /// Holds the opaque loading art for at least <see cref="LoadingMinHold"/> total (measured from
        /// when the art actually became visible, so URL-resolve + Prepare time both count toward it),
        /// fades it out over <see cref="LoadingFadeDuration"/> on the unscaled clock, then — if the
        /// cutscene is skippable — reveals the Skip button ~1s later. All waits are manual unscaled-time
        /// accumulator loops yielding null, so no per-frame GC.
        /// </summary>
        private IEnumerator HoldFadeLoadingThenRevealSkip(bool skippable)
        {
            // Minimum hold: remaining time so the art has been on-screen for LoadingMinHold total.
            // Using true elapsed on-screen time (covers URL-resolve + Prepare), not just Prepare polling.
            float shownFor      = Time.unscaledTime - _loadingShownUnscaledTime;
            float remainingHold = LoadingMinHold - shownFor;
            float held = 0f;
            while (held < remainingHold)
            {
                held += Time.unscaledDeltaTime;
                yield return null;
            }

            // Fade alpha 1 → 0 on the unscaled clock (yielding null allocates nothing).
            float t = 0f;
            while (t < LoadingFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float a = 1f - (t / LoadingFadeDuration);
                if (a < 0f) a = 0f;
                if (_loadingGroup != null) _loadingGroup.alpha = a;
                yield return null;
            }

            HideLoadingScreen();

            // Only now allow the Skip button to appear (~1s after the fade, matching normal timing).
            if (skippable)
            {
                if (_skipRevealRoutine != null) StopCoroutine(_skipRevealRoutine);
                _skipRevealRoutine = StartCoroutine(RevealSkipAfterDelay());
            }

            _loadingRoutine = null;
        }

        /// <summary>
        /// Produces a URL the VideoPlayer can read. Editor/iOS use the StreamingAssets path
        /// directly. Android (and any platform where the direct path isn't a plain file) copies
        /// the asset to persistentDataPath via UnityWebRequest on first use, then plays from there.
        /// </summary>
        private IEnumerator ResolvePlayableUrl(string fileName, Action<string> onResolved)
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, CutsceneSubfolder, fileName);

            // A plain, already-readable file path (Editor, standalone, iOS): use it directly.
            bool needsCopy = streamingPath.Contains("://"); // jar:file:// on Android, etc.

            if (!needsCopy && File.Exists(streamingPath))
            {
                onResolved(streamingPath);
                yield break;
            }

            // Cached copy from a previous run?
            string cacheDir  = Path.Combine(Application.persistentDataPath, CutsceneSubfolder);
            string cachePath = Path.Combine(cacheDir, fileName);
            if (File.Exists(cachePath))
            {
                onResolved(cachePath);
                yield break;
            }

            // Copy StreamingAssets → persistentDataPath via UnityWebRequest (works for jar: URIs).
            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);

            using (UnityWebRequest request = UnityWebRequest.Get(streamingPath))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[CutscenePlayer] Failed to copy '{fileName}' from StreamingAssets: {request.error}");
                    // Last resort: hand the raw streaming URL to VideoPlayer and hope the platform reads it.
                    onResolved(streamingPath);
                    yield break;
                }

                try
                {
                    File.WriteAllBytes(cachePath, request.downloadHandler.data);
                    onResolved(cachePath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CutscenePlayer] Failed to write cached cutscene '{fileName}': {e.Message}");
                    onResolved(streamingPath);
                }
            }
        }

        private IEnumerator RevealSkipAfterDelay()
        {
            yield return _skipRevealWait;
            SetSkipInteractable(true, true);
            _skipRevealRoutine = null;
        }

        private void OnLoopPointReached(VideoPlayer source)
        {
            EndPlayback();
        }

        private void OnVideoError(VideoPlayer source, string message)
        {
            Debug.LogError($"[CutscenePlayer] VideoPlayer error: {message}. Ending cutscene.");
            EndPlayback();
        }

        private void OnSkipClicked()
        {
            EndPlayback();
        }

        /// <summary>Stops playback, hides the overlay, restores input, and fires the callback once.</summary>
        private void EndPlayback()
        {
            if (!IsPlaying) return;
            IsPlaying = false;

            if (_skipRevealRoutine != null) { StopCoroutine(_skipRevealRoutine); _skipRevealRoutine = null; }
            if (_prepareRoutine    != null) { StopCoroutine(_prepareRoutine);    _prepareRoutine    = null; }
            if (_loadingRoutine    != null) { StopCoroutine(_loadingRoutine);    _loadingRoutine    = null; }

            // Reset the loading image so the next (non-loading) cutscene never flashes the art.
            HideLoadingScreen();

            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
                _videoPlayer.url = string.Empty;
            }

            RestorePlayerInput();
            HideOverlay();

            Action cb = _onFinished;
            _onFinished = null;
            cb?.Invoke();
        }

        // ── Player input gating ──────────────────────────────────────────────────────

        private void DisablePlayerInput()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null && playerObj.TryGetComponent(out _playerInput))
            {
                _playerInputWasEnabled = _playerInput.enabled;
                _playerInput.enabled = false;
            }
            else
            {
                _playerInput = null;
            }
        }

        private void RestorePlayerInput()
        {
            // Only restore if we found and disabled it, and it still exists (scene may have changed).
            if (_playerInput != null)
            {
                if (_playerInputWasEnabled) _playerInput.enabled = true;
                _playerInput = null;
            }
        }

        // ── Overlay construction (all in code) ───────────────────────────────────────

        private void BuildOverlay()
        {
            // Canvas — Screen Space Overlay, sorted above all HUD/panels.
            var canvasGO = new GameObject("CutsceneCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);

            _canvas = canvasGO.GetComponent<Canvas>();
            _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 32000; // above every HUD/panel canvas

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight  = 0.5f;

            // Full-rect black background — covers the whole screen behind the (pillarboxed) video.
            var bgGO = new GameObject("Background", typeof(Image));
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bg = bgGO.GetComponent<Image>();
            bg.color = Color.black;
            StretchFull(bg.rectTransform);

            // RawImage that displays the RenderTexture, sized by an AspectRatioFitter.
            var rawGO = new GameObject("VideoImage", typeof(RawImage), typeof(AspectRatioFitter));
            rawGO.transform.SetParent(canvasGO.transform, false);
            _rawImage = rawGO.GetComponent<RawImage>();
            _rawImage.color = Color.white;
            StretchFull(_rawImage.rectTransform);

            _aspectFitter = rawGO.GetComponent<AspectRatioFitter>();
            _aspectFitter.aspectMode  = AspectRatioFitter.AspectMode.FitInParent;
            _aspectFitter.aspectRatio = 16f / 9f;

            // Skip button, bottom-right, starts hidden (alpha 0, non-interactive).
            var skipGO = new GameObject("SkipButton",
                typeof(Image), typeof(Button), typeof(CanvasGroup));
            skipGO.transform.SetParent(canvasGO.transform, false);

            var skipRect = ((Image)skipGO.GetComponent<Image>()).rectTransform;
            skipRect.anchorMin = new Vector2(1f, 0f);
            skipRect.anchorMax = new Vector2(1f, 0f);
            skipRect.pivot     = new Vector2(1f, 0f);
            skipRect.anchoredPosition = new Vector2(-60f, 60f);
            skipRect.sizeDelta        = new Vector2(240f, 90f);

            var skipImg = skipGO.GetComponent<Image>();
            skipImg.color = new Color(0f, 0f, 0f, 0.6f);

            _skipButton = skipGO.GetComponent<Button>();
            _skipButton.targetGraphic = skipImg;
            _skipButton.onClick.AddListener(OnSkipClicked);

            _skipGroup = skipGO.GetComponent<CanvasGroup>();
            _skipGroup.alpha = 0f;
            _skipGroup.interactable   = false;
            _skipGroup.blocksRaycasts = false;

            // "SKIP" label using Unity's built-in font.
            var labelGO = new GameObject("Label", typeof(Text));
            labelGO.transform.SetParent(skipGO.transform, false);
            var label = labelGO.GetComponent<Text>();
            label.text      = "SKIP ▶";
            label.alignment = TextAnchor.MiddleCenter;
            label.color     = Color.white;
            label.fontSize  = 34;
            label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            StretchFull(label.rectTransform);

            // Full-screen loading image — added LAST so it draws on top of the video AND the Skip
            // button (uGUI renders in hierarchy order; the last child is topmost). Hidden by default
            // so non-loading cutscenes never show it.
            var loadingGO = new GameObject("LoadingImage",
                typeof(Image), typeof(AspectRatioFitter), typeof(CanvasGroup));
            loadingGO.transform.SetParent(canvasGO.transform, false);

            _loadingImage = loadingGO.GetComponent<Image>();
            _loadingImage.color = Color.white;
            StretchFull(_loadingImage.rectTransform);

            _loadingFitter = loadingGO.GetComponent<AspectRatioFitter>();
            _loadingFitter.aspectMode  = AspectRatioFitter.AspectMode.FitInParent;
            _loadingFitter.aspectRatio = LoadingAspect;

            _loadingGroup = loadingGO.GetComponent<CanvasGroup>();
            _loadingGroup.alpha          = 0f;
            _loadingGroup.blocksRaycasts = false;
            _loadingGroup.interactable   = false;

            loadingGO.SetActive(false);
        }

        private void BuildVideoPlayer()
        {
            // RenderTexture the VideoPlayer draws into and the RawImage samples.
            _renderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32)
            {
                name = "CutsceneRT"
            };
            _renderTexture.Create();
            _rawImage.texture = _renderTexture;

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.ignoreListenerPause = true; // audible even if AudioListener.pause is set
            _audioSource.volume = VideoVolume;

            _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            _videoPlayer.playOnAwake        = false;
            _videoPlayer.waitForFirstFrame  = true;
            _videoPlayer.isLooping          = false;
            _videoPlayer.renderMode         = VideoRenderMode.RenderTexture;
            _videoPlayer.targetTexture      = _renderTexture;
            _videoPlayer.audioOutputMode    = VideoAudioOutputMode.AudioSource;
            _videoPlayer.SetTargetAudioSource(0, _audioSource);
            // Video runs on its own clock, independent of Time.timeScale.
            _videoPlayer.playbackSpeed      = 1f;
            _videoPlayer.skipOnDrop         = true;

            _videoPlayer.loopPointReached += OnLoopPointReached;
            _videoPlayer.errorReceived    += OnVideoError;
        }

        private void ShowOverlay()
        {
            if (_canvas != null) _canvas.gameObject.SetActive(true);
        }

        private void HideOverlay()
        {
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        /// <summary>Loads the loading sprite, shows the LoadingImage, and sets it fully opaque.</summary>
        private void ShowLoadingScreen()
        {
            if (_loadingImage == null || _loadingGroup == null) return;

            if (_loadingImage.sprite == null)
            {
                Sprite loaded = Resources.Load<Sprite>(LoadingSpriteName);
                if (loaded == null)
                    Debug.LogWarning("[CutscenePlayer] loading_screen sprite not found in Resources — showing blank white loading screen.");
                _loadingImage.sprite = loaded; // null is acceptable — degrades to a white full-screen rect, no crash
            }

            _loadingImage.gameObject.SetActive(true);
            _loadingGroup.alpha = 1f;

            // Record when the art actually became visible so the min-hold measures true on-screen time
            // (covers URL-resolve + Prepare), not just the Prepare polling loop.
            _loadingShownUnscaledTime = Time.unscaledTime;
        }

        /// <summary>Hides and resets the LoadingImage so a later cutscene never flashes the art.</summary>
        private void HideLoadingScreen()
        {
            if (_loadingGroup != null) _loadingGroup.alpha = 0f;
            if (_loadingImage != null)
            {
                _loadingImage.gameObject.SetActive(false);
                _loadingImage.sprite = null;
            }
        }

        private void SetSkipInteractable(bool interactable, bool skippable)
        {
            if (_skipGroup == null) return;

            if (!skippable)
            {
                _skipGroup.alpha          = 0f;
                _skipGroup.interactable   = false;
                _skipGroup.blocksRaycasts = false;
                return;
            }

            _skipGroup.alpha          = interactable ? 1f : 0f;
            _skipGroup.interactable   = interactable;
            _skipGroup.blocksRaycasts = interactable;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.offsetMin        = Vector2.zero;
            rt.offsetMax        = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
