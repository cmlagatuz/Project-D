// Project D  |  Codename: Zantetsuken
// BuildValidator.cs  (Store — validate a champion's recommended build order)
//
// Engine-agnostic checks that a quick-buy build obeys the same rules the store and
// inventory enforce, so the loadout editor can warn designers before they save:
//   - every item id exists in the catalog,
//   - exactly ONE boots item (the dedicated boots slot),
//   - the champion's OWN exclusive is present (and no foreign exclusive),
//   - the non-boots "core" fits the 5 general slots (a full build is buildable),
//   - no duplicate ids.
//
// Pure C# — no UnityEngine — so it is unit-testable and shared by the editor + tests.

using System.Collections.Generic;

namespace ProjectD.Store
{
    public struct BuildIssue
    {
        public enum Level { Error, Warning, Info }
        public Level Severity;
        public string Message;
    }

    public interface IBuildCatalog
    {
        bool Exists(string itemId);
        ItemCategory CategoryOf(string itemId);
        string ExclusiveChampionOf(string itemId);   // "" if not exclusive
        int TotalCostOf(string itemId);
        string DisplayNameOf(string itemId);
    }

    public static class BuildValidator
    {
        /// <summary>Items that occupy a GENERAL slot (everything except boots/consumables/trinkets which stack).</summary>
        public static bool OccupiesGeneralSlot(ItemCategory c)
            => c != ItemCategory.Boots; // boots use the dedicated slot; others use general slots

        public static bool IsCoreGeneral(ItemCategory c)
            => c == ItemCategory.Basic || c == ItemCategory.Epic ||
               c == ItemCategory.Legendary || c == ItemCategory.ChampionExclusive || c == ItemCategory.Starter;

        public static List<BuildIssue> Validate(string championId, IReadOnlyList<string> order, IBuildCatalog catalog)
        {
            var issues = new List<BuildIssue>();
            if (order == null || order.Count == 0)
            {
                issues.Add(new BuildIssue { Severity = BuildIssue.Level.Warning, Message = "Build is empty." });
                return issues;
            }

            var seen = new HashSet<string>();
            int bootsCount = 0, ownExclusive = 0, coreGeneral = 0;

            foreach (var id in order)
            {
                if (string.IsNullOrEmpty(id)) { issues.Add(Err("Empty item slot in the order.")); continue; }
                if (!catalog.Exists(id)) { issues.Add(Err($"Unknown item id: '{id}'.")); continue; }
                if (!seen.Add(id)) issues.Add(Warn($"Duplicate item: {catalog.DisplayNameOf(id)}."));

                var cat = catalog.CategoryOf(id);
                if (cat == ItemCategory.Boots) bootsCount++;

                string exc = catalog.ExclusiveChampionOf(id);
                if (!string.IsNullOrEmpty(exc))
                {
                    if (exc == championId) ownExclusive++;
                    else issues.Add(Err($"{catalog.DisplayNameOf(id)} is exclusive to another champion ('{exc}')."));
                }

                // Core items that consume a general inventory slot in a finished build.
                if (cat == ItemCategory.Basic || cat == ItemCategory.Epic ||
                    cat == ItemCategory.Legendary || cat == ItemCategory.ChampionExclusive || cat == ItemCategory.Starter)
                    coreGeneral++;
            }

            if (bootsCount == 0) issues.Add(Warn("No boots in the build — the dedicated boots slot will be unused."));
            if (bootsCount > 1) issues.Add(Err($"{bootsCount} boots in the build — only ONE boots slot exists."));

            if (ownExclusive == 0) issues.Add(Warn($"Champion's exclusive item is not in the build."));
            if (ownExclusive > 1) issues.Add(Err("More than one champion-exclusive item in the build."));

            // A FINISHED build is 5 general + 1 boots. A recommendation may list more than
            // 5 general items (early -> late), but flag if the intended *final* set can't fit.
            // Heuristic: the last 5 non-boots core items are the intended endgame.
            if (coreGeneral > 5)
                issues.Add(Info($"{coreGeneral} general items listed — only the final 5 fit at once (early items get sold/replaced)."));

            if (issues.Count == 0)
                issues.Add(new BuildIssue { Severity = BuildIssue.Level.Info, Message = "Build is valid." });

            return issues;
        }

        public static int ApproxCost(IReadOnlyList<string> order, IBuildCatalog catalog, bool includeConsumables = false)
        {
            int total = 0;
            foreach (var id in order)
            {
                if (string.IsNullOrEmpty(id) || !catalog.Exists(id)) continue;
                if (!includeConsumables && catalog.CategoryOf(id) == ItemCategory.Consumable) continue;
                total += catalog.TotalCostOf(id);
            }
            return total;
        }

        private static BuildIssue Err(string m) => new BuildIssue { Severity = BuildIssue.Level.Error, Message = m };
        private static BuildIssue Warn(string m) => new BuildIssue { Severity = BuildIssue.Level.Warning, Message = m };
        private static BuildIssue Info(string m) => new BuildIssue { Severity = BuildIssue.Level.Info, Message = m };
    }
}
