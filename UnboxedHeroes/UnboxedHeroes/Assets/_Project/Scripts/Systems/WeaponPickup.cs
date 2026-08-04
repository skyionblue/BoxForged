using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Place on a world GameObject with a trigger Collider.
    /// When the player walks into the trigger, the raw weapon object is added
    /// to the player's WeaponInventory material bag. The pickup is disabled on
    /// success and left active when the bag is full.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WeaponPickup : MonoBehaviour
    {
        [SerializeField] private WeaponObjectSO _weaponObject;

        // Cached lazily on first trigger: the pickup is a world object, not a child of the
        // player, so GetComponentInParent at Awake would resolve nothing. Caching on the
        // first valid trigger contact avoids per-frame GetComponent cost.
        private WeaponInventory _weaponInventory;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (_weaponInventory == null)
                _weaponInventory = other.GetComponentInParent<WeaponInventory>();

            if (_weaponInventory == null) return;

            bool added = _weaponInventory.AddToMaterialBag(_weaponObject);

            if (!added)
            {
                Debug.Log($"[WeaponPickup] Material bag full — {_weaponObject.rawObjectName} not picked up.");
                return;
            }

            gameObject.SetActive(false);
        }
    }
}
