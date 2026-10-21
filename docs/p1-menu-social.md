# Main-Menu Shell + Social/Profile Screens (P1 UI)

> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design.

## Main-menu shell (home hub)
`MainMenuShell` + `MenuNavButton` (+ `MainMenuModel` core):
- Left **nav rail** of destinations (Play, Store, Champions, Profile, Events, Missions,
  Leaderboard, Social, Settings), each with a live **notification badge** (red dot +
  count) fed from aggregated app state (`RefreshBadges`).
- **Player summary header** (equipped border/icon, name, level, rank, KP balance).
- Big **PLAY** button with the current mode label; `OnNavigate` swaps the content host,
  `OnPlayPressed` enters matchmaking.
- Demo: Tools > Project D > Build Main Menu Shell Demo.

## Social / Profile showcase
`SocialProfileView` + `FriendRow` (+ `ProfileModel`, `FriendsModel` cores):
- **Profile showcase card** — equipped border + icon, name/level/title, rank showcase,
  season win-rate, favorite champion, player ID.
- **Friends list** — online-sorted (in-game > in-queue > online > away > offline) with
  presence dots, level/rank, status text, and Invite/Profile actions.
- **Requests** — incoming friend requests with an ACCEPT action (auto-refreshes).
- **Add friend** — input + button; mutual requests auto-accept (via FriendsModel).
- Demo: Tools > Project D > Build Social Profile Demo.

## Integration
- Header KP + level come from the KP Store / account leveling pack.
- Nav badges: Missions (live-ops), Social (friend requests), Store (new items),
  Profile (pending rewards), Events (new events).
- Friends list drives the leaderboard's Friends scope; equipped border/icon come from
  KP Store purchases. Server owns the profile + social graph; client mirrors.
