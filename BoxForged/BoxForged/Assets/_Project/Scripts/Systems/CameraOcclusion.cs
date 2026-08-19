using System.Collections.Generic;
using UnityEngine;

namespace Boxhead.Systems
{
    // Attach to Main Camera. Assign _target to the Player transform.
    //
    // ADR-0001 §2.7 / consequences: this is the ONE occlusion system BoxForged keeps.
    // BuildingOcclusionFader.cs used the same idea but (a) mutated shared Material assets
    // in place instead of instancing them — a real bug, not just a tuning issue, since it
    // permanently converts every building sharing that material to Transparent — and (b)
    // selected occluders via a single feet-height raycast, which at the old ~40.8° top-down
    // pitch rarely mattered but at the new 36° pitch misses walls that cover the torso while
    // leaving the feet clear. It has been deleted; this is the retained/retuned system.
    //
    // Selection is a single raycast from the camera to the player's TORSO (not feet, not a
    // full projected AABB rect) at every frame. At a low, near-level pitch a building's full
    // projected screen-space rect is far bigger than its actual silhouette against the player,
    // which is what caused mass over-fading under the old AABB-overlap test. A direct
    // line-of-sight raycast only flags something that is actually between the camera and the
    // torso, which is the correct question at any pitch.
    //
    // Any Renderer on the Building layer that occludes the line from camera to player torso
    // fades to _fadeAlpha, then smoothly restores when line of sight clears.
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
        [Tooltip("Height above the player's feet to aim the occlusion ray at. Approximates torso/chest height so occluders are judged by what actually covers the player's upper body at the new low camera pitch, not by feet-height or bounds-centre logic.")]
        [SerializeField] private float _torsoHeight = 1.3f;
        [Tooltip("Max simultaneous occluders resolved per frame along the camera-to-torso ray (e.g. a fence in front of a wall).")]
        [SerializeField] private int _maxHitsPerRay = 8;

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

        private RaycastHit[] _hitsBuffer; // reused for the camera→torso occlusion ray

        // ── Runtime refs ─────────────────────────────────────────────────────────

        private Camera _cam;

        // ── Per-frame bookkeeping ────────────────────────────────────────────────

        private readonly Dictionary<Renderer, WallState> _tracked  = new Dictionary<Renderer, WallState>();
        private readonly List<Renderer>                  _toRemove = new List<Renderer>();
        private MaterialPropertyBlock                    _mpb;

        // Collider → Renderer mapping never changes at runtime for static level geometry, so it
        // is resolved once per collider and cached rather than calling GetComponentInChildren
        // on every ray hit, every frame (see B35 — a standing violation of the project's own
        // "avoid per-frame GetComponent" rule once the occlusion retune removed the viewport-rect
        // rejection test that used to make this rare).
        private readonly Dictionary<Collider, Renderer> _rendererCache = new Dictionary<Collider, Renderer>();

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _wallMask   = LayerMask.GetMask("Building");
            _hitsBuffer = new RaycastHit[_maxHitsPerRay];
            _mpb        = new MaterialPropertyBlock();
            _cam        = GetComponent<Camera>();

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

            Vector3 camPos = transform.position;
            Vector3 aim    = _target.position + Vector3.up * _torsoHeight;
            Vector3 toAim  = aim - camPos;
            float   camDist = toAim.magnitude;
            if (camDist <= 0.0001f) return;
            Vector3 dir = toAim / camDist;

            // Mark all tracked renderers as not-yet-hit this frame
            foreach (var kv in _tracked)
                kv.Value.HitThisFrame = false;

            // Single line-of-sight raycast from the camera to the player's torso. Anything on
            // the Building layer between the two is, by definition, occluding the player —
            // no viewport-rect approximation needed, and it stays correct at any camera pitch.
            int hitCount = Physics.RaycastNonAlloc(
                camPos, dir, _hitsBuffer, camDist, _wallMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Renderer rend = ResolveRenderer(_hitsBuffer[i].collider);
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
            _rendererCache.Clear();
        }

        // Looks up (and caches) the Renderer for a hit Collider — see B35. Caches a null result
        // too (colliders with no Renderer, e.g. a trigger-only volume on the Building layer)
        // so those don't repeat the GetComponentInChildren search on every subsequent hit either.
        private Renderer ResolveRenderer(Collider collider)
        {
            if (_rendererCache.TryGetValue(collider, out Renderer cached))
                return cached;

            Renderer rend = collider.GetComponentInChildren<Renderer>();
            _rendererCache[collider] = rend;
            return rend;
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
