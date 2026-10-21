# Combined In-Match HUD
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design.

`MatchHudController` ties the P5 gameplay cores into one match overlay:
- MATCH CLOCK (top-center) drives buff timers, surrender availability, minute-scaling.
- OBJECTIVE / BUFF BAR (top-left) — active team blessings with remaining timers + stacks.
- VISION / WARD tracker (bottom-right) — per-type counts + a minimap plot of ward positions.
- EMOTE WHEEL (bottom-left) — radial, honoring the core per-emote cooldown + burst spam guard.
- SURRENDER panel (center overlay) — timed vote with a countdown + yes/needed tally + pass/fail.
- DAMAGE-NUMBER layer (center) — floating numbers styled by type (physical/magic/true) + crit, from DamageModel.

Server-authoritative in a real build; the demo buttons (grant buff, place/sweep ward,
spawn damage, call surrender) simulate the events the server would push.

Scene: Tools > Project D > Build Match HUD Scene.

## Tests
MatchHudTests (5) verifies the cores the HUD drives: buff remaining ticks, ward expiry,
surrender countdown/pass, emote cooldown, damage numbers matching the model.
