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

        // Called by ShurikenAbilityData immediately after instantiation to
        // override the prefab's inspector damage with the SO's projectileDamage value.
        public void Init(int damageOverride) => damage = damageOverride;

        private void Update()
        {
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            if (!other.TryGetComponent<EnemyStats>(out var stats)) return;
            if (stats.IsDead) return;

            stats.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
