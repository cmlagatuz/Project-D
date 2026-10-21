# Project D — In-Between Loading Screen (Tips & Trivia)

> Codename **Zantetsuken** · CONFIDENTIAL

The loading screen shown between Preparation and In-Game, with a picked-champion
focus, a weighted progress bar, and a rotating tips/trivia carousel. Champion
names/lore are Final Fantasy placeholders; all tip text is original (see
`docs/LICENSE-NOTICE.md`).

## Contents
- `Scripts/Loading/LoadingTipData.cs` — `ChampionTipSet` + `GeneralTipSet` ScriptableObjects + `TipCard`.
- `Scripts/Loading/TipRotationController.cs` — deterministic, no-repeat tip playlist (engine-agnostic).
- `Scripts/Loading/LoadingProgress.cs` — weighted multi-source progress aggregator.
- `Scripts/Loading/LoadingScreenView.cs` — the runtime screen (focus + bar + carousel).
- `Scripts/Editor/LoadingTipsImporter.cs` — **Tools ▸ Project D ▸ Import Loading Tips**.
- `Scripts/Editor/LoadingScreenBuilder.cs` — **Tools ▸ Project D ▸ Build Loading Screen**.
- `Scripts/Tests/LoadingTests.cs` — rotation determinism, no-adjacent-dupe, progress math.
- `GameData/Loading/loading_tips.seed.json` — 10 champions (50 tips + 20 trivia) + 12 general tips.
- `docs/loading-screen.md` — full guide · `docs/LICENSE-NOTICE.md`.

## Quick start
Import Loading Tips → Build Loading Screen → Play. Wire `Report(...)` /
`SetAllPlayersReady(...)` to real loaders and hand off on `OnReadyToEnterMatch`.
Fits MatchFlow between Preparation and In-Game.
