// Project D  |  Codename: Zantetsuken
// CombatExtrasImporter.cs  (Editor-only)
//
// Reads combat_extras.seed.json and fills the attackChain / reactions / healing /
// deathRespawn blocks on each existing ChampionAnimationSet (created by the skill
// AnimationImporter). Run AFTER "Import Champion Animations".
//
//   Tools > Project D > Import Combat Extras

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Art.Animation;

namespace ProjectD.EditorTools
{
    public static class CombatExtrasImporter
    {
        private const string Seed = "Assets/GameData/Animations/combat_extras.seed.json";
        private const string SetDir = "Assets/GameData/Animations";

        [MenuItem("Tools/Project D/Import Combat Extras", priority = 67)]
        public static void Import()
        {
            if (!File.Exists(Seed)) { EditorUtility.DisplayDialog("Project D", $"Seed not found:\n{Seed}", "OK"); return; }
            var root = MiniJSON.Deserialize(File.ReadAllText(Seed)) as Dictionary<string, object>;
            if (root == null || !root.TryGetValue("champions", out var co) || co is not List<object> champs)
            { Debug.LogError("[Project D] combat_extras.seed.json missing 'champions'."); return; }

            int updated = 0; var missing = new List<string>();
            foreach (var c in champs)
            {
                var d = (Dictionary<string, object>)c;
                string id = Str(d, "championId");
                var set = AssetDatabase.LoadAssetAtPath<ChampionAnimationSet>($"{SetDir}/AnimSet_{id}.asset");
                if (set == null) { missing.Add(id); continue; }

                set.attackChain = ReadChain(Get(d, "attackChain"));
                set.reactions = ReadReactions(Get(d, "reactions"));
                set.healing = ReadHealing(Get(d, "healing"));
                set.deathRespawn = ReadDeath(Get(d, "deathRespawn"));
                EditorUtility.SetDirty(set);
                updated++;
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            string msg = $"Updated {updated} animation set(s) with attack chains, reactions, healing, and death/respawn.";
            if (missing.Count > 0) msg += $"\n\nNo AnimSet found for: {string.Join(", ", missing)}\nRun 'Import Champion Animations' first.";
            EditorUtility.DisplayDialog("Project D", msg, "OK");
            Debug.Log("[Project D] " + msg.Replace("\n", " | "));
        }

        private static BasicAttackChain ReadChain(Dictionary<string, object> d)
        {
            if (d == null) return null;
            var chain = new BasicAttackChain
            {
                comboResetSeconds = Num(d, "comboResetSeconds", 1.2f),
                baseAttacksPerSecond = Num(d, "baseAttacksPerSecond", 0.65f),
                notes = Str(d, "notes"),
            };
            chain.weaponClass = System.Enum.TryParse<WeaponClass>(Str(d, "weaponClass"), true, out var wc) ? wc : WeaponClass.Sword;
            if (d.TryGetValue("swings", out var so) && so is List<object> sl)
            {
                chain.swings = new BasicAttackSwing[sl.Count];
                for (int i = 0; i < sl.Count; i++)
                {
                    var s = (Dictionary<string, object>)sl[i];
                    chain.swings[i] = new BasicAttackSwing
                    {
                        clipName = Str(s, "clipName"), hitFrame = Num(s, "hitFrame", 0.24f),
                        totalDuration = Num(s, "totalDuration", 0.5f), cancelWindow = Num(s, "cancelWindow", 0.3f),
                        impactVfxKey = Str(s, "impactVfxKey"), swingSfxKey = Str(s, "swingSfxKey"),
                    };
                }
            }
            return chain;
        }

        private static ReactionSet ReadReactions(Dictionary<string, object> d)
        {
            if (d == null) return null;
            ReactionClip R(string k) => ReadClip(Get(d, k));
            return new ReactionSet
            {
                flinch = R("flinch"), hitReact = R("hitReact"), knockback = R("knockback"),
                knockup = R("knockup"), airborneLoop = R("airborneLoop"), stun = R("stun"),
                root = R("root"), silence = R("silence"), slowOverlay = R("slowOverlay"),
                notes = Str(d, "notes"),
            };
        }

        private static ReactionClip ReadClip(Dictionary<string, object> d)
            => d == null ? null : new ReactionClip
            {
                clipName = Str(d, "clipName"), duration = Num(d, "duration", 0.4f),
                interrupts = !d.ContainsKey("interrupts") || Bool(d, "interrupts"), reactVfxKey = Str(d, "reactVfxKey"),
            };

        private static HealingEffectSet ReadHealing(Dictionary<string, object> d)
        {
            if (d == null) return null;
            return new HealingEffectSet
            {
                healReceiveClip = Str(d, "healReceiveClip"), healReceiveVfxKey = Str(d, "healReceiveVfxKey"),
                shieldGainClip = Str(d, "shieldGainClip"), shieldGainVfxKey = Str(d, "shieldGainVfxKey"),
                shieldBreakVfxKey = Str(d, "shieldBreakVfxKey"), buffReceiveVfxKey = Str(d, "buffReceiveVfxKey"),
                cleanseClip = Str(d, "cleanseClip"), cleanseVfxKey = Str(d, "cleanseVfxKey"),
                additiveOnly = !d.ContainsKey("additiveOnly") || Bool(d, "additiveOnly"), notes = Str(d, "notes"),
            };
        }

        private static DeathAndRespawn ReadDeath(Dictionary<string, object> d)
        {
            if (d == null) return null;
            return new DeathAndRespawn
            {
                deathClip = Str(d, "deathClip"), deathVfxKey = Str(d, "deathVfxKey"),
                countdownLoopClip = Str(d, "countdownLoopClip"), reviveClip = Str(d, "reviveClip"),
                reviveVfxKey = Str(d, "reviveVfxKey"), showCountdownNumber = !d.ContainsKey("showCountdownNumber") || Bool(d, "showCountdownNumber"),
                notes = Str(d, "notes"),
            };
        }

        // helpers
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
