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
    ///
    /// <para><b>Non-zero lateral (X) offset support:</b> the rig may carry a fixed sideways
    /// FollowOffset.x for an over-the-shoulder framing (yaw stays locked at 0, so a lateral shift
    /// is a pure world-space X translation of the camera, not a rotation). d_view — the quantity
    /// that governs pitch, F/R depth, and the width formula — is the slant distance in the
    /// camera's pitch plane only (its Y/Z components); X is orthogonal to that plane and must be
    /// excluded from d_view, or an inflated "distance" would under-correct the width floor on
    /// narrow aspects. X itself is still scaled by the same factor k as Y/Z: that keeps the
    /// player's fractional off-centre position in frame (X as a proportion of total visible width)
    /// constant across aspects, rather than letting a fixed-metre lateral offset become a shrinking
    /// fraction of an ever-widening frame as k grows on narrow screens.</para>
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

        // Authored rig value, captured once. Its Y/Z components encode pitch and the base view
        // distance the authored rig was derived for (ADR-0001: ~9.36m at zero lateral offset);
        // X, if non-zero, is a fixed lateral (over-the-shoulder) shift orthogonal to that plane.
        private Vector3 _baseFollowOffset;

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
            _follow.FollowOffset = ComputeScaledOffset(
                _baseFollowOffset, _camera.Lens.FieldOfView, aspect, _minLateralWidthMeters);
        }

        /// <summary>
        /// Pure, Unity-lifecycle-free core of the aspect-adaptive scaling — separated out so it can
        /// be unit tested without a live CinemachineCamera/CinemachineFollow pair. See the class
        /// summary for the derivation and the reasoning behind including X (lateral offset) in the
        /// uniform scale while excluding it from the distance used to size that scale.
        /// </summary>
        internal static Vector3 ComputeScaledOffset(
            Vector3 baseFollowOffset, float verticalFovDegrees, float aspect, float minLateralWidthMeters)
        {
            float verticalFovRad = verticalFovDegrees * Mathf.Deg2Rad;
            float horizontalHalfFov = Mathf.Atan(Mathf.Tan(verticalFovRad * 0.5f) * aspect);

            // d_view for the authored rig: the slant distance in the camera's pitch plane (Y/Z
            // only). A fixed lateral (X) offset is orthogonal to this plane and must be excluded —
            // folding it into the magnitude would inflate d_view, under-correcting the scale needed
            // to hold the width floor on narrow aspects.
            float baseViewDistance = new Vector2(baseFollowOffset.y, baseFollowOffset.z).magnitude;

            // View distance required to hold the width floor at this aspect.
            float requiredViewDistance = (minLateralWidthMeters * 0.5f) / Mathf.Tan(horizontalHalfFov);

            // Only pull the camera back further than its authored distance — never closer. On
            // aspects where the authored rig already clears the width floor (roughly 20:9+), this
            // is a no-op.
            float viewDistance = Mathf.Max(baseViewDistance, requiredViewDistance);
            float scale = viewDistance / baseViewDistance;

            // Scale X along with Y/Z: this keeps the player's off-centre position a constant
            // fraction of the visible width across aspects (see class summary), and — because
            // scale is always >= 1 — can only ever move the visible window further from the player
            // on both sides relative to their k=1 values, never closer.
            return baseFollowOffset * scale;
        }
    }
}
