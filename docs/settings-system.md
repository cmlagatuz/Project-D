# In-Game Settings (Mobile-First)

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

A device-aware settings system with five tabs. Build with
**Tools > Project D > Build Settings Menu** (runs standalone, no art).

## Tabs
- **Graphics** — quality preset (Low/Medium/High/Custom), **frame-rate cap
  (30/60/90/120/144)** gated by the device panel refresh, anti-aliasing, bloom,
  render scale, and **battery saver** (caps to 30 FPS).
- **Sound** — master/music/SFX/voice/ambience sliders, master mute, duck-music-on-VO.
- **Accessibility** — **VO subtitle toggle** + mode (Off / Speaker+Text / Text Only)
  + size + background opacity + speaker name; color filters; reduce shake / flashes /
  effect density.
- **UI** — HUD scale + opacity, damage numbers, ally bars, minimap side.
- **Controls** — movement style (Floating/Fixed/D-Pad), cast style (Tap/Quick/Confirm),
  handedness, dead zone, stick size, haptics, tap-to-move, and **Edit HUD Layout**.

## Device awareness (mobile focus)
`DeviceCapabilities` reads the device (panel refresh, RAM, cores):
- **Frame-rate options never exceed the panel refresh** — a 60Hz phone shows only
  30/60; a 120Hz phone shows 30/60/90/120; unsupported buttons are disabled with a
  "Device max: N FPS" hint.
- **First run** picks a preset + fps from the device **tier** (Low/Mid/High from RAM
  + cores), so a weak phone defaults to Low/30 and a flagship to High/120.
- On load, saved settings are **clamped** to what the current device supports
  (e.g. a config synced from a 144Hz phone drops to 60 on a 60Hz phone).

## Presets
`GraphicsPresets` maps Low/Medium/High to concrete shadow/texture/effect/model +
AA/bloom/render-scale values. Editing any single option flips the preset to
**Custom** automatically; the label updates live.

## VO subtitles (accessibility)
`SubtitleDisplay` renders champion/announcer lines honoring the settings:
- Off hides all; Text Only hides the speaker name; Speaker+Text shows "Kain: line".
- Text scale + background opacity apply live. Any voice system calls
  `subtitleDisplay.Show(speaker, line)`; the display decides what/if to draw.
- The menu applies changes live via `SettingsService.OnSettingsApplied`.

## Save / apply / revert
- The menu edits a **working copy**; **Apply** commits + saves + applies to the
  engine (targetFrameRate, quality level, subtitles, HUD), **Cancel** reverts,
  **Reset** restores device-recommended defaults.
- Persisted as JSON in `Application.persistentDataPath` (`settings.json`,
  `hud_layout.json`).

## Wire-in
```csharp
var service = new SettingsService();
service.Initialize();                 // probes device, loads/creates, applies
service.OnSettingsApplied += s => subtitleDisplay.ApplySettings(s);
// gameplay reads: service.Settings.graphics.frameRateCap, .accessibility.reduceEffects, etc.
```
