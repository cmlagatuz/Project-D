// Project D  |  Codename: Zantetsuken
// LoadingScreenBuilder.cs  (Editor-only)  — Tools > Project D > Build Loading Screen
//
// Builds the loading scene: champion focus, progress bar, a rotating tip card (with
// the pinned matchup callout), a matchup banner, and a VoiceLinePlayer with a
// subtitle. Runs without art (role-tinted splash) or audio (subtitle still shows).
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Loading;

namespace ProjectD.EditorTools
{
    public static class LoadingScreenBuilder
    {
        private const string ScenePath = "Assets/Scenes/LoadingScreen.unity";
        private static readonly Color Bg = new Color(0.05f, 0.06f, 0.09f);
        private static readonly Color Panel = new Color(0.10f, 0.12f, 0.18f, 0.96f);
        private static readonly Color Ink = new Color(0.93f, 0.95f, 0.99f);
        private static readonly Color Sub = new Color(0.68f, 0.73f, 0.83f);
        private static readonly Color Gold = new Color(0.95f, 0.80f, 0.32f);

        [MenuItem("Tools/Project D/Build Loading Screen", priority = 76)]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg;
            camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }

            var canvasGo = new GameObject("LoadingCanvas");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            var splash = ChildImage(root, "ChampionSplash", new Color(0.30f, 0.45f, 0.80f), stretch: true, raycast: false);
            ChildImage(root, "Scrim", new Color(0f, 0f, 0f, 0.55f), stretch: true, raycast: false);

            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,28), 18, new Color(1,1,1,0.30f), TextAnchor.MiddleRight, false);
            Label(root, "LoadingTitle", "ENTERING THE ARENA", new Vector2(0.5f,1), new Vector2(0,-46), new Vector2(900,50), 34, Ink, TextAnchor.MiddleCenter, true);
            var matchupBanner = Label(root, "MatchupBanner", "Your lane (Jungle) vs Cecil Harvey", new Vector2(0.5f,1), new Vector2(0,-100), new Vector2(1000,32), 22, Gold, TextAnchor.MiddleCenter, true);

            // Champion focus (bottom-left)
            var focus = MakePanel(root, "ChampionFocus", new Vector2(0,0), new Vector2(0,0), new Vector2(560, 200));
            focus.pivot = new Vector2(0,0); focus.anchoredPosition = new Vector2(40, 120);
            var champName = Label(focus, "ChampName", "Kain Highwind", new Vector2(0,1), new Vector2(24,-20), new Vector2(500,44), 34, Ink, TextAnchor.UpperLeft, true);
            var champSub = Label(focus, "ChampSub", "FF IV - Assassin", new Vector2(0,1), new Vector2(24,-70), new Vector2(500,28), 20, Gold, TextAnchor.UpperLeft, false);
            Label(focus, "YouLabel", "YOUR CHAMPION", new Vector2(0,0), new Vector2(24,20), new Vector2(400,24), 16, Sub, TextAnchor.LowerLeft, true);

            // Progress bar
            var barBack = MakePanel(root, "ProgressBack", new Vector2(0.5f,0), new Vector2(0, 70), new Vector2(1400, 26));
            barBack.GetComponent<Image>().color = new Color(1,1,1,0.10f);
            var fillGo = ChildImage(barBack.transform, "ProgressFill", Gold, raycast: false);
            var fr = (RectTransform)fillGo.transform; fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero;
            fillGo.type = Image.Type.Filled; fillGo.fillMethod = Image.FillMethod.Horizontal; fillGo.fillOrigin = (int)Image.OriginHorizontal.Left; fillGo.fillAmount = 0.2f;
            var pctLabel = Label(root, "ProgressPct", "20%", new Vector2(0.5f,0), new Vector2(0, 108), new Vector2(200,30), 22, Ink, TextAnchor.MiddleCenter, true);
            var statusLabel = Label(root, "Status", "Loading...", new Vector2(0.5f,0), new Vector2(0, 40), new Vector2(600,26), 18, Sub, TextAnchor.MiddleCenter, false);

            // Tip card (bottom-right)
            var tip = MakePanel(root, "TipCard", new Vector2(1,0), new Vector2(0,0), new Vector2(760, 220));
            tip.pivot = new Vector2(1,0); tip.anchoredPosition = new Vector2(-40, 120); tip.GetComponent<Image>().color = Panel;
            var tipGroup = tip.gameObject.AddComponent<CanvasGroup>();
            var tipHeading = Label(tip, "TipHeading", "YOUR LANE (Jungle) - Cecil Harvey", new Vector2(0,1), new Vector2(28,-22), new Vector2(700,30), 22, Gold, TextAnchor.UpperLeft, true);
            var tipBody = Wrapped(tip, "TipBody", 720, -70, Ink, 22);

            // Voice subtitle (center-bottom)
            var subGo = MakePanel(root, "VoiceSubtitle", new Vector2(0.5f,0), new Vector2(0, 170), new Vector2(1100, 44));
            subGo.GetComponent<Image>().color = new Color(0f,0f,0f,0.5f);
            var subGroup = subGo.gameObject.AddComponent<CanvasGroup>(); subGroup.alpha = 0f;
            var subText = Label(subGo, "SubText", "", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1080,40), 24, Ink, TextAnchor.MiddleCenter, false);

            // Voice player
            var voiceGo = new GameObject("VoiceLinePlayer");
            voiceGo.transform.SetParent(canvasGo.transform, false);
            var audio = voiceGo.AddComponent<AudioSource>(); audio.playOnAwake = false;
            var voice = voiceGo.AddComponent<VoiceLinePlayer>();
            var vso = new SerializedObject(voice);
            vso.FindProperty("audioSource").objectReferenceValue = audio;
            vso.FindProperty("subtitleGroup").objectReferenceValue = subGroup;
            vso.FindProperty("subtitleText").objectReferenceValue = subText;
            var interSets = LoadAll<ChampionInteractionSet>();
            var isp = vso.FindProperty("interactionSets"); isp.arraySize = interSets.Count;
            for (int i = 0; i < interSets.Count; i++) isp.GetArrayElementAtIndex(i).objectReferenceValue = interSets[i];
            vso.ApplyModifiedPropertiesWithoutUndo();

            // View + wiring
            var view = canvasGo.AddComponent<LoadingScreenView>();
            var so = new SerializedObject(view);
            so.FindProperty("championSplash").objectReferenceValue = splash;
            so.FindProperty("championName").objectReferenceValue = champName;
            so.FindProperty("championSub").objectReferenceValue = champSub;
            so.FindProperty("matchupBanner").objectReferenceValue = matchupBanner;
            so.FindProperty("progressFill").objectReferenceValue = fillGo;
            so.FindProperty("progressLabel").objectReferenceValue = pctLabel;
            so.FindProperty("statusLabel").objectReferenceValue = statusLabel;
            so.FindProperty("tipHeading").objectReferenceValue = tipHeading;
            so.FindProperty("tipBody").objectReferenceValue = tipBody;
            so.FindProperty("tipGroup").objectReferenceValue = tipGroup;
            so.FindProperty("voicePlayer").objectReferenceValue = voice;

            var champs = LoadAll<ChampionTipSet>();
            var champProp = so.FindProperty("championTips"); champProp.arraySize = champs.Count;
            for (int i = 0; i < champs.Count; i++) champProp.GetArrayElementAtIndex(i).objectReferenceValue = champs[i];
            var general = LoadFirst<GeneralTipSet>(); if (general) so.FindProperty("generalTips").objectReferenceValue = general;

            // Preview: local Kain (Jungle) vs an enemy team incl. Cecil in Baron.
            so.FindProperty("localChampionId").stringValue = "kain";
            so.FindProperty("localLane").stringValue = "Jungle";
            var enemies = so.FindProperty("enemyPicks"); enemies.arraySize = 3;
            SetEnemy(enemies, 0, "cecil", "Jungle");   // same lane -> the matchup + voice line target
            SetEnemy(enemies, 1, "exdeath", "Support");
            SetEnemy(enemies, 2, "emperor", "Mid");
            so.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                champs.Count == 0
                    ? "Loading screen built, but no tip assets found.\nRun Import Loading Tips + Import Champion Interactions first, then rebuild."
                    : $"Loading screen built:\n{ScenePath}\n\nPress Play. The matchup callout (vs Cecil) is pinned first, and Kain's first-encounter voice line fires (subtitle shows; add audio in GameData/Loading/Voice).",
                "OK");
            Debug.Log($"[Project D] Loading screen created ({champs.Count} tip sets, {interSets.Count} interaction sets).");
        }

        private static void SetEnemy(SerializedProperty arr, int i, string id, string lane)
        {
            var el = arr.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("championId").stringValue = id;
            el.FindPropertyRelative("lane").stringValue = lane;
        }

        // ---- primitives ---- //
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.GetComponent<Image>().color = Panel; return rt;
        }
        private static Image ChildImage(Transform parent, string name, Color c, bool stretch = false, bool raycast = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>(); img.color = c; img.raycastTarget = raycast;
            if (stretch) { var rt = (RectTransform)go.transform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }
            return img;
        }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(anchor.x, anchor.y); rt.anchoredPosition = pos; rt.sizeDelta = size;
            var t = go.GetComponent<Text>(); t.text = text; t.color = c; t.fontSize = fs; t.alignment = a; t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            return t;
        }
        private static Text Wrapped(RectTransform parent, string name, float w, float topY, Color c, int fs)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1);
            rt.anchoredPosition = new Vector2(28, topY); rt.sizeDelta = new Vector2(w - 56, 120);
            var t = go.GetComponent<Text>(); t.text = ""; t.color = c; t.fontSize = fs; t.alignment = TextAnchor.UpperLeft;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            return t;
        }
        private static List<T> LoadAll<T>() where T : Object
        {
            var list = new List<T>();
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { "Assets/GameData/Loading" }))
                list.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
            return list;
        }
        private static T LoadFirst<T>() where T : Object { var a = LoadAll<T>(); return a.Count > 0 ? a[0] : null; }
    }
}
#endif
