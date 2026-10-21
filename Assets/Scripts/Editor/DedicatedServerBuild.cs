// Project D  |  Codename: Zantetsuken
// DedicatedServerBuild.cs  (Editor-only)
//
// Builds a headless dedicated-server executable (Linux/Windows) with a server-only
// scene, and provides a CLI entry point for CI. The server scene contains just the
// DedicatedServerBootstrap, which starts AuthoritativeMatchServer in batchmode.
//
//   Tools > Project D > Build Headless Dedicated Server
//   CLI:  Unity -batchmode -quit -projectPath . -executeMethod ProjectD.EditorTools.DedicatedServerBuild.BuildLinux

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectD.Data;
using ProjectD.Networking.Server;

namespace ProjectD.EditorTools
{
    public static class DedicatedServerBuild
    {
        private const string ServerScenePath = "Assets/Scenes/DedicatedServer.unity";
        private const string OutputRoot = "Builds/Server";

        [MenuItem("Tools/Project D/Build Headless Dedicated Server", priority = 80)]
        public static void BuildForCurrentPlatform()
        {
            EnsureServerScene();
#if UNITY_EDITOR_WIN
            BuildWindows();
#else
            BuildLinux();
#endif
        }

        public static void BuildLinux()  => Build(BuildTarget.StandaloneLinux64, "ProjectD_Server.x86_64");
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "ProjectD_Server.exe");

        private static void Build(BuildTarget target, string exeName)
        {
            EnsureServerScene();
            string dir = Path.Combine(OutputRoot, target.ToString());
            Directory.CreateDirectory(dir);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ServerScenePath },
                locationPathName = Path.Combine(dir, exeName),
                target = target,
                // Headless server subtarget: no graphics, dedicated-server defines.
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.Development
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Project D] Server build ({target}) result: {report.summary.result} -> {options.locationPathName}\n" +
                      "Run: ProjectD_Server -batchmode -nographics -server -port 7777 -tick 30");
        }

        // Creates a minimal server-only scene if it does not exist.
        private static void EnsureServerScene()
        {
            if (File.Exists(ServerScenePath)) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("DedicatedServerBootstrap");
            var boot = go.AddComponent<DedicatedServerBootstrap>();

            var champ = FindFirstChampion();
            if (champ != null)
            {
                var so = new SerializedObject(boot);
                so.FindProperty("defaultChampion").objectReferenceValue = champ;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ServerScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[Project D] Created server scene at {ServerScenePath}");
        }

        private static ChampionDefinition FindFirstChampion()
        {
            var guids = AssetDatabase.FindAssets("t:ChampionDefinition", new[] { "Assets/GameData" });
            return guids.Length == 0 ? null
                : AssetDatabase.LoadAssetAtPath<ChampionDefinition>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
#endif
