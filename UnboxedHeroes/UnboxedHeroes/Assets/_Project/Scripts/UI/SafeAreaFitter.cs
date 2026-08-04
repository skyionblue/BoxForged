using UnityEngine;

namespace Boxhead.UI
{
    /// <summary>
    /// Adjusts a RectTransform so its edges respect Screen.safeArea.
    /// Attach to the root RectTransform of any Canvas that contains HUD or menu
    /// content that must stay clear of notches, home indicators, and punch-hole
    /// cameras on iOS and Android.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect          _lastSafeArea;
        private Vector2       _lastScreenSize;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            Apply();
        }

        // Re-apply when the screen rotates or the safe area changes (split-screen, etc.)
        private void Update()
        {
            var sa = Screen.safeArea;
            var ss = new Vector2(Screen.width, Screen.height);
            if (sa != _lastSafeArea || ss != _lastScreenSize)
                Apply();
        }

        private void Apply()
        {
            _lastSafeArea   = Screen.safeArea;
            _lastScreenSize = new Vector2(Screen.width, Screen.height);

            var sa = Screen.safeArea;
            float w = Screen.width;
            float h = Screen.height;

            // Convert safe area pixel rect to anchor fractions [0,1]
            Vector2 anchorMin = new Vector2(sa.x / w,       sa.y / h);
            Vector2 anchorMax = new Vector2((sa.x + sa.width) / w, (sa.y + sa.height) / h);

            _rt.anchorMin = anchorMin;
            _rt.anchorMax = anchorMax;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}
