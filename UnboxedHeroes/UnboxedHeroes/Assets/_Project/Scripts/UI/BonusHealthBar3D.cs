using UnityEngine;
using Boxhead.Player;

namespace Boxhead.UI
{
    /// <summary>
    /// Drives a thin 3D cube sitting above the main health bar to show the
    /// Max Health bonus earned from Spark upgrades. Bar grows as the player
    /// purchases more Max Health — empty when no bonus exists.
    ///
    /// References the main HealthFill3D transform directly so it always
    /// stays aligned regardless of where the health bar frame is positioned.
    /// </summary>
    public class BonusHealthBar3D : MonoBehaviour
    {
        [SerializeField] private Transform _fillTransform;

        /// <summary>The main HealthFill3D transform — used to read the correct base X position.</summary>
        [SerializeField] private Transform _mainHealthFill;

        /// <summary>Full-health width — must match HealthBar3D._fullWidth.</summary>
        [SerializeField] private float _fullWidth = 0.85f;

        private PlayerStats _playerStats;
        private Vector3     _baseScale;
        private float       _baseLocalX;
        private Renderer    _renderer;

        private void Awake()
        {
            if (_fillTransform != null)
            {
                _baseScale = _fillTransform.localScale;
                _renderer  = _fillTransform.GetComponent<Renderer>();
                if (_renderer != null) _renderer.enabled = false; // hidden until bonus exists
            }

            // Cache in Awake — before any Start() runs and HealthBar3D begins animating
            // the fill position. Reading it later would give a shifted value.
            _baseLocalX = _mainHealthFill != null
                ? _mainHealthFill.localPosition.x
                : (_fillTransform != null ? _fillTransform.localPosition.x : 0f);
        }

        private void Start()
        {

            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null) return;
            if (!playerGO.TryGetComponent(out _playerStats)) return;

            _playerStats.OnHealthChanged += OnHealthChanged;
            OnHealthChanged(_playerStats.CurrentHealth, _playerStats.MaxHealth);
        }

        private void OnDestroy()
        {
            if (_playerStats != null)
                _playerStats.OnHealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            if (_fillTransform == null || max <= 0) return;

            // Use the cached home X — _mainHealthFill animates as health changes so
            // reading it live would shift the bonus bar in the wrong direction.

            int bonusHP  = _playerStats != null ? _playerStats.CurrentBonusHealth : 0;
            int maxBonus = _playerStats != null ? _playerStats.MaxHealthBonus      : 0;

            if (bonusHP <= 0 || maxBonus <= 0)
            {
                if (_renderer != null) _renderer.enabled = false;
                Vector3 hidden = _baseScale;
                hidden.x = 0f;
                _fillTransform.localScale = hidden;
                if (_playerStats == null)
                    Debug.LogWarning("[BonusHealthBar3D] _playerStats is null — bar will not update.", this);
                return;
            }

            if (_renderer != null) _renderer.enabled = true;
            // Divide by total MaxHealth so the bar GROWS with each upgrade purchase
            // (small stripe at level 1, wider at level 10) and shrinks as hits land.
            float t        = max > 0 ? Mathf.Clamp01((float)bonusHP / max) : 0f;
            float newWidth = _fullWidth * t;

            Vector3 scale = _baseScale;
            scale.x = newWidth;
            _fillTransform.localScale = scale;

            // Left-anchored: same pivot pattern as HealthBar3D
            Vector3 pos = _fillTransform.localPosition;
            pos.x = _baseLocalX + (newWidth / 2f) - (_fullWidth / 2f);
            _fillTransform.localPosition = pos;
        }
    }
}
