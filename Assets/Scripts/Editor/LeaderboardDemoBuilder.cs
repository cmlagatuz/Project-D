// Project D  |  Codename: Zantetsuken
// LeaderboardDemoBuilder.cs  (Editor-only)  — Tools > Project D > Build Leaderboard Demo
//
// Builds a runnable apex-leaderboard scene: Server/Friends scope, tier filter
// (Both/Adamantite/Crystal), champion + lane dropdowns, a paged row list, and a
// pinned "your rank" card. Uses a mock apex pool so it runs without a backend.

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.LeaderboardUI;

namespace ProjectD.EditorTools
{
    public static class LeaderboardDemoBuilder
    {
        private const string ScenePath = "Assets/Scenes/LeaderboardDemo.unity";
        private static readonly Color Bg = new Color(0.05f, 0.06f, 0.10f);
        private static readonly Color Panel = new Color(0.11f, 0.13f, 0.18f, 0.98f);
        private static readonly Color Ink = new Color(0.93f, 0.95f, 0.99f);
        private static readonly Color Sub = new Color(0.68f, 0.73f, 0.83f);
        private static readonly Color Idle = new Color(0.16f, 0.18f, 0.24f);
        private static readonly Color Accent = new Color(0.36f, 0.62f, 0.92f);
        private static Font _font;

        [MenuItem("Tools/Project D/Build Leaderboard Demo", priority = 82)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }

            var canvasGo = new GameObject("LeaderboardCanvas");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            Label(root, "Title", "APEX LEADERBOARD", new Vector2(0.5f,1), new Vector2(0,-34), new Vector2(900,50), 34, Ink, TextAnchor.MiddleCenter, true);
            Label(root, "Sub", "Adamantite & Crystal", new Vector2(0.5f,1), new Vector2(0,-76), new Vector2(900,28), 20, Sub, TextAnchor.MiddleCenter, false);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,28), 18, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Filter bar
            var scopeServer = ChipButton(root, "SERVER", new Vector2(0,1), new Vector2(40,-120));
            var scopeFriends = ChipButton(root, "FRIENDS", new Vector2(0,1), new Vector2(200,-120));
            var tierBoth = ChipButton(root, "BOTH", new Vector2(0,1), new Vector2(400,-120));
            var tierAdam = ChipButton(root, "ADAMANTITE", new Vector2(0,1), new Vector2(520,-120));
            var tierCrys = ChipButton(root, "CRYSTAL", new Vector2(0,1), new Vector2(700,-120));
            var champDD = DropdownRow(root, "ChampionDD", new Vector2(0,1), new Vector2(860,-120), 260);
            var laneDD = DropdownRow(root, "LaneDD", new Vector2(0,1), new Vector2(1140,-120), 220);
            var countText = Label(root, "Count", "", new Vector2(1,1), new Vector2(-40,-124), new Vector2(280,30), 20, Sub, TextAnchor.MiddleRight, false);

            // Column headers
            var header = MakePanel(root, "Header", new Vector2(0.5f,1), new Vector2(0,-176), new Vector2(1760,36));
            header.GetComponent<Image>().color = new Color(0.08f,0.09f,0.13f,0.95f);
            Cols(header, isHeader:true);

            // Row list (scroll)
            var scroll = MakePanel(root, "Scroll", new Vector2(0.5f,0.5f), new Vector2(0, -30), new Vector2(1760, 640));
            var sr = scroll.gameObject.AddComponent<ScrollRect>();
            var viewport = MakePanel(scroll, "Viewport", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1740, 620));
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var contentGo = new GameObject("Content", typeof(RectTransform)); contentGo.transform.SetParent(viewport, false);
            var content = (RectTransform)contentGo.transform; content.anchorMin=new Vector2(0,1); content.anchorMax=new Vector2(1,1); content.pivot=new Vector2(0.5f,1); content.sizeDelta=new Vector2(0,620);
            var vlg = contentGo.AddComponent<VerticalLayoutGroup>(); vlg.spacing=4; vlg.padding=new RectOffset(8,8,8,8); vlg.childForceExpandHeight=false; vlg.childControlHeight=false;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.viewport = viewport; sr.content = content; sr.horizontal = false;

            var rowPrefab = BuildRowPrefab();

            // Paging
            var prev = ActionButton(root, "< PREV", new Vector2(0.5f,0), new Vector2(-260, 120), new Color(0.30f,0.38f,0.48f));
            var pageLabel = Label(root, "Page", "Page 1 / 1", new Vector2(0.5f,0), new Vector2(0, 132), new Vector2(300,30), 20, Ink, TextAnchor.MiddleCenter, true);
            var next = ActionButton(root, "NEXT >", new Vector2(0.5f,0), new Vector2(260, 120), new Color(0.30f,0.38f,0.48f));

            // Your-rank card
            var card = MakePanel(root, "ViewerCard", new Vector2(0.5f,0), new Vector2(0, 50), new Vector2(1760, 56));
            card.GetComponent<Image>().color = new Color(0.16f,0.22f,0.34f,0.98f);
            var viewerRow = BuildRowInstance(card, "ViewerRow");
            var missing = Label(root, "Missing", "You're not currently in the apex tiers.", new Vector2(0.5f,0), new Vector2(0, 60), new Vector2(900,30), 20, Sub, TextAnchor.MiddleCenter, false);
            missing.gameObject.SetActive(false);

            // Screen component
            var screen = canvasGo.AddComponent<LeaderboardScreen>();
            var so = new SerializedObject(screen);
            so.FindProperty("serverScopeButton").objectReferenceValue = scopeServer;
            so.FindProperty("friendsScopeButton").objectReferenceValue = scopeFriends;
            var tb = so.FindProperty("tierButtons"); tb.arraySize = 3;
            tb.GetArrayElementAtIndex(0).objectReferenceValue = tierBoth;
            tb.GetArrayElementAtIndex(1).objectReferenceValue = tierAdam;
            tb.GetArrayElementAtIndex(2).objectReferenceValue = tierCrys;
            so.FindProperty("championDropdown").objectReferenceValue = champDD;
            so.FindProperty("laneDropdown").objectReferenceValue = laneDD;
            so.FindProperty("resultCountText").objectReferenceValue = countText;
            so.FindProperty("rowParent").objectReferenceValue = content;
            so.FindProperty("rowPrefab").objectReferenceValue = rowPrefab;
            so.FindProperty("prevPageButton").objectReferenceValue = prev;
            so.FindProperty("nextPageButton").objectReferenceValue = next;
            so.FindProperty("pageLabel").objectReferenceValue = pageLabel;
            so.FindProperty("viewerRow").objectReferenceValue = viewerRow;
            so.FindProperty("viewerCard").objectReferenceValue = card.gameObject;
            so.FindProperty("viewerMissingLabel").objectReferenceValue = missing;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Demo driver feeds the mock pool.
            var driver = canvasGo.AddComponent<LeaderboardDemoDriver>();
            var dso = new SerializedObject(driver);
            dso.FindProperty("screen").objectReferenceValue = screen;
            dso.FindProperty("viewerId").stringValue = "you";
            dso.FindProperty("entryCount").intValue = 60;
            dso.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                $"Apex leaderboard demo built:\n{ScenePath}\n\nPress Play. Toggle SERVER/FRIENDS, filter by tier/champion/lane, page through, " +
                "and see your own rank pinned at the bottom.", "OK");
            Debug.Log("[Project D] Leaderboard demo scene created.");
        }

        // ---- row prefab ----
        private static LeaderboardRow BuildRowPrefab()
        {
            var go = BuildRowInstance(null, "LeaderboardRow");
            go.gameObject.SetActive(false);
            return go;
        }

        private static LeaderboardRow BuildRowInstance(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LeaderboardRow), typeof(LayoutElement));
            if (parent) go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.14f,0.16f,0.23f,0.9f);
            go.GetComponent<LayoutElement>().preferredHeight = 48;
            var rt = (RectTransform)go.transform; rt.sizeDelta = new Vector2(1740, 48);
            if (parent) { rt.anchorMin = new Vector2(0,0.5f); rt.anchorMax = new Vector2(1,0.5f); rt.offsetMin = new Vector2(10,4); rt.offsetMax = new Vector2(-10,-4); }

            var rank = Col(go, "Rank", 70, 40);
            var name2 = Col(go, "Name", 330, 40);
            var swatch = ColImage(go, "Swatch", 700, 40, new Color(0.6f,0.5f,0.75f));
            var tier = Col(go, "Tier", 740, 40);
            var lp = Col(go, "LP", 980, 40);
            var champ = Col(go, "Champion", 1120, 40);
            var lane = Col(go, "Lane", 1330, 40);
            var region = Col(go, "Region", 1470, 40);
            var wr = Col(go, "WinRate", 1580, 40);
            var friend = ColImage(go, "Friend", 300, 20, new Color(0.6f,0.9f,0.6f));

            var comp = go.GetComponent<LeaderboardRow>();
            var so = new SerializedObject(comp);
            so.FindProperty("background").objectReferenceValue = go.GetComponent<Image>();
            so.FindProperty("rankText").objectReferenceValue = rank;
            so.FindProperty("nameText").objectReferenceValue = name2;
            so.FindProperty("tierSwatch").objectReferenceValue = swatch;
            so.FindProperty("tierText").objectReferenceValue = tier;
            so.FindProperty("lpText").objectReferenceValue = lp;
            so.FindProperty("championText").objectReferenceValue = champ;
            so.FindProperty("laneText").objectReferenceValue = lane;
            so.FindProperty("regionText").objectReferenceValue = region;
            so.FindProperty("winRateText").objectReferenceValue = wr;
            so.FindProperty("friendIcon").objectReferenceValue = friend;
            so.ApplyModifiedPropertiesWithoutUndo();
            return comp;
        }

        private static void Cols(RectTransform header, bool isHeader)
        {
            HeaderLabel(header, "#", 70);
            HeaderLabel(header, "Player", 330);
            HeaderLabel(header, "Rank", 740);
            HeaderLabel(header, "LP", 980);
            HeaderLabel(header, "Champion", 1120);
            HeaderLabel(header, "Lane", 1330);
            HeaderLabel(header, "Region", 1470);
            HeaderLabel(header, "Win Rate", 1580);
        }

        private static void HeaderLabel(RectTransform header, string text, float x)
            => Label(header, "H_" + text, text, new Vector2(0,0.5f), new Vector2(x - 860 + 20, 0), new Vector2(200,30), 16, Sub, TextAnchor.MiddleLeft, true);

        private static Text Col(GameObject row, string name, float x, float h)
        {
            var t = Label((RectTransform)row.transform, name, "", new Vector2(0,0.5f), new Vector2(x - 860 + 20, 0), new Vector2(210, h), 18, Ink, TextAnchor.MiddleLeft, false);
            return t;
        }
        private static Image ColImage(GameObject row, string name, float x, float size, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(row.transform, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=new Vector2(0,0.5f); rt.anchorMax=new Vector2(0,0.5f); rt.pivot=new Vector2(0,0.5f); rt.anchoredPosition=new Vector2(x-860+20,0); rt.sizeDelta=new Vector2(size,size>30?28:size);
            go.GetComponent<Image>().color = c; return go.GetComponent<Image>();
        }

        // ---- widgets ----
        private static Button ChipButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos)
        {
            var go = new GameObject("Chip_" + text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(Mathf.Max(120,30+text.Length*12),44);
            go.GetComponent<Image>().color = Idle;
            Label(rt, "L", text, new Vector2(0.5f,0.5f), Vector2.zero, rt.sizeDelta, 16, Ink, TextAnchor.MiddleCenter, true);
            return go.GetComponent<Button>();
        }

        private static Dropdown DropdownRow(RectTransform parent, string name, Vector2 anchor, Vector2 pos, float width)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Dropdown)); go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(width,44);
            go.GetComponent<Image>().color = Idle;
            var caption = Label(rt, "Label", "All", new Vector2(0,0.5f), new Vector2(12,0), new Vector2(width-40,30), 16, Ink, TextAnchor.MiddleLeft, false);

            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(CanvasGroup)); template.transform.SetParent(go.transform, false);
            var trt=(RectTransform)template.transform; trt.anchorMin=new Vector2(0,0); trt.anchorMax=new Vector2(1,0); trt.pivot=new Vector2(0.5f,1); trt.sizeDelta=new Vector2(0,260);
            template.GetComponent<Image>().color = Panel;
            var vp = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask)); vp.transform.SetParent(template.transform, false);
            var vrt=(RectTransform)vp.transform; vrt.anchorMin=Vector2.zero; vrt.anchorMax=Vector2.one; vrt.offsetMin=Vector2.zero; vrt.offsetMax=Vector2.zero; vp.GetComponent<Mask>().showMaskGraphic=false;
            var cnt = new GameObject("Content", typeof(RectTransform)); cnt.transform.SetParent(vp.transform, false);
            var crt=(RectTransform)cnt.transform; crt.anchorMin=new Vector2(0,1); crt.anchorMax=new Vector2(1,1); crt.pivot=new Vector2(0.5f,1); crt.sizeDelta=new Vector2(0,40);
            var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle)); item.transform.SetParent(cnt.transform, false);
            var irt=(RectTransform)item.transform; irt.anchorMin=new Vector2(0,0.5f); irt.anchorMax=new Vector2(1,0.5f); irt.sizeDelta=new Vector2(0,36);
            var itemBg = new GameObject("Item Background", typeof(RectTransform), typeof(Image)); itemBg.transform.SetParent(item.transform, false); Stretch((RectTransform)itemBg.transform); itemBg.GetComponent<Image>().color = Idle;
            var itemLabel = Label((RectTransform)item.transform, "Item Label", "Option", new Vector2(0,0.5f), new Vector2(12,0), new Vector2(width-40,30), 16, Ink, TextAnchor.MiddleLeft, false);
            item.GetComponent<Toggle>().targetGraphic = itemBg.GetComponent<Image>();
            var scroll = template.GetComponent<ScrollRect>(); scroll.content = crt; scroll.viewport = vrt; scroll.horizontal = false;
            template.SetActive(false);

            var dd = go.GetComponent<Dropdown>();
            dd.template = trt; dd.captionText = caption; dd.itemText = itemLabel; dd.targetGraphic = go.GetComponent<Image>();
            return dd;
        }

        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c)
        {
            var go = new GameObject("Btn_" + text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(180,52);
            go.GetComponent<Image>().color = c;
            Label(rt, "L", text, new Vector2(0.5f,0.5f), Vector2.zero, rt.sizeDelta, 18, Ink, TextAnchor.MiddleCenter, true);
            return go.GetComponent<Button>();
        }

        // ---- primitives ----
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.GetComponent<Image>().color = Panel; return rt;
        }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x, anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size;
            var t = go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t;
        }
        private static void Stretch(RectTransform rt) { rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; }
    }
}
#endif
