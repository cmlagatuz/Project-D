// Project D  |  Codename: Zantetsuken
// MapEnvDemoBuilder.cs  (Editor-only)  — Tools > Project D > Build Map Env Demo
//
// Builds a small 3D scene: a river ground with two pits (top + bottom), a movable
// player marker, the ObjectiveDirector + ProximityBgmController wired up, and a HUD
// showing the pit layout + current objectives. Move the marker into a pit to hear
// that objective's theme (assign a PitBgmSet with your imported clips first).

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Map;
using ProjectD.Audio;

namespace ProjectD.EditorTools
{
    public static class MapEnvDemoBuilder
    {
        private const string ScenePath = "Assets/Scenes/MapEnvDemo.unity";

        [MenuItem("Tools/Project D/Build Map Env Demo", priority = 85)]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f,0.06f,0.09f);
            cam.orthographic = true; cam.orthographicSize = 45; camGo.transform.position = new Vector3(0, 60, 0); camGo.transform.rotation = Quaternion.Euler(90,0,0);
            camGo.AddComponent<AudioListener>();
            var lightGo = new GameObject("Light"); var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.transform.rotation = Quaternion.Euler(50,-30,0);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name = "Ground"; ground.transform.localScale = new Vector3(10,1,10);
            ground.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.18f,0.22f,0.26f));
            var river = GameObject.CreatePrimitive(PrimitiveType.Cube); river.name="River"; river.transform.position=new Vector3(0,0.05f,0); river.transform.localScale=new Vector3(14,0.1f,90); river.GetComponent<Renderer>().sharedMaterial=Mat(new Color(0.25f,0.45f,0.6f)); Object.DestroyImmediate(river.GetComponent<Collider>());

            var topPit = MakePit("TopPit", new Vector3(0,0.1f,30), new Color(0.6f,0.35f,0.6f));
            var botPit = MakePit("BottomPit", new Vector3(0,0.1f,-30), new Color(0.7f,0.55f,0.25f));

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule); player.name="LocalPlayer"; player.transform.position=new Vector3(0,1,0); player.transform.localScale=new Vector3(2,2,2);
            player.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.85f,0.8f,0.35f));

            var audioGo = new GameObject("Audio");
            var defaultMusic = audioGo.AddComponent<AudioSource>(); defaultMusic.playOnAwake=false; defaultMusic.loop=true; defaultMusic.volume=0.5f;
            var pitA = audioGo.AddComponent<AudioSource>(); pitA.playOnAwake=false;
            var pitB = audioGo.AddComponent<AudioSource>(); pitB.playOnAwake=false;

            var bgm = audioGo.AddComponent<ProximityBgmController>();
            var bso = new SerializedObject(bgm);
            bso.FindProperty("localPlayer").objectReferenceValue = player.transform;
            bso.FindProperty("defaultMusic").objectReferenceValue = defaultMusic;
            bso.FindProperty("pitSourceA").objectReferenceValue = pitA;
            bso.FindProperty("pitSourceB").objectReferenceValue = pitB;
            bso.ApplyModifiedPropertiesWithoutUndo();

            var dirGo = new GameObject("ObjectiveDirector");
            var director = dirGo.AddComponent<ObjectiveDirector>();
            var dso = new SerializedObject(director);
            dso.FindProperty("bgm").objectReferenceValue = bgm;
            dso.FindProperty("topPit").objectReferenceValue = topPit.transform;
            dso.FindProperty("bottomPit").objectReferenceValue = botPit.transform;
            dso.ApplyModifiedPropertiesWithoutUndo();

            var canvasGo = new GameObject("HUD");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }

            var status = Label((RectTransform)canvasGo.transform, "Status", "", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(1700,64), 22, Color.white, TextAnchor.UpperCenter);
            Label((RectTransform)canvasGo.transform, "Help", "Move: WASD / Arrows.  Walk into a pit to hear its theme (assign a PitBgmSet with your audio).", new Vector2(0.5f,1), new Vector2(0,-120), new Vector2(1700,30), 18, new Color(0.7f,0.75f,0.85f), TextAnchor.MiddleCenter);
            var killSummon = ActionButton((RectTransform)canvasGo.transform, "KILL SUMMON", new Vector2(0.5f,0), new Vector2(-340,60));
            var killDivine = ActionButton((RectTransform)canvasGo.transform, "KILL DIVINE", new Vector2(0.5f,0), new Vector2(-60,60));
            var flip = ActionButton((RectTransform)canvasGo.transform, "FLIP DUO SIDE", new Vector2(0.5f,0), new Vector2(240,60));

            var driverGo = new GameObject("MapEnvDemoDriver");
            var driver = driverGo.AddComponent<MapEnvDemoDriver>();
            var drso = new SerializedObject(driver);
            drso.FindProperty("director").objectReferenceValue = director;
            drso.FindProperty("bgm").objectReferenceValue = bgm;
            drso.FindProperty("localPlayer").objectReferenceValue = player.transform;
            drso.FindProperty("topPit").objectReferenceValue = topPit.transform;
            drso.FindProperty("bottomPit").objectReferenceValue = botPit.transform;
            drso.FindProperty("statusLabel").objectReferenceValue = status;
            drso.FindProperty("summonKillButton").objectReferenceValue = killSummon;
            drso.FindProperty("divineKillButton").objectReferenceValue = killDivine;
            drso.FindProperty("flipDuoButton").objectReferenceValue = flip;
            drso.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                $"Map environment demo built:\n{ScenePath}\n\nPress Play, move with WASD/arrows into a pit to hear its theme " +
                "(assign a PitBgmSet with your imported clips on the Audio object first). FLIP DUO SIDE moves the summon pit; " +
                "the divine pit is always opposite.", "OK");
            Debug.Log("[Project D] Map env demo scene created.");
        }

        private static GameObject MakePit(string name, Vector3 pos, Color c)
        {
            var pit = GameObject.CreatePrimitive(PrimitiveType.Cylinder); pit.name = name; pit.transform.position = pos; pit.transform.localScale = new Vector3(14,0.2f,14);
            pit.GetComponent<Renderer>().sharedMaterial = Mat(c); Object.DestroyImmediate(pit.GetComponent<Collider>());
            return pit;
        }
        private static Material Mat(Color c) { var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"); return new Material(sh){ color = c }; }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,1); rt.anchoredPosition=pos; rt.sizeDelta=size;
            var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf"); return t;
        }
        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos)
        {
            var go = new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(240,52);
            go.GetComponent<Image>().color = new Color(0.30f,0.40f,0.52f);
            Label(rt, "L", text, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(240,52), 18, Color.white, TextAnchor.MiddleCenter);
            return go.GetComponent<Button>();
        }
    }
}
#endif
