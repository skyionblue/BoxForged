using UnityEngine;

namespace Boxhead.UI
{
    /// <summary>
    /// Anchors a 3D HUD element to a fixed viewport-relative position so it
    /// appears in the correct location on any screen size or aspect ratio.
    ///
    /// Works by converting a normalised viewport coordinate (0–1, 0–1) into
    /// a world position via the HUD camera each time the screen dimensions
    /// change — handles device rotation, foldable inner/outer displays, and
    /// resolution differences between iOS and Android automatically.
    /// </summary>
    [ExecuteAlways]
    public class HUD3DPositioner : MonoBehaviour
    {
        [Tooltip("HUD camera that renders this element. Auto-found by name if left empty.")]
        [SerializeField] private Camera _hudCamera;

        [Tooltip("Normalised viewport position (0,0 = bottom-left, 1,1 = top-right).")]
        [SerializeField] private Vector2 _viewportAnchor = new Vector2(0.5f, 0.5f);

        [Tooltip("Distance from the HUD camera to place the element.")]
        [SerializeField] private float _depth = 2.5f;

        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            if (_hudCamera == null)
                _hudCamera = GameObject.Find("HUD_Camera")?.GetComponent<Camera>();
        }

        private void Start() => Reposition();

        private void Update()
        {
            // Only recalculate when screen dimensions change — foldables can
            // switch between inner and outer display at runtime.
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
                Reposition();
        }

        private void Reposition()
        {
            if (_hudCamera == null) return;

            _lastWidth  = Screen.width;
            _lastHeight = Screen.height;

            Vector3 viewportPoint = new Vector3(_viewportAnchor.x, _viewportAnchor.y, _depth);
            transform.position = _hudCamera.ViewportToWorldPoint(viewportPoint);
        }

#if UNITY_EDITOR
        // Show the current viewport position in the inspector as a live readout
        // so designers can tweak _viewportAnchor and see it move immediately.
        private void OnValidate() => Reposition();
#endif
    }
}
