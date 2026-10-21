// Project D  |  Codename: Zantetsuken
// MatchHudBuilder.cs  (Editor-only)  — Tools > Project D > Build Match HUD Scene
//
// Builds the combined in-match HUD: a match clock (top-center), an objective/buff bar
// (top-left), a vision/ward tracker with a minimap (bottom-right), an emote wheel
// (bottom-left), a surrender panel (center overlay), a damage-number layer (center),
// and demo buttons to simulate server events. Wires it all to MatchHudController.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.P5UI;

namespace ProjectD.EditorTools
{
    public static class MatchHudBuilder
    {
        private static readonly Color Bg = new Color(0.06f,0.09f,0.07f);   // faux "map" green tint
        private static readonly Color Panel = new Color(0.10f,0.12f,0.16f,0.85f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static readonly Color Idle = new Color(0.16f,0.18f,0.24f);
        private static Font _font;

        [MenuItem("Tools/Project D/Build Match HUD Scene", priority = 103)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }
            var canvasGo = new GameObject("HUDCanvas"); var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = canvasGo.AddComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920,1080); sc.matchWidthOrHeight = 0.5f; canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Match clock (top-center).
            var clockPanel = MakePanel(root, "Clock", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(180,60));
            var clock = Label(clockPanel, "T", "00:00", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(180,60), 34, Gold, TextAnchor.MiddleCenter, true);

            // Objective / buff bar (top-left).
            var buffPanel = MakePanel(root, "Buffs", new Vector2(0,1), new Vector2(200,-40), new Vector2(380,64)); buffPanel.pivot=new Vector2(0.5f,1);
            Label(buffPanel, "H", "OBJECTIVES", new Vector2(0,1), new Vector2(10,-2), new Vector2(200,16), 12, Sub, TextAnchor.MiddleLeft, true);
            var buffBarGo = new GameObject("Bar", typeof(RectTransform)); buffBarGo.transform.SetParent(buffPanel,false);
            var barRt=(RectTransform)buffBarGo.transform; barRt.anchorMin=new Vector2(0,0); barRt.anchorMax=new Vector2(1,0); barRt.pivot=new Vector2(0,0); barRt.anchoredPosition=new Vector2(8,8); barRt.sizeDelta=new Vector2(-16,44);
            var hlg=buffBarGo.AddComponent<HorizontalLayoutGroup>(); hlg.spacing=6; hlg.childForceExpandWidth=false; hlg.childControlWidth=false;
            var buffChip = BuildBuffChip();

            // Vision tracker + minimap (bottom-right).
            var visionPanel = MakePanel(root, "Vision", new Vector2(1,0), new Vector2(-190,180), new Vector2(340,340)); visionPanel.pivot=new Vector2(1,0);
            Label(visionPanel, "VT", "VISION", new Vector2(0,1), new Vector2(12,-10), new Vector2(200,22), 16, Gold, TextAnchor.MiddleLeft, true);
            var stealthTxt = Label(visionPanel, "S", "Stealth 0/3", new Vector2(0,1), new Vector2(12,-38), new Vector2(200,20), 15, Sub, TextAnchor.MiddleLeft, false);
            var controlTxt = Label(visionPanel, "C", "Control 0/1", new Vector2(1,1), new Vector2(-12,-38), new Vector2(200,20), 15, Sub, TextAnchor.MiddleRight, false);
            var minimap = MakePanel(visionPanel, "Minimap", new Vector2(0.5f,0), new Vector2(0,20), new Vector2(280,240)); minimap.GetComponent<Image>().color=new Color(0.10f,0.18f,0.12f,0.95f);
            var wardDot = BuildWardDot();

            // Emote wheel (bottom-left).
            var emotePanel = MakePanel(root, "Emotes", new Vector2(0,0), new Vector2(190,180), new Vector2(320,320)); emotePanel.pivot=new Vector2(0,0); emotePanel.GetComponent<Image>().color=new Color(0,0,0,0.25f);
            var emoteFeedback = Label(root, "EF", "", new Vector2(0,0), new Vector2(40,40), new Vector2(400,24), 16, Sub, TextAnchor.MiddleLeft, false);
            var emoteSlot = BuildEmoteSlot();

            // Surrender panel (center overlay).
            var surrender = MakePanel(root, "Surrender", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(560,260)); surrender.GetComponent<Image>().color=new Color(0,0,0,0.9f);
            var sGroup = surrender.gameObject.AddComponent<CanvasGroup>(); sGroup.alpha=0f;
            var sPrompt = Label(surrender, "P", "Surrender the match?", new Vector2(0.5f,1), new Vector2(0,-30), new Vector2(520,40), 26, Ink, TextAnchor.MiddleCenter, true);
            var sTally = Label(surrender, "Ta", "Yes 0 / 4 needed", new Vector2(0.5f,0.5f), new Vector2(0,10), new Vector2(520,28), 20, Gold, TextAnchor.MiddleCenter, true);
            var sTimer = Label(surrender, "Ti", "45s", new Vector2(0.5f,0.5f), new Vector2(0,-24), new Vector2(200,24), 18, Sub, TextAnchor.MiddleCenter, false);
            var sYes = ActionButton(surrender, "YES", new Vector2(0.5f,0), new Vector2(-120,40), new Color(0.30f,0.55f,0.40f), out _);
            var sNo = ActionButton(surrender, "NO", new Vector2(0.5f,0), new Vector2(120,40), new Color(0.62f,0.35f,0.35f), out _);

            // Damage-number layer (fills center; non-blocking).
            var dmgLayerGo = new GameObject("DamageLayer", typeof(RectTransform)); dmgLayerGo.transform.SetParent(root,false);
            var dmgLayer=(RectTransform)dmgLayerGo.transform; dmgLayer.anchorMin=new Vector2(0.5f,0.5f); dmgLayer.anchorMax=new Vector2(0.5f,0.5f); dmgLayer.pivot=new Vector2(0.5f,0.5f); dmgLayer.anchoredPosition=new Vector2(0,80); dmgLayer.sizeDelta=new Vector2(600,300);
            var dmgPrefab = BuildDamageNumber();

            // Demo buttons (bottom-center).
            var demoBar = MakePanel(root, "Demo", new Vector2(0.5f,0), new Vector2(0,40), new Vector2(1200,56)); demoBar.GetComponent<Image>().color=new Color(0,0,0,0);
            var dh=demoBar.gameObject.AddComponent<HorizontalLayoutGroup>(); dh.spacing=12; dh.childForceExpandWidth=true; dh.childForceExpandHeight=true;
            var grantBuff = Chip(demoBar, "GRANT BUFF");
            var placeWard = Chip(demoBar, "PLACE WARD");
            var sweep = Chip(demoBar, "SWEEP WARDS");
            var spawnDmg = Chip(demoBar, "SPAWN DAMAGE");
            var callSurr = Chip(demoBar, "CALL SURRENDER");

            // Controller wiring.
            var hud = canvasGo.AddComponent<MatchHudController>();
            var so = new SerializedObject(hud);
            so.FindProperty("clockText").objectReferenceValue = clock;
            so.FindProperty("buffBar").objectReferenceValue = barRt;
            so.FindProperty("buffChipPrefab").objectReferenceValue = buffChip;
            so.FindProperty("stealthCountText").objectReferenceValue = stealthTxt;
            so.FindProperty("controlCountText").objectReferenceValue = controlTxt;
            so.FindProperty("minimap").objectReferenceValue = minimap;
            so.FindProperty("wardDotPrefab").objectReferenceValue = wardDot;
            so.FindProperty("emoteWheelRoot").objectReferenceValue = emotePanel;
            so.FindProperty("emoteSlotPrefab").objectReferenceValue = emoteSlot;
            so.FindProperty("emoteFeedback").objectReferenceValue = emoteFeedback;
            so.FindProperty("surrenderGroup").objectReferenceValue = sGroup;
            so.FindProperty("surrenderPrompt").objectReferenceValue = sPrompt;
            so.FindProperty("surrenderTally").objectReferenceValue = sTally;
            so.FindProperty("surrenderTimer").objectReferenceValue = sTimer;
            so.FindProperty("surrenderYes").objectReferenceValue = sYes;
            so.FindProperty("surrenderNo").objectReferenceValue = sNo;
            so.FindProperty("damageLayer").objectReferenceValue = dmgLayer;
            so.FindProperty("damageNumberPrefab").objectReferenceValue = dmgPrefab;
            so.FindProperty("grantBuffButton").objectReferenceValue = grantBuff;
            so.FindProperty("placeWardButton").objectReferenceValue = placeWard;
            so.FindProperty("spawnDamageButton").objectReferenceValue = spawnDmg;
            so.FindProperty("callSurrenderButton").objectReferenceValue = callSurr;
            so.FindProperty("sweepButton").objectReferenceValue = sweep;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/MatchHudScene.unity");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D",
                "Match HUD scene built.\nPress Play: the clock runs, buffs tick down, wards plot on the minimap, the emote wheel + damage numbers fire, and CALL SURRENDER opens the vote.\nDemo buttons simulate the events the server would push.", "OK");
        }

        // ---- prefabs ----
        private static RectTransform BuildBuffChip()
        {
            var go=new GameObject("BuffChip", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.GetComponent<Image>().color=new Color(0.16f,0.18f,0.24f); go.GetComponent<LayoutElement>().preferredWidth=84; go.GetComponent<LayoutElement>().preferredHeight=44;
            var icon=new GameObject("Icon", typeof(RectTransform), typeof(Image)); icon.transform.SetParent(go.transform,false); var ir=(RectTransform)icon.transform; ir.anchorMin=new Vector2(0,0.5f); ir.anchorMax=new Vector2(0,0.5f); ir.pivot=new Vector2(0,0.5f); ir.anchoredPosition=new Vector2(6,0); ir.sizeDelta=new Vector2(20,20); icon.GetComponent<Image>().color=new Color(0.9f,0.7f,0.3f);
            Label((RectTransform)go.transform,"Name","Buff",new Vector2(0,1),new Vector2(30,-4),new Vector2(60,18),12,Ink,TextAnchor.MiddleLeft,true);
            Label((RectTransform)go.transform,"Timer","0s",new Vector2(0,0),new Vector2(30,4),new Vector2(60,16),11,Sub,TextAnchor.MiddleLeft,false);
            go.SetActive(false); return (RectTransform)go.transform;
        }
        private static Image BuildWardDot(){ var go=new GameObject("Ward", typeof(RectTransform), typeof(Image)); var rt=(RectTransform)go.transform; rt.sizeDelta=new Vector2(12,12); go.GetComponent<Image>().color=new Color(0.5f,0.85f,0.6f); go.SetActive(false); return go.GetComponent<Image>(); }
        private static Button BuildEmoteSlot(){ var go=new GameObject("Emote", typeof(RectTransform), typeof(Image), typeof(Button)); var rt=(RectTransform)go.transform; rt.sizeDelta=new Vector2(84,50); go.GetComponent<Image>().color=new Color(0.20f,0.24f,0.34f); Label(rt,"L","emote",new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(84,50),13,Ink,TextAnchor.MiddleCenter,true); go.SetActive(false); return go.GetComponent<Button>(); }
        private static Text BuildDamageNumber(){ var go=new GameObject("Dmg", typeof(RectTransform), typeof(Text)); var rt=(RectTransform)go.transform; rt.sizeDelta=new Vector2(120,40); var t=go.GetComponent<Text>(); t.text="0"; t.color=Ink; t.fontSize=24; t.alignment=TextAnchor.MiddleCenter; t.fontStyle=FontStyle.Bold; t.font=_font; t.raycastTarget=false; go.SetActive(false); return t; }

        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Panel; return rt; }
        private static Button Chip(RectTransform parent, string text){ var go=new GameObject("Chip_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); go.GetComponent<Image>().color=Idle; Label((RectTransform)go.transform,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(200,44),16,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c, out Text label){ var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(180,50); go.GetComponent<Image>().color=c; label=Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(180,50),18,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
    }
}
#endif
