// Assets/_Project/Scripts/UI/OnScreenButtonFix.cs
// Companion component for Unity's built-in OnScreenButton.
//
// Problem: OnScreenButton only handles PointerDown and PointerUp. On Android, if the
// player's finger slides off the button boundary before lifting, PointerUp never fires
// and the virtual gamepad button stays pressed indefinitely ("sticky" button feel).
//
// Fix: this component listens for PointerExit and synthetically forwards it as a
// PointerUp to the OnScreenButton on the same GameObject, which calls SendValueToControl(0)
// and releases the virtual button cleanly.
//
// Setup: Add this component to the same GameObject that has an OnScreenButton component.
// No additional configuration is required.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

namespace Boxhead.UI
{
    [RequireComponent(typeof(OnScreenButton))]
    public class OnScreenButtonFix : MonoBehaviour, IPointerExitHandler
    {
        private OnScreenButton _onScreenButton;

        private void Awake()
        {
            TryGetComponent(out _onScreenButton);
        }

        /// <summary>
        /// Fired when the pointer (finger or mouse) exits the button's RectTransform boundary
        /// without releasing. We treat this identically to a release so the virtual gamepad
        /// button does not stay stuck in the pressed state.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            // Delegate to OnScreenButton's own PointerUp handler.
            // OnScreenButton.OnPointerUp calls SendValueToControl(0f), which sends a
            // button-released state event into the Input System virtual device.
            // Casting to IPointerUpHandler avoids needing a public method on OnScreenButton.
            ((IPointerUpHandler)_onScreenButton).OnPointerUp(eventData);
        }
    }
}
