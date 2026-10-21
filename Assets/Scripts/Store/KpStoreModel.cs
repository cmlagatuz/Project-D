// Project D  |  Codename: Zantetsuken
// KpStoreModel.cs  (Store — catalog, ownership, purchase validation; engine-agnostic)
//
// The out-of-game Store logic the client and server both run: what's owned, whether
// a KP purchase is valid (enough KP, level gate met, not already owned, correct
// currency), and the resulting ownership grant. Pure C# — unit-testable and server-
// authoritative (the server runs the same TryPurchase against the account balance).

using System.Collections.Generic;
using ProjectD.Economy;

namespace ProjectD.Store
{
    // Engine-agnostic view of a Store item (so the model needs no UnityEngine).
    public struct StoreItemView
    {
        public string ItemId;
        public StoreCategory Category;
        public int KpPrice;
        public int KpSalePrice;
        public int RequiredAccountLevel;
        public string ChampionId;
        public string[] BundleItemIds;

        public bool OnSale => KpSalePrice > 0 && KpSalePrice < KpPrice;
        public int EffectivePrice => OnSale ? KpSalePrice : KpPrice;
    }

    public enum PurchaseResult { Success, NotEnoughKp, AlreadyOwned, LevelLocked, UnknownItem, WrongCurrency }

    // What the player owns + has equipped (profile showcase).
    public sealed class ProfileInventory
    {
        public readonly HashSet<string> OwnedItemIds = new();
        public readonly HashSet<string> OwnedChampionIds = new();
        public string EquippedBorderId = "";
        public string EquippedIconId = "";

        public bool Owns(string itemId) => OwnedItemIds.Contains(itemId);
        public bool OwnsChampion(string championId) => OwnedChampionIds.Contains(championId);
    }

    public sealed class KpStoreModel
    {
        private readonly Dictionary<string, StoreItemView> _catalog = new();

        public void Register(StoreItemView item) => _catalog[item.ItemId] = item;
        public StoreItemView Get(string id) => _catalog.TryGetValue(id, out var v) ? v : default;
        public bool Exists(string id) => _catalog.ContainsKey(id);
        public int Count => _catalog.Count;

        public IEnumerable<StoreItemView> ByCategory(StoreCategory cat)
        {
            foreach (var kv in _catalog) if (kv.Value.Category == cat) yield return kv.Value;
        }

        /// <summary>Is the item purchasable for this account? (not owned, level met).</summary>
        public bool IsPurchasable(string itemId, ProfileInventory inv, int accountLevel, out PurchaseResult reason)
        {
            reason = PurchaseResult.Success;
            if (!_catalog.TryGetValue(itemId, out var item)) { reason = PurchaseResult.UnknownItem; return false; }
            if (IsOwned(item, inv)) { reason = PurchaseResult.AlreadyOwned; return false; }
            if (item.RequiredAccountLevel > 0 && accountLevel < item.RequiredAccountLevel) { reason = PurchaseResult.LevelLocked; return false; }
            return true;
        }

        /// <summary>
        /// Validate + resolve a KP purchase. On success returns the KP cost; the caller
        /// deducts KP (AccountProgressionService.SpendKp) and applies GrantOwnership.
        /// payWith MUST be KP (Store guardrail).
        /// </summary>
        public PurchaseResult TryPurchase(string itemId, long kpBalance, int accountLevel,
                                          ProfileInventory inv, CurrencyType payWith, out int cost)
        {
            cost = 0;
            if (!EconomyTerms.IsValidPurchase(EconomyScope.Store, payWith)) return PurchaseResult.WrongCurrency;
            if (!_catalog.TryGetValue(itemId, out var item)) return PurchaseResult.UnknownItem;
            if (IsOwned(item, inv)) return PurchaseResult.AlreadyOwned;
            if (item.RequiredAccountLevel > 0 && accountLevel < item.RequiredAccountLevel) return PurchaseResult.LevelLocked;

            cost = item.EffectivePrice;
            if (kpBalance < cost) { cost = 0; return PurchaseResult.NotEnoughKp; }
            return PurchaseResult.Success;
        }

        /// <summary>Apply ownership after a successful, deducted purchase (idempotent).</summary>
        public void GrantOwnership(string itemId, ProfileInventory inv)
        {
            if (!_catalog.TryGetValue(itemId, out var item)) return;
            inv.OwnedItemIds.Add(item.ItemId);
            if (item.Category == StoreCategory.Champion && !string.IsNullOrEmpty(item.ChampionId))
                inv.OwnedChampionIds.Add(item.ChampionId);
            if (item.Category == StoreCategory.Bundle && item.BundleItemIds != null)
                foreach (var bid in item.BundleItemIds) GrantOwnership(bid, inv);
        }

        private static bool IsOwned(StoreItemView item, ProfileInventory inv)
        {
            if (inv.Owns(item.ItemId)) return true;
            if (item.Category == StoreCategory.Champion && !string.IsNullOrEmpty(item.ChampionId))
                return inv.OwnsChampion(item.ChampionId);
            return false;
        }
    }
}
