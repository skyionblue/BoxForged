using UnityEngine;

namespace Boxhead.Core
{
    /// <summary>
    /// Persistent "seen" flags for one-shot cutscenes, backed by PlayerPrefs.
    /// Keys are namespaced under "cutscene." so they never collide with other prefs.
    /// Kept deliberately tiny — cutscene triggers ask HasSeen/MarkSeen and nothing else.
    /// </summary>
    public static class CutsceneFlags
    {
        private const string Prefix = "cutscene.";

        /// <summary>True once <see cref="MarkSeen"/> has been called for this key (and saved to disk).</summary>
        public static bool HasSeen(string key)
        {
            return PlayerPrefs.GetInt(Prefix + key, 0) != 0;
        }

        /// <summary>Persists that a one-shot cutscene identified by <paramref name="key"/> has played.</summary>
        public static void MarkSeen(string key)
        {
            PlayerPrefs.SetInt(Prefix + key, 1);
            PlayerPrefs.Save();
        }

        /// <summary>Editor/debug helper — clears a single flag so the cutscene replays.</summary>
        public static void ClearSeen(string key)
        {
            PlayerPrefs.DeleteKey(Prefix + key);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Clears every one-shot cutscene flag so all "first time" cutscenes play again.
        /// Called by the pause-screen Reset Progression button for a true fresh-start.
        /// </summary>
        public static void ClearAll()
        {
            PlayerPrefs.DeleteKey(Prefix + CutsceneCatalog.KeyGameIntro);
            PlayerPrefs.DeleteKey(Prefix + CutsceneCatalog.KeyCulDeSacEnter);
            PlayerPrefs.DeleteKey(Prefix + CutsceneCatalog.KeyNinjaShowcase);
            PlayerPrefs.DeleteKey(Prefix + CutsceneCatalog.KeyCowboyShowcase);
            PlayerPrefs.DeleteKey(Prefix + CutsceneCatalog.KeyForgeFirst);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Central registry of cutscene file names and their play-once flag keys, so clips are
    /// swappable in one place (swap the alternate constants below to ship a different cut).
    /// File names are relative to StreamingAssets/Cutscenes/.
    /// </summary>
    public static class CutsceneCatalog
    {
        // ── Game intro (once ever) ──────────────────────────────────────────────
        public const string GameIntro       = "unboxed_intro.mp4";              // landscape ~2min — game opening
        public const string GameIntroAlt     = "boy_putting_box_on.mp4";        // landscape 10s (freed alt)
        public const string GameIntroAlt2    = "boy_putting_box_on_v2.mp4";     // portrait 30s (pillarboxed)
        public const string KeyGameIntro     = "game_intro";

        // ── Enter Cul-de-Sac zone (once per zone) ───────────────────────────────
        public const string CulDeSacEnter    = "wild_west_transform.mp4";
        public const string CulDeSacEnterAlt = "wild_west_change_phone.mp4";
        public const string KeyCulDeSacEnter = "culdesac_enter";

        // ── Character select showcases (once per character) ─────────────────────
        public const string NinjaShowcase    = "ninja_skills.mp4";
        public const string CowboyNinjaShowcase = "cowboy_ninja_skills.mp4";
        public const string KeyNinjaShowcase = "showcase_ninja";
        public const string KeyCowboyShowcase = "showcase_cowboy";

        // ── SpinCycle boss intro (every encounter, skippable) ───────────────────
        public const string SpinCycleStandoff = "spincycle_standoff.mp4";

        // ── First forge (once, tutorial) ────────────────────────────────────────
        public const string ForgeFirst      = "forge_whip_craft.mp4";
        public const string ForgeFirstAlt   = "forge_whip_2.mp4";
        public const string KeyForgeFirst   = "forge_first";
    }
}
