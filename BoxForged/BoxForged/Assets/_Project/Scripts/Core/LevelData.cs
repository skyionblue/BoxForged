using UnityEngine;

namespace Boxhead.Core
{
    [CreateAssetMenu(menuName = "BoxForged/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("XP Thresholds")]
        [Tooltip("XP required to reach each level. Index 0 = level 1 threshold.")]
        [SerializeField] private int[] _xpThresholds = { 50, 120, 220, 350, 520, 730, 990, 1300, 1670, 2100 };

        /// <summary>
        /// Returns the XP threshold for the given level (1-based).
        /// Returns int.MaxValue if the level is out of range (no further leveling).
        /// </summary>
        public int GetThreshold(int level) =>
            (level >= 1 && level <= _xpThresholds.Length) ? _xpThresholds[level - 1] : int.MaxValue;
    }
}
