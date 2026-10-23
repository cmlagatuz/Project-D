// Project D | Codename: Zantetsuken — StoreTransactionManagerTests.cs (buy / sell / undo)
using NUnit.Framework;
using ProjectD.Store;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class StoreTransactionManagerTests
    {
        private StoreModel _store;
        private InventoryModel _inventory;
        private StoreTransactionManager _tx;

        [SetUp]
        public void SetUp()
        {
            _store = TestItems.Catalog();
            _inventory = new InventoryModel(_store);
            _tx = new StoreTransactionManager(_store, _inventory, 5000);
        }

        // ---- BUY ---- //

        [Test]
        public void Buy_DeductsGold_PlacesItem_AndRecordsHistory()
        {
            var result = _tx.Buy("sword", 0f, out int slot);

            Assert.AreEqual(GoldPurchaseResult.Success, result);
            Assert.AreEqual(4000, _tx.Gold);
            Assert.AreEqual("sword", _inventory.GetSlot(slot).ItemId);
            Assert.AreEqual(1, _tx.History.Count);
            Assert.AreEqual(TxKind.Buy, _tx.History[0].Kind);
            Assert.AreEqual(-1000, _tx.History[0].GoldDelta);
            Assert.IsTrue(_tx.History[0].FullRefundEligible);
        }

        [Test]
        public void Buy_NotEnoughGold_ChangesNothing()
        {
            var poor = new StoreTransactionManager(_store, _inventory, 100);

            var result = poor.Buy("sword", 0f, out _);

            Assert.AreEqual(GoldPurchaseResult.NotEnoughGold, result);
            Assert.AreEqual(100, poor.Gold);
            Assert.AreEqual(0, poor.History.Count);
            Assert.IsFalse(_inventory.Contains("sword"));
        }

        // ---- SELL ---- //

        [Test]
        public void Sell_RefundsSixtyPercent_AndFreesTheSlot()
        {
            _tx.Buy("sword", 0f, out int slot);

            var result = _tx.Sell(slot, 1f, true, out int refund);

            Assert.AreEqual(SellResult.Success, result);
            Assert.AreEqual(600, refund);
            Assert.AreEqual(4600, _tx.Gold);
            Assert.IsTrue(_inventory.GetSlot(slot).IsEmpty);
            Assert.AreEqual(TxKind.Sell, _tx.History[_tx.History.Count - 1].Kind);
        }

        [Test]
        public void Sell_RoundsToNearestWholeGold()
        {
            _store.Register(TestItems.Make("odd", 333));
            _tx.Buy("odd", 0f, out int slot);

            _tx.Sell(slot, 1f, true, out int refund);

            Assert.AreEqual(200, refund);   // 333 * 0.6 = 199.8
        }

        [Test]
        public void Sell_UsesPerItemRefundOverride()
        {
            _store.Register(TestItems.Make("relic", 2000), 0.5f);
            _tx.Buy("relic", 0f, out int slot);

            _tx.Sell(slot, 1f, true, out int refund);

            Assert.AreEqual(1000, refund);
        }

        [Test]
        public void Sell_FromStack_RemovesOnlyOne()
        {
            _tx.Buy("potion", 0f, out int slot);
            _tx.Buy("potion", 0f, out _);

            _tx.Sell(slot, 1f, true, out int refund);

            Assert.AreEqual(30, refund);
            Assert.AreEqual(1, _inventory.GetSlot(slot).Stack);
        }

        [Test]
        public void Sell_WhenNotAllowed_ReturnsCannotSellNowAndKeepsItem()
        {
            _tx.Buy("sword", 0f, out int slot);

            var result = _tx.Sell(slot, 1f, false, out int refund);

            Assert.AreEqual(SellResult.CannotSellNow, result);
            Assert.AreEqual(0, refund);
            Assert.IsTrue(_inventory.Contains("sword"));
        }

        [Test]
        public void Sell_EmptyOrOutOfRangeSlot_ReturnsEmptySlot()
        {
            Assert.AreEqual(SellResult.EmptySlot, _tx.Sell(0, 0f, true, out _));
            Assert.AreEqual(SellResult.EmptySlot, _tx.Sell(-1, 0f, true, out _));
            Assert.AreEqual(SellResult.EmptySlot, _tx.Sell(InventoryModel.SlotCount, 0f, true, out _));
        }

        // ---- UNDO ---- //

        [Test]
        public void Undo_WithinWindow_GivesAFullRefund()
        {
            _tx.Buy("sword", 0f, out _);

            var result = _tx.Undo(5f, true, out int refund);

            Assert.AreEqual(UndoResult.Success, result);
            Assert.AreEqual(1000, refund);
            Assert.AreEqual(5000, _tx.Gold);
            Assert.IsFalse(_inventory.Contains("sword"));
            Assert.AreEqual(TxKind.Undo, _tx.History[_tx.History.Count - 1].Kind);
        }

        [Test]
        public void Undo_ExactlyAtTheWindowEdge_StillWorks_JustPastItDoesNot()
        {
            _tx.Buy("sword", 0f, out _);
            Assert.IsTrue(_tx.CanUndo(10f, true));
            Assert.IsFalse(_tx.CanUndo(10.01f, true));

            Assert.AreEqual(UndoResult.WindowExpired, _tx.Undo(10.01f, true, out int refund));
            Assert.AreEqual(0, refund);
            Assert.IsTrue(_inventory.Contains("sword"));
        }

        [Test]
        public void Undo_AfterTickExpiredTheEligibility_ReportsNothingToUndo()
        {
            _tx.Buy("sword", 0f, out _);
            _tx.Tick(11f);

            Assert.AreEqual(UndoResult.NothingToUndo, _tx.Undo(11f, true, out _));
        }

        [Test]
        public void Undo_ShopUnavailable_IsRejected()
        {
            _tx.Buy("sword", 0f, out _);

            Assert.AreEqual(UndoResult.ShopUnavailable, _tx.Undo(1f, false, out _));
            Assert.IsFalse(_tx.CanUndo(1f, false));
            Assert.IsTrue(_inventory.Contains("sword"));
        }

        [Test]
        public void Undo_WithNoPurchases_ReportsNothingToUndo()
        {
            Assert.AreEqual(UndoResult.NothingToUndo, _tx.Undo(0f, true, out _));
        }

        [Test]
        public void Undo_CannotBeUsedTwiceOnTheSamePurchase()
        {
            _tx.Buy("sword", 0f, out _);
            _tx.Undo(1f, true, out _);

            Assert.AreEqual(UndoResult.NothingToUndo, _tx.Undo(2f, true, out _));
            Assert.AreEqual(5000, _tx.Gold);
        }

        [Test]
        public void Undo_UnwindsPurchasesNewestFirst()
        {
            _tx.Buy("sword", 0f, out _);
            _tx.Buy("shield", 1f, out _);

            _tx.Undo(2f, true, out int firstRefund);
            Assert.AreEqual(800, firstRefund);
            Assert.IsTrue(_inventory.Contains("sword"));
            Assert.IsFalse(_inventory.Contains("shield"));

            _tx.Undo(3f, true, out int secondRefund);
            Assert.AreEqual(1000, secondRefund);
            Assert.AreEqual(5000, _tx.Gold);
        }

        [Test]
        public void Undo_IfTheItemWasRemovedBehindOurBack_ReportsInventoryChanged()
        {
            _tx.Buy("sword", 0f, out int slot);
            _inventory.RemoveOne(slot);   // bypasses the transaction manager

            var result = _tx.Undo(1f, true, out int refund);

            Assert.AreEqual(UndoResult.InventoryChanged, result);
            Assert.AreEqual(0, refund);
            Assert.AreEqual(4000, _tx.Gold);   // no free money
        }

        [Test]
        public void Sell_InvalidatesThePendingUndo_SoGoldCannotBeDuplicated()
        {
            _tx.Buy("sword", 0f, out int slot);
            _tx.Sell(slot, 1f, true, out _);

            Assert.AreEqual(UndoResult.NothingToUndo, _tx.Undo(2f, true, out _));
            Assert.AreEqual(4600, _tx.Gold);   // 5000 - 1000 + 600, nothing more
        }

        [Test]
        public void UndoTimeRemaining_CountsDownAndFloorsAtZero()
        {
            Assert.AreEqual(0f, _tx.UndoTimeRemaining(0f), 0.0001f);   // nothing bought yet

            _tx.Buy("sword", 0f, out _);

            Assert.AreEqual(6f, _tx.UndoTimeRemaining(4f), 0.0001f);
            Assert.AreEqual(0f, _tx.UndoTimeRemaining(20f), 0.0001f);
        }

        // ---- GOLD LEDGER ---- //

        [Test]
        public void AddGold_NeverGoesBelowZero()
        {
            _tx.AddGold(-999999);
            Assert.AreEqual(0, _tx.Gold);
        }

        [Test]
        public void SetGold_OverwritesTheBalance()
        {
            _tx.SetGold(1234);
            Assert.AreEqual(1234, _tx.Gold);
        }
    }
}
