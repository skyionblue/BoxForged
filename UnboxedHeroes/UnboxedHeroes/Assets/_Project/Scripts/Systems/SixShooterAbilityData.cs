using System.Collections;
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Systems
{
    // NOTE: _shotsRemaining is an instance field on this ScriptableObject asset.
    // This is correct for a single-player game where only one player uses this SO at a time.
    // If multi-player were added, per-instance state must move to the player's MonoBehaviour.
    [CreateAssetMenu(menuName = "Boxhead/Abilities/SixShooter")]
    public class SixShooterAbilityData : WeaponAbilityData
    {
        [SerializeField] private int        maxShots        = 6;
        [SerializeField] private float      reloadDuration  = 1.2f;
        [SerializeField] private float      bulletSpeed     = 20f;
        [SerializeField] private int        bulletDamage    = 8;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float      bulletLifetime  = 3f;
        [SerializeField] private GameObject muzzleFlashPrefab;

        private int            _shotsRemaining;
        private WaitForSeconds _waitReload;
        private Coroutine      _reloadCoroutine;
        private bool           _isReloading;
        private float          _reloadStartTime;

        // Fan the Hammer fires shots at these horizontal offsets (degrees) — 6 angles to match maxShots = 6.
        private static readonly float[] FanAngles = { -12.5f, -7.5f, -2.5f, 2.5f, 7.5f, 12.5f };

        public override float ProgressFraction
        {
            get
            {
                if (_isReloading)
                    return Mathf.Clamp01((Time.time - _reloadStartTime) / reloadDuration);
                return 1f; // ready to fire next shot
            }
        }

        public override bool FiresOnAttackButton => true;

        // Block activation while reloading so the CombatController never calls Activate
        // during reload — prevents the special meter from depleting and the button from blacking out.
        public override bool IsReadyToActivate => !_isReloading;

        private void OnEnable()
        {
            _shotsRemaining  = maxShots;
            _waitReload      = new WaitForSeconds(reloadDuration);
            _reloadCoroutine = null;
            _isReloading     = false;
        }

        public override IEnumerator Activate(AbilityActivationContext ctx, CombatController combat)
        {
            if (bulletPrefab == null)
            {
                Debug.LogWarning("[SixShooterAbilityData] bulletPrefab is not assigned.", combat);
                yield break;
            }

            // Guard against stale SO state when domain reload is disabled (Enter Play Mode Options).
            // _waitReload being null means OnEnable never ran this session — reinitialize.
            if (_waitReload == null)
            {
                _shotsRemaining  = maxShots;
                _waitReload      = new WaitForSeconds(reloadDuration);
                _reloadCoroutine = null;
                _isReloading     = false;
            }

            // IsReadyToActivate blocks calls during reload, so this path is only hit
            // in the rare one-frame gap between the last shot and ReloadRoutine starting.
            // Just ensure reload is running and return immediately — no meter consumed.
            if (_shotsRemaining <= 0)
            {
                if (!_isReloading && _reloadCoroutine == null)
                    _reloadCoroutine = combat.StartCoroutine(ReloadRoutine());
                yield break;
            }

            if (ctx.IsCounterWindow)
            {
                FanTheHammer(ctx);
                _shotsRemaining = 0;
                if (_reloadCoroutine != null) combat.StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = combat.StartCoroutine(ReloadRoutine());
            }
            else
            {
                FireOneBullet(ctx);
                _shotsRemaining--;

                if (_shotsRemaining <= 0)
                {
                    if (_reloadCoroutine != null) combat.StopCoroutine(_reloadCoroutine);
                    _reloadCoroutine = combat.StartCoroutine(ReloadRoutine());
                }
            }
        }

        private void FireOneBullet(AbilityActivationContext ctx)
        {
            // Spawn from chest height, offset forward — consistent regardless of weapon grip position.
            Vector3 spawnPos = ctx.PlayerPosition + Vector3.up * 0.8f + ctx.PlayerForward * 0.5f;
            SpawnBullet(spawnPos, ctx.PlayerForward);
        }

        private void FanTheHammer(AbilityActivationContext ctx)
        {
            for (int i = 0; i < FanAngles.Length; i++)
            {
                Vector3 dir = Quaternion.Euler(0f, FanAngles[i], 0f) * ctx.PlayerForward;
                SpawnBullet(ctx.MuzzlePosition, dir);
            }
        }

        private void SpawnBullet(Vector3 muzzlePos, Vector3 direction)
        {
            // The bullet mesh long axis is +Y; rotate -90° around local X so +Y faces the travel direction.
            Quaternion bulletRot = Quaternion.LookRotation(direction) * Quaternion.Euler(-90f, 0f, 0f);
            GameObject bullet = Object.Instantiate(bulletPrefab, muzzlePos, bulletRot);
            if (bullet.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = direction * bulletSpeed;
            Object.Destroy(bullet, bulletLifetime);

            if (muzzleFlashPrefab != null)
                Object.Instantiate(muzzleFlashPrefab, muzzlePos, Quaternion.LookRotation(direction));
        }

        private IEnumerator ReloadRoutine()
        {
            _isReloading     = true;
            _reloadStartTime = Time.time;
            yield return _waitReload;
            _isReloading     = false;
            _shotsRemaining  = maxShots;
            _reloadCoroutine = null;
        }
    }
}
