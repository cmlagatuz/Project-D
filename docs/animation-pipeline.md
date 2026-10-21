# Animation Pipeline & Integration

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

How the skill animations flow from design → data → clips → in-game playback.

## The four pieces
1. **Design/data** — `Assets/GameData/Animations/animations.seed.json` (all 50 skills,
   authored) → imported into `ChampionAnimationSet` assets.
2. **Clips** — the artist authors FBX animation clips named exactly like the
   `clipName` fields (e.g. `wol_q_radiant_cut`). Drop them next to the champion FBX.
3. **Controllers** — `AnimatorControllerGenerator` builds `AC_<id>.controller` with
   locomotion + ability states (named after the clips) + parameters/transitions.
4. **Runtime** — `AbilityAnimator` plays the ability clip on cast and fires the
   authored VFX/SFX cues; it listens to the server's cast verdict to keep animation
   honest with the authoritative outcome.

## Menu flow (Tools ▸ Project D)
1. **Import Champion Animations** → creates `AnimSet_<id>.asset` for all 10.
2. **Generate Animator Controllers** → creates `AC_<id>.controller` for all 10.
3. Assign `AC_<id>` to the champion prefab's **Animator**; add **AbilityAnimator**
   and assign the matching `AnimSet_<id>`.
4. As FBX clips arrive, they auto-fill the same-named states — no rewiring.

## How timing maps to gameplay (server-authoritative)
Each ability's **startup / active / recovery** in the animation data mirrors the
same fields on `AbilityDefinition`, so the *visual* wind-up matches the *gameplay*
telegraph. The recommended cast path:

```
Player taps Q
  → NetworkedCastController predicts + AbilityAnimator.PlayAbility(Q)   // instant feel
  → server validates (rank/cd/resource/lag-comp hit)
  → OnCastFeedback(accepted?) → AbilityAnimator.OnServerCastResult(Q, accepted)
       accepted  : animation plays out; VFX/SFX cues fire at authored times
       rejected  : animation aborts, snaps back to locomotion (no ghost cast)
```

The **active** window is where hits resolve on the server; the VFX cue at the same
time is the visual confirmation. Never resolve damage from the animation — the clip
follows the server, not the other way around.

## Weapon motion (uses the WeaponSocket rig)
- **Separate weapon** (attached to a hand bone): the weapon is a child of the hand,
  so any hand/arm keys in the ability clip move it naturally. For swings/thrusts,
  author the weapon's arc in the same clip; enable `animatesWeapon` and set a
  `weaponTrailKey` for the active window.
- **Combined weapon** (baked into the character FBX): it animates with the body
  automatically — no socket work; `animatesWeapon` still drives the trail cue.
- **Weapon-swap champions** (Firion, Onion Knight, Bartz): hide/show the relevant
  weapon via the socket at clip start (lance for Spearline, bow for Bow Volley…),
  restore on recovery.

## Root motion vs in-place
- **RootMotion** clips (dashes, leaps, charges: `wol_e`, `garland_w`, `ok_q`,
  `kain_w`, `kain_r`, `cecil_w`) drive displacement from the clip — the server still
  validates the destination against max speed/leash, so author travel to match
  `travelDistance`.
- **Steerable** (e.g. `cod_e` Unbound Form, `bartz_w`): partial root motion the
  player can nudge; blend a small input-driven offset on top.
- Everything else is **InPlace**; the mover component owns position.

## Layers
- **Base** — locomotion + ability states.
- **Upper (additive)** — passive auras, guard holds, stance shimmers. Passives live
  here so they never interrupt movement.
- **Stance champions** (Cecil dark/paladin, Onion Knight ninja/sage) swap the active
  ability/locomotion set via a stance flag; author both variants where noted.

## Naming conventions (must match for auto-fill)
- Ability clip: `<championId>_<slot>_<name>` — e.g. `kain_r_high_jump`.
- Charge/loop: `..._loop` (e.g. `kain_r_airborne`); recast: `..._<action>`.
- Locomotion: `<id>_idle`, `<id>_run`, `<id>_recall`, `<id>_death`, `<id>_spawn`.
- Basic chain: `<id>_attack_1..n`.

## Editing the design
Edit `animations.seed.json` (or the generated `AnimSet_*` assets), re-run **Import
Champion Animations**, then regenerate controllers if you added/removed states.
The readable spec `docs/animation-design.md` is generated from the same seed.

---

## Extended coverage (attacks, reactions, healing, death, neutrals)

Beyond the 50 skills, these systems complete the animation set:

### Import order (Tools ▸ Project D)
1. **Import Champion Animations** — creates `AnimSet_<id>` with skills + locomotion.
2. **Import Combat Extras** — fills each `AnimSet_<id>` with the **attack chain**,
   **reaction set** (CC), **healing overlays**, and **death/respawn countdown**.
3. **Import Neutral Animations** — creates `NeutralAnim_<id>` for summons, Odin,
   divine manifestations, monsters, and goblins.
4. **Generate Animator Controllers** (champions) and **Generate Neutral Controllers**.

### Runtime components (add to the prefab)
- **`AbilityAnimator`** — the five skills (already delivered).
- **`CombatReactionAnimator`** — basic-attack combo (`PlayBasicAttack(attackSpeed)`),
  server-driven `PlayReaction(CcType, duration)`, and healing overlays
  (`PlayHealReceive/PlayShieldGain/PlayShieldBreak/PlayCleanse`).
- **`DeathRespawnController`** — `OnServerDeath(seconds)` plays death → holds the
  respawn-wait loop with a **floating countdown number** → `reviveClip` at zero;
  `SyncRemaining(serverRemaining)` keeps the timer authoritative.
- **`NeutralAnimator`** — spawn / basic / `PlaySignature(A|B|Ultimate)` / `Enrage` /
  death / `PlayRewardGrant()` for objective blessings.

### Server-authoritative rule (important)
Reactions, crowd control, death timers, and objective rewards are all **driven by
the server**. The client plays what the server reports — it never decides that a
hit/stun/death happened. Death-timer *values* scale with match time on the server;
`DeathRespawnController` only visualizes them and corrects to `SyncRemaining`.

### Weapon classes → attack pacing
Attack-chain timing is derived per weapon class (Sword, Greatsword, Lance, Bow,
DualBlade, Staff, Gauntlet, Tentacle): heavier weapons hit later and recover slower.
Weapon-swap champions (Firion, Onion Knight, Bartz) hide/show the relevant weapon via
the `WeaponSocket` for the ability/attack clip, then restore.

### Docs
- `docs/animation-combat-extras.md` — attack tables + reactions + healing + death per champion.
- `docs/animation-neutrals.md` — every summon, monster, and minion with signatures + reward flourishes.
