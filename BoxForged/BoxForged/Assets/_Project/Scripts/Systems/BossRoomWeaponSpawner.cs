using System.Collections;
using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Spawns random weapon pickup prefabs above the arena on Start, then animates
    /// each one falling to a designer-placed spawn point on the floor.
    /// Colliders are disabled during the fall so the player cannot grab a mid-air weapon.
    /// </summary>
    public class BossRoomWeaponSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] _weaponPickupPrefabs;
        [SerializeField] private Transform[]  _spawnPoints;
        [SerializeField] private int          _spawnCount  = 3;

        [Header("Drop Animation")]
        [SerializeField] private float _dropHeight   = 12f;
        [SerializeField] private float _dropDuration = 1.2f;
        [SerializeField] private float _dropStagger  = 0.2f;

        // Pre-allocated shuffle index array — no runtime allocation after Awake.
        private int[] _shuffleIndices;

        // Cached yield — reused by all DropWeapon coroutines for stagger waits.
        private WaitForSeconds _waitStagger;

        private void Awake()
        {
            int pointCount = _spawnPoints != null ? _spawnPoints.Length : 0;
            _shuffleIndices = new int[pointCount];
            for (int i = 0; i < pointCount; i++)
                _shuffleIndices[i] = i;

            _waitStagger = new WaitForSeconds(_dropStagger);
        }

        private void Start()
        {
            ClearPlayerWeapons();
            SpawnWeapons();
        }

        // Boss hall always starts the player disarmed — weapons come from sky drops only.
        private void ClearPlayerWeapons()
        {
            var player = UnityEngine.GameObject.FindWithTag("Player");
            if (player == null) return;
            if (!player.TryGetComponent<Boxhead.Systems.Inventory>(out var inv)) return;
            inv.Drop(); // clears equipped, promotes backpack
            inv.Drop(); // clears promoted backpack (if any)
        }

        private void SpawnWeapons()
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0) return;
            if (_weaponPickupPrefabs == null || _weaponPickupPrefabs.Length == 0) return;

            int count = _spawnCount;
            if (count > _spawnPoints.Length)         count = _spawnPoints.Length;
            if (count > _weaponPickupPrefabs.Length) count = _weaponPickupPrefabs.Length;
            if (count <= 0) return;

            // Fisher-Yates partial shuffle — O(count), not O(total).
            int total = _shuffleIndices.Length;
            for (int i = 0; i < count; i++)
            {
                int j    = Random.Range(i, total);
                int temp = _shuffleIndices[i];
                _shuffleIndices[i] = _shuffleIndices[j];
                _shuffleIndices[j] = temp;
            }

            for (int i = 0; i < count; i++)
            {
                Transform point = _spawnPoints[_shuffleIndices[i]];
                if (point == null) continue;

                int        prefabIndex = Random.Range(0, _weaponPickupPrefabs.Length);
                GameObject prefab      = _weaponPickupPrefabs[prefabIndex];
                if (prefab == null) continue;

                Vector3    dropStart = point.position + Vector3.up * _dropHeight;
                GameObject spawned   = Instantiate(prefab, dropStart, point.rotation);

                StartCoroutine(DropWeapon(spawned, point.position, i));
            }
        }

        /// <summary>
        /// Staggered fall: waits staggerIndex * _dropStagger before falling, then
        /// lerps from above to the landing point using a quadratic ease-in curve
        /// that mimics gravitational acceleration.
        /// </summary>
        private IEnumerator DropWeapon(GameObject weapon, Vector3 landPos, int staggerIndex)
        {
            // Stagger: wait staggerIndex steps, each step = _dropStagger seconds.
            for (int s = 0; s < staggerIndex; s++)
            {
                if (weapon == null) yield break;
                yield return _waitStagger;
            }

            if (weapon == null) yield break;

            // Disable the pickup trigger so the player cannot grab a mid-air weapon.
            var col = weapon.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Vector3 startPos = weapon.transform.position;
            float   elapsed  = 0f;

            while (elapsed < _dropDuration)
            {
                if (weapon == null) yield break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _dropDuration);
                // t² ease-in: slow at top, fast near impact.
                weapon.transform.position = Vector3.Lerp(startPos, landPos, t * t);
                yield return null;
            }

            if (weapon == null) yield break;
            weapon.transform.position = landPos;
            if (col != null) col.enabled = true;
        }
    }
}
