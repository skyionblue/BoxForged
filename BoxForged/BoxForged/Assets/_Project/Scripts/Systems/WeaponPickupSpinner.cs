using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Slowly rotates the weapon visual inside a pickup bubble.
    /// Attach to the WeaponVisual child of a WeaponPickup GO.
    /// </summary>
    public class WeaponPickupSpinner : MonoBehaviour
    {
        [SerializeField] private float _spinSpeed = 90f;

        // Random speed multiplier so pickups don't all spin identically.
        private float _speedMult;

        private void Awake()
        {
            _speedMult = Random.Range(0.6f, 1.4f);
            // Randomise starting rotation so bubbles don't all begin aligned.
            transform.Rotate(0f, Random.Range(0f, 360f), 0f, Space.Self);
        }

        private void Update()
        {
            transform.Rotate(_spinSpeed * _speedMult * Time.deltaTime, 0f, 0f, Space.Self);
        }
    }
}
