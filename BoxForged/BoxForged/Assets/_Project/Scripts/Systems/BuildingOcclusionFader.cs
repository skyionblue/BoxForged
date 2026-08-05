using System.Collections.Generic;
using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Fades buildings that occlude the camera's line-of-sight to the player.
    /// Attach to any root GameObject in the scene — self-populates Player and Camera references.
    ///
    /// Uses Physics.RaycastNonAlloc and MaterialPropertyBlock (per material slot) for zero
    /// per-frame GC. Original material colours are cached on first encounter so fades preserve
    /// each building's actual tint rather than defaulting to white.
    /// </summary>
    public class BuildingOcclusionFader : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [SerializeField] private Transform _player;
        [SerializeField] private Camera    _camera;
        [SerializeField] private float     _fadeAlpha  = 0.20f;  // opacity at full fade
        [SerializeField] private float     _fadeSpeed  = 8f;
        [SerializeField] private int       _maxHits    = 16;

        // ── Private state ─────────────────────────────────────────────────────

        private RaycastHit[]    _hits;
        private MaterialPropertyBlock _mpb;

        // Renderers that have been faded at least once and may still need restoring.
        private HashSet<Renderer> _currentlyFaded;

        // Shared materials already switched to Transparent — converted once, never again.
        private HashSet<Material> _convertedMaterials;

        // Pre-allocated removal list — avoids per-frame heap alloc.
        private List<Renderer> _toRemove;

        // Original _BaseColor per renderer per material slot, captured on first encounter.
        // Keyed on Renderer so we can restore the exact tint (not just white).
        private Dictionary<Renderer, Color[]> _originalColors;

        // Current interpolated alpha per renderer. Tracked explicitly so we never
        // rely on reading back from the MaterialPropertyBlock (which returns 0 when unset).
        private Dictionary<Renderer, float> _currentAlphas;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _hits               = new RaycastHit[_maxHits];
            _mpb                = new MaterialPropertyBlock();
            _currentlyFaded     = new HashSet<Renderer>();
            _convertedMaterials = new HashSet<Material>();
            _toRemove           = new List<Renderer>(16);
            _originalColors     = new Dictionary<Renderer, Color[]>();
            _currentAlphas      = new Dictionary<Renderer, float>();
        }

        private void Start()
        {
            if (_player == null)
            {
                var playerGO = GameObject.FindWithTag("Player");
                if (playerGO != null)
                    _player = playerGO.transform;
                else
                    Debug.LogWarning("[BuildingOcclusionFader] No GameObject with tag 'Player' found.");
            }

            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                    Debug.LogWarning("[BuildingOcclusionFader] Camera.main is null — assign _camera manually.");
            }
        }

        private void Update()
        {
            if (_player == null || _camera == null) return;

            Vector3 cameraPos = _camera.transform.position;
            Vector3 playerPos = _player.position;
            Vector3 direction = playerPos - cameraPos;
            float   distance  = direction.magnitude;

            int hitCount = Physics.RaycastNonAlloc(cameraPos, direction.normalized, _hits, distance);

            // Add new occluders to the tracked set.
            for (int i = 0; i < hitCount; i++)
            {
                if (!_hits[i].collider.CompareTag("Building")) continue;

                var renderers = _hits[i].collider.GetComponentsInChildren<Renderer>();
                for (int r = 0; r < renderers.Length; r++)
                {
                    EnsureTransparent(renderers[r]);
                    _currentlyFaded.Add(renderers[r]);
                }
            }

            // Lerp every tracked renderer toward its target alpha.
            float dt = Time.deltaTime * _fadeSpeed;
            _toRemove.Clear();

            foreach (var rend in _currentlyFaded)
            {
                if (rend == null) { _toRemove.Add(rend); continue; }

                bool  isOccluding = IsRendererInHits(rend, hitCount);
                float targetAlpha = isOccluding ? _fadeAlpha : 1f;

                // Read and advance the tracked alpha (never from MPB — unset MPB returns 0).
                _currentAlphas.TryGetValue(rend, out float currentAlpha);
                if (currentAlpha == 0f) currentAlpha = 1f; // first-frame default

                float newAlpha = Mathf.Lerp(currentAlpha, targetAlpha, dt);
                _currentAlphas[rend] = newAlpha;

                // Apply per material slot so each material keeps its original RGB tint.
                ApplyFade(rend, newAlpha);

                if (!isOccluding && Mathf.Abs(newAlpha - 1f) < 0.01f)
                {
                    ApplyFade(rend, 1f);
                    _currentAlphas[rend] = 1f;
                    _toRemove.Add(rend);
                }
            }

            for (int i = 0; i < _toRemove.Count; i++)
                _currentlyFaded.Remove(_toRemove[i]);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ApplyFade(Renderer rend, float alpha)
        {
            if (!_originalColors.TryGetValue(rend, out Color[] origColors)) return;

            Material[] mats = rend.sharedMaterials;
            int count = Mathf.Min(mats.Length, origColors.Length);
            for (int m = 0; m < count; m++)
            {
                Color orig = origColors[m];
                rend.GetPropertyBlock(_mpb, m);
                _mpb.SetColor("_BaseColor", new Color(orig.r, orig.g, orig.b, alpha));
                rend.SetPropertyBlock(_mpb, m);
            }
        }

        private bool IsRendererInHits(Renderer rend, int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (!_hits[i].collider.CompareTag("Building")) continue;
                Transform ht = _hits[i].collider.transform;
                if (ht.IsChildOf(rend.transform) ||
                    rend.transform.IsChildOf(ht)  ||
                    ht == rend.transform)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// On first encounter: caches the renderer's per-slot base colours and switches
        /// each shared material to Alpha Blend transparent (once per material).
        /// </summary>
        private void EnsureTransparent(Renderer rend)
        {
            Material[] mats = rend.sharedMaterials;

            // Cache original colours before any conversion so we always have the true tint.
            if (!_originalColors.ContainsKey(rend))
            {
                Color[] origColors = new Color[mats.Length];
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m] != null && mats[m].HasProperty("_BaseColor"))
                        origColors[m] = mats[m].GetColor("_BaseColor");
                    else
                        origColors[m] = Color.white;

                    origColors[m].a = 1f; // always start fully opaque
                }
                _originalColors[rend] = origColors;
            }

            // Convert each shared material to transparent (idempotent via HashSet).
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == null || _convertedMaterials.Contains(mats[m])) continue;
                SetMaterialTransparent(mats[m]);
                _convertedMaterials.Add(mats[m]);
            }
        }

        /// <summary>
        /// Switches a URP Lit/Unlit material to Alpha Blend transparency.
        /// Called once per discovered material — never per frame.
        /// </summary>
        private static void SetMaterialTransparent(Material mat)
        {
            mat.SetFloat("_Surface",    1f);  // Transparent
            mat.SetFloat("_Blend",      0f);  // Alpha
            mat.SetFloat("_AlphaClip",  0f);
            mat.SetInt("_SrcBlend",      (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend",      (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_SrcBlendAlpha", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlendAlpha", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite",        0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }
}
