// Project D  |  Codename: Zantetsuken
// ChampionSelectSceneBuilder.cs  (Editor-only)  (GCD 5.4 Draft & Team Composition)
//
// Builds the Champion Select / pick-phase screen in code, populated from the
// imported champion assets:
//   - Left: BLUE team pick slots (3).
//   - Center-top: turn banner + step timer.
//   - Center: scrollable roster grid of ChampionCards.
//   - Right: RED team pick slots (3) + a detail panel showing the hovered
//     champion's world, release date, lore, playstyle, strengths/weaknesses,
//     tips, base stats, and full Passive/Q/W/E/Ultimate kit.
//   - Bottom: bans strip + LOCK IN button.
//
//   Tools > Project D > Build Champion Select Scene

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Data;
using ProjectD.UI.ChampionSelect;

namespace ProjectD.EditorTools
{
    public static class ChampionSelectSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/ChampionSelect.unity";
        private static readonly Color Bg = new Color(0.06f, 0.07f, 0.11f);
        private static readonly Color Panel = new Color(0.11f, 0.13f, 0.19f, 0.95f);
        private static readonly Color Blue = new Color(0.35f, 0.6f, 1f);
        private static readonly Color Red = new Color(1f, 0.45f, 0.45f);
        private static readonly Color Ink = new Color(0.92f, 0.94f, 0.98f);
        private static readonly Color Sub = new Color(0.70f, 0.75f, 0.85f);

        [MenuItem("Tools/Project D/Build Champion Select Scene", priority = 41)]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera + EventSystem
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg;
            camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }

            var canvasGo = new GameObject("ChampionSelectCanvas");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var canvas_rt = (RectTransform)canvasGo.transform;

            // Header
            var header = Label(canvas_rt, "Header", "CHAMPION SELECT", new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(800,60), 40, Ink, TextAnchor.MiddleCenter, true);
            var turn = Label(canvas_rt, "TurnBanner", "Blue team to BAN", new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-96), new Vector2(800,44), 28, Blue, TextAnchor.MiddleCenter, true);
            var timer = Label(canvas_rt, "Timer", "30", new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-150), new Vector2(120,50), 34, Ink, TextAnchor.MiddleCenter, true);
            var watermark = Label(canvas_rt, "Watermark", "PROJECT D · CONFIDENTIAL", new Vector2(1,1), new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,30), 18, new Color(1,1,1,0.30f), TextAnchor.MiddleRight, false);

            // Team pick columns
            var (blueSlots, _) = TeamColumn(canvas_rt, "BlueTeam", true);
            var (redSlots, _) = TeamColumn(canvas_rt, "RedTeam", false);

            // Roster grid (center, scrollable)
            var grid = RosterGrid(canvas_rt, out var gridContent, out var cardPrefab);

            // Detail panel (right-center, above red team)
            var detail = DetailPanel(canvas_rt, out var refs);

            // Bans strip + lock-in
            var bans = Label(canvas_rt, "Bans", "", new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0,120), new Vector2(1000,36), 20, Sub, TextAnchor.MiddleCenter, false);
            var lockBtnGo = new GameObject("LockInButton", typeof(RectTransform), typeof(Image), typeof(Button));
            lockBtnGo.transform.SetParent(canvas_rt, false);
            var lrt = (RectTransform)lockBtnGo.transform; lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f,0); lrt.pivot = new Vector2(0.5f,0);
            lrt.anchoredPosition = new Vector2(0,50); lrt.sizeDelta = new Vector2(360,72);
            lockBtnGo.GetComponent<Image>().color = new Color(0.85f,0.7f,0.3f);
            var lockLabel = Label((RectTransform)lockBtnGo.transform, "Label", "LOCK IN", new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(360,72), 30, new Color(0.08f,0.08f,0.10f), TextAnchor.MiddleCenter, true);
            var lockButton = lockBtnGo.GetComponent<Button>();

            // View component + wiring
            var view = canvasGo.AddComponent<ChampionSelectView>();
            var so = new SerializedObject(view);
            so.FindProperty("gridContent").objectReferenceValue = gridContent;
            so.FindProperty("cardPrefab").objectReferenceValue = cardPrefab;
            so.FindProperty("detailName").objectReferenceValue = refs.name;
            so.FindProperty("detailTitleWorld").objectReferenceValue = refs.titleWorld;
            so.FindProperty("detailLore").objectReferenceValue = refs.lore;
            so.FindProperty("detailPlaystyle").objectReferenceValue = refs.playstyle;
            so.FindProperty("detailStrengths").objectReferenceValue = refs.strengths;
            so.FindProperty("detailWeaknesses").objectReferenceValue = refs.weaknesses;
            so.FindProperty("detailTips").objectReferenceValue = refs.tips;
            so.FindProperty("detailStats").objectReferenceValue = refs.stats;
            so.FindProperty("detailAbilities").objectReferenceValue = refs.abilities;
            so.FindProperty("detailRoleBanner").objectReferenceValue = refs.roleBanner;
            so.FindProperty("turnBanner").objectReferenceValue = turn;
            so.FindProperty("timerLabel").objectReferenceValue = timer;
            so.FindProperty("lockInButton").objectReferenceValue = lockButton;
            so.FindProperty("bansLabel").objectReferenceValue = bans;
            var blueArr = so.FindProperty("bluePickLabels"); blueArr.arraySize = 3;
            var redArr = so.FindProperty("redPickLabels"); redArr.arraySize = 3;
            for (int i = 0; i < 3; i++) { blueArr.GetArrayElementAtIndex(i).objectReferenceValue = blueSlots[i]; redArr.GetArrayElementAtIndex(i).objectReferenceValue = redSlots[i]; }

            // Inject champion assets directly so the scene works without Resources setup.
            var champs = LoadAllChampions();
            var injected = so.FindProperty("_injected"); injected.arraySize = champs.Count;
            for (int i = 0; i < champs.Count; i++) injected.GetArrayElementAtIndex(i).objectReferenceValue = champs[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                champs.Count == 0
                    ? "Champion Select scene built, but NO champion assets were found.\nRun Tools > Project D > Import Champion Data first, then rebuild."
                    : $"Champion Select scene built with {champs.Count} champions:\n{ScenePath}\n\nPress Play, hover a card to see full details, then LOCK IN.",
                "OK");
            Debug.Log($"[Project D] Champion Select scene created ({champs.Count} champions).");
        }

        // ---- Layout helpers ---- //
        private static (Text[] slots, RectTransform col) TeamColumn(RectTransform parent, string name, bool isBlue)
        {
            var col = MakePanel(parent, name,
                isBlue ? new Vector2(0,0.5f) : new Vector2(1,0.5f),
                isBlue ? new Vector2(0,0.5f) : new Vector2(1,0.5f),
                new Vector2(isBlue ? 190 : -190, 40), new Vector2(320, 560));
            var title = Label(col, "Title", isBlue ? "BLUE TEAM" : "RED TEAM", new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-20), new Vector2(280,40), 24, isBlue ? Blue : Red, TextAnchor.MiddleCenter, true);
            var slots = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                var slot = MakePanel(col, $"Slot{i}", new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0, -80 - i*150), new Vector2(280,130));
                slot.GetComponent<Image>().color = new Color(isBlue ? 0.2f : 0.3f, 0.2f, isBlue ? 0.35f : 0.2f, 0.6f);
                slots[i] = Label(slot, "Pick", "—", new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(280,60), 22, Ink, TextAnchor.MiddleCenter, true);
            }
            return (slots, col);
        }

        private static RectTransform RosterGrid(RectTransform parent, out RectTransform content, out ChampionCard cardPrefab)
        {
            var scrollGo = MakePanel(parent, "RosterScroll", new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0, 30), new Vector2(760, 520));
            var scroll = scrollGo.gameObject.AddComponent<ScrollRect>();
            var viewportGo = MakePanel(scrollGo, "Viewport", new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(740, 500));
            viewportGo.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var contentGo = new GameObject("Content", typeof(RectTransform)); contentGo.transform.SetParent(viewportGo, false);
            content = (RectTransform)contentGo.transform; content.anchorMin = new Vector2(0,1); content.anchorMax = new Vector2(1,1); content.pivot = new Vector2(0.5f,1); content.anchoredPosition = Vector2.zero; content.sizeDelta = new Vector2(0, 600);
            var grid = contentGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160, 120); grid.spacing = new Vector2(12, 12); grid.padding = new RectOffset(12,12,12,12); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewportGo; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;

            cardPrefab = BuildCardPrefab();
            return scrollGo;
        }

        private static ChampionCard BuildCardPrefab()
        {
            // Build an in-scene template object; Instantiate at runtime clones it.
            var go = new GameObject("ChampionCard", typeof(RectTransform), typeof(Image), typeof(ChampionCard));
            var img = go.GetComponent<Image>(); img.color = new Color(0.16f,0.18f,0.26f);
            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image)); frame.transform.SetParent(go.transform,false);
            StretchFull((RectTransform)frame.transform); var fimg = frame.GetComponent<Image>(); fimg.color = new Color(1,1,1,0.25f); fimg.raycastTarget = false;
            var portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image)); portrait.transform.SetParent(go.transform,false);
            var prt = (RectTransform)portrait.transform; prt.anchorMin=new Vector2(0.5f,1); prt.anchorMax=new Vector2(0.5f,1); prt.pivot=new Vector2(0.5f,1); prt.anchoredPosition=new Vector2(0,-8); prt.sizeDelta=new Vector2(80,60); portrait.GetComponent<Image>().raycastTarget=false;
            var nameL = Label((RectTransform)go.transform, "Name", "Name", new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0,26), new Vector2(150,24), 16, Ink, TextAnchor.MiddleCenter, true); nameL.raycastTarget=false;
            var roleL = Label((RectTransform)go.transform, "Role", "Role", new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0,6), new Vector2(150,20), 13, Sub, TextAnchor.MiddleCenter, false); roleL.raycastTarget=false;
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image)); dim.transform.SetParent(go.transform,false); StretchFull((RectTransform)dim.transform); var dimg = dim.GetComponent<Image>(); dimg.color = new Color(0,0,0,0f); dimg.raycastTarget=false; dim.SetActive(false);

            var card = go.GetComponent<ChampionCard>();
            var so = new SerializedObject(card);
            so.FindProperty("nameLabel").objectReferenceValue = nameL;
            so.FindProperty("roleLabel").objectReferenceValue = roleL;
            so.FindProperty("portrait").objectReferenceValue = portrait.GetComponent<Image>();
            so.FindProperty("frame").objectReferenceValue = fimg;
            so.FindProperty("dimOverlay").objectReferenceValue = dimg;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.SetActive(false); // template stays disabled; clones are enabled on Instantiate
            return card;
        }

        private static RectTransform DetailPanel(RectTransform parent, out DetailRefs refs)
        {
            var panel = MakePanel(parent, "DetailPanel", new Vector2(1,0.5f), new Vector2(1,0.5f), new Vector2(-190, 40), new Vector2(360, 620));
            panel.GetComponent<Image>().color = Panel;
            var banner = new GameObject("RoleBanner", typeof(RectTransform), typeof(Image)); banner.transform.SetParent(panel,false);
            var brt=(RectTransform)banner.transform; brt.anchorMin=new Vector2(0,1); brt.anchorMax=new Vector2(1,1); brt.pivot=new Vector2(0.5f,1); brt.anchoredPosition=Vector2.zero; brt.sizeDelta=new Vector2(0,8);

            refs = new DetailRefs
            {
                name = Label(panel, "DName", "Select a champion", new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-20), new Vector2(340,34), 26, Ink, TextAnchor.MiddleCenter, true),
                titleWorld = Label(panel, "DWorld", "", new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-56), new Vector2(340,24), 15, Sub, TextAnchor.MiddleCenter, false),
                lore = Wrapped(panel, "DLore", -86, 96, Sub, 15),
                playstyle = Wrapped(panel, "DPlay", -188, 88, Ink, 15),
                strengths = Wrapped(panel, "DStr", -282, 42, new Color(0.55f,0.85f,0.6f), 14),
                weaknesses = Wrapped(panel, "DWeak", -328, 42, new Color(0.95f,0.6f,0.55f), 14),
                tips = Wrapped(panel, "DTips", -374, 60, new Color(0.9f,0.85f,0.6f), 14),
                stats = Wrapped(panel, "DStats", -440, 52, Ink, 14),
                abilities = Wrapped(panel, "DAbil", -498, 110, Sub, 13),
                roleBanner = banner.GetComponent<Image>()
            };
            return panel;
        }

        private struct DetailRefs { public Text name, titleWorld, lore, playstyle, strengths, weaknesses, tips, stats, abilities; public Image roleBanner; }

        // ---- Primitive builders ---- //
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.GetComponent<Image>().color = Panel; return rt;
        }

        private static Text Wrapped(RectTransform parent, string name, float topY, float height, Color c, int fs)
        {
            var t = Label(parent, name, "", new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0, topY), new Vector2(336, height), fs, c, TextAnchor.UpperLeft, false);
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate; t.supportRichText = true;
            return t;
        }

        private static Text Label(RectTransform parent, string name, string text, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor anchor, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = new Vector2(0.5f, aMax.y >= 1 ? 1 : (aMax.y <= 0 ? 0 : 0.5f)); rt.anchoredPosition = pos; rt.sizeDelta = size;
            var t = go.GetComponent<Text>(); t.text = text; t.color = c; t.fontSize = fs; t.alignment = anchor; t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            return t;
        }

        private static void StretchFull(RectTransform rt) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }

        private static List<ChampionDefinition> LoadAllChampions()
        {
            var list = new List<ChampionDefinition>();
            foreach (var guid in AssetDatabase.FindAssets("t:ChampionDefinition", new[] { "Assets/GameData" }))
                list.Add(AssetDatabase.LoadAssetAtPath<ChampionDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
            list.Sort((a, b) => string.CompareOrdinal(a.championId, b.championId));
            return list;
        }
    }
}
#endif
