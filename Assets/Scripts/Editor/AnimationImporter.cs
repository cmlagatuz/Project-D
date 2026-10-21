// Project D  |  Codename: Zantetsuken
// AnimationImporter.cs  (Editor-only)
//
// Reads Assets/GameData/Animations/animations.seed.json and generates one
// ChampionAnimationSet asset per champion (with the five ability animations and
// locomotion clip names). Uses the base project's MiniJSON if present; otherwise
// falls back to Unity's JsonUtility-free manual parse via the bundled MiniJSON.
//
//   Tools > Project D > Import Champion Animations
//
// Re-running updates assets in place (matched by championId).

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Art.Animation;

namespace ProjectD.EditorTools
{
    public static class AnimationImporter
    {
        private const string Seed = "Assets/GameData/Animations/animations.seed.json";
        private const string OutDir = "Assets/GameData/Animations";

        [MenuItem("Tools/Project D/Import Champion Animations", priority = 65)]
        public static void Import()
        {
            if (!File.Exists(Seed))
            {
                EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK");
                return;
            }

            var root = MiniJSON.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null || !root.TryGetValue("champions", out var cobj) || cobj is not List<object> champs)
            {
                Debug.LogError("[Project D] animations.seed.json missing 'champions'.");
                return;
            }

            EnsureFolder(OutDir);
            int count = 0;
            foreach (var c in champs) count += Upsert((Dictionary<string, object>)c);

            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D",
                $"Imported {count} champion animation set(s) into {OutDir}.\n\n" +
                "Next: Tools > Project D > Generate Animator Controllers.", "OK");
            Debug.Log($"[Project D] Imported {count} ChampionAnimationSet assets.");
        }

        private static int Upsert(Dictionary<string, object> c)
        {
            string id = Str(c, "championId");
            string path = $"{OutDir}/AnimSet_{id}.asset";
            var set = AssetDatabase.LoadAssetAtPath<ChampionAnimationSet>(path);
            if (set == null) { set = ScriptableObject.CreateInstance<ChampionAnimationSet>(); AssetDatabase.CreateAsset(set, path); }

            set.championId = id;
            set.displayName = Str(c, "displayName");

            if (c.TryGetValue("locomotion", out var lo) && lo is Dictionary<string, object> loc)
            {
                set.idleClip = Str(loc, "idle", set.idleClip);
                set.runClip = Str(loc, "run", set.runClip);
                set.recallClip = Str(loc, "recall", set.recallClip);
                set.deathClip = Str(loc, "death", set.deathClip);
                set.spawnClip = Str(loc, "spawn", set.spawnClip);
            }
            if (c.TryGetValue("basicAttackChain", out var bc) && bc is List<object> chain)
            {
                var arr = new string[chain.Count];
                for (int i = 0; i < chain.Count; i++) arr[i] = chain[i]?.ToString() ?? "";
                set.basicAttackChain = arr;
            }
            set.basicHitFrame = Num(c, "basicHitFrame", set.basicHitFrame);

            set.passive = ReadAbility(c, "passive");
            set.abilityQ = ReadAbility(c, "abilityQ");
            set.abilityW = ReadAbility(c, "abilityW");
            set.abilityE = ReadAbility(c, "abilityE");
            set.ultimate = ReadAbility(c, "ultimate");

            EditorUtility.SetDirty(set);
            return 1;
        }

        private static AbilityAnimation ReadAbility(Dictionary<string, object> c, string key)
        {
            if (!c.TryGetValue(key, out var raw) || raw is not Dictionary<string, object> a) return null;
            var an = new AbilityAnimation
            {
                abilityId = Str(a, "abilityId"),
                displayName = Str(a, "displayName"),
                clipName = Str(a, "clipName"),
                chargeLoopClip = Str(a, "chargeLoopClip"),
                recastClip = Str(a, "recastClip"),
                startup = Num(a, "startup", 0.2f),
                active = Num(a, "active", 0.1f),
                recovery = Num(a, "recovery", 0.3f),
                cancelWindow = Num(a, "cancelWindow", 0.3f),
                loop = Bool(a, "loop"),
                travelDistance = Num(a, "travelDistance", 0f),
                animatesWeapon = a.ContainsKey("animatesWeapon") ? Bool(a, "animatesWeapon") : true,
                weaponTrailKey = Str(a, "weaponTrailKey"),
                animatorNotes = Str(a, "animatorNotes"),
            };
            an.motion = System.Enum.TryParse<MotionKind>(Str(a, "motion"), true, out var mk) ? mk : MotionKind.InPlace;
            an.targeting = System.Enum.TryParse<AnimTargeting>(Str(a, "targeting"), true, out var tg) ? tg : AnimTargeting.Directional;
            an.vfxCues = ReadCues(a, "vfxCues");
            an.sfxCues = ReadCues(a, "sfxCues");
            return an;
        }

        private static AnimVfxCue[] ReadCues(Dictionary<string, object> a, string key)
        {
            if (!a.TryGetValue(key, out var raw) || raw is not List<object> list) return new AnimVfxCue[0];
            var cues = new AnimVfxCue[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var d = (Dictionary<string, object>)list[i];
                cues[i] = new AnimVfxCue { time = Num(d, "time", 0f), key = Str(d, "key"), socket = Str(d, "socket", "root") };
            }
            return cues;
        }

        // ---- helpers ---- //
        private static void EnsureFolder(string dir)
        {
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(dir).Replace('\\', '/'), Path.GetFileName(dir));
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
