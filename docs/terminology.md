# Project D — Finalized Terminology (Store/KP vs Shop/Gil)

> Codename **Zantetsuken** · CONFIDENTIAL
> RIGHTS: "Kupo", "Gil", and "Moogle" are Final Fantasy terms (Square Enix).
> Used as placeholders — the system is data-driven, so display names swap without code changes.

## The two economies

| | **Out of game** | **In game** |
|---|---|---|
| **Surface name** | **Store** | **Shop** |
| **Currency** | **KP** ("Kupo") | **Gil** |
| **Merchant** | menu storefront | **Moogle** stalls (like LoL's shopkeepers) |
| **What it buys** | champions, profile **borders**, profile **icons** | in‑match items |
| **How it's earned** | account/**profile level‑up** rewards | minions, jungle camps, kills, objectives |
| **Persists?** | yes (account‑wide, cosmetic/profile) | no (resets each match) |
| **Combat power?** | never (cosmetic + unlocks only) | in‑match stats only, resets each game |

## Hard rules
1. **Never mix currencies.** KP is spent only in the **Store**; Gil only in the **Shop**.
   `EconomyTerms.IsValidPurchase(surface, payWith)` enforces this.
2. **KP is cosmetic/unlock only** — champions, borders, profile icons "to showcase your
   profile to your friends." No KP item affects match strength.
3. **KP source = profile progression.** Leveling your account/profile grants KP; there is
   no in‑match KP.
4. **Gil source = in‑match economy** (the existing item Shop). Unchanged mechanically —
   only the naming is finalized (Shop + Gil + Moogle merchants).

## Naming migration (from earlier packs)
- The earlier "in‑game store" pack should read as the **Shop** with **Gil** and **Moogle**
  stalls. `EconomyTerms` centralizes the strings; the item Shop UI shows "Gil" and Moogle
  merchant flavor. No item stats change.
- A NEW out‑of‑game **Store** (KP) will sell champions / borders / profile icons and read
  KP from profile level‑up rewards.

## Where the strings live
- Code constants + helpers: `EconomyTerms` (surface names, currency names, formatting,
  the never‑mix guardrail, localization keys).
- Localization: `economy_terms.seed.json` keys (`surface.store`, `surface.shop`,
  `currency.kp`, `currency.gil`, `merchant.moogle`, …) so all terms translate and swap.

## Quick usage
```csharp
EconomyTerms.SurfaceName(EconomyScope.Store);          // "Store"
EconomyTerms.Format(CurrencyType.KP, 1250);            // "1,250 KP"
EconomyTerms.Format(CurrencyType.Gil, 3400);           // "3,400 Gil"
EconomyTerms.IsValidPurchase(EconomyScope.Store, CurrencyType.KP);  // true
EconomyTerms.IsValidPurchase(EconomyScope.Shop, CurrencyType.KP);   // false (guardrail)
```
