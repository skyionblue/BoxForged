using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Magic Wand Legendary — "All Eight".
    /// OnSpecial trigger. Tracks how many times this ability has fired since equip.
    /// Every 5th cast fires shurikens in all 8 cardinal and intercardinal directions
    /// simultaneously; all other casts fire a single forward shuriken.
    ///
    /// _castCount is an instance field on the ScriptableObject — ScriptableObjects are
    /// asset instances shared across play sessions in the Editor, but they are separate
    /// instances from the asset file in play mode. OnEquipped resets the counter so it
    /// always starts at 0 when the weapon is first equipped.
    ///
    /// Shuriken prefab and speed are serialized here rather than read from WeaponAbilityData
    /// (which belongs to the V3 inline ability system) — this behaviour is wired via AbilitySO.
    /// </summary>
    [CreateAssetMenu(fileName = "BHV_AllEight",
                     menuName = "Boxhead/Abilities/AllEightBehaviour")]
    public class AllEightBehaviour : AbilityBehaviour
    {
        [Header("Projectile")]
        [SerializeField] private GameObject _shurikenPrefab;
        [SerializeField] private float      _projectileSpeed   = 18f;
        [SerializeField] private float      _projectileLifetime = 4f;

        [Header("Pattern")]
        [SerializeField] private int _burstEveryNCasts = 5;

        // Per-equip cast counter. Resets in OnEquipped so unequipping and re-equipping
        // always starts the cycle fresh.
        private int _castCount;

        // 8-direction angles in the XZ plane, starting at forward (0°) and stepping 45°.
        private static readonly float[] AllEightAngles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

        public override void OnEquipped(AbilityExecutionContext ctx)
        {
            _castCount = 0;
        }

        public override void OnUnequipped()
        {
            _castCount = 0;
        }

        public override void Execute(AbilityExecutionContext ctx)
        {
            _castCount++;

            // magnitude on the AbilitySO is the per-shuriken damage value.
            float magnitude = ctx.ActiveWeapon?.Data.legendaryAbility?.magnitude ?? 20f;
            int damage = Mathf.RoundToInt(magnitude);

            Vector3 spawnPos = ctx.WeaponPosition + Vector3.up * 0.3f;
            Vector3 baseDir  = ctx.PlayerForward;
            if (baseDir == Vector3.zero) baseDir = Vector3.forward;

            if (_castCount % _burstEveryNCasts == 0)
            {
                // Burst cast: fire in all 8 directions.
                Debug.Log($"[AllEightBehaviour] All Eight burst on cast #{_castCount}.");
                for (int i = 0; i < AllEightAngles.Length; i++)
                {
                    Vector3 dir = Quaternion.Euler(0f, AllEightAngles[i], 0f) * baseDir;
                    SpawnProjectile(spawnPos, dir, damage);
                }
            }
            else
            {
                // Standard cast: single forward projectile.
                SpawnProjectile(spawnPos, baseDir, damage);
            }
        }

        private void SpawnProjectile(Vector3 spawnPos, Vector3 direction, int damage)
        {
            if (_shurikenPrefab == null)
            {
                Debug.LogWarning("[AllEightBehaviour] No shuriken prefab assigned — assign pfb_shuriken_projectile.", this);
                return;
            }

            Quaternion rot = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
            GameObject go = Object.Instantiate(_shurikenPrefab, spawnPos, rot);

            if (go.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = direction * _projectileSpeed;

            if (go.TryGetComponent<ShurikenProjectile>(out var shuriken))
                shuriken.Init(damage);

            Object.Destroy(go, _projectileLifetime);
        }
    }
}
