using System.Collections;
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Systems
{
    // NOTE: _throwsRemaining is an instance field on this ScriptableObject asset.
    // This is correct for a single-player game where only one player uses this SO at a time.
    // If multi-player were added, per-instance state must move to the player's MonoBehaviour.
    [CreateAssetMenu(menuName = "Boxhead/Abilities/Shuriken")]
    public class ShurikenAbilityData : WeaponAbilityData
    {
        [SerializeField] private float      projectileSpeed   = 18f;
        [SerializeField] private int        projectileDamage  = 10;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float      projectileLifetime = 4f;

        public override bool FiresOnAttackButton => true;

        public override IEnumerator Activate(AbilityActivationContext ctx, CombatController combat)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("[ShurikenAbilityData] projectilePrefab is not assigned.", combat);
                yield break;
            }

            // Spawn from chest height, offset forward — same pattern as SixShooter.
            Vector3 spawnPos = ctx.PlayerPosition + Vector3.up * 0.8f + ctx.PlayerForward * 0.5f;
            SpawnShuriken(spawnPos, ctx.PlayerForward);
        }

        private void SpawnShuriken(Vector3 muzzlePos, Vector3 direction)
        {
            if (direction == Vector3.zero) direction = Vector3.forward;
            // LookRotation aligns +Z with throw direction; Euler(90,0,0) then tilts the flat
            // mesh into the horizontal plane so it spins like a frisbee rather than a coin.
            Quaternion rot = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
            GameObject projectile = Object.Instantiate(projectilePrefab, muzzlePos, rot);

            if (projectile.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = direction * projectileSpeed;

            if (projectile.TryGetComponent<ShurikenProjectile>(out var shuriken))
                shuriken.Init(projectileDamage);

            Object.Destroy(projectile, projectileLifetime);
        }
    }
}
