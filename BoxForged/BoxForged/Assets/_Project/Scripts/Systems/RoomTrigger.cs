using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Thin trigger volume placed at each room entrance. Notifies RoomManager
    /// when the player steps through so the room's enemies can be activated.
    ///
    /// Scene setup: BoxCollider.isTrigger MUST be checked in the Inspector.
    /// MeshRenderer should be disabled so the volume is invisible at runtime.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class RoomTrigger : MonoBehaviour
    {
        [SerializeField] private int roomIndex;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            RoomManager.Instance?.OnRoomEntered(roomIndex);
        }
    }
}
