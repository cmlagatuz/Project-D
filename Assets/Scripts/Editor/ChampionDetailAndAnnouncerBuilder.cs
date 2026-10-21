// Project D  |  Codename: Zantetsuken
// ChampionDetailAndAnnouncerBuilder.cs  (Editor-only)
//   Tools > Project D > Build Champion Detail Demo
//   Tools > Project D > Build Announcer Banner Demo
//
// Builds two runnable scenes: the champion detail/preview page (stat bars, ability
// row, skins carousel, KP unlock panel) and an in-match announcer banner demo (queue,
// styling, animations). No art needed.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.ChampionDetailUI;
using ProjectD.AnnouncerUI;

namespace ProjectD.EditorTools
{
    public static class ChampionDetailAndAnnouncerBuilder
    {
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static Font _font;

        // ===================== CHAMPION DETAIL =====================
        [MenuItem("Tools/Project D/Build Champion Detail Demo", priority = 89)]
        public static void BuildChampionDetail()
        {
            _font = Font();
            var root = NewScene("Assets/Scenes/ChampionDetailDemo.unity", out var canvasGo);

            Label(root, "Back", "< CHAMPIONS", new Vector2(0,1), new Vector2(40,-34), new Vector2(300,36), 22, Sub, TextAnchor.MiddleLeft, true);
            var kp = Label(root, "KP", "6,000 KP", new Vector2(1,1), new Vector2(-40,-34), new Vector2(300,36), 26, Gold, TextAnchor.MiddleRight, true);

            // Splash (right).
            var splash = ChildImage(root, "Splash", new Color(0.30f,0.45f,0.80f), raycast:false);
            var sprt=(RectTransform)splash.transform; sprt.anchorMin=new Vector2(1,0.5f); sprt.anchorMax=new Vector2(1,0.5f); sprt.pivot=new Vector2(1,0.5f); sprt.anchoredPosition=new Vector2(-60,0); sprt.sizeDelta=new Vector2(900,760);

            // Header (left).
            var name = Label(root, "Name", "KAIN HIGHWIND", new Vector2(0,1), new Vector2(60,-120), new Vector2(760,60), 46, Ink, TextAnchor.UpperLeft, true);
            var title = Label(root, "Title", "The Dragoon of Baron", new Vector2(0,1), new Vector2(62,-176), new Vector2(760,32), 24, Sub, TextAnchor.UpperLeft, false);
            var role = Label(root, "Role", "Assassin / Fighter", new Vector2(0,1), new Vector2(62,-212), new Vector2(760,28), 20, Gold, TextAnchor.UpperLeft, true);

            // Stat bars.
            var dmgFill = StatBar(root, "Damage", -280);
            var tufFill = StatBar(root, "Toughness", -330);
            var utiFill = StatBar(root, "Utility", -380);
            var difFill = StatBar(root, "Difficulty", -430);

            // Ability row.
            var abilityIcons = new Image[5]; var abilityLabels = new Text[5];
            var abilityRow = new GameObject("Abilities", typeof(RectTransform)); abilityRow.transform.SetParent(root, false);
            var arr=(RectTransform)abilityRow.transform; arr.anchorMin=new Vector2(0,1); arr.anchorMax=new Vector2(0,1); arr.pivot=new Vector2(0,1); arr.anchoredPosition=new Vector2(60,-500); arr.sizeDelta=new Vector2(600,90);
            var hl = abilityRow.AddComponent<HorizontalLayoutGroup>(); hl.spacing=12; hl.childForceExpandWidth=false; hl.childControlWidth=false;
            for (int i = 0; i < 5; i++)
            {
                var cell = new GameObject("Ab"+i, typeof(RectTransform), typeof(Image), typeof(LayoutElement)); cell.transform.SetParent(abilityRow.transform, false);
                cell.GetComponent<Image>().color = new Color(0.28f,0.34f,0.5f); cell.GetComponent<LayoutElement>().preferredWidth=76; cell.GetComponent<LayoutElement>().preferredHeight=76;
                abilityIcons[i] = cell.GetComponent<Image>();
                abilityLabels[i] = Label((RectTransform)cell.transform, "L", new[]{"P","Q","W","E","R"}[i], new Vector2(0.5f,0), new Vector2(0,-14), new Vector2(76,18), 13, Sub, TextAnchor.MiddleCenter, true);
            }

            // Skins carousel (bottom-left).
            var skinPanel = MakePanel(root, "Skins", new Vector2(0,0), new Vector2(360, 180), new Vector2(680, 220));
            var skinPreview = ChildImage(skinPanel.transform, "Preview", new Color(0.5f,0.4f,0.7f), raycast:false); var skp=(RectTransform)skinPreview.transform; skp.anchorMin=new Vector2(0.5f,1); skp.anchorMax=new Vector2(0.5f,1); skp.pivot=new Vector2(0.5f,1); skp.anchoredPosition=new Vector2(0,-14); skp.sizeDelta=new Vector2(560,120);
            var skinName = Label(skinPanel, "SName", "Kain Highwind", new Vector2(0.5f,0), new Vector2(0,54), new Vector2(600,28), 22, Ink, TextAnchor.MiddleCenter, true);
            var skinRarity = Label(skinPanel, "SRar", "Common", new Vector2(0.5f,0), new Vector2(0,28), new Vector2(600,22), 16, Sub, TextAnchor.MiddleCenter, false);
            var skinIndex = Label(skinPanel, "SIdx", "1 / 3", new Vector2(0.5f,0), new Vector2(0,8), new Vector2(200,20), 14, Sub, TextAnchor.MiddleCenter, false);
            var skinLock = MakeBadge(skinPanel, "Lock", "LOCKED", new Color(0.5f,0.4f,0.3f)); skinLock.SetActive(false);
            var prev = SmallButton(skinPanel, "<", new Vector2(0,0.5f), new Vector2(24,20));
            var next = SmallButton(skinPanel, ">", new Vector2(1,0.5f), new Vector2(-24,20));

            // Buy panel (bottom-right).
            var buyPanel = MakePanel(root, "Buy", new Vector2(1,0), new Vector2(-360, 120), new Vector2(680, 120));
            var price = Label(buyPanel, "Price", "4,500 KP", new Vector2(0,0.5f), new Vector2(28,0), new Vector2(300,40), 30, Gold, TextAnchor.MiddleLeft, true);
            var buy = ActionButton(buyPanel, "UNLOCK  4,500 KP", new Vector2(1,0.5f), new Vector2(-24,0), new Color(0.30f,0.55f,0.32f), out var buyLabel);
            var feedback = Label(root, "Feedback", "", new Vector2(1,0), new Vector2(-360, 60), new Vector2(680,26), 18, Sub, TextAnchor.MiddleCenter, false);

            var view = canvasGo.AddComponent<ChampionDetailView>();
            var so = new SerializedObject(view);
            so.FindProperty("nameLabel").objectReferenceValue = name;
            so.FindProperty("titleLabel").objectReferenceValue = title;
            so.FindProperty("roleLabel").objectReferenceValue = role;
            so.FindProperty("splash").objectReferenceValue = splash;
            so.FindProperty("damageFill").objectReferenceValue = dmgFill;
            so.FindProperty("toughnessFill").objectReferenceValue = tufFill;
            so.FindProperty("utilityFill").objectReferenceValue = utiFill;
            so.FindProperty("difficultyFill").objectReferenceValue = difFill;
            var ai = so.FindProperty("abilityIcons"); ai.arraySize = 5; for (int i=0;i<5;i++) ai.GetArrayElementAtIndex(i).objectReferenceValue = abilityIcons[i];
            var al = so.FindProperty("abilityLabels"); al.arraySize = 5; for (int i=0;i<5;i++) al.GetArrayElementAtIndex(i).objectReferenceValue = abilityLabels[i];
            so.FindProperty("skinNameLabel").objectReferenceValue = skinName;
            so.FindProperty("skinRarityLabel").objectReferenceValue = skinRarity;
            so.FindProperty("skinPreview").objectReferenceValue = skinPreview;
            so.FindProperty("skinLockBadge").objectReferenceValue = skinLock;
            so.FindProperty("skinPrevButton").objectReferenceValue = prev;
            so.FindProperty("skinNextButton").objectReferenceValue = next;
            so.FindProperty("skinIndexLabel").objectReferenceValue = skinIndex;
            so.FindProperty("priceLabel").objectReferenceValue = price;
            so.FindProperty("buyButton").objectReferenceValue = buy;
            so.FindProperty("buyButtonLabel").objectReferenceValue = buyLabel;
            so.FindProperty("feedbackLabel").objectReferenceValue = feedback;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<ChampionDetailDemoDriver>();
            var dso = new SerializedObject(driver); dso.FindProperty("view").objectReferenceValue = view; dso.FindProperty("accountLevel").intValue = 20; dso.FindProperty("kpBalance").longValue = 6000; dso.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("Assets/Scenes/ChampionDetailDemo.unity");
            EditorUtility.DisplayDialog("Project D", "Champion Detail demo built.\nPress Play: stat bars, ability row, skins carousel (</> ), and the KP unlock panel.", "OK");
        }

        // ===================== ANNOUNCER =====================
        [MenuItem("Tools/Project D/Build Announcer Banner Demo", priority = 90)]
        public static void BuildAnnouncer()
        {
            _font = Font();
            var root = NewScene("Assets/Scenes/AnnouncerDemo.unity", out var canvasGo);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,28), 16, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);
            Label(root, "Note", "In-match announcer banners (server-driven).", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(900,30), 20, Sub, TextAnchor.MiddleCenter, false);

            // Banner (upper-center of the HUD).
            var bannerPanel = MakePanel(root, "BannerPanel", new Vector2(0.5f,1), new Vector2(0,-220), new Vector2(900, 120));
            bannerPanel.GetComponent<Image>().color = new Color(0.08f,0.09f,0.13f,0.85f);
            var group = bannerPanel.gameObject.AddComponent<CanvasGroup>(); group.alpha = 0f;
            var accent = ChildImage(bannerPanel.transform, "Accent", Gold, raycast:false); var acr=(RectTransform)accent.transform; acr.anchorMin=new Vector2(0,0); acr.anchorMax=new Vector2(1,0); acr.pivot=new Vector2(0.5f,0); acr.sizeDelta=new Vector2(0,6); acr.anchoredPosition=Vector2.zero;
            var headline = Label(bannerPanel, "Headline", "DOUBLE KILL", new Vector2(0.5f,0.5f), new Vector2(0,10), new Vector2(880,60), 42, Ink, TextAnchor.MiddleCenter, true);
            var subject = Label(bannerPanel, "Subject", "You", new Vector2(0.5f,0), new Vector2(0,14), new Vector2(880,26), 20, Sub, TextAnchor.MiddleCenter, false);

            var banner = canvasGo.AddComponent<AnnouncerBanner>();
            var bso = new SerializedObject(banner);
            bso.FindProperty("group").objectReferenceValue = group;
            bso.FindProperty("panel").objectReferenceValue = bannerPanel;
            bso.FindProperty("accentBar").objectReferenceValue = accent;
            bso.FindProperty("headlineText").objectReferenceValue = headline;
            bso.FindProperty("subjectText").objectReferenceValue = subject;
            bso.ApplyModifiedPropertiesWithoutUndo();

            // Trigger buttons.
            var dk = ActionButton(root, "DOUBLE KILL", new Vector2(0.5f,0), new Vector2(-460,80), new Color(0.30f,0.45f,0.62f), out _);
            var ace = ActionButton(root, "ACE", new Vector2(0.5f,0), new Vector2(-160,80), new Color(0.62f,0.35f,0.45f), out _);
            var odin = ActionButton(root, "ODIN SLAIN", new Vector2(0.5f,0), new Vector2(160,80), new Color(0.55f,0.45f,0.30f), out _);
            var core = ActionButton(root, "CORE EXPOSED", new Vector2(0.5f,0), new Vector2(480,80), new Color(0.60f,0.30f,0.45f), out _);
            var seq = ActionButton(root, "PLAY SEQUENCE", new Vector2(0.5f,0), new Vector2(0,20), new Color(0.36f,0.62f,0.42f), out _);

            var driver = canvasGo.AddComponent<AnnouncerDemoDriver>();
            var dso = new SerializedObject(driver);
            dso.FindProperty("banner").objectReferenceValue = banner;
            dso.FindProperty("doubleKillButton").objectReferenceValue = dk;
            dso.FindProperty("aceButton").objectReferenceValue = ace;
            dso.FindProperty("odinButton").objectReferenceValue = odin;
            dso.FindProperty("coreButton").objectReferenceValue = core;
            dso.FindProperty("sequenceButton").objectReferenceValue = seq;
            dso.ApplyModifiedPropertiesWithoutUndo();

            SaveScene("Assets/Scenes/AnnouncerDemo.unity");
            EditorUtility.DisplayDialog("Project D", "Announcer Banner demo built.\nPress Play: a sequence auto-plays; buttons trigger specific callouts.", "OK");
        }

        // ---- shared helpers ----
        private static RectTransform NewScene(string path, out GameObject canvasGo)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }
            canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            return (RectTransform)canvasGo.transform;
        }
        private static void SaveScene(string path)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
            AssetDatabase.Refresh();
        }
        private static Image StatBar(RectTransform root, string label, float y)
        {
            Label(root, "SL_" + label, label, new Vector2(0,1), new Vector2(60, y), new Vector2(180,26), 18, Sub, TextAnchor.MiddleLeft, true);
            var back = MakePanel(root, "SB_" + label, new Vector2(0,1), new Vector2(500, y-4), new Vector2(520, 18)); back.pivot=new Vector2(0.5f,1); back.anchorMin=new Vector2(0,1); back.anchorMax=new Vector2(0,1); back.GetComponent<Image>().color=new Color(1,1,1,0.12f);
            var fill = ChildImage(back.transform, "Fill", new Color(0.36f,0.72f,0.92f), raycast:false); var fr=(RectTransform)fill.transform; fr.anchorMin=Vector2.zero; fr.anchorMax=Vector2.one; fr.offsetMin=Vector2.zero; fr.offsetMax=Vector2.zero; fill.type=Image.Type.Filled; fill.fillMethod=Image.FillMethod.Horizontal; fill.fillOrigin=(int)Image.OriginHorizontal.Left; fill.fillAmount=0.5f;
            return fill;
        }
        private static GameObject MakeBadge(RectTransform parent, string name, string text, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=new Vector2(1,1); rt.anchorMax=new Vector2(1,1); rt.pivot=new Vector2(1,1); rt.anchoredPosition=new Vector2(-10,-10); rt.sizeDelta=new Vector2(90,28);
            go.GetComponent<Image>().color = c; Label(rt, "T", text, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(90,28), 13, Color.white, TextAnchor.MiddleCenter, true);
            return go;
        }
        private static Button SmallButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos)
        {
            var go = new GameObject("SB_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(44,44);
            go.GetComponent<Image>().color = new Color(0.30f,0.40f,0.52f);
            Label(rt, "L", text, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(44,44), 24, Ink, TextAnchor.MiddleCenter, true);
            return go.GetComponent<Button>();
        }
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Panel; return rt; }
        private static Image ChildImage(Transform parent, string name, Color c, bool stretch=false, bool raycast=true)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var img=go.GetComponent<Image>(); img.color=c; img.raycastTarget=raycast; if(stretch){var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;} return img; }
        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c, out Text label)
        { var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(280,52); go.GetComponent<Image>().color=c; label=Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(280,52),18,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
        private static Font Font() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
#endif
