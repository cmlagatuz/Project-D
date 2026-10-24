# Summoner Spells

![Summoner spells](images/summoner-spells.png)

Every player brings **two** summoner spells into a match. Spell names are Final Fantasy-flavoured **placeholders**;
icons are **original placeholder art** (editable SVG sources in `docs/images/summoner/`).

Level scaling runs from level 1 to level 15 and is linear between the two values shown.

## The roster

### Standard set (Normal and Ranked, and the other modes where the map rule allows)

| Spell | Plays like | Cooldown | What it does |
|---|---|---|---|
| **Haste** | Ghost | 90 s | A burst of speed that settles into **+25% movement speed** for **8 s**. Each takedown adds **6 s** back (never beyond the original 8 s) and refreshes the burst. |
| **Cure** | Heal | 100 s | Heals you and your most wounded nearby ally for **110 to 400** (by level) and gives both **+30% movement speed for 2 s**. A target healed recently receives **half**. |
| **Protect** | Barrier | 100 s | Shield for **120 to 560** (by level) for **2.5 s**. |
| **Slow** | Exhaust | 100 s | Enemy champion: **35% slow that fades to nothing** over **2.5 s**, and **-40% damage dealt** for 2.5 s. |
| **Esuna** | Cleanse | 110 s | Removes disables (including spell debuffs), then **0.25 s** of disable immunity. |
| **Warp** | Flash | 150 s | Blink a short distance (4.5 units) toward the aimed direction. |
| **Bio** | Ignite | 100 s | **72 to 380** true damage over **5 s**, **-60% healing received**, and reveals the target for 5 s. |
| **Ultima** | Smite | 10 s between casts | **600** true damage to minions and monsters (see below). Up to **2 charges**, one regained every **45 s**. |
| **Teleport** | Teleport | 150 s | **3.5 s** channel, then travel to an allied champion or ward, or to an allied structure **during the first 6 minutes only**. |

### Additional set (Co-Op vs AI, Practice, World Rift only)

| Spell | Plays like | Cooldown | What it does |
|---|---|---|---|
| **Pray** | Clarity | 110 s | Restore 50% resource to you and 25% to nearby allies (placeholder numbers) |
| **Warpstrike** | Snowball | 10 s | Throw a warp mark (range 14). Press again within 3 s to dash to what it hit (placeholder numbers) |

Cooldowns are exactly the design brief. The numbers above come from the Wild Rift reference you supplied, except
where the table says placeholder.

## Ultima (Smite-style) in detail

| Rule | Value |
|---|---|
| Damage by tier | Tier 0: **600**, tier 1: **1000**, tier 2: **1400** true damage |
| Against monsters | **+10%** damage |
| Camp burn after attacking a monster | 22 true damage per second for 2 s in an area at level 1, plus 10% bonus AD, 12% bonus AP, 25% bonus armor, 25% bonus magic resist, 4% bonus health |
| Healing while attacking monsters | 5 (level 1) to 35 (level 15) per second |
| Hunter's Resource | Stacks are earned over time. Clearing a camp spends one for **40 bonus gold** |
| Upgrades | After **8** stacks spent: tier 1. After **20**: tier 2 |
| Fully upgraded (tier 2) | Movement speed in jungle or river: **+10%** out of combat, **+5%** in combat |
| On a champion (tier 1+) | **40** true damage and steals **25%** of their movement speed for 2 s |
| Charges | Max 2, one regained every 45 s; 10 s between casts. A new match starts with both |

## Rules

- Two slots, **no duplicates**.
- **Normal and Ranked offer only the nine standard spells.** Pray and Warpstrike are refused by the loadout
  and by the tracker in those modes (`NotAvailableInMode` / `ModeLocked`).
- **Ultima and Teleport are main-map spells.** They work on the canonical Crystalline Rift map (Ranked, Normal,
  Co-Op, Practice) and are refused in **World Rift**, which uses rotating realms. This mirrors the reference,
  where Smite and Teleport exist on the main map but not on the single-lane map.
- Co-Op and Practice offer all eleven; World Rift offers nine (no Ultima, no Teleport).
- A match snapshots the loadout at start; later edits do not change a running match.
- Summoner-spell haste uses the ability-haste formula: `time * 100 / (100 + haste)`, applied to cooldowns and
  charge refills.
- Default loadout: **Cure + Warp**.
- A diamond pip on an icon marks an additional spell (a shape cue, not colour alone).

## Code

All under `Assets/Scripts/SummonerSpells/` (namespace `ProjectD.SummonerSpells`).

| File | Role |
|---|---|
| `SummonerSpellCatalog.cs` | Ids, definitions, effect lines (scaling, decay), per-mode and per-map availability |
| `SummonerLoadout.cs` | The two slots and legality rules |
| `SummonerSpellTracker.cs` | Deterministic cooldowns and charges (time is passed in, never read from a clock) |
| `SummonerSpellRules.cs` | `HasteBuff` (takedown extension), `CureHealTracker` (halved repeat heals), most-wounded pick, Teleport target legality |
| `SmiteProgress.cs` | Ultima tiers, Hunter's Resource stacks, damage and speed maths |
| `SummonerSpellIcons.cs` | `Resources.Load<Sprite>` icon lookup with caching |
| `Editor/SummonerIconImporter.cs` | Auto-imports `Assets/Resources/UI/SummonerSpells/*.png` as Sprites |

```csharp
var loadout = SummonerLoadout.Default();
loadout.TrySet(1, SummonerSpellId.Haste, GameMode.RankedSolo);          // LoadoutResult.Ok

var spells = new SummonerSpellTracker(loadout, GameMode.RankedSolo);
if (spells.TryCast(0, matchSeconds, out var def) == SummonerCastResult.Success)
    ApplyEffects(def);   // combat reads def.Effects, def.Targeting, def.RangeUnits

float fill = spells.Remaining01(0, matchSeconds);                      // HUD radial fill
int casts = spells.Charges(0, matchSeconds);                           // charge pips for Ultima
Sprite icon = SummonerSpellIcons.Load(SummonerSpellId.Cure);
```

The catalog says *what a spell does*; applying the effects belongs to the combat system, which does not yet exist as a
playable loop. Until then, casts only start cooldowns.

## Placeholders to confirm

These were not in the reference, so they are guesses:

- Haste's opening burst (peak 55%, easing to 25% over 2 s).
- Cure's "recently healed" window (35 s).
- How often Hunter's Resource stacks are earned (30 s) and how many can be stored (4).
- Camp-burn growth per level (+4 per level on the 22 base).
- Whether a new match starts with both Ultima charges (assumed yes).
- Champion-targeting unlocks at tier 1 (assumed from the order of the reference text).
- Pray and Warpstrike effect numbers.
- Teleport's "not near enemy structures" restriction is not implemented (no such structure type yet).

## Not done yet

- HUD buttons: `HudElementId.SummonerSpell1` / `SummonerSpell2` have layout slots, but the Greybox HUD does not
  build the buttons. They should show `SummonerSpellIcons.Load(...)` with a radial fill from `Remaining01`
  and charge pips from `Charges`.
- Spell-select UI in Champion Select (calls `TrySet`).
- Effect application, targeting, VFX/SFX, and server-side cast validation.

## IP note

All Final Fantasy / Square Enix spell names are placeholders. Icons are original vector art with no
reproduced game assets. Licensed art needs a documented chain of title, or replacement with original IP,
before any public build.
