using System.Collections;
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Systems
{
    [CreateAssetMenu(menuName = "Boxhead/Abilities/DynamiteBundle")]
    public class DynamiteBundleAbilityData : WeaponAbilityData
    {
        [SerializeField] private float     windUpDuration    = 1.2f;
        [SerializeField] private float     projectileSpeed   = 8f;
        [SerializeField] private float     detonationRadius  = 2.5f;
        [SerializeField] private int       damage            = 25;
        [SerializeField] private GameObject projectilePrefab;

        private const float MaxFlightTime = 5f;

        private WaitForSeconds _waitWindUp;
        private GameObject     _activeProjectile;

        public override bool FiresOnAttackButton => true;

        // Blocks re-throw while a dynamite is still in flight or hasn't exploded yet.
        // Unity's == null returns true on a destroyed GameObject, so this clears automatically
        // when DynamiteProjectile calls Destroy(gameObject).
        public override bool IsReadyToActivate
        {
            get
            {
                if (_activeProjectile == null)
                {
                    _activeProjectile = null; // release ghost reference for GC
                    return true;
                }
                return false;
            }
        }

        private void OnEnable()
        {
            _waitWindUp       = new WaitForSeconds(windUpDuration);
            _activeProjectile = null;
        }

        public override IEnumerator Activate(AbilityActivationContext ctx, CombatController combat)
        {
            // Play the punch/attack animation as the throw wind-up starts.
            combat.TriggerAttackAnimation();

            // Wind-up: player is in SpecialAttacking state during this yield.
            // A dodge during wind-up calls StopActive() on CombatController, killing this coroutine —
            // no explicit cancel check is needed here.
            yield return _waitWindUp;

            if (projectilePrefab == null)
            {
                Debug.LogWarning("[DynamiteBundleAbilityData] projectilePrefab is not assigned — skipping throw.", combat);
                yield break;
            }

            // Spawn at current player position at throw time (not pre-wind-up capture)
            // so the dynamite exits from wherever the player actually is now.
            Vector3 spawnPos = combat.transform.position + Vector3.up * 1.0f + (-combat.transform.forward) * 0.5f;
            _activeProjectile = Object.Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            if (_activeProjectile.TryGetComponent<Rigidbody>(out var rb))
            {
                // Use the player's facing direction at throw time, not the pre-wind-up
                // capture — avoids stale direction when the player turns during wind-up.
                // -combat.transform.forward = visual facing (Y=180° child offset).
                Vector3 forward  = -combat.transform.forward;
                Vector3 throwDir = (forward + Vector3.up * 0.4f).normalized;
                rb.linearVelocity = throwDir * projectileSpeed;
            }

            // Poll until the dynamite explodes (DynamiteProjectile calls Destroy) or the safety
            // timeout elapses. IsReadyToActivate stays false while _activeProjectile is alive,
            // preventing a second throw regardless of what state the coroutine is in.
            float elapsed = 0f;
            while (_activeProjectile != null && elapsed < MaxFlightTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            _activeProjectile = null;
        }
    }
}
