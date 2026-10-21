// Project D  |  Codename: Zantetsuken
// SettingsService.cs  (Settings — load/save/apply + device-aware validation)
//
// The runtime hub the whole game reads. It loads GameSettings + HudLayout from disk,
// validates them against the device (clamps fps to the panel, suggests a preset on
// first run), applies them to Unity (Application.targetFrameRate, QualitySettings,
// AudioMixer, VFX reduce flag, subtitle state), and raises change events so live UI
// (subtitles, HUD, controls) updates immediately. Persisted as JSON in persistentDataPath.

using System;
using UnityEngine;

namespace ProjectD.Settings
{
    public sealed class SettingsService
    {
        private const string SettingsFile = "settings.json";
        private const string LayoutFile = "hud_layout.json";

        public GameSettings Settings { get; private set; }
        public HudLayout Layout { get; private set; }
        public DeviceInfo Device { get; private set; }
        public DeviceTier Tier { get; private set; }

        // Fired after any Apply so live systems refresh (subtitles, HUD scale, audio...).
        public event Action<GameSettings> OnSettingsApplied;
        public event Action<HudLayout> OnLayoutApplied;

        private string SettingsPath => System.IO.Path.Combine(Application.persistentDataPath, SettingsFile);
        private string LayoutPath => System.IO.Path.Combine(Application.persistentDataPath, LayoutFile);

        public void Initialize()
        {
            Device = ProbeDevice();
            Tier = DeviceCapabilities.ClassifyTier(Device);

            Settings = LoadSettingsOrDefaults();
            Layout = LoadLayoutOrDefault();

            ValidateAgainstDevice(Settings);
            Apply();
        }

        // ---- device probe ----
        private static DeviceInfo ProbeDevice()
        {
            int refresh = 60;
#if UNITY_2022_2_OR_NEWER
            var rr = Screen.currentResolution.refreshRateRatio;
            refresh = rr.value > 1 ? Mathf.RoundToInt((float)rr.value) : Screen.currentResolution.refreshRate;
#else
            refresh = Screen.currentResolution.refreshRate;
#endif
            if (refresh <= 0) refresh = 60;

            return new DeviceInfo
            {
                MaxRefreshRateHz = refresh,
                SystemMemoryMB = SystemInfo.systemMemorySize,
                GpuMemoryMB = SystemInfo.graphicsMemorySize,
                ProcessorCount = SystemInfo.processorCount,
                IsMobile = Application.isMobilePlatform
            };
        }

        // ---- first-run recommendation + validation ----
        private GameSettings LoadSettingsOrDefaults()
        {
            if (System.IO.File.Exists(SettingsPath))
            {
                var loaded = MiniSettingsJson.Deserialize(System.IO.File.ReadAllText(SettingsPath));
                if (loaded != null) return loaded;
            }
            // First run: recommend based on the device tier.
            var s = GameSettings.CreateDefault();
            s.graphics.preset = DeviceCapabilities.RecommendedPreset(Tier);
            GraphicsPresets.Apply(s.graphics, s.graphics.preset);
            s.graphics.frameRateCap = (int)DeviceCapabilities.RecommendedFrameRate(Tier, Device.MaxRefreshRateHz);
            return s;
        }

        private HudLayout LoadLayoutOrDefault()
        {
            if (System.IO.File.Exists(LayoutPath))
            {
                try
                {
                    var l = JsonUtility.FromJson<HudLayout>(System.IO.File.ReadAllText(LayoutPath));
                    if (l != null && l.elements != null && l.elements.Count > 0) return l;
                }
                catch { /* fall through to default */ }
            }
            return HudLayout.CreateDefault(HudHandedness.RightHanded);
        }

        /// <summary>Clamp any device-invalid choices (e.g. 144fps on a 60Hz panel).</summary>
        public void ValidateAgainstDevice(GameSettings s)
        {
            var maxFps = (int)DeviceCapabilities.MaxSupported(Device.MaxRefreshRateHz);
            if (s.graphics.frameRateCap > maxFps) s.graphics.frameRateCap = maxFps;
            if (s.graphics.frameRateCap < 30) s.graphics.frameRateCap = 30;
            s.graphics.renderScale = Mathf.Clamp(s.graphics.renderScale, 0.6f, 1.0f);
            GraphicsPresets.RefreshPreset(s.graphics);
        }

        // ---- apply to engine ----
        public void Apply()
        {
            ValidateAgainstDevice(Settings);
            ApplyGraphics(Settings.graphics);
            OnSettingsApplied?.Invoke(Settings);
            OnLayoutApplied?.Invoke(Layout);
        }

        private void ApplyGraphics(GraphicsSettings g)
        {
            // Frame rate: mobile ignores vSync, so drive targetFrameRate directly.
            QualitySettings.vSyncCount = 0;
            int fps = g.batterySaver ? Mathf.Min(g.frameRateCap, 30) : g.frameRateCap;
            Application.targetFrameRate = fps;

            // Map preset -> Unity QualitySettings level if names line up (best-effort).
            int qi = g.preset switch { QualityPreset.Low => 0, QualityPreset.Medium => 1, QualityPreset.High => 2, _ => 1 };
            if (QualitySettings.names != null && qi < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(qi, applyExpensiveChanges: false);

            // Render scale would drive URP asset's renderScale in a full project.
            // (Left as a hook; the settings value is authoritative.)
        }

        // ---- persistence ----
        public void SaveSettings()
        {
            ValidateAgainstDevice(Settings);
            System.IO.File.WriteAllText(SettingsPath, MiniSettingsJson.Serialize(Settings));
        }

        public void SaveLayout()
        {
            System.IO.File.WriteAllText(LayoutPath, JsonUtility.ToJson(Layout, true));
            OnLayoutApplied?.Invoke(Layout);
        }

        public void ResetLayoutToDefault()
        {
            Layout = HudLayout.CreateDefault(Settings.controls.handedness);
            SaveLayout();
        }

        public void ResetAllToRecommended()
        {
            Settings = GameSettings.CreateDefault();
            Settings.graphics.preset = DeviceCapabilities.RecommendedPreset(Tier);
            GraphicsPresets.Apply(Settings.graphics, Settings.graphics.preset);
            Settings.graphics.frameRateCap = (int)DeviceCapabilities.RecommendedFrameRate(Tier, Device.MaxRefreshRateHz);
            Apply(); SaveSettings();
        }
    }
}
