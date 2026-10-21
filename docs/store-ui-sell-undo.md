# Store UI — Sell / Undo Buttons & Recommended Quick-Buy Row

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

Wires the sell/undo transaction logic and per-champion build recommendations into
the live shop screen. Build it with **Tools ▸ Project D ▸ Build Store Screen**.

## What's on screen now
- **BUY** (detail panel) — purchases the selected catalog item (unchanged).
- **SELL** — sells the **selected inventory slot**. Click an owned inventory slot to
  select it (it highlights); the button shows the refund, e.g. `SELL  +1980g`
  (60% of total; consumables 100%). Disabled when nothing is selected or the shop
  isn't usable.
- **UNDO** — reverses the **most recent purchase for a full refund** while inside the
  grace window. The button shows a live countdown (`UNDO  7s`) and a horizontal fill
  that drains over `undoWindowSeconds` (default 10s). Auto-disables when the window
  expires or the shop is unavailable.
- **Recommended quick-buy row** — a horizontal strip above the inventory showing the
  locked champion's **next up-to-5 unowned build items** in order. Each mini-card has
  a one-tap **BUY**; the very next recommended item is accented, and the title reads
  `Recommended — next: <item>`.

## Interaction flow
```
Click inventory slot  → selects it → SELL shows its refund
BUY / quick-buy BUY   → StoreTransactionManager.Buy → marks undo-eligible (t0)
UNDO (within 10s, shop open) → full refund, slot cleared, timer resets
SELL (shop open)      → 60% refund, slot cleared, pending undo invalidated
Leave shop            → SetShopAvailable(false) disables SELL + UNDO
```

## Wiring (already done by the scene builder)
`StoreScreenView` gains serialized fields: `sellButton`, `sellButtonLabel`,
`undoButton`, `undoButtonLabel`, `undoTimerFill`, `quickBuyRow`, `quickBuyCardPrefab`,
`quickBuyTitle`, plus `championBuilds` (loaded from `build_recommendations.seed.json`)
and `shopAvailable` / `undoWindowSeconds`. It drives everything through the
server-authoritative `StoreTransactionManager`:
- `Update()` calls `Tick()` (expire undo) and refreshes the countdown fill.
- `SetShopAvailable(bool)` gates SELL and UNDO (call it from combat/fountain state).
- `SetLockedChampion(id, role)` re-filters the grid and refreshes the quick-buy row.

## Server authority
The UI predicts locally for instant feedback; in a networked build the client sends
a buy/sell/undo intent and the server runs the **same** `StoreTransactionManager`
calls against authoritative gold + inventory, then confirms or rolls back (GCD §14.3).
