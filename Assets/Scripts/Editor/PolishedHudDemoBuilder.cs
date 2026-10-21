// Project D  |  Codename: Zantetsuken
// PolishedHudDemoBuilder.cs  (Editor-only)  — Tools > Project D > Build Polished HUD Demo
//
// Showcases the polish toolkit with zero external art: rounded gradient panels, an
// animated bar, a count-up gold readout, a pulsing low-buff badge with a soft glow,
// minimap ping ripples, and arcing damage numbers. Demonstrates the animation layer
// running in a real scene.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Polish.Art;
using ProjectD.Polish.Anim;

namespace ProjectD.EditorTools
{
    public static class PolishedHudDemoBuilder
    {
        private static Font _font;

        [MenuItem("Tools/Project D/Build Polished HUD Demo", priority = 104)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f,0.06f,0.10f); camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }
            var canvasGo = new GameObject("Canvas"); var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = canvasGo.AddComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920,1080); sc.matchWidthOrHeight = 0.5f; canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            Label(root, "Title", "POLISH TOOLKIT — Procedural Art + Animation", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(1200,44), 30, UIStyle.Ink, TextAnchor.MiddleCenter, true);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Styled panel with a rounded frame + gradient header.
            var panel = MakeImage(root, "Panel", new Vector2(0.5f,0.5f), new Vector2(0,120), new Vector2(680,360)); UIStyle.Panel(panel);
            var header = MakeImage((RectTransform)panel.transform, "Header", new Vector2(0.5f,1), new Vector2(0,-6), new Vector2(660,50));
            header.sprite = ProceduralSprites.VerticalGradient(new Color(0.2f,0.28f,0.42f), new Color(0.12f,0.16f,0.24f)); header.type = Image.Type.Sliced;
            Label((RectTransform)header.transform, "H", "Objective Buffs", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(600,40), 22, UIStyle.Gold, TextAnchor.MiddleCenter, true);

            // Animated bar.
            var barBack = MakeImage((RectTransform)panel.transform, "BarBack", new Vector2(0.5f,0.5f), new Vector2(0,40), new Vector2(560,26)); barBack.sprite = ProceduralSprites.RoundedRect(24,10); barBack.type = Image.Type.Sliced; barBack.color = new Color(1,1,1,0.10f);
            var barFill = MakeImage((RectTransform)barBack.transform, "BarFill", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(560,26)); Stretch((RectTransform)barFill.transform); UIStyle.BarFill(barFill, UIStyle.Accent);
            var animBar = barFill.gameObject.AddComponent<AnimatedBarFill>();
            Set(animBar, "fill", barFill); Set(animBar, "lerpSpeed", 0.6f); animBar.SetTarget(0.85f);

            // Count-up number.
            var countLabel = Label((RectTransform)panel.transform, "Count", "0", new Vector2(0.5f,0.5f), new Vector2(0,-20), new Vector2(400,40), 34, UIStyle.Gold, TextAnchor.MiddleCenter, true);
            var count = countLabel.gameObject.AddComponent<CountUpNumber>();
            Set(count, "label", countLabel); Set(count, "suffix", " g"); count.SetValue(14250, 1.4f);

            // Pulsing low-buff badge + glow.
            var badge = MakeImage((RectTransform)panel.transform, "Badge", new Vector2(0.5f,0), new Vector2(0,40), new Vector2(56,56)); UIStyle.Badge(badge, new Color(0.9f,0.4f,0.3f));
            GlowBehind.Attach((RectTransform)badge.transform, new Color(0.95f,0.5f,0.35f,0.5f), 1.8f);
            var pulse = badge.gameObject.AddComponent<PulseWhenLow>(); Set(pulse, "target", badge); pulse.SetRemaining(3f);
            Label((RectTransform)panel.transform, "BadgeLabel", "Infernal expiring!", new Vector2(0.5f,0), new Vector2(0,10), new Vector2(400,20), 14, UIStyle.Ink, TextAnchor.MiddleCenter, false);

            // Minimap with ping ripples + damage-number layer.
            var minimap = MakeImage(root, "Minimap", new Vector2(1,0), new Vector2(-180,180), new Vector2(300,300)); UIStyle.Panel(minimap); minimap.color = new Color(0.10f,0.18f,0.12f,0.95f);
            var dmgLayerGo = new GameObject("DamageLayer", typeof(RectTransform)); dmgLayerGo.transform.SetParent(root,false);
            var dmgLayer=(RectTransform)dmgLayerGo.transform; dmgLayer.anchorMin=new Vector2(0.5f,0.5f); dmgLayer.anchorMax=new Vector2(0.5f,0.5f); dmgLayer.pivot=new Vector2(0.5f,0.5f); dmgLayer.anchoredPosition=new Vector2(-300,120); dmgLayer.sizeDelta=new Vector2(500,300);
            var dmgPrefab = BuildDamageNumber();

            var driver = canvasGo.AddComponent<PolishDemoDriver>();
            var dso = new SerializedObject(driver);
            dso.FindProperty("minimap").objectReferenceValue = (RectTransform)minimap.transform;
            dso.FindProperty("damageLayer").objectReferenceValue = dmgLayer;
            dso.FindProperty("damagePrefab").objectReferenceValue = dmgPrefab;
            dso.FindProperty("animBar").objectReferenceValue = animBar;
            dso.FindProperty("count").objectReferenceValue = count;
            dso.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/PolishedHudDemo.unity");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D", "Polished HUD demo built.\nPress Play: rounded gradient panel, animated bar, count-up gold, a pulsing low-buff badge with glow, minimap ping ripples, and arcing damage numbers — all procedural, no art files.", "OK");
        }

        private static Text BuildDamageNumber(){ var go=new GameObject("Dmg", typeof(RectTransform), typeof(Text)); ((RectTransform)go.transform).sizeDelta=new Vector2(140,40); var t=go.GetComponent<Text>(); t.text="0"; t.color=Color.white; t.fontSize=26; t.alignment=TextAnchor.MiddleCenter; t.fontStyle=FontStyle.Bold; t.font=_font; t.raycastTarget=false; go.SetActive(false); return t; }
        private static Image MakeImage(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size){ var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; return go.GetComponent<Image>(); }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
        private static void Stretch(RectTransform rt){ rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; }
        private static void Set(Object comp, string field, object value){ var so=new SerializedObject(comp); var p=so.FindProperty(field); if(p==null) return; if(value is Object o) p.objectReferenceValue=o; else if(value is float f) p.floatValue=f; else if(value is int i) p.intValue=i; else if(value is string s) p.stringValue=s; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
#endif
