# Project D — P13 Backlog (World Persistence & Living-World Systems)

> Codename **Zantetsuken** · CONFIDENTIAL · All original design.
> P0–P12 delivered the game, meta, backend, content, data, community, and ops layers.
> **P13 is the living-world tier**: the persistent, evolving world *around* the matches
> that gives long-term players a home, a story that changes, and reasons to return that
> aren't just the next game. This is what turns "a MOBA" into "a world."
>
> Engine-agnostic, deterministic, unit-tested cores.

## Why P13
Matches are the heartbeat, but retention over years comes from a **world that persists
and evolves**: a shared map that shifts with player action, a narrative that branches,
a personal home to customize, companions, deep crafting mastery, faction allegiance,
seasonal atmosphere, lore to discover, mentorship bonds, and legacy rewards for
veterans. P13 is the persistent metagame that lives between matches.

## Design principles
- **Persistent + evolving:** world state carries across sessions and shifts over time.
- **Player agency matters:** collective + individual actions visibly change the world.
- **Cosmetic/expression, not power:** housing/pets/faction perks never gate competitive strength.
- **Deterministic:** same inputs → same world state (reproducible + testable).

## 10 systems
1.  **Dynamic world map** — regions with control state that shifts from faction activity.
2.  **Dynamic narrative** — a branching story graph whose path depends on world/player choices.
3.  **Player housing** — a customizable personal space: rooms, placeable decor, a rating.
4.  **Pet/companions** — collectible companions that level, gain traits, and can be equipped.
5.  **Crafting mastery** — a deep crafting-skill tree with recipe unlocks + quality tiers.
6.  **Faction war** — persistent 3-faction meta-conflict with weekly territory scoring.
7.  **Seasonal atmosphere** — a weather/season cycle affecting the world map presentation.
8.  **Lore codex 2.0** — a discovery-driven encyclopedia with entry links + completion sets.
9.  **Mentor program** — veteran↔newcomer pairing with shared-goal progress + rewards.
10. **Legacy rewards** — veteran-tenure milestones granting prestige cosmetics + recognition.

## Cross-links
- Faction war feeds the dynamic world map's control state; both feed seasonal atmosphere.
- Dynamic narrative reads world/faction state to pick branches.
- Housing/pets/lore extend P9 expression + P11 collections; legacy extends P11 prestige.
- Mentor program extends the P11 new-player funnel + endorsements.
