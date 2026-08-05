using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class SixShooterBullet : MonoBehaviour
    {
        [SerializeField] private int damage = 8;

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
