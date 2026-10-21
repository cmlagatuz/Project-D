// Project D  |  Codename: Zantetsuken
// SettingsEnums.cs  (Settings — shared enums)
//
// Enumerations for the mobile-first in-game settings: graphics quality presets,
// frame-rate caps, and the accessibility/sound/UI options. Kept UnityEngine-free
// so the settings model is unit-testable and can be validated on any platform.

namespace ProjectD.Settings
{
    // Overall graphics preset. "Custom" appears when the player edits an individual
    // graphics option away from a preset's defaults.
    public enum QualityPreset { Low, Medium, High, Custom }

    // Frame-rate cap options (fps). Availability is device-dependent: a 60Hz phone
    // hides 90/120/144, etc. See DeviceCapabilities.SupportedFrameRates.
    public enum FrameRateCap { FPS30 = 30, FPS60 = 60, FPS90 = 90, FPS120 = 120, FPS144 = 144 }

    // Per-option graphics levels (used when Custom).
    public enum GraphicsLevel { Low, Medium, High }

    // Who receives VO subtitles / how much text.
    public enum SubtitleMode { Off, SpeakerAndText, TextOnly }

    // Colorblind filter modes.
    public enum ColorFilter { None, Protanopia, Deuteranopia, Tritanopia }

    // On-screen control input styles.
    public enum MovementControlStyle { FloatingStick, FixedStick, DPad }
    public enum AbilityCastStyle { TapToCast, QuickCastWithIndicator, TapThenConfirm }

    // Handedness for the whole HUD mirror.
    public enum HudHandedness { RightHanded, LeftHanded }
}
