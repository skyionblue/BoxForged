using UnityEngine;

namespace Boxhead.Core
{
    /// <summary>
    /// Silences Unity's "Function 'FootL' not found" warnings produced by RPG
    /// Character Mecanim animation events baked into locomotion clips.
    /// Add this component to any character root that uses those clips.
    /// Wire actual footstep audio here once SoundEvent.PlayerFootstep is added.
    /// </summary>
    public class FootstepReceiver : MonoBehaviour
    {
        // Called by animation events on every left-foot plant.
        private void FootL() { }

        // Called by animation events on every right-foot plant.
        private void FootR() { }

        // Called by animation events on attack clips (e.g. Unarmed-Attack-L1 Hit event).
        private void Hit() { }
    }
}
