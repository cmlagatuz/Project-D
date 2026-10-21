# Project D — Importing 3D Models

> Codename **Zantetsuken** · CONFIDENTIAL · GCD §13.2

Each unit has a dedicated folder under `Assets/Models/`, so artists import assets
without touching code, then bind them with one menu click.

## Folders
- `Models/Champions/<championId>/` — one per champion (`wol`, `garland`, `firion`,
  `emperor`, `onion_knight`, `cloud_of_darkness`, `cecil`, `kain`, `bartz`, `exdeath`),
  each with `Mesh · Materials · Textures · Animations · Prefabs · VFX` and a README.
- `Models/Summons/<id>/` — Ifrit, Shiva, Ramuh, Garuda, Titan, Leviathan, Odin,
  Materia, Cosmos, Spiritus, Chaos.
- `Models/Monsters/{Small,Medium,Large}/<id>/` and `Models/Minions/<id>/`.

## Steps
1. **Verify rights / chain of title** first (see `LICENSE-NOTICE.md`). Never commit
   unverified art; keep it out of source control until cleared.
2. Drop the model as `<id>.fbx` in `Mesh/`; author materials/textures/animations.
3. Assemble the rigged prefab as `Prefabs/Prefab_<id>.prefab`.
4. Run **Tools ▸ Project D ▸ Link All Models** (or the champion/neutral variants).
   The linker matches by folder id and sets `modelPrefab` on the data asset.
   Missing prefabs are skipped and reported — import art incrementally.

## Budgets (GCD §13.3, starting targets)
- Hero mesh 25k–60k tris · major summon 40k–80k · jungle monster 8k–30k · minion 3k–10k
- 1–2 primary materials for normal units · importance-based texture sizes · LOD0–LOD3
- Critical danger telegraphs must remain visible at every quality level.
