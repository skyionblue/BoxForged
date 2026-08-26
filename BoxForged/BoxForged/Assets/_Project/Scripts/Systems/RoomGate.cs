using UnityEngine;
using UnityEngine.AI;

namespace Boxhead.Systems
{
    /// <summary>
    /// Controls the visibility, collision, and (optionally) NavMesh carving of a door/gate
    /// primitive that blocks passage between rooms. Toggled by RoomManager (or, in single-scene
    /// worlds like CulDeSac_WildWestCity, a scene-local director) when a room/zone is cleared.
    /// Open/Close traverse the full child hierarchy so child visual, collider, and obstacle
    /// GameObjects (e.g. GateVisual) are included — root-only calls missed them.
    ///
    /// NavMeshObstacle is optional (GetComponentsInChildren returns an empty array when none
    /// exists, so this is a no-op for gates without one). A gate that permanently blocks a
    /// walkable corridor needs one — mirrors the covered-wagon carving setup in
    /// CulDeSac_WildWestCity (NavMeshModifier.ignoreFromBuild = true so the runtime NavMesh
    /// bake treats the corridor as walkable ground, plus a carving NavMeshObstacle so the
    /// closed gate carves the corridor unwalkable while it exists). Disabling the obstacle on
    /// Open() releases that carve immediately so enemies/AI can path through the now-open gate;
    /// re-enabling it on Close() restores the block (docs/BACKLOG.md B2).
    /// </summary>
    public class RoomGate : MonoBehaviour
    {
        public void Open()
        {
            foreach (var col in GetComponentsInChildren<Collider>(true))
                col.enabled = false;
            foreach (var rend in GetComponentsInChildren<Renderer>(true))
                rend.enabled = false;
            foreach (var obstacle in GetComponentsInChildren<NavMeshObstacle>(true))
                obstacle.enabled = false;
        }

        public void Close()
        {
            foreach (var col in GetComponentsInChildren<Collider>(true))
                col.enabled = true;
            foreach (var rend in GetComponentsInChildren<Renderer>(true))
                rend.enabled = true;
            foreach (var obstacle in GetComponentsInChildren<NavMeshObstacle>(true))
                obstacle.enabled = true;
        }
    }
}
