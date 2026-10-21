// Project D  |  Codename: Zantetsuken
// SupportedLanguages.cs  (Localization — the language catalog)
//
// The set of locales the game ships. Each entry has an ISO code (BCP-47-ish), a
// native display name (shown in the dropdown), an English name, and a right-to-left
// flag for layout mirroring. Kept UnityEngine-free so it is unit-testable and shared
// by the client, the server (for account language), and tooling.
//
// For an ONLINE RANKED game, the language is an account preference (synced), but the
// device system language is used as the first-run default. See LocalizationService.

using System.Collections.Generic;

namespace ProjectD.Localization
{
    public struct LanguageInfo
    {
        public string Code;        // e.g. "en", "es-419", "pt-BR", "zh-Hans", "ar"
        public string NativeName;  // shown in the dropdown, in that language
        public string EnglishName; // for logs / support
        public bool RightToLeft;   // Arabic etc. -> mirror layout
    }

    public static class SupportedLanguages
    {
        // Launch set (extend as translations are delivered). Order = dropdown order.
        public static readonly LanguageInfo[] All =
        {
            new LanguageInfo { Code = "en",      NativeName = "English",             EnglishName = "English",              RightToLeft = false },
            new LanguageInfo { Code = "es-419",  NativeName = "Espanol (LatAm)",     EnglishName = "Spanish (Latin America)", RightToLeft = false },
            new LanguageInfo { Code = "es-ES",   NativeName = "Espanol (Espana)",    EnglishName = "Spanish (Spain)",      RightToLeft = false },
            new LanguageInfo { Code = "pt-BR",   NativeName = "Portugues (Brasil)",  EnglishName = "Portuguese (Brazil)",  RightToLeft = false },
            new LanguageInfo { Code = "fr",      NativeName = "Francais",            EnglishName = "French",               RightToLeft = false },
            new LanguageInfo { Code = "de",      NativeName = "Deutsch",             EnglishName = "German",               RightToLeft = false },
            new LanguageInfo { Code = "it",      NativeName = "Italiano",            EnglishName = "Italian",              RightToLeft = false },
            new LanguageInfo { Code = "ru",      NativeName = "Russkiy",             EnglishName = "Russian",              RightToLeft = false },
            new LanguageInfo { Code = "tr",      NativeName = "Turkce",              EnglishName = "Turkish",              RightToLeft = false },
            new LanguageInfo { Code = "pl",      NativeName = "Polski",              EnglishName = "Polish",               RightToLeft = false },
            new LanguageInfo { Code = "ar",      NativeName = "Arabic",              EnglishName = "Arabic",               RightToLeft = true  },
            new LanguageInfo { Code = "th",      NativeName = "Thai",                EnglishName = "Thai",                 RightToLeft = false },
            new LanguageInfo { Code = "vi",      NativeName = "Tieng Viet",          EnglishName = "Vietnamese",           RightToLeft = false },
            new LanguageInfo { Code = "id",      NativeName = "Bahasa Indonesia",    EnglishName = "Indonesian",           RightToLeft = false },
            new LanguageInfo { Code = "ja",      NativeName = "Japanese",            EnglishName = "Japanese",             RightToLeft = false },
            new LanguageInfo { Code = "ko",      NativeName = "Korean",              EnglishName = "Korean",               RightToLeft = false },
            new LanguageInfo { Code = "zh-Hans", NativeName = "Chinese (Simplified)",  EnglishName = "Chinese (Simplified)",  RightToLeft = false },
            new LanguageInfo { Code = "zh-Hant", NativeName = "Chinese (Traditional)", EnglishName = "Chinese (Traditional)", RightToLeft = false },
        };

        public const string Default = "en";

        public static bool IsSupported(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            foreach (var l in All) if (l.Code == code) return true;
            return false;
        }

        public static LanguageInfo Get(string code)
        {
            foreach (var l in All) if (l.Code == code) return l;
            return All[0];
        }

        public static bool IsRightToLeft(string code) => Get(code).RightToLeft;

        public static IReadOnlyList<string> Codes()
        {
            var list = new List<string>(All.Length);
            foreach (var l in All) list.Add(l.Code);
            return list;
        }

        /// <summary>
        /// Best match for a device/OS language code (e.g. "es-MX" -> "es-419",
        /// "pt-PT" -> "pt-BR" fallback, "en-US" -> "en"). Falls back to Default.
        /// </summary>
        public static string ResolveFromSystem(string systemCode)
        {
            if (string.IsNullOrEmpty(systemCode)) return Default;
            systemCode = systemCode.Replace('_', '-');

            // 1) exact match
            if (IsSupported(systemCode)) return systemCode;

            // 2) language-region special cases
            string lang = systemCode.Split('-')[0].ToLowerInvariant();
            if (lang == "es")
            {
                // European Spanish -> es-ES, everything else Latin American
                return systemCode.StartsWith("es-ES") ? "es-ES" : "es-419";
            }
            if (lang == "pt") return systemCode.StartsWith("pt-PT") ? "pt-BR" : "pt-BR"; // only BR shipped
            if (lang == "zh")
            {
                if (systemCode.Contains("Hant") || systemCode.Contains("TW") || systemCode.Contains("HK")) return "zh-Hant";
                return "zh-Hans";
            }

            // 3) bare language match (first supported with same primary subtag)
            foreach (var l in All)
                if (l.Code.Split('-')[0].ToLowerInvariant() == lang) return l.Code;

            return Default;
        }
    }
}
