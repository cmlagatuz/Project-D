// Project D  |  Codename: Zantetsuken
// SettingsMenuBuilder.cs  (Editor-only)  — Tools > Project D > Build Settings Menu
//
// Builds a runnable, mobile-first settings menu scene in code: a left tab rail
// (Graphics / Sound / Accessibility / UI / Controls) and a content area with the
// controls wired to SettingsMenuController. It also drops a SubtitleDisplay and a
// small draggable-HUD demo so the "edit your buttons" flow can be previewed.
//
// Everything is generated with Unity UI (no art), so it runs immediately.

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.SettingsUI;
using ProjectD.Settings;
using ProjectD.Localization;
using ProjectD.LocalizationUI;

namespace ProjectD.EditorTools
{
    public static class SettingsMenuBuilder
    {
        private const string ScenePath = "Assets/Scenes/SettingsMenu.unity";
        private static readonly Color Bg = new Color(0.06f, 0.07f, 0.10f);
        private static readonly Color Panel = new Color(0.11f, 0.13f, 0.18f, 0.98f);
        private static readonly Color Ink = new Color(0.93f, 0.95f, 0.99f);
        private static readonly Color Sub = new Color(0.68f, 0.73f, 0.83f);
        private static readonly Color Accent = new Color(0.36f, 0.62f, 0.92f);
        private static readonly Color Idle = new Color(0.16f, 0.18f, 0.24f);

        private static Font _font;
        private static Canvas _canvas;

        [MenuItem("Tools/Project D/Build Settings Menu", priority = 78)]
        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Bg;
            camGo.AddComponent<AudioListener>();
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            { var es = new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }

            var canvasGo = new GameObject("SettingsCanvas");
            _canvas = canvasGo.AddComponent<Canvas>(); _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasGo.transform;

            Label(root, "Title", "SETTINGS", new Vector2(0.5f,1), new Vector2(0,-34), new Vector2(600,48), 32, Ink, TextAnchor.MiddleCenter, true);
            var deviceInfo = Label(root, "DeviceInfo", "Device: ...", new Vector2(0.5f,1), new Vector2(0,-80), new Vector2(1400,26), 18, Sub, TextAnchor.MiddleCenter, false);

            // Tab rail (left) — pages toggle content panels.
            var pages = new Dictionary<string, GameObject>();
            var rail = MakePanel(root, "TabRail", new Vector2(0,0.5f), new Vector2(150, 0), new Vector2(260, 720));
            var railV = rail.gameObject.AddComponent<VerticalLayoutGroup>(); railV.spacing = 8; railV.padding = new RectOffset(16,16,20,20); railV.childForceExpandHeight = false; railV.childControlHeight = false;

            var content = MakePanel(root, "Content", new Vector2(0.5f,0.5f), new Vector2(150, 0), new Vector2(1300, 760));
            content.GetComponent<Image>().color = Panel;

            var ctrl = canvasGo.AddComponent<SettingsMenuController>();
            var so = new SerializedObject(ctrl);

            // Build each page and its tab.
            pages["Graphics"] = BuildGraphicsPage(content, so, ctrl);
            pages["Sound"] = BuildSoundPage(content, so);
            pages["Accessibility"] = BuildAccessibilityPage(content, so);
            pages["UI"] = BuildUiPage(content, so);
            pages["Controls"] = BuildControlsPage(content, so);

            var tabButtons = new List<Button>();
            foreach (var name in new[] { "Graphics", "Sound", "Accessibility", "UI", "Controls" })
            {
                var b = TabButton(rail, name);
                string key = name; var page = pages[key];
                b.onClick.AddListener(() => { foreach (var kv in pages) kv.Value.SetActive(kv.Key == key); Highlight(tabButtons, b); });
                tabButtons.Add(b);
            }
            // default tab
            foreach (var kv in pages) kv.Value.SetActive(kv.Key == "Graphics");
            Highlight(tabButtons, tabButtons[0]);

            // Footer buttons
            var footer = MakePanel(root, "Footer", new Vector2(0.5f,0), new Vector2(150, 40), new Vector2(1300, 64));
            footer.GetComponent<Image>().color = new Color(0,0,0,0);
            var fh = footer.gameObject.AddComponent<HorizontalLayoutGroup>(); fh.spacing = 16; fh.childAlignment = TextAnchor.MiddleCenter; fh.childForceExpandWidth = false;
            var apply = ActionButton(footer, "APPLY", new Color(0.30f,0.55f,0.32f));
            var cancel = ActionButton(footer, "CANCEL", Idle);
            var reset = ActionButton(footer, "RESET (Recommended)", new Color(0.55f,0.35f,0.30f));

            // Subtitle display (bottom center) so the VO toggle can be seen live.
            var subGo = MakePanel(root, "SubtitleDisplay", new Vector2(0.5f,0), new Vector2(0, 130), new Vector2(1100, 46));
            subGo.GetComponent<Image>().color = new Color(0,0,0,0.5f);
            var subGroup = subGo.gameObject.AddComponent<CanvasGroup>(); subGroup.alpha = 0f;
            var subText = Label(subGo, "SubText", "", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1080,42), 24, Ink, TextAnchor.MiddleCenter, false);
            var subDisplay = subGo.gameObject.AddComponent<SubtitleDisplay>();
            var subSo = new SerializedObject(subDisplay);
            subSo.FindProperty("group").objectReferenceValue = subGroup;
            subSo.FindProperty("label").objectReferenceValue = subText;
            subSo.FindProperty("background").objectReferenceValue = subGo.GetComponent<Image>();
            subSo.FindProperty("panel").objectReferenceValue = subGo;
            subSo.ApplyModifiedPropertiesWithoutUndo();

            so.FindProperty("subtitleDisplay").objectReferenceValue = subDisplay;
            so.FindProperty("applyButton").objectReferenceValue = apply;
            so.FindProperty("cancelButton").objectReferenceValue = cancel;
            so.FindProperty("resetButton").objectReferenceValue = reset;
            so.FindProperty("deviceInfoLabel").objectReferenceValue = deviceInfo;

            // Inject imported StringTable assets so the language dropdowns populate.
            var tables = LoadAll<StringTable>();
            var stProp = so.FindProperty("stringTables"); stProp.arraySize = tables.Count;
            for (int i = 0; i < tables.Count; i++) stProp.GetArrayElementAtIndex(i).objectReferenceValue = tables[i];

            so.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                $"Settings menu built:\n{ScenePath}\n\nPress Play. Frame-rate buttons above your device's refresh rate are disabled. " +
                "Toggle VO subtitles in Accessibility; the change applies on Apply.", "OK");
            Debug.Log("[Project D] Settings menu scene created.");
        }

        // ---------------- pages ---------------- //
        private static GameObject BuildGraphicsPage(RectTransform parent, SerializedObject so, SettingsMenuController ctrl)
        {
            var page = Page(parent, "Graphics");
            float y = -30;
            SectionHeader(page, "QUALITY PRESET", ref y);
            var presets = RowButtons(page, ref y, new[] { "Low", "Medium", "High" });
            var presetVal = InlineValue(page, ref y, "Current preset:");
            SetArray(so, "presetButtons", presets);
            so.FindProperty("presetValueLabel").objectReferenceValue = presetVal;

            SectionHeader(page, "FRAME RATE (device-gated)", ref y);
            var fps = RowButtons(page, ref y, new[] { "30", "60", "90", "120", "144" });
            var fpsHint = InlineValue(page, ref y, "");
            SetArray(so, "frameRateButtons", fps);
            so.FindProperty("frameRateHint").objectReferenceValue = fpsHint;

            SectionHeader(page, "DETAIL", ref y);
            so.FindProperty("antiAliasingToggle").objectReferenceValue = ToggleRow(page, ref y, "Anti-aliasing");
            so.FindProperty("bloomToggle").objectReferenceValue = ToggleRow(page, ref y, "Bloom");
            so.FindProperty("batterySaverToggle").objectReferenceValue = ToggleRow(page, ref y, "Battery saver (caps 30 FPS)");
            so.FindProperty("renderScaleSlider").objectReferenceValue = SliderRow(page, ref y, "Render scale", 0.6f, 1f);
            return page;
        }

        private static GameObject BuildSoundPage(RectTransform parent, SerializedObject so)
        {
            var page = Page(parent, "Sound");
            float y = -30;
            SectionHeader(page, "VOLUME", ref y);
            so.FindProperty("masterMuteToggle").objectReferenceValue = ToggleRow(page, ref y, "Master mute");
            so.FindProperty("masterSlider").objectReferenceValue = SliderRow(page, ref y, "Master", 0, 1);
            so.FindProperty("musicSlider").objectReferenceValue = SliderRow(page, ref y, "Music", 0, 1);
            so.FindProperty("sfxSlider").objectReferenceValue = SliderRow(page, ref y, "SFX", 0, 1);
            so.FindProperty("voiceSlider").objectReferenceValue = SliderRow(page, ref y, "Voice / VO", 0, 1);
            so.FindProperty("ambienceSlider").objectReferenceValue = SliderRow(page, ref y, "Ambience", 0, 1);
            so.FindProperty("duckMusicToggle").objectReferenceValue = ToggleRow(page, ref y, "Lower music during VO");
            return page;
        }

        private static GameObject BuildAccessibilityPage(RectTransform parent, SerializedObject so)
        {
            var page = Page(parent, "Accessibility");
            float y = -30;
            SectionHeader(page, "VO SUBTITLES", ref y);
            so.FindProperty("subtitlesEnabledToggle").objectReferenceValue = ToggleRow(page, ref y, "Subtitles ON");
            var modes = RowButtons(page, ref y, new[] { "Off", "Speaker + Text", "Text Only" });
            SetArray(so, "subtitleModeButtons", modes);
            so.FindProperty("speakerNameToggle").objectReferenceValue = ToggleRow(page, ref y, "Show speaker name");
            so.FindProperty("subtitleScaleSlider").objectReferenceValue = SliderRow(page, ref y, "Subtitle size", 0.8f, 1.6f);
            so.FindProperty("subtitleBgOpacitySlider").objectReferenceValue = SliderRow(page, ref y, "Background opacity", 0, 1);

            SectionHeader(page, "SUBTITLE LANGUAGE", ref y);
            so.FindProperty("subtitleMatchesUiToggle").objectReferenceValue = ToggleRow(page, ref y, "Match UI language");
            so.FindProperty("subtitleLanguageDropdown").objectReferenceValue = LanguageDropdownRow(page, ref y, "Subtitle language", LanguageDropdown.Target.SubtitleLanguage);

            SectionHeader(page, "VISUAL", ref y);
            var filters = RowButtons(page, ref y, new[] { "None", "Protan", "Deutan", "Tritan" });
            SetArray(so, "colorFilterButtons", filters);
            so.FindProperty("reduceShakeToggle").objectReferenceValue = ToggleRow(page, ref y, "Reduce screen shake");
            so.FindProperty("reduceFlashesToggle").objectReferenceValue = ToggleRow(page, ref y, "Reduce flashes");
            so.FindProperty("reduceEffectsToggle").objectReferenceValue = ToggleRow(page, ref y, "Reduce effect density");
            return page;
        }

        private static GameObject BuildUiPage(RectTransform parent, SerializedObject so)
        {
            var page = Page(parent, "UI");
            float y = -30;
            SectionHeader(page, "LANGUAGE", ref y);
            so.FindProperty("uiLanguageDropdown").objectReferenceValue = LanguageDropdownRow(page, ref y, "UI language", LanguageDropdown.Target.UiLanguage);

            SectionHeader(page, "HUD", ref y);
            so.FindProperty("hudScaleSlider").objectReferenceValue = SliderRow(page, ref y, "HUD scale", 0.8f, 1.3f);
            so.FindProperty("hudOpacitySlider").objectReferenceValue = SliderRow(page, ref y, "HUD opacity", 0.5f, 1f);
            so.FindProperty("damageNumbersToggle").objectReferenceValue = ToggleRow(page, ref y, "Show damage numbers");
            so.FindProperty("allyBarsToggle").objectReferenceValue = ToggleRow(page, ref y, "Show ally health bars");
            so.FindProperty("minimapLeftToggle").objectReferenceValue = ToggleRow(page, ref y, "Minimap on left");
            return page;
        }

        private static GameObject BuildControlsPage(RectTransform parent, SerializedObject so)
        {
            var page = Page(parent, "Controls");
            float y = -30;
            SectionHeader(page, "INPUT STYLE", ref y);
            SetArray(so, "movementStyleButtons", RowButtons(page, ref y, new[] { "Floating Stick", "Fixed Stick", "D-Pad" }));
            SetArray(so, "castStyleButtons", RowButtons(page, ref y, new[] { "Tap", "Quick Cast", "Tap + Confirm" }));
            SetArray(so, "handednessButtons", RowButtons(page, ref y, new[] { "Right-handed", "Left-handed" }));

            SectionHeader(page, "TUNING", ref y);
            so.FindProperty("deadZoneSlider").objectReferenceValue = SliderRow(page, ref y, "Stick dead zone", 0, 0.4f);
            so.FindProperty("stickSizeSlider").objectReferenceValue = SliderRow(page, ref y, "Stick size", 0.7f, 1.3f);
            so.FindProperty("hapticsToggle").objectReferenceValue = ToggleRow(page, ref y, "Haptics");
            so.FindProperty("tapToMoveToggle").objectReferenceValue = ToggleRow(page, ref y, "Tap-to-move fallback");

            SectionHeader(page, "HUD LAYOUT", ref y);
            var edit = ActionButtonInPage(page, ref y, "EDIT HUD LAYOUT (drag your buttons)", Accent);
            so.FindProperty("editHudLayoutButton").objectReferenceValue = edit;
            return page;
        }

        // ---------------- widget helpers ---------------- //
        private static GameObject Page(RectTransform parent, string name)
        {
            var go = new GameObject("Page_" + name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = new Vector2(20,20); rt.offsetMax = new Vector2(-20,-20);
            return go;
        }

        private static void SectionHeader(GameObject page, string text, ref float y)
        {
            Label((RectTransform)page.transform, "Sec_" + text, text, new Vector2(0,1), new Vector2(10, y), new Vector2(900,26), 18, Accent, TextAnchor.MiddleLeft, true);
            y -= 40;
        }

        private static Button[] RowButtons(GameObject page, ref float y, string[] labels)
        {
            var row = new GameObject("Row", typeof(RectTransform)); row.transform.SetParent(page.transform, false);
            var rt = (RectTransform)row.transform; rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1); rt.anchoredPosition = new Vector2(10, y); rt.sizeDelta = new Vector2(1200, 46);
            var h = row.AddComponent<HorizontalLayoutGroup>(); h.spacing = 10; h.childForceExpandWidth = false; h.childControlWidth = false; h.childControlHeight = true;
            var outp = new Button[labels.Length];
            for (int i = 0; i < labels.Length; i++) outp[i] = Chip(rt, labels[i]);
            y -= 58;
            return outp;
        }

        private static Button Chip(RectTransform parent, string text)
        {
            var go = new GameObject("Chip_" + text, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = Idle;
            var le = go.GetComponent<LayoutElement>(); le.preferredWidth = Mathf.Max(120, 34 + text.Length * 11); le.preferredHeight = 44;
            Label((RectTransform)go.transform, "L", text, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(le.preferredWidth, 44), 18, Ink, TextAnchor.MiddleCenter, true);
            return go.GetComponent<Button>();
        }

        private static Toggle ToggleRow(GameObject page, ref float y, string text)
        {
            var go = new GameObject("Toggle_" + text, typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(page.transform, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1); rt.anchoredPosition = new Vector2(10, y); rt.sizeDelta = new Vector2(560, 40);
            var box = new GameObject("Box", typeof(RectTransform), typeof(Image)); box.transform.SetParent(go.transform, false);
            var brt = (RectTransform)box.transform; brt.anchorMin = new Vector2(0,0.5f); brt.anchorMax = new Vector2(0,0.5f); brt.pivot = new Vector2(0,0.5f); brt.anchoredPosition = new Vector2(0,0); brt.sizeDelta = new Vector2(34,34);
            box.GetComponent<Image>().color = Idle;
            var check = new GameObject("Check", typeof(RectTransform), typeof(Image)); check.transform.SetParent(box.transform, false);
            var crt = (RectTransform)check.transform; crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one; crt.offsetMin = new Vector2(5,5); crt.offsetMax = new Vector2(-5,-5);
            check.GetComponent<Image>().color = Accent;
            Label((RectTransform)go.transform, "L", text, new Vector2(0,0.5f), new Vector2(46,0), new Vector2(500,34), 18, Ink, TextAnchor.MiddleLeft, false);
            var t = go.GetComponent<Toggle>(); t.targetGraphic = box.GetComponent<Image>(); t.graphic = check.GetComponent<Image>();
            y -= 50;
            return t;
        }

        private static Slider SliderRow(GameObject page, ref float y, string text, float min, float max)
        {
            Label((RectTransform)page.transform, "SL_" + text, text, new Vector2(0,1), new Vector2(10, y), new Vector2(300,30), 18, Sub, TextAnchor.MiddleLeft, false);
            var go = new GameObject("Slider_" + text, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(page.transform, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1); rt.anchoredPosition = new Vector2(320, y - 4); rt.sizeDelta = new Vector2(520, 24);
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image)); bg.transform.SetParent(go.transform, false); Stretch((RectTransform)bg.transform); bg.GetComponent<Image>().color = Idle;
            var fillArea = new GameObject("Fill Area", typeof(RectTransform)); fillArea.transform.SetParent(go.transform, false); Stretch((RectTransform)fillArea.transform);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); fill.transform.SetParent(fillArea.transform, false); Stretch((RectTransform)fill.transform); fill.GetComponent<Image>().color = Accent;
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)); handleArea.transform.SetParent(go.transform, false); Stretch((RectTransform)handleArea.transform);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image)); handle.transform.SetParent(handleArea.transform, false); ((RectTransform)handle.transform).sizeDelta = new Vector2(20,28); handle.GetComponent<Image>().color = Ink;
            var s = go.GetComponent<Slider>(); s.fillRect = (RectTransform)fill.transform; s.handleRect = (RectTransform)handle.transform; s.targetGraphic = handle.GetComponent<Image>(); s.minValue = min; s.maxValue = max; s.value = (min+max)/2;
            y -= 50;
            return s;
        }

        // Builds a labeled Unity Dropdown + a LanguageDropdown component (populated at runtime).
        private static LanguageDropdown LanguageDropdownRow(GameObject page, ref float y, string label, LanguageDropdown.Target target)
        {
            Label((RectTransform)page.transform, "LDL_" + label, label, new Vector2(0,1), new Vector2(10, y), new Vector2(300,30), 18, Sub, TextAnchor.MiddleLeft, false);

            var go = new GameObject("Dropdown_" + label, typeof(RectTransform), typeof(Image), typeof(Dropdown));
            go.transform.SetParent(page.transform, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1);
            rt.anchoredPosition = new Vector2(320, y); rt.sizeDelta = new Vector2(420, 40);
            go.GetComponent<Image>().color = Idle;

            var caption = Label(rt, "Label", "English", new Vector2(0,0.5f), new Vector2(14,0), new Vector2(360,30), 18, Ink, TextAnchor.MiddleLeft, false);
            var arrow = Label(rt, "Arrow", "v", new Vector2(1,0.5f), new Vector2(-16,0), new Vector2(24,30), 18, Sub, TextAnchor.MiddleRight, true);

            // Minimal template so the dropdown can open in Play mode.
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(CanvasGroup));
            template.transform.SetParent(go.transform, false);
            var trt = (RectTransform)template.transform; trt.anchorMin = new Vector2(0,0); trt.anchorMax = new Vector2(1,0); trt.pivot = new Vector2(0.5f,1); trt.anchoredPosition = new Vector2(0,0); trt.sizeDelta = new Vector2(0, 240);
            template.GetComponent<Image>().color = Panel;
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask)); viewport.transform.SetParent(template.transform, false);
            var vrt = (RectTransform)viewport.transform; vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one; vrt.offsetMin = Vector2.zero; vrt.offsetMax = Vector2.zero;
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            var contentGo = new GameObject("Content", typeof(RectTransform)); contentGo.transform.SetParent(viewport.transform, false);
            var crt = (RectTransform)contentGo.transform; crt.anchorMin = new Vector2(0,1); crt.anchorMax = new Vector2(1,1); crt.pivot = new Vector2(0.5f,1); crt.sizeDelta = new Vector2(0, 40);
            var itemGo = new GameObject("Item", typeof(RectTransform), typeof(Toggle)); itemGo.transform.SetParent(contentGo.transform, false);
            var irt = (RectTransform)itemGo.transform; irt.anchorMin = new Vector2(0,0.5f); irt.anchorMax = new Vector2(1,0.5f); irt.sizeDelta = new Vector2(0,36);
            var itemBg = new GameObject("Item Background", typeof(RectTransform), typeof(Image)); itemBg.transform.SetParent(itemGo.transform, false); Stretch((RectTransform)itemBg.transform); itemBg.GetComponent<Image>().color = new Color(0.16f,0.18f,0.24f);
            var itemLabel = Label((RectTransform)itemGo.transform, "Item Label", "Option", new Vector2(0,0.5f), new Vector2(14,0), new Vector2(360,30), 18, Ink, TextAnchor.MiddleLeft, false);
            itemGo.GetComponent<Toggle>().targetGraphic = itemBg.GetComponent<Image>();
            var sr = template.GetComponent<ScrollRect>(); sr.content = crt; sr.viewport = vrt; sr.horizontal = false;
            template.SetActive(false);

            var dd = go.GetComponent<Dropdown>();
            dd.template = trt; dd.captionText = caption; dd.itemText = itemLabel; dd.targetGraphic = go.GetComponent<Image>();

            var comp = go.AddComponent<LanguageDropdown>();
            var so = new SerializedObject(comp);
            so.FindProperty("dropdown").objectReferenceValue = dd;
            so.FindProperty("target").enumValueIndex = (int)target;
            so.ApplyModifiedPropertiesWithoutUndo();

            y -= 56;
            return comp;
        }

        private static Text InlineValue(GameObject page, ref float y, string prefix)
        {
            var t = Label((RectTransform)page.transform, "IV", prefix, new Vector2(0,1), new Vector2(10, y), new Vector2(900,26), 16, Sub, TextAnchor.MiddleLeft, false);
            y -= 34;
            return t;
        }

        private static Button ActionButtonInPage(GameObject page, ref float y, string text, Color c)
        {
            var go = new GameObject("Act_" + text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(page.transform, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1); rt.anchoredPosition = new Vector2(10, y); rt.sizeDelta = new Vector2(560, 52);
            go.GetComponent<Image>().color = c;
            Label((RectTransform)go.transform, "L", text, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(560,52), 18, Ink, TextAnchor.MiddleCenter, true);
            y -= 64;
            return go.GetComponent<Button>();
        }

        private static Button TabButton(RectTransform rail, string text)
        {
            var go = new GameObject("Tab_" + text, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(rail, false);
            go.GetComponent<Image>().color = Idle; go.GetComponent<LayoutElement>().preferredHeight = 56;
            Label((RectTransform)go.transform, "L", text, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(228,56), 20, Ink, TextAnchor.MiddleCenter, true);
            return go.GetComponent<Button>();
        }

        private static Button ActionButton(RectTransform footer, string text, Color c)
        {
            var go = new GameObject("Btn_" + text, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(footer, false);
            go.GetComponent<Image>().color = c; var le = go.GetComponent<LayoutElement>(); le.preferredWidth = Mathf.Max(180, 40 + text.Length * 12); le.preferredHeight = 52;
            Label((RectTransform)go.transform, "L", text, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(le.preferredWidth, 52), 18, Ink, TextAnchor.MiddleCenter, true);
            return go.GetComponent<Button>();
        }

        private static void SetArray(SerializedObject so, string prop, Button[] buttons)
        {
            var p = so.FindProperty(prop); p.arraySize = buttons.Length;
            for (int i = 0; i < buttons.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Highlight(List<Button> group, Button active)
        {
            foreach (var b in group) { var img = b.GetComponent<Image>(); if (img) img.color = (b == active) ? Accent : Idle; }
        }

        // ---------------- primitives ---------------- //
        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.GetComponent<Image>().color = Panel; return rt;
        }
        private static Text Label(RectTransform parent, string name, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fs, Color c, TextAnchor a, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(anchor.x, anchor.y); rt.anchoredPosition = pos; rt.sizeDelta = size;
            var t = go.GetComponent<Text>(); t.text = text; t.color = c; t.fontSize = fs; t.alignment = a; t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; t.font = _font;
            return t;
        }
        private static void Stretch(RectTransform rt) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }

        private static List<T> LoadAll<T>() where T : Object
        {
            var list = new List<T>();
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { "Assets/GameData/Localization" }))
                list.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
            return list;
        }
    }
}
#endif
