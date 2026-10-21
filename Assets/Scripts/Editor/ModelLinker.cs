// Project D  |  Codename: Zantetsuken
// ModelLinker.cs  (Editor-only)
//
// Binds the per-unit model prefabs in Assets/Models/** onto the generated data
// assets by matching folder id. Artists drop `Prefab_<id>.prefab` into a unit's
// Prefabs/ folder, then run one menu item to wire every prefab that exists.
//
//   Tools > Project D > Link Champion Models
//   Tools > Project D > Link Neutral/Minion Models
//   Tools > Project D > Link All Models
//
// Missing prefabs are skipped and reported so art can be imported incrementally.

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectD.Data;

namespace ProjectD.EditorTools
{
    public static class ModelLinker
    {
        private const string ChampionsData = "Assets/GameData/Champions";
        private const string SummonsData = "Assets/GameData/Summons";
        private const string ChampionModels = "Assets/Models/Champions";

        [MenuItem("Tools/Project D/Link All Models", priority = 60)]
        public static void LinkAll()
        {
            int c = LinkChampions(false);
            int n = LinkNeutrals(false);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D",
                $"Model linking complete.\nChampions bound: {c}\nNeutral/minion bound: {n}\n\nUnits without a prefab yet were skipped.", "OK");
        }

        [MenuItem("Tools/Project D/Link Champion Models", priority = 61)]
        public static void LinkChampionsMenu()
        {
            int c = LinkChampions(true);
            Debug.Log($"[Project D] Linked {c} champion model prefabs.");
        }

        [MenuItem("Tools/Project D/Link Neutral and Minion Models", priority = 62)]
        public static void LinkNeutralsMenu()
        {
            int n = LinkNeutrals(true);
            Debug.Log($"[Project D] Linked {n} neutral/minion model prefabs.");
        }

        // ------------------------------------------------------------------ //
        private static int LinkChampions(bool save)
        {
            int bound = 0;
            var skipped = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:ChampionDefinition", new[] { ChampionsData }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var champ = AssetDatabase.LoadAssetAtPath<ChampionDefinition>(path);
                if (champ == null || string.IsNullOrEmpty(champ.championId)) continue;

                string prefabPath = $"{ChampionModels}/{champ.championId}/Prefabs/Prefab_{champ.championId}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) { skipped.Add(champ.championId); continue; }

                champ.modelPrefab = prefab;
                EditorUtility.SetDirty(champ);
                bound++;
            }
            if (skipped.Count > 0)
                Debug.Log($"[Project D] Champions awaiting art ({skipped.Count}): {string.Join(", ", skipped)}");
            if (save) { AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }
            return bound;
        }

        private static int LinkNeutrals(bool save)
        {
            // Map each NeutralMonsterDefinition id to its model root by tier.
            var roots = new Dictionary<string, string>();
            foreach (var summon in new[] { "ifrit", "shiva", "ramuh", "garuda", "titan", "leviathan", "odin", "materia", "cosmos", "spiritus", "chaos" })
                roots[summon] = $"Assets/Models/Summons/{summon}";
            foreach (var m in new[] { "bomb", "flan", "mu", "floating_eye" }) roots[m] = $"Assets/Models/Monsters/Small/{m}";
            foreach (var m in new[] { "tonberry", "coeurl", "ahriman", "sahagin" }) roots[m] = $"Assets/Models/Monsters/Medium/{m}";
            foreach (var m in new[] { "iron_giant", "malboro", "behemoth", "adamantoise" }) roots[m] = $"Assets/Models/Monsters/Large/{m}";
            foreach (var m in new[] { "goblin_melee", "goblin_ranged", "goblin_mage", "goblin_armored", "goblin_warlord" }) roots[m] = $"Assets/Models/Minions/{m}";

            int bound = 0;
            var skipped = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:NeutralMonsterDefinition", new[] { SummonsData, "Assets/GameData/Monsters" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mon = AssetDatabase.LoadAssetAtPath<NeutralMonsterDefinition>(path);
                if (mon == null) continue;

                // Data ids look like "summon_ifrit" / "divine_chaos"; strip the prefix to match folder id.
                string key = NormalizeId(mon.monsterId);
                if (!roots.TryGetValue(key, out var root)) { skipped.Add(mon.monsterId); continue; }

                string prefabPath = $"{root}/Prefabs/Prefab_{key}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) { skipped.Add(key); continue; }

                mon.prefab = prefab;
                EditorUtility.SetDirty(mon);
                bound++;
            }
            if (skipped.Count > 0)
                Debug.Log($"[Project D] Neutral/minion units awaiting art ({skipped.Count}): {string.Join(", ", skipped)}");
            if (save) { AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }
            return bound;
        }

        private static string NormalizeId(string monsterId)
        {
            if (string.IsNullOrEmpty(monsterId)) return monsterId;
            foreach (var prefix in new[] { "summon_", "divine_", "monster_" })
                if (monsterId.StartsWith(prefix)) return monsterId.Substring(prefix.Length);
            return monsterId;
        }
    }
}
#endif
