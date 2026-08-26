using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Cheap idle "look at me" motion for world pickups (e.g. cardboard piles) that would
    /// otherwise blend into a busy scene. Adds a slow vertical bob plus a steady spin.
    ///
    /// Pure transform math each frame: no allocations, no GetComponent calls, no material
    /// instancing. Safe to have many active at once. Purely visual - does not touch pickup
    /// logic (see CardboardPickup), so it has no effect on when/whether a pickup is collected.
    /// </summary>
    public class PickupIdleFX : MonoBehaviour
    {
        [Tooltip("How far the pickup bobs up and down from its starting position, in local units.")]
        [SerializeField] private float _bobHeight = 0.12f;

        [Tooltip("Bob cycles per second (full up-and-down loops).")]
        [SerializeField] private float _bobSpeed = 1.2f;

        [Tooltip("Idle spin speed in degrees per second around the world up axis.")]
        [SerializeField] private float _spinDegreesPerSecond = 40f;

        private Vector3 _basePosition;
        private float _phaseOffset;

        private void Awake()
        {
            _basePosition = transform.localPosition;
            // Randomize phase so multiple pickups visible at once don't bob in lockstep.
            _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float bobOffset = Mathf.Sin((Time.time * _bobSpeed * Mathf.PI * 2f) + _phaseOffset) * _bobHeight;
            Vector3 position = _basePosition;
            position.y += bobOffset;
            transform.localPosition = position;

            transform.Rotate(Vector3.up, _spinDegreesPerSecond * Time.deltaTime, Space.World);
        }
    }
}
