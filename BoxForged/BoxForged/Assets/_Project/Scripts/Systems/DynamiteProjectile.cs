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
        private Collider[]     _buffer       = new Collider[8];

        // Separate buffer for the spread pass — avoids aliasing with the enemy damage buffer.
        // Allocated once per instance, not per detonation.
        private readonly Collider[] _spreadBuffer = new Collider[8];

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

            // Bigger Bang ability multiplies explosion radius when active
            float radius = detonationRadius * AbilityExecutor.ActiveExplosionRadiusMult;
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _buffer, enemyLayerMask);
            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].CompareTag("Enemy")) continue;
                if (!_buffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;
                stats.TakeDamage(damage);
            }

            // "It Spreads" passive: chain the explosion to nearby cardboard pickups.
            // Runs only when the Dynamite Legendary ability is equipped.
            if (DynamiteSpreadBehaviour.SpreadActive)
            {
                float spreadRadius = radius * 2f;
                int spreadCount = Physics.OverlapSphereNonAlloc(
                    transform.position, spreadRadius, _spreadBuffer, ~0);

                for (int i = 0; i < spreadCount; i++)
                {
                    var cardboard = _spreadBuffer[i].GetComponent<CardboardPickup>();
                    if (cardboard == null) continue;

                    if (explosionVFX != null)
                        Object.Instantiate(explosionVFX, _spreadBuffer[i].transform.position, Quaternion.identity);

                    Object.Destroy(_spreadBuffer[i].gameObject);
                }
            }

            Destroy(gameObject);
        }
    }
}
