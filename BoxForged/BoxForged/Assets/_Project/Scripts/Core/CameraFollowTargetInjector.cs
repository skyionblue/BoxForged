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
        private CinemachineCamera _cam;
        private bool _targetAssigned;

        private void Awake()
        {
            _cam = GetComponent<CinemachineCamera>();
        }

        private void Start()
        {
            TryAssignTarget();
        }

        // Scene root GameObject initialization order is not guaranteed — Start() can run
        // before pfb_player has been instantiated/activated. RunStartUI already documents
        // and works around this exact hazard with its own lazy re-find in Show() (see its
        // "Awake() may have run before pfb_player was instantiated" comment); this component
        // previously had no such retry, so it could permanently fail to wire Follow/LookAt
        // depending on scene object order — non-deterministic, not scoped to any one scene.
        // Self-terminating: disables itself the instant assignment succeeds, so there is no
        // ongoing per-frame cost once the camera is correctly following.
        private void Update()
        {
            if (!_targetAssigned) TryAssignTarget();
        }

        private void TryAssignTarget()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[CameraFollowTargetInjector] No Player found in scene — will retry next frame.");
                return;
            }

            _cam.Follow = player.transform;

            // Prefer CameraLookTarget child (gives smoother look-at point at chest level)
            Transform lookTarget = null;
            foreach (var t in player.GetComponentsInChildren<Transform>(true))
                if (t.name == "CameraLookTarget") { lookTarget = t; break; }

            // ADR-0001: pfb_CM_FollowCam no longer has a CinemachineHardLookAt (or any other
            // Aim component) — rotation is authored on the vcam's own transform and never
            // computed at runtime. Setting LookAt here is now a no-op (nothing reads it), but
            // it is harmless, so it is left in place rather than ripped out for its own sake.
            _cam.LookAt = lookTarget != null ? lookTarget : player.transform;

            _targetAssigned = true;
            enabled = false; // Done — stop receiving Update() entirely.
        }
    }
}
