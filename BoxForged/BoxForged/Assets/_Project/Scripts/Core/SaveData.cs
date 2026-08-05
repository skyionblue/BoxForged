using System;

namespace Boxhead.Core
{
    [Serializable]
    public class SaveData
    {
        public int      sparkTotal         = 0;
        public string[] permanentUnlocks   = new string[0];
        public string   lastFightingStyle  = "";
        public int      totalRunsCompleted = 0;
        public int      characterLevel     = 0;
        public int[]    statLevels         = new int[5];  // one per stat, Phase 2 prep
        public string[] completedQuestIds  = new string[0]; // Phase 2 prep
        // 0 = Cul-de-Sac not yet cleared; 1 = Town Square unlocked (Cul-de-Sac boss beaten)
        public int      highestZoneReached = 0;
        public int      version            = 1;
    }
}
