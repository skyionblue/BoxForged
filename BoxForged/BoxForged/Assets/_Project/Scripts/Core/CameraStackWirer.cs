using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Boxhead.Core
{
    /// <summary>
    /// Adds the scene's HUD_Camera (URP Overlay) to the Main Camera's URP camera stack at
    /// runtime. Required because pfb_camera_rig and pfb_hud_v2 are separate prefabs and
    /// cannot be cross-wired in the prefab asset.
    ///
    /// Attach to: Main Camera child inside pfb_camera_rig.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(UniversalAdditionalCameraData))]
    public sealed class CameraStackWirer : MonoBehaviour
    {
        private void Awake()
        {
            GameObject hudCameraGO = GameObject.Find("HUD_Camera");
            if (hudCameraGO == null)
            {
                Debug.LogWarning("[CameraStackWirer] HUD_Camera not found in scene. " +
                                 "Ensure pfb_hud_v2 is present before this Awake runs.", this);
                return;
            }

            if (!hudCameraGO.TryGetComponent(out Camera hudCamera))
            {
                Debug.LogWarning("[CameraStackWirer] HUD_Camera GameObject has no Camera " +
                                 "component.", this);
                return;
            }

            if (!TryGetComponent(out UniversalAdditionalCameraData baseData))
            {
                Debug.LogError("[CameraStackWirer] UniversalAdditionalCameraData missing on " +
                               "Main Camera. URP camera stack cannot be configured.", this);
                return;
            }

            // Guard against duplicate entries — iterate without LINQ.
            bool alreadyInStack = false;
            for (int i = 0; i < baseData.cameraStack.Count; i++)
            {
                if (baseData.cameraStack[i] == hudCamera)
                {
                    alreadyInStack = true;
                    break;
                }
            }

            if (!alreadyInStack)
            {
                baseData.cameraStack.Add(hudCamera);
            }
        }
    }
}
