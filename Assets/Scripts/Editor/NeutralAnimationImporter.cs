// Project D  |  Codename: Zantetsuken
// NeutralAnimationImporter.cs  (Editor-only)
//
// Reads neutrals.seed.json and generates one NeutralAnimationSet asset per
// summon / divine manifestation / monster / minion.
//
//   Tools > Project D > Import Neutral Animations
//
// Re-running updates assets in place (matched by neutralId).

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Art.Animation;

namespace ProjectD.EditorTools
{
    public static class NeutralAnimationImporter
    {
        private const string Seed = "Assets/GameData/Animations/neutrals.seed.json";
        private const string OutDir = "Assets/GameData/Animations/Neutrals";

        [MenuItem("Tools/Project D/Import Neutral Animations", priority = 68)]
        public static void Import()
        {
            if (!File.Exists(Seed)) { EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK"); return; }
            var root = MiniJSON.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null || !root.TryGetValue("neutrals", out var no) || no is not List<object> list)
            { Debug.LogError("[Project D] neutrals.seed.json missing 'neutrals'."); return; }

            EnsureFolder(OutDir);
            int count = 0;
            foreach (var n in list) count += Upsert((Dictionary<string, object>)n);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Project D",
                $"Imported {count} neutral animation set(s) into {OutDir}.\n\n" +
                "Next: Generate Animator Controllers picks these up too.", "OK");
            Debug.Log($"[Project D] Imported {count} NeutralAnimationSet assets.");
        }

        private static int Upsert(Dictionary<string, object> d)
        {
            string id = Str(d, "neutralId");
            string path = $"{OutDir}/NeutralAnim_{id}.asset";
            var set = AssetDatabase.LoadAssetAtPath<NeutralAnimationSet>(path);
            if (set == null) { set = ScriptableObject.CreateInstance<NeutralAnimationSet>(); AssetDatabase.CreateAsset(set, path); }

            set.neutralId = id;
            set.displayName = Str(d, "displayName");
            set.kind = System.Enum.TryParse<NeutralKind>(Str(d, "kind"), true, out var k) ? k : NeutralKind.MonsterMedium;
            set.spawnClip = Str(d, "spawnClip", "spawn");
            set.idleClip = Str(d, "idleClip", "idle");
            set.moveClip = Str(d, "moveClip", "move");
            set.basicHitFrame = Num(d, "basicHitFrame", 0.3f);
            if (d.TryGetValue("basicAttacks", out var ba) && ba is List<object> bl)
            {
                set.basicAttacks = new string[bl.Count];
                for (int i = 0; i < bl.Count; i++) set.basicAttacks[i] = bl[i]?.ToString() ?? "";
            }
            set.signatureA = ReadAbility(Get(d, "signatureA"));
            set.signatureB = ReadAbility(Get(d, "signatureB"));
            set.signatureUltimate = ReadAbility(Get(d, "signatureUltimate"));
            set.hitReactClip = Str(d, "hitReactClip", "hit_react");
            set.enrageClip = Str(d, "enrageClip");
            set.deathClip = Str(d, "deathClip", "death");
            set.deathVfxKey = Str(d, "deathVfxKey");
            set.rewardGrantClip = Str(d, "rewardGrantClip");
            set.rewardGrantVfxKey = Str(d, "rewardGrantVfxKey");
            set.notes = Str(d, "notes");

            EditorUtility.SetDirty(set);
            return 1;
        }

        private static AbilityAnimation ReadAbility(Dictionary<string, object> a)
        {
            if (a == null) return null;
            var an = new AbilityAnimation
            {
                abilityId = Str(a, "abilityId"), displayName = Str(a, "displayName"), clipName = Str(a, "clipName"),
                chargeLoopClip = Str(a, "chargeLoopClip"), recastClip = Str(a, "recastClip"),
                startup = Num(a, "startup", 0.3f), active = Num(a, "active", 0.2f), recovery = Num(a, "recovery", 0.4f),
                cancelWindow = Num(a, "cancelWindow", 0.5f), loop = Bool(a, "loop"),
                travelDistance = Num(a, "travelDistance", 0f),
                animatesWeapon = a.ContainsKey("animatesWeapon") ? Bool(a, "animatesWeapon") : false,
                weaponTrailKey = Str(a, "weaponTrailKey"), animatorNotes = Str(a, "animatorNotes"),
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

        private static void EnsureFolder(string dir)
        {
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(dir).Replace('\\', '/'), Path.GetFileName(dir));
        }
        private static Dictionary<string, object> Get(Dictionary<string, object> d, string k)
            => d != null && d.TryGetValue(k, out var v) && v is Dictionary<string, object> dd ? dd : null;
        private static string Str(Dictionary<string, object> d, string k, string fb = "")
            => d != null && d.TryGetValue(k, out var v) && v != null ? v.ToString() : fb;
        private static float Num(Dictionary<string, object> d, string k, float fb)
        { if (d != null && d.TryGetValue(k, out var v) && v != null) { if (v is long l) return l; if (v is double db) return (float)db; if (float.TryParse(v.ToString(), out var f)) return f; } return fb; }
        private static bool Bool(Dictionary<string, object> d, string k)
            => d != null && d.TryGetValue(k, out var v) && v is bool b && b;
    }
}
#endif
