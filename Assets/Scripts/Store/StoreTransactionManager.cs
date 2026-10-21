// Project D  |  Codename: Zantetsuken
// StoreTransactionManager.cs  (Store — buy / sell / undo, engine-agnostic)
//
// Wraps StoreModel + InventoryModel with the transactional rules players expect:
//   - BUY: validated purchase (delegates to StoreModel), recorded in history.
//   - SELL: refund an owned item (sellRefund fraction of totalCost), frees the slot.
//   - UNDO: within a short GRACE WINDOW and only while the shop is usable (out of
//     combat / at fountain, decided by the caller), the last purchase can be undone
//     for a FULL refund. This is the standard MOBA "recently bought" reversal —
//     distinct from selling, which always applies the sell penalty.
//
// All state is authoritative-server friendly: the same instance runs on the server
// (source of truth) and the client mirrors it for instant UI, reconciling on the
// server's response (GCD 14.3). No UnityEngine — unit-testable.

using System.Collections.Generic;

namespace ProjectD.Store
{
    public enum TxKind { Buy, Sell, Undo }

    public struct TxRecord
    {
        public TxKind Kind;
        public string ItemId;
        public int SlotIndex;
        public int GoldDelta;       // + = gold gained (sell/undo), - = gold spent (buy)
        public float ServerTime;    // when it happened (for the undo window)
        public bool FullRefundEligible; // true for a buy that can still be undone
    }

    public enum SellResult { Success, NotOwned, EmptySlot, CannotSellNow }
    public enum UndoResult { Success, NothingToUndo, WindowExpired, ShopUnavailable, InventoryChanged }

    public sealed class StoreTransactionManager
    {
        private readonly StoreModel _store;
        private readonly InventoryModel _inventory;
        private readonly List<TxRecord> _history = new();

        /// <summary>Seconds after a purchase during which a full-refund undo is allowed.</summary>
        public float UndoWindowSeconds { get; set; } = 10f;

        public int Gold { get; private set; }
        public IReadOnlyList<TxRecord> History => _history;

        public StoreTransactionManager(StoreModel store, InventoryModel inventory, int startingGold = 0)
        {
            _store = store; _inventory = inventory; Gold = startingGold;
        }

        public void SetGold(int gold) => Gold = gold;          // server sync
        public void AddGold(int delta) => Gold = System.Math.Max(0, Gold + delta);

        // ---- BUY ---- //
        public GoldPurchaseResult Buy(string itemId, float serverTime, out int slotUsed)
        {
            var result = _store.TryPurchase(itemId, Gold, _inventory, out int cost, out slotUsed);
            if (result != GoldPurchaseResult.Success) return result;

            Gold -= cost;
            _history.Add(new TxRecord
            {
                Kind = TxKind.Buy, ItemId = itemId, SlotIndex = slotUsed,
                GoldDelta = -cost, ServerTime = serverTime, FullRefundEligible = true
            });
            return GoldPurchaseResult.Success;
        }

        // ---- SELL ---- //
        /// <summary>
        /// Sell the item in a slot for its sell value (refund penalty applies).
        /// canSellNow lets the caller gate selling to shop-usable states.
        /// </summary>
        public SellResult Sell(int slotIndex, float serverTime, bool canSellNow, out int refund)
        {
            refund = 0;
            if (!canSellNow) return SellResult.CannotSellNow;
            if (slotIndex < 0 || slotIndex >= InventoryModel.SlotCount) return SellResult.EmptySlot;

            var slot = _inventory.GetSlot(slotIndex);
            if (slot.IsEmpty) return SellResult.EmptySlot;

            var view = _store.Get(slot.ItemId);
            if (string.IsNullOrEmpty(view.ItemId)) return SellResult.NotOwned;

            refund = SellValueOf(view);
            _inventory.RemoveOne(slotIndex);   // removes one (stack-aware)
            Gold += refund;

            // A sold item invalidates a pending undo for that same slot/item.
            InvalidateUndoFor(slot.ItemId, slotIndex);

            _history.Add(new TxRecord
            {
                Kind = TxKind.Sell, ItemId = slot.ItemId, SlotIndex = slotIndex,
                GoldDelta = refund, ServerTime = serverTime, FullRefundEligible = false
            });
            return SellResult.Success;
        }

        // ---- UNDO (full refund of the last eligible purchase) ---- //
        public bool CanUndo(float serverTime, bool shopAvailable)
        {
            if (!shopAvailable) return false;
            int idx = LastUndoableIndex();
            if (idx < 0) return false;
            return serverTime - _history[idx].ServerTime <= UndoWindowSeconds;
        }

        /// <summary>Seconds left on the last eligible purchase's undo window (0 if none).</summary>
        public float UndoTimeRemaining(float serverTime)
        {
            int idx = LastUndoableIndex();
            if (idx < 0) return 0f;
            float remaining = UndoWindowSeconds - (serverTime - _history[idx].ServerTime);
            return remaining > 0f ? remaining : 0f;
        }

        public UndoResult Undo(float serverTime, bool shopAvailable, out int refund)
        {
            refund = 0;
            if (!shopAvailable) return UndoResult.ShopUnavailable;

            int idx = LastUndoableIndex();
            if (idx < 0) return UndoResult.NothingToUndo;

            var tx = _history[idx];
            if (serverTime - tx.ServerTime > UndoWindowSeconds) return UndoResult.WindowExpired;

            // The bought item must still be exactly where we placed it.
            var slot = _inventory.GetSlot(tx.SlotIndex);
            if (slot.IsEmpty || slot.ItemId != tx.ItemId) return UndoResult.InventoryChanged;

            _inventory.RemoveOne(tx.SlotIndex);
            refund = -tx.GoldDelta;            // full price back (buy delta was negative)
            Gold += refund;

            // Mark consumed and log the undo.
            tx.FullRefundEligible = false; _history[idx] = tx;
            _history.Add(new TxRecord
            {
                Kind = TxKind.Undo, ItemId = tx.ItemId, SlotIndex = tx.SlotIndex,
                GoldDelta = refund, ServerTime = serverTime, FullRefundEligible = false
            });
            return UndoResult.Success;
        }

        /// <summary>Expire undo-eligibility for purchases past the window (call each tick).</summary>
        public void Tick(float serverTime)
        {
            for (int i = 0; i < _history.Count; i++)
            {
                var tx = _history[i];
                if (tx.Kind == TxKind.Buy && tx.FullRefundEligible &&
                    serverTime - tx.ServerTime > UndoWindowSeconds)
                { tx.FullRefundEligible = false; _history[i] = tx; }
            }
        }

        // ---- helpers ---- //
        public int SellValueOf(ItemView v)
        {
            // Default 60% unless the catalog carries a per-item refund (kept in StoreModel).
            float refundPct = _store.SellRefundFor(v.ItemId);
            return (int)System.Math.Round(v.TotalCost * refundPct);
        }

        private int LastUndoableIndex()
        {
            for (int i = _history.Count - 1; i >= 0; i--)
                if (_history[i].Kind == TxKind.Buy && _history[i].FullRefundEligible)
                    return i;
            return -1;
        }

        private void InvalidateUndoFor(string itemId, int slotIndex)
        {
            for (int i = _history.Count - 1; i >= 0; i--)
            {
                var tx = _history[i];
                if (tx.Kind == TxKind.Buy && tx.FullRefundEligible &&
                    tx.ItemId == itemId && tx.SlotIndex == slotIndex)
                { tx.FullRefundEligible = false; _history[i] = tx; return; }
            }
        }
    }
}
