using UnityEngine;
using Boxhead.Player;

namespace Boxhead.UI
{
    /// <summary>
    /// Shows the correct character bust on HUD_StyleIconFrame when the player
    /// confirms a fighting style on the run-start screen.
    /// Ninja (ShadowDash) → Bust_Ninja active; Cowboy (Tumbleshot) → Bust_Cowboy active.
    /// </summary>
    public class StyleBustSwapper : MonoBehaviour
    {
        [SerializeField] private GameObject _bustNinja;
        [SerializeField] private GameObject _bustCowboy;

        private CombatController _combat;

        private void Start()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null && playerGO.TryGetComponent(out _combat))
                _combat.OnStyleChanged += HandleStyleChanged;
        }

        private void OnDestroy()
        {
            if (_combat != null)
                _combat.OnStyleChanged -= HandleStyleChanged;
        }

        private void HandleStyleChanged(FightingStyleData style)
        {
            if (style == null) return;

            bool isNinja = style.SpecialMove == SpecialMoveType.ShadowDash;
            if (_bustNinja  != null) _bustNinja.SetActive(isNinja);
            if (_bustCowboy != null) _bustCowboy.SetActive(!isNinja);
        }
    }
}
