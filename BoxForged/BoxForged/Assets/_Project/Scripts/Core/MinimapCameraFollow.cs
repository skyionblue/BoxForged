using UnityEngine;

namespace Boxhead.Core
{
    /// <summary>
    /// Attached to <c>pfb_MinimapCamera</c>. Keeps the top-down minimap camera centred on the
    /// player's X/Z position every frame (in <see cref="LateUpdate"/>, after the player has moved),
    /// while clamping that centre so the camera's captured square never extends past the level's
    /// <see cref="_ground"/> plane. Camera height, rotation, near/far clip planes, and culling mask
    /// are left exactly as authored on the prefab/scene instance — only X/Z becomes dynamic.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class MinimapCameraFollow : MonoBehaviour
    {
        [Tooltip("The scene's Ground plane. Its Transform (position/rotation/scale) plus its " +
                 "MeshCollider's shared mesh bounds define the level's real walkable rectangle in " +
                 "world space, including any yaw the level is rotated by. This can only be wired as " +
                 "a scene-instance reference, never baked into the prefab asset — see " +
                 "CameraFollowTargetInjector for the same cross-scene-reference limitation.")]
        [SerializeField] private Transform _ground;

        private Camera _camera;
        private Transform _player;

        // The mesh's own local (unscaled) bounds are cached once — a mesh asset's bounds never
        // change at runtime. The ground rectangle's world-space center/rotation/half-extents are
        // NOT cached: they are recomputed from the live _ground.Transform every LateUpdate instead,
        // which is cheap (no GetComponent, no allocations, a couple of vector ops).
        private bool _groundBoundsValid;
        private Bounds _groundLocalBounds;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            CacheGroundMeshBounds();
        }

        private void LateUpdate()
        {
            if (!TryGetPlayer(out Transform player)) return;

            Vector3 targetXZ = player.position;

            if (_groundBoundsValid)
            {
                Vector3 groundCenterWorld = _ground.TransformPoint(_groundLocalBounds.center);
                Quaternion groundRotation = _ground.rotation;
                Vector3 groundHalfExtentsWorld = Vector3.Scale(_groundLocalBounds.extents, _ground.lossyScale);

                targetXZ = ClampToGroundBounds(
                    targetXZ, groundCenterWorld, groundRotation, groundHalfExtentsWorld, _camera.orthographicSize);
            }

            Vector3 pos = transform.position;
            pos.x = targetXZ.x;
            pos.z = targetXZ.z;
            transform.position = pos; // Y, rotation, clip planes, culling mask all untouched.
        }

        private bool TryGetPlayer(out Transform player)
        {
            if (_player != null)
            {
                player = _player;
                return true;
            }

            var go = GameObject.FindWithTag("Player");
            if (go == null)
            {
                player = null;
                return false;
            }

            _player = go.transform;
            player = _player;
            return true;
        }

        private void CacheGroundMeshBounds()
        {
            if (_ground == null)
            {
                Debug.LogError($"[MinimapCameraFollow] {name}: _ground is not assigned — the minimap " +
                                "will follow the player with no edge clamp, so void may become visible " +
                                "near the level's boundary.", this);
                _groundBoundsValid = false;
                return;
            }

            // Deliberately read the MeshCollider's shared mesh, not the MeshFilter's. Ground is
            // marked Batching Static, and Unity's static batching replaces a batched renderer's
            // MeshFilter.sharedMesh at runtime with one combined mesh covering the whole static
            // batch (bounds spanning every batched object, not just Ground) — confirmed live: at
            // runtime MeshFilter.sharedMesh.bounds here reads as a "Combined Mesh (root: scene)"
            // with real height, nothing like the flat 5x5 Plane authored in the scene. Static
            // batching never touches colliders, so MeshCollider.sharedMesh reliably stays the real,
            // unmodified Plane asset (Center (0,0,0), Extents (5,0,5)) in both Edit and Play mode.
            var meshCollider = _ground.GetComponent<MeshCollider>();
            if (meshCollider == null || meshCollider.sharedMesh == null)
            {
                Debug.LogError($"[MinimapCameraFollow] {name}: _ground ('{_ground.name}') has no " +
                                "MeshCollider/mesh to read bounds from — edge clamp disabled.", this);
                _groundBoundsValid = false;
                return;
            }

            // Local (unscaled) mesh bounds only — safe to cache once, a mesh asset's bounds are
            // immutable at runtime. World-space center/rotation/extents are deliberately NOT cached
            // here; see the field comment on _groundBoundsValid for why.
            _groundLocalBounds = meshCollider.sharedMesh.bounds;
            _groundBoundsValid = true;
        }

        /// <summary>
        /// Pure, Unity-lifecycle-free clamp core — kept separate so it can be exercised directly
        /// (reflection, or a future EditMode test) without a live Camera/scene. Clamps
        /// <paramref name="worldPos"/>'s X/Z so a square capture of half-size <paramref name="margin"/>,
        /// axis-aligned to world X/Z (this camera never yaws — it only ever looks straight down),
        /// stays entirely within the ground rectangle described by <paramref name="groundCenter"/>/
        /// <paramref name="groundRotation"/>/<paramref name="groundHalfExtents"/>.
        /// </summary>
        /// <remarks>
        /// Clamping the camera's centre to (ground half-extent - margin) along the ground's own
        /// local axes is only correct if the captured square shares those axes. Here it doesn't: the
        /// capture window is fixed to world X/Z while the ground can be yawed (this project's street
        /// is 45°) — so a world-axis-aligned square's corners can poke outside a rotated rectangle's
        /// edge even while its centre sits a full <c>margin</c> inside that edge measured naively.
        /// The fix is a support-function projection: for a ground-local axis unit vector <c>u</c>,
        /// the square's extent along <c>u</c> is <c>margin * (|u.x| + |u.z|)</c> (a square's support
        /// in any direction is its half-size times the L1 norm of that direction in the square's own
        /// axes). At the ground's actual yaw this factor is >= 1 whenever the ground isn't world-axis
        /// aligned (exactly root-2 at 45°), so it strictly supersedes the naive margin — never smaller.
        /// This is the necessary and sufficient condition (derived, not just heuristic): a corner of
        /// the square maximises |offset . u| at <c>|offsetLocal . u| + margin*(|u.x|+|u.z|)</c>, so
        /// keeping that at or under the ground's half-extent for both local axes keeps every corner
        /// inside the rectangle.
        /// </remarks>
        internal static Vector3 ClampToGroundBounds(
            Vector3 worldPos, Vector3 groundCenter, Quaternion groundRotation, Vector3 groundHalfExtents, float margin)
        {
            Vector3 localXAxis = groundRotation * Vector3.right;
            Vector3 localZAxis = groundRotation * Vector3.forward;

            float marginOnLocalX = margin * (Mathf.Abs(localXAxis.x) + Mathf.Abs(localXAxis.z));
            float marginOnLocalZ = margin * (Mathf.Abs(localZAxis.x) + Mathf.Abs(localZAxis.z));

            Vector3 offsetWorld = worldPos - groundCenter;
            Vector3 offsetLocal = Quaternion.Inverse(groundRotation) * offsetWorld;

            float maxX = Mathf.Max(0f, groundHalfExtents.x - marginOnLocalX);
            float maxZ = Mathf.Max(0f, groundHalfExtents.z - marginOnLocalZ);
            offsetLocal.x = Mathf.Clamp(offsetLocal.x, -maxX, maxX);
            offsetLocal.z = Mathf.Clamp(offsetLocal.z, -maxZ, maxZ);

            return groundCenter + groundRotation * offsetLocal;
        }
    }
}
