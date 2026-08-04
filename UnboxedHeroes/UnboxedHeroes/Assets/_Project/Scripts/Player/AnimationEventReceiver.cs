using UnityEngine;

namespace Boxhead.Player
{
    /// <summary>
    /// Receives RPG Mecanim animation events (FootR, FootL, Hit) fired by the
    /// character model's Animator. Must be on the same GameObject as the Animator
    /// (the character model child) — Unity SendMessage does not propagate to parents.
    /// </summary>
    public class AnimationEventReceiver : MonoBehaviour
    {
        private void FootR() { }
        private void FootL() { }
        private void Hit()   { }
    }
}
