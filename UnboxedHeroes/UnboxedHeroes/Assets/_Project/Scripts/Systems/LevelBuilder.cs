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
            SpawnWeaponPickups();
            SpawnCardboardPiles();
            SpawnWorkbenches();
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
        }
#endif
    }
}
