using System.Collections.Generic;
using UnityEngine;

namespace Boxhead.Systems
{
    // Attach to Main Camera. Assign _target to the Player transform.
    // Any Renderer on the Building layer whose screen-space projection overlaps the
    // player AND is closer to the camera than the player fades to _fadeAlpha, then
    // smoothly restores when line of sight clears.
    //
    // Detection uses viewport-space bounds overlap so buildings to the left, right,
    // north, or south of the player are all caught regardless of camera angle.
    //
    // First occlusion: creates one per-instance URP Transparent material (once per wall).
    // Subsequent alpha animation uses MaterialPropertyBlock — zero GC in steady state.
    // On restore: destroys the material instance to prevent leaks.
    [RequireComponent(typeof(Camera))]
    public sealed class CameraOcclusion : MonoBehaviour
    {
        // ── Serialized fields ────────────────────────────────────────────────────

        [SerializeField] private Transform _target;
        [SerializeField] private LayerMask _wallMask;   // set to Building layer in Awake
        [Tooltip("Alpha the wall fades to when it occludes the player (0 = invisible, 1 = opaque).")]
        [SerializeField, Range(0f, 1f)] private float _fadeAlpha  = 0.2f;
        [Tooltip("Speed of the fade lerp.")]
        [SerializeField] private float _fadeSpeed  = 10f;
        [Tooltip("Radius around player to search for Building-layer colliders.")]
        [SerializeField] private float _searchRadius = 30f;

        // ── Shader property IDs (static, zero GC) ───────────────────────────────

        private static readonly int s_BaseColorId     = Shader.PropertyToID("_BaseColor");
        private static readonly int s_SurfaceId       = Shader.PropertyToID("_Surface");
        private static readonly int s_BlendId         = Shader.PropertyToID("_Blend");
        private static readonly int s_SrcBlendId      = Shader.PropertyToID("_SrcBlend");
        private static readonly int s_DstBlendId      = Shader.PropertyToID("_DstBlend");
        private static readonly int s_SrcBlendAlphaId = Shader.PropertyToID("_SrcBlendAlpha");
        private static readonly int s_DstBlendAlphaId = Shader.PropertyToID("_DstBlendAlpha");
        private static readonly int s_ZWriteId        = Shader.PropertyToID("_ZWrite");
        private static readonly int s_ZWriteControlId = Shader.PropertyToID("_ZWriteControl");

        // ── Pre-allocated buffers (Awake) ────────────────────────────────────────

        private Collider[]  _overlapBuffer; // size 32, nearby Building-layer colliders
        private Vector3[]   _corners;       // size 8, reused for bounds projection

        // ── Runtime refs ─────────────────────────────────────────────────────────

        private Camera _cam;

        // ── Per-frame bookkeeping ────────────────────────────────────────────────

        private readonly Dictionary<Renderer, WallState> _tracked  = new Dictionary<Renderer, WallState>();
        private readonly List<Renderer>                  _toRemove = new List<Renderer>();
        private MaterialPropertyBlock                    _mpb;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _wallMask      = LayerMask.GetMask("Building");
            _overlapBuffer = new Collider[32];
            _corners       = new Vector3[8];
            _mpb           = new MaterialPropertyBlock();
            _cam           = GetComponent<Camera>();

            // Fallback: find player by tag if the Inspector reference was dropped by a scene re-save.
            if (_target == null)
            {
                var playerGO = GameObject.FindWithTag("Player");
                if (playerGO != null) _target = playerGO.transform;
            }
        }

        private void LateUpdate()
        {
            if (_target == null || _cam == null) return;

            Vector3 camPos  = transform.position;
            Vector3 aim     = _target.position + Vector3.up * 0.5f;
            float   camDist = Vector3.Distance(camPos, aim);

            // Project the player to viewport space once per frame
            Vector3 playerVP = _cam.WorldToViewportPoint(aim);
            if (playerVP.z <= 0f) return; // player behind camera — skip

            // Mark all tracked renderers as not-yet-hit this frame
            foreach (var kv in _tracked)
                kv.Value.HitThisFrame = false;

            // Find all Building-layer colliders near the player
            int colCount = Physics.OverlapSphereNonAlloc(
                aim, _searchRadius, _overlapBuffer, _wallMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < colCount; i++)
            {
                Collider col = _overlapBuffer[i];

                // Building must be closer to camera than the player is
                float buildingDist = Vector3.Distance(camPos, col.bounds.center);
                if (buildingDist >= camDist) continue;

                // Project building's 8 AABB corners to viewport space and find 2-D extent
                Bounds b = col.bounds;
                _corners[0] = new Vector3(b.min.x, b.min.y, b.min.z);
                _corners[1] = new Vector3(b.max.x, b.min.y, b.min.z);
                _corners[2] = new Vector3(b.min.x, b.max.y, b.min.z);
                _corners[3] = new Vector3(b.max.x, b.max.y, b.min.z);
                _corners[4] = new Vector3(b.min.x, b.min.y, b.max.z);
                _corners[5] = new Vector3(b.max.x, b.min.y, b.max.z);
                _corners[6] = new Vector3(b.min.x, b.max.y, b.max.z);
                _corners[7] = new Vector3(b.max.x, b.max.y, b.max.z);

                float vMinX = float.MaxValue, vMaxX = float.MinValue;
                float vMinY = float.MaxValue, vMaxY = float.MinValue;
                for (int j = 0; j < 8; j++)
                {
                    Vector3 vp = _cam.WorldToViewportPoint(_corners[j]);
                    if (vp.z <= 0f) continue; // corner behind camera
                    if (vp.x < vMinX) vMinX = vp.x;
                    if (vp.x > vMaxX) vMaxX = vp.x;
                    if (vp.y < vMinY) vMinY = vp.y;
                    if (vp.y > vMaxY) vMaxY = vp.y;
                }
                if (vMinX == float.MaxValue) continue; // all corners behind camera

                // Check if player's viewport position falls inside building's projected rect
                if (playerVP.x < vMinX || playerVP.x > vMaxX ||
                    playerVP.y < vMinY || playerVP.y > vMaxY)
                    continue;

                Renderer rend = col.gameObject.GetComponentInChildren<Renderer>();
                if (rend == null || rend.sharedMaterial == null) continue;

                    if (!_tracked.TryGetValue(rend, out WallState state))
                    {
                        state = new WallState(
                            rend,
                            s_SurfaceId, s_BlendId,
                            s_SrcBlendId, s_DstBlendId,
                            s_SrcBlendAlphaId, s_DstBlendAlphaId,
                            s_ZWriteId, s_ZWriteControlId);
                        _tracked[rend] = state;
                    }

                    state.HitThisFrame = true;
                    state.TargetAlpha  = _fadeAlpha;
            }

            // Animate alpha and clean up fully-restored renderers
            _toRemove.Clear();
            foreach (var kv in _tracked)
            {
                Renderer  rend  = kv.Key;
                WallState state = kv.Value;

                if (!state.HitThisFrame) state.TargetAlpha = 1f;

                state.Tick(_fadeSpeed, s_BaseColorId, _mpb, rend);

                if (!state.HitThisFrame && state.IsRestored)
                {
                    // Clear MPB, restore original shared material, destroy instance
                    rend.SetPropertyBlock(null);
                    state.RestoreSharedMaterial(rend);
                    state.DestroyInstance();
                    _toRemove.Add(rend);
                }
            }

            for (int i = 0; i < _toRemove.Count; i++)
                _tracked.Remove(_toRemove[i]);
        }

        private void OnDestroy()
        {
            // Destroy all outstanding per-instance materials
            foreach (var kv in _tracked)
                kv.Value.DestroyInstance();
            _tracked.Clear();
        }

        // ── WallState ────────────────────────────────────────────────────────────

        private sealed class WallState
        {
            public bool  HitThisFrame;
            public float TargetAlpha = 1f;

            private readonly Color    _originalColor;
            private readonly Material _originalShared; // cached so we can restore correctly
            private Material          _matInstance;
            private float             _alpha = 1f;

            public bool IsRestored => _alpha >= 0.99f;

            public WallState(
                Renderer r,
                int surfaceId, int blendId,
                int srcBlendId, int dstBlendId,
                int srcBlendAlphaId, int dstBlendAlphaId,
                int zWriteId, int zWriteControlId)
            {
                _originalShared = r.sharedMaterial;
                _originalColor  = _originalShared.GetColor("_BaseColor");

                // Copy the shared material, switch it to URP Transparent (Alpha blend).
                // URP 17 requires _SrcBlendAlpha/_DstBlendAlpha in addition to the
                // standard _SrcBlend/_DstBlend — without them the material enters an
                // invalid blend state and renders as error magenta.
                _matInstance = new Material(_originalShared);
                _matInstance.SetFloat(surfaceId,        1f);  // Transparent
                _matInstance.SetFloat(blendId,          0f);  // Alpha blend
                _matInstance.SetFloat(srcBlendId,       5f);  // SrcAlpha
                _matInstance.SetFloat(dstBlendId,       10f); // OneMinusSrcAlpha
                _matInstance.SetFloat(srcBlendAlphaId,  1f);  // One
                _matInstance.SetFloat(dstBlendAlphaId,  10f); // OneMinusSrcAlpha
                _matInstance.SetFloat(zWriteId,         0f);  // ZWrite off
                _matInstance.SetFloat(zWriteControlId,  0f);  // Auto
                _matInstance.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                _matInstance.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                _matInstance.renderQueue = 3000;

                r.sharedMaterial = _matInstance;
            }

            /// <summary>Lerp alpha and write the result via the shared MaterialPropertyBlock.</summary>
            public void Tick(float speed, int colorId, MaterialPropertyBlock mpb, Renderer r)
            {
                _alpha = Mathf.Lerp(_alpha, TargetAlpha, speed * Time.deltaTime);

                r.GetPropertyBlock(mpb);
                mpb.SetColor(colorId,
                    new Color(_originalColor.r, _originalColor.g, _originalColor.b, _alpha));
                r.SetPropertyBlock(mpb);
            }

            /// <summary>Restore the renderer to its original shared material.</summary>
            public void RestoreSharedMaterial(Renderer r) => r.sharedMaterial = _originalShared;

            /// <summary>Destroy the per-instance material. Call after RestoreSharedMaterial.</summary>
            public void DestroyInstance()
            {
                if (_matInstance != null)
                {
                    Object.Destroy(_matInstance);
                    _matInstance = null;
                }
            }
        }
    }
}
