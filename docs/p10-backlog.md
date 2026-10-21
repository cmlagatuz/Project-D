# Project D — P10 Backlog (Data Science, Personalization & Retention)

> Codename **Zantetsuken** · CONFIDENTIAL · All original design.
> P0–P9 delivered the game, meta, backend, and content. **P10 is the data layer** that
> makes the live service smart: understanding players, personalizing their experience,
> and keeping them healthy and engaged. These are engine-agnostic, deterministic,
> unit-tested cores — no ML frameworks, just transparent, explainable scoring models
> a backend can run and reason about.

## Why P10
A modern live game lives or dies on **retention and fairness**. P10 turns the raw
telemetry (from P7) and progression (from P0/P8/P9) into actionable signals: who is
about to churn, what they should play/buy next, whether matches feel fair, and how a
change moved the needle — all with **explainable, auditable math** (no black boxes).

## Design principles
- **Explainable over opaque:** every score exposes its contributing factors.
- **Deterministic:** same inputs → same outputs (reproducible for tests + audits).
- **Bounded + safe:** scores clamp to sane ranges; personalization never manipulates
  vulnerable players (spend caps, cool-downs, no dark patterns).
- **Privacy-respecting:** operate on in-game behavior aggregates, not PII.

## 10 systems
1.  **Churn prediction** — a weighted risk score from recency/frequency/loss-streak/
    social signals, with the top contributing factors surfaced.
2.  **Champion recommender** — suggest champions to try from role gaps, win-rate, and
    mastery, with a "why" reason.
3.  **Dynamic difficulty (PvE)** — adjust bot difficulty to keep the player in the
    "flow channel" (not too easy/hard) from recent performance.
4.  **Match fairness scoring** — pre/post-match balance metrics (MMR spread, predicted
    win prob, blowout detection) to audit matchmaking quality.
5.  **Sentiment/behavior model** — fold honor + reports + chat flags into a behavior
    trend with a health band.
6.  **LTV / engagement value** — a non-monetary engagement score + spend-tier bucket
    that respects healthy-play caps.
7.  **A/B analysis** — two-proportion significance test (z-score) so experiments have
    a real "is this better?" answer, not vibes.
8.  **Cohort retention** — D1/D7/D30 retention + rolling-window active-user math from
    login events.
9.  **Personalized offers** — pick the best-fit, respectful offer (owned-aware, spend-
    capped, cool-down-gated) with an explanation.
10. **Content recommender** — surface the next best content (missions/events/modes) by
    an interest score from recent activity.

## Cross-links
- Consumes P7 telemetry + P3 honor + P0/P8/P9 progression.
- Churn/offers feed the P1 main-menu (targeted callouts) and P9 emporium.
- Fairness feeds the P7 matchmaker/ops dashboard; A/B feeds the P4 experiments.
- Dynamic difficulty feeds the P6 bot difficulty tiers.
