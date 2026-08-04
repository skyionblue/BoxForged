using System.Collections;
using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Enemy
{
    [RequireComponent(typeof(Collider))]
    public class MarshalBullet : MonoBehaviour
    {
        [SerializeField] private float _speed    = 8f;
        [SerializeField] private float _lifetime = 3f;

        private int  _damage;
        private bool _isParryable;
        private bool _hasHit;

        private WaitForSeconds _waitLifetime;

        public void Init(int damage, bool isParryable)
        {
            _damage     = damage;
            _isParryable = isParryable;
        }

        private void Awake()
        {
            _waitLifetime = new WaitForSeconds(_lifetime);

            // Ensure the collider is a trigger
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Start()
        {
            StartCoroutine(LifetimeRoutine());
        }

        private void Update()
        {
            transform.position += transform.forward * (_speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;
            if (!other.CompareTag("Player")) return;

            _hasHit = true;

            var combat = other.GetComponentInParent<CombatController>();
            if (combat != null)
                combat.TryReceiveAttack(_damage, _isParryable);

            Destroy(gameObject);
        }

        private IEnumerator LifetimeRoutine()
        {
            yield return _waitLifetime;
            Destroy(gameObject);
        }
    }
}
