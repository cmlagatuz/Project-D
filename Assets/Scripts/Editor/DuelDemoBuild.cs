// Project D  |  Codename: Zantetsuken
// DuelDemoBuild.cs  (Editor-only)
//
// Builds the duel demo as an Android APK you can install on a phone:
//   Tools > Project D > Build Duel Demo APK (Android)
//   Tools > Project D > Build and Run Duel Demo on Phone     (phone connected over USB with debugging on)
// Needs the "Android Build Support" module (with OpenJDK + Android SDK & NDK) installed for this Unity version.
// iOS needs a Mac with Xcode, so it is not covered here.
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ProjectD.EditorTools
{
    public static class DuelDemoBuild
    {
        private const string OutputPath = "Builds/ProjectD_DuelDemo.apk";

        [MenuItem("Tools/Project D/Build Duel Demo APK (Android)", priority = 42)]
        public static void BuildApk() { Build(false); }

        [MenuItem("Tools/Project D/Build and Run Duel Demo on Phone", priority = 43)]
        public static void BuildAndRun() { Build(true); }

        /// <summary>Command-line entry point: Unity -batchmode -quit -executeMethod ProjectD.EditorTools.DuelDemoBuild.BuildApkBatch</summary>
        public static void BuildApkBatch()
        {
            var result = Build(false);
            EditorApplication.Exit(result == BuildResult.Succeeded ? 0 : 1);
        }

        private static BuildResult Build(bool run)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                const string msg = "Android Build Support is not installed for this Unity version.\n\n" +
                    "Unity Hub > Installs > (your 6000.5 editor) > Add modules > Android Build Support, " +
                    "and tick OpenJDK and Android SDK & NDK Tools. Then run this again.";
                Debug.LogError("[Project D] " + msg);
                if (!Application.isBatchMode) EditorUtility.DisplayDialog("Project D", msg, "OK");
                return BuildResult.Failed;
            }

            DuelSceneBuilder.BuildScene();   // always rebuild so the scene matches the code

            if (!DuelAndroidGradleFix.Apply(out string gradleMessage))   // Kotlin duplicate-class fix (Unity IAP 5 / Play Billing)
            {
                Debug.LogError("[Project D] " + gradleMessage);
                if (!Application.isBatchMode) EditorUtility.DisplayDialog("Project D", gradleMessage, "OK");
                return BuildResult.Failed;
            }

            var target = NamedBuildTarget.Android;
            PlayerSettings.SetScriptingBackend(target, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            EditorUserBuildSettings.buildAppBundle = false;   // .apk, not .aab

            string id = PlayerSettings.GetApplicationIdentifier(target);
            if (string.IsNullOrEmpty(id) || id.StartsWith("com.DefaultCompany"))
                PlayerSettings.SetApplicationIdentifier(target, "com.projectd.dueldemo");

            Directory.CreateDirectory("Builds");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { DuelSceneBuilder.ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = run ? BuildOptions.AutoRunPlayer : BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[Project D] APK built: " + OutputPath + " (" + (summary.totalSize / (1024 * 1024)) + " MB). " +
                          "Copy it to your phone and open it, or run `adb install -r " + OutputPath + "`.");
                if (!Application.isBatchMode && !run) EditorUtility.RevealInFinder(OutputPath);
            }
            else
            {
                Debug.LogError("[Project D] APK build " + summary.result + " with " + summary.totalErrors + " error(s). See the Console.");
                string reportPath = DuelBuildErrorReport.Save();
                if (reportPath != null) Debug.LogError("[Project D] The real Gradle error was saved to " + reportPath + " - open it and send it over.");
            }
            return summary.result;
        }
    }
}
#endif
