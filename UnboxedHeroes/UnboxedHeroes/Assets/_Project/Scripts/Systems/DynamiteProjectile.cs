using System.Collections;
using UnityEngine;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    [RequireComponent(typeof(Rigidbody))]
    public class DynamiteProjectile : MonoBehaviour
    {
        [SerializeField] private float      detonationRadius    = 2.5f;
        [SerializeField] private int        damage              = 25;
        [SerializeField] private LayerMask  enemyLayerMask;
        [SerializeField] private float      lifetimeAfterBounce = 0.3f;
        [SerializeField] private float      spinSpeed           = 540f;
        [SerializeField] private GameObject explosionVFX;

        private Rigidbody      _rb;
        private bool           _hasBounced;
        private Collider[]     _buffer = new Collider[8];
        private WaitForSeconds _waitDetonate;

        private void Awake()
        {
            _rb           = GetComponent<Rigidbody>();
            _waitDetonate = new WaitForSeconds(lifetimeAfterBounce);
        }

        private void Update()
        {
            transform.Rotate(spinSpeed * Time.deltaTime, 0f, spinSpeed * 0.6f * Time.deltaTime, Space.Self);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_hasBounced) return;

            _hasBounced = true;
            Vector3 normal = collision.contacts[0].normal;
            _rb.linearVelocity   = Vector3.Reflect(_rb.linearVelocity, normal) * 0.4f;
            StartCoroutine(Detonate());
        }

        private IEnumerator Detonate()
        {
            yield return _waitDetonate;

            if (explosionVFX != null)
                Object.Instantiate(explosionVFX, transform.position, Quaternion.identity);

            int count = Physics.OverlapSphereNonAlloc(transform.position, detonationRadius, _buffer, enemyLayerMask);
            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                if (!_buffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;
                stats.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
