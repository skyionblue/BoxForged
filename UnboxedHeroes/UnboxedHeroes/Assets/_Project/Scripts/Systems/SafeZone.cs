using UnityEngine;

namespace Boxhead.Systems
{
    [RequireComponent(typeof(Collider))]
    public class SafeZone : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            other.GetComponentInParent<BoxSystem>()?.EnterSafeZone();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            other.GetComponentInParent<BoxSystem>()?.ExitSafeZone();
        }
    }
}
