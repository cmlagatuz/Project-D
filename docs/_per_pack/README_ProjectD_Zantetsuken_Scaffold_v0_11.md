# Project D — In-Game Store UI + Champion-Exclusive Flow

> Codename **Zantetsuken** · CONFIDENTIAL

The shop screen for the store economy: category tabs, role recommendations, a
detail/build panel, gold + 6-slot inventory (dedicated boots slot), and the
champion-exclusive purchase logic with its gold UI highlight. Item **names are Final
Fantasy placeholders**; stats/costs/UI are original (see `docs/LICENSE-NOTICE.md`).

## Contents
- `Scripts/Store/` — item data + engine-agnostic rules (ItemDefinition, InventoryModel,
  StoreModel, **ChampionExclusiveController**).
- `Scripts/UI/Store/` — runtime UI: StoreScreenView, StoreItemCard, StoreCategoryTab,
  StoreCatalogLoader.
- `Scripts/Editor/` — StoreItemImporter + **StoreScreenBuilder** (Tools ▸ Project D).
- `Scripts/Tests/` — StoreTests + ExclusiveStoreTests.
- `GameData/Items/items.seed.json` — 86-item catalog.
- `docs/store-ui.md` — screen + exclusive flow · `docs/LICENSE-NOTICE.md`.

## Run it
Tools ▸ Project D ▸ **Import Store Items** → **Build Store Screen** → Play.
Bartz is locked in by default, so the gold **Exclusive** tab is live (Chicken Knife).

## Champion-exclusive highlights
The exclusive tab/card only appears when its champion is picked, uses a gold frame +
EXCLUSIVE ribbon, allows one copy per player, and takes a general slot (never boots).
Wire on draft lock-in: `SetLockedChampion(championId, role)`.


## Sell, Undo & Build Recommendations (this update)
- **Sell** — refund an item for its sell value (default 60%; consumables 100%), stack-aware, gated to shop-usable states.
- **Undo** — reverse the most recent purchase for a **full refund** within a grace window (default 10s) while the shop is open — the standard MOBA "recently bought" reversal.
- **Build recommendations** — per-champion order (Starter → Boots → core legendaries incl. the champion-exclusive → situational). `NextRecommended()` surfaces the next unowned item for a quick-buy highlight.
- New: `StoreTransactionManager` (buy/sell/undo + history), `StoreModel` sell-refund + build APIs, `StoreCatalogLoader` (populate the model from assets + build seed).
- New data: `GameData/Items/build_recommendations.seed.json` (10 validated builds).
- New tests: `SellUndoTests` (sell math, undo window/expiry/shop-gate, most-recent-only, build next-recommended).
- Docs: `docs/sell-undo-builds.md`, `docs/build-recommendations.md`.


## Store UI: Sell / Undo buttons + Recommended quick-buy row (this update)
- **SELL** button sells the selected inventory slot (click a slot to select); shows the refund (60%, consumables 100%).
- **UNDO** button reverses the last purchase for a full refund within a live countdown window (default 10s) with a draining fill.
- **Recommended quick-buy row** shows the locked champion's next unowned build items (one-tap BUY, next item accented).
- `StoreScreenView` now runs everything through `StoreTransactionManager`; the scene builder constructs + wires the new UI.
- `SetShopAvailable(bool)` gates sell/undo to shop-usable states. Docs: `docs/store-ui-sell-undo.md`.


## Loadout Editor (this update)
A designer window (**Tools ▸ Project D ▸ Loadout Editor**) to author each champion's quick-buy build order:
- Champion dropdown + reorderable build list (drag to reorder, category tags, costs, missing-id highlighting).
- Searchable, category-filtered item palette; exclusives limited to the selected champion.
- **Live validation** (one boots, own exclusive, 5-general-slot fit, duplicates) + approximate build cost.
- **Auto-Sort** to the canonical order and **Save** back to `build_recommendations.seed.json`.
- New: `BuildValidator` (engine-agnostic, shared with tests) + `LoadoutEditorWindow`. Docs: `docs/loadout-editor.md`.
