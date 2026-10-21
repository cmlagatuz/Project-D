# Localization & Language Dropdown

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

Language selection for **UI text** and **VO subtitles**, built for an online ranked
game (language is an account preference that follows the player across devices).

## Two dropdowns
- **UI Language** (UI tab) — drives all `LocalizedText` in menus/HUD; changing it
  re-localizes instantly and, if "Match UI language" is on, re-points subtitles too.
- **Subtitle Language** (Accessibility tab) — independent unless "Match UI language"
  is enabled. Sits next to the VO subtitle toggle built earlier.

Both list languages by **native name** and only show languages that have a table
(English always available as the fallback).

## Languages
18 launch locales in `SupportedLanguages` (English, Spanish LatAm/Spain, Portuguese
BR, French, German, Italian, Russian, Turkish, Polish, Arabic (RTL), Thai,
Vietnamese, Indonesian, Japanese, Korean, Chinese Simplified/Traditional). Extend by
adding a table.

## First-run language (device- and account-aware)
`LocalizationService.Initialize`:
1. **Account preference** wins (synced for the online ranked game).
2. else the **persisted local setting**.
3. else the **device/OS system language**, resolved to the nearest shipped locale
   (`es-MX` -> `es-419`, `zh-TW` -> `zh-Hant`, unknown -> English).

## Fallback + coverage
`Localizer.Get(key)`: active language -> **English fallback** -> the key itself
(so missing strings are obvious in QA). Partial translations are fine; untranslated
keys fall back automatically. `Localizer.Coverage()` reports % translated for a
localization dashboard.

## Data
`localization.seed.json` -> `StringTable_<code>` assets via **Tools > Project D >
Import Localization** (reports per-language coverage). 77 UI keys seeded in English
(settings, sound, accessibility, controls, ranked, loading), with sample partial
translations to demonstrate the pipeline.

## RTL
Arabic is flagged RTL; `LocalizedText` right-aligns and the localizer exposes
`IsRightToLeft` for full layout mirroring.

## Online / ranked account sync
Hook the service to your account backend:
```csharp
loc.PullAccountLanguage = () => account.LanguageCode;   // used at first run
loc.PushAccountLanguage = code => account.SetLanguage(code); // saved on change
```
So a player's language choice persists server-side and follows them to any device —
important for a ranked title where the same account is played across phones/tablets.

## Wire-in
```csharp
var loc = new LocalizationService();
loc.RegisterFromStringTables(stringTables);
loc.Initialize(settings);              // picks account/device language
LocalizedText.Bind(loc.UI);            // all LocalizedText follow the UI language
subtitleDisplay + voice systems read loc.Subtitles for subtitle text.
```
