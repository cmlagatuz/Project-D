// Project D  |  Codename: Zantetsuken
// P8DemoDriver.cs  (P8 UI — populates whichever content views are present with sample data)

using System.Collections.Generic;
using UnityEngine;
using ProjectD.P8.ItemForge;
using ProjectD.P8.Runes;
using ProjectD.P8.Coaching;
using ProjectD.P8UI;

namespace ProjectD.P8UI
{
    public sealed class P8DemoDriver : MonoBehaviour
    {
        [SerializeField] private ItemBuildTreeView itemTree;
        [SerializeField] private RunePageEditorView runeEditor;
        [SerializeField] private CoachingReportView coaching;

        private void Start()
        {
            if (itemTree)
            {
                var cat = new ItemCatalog();
                cat.Register(new ItemDef { id = "sword", name = "Broadsword", totalCost = 350 });
                cat.Register(new ItemDef { id = "gem", name = "Sorcery Gem", totalCost = 400 });
                cat.Register(new ItemDef { id = "cloak", name = "Null Cloak", totalCost = 300 });
                cat.Register(new ItemDef { id = "runeblade", name = "Runeblade of Baron", totalCost = 1200, componentIds = new[] { "sword", "gem", "cloak" } });
                itemTree.Bind(cat, "runeblade", gold: 900);
                itemTree.OnBought += (id, cost) => Debug.Log($"[Project D] Bought {id} for {cost}g");
            }
            if (runeEditor) runeEditor.Bind(SampleRunes());
            if (coaching) coaching.Bind(new MatchProfile { minutes = 26, csAt10 = 44, deaths = 9, visionScore = 12, kills = 5, assists = 7, objectivesTaken = 0, role = "Mid" });
        }

        private static List<Rune> SampleRunes()
        {
            var runes = new List<Rune>();
            // Precision keystones + runes.
            runes.Add(new Rune { id = "ks_conq", name = "Conqueror", path = RunePath.Precision, keystone = true, stats = { { RuneStat.AttackDamage, 12 } } });
            runes.Add(new Rune { id = "ks_lethal", name = "Lethal Tempo", path = RunePath.Precision, keystone = true, stats = { { RuneStat.AttackDamage, 8 } } });
            runes.Add(new Rune { id = "pr_triumph", name = "Triumph", path = RunePath.Precision, stats = { { RuneStat.Health, 25 } } });
            runes.Add(new Rune { id = "pr_legend", name = "Legend: Alacrity", path = RunePath.Precision, stats = { { RuneStat.AttackDamage, 5 } } });
            runes.Add(new Rune { id = "pr_coup", name = "Coup de Grace", path = RunePath.Precision, stats = { { RuneStat.AttackDamage, 6 } } });
            runes.Add(new Rune { id = "pr_presence", name = "Presence of Mind", path = RunePath.Precision, stats = { { RuneStat.CooldownReduction, 8 } } });
            // Sorcery secondary.
            runes.Add(new Rune { id = "so_manaflow", name = "Manaflow Band", path = RunePath.Sorcery, stats = { { RuneStat.AbilityPower, 10 } } });
            runes.Add(new Rune { id = "so_transcend", name = "Transcendence", path = RunePath.Sorcery, stats = { { RuneStat.CooldownReduction, 10 } } });
            runes.Add(new Rune { id = "so_scorch", name = "Scorch", path = RunePath.Sorcery, stats = { { RuneStat.AbilityPower, 8 } } });
            // Resolve secondary alt.
            runes.Add(new Rune { id = "re_bone", name = "Bone Plating", path = RunePath.Resolve, stats = { { RuneStat.Armor, 15 } } });
            runes.Add(new Rune { id = "re_over", name = "Overgrowth", path = RunePath.Resolve, stats = { { RuneStat.Health, 40 } } });
            return runes;
        }
    }
}
