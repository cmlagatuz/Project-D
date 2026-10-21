# Project D — Rank Decay (Inactivity)

> Codename **Zantetsuken** · CONFIDENTIAL

LP decay for inactive high-tier players. Drop-in to the Ranked pack; no RankState edit.

- Only **Platinum+** decays; banked protection days spend before decay; ranked games refill the bank.
- Per-tick LP loss scales by tier (apex 75); never demotes below the Platinum floor.
- Self-contained calculator (`RankDecay.Compute`) + `RankDecayState` you persist per account.
- Surfaces as "falling" on the apex leaderboard's live movement indicator.

Files: `Scripts/Ranked/RankDecay.cs`, `Scripts/Tests/RankDecayTests.cs`, `docs/rank-decay.md`.
