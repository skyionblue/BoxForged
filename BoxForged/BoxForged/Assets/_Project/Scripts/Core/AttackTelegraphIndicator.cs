using UnityEngine;

namespace Boxhead.Core
{
    /// <summary>
    /// One pooled overhead telegraph indicator (ADR-0003). Built once by
    /// AttackTelegraphService's pool warm-up and only ever reconfigured by Activate() —
    /// never instantiated per wind-up.
    ///
    /// Shape carries the parryable/un-parryable bit (filled circle vs filled triangle) so it
    /// survives greyscale and colour-blind simulation; colour is a redundant reinforcement only,
    /// never the sole signal. Renders through BoxForged/TelegraphOverlayUnlit, a small ZTest-
    /// Always shader, so it stays visible through walls/props without a decal projector or a
    /// depth prepass (both explicitly rejected in ADR-0003 on mobile cost grounds).
    ///
    /// Billboards to face the main camera every frame it is active, refreshed on camera or
    /// target movement — the same pattern as EnemyHealthBar's ApplyBillboard.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AttackTelegraphIndicator : MonoBehaviour
    {
        private static Mesh s_CircleMesh;
        private static Mesh s_TriangleMesh;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterEditorCleanup()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    // Destroy before nulling — these are real Mesh objects (managed + native
                    // GPU-side data); dropping only the managed reference leaks the native side
                    // for the rest of the Editor session.
                    if (s_CircleMesh   != null) Object.DestroyImmediate(s_CircleMesh);
                    if (s_TriangleMesh != null) Object.DestroyImmediate(s_TriangleMesh);
                    s_CircleMesh   = null;
                    s_TriangleMesh = null;
                }
            };
        }
#endif

        private MeshFilter   _meshFilter;
        private MeshRenderer _meshRenderer;

        // Shared, pre-tinted materials owned by AttackTelegraphService — see B32. Assigned once
        // via Initialize() right after this component is added; never instantiated per-indicator.
        private Material _parryableMaterial;
        private Material _unparryableMaterial;

        private Camera _mainCamera;

        private Transform _target;
        private float     _heightOffset;
        private float     _remaining;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            EnsureSharedMeshes();

            _meshFilter   = gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows    = false;
        }

        /// <summary>
        /// Assigns the two shared, pre-tinted overlay materials (parryable / un-parryable) this
        /// indicator switches sharedMaterial between. Called once by AttackTelegraphService
        /// immediately after AddComponent&lt;AttackTelegraphIndicator&gt;() during pool warm-up —
        /// collapses what used to be 8 per-instance runtime materials (one "new Material(...)"
        /// per pooled indicator) down to 2 shared ones, owned and destroyed by the service.
        /// </summary>
        internal void Initialize(Material parryableMaterial, Material unparryableMaterial)
        {
            _parryableMaterial   = parryableMaterial;
            _unparryableMaterial = unparryableMaterial;
        }

        /// <summary>Configures and shows this indicator. Safe to call on an already-active instance.</summary>
        public void Activate(Transform target, AttackTelegraphKind kind, float heightOffset, float duration)
        {
            _target       = target;
            _heightOffset = heightOffset;
            _remaining    = duration;
            if (_mainCamera == null) _mainCamera = Camera.main;

            bool parryable = kind == AttackTelegraphKind.MeleeParryable
                           || kind == AttackTelegraphKind.ProjectileParryable;

            if (_meshFilter != null)
                _meshFilter.sharedMesh = parryable ? s_CircleMesh : s_TriangleMesh;
            if (_meshRenderer != null)
            {
                Material mat = parryable ? _parryableMaterial : _unparryableMaterial;
                if (mat != null) _meshRenderer.sharedMaterial = mat;
            }

            IsActive = true;
            gameObject.SetActive(true);
            // Set position immediately, before the billboard/first render — Update() (where
            // position normally tracks the target) does not run until the frame AFTER Activate()
            // for coroutine-driven wind-up calls. Without this, a freshly-activated indicator
            // renders once at this GameObject's current position — the AttackTelegraphService
            // parent at the world origin — before jumping to the correct spot next frame.
            transform.position = target.position + Vector3.up * heightOffset;
            ApplyBillboard();
        }

        public void Deactivate()
        {
            IsActive = false;
            _target  = null;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!IsActive) return;

            if (_target == null)
            {
                Deactivate();
                return;
            }

            _remaining -= Time.deltaTime;
            if (_remaining <= 0f)
            {
                Deactivate();
                return;
            }

            transform.position = _target.position + Vector3.up * _heightOffset;
            ApplyBillboard();
        }

        private void ApplyBillboard()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Vector3 dir = _mainCamera.transform.position - transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(-dir, Vector3.up);
        }

        // ── Shared geometry (built once, reused by every pooled instance) ──────────────────────

        private static void EnsureSharedMeshes()
        {
            if (s_CircleMesh == null)   s_CircleMesh   = BuildCircleMesh(0.45f, 20);
            if (s_TriangleMesh == null) s_TriangleMesh = BuildTriangleMesh(0.55f);
        }

        private static Mesh BuildCircleMesh(float radius, int segments)
        {
            var mesh = new Mesh { name = "TelegraphCircle" };
            var vertices  = new Vector3[segments + 1];
            var triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            }

            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3]     = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }

            mesh.vertices  = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildTriangleMesh(float size)
        {
            var mesh = new Mesh { name = "TelegraphTriangle" };
            var vertices = new[]
            {
                new Vector3(0f, size, 0f),
                new Vector3(-size * 0.9f, -size * 0.7f, 0f),
                new Vector3(size * 0.9f, -size * 0.7f, 0f),
            };
            var triangles = new[] { 0, 1, 2 };

            mesh.vertices  = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
