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
