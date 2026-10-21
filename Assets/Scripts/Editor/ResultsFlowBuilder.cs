// Project D  |  Codename: Zantetsuken
// ResultsFlowBuilder.cs  (Editor-only)  — Tools > Project D > Build PostGame Results+Analytics
//
// Builds the connected post-game flow: a polished RESULTS panel (outcome, summary,
// rewards, "VIEW MATCH ANALYTICS" + "CONTINUE") and the ANALYTICS panel (header +
// fairness gauge, gold-lead timeline, both scoreboards with MVP, coaching). The
// results screen swaps to analytics and back with fade transitions. No external art.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.PostGameUI;
using ProjectD.Polish;

namespace ProjectD.EditorTools
{
    public static class ResultsFlowBuilder
    {
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static Font _font;

        [MenuItem("Tools/Project D/Build PostGame Results+Analytics", priority = 109)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }
            var canvasGo = new GameObject("Canvas"); var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f; canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // ---------------- RESULTS PANEL ----------------
            var resultsPanel = FullPanel(root, "ResultsPanel");
            var resultsGroup = resultsPanel.gameObject.AddComponent<CanvasGroup>();
            var outcome = Label(resultsPanel, "Outcome", "VICTORY", new Vector2(0.5f,1), new Vector2(0,-90), new Vector2(700,70), 56, Style.Ink, TextAnchor.MiddleCenter, true);
            var summary = Label(resultsPanel, "Summary", "", new Vector2(0.5f,1), new Vector2(0,-170), new Vector2(900,32), 22, Style.Sub, TextAnchor.MiddleCenter, false);
            var rewardsCard = StyledPanel(resultsPanel, "Rewards", new Vector2(0.5f,0.5f), new Vector2(0,40), new Vector2(720,120));
            Label(rewardsCard, "RT", "MATCH REWARDS", new Vector2(0.5f,1), new Vector2(0,-14), new Vector2(400,24), 16, Style.Sub, TextAnchor.MiddleCenter, true);
            var reward = Label(rewardsCard, "R", "", new Vector2(0.5f,0.5f), new Vector2(0,-6), new Vector2(680,36), 26, Style.Gold, TextAnchor.MiddleCenter, true);
            var analyticsBtn = StyledButton(resultsPanel, "VIEW MATCH ANALYTICS", new Vector2(0.5f,0), new Vector2(-190,140), new Color(0.30f,0.45f,0.62f), 340);
            var continueBtn = StyledButton(resultsPanel, "CONTINUE", new Vector2(0.5f,0), new Vector2(190,140), new Color(0.30f,0.55f,0.40f), 300);

            // ---------------- ANALYTICS PANEL ----------------
            var analyticsPanel = FullPanel(root, "AnalyticsPanel");
            var analyticsGroup = analyticsPanel.gameObject.AddComponent<CanvasGroup>();
            Label(analyticsPanel, "Crumb", "POST-GAME  ›  MATCH ANALYTICS", new Vector2(0,1), new Vector2(40,-30), new Vector2(700,26), 16, Style.Sub, TextAnchor.MiddleLeft, true);
            var aOutcome = Label(analyticsPanel, "AOutcome", "VICTORY", new Vector2(0.5f,1), new Vector2(0,-52), new Vector2(500,50), 38, Style.Ink, TextAnchor.MiddleCenter, true);
            var aDur = Label(analyticsPanel, "ADur", "31:15", new Vector2(0.5f,1), new Vector2(0,-98), new Vector2(300,24), 20, Style.Sub, TextAnchor.MiddleCenter, false);
            var backBtn = StyledButton(analyticsPanel, "\u2039 BACK", new Vector2(0,1), new Vector2(220,-64), new Color(0.24f,0.28f,0.36f), 160);

            var head = StyledPanel(analyticsPanel, "Head", new Vector2(0.5f,1), new Vector2(0,-150), new Vector2(1700,64));
            var fairBg = StyledPanel(head, "FBg", new Vector2(0,0.5f), new Vector2(20,0), new Vector2(360,18)); fairBg.pivot=new Vector2(0,0.5f); fairBg.anchorMin=new Vector2(0,0.5f); fairBg.anchorMax=new Vector2(0,0.5f); fairBg.GetComponent<Image>().color=new Color(1,1,1,0.1f);
            var fairGauge = Fill(fairBg.transform, new Color(0.45f,0.85f,0.5f));
            var grade = Label(head, "Grade", "Match quality: —", new Vector2(0,0.5f), new Vector2(400,0), new Vector2(500,26), 18, Style.Ink, TextAnchor.MiddleLeft, true);
            var spread = Label(head, "Spread", "MMR spread: —", new Vector2(1,0.5f), new Vector2(-20,0), new Vector2(600,24), 16, Style.Sub, TextAnchor.MiddleRight, false);

            var tl = StyledPanel(analyticsPanel, "Timeline", new Vector2(0.5f,0.5f), new Vector2(0,150), new Vector2(1700,280));
            Label(tl, "TT", "GOLD LEAD OVER TIME", new Vector2(0,1), new Vector2(20,-12), new Vector2(500,24), 16, Style.Sub, TextAnchor.MiddleLeft, true);
            var zero = ChildImage(tl.transform, "Zero", new Color(1,1,1,0.18f)); var zr=(RectTransform)zero.transform; zr.anchorMin=new Vector2(0,0.5f); zr.anchorMax=new Vector2(1,0.5f); zr.pivot=new Vector2(0.5f,0.5f); zr.sizeDelta=new Vector2(-40,2); zr.anchoredPosition=new Vector2(0,-10);
            var area = new GameObject("Area", typeof(RectTransform)); area.transform.SetParent(tl,false); var art=(RectTransform)area.transform; art.anchorMin=new Vector2(0.5f,0.5f); art.anchorMax=new Vector2(0.5f,0.5f); art.pivot=new Vector2(0.5f,0.5f); art.anchoredPosition=new Vector2(0,-10); art.sizeDelta=new Vector2(1620,190);
            var seg = BuildSegment();
            var peakB = Label(tl, "PB", "", new Vector2(0,0), new Vector2(24,14), new Vector2(400,22), 15, new Color(0.4f,0.65f,1f), TextAnchor.MiddleLeft, true);
            var peakR = Label(tl, "PR", "", new Vector2(0.5f,0), new Vector2(0,14), new Vector2(400,22), 15, new Color(0.9f,0.45f,0.45f), TextAnchor.MiddleCenter, true);
            var finalD = Label(tl, "FD", "", new Vector2(1,0), new Vector2(-24,14), new Vector2(400,22), 16, Style.Ink, TextAnchor.MiddleRight, true);

            var blue = TeamPanel(analyticsPanel, "BLUE TEAM", new Vector2(0.5f,0), new Vector2(-470,250), new Color(0.4f,0.65f,1f), out var blueList);
            var red = TeamPanel(analyticsPanel, "RED TEAM", new Vector2(0.5f,0), new Vector2(470,250), new Color(1f,0.45f,0.45f), out var redList);
            var rowPrefab = BuildPlayerRow();

            var coach = StyledPanel(analyticsPanel, "Coaching", new Vector2(0.5f,0), new Vector2(0,40), new Vector2(1700,180));
            var coachHeader = Label(coach, "CH", "Coaching", new Vector2(0,1), new Vector2(20,-14), new Vector2(900,26), 18, Style.Gold, TextAnchor.MiddleLeft, true);
            var coachList = new GameObject("List", typeof(RectTransform)); coachList.transform.SetParent(coach,false); var clrt=(RectTransform)coachList.transform; clrt.anchorMin=new Vector2(0,0); clrt.anchorMax=new Vector2(1,1); clrt.offsetMin=new Vector2(16,12); clrt.offsetMax=new Vector2(-16,-46);
            var cvlg=coachList.AddComponent<VerticalLayoutGroup>(); cvlg.spacing=6; cvlg.childForceExpandHeight=false; cvlg.childControlHeight=false;
            var coachRow = BuildCoachRow();

            // Analytics view component.
            var av = analyticsPanel.gameObject.AddComponent<MatchAnalyticsView>();
            var avso = new SerializedObject(av);
            avso.FindProperty("outcomeText").objectReferenceValue = aOutcome;
            avso.FindProperty("durationText").objectReferenceValue = aDur;
            avso.FindProperty("mmrSpreadText").objectReferenceValue = spread;
            avso.FindProperty("fairnessGradeText").objectReferenceValue = grade;
            avso.FindProperty("fairnessGauge").objectReferenceValue = fairGauge;
            avso.FindProperty("timelineArea").objectReferenceValue = art;
            avso.FindProperty("segmentPrefab").objectReferenceValue = seg;
            avso.FindProperty("peakBlueText").objectReferenceValue = peakB;
            avso.FindProperty("peakRedText").objectReferenceValue = peakR;
            avso.FindProperty("finalDiffText").objectReferenceValue = finalD;
            avso.FindProperty("blueParent").objectReferenceValue = blueList;
            avso.FindProperty("redParent").objectReferenceValue = redList;
            avso.FindProperty("playerRowPrefab").objectReferenceValue = rowPrefab;
            avso.FindProperty("coachingHeader").objectReferenceValue = coachHeader;
            avso.FindProperty("coachingListParent").objectReferenceValue = clrt;
            avso.FindProperty("coachingRowPrefab").objectReferenceValue = coachRow;
            avso.ApplyModifiedPropertiesWithoutUndo();

            // Results screen component (owns both panels).
            var screen = canvasGo.AddComponent<PostGameResultsScreen>();
            var so = new SerializedObject(screen);
            so.FindProperty("resultsPanel").objectReferenceValue = resultsPanel.gameObject;
            so.FindProperty("resultsGroup").objectReferenceValue = resultsGroup;
            so.FindProperty("outcomeText").objectReferenceValue = outcome;
            so.FindProperty("summaryText").objectReferenceValue = summary;
            so.FindProperty("rewardText").objectReferenceValue = reward;
            so.FindProperty("analyticsButton").objectReferenceValue = analyticsBtn;
            so.FindProperty("continueButton").objectReferenceValue = continueBtn;
            so.FindProperty("analyticsPanel").objectReferenceValue = analyticsPanel.gameObject;
            so.FindProperty("analyticsGroup").objectReferenceValue = analyticsGroup;
            so.FindProperty("analyticsView").objectReferenceValue = av;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<ResultsFlowDemoDriver>();
            var dso = new SerializedObject(driver); dso.FindProperty("screen").objectReferenceValue = screen; dso.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/PostGameResultsFlow.unity");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D", "PostGame Results + Analytics built.\nPress Play: the results screen shows VICTORY + rewards. Tap VIEW MATCH ANALYTICS to open the polished analytics (animated fairness gauge, timeline, staggered scoreboards, MVP glow). BACK returns.", "OK");
        }

        // ---- panels / prefabs ----
        private static RectTransform FullPanel(RectTransform root, string name){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(root,false); var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; go.GetComponent<Image>().color=new Color(0,0,0,0); return rt; }
        private static RectTransform TeamPanel(RectTransform root, string title, Vector2 anchor, Vector2 pos, Color c, out RectTransform list)
        {
            var panel = StyledPanel(root, "Team_"+title, anchor, pos, new Vector2(760, 380)); panel.pivot = new Vector2(0.5f,0);
            Label(panel, "T", title, new Vector2(0,1), new Vector2(20,-14), new Vector2(300,26), 18, c, TextAnchor.MiddleLeft, true);
            var hdr = new[] { ("Player",20f),("K/D/A",300f),("CS",440f),("Gold",520f),("DMG",620f) };
            foreach (var (t,x) in hdr) Label(panel, "H_"+t, t, new Vector2(0,1), new Vector2(x,-44), new Vector2(120,20), 13, Style.Sub, TextAnchor.MiddleLeft, true);
            var lg = new GameObject("List", typeof(RectTransform)); lg.transform.SetParent(panel,false);
            var lrt=(RectTransform)lg.transform; lrt.anchorMin=new Vector2(0,1); lrt.anchorMax=new Vector2(1,1); lrt.pivot=new Vector2(0.5f,1); lrt.anchoredPosition=new Vector2(0,-72); lrt.sizeDelta=new Vector2(-24,300);
            var vlg=lg.AddComponent<VerticalLayoutGroup>(); vlg.spacing=6; vlg.padding=new RectOffset(12,12,0,0); vlg.childForceExpandHeight=false; vlg.childControlHeight=false;
            list = lrt; return panel;
        }
        private static Button BuildPlayerRow()
        {
            var go = new GameObject("PlayerRow", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            Style.Panel(go.GetComponent<Image>()); go.GetComponent<Image>().color = new Color(0.13f,0.15f,0.21f,0.9f); go.GetComponent<LayoutElement>().preferredHeight = 48;
            var crown = ChildImage(go.transform, "Crown", Style.Gold); Style.Badge(crown, Style.Gold); var cr=(RectTransform)crown.transform; cr.anchorMin=new Vector2(0,1); cr.anchorMax=new Vector2(0,1); cr.pivot=new Vector2(0,1); cr.anchoredPosition=new Vector2(4,-2); cr.sizeDelta=new Vector2(18,18); crown.enabled=false;
            Label((RectTransform)go.transform, "Name", "player", new Vector2(0,0.5f), new Vector2(26,0), new Vector2(260,24), 16, Style.Ink, TextAnchor.MiddleLeft, true);
            Label((RectTransform)go.transform, "Kda", "0/0/0", new Vector2(0,0.5f), new Vector2(300,0), new Vector2(130,24), 15, Style.Sub, TextAnchor.MiddleLeft, false);
            Label((RectTransform)go.transform, "Cs", "0 cs", new Vector2(0,0.5f), new Vector2(440,0), new Vector2(90,24), 14, Style.Sub, TextAnchor.MiddleLeft, false);
            Label((RectTransform)go.transform, "Gold", "0g", new Vector2(0,0.5f), new Vector2(520,0), new Vector2(100,24), 14, Style.Gold, TextAnchor.MiddleLeft, false);
            Label((RectTransform)go.transform, "Dmg", "0% dmg", new Vector2(1,0.5f), new Vector2(-8,0), new Vector2(120,24), 14, Style.Sub, TextAnchor.MiddleRight, false);
            go.SetActive(false); return go.GetComponent<Button>();
        }
        private static RectTransform BuildCoachRow()
        {
            var go = new GameObject("CoachRow", typeof(RectTransform), typeof(LayoutElement)); go.GetComponent<LayoutElement>().preferredHeight = 28;
            var icon = ChildImage(go.transform, "Icon", new Color(0.6f,0.7f,0.8f)); Style.Badge(icon, new Color(0.6f,0.7f,0.8f)); var ir=(RectTransform)icon.transform; ir.anchorMin=new Vector2(0,0.5f); ir.anchorMax=new Vector2(0,0.5f); ir.pivot=new Vector2(0,0.5f); ir.anchoredPosition=new Vector2(4,0); ir.sizeDelta=new Vector2(16,16);
            Label((RectTransform)go.transform, "Msg", "insight", new Vector2(0,0.5f), new Vector2(28,0), new Vector2(1600,24), 15, Style.Ink, TextAnchor.MiddleLeft, false);
            go.SetActive(false); return (RectTransform)go.transform;
        }
        private static Image BuildSegment(){ var go=new GameObject("Seg", typeof(RectTransform), typeof(Image)); go.GetComponent<Image>().color=new Color(0.4f,0.65f,1f); go.SetActive(false); return go.GetComponent<Image>(); }

        private static RectTransform StyledPanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size){ var rt=RawPanel(parent,name,anchor,pos,size); Style.Panel(rt.GetComponent<Image>()); rt.GetComponent<Image>().color=Panel; return rt; }
        private static RectTransform RawPanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Panel; return rt; }
        private static Button StyledButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c, float width){ var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(width,56); Style.Button(go.GetComponent<Image>(),c); Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(width,56),18,Style.Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Image ChildImage(Transform parent, string name, Color c){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); go.GetComponent<Image>().color=c; go.GetComponent<Image>().raycastTarget=false; return go.GetComponent<Image>(); }
        private static Image Fill(Transform parent, Color c){ var go=new GameObject("Fill", typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; var img=go.GetComponent<Image>(); img.color=c; img.type=Image.Type.Filled; img.fillMethod=Image.FillMethod.Horizontal; img.fillOrigin=(int)Image.OriginHorizontal.Left; img.fillAmount=0f; return img; }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
    }
}
#endif
