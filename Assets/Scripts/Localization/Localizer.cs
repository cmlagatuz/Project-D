// Project D  |  Codename: Zantetsuken
// Localizer.cs  (Localization — resolve keys to text with fallback + formatting)
//
// The engine-agnostic core: holds the active language table + an English fallback
// table, resolves a key to a string, supports {0}/{1} argument formatting, and
// exposes RTL info. Kept UnityEngine-free (works on a simple key->text dictionary)
// so it is unit-testable and reusable on the server (e.g. localized system messages).

using System.Collections.Generic;

namespace ProjectD.Localization
{
    public sealed class Localizer
    {
        private IReadOnlyDictionary<string, string> _active;
        private IReadOnlyDictionary<string, string> _fallback; // always English
        private string _activeCode = SupportedLanguages.Default;

        public string ActiveCode => _activeCode;
        public bool IsRightToLeft => SupportedLanguages.IsRightToLeft(_activeCode);

        /// <summary>Fired when the active language changes so live UI re-localizes.</summary>
        public event System.Action OnLanguageChanged;

        public void SetTables(string code, IReadOnlyDictionary<string, string> active,
                              IReadOnlyDictionary<string, string> englishFallback)
        {
            _activeCode = SupportedLanguages.IsSupported(code) ? code : SupportedLanguages.Default;
            _active = active;
            _fallback = englishFallback;
            OnLanguageChanged?.Invoke();
        }

        /// <summary>Resolve a key. Missing -> English fallback -> the key itself (dev-visible).</summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (_active != null && _active.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v)) return v;
            if (_fallback != null && _fallback.TryGetValue(key, out var f) && !string.IsNullOrEmpty(f)) return f;
            return key; // surfaced so missing strings are obvious in QA
        }

        /// <summary>Resolve + format with {0}, {1}... arguments.</summary>
        public string Format(string key, params object[] args)
        {
            string template = Get(key);
            if (args == null || args.Length == 0) return template;
            try { return string.Format(template, args); }
            catch { return template; }
        }

        /// <summary>True if the key exists in the active or fallback table.</summary>
        public bool Has(string key)
            => (_active != null && _active.ContainsKey(key)) || (_fallback != null && _fallback.ContainsKey(key));

        /// <summary>Coverage of the active language vs the fallback (0..1) for QA dashboards.</summary>
        public float Coverage()
        {
            if (_fallback == null || _fallback.Count == 0) return 1f;
            if (_active == null) return 0f;
            int have = 0;
            foreach (var k in _fallback.Keys)
                if (_active.TryGetValue(k, out var v) && !string.IsNullOrEmpty(v)) have++;
            return (float)have / _fallback.Count;
        }
    }
}
