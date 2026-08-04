using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Place on a world GameObject with a trigger Collider.
    /// When the player walks into the trigger, cardboard is added to the
    /// player's CardboardResource and the pickup is disabled.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CardboardPickup : MonoBehaviour
    {
        [SerializeField] private int _amount = 3;

        // Cached lazily on first trigger: this is a world pickup, not attached to the player
        // hierarchy, so resolving in Awake would always return null.
        private CardboardResource _cardboardResource;

        public void SetAmount(int amount) { _amount = amount; }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (_cardboardResource == null)
                _cardboardResource = other.GetComponentInParent<CardboardResource>();

            if (_cardboardResource == null) return;

            _cardboardResource.Add(_amount);
            gameObject.SetActive(false);
        }
    }
}
