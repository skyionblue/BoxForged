using UnityEngine;
using UnityEngine.UI;
using Boxhead.Player;
using Boxhead.Core;
using Boxhead.Enemy;
using Boxhead.Systems;

namespace Boxhead.UI
{
    /// <summary>
    /// Run-start panel presented before every run. Replaces StyleSelectUI.
    /// Lets the player choose: Gender (Male/Female), Fighting Style (Ninja/Cowboy),
    /// and Difficulty (Easy/Medium/Hard) before the session begins.
    ///
    /// GameManager calls Show() in Start(); OnStartClicked() applies all selections and
    /// calls Hide(). The panel starts inactive in the scene hierarchy.
    /// </summary>
    public class RunStartUI : MonoBehaviour
    {
        // ── Serialized data ──────────────────────────────────────────────────────
        [Header("Fighting Styles")]
        [SerializeField] private FightingStyleData _ninjaStyle;
        [SerializeField] private FightingStyleData _cowboyStyle;

        [Header("Difficulty Assets")]
        [Tooltip("Index 0 = Easy, 1 = Medium, 2 = Hard")]
        [SerializeField] private DifficultyData[] _difficulties;

        [Header("Gender Buttons")]
        [SerializeField] private Button _btnMale;
        [SerializeField] private Button _btnFemale;

        [Header("Style Buttons")]
        [SerializeField] private Button _btnNinja;
        [SerializeField] private Button _btnCowboy;

        [Header("Difficulty Buttons")]
        [SerializeField] private Button _btnEasy;
        [SerializeField] private Button _btnMedium;
        [SerializeField] private Button _btnHard;

        [Header("Start Button")]
        [SerializeField] private Button _btnStart;

        // ── Selection state ──────────────────────────────────────────────────────
        // 0 = Male, 1 = Female
        // Default to Female (1) — Male Ninja has broken Mixamo clips until re-downloaded
        private int _selectedGender = 1;
        // 0 = Ninja, 1 = Cowboy
        private int _selectedStyle = 0;
        // 0 = Easy, 1 = Medium, 2 = Hard
        private int _selectedDifficulty = 1;

        // ── Visual colours ───────────────────────────────────────────────────────
        private static readonly Color _selectedColor = new Color(0.9f, 0.7f, 0.2f, 1f);  // gold
        private static readonly Color _defaultColor  = new Color(0.3f, 0.3f, 0.3f, 1f);  // dark grey

        // ── Cached button Image components ───────────────────────────────────────
        private Image _imgMale;
        private Image _imgFemale;
        private Image _imgNinja;
        private Image _imgCowboy;
        private Image _imgEasy;
        private Image _imgMedium;
        private Image _imgHard;

        // ── Cached runtime references ────────────────────────────────────────────
        private CombatController _playerCombat;
        private UnityEngine.InputSystem.PlayerInput _playerInput;
        private BoxSystem _boxSystem;
        private Animator  _cachedPlayerAnimator;

        // Character model child names on the Player prefab.
        private Transform _modelNinjaMale;
        private Transform _modelNinjaFemale;
        private Transform _modelCowboy;
        private Transform _modelCowgirl;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                if (!playerObj.TryGetComponent<CombatController>(out _playerCombat))
                    Debug.LogWarning("[RunStartUI] CombatController not found on Player.", this);

                if (!playerObj.TryGetComponent<UnityEngine.InputSystem.PlayerInput>(out _playerInput))
                    Debug.LogWarning("[RunStartUI] PlayerInput not found on Player.", this);

                if (!playerObj.TryGetComponent<BoxSystem>(out _boxSystem))
                    Debug.LogWarning("[RunStartUI] BoxSystem not found on Player.", this);

                _modelNinjaMale   = playerObj.transform.Find("NinjaMale_CharacterModel");
                _modelNinjaFemale = playerObj.transform.Find("NinjaFemale_CharacterModel");
                _modelCowboy      = playerObj.transform.Find("Cowboy_CharacterModel");
                _modelCowgirl     = playerObj.transform.Find("Cowgirl_CharacterModel");
            }
            else
            {
                Debug.LogWarning("[RunStartUI] No GameObject with tag 'Player' found in scene.", this);
            }

            // Cache button Image components once rather than calling GetComponent every highlight refresh.
            if (_btnMale   != null) _btnMale.TryGetComponent<Image>(out _imgMale);
            if (_btnFemale != null) _btnFemale.TryGetComponent<Image>(out _imgFemale);
            if (_btnNinja  != null) _btnNinja.TryGetComponent<Image>(out _imgNinja);
            if (_btnCowboy != null) _btnCowboy.TryGetComponent<Image>(out _imgCowboy);
            if (_btnEasy   != null) _btnEasy.TryGetComponent<Image>(out _imgEasy);
            if (_btnMedium != null) _btnMedium.TryGetComponent<Image>(out _imgMedium);
            if (_btnHard   != null) _btnHard.TryGetComponent<Image>(out _imgHard);
        }

        private void Start()
        {
            WireButtons();
            RefreshAllHighlights();
        }

        // ── Public API (called by GameManager) ───────────────────────────────────

        public void Show()
        {
            // Awake() may have run before pfb_player was instantiated (scene object order).
            // Re-find all player references if any are missing so ApplyGender/Style work correctly.
            if (_modelNinjaMale == null)
            {
                var p = GameObject.FindWithTag("Player");
                Debug.LogWarning($"[RunStartUI] Show() lazy re-find — player={(p != null ? p.name : "NULL")}");
                if (p != null)
                {
                    if (_boxSystem    == null) p.TryGetComponent(out _boxSystem);
                    if (_playerCombat == null) p.TryGetComponent(out _playerCombat);
                    if (_playerInput  == null) p.TryGetComponent<UnityEngine.InputSystem.PlayerInput>(out _playerInput);
                    _modelNinjaMale   = p.transform.Find("NinjaMale_CharacterModel");
                    _modelNinjaFemale = p.transform.Find("NinjaFemale_CharacterModel");
                    _modelCowboy      = p.transform.Find("Cowboy_CharacterModel");
                    _modelCowgirl     = p.transform.Find("Cowgirl_CharacterModel");
                    if (_cachedPlayerAnimator == null)
                        _cachedPlayerAnimator = p.GetComponentInChildren<Animator>();
                    Debug.LogWarning($"[RunStartUI] Models after re-find — NinjaMale={(_modelNinjaMale != null ? "found" : "NULL")} NinjaFemale={(_modelNinjaFemale != null ? "found" : "NULL")}");
                }
            }

            // Scene transition: silently restore prior selection rather than showing the picker.
            var ps = ProgressionSystem.Instance;
            Debug.LogWarning($"[RunStartUI] Show() — PS={(ps != null ? "found" : "NULL")} HasSelection={(ps != null ? ps.HasRunSelection.ToString() : "N/A")}");

            // Boss arenas and mid-run scenes are never valid run-start screens.
            // If we land here without a prior selection (e.g. dev entering the boss arena
            // directly from the editor), silently apply defaults so timeScale is never set
            // to 0 and the boss intro / DefeatSequence can run unobstructed.
            bool atRunStartRoom = Boxhead.Core.GameManager.ZoneStartScene.ContainsValue(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

            if ((ps != null && ps.HasRunSelection) || !atRunStartRoom)
            {
                if (ps != null && ps.HasRunSelection)
                {
                    // Restore the saved selection and re-apply it to the character model.
                    // This is the only code path that should ever call ApplyGender/Style/Difficulty
                    // during a silent restore — it only runs when we have a confirmed prior selection.
                    _selectedGender     = ps.RunGender;
                    _selectedStyle      = ps.RunStyle;
                    _selectedDifficulty = ps.RunDifficulty;

                    Debug.LogWarning($"[RunStartUI] Silent restore — gender={_selectedGender} style={_selectedStyle} NinjaMaleRef={(_modelNinjaMale != null ? "found" : "NULL")}");
                    ApplyGender();
                    ApplyStyle();
                    ApplyDifficulty();
                    if (_boxSystem != null)
                    {
                        _boxSystem.ForceApplyBox(ps.RunBoxIndex);
                        _boxSystem.NotifyModelChanged();
                    }
                    else
                    {
                        var rf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                        var playerObj = _playerCombat?.gameObject;
                        if (playerObj != null)
                        {
                            playerObj.GetComponent<CombatController>()
                                     ?.GetType().GetMethod("RefreshAnimator", rf)
                                     ?.Invoke(playerObj.GetComponent<CombatController>(), null);
                            playerObj.GetComponent<PlayerController>()
                                     ?.GetType().GetMethod("RefreshAnimator", rf)
                                     ?.Invoke(playerObj.GetComponent<PlayerController>(), null);
                            playerObj.GetComponent<Boxhead.Player.WeaponHolder>()
                                     ?.GetType().GetMethod("OnModelChanged", rf)
                                     ?.Invoke(playerObj.GetComponent<Boxhead.Player.WeaponHolder>(), null);
                        }
                    }
                }
                else
                {
                    // Not at a run-start room and no prior selection (e.g. dev loaded boss arena
                    // directly). Do NOT apply any gender/style defaults — that would overwrite
                    // whichever model is currently active with the Female-Ninja class-level default.
                    // Just enable player input and let the scene run as-is.
                    Debug.LogWarning("[RunStartUI] Silent pass-through — no saved selection and not at run-start room. Skipping character apply to avoid clobbering active model.");
                }
                if (_playerInput != null) _playerInput.enabled = true;
                return;
            }

            gameObject.SetActive(true);
            if (_playerInput != null) _playerInput.enabled = false;
            if (_cachedPlayerAnimator != null) _cachedPlayerAnimator.speed = 0f;
            // Pause the whole background (enemies, spawners, boss intro) while the player
            // is choosing sex / style / difficulty. Restored in Hide(). UI buttons still work
            // at timeScale 0 since the EventSystem is not time-gated.
            Time.timeScale = 0f;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[RunStartUI] Show — player input disabled");
#endif
        }

        public void Hide()
        {
            // Un-pause the background (see Show()).
            Time.timeScale = 1f;
            if (_playerInput != null) _playerInput.enabled = true;
            if (_cachedPlayerAnimator != null) _cachedPlayerAnimator.speed = 1f;
            gameObject.SetActive(false);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[RunStartUI] Hide — player input re-enabled");
#endif
        }

        // ── Button handlers (public so Inspector persistent listeners can reach them) ──

        public void OnGenderSelected(int index)
        {
            _selectedGender = index;
            RefreshGenderHighlights();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[RunStartUI] Gender selected: {(index == 0 ? "Male" : "Female")}");
#endif
        }

        public void OnStyleSelected(int index)
        {
            _selectedStyle = index;
            RefreshStyleHighlights();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[RunStartUI] Style selected: {(index == 0 ? "Ninja" : "Cowboy")}");
#endif
        }

        public void OnDifficultySelected(int index)
        {
            _selectedDifficulty = index;
            RefreshDifficultyHighlights();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[RunStartUI] Difficulty selected: {index}");
#endif
        }

        public void OnStartClicked()
        {
            ApplyGender();
            ApplyStyle();
            ApplyDifficulty();
            // NotifyModelChanged refreshes cached Animator references in CombatController
            // and PlayerController. If BoxSystem is missing (no box swapping in V3),
            // call the private RefreshAnimator method directly via reflection.
            if (_boxSystem != null)
            {
                _boxSystem.NotifyModelChanged();
            }
            else
            {
                var rf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var playerObj = _playerCombat?.gameObject;
                if (playerObj != null)
                {
                    playerObj.GetComponent<CombatController>()
                             ?.GetType().GetMethod("RefreshAnimator", rf)
                             ?.Invoke(playerObj.GetComponent<CombatController>(), null);
                    playerObj.GetComponent<PlayerController>()
                             ?.GetType().GetMethod("RefreshAnimator", rf)
                             ?.Invoke(playerObj.GetComponent<PlayerController>(), null);
                    // WeaponHolder also caches handBone from the active character model.
                    // If not refreshed after gender swap, weapons attach to the inactive model's hand.
                    playerObj.GetComponent<Boxhead.Player.WeaponHolder>()
                             ?.GetType().GetMethod("OnModelChanged", rf)
                             ?.Invoke(playerObj.GetComponent<Boxhead.Player.WeaponHolder>(), null);
                }
            }
            ProgressionSystem.Instance?.SetRunSelection(_selectedGender, _selectedStyle, _boxSystem?.CurrentBoxIndex ?? 0, _selectedDifficulty);
            // Fresh character pick = brand-new run: wipe any carried-over cardboard/weapons.
            ProgressionSystem.Instance?.ClearRunLoadout();

            // Play a once-per-character showcase cutscene before the run begins, then Hide().
            // The video plays on its own clock (unaffected by Show()'s timeScale = 0), and Hide()
            // runs on the cutscene callback so the run only starts after the showcase completes.
            // 0 = Ninja → ninja_skills; 1 = Cowboy → cowboy_ninja_skills showcase.
            string showcaseFile = _selectedStyle == 0
                ? Boxhead.Core.CutsceneCatalog.NinjaShowcase
                : Boxhead.Core.CutsceneCatalog.CowboyNinjaShowcase;
            string showcaseKey = _selectedStyle == 0
                ? Boxhead.Core.CutsceneCatalog.KeyNinjaShowcase
                : Boxhead.Core.CutsceneCatalog.KeyCowboyShowcase;

            // Only play the character showcase at a genuine run-start room (a zone start scene).
            // The picker can still appear mid-run — e.g. loading the boss arena directly for
            // testing with no saved selection — but the showcase must NOT play before a boss fight.
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool atRunStartRoom = Boxhead.Core.GameManager.ZoneStartScene.ContainsValue(sceneName);

            var cutscene = Boxhead.Core.CutscenePlayer.Instance;
            if (atRunStartRoom && cutscene != null && !Boxhead.Core.CutsceneFlags.HasSeen(showcaseKey))
            {
                Boxhead.Core.CutsceneFlags.MarkSeen(showcaseKey);
                cutscene.Play(showcaseFile, onFinished: Hide, skippable: true);
            }
            else
            {
                Hide();
            }
        }

        // ── Apply selections ─────────────────────────────────────────────────────

        private void ApplyGender()
        {
            // Determine target model based on gender + style combination.
            Transform target = ResolveCharacterModel();

            SetModelActive(_modelNinjaMale,   target == _modelNinjaMale);
            SetModelActive(_modelNinjaFemale, target == _modelNinjaFemale);
            SetModelActive(_modelCowboy,      target == _modelCowboy);
            SetModelActive(_modelCowgirl,     target == _modelCowgirl);

            Debug.LogWarning($"[RunStartUI] ApplyGender — target={(target != null ? target.name : "NULL")} NinjaMaleRef={(_modelNinjaMale != null ? "found" : "NULL")}");
        }

        private void ApplyStyle()
        {
            FightingStyleData style = _selectedStyle == 0 ? _ninjaStyle : _cowboyStyle;
            if (style == null)
            {
                Debug.LogWarning("[RunStartUI] FightingStyleData is null — cannot apply style.", this);
                return;
            }
            _playerCombat?.SetFightingStyle(style);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[RunStartUI] Style applied: {style.StyleName}");
#endif
        }

        private void ApplyDifficulty()
        {
            if (_difficulties == null || _difficulties.Length == 0)
            {
                Debug.LogWarning("[RunStartUI] No DifficultyData assets assigned.", this);
                return;
            }

            int clampedIndex = Mathf.Clamp(_selectedDifficulty, 0, _difficulties.Length - 1);
            DifficultyData chosen = _difficulties[clampedIndex];
            if (chosen == null)
            {
                Debug.LogWarning($"[RunStartUI] DifficultyData at index {clampedIndex} is null.", this);
                return;
            }

            DifficultyManager.Instance?.Set(chosen);

            // Push spawn counts to every EnemySpawner in the scene.
            // FindObjectsByType is called once here at run-start (not per frame) — allocation is acceptable.
            var spawners = Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < spawners.Length; i++)
            {
                int maxActive;
                int maxTotal;

                switch (spawners[i].Type)
                {
                    case SpawnerType.Roller:
                        maxActive = chosen.RollerSpawnerActive;
                        maxTotal  = chosen.RollerSpawnerMax;
                        break;
                    case SpawnerType.Sentinel:
                        maxActive = chosen.SentinelSpawnerActive;
                        maxTotal  = chosen.SentinelSpawnerMax;
                        break;
                    default:
                        maxActive = chosen.GruntSpawnerActive;
                        maxTotal  = chosen.GruntSpawnerMax;
                        break;
                }

                spawners[i].ApplyDifficulty(maxActive, maxTotal);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[RunStartUI] Difficulty applied: {chosen.DifficultyName} to {spawners.Length} spawners");
#endif
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private Transform ResolveCharacterModel()
        {
            // Male=0, Female=1 / Ninja=0, Cowboy=1
            bool isFemale = _selectedGender == 1;
            bool isCowboy = _selectedStyle  == 1;

            if (!isFemale && !isCowboy) return _modelNinjaMale;
            if ( isFemale && !isCowboy) return _modelNinjaFemale;
            if (!isFemale &&  isCowboy) return _modelCowboy;
            return _modelCowgirl;
        }

        private static void SetModelActive(Transform model, bool active)
        {
            if (model != null) model.gameObject.SetActive(active);
        }

        // ── Button wiring ────────────────────────────────────────────────────────

        private void WireButtons()
        {
            if (_btnMale   != null) { _btnMale.onClick.RemoveAllListeners();   _btnMale.onClick.AddListener(() => OnGenderSelected(0)); }
            if (_btnFemale != null) { _btnFemale.onClick.RemoveAllListeners(); _btnFemale.onClick.AddListener(() => OnGenderSelected(1)); }

            if (_btnNinja  != null) { _btnNinja.onClick.RemoveAllListeners();  _btnNinja.onClick.AddListener(() => OnStyleSelected(0)); }
            if (_btnCowboy != null) { _btnCowboy.onClick.RemoveAllListeners(); _btnCowboy.onClick.AddListener(() => OnStyleSelected(1)); }

            if (_btnEasy   != null) { _btnEasy.onClick.RemoveAllListeners();   _btnEasy.onClick.AddListener(() => OnDifficultySelected(0)); }
            if (_btnMedium != null) { _btnMedium.onClick.RemoveAllListeners(); _btnMedium.onClick.AddListener(() => OnDifficultySelected(1)); }
            if (_btnHard   != null) { _btnHard.onClick.RemoveAllListeners();   _btnHard.onClick.AddListener(() => OnDifficultySelected(2)); }

            if (_btnStart  != null) { _btnStart.onClick.RemoveAllListeners();  _btnStart.onClick.AddListener(OnStartClicked); }
        }

        // ── Visual highlight helpers ──────────────────────────────────────────────

        private void RefreshAllHighlights()
        {
            RefreshGenderHighlights();
            RefreshStyleHighlights();
            RefreshDifficultyHighlights();
        }

        private void RefreshGenderHighlights()
        {
            SetHighlight(_imgMale,   _selectedGender == 0);
            SetHighlight(_imgFemale, _selectedGender == 1);
        }

        private void RefreshStyleHighlights()
        {
            SetHighlight(_imgNinja,  _selectedStyle == 0);
            SetHighlight(_imgCowboy, _selectedStyle == 1);
        }

        private void RefreshDifficultyHighlights()
        {
            SetHighlight(_imgEasy,   _selectedDifficulty == 0);
            SetHighlight(_imgMedium, _selectedDifficulty == 1);
            SetHighlight(_imgHard,   _selectedDifficulty == 2);
        }

        private static void SetHighlight(Image img, bool isSelected)
        {
            if (img != null) img.color = isSelected ? _selectedColor : _defaultColor;
        }
    }
}
