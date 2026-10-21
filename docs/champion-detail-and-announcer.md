# Champion Detail Page + Announcer Banners (UI)

> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design.

## Champion Detail / Preview page (batch-4)
`ChampionDetailView` + `ChampionDetailModel`:
- Name + title + role tags; role-tinted splash.
- Four **stat bars** (Damage/Toughness/Utility/Difficulty) derived from base stats —
  utility rewards heal/shield + Support/Vanguard so tanks/supports read correctly.
- **Ability icon row** (Passive, Q, W, E, R).
- **Skins carousel** (prev/next through owned + locked skins, rarity-tinted, lock badge).
- **KP unlock/buy panel** reflecting Owned / Purchasable / LevelLocked / NotEnoughKp;
  OnBuy -> `KpStoreService.Buy(championId)`; `ApplyPurchaseResult` syncs after the server.
- Demo: Tools > Project D > Build Champion Detail Demo.

## Announcer banners (in-match HUD)
`AnnouncerBanner` (+ `Announcer` core):
- Drains the priority `AnnouncementQueue` and shows one banner at a time (biggest
  priority first), styled + scaled per kind (Penta/Ace/Core = pink punch, Quadra/Odin
  = gold, Triple/Divine/Summon = purple, Double/FirstBlood/FirstTurret = blue).
- Slide-in + hold + slide-out, with a punch on high-priority callouts.
- Fires **OnVoCue / OnSfxCue** (wire to the effects pack) and resolves subject names +
  localized headlines via injected functions (`ResolveSubjectName`, `ResolveText`).
- Server-driven: the server raises announcements from authoritative combat/objective
  events (First Blood, multi-kills, First Turret, Summon/Odin/Divine slain, Ace, Core
  Exposed); the client just presents.
- Demo: Tools > Project D > Build Announcer Banner Demo (auto sequence + buttons).

## Integration
- Champion Detail unlock -> KP Store buy flow (KP Store pack).
- Announcer VO/SFX keys -> `EffectCatalog`/`AudioCuePlayer`; headline keys -> localization.
