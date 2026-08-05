using UnityEngine;
using UnityEngine.Pool;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    // Handles both damage-on-hit and pool release for water burst projectiles.
    // Do NOT put a Destroy-on-lifetime script (e.g. BossProjectile) on water burst prefabs.
    internal sealed class WaterBurstReturn : MonoBehaviour
    {
        [SerializeField] private int _damage = 10;
        [SerializeField] private bool _isParryable = true;
        [SerializeField] private GameObject _impactPrefab;

        private IObjectPool<GameObject> _pool;
        private float _lifetime;
        private float _elapsed;
        private bool _hasHit;

        internal void Init(IObjectPool<GameObject> pool, float lifetime)
        {
            _pool     = pool;
            _lifetime = lifetime;
            _elapsed  = 0f;
            _hasHit   = false;
        }

        private void OnEnable()
        {
            _elapsed = 0f;
            _hasHit  = false;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime && _pool != null)
            {
                if (_impactPrefab != null)
                    Object.Instantiate(_impactPrefab, transform.position, Quaternion.identity);
                _pool.Release(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;
            if (!other.CompareTag("Player")) return;

            _hasHit = true;

            var combat = other.GetComponentInParent<CombatController>();
            if (combat != null)
                combat.TryReceiveAttack(_damage, _isParryable);

            if (_pool != null)
            {
                if (_impactPrefab != null)
                    Object.Instantiate(_impactPrefab, transform.position, Quaternion.identity);
                _pool.Release(gameObject);
            }
        }
    }
}
