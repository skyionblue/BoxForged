using UnityEngine;
using Unity.Cinemachine;

namespace Boxhead.Core
{
    /// <summary>
    /// ADR-0001's fixed-follow rig (pitch 36°, vertical FOV 45°) only clears its own lateral-width
    /// acceptance floor (&gt;= 16m at Kid's depth, see the ADR's acceptance table) at wide aspect
    /// ratios (roughly 20:9+). Narrower aspects fall short — ~13.8m at 16:9 (the project's actual
    /// mobile-performance target hardware), ~10.3m at 4:3 (tablets). See docs/BACKLOG.md B27.
    ///
    /// Per ADR-0001 section 2.6, the fix is to recover lateral coverage on narrow aspects by
    /// increasing camera distance along the fixed view axis — never by changing pitch or vertical
    /// FOV, both of which must stay locked (changing either would either roll the horizon into
    /// frame at low pitch, or vary the control-mapping-relevant camera behaviour with aspect).
    ///
    /// The math: <c>FollowOffset = d_view * (0, sin(pitch), -cos(pitch))</c>, where d_view is the
    /// camera's slant distance to the player along its optical axis. Scaling the whole offset
    /// vector by a single factor k scales d_view (and therefore height and horizontal setback)
    /// uniformly, which leaves their ratio — and therefore pitch — exactly unchanged. Because the
    /// top-ray angle above the ground plane is <c>pitch - verticalFOV/2</c> (an angular quantity
    /// independent of distance), scaling d_view up never risks bringing the horizon into frame; it
    /// also only ever increases ground-ahead/ground-behind depth, never decreases it below the
    /// authored rig's values (which already clear their own floors). So the only thing this needs
    /// to solve for is the width floor, and only pull the camera back — never in — relative to the
    /// authored base offset.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CinemachineCamera))]
    [RequireComponent(typeof(CinemachineFollow))]
    public class AspectAdaptiveCameraFraming : MonoBehaviour
    {
        [Tooltip("Minimum lateral ground width (metres) that must stay visible at the player's depth, per ADR-0001's acceptance table.")]
        [SerializeField] private float _minLateralWidthMeters = 16f;

        private CinemachineCamera _camera;
        private CinemachineFollow _follow;

        // Authored rig values, captured once. FollowOffset direction encodes pitch; magnitude is
        // the base view distance the authored rig was derived for (ADR-0001: ~9.36m).
        private Vector3 _baseFollowOffset;
        private float _baseViewDistance;

#if UNITY_EDITOR
        // Editor-only: lets Game View aspect changes re-apply live during iteration, without
        // requiring a Play Mode restart. Compiled out entirely on device, so it costs nothing
        // there — on a real device the aspect never changes after launch, so the one-time Start
        // computation below is sufficient by itself.
        private float _lastAppliedAspect = -1f;
#endif

        private void Awake()
        {
            _camera = GetComponent<CinemachineCamera>();
            _follow = GetComponent<CinemachineFollow>();
            _baseFollowOffset = _follow.FollowOffset;
            _baseViewDistance = _baseFollowOffset.magnitude;
        }

        private void Start()
        {
            Apply(ComputeScreenAspect());
        }

#if UNITY_EDITOR
        private void Update()
        {
            float aspect = ComputeScreenAspect();
            if (!Mathf.Approximately(aspect, _lastAppliedAspect))
            {
                Apply(aspect);
            }
        }
#endif

        private static float ComputeScreenAspect()
        {
            return (float)Screen.width / Screen.height;
        }

        private void Apply(float aspect)
        {
#if UNITY_EDITOR
            _lastAppliedAspect = aspect;
#endif

            float verticalFovRad = _camera.Lens.FieldOfView * Mathf.Deg2Rad;
            float horizontalHalfFov = Mathf.Atan(Mathf.Tan(verticalFovRad * 0.5f) * aspect);

            // View distance required to hold the width floor at this aspect.
            float requiredViewDistance = (_minLateralWidthMeters * 0.5f) / Mathf.Tan(horizontalHalfFov);

            // Only pull the camera back further than its authored distance — never closer. On
            // aspects where the authored rig already clears the width floor (roughly 20:9+), this
            // is a no-op.
            float viewDistance = Mathf.Max(_baseViewDistance, requiredViewDistance);
            float scale = viewDistance / _baseViewDistance;

            _follow.FollowOffset = _baseFollowOffset * scale;
        }
    }
}
