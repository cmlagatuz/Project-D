# Project D — Combat Animation: Attacks · Reactions · Healing · Death

> Codename **Zantetsuken** · CONFIDENTIAL · Source: `Assets/GameData/Animations/combat_extras.seed.json`

Completes each champion's non-ability coverage. **All reactions and crowd control are triggered by the authoritative server** — never guessed on the client — so animation always matches the real game state. Healing/shield/cleanse are **additive-layer overlays** that never interrupt movement, casts, or attacks.

## Basic-attack chains by weapon class

Swing timing scales by weapon weight: lighter weapons hit sooner and recover faster. `AttackSpeed` scales playback around each weapon's baseline; the combo resets after the idle window.


---

## wol

**Weapon class:** Sword · base 0.7 atk/s · combo reset 1.2s  
*Measured knightly swings; 3rd hit is a heavier overhead. Shield stays raised between swings (defensive read).*

| # | Clip | Hit frame | Total | Cancel |
|---|---|---|---|---|
| 1 | `wol_attack_1` | 0.2s | 0.48s | 0.3s |
| 2 | `wol_attack_2` | 0.22s | 0.5s | 0.3s |
| 3 | `wol_attack_3` | 0.26s | 0.6s | 0.34s |

**Reactions (server-driven):**  
`wol_flinch` (0.2s) · `wol_hit_react` (0.35s) · `wol_knockback` (0.45s) · `wol_knockup` (0.55s) · `wol_stun` (0.6s) · `wol_root` (0.5s) · `wol_silence` (0.5s) · `wol_slow_overlay` (0.0s)

**Healing/shield/cleanse (additive):** heal `wol_heal_receive` + `vfx_heal_glow` · shield `vfx_shield_on`/`vfx_shield_break` · cleanse `wol_cleanse` + `vfx_cleanse_burst`  
*Heal/shield/buff overlays play on the ADDITIVE upper layer so they never interrupt locomotion, casts, or attacks. Shield gain shows a hex-plate flash; shield break shatters it; cleanse sweeps a ring from the feet up.*

**Death & respawn countdown:** `wol_death` + `vfx_death_dissolve` → hold `wol_respawn_wait` at fountain (floating timer: true) → revive `wol_spawn` + `vfx_respawn_materialize`.  
*On death play wol_death then hold wol_respawn_wait at the fountain during the countdown (floating timer shown). At zero, wol_spawn re-materializes with vfx_respawn_materialize.*


---

## garland

**Weapon class:** Greatsword · base 0.55 atk/s · combo reset 1.5s  
*Ponderous two-hit greatsword combo; huge follow-through, slow reset — rewards commitment.*

| # | Clip | Hit frame | Total | Cancel |
|---|---|---|---|---|
| 1 | `garland_attack_1` | 0.32s | 0.7s | 0.4s |
| 2 | `garland_attack_2` | 0.36s | 0.78s | 0.44s |

**Reactions (server-driven):**  
`garland_flinch` (0.2s) · `garland_hit_react` (0.35s) · `garland_knockback` (0.45s) · `garland_knockup` (0.55s) · `garland_stun` (0.6s) · `garland_root` (0.5s) · `garland_silence` (0.5s) · `garland_slow_overlay` (0.0s)

**Healing/shield/cleanse (additive):** heal `garland_heal_receive` + `vfx_heal_glow` · shield `vfx_shield_on`/`vfx_shield_break` · cleanse `garland_cleanse` + `vfx_cleanse_burst`  
*Heal/shield/buff overlays play on the ADDITIVE upper layer so they never interrupt locomotion, casts, or attacks. Shield gain shows a hex-plate flash; shield break shatters it; cleanse sweeps a ring from the feet up.*

**Death & respawn countdown:** `garland_death` + `vfx_death_dissolve` → hold `garland_respawn_wait` at fountain (floating timer: true) → revive `garland_spawn` + `vfx_respawn_materialize`.  
*On death play garland_death then hold garland_respawn_wait at the fountain during the countdown (floating timer shown). At zero, garland_spawn re-materializes with vfx_respawn_materialize.*


---

## firion

**Weapon class:** Bow · base 0.75 atk/s · combo reset 1.0s  
*Default ranged bow auto; on ability the WeaponSocket swaps to lance/sword for that clip then back to bow.*

| # | Clip | Hit frame | Total | Cancel |
|---|---|---|---|---|
| 1 | `firion_attack_1` | 0.16s | 0.4s | 0.22s |
| 2 | `firion_attack_2` | 0.18s | 0.42s | 0.24s |

**Reactions (server-driven):**  
`firion_flinch` (0.2s) · `firion_hit_react` (0.35s) · `firion_knockback` (0.45s) · `firion_knockup` (0.55s) · `firion_stun` (0.6s) · `firion_root` (0.5s) · `firion_silence` (0.5s) · `firion_slow_overlay` (0.0s)

**Healing/shield/cleanse (additive):** heal `firion_heal_receive` + `vfx_heal_glow` · shield `vfx_shield_on`/`vfx_shield_break` · cleanse `firion_cleanse` + `vfx_cleanse_burst`  
*Heal/shield/buff overlays play on the ADDITIVE upper layer so they never interrupt locomotion, casts, or attacks. Shield gain shows a hex-plate flash; shield break shatters it; cleanse sweeps a ring from the feet up.*

**Death & respawn countdown:** `firion_death` + `vfx_death_dissolve` → hold `firion_respawn_wait` at fountain (floating timer: true) → revive `firion_spawn` + `vfx_respawn_materialize`.  
*On death play firion_death then hold firion_respawn_wait at the fountain during the countdown (floating timer shown). At zero, firion_spawn re-materializes with vfx_respawn_materialize.*


---

## emperor

**Weapon class:** Staff · base 0.6 atk/s · combo reset 1.3s  
*Ranged arcane bolts flicked from the staff tip; regal, minimal footwork.*

| # | Clip | Hit frame | Total | Cancel |
|---|---|---|---|---|
| 1 | `emperor_attack_1` | 0.22s | 0.52s | 0.3s |
| 2 | `emperor_attack_2` | 0.24s | 0.54s | 0.32s |

**Reactions (server-driven):**  
`emperor_flinch` (0.2s) · `emperor_hit_react` (0.35s) · `emperor_knockback` (0.45s) · `emperor_knockup` (0.55s) · `emperor_stun` (0.6s) · `emperor_root` (0.5s) · `emperor_silence` (0.5s) · `emperor_slow_overlay` (0.0s)

**Healing/shield/cleanse (additive):** heal `emperor_heal_receive` + `vfx_heal_glow` · shield `vfx_shield_on`/`vfx_shield_break` · cleanse `emperor_cleanse` + `vfx_cleanse_burst`  
*Heal/shield/buff overlays play on the ADDITIVE upper layer so they never interrupt locomotion, casts, or attacks. Shield gain shows a hex-plate flash; shield break shatters it; cleanse sweeps a ring from the feet up.*

**Death & respawn countdown:** `emperor_death` + `vfx_dark_dissolve` → hold `emperor_respawn_wait` at fountain (floating timer: true) → revive `emperor_spawn` + `vfx_respawn_materialize`.  
*On death play emperor_death then hold emperor_respawn_wait at the fountain during the countdown (floating timer shown). At zero, emperor_spawn re-materializes with vfx_respawn_materialize.*


---

## onion_knight

**Weapon class:** DualBlade · base 0.85 atk/s · combo reset 1.0s  
*Fast 3-hit flurry; in Ninja stance clips play ~15% faster (scale by attack speed), Sage stance is ranged bolt autos.*

| # | Clip | Hit frame | Total | Cancel |
|---|---|---|---|---|
| 1 | `onion_knight_attack_1` | 0.14s | 0.34s | 0.18s |
| 2 | `onion_knight_attack_2` | 0.15s | 0.34s | 0.18s |
| 3 | `onion_knight_attack_3` | 0.16s | 0.36s | 0.2s |

**Reactions (server-driven):**  
`onion_knight_flinch` (0.2s) · `onion_knight_hit_react` (0.35s) · `onion_knight_knockback` (0.45s) · `onion_knight_knockup` (0.55s) · `onion_knight_stun` (0.6s) · `onion_knight_root` (0.5s) · `onion_knight_silence` (0.5s) · `onion_knight_slow_overlay` (0.0s)

**Healing/shield/cleanse (additive):** heal `onion_knight_heal_receive` + `vfx_heal_glow` · shield `vfx_shield_on`/`vfx_shield_break` · cleanse `onion_knight_cleanse` + `vfx_cleanse_burst`  
*Heal/shield/buff overlays play on the ADDITIVE upper layer so they never interrupt locomotion, casts, or attacks. Shield gain shows a hex-plate flash; shield break shatters it; cleanse sweeps a ring from the feet up.*

**Death & respawn countdown:** `onion_knight_death` + `vfx_death_dissolve` → hold `onion_knight_respawn_wait` at fountain (floating timer: true) → revive `onion_knight_spawn` + `vfx_respawn_materialize`.  
*On death play onion_knight_death then hold onion_knight_respawn_wait at the fountain during the countdown (floating timer shown). At zero, onion_knight_spawn re-materializes with vfx_respawn_materialize.*


---

## cloud_of_darkness

**Weapon class:** Tentacle · base 0.62 atk/s · combo reset 1.3s  
*Ranged particle-beam jabs from the tentacles; body floats, upper torso leads.*

| # | Clip | Hit frame | Total | Cancel |
|---|---|---|---|---|
| 1 | `cloud_of_darkness_attack_1` | 0.2s | 0.5s | 0.28s |
| 2 | `cloud_of_darkness_attack_2` | 0.22s | 0.52s | 0.3s |

**Reactions (server-driven):**  
`cloud_of_darkness_flinch` (0.2s) · `cloud_of_darkness_hit_react` (0.35s) · `cloud_of_darkness_knockback` (0.45s) · `cloud_of_darkness_knockup` (0.55s) · `cloud_of_darkness_stun` (0.6s) · `cloud_of_darkness_root` (0.5s) · `cloud_of_darkness_silence` (0.5s) · `cloud_of_darkness_slow_overlay` (0.0s)

**Healing/shield/cleanse (additive):** heal `cloud_of_darkness_heal_receive` + `vfx_heal_glow` · shield `vfx_shield_on`/`vfx_shield_break` · cleanse `cloud_of_darkness_cleanse` + `vfx_cleanse_burst`  
*Heal/shield/buff overlays play on the ADDITIVE upper layer so they never interrupt locomotion, casts, or attacks. Shield gain shows a hex-plate flash; shield break shatters it; cleanse sweeps a ring from the feet up.*

**Death & respawn countdown:** `cloud_of_darkness_death` + `vfx_void_dissolve` → hold `cloud_of_darkness_respawn_wait` at fountain (floating timer: true) → revive `cloud_of_darkness_spawn` + `vfx_respawn_materialize`.  
*On death play cloud_of_darkness_death then hold cloud_of_darkness_respawn_wait at the fountain during the countdown (floating timer shown). At zero, cloud_of_darkness_spawn re-materializes with vfx_respawn_materialize.*


---

## cecil

**Weapon class:** Sword · base 0.7 atk/s · combo reset 1.2s  
*Stance-flavored autos: Dark = heavier dark-edged cuts, Paladin = brighter faster cuts (share timing).*

| # | Clip | Hit frame | Total | Cancel |
|---|---|---|---|---|
| 1 | `cecil_attack_1` | 0.2s | 0.48s | 0.3s |
| 2 | `cecil_attack_2` | 0.22s | 0.5s | 0.3s |
| 3 | `cecil_attack_3` | 0.26s | 0.6s | 0.34s |

**Reactions (server-driven):**  
`cecil_flinch` (0.2s) · `cecil_hit_react` (0.35s) · `cecil_knockback` (0.45s) · `cecil_knockup` (0.55s) · `cecil_stun` (0.6s) · `cecil_root` (0.5s) · `cecil_silence` (0.5s) · `cecil_slow_overlay` (0.0s)

**Healing/shield/cleanse (additive):** heal `cecil_heal_receive` + `vfx_heal_glow` · shield `vfx_shield_on`/`vfx_shield_break` · cleanse `cecil_cleanse` + `vfx_cleanse_burst`  
*Heal/shield/buff overlays play on the ADDITIVE upper layer so they never interrupt locomotion, casts, or attacks. Shield gain shows a hex-plate flash; shield break shatters it; cleanse sweeps a ring from the feet up.*

**Death & respawn countdown:** `cecil_death` + `vfx_light_dissolve` → hold `cecil_respawn_wait` at fountain (floating timer: true) → revive `cecil_spawn` + `vfx_respawn_materialize`.  
*On death play cecil_death then hold cecil_respawn_wait at the fountain during the countdown (floating timer shown). At zero, cecil_spawn re-materializes with vfx_respawn_materialize.*


---

## kain

**Weapon class:** Lance · base 0.72 atk/s · combo reset 1.2s  
*Quick two-hit lance pokes with long reach; light footwork keeps him mobile.*

| # | Clip | Hit frame | Total | Cancel |
|---|---|---|---|---|
| 1 | `kain_attack_1` | 0.18s | 0.44s | 0.26s |
| 2 | `kain_attack_2` | 0.2s | 0.46s | 0.28s |

**Reactions (server-driven):**  
`kain_flinch` (0.2s) · `kain_hit_react` (0.35s) · `kain_knockback` (0.45s) · `kain_knockup` (0.55s) · `kain_stun` (0.6s) · `kain_root` (0.5s) · `kain_silence` (0.5s) · `kain_slow_overlay` (0.0s)

**Healing/shield/cleanse (additive):** heal `kain_heal_receive` + `vfx_heal_glow` · shield `vfx_shield_on`/`vfx_shield_break` · cleanse `kain_cleanse` + `vfx_cleanse_burst`  
*Heal/shield/buff overlays play on the ADDITIVE upper layer so they never interrupt locomotion, casts, or attacks. Shield gain shows a hex-plate flash; shield break shatters it; cleanse sweeps a ring from the feet up.*

**Death & respawn countdown:** `kain_death` + `vfx_death_dissolve` → hold `kain_respawn_wait` at fountain (floating timer: true) → revive `kain_spawn` + `vfx_respawn_materialize`.  
*On death play kain_death then hold kain_respawn_wait at the fountain during the countdown (floating timer shown). At zero, kain_spawn re-materializes with vfx_respawn_materialize.*


---

## bartz

**Weapon class:** Sword · base 0.7 atk/s · combo reset 1.2s  
*Adaptable 3-hit sword combo; personality-forward flourishes on the 3rd hit.*

| # | Clip | Hit frame | Total | Cancel |
|---|---|---|---|---|
| 1 | `bartz_attack_1` | 0.2s | 0.48s | 0.3s |
| 2 | `bartz_attack_2` | 0.22s | 0.5s | 0.3s |
| 3 | `bartz_attack_3` | 0.26s | 0.6s | 0.34s |

**Reactions (server-driven):**  
`bartz_flinch` (0.2s) · `bartz_hit_react` (0.35s) · `bartz_knockback` (0.45s) · `bartz_knockup` (0.55s) · `bartz_stun` (0.6s) · `bartz_root` (0.5s) · `bartz_silence` (0.5s) · `bartz_slow_overlay` (0.0s)

**Healing/shield/cleanse (additive):** heal `bartz_heal_receive` + `vfx_heal_glow` · shield `vfx_shield_on`/`vfx_shield_break` · cleanse `bartz_cleanse` + `vfx_cleanse_burst`  
*Heal/shield/buff overlays play on the ADDITIVE upper layer so they never interrupt locomotion, casts, or attacks. Shield gain shows a hex-plate flash; shield break shatters it; cleanse sweeps a ring from the feet up.*

**Death & respawn countdown:** `bartz_death` + `vfx_death_dissolve` → hold `bartz_respawn_wait` at fountain (floating timer: true) → revive `bartz_spawn` + `vfx_respawn_materialize`.  
*On death play bartz_death then hold bartz_respawn_wait at the fountain during the countdown (floating timer shown). At zero, bartz_spawn re-materializes with vfx_respawn_materialize.*


---

## exdeath

**Weapon class:** Greatsword · base 0.55 atk/s · combo reset 1.5s  
*Heavy planted greatsword swings; almost no travel — an immovable bruiser-caster.*

| # | Clip | Hit frame | Total | Cancel |
|---|---|---|---|---|
| 1 | `exdeath_attack_1` | 0.32s | 0.7s | 0.4s |
| 2 | `exdeath_attack_2` | 0.36s | 0.78s | 0.44s |

**Reactions (server-driven):**  
`exdeath_flinch` (0.2s) · `exdeath_hit_react` (0.35s) · `exdeath_knockback` (0.45s) · `exdeath_knockup` (0.55s) · `exdeath_stun` (0.6s) · `exdeath_root` (0.5s) · `exdeath_silence` (0.5s) · `exdeath_slow_overlay` (0.0s)

**Healing/shield/cleanse (additive):** heal `exdeath_heal_receive` + `vfx_heal_glow` · shield `vfx_shield_on`/`vfx_shield_break` · cleanse `exdeath_cleanse` + `vfx_cleanse_burst`  
*Heal/shield/buff overlays play on the ADDITIVE upper layer so they never interrupt locomotion, casts, or attacks. Shield gain shows a hex-plate flash; shield break shatters it; cleanse sweeps a ring from the feet up.*

**Death & respawn countdown:** `exdeath_death` + `vfx_void_dissolve` → hold `exdeath_respawn_wait` at fountain (floating timer: true) → revive `exdeath_spawn` + `vfx_respawn_materialize`.  
*On death play exdeath_death then hold exdeath_respawn_wait at the fountain during the countdown (floating timer shown). At zero, exdeath_spawn re-materializes with vfx_respawn_materialize.*
