// Assets/_Project/Scripts/UI/HUDCameraInjector.cs
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Boxhead.UI
{
    /// <summary>
    /// Drop-in component for pfb_hud_v2. On Awake it:
    ///   1. Finds Camera.main and adds _hudCamera to its URP overlay stack.
    ///   2. Creates an EventSystem + InputSystemUIInputModule if none exists.
    /// On Destroy it removes _hudCamera from the stack so the prefab cleans up
    /// after itself when the scene unloads.
    ///
    /// Place this on the root of pfb_hud_v2 and wire _hudCamera in the Inspector.
    /// No other per-scene setup is required.
    /// </summary>
    [DisallowMultipleComponent]
    public class HUDCameraInjector : MonoBehaviour
    {
        [SerializeField] private Camera _hudCamera;

        private Camera _mainCamera;

        private void Awake()
        {
            EnsureEventSystem();
            InjectHUDCamera();
        }

        private void OnDestroy()
        {
            RemoveHUDCamera();
        }

        // ── Camera stack ──────────────────────────────────────────────────

        private void InjectHUDCamera()
        {
            if (_hudCamera == null)
            {
                Debug.LogWarning("[HUDCameraInjector] _hudCamera is not assigned. HUD will not render.");
                return;
            }

            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogWarning("[HUDCameraInjector] No Camera.main found. Retrying next frame.");
                // Defer one frame — camera may not be active yet on the first Awake pass.
                StartCoroutine(RetryInject());
                return;
            }

            AddToStack(_mainCamera);
        }

        private System.Collections.IEnumerator RetryInject()
        {
            yield return null;
            _mainCamera = Camera.main;
            if (_mainCamera != null)
                AddToStack(_mainCamera);
            else
                Debug.LogError("[HUDCameraInjector] Camera.main still not found. HUD camera not injected.");
        }

        private void AddToStack(Camera main)
        {
            var urp = main.GetComponent<UniversalAdditionalCameraData>();
            if (urp == null)
            {
                Debug.LogWarning("[HUDCameraInjector] Camera.main has no UniversalAdditionalCameraData. Is URP active?");
                return;
            }

            if (!urp.cameraStack.Contains(_hudCamera))
                urp.cameraStack.Add(_hudCamera);
        }

        private void RemoveHUDCamera()
        {
            if (_hudCamera == null || _mainCamera == null) return;
            var urp = _mainCamera.GetComponent<UniversalAdditionalCameraData>();
            urp?.cameraStack.Remove(_hudCamera);
        }

        // ── EventSystem ───────────────────────────────────────────────────

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;

            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();
        }
    }
}
