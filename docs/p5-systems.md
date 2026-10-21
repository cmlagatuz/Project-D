# P5 — Systems Reference (Core Gameplay Depth)
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design. Engine-agnostic, server-authoritative.
1.  Draft/DraftEngine — competitive ban/pick order, uniqueness, per-team turns, Complete.
2.  Objectives/ObjectiveBuffs — blessings + team buff timers (stack/expire/permanent), time-scaled bounty.
3.  Vision/VisionSystem — ward caps per type, expiry, sweep, HasVisionAt.
4.  Jungle/JungleEconomy — camp gold/xp scaling by minute, smite (900 true, 15s cd), secures at low HP.
5.  Waves/MinionWaves — wave composition (cannon every 3rd, super after seal), push state, CS gold.
6.  Turrets/TurretSystem — plating gold windows + time expiry, first-turret bonus, champion aggro rule.
7.  Combat/DamageModel — resist reduction resist/(resist+100), flat/% pen, true damage, crit, effective HP.
8.  Comeback/ComebackMechanics — shutdown bounties (capped), catch-up XP (+35%) & gold (+20%) when behind.
9.  Surrender/SurrenderVote — earliest time + cooldown, 4/5 threshold, window/failure, no-LP remake.
10. Emotes/EmoteWheel — per-emote cooldown + burst spam guard + wheel slots.

## Tests
P5Tests (10) covers every system. See docs/p5-backlog.md for scope.
