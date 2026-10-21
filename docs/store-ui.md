# In-Game Store — UI Screen & Champion-Exclusive Flow

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

The shop screen, built from the imported `ItemDefinition` assets.

## Build & run
1. **Tools ▸ Project D ▸ Import Store Items** (generates the item assets).
2. **Tools ▸ Project D ▸ Build Store Screen** → `Assets/Scenes/StoreScreen.unity`.
3. Press **Play**. The scene locks in **Bartz** by default, so the gold **Exclusive**
   tab is enabled — open it and buy **Chicken Knife** to see the highlight + purchase.

## Layout
- **Category tab bar** — Starter · Consumables · Trinkets · Boots · Basic · Epic ·
  Legendary · **Exclusive** (gold). The exclusive tab is greyed until a champion with
  an exclusive is locked in.
- **Item grid** — cards show name, cost (red if unaffordable), a role dot, an owned
  check, a **recommend accent** for your role, and a **gold frame + EXCLUSIVE ribbon**
  for the champion-exclusive.
- **Recommended-for-my-role toggle** — filters the grid to your picked role
  (exclusives always stay visible).
- **Detail / build panel** — stats, passive/active, build-from / builds-into path,
  flavor, and a context-aware **BUY** button (shows the gold deficit / "BOOTS SLOT
  FULL" / "NO SLOT" / "UNAVAILABLE").
- **Gold bar** + **6-slot inventory row** — five general slots plus the dedicated
  **boots slot** (slot 6), labeled.

## Role recommendations
`StoreModel.RecommendedFor(role)` sorts the locked champion's role first, then by
cost, and `IsRecommended` drives the accent border + role dot. Anyone can still buy
anything — it is a highlight, not a restriction.

## Champion-exclusive logic (`ChampionExclusiveController`)
Engine-agnostic so the server shares it:
- **Visibility** — the exclusive only appears when its champion is locked in
  (`StoreModel.IsAvailable` gates it; the exclusive tab enables via `HasExclusive`).
- **Highlight state** — `GetHighlight()` returns None / Available / Unaffordable /
  NoSlot / Owned, which the UI renders with the gold treatment.
- **Purchase** — `TryPurchaseExclusive()` enforces the right champion, **one copy per
  player**, gold, and a free **general** slot (never the boots slot).
- Wire on lock-in: `storeScreenView.SetLockedChampion(championId, role)`.

## Server-authoritative purchases
The UI predicts via `StoreModel.TryPurchase` for instant response; in a full build
the client sends a buy intent and the server runs the **same** call against
authoritative gold + inventory, then broadcasts the result (GCD §14.3). Wire
`InventoryModel.AggregateStats()` into your `StatBlock` so items feed combat stats.

## Tests
`Assets/Scripts/Tests/`: `StoreTests` (slot/boots/stacking/purchase) and
`ExclusiveStoreTests` (visibility gating, highlight states, one-copy rule, general-slot
placement).
