// Project D  |  Codename: Zantetsuken
// KpStoreDemoBuilder.cs  (Editor-only)  — Tools > Project D > Build KP Store Demo
//
// Builds a runnable out-of-game Store scene: currency header (level + XP + KP),
// category tabs (Featured/Champions/Borders/Icons/Bundles), an item grid, a detail/
// buy panel, and a "GRANT XP" button to simulate post-match XP so you can watch the
// account level up, earn KP, and spend it on champions/borders/icons. Uses the
// imported StoreItemDefinition assets (run Import KP Store Items first).

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.StoreUI;
using ProjectD.Store;

namespace ProjectD.EditorTools
{
    public static class KpStoreDemoBuilder
    {
        private const string ScenePath = "Assets/Scenes/KpStoreDemo.unity";
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static readonly Color Idle = new Color(0.16f,0.18f,0.24f);
        private static Font _font;

        [MenuItem("Tools/Project D/Build KP Store Demo", priority = 87)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }

            var canvasGo = new GameObject("StoreCanvas");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            Label(root, "Title", "STORE", new Vector2(0,1), new Vector2(40,-34), new Vector2(400,50), 34, Ink, TextAnchor.MiddleLeft, true);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,28), 18, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Currency header (top-right).
            var header = MakePanel(root, "CurrencyHeader", new Vector2(1,1), new Vector2(-260,-60), new Vector2(480,72));
            var lvl = Label(header, "Lvl", "Lv 1", new Vector2(0,0.5f), new Vector2(16,10), new Vector2(120,28), 22, Ink, TextAnchor.MiddleLeft, true);
            var xpBack = MakePanel(header, "XpBack", new Vector2(0,0.5f), new Vector2(150,10), new Vector2(300,14)); xpBack.pivot=new Vector2(0,0.5f); xpBack.anchorMin=new Vector2(0,0.5f); xpBack.anchorMax=new Vector2(0,0.5f); xpBack.GetComponent<Image>().color=new Color(1,1,1,0.12f);
            var xpFill = ChildImage(xpBack.transform, "Fill", Gold, raycast:false); var fr=(RectTransform)xpFill.transform; fr.anchorMin=Vector2.zero; fr.anchorMax=Vector2.one; fr.offsetMin=Vector2.zero; fr.offsetMax=Vector2.zero; xpFill.type=Image.Type.Filled; xpFill.fillMethod=Image.FillMethod.Horizontal; xpFill.fillOrigin=(int)Image.OriginHorizontal.Left; xpFill.fillAmount=0.2f;
            var xpTxt = Label(header, "Xp", "0 / 600 XP", new Vector2(0,0.5f), new Vector2(150,-16), new Vector2(300,20), 14, Sub, TextAnchor.MiddleLeft, false);
            var kpTxt = Label(header, "Kp", "0 KP", new Vector2(1,0.5f), new Vector2(-16,0), new Vector2(160,30), 24, Gold, TextAnchor.MiddleRight, true);

            var currency = header.gameObject.AddComponent<CurrencyHeader>();
            var cso = new SerializedObject(currency);
            cso.FindProperty("levelText").objectReferenceValue = lvl;
            cso.FindProperty("xpFill").objectReferenceValue = xpFill;
            cso.FindProperty("xpText").objectReferenceValue = xpTxt;
            cso.FindProperty("kpText").objectReferenceValue = kpTxt;
            cso.ApplyModifiedPropertiesWithoutUndo();

            // Category tabs.
            var tabBar = MakePanel(root, "Tabs", new Vector2(0.5f,1), new Vector2(0,-96), new Vector2(1720,52)); tabBar.GetComponent<Image>().color=new Color(0,0,0,0);
            var hl = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>(); hl.spacing=8; hl.childForceExpandWidth=true; hl.childForceExpandHeight=true;
            var tabNames = new[] { "Featured", "Champions", "Borders", "Icons", "Bundles" };
            var tabButtons = new Button[tabNames.Length];
            for (int i = 0; i < tabNames.Length; i++) tabButtons[i] = Chip(tabBar, tabNames[i]);

            // Grid scroll (left).
            var scroll = MakePanel(root, "Grid", new Vector2(0.5f,0.5f), new Vector2(-320,-20), new Vector2(1060,640));
            var sr = scroll.gameObject.AddComponent<ScrollRect>();
            var vp = MakePanel(scroll, "Viewport", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1040,620)); vp.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var contentGo = new GameObject("Content", typeof(RectTransform)); contentGo.transform.SetParent(vp, false);
            var content=(RectTransform)contentGo.transform; content.anchorMin=new Vector2(0,1); content.anchorMax=new Vector2(1,1); content.pivot=new Vector2(0.5f,1); content.sizeDelta=new Vector2(0,620);
            var grid = contentGo.AddComponent<GridLayoutGroup>(); grid.cellSize=new Vector2(200,150); grid.spacing=new Vector2(14,14); grid.padding=new RectOffset(14,14,14,14); grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount=4;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            sr.viewport=vp; sr.content=content; sr.horizontal=false;
            var cardPrefab = BuildCardPrefab();

            // Detail / buy panel (right).
            var detail = MakePanel(root, "Detail", new Vector2(1,0.5f), new Vector2(-190,-20), new Vector2(360,640)); detail.GetComponent<Image>().color=Panel;
            var dPrev = ChildImage(detail.transform, "Preview", new Color(0.3f,0.35f,0.5f), raycast:false); var drt=(RectTransform)dPrev.transform; drt.anchorMin=new Vector2(0.5f,1); drt.anchorMax=new Vector2(0.5f,1); drt.pivot=new Vector2(0.5f,1); drt.anchoredPosition=new Vector2(0,-20); drt.sizeDelta=new Vector2(300,180);
            var dName = Label(detail, "Name", "", new Vector2(0.5f,1), new Vector2(0,-214), new Vector2(340,32), 24, Ink, TextAnchor.MiddleCenter, true);
            var dDesc = Wrapped(detail, "Desc", -256, 80, Sub, 16);
            var dPrice = Label(detail, "Price", "", new Vector2(0.5f,0), new Vector2(0,200), new Vector2(340,34), 26, Gold, TextAnchor.MiddleCenter, true);
            var buy = ActionButton(detail, "BUY", new Vector2(0.5f,0), new Vector2(0,130), new Color(0.30f,0.55f,0.32f), out var buyLabel);
            var equip = ActionButton(detail, "EQUIP", new Vector2(0.5f,0), new Vector2(0,70), new Color(0.30f,0.45f,0.62f), out _);
            var feedback = Label(detail, "Feedback", "", new Vector2(0.5f,0), new Vector2(0,26), new Vector2(340,26), 18, Sub, TextAnchor.MiddleCenter, false);

            // Demo: GRANT XP button (bottom-left) to simulate post-match progression.
            var grantXp = ActionButton(root, "GRANT 800 XP", new Vector2(0,0), new Vector2(160,60), new Color(0.45f,0.40f,0.62f), out _);

            // Level-up popup overlay.
            var popup = MakePanel(root, "LevelUpPopup", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1920,1080)); popup.GetComponent<Image>().color=new Color(0,0,0,0.78f);
            var pGroup = popup.gameObject.AddComponent<CanvasGroup>(); pGroup.alpha=0f;
            var pHead = Label(popup, "H", "LEVEL UP!", new Vector2(0.5f,0.5f), new Vector2(0,160), new Vector2(800,70), 54, Gold, TextAnchor.MiddleCenter, true);
            var pLevel = Label(popup, "L", "Level 2", new Vector2(0.5f,0.5f), new Vector2(0,70), new Vector2(600,44), 30, Ink, TextAnchor.MiddleCenter, true);
            var pKp = Label(popup, "K", "+60 KP", new Vector2(0.5f,0.5f), new Vector2(0,10), new Vector2(400,40), 28, new Color(0.6f,0.9f,0.6f), TextAnchor.MiddleCenter, true);
            var pRing = ChildImage(popup.transform, "Ring", new Color(0.36f,0.62f,0.92f,0.25f), raycast:false); var prt=(RectTransform)pRing.transform; prt.anchorMin=new Vector2(0.5f,0.5f); prt.anchorMax=new Vector2(0.5f,0.5f); prt.pivot=new Vector2(0.5f,0.5f); prt.anchoredPosition=new Vector2(0,-90); prt.sizeDelta=new Vector2(120,120);
            var pCont = ActionButton(popup, "CONTINUE", new Vector2(0.5f,0.5f), new Vector2(0,-200), new Color(0.30f,0.45f,0.62f), out _);
            var popupComp = popup.gameObject.AddComponent<LevelUpPopup>();
            var pso = new SerializedObject(popupComp);
            pso.FindProperty("group").objectReferenceValue = pGroup;
            pso.FindProperty("headline").objectReferenceValue = pHead;
            pso.FindProperty("levelLabel").objectReferenceValue = pLevel;
            pso.FindProperty("kpLabel").objectReferenceValue = pKp;
            pso.FindProperty("levelRing").objectReferenceValue = pRing;
            pso.FindProperty("continueButton").objectReferenceValue = pCont;
            pso.ApplyModifiedPropertiesWithoutUndo();

            // Screen component + wiring.
            var screen = canvasGo.AddComponent<KpStoreScreen>();
            var so = new SerializedObject(screen);
            so.FindProperty("currencyHeader").objectReferenceValue = currency;
            var tb = so.FindProperty("categoryButtons"); tb.arraySize = tabButtons.Length;
            for (int i = 0; i < tabButtons.Length; i++) tb.GetArrayElementAtIndex(i).objectReferenceValue = tabButtons[i];
            so.FindProperty("gridContent").objectReferenceValue = content;
            so.FindProperty("cardPrefab").objectReferenceValue = cardPrefab;
            so.FindProperty("detailPanel").objectReferenceValue = detail.gameObject;
            so.FindProperty("detailPreview").objectReferenceValue = dPrev;
            so.FindProperty("detailName").objectReferenceValue = dName;
            so.FindProperty("detailDescription").objectReferenceValue = dDesc;
            so.FindProperty("detailPrice").objectReferenceValue = dPrice;
            so.FindProperty("buyButton").objectReferenceValue = buy;
            so.FindProperty("buyButtonLabel").objectReferenceValue = buyLabel;
            so.FindProperty("equipButton").objectReferenceValue = equip;
            so.FindProperty("feedbackLabel").objectReferenceValue = feedback;
            so.FindProperty("levelUpPopup").objectReferenceValue = popupComp;

            // Inject catalog + a starting account (level 1, some KP) for the demo.
            var items = LoadAll<StoreItemDefinition>();
            var cat = so.FindProperty("catalog"); cat.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++) cat.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            // Hook GRANT XP button to the screen's public GrantXp via a tiny relay.
            var relay = canvasGo.AddComponent<KpStoreDemoRelay>();
            var rso = new SerializedObject(relay);
            rso.FindProperty("screen").objectReferenceValue = screen;
            rso.FindProperty("grantXpButton").objectReferenceValue = grantXp;
            rso.FindProperty("xpPerClick").intValue = 800;
            rso.FindProperty("startingKp").intValue = 8000;
            rso.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                items.Count == 0
                    ? "KP Store demo built, but no items found.\nRun Import KP Store Items first, then rebuild."
                    : $"KP Store demo built with {items.Count} items:\n{ScenePath}\n\nPress Play. Tap GRANT 800 XP to level up and earn KP, " +
                      "then buy champions/borders/icons. KP-only (Store guardrail).",
                "OK");
            Debug.Log($"[Project D] KP Store demo created ({items.Count} items).");
        }

        private static StoreItemCard BuildCardPrefab()
        {
            var go = new GameObject("StoreItemCard", typeof(RectTransform), typeof(Image), typeof(StoreItemCard));
            go.GetComponent<Image>().color = new Color(0.13f,0.15f,0.21f,0.95f);
            var frame = ChildImage(go.transform, "Frame", new Color(1,1,1,0.25f), stretch:true, raycast:false);
            var prev = ChildImage(go.transform, "Preview", new Color(0.3f,0.35f,0.5f), raycast:false); var prt=(RectTransform)prev.transform; prt.anchorMin=new Vector2(0.5f,1); prt.anchorMax=new Vector2(0.5f,1); prt.pivot=new Vector2(0.5f,1); prt.anchoredPosition=new Vector2(0,-10); prt.sizeDelta=new Vector2(120,80);
            var name = Label((RectTransform)go.transform,"Name","Item",new Vector2(0.5f,0),new Vector2(0,42),new Vector2(190,22),15,Ink,TextAnchor.MiddleCenter,true);
            var price = Label((RectTransform)go.transform,"Price","0 KP",new Vector2(0.5f,0),new Vector2(0,18),new Vector2(190,22),16,Gold,TextAnchor.MiddleCenter,true);
            var sale = Label((RectTransform)go.transform,"Sale","",new Vector2(0.5f,0),new Vector2(0,2),new Vector2(190,16),13,new Color(0.6f,0.9f,0.6f),TextAnchor.MiddleCenter,true); sale.gameObject.SetActive(false);
            var owned = MakeBadge(go.transform, "Owned", "OWNED", new Color(0.3f,0.6f,0.4f));
            var lockB = MakeBadge(go.transform, "Lock", "Lv", new Color(0.5f,0.4f,0.3f));
            var lockText = lockB.GetComponentInChildren<Text>();

            var card = go.GetComponent<StoreItemCard>();
            var so = new SerializedObject(card);
            so.FindProperty("background").objectReferenceValue = go.GetComponent<Image>();
            so.FindProperty("rarityFrame").objectReferenceValue = frame;
            so.FindProperty("preview").objectReferenceValue = prev;
            so.FindProperty("nameLabel").objectReferenceValue = name;
            so.FindProperty("priceLabel").objectReferenceValue = price;
            so.FindProperty("salePriceLabel").objectReferenceValue = sale;
            so.FindProperty("ownedBadge").objectReferenceValue = owned;
            so.FindProperty("lockBadge").objectReferenceValue = lockB;
            so.FindProperty("lockText").objectReferenceValue = lockText;
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(false);
            return card;
        }

        private static GameObject MakeBadge(Transform parent, string name, string text, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=new Vector2(1,1); rt.anchorMax=new Vector2(1,1); rt.pivot=new Vector2(1,1); rt.anchoredPosition=new Vector2(-6,-6); rt.sizeDelta=new Vector2(64,24);
            go.GetComponent<Image>().color = c;
            Label(rt, "T", text, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(64,24), 13, Color.white, TextAnchor.MiddleCenter, true);
            go.SetActive(false);
            return go;
        }

        // ---- primitives ----
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Panel; return rt; }
        private static Image ChildImage(Transform parent, string name, Color c, bool stretch=false, bool raycast=true)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var img=go.GetComponent<Image>(); img.color=c; img.raycastTarget=raycast; if(stretch){var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;} return img; }
        private static Button Chip(RectTransform parent, string text)
        { var go=new GameObject("Chip_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); go.GetComponent<Image>().color=Idle; Label((RectTransform)go.transform,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(180,40),18,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c, out Text label)
        { var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(280,52); go.GetComponent<Image>().color=c; label=Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(280,52),18,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Text Wrapped(RectTransform parent, string name, float topY, float h, Color c, int fs)
        { var t=Label(parent,name,"",new Vector2(0.5f,1),new Vector2(0,topY),new Vector2(336,h),fs,c,TextAnchor.UpperLeft,false); t.horizontalOverflow=HorizontalWrapMode.Wrap; t.verticalOverflow=VerticalWrapMode.Truncate; return t; }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
        private static List<T> LoadAll<T>() where T : Object
        { var list=new List<T>(); foreach(var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[]{"Assets/GameData/Store"})) list.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))); return list; }
    }
}
#endif
