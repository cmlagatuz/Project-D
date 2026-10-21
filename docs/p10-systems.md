# P10 — Systems Reference (Data Science, Personalization & Retention)
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design. Engine-agnostic, explainable, deterministic, tested (P10Tests, 15).
1.  Churn/ChurnModel — weighted risk 0..1 with top factors surfaced + band.
2.  Recommend/ChampionRecommender — role-gap + win-rate + novelty scoring, with a "why".
3.  DynamicDifficulty/FlowChannel — adapts bot level toward a target win rate (flow channel).
4.  Fairness/MatchFairness — Elo win prob, pre-match fairness, MMR spread, blowout, grade.
5.  Sentiment/BehaviorTrend — EMA of commends/reports/chat -> health band.
6.  LTV/EngagementValue — 0..100 engagement score + tier + healthy-play cap.
7.  ABAnalysis/ABTest — two-proportion z-test, verdict, recommended sample size.
8.  Cohorts/CohortAnalyzer — D1/D7/D30 retention + rolling active-user math.
9.  Offers/OfferEngine — best-fit offer honoring owned/level/spend-cap/comeback (respectful).
10. ContentRec/ContentRecommender — next-best content by tag affinity + safety nudges.

## Design principles
Explainable (factors exposed), deterministic (reproducible), bounded/safe (clamped, wellbeing caps,
no dark patterns), privacy-respecting (behavior aggregates, not PII).

## Tests
P10Tests (15) covers every system.
