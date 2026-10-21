# Project D — In-Game Settings (Mobile)

> Codename **Zantetsuken** · CONFIDENTIAL

Device-aware mobile settings: Graphics (frame-rate 30/60/90/120/144 + Low/Medium/High
presets, gated by the device), Sound, Accessibility (**VO subtitle toggle**), UI, and a
**drag-your-buttons HUD layout editor**.

## Contents
- `Scripts/Settings/` — enums, `DeviceCapabilities`, `GameSettings`, `GraphicsPresets`,
  `HudLayout`, `SettingsService` (load/save/apply + validation).
- `Scripts/UI/` — `SettingsMenuController`, `SubtitleDisplay`, `DraggableHudElement`,
  `HudLayoutEditor`.
- `Scripts/Editor/SettingsMenuBuilder.cs` — **Tools > Project D > Build Settings Menu**.
- `Scripts/Tests/SettingsTests.cs` — device gating, tier, presets, HUD layout.
- `docs/` — settings-system.md, hud-layout-editor.md.

## Highlights
- **Frame-rate options gated by the panel** (60Hz phone shows only 30/60; 120Hz shows
  up to 120), with first-run defaults chosen from the device **tier** (RAM + cores).
- **Low/Medium/High** presets map to concrete per-option values; editing one flips to
  **Custom**.
- **VO subtitle toggle** (Off / Speaker+Text / Text Only) applies live to `SubtitleDisplay`.
- **Editable UI controller layout** — drag any button to any screen position (normalized,
  safe-area aware); Save/Cancel/Reset; handedness mirror.

## Quick start
Build Settings Menu -> Play. Frame-rate buttons above your device refresh are disabled.
Toggle VO subtitles in Accessibility; Apply saves + applies live. Controls -> Edit HUD
Layout to move your buttons.


## Localization / Language dropdown (this update)
- **UI Language** + **Subtitle Language** dropdowns (native names), with a "Match UI language" toggle.
- 18 launch locales (incl. Arabic RTL); English is the source/fallback. Missing keys fall back to English.
- First-run language from the **account preference** (online ranked, follows the player), else device/OS language resolved to the nearest shipped locale.
- `LocalizedText` binds any UI Text to a key and re-localizes instantly; `Localizer` supports {0} args, RTL, and coverage %.
- Data: `localization.seed.json` (77 UI keys) -> `StringTable_<code>` via **Tools > Project D > Import Localization**.
- New: `SupportedLanguages`, `Localizer`, `LocalizationService`, `StringTable`, `LocalizedText`, `LanguageDropdown`, `LocalizationImporter`, `LocalizationTests`. Docs: `docs/localization.md`.
