// Project D  |  Codename: Zantetsuken
// PitBgmImporter.cs  (Editor-only)  — Tools > Project D > Import Pit BGM
//
// Reads pit_bgm.seed.json -> a PitBgmSet asset, and auto-binds each theme's AudioClip
// from Assets/GameData/Audio/Summons and /Divine by filename (== clipKey). Re-running
// preserves any clip you assigned manually and reports which themes still need audio.
// The music files are YOUR imported tracks (original/licensed).
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Audio;

namespace ProjectD.EditorTools
{
    public static class PitBgmImporter
    {
        private const string Seed = "Assets/GameData/Audio/pit_bgm.seed.json";
        private const string OutDir = "Assets/GameData/Audio";
        private const string SummonAudioDir = "Assets/GameData/Audio/Summons";
        private const string DivineAudioDir = "Assets/GameData/Audio/Divine";

        [MenuItem("Tools/Project D/Import Pit BGM", priority = 84)]
        public static void Import()
        {
            if (!File.Exists(Seed)) { EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK"); return; }
            var root = MiniJSONLite.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null) { Debug.LogError("[Project D] Failed to parse pit_bgm.seed.json"); return; }

            EnsureFolders();
            string path = $"{OutDir}/PitBgmSet.asset";
            var set = AssetDatabase.LoadAssetAtPath<PitBgmSet>(path);
            if (set == null) { set = ScriptableObject.CreateInstance<PitBgmSet>(); AssetDatabase.CreateAsset(set, path); }

            int bound = 0, missing = 0;
            set.summonThemes = ReadThemes(root, "summonThemes", SummonAudioDir, set.summonThemes, ref bound, ref missing);
            set.divineThemes = ReadThemes(root, "divineThemes", DivineAudioDir, set.divineThemes, ref bound, ref missing);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                $"Imported Pit BGM set ({set.summonThemes.Length + set.divineThemes.Length} themes).\n" +
                $"Audio bound: {bound}   Awaiting audio: {missing}\n\n" +
                $"Drop your tracks in {SummonAudioDir}/<clipKey>.mp3 and {DivineAudioDir}/<clipKey>.mp3, then re-import.", "OK");
            Debug.Log($"[Project D] Pit BGM imported: {bound} clips bound, {missing} awaiting audio.");
        }

        private static ObjectiveBgm[] ReadThemes(Dictionary<string, object> root, string key, string audioDir, ObjectiveBgm[] existing, ref int bound, ref int missing)
        {
            if (!root.TryGetValue(key, out var lo) || lo is not List<object> list) return existing ?? new ObjectiveBgm[0];
            var byId = new Dictionary<string, ObjectiveBgm>();
            if (existing != null) foreach (var e in existing) if (!string.IsNullOrEmpty(e.objectiveId)) byId[e.objectiveId] = e;

            var outArr = new ObjectiveBgm[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var d = (Dictionary<string, object>)list[i];
                string oid = Str(d, "objectiveId");
                var bgm = byId.TryGetValue(oid, out var prev) ? prev : new ObjectiveBgm();
                bgm.objectiveId = oid;
                bgm.displayName = Str(d, "displayName");
                bgm.volume = (float)Num(d, "volume", 0.8);
                bgm.fadeInSeconds = (float)Num(d, "fadeInSeconds", 1.2);
                bgm.fadeOutSeconds = (float)Num(d, "fadeOutSeconds", 1.5);
                bgm.loop = Bool(d, "loop", true);

                if (bgm.clip == null)
                {
                    string clipKey = Str(d, "clipKey");
                    var clip = FindClip(audioDir, clipKey);
                    if (clip != null) { bgm.clip = clip; bound++; } else missing++;
                }
                else bound++;

                outArr[i] = bgm;
            }
            return outArr;
        }

        private static AudioClip FindClip(string dir, string key)
        {
            if (string.IsNullOrEmpty(key) || !AssetDatabase.IsValidFolder(dir)) return null;
            foreach (var guid in AssetDatabase.FindAssets($"{key} t:AudioClip", new[] { dir }))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(p) == key) return AssetDatabase.LoadAssetAtPath<AudioClip>(p);
            }
            return null;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/GameData")) AssetDatabase.CreateFolder("Assets", "GameData");
            if (!AssetDatabase.IsValidFolder(OutDir)) AssetDatabase.CreateFolder("Assets/GameData", "Audio");
            if (!AssetDatabase.IsValidFolder(SummonAudioDir)) AssetDatabase.CreateFolder(OutDir, "Summons");
            if (!AssetDatabase.IsValidFolder(DivineAudioDir)) AssetDatabase.CreateFolder(OutDir, "Divine");
        }

        private static string Str(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out var v) && v != null ? v.ToString() : "";
        private static double Num(Dictionary<string, object> d, string k, double fb) { if (d != null && d.TryGetValue(k, out var v)) { if (v is long l) return l; if (v is double db) return db; double.TryParse(v?.ToString(), out var r); return r; } return fb; }
        private static bool Bool(Dictionary<string, object> d, string k, bool fb) => d != null && d.TryGetValue(k, out var v) && v is bool b ? b : fb;
    }

    // MiniJSONLite lives in LocalizationImporter.cs (same namespace) — removed duplicate here.
}
#endif
