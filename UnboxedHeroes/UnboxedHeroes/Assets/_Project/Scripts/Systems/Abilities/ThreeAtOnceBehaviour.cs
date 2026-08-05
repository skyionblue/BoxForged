using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Shurikens Legendary — "Three at Once".
    /// Spawns three shurikens in a fan: -15°, 0°, and +15° from player forward direction.
    /// Reads the shuriken prefab and stats directly from the active weapon's ShurikenAbilityData
    /// if available; falls back to the serialized prefab override on this SO.
    /// </summary>
    [CreateAssetMenu(fileName = "ThreeAtOnceBehaviour",
                     menuName = "Boxhead/Abilities/Behaviours/ThreeAtOnce")]
    public class ThreeAtOnceBehaviour : AbilityBehaviour
    {
        [Header("Shuriken Override (used when weapon has no ShurikenAbilityData)")]
        [SerializeField] private GameObject _fallbackPrefab;
        [SerializeField] private int        _fallbackDamage = 10;
        [SerializeField] private float      _projectileSpeed = 18f;
        [SerializeField] private float      _projectileLifetime = 4f;

        private static readonly float[] FanAngles = { -15f, 0f, 15f };

        public override void Execute(AbilityExecutionContext ctx)
        {
            Vector3 spawnPos = ctx.WeaponPosition + Vector3.up * 0.3f;
            Vector3 baseDir = ctx.PlayerForward;
            if (baseDir == Vector3.zero) baseDir = Vector3.forward;

            for (int i = 0; i < FanAngles.Length; i++)
            {
                Vector3 dir = Quaternion.Euler(0f, FanAngles[i], 0f) * baseDir;
                SpawnShuriken(spawnPos, dir);
            }
        }

        private void SpawnShuriken(Vector3 muzzlePos, Vector3 direction)
        {
            GameObject prefab = _fallbackPrefab;
            int dmg = _fallbackDamage;

            if (prefab == null)
            {
                Debug.LogWarning("[ThreeAtOnceBehaviour] No shuriken prefab assigned.", this);
                return;
            }

            Quaternion rot = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
            GameObject go = Object.Instantiate(prefab, muzzlePos, rot);

            if (go.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = direction * _projectileSpeed;

            if (go.TryGetComponent<ShurikenProjectile>(out var shuriken))
                shuriken.Init(dmg);

            Object.Destroy(go, _projectileLifetime);
        }
    }
}
