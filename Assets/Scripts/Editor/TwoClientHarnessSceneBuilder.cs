// Project D  |  Codename: Zantetsuken
// TwoClientHarnessSceneBuilder.cs  (Editor-only)
//
// Builds a scene that runs one server + two clients in-process (UDP loopback) so
// you can watch multi-client replication, interest management, and reconnect
// without a build. A small on-screen button forces client B to drop, exercising
// the reconnect/resync path live.
//
//   Tools > Project D > Build Two-Client Test Harness Scene

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Data;
using ProjectD.Networking.TestHarness;

namespace ProjectD.EditorTools
{
    public static class TwoClientHarnessSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/TwoClientHarness.unity";

        [MenuItem("Tools/Project D/Build Two-Client Test Harness Scene", priority = 81)]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera + light so the scene isn't black.
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.11f); camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0, 20, -12); camGo.transform.rotation = Quaternion.Euler(60, 0, 0);

            // Harness driver
            var harnessGo = new GameObject("TwoClientHarness");
            var harness = harnessGo.AddComponent<TwoClientHarness>();
            var champ = FindFirstChampion();
            if (champ != null)
            {
                var so = new SerializedObject(harness);
                so.FindProperty("champion").objectReferenceValue = champ;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Minimal UI: status text + "Drop Client B" button.
            BuildUI(harness);

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                champ == null
                    ? "Harness scene built, but no champion asset was found.\nRun Import Champion Data first."
                    : "Two-client harness built:\n" + ScenePath +
                      "\n\nPress Play. Console shows each client seeing the OTHER move.\n" +
                      "Click 'Drop Client B' to test reconnect + resync.",
                "OK");
            Debug.Log("[Project D] Two-client harness scene created at " + ScenePath);
        }

        private static void BuildUI(TwoClientHarness harness)
        {
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            var canvasGo = new GameObject("HarnessUI");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var title = new GameObject("Title", typeof(RectTransform), typeof(Text));
            title.transform.SetParent(canvasGo.transform, false);
            var trt = (RectTransform)title.transform; trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 1); trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = new Vector2(0, -20); trt.sizeDelta = new Vector2(900, 40);
            var tt = title.GetComponent<Text>(); tt.font = font; tt.fontSize = 22; tt.alignment = TextAnchor.MiddleCenter;
            tt.color = new Color(0.9f, 0.94f, 1f); tt.text = "PROJECT D · TWO-CLIENT HARNESS (server + 2 clients, UDP loopback)";

            var btnGo = new GameObject("DropClientB", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(canvasGo.transform, false);
            var brt = (RectTransform)btnGo.transform; brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0); brt.pivot = new Vector2(0.5f, 0);
            brt.anchoredPosition = new Vector2(0, 40); brt.sizeDelta = new Vector2(280, 64);
            btnGo.GetComponent<Image>().color = new Color(0.85f, 0.4f, 0.4f);
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(btnGo.transform, false);
            var lrt = (RectTransform)label.transform; lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            var lt = label.GetComponent<Text>(); lt.font = font; lt.fontSize = 22; lt.alignment = TextAnchor.MiddleCenter; lt.color = Color.white; lt.text = "Drop Client B (test reconnect)";

            btnGo.GetComponent<Button>().onClick.AddListener(harness.ForceDropClientB);
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
