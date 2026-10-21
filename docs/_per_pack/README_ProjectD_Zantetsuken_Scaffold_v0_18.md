# Project D — KP Store + Account Leveling

> Codename **Zantetsuken** · CONFIDENTIAL

The out-of-game **Store** (KP) + account/profile leveling that sources KP.
- **Account leveling**: XP -> level-ups -> KP rewards (milestones every 10 levels).
- **KP Store**: buy champions, profile borders, profile icons (+ bundles); equip borders/icons.
- **Guardrail**: KP only in the Store (never Gil); cosmetic/unlock only (no combat power).
- Server-authoritative service with backend pull/push; 18 unit tests.

## Contents
- `Scripts/Account/` — EconomyTerms, AccountLevelCurve, AccountProgression.
- `Scripts/Store/` — StoreItemDefinition, KpStoreModel, KpStoreService.
- `Scripts/UI/` — CurrencyHeader, LevelUpPopup, StoreItemCard, KpStoreScreen, demo relay.
- `Scripts/Editor/` — KpStoreImporter, KpStoreDemoBuilder.
- `Scripts/Tests/KpStoreTests.cs` — curve, KP grants, purchase gates, bundles, equip.
- `GameData/Store/kp_store.seed.json` (20 items).
- `docs/kp-store.md`.

## Quick start
Import KP Store Items -> Build KP Store Demo -> Play -> GRANT 800 XP to level up + earn KP -> buy.
