// Project D  |  Codename: Zantetsuken
// StoreModel.cs  (Store — in-match Gil Shop catalog + purchase rules, engine-agnostic)
//
// This is the IN-MATCH gold shop (distinct from the out-of-game KP Store in
// KpStoreModel.cs). Wraps a catalog of ItemView entries (built from ItemDefinition
// assets at load) and answers: is this item visible right now (handles champion-
// exclusive gating via LockedChampionId), and can the player afford + place it.
// Implements IItemLookup so it can back an InventoryModel directly. No UnityEngine —
// unit-testable and safe to run on the server as the source of truth (GCD 14.3).

using System.Collections.Generic;

namespace ProjectD.Store
{
    // Separate from KpStoreModel's PurchaseResult (out-of-game KP purchases have
    // different failure modes). Keeping these distinct avoids conflating two
    // unrelated purchase pipelines that happen to share the "Store" namespace.
    public enum GoldPurchaseResult { Success, NotAvailable, AlreadyOwned, NotEnoughGold, InventoryFull }

    public sealed class StoreModel : IItemLookup
    {
        private readonly Dictionary<string, ItemView> _catalog = new();
        private readonly Dictionary<string, float> _sellRefundOverrides = new();

        /// <summary>The champion currently locked in for this match (drives exclusive-item visibility).</summary>
        public string LockedChampionId { get; set; } = "";

        public void Register(ItemView item, float? sellRefundOverride = null)
        {
            _catalog[item.ItemId] = item;
            if (sellRefundOverride.HasValue) _sellRefundOverrides[item.ItemId] = sellRefundOverride.Value;
        }

        public bool Exists(string itemId) => _catalog.ContainsKey(itemId);

        public ItemView Get(string itemId) => _catalog.TryGetValue(itemId, out var v) ? v : default;

        /// <summary>Visible/buyable right now: exists, and if champion-exclusive, matches the locked champion.</summary>
        public bool IsAvailable(string itemId)
        {
            if (!_catalog.TryGetValue(itemId, out var item)) return false;
            if (!string.IsNullOrEmpty(item.ExclusiveToChampionId))
                return item.ExclusiveToChampionId == LockedChampionId;
            return true;
        }

        /// <summary>
        /// Validate + apply a gold purchase into the given inventory. On success the
        /// item has already been placed and gold is NOT deducted here — the caller
        /// (StoreTransactionManager) owns the gold ledger so it can log history/undo.
        /// </summary>
        public GoldPurchaseResult TryPurchase(string itemId, int gold, InventoryModel inventory, out int cost, out int slot)
        {
            cost = 0; slot = -1;
            if (!IsAvailable(itemId)) return GoldPurchaseResult.NotAvailable;

            var item = _catalog[itemId];
            if (item.MaxStack <= 1 && inventory.Contains(itemId)) return GoldPurchaseResult.AlreadyOwned;

            cost = item.TotalCost;
            if (gold < cost) { cost = 0; return GoldPurchaseResult.NotEnoughGold; }
            if (!inventory.CanPlace(item)) { cost = 0; return GoldPurchaseResult.InventoryFull; }

            slot = inventory.Place(item);
            if (slot < 0) { cost = 0; return GoldPurchaseResult.InventoryFull; }
            return GoldPurchaseResult.Success;
        }

        /// <summary>Sell-back fraction (0..1) for an item; falls back to a 60% default if not overridden.</summary>
        public float SellRefundFor(string itemId)
            => _sellRefundOverrides.TryGetValue(itemId, out var pct) ? pct : 0.6f;
    }
}
