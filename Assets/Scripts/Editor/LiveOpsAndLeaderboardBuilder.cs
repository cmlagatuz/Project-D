// Project D  |  Codename: Zantetsuken
// LiveOpsAndLeaderboardBuilder.cs  (Editor-only)
//   Tools > Project D > Build Live-Ops Demo
//   Tools > Project D > Build Champion Leaderboard Demo
//
// Builds two runnable P1 screens: the Live-ops screen (mission tabs + cards + weekly
// chest widget) and the per-champion "Server No.1" leaderboard (grid + focus toggle).

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.LiveOpsUI;
using ProjectD.LeaderboardXUI;
using ProjectD.P1UI2;

namespace ProjectD.EditorTools
{
    public static class LiveOpsAndLeaderboardBuilder
    {
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static readonly Color Idle = new Color(0.16f,0.18f,0.24f);
        private static Font _font;

        // ===================== LIVE-OPS =====================
        [MenuItem("Tools/Project D/Build Live-Ops Demo", priority = 93)]
        public static void BuildLiveOps()
        {
            _font = Font();
            var root = NewScene(out var canvasGo);
            Label(root, "Title", "MISSIONS & REWARDS", new Vector2(0,1), new Vector2(40,-34), new Vector2(700,44), 32, Ink, TextAnchor.MiddleLeft, true);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Cadence tabs.
            var tabBar = MakePanel(root, "Tabs", new Vector2(0,1), new Vector2(430,-96), new Vector2(560,52)); tabBar.pivot=new Vector2(0,0.5f); tabBar.anchorMin=new Vector2(0,1); tabBar.anchorMax=new Vector2(0,1); tabBar.GetComponent<Image>().color=new Color(0,0,0,0);
            var hl=tabBar.gameObject.AddComponent<HorizontalLayoutGroup>(); hl.spacing=8; hl.childForceExpandWidth=true; hl.childForceExpandHeight=true;
            var daily=Chip(tabBar,"DAILY"); var weekly=Chip(tabBar,"WEEKLY"); var evt=Chip(tabBar,"EVENT");

            // Mission grid (left, scroll).
            var scroll = MakePanel(root, "Grid", new Vector2(0,0.5f), new Vector2(430,-30), new Vector2(1160,760)); scroll.pivot=new Vector2(0,0.5f); scroll.anchorMin=new Vector2(0,0.5f); scroll.anchorMax=new Vector2(0,0.5f);
            var vp = MakePanel(scroll, "Viewport", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1140,740)); vp.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var contentGo=new GameObject("Content", typeof(RectTransform)); contentGo.transform.SetParent(vp,false);
            var content=(RectTransform)contentGo.transform; content.anchorMin=new Vector2(0,1); content.anchorMax=new Vector2(1,1); content.pivot=new Vector2(0.5f,1); content.sizeDelta=new Vector2(0,740);
            var vlg=contentGo.AddComponent<VerticalLayoutGroup>(); vlg.spacing=10; vlg.padding=new RectOffset(14,14,14,14); vlg.childForceExpandHeight=false; vlg.childControlHeight=false;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var sr=scroll.gameObject.AddComponent<ScrollRect>(); sr.viewport=vp; sr.content=content; sr.horizontal=false;
            var cardPrefab = BuildMissionCardPrefab();

            // Weekly chest widget (right).
            var chestPanel = MakePanel(root, "Chest", new Vector2(1,0.5f), new Vector2(-330,-30), new Vector2(560,540));
            Label(chestPanel, "ChestTitle", "WEEKLY CHEST", new Vector2(0.5f,1), new Vector2(0,-20), new Vector2(500,32), 24, Gold, TextAnchor.MiddleCenter, true);
            var chestIcon = ChildImage(chestPanel.transform, "ChestIcon", new Color(0.6f,0.5f,0.3f), raycast:false); var cic=(RectTransform)chestIcon.transform; cic.anchorMin=new Vector2(0.5f,1); cic.anchorMax=new Vector2(0.5f,1); cic.pivot=new Vector2(0.5f,1); cic.anchoredPosition=new Vector2(0,-70); cic.sizeDelta=new Vector2(160,140);
            var fragBack = MakePanel(chestPanel, "FragBack", new Vector2(0.5f,1), new Vector2(0,-240), new Vector2(440,20)); fragBack.GetComponent<Image>().color=new Color(1,1,1,0.12f);
            var fragFill = ChildImage(fragBack.transform, "Fill", Gold, raycast:false); var ff=(RectTransform)fragFill.transform; ff.anchorMin=Vector2.zero; ff.anchorMax=Vector2.one; ff.offsetMin=Vector2.zero; ff.offsetMax=Vector2.zero; fragFill.type=Image.Type.Filled; fragFill.fillMethod=Image.FillMethod.Horizontal; fragFill.fillOrigin=(int)Image.OriginHorizontal.Left; fragFill.fillAmount=0.5f;
            var fragText = Label(chestPanel, "Frag", "2 / 4 fragments", new Vector2(0.5f,1), new Vector2(0,-268), new Vector2(500,24), 16, Sub, TextAnchor.MiddleCenter, false);
            var chestCount = Label(chestPanel, "ChestCount", "Chests: 1", new Vector2(0,0), new Vector2(30,120), new Vector2(240,26), 18, Ink, TextAnchor.MiddleLeft, true);
            var keyCount = Label(chestPanel, "KeyCount", "Keys: 2", new Vector2(1,0), new Vector2(-30,120), new Vector2(240,26), 18, Ink, TextAnchor.MiddleRight, true);
            var openBtn = ActionButton(chestPanel, "OPEN CHEST", new Vector2(0.5f,0), new Vector2(0,60), new Color(0.30f,0.55f,0.40f), out var openLabel);
            var chestFeedback = Label(chestPanel, "ChestFb", "", new Vector2(0.5f,0), new Vector2(0,24), new Vector2(500,22), 15, Sub, TextAnchor.MiddleCenter, false);
            var feedback = Label(root, "Feedback", "", new Vector2(0,0), new Vector2(430,40), new Vector2(700,24), 16, Sub, TextAnchor.MiddleLeft, false);

            var screen = canvasGo.AddComponent<LiveOpsScreen>();
            var so = new SerializedObject(screen);
            var cb=so.FindProperty("cadenceButtons"); cb.arraySize=3; cb.GetArrayElementAtIndex(0).objectReferenceValue=daily; cb.GetArrayElementAtIndex(1).objectReferenceValue=weekly; cb.GetArrayElementAtIndex(2).objectReferenceValue=evt;
            so.FindProperty("gridParent").objectReferenceValue=content;
            so.FindProperty("cardPrefab").objectReferenceValue=cardPrefab;
            so.FindProperty("chestFragmentFill").objectReferenceValue=fragFill;
            so.FindProperty("chestFragmentText").objectReferenceValue=fragText;
            so.FindProperty("chestCountText").objectReferenceValue=chestCount;
            so.FindProperty("keyCountText").objectReferenceValue=keyCount;
            so.FindProperty("openChestButton").objectReferenceValue=openBtn;
            so.FindProperty("openChestLabel").objectReferenceValue=openLabel;
            so.FindProperty("chestFeedback").objectReferenceValue=chestFeedback;
            so.FindProperty("feedbackLabel").objectReferenceValue=feedback;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<LiveOpsDemoDriver>();
            var dso=new SerializedObject(driver); dso.FindProperty("screen").objectReferenceValue=screen; dso.ApplyModifiedPropertiesWithoutUndo();

            Save("Assets/Scenes/LiveOpsDemo.unity");
            EditorUtility.DisplayDialog("Project D", "Live-Ops demo built.\nPress Play: cadence tabs (Daily/Weekly/Event), claimable missions, and the weekly chest widget.", "OK");
        }

        // ===================== CHAMPION LEADERBOARD =====================
        [MenuItem("Tools/Project D/Build Champion Leaderboard Demo", priority = 94)]
        public static void BuildChampionLeaderboard()
        {
            _font = Font();
            var root = NewScene(out var canvasGo);
            var title = Label(root, "Title", "SERVER No.1 - Best per Champion", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(1200,44), 30, Ink, TextAnchor.MiddleCenter, true);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);
            var region = DropdownRow(root, new Vector2(0,1), new Vector2(40,-100), 240);
            var back = ActionButton(root, "< BACK TO GRID", new Vector2(0,1), new Vector2(200,-120), new Color(0.30f,0.45f,0.62f), out _); ((RectTransform)back.transform).pivot=new Vector2(0,0.5f); back.gameObject.SetActive(false);
            var count = Label(root, "Count", "", new Vector2(1,1), new Vector2(-40,-104), new Vector2(300,28), 20, Sub, TextAnchor.MiddleRight, false);

            var scroll = MakePanel(root, "List", new Vector2(0.5f,0.5f), new Vector2(0,-40), new Vector2(1400,780));
            var vp = MakePanel(scroll, "Viewport", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1380,760)); vp.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var contentGo=new GameObject("Content", typeof(RectTransform)); contentGo.transform.SetParent(vp,false);
            var content=(RectTransform)contentGo.transform; content.anchorMin=new Vector2(0,1); content.anchorMax=new Vector2(1,1); content.pivot=new Vector2(0.5f,1); content.sizeDelta=new Vector2(0,760);
            var vlg=contentGo.AddComponent<VerticalLayoutGroup>(); vlg.spacing=6; vlg.padding=new RectOffset(12,12,10,10); vlg.childForceExpandHeight=false; vlg.childControlHeight=false;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var sr=scroll.gameObject.AddComponent<ScrollRect>(); sr.viewport=vp; sr.content=content; sr.horizontal=false;
            var rowPrefab = BuildChampRowPrefab();

            var screen = canvasGo.AddComponent<ChampionLeaderboardScreen>();
            var so=new SerializedObject(screen);
            so.FindProperty("titleText").objectReferenceValue=title;
            so.FindProperty("regionDropdown").objectReferenceValue=region;
            so.FindProperty("backToGridButton").objectReferenceValue=back;
            so.FindProperty("rowParent").objectReferenceValue=content;
            so.FindProperty("rowPrefab").objectReferenceValue=rowPrefab;
            so.FindProperty("resultCountText").objectReferenceValue=count;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<ChampionLeaderboardDemoDriver>();
            var dso=new SerializedObject(driver); dso.FindProperty("screen").objectReferenceValue=screen; dso.ApplyModifiedPropertiesWithoutUndo();

            Save("Assets/Scenes/ChampionLeaderboardDemo.unity");
            EditorUtility.DisplayDialog("Project D", "Champion Leaderboard demo built.\nPress Play: Server No.1 grid (best per champion). Click a champion for its top ladder; BACK returns.", "OK");
        }

        // ---- prefabs ----
        private static MissionCard BuildMissionCardPrefab()
        {
            var go=new GameObject("MissionCard", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(MissionCard));
            go.GetComponent<Image>().color=new Color(0.13f,0.15f,0.21f,0.95f); go.GetComponent<LayoutElement>().preferredHeight=110;
            var tag=ChildImage(go.transform,"Tag",new Color(0.35f,0.62f,0.92f),raycast:false); var tr=(RectTransform)tag.transform; tr.anchorMin=new Vector2(0,0); tr.anchorMax=new Vector2(0,1); tr.pivot=new Vector2(0,0.5f); tr.sizeDelta=new Vector2(6,0); tr.anchoredPosition=Vector2.zero;
            var tagLabel=Label((RectTransform)go.transform,"Cadence","DAILY",new Vector2(0,1),new Vector2(20,-12),new Vector2(140,20),13,Sub,TextAnchor.MiddleLeft,true);
            var title=Label((RectTransform)go.transform,"Title","Mission",new Vector2(0,1),new Vector2(20,-30),new Vector2(700,28),20,Ink,TextAnchor.MiddleLeft,true);
            var desc=Label((RectTransform)go.transform,"Desc","Do the thing.",new Vector2(0,1),new Vector2(20,-58),new Vector2(700,22),15,Sub,TextAnchor.MiddleLeft,false);
            var pBack=MakePanel((RectTransform)go.transform,"PBack",new Vector2(0,0),new Vector2(20,24),new Vector2(560,14)); pBack.pivot=new Vector2(0,0.5f); pBack.anchorMin=new Vector2(0,0); pBack.anchorMax=new Vector2(0,0); pBack.GetComponent<Image>().color=new Color(1,1,1,0.12f);
            var pFill=ChildImage(pBack.transform,"Fill",new Color(0.36f,0.72f,0.92f),raycast:false); var pf=(RectTransform)pFill.transform; pf.anchorMin=Vector2.zero; pf.anchorMax=Vector2.one; pf.offsetMin=Vector2.zero; pf.offsetMax=Vector2.zero; pFill.type=Image.Type.Filled; pFill.fillMethod=Image.FillMethod.Horizontal; pFill.fillOrigin=(int)Image.OriginHorizontal.Left; pFill.fillAmount=0.5f;
            var pText=Label((RectTransform)go.transform,"PText","2 / 3",new Vector2(0,0),new Vector2(590,24),new Vector2(120,20),14,Sub,TextAnchor.MiddleLeft,false);
            var reward=Label((RectTransform)go.transform,"Reward","+500 XP",new Vector2(1,1),new Vector2(-180,-30),new Vector2(300,24),16,Gold,TextAnchor.MiddleRight,true);
            var claim=new GameObject("Claim", typeof(RectTransform), typeof(Image), typeof(Button)); claim.transform.SetParent(go.transform,false);
            var crt=(RectTransform)claim.transform; crt.anchorMin=new Vector2(1,0); crt.anchorMax=new Vector2(1,0); crt.pivot=new Vector2(1,0); crt.anchoredPosition=new Vector2(-16,16); crt.sizeDelta=new Vector2(150,44); claim.GetComponent<Image>().color=new Color(0.30f,0.55f,0.40f);
            var claimLabel=Label(crt,"L","CLAIM",new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(150,44),16,Ink,TextAnchor.MiddleCenter,true);
            var comp=go.GetComponent<MissionCard>();
            var so=new SerializedObject(comp);
            so.FindProperty("background").objectReferenceValue=go.GetComponent<Image>();
            so.FindProperty("cadenceTag").objectReferenceValue=tag;
            so.FindProperty("cadenceLabel").objectReferenceValue=tagLabel;
            so.FindProperty("titleText").objectReferenceValue=title;
            so.FindProperty("descText").objectReferenceValue=desc;
            so.FindProperty("progressFill").objectReferenceValue=pFill;
            so.FindProperty("progressText").objectReferenceValue=pText;
            so.FindProperty("rewardText").objectReferenceValue=reward;
            so.FindProperty("claimButton").objectReferenceValue=claim.GetComponent<Button>();
            so.FindProperty("claimLabel").objectReferenceValue=claimLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(false);
            return comp;
        }

        private static ChampionLeaderRow BuildChampRowPrefab()
        {
            var go=new GameObject("ChampRow", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(ChampionLeaderRow));
            go.GetComponent<Image>().color=new Color(0.13f,0.15f,0.21f,0.9f); go.GetComponent<LayoutElement>().preferredHeight=60;
            var swatch=ChildImage(go.transform,"Champ",new Color(0.4f,0.4f,0.55f),raycast:false); var sr=(RectTransform)swatch.transform; sr.anchorMin=new Vector2(0,0.5f); sr.anchorMax=new Vector2(0,0.5f); sr.pivot=new Vector2(0,0.5f); sr.anchoredPosition=new Vector2(16,0); sr.sizeDelta=new Vector2(46,46);
            var crown=ChildImage(go.transform,"Crown",Gold,raycast:false); var cr=(RectTransform)crown.transform; cr.anchorMin=new Vector2(0,1); cr.anchorMax=new Vector2(0,1); cr.pivot=new Vector2(0,1); cr.anchoredPosition=new Vector2(12,-2); cr.sizeDelta=new Vector2(20,20); crown.gameObject.SetActive(false);
            var champText=Label((RectTransform)go.transform,"ChampName","kain",new Vector2(0,0.5f),new Vector2(76,0),new Vector2(200,26),18,Ink,TextAnchor.MiddleLeft,true);
            var rankNum=Label((RectTransform)go.transform,"RankNum","#1",new Vector2(0,0.5f),new Vector2(76,0),new Vector2(90,26),20,Gold,TextAnchor.MiddleLeft,true); rankNum.gameObject.SetActive(false);
            var player=Label((RectTransform)go.transform,"Player","Player",new Vector2(0,0.5f),new Vector2(300,10),new Vector2(400,24),18,Ink,TextAnchor.MiddleLeft,true);
            var sub=Label((RectTransform)go.transform,"Sub","Crystal A · 58% WR",new Vector2(0,0.5f),new Vector2(300,-14),new Vector2(500,20),14,Sub,TextAnchor.MiddleLeft,false);
            var rating=Label((RectTransform)go.transform,"Rating","1,920",new Vector2(1,0.5f),new Vector2(-24,0),new Vector2(200,30),24,Gold,TextAnchor.MiddleRight,true);
            var comp=go.GetComponent<ChampionLeaderRow>();
            var so=new SerializedObject(comp);
            so.FindProperty("background").objectReferenceValue=go.GetComponent<Image>();
            so.FindProperty("championSwatch").objectReferenceValue=swatch;
            so.FindProperty("championText").objectReferenceValue=champText;
            so.FindProperty("rankNumberText").objectReferenceValue=rankNum;
            so.FindProperty("playerText").objectReferenceValue=player;
            so.FindProperty("ratingText").objectReferenceValue=rating;
            so.FindProperty("subText").objectReferenceValue=sub;
            so.FindProperty("crown").objectReferenceValue=crown.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(false);
            return comp;
        }

        // ---- helpers ----
        private static Dropdown DropdownRow(RectTransform parent, Vector2 anchor, Vector2 pos, float width)
        {
            var go=new GameObject("RegionDD", typeof(RectTransform), typeof(Image), typeof(Dropdown)); go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(width,44); go.GetComponent<Image>().color=Idle;
            var caption=Label(rt,"Label","All Regions",new Vector2(0,0.5f),new Vector2(12,0),new Vector2(width-30,30),16,Ink,TextAnchor.MiddleLeft,false);
            var template=new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(CanvasGroup)); template.transform.SetParent(go.transform,false);
            var trt=(RectTransform)template.transform; trt.anchorMin=new Vector2(0,0); trt.anchorMax=new Vector2(1,0); trt.pivot=new Vector2(0.5f,1); trt.sizeDelta=new Vector2(0,220); template.GetComponent<Image>().color=Panel;
            var vpg=new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask)); vpg.transform.SetParent(template.transform,false); var vrt=(RectTransform)vpg.transform; vrt.anchorMin=Vector2.zero; vrt.anchorMax=Vector2.one; vrt.offsetMin=Vector2.zero; vrt.offsetMax=Vector2.zero; vpg.GetComponent<Mask>().showMaskGraphic=false;
            var cnt=new GameObject("Content", typeof(RectTransform)); cnt.transform.SetParent(vpg.transform,false); var crt=(RectTransform)cnt.transform; crt.anchorMin=new Vector2(0,1); crt.anchorMax=new Vector2(1,1); crt.pivot=new Vector2(0.5f,1); crt.sizeDelta=new Vector2(0,40);
            var item=new GameObject("Item", typeof(RectTransform), typeof(Toggle)); item.transform.SetParent(cnt.transform,false); var irt=(RectTransform)item.transform; irt.anchorMin=new Vector2(0,0.5f); irt.anchorMax=new Vector2(1,0.5f); irt.sizeDelta=new Vector2(0,36);
            var itemBg=new GameObject("Item Background", typeof(RectTransform), typeof(Image)); itemBg.transform.SetParent(item.transform,false); var ibr=(RectTransform)itemBg.transform; ibr.anchorMin=Vector2.zero; ibr.anchorMax=Vector2.one; ibr.offsetMin=Vector2.zero; ibr.offsetMax=Vector2.zero; itemBg.GetComponent<Image>().color=Idle;
            var itemLabel=Label((RectTransform)item.transform,"Item Label","Option",new Vector2(0,0.5f),new Vector2(12,0),new Vector2(width-30,30),16,Ink,TextAnchor.MiddleLeft,false);
            item.GetComponent<Toggle>().targetGraphic=itemBg.GetComponent<Image>();
            var scroll=template.GetComponent<ScrollRect>(); scroll.content=crt; scroll.viewport=vrt; scroll.horizontal=false; template.SetActive(false);
            var dd=go.GetComponent<Dropdown>(); dd.template=trt; dd.captionText=caption; dd.itemText=itemLabel; dd.targetGraphic=go.GetComponent<Image>();
            return dd;
        }
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
        private static Button Chip(RectTransform parent, string text){ var go=new GameObject("Chip_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); go.GetComponent<Image>().color=Idle; Label((RectTransform)go.transform,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(180,40),18,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c, out Text label){ var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(260,52); go.GetComponent<Image>().color=c; label=Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(260,52),18,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
        private static Font Font()=>Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
#endif
