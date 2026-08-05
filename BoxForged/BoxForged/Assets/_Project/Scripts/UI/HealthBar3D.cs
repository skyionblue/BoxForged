// Assets/_Project/Scripts/UI/HealthBar3D.cs
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.UI
{
    /// <summary>
    /// Drives a 3D Quad child of HUD_HealthbarFrame as a health fill bar.
    /// Subscribes to PlayerStats.OnHealthChanged — zero per-frame cost at steady state.
    /// The fill quad's pivot is at its centre, so we shift localPosition.x to keep the
    /// left edge anchored as health decreases (bar shrinks from the right).
    /// </summary>
    public class HealthBar3D : MonoBehaviour
    {
        [SerializeField] private Transform _fillTransform;

        /// <summary>Full-health width in the fill quad's local X units.</summary>
        [SerializeField] private float _fullWidth = 0.85f;

        private PlayerStats _playerStats;
        private Vector3 _baseScale;
        private float _baseLocalX;

        private void Awake()
        {
            if (_fillTransform != null)
            {
                _baseScale = _fillTransform.localScale;
                _baseLocalX = _fillTransform.localPosition.x;
            }
        }

        private void Start()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null)
            {
                Debug.LogWarning("[HealthBar3D] No GameObject tagged 'Player' found. Bar will not update.");
                return;
            }

            if (!playerGO.TryGetComponent(out _playerStats))
            {
                Debug.LogWarning("[HealthBar3D] Player has no PlayerStats component. Bar will not update.");
                return;
            }

            _playerStats.OnHealthChanged += OnHealthChanged;
            // Sync immediately so the bar reflects current health on first frame.
            OnHealthChanged(_playerStats.CurrentHealth, _playerStats.MaxHealth);
        }

        private void OnDestroy()
        {
            if (_playerStats != null)
                _playerStats.OnHealthChanged -= OnHealthChanged;
        }

        // ── Health update ─────────────────────────────────────────────────────

        private void OnHealthChanged(int current, int max)
        {
            if (_fillTransform == null || max <= 0) return;

            float t = Mathf.Clamp01((float)current / max);
            float newWidth = _fullWidth * t;

            // Scale the quad's X so it represents the current health fraction.
            Vector3 scale = _baseScale;
            scale.x = newWidth;
            _fillTransform.localScale = scale;

            // Shift the pivot so the bar always shrinks from its right edge.
            // At full health: localX = _baseLocalX (centred in frame).
            // At zero health: localX = _baseLocalX - _fullWidth / 2 (off to the left).
            Vector3 pos = _fillTransform.localPosition;
            pos.x = _baseLocalX + (newWidth / 2f) - (_fullWidth / 2f);
            _fillTransform.localPosition = pos;
        }
    }
}
