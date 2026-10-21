// Project D  |  Codename: Zantetsuken
// StoreItemImporter.cs  (Editor-only) — items.seed.json -> ItemDefinition assets by category.
//   Tools > Project D > Import Store Items

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Store;

namespace ProjectD.EditorTools
{
    public static class StoreItemImporter
    {
        private const string Seed = "Assets/GameData/Items/items.seed.json";
        private const string OutRoot = "Assets/GameData/Items";

        [MenuItem("Tools/Project D/Import Store Items", priority = 72)]
        public static void Import()
        {
            if (!File.Exists(Seed)) { EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK"); return; }
            var root = MiniJSON.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null || !root.TryGetValue("items", out var io) || io is not List<object> list)
            { Debug.LogError("[Project D] items.seed.json missing 'items'."); return; }

            EnsureFolder(OutRoot);
            int count = 0;
            foreach (var it in list)
            {
                var d = (Dictionary<string, object>)it;
                string cat = Str(d, "category", "Basic");
                EnsureFolder($"{OutRoot}/{cat}");
                if (Upsert(d, cat)) count++;
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D", $"Imported {count} store items into {OutRoot}.", "OK");
            Debug.Log($"[Project D] Imported {count} store items.");
        }

        private static bool Upsert(Dictionary<string, object> d, string cat)
        {
            string id = Str(d, "itemId"); if (string.IsNullOrEmpty(id)) return false;
            string path = $"{OutRoot}/{cat}/Item_{id}.asset";
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (item == null) { item = ScriptableObject.CreateInstance<ItemDefinition>(); AssetDatabase.CreateAsset(item, path); }

            item.itemId = id;
            item.displayName = Str(d, "displayName");
            item.flavor = Str(d, "flavor");
            item.category = Enum<ItemCategory>(cat, ItemCategory.Basic);
            item.primaryRole = Enum<StoreRole>(Str(d, "primaryRole"), StoreRole.Universal);
            item.secondaryRole = Enum<StoreRole>(Str(d, "secondaryRole"), StoreRole.Universal);
            item.slotRestriction = Enum<SlotRestriction>(Str(d, "slotRestriction"), SlotRestriction.GeneralOnly);
            item.totalCost = (int)Num(d, "totalCost", 0);
            item.combineCost = (int)Num(d, "combineCost", 0);
            item.sellRefund = Num(d, "sellRefund", 0.6f);
            item.componentIds = StrArray(d, "componentIds");
            item.buildsIntoIds = StrArray(d, "buildsIntoIds");
            item.passiveName = Str(d, "passiveName");
            item.passiveDescription = Str(d, "passiveDescription");
            item.activeName = Str(d, "activeName");
            item.activeDescription = Str(d, "activeDescription");
            item.activeCooldown = Num(d, "activeCooldown", 0);
            item.isConsumable = Bool(d, "isConsumable");
            item.maxStack = (int)Num(d, "maxStack", 1);
            item.exclusiveToChampionId = Str(d, "exclusiveToChampionId");

            if (d.TryGetValue("stats", out var so) && so is List<object> sl)
            {
                item.stats = new StatMod[sl.Count];
                for (int i = 0; i < sl.Count; i++)
                {
                    var s = (Dictionary<string, object>)sl[i];
                    item.stats[i] = new StatMod { stat = Enum<StatType>(Str(s, "stat"), StatType.AttackDamage), value = Num(s, "value", 0) };
                }
            }
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
        private static float Num(Dictionary<string, object> d, string k, float fb) { if (d != null && d.TryGetValue(k, out var v) && v != null) { if (v is long l) return l; if (v is double db) return (float)db; if (float.TryParse(v.ToString(), out var f)) return f; } return fb; }
        private static bool Bool(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out var v) && v is bool b && b;
        private static string[] StrArray(Dictionary<string, object> d, string k) { if (d != null && d.TryGetValue(k, out var v) && v is List<object> l) { var a = new string[l.Count]; for (int i = 0; i < l.Count; i++) a[i] = l[i]?.ToString() ?? ""; return a; } return new string[0]; }
    }
}
#endif
