using UnityEngine;
using Unity.Cinemachine;

namespace Boxhead.Core
{
    /// <summary>
    /// Attached to the CM_FollowCam prefab. At runtime, finds the Player in the scene
    /// and wires the CinemachineCamera Follow and LookAt targets automatically.
    /// This is necessary because prefab scene-object references can't be serialised
    /// cross-scene — so we resolve them at Start instead.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraFollowTargetInjector : MonoBehaviour
    {
        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[CameraFollowTargetInjector] No Player found in scene.");
                return;
            }

            var cam = GetComponent<CinemachineCamera>();
            cam.Follow = player.transform;

            // Prefer CameraLookTarget child (gives smoother look-at point at chest level)
            Transform lookTarget = null;
            foreach (var t in player.GetComponentsInChildren<Transform>(true))
                if (t.name == "CameraLookTarget") { lookTarget = t; break; }

            // ADR-0001: pfb_CM_FollowCam no longer has a CinemachineHardLookAt (or any other
            // Aim component) — rotation is authored on the vcam's own transform and never
            // computed at runtime. Setting LookAt here is now a no-op (nothing reads it), but
            // it is harmless, so it is left in place rather than ripped out for its own sake.
            cam.LookAt = lookTarget != null ? lookTarget : player.transform;
        }
    }
}
