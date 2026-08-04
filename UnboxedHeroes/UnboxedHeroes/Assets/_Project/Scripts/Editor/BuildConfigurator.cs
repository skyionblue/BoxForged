using UnityEditor;
using UnityEngine;

namespace Boxhead.Editor
{
    /// <summary>
    /// Menu items for switching between profiling and release iOS build configurations.
    /// Access via Unboxed Heroes → Build in the Unity menu bar.
    /// </summary>
    public static class BuildConfigurator
    {
        private const string MenuRoot = "Unboxed Heroes/Build/";

        // ── Profiling Build ───────────────────────────────────────────────────

        [MenuItem(MenuRoot + "Configure: Profiling Build")]
        public static void ConfigureProfilingBuild()
        {
            EditorUserBuildSettings.development              = true;
            EditorUserBuildSettings.connectProfiler         = true;
            EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
            EditorUserBuildSettings.allowDebugging          = true;

            PlayerSettings.SetIl2CppCompilerConfiguration(
                BuildTargetGroup.iOS, Il2CppCompilerConfiguration.Debug);

            AssetDatabase.SaveAssets();
            Debug.Log("[BuildConfigurator] Profiling Build configured.\n" +
                      "Development=true  AutoconnectProfiler=true  IL2CPP=Debug\n" +
                      "Next: File → Build Settings → Build, run from Xcode, " +
                      "then Window → Analysis → Profiler in the Editor.");
        }

        // ── Release Build ─────────────────────────────────────────────────────

        [MenuItem(MenuRoot + "Configure: Release Build (TestFlight)")]
        public static void ConfigureReleaseBuild()
        {
            EditorUserBuildSettings.development              = false;
            EditorUserBuildSettings.connectProfiler         = false;
            EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
            EditorUserBuildSettings.allowDebugging          = false;

            PlayerSettings.SetIl2CppCompilerConfiguration(
                BuildTargetGroup.iOS, Il2CppCompilerConfiguration.Release);

            AssetDatabase.SaveAssets();
            Debug.Log("[BuildConfigurator] Release Build configured.\n" +
                      "Development=false  AutoconnectProfiler=false  IL2CPP=Release\n" +
                      "Next: File → Build Settings → Build, open in Xcode, " +
                      "Product → Archive → Distribute to TestFlight.");
        }

        // ── Status check ──────────────────────────────────────────────────────

        [MenuItem(MenuRoot + "Show Current Build Config")]
        public static void ShowCurrentConfig()
        {
            var mode = EditorUserBuildSettings.development ? "PROFILING" : "RELEASE";
            Debug.Log($"[BuildConfigurator] Current config: {mode}\n" +
                      $"  Development Build:     {EditorUserBuildSettings.development}\n" +
                      $"  Autoconnect Profiler:  {EditorUserBuildSettings.connectProfiler}\n" +
                      $"  Allow Debugging:       {EditorUserBuildSettings.allowDebugging}\n" +
                      $"  IL2CPP Config:         {PlayerSettings.GetIl2CppCompilerConfiguration(BuildTargetGroup.iOS)}");
        }
    }
}
