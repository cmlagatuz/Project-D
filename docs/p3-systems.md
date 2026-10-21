# P3 — Systems Reference

> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design.
Engine-agnostic, server-authoritative cores. UI/scene binding comes later (like P1/P2).

- Honor/HonorSystem — commend cap/voter, behavior score 0..1, honor 0..5, restrictions, RankedAllowed<0.3.
- Replay/ReplaySystem — ReplayRecording (header+events+keyframes), PlaybackController (0.1-8x, seek).
- Spectator/SpectatorSystem — VisibleMatchTime (delay), fog modes, viewer roster.
- Guild/GuildSystem — roles/permissions, join approval, XP->level (grows capacity).
- BattlePass/BattlePass — free+premium tiers, AddXp, Claim gating (tier+premium+no double-claim).
- Party/PartySystem — invites, AllReady, caps per mode, AverageMmr + premade premium.
- Mastery/ChampionMastery — PointsForMatch, AddPoints (L5 gates), GrantToken beyond.
- Practice/PracticeTool — PracticeConfig + ToShareCode/FromShareCode round-trip.
- News/NewsFeed — Active (pinned+newest), UnreadCount, ByCategory.
- Integrity/IntegritySignals — CheckMove/CheckActionRate, severity accumulation, flag-for-review.

## Cross-links
Honor gates Ranked + rewards · BattlePass+Mastery XP from PostGame · Party MMR -> P2 matchmaking ·
Replay/Spectator read the match record · News unread -> P1 main-menu badge.

## Tests
P3Tests (15) covers every system's core behavior.
