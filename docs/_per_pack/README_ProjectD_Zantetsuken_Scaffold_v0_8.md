# Project D — Skill Animation Design Add-on

> Codename **Zantetsuken** · CONFIDENTIAL

Designs and wires the animation for **every champion skill** (10 champions ×
Passive/Q/W/E/Ultimate = **50 abilities**), plus locomotion and reactions.
Drop-in on top of your existing Project D `Assets/` (uses the base project's
MiniJSON, WeaponSocket, and networking cast feedback).

## What's inside
- **`Scripts/Art/AbilityAnimation.cs` / `ChampionAnimationSet.cs`** — animation data
  (clip names, startup/active/recovery, cancel window, motion, weapon trail, VFX/SFX cues).
- **`Scripts/Art/AbilityAnimator.cs`** — runtime: plays the skill clip on cast, fires
  cues at authored times, and respects the **server's** accept/reject verdict.
- **`Scripts/Editor/AnimationImporter.cs`** — JSON seed → `AnimSet_<id>.asset` ×10.
- **`Scripts/Editor/AnimatorControllerGenerator.cs`** — builds `AC_<id>.controller`
  with locomotion + ability states + parameters/transitions.
- **`GameData/Animations/animations.seed.json`** — the authored design for all 50 skills.
- **`docs/animation-design.md`** — the full readable spec (generated from the seed).
- **`docs/animation-pipeline.md`** — integration + naming conventions.

## 3-step flow (Tools ▸ Project D)
1. **Import Champion Animations** → `AnimSet_<id>` assets.
2. **Generate Animator Controllers** → `AC_<id>` controllers (states named after clips).
3. Assign the controller + `AbilityAnimator` to each champion prefab. As FBX clips
   arrive (named like the design), they auto-fill the matching states.

Timings mirror `AbilityDefinition` so the visual wind-up matches the gameplay
telegraph; damage always resolves on the authoritative server, never from the clip.


## Extended coverage (this update)
- **Basic-attack chains** per weapon class (timing, hit frames, cancel windows, combo reset).
- **Reactions** — flinch/hit/knockback/knockup→airborne/stun/root/silence/slow, all **server-driven**.
- **Healing effects** — heal-receive, shield gain/break, buff, cleanse (additive overlays).
- **Death + respawn countdown** — death → respawn-wait loop with a floating timer → revive.
- **Summons & monsters** — 28 neutral sets: 6 elemental summons, **Odin** (3 signatures + enrage + Zantetsuken),
  4 divine manifestations, 12 jungle monsters (small/medium/large), 5 goblin minions — each with a
  signature mechanic and, for objectives, a **reward-grant flourish**.

New tools: **Import Combat Extras**, **Import Neutral Animations**, **Generate Neutral Controllers**.
New runtime: `CombatReactionAnimator`, `DeathRespawnController`, `NeutralAnimator`.
New docs: `docs/animation-combat-extras.md`, `docs/animation-neutrals.md`.


## VFX/SFX key catalog (this update)
Every `vfx_*`/`sfx_*` cue in the animation designs now has a defined asset + spawn rule.
- **333 keys catalogued** (213 VFX + 120 SFX) — auto-classified into
  categories (AbilityCast/Impact, WeaponTrail, Projectile, GroundZone, Aura, Heal, Shield,
  Cleanse, Reaction, Death, Respawn, Objective, Summon/Monster/Minion, and SFX families).
- Each `EffectDefinition` carries: **spawn rule** (attach/at-socket/ground-point/projectile/…),
  default **socket**, **lifetime**, **pooling** (prewarm + max-concurrent), **danger-telegraph**
  flag (43 keys, never reduced), **team-color** flag (48 keys), and SFX **priority**.
- Runtime: `EffectCatalog` (key→def lookup), `EffectSpawner` (pooled, team-tinted, budgeted VFX),
  `AudioCuePlayer` (priority voice limiter). Subscribe them to the animators' `OnVfxCue`/`OnSfxCue`.
- Tool: **Import Effect Catalog** → `FX_<key>.asset` ×333 + `EffectCatalog.asset` (re-import preserves
  assigned prefabs/clips). Docs: `docs/effects-catalog.md` (full key list) + `docs/effects-pipeline.md`.
