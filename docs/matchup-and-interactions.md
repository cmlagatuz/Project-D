# Matchup Callout & Champion Voice Interactions

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

Two features layered onto the loading screen.

## 1) Matchup callout (auto-pick tip/trivia for your laner)
`MatchupCallout.Resolve(localLane, enemyPicks, champions)`:
- Finds the enemy in YOUR lane (falls back to the first enemy pick if none match).
- Builds a personalized card ("YOUR LANE (Jungle) - Cecil Harvey" + a "how to play
  against Cecil" tip), which `TipRotationController` **pins first** in the carousel.
- A **matchup banner** at the top of the screen names your opponent.
- Enemy picks carry a lane, e.g. `{ championId:"cecil", lane:"Baron" }`.

## 2) Champion voice interactions (first-encounter lines)
When you first face a specific enemy, your champion plays a line + subtitle.
- Data: `ChampionInteractionSet` per champion — a `genericFirstEncounter` line plus
  `matchups[]` keyed by enemy id (e.g. kain vs cecil).
- `VoiceLinePlayer.PlayEncounter(localId, enemyId)` resolves the best line (specific
  matchup, else generic), plays the assigned `AudioClip`, shows the subtitle, and
  fires it **at most once per session** per (local, enemy) pair.
- The loading screen auto-triggers it for the resolved laner after a short delay;
  you can also call it in-match on the "first time you see this enemy" event.

### Audio
Drop `.mp3/.wav/.ogg` in `Assets/GameData/Loading/Voice/<voiceClipKey>.mp3` and run
**Import Champion Interactions** to auto-bind by filename. No audio yet? The subtitle
still shows so the matchup reads.

> RIGHTS: quote text in the seed is ORIGINAL placeholder copy — **not** the Dissidia
> dialogue (Square Enix IP). Replace text + audio with licensed content. See LICENSE-NOTICE.

## Build it
1. **Import Loading Tips** → tip sets (+ lanes).
2. **Import Champion Interactions** → interaction sets (auto-binds any audio present).
3. **Build Loading Screen** → the scene previews Kain (Jungle) vs Cecil: the callout
   is pinned first and Kain's first-encounter line fires (subtitle shows).

## Wire into the match flow
```csharp
view.Configure("kain", "Jungle", enemyPicksWithLanes, matchSeed);   // from draft result
view.OnReadyToEnterMatch += LoadMatchScene;
// in-match, on first sighting of an enemy:
voicePlayer.PlayEncounter(localChampionId, enemyChampionId);
```

## Notes
- Animated splash/parallax for the champion focus is reserved for Legendary/Ultimate
  skins later; the focus is a static role-tinted image for now.
- All logic is engine-agnostic and unit-tested (`MatchupTests`).
