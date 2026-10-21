// Project D  |  Codename: Zantetsuken
// RankedDemoBuilder.cs  (Editor-only)  — Tools > Project D > Build Ranked Demo
//
// Builds a runnable scene: a rank badge, a status line, Win/Loss/Remake/New-Season
// buttons, and the rank-up screen. Press Play to climb the ladder (Bronze D ->
// Crystal A) and watch promotions/demotions + the rank-up celebration. No art needed.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.RankedUI;

namespace ProjectD.EditorTools
{
    public static class RankedDemoBuilder
    {
        private const string ScenePath = "Assets/Scenes/RankedDemo.unity";
        private static readonly Color Bg = new Color(0.05f, 0.06f, 0.10f);
        private static readonly Color Panel = new Color(0.11f, 0.13f, 0.18f, 0.98f);
        private static readonly Color Ink = new Color(0.93f, 0.95f, 0.99f);
        private static readonly Color Sub = new Color(0.68f, 0.73f, 0.83f);
        private static Font _font;

        [MenuItem("Tools/Project D/Build Ranked Demo", priority = 80)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }

            var canvasGo = new GameObject("RankedCanvas");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            Label(root, "Title", "RANKED", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(600,50), 34, Ink, TextAnchor.MiddleCenter, true);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,28), 18, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Rank badge (center-top)
            var badge = BuildBadge(root, new Vector2(0.5f,0.5f), new Vector2(0, 180));
            var status = Label(root, "Status", "", new Vector2(0.5f,0.5f), new Vector2(0, 40), new Vector2(1200,30), 22, Sub, TextAnchor.MiddleCenter, false);

            // Buttons
            var winB = ActionButton(root, "WIN", new Vector2(0.5f,0.5f), new Vector2(-330,-80), new Color(0.30f,0.55f,0.32f));
            var lossB = ActionButton(root, "LOSS", new Vector2(0.5f,0.5f), new Vector2(-110,-80), new Color(0.62f,0.30f,0.34f));
            var remakeB = ActionButton(root, "REMAKE", new Vector2(0.5f,0.5f), new Vector2(120,-80), new Color(0.35f,0.38f,0.48f));
            var seasonB = ActionButton(root, "NEW SEASON", new Vector2(0.5f,0.5f), new Vector2(360,-80), new Color(0.45f,0.40f,0.62f));

            // Rank-up screen (overlay)
            var rankUp = BuildRankUpScreen(root, out var badge2);

            // Driver
            var driver = canvasGo.AddComponent<RankedDemoDriver>();
            var so = new SerializedObject(driver);
            so.FindProperty("badge").objectReferenceValue = badge.component;
            so.FindProperty("rankUpScreen").objectReferenceValue = rankUp;
            so.FindProperty("statusLabel").objectReferenceValue = status;
            so.FindProperty("winButton").objectReferenceValue = winB;
            so.FindProperty("lossButton").objectReferenceValue = lossB;
            so.FindProperty("remakeButton").objectReferenceValue = remakeB;
            so.FindProperty("newSeasonButton").objectReferenceValue = seasonB;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                $"Ranked demo built:\n{ScenePath}\n\nPress Play. Tap WIN to climb Bronze D -> Crystal A; " +
                "promotions/demotions trigger the rank-up screen (tier-ups get a bigger flourish).", "OK");
            Debug.Log("[Project D] Ranked demo scene created.");
        }

        private struct BadgeRefs { public RankBadge component; }

        private static BadgeRefs BuildBadge(RectTransform parent, Vector2 anchor, Vector2 pos)
        {
            var panel = MakePanel(parent, "RankBadge", anchor, pos, new Vector2(420, 200));
            var frame = ChildImage(panel.transform, "Frame", new Color(1,1,1,0.3f), stretch:true, raycast:false);
            var emblem = ChildImage(panel.transform, "Emblem", new Color(0.8f,0.7f,0.3f), raycast:false);
            var ert = (RectTransform)emblem.transform; ert.anchorMin=new Vector2(0,0.5f); ert.anchorMax=new Vector2(0,0.5f); ert.pivot=new Vector2(0,0.5f); ert.anchoredPosition=new Vector2(24,0); ert.sizeDelta=new Vector2(110,110);
            var tier = Label(panel, "Tier", "Gold A", new Vector2(0,1), new Vector2(150,-28), new Vector2(260,40), 30, Ink, TextAnchor.UpperLeft, true);
            var lp = Label(panel, "LP", "75 / 100 LP", new Vector2(0,1), new Vector2(150,-76), new Vector2(260,28), 20, Sub, TextAnchor.UpperLeft, false);
            var barBack = MakePanel(panel, "LpBack", new Vector2(0,0), new Vector2(150, 34), new Vector2(230, 18)); barBack.pivot=new Vector2(0,0.5f); barBack.anchorMin=new Vector2(0,0); barBack.anchorMax=new Vector2(0,0); barBack.GetComponent<Image>().color=new Color(1,1,1,0.12f);
            var fill = ChildImage(barBack.transform, "Fill", new Color(0.9f,0.75f,0.3f), raycast:false);
            var frt=(RectTransform)fill.transform; frt.anchorMin=Vector2.zero; frt.anchorMax=Vector2.one; frt.offsetMin=Vector2.zero; frt.offsetMax=Vector2.zero;
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin=(int)Image.OriginHorizontal.Left; fill.fillAmount=0.6f;

            var comp = panel.gameObject.AddComponent<RankBadge>();
            var so = new SerializedObject(comp);
            so.FindProperty("frame").objectReferenceValue = frame;
            so.FindProperty("emblem").objectReferenceValue = emblem;
            so.FindProperty("tierText").objectReferenceValue = tier;
            so.FindProperty("lpText").objectReferenceValue = lp;
            so.FindProperty("lpFill").objectReferenceValue = fill;
            so.ApplyModifiedPropertiesWithoutUndo();
            return new BadgeRefs { component = comp };
        }

        private static RankUpScreen BuildRankUpScreen(RectTransform parent, out RankBadge innerBadge)
        {
            var overlay = MakePanel(parent, "RankUpScreen", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1920,1080));
            overlay.GetComponent<Image>().color = new Color(0,0,0,0.82f);
            var group = overlay.gameObject.AddComponent<CanvasGroup>(); group.alpha = 0f;

            var flash = ChildImage(overlay.transform, "TierFlash", new Color(0.9f,0.75f,0.3f,0.18f), stretch:true, raycast:false);
            var head = Label(overlay, "Headline", "PROMOTED!", new Vector2(0.5f,0.5f), new Vector2(0, 260), new Vector2(900,70), 54, Ink, TextAnchor.MiddleCenter, true);
            var from = Label(overlay, "From", "Gold A", new Vector2(0.5f,0.5f), new Vector2(-220, 150), new Vector2(360,44), 30, Sub, TextAnchor.MiddleCenter, true);
            var arrow = Label(overlay, "Arrow", ">", new Vector2(0.5f,0.5f), new Vector2(0, 150), new Vector2(80,44), 34, Ink, TextAnchor.MiddleCenter, true);
            var to = Label(overlay, "To", "Platinum E", new Vector2(0.5f,0.5f), new Vector2(220, 150), new Vector2(360,44), 30, Ink, TextAnchor.MiddleCenter, true);
            var lpDelta = Label(overlay, "LpDelta", "+22 LP", new Vector2(0.5f,0.5f), new Vector2(0, 96), new Vector2(400,36), 26, new Color(0.6f,0.9f,0.6f), TextAnchor.MiddleCenter, true);

            var innerBadgeRefs = BuildBadge(overlay, new Vector2(0.5f,0.5f), new Vector2(0, -40));
            innerBadge = innerBadgeRefs.component;

            var cont = ActionButton(overlay, "CONTINUE", new Vector2(0.5f,0.5f), new Vector2(0, -230), new Color(0.30f,0.45f,0.62f));

            var comp = overlay.gameObject.AddComponent<RankUpScreen>();
            var so = new SerializedObject(comp);
            so.FindProperty("group").objectReferenceValue = group;
            so.FindProperty("headline").objectReferenceValue = head;
            so.FindProperty("fromLabel").objectReferenceValue = from;
            so.FindProperty("toLabel").objectReferenceValue = to;
            so.FindProperty("lpDeltaLabel").objectReferenceValue = lpDelta;
            so.FindProperty("tierFlash").objectReferenceValue = flash;
            so.FindProperty("badge").objectReferenceValue = innerBadge;
            so.FindProperty("continueButton").objectReferenceValue = cont;
            so.ApplyModifiedPropertiesWithoutUndo();
            return comp;
        }

        // ---- primitives ---- //
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.GetComponent<Image>().color = Panel; return rt;
        }
        private static Image ChildImage(Transform parent, string name, Color c, bool stretch=false, bool raycast=true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>(); img.color = c; img.raycastTarget = raycast;
            if (stretch) { var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; }
            return img;
        }
        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c)
        {
            var go = new GameObject("Btn_" + text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(Mathf.Max(180, 40+text.Length*14), 60);
            go.GetComponent<Image>().color = c;
            Label(rt, "L", text, new Vector2(0.5f,0.5f), Vector2.zero, rt.sizeDelta, 22, Ink, TextAnchor.MiddleCenter, true);
            return go.GetComponent<Button>();
        }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x, anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size;
            var t = go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font;
            return t;
        }
    }
}
#endif
