// Project D  |  Codename: Zantetsuken
// SettingsMenuController.cs  (Settings UI — the in-game settings menu binder)
//
// Wires the settings menu tabs to the model and applies changes live:
//   Graphics: quality preset (Low/Medium/High/Custom) + device-gated frame-rate
//             cap (30/60/90/120/144) + per-option toggles + battery saver.
//   Sound:    master/music/sfx/voice/ambience sliders + mutes.
//   Accessibility: VO SUBTITLE toggle + mode + scale + speaker name + language,
//             color filter, reduce shake/flashes/effects.
//   UI:       HUD scale/opacity, damage numbers, ally bars, minimap side.
//   Controls: input styles, dead zone, stick size, haptics + "Edit HUD Layout".
//
// Uses a working copy so Cancel reverts. Frame-rate buttons that exceed the device's
// panel refresh are disabled with a "device max" hint.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Settings;
using ProjectD.Localization;
using ProjectD.LocalizationUI;

namespace ProjectD.SettingsUI
{
    public sealed class SettingsMenuController : MonoBehaviour
    {
        [Header("Service")]
        [SerializeField] private SubtitleDisplay subtitleDisplay;  // updated live on apply
        [SerializeField] private HudLayoutEditor hudEditor;

        [Header("Graphics")]
        [SerializeField] private Button[] presetButtons;           // Low, Medium, High
        [SerializeField] private Text presetValueLabel;            // shows Custom when edited
        [SerializeField] private Button[] frameRateButtons;        // 30,60,90,120,144 (in order)
        [SerializeField] private Text frameRateHint;
        [SerializeField] private Toggle antiAliasingToggle;
        [SerializeField] private Toggle bloomToggle;
        [SerializeField] private Toggle batterySaverToggle;
        [SerializeField] private Slider renderScaleSlider;

        [Header("Sound")]
        [SerializeField] private Toggle masterMuteToggle;
        [SerializeField] private Slider masterSlider, musicSlider, sfxSlider, voiceSlider, ambienceSlider;
        [SerializeField] private Toggle duckMusicToggle;

        [Header("Accessibility - VO subtitles")]
        [SerializeField] private Toggle subtitlesEnabledToggle;    // the requested toggle
        [SerializeField] private Button[] subtitleModeButtons;     // Off, Speaker+Text, TextOnly
        [SerializeField] private Toggle speakerNameToggle;
        [SerializeField] private Slider subtitleScaleSlider;
        [SerializeField] private Slider subtitleBgOpacitySlider;
        [SerializeField] private Button[] languageButtons;         // optional
        [Header("Accessibility - other")]
        [SerializeField] private Button[] colorFilterButtons;      // None, Prot, Deut, Trit
        [SerializeField] private Toggle reduceShakeToggle, reduceFlashesToggle, reduceEffectsToggle;

        [Header("UI")]
        [SerializeField] private Slider hudScaleSlider, hudOpacitySlider;
        [SerializeField] private Toggle damageNumbersToggle, allyBarsToggle, minimapLeftToggle;

        [Header("Localization (language dropdowns)")]
        [SerializeField] private LanguageDropdown uiLanguageDropdown;
        [SerializeField] private LanguageDropdown subtitleLanguageDropdown;
        [SerializeField] private Toggle subtitleMatchesUiToggle;
        [SerializeField] private System.Collections.Generic.List<StringTable> stringTables = new();

        [Header("Controls")]
        [SerializeField] private Button[] movementStyleButtons;    // Floating, Fixed, DPad
        [SerializeField] private Button[] castStyleButtons;        // Tap, QuickCast, TapConfirm
        [SerializeField] private Button[] handednessButtons;       // Right, Left
        [SerializeField] private Slider deadZoneSlider, stickSizeSlider;
        [SerializeField] private Toggle hapticsToggle, tapToMoveToggle;
        [SerializeField] private Button editHudLayoutButton;

        [Header("Footer")]
        [SerializeField] private Button applyButton, cancelButton, resetButton;
        [SerializeField] private Text deviceInfoLabel;

        private readonly SettingsService _service = new();
        private readonly LocalizationService _loc = new();
        private GameSettings _work;   // working copy

        private void Start()
        {
            _service.Initialize();

            // Localization: load tables, pick first-run language (account/device), bind UI.
            _loc.RegisterFromStringTables(stringTables);
            _loc.Initialize(_service.Settings);
            LocalizedText.Bind(_loc.UI);   // all LocalizedText follow the UI language

            _work = _service.Settings.Clone();

            // Live-apply subtitle display + HUD from the applied settings.
            _service.OnSettingsApplied += s => { if (subtitleDisplay) subtitleDisplay.ApplySettings(s); };
            _service.OnLayoutApplied += l => { if (hudEditor) hudEditor.ApplyLayout(l); };
            if (hudEditor) hudEditor.Bind(_service);
            if (subtitleDisplay) subtitleDisplay.ApplySettings(_service.Settings);

            HookFooter();
            HookGraphics();
            HookSound();
            HookAccessibility();
            HookUi();
            HookControls();
            HookLocalization();

            ShowDeviceInfo();
            RefreshAll();
        }

        // ---------------- GRAPHICS ---------------- //
        private void HookGraphics()
        {
            for (int i = 0; i < presetButtons.Length; i++)
            {
                var preset = (QualityPreset)i; // 0 Low,1 Med,2 High
                if (presetButtons[i]) presetButtons[i].onClick.AddListener(() =>
                {
                    GraphicsPresets.Apply(_work.graphics, preset);
                    RefreshGraphics();
                });
            }

            // Frame-rate buttons, gated by device capability.
            var order = new[] { FrameRateCap.FPS30, FrameRateCap.FPS60, FrameRateCap.FPS90, FrameRateCap.FPS120, FrameRateCap.FPS144 };
            var supported = DeviceCapabilities.SupportedFrameRates(_service.Device.MaxRefreshRateHz);
            var supportedSet = new HashSet<FrameRateCap>(supported);
            for (int i = 0; i < frameRateButtons.Length && i < order.Length; i++)
            {
                var cap = order[i];
                bool ok = supportedSet.Contains(cap);
                if (frameRateButtons[i])
                {
                    frameRateButtons[i].interactable = ok;
                    frameRateButtons[i].onClick.AddListener(() => { _work.graphics.frameRateCap = (int)cap; RefreshGraphics(); });
                }
            }
            if (frameRateHint) frameRateHint.text = $"Device max: {(int)DeviceCapabilities.MaxSupported(_service.Device.MaxRefreshRateHz)} FPS";

            if (antiAliasingToggle) antiAliasingToggle.onValueChanged.AddListener(v => { _work.graphics.antiAliasing = v; GraphicsPresets.RefreshPreset(_work.graphics); RefreshGraphics(); });
            if (bloomToggle) bloomToggle.onValueChanged.AddListener(v => { _work.graphics.bloom = v; GraphicsPresets.RefreshPreset(_work.graphics); RefreshGraphics(); });
            if (batterySaverToggle) batterySaverToggle.onValueChanged.AddListener(v => _work.graphics.batterySaver = v);
            if (renderScaleSlider) renderScaleSlider.onValueChanged.AddListener(v => { _work.graphics.renderScale = Mathf.Clamp(v, 0.6f, 1f); GraphicsPresets.RefreshPreset(_work.graphics); RefreshGraphics(); });
        }

        private void RefreshGraphics()
        {
            GraphicsPresets.RefreshPreset(_work.graphics);
            if (presetValueLabel) presetValueLabel.text = _work.graphics.preset.ToString();
            HighlightPresetButtons();
            HighlightFrameRateButtons();
            if (antiAliasingToggle) antiAliasingToggle.SetIsOnWithoutNotify(_work.graphics.antiAliasing);
            if (bloomToggle) bloomToggle.SetIsOnWithoutNotify(_work.graphics.bloom);
            if (batterySaverToggle) batterySaverToggle.SetIsOnWithoutNotify(_work.graphics.batterySaver);
            if (renderScaleSlider) renderScaleSlider.SetValueWithoutNotify(_work.graphics.renderScale);
        }

        private void HighlightPresetButtons()
        {
            for (int i = 0; i < presetButtons.Length; i++)
                Highlight(presetButtons[i], (int)_work.graphics.preset == i);
        }
        private void HighlightFrameRateButtons()
        {
            var order = new[] { 30, 60, 90, 120, 144 };
            for (int i = 0; i < frameRateButtons.Length && i < order.Length; i++)
                Highlight(frameRateButtons[i], _work.graphics.frameRateCap == order[i]);
        }

        // ---------------- SOUND ---------------- //
        private void HookSound()
        {
            if (masterMuteToggle) masterMuteToggle.onValueChanged.AddListener(v => _work.sound.masterMute = v);
            Bind(masterSlider, v => _work.sound.masterVolume = v);
            Bind(musicSlider, v => _work.sound.musicVolume = v);
            Bind(sfxSlider, v => _work.sound.sfxVolume = v);
            Bind(voiceSlider, v => _work.sound.voiceVolume = v);
            Bind(ambienceSlider, v => _work.sound.ambienceVolume = v);
            if (duckMusicToggle) duckMusicToggle.onValueChanged.AddListener(v => _work.sound.duckMusicOnVoice = v);
        }

        // ---------------- ACCESSIBILITY (VO subtitles) ---------------- //
        private void HookAccessibility()
        {
            if (subtitlesEnabledToggle) subtitlesEnabledToggle.onValueChanged.AddListener(v =>
            {
                _work.accessibility.subtitlesEnabled = v;
                if (v && _work.accessibility.subtitleMode == SubtitleMode.Off)
                    _work.accessibility.subtitleMode = SubtitleMode.SpeakerAndText;
                RefreshAccessibility();
            });

            for (int i = 0; i < subtitleModeButtons.Length; i++)
            {
                var mode = (SubtitleMode)i; // 0 Off,1 Speaker+Text,2 TextOnly
                if (subtitleModeButtons[i]) subtitleModeButtons[i].onClick.AddListener(() =>
                {
                    _work.accessibility.subtitleMode = mode;
                    _work.accessibility.subtitlesEnabled = mode != SubtitleMode.Off;
                    RefreshAccessibility();
                });
            }
            if (speakerNameToggle) speakerNameToggle.onValueChanged.AddListener(v => _work.accessibility.subtitleSpeakerName = v);
            Bind(subtitleScaleSlider, v => _work.accessibility.subtitleTextScale = Mathf.Clamp(v, 0.8f, 1.6f));
            Bind(subtitleBgOpacitySlider, v => _work.accessibility.subtitleBackgroundOpacity = v);

            for (int i = 0; i < colorFilterButtons.Length; i++)
            {
                var cf = (ColorFilter)i;
                if (colorFilterButtons[i]) colorFilterButtons[i].onClick.AddListener(() => { _work.accessibility.colorFilter = cf; RefreshAccessibility(); });
            }
            if (reduceShakeToggle) reduceShakeToggle.onValueChanged.AddListener(v => _work.accessibility.reduceScreenShake = v);
            if (reduceFlashesToggle) reduceFlashesToggle.onValueChanged.AddListener(v => _work.accessibility.reduceFlashes = v);
            if (reduceEffectsToggle) reduceEffectsToggle.onValueChanged.AddListener(v => _work.accessibility.reduceEffects = v);
        }

        private void RefreshAccessibility()
        {
            var a = _work.accessibility;
            if (subtitlesEnabledToggle) subtitlesEnabledToggle.SetIsOnWithoutNotify(a.subtitlesEnabled && a.subtitleMode != SubtitleMode.Off);
            for (int i = 0; i < subtitleModeButtons.Length; i++) Highlight(subtitleModeButtons[i], (int)a.subtitleMode == i);
            for (int i = 0; i < colorFilterButtons.Length; i++) Highlight(colorFilterButtons[i], (int)a.colorFilter == i);
            if (speakerNameToggle) speakerNameToggle.SetIsOnWithoutNotify(a.subtitleSpeakerName);
            if (subtitleScaleSlider) subtitleScaleSlider.SetValueWithoutNotify(a.subtitleTextScale);
            if (subtitleBgOpacitySlider) subtitleBgOpacitySlider.SetValueWithoutNotify(a.subtitleBackgroundOpacity);
            if (reduceShakeToggle) reduceShakeToggle.SetIsOnWithoutNotify(a.reduceScreenShake);
            if (reduceFlashesToggle) reduceFlashesToggle.SetIsOnWithoutNotify(a.reduceFlashes);
            if (reduceEffectsToggle) reduceEffectsToggle.SetIsOnWithoutNotify(a.reduceEffects);
        }

        // ---------------- UI ---------------- //
        private void HookUi()
        {
            Bind(hudScaleSlider, v => _work.ui.hudScale = Mathf.Clamp(v, 0.8f, 1.3f));
            Bind(hudOpacitySlider, v => _work.ui.hudOpacity = Mathf.Clamp(v, 0.5f, 1f));
            if (damageNumbersToggle) damageNumbersToggle.onValueChanged.AddListener(v => _work.ui.showDamageNumbers = v);
            if (allyBarsToggle) allyBarsToggle.onValueChanged.AddListener(v => _work.ui.showAllyHealthBars = v);
            if (minimapLeftToggle) minimapLeftToggle.onValueChanged.AddListener(v => _work.ui.minimapOnLeft = v);
        }

        // ---------------- LOCALIZATION (language dropdowns) ---------------- //
        private void HookLocalization()
        {
            // UI language: changing it applies live to all LocalizedText, and (if the
            // "match UI" option is on) re-points the subtitle language to match.
            if (uiLanguageDropdown)
                uiLanguageDropdown.Bind(_loc, _work.ui.uiLanguage, code =>
                {
                    _work.ui.uiLanguage = code;
                    if (_work.ui.subtitleMatchesUi)
                    {
                        _work.accessibility.subtitleLanguage = code;
                        _loc.ApplySubtitleLanguage(code);
                        if (subtitleLanguageDropdown) subtitleLanguageDropdown.SetSelected(code);
                    }
                });

            // Subtitle language: independent unless "match UI" is on.
            if (subtitleLanguageDropdown)
                subtitleLanguageDropdown.Bind(_loc, _work.accessibility.subtitleLanguage, code =>
                {
                    _work.accessibility.subtitleLanguage = code;
                    _work.ui.subtitleMatchesUi = (code == _work.ui.uiLanguage);
                    if (subtitleMatchesUiToggle) subtitleMatchesUiToggle.SetIsOnWithoutNotify(_work.ui.subtitleMatchesUi);
                });

            if (subtitleMatchesUiToggle)
                subtitleMatchesUiToggle.onValueChanged.AddListener(v =>
                {
                    _work.ui.subtitleMatchesUi = v;
                    if (v)
                    {
                        _work.accessibility.subtitleLanguage = _work.ui.uiLanguage;
                        _loc.ApplySubtitleLanguage(_work.ui.uiLanguage);
                        if (subtitleLanguageDropdown) subtitleLanguageDropdown.SetSelected(_work.ui.uiLanguage);
                    }
                });
        }

        // ---------------- CONTROLS ---------------- //
        private void HookControls()
        {
            for (int i = 0; i < movementStyleButtons.Length; i++)
            { var st = (MovementControlStyle)i; if (movementStyleButtons[i]) movementStyleButtons[i].onClick.AddListener(() => { _work.controls.movementStyle = st; RefreshControls(); }); }
            for (int i = 0; i < castStyleButtons.Length; i++)
            { var st = (AbilityCastStyle)i; if (castStyleButtons[i]) castStyleButtons[i].onClick.AddListener(() => { _work.controls.castStyle = st; RefreshControls(); }); }
            for (int i = 0; i < handednessButtons.Length; i++)
            { var h = (HudHandedness)i; if (handednessButtons[i]) handednessButtons[i].onClick.AddListener(() => { _work.controls.handedness = h; RefreshControls(); }); }
            Bind(deadZoneSlider, v => _work.controls.stickDeadZone = Mathf.Clamp(v, 0f, 0.4f));
            Bind(stickSizeSlider, v => _work.controls.stickSize = Mathf.Clamp(v, 0.7f, 1.3f));
            if (hapticsToggle) hapticsToggle.onValueChanged.AddListener(v => _work.controls.enableHaptics = v);
            if (tapToMoveToggle) tapToMoveToggle.onValueChanged.AddListener(v => _work.controls.tapToMoveFallback = v);
            if (editHudLayoutButton) editHudLayoutButton.onClick.AddListener(() => hudEditor?.EnterEditMode());
        }

        private void RefreshControls()
        {
            for (int i = 0; i < movementStyleButtons.Length; i++) Highlight(movementStyleButtons[i], (int)_work.controls.movementStyle == i);
            for (int i = 0; i < castStyleButtons.Length; i++) Highlight(castStyleButtons[i], (int)_work.controls.castStyle == i);
            for (int i = 0; i < handednessButtons.Length; i++) Highlight(handednessButtons[i], (int)_work.controls.handedness == i);
            if (deadZoneSlider) deadZoneSlider.SetValueWithoutNotify(_work.controls.stickDeadZone);
            if (stickSizeSlider) stickSizeSlider.SetValueWithoutNotify(_work.controls.stickSize);
            if (hapticsToggle) hapticsToggle.SetIsOnWithoutNotify(_work.controls.enableHaptics);
            if (tapToMoveToggle) tapToMoveToggle.SetIsOnWithoutNotify(_work.controls.tapToMoveFallback);
        }

        // ---------------- FOOTER ---------------- //
        private void HookFooter()
        {
            if (applyButton) applyButton.onClick.AddListener(ApplyAndSave);
            if (cancelButton) cancelButton.onClick.AddListener(Cancel);
            if (resetButton) resetButton.onClick.AddListener(ResetToRecommended);
        }

        private void ApplyAndSave()
        {
            _service.Settings.graphics = _work.graphics;
            _service.Settings.sound = _work.sound;
            _service.Settings.accessibility = _work.accessibility;
            _service.Settings.ui = _work.ui;
            _service.Settings.controls = _work.controls;
            _service.Apply();       // clamps, applies to engine, raises events (subtitles/HUD refresh)
            _service.SaveSettings();
            // Commit the chosen languages (UI + subtitle) and sync to the account.
            _loc.ApplyUiLanguage(_service.Settings.ui.uiLanguage);
            _loc.ApplySubtitleLanguage(_service.Settings.accessibility.subtitleLanguage);
            _work = _service.Settings.Clone();
            RefreshAll();
        }

        private void Cancel()
        {
            _work = _service.Settings.Clone();
            RefreshAll();
        }

        private void ResetToRecommended()
        {
            _service.ResetAllToRecommended();
            _work = _service.Settings.Clone();
            if (subtitleDisplay) subtitleDisplay.ApplySettings(_service.Settings);
            RefreshAll();
        }

        // ---------------- shared ---------------- //
        private void RefreshAll()
        {
            RefreshGraphics(); RefreshAccessibility(); RefreshControls();
            if (masterMuteToggle) masterMuteToggle.SetIsOnWithoutNotify(_work.sound.masterMute);
            SetSlider(masterSlider, _work.sound.masterVolume); SetSlider(musicSlider, _work.sound.musicVolume);
            SetSlider(sfxSlider, _work.sound.sfxVolume); SetSlider(voiceSlider, _work.sound.voiceVolume);
            SetSlider(ambienceSlider, _work.sound.ambienceVolume);
            if (duckMusicToggle) duckMusicToggle.SetIsOnWithoutNotify(_work.sound.duckMusicOnVoice);
            SetSlider(hudScaleSlider, _work.ui.hudScale); SetSlider(hudOpacitySlider, _work.ui.hudOpacity);
            if (damageNumbersToggle) damageNumbersToggle.SetIsOnWithoutNotify(_work.ui.showDamageNumbers);
            if (allyBarsToggle) allyBarsToggle.SetIsOnWithoutNotify(_work.ui.showAllyHealthBars);
            if (minimapLeftToggle) minimapLeftToggle.SetIsOnWithoutNotify(_work.ui.minimapOnLeft);
        }

        private void ShowDeviceInfo()
        {
            var d = _service.Device;
            if (deviceInfoLabel)
                deviceInfoLabel.text = $"Device: {(d.IsMobile ? "Mobile" : "PC")} - {_service.Tier} tier - " +
                                       $"{d.MaxRefreshRateHz}Hz - {d.SystemMemoryMB}MB RAM - {d.ProcessorCount} cores";
        }

        private static void Bind(Slider s, System.Action<float> set) { if (s) s.onValueChanged.AddListener(v => set(v)); }
        private static void SetSlider(Slider s, float v) { if (s) s.SetValueWithoutNotify(v); }
        private static void Highlight(Button b, bool on)
        {
            if (!b) return;
            var img = b.GetComponent<Image>();
            if (img) img.color = on ? new Color(0.36f, 0.62f, 0.92f) : new Color(0.16f, 0.18f, 0.24f);
        }
    }
}
