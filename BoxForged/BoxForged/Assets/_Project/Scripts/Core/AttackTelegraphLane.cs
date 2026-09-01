using UnityEngine;

namespace Boxhead.Core
{
    /// <summary>
    /// ADR-0007: one pooled ground-plane telegraph lane, the channel's second geometry
    /// alongside <see cref="AttackTelegraphIndicator"/>. Built once by
    /// AttackTelegraphService's dedicated lane pool warm-up and only ever reconfigured by
    /// Activate() — never instantiated per attack.
    ///
    /// World-space anchored and never target-tracking. This is the point: the lane represents
    /// committed geometry (a heading the attack has already aimed down and cannot re-aim),
    /// unlike the billboard indicator, which deliberately follows its caster. Update is a
    /// duration countdown and nothing else — no billboarding, no target tracking — so this
    /// class does not share AttackTelegraphIndicator's "billboards every active frame"
    /// invariant and is not folded into that component (see ADR-0007 §3).
    ///
    /// Renders through the same BoxForged/TelegraphOverlayUnlit shader as the billboard, but at
    /// ZTest LEqual (mat_TelegraphLane.mat) instead of ZTest Always — a floor marking should
    /// read as standing ON the ground, with the player/boss correctly occluding it, rather than
    /// painting through them the way the always-visible billboard needs to.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AttackTelegraphLane : MonoBehaviour
    {
        private static Mesh s_QuadMesh;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterEditorCleanup()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    // Destroy before nulling — this is a real Mesh object (managed + native
                    // GPU-side data); dropping only the managed reference leaks the native side
                    // for the rest of the Editor session. Same pattern as
                    // AttackTelegraphIndicator.RegisterEditorCleanup.
                    if (s_QuadMesh != null) Object.DestroyImmediate(s_QuadMesh);
                    s_QuadMesh = null;
                }
            };
        }
#endif

        private MeshFilter   _meshFilter;
        private MeshRenderer _meshRenderer;
        private Material     _material;

        private float _remaining;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            EnsureSharedMesh();

            _meshFilter   = gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows    = false;
            _meshFilter.sharedMesh = s_QuadMesh;
        }

        /// <summary>
        /// Assigns the shared lane material this instance renders with. Called once by
        /// AttackTelegraphService immediately after AddComponent&lt;AttackTelegraphLane&gt;()
        /// during pool warm-up — one shared, pre-tinted material for every pooled lane, owned
        /// and destroyed by the service (same convention as AttackTelegraphIndicator.Initialize).
        /// </summary>
        internal void Initialize(Material material)
        {
            _material = material;
            if (_meshRenderer != null && _material != null)
                _meshRenderer.sharedMaterial = _material;
        }

        /// <summary>
        /// Raises the lane at world-space <paramref name="start"/>, extending
        /// <paramref name="length"/> meters along <paramref name="direction"/> (expected
        /// pre-normalized on XZ by the caller — AttackTelegraphService.ShowGroundLane does this),
        /// <paramref name="width"/> meters wide, lifted just above <paramref name="groundY"/>,
        /// for <paramref name="duration"/> seconds.
        /// </summary>
        public void Activate(Vector3 start, Vector3 direction, float length, float width, float groundY, float duration)
        {
            _remaining = duration;

            if (_meshRenderer != null && _material != null)
                _meshRenderer.sharedMaterial = _material;

            transform.position   = start + Vector3.up * (groundY + 0.02f);
            transform.rotation   = Quaternion.LookRotation(direction, Vector3.up);
            transform.localScale = new Vector3(width, 1f, length);

            IsActive = true;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            IsActive = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!IsActive) return;

            _remaining -= Time.deltaTime;
            if (_remaining <= 0f)
                Deactivate();
        }

        // ── Shared geometry (built once, reused by every pooled instance) ──────────────────────

        // XZ plane, x in [-0.5, 0.5], z in [0, 1], normal +Y — ADR-0007 §2. Scaled/rotated per
        // activation via transform.localScale/rotation rather than rebuilt, so every pooled
        // lane shares one native mesh and Activate() allocates nothing.
        private static void EnsureSharedMesh()
        {
            if (s_QuadMesh != null) return;

            s_QuadMesh = new Mesh { name = "TelegraphLaneQuad" };
            var vertices = new[]
            {
                new Vector3(-0.5f, 0f, 0f),
                new Vector3( 0.5f, 0f, 0f),
                new Vector3( 0.5f, 0f, 1f),
                new Vector3(-0.5f, 0f, 1f),
            };
            var triangles = new[] { 0, 1, 2, 0, 2, 3 };
            var normals   = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };

            s_QuadMesh.vertices  = vertices;
            s_QuadMesh.triangles = triangles;
            s_QuadMesh.normals   = normals;
            s_QuadMesh.RecalculateBounds();
        }
    }
}
