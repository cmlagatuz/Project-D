// Project D | Codename: Zantetsuken — TestItems.cs (shared item builders for the Store tests)
using ProjectD.Store;

namespace ProjectD.Tests
{
    internal static class TestItems
    {
        public static ItemView Make(string id, int cost, bool boots = false, int maxStack = 1,
                                    string exclusiveTo = null, StatModView[] stats = null)
        {
            return new ItemView
            {
                ItemId = id,
                TotalCost = cost,
                IsBoots = boots,
                MaxStack = maxStack,
                ExclusiveToChampionId = exclusiveTo,
                Category = boots ? ItemCategory.Boots : ItemCategory.Basic,
                Stats = stats
            };
        }

        /// <summary>A store pre-loaded with a small, predictable catalog.</summary>
        public static StoreModel Catalog()
        {
            var store = new StoreModel();
            store.Register(Make("sword", 1000));
            store.Register(Make("shield", 800));
            store.Register(Make("ring", 500));
            store.Register(Make("cloak", 400));
            store.Register(Make("gem", 300));
            store.Register(Make("crown", 2500));
            store.Register(Make("boots_a", 300, boots: true));
            store.Register(Make("boots_b", 350, boots: true));
            store.Register(Make("potion", 50, maxStack: 3));
            store.Register(Make("zan_blade", 2000, exclusiveTo: "zantetsuken"));
            return store;
        }

        /// <summary>Fill all five general slots with distinct, non-boots items.</summary>
        public static void FillGeneralSlots(InventoryModel inventory, StoreModel store)
        {
            string[] ids = { "sword", "shield", "ring", "cloak", "gem" };
            foreach (var id in ids) inventory.Place(store.Get(id));
        }
    }
}
