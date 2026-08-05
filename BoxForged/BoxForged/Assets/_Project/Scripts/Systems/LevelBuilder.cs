using UnityEngine;

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

        private void Start()
        {
            SpawnEnvProps();
            SpawnWeaponPickups();
            SpawnCardboardPiles();
            SpawnWorkbenches();
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
