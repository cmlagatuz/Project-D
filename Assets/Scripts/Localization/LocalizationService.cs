// Project D  |  Codename: Zantetsuken
// LocalizationService.cs  (Localization — load tables, drive the active language)
//
// The runtime hub the game reads for text. It:
//   - loads StringTable assets (one per language) + the English fallback,
//   - picks a first-run language from the ACCOUNT preference, else the device/OS
//     system language (resolved to the nearest shipped locale),
//   - applies the active language to the Localizer and raises change events so all
//     LocalizedText and subtitles re-localize instantly,
//   - exposes a hook to push/pull the language to the ACCOUNT for the online ranked
//     game (so the choice follows the player across devices).
//
// Separation of concerns: SEPARATE UI language and SUBTITLE language are supported;
// the settings store both, and a "match UI" convenience keeps them in sync.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.Settings;

namespace ProjectD.Localization
{
    public sealed class LocalizationService
    {
        public Localizer UI { get; } = new Localizer();          // UI text
        public Localizer Subtitles { get; } = new Localizer();   // VO subtitles

        private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _tables = new();
        private IReadOnlyDictionary<string, string> _english;

        /// <summary>Called to sync the chosen language up to the player's account (online ranked).</summary>
        public System.Action<string> PushAccountLanguage;
        /// <summary>Returns the account's saved language (or null if none / offline).</summary>
        public System.Func<string> PullAccountLanguage;

        // ---- table registration ----
        public void RegisterTable(string code, IReadOnlyDictionary<string, string> map)
        {
            _tables[code] = map;
            if (code == SupportedLanguages.Default) _english = map;
        }

        public void RegisterFromStringTables(IEnumerable<StringTable> tables)
        {
            foreach (var t in tables)
            {
                if (t == null || string.IsNullOrEmpty(t.languageCode)) continue;
                var map = new Dictionary<string, string>(t.Count);
                foreach (var e in t.entries) if (!string.IsNullOrEmpty(e.key)) map[e.key] = e.value;
                RegisterTable(t.languageCode, map);
            }
        }

        // ---- initialization ----
        /// <summary>Decide the starting language and apply it. settings carries the persisted choice.</summary>
        public void Initialize(GameSettings settings)
        {
            string uiCode = ResolveStartingLanguage(settings.ui != null ? settings.ui.uiLanguage : null);
            string subCode = SupportedLanguages.IsSupported(settings.accessibility?.subtitleLanguage)
                ? settings.accessibility.subtitleLanguage : uiCode;

            settings.ui.uiLanguage = uiCode;
            settings.accessibility.subtitleLanguage = subCode;

            ApplyUiLanguage(uiCode);
            ApplySubtitleLanguage(subCode);
        }

        private string ResolveStartingLanguage(string persisted)
        {
            // 1) explicit account preference wins (follows the player across devices).
            string account = PullAccountLanguage?.Invoke();
            if (SupportedLanguages.IsSupported(account)) return account;

            // 2) the persisted local setting.
            if (SupportedLanguages.IsSupported(persisted)) return persisted;

            // 3) device/OS system language, resolved to the nearest shipped locale.
            string sys = SystemLanguageCode();
            return SupportedLanguages.ResolveFromSystem(sys);
        }

        // ---- apply ----
        public void ApplyUiLanguage(string code)
        {
            if (!SupportedLanguages.IsSupported(code)) code = SupportedLanguages.Default;
            _tables.TryGetValue(code, out var active);
            UI.SetTables(code, active, _english);
            PushAccountLanguage?.Invoke(code);   // sync to account for ranked/online
        }

        public void ApplySubtitleLanguage(string code)
        {
            if (!SupportedLanguages.IsSupported(code)) code = SupportedLanguages.Default;
            _tables.TryGetValue(code, out var active);
            Subtitles.SetTables(code, active, _english);
        }

        /// <summary>Convenience: point subtitles at the same language as the UI.</summary>
        public void MatchSubtitlesToUi() => ApplySubtitleLanguage(UI.ActiveCode);

        public IReadOnlyList<string> AvailableCodes()
        {
            // Only offer languages we actually have a table for (or English fallback).
            var list = new List<string>();
            foreach (var l in SupportedLanguages.All)
                if (_tables.ContainsKey(l.Code) || l.Code == SupportedLanguages.Default) list.Add(l.Code);
            return list;
        }

        private static string SystemLanguageCode()
        {
            // Map Unity's SystemLanguage to a rough ISO code; ResolveFromSystem narrows it.
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English: return "en";
                case SystemLanguage.Spanish: return "es-419";
                case SystemLanguage.Portuguese: return "pt-BR";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.Polish: return "pl";
                case SystemLanguage.Arabic: return "ar";
                case SystemLanguage.Thai: return "th";
                case SystemLanguage.Vietnamese: return "vi";
                case SystemLanguage.Indonesian: return "id";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                case SystemLanguage.ChineseSimplified: return "zh-Hans";
                case SystemLanguage.ChineseTraditional: return "zh-Hant";
                case SystemLanguage.Chinese: return "zh-Hans";
                default: return "en";
            }
        }
    }
}
