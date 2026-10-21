# Profile Match History (gap closer)
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design.
Match history was NOT previously viewable in the profile (profile held only aggregate
season stats; post-game analytics covered just the current match). This adds it.

- MatchHistory (core): per-player log; Record() writes a compact summary each match,
  newest-first, capped. Derives profile stats: W-L, win-rate, win streak, most-played
  champion, per-champion record. ChampionRecord() powers a champions tab.
- ProfileHistoryView (UI): a record header + recent-match rows (win/loss accent, champion,
  KDA, CS, mode, duration, MVP crown). Tapping a row raises OnOpenMatch(matchId) so the
  app opens that match's post-game analytics (MatchAnalyticsView).
- Wiring: on match end, the PostGame flow records a MatchHistoryEntry (from the same
  result payload it already builds); the profile reads MatchHistory.

Scene: Tools > Project D > Build Profile Match History.

## Tests
MatchHistoryTests (3): newest-first + cap, profile stats (W-L/WR/streak/most-played), get-by-id.
