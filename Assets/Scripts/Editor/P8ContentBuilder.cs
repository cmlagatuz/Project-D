// Project D  |  Codename: Zantetsuken
// P8ContentBuilder.cs  (Editor-only)  — Tools > Project D > Build Item Build Tree Demo
//
// Builds a runnable item build-tree screen (flagship P8 content UI): the completed item
// node on top, its components in a row below, recipe/upgrade cost + affordability, and a
// buy button. The rune editor + coaching report bind the same way via P8DemoDriver.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.P8UI;

namespace ProjectD.EditorTools
{
    public static class P8ContentBuilder
    {
        private static readonly Color Bg = new Color(0.05f,0.06f,0.10f);
        private static readonly Color Panel = new Color(0.11f,0.13f,0.18f,0.98f);
        private static readonly Color Ink = new Color(0.93f,0.95f,0.99f);
        private static readonly Color Sub = new Color(0.68f,0.73f,0.83f);
        private static readonly Color Gold = new Color(0.95f,0.80f,0.32f);
        private static Font _font;

        [MenuItem("Tools/Project D/Build Item Build Tree Demo", priority = 106)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg; camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }
            var canvasGo = new GameObject("Canvas"); var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = canvasGo.AddComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920,1080); sc.matchWidthOrHeight = 0.5f; canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            Label(root, "Title", "ITEM BUILD TREE", new Vector2(0.5f,1), new Vector2(0,-40), new Vector2(700,44), 30, Ink, TextAnchor.MiddleCenter, true);
            Label(root, "Watermark", "PROJECT D - CONFIDENTIAL", new Vector2(1,1), new Vector2(-20,-16), new Vector2(420,24), 14, new Color(1,1,1,0.3f), TextAnchor.MiddleRight, false);

            // Completed item node (top).
            var completedParent = new GameObject("Completed", typeof(RectTransform)); completedParent.transform.SetParent(root,false);
            var cp=(RectTransform)completedParent.transform; cp.anchorMin=new Vector2(0.5f,0.5f); cp.anchorMax=new Vector2(0.5f,0.5f); cp.pivot=new Vector2(0.5f,0.5f); cp.anchoredPosition=new Vector2(0,180); cp.sizeDelta=new Vector2(300,120);
            var chl=completedParent.AddComponent<HorizontalLayoutGroup>(); chl.childAlignment=TextAnchor.MiddleCenter; chl.childForceExpandWidth=false; chl.childControlWidth=false;

            // Components row (below).
            var componentsParent = new GameObject("Components", typeof(RectTransform)); componentsParent.transform.SetParent(root,false);
            var comp=(RectTransform)componentsParent.transform; comp.anchorMin=new Vector2(0.5f,0.5f); comp.anchorMax=new Vector2(0.5f,0.5f); comp.pivot=new Vector2(0.5f,0.5f); comp.anchoredPosition=new Vector2(0,20); comp.sizeDelta=new Vector2(760,120);
            var comphl=componentsParent.AddComponent<HorizontalLayoutGroup>(); comphl.spacing=20; comphl.childAlignment=TextAnchor.MiddleCenter; comphl.childForceExpandWidth=false; comphl.childControlWidth=false;

            var nodePrefab = BuildNodePrefab();

            var cost = Label(root, "Cost", "", new Vector2(0.5f,0), new Vector2(0,200), new Vector2(900,28), 20, Sub, TextAnchor.MiddleCenter, false);
            var afford = Label(root, "Afford", "Gold: 0", new Vector2(0.5f,0), new Vector2(0,160), new Vector2(400,28), 20, Gold, TextAnchor.MiddleCenter, true);
            var buy = Button_(root, "BUY", new Vector2(0.5f,0), new Vector2(0,90), new Color(0.30f,0.55f,0.40f), out var buyLabel);

            var view = canvasGo.AddComponent<ItemBuildTreeView>();
            var so = new SerializedObject(view);
            so.FindProperty("completedParent").objectReferenceValue = cp;
            so.FindProperty("componentsParent").objectReferenceValue = comp;
            so.FindProperty("nodePrefab").objectReferenceValue = nodePrefab;
            so.FindProperty("costText").objectReferenceValue = cost;
            so.FindProperty("affordText").objectReferenceValue = afford;
            so.FindProperty("buyButton").objectReferenceValue = buy;
            so.FindProperty("buyLabel").objectReferenceValue = buyLabel;
            so.ApplyModifiedPropertiesWithoutUndo();

            var driver = canvasGo.AddComponent<P8DemoDriver>();
            var dso = new SerializedObject(driver); dso.FindProperty("itemTree").objectReferenceValue = view; dso.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/ItemBuildTreeDemo.unity");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D", "Item Build Tree demo built.\nPress Play: the completed item + its components, recipe/upgrade cost, and a BUY button (owned components reduce the upgrade cost).\n(The rune editor + coaching report bind the same way via P8DemoDriver.)", "OK");
        }

        private static RectTransform BuildNodePrefab()
        {
            var go = new GameObject("ItemNode", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.GetComponent<Image>().color = new Color(0.14f,0.16f,0.22f,0.95f); go.GetComponent<LayoutElement>().preferredWidth = 150; go.GetComponent<LayoutElement>().preferredHeight = 100;
            Label((RectTransform)go.transform, "Name", "Item", new Vector2(0.5f,1), new Vector2(0,-12), new Vector2(140,40), 16, Ink, TextAnchor.UpperCenter, true);
            Label((RectTransform)go.transform, "Cost", "0g", new Vector2(0.5f,0), new Vector2(0,10), new Vector2(140,24), 16, Gold, TextAnchor.MiddleCenter, true);
            go.SetActive(false); return (RectTransform)go.transform;
        }

        private static Button Button_(RectTransform parent, string text, Vector2 anchor, Vector2 pos, Color c, out Text label){ var go=new GameObject("Btn_"+text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(260,56); go.GetComponent<Image>().color=c; label=Label(rt,"L",text,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(260,56),20,Ink,TextAnchor.MiddleCenter,true); return go.GetComponent<Button>(); }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold){ var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=anchor; rt.anchorMax=anchor; rt.pivot=new Vector2(anchor.x,anchor.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=a; t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; t.font=_font; return t; }
    }
}
#endif
