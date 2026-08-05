using UnityEngine;

namespace Boxhead.UI
{
    /// <summary>
    /// Rotates a UI RectTransform continuously around its Z axis.
    /// Attach to weapon slot icons so equipped weapons appear to spin.
    /// </summary>
    public class SpinIcon : MonoBehaviour
    {
        [SerializeField] private float _degreesPerSecond = 90f;

        private RectTransform _rt;

        private void Awake()
        {
            TryGetComponent(out _rt);
        }

        private void Update()
        {
            if (_rt == null) return;
            _rt.Rotate(0f, 0f, _degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
