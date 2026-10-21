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
