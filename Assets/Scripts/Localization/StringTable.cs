// Project D  |  Codename: Zantetsuken
// StringTable.cs  (Localization — a per-language key->text table)
//
// One asset per language holding UI + subtitle strings keyed by a stable id
// (e.g. "settings.title", "vo.kain_vs_cecil"). Loaded by the LocalizationService;
// the runtime Localizer resolves keys with fallback to English. Data-driven so
// translators fill tables without touching code (GCD Appendix A).

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.Localization
{
    [Serializable]
    public struct LocalizedEntry
    {
        public string key;
        [TextArea(1, 3)] public string value;
    }

    [CreateAssetMenu(fileName = "StringTable_", menuName = "Project D/Localization String Table")]
    public sealed class StringTable : ScriptableObject
    {
        public string languageCode = "en";
        public LocalizedEntry[] entries = new LocalizedEntry[0];

        private Dictionary<string, string> _map;

        public void BuildIndex()
        {
            _map = new Dictionary<string, string>(entries.Length);
            foreach (var e in entries)
                if (!string.IsNullOrEmpty(e.key)) _map[e.key] = e.value;
        }

        public bool TryGet(string key, out string value)
        {
            if (_map == null) BuildIndex();
            return _map.TryGetValue(key, out value);
        }

        public int Count => entries != null ? entries.Length : 0;

#if UNITY_EDITOR
        private void OnValidate() => _map = null;
#endif
    }
}
