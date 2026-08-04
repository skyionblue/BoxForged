using UnityEngine;

namespace Boxhead.Player
{
    /// <summary>
    /// Projectile fired by the Cowboy fighting style's Tumbleshot special.
    /// Travels in a straight line, pierces one enemy, then destroys itself.
    /// Damage, speed, and range are set by CombatController at spawn time.
    ///
    /// Hit detection uses Physics.SphereCastNonAlloc rather than OnTriggerEnter because
    /// Unity does not generate trigger events between two kinematic Rigidbodies (e.g. the
    /// bullet and SpinCycle's kinematic root), so the trigger approach silently misses bosses.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class TumbleshotBullet : MonoBehaviour
    {
        [HideInInspector] public int   Damage;
        [HideInInspector] public float Speed;
        [HideInInspector] public float MaxRange;

        private const float CastRadius = 0.4f;

        private bool  _hasPierced;
        private float _travelled;

        // Pre-allocated cast buffer — bullet is a single-target pierce, 4 slots is plenty.
        private readonly RaycastHit[] _castBuffer = new RaycastHit[4];

        private void Awake()
        {
            // Rigidbody required by the component attribute; kept kinematic so physics doesn't
            // fight the manual translation in Update.
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;

            // Collider is kept as a visual-only trigger (non-physical); hit detection is done
            // by SphereCastNonAlloc which works regardless of the target's Rigidbody settings.
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            float step = Speed * Time.deltaTime;

            // Cast a sphere from the previous position toward travel direction before moving.
            // This catches enemies the bullet passes through in a single frame.
            int hitCount = Physics.SphereCastNonAlloc(
                transform.position,
                CastRadius,
                transform.forward,
                _castBuffer,
                step,
                ~0,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider other = _castBuffer[i].collider;
                if (!other.CompareTag("Enemy")) continue;

                // Use GetComponentInParent so a collider on a child bone still resolves to
                // the EnemyStats on the enemy root (e.g. SpinCycle's separate collider objects).
                var stats = other.GetComponentInParent<Enemy.EnemyStats>();
                if (stats == null || stats.IsDead) continue;

                stats.TakeDamage(Damage);

                if (_hasPierced)
                {
                    Destroy(gameObject);
                    return;
                }
                _hasPierced = true;
                // Continue the cast loop — only one new target per frame due to the pierce flag.
                break;
            }

            transform.Translate(Vector3.forward * step, Space.Self);
            _travelled += step;
            if (_travelled >= MaxRange)
                Destroy(gameObject);
        }
    }
}
