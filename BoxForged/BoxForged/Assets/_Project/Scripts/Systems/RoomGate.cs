using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Controls the visibility and collision of a door/gate primitive that blocks
    /// passage between rooms. Toggled by RoomManager when a room is cleared.
    /// Open/Close traverse the full child hierarchy so child visual and collider
    /// GameObjects (e.g. GateVisual) are included — root-only calls missed them.
    /// </summary>
    public class RoomGate : MonoBehaviour
    {
        public void Open()
        {
            foreach (var col in GetComponentsInChildren<Collider>(true))
                col.enabled = false;
            foreach (var rend in GetComponentsInChildren<Renderer>(true))
                rend.enabled = false;
        }

        public void Close()
        {
            foreach (var col in GetComponentsInChildren<Collider>(true))
                col.enabled = true;
            foreach (var rend in GetComponentsInChildren<Renderer>(true))
                rend.enabled = true;
        }
    }
}
