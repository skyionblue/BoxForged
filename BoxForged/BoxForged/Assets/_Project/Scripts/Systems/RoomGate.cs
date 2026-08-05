using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Controls the visibility and collision of a door/gate primitive that blocks
    /// passage between rooms. Toggled by RoomManager when a room is cleared.
    /// RequireComponent ensures GetComponent never returns null — Open/Close
    /// are unconditional so missing components fail loudly at edit time.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class RoomGate : MonoBehaviour
    {
        private Collider _col;
        private MeshRenderer _rend;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            // MeshRenderer is optional (a gate could be invisible geometry),
            // so we still null-check it rather than RequireComponent.
            _rend = GetComponent<MeshRenderer>();
        }

        public void Open()
        {
            _col.enabled = false;
            if (_rend != null) _rend.enabled = false;
        }

        public void Close()
        {
            _col.enabled = true;
            if (_rend != null) _rend.enabled = true;
        }
    }
}
