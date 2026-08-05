using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Boxhead.Enemy;

namespace Boxhead.Systems
{
    /// <summary>
    /// Lasso Legendary — "Spin and Throw".
    /// OnHit trigger. Grabs the nearest enemy within 4m, turns it into a physics projectile,
    /// and hurls it at the second-nearest enemy within 8m. After 1s the thrown enemy deals
    /// impact damage to any enemy within 1m of its landing position.
    ///
    /// Physics approach: temporarily disables the NavMeshAgent, adds a Rigidbody (or reuses
    /// one), applies an impulse toward the target, then after the flight window resolves impact
    /// damage and restores normal AI. The NavMeshAgent is disabled while the Rigidbody drives
    /// position — the two components must not be active simultaneously.
    /// </summary>
    [CreateAssetMenu(fileName = "BHV_SpinAndThrow",
                     menuName = "Boxhead/Abilities/SpinAndThrowBehaviour")]
    public class SpinAndThrowBehaviour : AbilityBehaviour
    {
        [SerializeField] private float _grabRadius   = 4f;
        [SerializeField] private float _targetRadius = 8f;
        [SerializeField] private float _throwForce   = 15f;
        [SerializeField] private float _flightTime   = 1f;
        [SerializeField] private float _impactRadius = 1f;
        [SerializeField] private float _baseDamage   = 30f;   // overridden by magnitude at runtime

        // Pre-allocated search buffers — zero GC in Execute.
        // _searchBuffer is used only within Execute (synchronous) — safe to share on the SO.
        // ThrowRoutine allocates its own local impact buffers to prevent aliasing if the
        // coroutine somehow runs concurrently on two different CombatController instances.
        private readonly Collider[] _searchBuffer = new Collider[8];

        // Cached WaitForSeconds — allocated once on first Execute call (not during a scene hot path).
        private WaitForSeconds _waitFlight;

        private WaitForSeconds FlightWait
        {
            get
            {
                if (_waitFlight == null) _waitFlight = new WaitForSeconds(_flightTime);
                return _waitFlight;
            }
        }

        public override void Execute(AbilityExecutionContext ctx)
        {
            float magnitude = ctx.ActiveWeapon?.Data.legendaryAbility?.magnitude ?? _baseDamage;

            // Find the nearest enemy to grab.
            int count = Physics.OverlapSphereNonAlloc(
                ctx.PlayerPosition, _grabRadius, _searchBuffer, ctx.EnemyLayer);

            EnemyStats grabbed = null;
            float nearestSq = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!_searchBuffer[i].CompareTag("Enemy")) continue;
                if (!_searchBuffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;

                float sq = (_searchBuffer[i].transform.position - ctx.PlayerPosition).sqrMagnitude;
                if (sq >= nearestSq) continue;

                nearestSq = sq;
                grabbed = stats;
            }

            if (grabbed == null) return;

            // Find a second enemy within the larger radius to use as the throw target.
            int count2 = Physics.OverlapSphereNonAlloc(
                ctx.PlayerPosition, _targetRadius, _searchBuffer, ctx.EnemyLayer);

            EnemyStats target = null;
            float secondNearestSq = float.MaxValue;

            for (int i = 0; i < count2; i++)
            {
                if (!_searchBuffer[i].CompareTag("Enemy")) continue;
                if (!_searchBuffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;
                if (stats == grabbed) continue;  // skip the grabbed enemy

                float sq = (_searchBuffer[i].transform.position - ctx.PlayerPosition).sqrMagnitude;
                if (sq >= secondNearestSq) continue;

                secondNearestSq = sq;
                target = stats;
            }

            // If no second target, throw in the player's forward direction as a punting throw.
            Vector3 throwTarget = target != null
                ? target.transform.position
                : ctx.PlayerPosition + ctx.PlayerForward * _targetRadius;

            // Delegate the physics throw to CombatController's StartCoroutine.
            ctx.Combat.StartCoroutine(ThrowRoutine(grabbed, throwTarget, magnitude, ctx.EnemyLayer));
        }

        private IEnumerator ThrowRoutine(EnemyStats grabbed, Vector3 throwTarget, float magnitude, LayerMask enemyLayer)
        {
            if (grabbed == null) yield break;

            // Disable the NavMeshAgent so it doesn't fight the Rigidbody for position authority.
            NavMeshAgent agent = grabbed.GetComponent<NavMeshAgent>();
            IEnemyBehavior behavior = grabbed.GetComponent<IEnemyBehavior>();
            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            // Add or reuse a Rigidbody on the grabbed enemy.
            Rigidbody rb = grabbed.GetComponent<Rigidbody>();
            bool addedRb = rb == null;
            if (addedRb)
                rb = grabbed.gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = true;

            // Compute throw direction: arc toward the target with a slight upward component.
            // Keep upward component small (0.2) so the enemy lands near the ground, not above it.
            Vector3 throwDir = (throwTarget - grabbed.transform.position).normalized + Vector3.up * 0.2f;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(throwDir.normalized * _throwForce, ForceMode.Impulse);

            yield return FlightWait;

            if (grabbed == null) yield break;

            // Impact: deal damage to any enemy within impactRadius of the thrown enemy's position.
            // Local buffers here — ScriptableObject fields must not hold coroutine-frame state.
            Vector3 landPos = grabbed.transform.position;
            Collider[]   impactBuffer = new Collider[4];
            EnemyStats[] impactStats  = new EnemyStats[4];
            int hitCount = Physics.OverlapSphereNonAlloc(landPos, _impactRadius, impactBuffer, enemyLayer);
            int resolved = 0;

            for (int i = 0; i < hitCount; i++)
            {
                if (!impactBuffer[i].CompareTag("Enemy")) continue;
                if (!impactBuffer[i].TryGetComponent<EnemyStats>(out var stats)) continue;
                if (stats.IsDead) continue;

                bool seen = false;
                for (int j = 0; j < resolved; j++)
                    if (impactStats[j] == stats) { seen = true; break; }
                if (seen) continue;
                if (resolved >= impactStats.Length) break;

                impactStats[resolved++] = stats;
                stats.TakeDamage(Mathf.RoundToInt(magnitude));
            }

            Debug.Log($"[SpinAndThrowBehaviour] Spin and Throw: impact hit {resolved} enemies for {magnitude} damage.");

            // Restore the enemy: remove the temporary Rigidbody and re-enable the agent.
            if (addedRb && rb != null)
                Object.Destroy(rb);
            else if (rb != null)
                rb.isKinematic = true;

            if (agent != null && grabbed != null)
            {
                agent.enabled = true;
                agent.isStopped = false;
            }
        }
    }
}
