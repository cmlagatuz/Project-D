# Project D — Skill Animation Design (All Champions)

> Codename **Zantetsuken** · CONFIDENTIAL · Source of truth: `Assets/GameData/Animations/animations.seed.json`

Phase timings (**startup / active / recovery**, seconds) mirror `AbilityDefinition` so animation and gameplay stay in lockstep. **Motion** = InPlace / RootMotion (clip drives displacement) / Steerable. Weapon motion uses the `WeaponSocket` rig — separate weapons are keyed in the clip, combined weapons animate with the body. Clip names are what artists author in the DCC.

## Universal clips (every champion)

`idle · run · turn_l · turn_r · spawn · recall · death · hit_react · knockup · stun` plus a **basic-attack chain** (2–3 clips) with a hit frame. Reaction clips are server-driven triggers (crowd control, death). Passives are usually **additive-layer** FX with no base-pose change.


---

## Warrior of Light  (`wol`)

**Locomotion:** idle `wol_idle` · run `wol_run` · spawn `wol_spawn` · recall `wol_recall` · death `wol_death`  
**Basic attacks:** `wol_attack_1`, `wol_attack_2`, `wol_attack_3` · hit frame 0.24s


### Passive — Crystal Resolve  ·  clip `wol_passive_resolve`
- **Timing (startup/active/recovery):** 0.0 / 0.0 / 0.0s · cancel @ 0.0s · targeting Self · **loop/stance**
- **VFX cues:** `vfx_resolve_shield_glow`@0.0s (shield)
- **SFX cues:** —
- **Animation:** No dedicated clip; a shield-glow additive layer pulses on the shield socket as Resolve stacks. Blend intensity with stack count on an additive layer so it never interrupts locomotion.

### Q — Radiant Cut  ·  clip `wol_q_radiant_cut`
- **Timing (startup/active/recovery):** 0.18 / 0.12 / 0.3s · cancel @ 0.3s · targeting Directional · motion **Steerable** ~0.5m · trail `vfx_light_slash_trail`
- **VFX cues:** `vfx_radiant_arc`@0.18s (weapon_tip), `vfx_light_impact`@0.3s (weapon_tip)
- **SFX cues:** `sfx_sword_woosh`@0.18s (root), `sfx_light_hit`@0.24s (root)
- **Animation:** Right-to-left horizontal sword arc. Wind-up (0.18) draws the blade back over the shoulder with a small hip coil; active (0.12) sweeps the arc — spawn the light-trail along the blade and detonate the outer-edge burst at the arc's far end. Recovery re-centres to a guard stance. Cancelable into move after 0.30.

### W — Shield of Light  ·  clip `wol_w_shield_guard`
- **Timing (startup/active/recovery):** 0.14 / 0.6 / 0.22s · cancel @ 0.74s · targeting Directional · charge `wol_w_guard_loop`
- **VFX cues:** `vfx_light_barrier`@0.14s (shield), `vfx_light_wave`@0.74s (shield)
- **SFX cues:** `sfx_shield_raise`@0.14s (root), `sfx_light_wave`@0.74s (root)
- **Animation:** Plant the shield forward. Startup (0.14) snaps the shield up and braces the legs; the guard loop (charge clip) holds while damage is reduced (blend to a strained hold if hit); on release the wave fires — kick the shield forward a few cm and flash the barrier. Upper body on an isolated layer so the character can still be nudged.

### E — Vanguard Step  ·  clip `wol_e_vanguard_step`
- **Timing (startup/active/recovery):** 0.1 / 0.22 / 0.24s · cancel @ 0.32s · targeting Unit · motion **RootMotion** ~5.0m · trail `vfx_dash_streak`
- **VFX cues:** `vfx_dash_start`@0.1s (root), `vfx_shield_bash`@0.32s (shield)
- **SFX cues:** `sfx_dash`@0.1s (root), `sfx_shield_bash`@0.32s (root)
- **Animation:** Explosive shield-first dash. Short crouch load (0.10), then root-motion lunge covering up to 5 m; on an enemy arrival play a shield-bash contact that displaces (spawn bash VFX on the shield socket). On an ally arrival, skip the bash and slide to a protective stop.

### Ultimate (R) — Shining Bulwark  ·  clip `wol_r_shining_bulwark`
- **Timing (startup/active/recovery):** 0.35 / 0.3 / 0.45s · cancel @ 0.65s · targeting Point
- **VFX cues:** `vfx_bulwark_field`@0.35s (root), `vfx_enemy_mark`@0.45s (root)
- **SFX cues:** `sfx_ult_charge`@0.1s (root), `sfx_bulwark_slam`@0.35s (root)
- **Animation:** Raise sword and shield skyward then slam the shield to the ground to project the field. Big anticipation (0.35) with a rising camera-worthy pose; on the slam, spawn the dome field and pulse the enemy marks outward. Long recovery sells the weight; super-armor during startup so it reads as a committed team ult.

---

## Garland  (`garland`)

**Locomotion:** idle `garland_idle` · run `garland_run` · spawn `garland_spawn` · recall `garland_recall` · death `garland_death`  
**Basic attacks:** `garland_attack_1`, `garland_attack_2` · hit frame 0.3s


### Passive — Cycle of Strife  ·  clip `garland_passive_cycle`
- **Timing (startup/active/recovery):** 0.0 / 0.0 / 0.0s · cancel @ 0.0s · targeting Self · **loop/stance**
- **VFX cues:** `vfx_strife_embers`@0.0s (chest)
- **SFX cues:** —
- **Animation:** Additive ember/armor-plating glow that intensifies per stack on an additive upper-body layer; fades when stacks drop out of combat. No base-pose change.

### Q — Greatblade Reap  ·  clip `garland_q_reap`
- **Timing (startup/active/recovery):** 0.26 / 0.18 / 0.34s · cancel @ 0.44s · targeting Directional · trail `vfx_heavy_arc`
- **VFX cues:** `vfx_reap_arc`@0.26s (weapon_edge), `vfx_pull_wisps`@0.3s (root)
- **SFX cues:** `sfx_greatsword_swing`@0.26s (root), `sfx_heavy_hit`@0.34s (root)
- **Animation:** Wide two-handed horizontal reap. Heavy back-coil (0.26) plants the rear foot; the sweep drags edge targets inward — emit inward wisps toward Garland during active. Big follow-through past the hip; slow recovery communicates commitment.

### W — Iron Advance  ·  clip `garland_w_advance`
- **Timing (startup/active/recovery):** 0.3 / 0.28 / 0.3s · cancel @ 0.0s · targeting Directional · motion **RootMotion** ~4.5m · charge `garland_w_windup`
- **VFX cues:** `vfx_unstoppable_aura`@0.0s (root), `vfx_shoulder_impact`@0.55s (chest)
- **SFX cues:** `sfx_armor_brace`@0.05s (root), `sfx_knockback`@0.55s (root)
- **Animation:** Unstoppable shoulder charge. Startup is a braced wind-up loop (charge clip) with an unstoppable aura; on release, root-motion charge ~4.5 m ending in a shoulder-check knockback. Keep the sword low/trailing so the body silhouette leads.

### E — Chain of Ruin  ·  clip `garland_e_chain`
- **Timing (startup/active/recovery):** 0.2 / 0.14 / 0.26s · cancel @ 0.34s · targeting Directional · recast `garland_e_lunge` · trail `vfx_chain_glow`
- **VFX cues:** `vfx_chain_throw`@0.2s (weapon_tip), `vfx_chain_hook`@0.34s (weapon_tip)
- **SFX cues:** `sfx_chain_throw`@0.2s (root), `sfx_chain_reel`@0.6s (root)
- **Animation:** Throw a chain-blade forward (over-the-shoulder toss). On hit it slows; the RECAST clip reels Garland to the target with a short root-motion lunge (author as a separate clip triggered by recast). Keep the throw's off-hand anchored for a believable chain line.

### Ultimate (R) — Chaosbringer  ·  clip `garland_r_chaosbringer`
- **Timing (startup/active/recovery):** 0.4 / 0.0 / 0.4s · cancel @ 0.4s · targeting Self · **loop/stance**
- **VFX cues:** `vfx_chaos_erupt`@0.2s (root), `vfx_size_grow`@0.4s (root)
- **SFX cues:** `sfx_transform_growl`@0.1s (root), `sfx_chaos_boom`@0.4s (root)
- **Animation:** Transformation buff. A one-shot transform-in (0.40) roars and erupts dark energy while a scale/aura blend grows the silhouette; then locomotion/attacks swap to an EMPOWERED layer for the duration (author empowered idle/run/attack variants). Shockwaves fire on a repeating additive event, not the base clip.

---

## Firion  (`firion`)

**Locomotion:** idle `firion_idle` · run `firion_run` · spawn `firion_spawn` · recall `firion_recall` · death `firion_death`  
**Basic attacks:** `firion_bow_1`, `firion_bow_2` · hit frame 0.18s


### Passive — Arsenal Rhythm  ·  clip `firion_passive_rhythm`
- **Timing (startup/active/recovery):** 0.0 / 0.0 / 0.0s · cancel @ 0.0s · targeting Self · **loop/stance**
- **VFX cues:** `vfx_rhythm_glint`@0.0s (hand_r)
- **SFX cues:** —
- **Animation:** No pose change; a small weapon-glint FX plays each time the ability CLASS changes (handled by the effects system reading the passive). Optionally a subtle stance micro-shift on the additive layer per stack.

### Q — Spearline  ·  clip `firion_q_spearline`
- **Timing (startup/active/recovery):** 0.16 / 0.1 / 0.24s · cancel @ 0.26s · targeting Directional · trail `vfx_lance_thrust`
- **VFX cues:** `vfx_pierce_line`@0.16s (weapon_tip), `vfx_sweetspot_flash`@0.26s (weapon_tip)
- **SFX cues:** `sfx_lance_thrust`@0.16s (root)
- **Animation:** Swap to LANCE for a piercing straight thrust. Quick chamber (0.16) pulls the lance to the hip, then a snappy forward stab; flash the sweet-spot burst at max reach. Weapon swap is instantaneous via the WeaponSocket (hide bow, show lance for the clip's duration).

### W — Bow Volley  ·  clip `firion_w_volley`
- **Timing (startup/active/recovery):** 0.24 / 0.1 / 0.26s · cancel @ 0.34s · targeting Point
- **VFX cues:** `vfx_arrow_loose`@0.24s (weapon_tip), `vfx_arrow_rain`@0.4s (root)
- **SFX cues:** `sfx_bow_release`@0.24s (root)
- **Animation:** Draw the BOW and loose a high volley toward the point. Draw-and-hold on startup (0.24), release arcs arrows up; the rain + reveal resolve at the target. Keep the draw arm on an isolated layer so lower body can micro-adjust facing.

### E — Blood Blade  ·  clip `firion_e_blood_blade`
- **Timing (startup/active/recovery):** 0.14 / 0.12 / 0.22s · cancel @ 0.26s · targeting Unit · trail `vfx_lifesteal_arc`
- **VFX cues:** `vfx_sword_slash`@0.14s (weapon_edge), `vfx_life_drain`@0.26s (chest)
- **SFX cues:** `sfx_sword_slash`@0.14s (root), `sfx_heal_pulse`@0.26s (root)
- **Animation:** Swap to SWORD for a close upward slash that heals. Short step-in, rising diagonal cut; on contact draw life-wisps back into Firion's chest (heal cue). Snappy, low-commitment recovery to keep him kiting.

### Ultimate (R) — Wild Rose Arsenal  ·  clip `firion_r_arsenal`
- **Timing (startup/active/recovery):** 0.3 / 0.7 / 0.35s · cancel @ 1.0s · targeting Directional · charge `firion_r_arsenal_loop` · trail `vfx_weapon_storm`
- **VFX cues:** `vfx_arsenal_summon`@0.3s (root), `vfx_finishing_shot`@1.0s (weapon_tip)
- **SFX cues:** `sfx_arsenal_rise`@0.3s (root), `sfx_final_loose`@1.0s (root)
- **Animation:** Summon a storm of spectral weapons that fire in sequence (active loop), ending in a controllable finishing shot. Startup raises the arsenal around him; the loop rapidly projects weapons forward; the finisher is a single big aimed shot — snap to an aiming pose, then release. Player may steer the final shot during the loop.

---

## The Emperor  (`emperor`)

**Locomotion:** idle `emperor_idle` · run `emperor_glide` · spawn `emperor_spawn` · recall `emperor_recall` · death `emperor_death`  
**Basic attacks:** `emperor_cast_1`, `emperor_cast_2` · hit frame 0.22s


### Passive — Dominion  ·  clip `emperor_passive_dominion`
- **Timing (startup/active/recovery):** 0.0 / 0.0 / 0.0s · cancel @ 0.0s · targeting Self · **loop/stance**
- **VFX cues:** —
- **SFX cues:** —
- **Animation:** No clip; an 'Exposed' mark FX is applied to enemies by the effects system. Optional subtle crown/eye glow additive when a trap is armed nearby.

### Q — Thunder Sigil  ·  clip `emperor_q_sigil`
- **Timing (startup/active/recovery):** 0.22 / 0.1 / 0.26s · cancel @ 0.32s · targeting Point
- **VFX cues:** `vfx_sigil_place`@0.22s (weapon_tip), `vfx_thunder_strike`@0.55s (root)
- **SFX cues:** `sfx_sigil_cast`@0.22s (root), `sfx_thunder`@0.55s (root)
- **Animation:** Point the staff to inscribe a delayed lightning seal. A commanding downward staff-point on startup places the sigil; the strike resolves after the ability's delay (VFX-driven, not in the body clip). Regal, minimal body travel.

### W — Hell Snare  ·  clip `emperor_w_snare`
- **Timing (startup/active/recovery):** 0.2 / 0.1 / 0.24s · cancel @ 0.3s · targeting Point
- **VFX cues:** `vfx_snare_plant`@0.2s (hand_l)
- **SFX cues:** `sfx_dark_whisper`@0.2s (root)
- **Animation:** Subtle off-hand gesture that plants a HIDDEN trap (low tell on purpose). Small, quiet cast — a flick of the free hand toward the point; keep animation understated so opponents can't easily read it from silhouette.

### E — Imperial Repulse  ·  clip `emperor_e_repulse`
- **Timing (startup/active/recovery):** 0.18 / 0.12 / 0.28s · cancel @ 0.3s · targeting Directional
- **VFX cues:** `vfx_repulse_cone`@0.18s (chest), `vfx_pushback`@0.3s (root)
- **SFX cues:** `sfx_force_blast`@0.18s (root)
- **Animation:** Thrust the free palm forward to emit a repelling cone. Body leans back on startup then punches the palm out; sell the recoil in recovery. Cone VFX from the chest/palm; enemies pushed toward prepared zones.

### Ultimate (R) — Pandemonium  ·  clip `emperor_r_pandemonium`
- **Timing (startup/active/recovery):** 0.45 / 0.2 / 0.4s · cancel @ 0.65s · targeting Point
- **VFX cues:** `vfx_ult_charge`@0.2s (weapon_tip), `vfx_domain_expand`@0.45s (root)
- **SFX cues:** `sfx_ult_charge`@0.1s (root), `sfx_domain`@0.45s (root)
- **Animation:** Raise the staff two-handed overhead to conjure a domain. Long theatrical charge (0.45) with the staff aloft and cape flare; on release the domain expands across the target area (bolts + faster trap arming are VFX/gameplay driven). Recovery lowers the staff with menace.

---

## Onion Knight  (`onion_knight`)

**Locomotion:** idle `ok_idle` · run `ok_run` · spawn `ok_spawn` · recall `ok_recall` · death `ok_death`  
**Basic attacks:** `ok_attack_1`, `ok_attack_2`, `ok_attack_3` · hit frame 0.16s


### Passive — Job Mastery  ·  clip `ok_passive_mastery`
- **Timing (startup/active/recovery):** 0.0 / 0.0 / 0.0s · cancel @ 0.0s · targeting Self · **loop/stance**
- **VFX cues:** `vfx_job_glow`@0.0s (hand_r)
- **SFX cues:** —
- **Animation:** A colored aura (blue=Ninja bonus ready, gold=Sage bonus ready) on an additive layer that flips when the next opposite-type cast is empowered. No pose change.

### Q — Swift Blade  ·  clip `ok_q_swift_blade`
- **Timing (startup/active/recovery):** 0.1 / 0.14 / 0.18s · cancel @ 0.24s · targeting Directional · motion **RootMotion** ~4.0m · recast `ok_q_reset` · trail `vfx_blade_streak`
- **VFX cues:** `vfx_dash_slash`@0.1s (weapon_edge)
- **SFX cues:** `sfx_quick_slash`@0.1s (root)
- **Animation:** Fast dash-slash. Minimal wind-up, root-motion dart-through ~4 m with a single clean cut; on a reset (marked target/kill) blend instantly back to the same clip via the recast so it reads as a chain. In NINJA stance the dash is longer/faster (scale root motion up).

### W — Dualcast  ·  clip `ok_w_dualcast`
- **Timing (startup/active/recovery):** 0.16 / 0.12 / 0.22s · cancel @ 0.28s · targeting Directional
- **VFX cues:** `vfx_bolt_1`@0.16s (hand_l), `vfx_bolt_2`@0.22s (hand_r)
- **SFX cues:** `sfx_magic_cast`@0.16s (root)
- **Animation:** Two quick hand-casts launching compact bolts. Alternate hands for the two bolts. In SAGE stance widen the stance and add a small back-lean for a 'caster' read; bolts travel further (VFX/gameplay). Sword sheathed for this clip.

### E — Job Shift  ·  clip `ok_e_job_shift`
- **Timing (startup/active/recovery):** 0.12 / 0.0 / 0.16s · cancel @ 0.12s · targeting Self
- **VFX cues:** `vfx_stance_swap`@0.12s (root)
- **SFX cues:** `sfx_stance_swap`@0.12s (root)
- **Animation:** Instant stance toggle. A quick spin/flourish that swaps the crystal's color and switches the ACTIVE locomotion+ability layer between Ninja (agile, low stance) and Sage (upright, staff-grip). Very short so it can be woven mid-combo.

### Ultimate (R) — Mastered Jobs  ·  clip `ok_r_mastered`
- **Timing (startup/active/recovery):** 0.3 / 0.0 / 0.3s · cancel @ 0.3s · targeting Self · **loop/stance**
- **VFX cues:** `vfx_all_jobs`@0.15s (root), `vfx_dual_aura`@0.3s (root)
- **SFX cues:** `sfx_ult_power`@0.1s (root), `sfx_dual_aura`@0.3s (root)
- **Animation:** Brief empowerment activating BOTH stances. Transform-in flourish summons ghost-images of multiple jobs; for the duration, both Ninja and Sage bonuses apply — drive an empowered dual aura and enable a special finishing recast (author ok_r_finisher). One-shot in, buff layer for uptime.

---

## Cloud of Darkness  (`cloud_of_darkness`)

**Locomotion:** idle `cod_idle_float` · run `cod_float_move` · spawn `cod_spawn` · recall `cod_recall` · death `cod_death`  
**Basic attacks:** `cod_beam_1`, `cod_beam_2` · hit frame 0.2s


### Passive — Particle Beam  ·  clip `cod_passive_particle`
- **Timing (startup/active/recovery):** 0.0 / 0.0 / 0.0s · cancel @ 0.0s · targeting Self · **loop/stance**
- **VFX cues:** —
- **SFX cues:** —
- **Animation:** Void stacks are FX on the target. Idle floats; the tentacles subtly writhe on an additive layer that grows more agitated at higher stacks.

### Q — Zero Ray  ·  clip `cod_q_zero_ray`
- **Timing (startup/active/recovery):** 0.24 / 0.2 / 0.28s · cancel @ 0.44s · targeting Directional · charge `cod_q_charge` · trail `vfx_void_beam`
- **VFX cues:** `vfx_beam_fire`@0.24s (chest)
- **SFX cues:** `sfx_void_beam`@0.24s (root)
- **Animation:** Cross the tentacles forward and fire a narrow beam from the chest. Startup gathers void energy (optional charge loop extends range); the active holds the beam pose. Almost no lower-body travel — she floats in place; upper body braces against the beam recoil.

### W — Dark Flood  ·  clip `cod_w_flood`
- **Timing (startup/active/recovery):** 0.2 / 0.14 / 0.24s · cancel @ 0.34s · targeting Directional
- **VFX cues:** `vfx_flood_advance`@0.2s (root)
- **SFX cues:** `sfx_void_wash`@0.2s (root)
- **Animation:** Sweep an arm low to release an advancing floor of void. A slow, graceful arm sweep; the field advances forward (VFX). Keep her serene and imperious — minimal effort read, maximum menace.

### E — Unbound Form  ·  clip `cod_e_unbound`
- **Timing (startup/active/recovery):** 0.08 / 0.4 / 0.14s · cancel @ 0.48s · targeting Self · motion **Steerable** ~3.0m · **loop/stance**
- **VFX cues:** `vfx_dissolve_in`@0.08s (root), `vfx_dissolve_out`@0.48s (root)
- **SFX cues:** `sfx_phase`@0.08s (root)
- **Animation:** Dissolve into a drifting void form (untargetable). Body dissolves to particles and drifts (steerable, slow); no casting allowed during the loop. Re-form on exit. Great for dodging — the silhouette should clearly read as 'intangible' during the drift.

### Ultimate (R) — World of Nothing  ·  clip `cod_r_world`
- **Timing (startup/active/recovery):** 0.4 / 0.3 / 0.4s · cancel @ 0.7s · targeting Point
- **VFX cues:** `vfx_ult_gather`@0.2s (chest), `vfx_void_zone`@0.4s (root)
- **SFX cues:** `sfx_ult_gather`@0.1s (root), `sfx_void_detonate`@0.4s (root)
- **Animation:** Spread both arms wide, tentacles flared, to open an expanding void zone that reduces vision and detonates stored Void. Grand cruciform pose on the gather; on release the zone blooms outward. Slow, dominant recovery folding the arms back in.

---

## Cecil Harvey  (`cecil`)

**Locomotion:** idle `cecil_idle` · run `cecil_run` · spawn `cecil_spawn` · recall `cecil_recall` · death `cecil_death`  
**Basic attacks:** `cecil_attack_1`, `cecil_attack_2` · hit frame 0.26s


### Passive — Dual Nature  ·  clip `cecil_passive_dual`
- **Timing (startup/active/recovery):** 0.0 / 0.0 / 0.0s · cancel @ 0.0s · targeting Self · **loop/stance**
- **VFX cues:** `vfx_light_gauge`@0.0s (chest)
- **SFX cues:** —
- **Animation:** A Light-gauge glow builds on the chest while in Dark Knight; consumed visibly when Paladin spends it. Additive layer only; base locomotion swaps with the stance layer.

### Q — Darkness / Holy Edge  ·  clip `cecil_q_edge`
- **Timing (startup/active/recovery):** 0.2 / 0.14 / 0.28s · cancel @ 0.34s · targeting Directional · trail `vfx_edge_trail`
- **VFX cues:** `vfx_dark_cleave`@0.2s (weapon_edge), `vfx_holy_slash`@0.2s (weapon_edge)
- **SFX cues:** `sfx_heavy_slash`@0.2s (root)
- **Animation:** Stance-dependent cleave. DARK: a brooding downward cleave that costs HP — heavier, hunched. PALADIN: a brighter rising slash. Author BOTH as variants selected by stance; share timing so gameplay stays identical. Dark trail is violet, holy trail is gold.

### W — Shadow March / Cover  ·  clip `cecil_w_cover`
- **Timing (startup/active/recovery):** 0.14 / 0.22 / 0.22s · cancel @ 0.36s · targeting Unit · motion **RootMotion** ~5.0m
- **VFX cues:** `vfx_dark_aura`@0.14s (root), `vfx_cover_shield`@0.36s (chest)
- **SFX cues:** `sfx_leap`@0.14s (root)
- **Animation:** PALADIN: a protective leap to an ally ending in a shielding guard pose (root-motion ~5 m). DARK: an in-place pressure-aura stomp instead (no travel — gate root motion by stance). The leap's landing plants firmly to sell 'I've got you'.

### E — Nightfall / Radiant Chain  ·  clip `cecil_e_field`
- **Timing (startup/active/recovery):** 0.18 / 0.14 / 0.24s · cancel @ 0.32s · targeting Point
- **VFX cues:** `vfx_slow_field`@0.18s (weapon_tip), `vfx_holy_tether`@0.18s (weapon_tip)
- **SFX cues:** `sfx_field_cast`@0.18s (root)
- **Animation:** DARK: stab the sword down to spawn a slow field. PALADIN: point the sword to tether/protect an ally. Same body clip, different weapon glow + VFX by stance.

### Ultimate (R) — Redemption  ·  clip `cecil_r_redemption`
- **Timing (startup/active/recovery):** 0.5 / 0.0 / 0.35s · cancel @ 0.5s · targeting Self
- **VFX cues:** `vfx_transform_light`@0.25s (root), `vfx_cleanse_burst`@0.5s (root)
- **SFX cues:** `sfx_transform`@0.1s (root), `sfx_cleanse`@0.5s (root)
- **Animation:** The signature Dark↔Paladin transformation. A held, cinematic pose (0.50): armor sheds shadow / dons light (or vice-versa), a cleanse burst clears debuffs, and the ACTIVE stance layer swaps. Sell the identity change — this is his hero moment.

---

## Kain Highwind  (`kain`)

**Locomotion:** idle `kain_idle` · run `kain_run` · spawn `kain_spawn` · recall `kain_recall` · death `kain_death`  
**Basic attacks:** `kain_attack_1`, `kain_attack_2` · hit frame 0.22s


### Passive — Dragoon's Poise  ·  clip `kain_passive_poise`
- **Timing (startup/active/recovery):** 0.0 / 0.0 / 0.0s · cancel @ 0.0s · targeting Self · **loop/stance**
- **VFX cues:** —
- **SFX cues:** —
- **Animation:** No clip; a speed buff toward isolated enemies. Optionally a faint wind-streak additive when the bonus is active.

### Q — Lancet Thrust  ·  clip `kain_q_lancet`
- **Timing (startup/active/recovery):** 0.14 / 0.1 / 0.22s · cancel @ 0.24s · targeting Directional · trail `vfx_lance_line`
- **VFX cues:** `vfx_thrust_pierce`@0.14s (weapon_tip), `vfx_lifesteal`@0.24s (chest)
- **SFX cues:** `sfx_lance_thrust`@0.14s (root)
- **Animation:** Straight lance thrust that heals vs champions. Chamber the lance to the hip, explosive forward stab, draw a life-wisp back on hit. Crisp and low-commitment.

### W — Vault  ·  clip `kain_w_vault`
- **Timing (startup/active/recovery):** 0.1 / 0.3 / 0.16s · cancel @ 0.4s · targeting Directional · motion **RootMotion** ~4.0m · trail `vfx_hop_arc`
- **VFX cues:** `vfx_jump_dust`@0.1s (feet), `vfx_land_strike`@0.4s (weapon_tip)
- **SFX cues:** `sfx_jump`@0.1s (root), `sfx_land_hit`@0.4s (root)
- **Animation:** A short hop OVER terrain with a landing strike. Crouch-load, arc up and forward (root motion clears walls), spear-first landing that hits. Airtime pose reads as a mini-dragoon-jump, foreshadowing the ult.

### E — Dragon Pin  ·  clip `kain_e_pin`
- **Timing (startup/active/recovery):** 0.18 / 0.1 / 0.24s · cancel @ 0.28s · targeting Directional
- **VFX cues:** `vfx_spear_throw`@0.18s (weapon_tip), `vfx_pin_mark`@0.3s (weapon_tip)
- **SFX cues:** `sfx_spear_throw`@0.18s (root)
- **Animation:** Throw the lance to mark+slow (then he fights unarmed briefly or pulls a spare — art choice; recommend a quick spectral lance refill on recovery so his hand isn't empty). Over-shoulder javelin throw with full body torque.

### Ultimate (R) — High Jump  ·  clip `kain_r_high_jump`
- **Timing (startup/active/recovery):** 0.3 / 1.2 / 0.35s · cancel @ 1.5s · targeting Point · motion **RootMotion** ~12.0m · charge `kain_r_airborne` · trail `vfx_dive_trail`
- **VFX cues:** `vfx_launch`@0.3s (feet), `vfx_crash_impact`@1.5s (weapon_tip)
- **SFX cues:** `sfx_launch`@0.3s (root), `sfx_crash`@1.5s (root)
- **Animation:** The iconic dragoon leap. Deep crouch-load (0.30) then LAUNCH off-screen (untargetable) — hold an airborne loop while the player selects the landing point — then CRASH straight down, lance-first, into a knock-up. Three clips: launch, airborne_loop (charge), crash. Camera-worthy; the airborne window doubles as a dodge.

---

## Bartz Klauser  (`bartz`)

**Locomotion:** idle `bartz_idle` · run `bartz_run` · spawn `bartz_spawn` · recall `bartz_recall` · death `bartz_death`  
**Basic attacks:** `bartz_attack_1`, `bartz_attack_2`, `bartz_attack_3` · hit frame 0.2s


### Passive — Freelancer  ·  clip `bartz_passive_freelancer`
- **Timing (startup/active/recovery):** 0.0 / 0.0 / 0.0s · cancel @ 0.0s · targeting Self · **loop/stance**
- **VFX cues:** —
- **SFX cues:** —
- **Animation:** No clip; small stat bonuses from item categories. Optional micro-idle variety (weight shifts) to sell his easygoing personality.

### Q — Spellblade Arc  ·  clip `bartz_q_spellblade`
- **Timing (startup/active/recovery):** 0.18 / 0.14 / 0.26s · cancel @ 0.32s · targeting Directional · trail `vfx_element_arc`
- **VFX cues:** `vfx_element_charge`@0.18s (weapon_edge), `vfx_element_burst`@0.32s (weapon_edge)
- **SFX cues:** `sfx_spellblade`@0.18s (root)
- **Animation:** Enchant the blade with a chosen element then a diagonal slash. Startup flares the element along the blade (recolor the trail by element: fire/ice/etc.); the slash bursts on contact. One clip, element-swappable VFX.

### W — Ranger Step  ·  clip `bartz_w_ranger`
- **Timing (startup/active/recovery):** 0.14 / 0.1 / 0.2s · cancel @ 0.24s · targeting Directional · motion **Steerable** ~1.5m
- **VFX cues:** `vfx_arrow_snap`@0.14s (weapon_tip)
- **SFX cues:** `sfx_bow_snap`@0.14s (root)
- **Animation:** A quick mobile bow-shot: a light back-hop (steerable) while loosing an arrow, so he can fire-and-reposition. Swap to bow via WeaponSocket for the clip; snappy and kite-friendly.

### E — Guard Job  ·  clip `bartz_e_guard`
- **Timing (startup/active/recovery):** 0.1 / 0.0 / 0.16s · cancel @ 0.1s · targeting Self · **loop/stance** · charge `bartz_e_guard_loop`
- **VFX cues:** `vfx_guard_stance`@0.1s (root)
- **SFX cues:** `sfx_guard_up`@0.1s (root)
- **Animation:** A brief defensive stance converting damage to speed. Quick snap into a braced guard loop (charge clip) with a shimmer; on expiry, a small release. Keep it short and readable so enemies know the window.

### Ultimate (R) — Job Parade  ·  clip `bartz_r_parade`
- **Timing (startup/active/recovery):** 0.25 / 0.0 / 0.25s · cancel @ 0.25s · targeting Self · **loop/stance** · recast `bartz_r_job_action`
- **VFX cues:** `vfx_job_summon`@0.12s (root)
- **SFX cues:** `sfx_job_parade`@0.12s (root)
- **Animation:** Cycle three pre-chosen empowered job actions. A quick 'call the jobs' flourish enters the state; each of the three recasts plays a distinct job-action clip (author bartz_r_job_action as a small library keyed by loadout). Personality-forward — a showman's spin between actions.

---

## Exdeath  (`exdeath`)

**Locomotion:** idle `exdeath_idle` · run `exdeath_stride` · spawn `exdeath_spawn` · recall `exdeath_recall` · death `exdeath_death`  
**Basic attacks:** `exdeath_attack_1`, `exdeath_attack_2` · hit frame 0.3s


### Passive — Rooted in the Void  ·  clip `exdeath_passive_rooted`
- **Timing (startup/active/recovery):** 0.0 / 0.0 / 0.0s · cancel @ 0.0s · targeting Self · **loop/stance**
- **VFX cues:** `vfx_root_energy`@0.0s (feet)
- **SFX cues:** —
- **Animation:** While standing still, defense builds and void energy gathers at the feet (additive glow intensifies over time); moving 'spends' it (a brief release FX). Base idle stays planted and imposing.

### Q — Black Branch  ·  clip `exdeath_q_branch`
- **Timing (startup/active/recovery):** 0.26 / 0.12 / 0.3s · cancel @ 0.38s · targeting Directional
- **VFX cues:** `vfx_branch_line`@0.26s (weapon_tip), `vfx_branch_erupt`@0.6s (root)
- **SFX cues:** `sfx_dark_cast`@0.26s (root), `sfx_roots_erupt`@0.6s (root)
- **Animation:** Slam or point the greatsword to send a line of dark roots that erupt after a delay. Heavy two-handed downward point on startup; the eruption is delayed (VFX/gameplay). Minimal travel — he's an immovable caster-bruiser.

### W — Gravity Well  ·  clip `exdeath_w_gravity`
- **Timing (startup/active/recovery):** 0.24 / 0.16 / 0.28s · cancel @ 0.4s · targeting Point
- **VFX cues:** `vfx_gravity_form`@0.24s (hand_l), `vfx_gravity_pull`@0.4s (root)
- **SFX cues:** `sfx_gravity`@0.24s (root)
- **Animation:** Raise the free hand and clench to form a well that pulls enemies inward. Slow, deliberate arm raise then a fist-clench that triggers the pull. Weighty, sorcerous, unhurried.

### E — Void Gate  ·  clip `exdeath_e_gate`
- **Timing (startup/active/recovery):** 0.2 / 0.12 / 0.24s · cancel @ 0.32s · targeting Point
- **VFX cues:** `vfx_portal_open_a`@0.2s (hand_r), `vfx_portal_open_b`@0.28s (root)
- **SFX cues:** `sfx_portal`@0.2s (root)
- **Animation:** Open a paired portal set with an outward two-hand gesture. Both portals bloom (one at Exdeath, one at the point). Understated body motion — the FX does the talking.

### Ultimate (R) — Grand Cross  ·  clip `exdeath_r_grand_cross`
- **Timing (startup/active/recovery):** 0.55 / 0.3 / 0.45s · cancel @ 0.85s · targeting Point
- **VFX cues:** `vfx_cross_gather`@0.25s (weapon_tip), `vfx_grand_cross`@0.55s (root)
- **SFX cues:** `sfx_ult_charge`@0.1s (root), `sfx_grand_cross`@0.55s (root)
- **Animation:** Raise the greatsword to form a great cross of void that patterns the zone with sequential detonations + random debuffs. Long, grand anticipation (0.55) with the blade aloft and cape billow; on release the cross imprints and detonations march outward. Slow, final-boss recovery.