// Project D  |  Codename: Zantetsuken
// ChampionExclusiveController.cs  (Store — champion-exclusive purchase logic + highlight)
//
// Encapsulates the exclusive-item rules so both the UI and the server share them:
//   - Resolve which (if any) exclusive item belongs to the locked champion.
//   - Gate visibility: the item is only offered while that champion is locked in.
//   - Enforce a single owned exclusive per player and one general slot of cost.
//   - Provide highlight state (Locked / Available / Owned / Unaffordable) so the UI
//     can render the gold treatment consistently.
//
// Engine-agnostic (no UnityEngine) -> unit-testable + server-reusable (GCD 14.3).

using System.Collections.Generic;

namespace ProjectD.Store
{
    public enum ExclusiveHighlight
    {
        None,          // no exclusive for this champion (or none locked in)
        Available,     // visible & buyable (gold frame, buy enabled)
        Unaffordable,  // visible but not enough gold (gold frame, buy shows deficit)
        NoSlot,        // visible but no free general slot
        Owned          // already purchased (gold frame + owned check)
    }

    public sealed class ChampionExclusiveController
    {
        private readonly StoreModel _store;
        // championId -> exclusive itemId
        private readonly Dictionary<string, string> _exclusiveByChampion = new();

        public ChampionExclusiveController(StoreModel store) { _store = store; }

        /// <summary>Register an exclusive mapping (called once per exclusive item at load).</summary>
        public void Register(string championId, string itemId)
        {
            if (!string.IsNullOrEmpty(championId) && !string.IsNullOrEmpty(itemId))
                _exclusiveByChampion[championId] = itemId;
        }

        /// <summary>The exclusive item id for the currently locked champion, or null.</summary>
        public string ExclusiveItemId
            => _exclusiveByChampion.TryGetValue(_store.LockedChampionId, out var id) ? id : null;

        public bool HasExclusive => !string.IsNullOrEmpty(ExclusiveItemId);

        /// <summary>Compute the highlight state for the locked champion's exclusive.</summary>
        public ExclusiveHighlight GetHighlight(int gold, InventoryModel inventory)
        {
            var itemId = ExclusiveItemId;
            if (string.IsNullOrEmpty(itemId)) return ExclusiveHighlight.None;
            if (!_store.IsAvailable(itemId)) return ExclusiveHighlight.None;

            if (inventory.Contains(itemId)) return ExclusiveHighlight.Owned;

            var view = _store.Get(itemId);
            if (gold < view.TotalCost) return ExclusiveHighlight.Unaffordable;
            if (!inventory.CanPlace(view)) return ExclusiveHighlight.NoSlot;
            return ExclusiveHighlight.Available;
        }

        /// <summary>
        /// Attempt to purchase the locked champion's exclusive. Enforces:
        /// availability (right champion), single-copy, gold, and a free general slot.
        /// Returns the same GoldPurchaseResult the general path uses.
        /// </summary>
        public GoldPurchaseResult TryPurchaseExclusive(int gold, InventoryModel inventory, out int cost, out int slot)
        {
            cost = 0; slot = -1;
            var itemId = ExclusiveItemId;
            if (string.IsNullOrEmpty(itemId)) return GoldPurchaseResult.NotAvailable;
            if (inventory.Contains(itemId)) return GoldPurchaseResult.NotAvailable; // one exclusive only
            return _store.TryPurchase(itemId, gold, inventory, out cost, out slot);
        }
    }
}
