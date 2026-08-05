using UnityEngine;
using Boxhead.Core;
using Boxhead.Player;
using Boxhead.Systems;

namespace Boxhead.Enemy
{
    /// <summary>
    /// Shared projectile for ClothesBall (Phase 1) and SudsBlob (Phase 2).
    /// Call Initialize() immediately after Instantiate to inject the player reference.
    /// Checks ProjectileDeflector.IsActive on player collision — if active, reverses
    /// velocity instead of dealing damage (Lightsaber Legendary ability).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class BossProjectile : MonoBehaviour
    {
        [Header("Combat")]
        [SerializeField] private int   _damage      = 20;
        [SerializeField] private float _lifetime    = 4f;
        [SerializeField] private bool  _isParryable = false;

        private CombatController _playerCombat;
        private Rigidbody _rb;
        private float _timer;
        private bool _destroyed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        /// <summary>Called by SpinCycleAI after Instantiate to avoid FindWithTag per projectile.</summary>
        public void Initialize(CombatController playerCombat)
        {
            _playerCombat = playerCombat;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
                SelfDestruct();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // SendItBack (Lightsaber Legendary): deflect instead of dealing damage.
            if (ProjectileDeflector.IsActive)
            {
                ProjectileDeflector.IsActive = false;
                if (_rb != null)
                    _rb.linearVelocity = -_rb.linearVelocity;
                // Swap tag so the reversed projectile can hit enemies instead of the player.
                gameObject.tag = "PlayerProjectile";
                return;
            }

            if (_playerCombat != null)
            {
                AttackResult result = _playerCombat.TryReceiveAttack(_damage, _isParryable);
                if (result == AttackResult.Hit)
                    AudioManager.Instance?.Play(SoundEvent.EnemyHit);
            }

            SelfDestruct();
        }

        private void SelfDestruct()
        {
            if (_destroyed) return;
            _destroyed = true;
            Destroy(gameObject);
        }
    }
}
