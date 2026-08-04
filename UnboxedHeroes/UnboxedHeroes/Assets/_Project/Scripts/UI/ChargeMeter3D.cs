// Assets/_Project/Scripts/UI/ChargeMeter3D.cs
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.UI
{
    /// <summary>
    /// Drives a 3D fill bar (a child cube Transform) based on CombatController.SpecialAbilityProgress.
    /// The fill cube's pivot is at its centre, so localPosition.x is shifted to keep the left edge
    /// anchored as the charge decreases — identical pivot math to HealthBar3D.
    ///
    /// Because CombatController exposes SpecialAbilityProgress as a property (no change event),
    /// this script polls in Update. The poll is a single float read + two Vector3 writes per frame
    /// — well within mobile GC budget.
    /// </summary>
    public class ChargeMeter3D : MonoBehaviour
    {
        [SerializeField] private Transform _fillTransform;

        /// <summary>Full-charge width in the fill cube's local X units.</summary>
        [SerializeField] private float _fullWidth = 1.7f;

        private CombatController _combat;
        private Vector3 _baseScale;
        private float _baseLocalX;

        private void Awake()
        {
            if (_fillTransform != null)
            {
                _baseScale  = _fillTransform.localScale;
                _baseLocalX = _fillTransform.localPosition.x;
            }
        }

        private void Start()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
                playerGO.TryGetComponent(out _combat);

            if (_combat == null)
                Debug.LogWarning("[ChargeMeter3D] No CombatController found on Player. Bar will not update.");
        }

        private void Update()
        {
            if (_fillTransform == null || _combat == null) return;
            SetFill(_combat.SpecialAbilityProgress);
        }

        // ── Fill helper ───────────────────────────────────────────────────────

        private void SetFill(float t)
        {
            t = Mathf.Clamp01(t);
            float newWidth = _fullWidth * t;

            // Scale the cube's X to represent the current charge fraction.
            Vector3 scale = _baseScale;
            scale.x = newWidth;
            _fillTransform.localScale = scale;

            // Shift the pivot so the bar always drains from the right edge.
            // At full charge:  localX = _baseLocalX (centred in frame).
            // At zero charge:  localX = _baseLocalX - _fullWidth / 2 (off to the left).
            Vector3 pos = _fillTransform.localPosition;
            pos.x = _baseLocalX + (newWidth / 2f) - (_fullWidth / 2f);
            _fillTransform.localPosition = pos;
        }
    }
}
