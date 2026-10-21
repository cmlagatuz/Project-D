// Project D  |  Codename: Zantetsuken
// InteractionsImporter.cs  (Editor-only)  — Tools > Project D > Import Champion Interactions
//
// Reads interactions.seed.json -> one ChampionInteractionSet per champion. Then it
// tries to auto-bind an AudioClip for each voiceClipKey from GameData/Loading/Voice
// (drop your .mp3/.wav/.ogg there named <voiceClipKey>). Re-running preserves any
// clip you assigned manually. QUOTE TEXT is original placeholder copy — replace with
// licensed lines/audio before shipping.
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Loading;

namespace ProjectD.EditorTools
{
    public static class InteractionsImporter
    {
        private const string Seed = "Assets/GameData/Loading/interactions.seed.json";
        private const string OutDir = "Assets/GameData/Loading";
        private const string VoiceDir = "Assets/GameData/Loading/Voice";

        [MenuItem("Tools/Project D/Import Champion Interactions", priority = 77)]
        public static void Import()
        {
            if (!File.Exists(Seed)) { EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK"); return; }
            var root = MiniJSON.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null || !root.TryGetValue("champions", out var co) || co is not List<object> list)
            { Debug.LogError("[Project D] interactions.seed.json missing 'champions'."); return; }

            if (!AssetDatabase.IsValidFolder(OutDir)) AssetDatabase.CreateFolder("Assets/GameData", "Loading");
            if (!AssetDatabase.IsValidFolder(VoiceDir)) AssetDatabase.CreateFolder(OutDir, "Voice");

            int champs = 0, bound = 0, missing = 0;
            foreach (var c in list)
            {
                var r = Upsert((Dictionary<string, object>)c, ref bound, ref missing);
                if (r) champs++;
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                $"Imported {champs} champion interaction set(s).\nAudio bound: {bound}   Awaiting audio: {missing}\n\n" +
                $"Drop voice files in {VoiceDir}/<voiceClipKey>.mp3 and re-import to auto-bind.\n\n" +
                "NOTE: quote text is placeholder — replace text + audio with licensed content.", "OK");
            Debug.Log($"[Project D] Interactions imported: {champs} sets, {bound} clips bound, {missing} awaiting audio.");
        }

        private static bool Upsert(Dictionary<string, object> d, ref int bound, ref int missing)
        {
            string id = Str(d, "championId");
            if (string.IsNullOrEmpty(id)) return false;
            string path = $"{OutDir}/Interactions_{id}.asset";
            var set = AssetDatabase.LoadAssetAtPath<ChampionInteractionSet>(path);
            if (set == null) { set = ScriptableObject.CreateInstance<ChampionInteractionSet>(); AssetDatabase.CreateAsset(set, path); }

            set.championId = id;
            set.displayName = Str(d, "displayName");
            set.genericFirstEncounter = ReadLine(Get(d, "genericFirstEncounter"), set.genericFirstEncounter, ref bound, ref missing);

            if (d.TryGetValue("matchups", out var mo) && mo is List<object> ml)
            {
                var arr = new VoiceLine[ml.Count];
                for (int i = 0; i < ml.Count; i++)
                {
                    var md = (Dictionary<string, object>)ml[i];
                    var existing = (set.matchups != null && i < set.matchups.Length) ? set.matchups[i] : null;
                    var vl = ReadLine(md, existing, ref bound, ref missing);
                    vl.enemyChampionId = Str(md, "enemyChampionId");
                    arr[i] = vl;
                }
                set.matchups = arr;
            }
            EditorUtility.SetDirty(set);
            return true;
        }

        private static VoiceLine ReadLine(Dictionary<string, object> d, VoiceLine existing, ref int bound, ref int missing)
        {
            var vl = existing ?? new VoiceLine();
            if (d == null) return vl;
            vl.enemyChampionId = Str(d, "enemyChampionId");
            vl.line = Str(d, "line");
            vl.voiceClipKey = Str(d, "voiceClipKey");

            // Auto-bind the clip if a matching audio asset exists (only if not already set).
            if (vl.clip == null && !string.IsNullOrEmpty(vl.voiceClipKey))
            {
                var clip = FindClip(vl.voiceClipKey);
                if (clip != null) { vl.clip = clip; bound++; } else missing++;
            }
            else if (vl.clip != null) bound++;
            return vl;
        }

        private static AudioClip FindClip(string key)
        {
            foreach (var guid in AssetDatabase.FindAssets($"{key} t:AudioClip", new[] { VoiceDir }))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(p) == key)
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(p);
            }
            return null;
        }

        private static Dictionary<string, object> Get(Dictionary<string, object> d, string k)
            => d != null && d.TryGetValue(k, out var v) && v is Dictionary<string, object> dd ? dd : null;
        private static string Str(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out var v) && v != null ? v.ToString() : "";
    }
}
#endif
