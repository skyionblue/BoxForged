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
            // Randomise starting rotation so pickups don't all begin aligned.
            transform.Rotate(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f), Space.World);
        }

        private void Update()
        {
            // Spin on all three axes for a floating tumble effect.
            float delta = _spinSpeed * _speedMult * Time.deltaTime;
            transform.Rotate(delta, delta * 0.7f, delta * 0.4f, Space.World);
        }
    }
}
