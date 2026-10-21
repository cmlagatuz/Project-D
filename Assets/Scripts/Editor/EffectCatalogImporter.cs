// Project D  |  Codename: Zantetsuken
// EffectCatalogImporter.cs  (Editor-only)
//
// Reads Effects_catalog.seed.json and generates one EffectDefinition asset per
// vfx_*/sfx_* key, then collects them into an EffectCatalog lookup asset so the
// runtime EffectSpawner/AudioCuePlayer can resolve keys -> definitions.
//
//   Tools > Project D > Import Effect Catalog
//
// Re-running preserves any prefab/clip references the artist has assigned (it only
// overwrites the classification fields), and adds new keys as they appear.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Art.Effects;

namespace ProjectD.EditorTools
{
    public static class EffectCatalogImporter
    {
        private const string Seed = "Assets/GameData/Effects_catalog.seed.json";
        private const string OutDir = "Assets/GameData/Effects";
        private const string CatalogPath = "Assets/GameData/Effects/EffectCatalog.asset";

        [MenuItem("Tools/Project D/Import Effect Catalog", priority = 70)]
        public static void Import()
        {
            if (!File.Exists(Seed)) { EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK"); return; }
            var root = MiniJSON.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null || !root.TryGetValue("effects", out var eo) || eo is not List<object> effects)
            { Debug.LogError("[Project D] Effects_catalog.seed.json missing 'effects'."); return; }

            EnsureFolders();
            var all = new List<EffectDefinition>();
            int vfx = 0, sfx = 0;
            foreach (var e in effects)
            {
                var def = Upsert((Dictionary<string, object>)e);
                if (def != null) { all.Add(def); if (def.kind == EffectKind.Vfx) vfx++; else sfx++; }
            }

            // Build/refresh the lookup catalog.
            var catalog = AssetDatabase.LoadAssetAtPath<EffectCatalog>(CatalogPath);
            if (catalog == null) { catalog = ScriptableObject.CreateInstance<EffectCatalog>(); AssetDatabase.CreateAsset(catalog, CatalogPath); }
            catalog.entries = all.ToArray();
            EditorUtility.SetDirty(catalog);

            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D",
                $"Imported {all.Count} effect definitions ({vfx} VFX, {sfx} SFX).\n" +
                $"Catalog: {CatalogPath}\n\nAssign prefabs/clips on each FX_ asset; the runtime resolves keys automatically.", "OK");
            Debug.Log($"[Project D] Effect catalog imported: {all.Count} definitions.");
        }

        private static EffectDefinition Upsert(Dictionary<string, object> d)
        {
            string key = Str(d, "key");
            if (string.IsNullOrEmpty(key)) return null;
            string safe = key.Replace("/", "_");
            string path = $"{OutDir}/FX_{safe}.asset";

            var def = AssetDatabase.LoadAssetAtPath<EffectDefinition>(path);
            if (def == null) { def = ScriptableObject.CreateInstance<EffectDefinition>(); AssetDatabase.CreateAsset(def, path); }

            // Classification fields (overwritten on re-import); prefab/clip refs preserved.
            def.key = key;
            def.kind = System.Enum.TryParse<EffectKind>(Str(d, "kind"), true, out var k) ? k : EffectKind.Vfx;
            def.category = System.Enum.TryParse<EffectCategory>(Str(d, "category"), true, out var cat) ? cat : EffectCategory.AbilityImpact;
            def.description = Str(d, "description");
            def.spawnRule = System.Enum.TryParse<SpawnRule>(Str(d, "spawnRule"), true, out var sr) ? sr : SpawnRule.SpawnAtSocket;
            def.defaultSocket = Str(d, "defaultSocket", "root");
            def.lifetime = Num(d, "lifetime", 2f);
            def.followSocket = Bool(d, "followSocket");
            def.pooled = !d.ContainsKey("pooled") || Bool(d, "pooled");
            def.prewarmCount = (int)Num(d, "prewarmCount", 4);
            def.maxConcurrent = (int)Num(d, "maxConcurrent", 12);
            def.isDangerTelegraph = Bool(d, "isDangerTelegraph");
            def.teamColored = Bool(d, "teamColored");
            if (def.kind == EffectKind.Sfx)
            {
                def.volume = Num(d, "volume", 1f);
                def.is3D = !d.ContainsKey("is3D") || Bool(d, "is3D");
                def.priority = (int)Num(d, "priority", 5);
            }
            EditorUtility.SetDirty(def);
            return def;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/GameData")) AssetDatabase.CreateFolder("Assets", "GameData");
            if (!AssetDatabase.IsValidFolder(OutDir)) AssetDatabase.CreateFolder("Assets/GameData", "Effects");
        }
        private static string Str(Dictionary<string, object> d, string k, string fb = "")
            => d != null && d.TryGetValue(k, out var v) && v != null ? v.ToString() : fb;
        private static float Num(Dictionary<string, object> d, string k, float fb)
        { if (d != null && d.TryGetValue(k, out var v) && v != null) { if (v is long l) return l; if (v is double db) return (float)db; if (float.TryParse(v.ToString(), out var f)) return f; } return fb; }
        private static bool Bool(Dictionary<string, object> d, string k)
            => d != null && d.TryGetValue(k, out var v) && v is bool b && b;
    }
}
#endif
