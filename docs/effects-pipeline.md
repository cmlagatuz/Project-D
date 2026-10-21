# VFX/SFX Catalog — Pipeline & Wiring

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

Binds every animation cue key to a concrete effect and plays it by rule.

## The chain
```
Animation clip cue (time, key, socket)
  → AbilityAnimator / CombatReactionAnimator / NeutralAnimator / DeathRespawnController
      raise OnVfxCue(key, socket)  and  OnSfxCue(key, socket)
  → EffectSpawner.Play / AudioCuePlayer.Play
  → EffectCatalog.Resolve(key) → EffectDefinition (asset + spawn rule)
  → spawn VFX by SpawnRule (pooled, team-tinted, budgeted) / play SFX (priority voice)
```

## Import order (Tools ▸ Project D)
1. Import Champion Animations → Import Combat Extras → Import Neutral Animations
2. **Import Effect Catalog** → generates `Assets/GameData/Effects/FX_<key>.asset` for
   all 333 keys + a lookup `EffectCatalog.asset`.
3. Artists open each `FX_*` asset and assign the **VFX prefab** or **AudioClip**.
   Everything else (category, spawn rule, socket, lifetime, pooling, telegraph,
   team-color, priority) is pre-filled and editable.

## Runtime wiring (per unit prefab)
Add `EffectSpawner` + `AudioCuePlayer`, assign the shared `EffectCatalog`, then:
```csharp
// champions
abilityAnimator.OnVfxCue      += effectSpawner.Play;
abilityAnimator.OnSfxCue?     // ability animator forwards vfx+sfx as cues
combatReactionAnimator.OnVfxCue += effectSpawner.Play;
deathRespawnController         // fires death/respawn vfx via its set
// neutrals
neutralAnimator.OnVfxCue      += effectSpawner.Play;

// route audio-family keys to the audio player (both share the (key,socket) signature)
abilityAnimator.OnVfxCue      += audioCuePlayer.Play; // AudioCuePlayer ignores non-Sfx keys
```
`EffectSpawner` ignores SFX keys and `AudioCuePlayer` ignores VFX keys (each checks
the definition's `kind`), so you can safely subscribe both to the same event.

Set `effectSpawner.SetOwnerTeam(team)` so team-colored effects tint correctly, and
`effectSpawner.SetReduceEffects(true)` from the accessibility menu (danger
telegraphs are never reduced).

## Spawn rules (what each key does)
- **AttachToSocket** — auras, held weapon trails, shields: parented, follows the bone.
- **SpawnAtSocket** — most impacts: spawned at the socket, detaches.
- **SpawnAtGroundPoint** — zones/telegraphs: projected on the ground at the aim point.
- **ProjectileTravel** — beams/bolts/Gungnir: origin at socket, travel handled by your
  projectile system via `EffectSpawner.ProjectileHandler`.
- **SpawnAtRoot / SpawnAtTarget / WorldZone** — root-anchored, target-anchored, or a
  large persistent world zone.

## Performance & readability (GCD §13.3)
- **Pooling** is on for high-churn keys (impacts, minions, projectiles); `prewarmCount`
  seeds the pool; `maxConcurrent` caps simultaneous instances per key.
- **Danger telegraphs** (⚠, 43 keys) stay visible at every quality level — the
  reduce-effects setting only culls non-essential families (trails, auras, buffs, env).
- **Team-colored** (🎨, 48 keys) are tinted by the owner's team at spawn so friend/foe
  reads correctly without relying on shape alone.

## Audio limiter
`AudioCuePlayer` runs a small voice pool; when full it steals the lowest-priority
active voice only if the new cue outranks it — so ults/deaths/voice (priority 7–8)
win over whooshes/footsteps (priority 4–5).

## Editing / adding keys
Add the key to an animation seed, re-run its importer, then re-run **Import Effect
Catalog** — new keys are added (auto-classified); your assigned prefabs/clips on
existing keys are preserved. The reference doc `docs/effects-catalog.md` lists all
keys by category and is regenerated from the seed.

## Naming → classification (how defaults are inferred)
- `*_trail`, `*_arc`, `*_streak` → WeaponTrail (attach, weapon_tip)
- `*_cone/field/zone/domain/cross/lanes/rain/…` → GroundZone (⚠, ground point)
- `*_beam/ray/bolt/gungnir/arrow/shot/…` → Projectile (travel)
- `*_impact/hit/burst/crash/slash/…` → AbilityImpact (spawn at socket)
- `*_charge/gather/summon/transform/…` → AbilityCast
- `*_aura/glow/embers/particle` → Aura (attach, persistent)
- `*_favor/blessing/edge/reward` → Objective (⚠ 🎨)
- `*_death/dissolve/poof/crumble` → Death ; `*_respawn/materialize` → Respawn
- `heal/shield/cleanse/buff` → Heal/Shield/Cleanse/Buff (additive overlays)
- SFX: `roar/chant/voice` → SfxVoice(7) ; `ult/domain/zan` → SfxImpact(8) ;
  `hit/blast/thunder` → SfxImpact(6) ; `woosh/swing/dash` → SfxWhoosh(4) ;
  `cast/magic/portal` → SfxCast(5)
