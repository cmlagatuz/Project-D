// Project D  |  Codename: Zantetsuken
// GreyboxSceneBuilder.cs  (Editor-only)  (GCD 6.1 map, 10.x controls, 14.x net, 15.3)
//
// One-click greybox: 3-lane arena, two bases + Crystal Cores, a PrototypePawn, an
// ArenaCameraRig, a touch HUD, and (Sprint 5) a real AuthoritativeMatchServer +
// pawn so casts/movement run the server-authoritative predict/reconcile path.
//
//   Tools > Project D > Build Greybox Prototype Scene

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Core;
using ProjectD.Data;
using ProjectD.Gameplay.Champions;
using ProjectD.Gameplay.InputSystem;
using ProjectD.Gameplay.CameraRig;
using ProjectD.Gameplay.Map;
using ProjectD.UI.HudEditor;
using ProjectD.Networking;

namespace ProjectD.EditorTools
{
    public static class GreyboxSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Greybox_Prototype.unity";

        [MenuItem("Tools/Project D/Build Greybox Prototype Scene", priority = 40)]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildLighting();
            BuildArena();
            BuildBase(Team.Blue, new Vector3(-38, 0, -38));
            BuildBase(Team.Red,  new Vector3( 38, 0,  38));
            var pawn = BuildPawn();
            var rig = BuildCamera(pawn.transform);
            var client = BuildNetworkedServerAndClient(pawn);   // real transport (UDP loopback)
            WirePawnServer(pawn, client, rig);
            BuildTouchHUD(pawn, rig);

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D",
                "Greybox prototype scene built (networked path):\n" + ScenePath +
                "\n\nPress Play. Drag left to move; tap Q/W/E/R to cast.\n" +
                "Console shows CONFIRMED/REJECTED casts from the loopback server.", "OK");
            Debug.Log("[Project D] Greybox scene created at " + ScenePath);
        }

        private static void BuildLighting()
        {
            var l = new GameObject("Directional Light").AddComponent<Light>();
            l.type = LightType.Directional; l.intensity = 1.1f; l.color = new Color(1f, 0.96f, 0.9f);
            l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.45f, 0.47f, 0.55f);
        }

        private static void BuildArena()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena_Ground"; ground.transform.localScale = new Vector3(12, 1, 12);
            ground.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.20f, 0.23f, 0.28f));
            CreateLane("Lane_Top", new Vector3(0, .02f, 30), new Vector3(90,1,6), new Color(.30f,.34f,.42f), 0);
            CreateLane("Lane_Mid", new Vector3(0, .02f, 0),  new Vector3(110,1,6), new Color(.34f,.30f,.42f), 45);
            CreateLane("Lane_Bottom", new Vector3(0, .02f,-30), new Vector3(90,1,6), new Color(.30f,.40f,.36f), 0);
            var river = GameObject.CreatePrimitive(PrimitiveType.Cube);
            river.name = "River"; river.transform.position = new Vector3(0, .01f, 0);
            river.transform.localScale = new Vector3(120, .02f, 10); river.transform.rotation = Quaternion.Euler(0,45,0);
            river.GetComponent<Renderer>().sharedMaterial = Mat(new Color(.25f,.45f,.6f));
            Object.DestroyImmediate(river.GetComponent<BoxCollider>());
            CreatePit("Pit_SummonSanctum", new Vector3(-14,.05f,14), new Color(.7f,.5f,.2f));
            CreatePit("Pit_RiftOfGods", new Vector3(14,.05f,-14), new Color(.6f,.3f,.6f));
            BuildWall(new Vector3(0,1,60), new Vector3(122,4,2)); BuildWall(new Vector3(0,1,-60), new Vector3(122,4,2));
            BuildWall(new Vector3(60,1,0), new Vector3(2,4,122)); BuildWall(new Vector3(-60,1,0), new Vector3(2,4,122));
        }

        private static void CreateLane(string n, Vector3 p, Vector3 s, Color c, float yaw)
        { var l = GameObject.CreatePrimitive(PrimitiveType.Cube); l.name=n; l.transform.position=p; l.transform.localScale=s; l.transform.rotation=Quaternion.Euler(0,yaw,0); l.GetComponent<Renderer>().sharedMaterial=Mat(c); Object.DestroyImmediate(l.GetComponent<BoxCollider>()); }
        private static void CreatePit(string n, Vector3 p, Color c)
        { var pit=GameObject.CreatePrimitive(PrimitiveType.Cylinder); pit.name=n; pit.transform.position=p; pit.transform.localScale=new Vector3(8,.1f,8); pit.GetComponent<Renderer>().sharedMaterial=Mat(c); Object.DestroyImmediate(pit.GetComponent<CapsuleCollider>()); }
        private static void BuildWall(Vector3 p, Vector3 s)
        { var w=GameObject.CreatePrimitive(PrimitiveType.Cube); w.name="Wall"; w.transform.position=p; w.transform.localScale=s; w.GetComponent<Renderer>().sharedMaterial=Mat(new Color(.15f,.16f,.2f)); }

        private static void BuildBase(Team team, Vector3 pos)
        {
            Color c = team == Team.Blue ? new Color(.3f,.55f,.95f) : new Color(.95f,.35f,.35f);
            var core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = $"CrystalCore_{team}"; core.transform.position = pos + Vector3.up * 1.5f;
            core.transform.localScale = new Vector3(3,3,3); core.transform.rotation = Quaternion.Euler(45,45,0);
            core.GetComponent<Renderer>().sharedMaterial = Mat(c); core.AddComponent<CrystalCore>();
            for (int i = -1; i <= 1; i += 2)
            {
                var t = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                t.name = $"Tower_{team}"; t.transform.position = pos + new Vector3(i*6,2,i*6);
                t.transform.localScale = new Vector3(2,4,2); t.GetComponent<Renderer>().sharedMaterial = Mat(c*.8f);
                t.AddComponent<Structure>();
            }
        }

        private static PrototypePawn BuildPawn()
        {
            var go = new GameObject("PrototypePawn"); go.transform.position = new Vector3(-30, 1.1f, -30);
            var cc = go.AddComponent<CharacterController>(); cc.height = 2f; cc.radius = .5f; cc.center = new Vector3(0,1,0);
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); body.name="Body"; body.transform.SetParent(go.transform,false);
            body.transform.localPosition = new Vector3(0,1,0); Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());
            body.GetComponent<Renderer>().sharedMaterial = Mat(new Color(.85f,.8f,.35f));
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube); nose.name="Facing"; nose.transform.SetParent(go.transform,false);
            nose.transform.localPosition = new Vector3(0,1,.6f); nose.transform.localScale = new Vector3(.25f,.25f,.6f);
            Object.DestroyImmediate(nose.GetComponent<BoxCollider>()); nose.GetComponent<Renderer>().sharedMaterial = Mat(Color.white);
            var pawn = go.AddComponent<PrototypePawn>();
            var champ = FindFirstChampion();
            if (champ != null) SetField(pawn, "champion", champ);
            return pawn;
        }

        // Real transport: spawn the authoritative server AND a transport client in
        // the one prototype scene, talking over UDP loopback (127.0.0.1:7777). The
        // pawn drives the NetworkTransportClient, which implements IAuthoritativeServer.
        private static ProjectD.Networking.NetworkTransportClient BuildNetworkedServerAndClient(PrototypePawn pawn)
        {
            var champ = FindFirstChampion();

            var serverGo = new GameObject("AuthoritativeMatchServer");
            var server = serverGo.AddComponent<ProjectD.Networking.Server.AuthoritativeMatchServer>();
            if (champ != null) SetField(server, "defaultChampion", champ);

            var clientGo = new GameObject("NetworkTransportClient");
            var client = clientGo.AddComponent<ProjectD.Networking.NetworkTransportClient>();
            // host/port default to 127.0.0.1:7777, matching the server.

            return client;
        }

        private static void WirePawnServer(PrototypePawn pawn, ProjectD.Networking.NetworkTransportClient client, ArenaCameraRig rig)
        {
            SetField(pawn, "serverBehaviour", client);   // pawn talks to the real transport client
            SetField(pawn, "cameraRig", rig);
        }

        private static ArenaCameraRig BuildCamera(Transform target)
        {
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.backgroundColor = new Color(.05f,.06f,.09f); cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.AddComponent<AudioListener>();
            var rig = camGo.AddComponent<ArenaCameraRig>(); rig.SetTarget(target); return rig;
        }

        private static void BuildTouchHUD(PrototypePawn pawn, ArenaCameraRig rig)
        {
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }

            var canvasGo = new GameObject("TouchHUD"); var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = .5f; canvasGo.AddComponent<GraphicRaycaster>();

            var stickRoot = Panel(canvasGo.transform, "MoveStick", Vector2.zero, Vector2.zero, new Vector2(320,320), new Vector2(220,220));
            var bg = Circle(stickRoot.transform, "Background", 200, new Color(1,1,1,.10f));
            var handle = Circle(bg.transform, "Handle", 90, new Color(1,1,1,.25f));
            var joystick = stickRoot.gameObject.AddComponent<TouchJoystick>();
            SetField(joystick, "background", bg); SetField(joystick, "handle", handle);
            var stickHandle = MakeEditableHandle(stickRoot.gameObject, "MoveStick");

            var abilities = new System.Collections.Generic.List<AbilityButton>
            {
                MakeAbilityButton(canvasGo.transform, AbilitySlot.E, new Vector2(-360,120), new Color(.35f,.6f,.9f), out var eHandle),
                MakeAbilityButton(canvasGo.transform, AbilitySlot.Q, new Vector2(-250,210), new Color(.4f,.75f,.5f), out var qHandle),
                MakeAbilityButton(canvasGo.transform, AbilitySlot.W, new Vector2(-130,250), new Color(.9f,.7f,.35f), out var wHandle),
                MakeAbilityButton(canvasGo.transform, AbilitySlot.Ultimate, new Vector2(-180,110), new Color(.85f,.4f,.85f), out var rHandle),
            };

            var so = new SerializedObject(pawn);
            so.FindProperty("moveStick").objectReferenceValue = joystick;
            so.FindProperty("cameraRig").objectReferenceValue = rig;
            var arr = so.FindProperty("abilityButtons"); arr.arraySize = abilities.Count;
            for (int i = 0; i < abilities.Count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = abilities[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            BuildHudEditUI(canvasGo.transform, new[] { stickHandle, eHandle, qHandle, wHandle, rHandle });

            var wm = Label(canvasGo.transform, "Watermark", "PROJECT D · GREYBOX (NETWORKED) · CONFIDENTIAL",
                new Vector2(.5f,1f), new Vector2(0,-30), 22, new Color(1,1,1,.35f)); wm.alignment = TextAnchor.MiddleCenter;
        }

        private static HudElementHandle MakeEditableHandle(GameObject go, string elementId)
        {
            var h = go.AddComponent<HudElementHandle>();
            var hso = new SerializedObject(h);
            hso.FindProperty("elementId").stringValue = elementId;
            hso.ApplyModifiedPropertiesWithoutUndo();
            return h;
        }

        private static AbilityButton MakeAbilityButton(Transform parent, AbilitySlot slot, Vector2 pos, Color color, out HudElementHandle editHandle)
        {
            var root = Panel(parent, $"Ability_{slot}", new Vector2(1,0), new Vector2(1,0), new Vector2(150,150), pos);
            var face = Circle(root.transform, "Face", 130, color);
            var overlay = Circle(face.transform, "Cooldown", 130, new Color(0,0,0,.55f));
            var oimg = overlay.GetComponent<Image>(); oimg.type = Image.Type.Filled; oimg.fillMethod = Image.FillMethod.Radial360;
            oimg.fillOrigin = (int)Image.Origin360.Top; oimg.fillClockwise = false; oimg.fillAmount = 0f;
            var label = Label(face.transform, "Label", slot == AbilitySlot.Ultimate ? "R" : slot.ToString(), new Vector2(.5f,.5f), Vector2.zero, 44, Color.white);
            label.alignment = TextAnchor.MiddleCenter;
            var ab = root.gameObject.AddComponent<AbilityButton>();
            var so = new SerializedObject(ab);
            so.FindProperty("slot").enumValueIndex = (int)slot;
            so.FindProperty("cooldownOverlay").objectReferenceValue = oimg;
            so.FindProperty("label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
            editHandle = MakeEditableHandle(root.gameObject, $"Ability_{slot}");
            return ab;
        }

        // ---- HUD edit mode (customizable touch controls) ---- //
        private static readonly Color EditPanelBg = new Color(0.09f, 0.10f, 0.15f, 0.96f);
        private static readonly Color EditAccent = new Color(0.95f, 0.80f, 0.32f);

        private static void BuildHudEditUI(Transform canvasParent, HudElementHandle[] handles)
        {
            // Small always-visible toggle, top-right — never blocks the play-field center.
            var toggleGo = LabeledBtn(canvasParent, "EditHudToggle", "\u2699", new Vector2(1,1), new Vector2(-40,-90), new Vector2(72,72),
                new Color(0.16f,0.18f,0.24f), 30, out var toggleBtn, out _);

            // Compact edit drawer along the left edge, hidden until toggled on.
            var panel = Panel(canvasParent, "HudEditPanel", new Vector2(0,0.5f), new Vector2(0,0.5f), new Vector2(360,520), new Vector2(210,0));
            var panelImg = panel.gameObject.AddComponent<Image>(); panelImg.color = EditPanelBg;
            panel.gameObject.SetActive(false);

            Label(panel, "Title", "CUSTOMIZE CONTROLS", new Vector2(0.5f,1), new Vector2(0,-30), 22, Color.white).alignment = TextAnchor.MiddleCenter;
            var selectedLabel = Label(panel, "Selected", "—", new Vector2(0.5f,1), new Vector2(0,-70), 18, EditAccent);
            selectedLabel.alignment = TextAnchor.MiddleCenter;

            Label(panel, "SizeCaption", "Size", new Vector2(0,1), new Vector2(24,-110), 16, new Color(1,1,1,.7f));
            var sizeSlider = MakeSlider(panel, "SizeSlider", new Vector2(0,1), new Vector2(24,-140), new Vector2(312,28), 0.65f, 1.5f, 1f);

            Label(panel, "OpacityCaption", "Opacity", new Vector2(0,1), new Vector2(24,-180), 16, new Color(1,1,1,.7f));
            var opacitySlider = MakeSlider(panel, "OpacitySlider", new Vector2(0,1), new Vector2(24,-210), new Vector2(312,28), 0.2f, 1f, 1f);

            LabeledBtn(panel, "ResetSelected", "Reset", new Vector2(0,1), new Vector2(24,-260), new Vector2(150,52), new Color(0.30f,0.42f,0.60f), 18, out var resetSelBtn, out _);
            LabeledBtn(panel, "ResetAll", "Reset All", new Vector2(1,1), new Vector2(-24,-260), new Vector2(150,52), new Color(0.30f,0.42f,0.60f), 18, out var resetAllBtn, out _);
            LabeledBtn(panel, "Save", "SAVE", new Vector2(0,0), new Vector2(24,60), new Vector2(150,56), new Color(0.32f,0.55f,0.36f), 20, out var saveBtn, out _);
            LabeledBtn(panel, "Quit", "QUIT", new Vector2(1,0), new Vector2(-24,60), new Vector2(150,56), new Color(0.62f,0.30f,0.34f), 20, out var quitBtn, out _);

            var controllerGo = new GameObject("TouchHudEditController");
            controllerGo.transform.SetParent(canvasParent, false);
            var controller = controllerGo.AddComponent<TouchHudEditController>();
            var cso = new SerializedObject(controller);
            var handlesProp = cso.FindProperty("handles"); handlesProp.arraySize = handles.Length;
            for (int i = 0; i < handles.Length; i++) handlesProp.GetArrayElementAtIndex(i).objectReferenceValue = handles[i];
            cso.FindProperty("editToggleButton").objectReferenceValue = toggleBtn;
            cso.FindProperty("editPanelRoot").objectReferenceValue = panel.gameObject;
            cso.FindProperty("selectedLabel").objectReferenceValue = selectedLabel;
            cso.FindProperty("sizeSlider").objectReferenceValue = sizeSlider;
            cso.FindProperty("opacitySlider").objectReferenceValue = opacitySlider;
            cso.FindProperty("resetSelectedButton").objectReferenceValue = resetSelBtn;
            cso.FindProperty("resetAllButton").objectReferenceValue = resetAllBtn;
            cso.FindProperty("saveButton").objectReferenceValue = saveBtn;
            cso.FindProperty("quitButton").objectReferenceValue = quitBtn;
            cso.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject LabeledBtn(Transform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, Color bg, int fontSize, out Button button, out Text label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(.5f,.5f);
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>(); img.color = bg;
            button = go.GetComponent<Button>();
            label = Label(rt, "Label", text, new Vector2(.5f,.5f), Vector2.zero, fontSize, Color.white);
            label.alignment = TextAnchor.MiddleCenter;
            return go;
        }

        private static Slider MakeSlider(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, float min, float max, float value)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0,.5f);
            rt.sizeDelta = size; rt.anchoredPosition = pos;

            var bgGo = new GameObject("Track", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(rt, false); var bgRt = (RectTransform)bgGo.transform;
            bgRt.anchorMin = new Vector2(0,.25f); bgRt.anchorMax = new Vector2(1,.75f); bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            bgGo.GetComponent<Image>().color = new Color(1,1,1,.12f);

            var fillAreaGo = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGo.transform.SetParent(rt, false); var fillAreaRt = (RectTransform)fillAreaGo.transform;
            fillAreaRt.anchorMin = new Vector2(0,.25f); fillAreaRt.anchorMax = new Vector2(1,.75f); fillAreaRt.offsetMin = fillAreaRt.offsetMax = Vector2.zero;

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(fillAreaRt, false); var fillRt = (RectTransform)fillGo.transform;
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(0,1); fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;
            fillGo.GetComponent<Image>().color = EditAccent;

            var handleAreaGo = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaGo.transform.SetParent(rt, false); var handleAreaRt = (RectTransform)handleAreaGo.transform;
            handleAreaRt.anchorMin = new Vector2(0,0); handleAreaRt.anchorMax = new Vector2(1,1); handleAreaRt.offsetMin = handleAreaRt.offsetMax = Vector2.zero;

            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleGo.transform.SetParent(handleAreaRt, false); var handleRt = (RectTransform)handleGo.transform;
            handleRt.sizeDelta = new Vector2(24, size.y); handleGo.GetComponent<Image>().color = Color.white;

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fillRt; slider.handleRect = handleRt; slider.targetGraphic = handleGo.GetComponent<Image>();
            slider.minValue = min; slider.maxValue = max; slider.value = value;
            return slider;
        }

        // ---- UI primitives ---- //
        private static RectTransform Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 size, Vector2 pos)
        { var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); var rt=(RectTransform)go.transform; rt.anchorMin=aMin; rt.anchorMax=aMax; rt.pivot=new Vector2(.5f,.5f); rt.sizeDelta=size; rt.anchoredPosition=pos; return rt; }
        private static RectTransform Circle(Transform parent, string name, float d, Color c)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.sizeDelta=new Vector2(d,d); rt.anchoredPosition=Vector2.zero; var img=go.GetComponent<Image>(); img.color=c; img.sprite=Knob(); return rt; }
        private static Text Label(Transform parent, string name, string text, Vector2 anchor, Vector2 pos, int fs, Color c)
        { var go=new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=rt.anchorMax=anchor; rt.pivot=new Vector2(.5f,.5f); rt.sizeDelta=new Vector2(600,80); rt.anchoredPosition=pos; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.font=Font(); return t; }
        private static Sprite Knob() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        private static Font Font() { var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); return f != null ? f : Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        private static void SetField(Object target, string field, Object value)
        { var so = new SerializedObject(target); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static ChampionDefinition FindFirstChampion()
        { var g = AssetDatabase.FindAssets("t:ChampionDefinition", new[]{"Assets/GameData"}); if (g.Length==0) return null; return AssetDatabase.LoadAssetAtPath<ChampionDefinition>(AssetDatabase.GUIDToAssetPath(g[0])); }
        private static Material Mat(Color c) { var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"); return new Material(sh){ color=c }; }
    }
}
#endif
