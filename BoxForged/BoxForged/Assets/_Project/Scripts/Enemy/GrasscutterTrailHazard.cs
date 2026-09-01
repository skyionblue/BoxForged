using UnityEngine;
using Boxhead.Core;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    /// <summary>
    /// One pooled Cut-Grass Trail hazard segment (Grasscutter Phase 2 "Rev" — World 2 GDD §5).
    /// Pre-warmed and Activate()/Deactivate()-recycled by GrasscutterAI's pool, mirroring the
    /// exact warm-pool pattern AttackTelegraphService/AttackTelegraphIndicator already use
    /// (ADR-0003) — never Instantiate()/Destroy() per dash segment, so laying a trail during
    /// Spin-Dash costs zero per-frame allocation (TDD §3.2 steady-state GC budget).
    ///
    /// Deals damage to the player on overlap, re-hitting at most once per <see cref="_hitCooldown"/>
    /// while they remain standing in it, and returns itself to the pool once its lifetime elapses.
    /// Damage is always un-parryable — it is a passive environmental hazard left behind by a dash,
    /// not an attack with its own tell.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public sealed class GrasscutterTrailHazard : MonoBehaviour
    {
        private static Mesh s_QuadMesh;

        [Tooltip("Seconds between repeat hits while the player stands in an active hazard.")]
        [SerializeField] private float _hitCooldown = 0.6f;

        private SphereCollider _collider;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private int _damage;
        private float _remaining;
        private float _hitTimer;
        private CombatController _playerInside;

        // The pool holder (GrasscutterAI's "CutGrassTrailPool" child) this instance lives under
        // while inactive, cached once in Awake so Deactivate() can return it there for tidy
        // hierarchy organization. Captured here rather than passed in because GrasscutterAI's
        // WarmTrailPool parents each instance under the holder before AddComponent runs Awake.
        private Transform _poolParent;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            _poolParent = transform.parent;

            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;

            EnsureSharedMesh();
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _meshFilter.sharedMesh = s_QuadMesh;
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
        }

        /// <summary>
        /// Assigns the shared visual material every pooled instance uses. Called once by
        /// GrasscutterAI right after AddComponent&lt;GrasscutterTrailHazard&gt;() during pool
        /// warm-up — same "owned and destroyed by the caller" convention AttackTelegraphService
        /// uses for its two shared materials (see B32's shader-stripping lesson: this project
        /// requires a real serialized Material reference, never a runtime Shader.Find lookup).
        /// A null material is tolerated — the hazard still functions (damage + pooling), it just
        /// renders with Unity's default/error material until art assigns one.
        /// </summary>
        public void Initialize(Material material)
        {
            if (_meshRenderer != null && material != null)
                _meshRenderer.sharedMaterial = material;
        }

        public void Activate(Vector3 position, float radius, float duration, int damage)
        {
            // B3 fix: a "trail" hazard is supposed to mark ground the boss already passed
            // through. While parented under the boss (or its pool holder, which is itself a
            // child of the boss), a segment silently rode along and rotated with every
            // subsequent boss move for its full lifetime instead of staying planted at its
            // drop point. Detach to the scene root for the duration of its active life; Deactivate()
            // below returns it to the pool holder for tidy hierarchy organization once recycled.
            transform.SetParent(null, true);
            transform.position = position;
            _collider.radius = radius;
            _damage = damage;
            _remaining = duration;
            _hitTimer = 0f;
            _playerInside = null;
            IsActive = true;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            IsActive = false;
            _playerInside = null;
            gameObject.SetActive(false);
            // Return to the pool holder now that this instance is no longer live in the world.
            // _poolParent compares equal to null via Unity's fake-null if the boss (and its pool
            // holder) was already destroyed — SetParent(null, ...) is a safe no-op in that case.
            transform.SetParent(_poolParent, false);
        }

        private void Update()
        {
            if (!IsActive) return;

            _remaining -= Time.deltaTime;
            if (_remaining <= 0f)
            {
                Deactivate();
                return;
            }

            if (_hitTimer > 0f) _hitTimer -= Time.deltaTime;

            if (_playerInside != null && _hitTimer <= 0f)
            {
                AttackResult result = _playerInside.TryReceiveAttack(_damage, parryable: false);
                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
                _hitTimer = _hitCooldown;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive || !other.CompareTag("Player")) return;
            _playerInside = other.GetComponent<CombatController>();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = null;
        }

        // Simple flat quad on the ground plane, built once and shared by every pooled instance
        // (radius/scale driven by the collider, not the mesh) — same "shared static geometry"
        // convention as AttackTelegraphIndicator's circle/triangle meshes.
        private static void EnsureSharedMesh()
        {
            if (s_QuadMesh != null) return;
            s_QuadMesh = new Mesh { name = "TrailHazardQuad" };
            var vertices = new[]
            {
                new Vector3(-0.5f, 0.02f, -0.5f),
                new Vector3(0.5f, 0.02f, -0.5f),
                new Vector3(0.5f, 0.02f, 0.5f),
                new Vector3(-0.5f, 0.02f, 0.5f),
            };
            var triangles = new[] { 0, 2, 1, 0, 3, 2 };
            s_QuadMesh.vertices = vertices;
            s_QuadMesh.triangles = triangles;
            s_QuadMesh.RecalculateNormals();
            s_QuadMesh.RecalculateBounds();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterEditorCleanup()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    if (s_QuadMesh != null) Object.DestroyImmediate(s_QuadMesh);
                    s_QuadMesh = null;
                }
            };
        }
#endif
    }
}
