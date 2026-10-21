// Project D  |  Codename: Zantetsuken
// KpStoreImporter.cs  (Editor-only)  — Tools > Project D > Import KP Store Items
//
// Reads kp_store.seed.json -> StoreItemDefinition assets (by category subfolder).
// Re-running updates in place (matched by itemId) and preserves any preview sprite.
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Store;
using ProjectD.Economy;

namespace ProjectD.EditorTools
{
    public static class KpStoreImporter
    {
        private const string Seed = "Assets/GameData/Store/kp_store.seed.json";
        private const string OutRoot = "Assets/GameData/Store";

        [MenuItem("Tools/Project D/Import KP Store Items", priority = 86)]
        public static void Import()
        {
            if (!File.Exists(Seed)) { EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK"); return; }
            var root = MiniJSONLite.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null || !root.TryGetValue("items", out var io) || io is not List<object> list)
            { Debug.LogError("[Project D] kp_store.seed.json missing 'items'."); return; }

            EnsureFolder(OutRoot);
            int count = 0; var byCat = new Dictionary<string, int>();
            foreach (var it in list)
            {
                var d = (Dictionary<string, object>)it;
                string cat = Str(d, "category", "ProfileIcon");
                EnsureFolder($"{OutRoot}/{cat}");
                if (Upsert(d, cat)) { count++; byCat[cat] = byCat.TryGetValue(cat, out var n) ? n + 1 : 1; }
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            string summary = string.Join(", ", System.Linq.Enumerable.Select(byCat, kv => $"{kv.Key} {kv.Value}"));
            EditorUtility.DisplayDialog("Project D", $"Imported {count} KP Store items.\n{summary}", "OK");
            Debug.Log($"[Project D] Imported {count} KP Store items ({summary}).");
        }

        private static bool Upsert(Dictionary<string, object> d, string cat)
        {
            string id = Str(d, "itemId");
            if (string.IsNullOrEmpty(id)) return false;
            string path = $"{OutRoot}/{cat}/StoreItem_{id}.asset";
            var item = AssetDatabase.LoadAssetAtPath<StoreItemDefinition>(path);
            if (item == null) { item = ScriptableObject.CreateInstance<StoreItemDefinition>(); AssetDatabase.CreateAsset(item, path); }

            item.itemId = id;
            item.displayName = Str(d, "displayName");
            item.description = Str(d, "description");
            item.category = Enum<StoreCategory>(cat, StoreCategory.ProfileIcon);
            item.rarity = Enum<StoreRarity>(Str(d, "rarity"), StoreRarity.Common);
            item.kpPrice = (int)Num(d, "kpPrice", 500);
            item.kpSalePrice = (int)Num(d, "kpSalePrice", 0);
            item.championId = Str(d, "championId");
            item.requiredAccountLevel = (int)Num(d, "requiredAccountLevel", 0);
            item.featured = Bool(d, "featured", false);
            item.bundleItemIds = StrArray(d, "bundleItemIds");
            EditorUtility.SetDirty(item);
            return true;
        }

        private static void EnsureFolder(string dir)
        {
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(dir).Replace('\\', '/'), Path.GetFileName(dir));
        }
        private static T Enum<T>(string s, T fb) where T : struct => System.Enum.TryParse<T>(s, true, out var v) ? v : fb;
        private static string Str(Dictionary<string, object> d, string k, string fb = "") => d != null && d.TryGetValue(k, out var v) && v != null ? v.ToString() : fb;
        private static double Num(Dictionary<string, object> d, string k, double fb) { if (d != null && d.TryGetValue(k, out var v)) { if (v is long l) return l; if (v is double db) return db; double.TryParse(v?.ToString(), out var r); return r; } return fb; }
        private static bool Bool(Dictionary<string, object> d, string k, bool fb) => d != null && d.TryGetValue(k, out var v) && v is bool b ? b : fb;
        private static string[] StrArray(Dictionary<string, object> d, string k)
        { if (d != null && d.TryGetValue(k, out var v) && v is List<object> l) { var a = new string[l.Count]; for (int i = 0; i < l.Count; i++) a[i] = l[i]?.ToString() ?? ""; return a; } return new string[0]; }
    }

    // MiniJSONLite lives in LocalizationImporter.cs (same namespace) — removed duplicate here.
}
#endif
