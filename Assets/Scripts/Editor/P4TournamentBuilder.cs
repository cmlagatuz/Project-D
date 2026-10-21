// Project D  |  Codename: Zantetsuken
// P4TournamentBuilder.cs  (Editor-only)  — Tools > Project D > Build Tournament Demo
//
// Builds a runnable tournament bracket screen (flagship P4 esports UI): round columns
// of match cells; click a match to toggle the winner and auto-advance the bracket.
// The other P4 views (Lobby, Server picker, Support) bind the same way via P4DemoDriver.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.P4UI;

namespace ProjectD.EditorTools
{
    public static class P4TournamentBuilder
    {
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static Font _font;

        [MenuItem("Tools/Project D/Build Tournament Demo", priority = 101)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }
            var canvasGo = new GameObject("Canvas"); var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = canvasGo.AddComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920,1080); sc.matchWidthOrHeight = 0.5f; canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            Label(root, "Title", "TOURNAMENT — Crystal Cup", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(1200,44), 30, Ink, TextAnchor.MiddleCenter, true);
            Label(root, "Hint", "Click a match to set the winner; the bracket advances automatically.", new Vector2(0.5f,1), new Vector2(0,-84), new Vector2(1400,26), 18, Sub, TextAnchor.MiddleCenter, false);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);
            var champ = Label(root, "Champion", "", new Vector2(0.5f,0), new Vector2(0,60), new Vector2(900,36), 26, Gold, TextAnchor.MiddleCenter, true);

            var rounds = MakePanel(root, "Rounds", new Vector2(0.5f,0.5f), new Vector2(0,-20), new Vector2(1720,720)); rounds.GetComponent<Image>().color = new Color(0,0,0,0);
            var hlg = rounds.gameObject.AddComponent<HorizontalLayoutGroup>(); hlg.spacing = 40; hlg.childAlignment = TextAnchor.MiddleCenter; hlg.childForceExpandWidth = false; hlg.childControlWidth = false;

            var columnPrefab = BuildColumnPrefab();
            var cellPrefab = BuildCellPrefab();

            var view = canvasGo.AddComponent<TournamentBracketView>();
            var so = new SerializedObject(view);
            so.FindProperty("roundsParent").objectReferenceValue = rounds;
            so.FindProperty("columnPrefab").objectReferenceValue = columnPrefab;
            so.FindProperty("matchCellPrefab").objectReferenceValue = cellPrefab;
            so.FindProperty("championText").objectReferenceValue = champ;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<P4DemoDriver>();
            var dso = new SerializedObject(driver); dso.FindProperty("bracket").objectReferenceValue = view; dso.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/TournamentDemo.unity");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D", "Tournament demo built.\nPress Play, click matches to set winners; the bracket advances to the Final. (Other P4 views bind the same way via P4DemoDriver.)", "OK");
        }

        private static RectTransform BuildColumnPrefab()
        {
            var go = new GameObject("RoundColumn", typeof(RectTransform), typeof(LayoutElement));
            go.GetComponent<LayoutElement>().preferredWidth = 260; go.GetComponent<LayoutElement>().preferredHeight = 700;
            var vlg = go.AddComponent<VerticalLayoutGroup>(); vlg.spacing = 16; vlg.childAlignment = TextAnchor.MiddleCenter; vlg.childForceExpandHeight = false; vlg.childControlHeight = false;
            Label((RectTransform)go.transform, "Header", "Round", new Vector2(0.5f,1), Vector2.zero, new Vector2(240,30), 18, Gold, TextAnchor.MiddleCenter, true);
            go.SetActive(false);
            return (RectTransform)go.transform;
        }

        private static Button BuildCellPrefab()
        {
            var go = new GameObject("MatchCell", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.GetComponent<Image>().color = Panel; go.GetComponent<LayoutElement>().preferredWidth = 240; go.GetComponent<LayoutElement>().preferredHeight = 84;
            Label((RectTransform)go.transform, "TeamA", "Team A", new Vector2(0.5f,1), new Vector2(0,-8), new Vector2(220,34), 18, Ink, TextAnchor.MiddleLeft, true);
            var div = new GameObject("Div", typeof(RectTransform), typeof(Image)); div.transform.SetParent(go.transform,false); var dr=(RectTransform)div.transform; dr.anchorMin=new Vector2(0,0.5f); dr.anchorMax=new Vector2(1,0.5f); dr.sizeDelta=new Vector2(-16,2); dr.anchoredPosition=Vector2.zero; div.GetComponent<Image>().color=new Color(1,1,1,0.12f);
            Label((RectTransform)go.transform, "TeamB", "Team B", new Vector2(0.5f,0), new Vector2(0,8), new Vector2(220,34), 18, Ink, TextAnchor.MiddleLeft, true);
            go.SetActive(false);
            return go.GetComponent<Button>();
        }

        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Panel; return rt; }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
    }
}
#endif
