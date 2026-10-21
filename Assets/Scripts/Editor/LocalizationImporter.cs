// Project D  |  Codename: Zantetsuken
// LocalizationImporter.cs  (Editor-only)  — Tools > Project D > Import Localization
//
// Reads localization.seed.json and generates one StringTable asset per language.
// Re-running updates in place (matched by languageCode) and reports coverage vs the
// English (source) table, so localization QA can see which languages are incomplete.
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Localization;

namespace ProjectD.EditorTools
{
    public static class LocalizationImporter
    {
        private const string Seed = "Assets/GameData/Localization/localization.seed.json";
        private const string OutDir = "Assets/GameData/Localization";

        [MenuItem("Tools/Project D/Import Localization", priority = 79)]
        public static void Import()
        {
            if (!File.Exists(Seed)) { EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK"); return; }
            var root = MiniJSONLite.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null || !root.TryGetValue("languages", out var lo) || lo is not List<object> langs)
            { Debug.LogError("[Project D] localization.seed.json missing 'languages'."); return; }

            if (!AssetDatabase.IsValidFolder(OutDir)) AssetDatabase.CreateFolder("Assets/GameData", "Localization");

            int enKeys = 0;
            var report = new System.Text.StringBuilder();
            foreach (var l in langs)
            {
                var d = (Dictionary<string, object>)l;
                string code = Str(d, "languageCode");
                if (string.IsNullOrEmpty(code)) continue;

                var entries = new List<LocalizedEntry>();
                if (d.TryGetValue("entries", out var eo) && eo is List<object> el)
                    foreach (var e in el)
                    {
                        var ed = (Dictionary<string, object>)e;
                        entries.Add(new LocalizedEntry { key = Str(ed, "key"), value = Str(ed, "value") });
                    }

                if (code == SupportedLanguages.Default) enKeys = entries.Count;

                string path = $"{OutDir}/StringTable_{code}.asset";
                var table = AssetDatabase.LoadAssetAtPath<StringTable>(path);
                if (table == null) { table = ScriptableObject.CreateInstance<StringTable>(); AssetDatabase.CreateAsset(table, path); }
                table.languageCode = code;
                table.entries = entries.ToArray();
                EditorUtility.SetDirty(table);

                float cov = enKeys > 0 ? (float)entries.Count / enKeys * 100f : 100f;
                report.AppendLine($"  {code,-8}  {entries.Count,3} keys  ({cov:0.0}% of source)");
            }

            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D",
                $"Imported {langs.Count} language table(s). Source keys: {enKeys}\n\n{report}\n" +
                "Missing keys fall back to English at runtime.", "OK");
            Debug.Log($"[Project D] Localization imported ({langs.Count} tables, {enKeys} source keys).\n{report}");
        }

        private static string Str(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out var v) && v != null ? v.ToString() : "";
    }

    // Tiny JSON reader so this importer is standalone (no external MiniJSON dependency).
    internal static class MiniJSONLite
    {
        public static object Deserialize(string json) => new P(json).Val();
        private sealed class P
        {
            private readonly string _s; private int _i;
            public P(string s) { _s = s; }
            public object Val()
            {
                Ws();
                char c = _s[_i];
                if (c == '{') return Obj();
                if (c == '[') return Arr();
                if (c == '"') return Str();
                return Word();
            }
            private Dictionary<string, object> Obj()
            {
                var o = new Dictionary<string, object>(); _i++;
                while (true) { Ws(); if (_s[_i] == '}') { _i++; return o; } if (_s[_i] == ',') { _i++; continue; }
                    string k = Str(); Ws(); _i++; o[k] = Val(); }
            }
            private List<object> Arr()
            {
                var a = new List<object>(); _i++;
                while (true) { Ws(); if (_s[_i] == ']') { _i++; return a; } if (_s[_i] == ',') { _i++; continue; } a.Add(Val()); }
            }
            private string Str()
            {
                var sb = new System.Text.StringBuilder(); _i++;
                while (true) { char c = _s[_i++]; if (c == '"') break; if (c == '\\') { char e = _s[_i++];
                    sb.Append(e == 'n' ? '\n' : e == 't' ? '\t' : e == 'r' ? '\r' : e == 'u' ? UnicodeEscape() : e); } else sb.Append(c); }
                return sb.ToString();
            }
            private char UnicodeEscape() { string hex = _s.Substring(_i, 4); _i += 4; return (char)System.Convert.ToInt32(hex, 16); }
            private object Word()
            {
                int start = _i;
                while (_i < _s.Length && ",}] \t\r\n".IndexOf(_s[_i]) < 0) _i++;
                string w = _s.Substring(start, _i - start);
                if (w == "true") return true; if (w == "false") return false; if (w == "null") return null;
                if (long.TryParse(w, out var l)) return l; if (double.TryParse(w, out var d)) return d; return w;
            }
            private void Ws() { while (_i < _s.Length && " \t\r\n".IndexOf(_s[_i]) >= 0) _i++; }
        }
    }
}
#endif
