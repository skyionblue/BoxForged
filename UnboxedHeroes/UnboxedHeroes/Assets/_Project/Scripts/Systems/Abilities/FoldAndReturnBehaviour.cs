using UnityEngine;

namespace Boxhead.Systems
{
    /// <summary>
    /// Shurikens Epic — "Fold and Return".
    /// On Execute (OnHit trigger), enables bounce mode on all currently-live ShurikenProjectiles.
    /// Each shuriken bounces once off a non-enemy surface, then destroys normally on the next
    /// collision. ShurikenProjectile must have BounceModeEnabled called to activate this path.
    /// </summary>
    [CreateAssetMenu(fileName = "FoldAndReturnBehaviour",
                     menuName = "Boxhead/Abilities/Behaviours/FoldAndReturn")]
    public class FoldAndReturnBehaviour : AbilityBehaviour
    {
        public override void Execute(AbilityExecutionContext ctx)
        {
            // Enable bounce on every live shuriken in the scene.
            // FindObjectsByType uses the non-allocating sorted variant; the result array is
            // temporary (managed) but Execute is called only once per ability trigger,
            // so this is acceptable — it is NOT in a per-frame path.
            var projectiles = Object.FindObjectsByType<ShurikenProjectile>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < projectiles.Length; i++)
                projectiles[i].EnableBounce();
        }
    }
}
