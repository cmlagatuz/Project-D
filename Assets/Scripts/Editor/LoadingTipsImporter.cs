// Project D  |  Codename: Zantetsuken
// LoadingTipsImporter.cs  (Editor-only)  — Tools > Project D > Import Loading Tips
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Loading;

namespace ProjectD.EditorTools
{
    public static class LoadingTipsImporter
    {
        private const string Seed = "Assets/GameData/Loading/loading_tips.seed.json";
        private const string OutDir = "Assets/GameData/Loading";

        [MenuItem("Tools/Project D/Import Loading Tips", priority = 75)]
        public static void Import()
        {
            if (!File.Exists(Seed)) { EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK"); return; }
            var root = MiniJSON.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null) { Debug.LogError("[Project D] Failed to parse loading_tips.seed.json"); return; }

            EnsureFolder(OutDir);
            int champs = 0;
            if (root.TryGetValue("champions", out var co) && co is List<object> list)
                foreach (var c in list) champs += UpsertChampion((Dictionary<string, object>)c);

            var gt = AssetDatabase.LoadAssetAtPath<GeneralTipSet>($"{OutDir}/GeneralLoadingTips.asset");
            if (gt == null) { gt = ScriptableObject.CreateInstance<GeneralTipSet>(); AssetDatabase.CreateAsset(gt, $"{OutDir}/GeneralLoadingTips.asset"); }
            gt.tips = StrArray(root, "generalTips"); EditorUtility.SetDirty(gt);

            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D", $"Imported {champs} champion tip set(s) + {gt.tips.Length} general tips.", "OK");
            Debug.Log($"[Project D] Imported {champs} champion tip sets + general tips.");
        }

        private static int UpsertChampion(Dictionary<string, object> d)
        {
            string id = Str(d, "championId");
            if (string.IsNullOrEmpty(id)) return 0;
            string path = $"{OutDir}/LoadingTips_{id}.asset";
            var set = AssetDatabase.LoadAssetAtPath<ChampionTipSet>(path);
            if (set == null) { set = ScriptableObject.CreateInstance<ChampionTipSet>(); AssetDatabase.CreateAsset(set, path); }
            set.championId = id; set.displayName = Str(d, "displayName"); set.sourceWorld = Str(d, "sourceWorld");
            set.role = Str(d, "role"); set.defaultLane = Str(d, "defaultLane");
            set.playingTips = StrArray(d, "playingTips"); set.playingAgainstTips = StrArray(d, "playingAgainstTips"); set.trivia = StrArray(d, "trivia");
            EditorUtility.SetDirty(set); return 1;
        }

        private static void EnsureFolder(string dir){ if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder(Path.GetDirectoryName(dir).Replace('\\','/'), Path.GetFileName(dir)); }
        private static string Str(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out var v) && v != null ? v.ToString() : "";
        private static string[] StrArray(Dictionary<string, object> d, string k)
        { if (d != null && d.TryGetValue(k, out var v) && v is List<object> l) { var a = new string[l.Count]; for (int i = 0; i < l.Count; i++) a[i] = l[i]?.ToString() ?? ""; return a; } return new string[0]; }
    }
}
#endif
