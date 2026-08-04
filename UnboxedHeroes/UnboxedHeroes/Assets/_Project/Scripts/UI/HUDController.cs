using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Boxhead.Player;
using Boxhead.Systems;

namespace Boxhead.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Health Segments")]
        [SerializeField] private Image[] healthSegments;
        [SerializeField] private GameObject[] healthXMarks;

        [Header("IP Counter")]
        [SerializeField] private TextMeshProUGUI ipTallyText;
        [SerializeField] private TextMeshProUGUI ipNumberText;
        [SerializeField] private float ipCapFadeDuration = 1.3f;

        private const int IP_TALLY_CAP = 20;

        [Header("Active Box")]
        [SerializeField] private Image activeBoxIcon;
        [SerializeField] private Image activeBoxPortrait;
        [SerializeField] private Button activeBoxButton;

        private static readonly Color SegmentFull  = new Color(0.85f, 0.13f, 0.13f, 1.00f);
        private static readonly Color SegmentEmpty  = new Color(0.22f, 0.04f, 0.04f, 0.70f);

        private PlayerStats _stats;
        private BoxSystem   _boxSystem;
        private int         _ipCount;
        private bool        _ipCapped;
        private Coroutine   _ipFadeCoroutine;

        // Cached to avoid per-kill heap allocation.
        private readonly System.Text.StringBuilder _sb = new System.Text.StringBuilder(64);

        // Awake runs before OnEnable — resolve refs here so OnEnable subscriptions can use them.
        private void Awake()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogError("[HUDController] No GameObject with tag 'Player' found. HUD will not function.");
                return;
            }
            _stats     = playerObj.GetComponent<PlayerStats>();
            _boxSystem = playerObj.GetComponent<BoxSystem>();
        }

        private void Start()
        {
            // Initial UI sync (subscriptions already registered in OnEnable)
            if (_stats != null)
            {
                UpdateHealthSegments(_stats.CurrentHealth, _stats.MaxHealth);
                if (_stats.ActiveBox != null) UpdateBoxIcon(_stats.ActiveBox);
            }

            if (activeBoxButton != null)
                activeBoxButton.interactable = false;

            UpdateIPDisplay();
        }

        private void OnEnable()
        {
            if (_stats != null)
            {
                _stats.OnHealthChanged += UpdateHealthSegments;
                _stats.OnBoxChanged    += UpdateBoxIcon;
            }
            Enemy.EnemyStats.OnAnyEnemyDeath += IncrementIP;
            if (_boxSystem != null)
                _boxSystem.OnSafeZoneChanged += OnSafeZoneChanged;
            if (activeBoxButton != null)
                activeBoxButton.onClick.AddListener(OnActiveBoxClicked);
        }

        private void OnDisable()
        {
            if (_ipFadeCoroutine != null)
            {
                StopCoroutine(_ipFadeCoroutine);
                _ipFadeCoroutine = null;
            }
            if (_stats != null)
            {
                _stats.OnHealthChanged -= UpdateHealthSegments;
                _stats.OnBoxChanged    -= UpdateBoxIcon;
            }
            Enemy.EnemyStats.OnAnyEnemyDeath -= IncrementIP;
            if (_boxSystem != null)
                _boxSystem.OnSafeZoneChanged -= OnSafeZoneChanged;
            if (activeBoxButton != null)
                activeBoxButton.onClick.RemoveListener(OnActiveBoxClicked);
        }

        // ── Health ───────────────────────────────────────────────────────────

        private void UpdateHealthSegments(int current, int max)
        {
            if (healthSegments == null) return;
            int count = healthSegments.Length;
            float hpPerSeg = count > 0 && max > 0 ? (float)max / count : 1f;
            bool hasXMarks = healthXMarks != null;

            for (int i = 0; i < count; i++)
            {
                bool full = current >= (i + 1) * hpPerSeg;

                if (healthSegments[i] != null)
                    healthSegments[i].color = full ? SegmentFull : SegmentEmpty;

                if (hasXMarks && i < healthXMarks.Length && healthXMarks[i] != null)
                    healthXMarks[i].SetActive(!full);
            }
        }

        // ── IP Counter ────────────────────────────────────────────────────────

        private void IncrementIP()
        {
            if (_ipCount < IP_TALLY_CAP) _ipCount++;
            UpdateIPDisplay();
        }

        private void UpdateIPDisplay()
        {
            bool capped = _ipCount >= IP_TALLY_CAP;

            if (!capped)
            {
                if (ipTallyText != null)
                {
                    if (_ipCount == 0)
                    {
                        var c = ipTallyText.color; c.a = 0f; ipTallyText.color = c;
                    }
                    else
                    {
                        var c = ipTallyText.color; c.a = 1f; ipTallyText.color = c;
                        BuildTallyString(_ipCount);
                        ipTallyText.SetText(_sb);
                    }
                }
                if (ipNumberText != null) ipNumberText.gameObject.SetActive(false);
                _ipCapped = false;
                return;
            }

            // Just hit the cap for the first time — animate the transition.
            if (!_ipCapped)
            {
                _ipCapped = true;
                if (ipNumberText != null)
                {
                    var nc = ipNumberText.color; nc.a = 0f; ipNumberText.color = nc;
                    _sb.Clear();
                    _sb.Append(IP_TALLY_CAP);
                    ipNumberText.SetText(_sb);
                    ipNumberText.gameObject.SetActive(true);
                }
                if (_ipFadeCoroutine != null) StopCoroutine(_ipFadeCoroutine);
                _ipFadeCoroutine = StartCoroutine(FadeToCapState());
            }
        }

        private IEnumerator FadeToCapState()
        {
            float elapsed = 0f;
            while (elapsed < ipCapFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / ipCapFadeDuration);

                if (ipTallyText != null)
                {
                    var c = ipTallyText.color;
                    c.a = Mathf.Lerp(1f, 0.04f, t);
                    ipTallyText.color = c;
                }
                if (ipNumberText != null)
                {
                    var c = ipNumberText.color;
                    c.a = Mathf.Lerp(0f, 1f, t);
                    ipNumberText.color = c;
                }
                yield return null;
            }
            _ipFadeCoroutine = null;
        }

        // Each completed group of 5: 4 pipes with a same-height / overlaid in the centre.
        // <space=-11px> moves cursor back to the middle of the 22px-wide group so the slash
        // sits centred over the pipes. Fills _sb — caller passes _sb to SetText (zero-alloc).
        private void BuildTallyString(int count)
        {
            _sb.Clear();
            int groups    = count / 5;
            int remainder = count % 5;
            for (int g = 0; g < groups; g++)
            {
                if (_sb.Length > 0) _sb.Append(' ');
                _sb.Append("||||<space=-11px>/");
            }
            if (groups > 0 && remainder > 0) _sb.Append(' ');
            for (int r = 0; r < remainder; r++) _sb.Append('|');
        }

        // ── Active Box ────────────────────────────────────────────────────────

        private void UpdateBoxIcon(BoxData box)
        {
            if (box == null) return;
            if (activeBoxIcon != null) activeBoxIcon.color = box.primaryColor;
            if (activeBoxPortrait != null && box.portrait != null)
                activeBoxPortrait.sprite = box.portrait;
        }

        // ── Active Box Button ─────────────────────────────────────────────────

        private void OnSafeZoneChanged(bool inZone)
        {
            if (activeBoxButton != null)
                activeBoxButton.interactable = inZone;
        }

        private void OnActiveBoxClicked()
        {
            _boxSystem?.TrySwitchToNext();
        }
    }
}
