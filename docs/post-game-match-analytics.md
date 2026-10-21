# Post-Game Match Analytics
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design.

Corrected placement: match analytics belongs in the POST-GAME flow — after a match,
tap "VIEW MATCH ANALYTICS" to see THAT match's breakdown (not a global ops console).

`MatchAnalytics` (core) assembles one finished match:
- team lines (KDA/CS/gold/damage/vision/objectives/MMR) + a gold/xp lead timeline;
- Build() derives: team-avg MMR + spread, pre-match fairness + grade (reuses P10
  MatchFairness), blowout flag, peak blue/red gold leads, final diff, damage share, MVP.
- CoachingFor(playerId) returns that player's tips via the P10 CoachingInsights core.

`MatchAnalyticsView` (UI) renders:
- Header: outcome + duration + fairness gauge/grade + MMR spread.
- Timeline: gold-lead line (blue up / red down) + peak leads + final diff.
- Scoreboards: both teams sorted by performance, MVP crowned; click a player for coaching.
- Coaching: the selected player's praise/tip/warning insights.

Scene: Tools > Project D > Build Match Analytics Demo (opens as if from the results screen).

## Tests
MatchAnalyticsTests (5): fairness/leads derivation, MVP pick, damage-share sums to 1,
per-player coaching, scoreboard sort.
