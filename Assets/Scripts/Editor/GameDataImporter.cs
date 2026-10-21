// Project D  |  Codename: Zantetsuken
// GameDataImporter.cs  (Editor-only)
//
// Turns the JSON authoring seeds into ScriptableObject .asset files.
//   Tools > Project D > Import Summon Data
//   Tools > Project D > Import Champion Data   (now maps the extended bio fields)
//   Tools > Project D > Import All Game Data
// Re-running updates assets in place (matched by id).

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectD.Core;
using ProjectD.Data;

namespace ProjectD.EditorTools
{
    public static class GameDataImporter
    {
        private const string Root = "Assets/GameData";
        private const string ChampionsDir = Root + "/Champions";
        private const string SummonsDir = Root + "/Summons";
        private const string MonstersDir = Root + "/Monsters";
        private const string BuffsDir = Root + "/Buffs";
        private const string AbilitiesDir = Root + "/Abilities";
        private const string ChampionSeed = Root + "/champions.seed.json";
        private const string SummonSeed = Root + "/summons.seed.json";

        [MenuItem("Tools/Project D/Import All Game Data", priority = 0)]
        public static void ImportAll()
        {
            ImportSummons();
            ImportChampions();
            EditorUtility.DisplayDialog("Project D", "All game data imported.\nSee Assets/GameData.", "OK");
        }

        [MenuItem("Tools/Project D/Import Summon Data", priority = 20)]
        public static void ImportSummons()
        {
            EnsureFolders();
            var root = ReadJson(SummonSeed); if (root == null) return;
            int count = 0;
            if (root.TryGetValue("elementalSummons", out var e) && e is List<object> el)
                foreach (var x in el) count += UpsertMonster((Dictionary<string, object>)x, NeutralTier.ElementalSummon, true);
            if (root.TryGetValue("elderSummon", out var o) && o is Dictionary<string, object> od)
                count += UpsertMonster(od, NeutralTier.ElderSummon, true);
            if (root.TryGetValue("divineManifestations", out var d) && d is List<object> dl)
                foreach (var x in dl) count += UpsertMonster((Dictionary<string, object>)x, NeutralTier.DivineManifestation, true);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log($"[Project D] Imported/updated {count} summon + divine objective assets.");
        }

        private static int UpsertMonster(Dictionary<string, object> d, NeutralTier tier, bool major)
        {
            string id = Str(d, "monsterId"), display = Str(d, "displayName");
            var buff = UpsertBuff(id + "_reward", display + " Reward", Str(d, "reward"),
                                  major ? BuffApplication.EntireTeam : BuffApplication.KillerOnly);
            string path = $"{SummonsDir}/Monster_{id}.asset";
            var mon = AssetDatabase.LoadAssetAtPath<NeutralMonsterDefinition>(path);
            if (mon == null) { mon = ScriptableObject.CreateInstance<NeutralMonsterDefinition>(); AssetDatabase.CreateAsset(mon, path); }
            mon.monsterId = id; mon.displayName = display; mon.tier = tier; mon.signatureMechanic = Str(d, "domain");
            mon.rewardBuff = buff; mon.teamGold = major ? 0 : 60;
            mon.maxHealth = tier == NeutralTier.ElderSummon ? 9000 : (major ? 6000 : 1200);
            mon.respawnSeconds = major ? 300 : 90;
            EditorUtility.SetDirty(mon); return 1;
        }

        [MenuItem("Tools/Project D/Import Champion Data", priority = 21)]
        public static void ImportChampions()
        {
            EnsureFolders();
            var root = ReadJson(ChampionSeed); if (root == null) return;
            if (!root.TryGetValue("champions", out var champs) || champs is not List<object> list)
            { Debug.LogError("[Project D] champions.seed.json missing 'champions'."); return; }
            int count = 0;
            foreach (var c in list) count += UpsertChampion((Dictionary<string, object>)c);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log($"[Project D] Imported/updated {count} champions (+ abilities).");
        }

        private static int UpsertChampion(Dictionary<string, object> c)
        {
            string id = Str(c, "championId");
            string path = $"{ChampionsDir}/Champion_{id}.asset";
            var champ = AssetDatabase.LoadAssetAtPath<ChampionDefinition>(path);
            if (champ == null) { champ = ScriptableObject.CreateInstance<ChampionDefinition>(); AssetDatabase.CreateAsset(champ, path); }

            champ.championId = id;
            champ.displayName = Str(c, "displayName");
            champ.sourceWorld = Str(c, "sourceWorld");
            champ.sourceReleaseDate = Str(c, "sourceReleaseDate");
            champ.primaryRole = ParseRole(Str(c, "primaryRole"));
            champ.secondaryRole = ParseRole(Str(c, "secondaryRole"));
            champ.coreFantasy = Str(c, "coreFantasy");
            champ.mechanicalDifficulty = (int)Num(c, "difficulty", 3);
            champ.availableInStarterRoster = Boolean(c, "starter");

            // Extended bio (feeds Champion Select + codex).
            champ.loreSummary = Str(c, "lore");
            champ.playstyle = Str(c, "playstyle");
            champ.strengths = Str(c, "strengths");
            champ.weaknesses = Str(c, "weaknesses");
            champ.tipsAndCombos = Str(c, "tips");

            if (c.TryGetValue("stats", out var s) && s is Dictionary<string, object> st)
            {
                champ.baseHealth = Num(st, "health", 600); champ.baseResource = Num(st, "resource", 300);
                champ.baseAttackDamage = Num(st, "attackDamage", 60); champ.baseMagicPower = Num(st, "magicPower", 0);
                champ.baseArmor = Num(st, "armor", 30); champ.baseMagicResist = Num(st, "magicResist", 30);
                champ.baseMoveSpeed = Num(st, "moveSpeed", 3.5f); champ.baseAttackRange = Num(st, "attackRange", 1.75f);
                champ.baseAttacksPerSecond = Num(st, "attacksPerSecond", 0.65f);
            }
            if (c.TryGetValue("growth", out var g) && g is Dictionary<string, object> gr)
            {
                champ.healthGrowth = Num(gr, "health", 90); champ.attackDamageGrowth = Num(gr, "attackDamage", 3.5f);
                champ.armorGrowth = Num(gr, "armor", 4f); champ.magicResistGrowth = Num(gr, "magicResist", 1.5f);
            }
            if (c.TryGetValue("abilities", out var a) && a is Dictionary<string, object> ab)
            {
                champ.passive = UpsertAbility(id, "passive", AbilitySlot.Passive, ab);
                champ.abilityQ = UpsertAbility(id, "q", AbilitySlot.Q, ab);
                champ.abilityW = UpsertAbility(id, "w", AbilitySlot.W, ab);
                champ.abilityE = UpsertAbility(id, "e", AbilitySlot.E, ab);
                champ.ultimate = UpsertAbility(id, "ultimate", AbilitySlot.Ultimate, ab);
            }
            EditorUtility.SetDirty(champ); return 1;
        }

        private static AbilityDefinition UpsertAbility(string champId, string key, AbilitySlot slot, Dictionary<string, object> abilities)
        {
            if (!abilities.TryGetValue(key, out var raw) || raw is not Dictionary<string, object> a) return null;
            string path = $"{AbilitiesDir}/Ability_{champId}_{key}.asset";
            var ab = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(path);
            if (ab == null) { ab = ScriptableObject.CreateInstance<AbilityDefinition>(); AssetDatabase.CreateAsset(ab, path); }
            ab.abilityId = $"{champId}_{key}"; ab.displayName = Str(a, "name"); ab.slot = slot;
            ab.designerNotes = Str(a, "notes"); ab.targeting = ParseTargeting(Str(a, "targeting"));
            ab.damageType = ParseDamage(Str(a, "type")); ab.range = Num(a, "range", 6f); ab.radius = Num(a, "radius", 1.5f);
            ab.baseCooldown = Num(a, "cooldown", 8f); ab.resourceCost = Num(a, "cost", 40f);
            ab.damageRatio = Num(a, "ratio", 0.5f); ab.controlDurationSeconds = Num(a, "control", 0f);
            ab.baseDamagePerRank = NumArray(a, "damage", new[] { 60f, 100f, 140f, 180f });
            EditorUtility.SetDirty(ab); return ab;
        }

        private static BuffDefinition UpsertBuff(string id, string display, string desc, BuffApplication app)
        {
            string path = $"{BuffsDir}/Buff_{id}.asset";
            var buff = AssetDatabase.LoadAssetAtPath<BuffDefinition>(path);
            if (buff == null) { buff = ScriptableObject.CreateInstance<BuffDefinition>(); AssetDatabase.CreateAsset(buff, path); }
            buff.buffId = id; buff.displayName = display; buff.description = desc; buff.application = app;
            buff.durationSeconds = app == BuffApplication.EntireTeam ? 120f : 90f;
            EditorUtility.SetDirty(buff); return buff;
        }

        private static Dictionary<string, object> ReadJson(string path)
        {
            if (!File.Exists(path)) { Debug.LogError($"[Project D] Seed not found: {path}"); return null; }
            var parsed = MiniJSON.Deserialize(File.ReadAllText(path)) as Dictionary<string, object>;
            if (parsed == null) Debug.LogError($"[Project D] Failed to parse: {path}");
            return parsed;
        }

        private static void EnsureFolders()
        {
            foreach (var dir in new[] { Root, ChampionsDir, SummonsDir, MonstersDir, BuffsDir, AbilitiesDir })
                if (!AssetDatabase.IsValidFolder(dir))
                    AssetDatabase.CreateFolder(Path.GetDirectoryName(dir).Replace('\\', '/'), Path.GetFileName(dir));
        }

        private static string Str(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out var v) && v != null ? v.ToString() : string.Empty;
        private static float Num(Dictionary<string, object> d, string k, float f)
        { if (d != null && d.TryGetValue(k, out var v) && v != null) { if (v is long l) return l; if (v is double db) return (float)db; if (float.TryParse(v.ToString(), out var r)) return r; } return f; }
        private static bool Boolean(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out var v) && v is bool b && b;
        private static float[] NumArray(Dictionary<string, object> d, string k, float[] f)
        { if (d != null && d.TryGetValue(k, out var v) && v is List<object> list && list.Count > 0) { var a = new float[list.Count]; for (int i = 0; i < list.Count; i++) { var it = list[i]; a[i] = it is long l ? l : it is double db ? (float)db : 0f; } return a; } return f; }
        private static ChampionRole ParseRole(string s) => System.Enum.TryParse<ChampionRole>(s, true, out var r) ? r : ChampionRole.Fighter;
        private static DamageType ParseDamage(string s) => System.Enum.TryParse<DamageType>(s, true, out var r) ? r : DamageType.Adaptive;
        private static TargetingMode ParseTargeting(string s) => System.Enum.TryParse<TargetingMode>(s, true, out var r) ? r : TargetingMode.SkillshotLine;
    }
}
#endif
