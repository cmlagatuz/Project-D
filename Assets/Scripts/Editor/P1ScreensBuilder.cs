// Project D  |  Codename: Zantetsuken
// P1ScreensBuilder.cs  (Editor-only)
//   Tools > Project D > Build Main Menu Shell Demo
//   Tools > Project D > Build Social Profile Demo
//
// Builds two runnable P1 screens: the home hub (nav rail + notification badges +
// player header + PLAY) and the Social/Profile screen (showcase card + friends list +
// requests + add-friend). No art needed.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.MainMenuUI;
using ProjectD.SocialUI;
using ProjectD.P1UI;

namespace ProjectD.EditorTools
{
    public static class P1ScreensBuilder
    {
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static readonly Color Red = new Color(0.85f,0.30f,0.32f);
        private static Font _font;

        // ===================== MAIN MENU SHELL =====================
        [MenuItem("Tools/Project D/Build Main Menu Shell Demo", priority = 91)]
        public static void BuildMainMenu()
        {
            _font = Font();
            var root = NewScene(out var canvasGo);

            // Player header (top).
            var header = MakePanel(root, "Header", new Vector2(0.5f,1), new Vector2(0,-58), new Vector2(1760, 96));
            var icon = ChildImage(header.transform, "Icon", new Color(0.36f,0.62f,0.92f), raycast:false); var ic=(RectTransform)icon.transform; ic.anchorMin=new Vector2(0,0.5f); ic.anchorMax=new Vector2(0,0.5f); ic.pivot=new Vector2(0,0.5f); ic.anchoredPosition=new Vector2(20,0); ic.sizeDelta=new Vector2(70,70);
            var border = ChildImage(header.transform, "Border", Gold, raycast:false); var bd=(RectTransform)border.transform; bd.anchorMin=new Vector2(0,0.5f); bd.anchorMax=new Vector2(0,0.5f); bd.pivot=new Vector2(0,0.5f); bd.anchoredPosition=new Vector2(16,0); bd.sizeDelta=new Vector2(78,78); border.transform.SetAsFirstSibling();
            var pname = Label(header, "Name", "You", new Vector2(0,0.5f), new Vector2(108,16), new Vector2(400,32), 26, Ink, TextAnchor.MiddleLeft, true);
            var plevel = Label(header, "Level", "Lv 42", new Vector2(0,0.5f), new Vector2(108,-18), new Vector2(200,24), 18, Sub, TextAnchor.MiddleLeft, false);
            var prank = Label(header, "Rank", "Crystal A", new Vector2(0,0.5f), new Vector2(220,-18), new Vector2(300,24), 18, Gold, TextAnchor.MiddleLeft, true);
            var kp = Label(header, "KP", "12,500 KP", new Vector2(1,0.5f), new Vector2(-30,0), new Vector2(300,36), 26, Gold, TextAnchor.MiddleRight, true);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Nav rail (left).
            var rail = MakePanel(root, "NavRail", new Vector2(0,0.5f), new Vector2(150, -30), new Vector2(280, 780));
            var railV = rail.gameObject.AddComponent<VerticalLayoutGroup>(); railV.spacing=8; railV.padding=new RectOffset(16,16,16,16); railV.childForceExpandHeight=false; railV.childControlHeight=false;
            var navPrefab = BuildNavPrefab();

            // Content host (center) + title.
            var content = MakePanel(root, "Content", new Vector2(0.5f,0.5f), new Vector2(150, -30), new Vector2(1180, 720));
            var contentTitle = Label(content, "Title", "Play", new Vector2(0,1), new Vector2(30,-24), new Vector2(600,40), 30, Ink, TextAnchor.UpperLeft, true);
            Label(content, "Hint", "Content panels (Store / Champions / Profile / Events / ...) mount here.", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(900,40), 20, Sub, TextAnchor.MiddleCenter, false);

            // Big PLAY (bottom-right).
            var play = MakePanel(root, "Play", new Vector2(1,0), new Vector2(-260, 90), new Vector2(440, 120));
            play.GetComponent<Image>().color = new Color(0.30f,0.55f,0.40f);
            var playBtn = play.gameObject.AddComponent<Button>();
            var playMode = Label(play, "Mode", "RANKED", new Vector2(0.5f,1), new Vector2(0,-16), new Vector2(400,26), 18, new Color(0.8f,0.95f,0.85f), TextAnchor.MiddleCenter, true);
            Label(play, "PlayLabel", "PLAY", new Vector2(0.5f,0), new Vector2(0,24), new Vector2(400,50), 44, Ink, TextAnchor.MiddleCenter, true);

            var shell = canvasGo.AddComponent<MainMenuShell>();
            var so = new SerializedObject(shell);
            so.FindProperty("navParent").objectReferenceValue = rail;
            so.FindProperty("navPrefab").objectReferenceValue = navPrefab;
            so.FindProperty("playerNameText").objectReferenceValue = pname;
            so.FindProperty("playerLevelText").objectReferenceValue = plevel;
            so.FindProperty("kpText").objectReferenceValue = kp;
            so.FindProperty("borderImage").objectReferenceValue = border;
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("rankText").objectReferenceValue = prank;
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("playModeLabel").objectReferenceValue = playMode;
            so.FindProperty("contentTitle").objectReferenceValue = contentTitle;
            so.FindProperty("contentHost").objectReferenceValue = content;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<MainMenuDemoDriver>();
            var dso = new SerializedObject(driver); dso.FindProperty("shell").objectReferenceValue = shell; dso.ApplyModifiedPropertiesWithoutUndo();

            Save("Assets/Scenes/MainMenuShellDemo.unity");
            EditorUtility.DisplayDialog("Project D", "Main Menu Shell demo built.\nPress Play: nav rail with notification badges (Missions 3, Social 2, Store 5, Profile 1, Events *), player header, PLAY.", "OK");
        }

        // ===================== SOCIAL / PROFILE =====================
        [MenuItem("Tools/Project D/Build Social Profile Demo", priority = 92)]
        public static void BuildSocial()
        {
            _font = Font();
            var root = NewScene(out var canvasGo);
            Label(root, "Title", "SOCIAL", new Vector2(0,1), new Vector2(40,-34), new Vector2(400,44), 32, Ink, TextAnchor.MiddleLeft, true);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Profile showcase (left).
            var card = MakePanel(root, "Showcase", new Vector2(0,0.5f), new Vector2(400, -20), new Vector2(720, 780));
            var border = ChildImage(card.transform, "Border", Gold, raycast:false); var bd=(RectTransform)border.transform; bd.anchorMin=new Vector2(0.5f,1); bd.anchorMax=new Vector2(0.5f,1); bd.pivot=new Vector2(0.5f,1); bd.anchoredPosition=new Vector2(0,-30); bd.sizeDelta=new Vector2(200,200);
            var icon = ChildImage(card.transform, "Icon", new Color(0.36f,0.62f,0.92f), raycast:false); var ic=(RectTransform)icon.transform; ic.anchorMin=new Vector2(0.5f,1); ic.anchorMax=new Vector2(0.5f,1); ic.pivot=new Vector2(0.5f,1); ic.anchoredPosition=new Vector2(0,-42); ic.sizeDelta=new Vector2(176,176);
            var name = Label(card, "Name", "You", new Vector2(0.5f,1), new Vector2(0,-250), new Vector2(680,40), 34, Ink, TextAnchor.MiddleCenter, true);
            var title = Label(card, "Title", "Dragoon", new Vector2(0.5f,1), new Vector2(0,-296), new Vector2(680,28), 20, Gold, TextAnchor.MiddleCenter, true);
            var level = Label(card, "Level", "Lv 42", new Vector2(0.5f,1), new Vector2(0,-330), new Vector2(680,26), 18, Sub, TextAnchor.MiddleCenter, false);
            var rank = Label(card, "Rank", "Crystal A", new Vector2(0.5f,1), new Vector2(0,-380), new Vector2(680,34), 24, new Color(0.55f,0.85f,1f), TextAnchor.MiddleCenter, true);
            var wr = Label(card, "WR", "57% WR (137-105)", new Vector2(0.5f,1), new Vector2(0,-420), new Vector2(680,26), 18, Sub, TextAnchor.MiddleCenter, false);
            var fav = Label(card, "Fav", "kain", new Vector2(0.5f,1), new Vector2(0,-460), new Vector2(680,26), 18, Sub, TextAnchor.MiddleCenter, false);
            var code = Label(card, "Code", "ID: you", new Vector2(0.5f,0), new Vector2(0,30), new Vector2(680,24), 16, new Color(0.5f,0.55f,0.62f), TextAnchor.MiddleCenter, false);

            // Friends panel (right).
            var friendsPanel = MakePanel(root, "Friends", new Vector2(1,0.5f), new Vector2(-560, -20), new Vector2(900, 500));
            var friendCount = Label(friendsPanel, "FCount", "Friends (5)", new Vector2(0,1), new Vector2(20,-14), new Vector2(400,28), 22, Ink, TextAnchor.MiddleLeft, true);
            var friendsList = ScrollList(friendsPanel, new Vector2(0,-58), new Vector2(-24,410));

            // Requests panel (right-bottom).
            var reqPanel = MakePanel(root, "Requests", new Vector2(1,0), new Vector2(-560, 200), new Vector2(900, 200));
            var reqCount = Label(reqPanel, "RCount", "Requests (2)", new Vector2(0,1), new Vector2(20,-14), new Vector2(400,28), 22, Ink, TextAnchor.MiddleLeft, true);
            var reqList = ScrollList(reqPanel, new Vector2(0,-58), new Vector2(-24,120));

            // Add friend (bottom-right).
            var addPanel = MakePanel(root, "AddFriend", new Vector2(1,0), new Vector2(-560, 70), new Vector2(900, 90));
            var input = BuildInput(addPanel, new Vector2(0,0.5f), new Vector2(20,0), new Vector2(560,50));
            var addBtn = ActionButton(addPanel, "ADD FRIEND", new Vector2(1,0.5f), new Vector2(-24,0), new Color(0.30f,0.45f,0.62f), out _);
            var feedback = Label(root, "Feedback", "", new Vector2(1,0), new Vector2(-560, 20), new Vector2(900,24), 16, Sub, TextAnchor.MiddleCenter, false);

            var friendRowPrefab = BuildFriendRowPrefab(acceptStyle:false);
            var requestRowPrefab = BuildFriendRowPrefab(acceptStyle:true);

            var view = canvasGo.AddComponent<SocialProfileView>();
            var so = new SerializedObject(view);
            so.FindProperty("borderImage").objectReferenceValue = border;
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("nameText").objectReferenceValue = name;
            so.FindProperty("levelText").objectReferenceValue = level;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("rankText").objectReferenceValue = rank;
            so.FindProperty("winRateText").objectReferenceValue = wr;
            so.FindProperty("favoriteChampionText").objectReferenceValue = fav;
            so.FindProperty("friendCodeText").objectReferenceValue = code;
            so.FindProperty("friendRowParent").objectReferenceValue = friendsList;
            so.FindProperty("friendRowPrefab").objectReferenceValue = friendRowPrefab;
            so.FindProperty("friendCountText").objectReferenceValue = friendCount;
            so.FindProperty("requestRowParent").objectReferenceValue = reqList;
            so.FindProperty("requestRowPrefab").objectReferenceValue = requestRowPrefab;
            so.FindProperty("requestCountText").objectReferenceValue = reqCount;
            so.FindProperty("addFriendInput").objectReferenceValue = input;
            so.FindProperty("addFriendButton").objectReferenceValue = addBtn;
            so.FindProperty("feedbackText").objectReferenceValue = feedback;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<SocialDemoDriver>();
            var dso = new SerializedObject(driver); dso.FindProperty("view").objectReferenceValue = view; dso.ApplyModifiedPropertiesWithoutUndo();

            Save("Assets/Scenes/SocialProfileDemo.unity");
            EditorUtility.DisplayDialog("Project D", "Social/Profile demo built.\nPress Play: profile showcase + friends (online-sorted) + requests (accept) + add-friend.", "OK");
        }

        // ---- prefabs ----
        private static MenuNavButton BuildNavPrefab()
        {
            var go = new GameObject("NavButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(MenuNavButton));
            go.GetComponent<Image>().color = new Color(0.13f,0.15f,0.21f,0.95f); go.GetComponent<LayoutElement>().preferredHeight = 60;
            var accent = ChildImage(go.transform, "Accent", new Color(1,1,1,0), raycast:false); var ar=(RectTransform)accent.transform; ar.anchorMin=new Vector2(0,0); ar.anchorMax=new Vector2(0,1); ar.pivot=new Vector2(0,0.5f); ar.sizeDelta=new Vector2(5,0); ar.anchoredPosition=Vector2.zero;
            var label = Label((RectTransform)go.transform, "Label", "Nav", new Vector2(0,0.5f), new Vector2(24,0), new Vector2(200,30), 20, Ink, TextAnchor.MiddleLeft, true);
            var badge = new GameObject("Badge", typeof(RectTransform), typeof(Image)); badge.transform.SetParent(go.transform,false); var brt=(RectTransform)badge.transform; brt.anchorMin=new Vector2(1,0.5f); brt.anchorMax=new Vector2(1,0.5f); brt.pivot=new Vector2(1,0.5f); brt.anchoredPosition=new Vector2(-14,0); brt.sizeDelta=new Vector2(30,30); badge.GetComponent<Image>().color=Red;
            var bc = Label(brt, "Count", "0", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(30,30), 15, Color.white, TextAnchor.MiddleCenter, true);
            badge.SetActive(false);
            var comp = go.GetComponent<MenuNavButton>();
            var so=new SerializedObject(comp);
            so.FindProperty("background").objectReferenceValue=go.GetComponent<Image>();
            so.FindProperty("label").objectReferenceValue=label;
            so.FindProperty("accent").objectReferenceValue=accent;
            so.FindProperty("badge").objectReferenceValue=badge;
            so.FindProperty("badgeCount").objectReferenceValue=bc;
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(false);
            return comp;
        }

        private static FriendRow BuildFriendRowPrefab(bool acceptStyle)
        {
            var go = new GameObject("FriendRow", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(FriendRow));
            go.GetComponent<Image>().color = new Color(0.13f,0.15f,0.21f,0.9f); go.GetComponent<LayoutElement>().preferredHeight = 56;
            var dot = ChildImage(go.transform, "Dot", new Color(0.36f,0.62f,0.92f), raycast:false); var dr=(RectTransform)dot.transform; dr.anchorMin=new Vector2(0,0.5f); dr.anchorMax=new Vector2(0,0.5f); dr.pivot=new Vector2(0,0.5f); dr.anchoredPosition=new Vector2(14,0); dr.sizeDelta=new Vector2(16,16);
            var name = Label((RectTransform)go.transform, "Name", "Friend", new Vector2(0,0.5f), new Vector2(44,10), new Vector2(280,26), 18, Ink, TextAnchor.MiddleLeft, true);
            var status = Label((RectTransform)go.transform, "Status", "Online", new Vector2(0,0.5f), new Vector2(44,-14), new Vector2(320,20), 14, Sub, TextAnchor.MiddleLeft, false);
            var level = Label((RectTransform)go.transform, "Level", "Lv 1", new Vector2(0,0.5f), new Vector2(360,10), new Vector2(120,22), 15, Sub, TextAnchor.MiddleLeft, false);
            var rank = Label((RectTransform)go.transform, "Rank", "", new Vector2(0,0.5f), new Vector2(360,-14), new Vector2(180,20), 14, Gold, TextAnchor.MiddleLeft, false);
            var invite = ActionButtonMini(go.transform, acceptStyle?"ACCEPT":"INVITE", new Vector2(1,0.5f), new Vector2(-140,0), acceptStyle?new Color(0.30f,0.55f,0.40f):new Color(0.30f,0.45f,0.62f));
            var profile = ActionButtonMini(go.transform, "PROFILE", new Vector2(1,0.5f), new Vector2(-16,0), new Color(0.24f,0.28f,0.36f));
            var comp = go.GetComponent<FriendRow>();
            var so=new SerializedObject(comp);
            so.FindProperty("background").objectReferenceValue=go.GetComponent<Image>();
            so.FindProperty("presenceDot").objectReferenceValue=dot;
            so.FindProperty("nameText").objectReferenceValue=name;
            so.FindProperty("levelText").objectReferenceValue=level;
            so.FindProperty("rankText").objectReferenceValue=rank;
            so.FindProperty("statusText").objectReferenceValue=status;
            so.FindProperty("inviteButton").objectReferenceValue=invite;
            so.FindProperty("profileButton").objectReferenceValue=profile;
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(false);
            return comp;
        }

        // ---- helpers ----
        private static RectTransform ScrollList(RectTransform panel, Vector2 pos, Vector2 size)
        {
            var vp = MakePanel(panel, "Viewport", new Vector2(0.5f,1), pos, new Vector2(panel.sizeDelta.x + size.x, size.y)); vp.pivot=new Vector2(0.5f,1); vp.GetComponent<Image>().color=new Color(0,0,0,0.15f);
            vp.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var content = new GameObject("Content", typeof(RectTransform)); content.transform.SetParent(vp, false);
            var crt=(RectTransform)content.transform; crt.anchorMin=new Vector2(0,1); crt.anchorMax=new Vector2(1,1); crt.pivot=new Vector2(0.5f,1); crt.sizeDelta=new Vector2(0,size.y);
            var vlg=content.AddComponent<VerticalLayoutGroup>(); vlg.spacing=6; vlg.padding=new RectOffset(12,12,8,8); vlg.childForceExpandHeight=false; vlg.childControlHeight=false;
            content.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var sr=vp.gameObject.AddComponent<ScrollRect>(); sr.viewport=vp; sr.content=crt; sr.horizontal=false;
            return crt;
        }
        private static InputField BuildInput(RectTransform parent, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go=new GameObject("Input", typeof(RectTransform), typeof(Image), typeof(InputField)); go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size;
            go.GetComponent<Image>().color=new Color(0.16f,0.18f,0.24f);
            var ph=Label(rt,"Placeholder","Enter player ID...",new Vector2(0,0.5f),new Vector2(14,0),new Vector2(size.x-28,size.y),18,new Color(0.5f,0.55f,0.62f),TextAnchor.MiddleLeft,false);
            var txt=Label(rt,"Text","",new Vector2(0,0.5f),new Vector2(14,0),new Vector2(size.x-28,size.y),18,Ink,TextAnchor.MiddleLeft,false);
            var inp=go.GetComponent<InputField>(); inp.textComponent=txt; inp.placeholder=ph; return inp;
        }
        private static Button ActionButtonMini(Transform parent, string text, Vector2 anchor, Vector2 pos, Color c)
        {
            var go=new GameObject("Mini_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(1,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(118,40);
            go.GetComponent<Image>().color=c; Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(118,40),14,Ink,TextAnchor.MiddleCenter,true);
            return go.GetComponent<Button>();
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
        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c, out Text label){ var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(1,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(240,50); go.GetComponent<Image>().color=c; label=Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(240,50),18,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
        private static Font Font()=>Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
#endif
