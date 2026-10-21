# KP Store + Account Leveling (out-of-game meta)

> Project D · Codename **Zantetsuken** · CONFIDENTIAL
> RIGHTS: "KP/Kupo", "Gil", "Moogle" are placeholders (Square Enix terms). Names are
> data-driven and swappable; all logic/design is original Project D.

The out-of-game **Store** where players spend **KP** on champions, profile borders,
and profile icons. KP is earned only from **account/profile level-ups** — never from
matches. (The in-match **Shop** uses **Gil**; the two never mix.)

## Account leveling (the KP source)
- `AccountLevelCurve` — XP-to-next ramps per level; each level-up grants **KP**
  (base 60, +250 milestone bonus every 10 levels).
- `AccountProgression` + `AccountProgressionService.AddXp` — apply post-match/event XP;
  advances the level (multi-level on big grants) and accumulates the KP earned.
- `LevelUpPopup` celebrates the level-up and shows the KP reward.

## The Store
- `StoreItemDefinition` — a KP item: Champion / ProfileBorder / ProfileIcon / Bundle,
  rarity, KP price (+ optional sale), champion link, level gate, featured flag.
- `KpStoreModel` — catalog + ownership + purchase validation (KP-only guardrail,
  level gate, no double-buy, bundle expansion).
- `KpStoreService` — ties account balance + inventory + catalog into one **Buy** flow,
  with backend pull/push hooks (server-authoritative online).
- UI: `CurrencyHeader` (level + XP bar + KP), `StoreItemCard`, `KpStoreScreen`
  (category tabs, grid, detail/buy, equip borders/icons).

## Guardrails
- **KP only in the Store** — `EconomyTerms.IsValidPurchase` rejects Gil here
  (`WrongCurrency`). KP items are **cosmetic/unlock only** (no combat power).
- Owned items can't be re-bought; level-locked items show "Unlock at Lv N".
- Borders/icons can be **equipped** to showcase your profile to friends.

## Online flow (server-authoritative)
```csharp
service.PullAccount = () => backend.LoadAccount();      // KP + level on login
service.PushAccount = a => backend.SaveAccount(a);      // after XP grant / purchase
service.PullInventory = () => backend.LoadInventory();
service.PushInventory = inv => backend.SaveInventory(inv);
var r = service.AddXp(postMatchXp);                     // level-ups grant KP
service.Buy("champ_kain");                              // KP-validated purchase
```
The client predicts; the server runs the same `TryPurchase`/`AddXp` against the
authoritative balance and pushes the result.

## Data
`kp_store.seed.json` (20 items: 10 champions, 4 borders, 5 icons, 1 bundle) ->
`StoreItemDefinition` assets via **Tools > Project D > Import KP Store Items**.

## Demo
**Tools > Project D > Build KP Store Demo**: tap **GRANT 800 XP** to level up + earn
KP (watch the level-up popup), then buy champions/borders/icons and equip a border/icon.
