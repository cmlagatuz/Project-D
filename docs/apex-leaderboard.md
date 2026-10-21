# Apex Leaderboard — Adamantite & Crystal

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

A ranking board for the **top two tiers only** (Adamantite, Crystal), filterable by
champion, lane, and scope (Server or Friends). Server-authoritative: the ranking +
filtering logic is engine-agnostic and unit-tested; the client mirrors the server's
ladder snapshot.

## What it shows
Each row: rank #, player (self/friend accented), tier + division badge, LP, most-
played champion, main lane, region, and win-rate (W-L). A pinned **"your rank" card**
shows the viewer's position even when it's off the current page.

## Scopes
- **Server** — the whole apex population (optionally region-filtered).
- **Friends** — only the viewer's friends (+ the viewer), for a personal board.

## Filters
- **Tier:** Both / Adamantite / Crystal.
- **Champion:** any apex champion (dropdown built from who's actually on the board).
- **Lane:** Mid / Baron / Dragon / Support / Jungle / Any.
Filters combine (e.g. Crystal + Kain + Jungle). Changing a filter re-queries locally,
so the full apex pool is fetched once and paged/filtered client-side.

## Ranking order (deterministic)
Higher **ladder step** first (Crystal A above Adamantite E), then more **LP**, then
higher hidden **MMR** (server tie-break), then **recency**, then **win-rate**, then a
stable id tie-break. So two players tied on rank+LP are ordered by the server's MMR.

## Only apex tiers appear
`LeaderboardEngine.IsApexEntry` filters to Adamantite/Crystal; a player below
Adamantite (or unranked) is never on this board. `EntryFromRank` returns null for
non-apex players, so the server snapshot naturally contains only apex entries.

## Online flow (server-authoritative)
```
Server maintains an apex ladder snapshot (recomputed periodically):
  for each apex player: LeaderboardService.EntryFromRank(id, name, region, rankState,
                                                          topChampion, mainLane, isFriend, lastActive)
Client:
  service.FetchPool = scope => backend.GetApexPool(scope);   // server or friends
  var page = service.Query(query);                            // filter + rank + paginate
  screen.Refresh();                                           // renders rows + your-rank card
```
Filtering/ranking runs the SAME `LeaderboardEngine` on client and server, so the
displayed order always matches the authoritative board.

## Champion + lane data
`topChampionId` and `mainLane` come from the player's season stats (most-played this
season). They power the filters and the per-row display, so you can browse e.g. the
best Kain junglers in Crystal.

## Demo
**Tools > Project D > Build Leaderboard Demo** builds a runnable board with a
deterministic mock apex pool (60 players): toggle Server/Friends, filter by tier/
champion/lane, page through, and see your own rank pinned at the bottom.
