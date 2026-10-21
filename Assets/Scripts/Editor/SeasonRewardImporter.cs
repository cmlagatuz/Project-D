// Project D  |  Codename: Zantetsuken
// SeasonRewardImporter.cs  (Editor-only)  — Tools > Project D > Import Season Rewards
//
// Reads season_rewards.seed.json and generates a SeasonRewardConfig asset.
// Re-running updates it in place.
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Ranked;

namespace ProjectD.EditorTools
{
    public static class SeasonRewardImporter
    {
        private const string Seed = "Assets/GameData/Ranked/season_rewards.seed.json";
        private const string OutDir = "Assets/GameData/Ranked";

        [MenuItem("Tools/Project D/Import Season Rewards", priority = 81)]
        public static void Import()
        {
            if (!File.Exists(Seed)) { EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK"); return; }
            var root = MiniJSONLite.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null) { Debug.LogError("[Project D] Failed to parse season_rewards.seed.json"); return; }

            if (!AssetDatabase.IsValidFolder(OutDir)) AssetDatabase.CreateFolder("Assets/GameData", "Ranked");
            string path = $"{OutDir}/SeasonRewards_S{(int)Num(root, "seasonId", 1)}.asset";
            var cfg = AssetDatabase.LoadAssetAtPath<SeasonRewardConfig>(path);
            if (cfg == null) { cfg = ScriptableObject.CreateInstance<SeasonRewardConfig>(); AssetDatabase.CreateAsset(cfg, path); }

            cfg.seasonId = (int)Num(root, "seasonId", 1);
            cfg.seasonName = Str(root, "seasonName");
            cfg.cumulativeUpToPeak = Bool(root, "cumulativeUpToPeak", true);

            // tier rewards
            var trList = new List<TierReward>();
            if (root.TryGetValue("tierRewards", out var tro) && tro is List<object> trs)
                foreach (var t in trs) trList.Add(ReadTierReward((Dictionary<string, object>)t));
            cfg.tierRewards = trList.ToArray();

            // win milestones
            if (root.TryGetValue("winMilestones", out var wmo) && wmo is List<object> wm)
            {
                var arr = new int[wm.Count];
                for (int i = 0; i < wm.Count; i++) arr[i] = (int)ToNum(wm[i]);
                cfg.winMilestones = arr;
            }
            var wrList = new List<RewardBundle>();
            if (root.TryGetValue("winMilestoneRewards", out var wro) && wro is List<object> wrs)
                foreach (var b in wrs) wrList.Add(ReadBundle((Dictionary<string, object>)b));
            cfg.winMilestoneRewards = wrList.ToArray();

            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                $"Imported season reward config:\n{path}\n\n{cfg.tierRewards.Length} tier rewards, " +
                $"{cfg.winMilestones.Length} win milestones.", "OK");
            Debug.Log($"[Project D] Season rewards imported ({cfg.tierRewards.Length} tiers).");
        }

        private static TierReward ReadTierReward(Dictionary<string, object> d)
        {
            var tr = new TierReward { minRankedGames = (int)Num(d, "minRankedGames", 5) };
            System.Enum.TryParse<RankTier>(Str(d, "tier"), true, out tr.tier);
            tr.bundle = ReadBundle(Get(d, "bundle"));
            return tr;
        }

        private static RewardBundle ReadBundle(Dictionary<string, object> d)
        {
            var b = new RewardBundle();
            if (d == null) return b;
            b.emblemId = Str(d, "emblemId");
            b.bannerId = Str(d, "bannerId");
            b.honorPoints = (int)Num(d, "honorPoints", 0);
            b.displayName = Str(d, "displayName");
            b.nameKey = Str(d, "nameKey");
            if (d.TryGetValue("bonusCosmeticIds", out var bo) && bo is List<object> bl)
            {
                b.bonusCosmeticIds = new string[bl.Count];
                for (int i = 0; i < bl.Count; i++) b.bonusCosmeticIds[i] = bl[i]?.ToString() ?? "";
            }
            return b;
        }

        private static Dictionary<string, object> Get(Dictionary<string, object> d, string k)
            => d != null && d.TryGetValue(k, out var v) && v is Dictionary<string, object> dd ? dd : null;
        private static string Str(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out var v) && v != null ? v.ToString() : "";
        private static double Num(Dictionary<string, object> d, string k, double fb) => d != null && d.TryGetValue(k, out var v) ? ToNum(v) : fb;
        private static double ToNum(object v) { if (v is long l) return l; if (v is double db) return db; double.TryParse(v?.ToString(), out var r); return r; }
        private static bool Bool(Dictionary<string, object> d, string k, bool fb) => d != null && d.TryGetValue(k, out var v) && v is bool b ? b : fb;
    }

    // MiniJSONLite lives in LocalizationImporter.cs (same namespace) — removed duplicate here.
}
#endif
