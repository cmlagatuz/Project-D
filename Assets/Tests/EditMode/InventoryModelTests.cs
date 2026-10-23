// Project D | Codename: Zantetsuken — InventoryModelTests.cs (6-slot inventory)
using NUnit.Framework;
using ProjectD.Store;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class InventoryModelTests
    {
        private StoreModel _store;
        private InventoryModel _inventory;

        [SetUp]
        public void SetUp()
        {
            _store = TestItems.Catalog();
            _inventory = new InventoryModel(_store);
        }

        [Test]
        public void NewInventory_IsEmpty()
        {
            Assert.AreEqual(InventoryModel.SlotCount, _inventory.Slots.Count);
            Assert.IsFalse(_inventory.IsFull);
            Assert.IsFalse(_inventory.HasBoots);
            for (int i = 0; i < InventoryModel.SlotCount; i++)
                Assert.IsTrue(_inventory.GetSlot(i).IsEmpty);
        }

        [Test]
        public void Place_FillsGeneralSlotsInOrder()
        {
            Assert.AreEqual(0, _inventory.Place(_store.Get("sword")));
            Assert.AreEqual(1, _inventory.Place(_store.Get("shield")));
            Assert.AreEqual(2, _inventory.Place(_store.Get("ring")));
        }

        [Test]
        public void Place_WhenGeneralSlotsFull_ReturnsMinusOneAndCanPlaceIsFalse()
        {
            TestItems.FillGeneralSlots(_inventory, _store);

            Assert.IsTrue(_inventory.IsFull);
            Assert.IsFalse(_inventory.CanPlace(_store.Get("crown")));
            Assert.AreEqual(-1, _inventory.Place(_store.Get("crown")));
        }

        [Test]
        public void Boots_UseSlotSixOnly_AndOnlyOnePair()
        {
            Assert.AreEqual(InventoryModel.BootsSlotIndex, _inventory.Place(_store.Get("boots_a")));
            Assert.IsTrue(_inventory.HasBoots);
            Assert.IsFalse(_inventory.CanPlace(_store.Get("boots_b")));
            Assert.AreEqual(-1, _inventory.Place(_store.Get("boots_b")));
        }

        [Test]
        public void Boots_DoNotConsumeAGeneralSlot()
        {
            _inventory.Place(_store.Get("boots_a"));
            Assert.IsFalse(_inventory.IsFull);
            Assert.AreEqual(0, _inventory.Place(_store.Get("sword")));
        }

        [Test]
        public void RemoveOne_FreesSlot_AndRejectsBadIndexes()
        {
            int slot = _inventory.Place(_store.Get("sword"));

            Assert.IsTrue(_inventory.RemoveOne(slot));
            Assert.IsTrue(_inventory.GetSlot(slot).IsEmpty);
            Assert.IsFalse(_inventory.RemoveOne(slot));   // already empty
            Assert.IsFalse(_inventory.RemoveOne(-1));
            Assert.IsFalse(_inventory.RemoveOne(InventoryModel.SlotCount));
        }

        [Test]
        public void RemoveOne_OnStack_DecrementsInsteadOfClearing()
        {
            _inventory.Place(_store.Get("potion"));
            _inventory.Place(_store.Get("potion"));

            _inventory.RemoveOne(0);

            Assert.AreEqual(1, _inventory.GetSlot(0).Stack);
            Assert.IsTrue(_inventory.Contains("potion"));
        }

        [Test]
        public void Contains_ReflectsPlacedItems()
        {
            Assert.IsFalse(_inventory.Contains("sword"));
            _inventory.Place(_store.Get("sword"));
            Assert.IsTrue(_inventory.Contains("sword"));
        }

        [Test]
        public void AggregateStats_SumsAcrossSlots_AndMultipliesByStack()
        {
            _store.Register(TestItems.Make("blade", 700,
                stats: new[] { new StatModView { stat = StatType.AttackDamage, value = 10f } }));
            _store.Register(TestItems.Make("tonic", 100, maxStack: 3,
                stats: new[] { new StatModView { stat = StatType.AttackDamage, value = 2f },
                               new StatModView { stat = StatType.Health, value = 5f } }));

            _inventory.Place(_store.Get("blade"));
            _inventory.Place(_store.Get("tonic"));
            _inventory.Place(_store.Get("tonic"));   // stack of 2

            var totals = _inventory.AggregateStats();

            Assert.AreEqual(14f, totals[StatType.AttackDamage], 0.0001f);   // 10 + 2*2
            Assert.AreEqual(10f, totals[StatType.Health], 0.0001f);         // 5*2
        }

        [Test]
        public void AggregateStats_ItemWithNoStats_IsIgnored()
        {
            _inventory.Place(_store.Get("sword"));   // Stats == null
            Assert.AreEqual(0, _inventory.AggregateStats().Count);
        }
    }
}
