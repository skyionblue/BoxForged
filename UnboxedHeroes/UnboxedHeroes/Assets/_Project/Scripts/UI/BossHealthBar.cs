using System;
using UnityEngine;
using UnityEngine.UI;
using Boxhead.Enemy;

namespace Boxhead.UI
{
    /// <summary>
    /// Screen-space boss health bar. Shown at the top of the viewport during the boss fight.
    /// Drives fill directly via Image.fillAmount — no Slider required.
    /// Call Activate() (via PermitPulperBossIntro) to reveal the bar when combat begins.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private Color _phase1Color = new Color(0.8f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color _phase2Color = new Color(0.7f, 0.1f, 0.9f, 1f);

        private EnemyStats _stats;
        private Canvas     _canvas;
        private bool       _inPhase2;

        private Action<int, int> _onHealthChangedDelegate;
        private Action           _onDeathDelegate;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _onHealthChangedDelegate = OnHealthChanged;
            _onDeathDelegate         = OnBossDied;
            _canvas.enabled = false;
        }

        private void Start()
        {
            var bossAI = UnityEngine.Object.FindAnyObjectByType<PermitPulperBossAI>();
            if (bossAI == null)
            {
                Debug.LogWarning("[BossHealthBar] No PermitPulperBossAI found in scene.", this);
                return;
            }

            if (!bossAI.TryGetComponent<EnemyStats>(out _stats))
            {
                Debug.LogError("[BossHealthBar] PermitPulperBossAI missing EnemyStats.", this);
                return;
            }

            _stats.OnHealthChanged += _onHealthChangedDelegate;
            _stats.OnDeath         += _onDeathDelegate;

            if (_fillImage == null)
            {
                Debug.LogError("[BossHealthBar] _fillImage not assigned in inspector.", this);
                return;
            }

            // Use Simple type — width is driven by anchorMax.x rather than fillAmount,
            // which is more reliable across Canvas configurations.
            _fillImage.type  = Image.Type.Simple;
            _fillImage.color = _phase1Color;
            SetFill(1f);
        }

        private void OnDestroy()
        {
            if (_stats == null) return;
            _stats.OnHealthChanged -= _onHealthChangedDelegate;
            _stats.OnDeath         -= _onDeathDelegate;
        }

        public void Activate() => _canvas.enabled = true;

        private void OnHealthChanged(int current, int max)
        {
            if (_fillImage == null) return;
            float normalized = max > 0 ? (float)current / max : 0f;
            SetFill(normalized);

            if (!_inPhase2 && current <= max * 0.5f)
            {
                _inPhase2        = true;
                _fillImage.color = _phase2Color;
            }
        }

        // Drives the fill bar by shrinking anchorMax.x — reliable across all Canvas setups.
        private void SetFill(float normalized)
        {
            var rt = _fillImage.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void OnBossDied() => _canvas.enabled = false;
    }
}
