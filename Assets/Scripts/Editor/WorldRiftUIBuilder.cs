// Project D  |  Codename: Zantetsuken
// WorldRiftUIBuilder.cs  (Editor-only)
//   Tools > Project D > Build Mode Select
//   Tools > Project D > Build Manifestation HUD Demo
//
// Builds the Mode-Select screen (Ranked/Normal/Co-op/World Rift cards + realm-reveal
// splash) and the in-match Manifestation HUD demo (spawn timer, boss HP, contest bar,
// claim banner, summon-strike telegraph + flash, buff indicator). No external art.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.WorldRiftUI;

namespace ProjectD.EditorTools
{
    public static class WorldRiftUIBuilder
    {
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static readonly Color Idle = new Color(0.16f,0.18f,0.24f);
        private static Font _font;

        // ===================== MODE SELECT =====================
        [MenuItem("Tools/Project D/Build Mode Select", priority = 111)]
        public static void BuildModeSelect()
        {
            _font = Font(); var root = NewScene(out var canvasGo);
            Label(root, "Title", "SELECT MODE", new Vector2(0.5f,1), new Vector2(0,-50), new Vector2(700,50), 36, Ink, TextAnchor.MiddleCenter, true);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Mode cards row.
            var row = MakePanel(root, "Cards", new Vector2(0.5f,0.5f), new Vector2(0,120), new Vector2(1500,300)); row.GetComponent<Image>().color = new Color(0,0,0,0);
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>(); hl.spacing = 24; hl.childAlignment = TextAnchor.MiddleCenter; hl.childForceExpandWidth = false; hl.childControlWidth = false;
            var ranked = ModeCard(row, "RANKED", "Crystalline Rift · LP", new Color(0.30f,0.45f,0.62f));
            var normal = ModeCard(row, "NORMAL", "Crystalline Rift", new Color(0.28f,0.40f,0.52f));
            var coop = ModeCard(row, "CO-OP vs AI", "Crystalline Rift", new Color(0.30f,0.42f,0.40f));
            var worldRift = ModeCard(row, "WORLD RIFT", "Rotating Realm · Casual", new Color(0.55f,0.40f,0.62f));

            var selMode = Label(root, "Sel", "RANKED", new Vector2(0.5f,0.5f), new Vector2(0,-80), new Vector2(600,36), 26, Gold, TextAnchor.MiddleCenter, true);
            var policy = Label(root, "Policy", "", new Vector2(0.5f,0.5f), new Vector2(0,-120), new Vector2(900,26), 18, Sub, TextAnchor.MiddleCenter, false);
            var queueBtn = ActionButton(root, "FIND MATCH", new Vector2(0.5f,0), new Vector2(0,120), new Color(0.30f,0.55f,0.40f), out var queueLabel, 360);

            // Realm reveal splash (World Rift).
            var reveal = MakePanel(root, "RealmReveal", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1920,1080)); reveal.GetComponent<Image>().color = new Color(0,0,0,0.85f);
            var revealGroup = reveal.gameObject.AddComponent<CanvasGroup>();
            var banner = ChildImage(reveal.transform, "Banner", new Color(0.4f,0.3f,0.5f)); var br=(RectTransform)banner.transform; br.anchorMin=new Vector2(0.5f,0.5f); br.anchorMax=new Vector2(0.5f,0.5f); br.pivot=new Vector2(0.5f,0.5f); br.anchoredPosition=new Vector2(0,120); br.sizeDelta=new Vector2(900,240);
            Label(reveal, "RevLabel", "REALM REVEALED", new Vector2(0.5f,0.5f), new Vector2(0,280), new Vector2(700,30), 18, Sub, TextAnchor.MiddleCenter, true);
            var realmName = Label(reveal, "RName", "—", new Vector2(0.5f,0.5f), new Vector2(0,120), new Vector2(900,60), 44, Ink, TextAnchor.MiddleCenter, true);
            var realmMod = Label(reveal, "RMod", "", new Vector2(0.5f,0.5f), new Vector2(0,20), new Vector2(1000,40), 20, Gold, TextAnchor.MiddleCenter, false);
            var realmBoss = Label(reveal, "RBoss", "", new Vector2(0.5f,0.5f), new Vector2(0,-30), new Vector2(700,30), 20, Sub, TextAnchor.MiddleCenter, true);
            var confirm = ActionButton(reveal, "ENTER THE RIFT", new Vector2(0.5f,0.5f), new Vector2(-160,-140), new Color(0.30f,0.55f,0.40f), out _, 300);
            var reroll = ActionButton(reveal, "REROLL REALM", new Vector2(0.5f,0.5f), new Vector2(170,-140), new Color(0.45f,0.40f,0.55f), out _, 280);
            reveal.gameObject.SetActive(false);

            var view = canvasGo.AddComponent<ModeSelectView>();
            var so = new SerializedObject(view);
            so.FindProperty("rankedButton").objectReferenceValue = ranked;
            so.FindProperty("normalButton").objectReferenceValue = normal;
            so.FindProperty("coopButton").objectReferenceValue = coop;
            so.FindProperty("worldRiftButton").objectReferenceValue = worldRift;
            so.FindProperty("selectedModeText").objectReferenceValue = selMode;
            so.FindProperty("mapPolicyText").objectReferenceValue = policy;
            so.FindProperty("queueButtonLabel").objectReferenceValue = queueLabel;
            so.FindProperty("queueButton").objectReferenceValue = queueBtn;
            so.FindProperty("realmRevealPanel").objectReferenceValue = reveal.gameObject;
            so.FindProperty("realmRevealGroup").objectReferenceValue = revealGroup;
            so.FindProperty("realmNameText").objectReferenceValue = realmName;
            so.FindProperty("realmModifierText").objectReferenceValue = realmMod;
            so.FindProperty("realmBossText").objectReferenceValue = realmBoss;
            so.FindProperty("realmBanner").objectReferenceValue = banner;
            so.FindProperty("realmConfirmButton").objectReferenceValue = confirm;
            so.FindProperty("realmRerollButton").objectReferenceValue = reroll;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<ModeSelectDemoDriver>();
            var dso = new SerializedObject(driver); dso.FindProperty("view").objectReferenceValue = view; dso.ApplyModifiedPropertiesWithoutUndo();

            Save("Assets/Scenes/ModeSelect.unity");
            Done("Mode Select built. Ranked/Normal/Co-op always show Crystalline Rift (canonical, no map choice). WORLD RIFT -> ROLL REALM -> realm-reveal splash (name + modifier + Manifestation) -> ENTER THE RIFT. Queue action logs whether Ranked LP applies.");
        }

        // ===================== MANIFESTATION HUD =====================
        [MenuItem("Tools/Project D/Build Manifestation HUD Demo", priority = 112)]
        public static void BuildHud()
        {
            _font = Font(); var root = NewScene(out var canvasGo);
            Label(root, "Note", "IN-MATCH · WORLD RIFT · MANIFESTATION HUD", new Vector2(0.5f,1), new Vector2(0,-30), new Vector2(1000,26), 18, Sub, TextAnchor.MiddleCenter, false);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Spawn timer (top-center).
            var spawn = Label(root, "Spawn", "Manifestation in —", new Vector2(0.5f,1), new Vector2(0,-70), new Vector2(700,28), 20, Gold, TextAnchor.MiddleCenter, true);

            // Boss panel (upper-center): name + HP + contest bar.
            var bossPanel = MakePanel(root, "BossPanel", new Vector2(0.5f,1), new Vector2(0,-160), new Vector2(760,150));
            var bossName = Label(bossPanel, "BName", "Ifrit", new Vector2(0.5f,1), new Vector2(0,-10), new Vector2(700,28), 22, Ink, TextAnchor.MiddleCenter, true);
            var hpBg = MakePanel(bossPanel, "HpBg", new Vector2(0.5f,1), new Vector2(0,-46), new Vector2(700,22)); hpBg.GetComponent<Image>().color = new Color(1,1,1,0.1f);
            var hpFill = Fill(hpBg.transform, new Color(0.85f,0.35f,0.3f), Image.FillMethod.Horizontal, (int)Image.OriginHorizontal.Left);
            // Contest bar: two fills meeting in the middle.
            var contestBg = MakePanel(bossPanel, "ContestBg", new Vector2(0.5f,1), new Vector2(0,-84), new Vector2(700,26)); contestBg.GetComponent<Image>().color = new Color(1,1,1,0.08f);
            var blueFill = Fill(contestBg.transform, new Color(0.4f,0.65f,1f), Image.FillMethod.Horizontal, (int)Image.OriginHorizontal.Left);
            var redFill = Fill(contestBg.transform, new Color(0.9f,0.45f,0.45f), Image.FillMethod.Horizontal, (int)Image.OriginHorizontal.Right);
            var leader = Label(bossPanel, "Leader", "CONTESTED", new Vector2(0.5f,0), new Vector2(0,8), new Vector2(400,22), 15, Sub, TextAnchor.MiddleCenter, true);

            // Buff indicator (top-left).
            var buffPanel = MakePanel(root, "BuffPanel", new Vector2(0,1), new Vector2(180,-70), new Vector2(320,50));
            var buffText = Label(buffPanel, "Buff", "", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(300,30), 16, Ink, TextAnchor.MiddleCenter, true);
            buffPanel.gameObject.SetActive(false);

            // Claim banner (center).
            var claim = MakePanel(root, "ClaimBanner", new Vector2(0.5f,0.5f), new Vector2(0,60), new Vector2(1200,120)); claim.GetComponent<Image>().color = new Color(0,0,0,0.55f);
            var claimGroup = claim.gameObject.AddComponent<CanvasGroup>(); claimGroup.alpha = 0f;
            var claimText = Label(claim, "CT", "BLUE CLAIMS IFRIT!", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1150,70), 48, Ink, TextAnchor.MiddleCenter, true);
            claim.gameObject.SetActive(false);

            // Summon-strike telegraph (center pit) + screen flash.
            var telegraph = new GameObject("Telegraph", typeof(RectTransform), typeof(Image)); telegraph.transform.SetParent(root,false); var tr=(RectTransform)telegraph.transform; tr.anchorMin=new Vector2(0.5f,0.5f); tr.anchorMax=new Vector2(0.5f,0.5f); tr.pivot=new Vector2(0.5f,0.5f); tr.anchoredPosition=new Vector2(0,-40); tr.sizeDelta=new Vector2(20,20); telegraph.GetComponent<Image>().color=new Color(1f,0.6f,0.3f,0.7f); telegraph.SetActive(false);
            var flash = new GameObject("Flash", typeof(RectTransform), typeof(Image)); flash.transform.SetParent(root,false); var fr=(RectTransform)flash.transform; fr.anchorMin=Vector2.zero; fr.anchorMax=Vector2.one; fr.offsetMin=Vector2.zero; fr.offsetMax=Vector2.zero; var fimg=flash.GetComponent<Image>(); fimg.color=new Color(1,1,1,0); fimg.raycastTarget=false;

            var hud = canvasGo.AddComponent<ManifestationHudView>();
            var so = new SerializedObject(hud);
            so.FindProperty("spawnTimerText").objectReferenceValue = spawn;
            so.FindProperty("bossNameText").objectReferenceValue = bossName;
            so.FindProperty("bossPanel").objectReferenceValue = bossPanel.gameObject;
            so.FindProperty("bossHpFill").objectReferenceValue = hpFill;
            so.FindProperty("contestBlueFill").objectReferenceValue = blueFill;
            so.FindProperty("contestRedFill").objectReferenceValue = redFill;
            so.FindProperty("contestLeaderText").objectReferenceValue = leader;
            so.FindProperty("claimBanner").objectReferenceValue = claim.gameObject;
            so.FindProperty("claimGroup").objectReferenceValue = claimGroup;
            so.FindProperty("claimText").objectReferenceValue = claimText;
            so.FindProperty("strikeTelegraph").objectReferenceValue = tr;
            so.FindProperty("screenFlash").objectReferenceValue = fimg;
            so.FindProperty("buffPanel").objectReferenceValue = buffPanel.gameObject;
            so.FindProperty("buffText").objectReferenceValue = buffText;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<ManifestationDemoDriver>();
            var dso = new SerializedObject(driver); dso.FindProperty("hud").objectReferenceValue = hud; dso.FindProperty("manifestationName").stringValue = "Ifrit"; dso.ApplyModifiedPropertiesWithoutUndo();

            Save("Assets/Scenes/ManifestationHudDemo.unity");
            Done("Manifestation HUD built. Watch: spawn timer -> boss spawns -> contest bar shifts as both teams fight -> a team lands the finishing blow -> CLAIM banner + summon-strike telegraph + screen flash -> buff timer -> respawn. Loops.");
        }

        // ---- widgets ----
        private static Button ModeCard(RectTransform parent, string title, string sub, Color c)
        {
            var go = new GameObject("Card_"+title, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent,false); go.GetComponent<Image>().color = Idle; go.GetComponent<LayoutElement>().preferredWidth = 320; go.GetComponent<LayoutElement>().preferredHeight = 260;
            var accent = ChildImage(go.transform, "Accent", c); var ar=(RectTransform)accent.transform; ar.anchorMin=new Vector2(0,1); ar.anchorMax=new Vector2(1,1); ar.pivot=new Vector2(0.5f,1); ar.sizeDelta=new Vector2(0,90); ar.anchoredPosition=Vector2.zero;
            Label((RectTransform)go.transform, "T", title, new Vector2(0.5f,1), new Vector2(0,-30), new Vector2(300,36), 24, Ink, TextAnchor.MiddleCenter, true);
            Label((RectTransform)go.transform, "S", sub, new Vector2(0.5f,0.5f), new Vector2(0,-10), new Vector2(300,40), 16, Sub, TextAnchor.MiddleCenter, false);
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
        private static void Done(string msg)=>EditorUtility.DisplayDialog("Project D", msg, "OK");
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Panel; return rt; }
        private static Image ChildImage(Transform parent, string name, Color c){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); go.GetComponent<Image>().color=c; go.GetComponent<Image>().raycastTarget=false; return go.GetComponent<Image>(); }
        private static Image Fill(Transform parent, Color c, Image.FillMethod method, int origin){ var go=new GameObject("Fill", typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; var img=go.GetComponent<Image>(); img.color=c; img.type=Image.Type.Filled; img.fillMethod=method; img.fillOrigin=origin; img.fillAmount=0f; return img; }
        private static Button ActionButton(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c, out Text label, float w){ var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(w,56); go.GetComponent<Image>().color=c; label=Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(w,56),18,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
        private static Font Font()=>Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
#endif
