using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Boxhead.Systems
{
    public class LevelBuilder : MonoBehaviour
    {
        [Header("Level Data")]
        [SerializeField] private WeaponDropTableSO _dropTable;

        [Header("Prop Prefabs")]
        [SerializeField] private GameObject _workbenchPrefab;
        [SerializeField] private GameObject _cardboardPilePrefab;

        [Header("Spawn Container")]
        [SerializeField] private Transform _spawnRoot;

        [Header("Camera Clearance (Editor validation only — ADR-0001 §2.7)")]
        [Tooltip("Layers that count as camera-blocking level geometry (walls, buildings, ceilings). Defaults to the Building layer only — matches CameraOcclusion's own _wallMask. Declared outside the UNITY_EDITOR guard (only ValidateCameraClearance's logic below is editor-only) so a serialized field does not appear/disappear across build configurations, which would otherwise create a prefab/scene serialization mismatch.")]
        [SerializeField] private LayerMask _cameraClearanceMask = 1 << 8; // "Building" layer (index 8 per ProjectSettings/TagManager.asset) — the same layer CameraOcclusion._wallMask resolves via LayerMask.GetMask("Building") in its own Awake(). Hardcoded here (rather than a GetMask() field initializer) so the value is a plain, inspectable serialized int like any other default; re-check the index if "Building" is ever moved in Edit > Project Settings > Tags and Layers.

        private NavMeshSurface _navMeshSurface;
        private Coroutine _buildNavMeshRoutine;

        private void Start()
        {
            SpawnEnvProps();
            SpawnWeaponPickups();
            SpawnCardboardPiles();
            SpawnWorkbenches();

            _buildNavMeshRoutine = StartCoroutine(BuildNavMeshDeferred());
        }

        private void OnDestroy()
        {
            if (_buildNavMeshRoutine != null)
            {
                StopCoroutine(_buildNavMeshRoutine);
                _buildNavMeshRoutine = null;
            }
        }

        // Runtime NavMesh bake. Deferred by one frame because freshly-Instantiated
        // colliders/meshes are not fully registered on the same frame they spawn.
        private IEnumerator BuildNavMeshDeferred()
        {
            yield return null;

            if (_navMeshSurface == null)
            {
                Transform parent = _spawnRoot != null ? _spawnRoot : transform;
                var surfaceGo = new GameObject("RuntimeNavMeshSurface");
                surfaceGo.transform.SetParent(parent, false);
                _navMeshSurface = surfaceGo.AddComponent<NavMeshSurface>();
            }

            // Collect the whole scene from physics colliders. RenderMeshes requires every
            // source mesh to be Read/Write enabled (FBX meshes usually are not, so they get
            // silently skipped and props fail to carve). PhysicsColliders bakes from colliders,
            // and the ENV building MeshColliders back the walkable/obstacle geometry.
            _navMeshSurface.collectObjects = CollectObjects.All;
            _navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            // Make this runtime bake the sole navmesh: strip any legacy baked data from the
            // scene so agents cannot path through props via stale navmesh geometry.
            NavMesh.RemoveAllNavMeshData();

            _navMeshSurface.BuildNavMesh();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Diagnostic only. CalculateTriangulation allocates managed arrays and the string
            // interpolation allocates, so keep both out of shipping mobile builds.
            NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
            Debug.Log($"[LevelBuilder] Runtime NavMesh baked: {tri.vertices.Length} verts, {tri.indices.Length / 3} tris.");
#endif

#if UNITY_EDITOR
            ValidateCameraClearance();
#endif

            _buildNavMeshRoutine = null;
        }

#if UNITY_EDITOR
        // ADR-0001 §2.7: the follow camera is fixed at 0 yaw/roll with a permanent 36° pitch —
        // every room sees the same camera axis, so "behind" is always world -Z and "above" is
        // always world +Y, regardless of where the player stands. There is no deoccluder in
        // this design (deliberately — see the ADR), so camera clearance is a level-design
        // constraint instead: every walkable point needs >= 8 m clear behind it and >= 6 m
        // clear above it along that fixed axis, or the camera (or its view) clips level geometry.
        //
        // This is intentionally a diagnostic, not a hard failure — LevelBuilder only spawns
        // props/pickups/workbenches onto scene-authored room geometry it does not own (that
        // extraction is separate work, ADR-0002/B3). It cannot fix a violation, only report one
        // loudly so a level author catches it before it reaches a build.
        //
        // Deliberately mask-restricted to _cameraClearanceMask (Building layer by default, see
        // the field above) rather than every layer, and summarised into one warning per category
        // rather than one error-with-stack-trace per vertex (see B33) — an unrestricted mask over
        // thousands of NavMesh triangulation vertices, each firing up to two Debug.LogError calls,
        // was flooding the console and adding a multi-second Play Mode entry hang.
        private const float RequiredClearBehind = 8f;
        private const float RequiredClearAbove  = 6f;
        private static readonly Vector3 CameraBehindDirection = Vector3.back; // world -Z, fixed yaw 0
        private const float GroundEpsilon = 0.1f; // lift above the navmesh surface before raycasting up

        private void ValidateCameraClearance()
        {
            NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
            if (tri.vertices == null || tri.vertices.Length == 0) return;

            int behindViolations = 0;
            int aboveViolations  = 0;

            for (int i = 0; i < tri.vertices.Length; i++)
            {
                Vector3 liftedPoint = tri.vertices[i] + Vector3.up * GroundEpsilon;

                if (Physics.Raycast(liftedPoint, CameraBehindDirection, RequiredClearBehind,
                        _cameraClearanceMask, QueryTriggerInteraction.Ignore))
                    behindViolations++;

                if (Physics.Raycast(liftedPoint, Vector3.up, RequiredClearAbove,
                        _cameraClearanceMask, QueryTriggerInteraction.Ignore))
                    aboveViolations++;
            }

            if (behindViolations > 0 || aboveViolations > 0)
            {
                Debug.LogWarning(
                    $"[LevelBuilder] Camera clearance ('{gameObject.scene.name}', ADR-0001 §2.7 — " +
                    $"diagnostic only, does not block Play Mode): {behindViolations} of {tri.vertices.Length} " +
                    $"NavMesh vertices have < {RequiredClearBehind:F1}m clear behind, {aboveViolations} of " +
                    $"{tri.vertices.Length} have < {RequiredClearAbove:F1}m clear above.", this);
            }
        }
#endif

        private void SpawnEnvProps()
        {
            if (_dropTable == null) return;

            EnvPropSpawnEntry[] props = _dropTable.envProps;
            if (props == null) return;

            for (int i = 0; i < props.Length; i++)
            {
                if (props[i].prefab == null)
                {
                    Debug.LogWarning($"[LevelBuilder] envProps[{i}] has null prefab — skipping.", this);
                    continue;
                }
                // Use the prefab's baked root rotation so FBX axis corrections are preserved,
                // then rotate only around world Y to set facing direction.
                GameObject go = Instantiate(props[i].prefab,
                                            props[i].worldPosition,
                                            props[i].prefab.transform.rotation,
                                            _spawnRoot);
                if (props[i].eulerRotation.y != 0f)
                    go.transform.Rotate(0f, props[i].eulerRotation.y, 0f, Space.World);
                if (props[i].localScale != Vector3.one)
                    go.transform.localScale = props[i].localScale;
            }
        }

        private void SpawnWeaponPickups()
        {
            if (_dropTable == null)
            {
                Debug.LogWarning("[LevelBuilder] _dropTable not assigned.", this);
                return;
            }

            WeaponSpawnEntry[] scattered = _dropTable.scatteredObjects;
            WeaponSpawnEntry[] lootZone = _dropTable.lootZoneObjects;

            for (int i = 0; i < scattered.Length; i++)
            {
                SpawnWeaponEntry(scattered[i]);
            }

            for (int i = 0; i < lootZone.Length; i++)
            {
                SpawnWeaponEntry(lootZone[i]);
            }
        }

        private void SpawnWeaponEntry(WeaponSpawnEntry entry)
        {
            if (entry.weaponObject == null)
            {
                Debug.LogWarning("[LevelBuilder] WeaponSpawnEntry has null weaponObject — skipping.", this);
                return;
            }

            if (entry.weaponObject.rawObjectPrefab == null)
            {
                Debug.LogWarning($"[LevelBuilder] WeaponObjectSO '{entry.weaponObject.name}' has no rawObjectPrefab — skipping.", this);
                return;
            }

            Instantiate(entry.weaponObject.rawObjectPrefab, entry.worldPosition, Quaternion.identity, _spawnRoot);
        }

        private void SpawnCardboardPiles()
        {
            if (_cardboardPilePrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] _cardboardPilePrefab not assigned.", this);
                return;
            }

            if (_dropTable == null) return;

            CardboardSpawnEntry[] piles = _dropTable.cardboardPiles;

            for (int i = 0; i < piles.Length; i++)
            {
                GameObject go = Instantiate(_cardboardPilePrefab, piles[i].worldPosition, Quaternion.identity, _spawnRoot);

                if (go.TryGetComponent(out CardboardPickup pickup))
                {
                    pickup.SetAmount(piles[i].amount);
                }
                else
                {
                    Debug.LogWarning($"[LevelBuilder] _cardboardPilePrefab has no CardboardPickup component — amount not applied at index {i}.", this);
                }
            }
        }

        private void SpawnWorkbenches()
        {
            if (_workbenchPrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] _workbenchPrefab not assigned.", this);
                return;
            }

            if (_dropTable == null) return;

            Vector3[] positions = _dropTable.workbenchPositions;

            for (int i = 0; i < positions.Length; i++)
            {
                Instantiate(_workbenchPrefab, positions[i], Quaternion.identity, _spawnRoot);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_dropTable == null)
            {
                Debug.LogWarning("[LevelBuilder] _dropTable is not assigned.", this);
                return;
            }

            if (_workbenchPrefab == null && _dropTable.workbenchPositions != null && _dropTable.workbenchPositions.Length > 0)
                Debug.LogWarning("[LevelBuilder] _workbenchPrefab is not assigned but the drop table has workbench positions.", this);

            if (_cardboardPilePrefab == null && _dropTable.cardboardPiles != null && _dropTable.cardboardPiles.Length > 0)
                Debug.LogWarning("[LevelBuilder] _cardboardPilePrefab is not assigned but the drop table has cardboard pile entries.", this);

            if (_dropTable != null && _dropTable.envProps != null)
            {
                for (int i = 0; i < _dropTable.envProps.Length; i++)
                {
                    if (_dropTable.envProps[i].prefab == null)
                        Debug.LogWarning($"[LevelBuilder] envProps[{i}] has a null prefab reference.", this);
                }
            }
        }
#endif
    }
}
