namespace Boxhead.Player
{
    /// <summary>
    /// Additive per-run stat bonuses applied on top of CharacterStatsSO base values.
    /// Reset to zero at the start of each run; rebuilt from SaveData.statLevels[]
    /// by ProgressionSystem.RebuildOverlay(). The ScriptableObject is never mutated.
    /// </summary>
    public struct StatOverlay
    {
        public int   maxHealthBonus;
        public int   attackPowerBonus;
        public float agilityBonus;
        public float luckBonus;
        public int   defenseBonus;

        public static StatOverlay Zero => default;
    }
}
