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

            _buildNavMeshRoutine = null;
        }

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
