// Project D  |  Codename: Zantetsuken
// GameSettings.cs  (Settings — the serializable settings model)
//
// The full in-game settings for the mobile build: Graphics, Sound, Accessibility
// (incl. VO subtitles), UI (HUD scale/opacity), and Controls (input styles +
// handedness). Plain serializable data (no UnityEngine types beyond primitives)
// so it can be saved to JSON, validated, unit-tested, and mirrored per-account.
//
// The custom UI-controller LAYOUT (button positions) is stored separately in
// HudLayout so it can be edited/reset independently.

using System;

namespace ProjectD.Settings
{
    [Serializable]
    public sealed class GraphicsSettings
    {
        public QualityPreset preset = QualityPreset.Medium;
        public int frameRateCap = 60;               // stored as int fps for easy JSON
        public GraphicsLevel shadows = GraphicsLevel.Medium;
        public GraphicsLevel textures = GraphicsLevel.Medium;
        public GraphicsLevel effects = GraphicsLevel.Medium;   // particle/VFX density
        public GraphicsLevel modelDetail = GraphicsLevel.Medium;
        public bool antiAliasing = true;
        public bool bloom = true;
        public float renderScale = 1.0f;            // 0.6..1.0 dynamic resolution floor
        public bool batterySaver = false;           // caps fps + lowers effects when on
    }

    [Serializable]
    public sealed class SoundSettings
    {
        public bool masterMute = false;
        public float masterVolume = 1.0f;   // 0..1
        public float musicVolume = 0.7f;
        public float sfxVolume = 1.0f;
        public float voiceVolume = 1.0f;    // champion VO / announcer
        public float ambienceVolume = 0.6f;
        public bool muteWhenBackgrounded = true;
        public bool duckMusicOnVoice = true; // lower music while a VO line plays
    }

    [Serializable]
    public sealed class AccessibilitySettings
    {
        // ---- VO subtitles (the requested toggle) ----
        public SubtitleMode subtitleMode = SubtitleMode.SpeakerAndText;
        public bool subtitlesEnabled = true;         // master on/off convenience
        public float subtitleTextScale = 1.0f;       // 0.8..1.6
        public float subtitleBackgroundOpacity = 0.6f;
        public bool subtitleSpeakerName = true;      // show "Kain:" prefix
        public string subtitleLanguage = "en";       // ISO code

        // ---- other accessibility ----
        public ColorFilter colorFilter = ColorFilter.None;
        public bool reduceScreenShake = false;
        public bool reduceFlashes = false;
        public bool reduceEffects = false;           // ties into the VFX reduce budget
        public bool holdToggleForAim = false;        // toggle vs hold for ability aim
    }

    [Serializable]
    public sealed class UiSettings
    {
        public float hudScale = 1.0f;                // 0.8..1.3
        public float hudOpacity = 1.0f;              // 0.5..1.0
        public bool showDamageNumbers = true;
        public bool showAllyHealthBars = true;
        public bool showTimersOnMinimap = true;
        public bool colorblindTeamColors = false;    // shape/color team indicators
        public bool minimapOnLeft = false;           // side of the minimap

        // ---- Localization ----
        public string uiLanguage = "en";             // ISO code for all UI text
        public bool subtitleMatchesUi = true;        // keep subtitle language == UI language
    }

    [Serializable]
    public sealed class ControlSettings
    {
        public HudHandedness handedness = HudHandedness.RightHanded;
        public MovementControlStyle movementStyle = MovementControlStyle.FloatingStick;
        public AbilityCastStyle castStyle = AbilityCastStyle.QuickCastWithIndicator;
        public float stickDeadZone = 0.12f;          // 0..0.4
        public float stickSize = 1.0f;               // 0.7..1.3
        public bool enableHaptics = true;
        public bool tapToMoveFallback = false;       // tap ground to path (accessibility)
        public float cameraPanSensitivity = 1.0f;
        public bool lockCameraOnHero = true;
    }

    [Serializable]
    public sealed class GameSettings
    {
        public int version = 1;                      // schema version for migration
        public GraphicsSettings graphics = new GraphicsSettings();
        public SoundSettings sound = new SoundSettings();
        public AccessibilitySettings accessibility = new AccessibilitySettings();
        public UiSettings ui = new UiSettings();
        public ControlSettings controls = new ControlSettings();

        public static GameSettings CreateDefault() => new GameSettings();

        /// <summary>Deep-ish clone for the "revert on cancel" pattern in the menu.</summary>
        public GameSettings Clone()
        {
            var json = MiniSettingsJson.Serialize(this);
            return MiniSettingsJson.Deserialize(json) ?? CreateDefault();
        }
    }
}
