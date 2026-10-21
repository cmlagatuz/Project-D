// Project D  |  Codename: Zantetsuken
// MatchAnalyticsBuilder.cs  (Editor-only)  — Tools > Project D > Build Match Analytics Demo
//
// Builds the post-game "Match Analytics" screen: a header with outcome + fairness gauge,
// a gold-lead timeline, both team scoreboards (MVP crowned, click a player), and a
// coaching panel. This is the screen reached from the post-game results via a
// "VIEW MATCH ANALYTICS" button. Populated live from the MatchAnalytics core.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.PostGameUI;

namespace ProjectD.EditorTools
{
    public static class MatchAnalyticsBuilder
    {
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static Font _font;

        [MenuItem("Tools/Project D/Build Match Analytics Demo", priority = 108)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }
            var canvasGo = new GameObject("Canvas"); var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = canvasGo.AddComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920,1080); sc.matchWidthOrHeight = 0.5f; canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            Label(root, "Crumb", "POST-GAME  ›  MATCH ANALYTICS", new Vector2(0,1), new Vector2(40,-30), new Vector2(700,26), 16, Sub, TextAnchor.MiddleLeft, true);
            var outcome = Label(root, "Outcome", "VICTORY", new Vector2(0.5f,1), new Vector2(0,-56), new Vector2(500,50), 40, Ink, TextAnchor.MiddleCenter, true);
            var duration = Label(root, "Dur", "31:15", new Vector2(0.5f,1), new Vector2(0,-104), new Vector2(300,26), 20, Sub, TextAnchor.MiddleCenter, false);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Header stats: fairness gauge + grade + spread.
            var head = Panel_(root, "Head", new Vector2(0.5f,1), new Vector2(0,-160), new Vector2(1700,70));
            var fairBg = Panel_(head, "FBg", new Vector2(0,0.5f), new Vector2(20,0), new Vector2(360,18)); fairBg.pivot=new Vector2(0,0.5f); fairBg.anchorMin=new Vector2(0,0.5f); fairBg.anchorMax=new Vector2(0,0.5f); fairBg.GetComponent<Image>().color=new Color(1,1,1,0.1f);
            var fairGauge = Fill(fairBg.transform, new Color(0.45f,0.85f,0.5f));
            var grade = Label(head, "Grade", "Match quality: —", new Vector2(0,0.5f), new Vector2(400,0), new Vector2(500,28), 18, Ink, TextAnchor.MiddleLeft, true);
            var spread = Label(head, "Spread", "MMR spread: —", new Vector2(1,0.5f), new Vector2(-20,0), new Vector2(600,26), 16, Sub, TextAnchor.MiddleRight, false);

            // Timeline.
            var tl = Panel_(root, "Timeline", new Vector2(0.5f,0.5f), new Vector2(0,150), new Vector2(1700,300));
            Label(tl, "TT", "GOLD LEAD OVER TIME", new Vector2(0,1), new Vector2(20,-12), new Vector2(500,24), 16, Sub, TextAnchor.MiddleLeft, true);
            var zero = ChildImage(tl.transform, "Zero", new Color(1,1,1,0.18f)); var zr=(RectTransform)zero.transform; zr.anchorMin=new Vector2(0,0.5f); zr.anchorMax=new Vector2(1,0.5f); zr.pivot=new Vector2(0.5f,0.5f); zr.sizeDelta=new Vector2(-40,2); zr.anchoredPosition=new Vector2(0,-10);
            var area = new GameObject("Area", typeof(RectTransform)); area.transform.SetParent(tl,false); var art=(RectTransform)area.transform; art.anchorMin=new Vector2(0.5f,0.5f); art.anchorMax=new Vector2(0.5f,0.5f); art.pivot=new Vector2(0.5f,0.5f); art.anchoredPosition=new Vector2(0,-10); art.sizeDelta=new Vector2(1620,200);
            var seg = BuildSegment();
            var peakB = Label(tl, "PB", "", new Vector2(0,0), new Vector2(24,16), new Vector2(400,22), 15, new Color(0.4f,0.65f,1f), TextAnchor.MiddleLeft, true);
            var peakR = Label(tl, "PR", "", new Vector2(0.5f,0), new Vector2(0,16), new Vector2(400,22), 15, new Color(0.9f,0.45f,0.45f), TextAnchor.MiddleCenter, true);
            var finalD = Label(tl, "FD", "", new Vector2(1,0), new Vector2(-24,16), new Vector2(400,22), 16, Ink, TextAnchor.MiddleRight, true);

            // Scoreboards.
            var blue = TeamPanel(root, "BLUE TEAM", new Vector2(0.5f,0), new Vector2(-470,250), new Color(0.4f,0.65f,1f), out var blueList);
            var red = TeamPanel(root, "RED TEAM", new Vector2(0.5f,0), new Vector2(470,250), new Color(1f,0.45f,0.45f), out var redList);
            var rowPrefab = BuildPlayerRow();

            // Coaching panel.
            var coach = Panel_(root, "Coaching", new Vector2(0.5f,0), new Vector2(0,40), new Vector2(1700,180));
            var coachHeader = Label(coach, "CH", "Coaching", new Vector2(0,1), new Vector2(20,-14), new Vector2(700,26), 18, Gold, TextAnchor.MiddleLeft, true);
            var coachList = new GameObject("List", typeof(RectTransform)); coachList.transform.SetParent(coach,false); var clrt=(RectTransform)coachList.transform; clrt.anchorMin=new Vector2(0,0); clrt.anchorMax=new Vector2(1,1); clrt.offsetMin=new Vector2(16,12); clrt.offsetMax=new Vector2(-16,-46);
            var vlg=coachList.AddComponent<VerticalLayoutGroup>(); vlg.spacing=6; vlg.childForceExpandHeight=false; vlg.childControlHeight=false;
            var coachRow = BuildCoachRow();

            var view = canvasGo.AddComponent<MatchAnalyticsView>();
            var so = new SerializedObject(view);
            so.FindProperty("outcomeText").objectReferenceValue = outcome;
            so.FindProperty("durationText").objectReferenceValue = duration;
            so.FindProperty("mmrSpreadText").objectReferenceValue = spread;
            so.FindProperty("fairnessGradeText").objectReferenceValue = grade;
            so.FindProperty("fairnessGauge").objectReferenceValue = fairGauge;
            so.FindProperty("timelineArea").objectReferenceValue = art;
            so.FindProperty("segmentPrefab").objectReferenceValue = seg;
            so.FindProperty("peakBlueText").objectReferenceValue = peakB;
            so.FindProperty("peakRedText").objectReferenceValue = peakR;
            so.FindProperty("finalDiffText").objectReferenceValue = finalD;
            so.FindProperty("blueParent").objectReferenceValue = blueList;
            so.FindProperty("redParent").objectReferenceValue = redList;
            so.FindProperty("playerRowPrefab").objectReferenceValue = rowPrefab;
            so.FindProperty("coachingHeader").objectReferenceValue = coachHeader;
            so.FindProperty("coachingListParent").objectReferenceValue = clrt;
            so.FindProperty("coachingRowPrefab").objectReferenceValue = coachRow;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<MatchAnalyticsDemoDriver>();
            var dso = new SerializedObject(driver); dso.FindProperty("view").objectReferenceValue = view; dso.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/MatchAnalyticsDemo.unity");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D", "Match Analytics (post-game) built.\nPress Play: this match's outcome + fairness grade, the gold-lead timeline, both scoreboards (MVP crowned), and coaching. Click a player row to see THEIR coaching tips.", "OK");
        }

        private static RectTransform TeamPanel(RectTransform root, string title, Vector2 anchor, Vector2 pos, Color c, out RectTransform list)
        {
            var panel = Panel_(root, "Team_"+title, anchor, pos, new Vector2(760, 380)); panel.pivot = new Vector2(0.5f,0);
            Label(panel, "T", title, new Vector2(0,1), new Vector2(20,-14), new Vector2(300,26), 18, c, TextAnchor.MiddleLeft, true);
            var hdr = new[] { ("Player",20f),("K/D/A",300f),("CS",440f),("Gold",520f),("DMG",620f) };
            foreach (var (t,x) in hdr) Label(panel, "H_"+t, t, new Vector2(0,1), new Vector2(x,-44), new Vector2(120,20), 13, Sub, TextAnchor.MiddleLeft, true);
            var lg = new GameObject("List", typeof(RectTransform)); lg.transform.SetParent(panel,false);
            var lrt=(RectTransform)lg.transform; lrt.anchorMin=new Vector2(0,1); lrt.anchorMax=new Vector2(1,1); lrt.pivot=new Vector2(0.5f,1); lrt.anchoredPosition=new Vector2(0,-72); lrt.sizeDelta=new Vector2(-24,300);
            var vlg=lg.AddComponent<VerticalLayoutGroup>(); vlg.spacing=6; vlg.padding=new RectOffset(12,12,0,0); vlg.childForceExpandHeight=false; vlg.childControlHeight=false;
            list = lrt; return panel;
        }

        private static Button BuildPlayerRow()
        {
            var go = new GameObject("PlayerRow", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.GetComponent<Image>().color = new Color(0.13f,0.15f,0.21f,0.9f); go.GetComponent<LayoutElement>().preferredHeight = 48;
            var crown = ChildImage(go.transform, "Crown", Gold); var cr=(RectTransform)crown.transform; cr.anchorMin=new Vector2(0,1); cr.anchorMax=new Vector2(0,1); cr.pivot=new Vector2(0,1); cr.anchoredPosition=new Vector2(4,-2); cr.sizeDelta=new Vector2(18,18); crown.enabled=false;
            Label((RectTransform)go.transform, "Name", "player", new Vector2(0,0.5f), new Vector2(26,0), new Vector2(260,24), 16, Ink, TextAnchor.MiddleLeft, true);
            Label((RectTransform)go.transform, "Kda", "0/0/0", new Vector2(0,0.5f), new Vector2(300,0), new Vector2(130,24), 15, Sub, TextAnchor.MiddleLeft, false);
            Label((RectTransform)go.transform, "Cs", "0 cs", new Vector2(0,0.5f), new Vector2(440,0), new Vector2(90,24), 14, Sub, TextAnchor.MiddleLeft, false);
            Label((RectTransform)go.transform, "Gold", "0g", new Vector2(0,0.5f), new Vector2(520,0), new Vector2(100,24), 14, Gold, TextAnchor.MiddleLeft, false);
            Label((RectTransform)go.transform, "Dmg", "0% dmg", new Vector2(1,0.5f), new Vector2(-8,0), new Vector2(120,24), 14, Sub, TextAnchor.MiddleRight, false);
            go.SetActive(false); return go.GetComponent<Button>();
        }
        private static RectTransform BuildCoachRow()
        {
            var go = new GameObject("CoachRow", typeof(RectTransform), typeof(LayoutElement));
            go.GetComponent<LayoutElement>().preferredHeight = 28;
            var icon = ChildImage(go.transform, "Icon", new Color(0.6f,0.7f,0.8f)); var ir=(RectTransform)icon.transform; ir.anchorMin=new Vector2(0,0.5f); ir.anchorMax=new Vector2(0,0.5f); ir.pivot=new Vector2(0,0.5f); ir.anchoredPosition=new Vector2(4,0); ir.sizeDelta=new Vector2(16,16);
            Label((RectTransform)go.transform, "Msg", "insight", new Vector2(0,0.5f), new Vector2(28,0), new Vector2(1600,24), 15, Ink, TextAnchor.MiddleLeft, false);
            go.SetActive(false); return (RectTransform)go.transform;
        }
        private static Image BuildSegment(){ var go=new GameObject("Seg", typeof(RectTransform), typeof(Image)); go.GetComponent<Image>().color=new Color(0.4f,0.65f,1f); go.SetActive(false); return go.GetComponent<Image>(); }

        private static RectTransform Panel_(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Panel; return rt; }
        private static Image ChildImage(Transform parent, string name, Color c){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); go.GetComponent<Image>().color=c; go.GetComponent<Image>().raycastTarget=false; return go.GetComponent<Image>(); }
        private static Image Fill(Transform parent, Color c){ var go=new GameObject("Fill", typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; var img=go.GetComponent<Image>(); img.color=c; img.type=Image.Type.Filled; img.fillMethod=Image.FillMethod.Horizontal; img.fillOrigin=(int)Image.OriginHorizontal.Left; img.fillAmount=0f; return img; }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
    }
}
#endif
