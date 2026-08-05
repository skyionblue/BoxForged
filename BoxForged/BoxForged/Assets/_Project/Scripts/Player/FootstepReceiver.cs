using UnityEngine;

namespace Boxhead.Player
{
    // Silences animation event warnings from RPG Mecanim clips (FootR, FootL, Hit).
    // Extend later: FootR/FootL → footstep audio; Hit → weapon hit detection.
    public sealed class FootstepReceiver : MonoBehaviour
    {
        private void FootR() { }
        private void FootL() { }
        private void Hit() { }
    }
}
