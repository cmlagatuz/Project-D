// Project D | Codename: Zantetsuken — ChampionExclusiveControllerTests.cs
using NUnit.Framework;
using ProjectD.Store;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class ChampionExclusiveControllerTests
    {
        private StoreModel _store;
        private InventoryModel _inventory;
        private ChampionExclusiveController _controller;

        [SetUp]
        public void SetUp()
        {
            _store = TestItems.Catalog();
            _inventory = new InventoryModel(_store);
            _controller = new ChampionExclusiveController(_store);
            _controller.Register("zantetsuken", "zan_blade");
            _store.LockedChampionId = "zantetsuken";
        }

        [Test]
        public void ExclusiveItemId_FollowsTheLockedChampion()
        {
            Assert.AreEqual("zan_blade", _controller.ExclusiveItemId);
            Assert.IsTrue(_controller.HasExclusive);

            _store.LockedChampionId = "someone_else";
            Assert.IsNull(_controller.ExclusiveItemId);
            Assert.IsFalse(_controller.HasExclusive);
        }

        [Test]
        public void Register_IgnoresEmptyIds()
        {
            var fresh = new ChampionExclusiveController(_store);
            fresh.Register("", "zan_blade");
            fresh.Register("zantetsuken", "");
            Assert.IsFalse(fresh.HasExclusive);
        }

        [Test]
        public void GetHighlight_NoExclusiveForChampion_IsNone()
        {
            _store.LockedChampionId = "someone_else";
            Assert.AreEqual(ExclusiveHighlight.None, _controller.GetHighlight(9999, _inventory));
        }

        [Test]
        public void GetHighlight_ProgressesThroughUnaffordableNoSlotAvailableOwned()
        {
            Assert.AreEqual(ExclusiveHighlight.Unaffordable, _controller.GetHighlight(1999, _inventory));
            Assert.AreEqual(ExclusiveHighlight.Available, _controller.GetHighlight(2000, _inventory));

            TestItems.FillGeneralSlots(_inventory, _store);
            Assert.AreEqual(ExclusiveHighlight.NoSlot, _controller.GetHighlight(9999, _inventory));
        }

        [Test]
        public void GetHighlight_AfterPurchase_IsOwned()
        {
            _controller.TryPurchaseExclusive(9999, _inventory, out _, out _);
            Assert.AreEqual(ExclusiveHighlight.Owned, _controller.GetHighlight(9999, _inventory));
        }

        [Test]
        public void TryPurchaseExclusive_Success_ThenSecondAttemptIsRejected()
        {
            var first = _controller.TryPurchaseExclusive(5000, _inventory, out int cost, out int slot);
            Assert.AreEqual(GoldPurchaseResult.Success, first);
            Assert.AreEqual(2000, cost);
            Assert.GreaterOrEqual(slot, 0);

            var second = _controller.TryPurchaseExclusive(5000, _inventory, out _, out _);
            Assert.AreEqual(GoldPurchaseResult.NotAvailable, second);
        }

        [Test]
        public void TryPurchaseExclusive_WrongChampion_IsNotAvailable()
        {
            _store.LockedChampionId = "someone_else";
            var result = _controller.TryPurchaseExclusive(9999, _inventory, out _, out _);
            Assert.AreEqual(GoldPurchaseResult.NotAvailable, result);
        }

        [Test]
        public void TryPurchaseExclusive_DoesNotTouchTheGoldLedger_CallerMustDeduct()
        {
            // Regression guard for StoreScreenView: the exclusive path places the item but
            // never spends gold, so the caller has to deduct `cost` itself.
            var tx = new StoreTransactionManager(_store, _inventory, 5000);

            var result = _controller.TryPurchaseExclusive(tx.Gold, _inventory, out int cost, out _);

            Assert.AreEqual(GoldPurchaseResult.Success, result);
            Assert.AreEqual(5000, tx.Gold);

            tx.AddGold(-cost);
            Assert.AreEqual(3000, tx.Gold);
        }
    }
}
