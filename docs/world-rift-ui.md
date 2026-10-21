# World Rift — Mode Select + Manifestation HUD
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design. Names are placeholders.

## Mode Select (ModeSelectView)
- Ranked / Normal / Co-op cards ALWAYS map to the canonical Crystalline Rift (no map choice) — the
  map policy is read from GameModeRegistry and enforced; only ranked-eligible modes show "LP".
- World Rift card -> ROLL REALM -> a realm-reveal splash (name + world modifier description +
  Manifestation) -> ENTER THE RIFT (or REROLL). Queue emits (mode, mapId); World Rift never awards LP.
- Scene: Tools > Project D > Build Mode Select.

## Manifestation HUD (ManifestationHudView)
- Spawn timer -> boss spawns -> boss HP bar + a blue/red CONTEST BAR (damage stake) with a leader tag ->
  finishing blow -> CLAIM banner + summon-strike telegraph ring + screen flash -> buff indicator (team +
  remaining) -> respawn. Binds a ManifestationBoss and reacts to OnSpawned/OnClaimed.
- Scene: Tools > Project D > Build Manifestation HUD Demo (loops the full fight).

## Tests
WorldRiftUITests (3): queue map policy + no-LP for World Rift, realm reveal fields, HUD contest/HP/claim/buff.
