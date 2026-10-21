# Project D — Game Modes & the World Rift (Refocused P14: Mobile-First, Competitive-Fair)

> Codename **Zantetsuken** · CONFIDENTIAL · All original design.
> RIGHTS: champion/summon/world names here are PLACEHOLDERS (FF-flavored). Game
> mechanics are not copyrightable and are original; the "contestable summon that turns
> the tide" is an original implementation. All content (names/art/lore) must be original
> or licensed before ship. Inspired-by, not copied-from.

## The decision (per direction)
- **Mobile is the primary platform.** Design targets touch parity + competitive fairness.
- **Ranked & Normal always use ONE canonical map** — the *Crystalline Rift* (our standard
  5v5: 3 lanes, jungle, river pits, dynamic summon/divine pits). No map variety in the
  competitive queues → no confusion, no unfamiliarity, pure skill. (Wild Rift does the
  same: Ranked/Normal on Summoner's Rift only; special modes are separate.)
- **The "world" work is salvaged into a NEW rotating mode — the World Rift** — a
  Dissidia-NT-inspired 5v5 where the battlefield is a **random champion's realm**, each
  with a map-wide **World Modifier** and a contestable **Manifestation** (world boss).
  This mode is **excluded from Ranked** to protect competitive integrity.

## Mode matrix
| Mode              | Map                | Ranked? | MMR/LP      | Notes |
|-------------------|--------------------|---------|-------------|-------|
| Ranked Solo/Duo   | Crystalline Rift   | Yes     | Ranked LP   | Canonical only |
| Ranked Flex       | Crystalline Rift   | Yes     | Ranked LP   | Canonical only |
| Normal Draft      | Crystalline Rift   | No      | Hidden MMR  | Canonical only |
| Normal Blind      | Crystalline Rift   | No      | Hidden MMR  | Canonical only |
| Co-op vs AI       | Crystalline Rift   | No      | —           | Canonical only |
| Practice          | Crystalline Rift   | No      | —           | Canonical only |
| **World Rift**    | **Rotating realm** | **No**  | Casual only | Random world + boss |

`GameModeRegistry` **enforces** the map policy: any attempt to run Ranked/Normal on a
non-canonical map is rejected. World Rift can *only* run rotating worlds and can *never*
award Ranked LP. This is the fairness guardrail, in code.

## The World Rift — how it plays (the "impress me")
5v5, standard lanes/objectives, BUT:

1. **World selection** — at match start a realm is picked from the 10 launch champions'
   home worlds (`WorldSelector`, deterministic, no-repeat weighting so you don't get the
   same world twice in a row).
2. **World Modifier** — a map-wide rule that reshapes the game (e.g., Low Gravity =
   longer dashes; Umbral = shorter vision; Wild Rose = bonus gold on kills). Each world
   has exactly one, so the mode is varied but readable.
3. **The Manifestation (world boss)** — the centerpiece, Dissidia-NT-style: a powerful
   Divine Manifestation spawns mid-match in the central pit. **Both teams contest it**;
   the team that lands the finishing blow **claims it**. On claim the Manifestation
   **empowers that team** — a timed team-wide buff PLUS a one-shot **Summon Strike**
   (an AoE nuke on nearby enemies), like calling in the Eidolon in Dissidia NT. It
   respawns later. Claiming a Manifestation is the swing moment of a World Rift match.
4. **World events** — scripted mid-match beats themed to the realm (meteor fall, tidal
   surge, eruption...) via `WorldEventDirector`, adding drama without touching balance
   of the *competitive* map.
5. **Rewards** — cosmetic/fun currency + World Rift casual rating only. **No Ranked LP.**

## The 10 worlds (placeholders)
Each realm ties to a launch champion, carries one modifier, and hosts a Manifestation
(reusing our existing summon roster: Ifrit/Shiva/Ramuh/Garuda/Titan/Leviathan/Odin).

| # | Champion (placeholder) | Realm (placeholder)     | Modifier   | Manifestation |
|---|------------------------|-------------------------|------------|---------------|
| 1 | wol                    | Dawnspire Sanctum       | Radiant    | Alexander     |
| 2 | garland                | The Chaos Rift          | Unstable   | Ifrit         |
| 3 | firion                 | Rebellion Bastion       | WildRose   | Ramuh         |
| 4 | emperor                | Palamecia Throne        | Dominion   | Odin          |
| 5 | onion_knight           | Crystal Nexus           | JobShift   | Shiva         |
| 6 | cloud_of_darkness      | The Void Between        | Umbral     | Leviathan     |
| 7 | cecil                  | Baron Skyfortress       | Twilight   | Bahamut       |
| 8 | kain                   | Highwind Peaks          | LowGravity | Garuda        |
| 9 | bartz                  | Windwalker Meadows      | Gale       | Titan         |
| 10| exdeath                | The Great Tree          | Rootbound  | Odin          |

## Cross-links to existing systems
- Reuses the **map objective/BGM** system (summon pit, per-objective proximity BGM).
- Reuses **matchmaking** (a separate World Rift casual queue) + **rewards** (cosmetic).
- The **Manifestation** extends the existing summon/divine objective director.
- Fairness enforcement complements **P3 ranked** + **P7 matchmaker**.

## Backlog (this pack)
1. GameModeRegistry — modes, canonical-map policy, ranked-eligibility enforcement.
2. WorldCatalog — the 10 worlds + modifiers + manifestations.
3. WorldSelector — random no-repeat world pick.
4. WorldModifierEngine — resolves a modifier into concrete gameplay parameter deltas.
5. ManifestationBoss — spawn/HP-scaling/contest/claim + victor buff + summon strike.
6. WorldEventDirector — scheduled world events themed per realm.
7. WorldRiftMatch — orchestrates a full World Rift match from the cores.
8. WorldRiftRewards — cosmetic rewards + casual rating (never Ranked LP).
