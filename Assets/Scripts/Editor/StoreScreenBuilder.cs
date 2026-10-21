// Project D  |  Codename: Zantetsuken
// StoreScreenBuilder.cs  (Editor-only)
//
// Builds the store screen in code, populated from the imported ItemDefinition
// assets: a category tab bar, a scrollable item grid, a detail/build panel, the
// gold bar, a 6-slot inventory row (slot 6 = boots), a "recommended only" toggle,
// and the champion-exclusive gold tab. Assign a locked champion id to preview the
// exclusive flow.
//
//   Tools > Project D > Build Store Screen

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Store;
using ProjectD.UI.Store;

namespace ProjectD.EditorTools
{
    public static class StoreScreenBuilder
    {
        private const string ScenePath = "Assets/Scenes/StoreScreen.unity";
        private static readonly Color Bg = new Color(0.06f, 0.07f, 0.11f);
        private static readonly Color Panel = new Color(0.11f, 0.13f, 0.19f, 0.98f);
        private static readonly Color Ink = new Color(0.92f, 0.94f, 0.98f);
        private static readonly Color Sub = new Color(0.70f, 0.75f, 0.85f);
        private static readonly Color Gold = new Color(0.95f, 0.80f, 0.32f);

        [MenuItem("Tools/Project D/Build Store Screen", priority = 73)]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg;
            camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }

            var canvasGo = new GameObject("StoreCanvas");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var canvas_rt = (RectTransform)canvasGo.transform;

            Label(canvas_rt, "Title", "ITEM SHOP", new Vector2(0.5f,1), new Vector2(0,-30), new Vector2(600,50), 34, Ink, TextAnchor.MiddleCenter, true);
            var gold = Label(canvas_rt, "Gold", "3000g", new Vector2(1,1), new Vector2(-40,-34), new Vector2(240,44), 30, Gold, TextAnchor.MiddleRight, true);
            Label(canvas_rt, "Watermark", "PROJECT D · CONFIDENTIAL", new Vector2(0,1), new Vector2(20,-16), new Vector2(420,30), 18, new Color(1,1,1,0.3f), TextAnchor.MiddleLeft, false);

            // Tab bar
            var tabBar = MakePanel(canvas_rt, "TabBar", new Vector2(0.5f,1), new Vector2(0,-96), new Vector2(1720,54));
            var hl = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 6; hl.childForceExpandWidth = true; hl.childForceExpandHeight = true; hl.childAlignment = TextAnchor.MiddleCenter;
            var tabPrefab = BuildTabPrefab();

            // Grid (left) in a scroll view
            var (gridScroll, gridContent) = BuildGrid(canvas_rt);
            var cardPrefab = BuildCardPrefab();

            // Recommended-only toggle
            var toggle = BuildToggle(canvas_rt);

            // Detail panel (right)
            var detail = BuildDetailPanel(canvas_rt, out var refs, out var buyButton, out var buyLabel);

            // Inventory row (bottom): 5 general + 1 boots
            var invCards = BuildInventoryRow(canvas_rt, cardPrefab);
            var feedback = Label(canvas_rt, "Feedback", "", new Vector2(0.5f,0), new Vector2(0,150), new Vector2(900,30), 20, Sub, TextAnchor.MiddleCenter, false);

            // Sell / Undo buttons (below the detail panel's BUY button)
            var sellGo = BuildLabeledButton(canvas_rt, "SellButton", "SELL", new Vector2(1,0.5f), new Vector2(-300, -170), new Vector2(280, 56),
                new Color(0.62f,0.30f,0.34f), out var sellLabel, out var sellButton);
            var undoGo = BuildLabeledButton(canvas_rt, "UndoButton", "UNDO", new Vector2(1,0.5f), new Vector2(-300, -234), new Vector2(280, 56),
                new Color(0.30f,0.42f,0.60f), out var undoLabel, out var undoButton);
            // Undo countdown fill overlay on the undo button.
            var undoFillGo = ChildImage(undoGo.transform, "UndoTimer", new Color(1f,1f,1f,0.18f), stretch:true, raycast:false);
            var undoFill = undoFillGo.GetComponent<Image>();
            undoFill.type = Image.Type.Filled; undoFill.fillMethod = Image.FillMethod.Horizontal;
            undoFill.fillOrigin = (int)Image.OriginHorizontal.Left; undoFill.fillAmount = 0f;

            // Recommended quick-buy row (above the inventory)
            var quickTitle = Label(canvas_rt, "QuickBuyTitle", "Recommended", new Vector2(0,0), new Vector2(40, 300), new Vector2(700,28), 20, Gold, TextAnchor.MiddleLeft, true);
            var quickRow = BuildQuickBuyRow(canvas_rt, out var quickCardPrefab);

            // View component + wiring
            var view = canvasGo.AddComponent<StoreScreenView>();
            var so = new SerializedObject(view);
            so.FindProperty("sellButton").objectReferenceValue = sellButton;
            so.FindProperty("sellButtonLabel").objectReferenceValue = sellLabel;
            so.FindProperty("undoButton").objectReferenceValue = undoButton;
            so.FindProperty("undoButtonLabel").objectReferenceValue = undoLabel;
            so.FindProperty("undoTimerFill").objectReferenceValue = undoFill;
            so.FindProperty("quickBuyRow").objectReferenceValue = quickRow;
            so.FindProperty("quickBuyCardPrefab").objectReferenceValue = quickCardPrefab;
            so.FindProperty("quickBuyTitle").objectReferenceValue = quickTitle;
            so.FindProperty("tabBar").objectReferenceValue = tabBar;
            so.FindProperty("tabPrefab").objectReferenceValue = tabPrefab;
            so.FindProperty("gridContent").objectReferenceValue = gridContent;
            so.FindProperty("cardPrefab").objectReferenceValue = cardPrefab;
            so.FindProperty("recommendedOnlyToggle").objectReferenceValue = toggle;
            so.FindProperty("detailName").objectReferenceValue = refs.name;
            so.FindProperty("detailCost").objectReferenceValue = refs.cost;
            so.FindProperty("detailStats").objectReferenceValue = refs.stats;
            so.FindProperty("detailPassive").objectReferenceValue = refs.passive;
            so.FindProperty("detailBuildPath").objectReferenceValue = refs.build;
            so.FindProperty("detailFlavor").objectReferenceValue = refs.flavor;
            so.FindProperty("detailRoleBanner").objectReferenceValue = refs.banner;
            so.FindProperty("buyButton").objectReferenceValue = buyButton;
            so.FindProperty("buyButtonLabel").objectReferenceValue = buyLabel;
            so.FindProperty("goldLabel").objectReferenceValue = gold;
            so.FindProperty("purchaseFeedback").objectReferenceValue = feedback;
            var invProp = so.FindProperty("inventorySlots"); invProp.arraySize = invCards.Count;
            for (int i = 0; i < invCards.Count; i++) invProp.GetArrayElementAtIndex(i).objectReferenceValue = invCards[i];

            // Inject item assets + a default locked champion (Bartz) to preview exclusive.
            var items = LoadAllItems();
            var itemsProp = so.FindProperty("allItems"); itemsProp.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++) itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            so.FindProperty("lockedChampionId").stringValue = "bartz";
            so.FindProperty("playerRole").enumValueIndex = (int)StoreRole.Fighter;

            // Inject champion build recommendations (drives the quick-buy row).
            InjectBuilds(so);
            so.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                items.Count == 0
                    ? "Store screen built, but NO item assets found.\nRun Import Store Items first, then rebuild."
                    : $"Store screen built with {items.Count} items:\n{ScenePath}\n\nPress Play. The Exclusive tab is gold and enabled because 'bartz' is locked in — buy Chicken Knife to see the highlight/purchase flow.",
                "OK");
            Debug.Log($"[Project D] Store screen created ({items.Count} items).");
        }

        // ---- component prefabs ---- //
        private static StoreCategoryTab BuildTabPrefab()
        {
            var go = new GameObject("CategoryTab", typeof(RectTransform), typeof(Image), typeof(StoreCategoryTab));
            go.GetComponent<Image>().color = new Color(0.12f,0.14f,0.20f);
            var label = Label((RectTransform)go.transform, "Label", "Tab", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(180,40), 20, Ink, TextAnchor.MiddleCenter, true);
            var under = new GameObject("Underline", typeof(RectTransform), typeof(Image));
            under.transform.SetParent(go.transform, false);
            var urt = (RectTransform)under.transform; urt.anchorMin = new Vector2(0,0); urt.anchorMax = new Vector2(1,0); urt.pivot = new Vector2(0.5f,0);
            urt.sizeDelta = new Vector2(0,4); urt.anchoredPosition = Vector2.zero; under.SetActive(false);
            var tab = go.GetComponent<StoreCategoryTab>();
            var so = new SerializedObject(tab);
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("background").objectReferenceValue = go.GetComponent<Image>();
            so.FindProperty("underline").objectReferenceValue = under.GetComponent<Image>();
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(false);
            return tab;
        }

        private static StoreItemCard BuildCardPrefab()
        {
            var go = new GameObject("ItemCard", typeof(RectTransform), typeof(Image), typeof(StoreItemCard));
            var bg = go.GetComponent<Image>(); bg.color = new Color(0.14f,0.16f,0.23f);
            var frame = ChildImage(go.transform, "Frame", new Color(1,1,1,0.18f), stretch:true, raycast:false);
            var icon = ChildImage(go.transform, "Icon", Color.white, raycast:false);
            var irt = (RectTransform)icon.transform; irt.anchorMin=new Vector2(0.5f,1); irt.anchorMax=new Vector2(0.5f,1); irt.pivot=new Vector2(0.5f,1); irt.anchoredPosition=new Vector2(0,-10); irt.sizeDelta=new Vector2(70,54);
            var ribbon = ChildImage(go.transform, "ExclusiveRibbon", Gold, raycast:false);
            var rrt=(RectTransform)ribbon.transform; rrt.anchorMin=new Vector2(0,1); rrt.anchorMax=new Vector2(1,1); rrt.pivot=new Vector2(0.5f,1); rrt.sizeDelta=new Vector2(0,18); rrt.anchoredPosition=Vector2.zero;
            Label((RectTransform)ribbon.transform,"RibbonText","EXCLUSIVE",new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(160,18),12,new Color(0.1f,0.08f,0.02f),TextAnchor.MiddleCenter,true);
            ribbon.gameObject.SetActive(false);
            var badge = ChildImage(go.transform, "RecBadge", new Color(0.4f,0.72f,1f), raycast:false);
            var brt=(RectTransform)badge.transform; brt.anchorMin=new Vector2(0,1); brt.anchorMax=new Vector2(0,1); brt.pivot=new Vector2(0,1); brt.anchoredPosition=new Vector2(8,-8); brt.sizeDelta=new Vector2(14,14);
            var name = Label((RectTransform)go.transform,"Name","Item",new Vector2(0.5f,0),new Vector2(0,30),new Vector2(150,24),15,Ink,TextAnchor.MiddleCenter,true);
            var cost = Label((RectTransform)go.transform,"Cost","0",new Vector2(0.5f,0),new Vector2(0,8),new Vector2(150,20),14,new Color(0.92f,0.86f,0.55f),TextAnchor.MiddleCenter,true);
            var owned = ChildImage(go.transform,"Owned",new Color(0.4f,0.8f,0.55f,0.85f),raycast:false);
            var ort=(RectTransform)owned.transform; ort.anchorMin=new Vector2(1,1); ort.anchorMax=new Vector2(1,1); ort.pivot=new Vector2(1,1); ort.anchoredPosition=new Vector2(-6,-6); ort.sizeDelta=new Vector2(18,18); owned.gameObject.SetActive(false);

            var card = go.GetComponent<StoreItemCard>();
            var so = new SerializedObject(card);
            so.FindProperty("background").objectReferenceValue = bg;
            so.FindProperty("frame").objectReferenceValue = frame;
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("exclusiveRibbon").objectReferenceValue = ribbon;
            so.FindProperty("recommendBadge").objectReferenceValue = badge;
            so.FindProperty("nameLabel").objectReferenceValue = name;
            so.FindProperty("costLabel").objectReferenceValue = cost;
            so.FindProperty("ownedCheck").objectReferenceValue = owned.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(false);
            return card;
        }

        private static (RectTransform scroll, RectTransform content) BuildGrid(RectTransform parent)
        {
            var scroll = MakePanel(parent, "GridScroll", new Vector2(0.5f,0.5f), new Vector2(-320, 20), new Vector2(1040, 620));
            var sr = scroll.gameObject.AddComponent<ScrollRect>();
            var viewport = MakePanel(scroll, "Viewport", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1020, 600));
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var contentGo = new GameObject("Content", typeof(RectTransform)); contentGo.transform.SetParent(viewport, false);
            var content = (RectTransform)contentGo.transform; content.anchorMin=new Vector2(0,1); content.anchorMax=new Vector2(1,1); content.pivot=new Vector2(0.5f,1); content.anchoredPosition=Vector2.zero; content.sizeDelta=new Vector2(0,600);
            var grid = contentGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(150,120); grid.spacing = new Vector2(12,12); grid.padding = new RectOffset(14,14,14,14);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 6;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.viewport = viewport; sr.content = content; sr.horizontal = false; sr.vertical = true;
            return (scroll, content);
        }

        private static Toggle BuildToggle(RectTransform parent)
        {
            var go = new GameObject("RecommendedOnly", typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=new Vector2(0,0.5f); rt.anchorMax=new Vector2(0,0.5f); rt.pivot=new Vector2(0,0.5f); rt.anchoredPosition=new Vector2(60,350); rt.sizeDelta=new Vector2(360,34);
            var box = ChildImage(go.transform,"Box",new Color(0.2f,0.24f,0.32f)); var brt=(RectTransform)box.transform; brt.anchorMin=new Vector2(0,0.5f); brt.anchorMax=new Vector2(0,0.5f); brt.pivot=new Vector2(0,0.5f); brt.sizeDelta=new Vector2(24,24); brt.anchoredPosition=Vector2.zero;
            var check = ChildImage(box.transform,"Check",new Color(0.4f,0.72f,1f)); ((RectTransform)check.transform).sizeDelta=new Vector2(16,16);
            Label((RectTransform)go.transform,"Label","Recommended for my role",new Vector2(0,0.5f),new Vector2(34,0),new Vector2(320,30),18,Sub,TextAnchor.MiddleLeft,false);
            var t = go.GetComponent<Toggle>(); t.targetGraphic = box; t.graphic = check; t.isOn = false;
            return t;
        }

        private struct DetailRefs { public Text name, cost, stats, passive, build, flavor; public Image banner; }

        private static RectTransform BuildDetailPanel(RectTransform parent, out DetailRefs refs, out Button buyButton, out Text buyLabel)
        {
            var panel = MakePanel(parent, "DetailPanel", new Vector2(1,0.5f), new Vector2(-190, 20), new Vector2(360, 640));
            panel.GetComponent<Image>().color = Panel;
            var banner = ChildImage(panel, "RoleBanner", new Color(0.9f,0.55f,0.35f)); var brt=(RectTransform)banner.transform; brt.anchorMin=new Vector2(0,1); brt.anchorMax=new Vector2(1,1); brt.pivot=new Vector2(0.5f,1); brt.sizeDelta=new Vector2(0,8); brt.anchoredPosition=Vector2.zero;

            refs = new DetailRefs {
                name = Label(panel,"DName","Select an item",new Vector2(0.5f,1),new Vector2(0,-24),new Vector2(340,34),26,Ink,TextAnchor.MiddleCenter,true),
                cost = Label(panel,"DCost","0g",new Vector2(0.5f,1),new Vector2(0,-60),new Vector2(340,26),20,Gold,TextAnchor.MiddleCenter,true),
                stats = Wrapped(panel,"DStats",-96,120,Ink,17),
                passive = Wrapped(panel,"DPassive",-224,150,Sub,15),
                build = Wrapped(panel,"DBuild",-384,80,new Color(0.7f,0.82f,0.95f),14),
                flavor = Wrapped(panel,"DFlavor",-470,60,new Color(0.6f,0.62f,0.7f),14),
                banner = banner
            };
            refs.flavor.fontStyle = FontStyle.Italic;

            var btn = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btn.transform.SetParent(panel, false);
            var lrt=(RectTransform)btn.transform; lrt.anchorMin=new Vector2(0.5f,0); lrt.anchorMax=new Vector2(0.5f,0); lrt.pivot=new Vector2(0.5f,0); lrt.anchoredPosition=new Vector2(0,24); lrt.sizeDelta=new Vector2(320,64);
            btn.GetComponent<Image>().color = new Color(0.85f,0.7f,0.3f);
            buyLabel = Label((RectTransform)btn.transform,"Label","BUY",new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(320,64),26,new Color(0.08f,0.08f,0.1f),TextAnchor.MiddleCenter,true);
            buyButton = btn.GetComponent<Button>();
            return panel;
        }

        private static List<StoreItemCard> BuildInventoryRow(RectTransform parent, StoreItemCard cardPrefab)
        {
            var row = MakePanel(parent, "InventoryRow", new Vector2(0.5f,0), new Vector2(0, 200), new Vector2(720, 130));
            row.GetComponent<Image>().color = new Color(0.09f,0.10f,0.15f,0.9f);
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 10; hl.childAlignment = TextAnchor.MiddleCenter; hl.childForceExpandWidth=false; hl.childForceExpandHeight=false;
            hl.padding = new RectOffset(12,12,10,10);
            var cards = new List<StoreItemCard>();
            for (int i = 0; i < 6; i++)
            {
                var card = Object.Instantiate(cardPrefab, row);
                card.gameObject.SetActive(true);
                ((RectTransform)card.transform).sizeDelta = new Vector2(100, 100);
                card.name = i == 5 ? "InvSlot_Boots" : $"InvSlot_{i}";
                cards.Add(card);
            }
            Label(parent,"InvLabel","INVENTORY   (slot 6 = boots)",new Vector2(0.5f,0),new Vector2(0,300),new Vector2(720,24),16,Sub,TextAnchor.MiddleCenter,true);
            return cards;
        }

        // ---- sell / undo / quick-buy ---- //
        private static GameObject BuildLabeledButton(RectTransform parent, string name, string text, Vector2 anchor,
            Vector2 pos, Vector2 size, Color color, out Text label, out Button button)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f,0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            button = go.GetComponent<Button>();
            label = Label(rt, "Label", text, new Vector2(0.5f,0.5f), Vector2.zero, size, 22, Ink, TextAnchor.MiddleCenter, true);
            label.transform.SetAsLastSibling(); // keep text above any fill overlay
            return go;
        }

        private static RectTransform BuildQuickBuyRow(RectTransform parent, out StoreItemCard quickCardPrefab)
        {
            var row = MakePanel(parent, "QuickBuyRow", new Vector2(0,0), new Vector2(40, 210), new Vector2(760, 150));
            row.GetComponent<Image>().color = new Color(0.10f,0.12f,0.18f,0.9f);
            row.pivot = new Vector2(0,0.5f); row.anchorMin = new Vector2(0,0); row.anchorMax = new Vector2(0,0);
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 10; hl.padding = new RectOffset(12,12,12,12);
            hl.childAlignment = TextAnchor.MiddleLeft; hl.childForceExpandWidth = false; hl.childForceExpandHeight = false;

            // A quick-buy card = the item card prefab plus a small BUY button.
            quickCardPrefab = BuildQuickBuyCardPrefab();
            return row;
        }

        private static StoreItemCard BuildQuickBuyCardPrefab()
        {
            var card = BuildCardPrefab();               // reuse the standard card
            var go = card.gameObject;
            ((RectTransform)go.transform).sizeDelta = new Vector2(120, 126);

            // Add a small BUY button at the bottom of the card.
            var btnGo = new GameObject("QuickBuy", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(go.transform, false);
            var brt = (RectTransform)btnGo.transform; brt.anchorMin = new Vector2(0,0); brt.anchorMax = new Vector2(1,0);
            brt.pivot = new Vector2(0.5f,0); brt.anchoredPosition = new Vector2(0,2); brt.sizeDelta = new Vector2(-8,26);
            btnGo.GetComponent<Image>().color = new Color(0.30f,0.55f,0.32f);
            Label(brt, "Label", "BUY", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(110,24), 14, Ink, TextAnchor.MiddleCenter, true);

            var so = new SerializedObject(card);
            so.FindProperty("quickBuyButton").objectReferenceValue = btnGo.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(false);
            return card;
        }

        private static void InjectBuilds(SerializedObject viewSo)
        {
            const string seedPath = "Assets/GameData/Items/build_recommendations.seed.json";
            if (!System.IO.File.Exists(seedPath)) return;
            var root = MiniJSON.Deserialize(System.IO.File.ReadAllText(seedPath)) as System.Collections.Generic.Dictionary<string, object>;
            if (root == null || !root.TryGetValue("builds", out var bo) || bo is not List<object> builds) return;

            var prop = viewSo.FindProperty("championBuilds");
            prop.arraySize = builds.Count;
            for (int i = 0; i < builds.Count; i++)
            {
                var d = (System.Collections.Generic.Dictionary<string, object>)builds[i];
                var el = prop.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("championId").stringValue = d.TryGetValue("championId", out var c) ? c?.ToString() : "";
                el.FindPropertyRelative("role").stringValue = d.TryGetValue("role", out var r) ? r?.ToString() : "";
                var order = d.TryGetValue("order", out var oo) && oo is List<object> ol ? ol : new List<object>();
                var orderProp = el.FindPropertyRelative("order");
                orderProp.arraySize = order.Count;
                for (int j = 0; j < order.Count; j++) orderProp.GetArrayElementAtIndex(j).stringValue = order[j]?.ToString();
            }
        }

        // ---- primitives ---- //
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size;
            go.GetComponent<Image>().color = Panel; return rt;
        }
        private static Image ChildImage(Transform parent, string name, Color c, bool stretch=false, bool raycast=true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>(); img.color = c; img.raycastTarget = raycast;
            if (stretch) { var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; }
            return img;
        }
        private static Text Wrapped(RectTransform parent, string name, float topY, float h, Color c, int fs)
        {
            var t = Label(parent,name,"",new Vector2(0.5f,1),new Vector2(0,topY),new Vector2(336,h),fs,c,TextAnchor.UpperLeft,false);
            t.horizontalOverflow=HorizontalWrapMode.Wrap; t.verticalOverflow=VerticalWrapMode.Truncate; t.supportRichText=true; return t;
        }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size;
            var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            return t;
        }
        private static List<ItemDefinition> LoadAllItems()
        {
            var list = new List<ItemDefinition>();
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/GameData/Items" }))
                list.Add(AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
            list.Sort((a,b)=>string.CompareOrdinal(a.itemId,b.itemId));
            return list;
        }
    }
}
#endif
