# Loadout Editor — Quick-Buy Build Authoring

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

A designer window to author each champion's recommended quick-buy build order
(the strip that drives `StoreModel.NextRecommended` and the store's quick-buy row).

Open with **Tools ▸ Project D ▸ Loadout Editor (Quick-Buy Builds)**.

## Layout
- **Toolbar** — champion dropdown, **Auto-Sort**, **Reload**, **Save**.
- **Left: build order** — the selected champion's ordered items as a reorderable
  list. Drag to reorder, or use the − to remove; each row shows step #, a colored
  category tag, the item name, and its cost. Missing/unknown ids show in red.
- **Left: validation** — live checks with an approximate build cost.
- **Right: item palette** — searchable, category-filtered list; click **Add** to
  append. Champion-exclusive items are limited to the selected champion.

## What it validates (mirrors the store slot rules)
- Every item id **exists** in the catalog.
- **Exactly one boots** (the dedicated boots slot); two boots is an error.
- The champion's **own exclusive** is present; a foreign exclusive is an error.
- The finished set fits **5 general + 1 boots** — listing more general items is
  allowed (early → late) and flagged as info ("only the final 5 fit at once").
- Duplicates are warned.

## Auto-Sort
Reorders the current build into the canonical shape:
**Starter → Boots → Champion-Exclusive → Legendary → Epic → Basic → Trinket → Consumable**
(ties broken by cost). Use it as a quick cleanup, then hand-tune.

## Save
Writes all ten builds back to `Assets/GameData/Items/build_recommendations.seed.json`
in the same schema the game and the store screen load. Unsaved changes prompt on
close, and the title shows `Save*` while dirty.

## Data flow
```
ItemDefinition assets ─┐
                       ├─ Loadout Editor ── writes ──► build_recommendations.seed.json
seed (existing builds)─┘                                      │
                                                              ▼
                         StoreScreenBuilder injects builds → StoreScreenView
                                                              │
                          StoreModel.RegisterBuild(...) → NextRecommended / quick-buy row
```
The shared, engine-agnostic `BuildValidator` powers both the editor's checks and
the `BuildValidatorTests` unit tests, so what the designer sees matches what ships.
