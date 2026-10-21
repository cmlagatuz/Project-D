// Project D  |  Codename: Zantetsuken
// OpsConsoleBuilder.cs  (Editor-only)  — Tools > Project D > Build Ops Console
//
// Builds the P12 live-ops / governance console: tiles for Live-Ops schedule (advance
// clock), Moderation queue (triaged), Incident board (advance), Experiment rollout
// (ramp/kill), and Clan war standings — all live from the P12 cores. No external art.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.P12UI;

namespace ProjectD.EditorTools
{
    public static class OpsConsoleBuilder
    {
        private static readonly Color Bg = new Color(0.05f,0.07f,0.10f);
        private static readonly Color Tile = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static Font _font;

        [MenuItem("Tools/Project D/Build Ops Console", priority = 110)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }
            var canvasGo = new GameObject("Canvas"); var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = canvasGo.AddComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920,1080); sc.matchWidthOrHeight = 0.5f; canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            Label(root, "Title", "LIVE-OPS & GOVERNANCE CONSOLE", new Vector2(0,1), new Vector2(40,-34), new Vector2(800,44), 30, Ink, TextAnchor.MiddleLeft, true);
            Label(root, "Status", "\u25CF OPERATOR", new Vector2(1,1), new Vector2(-40,-34), new Vector2(240,30), 20, Gold, TextAnchor.MiddleRight, true);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-40,-70), new Vector2(420,22), 13, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            var view = canvasGo.AddComponent<OpsConsoleView>();
            var so = new SerializedObject(view);

            // ---- Live-Ops schedule (top-left) ----
            var sch = Tile_(root, "Live-Ops Schedule", new Vector2(0,1), new Vector2(340,-300), new Vector2(600,380));
            var clock = Label(sch, "Clock", "Clock: 00:00", new Vector2(0,1), new Vector2(20,-52), new Vector2(300,24), 16, Ink, TextAnchor.MiddleLeft, true);
            var audit = Label(sch, "Audit", "Audit entries: 0", new Vector2(1,1), new Vector2(-20,-52), new Vector2(300,24), 14, Sub, TextAnchor.MiddleRight, false);
            var schList = VList(sch, new Vector2(0,-84), new Vector2(-40,220));
            var schRow = TwoCol();
            var advClock = Btn(sch, "ADVANCE +1h", new Vector2(0.5f,0), new Vector2(0,20), new Color(0.30f,0.45f,0.62f), 260);
            so.FindProperty("scheduleListParent").objectReferenceValue = schList;
            so.FindProperty("scheduleRowPrefab").objectReferenceValue = schRow;
            so.FindProperty("clockText").objectReferenceValue = clock;
            so.FindProperty("auditCountText").objectReferenceValue = audit;
            so.FindProperty("advanceClockButton").objectReferenceValue = advClock;

            // ---- Moderation queue (top-center) ----
            var mod = Tile_(root, "Moderation Queue", new Vector2(0.5f,1), new Vector2(0,-300), new Vector2(600,380));
            var modPending = Label(mod, "P", "Pending: 0", new Vector2(0,1), new Vector2(20,-52), new Vector2(300,24), 16, Ink, TextAnchor.MiddleLeft, true);
            var modList = VList(mod, new Vector2(0,-84), new Vector2(-40,260));
            var modRow = ThreeCol();
            so.FindProperty("modListParent").objectReferenceValue = modList;
            so.FindProperty("modRowPrefab").objectReferenceValue = modRow;
            so.FindProperty("modPendingText").objectReferenceValue = modPending;

            // ---- Incident board (top-right) ----
            var inc = Tile_(root, "Incident Board", new Vector2(1,1), new Vector2(-340,-300), new Vector2(600,380));
            var incOpen = Label(inc, "O", "Open: 0", new Vector2(0,1), new Vector2(20,-52), new Vector2(300,24), 16, Ink, TextAnchor.MiddleLeft, true);
            var incList = VList(inc, new Vector2(0,-84), new Vector2(-40,200));
            var incRow = ThreeCol();
            var advInc = Btn(inc, "ADVANCE INCIDENT", new Vector2(0.5f,0), new Vector2(0,20), new Color(0.55f,0.40f,0.35f), 300);
            so.FindProperty("incListParent").objectReferenceValue = incList;
            so.FindProperty("incRowPrefab").objectReferenceValue = incRow;
            so.FindProperty("incOpenText").objectReferenceValue = incOpen;
            so.FindProperty("advanceIncidentButton").objectReferenceValue = advInc;

            // ---- Experiment rollout (bottom-left) ----
            var roll = Tile_(root, "Experiment Rollout", new Vector2(0,0), new Vector2(340,240), new Vector2(600,300));
            var rStage = Label(roll, "S", "Stage: Off", new Vector2(0,1), new Vector2(20,-56), new Vector2(400,26), 20, Gold, TextAnchor.MiddleLeft, true);
            var rPct = Label(roll, "Pct", "0% of users", new Vector2(0,1), new Vector2(20,-90), new Vector2(400,24), 16, Sub, TextAnchor.MiddleLeft, false);
            var rBg = Panel(roll, "RBg", new Vector2(0,1), new Vector2(20,-120), new Vector2(540,20)); rBg.pivot=new Vector2(0,1); rBg.anchorMin=new Vector2(0,1); rBg.anchorMax=new Vector2(0,1); rBg.GetComponent<Image>().color=new Color(1,1,1,0.1f);
            var rBar = Fill(rBg.transform, new Color(0.45f,0.85f,0.5f));
            var rAdv = Btn(roll, "RAMP UP", new Vector2(0.5f,0), new Vector2(-140,26), new Color(0.30f,0.55f,0.40f), 240);
            var rKill = Btn(roll, "KILL SWITCH", new Vector2(0.5f,0), new Vector2(140,26), new Color(0.62f,0.35f,0.35f), 240);
            so.FindProperty("rolloutStageText").objectReferenceValue = rStage;
            so.FindProperty("rolloutPctText").objectReferenceValue = rPct;
            so.FindProperty("rolloutBar").objectReferenceValue = rBar;
            so.FindProperty("rolloutAdvanceButton").objectReferenceValue = rAdv;
            so.FindProperty("rolloutKillButton").objectReferenceValue = rKill;

            // ---- Clan war standings (bottom-right) ----
            var clan = Tile_(root, "Clan War Standings", new Vector2(1,0), new Vector2(-340,240), new Vector2(600,300));
            var clanList = VList(clan, new Vector2(0,-56), new Vector2(-40,220));
            var clanRow = ThreeCol();
            so.FindProperty("clanListParent").objectReferenceValue = clanList;
            so.FindProperty("clanRowPrefab").objectReferenceValue = clanRow;

            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<OpsConsoleDemoDriver>();
            var dso = new SerializedObject(driver); dso.FindProperty("view").objectReferenceValue = view; dso.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/OpsConsole.unity");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D", "Ops Console built.\nPress Play: advance the clock to toggle events, view the triaged moderation queue + routed actions, advance incidents (Sev1/Sev2 need postmortems to close), ramp/kill the experiment rollout, and read clan-war standings — all live from the P12 cores.", "OK");
        }

        // ---- prefabs ----
        private static RectTransform TwoCol(){ var go=new GameObject("Row2", typeof(RectTransform), typeof(LayoutElement)); go.GetComponent<LayoutElement>().preferredHeight=34; Label((RectTransform)go.transform,"A","name",new Vector2(0,0.5f),new Vector2(6,0),new Vector2(360,24),15,Ink,TextAnchor.MiddleLeft,false); Label((RectTransform)go.transform,"B","state",new Vector2(1,0.5f),new Vector2(-8,0),new Vector2(160,24),15,Sub,TextAnchor.MiddleRight,true); go.SetActive(false); return (RectTransform)go.transform; }
        private static RectTransform ThreeCol(){ var go=new GameObject("Row3", typeof(RectTransform), typeof(LayoutElement)); go.GetComponent<LayoutElement>().preferredHeight=34; Label((RectTransform)go.transform,"A","col1",new Vector2(0,0.5f),new Vector2(6,0),new Vector2(300,24),15,Ink,TextAnchor.MiddleLeft,false); Label((RectTransform)go.transform,"B","col2",new Vector2(0.5f,0.5f),new Vector2(40,0),new Vector2(140,24),15,Gold,TextAnchor.MiddleCenter,true); Label((RectTransform)go.transform,"C","col3",new Vector2(1,0.5f),new Vector2(-8,0),new Vector2(180,24),15,Sub,TextAnchor.MiddleRight,true); go.SetActive(false); return (RectTransform)go.transform; }

        private static RectTransform Tile_(RectTransform root, string title, Vector2 anchor, Vector2 pos, Vector2 size)
        { var t=Panel(root,"Tile_"+title,anchor,pos,size); t.GetComponent<Image>().color=Tile; Label(t,"TT",title.ToUpperInvariant(),new Vector2(0,1),new Vector2(20,-16),new Vector2(500,28),20,Gold,TextAnchor.MiddleLeft,true); return t; }
        private static RectTransform VList(RectTransform parent, Vector2 pos, Vector2 size){ var go=new GameObject("List", typeof(RectTransform)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=new Vector2(0,1); rt.anchorMax=new Vector2(1,1); rt.pivot=new Vector2(0.5f,1); rt.anchoredPosition=pos; rt.sizeDelta=size; var vlg=go.AddComponent<VerticalLayoutGroup>(); vlg.spacing=4; vlg.padding=new RectOffset(14,14,0,0); vlg.childForceExpandHeight=false; vlg.childControlHeight=false; return rt; }
        private static RectTransform Panel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Tile; return rt; }
        private static Image Fill(Transform parent, Color c){ var go=new GameObject("Fill", typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; var img=go.GetComponent<Image>(); img.color=c; img.type=Image.Type.Filled; img.fillMethod=Image.FillMethod.Horizontal; img.fillOrigin=(int)Image.OriginHorizontal.Left; img.fillAmount=0f; return img; }
        private static Button Btn(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c, float w){ var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(w,48); go.GetComponent<Image>().color=c; Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(w,48),16,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
    }
}
#endif
