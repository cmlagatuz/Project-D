// Project D  |  Codename: Zantetsuken
// P2ScreensBuilder.cs  (Editor-only)  — builds all P2 demo scenes:
//   Tools > Project D > Build Ping Wheel Demo
//   Tools > Project D > Build HUD Widgets Demo
//   Tools > Project D > Build Matchmaking Demo
//   Tools > Project D > Build Match Graphs Demo
//   Tools > Project D > Build Tutorial Demo

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.CommsUI;
using ProjectD.HUDUI;
using ProjectD.MatchmakingUI;
using ProjectD.Matchmaking;
using ProjectD.GraphsUI;
using ProjectD.TutorialUI;
using ProjectD.P2UI;

namespace ProjectD.EditorTools
{
    public static class P2ScreensBuilder
    {
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static readonly Color Idle = new Color(0.16f,0.18f,0.24f);
        private static Font _font;

        // ---------- PING WHEEL ----------
        [MenuItem("Tools/Project D/Build Ping Wheel Demo", priority = 95)]
        public static void BuildPingWheel()
        {
            _font = Font(); var root = NewScene(out var canvasGo);
            Label(root, "Title", "PING WHEEL / QUICK CHAT", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(900,40), 28, Ink, TextAnchor.MiddleCenter, true);
            Watermark(root);

            var wheelRoot = MakePanel(root, "WheelRoot", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(420,420)); wheelRoot.GetComponent<Image>().color = new Color(0,0,0,0.35f);
            var slotPrefab = BuildSlotPrefab();
            var strip = MakePanel(root, "QuickChatStrip", new Vector2(0.5f,0), new Vector2(0,120), new Vector2(1400,60)); strip.GetComponent<Image>().color=new Color(0,0,0,0);
            var stripH = strip.gameObject.AddComponent<HorizontalLayoutGroup>(); stripH.spacing=10; stripH.childForceExpandWidth=true; stripH.childForceExpandHeight=true;
            var qcPrefab = BuildQuickChatPrefab();

            var wheel = canvasGo.AddComponent<PingWheel>();
            var so=new SerializedObject(wheel);
            so.FindProperty("wheelRoot").objectReferenceValue=wheelRoot;
            so.FindProperty("slotPrefab").objectReferenceValue=slotPrefab;
            so.FindProperty("radius").floatValue=150f;
            so.FindProperty("quickChatStrip").objectReferenceValue=strip;
            so.FindProperty("quickChatButtonPrefab").objectReferenceValue=qcPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
            var driver=canvasGo.AddComponent<PingWheelDemoDriver>(); var dso=new SerializedObject(driver); dso.FindProperty("wheel").objectReferenceValue=wheel; dso.ApplyModifiedPropertiesWithoutUndo();

            Save("Assets/Scenes/PingWheelDemo.unity");
            Done("Ping Wheel demo built. 6-slot radial + quick-chat strip. Click a slot to fire a ping.");
        }

        // ---------- HUD WIDGETS ----------
        [MenuItem("Tools/Project D/Build HUD Widgets Demo", priority = 96)]
        public static void BuildHud()
        {
            _font = Font(); var root = NewScene(out var canvasGo);
            Label(root, "Title", "HUD WIDGETS", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(900,40), 28, Ink, TextAnchor.MiddleCenter, true);
            Watermark(root);

            // Recall bar.
            var recallPanel = MakePanel(root, "Recall", new Vector2(0.5f,0.5f), new Vector2(0,260), new Vector2(520,70));
            var rGroup = recallPanel.gameObject.AddComponent<CanvasGroup>();
            var rBack = MakePanel(recallPanel, "Back", new Vector2(0.5f,0.5f), new Vector2(0,-4), new Vector2(480,18)); rBack.GetComponent<Image>().color=new Color(1,1,1,0.12f);
            var rFill = ChildImage(rBack.transform, "Fill", new Color(0.4f,0.65f,1f), raycast:false); Stretch((RectTransform)rFill.transform); rFill.type=Image.Type.Filled; rFill.fillMethod=Image.FillMethod.Horizontal; rFill.fillOrigin=(int)Image.OriginHorizontal.Left; rFill.fillAmount=0f;
            var rLabel = Label(recallPanel, "L", "Recalling...", new Vector2(0.5f,1), new Vector2(0,-8), new Vector2(480,22), 16, Ink, TextAnchor.MiddleCenter, true);
            var recall = canvasGo.AddComponent<RecallBarWidget>();
            var rso=new SerializedObject(recall); rso.FindProperty("group").objectReferenceValue=rGroup; rso.FindProperty("fill").objectReferenceValue=rFill; rso.FindProperty("label").objectReferenceValue=rLabel; rso.ApplyModifiedPropertiesWithoutUndo();
            var beginRecall = ActionButton(root, "BEGIN RECALL (5s)", new Vector2(0.5f,0.5f), new Vector2(-160,180), new Color(0.30f,0.45f,0.62f), out _);
            var interrupt = ActionButton(root, "INTERRUPT", new Vector2(0.5f,0.5f), new Vector2(160,180), new Color(0.62f,0.35f,0.35f), out _);

            // Ability level-up bar.
            var abilityPanel = MakePanel(root, "Abilities", new Vector2(0.5f,0.5f), new Vector2(0,20), new Vector2(560,120));
            var levelUp = canvasGo.AddComponent<AbilityLevelUpBar>();
            var q = AbilitySlotUI(abilityPanel, "Q", -210); var w = AbilitySlotUI(abilityPanel, "W", -70); var e = AbilitySlotUI(abilityPanel, "E", 70); var r = AbilitySlotUI(abilityPanel, "R", 210);
            var pts = Label(abilityPanel, "Points", "1 pts", new Vector2(0.5f,1), new Vector2(0,-6), new Vector2(200,22), 16, Gold, TextAnchor.MiddleCenter, true);
            var lso=new SerializedObject(levelUp);
            BindSlot(lso, "qSlot", q); BindSlot(lso, "wSlot", w); BindSlot(lso, "eSlot", e); BindSlot(lso, "rSlot", r);
            lso.FindProperty("pointsText").objectReferenceValue=pts; lso.ApplyModifiedPropertiesWithoutUndo();
            var champUp = ActionButton(root, "CHAMPION LEVEL UP", new Vector2(0.5f,0.5f), new Vector2(0,-90), new Color(0.36f,0.55f,0.42f), out _);
            // wire buttons via a tiny relay
            var relay = canvasGo.AddComponent<HudDemoRelay>();
            var relayso=new SerializedObject(relay);
            relayso.FindProperty("recall").objectReferenceValue=recall;
            relayso.FindProperty("levelUp").objectReferenceValue=levelUp;
            relayso.FindProperty("beginRecallButton").objectReferenceValue=beginRecall;
            relayso.FindProperty("interruptButton").objectReferenceValue=interrupt;
            relayso.FindProperty("champLevelUpButton").objectReferenceValue=champUp;
            relayso.ApplyModifiedPropertiesWithoutUndo();

            // Status legend.
            var legendPanel = MakePanel(root, "Legend", new Vector2(0.5f,0), new Vector2(0,120), new Vector2(1200,320));
            Label(legendPanel, "LT", "STATUS EFFECTS", new Vector2(0,1), new Vector2(20,-12), new Vector2(300,26), 20, Gold, TextAnchor.MiddleLeft, true);
            var listGo = new GameObject("List", typeof(RectTransform)); listGo.transform.SetParent(legendPanel, false);
            var lrt=(RectTransform)listGo.transform; lrt.anchorMin=new Vector2(0,1); lrt.anchorMax=new Vector2(1,1); lrt.pivot=new Vector2(0.5f,1); lrt.anchoredPosition=new Vector2(0,-50); lrt.sizeDelta=new Vector2(-24,260);
            var glg=listGo.AddComponent<GridLayoutGroup>(); glg.cellSize=new Vector2(560,40); glg.spacing=new Vector2(16,6); glg.constraint=GridLayoutGroup.Constraint.FixedColumnCount; glg.constraintCount=2;
            var legendRow = BuildLegendRowPrefab();
            var legend = canvasGo.AddComponent<StatusLegendPanel>();
            var leso=new SerializedObject(legend); leso.FindProperty("listParent").objectReferenceValue=lrt; leso.FindProperty("rowPrefab").objectReferenceValue=legendRow; leso.ApplyModifiedPropertiesWithoutUndo();

            Save("Assets/Scenes/HudWidgetsDemo.unity");
            Done("HUD Widgets demo built. Recall bar, ability level-up bar (LEVEL UP to gain points), and the status legend.");
        }

        // ---------- MATCHMAKING ----------
        [MenuItem("Tools/Project D/Build Matchmaking Demo", priority = 97)]
        public static void BuildMatchmaking()
        {
            _font = Font(); var root = NewScene(out var canvasGo);
            Label(root, "Title", "PLAY", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(600,44), 32, Ink, TextAnchor.MiddleCenter, true);
            Watermark(root);

            var modes = new[] { ("Ranked", GameMode.RankedSolo), ("Normal", GameMode.Normal), ("Co-op vs AI", GameMode.CoOpVsAI), ("Custom", GameMode.Custom) };
            var modeBar = MakePanel(root, "Modes", new Vector2(0.5f,1), new Vector2(0,-120), new Vector2(1200,64)); modeBar.GetComponent<Image>().color=new Color(0,0,0,0);
            var mh=modeBar.gameObject.AddComponent<HorizontalLayoutGroup>(); mh.spacing=12; mh.childForceExpandWidth=true; mh.childForceExpandHeight=true;
            var modeButtons=new Button[modes.Length]; var modeVals=new GameMode[modes.Length];
            for(int i=0;i<modes.Length;i++){ modeButtons[i]=Chip(modeBar, modes[i].Item1); modeVals[i]=modes[i].Item2; }
            var selMode = Label(root, "SelMode", "Normal", new Vector2(0.5f,0.5f), new Vector2(0,180), new Vector2(600,32), 24, Gold, TextAnchor.MiddleCenter, true);

            var play = MakePanel(root, "Play", new Vector2(0.5f,0.5f), new Vector2(0,40), new Vector2(440,120)); play.GetComponent<Image>().color=new Color(0.30f,0.55f,0.40f);
            var playBtn = play.gameObject.AddComponent<Button>();
            var playLabel = Label(play, "PlayLabel", "FIND MATCH", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(440,50), 34, Ink, TextAnchor.MiddleCenter, true);
            var timer = Label(root, "Timer", "", new Vector2(0.5f,0.5f), new Vector2(0,-60), new Vector2(400,40), 34, Ink, TextAnchor.MiddleCenter, true);
            var estimate = Label(root, "Estimate", "", new Vector2(0.5f,0.5f), new Vector2(0,-110), new Vector2(900,26), 18, Sub, TextAnchor.MiddleCenter, false);

            // Ready-check.
            var ready = MakePanel(root, "Ready", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1920,1080)); ready.GetComponent<Image>().color=new Color(0,0,0,0.8f);
            Label(ready, "RT", "MATCH FOUND", new Vector2(0.5f,0.5f), new Vector2(0,120), new Vector2(800,60), 48, Gold, TextAnchor.MiddleCenter, true);
            var accept = ActionButton(ready, "ACCEPT", new Vector2(0.5f,0.5f), new Vector2(-160,-20), new Color(0.30f,0.55f,0.40f), out _);
            var decline = ActionButton(ready, "DECLINE", new Vector2(0.5f,0.5f), new Vector2(160,-20), new Color(0.62f,0.35f,0.35f), out _);
            ready.gameObject.SetActive(false);

            var view = canvasGo.AddComponent<MatchmakingView>();
            var so=new SerializedObject(view);
            var mb=so.FindProperty("modeButtons"); mb.arraySize=modeButtons.Length; for(int i=0;i<modeButtons.Length;i++) mb.GetArrayElementAtIndex(i).objectReferenceValue=modeButtons[i];
            var mv=so.FindProperty("modeValues"); mv.arraySize=modeVals.Length; for(int i=0;i<modeVals.Length;i++) mv.GetArrayElementAtIndex(i).enumValueIndex=(int)modeVals[i];
            so.FindProperty("selectedModeText").objectReferenceValue=selMode;
            so.FindProperty("playButton").objectReferenceValue=playBtn;
            so.FindProperty("playButtonLabel").objectReferenceValue=playLabel;
            so.FindProperty("timerText").objectReferenceValue=timer;
            so.FindProperty("estimateText").objectReferenceValue=estimate;
            so.FindProperty("readyPanel").objectReferenceValue=ready.gameObject;
            so.FindProperty("acceptButton").objectReferenceValue=accept;
            so.FindProperty("declineButton").objectReferenceValue=decline;
            so.FindProperty("demoFindAfterSeconds").floatValue=4f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Save("Assets/Scenes/MatchmakingDemo.unity");
            Done("Matchmaking demo built. Pick a mode, FIND MATCH, watch the timer + bracket, then the ready-check (auto-found in ~4s).");
        }

        // ---------- GRAPHS ----------
        [MenuItem("Tools/Project D/Build Match Graphs Demo", priority = 98)]
        public static void BuildGraphs()
        {
            _font = Font(); var root = NewScene(out var canvasGo);
            Label(root, "Title", "MATCH GRAPHS", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(700,40), 28, Ink, TextAnchor.MiddleCenter, true);
            Watermark(root);

            var graphPanel = MakePanel(root, "GraphPanel", new Vector2(0.5f,0.5f), new Vector2(0,60), new Vector2(1400,560));
            Label(graphPanel, "GT", "GOLD DIFFERENCE OVER TIME", new Vector2(0,1), new Vector2(20,-12), new Vector2(500,26), 18, Sub, TextAnchor.MiddleLeft, true);
            // Zero line.
            var zero = ChildImage(graphPanel.transform, "Zero", new Color(1,1,1,0.2f), raycast:false); var zr=(RectTransform)zero.transform; zr.anchorMin=new Vector2(0,0.5f); zr.anchorMax=new Vector2(1,0.5f); zr.pivot=new Vector2(0.5f,0.5f); zr.sizeDelta=new Vector2(-40,2); zr.anchoredPosition=new Vector2(0,-10);
            var area = new GameObject("Area", typeof(RectTransform)); area.transform.SetParent(graphPanel, false);
            var art=(RectTransform)area.transform; art.anchorMin=new Vector2(0.5f,0.5f); art.anchorMax=new Vector2(0.5f,0.5f); art.pivot=new Vector2(0.5f,0.5f); art.anchoredPosition=new Vector2(0,-10); art.sizeDelta=new Vector2(1320,420);
            var segPrefab = BuildSegmentPrefab();
            var peakB = Label(graphPanel, "PeakB", "", new Vector2(0,0), new Vector2(24,20), new Vector2(400,24), 16, new Color(0.4f,0.65f,1f), TextAnchor.MiddleLeft, true);
            var peakR = Label(graphPanel, "PeakR", "", new Vector2(0.5f,0), new Vector2(0,20), new Vector2(400,24), 16, new Color(1f,0.45f,0.45f), TextAnchor.MiddleCenter, true);
            var finalD = Label(graphPanel, "Final", "", new Vector2(1,0), new Vector2(-24,20), new Vector2(400,24), 18, Ink, TextAnchor.MiddleRight, true);

            var breakdownPanel = MakePanel(root, "Breakdown", new Vector2(0.5f,0), new Vector2(0,120), new Vector2(1400,120));
            Label(breakdownPanel, "BT", "GOLD SOURCES", new Vector2(0,1), new Vector2(20,-12), new Vector2(400,24), 18, Sub, TextAnchor.MiddleLeft, true);
            var bar = MakePanel(breakdownPanel, "Bar", new Vector2(0.5f,0.5f), new Vector2(0,-8), new Vector2(1340,40)); bar.GetComponent<Image>().color=new Color(1,1,1,0.06f);
            var barSeg = BuildSegmentPrefab();

            var view = canvasGo.AddComponent<MatchGraphView>();
            var so=new SerializedObject(view);
            so.FindProperty("graphArea").objectReferenceValue=art;
            so.FindProperty("segmentPrefab").objectReferenceValue=segPrefab;
            so.FindProperty("peakBlueText").objectReferenceValue=peakB;
            so.FindProperty("peakRedText").objectReferenceValue=peakR;
            so.FindProperty("finalDiffText").objectReferenceValue=finalD;
            so.FindProperty("breakdownBar").objectReferenceValue=bar;
            so.FindProperty("breakdownSegmentPrefab").objectReferenceValue=barSeg;
            so.ApplyModifiedPropertiesWithoutUndo();
            var driver=canvasGo.AddComponent<GraphDemoDriver>(); var dso=new SerializedObject(driver); dso.FindProperty("view").objectReferenceValue=view; dso.ApplyModifiedPropertiesWithoutUndo();

            Save("Assets/Scenes/MatchGraphsDemo.unity");
            Done("Match Graphs demo built. Gold-difference line (blue up / red down) + a gold-source breakdown bar.");
        }

        // ---------- TUTORIAL ----------
        [MenuItem("Tools/Project D/Build Tutorial Demo", priority = 99)]
        public static void BuildTutorial()
        {
            _font = Font(); var root = NewScene(out var canvasGo);
            Watermark(root);
            var panel = MakePanel(root, "Tutorial", new Vector2(0.5f,0.5f), new Vector2(0,120), new Vector2(1100,360));
            var idx = Label(panel, "Idx", "Step 1 / 6", new Vector2(0.5f,1), new Vector2(0,-16), new Vector2(400,24), 16, Sub, TextAnchor.MiddleCenter, false);
            var title = Label(panel, "Title", "Move", new Vector2(0.5f,1), new Vector2(0,-50), new Vector2(900,44), 34, Gold, TextAnchor.MiddleCenter, true);
            var body = Label(panel, "Body", "Instruction...", new Vector2(0.5f,0.5f), new Vector2(0,0), new Vector2(1000,60), 22, Ink, TextAnchor.MiddleCenter, false);
            var dotsGo=new GameObject("Dots", typeof(RectTransform)); dotsGo.transform.SetParent(panel,false); var drt=(RectTransform)dotsGo.transform; drt.anchorMin=new Vector2(0.5f,0); drt.anchorMax=new Vector2(0.5f,0); drt.pivot=new Vector2(0.5f,0); drt.anchoredPosition=new Vector2(0,80); drt.sizeDelta=new Vector2(400,20);
            var dh=dotsGo.AddComponent<HorizontalLayoutGroup>(); dh.spacing=10; dh.childAlignment=TextAnchor.MiddleCenter; dh.childForceExpandWidth=false;
            var dotPrefab = BuildDotPrefab();
            var skip = ActionButton(panel, "SKIP", new Vector2(1,0), new Vector2(-24,24), new Color(0.24f,0.28f,0.36f), out _);

            var complete = MakePanel(root, "Complete", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1920,1080)); complete.GetComponent<Image>().color=new Color(0,0,0,0.8f);
            Label(complete, "CT", "TUTORIAL COMPLETE", new Vector2(0.5f,0.5f), new Vector2(0,80), new Vector2(800,60), 44, Gold, TextAnchor.MiddleCenter, true);
            var reward = Label(complete, "Reward", "+350 XP", new Vector2(0.5f,0.5f), new Vector2(0,0), new Vector2(600,40), 30, new Color(0.6f,0.9f,0.6f), TextAnchor.MiddleCenter, true);
            complete.gameObject.SetActive(false);

            var view = canvasGo.AddComponent<TutorialView>();
            var so=new SerializedObject(view);
            so.FindProperty("stepTitle").objectReferenceValue=title;
            so.FindProperty("stepBody").objectReferenceValue=body;
            so.FindProperty("stepIndexText").objectReferenceValue=idx;
            so.FindProperty("dotsParent").objectReferenceValue=drt;
            so.FindProperty("dotPrefab").objectReferenceValue=dotPrefab;
            so.FindProperty("skipButton").objectReferenceValue=skip;
            so.FindProperty("completePanel").objectReferenceValue=complete.gameObject;
            so.FindProperty("completeRewardText").objectReferenceValue=reward;
            so.ApplyModifiedPropertiesWithoutUndo();
            var driver=canvasGo.AddComponent<TutorialDemoDriver>(); var dso=new SerializedObject(driver); dso.FindProperty("view").objectReferenceValue=view; dso.ApplyModifiedPropertiesWithoutUndo();

            // Trigger buttons to advance the tutorial.
            var triggers=new[]{"MOVE","ATTACK x3","LAST-HIT x3","CAST Q","RECALL","PING"};
            var trg=(ProjectD.Tutorial.TutorialTrigger[])System.Enum.GetValues(typeof(ProjectD.Tutorial.TutorialTrigger));
            var order=new[]{ProjectD.Tutorial.TutorialTrigger.Move,ProjectD.Tutorial.TutorialTrigger.BasicAttack,ProjectD.Tutorial.TutorialTrigger.LastHitMinion,ProjectD.Tutorial.TutorialTrigger.CastAbility,ProjectD.Tutorial.TutorialTrigger.Recall,ProjectD.Tutorial.TutorialTrigger.UsePing};
            var strip = MakePanel(root, "Triggers", new Vector2(0.5f,0), new Vector2(0,80), new Vector2(1500,60)); strip.GetComponent<Image>().color=new Color(0,0,0,0);
            var sh=strip.gameObject.AddComponent<HorizontalLayoutGroup>(); sh.spacing=8; sh.childForceExpandWidth=true; sh.childForceExpandHeight=true;
            for(int i=0;i<triggers.Length;i++){ var b=Chip(strip, triggers[i]); var t=order[i]; int rep = triggers[i].Contains("x3")?3:1; b.onClick.AddListener(()=>view.Report(t, rep)); }

            Save("Assets/Scenes/TutorialDemo.unity");
            Done("Tutorial demo built. Tap the trigger buttons in order to complete steps; finishing shows the reward.");
        }

        // ---- prefabs ----
        private static Button BuildSlotPrefab()
        { var go=new GameObject("Slot", typeof(RectTransform), typeof(Image), typeof(Button)); var rt=(RectTransform)go.transform; rt.sizeDelta=new Vector2(120,60); go.GetComponent<Image>().color=new Color(0.20f,0.28f,0.42f,0.95f); Label(rt,"L","Ping",new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(120,60),15,Ink,TextAnchor.MiddleCenter,true); go.SetActive(false); return go.GetComponent<Button>(); }
        private static Button BuildQuickChatPrefab()
        { var go=new GameObject("QC", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement)); go.GetComponent<Image>().color=Idle; go.GetComponent<LayoutElement>().preferredHeight=48; Label((RectTransform)go.transform,"L","phrase",new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(160,48),15,Ink,TextAnchor.MiddleCenter,true); go.SetActive(false); return go.GetComponent<Button>(); }
        private static Image BuildSegmentPrefab()
        { var go=new GameObject("Seg", typeof(RectTransform), typeof(Image)); go.GetComponent<Image>().color=new Color(0.4f,0.65f,1f); go.SetActive(false); return go.GetComponent<Image>(); }
        private static RectTransform BuildLegendRowPrefab()
        { var go=new GameObject("LegendRow", typeof(RectTransform)); var icon=ChildImage(go.transform,"Icon",new Color(0.6f,0.6f,0.7f),raycast:false); var ir=(RectTransform)icon.transform; ir.anchorMin=new Vector2(0,0.5f); ir.anchorMax=new Vector2(0,0.5f); ir.pivot=new Vector2(0,0.5f); ir.anchoredPosition=new Vector2(6,0); ir.sizeDelta=new Vector2(24,24); Label((RectTransform)go.transform,"Name","Effect",new Vector2(0,0.5f),new Vector2(40,0),new Vector2(140,24),16,Ink,TextAnchor.MiddleLeft,true); Label((RectTransform)go.transform,"Desc","Description",new Vector2(0,0.5f),new Vector2(180,0),new Vector2(360,24),13,Sub,TextAnchor.MiddleLeft,false); go.SetActive(false); return (RectTransform)go.transform; }
        private static Image BuildDotPrefab()
        { var go=new GameObject("Dot", typeof(RectTransform), typeof(Image), typeof(LayoutElement)); go.GetComponent<Image>().color=new Color(0.3f,0.32f,0.4f); go.GetComponent<LayoutElement>().preferredWidth=16; go.GetComponent<LayoutElement>().preferredHeight=16; go.SetActive(false); return go.GetComponent<Image>(); }
        private static AbilityLevelUpBar.SlotUI AbilitySlotUI(RectTransform parent, string letter, float x)
        {
            var cell=new GameObject("Slot_"+letter, typeof(RectTransform), typeof(Image)); cell.transform.SetParent(parent,false); var crt=(RectTransform)cell.transform; crt.anchorMin=new Vector2(0.5f,0.5f); crt.anchorMax=new Vector2(0.5f,0.5f); crt.pivot=new Vector2(0.5f,0.5f); crt.anchoredPosition=new Vector2(x,0); crt.sizeDelta=new Vector2(100,80); cell.GetComponent<Image>().color=new Color(0.20f,0.24f,0.34f);
            Label(crt,"Letter",letter,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(100,40),28,Ink,TextAnchor.MiddleCenter,true);
            var rank=Label(crt,"Rank","",new Vector2(0.5f,0),new Vector2(0,6),new Vector2(100,18),16,Gold,TextAnchor.MiddleCenter,true);
            var glow=ChildImage(cell.transform,"Glow",new Color(0.36f,0.82f,0.4f,0.4f),stretch:true,raycast:false); glow.gameObject.SetActive(false);
            var plus=new GameObject("Plus", typeof(RectTransform), typeof(Image), typeof(Button)); plus.transform.SetParent(cell.transform,false); var prt=(RectTransform)plus.transform; prt.anchorMin=new Vector2(1,1); prt.anchorMax=new Vector2(1,1); prt.pivot=new Vector2(1,1); prt.anchoredPosition=new Vector2(-2,-2); prt.sizeDelta=new Vector2(30,30); plus.GetComponent<Image>().color=new Color(0.36f,0.62f,0.42f);
            Label(prt,"P","+",new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(30,30),22,Ink,TextAnchor.MiddleCenter,true);
            return new AbilityLevelUpBar.SlotUI { plusButton=plus.GetComponent<Button>(), rankText=rank, plusGlow=glow.gameObject };
        }
        private static void BindSlot(SerializedObject so, string prop, AbilityLevelUpBar.SlotUI ui)
        {
            var p=so.FindProperty(prop);
            p.FindPropertyRelative("plusButton").objectReferenceValue=ui.plusButton;
            p.FindPropertyRelative("rankText").objectReferenceValue=ui.rankText;
            p.FindPropertyRelative("plusGlow").objectReferenceValue=ui.plusGlow;
        }

        // ---- shared ----
        private static void Watermark(RectTransform root)=>Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);
        private static void Done(string msg)=>EditorUtility.DisplayDialog("Project D", msg, "OK");
        private static RectTransform NewScene(out GameObject canvasGo)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo=new GameObject("Main Camera"); camGo.tag="MainCamera"; var cam=camGo.AddComponent<Camera>(); cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=Bg; camGo.AddComponent<AudioListener>();
            if(Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>()==null){ var es=new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }
            canvasGo=new GameObject("Canvas"); var canvas=canvasGo.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var sc=canvasGo.AddComponent<CanvasScaler>(); sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution=new Vector2(1920,1080); sc.matchWidthOrHeight=0.5f; canvasGo.AddComponent<GraphicRaycaster>();
            return (RectTransform)canvasGo.transform;
        }
        private static void Save(string path){ if(!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes"); EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path); AssetDatabase.Refresh(); }
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Panel; return rt; }
        private static Image ChildImage(Transform parent, string name, Color c, bool stretch=false, bool raycast=true){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var img=go.GetComponent<Image>(); img.color=c; img.raycastTarget=raycast; if(stretch){var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;} return img; }
        private static Button Chip(RectTransform parent, string text){ var go=new GameObject("Chip_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); go.GetComponent<Image>().color=Idle; Label((RectTransform)go.transform,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(180,44),18,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c, out Text label){ var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(260,52); go.GetComponent<Image>().color=c; label=Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(260,52),18,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
        private static void Stretch(RectTransform rt){ rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; }
        private static Font Font()=>Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
#endif
