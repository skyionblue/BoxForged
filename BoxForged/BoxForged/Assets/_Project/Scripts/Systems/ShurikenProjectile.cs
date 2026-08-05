using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class ShurikenProjectile : MonoBehaviour
    {
        [SerializeField] private int   damage    = 10;
        [SerializeField] private float spinSpeed = 720f;

        private Rigidbody _rb;
        private bool _bounceEnabled;
        private bool _hasBounced;

        // Layer mask built once in Awake — wall check uses ~EnemyLayer so any non-enemy
        // surface triggers the bounce. This avoids LayerMask.NameToLayer in per-frame code.
        private static readonly int EnemyLayerIndex = 7; // "Enemy" layer; adjust if project differs

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Called by ShurikenAbilityData immediately after instantiation to
        /// override the prefab's inspector damage with the SO's projectileDamage value.
        /// </summary>
        public void Init(int damageOverride) => damage = damageOverride;

        /// <summary>
        /// Called by FoldAndReturnBehaviour to activate the one-bounce return path.
        /// </summary>
        public void EnableBounce() => _bounceEnabled = true;

        private void Update()
        {
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Always damage enemies regardless of bounce state.
            if (other.CompareTag("Enemy"))
            {
                if (other.TryGetComponent<EnemyStats>(out var stats) && !stats.IsDead)
                    stats.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            // Bounce path: only active when FoldAndReturnBehaviour called EnableBounce().
            if (_bounceEnabled && !_hasBounced)
            {
                // Reflect velocity off the surface normal for a realistic ricochet.
                // ContactPoint is not available from OnTriggerEnter, so we use a simple
                // 180° reversal — sufficient for the ability's design intent.
                if (_rb != null)
                    _rb.linearVelocity = -_rb.linearVelocity;
                _hasBounced = true;
                return;
            }

            // Second non-enemy collision (or bounce not enabled): destroy.
            Destroy(gameObject);
        }
    }
}
