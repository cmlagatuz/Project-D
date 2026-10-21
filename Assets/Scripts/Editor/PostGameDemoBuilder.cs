// Project D  |  Codename: Zantetsuken
// PostGameDemoBuilder.cs  (Editor-only)  — Tools > Project D > Build PostGame Results Demo
//
// Builds a runnable post-game results scene: Victory/Defeat banner, both team
// scoreboards (with headers), the MVP crown + vote, and a rewards panel. Uses the
// PostGameDemoDriver to feed a sample 5v5 result. No art needed.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.PostGameUI;

namespace ProjectD.EditorTools
{
    public static class PostGameDemoBuilder
    {
        private const string ScenePath = "Assets/Scenes/PostGameResultsDemo.unity";
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static Font _font;

        [MenuItem("Tools/Project D/Build PostGame Results Demo", priority = 88)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }

            var canvasGo = new GameObject("PostGameCanvas");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            // Outcome banner.
            var accent = ChildImage(root, "Accent", new Color(0.30f,0.62f,0.92f), raycast:false); var art=(RectTransform)accent.transform; art.anchorMin=new Vector2(0.5f,1); art.anchorMax=new Vector2(0.5f,1); art.pivot=new Vector2(0.5f,1); art.anchoredPosition=new Vector2(0,-20); art.sizeDelta=new Vector2(1760,8);
            var banner = Label(root, "Outcome", "VICTORY", new Vector2(0.5f,1), new Vector2(0,-70), new Vector2(900,70), 56, Ink, TextAnchor.MiddleCenter, true);
            var duration = Label(root, "Duration", "31:15", new Vector2(0.5f,1), new Vector2(0,-130), new Vector2(400,28), 22, Sub, TextAnchor.MiddleCenter, false);
            var mvpName = Label(root, "Mvp", "MVP: You", new Vector2(0.5f,1), new Vector2(0,-162), new Vector2(700,30), 24, Gold, TextAnchor.MiddleCenter, true);
            var voteStatus = Label(root, "VoteStatus", "Vote for your MVP", new Vector2(0.5f,1), new Vector2(0,-192), new Vector2(700,26), 18, Sub, TextAnchor.MiddleCenter, false);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,28), 16, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Team scoreboards.
            var (blueParent, _) = TeamPanel(root, "BLUE TEAM", new Vector2(0.5f,0.5f), new Vector2(-470, -10), new Color(0.4f,0.65f,1f));
            var (redParent, _) = TeamPanel(root, "RED TEAM", new Vector2(0.5f,0.5f), new Vector2(470, -10), new Color(1f,0.45f,0.45f));

            var rowPrefab = BuildRowPrefab();

            // Rewards panel.
            var rewards = MakePanel(root, "Rewards", new Vector2(0.5f,0), new Vector2(0, 130), new Vector2(900, 110));
            var rewardXp = Label(rewards, "Xp", "+312 XP", new Vector2(0,0.5f), new Vector2(220,10), new Vector2(400,36), 28, Gold, TextAnchor.MiddleLeft, true);
            var rewardKp = Label(rewards, "Kp", "+400 KP (First Win)", new Vector2(0,0.5f), new Vector2(220,-24), new Vector2(500,26), 18, new Color(0.6f,0.9f,0.6f), TextAnchor.MiddleLeft, false);
            var xpBack = MakePanel(rewards, "XpBack", new Vector2(0,0.5f), new Vector2(20,0), new Vector2(160,18)); xpBack.pivot=new Vector2(0,0.5f); xpBack.anchorMin=new Vector2(0,0.5f); xpBack.anchorMax=new Vector2(0,0.5f); xpBack.GetComponent<Image>().color=new Color(1,1,1,0.12f);
            var xpFill = ChildImage(xpBack.transform, "Fill", Gold, raycast:false); var xf=(RectTransform)xpFill.transform; xf.anchorMin=Vector2.zero; xf.anchorMax=Vector2.one; xf.offsetMin=Vector2.zero; xf.offsetMax=Vector2.zero; xpFill.type=Image.Type.Filled; xpFill.fillMethod=Image.FillMethod.Horizontal; xpFill.fillOrigin=(int)Image.OriginHorizontal.Left; xpFill.fillAmount=0f;
            Label(rewards, "RewardTitle", "MATCH REWARDS", new Vector2(0,1), new Vector2(20,-8), new Vector2(300,24), 16, Sub, TextAnchor.MiddleLeft, true);

            var cont = ActionButton(root, "CONTINUE", new Vector2(0.5f,0), new Vector2(0, 50), new Color(0.30f,0.45f,0.62f));

            // View + wiring.
            var view = canvasGo.AddComponent<PostGameResultsView>();
            var so = new SerializedObject(view);
            so.FindProperty("outcomeBanner").objectReferenceValue = banner;
            so.FindProperty("outcomeAccent").objectReferenceValue = accent;
            so.FindProperty("durationLabel").objectReferenceValue = duration;
            so.FindProperty("blueRowParent").objectReferenceValue = blueParent;
            so.FindProperty("redRowParent").objectReferenceValue = redParent;
            so.FindProperty("rowPrefab").objectReferenceValue = rowPrefab;
            so.FindProperty("mvpNameLabel").objectReferenceValue = mvpName;
            so.FindProperty("voteStatusLabel").objectReferenceValue = voteStatus;
            so.FindProperty("rewardsPanel").objectReferenceValue = rewards.gameObject;
            so.FindProperty("rewardXpLabel").objectReferenceValue = rewardXp;
            so.FindProperty("rewardKpLabel").objectReferenceValue = rewardKp;
            so.FindProperty("xpBarFill").objectReferenceValue = xpFill;
            so.FindProperty("continueButton").objectReferenceValue = cont;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<PostGameDemoDriver>();
            var dso = new SerializedObject(driver);
            dso.FindProperty("view").objectReferenceValue = view;
            dso.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                $"PostGame results demo built:\n{ScenePath}\n\nPress Play. See VICTORY, both scoreboards (MVP crowned), " +
                "tap VOTE MVP on a player, and view the match rewards. Continue hands XP to account leveling.", "OK");
            Debug.Log("[Project D] PostGame results demo scene created.");
        }

        private static (RectTransform parent, RectTransform panel) TeamPanel(RectTransform root, string title, Vector2 anchor, Vector2 pos, Color titleColor)
        {
            var panel = MakePanel(root, "Team_" + title, anchor, pos, new Vector2(880, 560));
            Label(panel, "Title", title, new Vector2(0,1), new Vector2(20,-14), new Vector2(300,30), 22, titleColor, TextAnchor.MiddleLeft, true);
            // Column headers.
            var hdr = new[] { ("Player", 20f), ("K/D/A", 380f), ("CS", 520f), ("DMG", 620f), ("Gold", 740f) };
            foreach (var (t, x) in hdr) Label(panel, "H_" + t, t, new Vector2(0,1), new Vector2(x, -46), new Vector2(140,24), 15, Sub, TextAnchor.MiddleLeft, true);
            var listGo = new GameObject("Rows", typeof(RectTransform)); listGo.transform.SetParent(panel, false);
            var lrt = (RectTransform)listGo.transform; lrt.anchorMin=new Vector2(0,1); lrt.anchorMax=new Vector2(1,1); lrt.pivot=new Vector2(0.5f,1); lrt.anchoredPosition=new Vector2(0,-78); lrt.sizeDelta=new Vector2(-24,480);
            var vlg = listGo.AddComponent<VerticalLayoutGroup>(); vlg.spacing=6; vlg.padding=new RectOffset(12,12,0,0); vlg.childForceExpandHeight=false; vlg.childControlHeight=false;
            return (lrt, panel);
        }

        private static ScoreboardRow BuildRowPrefab()
        {
            var go = new GameObject("ScoreboardRow", typeof(RectTransform), typeof(Image), typeof(ScoreboardRow), typeof(LayoutElement));
            go.GetComponent<Image>().color = new Color(0.13f,0.15f,0.21f,0.9f);
            go.GetComponent<LayoutElement>().preferredHeight = 62;
            var swatch = ChildImage(go.transform, "Champ", new Color(0.4f,0.4f,0.55f), raycast:false); var srt=(RectTransform)swatch.transform; srt.anchorMin=new Vector2(0,0.5f); srt.anchorMax=new Vector2(0,0.5f); srt.pivot=new Vector2(0,0.5f); srt.anchoredPosition=new Vector2(12,0); srt.sizeDelta=new Vector2(46,46);
            var crown = ChildImage(go.transform, "Crown", Gold, raycast:false); var crt=(RectTransform)crown.transform; crt.anchorMin=new Vector2(0,1); crt.anchorMax=new Vector2(0,1); crt.pivot=new Vector2(0,1); crt.anchoredPosition=new Vector2(10,-2); crt.sizeDelta=new Vector2(20,20); crown.gameObject.SetActive(false);
            var name = Col(go, "Name", 70, 200, 17, TextAnchor.MiddleLeft, true);
            var kda = Col(go, "Kda", 380, 130, 17, TextAnchor.MiddleLeft, false);
            var cs = Col(go, "Cs", 520, 90, 16, TextAnchor.MiddleLeft, false);
            var dmg = Col(go, "Dmg", 620, 100, 16, TextAnchor.MiddleLeft, false);
            var gold = Col(go, "Gold", 740, 90, 16, TextAnchor.MiddleLeft, false);
            var voteCount = Col(go, "VoteCount", 640, 80, 16, TextAnchor.MiddleRight, true); voteCount.color = Gold;

            var voteGo = new GameObject("Vote", typeof(RectTransform), typeof(Image), typeof(Button)); voteGo.transform.SetParent(go.transform, false);
            var vrt=(RectTransform)voteGo.transform; vrt.anchorMin=new Vector2(1,0.5f); vrt.anchorMax=new Vector2(1,0.5f); vrt.pivot=new Vector2(1,0.5f); vrt.anchoredPosition=new Vector2(-10,0); vrt.sizeDelta=new Vector2(110,40);
            voteGo.GetComponent<Image>().color = new Color(0.30f,0.45f,0.62f);
            var voteLabel = Label(vrt, "L", "VOTE MVP", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(110,40), 13, Ink, TextAnchor.MiddleCenter, true);

            var row = go.GetComponent<ScoreboardRow>();
            var so = new SerializedObject(row);
            so.FindProperty("background").objectReferenceValue = go.GetComponent<Image>();
            so.FindProperty("championSwatch").objectReferenceValue = swatch;
            so.FindProperty("nameText").objectReferenceValue = name;
            so.FindProperty("kdaText").objectReferenceValue = kda;
            so.FindProperty("csText").objectReferenceValue = cs;
            so.FindProperty("damageText").objectReferenceValue = dmg;
            so.FindProperty("goldText").objectReferenceValue = gold;
            so.FindProperty("mvpCrown").objectReferenceValue = crown.gameObject;
            so.FindProperty("voteButton").objectReferenceValue = voteGo.GetComponent<Button>();
            so.FindProperty("voteButtonLabel").objectReferenceValue = voteLabel;
            so.FindProperty("voteCountText").objectReferenceValue = voteCount;
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(false);
            return row;
        }

        private static Text Col(GameObject row, string name, float x, float w, int fs, TextAnchor a, bool bold)
        {
            var t = Label((RectTransform)row.transform, name, "", new Vector2(0,0.5f), new Vector2(x, 0), new Vector2(w, 40), fs, Ink, a, bold);
            return t;
        }

        // ---- primitives ----
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Panel; return rt; }
        private static Image ChildImage(Transform parent, string name, Color c, bool stretch=false, bool raycast=true)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var img=go.GetComponent<Image>(); img.color=c; img.raycastTarget=raycast; if(stretch){var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;} return img; }
        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c)
        { var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(300,56); go.GetComponent<Image>().color=c; Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(300,56),20,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
    }
}
#endif
