using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace Boxhead.Editor
{
    /// <summary>
    /// Runs automatically after every iOS build. Stamps the Apple development team
    /// onto every target and every build configuration in the generated Xcode project.
    /// </summary>
    public static class iOSPostBuildProcessor
    {
        private const string TeamID = "V62D5FT8F5";

        [PostProcessBuild(1)]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS) return;

#if UNITY_IOS
            string projPath = PBXProject.GetPBXProjectPath(buildPath);
            var proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            // Collect every known target Unity generates — avoids GetAllTargetGuids()
            // which is not available in all Xcode package versions.
            var targetGuids = new List<string>();

            string main      = proj.GetUnityMainTargetGuid();
            string framework = proj.GetUnityFrameworkTargetGuid();
            if (!string.IsNullOrEmpty(main))      targetGuids.Add(main);
            if (!string.IsNullOrEmpty(framework)) targetGuids.Add(framework);

            // Test target and any additional named targets
            foreach (var name in new[] { "Unity-iPhone Tests", "UnityFramework", "GameAssembly" })
            {
                string g = proj.TargetGuidByName(name);
                if (!string.IsNullOrEmpty(g) && !targetGuids.Contains(g))
                    targetGuids.Add(g);
            }

            foreach (var guid in targetGuids)
            {
                proj.SetTeamId(guid, TeamID);
                proj.SetBuildProperty(guid, "DEVELOPMENT_TEAM",         TeamID);
                proj.SetBuildProperty(guid, "CODE_SIGN_STYLE",          "Automatic");
                proj.SetBuildProperty(guid, "CODE_SIGN_IDENTITY",       "Apple Development");
                // Generate dSYM files for all configs so Apple can symbolicate crash reports
                proj.SetBuildProperty(guid, "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
                proj.SetBuildProperty(guid, "COPY_PHASE_STRIP",         "NO");
            }

            // Stamp every named build configuration at project level
            string projectGuid = proj.ProjectGuid();
            var configs = new[] { "Debug", "Release", "ReleaseForProfiling", "ReleaseForRunning" };
            foreach (var config in configs)
            {
                string configGuid = proj.BuildConfigByName(projectGuid, config);
                if (string.IsNullOrEmpty(configGuid)) continue;
                proj.SetBuildPropertyForConfig(configGuid, "DEVELOPMENT_TEAM", TeamID);
                proj.SetBuildPropertyForConfig(configGuid, "CODE_SIGN_STYLE",  "Automatic");
            }

            File.WriteAllText(projPath, proj.WriteToString());

            // Add ITSAppUsesNonExemptEncryption = NO to Info.plist so Apple never
            // prompts for export compliance — the game uses no custom encryption.
            string plistPath = buildPath + "/Info.plist";
            if (File.Exists(plistPath))
            {
                var plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));
                plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
                File.WriteAllText(plistPath, plist.WriteToString());
            }

            UnityEngine.Debug.Log(
                $"[iOSPostBuildProcessor] Team {TeamID} stamped on " +
                $"{targetGuids.Count} targets × {configs.Length} configurations ✓");
#endif
        }
    }
}
