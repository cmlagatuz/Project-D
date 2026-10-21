# Store — Sell, Undo & Build Recommendations

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

Adds the transactional store behaviors players expect, all server-authoritative
(the server owns the truth; the client mirrors for instant UI and reconciles).

## Sell
`StoreTransactionManager.Sell(slotIndex, serverTime, canSellNow, out refund)`
- Refunds the item's **sell value** = `totalCost × sellRefund` (default **60%**;
  consumables refund 100%). Frees the slot (stack-aware) and adds gold.
- Gated by `canSellNow` so selling is only allowed in shop-usable states
  (out of combat / near the fountain — the caller decides).
- Selling an item **invalidates** any pending full-refund undo for it.

## Undo (full refund within a grace window)
`StoreTransactionManager.Undo(serverTime, shopAvailable, out refund)`
- Reverses the **most recent eligible purchase** for the **full price** — the
  standard MOBA "recently bought" reversal, distinct from selling.
- Allowed only while `shopAvailable` and within `UndoWindowSeconds` (default **10s**).
- Requires the bought item to still be in the slot it was placed in
  (`InventoryChanged` otherwise). `Tick(serverTime)` expires stale eligibility.
- `CanUndo(serverTime, shopAvailable)` drives the UI button's enabled state.

### Buy → Undo/Sell lifecycle
```
Buy(item)         → gold -cost, placed in slot, marked undo-eligible (t0)
  within 10s + shop open → Undo → gold +cost (FULL), slot cleared
  after window / in combat → Undo blocked; use Sell → gold +60%, slot cleared
```

## Build recommendations
Per-champion recommended order: **Starter → Boots → core legendaries (incl. the
champion-exclusive) → situational**.
- `StoreModel.RegisterBuild(championId, orderedItemIds)` — load from
  `build_recommendations.seed.json` (via `StoreCatalogLoader.RegisterBuilds`).
- `StoreModel.BuildFor(championId)` — the full ordered list.
- `StoreModel.NextRecommended(championId, inventory)` — the next item the player
  does **not** own yet (skips owned, respects champion-exclusive availability).
  Wire this to a "Recommended" highlight / quick-buy button in the shop.

Each build satisfies the slot rule: exactly one boots (dedicated slot) and its own
champion-exclusive; the non-boots core fits the 5 general slots. See
`docs/build-recommendations.md` for all 10 tables.

## Wiring (server-authoritative)
```
StoreCatalogLoader.RegisterItems(store, allItemDefinitions);      // catalog + sell refunds
StoreCatalogLoader.RegisterBuilds(store, buildSeedJsonText);      // per-champion builds
store.SetLockedChampion(draftResult.championId);                  // from Champion Select
var tx = new StoreTransactionManager(store, inventory, startingGold);

// each shop action: client predicts, server runs the SAME calls authoritatively
tx.Buy(id, serverTime, out slot);
tx.Sell(slot, serverTime, canSellNow: shopUsable, out refund);
tx.Undo(serverTime, shopAvailable: shopUsable, out refund);
tx.Tick(serverTime);                                             // expire undo window
```
`InventoryModel.AggregateStats()` continues to feed your `StatBlock` after any change.
