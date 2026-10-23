// Project D | Codename: Zantetsuken — StoreModelTests.cs (in-match gold shop rules)
using NUnit.Framework;
using ProjectD.Store;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class StoreModelTests
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
        public void Register_MakesItemVisibleToExistsAndGet()
        {
            Assert.IsTrue(_store.Exists("sword"));
            Assert.AreEqual(1000, _store.Get("sword").TotalCost);
            Assert.IsFalse(_store.Exists("missing"));
            Assert.IsNull(_store.Get("missing").ItemId);
        }

        [Test]
        public void IsAvailable_UnknownItem_IsFalse()
        {
            Assert.IsFalse(_store.IsAvailable("missing"));
        }

        [Test]
        public void IsAvailable_ExclusiveItem_OnlyForLockedChampion()
        {
            _store.LockedChampionId = "";
            Assert.IsFalse(_store.IsAvailable("zan_blade"));

            _store.LockedChampionId = "someone_else";
            Assert.IsFalse(_store.IsAvailable("zan_blade"));

            _store.LockedChampionId = "zantetsuken";
            Assert.IsTrue(_store.IsAvailable("zan_blade"));
        }

        [Test]
        public void IsAvailable_GeneralItem_NotAffectedByLockedChampion()
        {
            _store.LockedChampionId = "anyone";
            Assert.IsTrue(_store.IsAvailable("sword"));
        }

        [Test]
        public void TryPurchase_Success_ReturnsCostAndSlotAndPlacesItem()
        {
            var result = _store.TryPurchase("sword", 5000, _inventory, out int cost, out int slot);

            Assert.AreEqual(GoldPurchaseResult.Success, result);
            Assert.AreEqual(1000, cost);
            Assert.AreEqual(0, slot);
            Assert.IsTrue(_inventory.Contains("sword"));
        }

        [Test]
        public void TryPurchase_UnknownItem_IsNotAvailable()
        {
            var result = _store.TryPurchase("missing", 5000, _inventory, out int cost, out int slot);

            Assert.AreEqual(GoldPurchaseResult.NotAvailable, result);
            Assert.AreEqual(0, cost);
            Assert.AreEqual(-1, slot);
        }

        [Test]
        public void TryPurchase_ExclusiveForOtherChampion_IsNotAvailable()
        {
            _store.LockedChampionId = "someone_else";

            var result = _store.TryPurchase("zan_blade", 9999, _inventory, out _, out _);

            Assert.AreEqual(GoldPurchaseResult.NotAvailable, result);
            Assert.IsFalse(_inventory.Contains("zan_blade"));
        }

        [Test]
        public void TryPurchase_NotEnoughGold_ReportsZeroCostAndPlacesNothing()
        {
            var result = _store.TryPurchase("sword", 999, _inventory, out int cost, out int slot);

            Assert.AreEqual(GoldPurchaseResult.NotEnoughGold, result);
            Assert.AreEqual(0, cost);
            Assert.AreEqual(-1, slot);
            Assert.IsFalse(_inventory.Contains("sword"));
        }

        [Test]
        public void TryPurchase_ExactGold_Succeeds()
        {
            var result = _store.TryPurchase("sword", 1000, _inventory, out _, out _);
            Assert.AreEqual(GoldPurchaseResult.Success, result);
        }

        [Test]
        public void TryPurchase_NonStackableAlreadyOwned_IsRejectedBeforeGoldCheck()
        {
            _store.TryPurchase("sword", 5000, _inventory, out _, out _);

            var result = _store.TryPurchase("sword", 0, _inventory, out _, out _);

            Assert.AreEqual(GoldPurchaseResult.AlreadyOwned, result);
        }

        [Test]
        public void TryPurchase_AllGeneralSlotsFull_ReturnsInventoryFull()
        {
            TestItems.FillGeneralSlots(_inventory, _store);

            var result = _store.TryPurchase("crown", 9999, _inventory, out int cost, out int slot);

            Assert.AreEqual(GoldPurchaseResult.InventoryFull, result);
            Assert.AreEqual(0, cost);
            Assert.AreEqual(-1, slot);
        }

        [Test]
        public void TryPurchase_Boots_GoToTheDedicatedBootsSlot()
        {
            TestItems.FillGeneralSlots(_inventory, _store);

            var result = _store.TryPurchase("boots_a", 9999, _inventory, out _, out int slot);

            Assert.AreEqual(GoldPurchaseResult.Success, result);
            Assert.AreEqual(InventoryModel.BootsSlotIndex, slot);
        }

        [Test]
        public void TryPurchase_SecondPairOfBoots_IsRejected()
        {
            _store.TryPurchase("boots_a", 9999, _inventory, out _, out _);

            Assert.AreEqual(GoldPurchaseResult.InventoryFull,
                _store.TryPurchase("boots_b", 9999, _inventory, out _, out _));
            Assert.AreEqual(GoldPurchaseResult.AlreadyOwned,
                _store.TryPurchase("boots_a", 9999, _inventory, out _, out _));
        }

        [Test]
        public void TryPurchase_StackableItem_StacksInOneSlotUntilMax()
        {
            for (int i = 0; i < 3; i++)
                Assert.AreEqual(GoldPurchaseResult.Success,
                    _store.TryPurchase("potion", 9999, _inventory, out _, out _));

            Assert.AreEqual(3, _inventory.GetSlot(0).Stack);
            Assert.IsTrue(_inventory.GetSlot(1).IsEmpty);
        }

        [Test]
        public void TryPurchase_StackableItemAtMax_OverflowsIntoNextFreeSlot()
        {
            for (int i = 0; i < 3; i++) _store.TryPurchase("potion", 9999, _inventory, out _, out _);

            var result = _store.TryPurchase("potion", 9999, _inventory, out _, out int slot);

            Assert.AreEqual(GoldPurchaseResult.Success, result);
            Assert.AreEqual(1, slot);
            Assert.AreEqual(1, _inventory.GetSlot(1).Stack);
        }

        [Test]
        public void TryPurchase_DoesNotModifyAnyGoldLedger()
        {
            // StoreModel only validates + places; the caller owns the gold ledger.
            // (This is the contract StoreScreenView relies on for exclusive purchases.)
            int gold = 5000;
            _store.TryPurchase("sword", gold, _inventory, out int cost, out _);

            Assert.AreEqual(5000, gold);
            Assert.AreEqual(1000, cost);
        }

        [Test]
        public void SellRefundFor_DefaultsToSixtyPercent()
        {
            Assert.AreEqual(0.6f, _store.SellRefundFor("sword"), 0.0001f);
        }

        [Test]
        public void SellRefundFor_HonoursPerItemOverride()
        {
            _store.Register(TestItems.Make("relic", 2000), 0.8f);
            Assert.AreEqual(0.8f, _store.SellRefundFor("relic"), 0.0001f);
        }
    }
}
