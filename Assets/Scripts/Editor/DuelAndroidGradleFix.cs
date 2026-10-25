// Project D  |  Codename: Zantetsuken
// DuelAndroidGradleFix.cs  (Editor-only)
//
//   Tools > Project D > Fix Android Kotlin Duplicate-Class Build Error
//
// Fixes:  "Execution failed for task ':launcher:checkReleaseDuplicateClasses' ... Duplicate class
//          kotlin.collections.jdk8.CollectionsJDK8Kt found in modules kotlin-stdlib-1.8.22 and kotlin-stdlib-jdk8-1.6.21"
// Cause : a dependency (Unity IAP 5 / Google Play Billing) pulls old split Kotlin jars (jdk7/jdk8 1.6.21) next to the
//         merged kotlin-stdlib 1.8.22, so the same classes exist twice.
// Fix   : force every Kotlin stdlib artifact to one version in Unity's "Custom Base Gradle Template"
//         (Assets/Plugins/Android/baseProjectTemplate.gradle). This tool creates that file from Unity's own default
//         template (exactly what ticking the checkbox in Player Settings does) and appends the block once.
// Build Duel Demo APK / Build and Run call this automatically.
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectD.EditorTools
{
    public static class DuelAndroidGradleFix
    {
        private const string TemplateFolder = "Assets/Plugins/Android";
        private const string TemplatePath = "Assets/Plugins/Android/baseProjectTemplate.gradle";
        private const string Marker = "ProjectD: kotlin-stdlib duplicate fix";

        private const string Block =
            "\n// ProjectD: kotlin-stdlib duplicate fix (Unity IAP 5 / Play Billing pull mixed Kotlin stdlib versions)\n" +
            "allprojects {\n" +
            "    configurations.all {\n" +
            "        resolutionStrategy {\n" +
            "            force 'org.jetbrains.kotlin:kotlin-stdlib:1.8.22'\n" +
            "            force 'org.jetbrains.kotlin:kotlin-stdlib-jdk7:1.8.22'\n" +
            "            force 'org.jetbrains.kotlin:kotlin-stdlib-jdk8:1.8.22'\n" +
            "        }\n" +
            "    }\n" +
            "}\n";

        [MenuItem("Tools/Project D/Fix Android Kotlin Duplicate-Class Build Error", priority = 48)]
        public static void Menu()
        {
            bool ok = Apply(out string message);
            Debug.Log("[Project D] " + message);
            if (!Application.isBatchMode) EditorUtility.DisplayDialog("Project D", message, "OK");
            if (!ok) Debug.LogWarning("[Project D] Gradle fix not applied.");
        }

        /// <summary>Makes sure the base Gradle template exists and contains the Kotlin fix. Returns false if it could not.</summary>
        public static bool Apply(out string message)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Plugins")) AssetDatabase.CreateFolder("Assets", "Plugins");
            if (!AssetDatabase.IsValidFolder(TemplateFolder)) AssetDatabase.CreateFolder("Assets/Plugins", "Android");

            string full = Path.Combine(Directory.GetParent(Application.dataPath).FullName, TemplatePath);
            if (!File.Exists(full))
            {
                string defaults = Path.Combine(EditorApplication.applicationContentsPath,
                    "PlaybackEngines/AndroidPlayer/Tools/GradleTemplates/baseProjectTemplate.gradle");
                if (!File.Exists(defaults))
                {
                    message = "Could not find Unity's default base Gradle template at:\n" + defaults +
                              "\n\nDo it by hand once: Edit > Project Settings > Player > Android > Publishing Settings > Build > " +
                              "tick 'Custom Base Gradle Template', then run this again.";
                    return false;
                }
                File.Copy(defaults, full);
            }

            string text = File.ReadAllText(full);
            if (text.Contains(Marker))
            {
                message = "The Kotlin duplicate-class fix is already in " + TemplatePath + ".";
                return true;
            }

            File.WriteAllText(full, text.TrimEnd() + "\n" + Block);
            AssetDatabase.ImportAsset(TemplatePath, ImportAssetOptions.ForceUpdate);
            message = "Added the Kotlin duplicate-class fix to " + TemplatePath + ".\nRun the Android build again.";
            return true;
        }
    }
}
#endif
