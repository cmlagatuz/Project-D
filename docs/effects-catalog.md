# Project D — VFX/SFX Key Catalog

> Codename **Zantetsuken** · CONFIDENTIAL · Source: `Assets/GameData/Effects_catalog.seed.json`

Every cue key referenced by the animation designs has a defined asset slot + spawn rule. **333 keys total** (213 VFX, 120 SFX). Import into `EffectDefinition` assets (Tools ▸ Project D ▸ Import Effect Catalog), then bind real prefabs/clips. The runtime `EffectSpawner`/`AudioCuePlayer` resolve keys via the `EffectCatalog` and apply these rules.

## Spawn rules

| Rule | Meaning |
|---|---|
| AttachToSocket | Parented to a bone/socket; follows it (auras, held trails, shields). |
| SpawnAtSocket | Spawned at the socket world pos; detaches (most impacts). |
| SpawnAtGroundPoint | Projected on the ground at the aim/target point (zones, telegraphs). |
| ProjectileTravel | Spawned at origin, travels to target (handled by the projectile system). |
| SpawnAtRoot / SpawnAtTarget / WorldZone | At caster root / target / large persistent world zone. |

**Legend:** ⚠ = danger telegraph (never reduced by accessibility), 🎨 = team-colored. Lifetime in seconds (0/∞ handled specially).


---

# VFX


## AbilityCast  (24)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_adamantoise_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_ahriman_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_arsenal_summon` | SpawnAtSocket | root | 1.2 | — |
| `vfx_behemoth_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_blade_gather` | SpawnAtSocket | sword | 1.2 | — |
| `vfx_bomb_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_coeurl_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_element_charge` | SpawnAtSocket | weapon_edge | 1.2 | — |
| `vfx_enemy_mark` | SpawnAtSocket | root | 1.2 | — |
| `vfx_flan_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_floating_eye_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_gravity_form` | SpawnAtSocket | hand_l | 1.2 | — |
| `vfx_guard_stance` | SpawnAtSocket | root | 1.2 | — |
| `vfx_iron_giant_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_job_summon` | SpawnAtSocket | root | 1.2 | — |
| `vfx_malboro_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_mu_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_pin_mark` | SpawnAtSocket | weapon_tip | 1.2 | — |
| `vfx_sahagin_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_stance_swap` | SpawnAtSocket | root | 1.2 | — |
| `vfx_tonberry_windup` | SpawnAtSocket | root | 1.2 | — |
| `vfx_transform_light` | SpawnAtSocket | root | 1.2 | — |
| `vfx_ult_charge` | SpawnAtSocket | weapon_tip | 1.2 | — |
| `vfx_ult_gather` | SpawnAtSocket | chest | 1.2 | — |

## AbilityImpact  (82)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_adamantoise_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_ahriman_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_all_jobs` | SpawnAtSocket | root | 1.0 | — |
| `vfx_bartz_hit_1` | SpawnAtSocket | root | 1.0 | — |
| `vfx_bartz_hit_2` | SpawnAtSocket | root | 1.0 | — |
| `vfx_bartz_hit_3` | SpawnAtSocket | root | 1.0 | — |
| `vfx_behemoth_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_bomb_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_branch_erupt` | SpawnAtSocket | root | 1.0 | — |
| `vfx_branch_line` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_cecil_hit_1` | SpawnAtSocket | root | 1.0 | — |
| `vfx_cecil_hit_2` | SpawnAtSocket | root | 1.0 | — |
| `vfx_cecil_hit_3` | SpawnAtSocket | root | 1.0 | — |
| `vfx_chain_hook` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_chain_lightning` | SpawnAtSocket | root | 1.0 | — |
| `vfx_chain_throw` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_chaos_erupt` | SpawnAtSocket | root | 1.0 | — |
| `vfx_cloud_of_darkness_hit_1` | SpawnAtSocket | root | 1.0 | — |
| `vfx_cloud_of_darkness_hit_2` | SpawnAtSocket | root | 1.0 | — |
| `vfx_coeurl_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_crash_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_cyclone` | SpawnAtSocket | wings | 1.0 | — |
| `vfx_dark_cleave` | SpawnAtSocket | weapon_edge | 1.0 | — |
| `vfx_dash_slash` | SpawnAtSocket | weapon_edge | 1.0 | — |
| `vfx_dash_start` | SpawnAtSocket | feet | 1.0 | — |
| `vfx_element_burst` | SpawnAtSocket | weapon_edge | 1.0 | — |
| `vfx_emperor_hit_1` | SpawnAtSocket | root | 1.0 | — |
| `vfx_emperor_hit_2` | SpawnAtSocket | root | 1.0 | — |
| `vfx_execute_slash` | SpawnAtSocket | weapon_edge | 1.0 | — |
| `vfx_fire_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_firion_hit_1` | SpawnAtSocket | root | 1.0 | — |
| `vfx_firion_hit_2` | SpawnAtSocket | root | 1.0 | — |
| `vfx_flan_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_floating_eye_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_frost_burst` | SpawnAtSocket | root | 1.0 | — |
| `vfx_garland_hit_1` | SpawnAtSocket | feet | 1.0 | — |
| `vfx_garland_hit_2` | SpawnAtSocket | feet | 1.0 | — |
| `vfx_goblin_armored_hit` | SpawnAtSocket | weapon | 1.0 | — |
| `vfx_goblin_mage_hit` | SpawnAtSocket | weapon | 1.0 | — |
| `vfx_goblin_melee_hit` | SpawnAtSocket | weapon | 1.0 | — |
| `vfx_goblin_ranged_hit` | SpawnAtSocket | weapon | 1.0 | — |
| `vfx_goblin_warlord_hit` | SpawnAtSocket | weapon | 1.0 | — |
| `vfx_gust_displace` | SpawnAtSocket | root | 1.0 | — |
| `vfx_holy_slash` | SpawnAtSocket | weapon_edge | 1.0 | — |
| `vfx_holy_tether` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_iron_giant_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_jump_dust` | SpawnAtSocket | feet | 1.0 | — |
| `vfx_kain_hit_1` | SpawnAtSocket | root | 1.0 | — |
| `vfx_kain_hit_2` | SpawnAtSocket | root | 1.0 | — |
| `vfx_lance_line` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_lance_thrust` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_land_strike` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_leap` | SpawnAtSocket | feet | 1.0 | — |
| `vfx_lifesteal` | SpawnAtSocket | chest | 1.0 | — |
| `vfx_light_barrier` | SpawnAtSocket | shield | 1.0 | — |
| `vfx_light_gauge` | SpawnAtSocket | chest | 1.0 | — |
| `vfx_light_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_malboro_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_mu_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_onion_knight_hit_1` | SpawnAtSocket | root | 1.0 | — |
| `vfx_onion_knight_hit_2` | SpawnAtSocket | root | 1.0 | — |
| `vfx_onion_knight_hit_3` | SpawnAtSocket | root | 1.0 | — |
| `vfx_pierce_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_pierce_line` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_portal_open_a` | SpawnAtSocket | hand_r | 1.0 | — |
| `vfx_portal_open_b` | SpawnAtSocket | root | 1.0 | — |
| `vfx_pushback` | SpawnAtSocket | root | 1.0 | — |
| `vfx_rhythm_glint` | SpawnAtSocket | hand_r | 1.0 | — |
| `vfx_sahagin_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_shoulder_impact` | SpawnAtSocket | chest | 1.0 | — |
| `vfx_size_grow` | SpawnAtSocket | root | 1.0 | — |
| `vfx_sweetspot_flash` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_sword_slash` | SpawnAtSocket | weapon_edge | 1.0 | — |
| `vfx_thrust_pierce` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_thunder_strike` | SpawnAtSocket | root | 1.0 | — |
| `vfx_tonberry_impact` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_trample` | SpawnAtSocket | root | 1.0 | — |
| `vfx_water_surge` | SpawnAtSocket | head | 1.0 | — |
| `vfx_weapon_storm` | SpawnAtSocket | weapon_tip | 1.0 | — |
| `vfx_wol_hit_1` | SpawnAtSocket | root | 1.0 | — |
| `vfx_wol_hit_2` | SpawnAtSocket | root | 1.0 | — |
| `vfx_wol_hit_3` | SpawnAtSocket | root | 1.0 | — |

## WeaponTrail  (12)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_blade_streak` | AttachToSocket | weapon_tip | 0.5 | 🎨 follow |
| `vfx_charge_trail` | AttachToSocket | feet | 0.5 | 🎨 follow |
| `vfx_dash_streak` | AttachToSocket | weapon_tip | 0.5 | 🎨 follow |
| `vfx_dive_trail` | AttachToSocket | weapon_tip | 0.5 | 🎨 follow |
| `vfx_edge_trail` | AttachToSocket | weapon_tip | 0.5 | 🎨 follow |
| `vfx_element_arc` | AttachToSocket | weapon_tip | 0.5 | 🎨 follow |
| `vfx_heavy_arc` | AttachToSocket | weapon_tip | 0.5 | 🎨 follow |
| `vfx_hop_arc` | AttachToSocket | weapon_tip | 0.5 | 🎨 follow |
| `vfx_lifesteal_arc` | AttachToSocket | weapon_tip | 0.5 | 🎨 follow |
| `vfx_light_slash_trail` | AttachToSocket | weapon_tip | 0.5 | 🎨 follow |
| `vfx_radiant_arc` | AttachToSocket | weapon_tip | 0.5 | 🎨 follow |
| `vfx_reap_arc` | AttachToSocket | weapon_edge | 0.5 | 🎨 follow |

## Projectile  (11)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_arrow_loose` | ProjectileTravel | weapon_tip | 2.0 | ⚠ 🎨 |
| `vfx_arrow_snap` | ProjectileTravel | weapon_tip | 2.0 | ⚠ 🎨 |
| `vfx_beam_fire` | ProjectileTravel | chest | 2.0 | ⚠ 🎨 |
| `vfx_bolt_1` | ProjectileTravel | hand_l | 2.0 | ⚠ 🎨 |
| `vfx_bolt_2` | ProjectileTravel | hand_r | 2.0 | ⚠ 🎨 |
| `vfx_bolt_mark` | ProjectileTravel | staff_tip | 2.0 | ⚠ 🎨 |
| `vfx_divine_bolt` | ProjectileTravel | hands | 2.0 | ⚠ 🎨 |
| `vfx_finishing_shot` | ProjectileTravel | weapon_tip | 2.0 | ⚠ 🎨 |
| `vfx_gungnir_line` | ProjectileTravel | spear_tip | 2.0 | ⚠ 🎨 |
| `vfx_spear_throw` | ProjectileTravel | weapon_tip | 2.0 | ⚠ 🎨 |
| `vfx_void_beam` | ProjectileTravel | weapon_tip | 2.0 | ⚠ 🎨 |

## GroundZone  (20)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_arrow_rain` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_bulwark_field` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_cross_gather` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_domain_expand` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_fire_cone` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_flood_advance` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_grand_cross` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_gravity_pull` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_ground_crack` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_ground_lines` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_ice_lanes` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_life_drain` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_light_wave` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_pull_wisps` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_repulse_cone` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_rock_fall` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_sigil_place` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_slow_field` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_snare_plant` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |
| `vfx_void_zone` | SpawnAtGroundPoint | ground | 3.0 | ⚠ |

## Aura  (7)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_chain_glow` | AttachToSocket | weapon_tip | ∞ | 🎨 follow |
| `vfx_dark_aura` | AttachToSocket | root | ∞ | 🎨 follow |
| `vfx_dual_aura` | AttachToSocket | root | ∞ | 🎨 follow |
| `vfx_job_glow` | AttachToSocket | hand_r | ∞ | 🎨 follow |
| `vfx_root_energy` | AttachToSocket | feet | ∞ | 🎨 follow |
| `vfx_strife_embers` | AttachToSocket | chest | ∞ | 🎨 follow |
| `vfx_unstoppable_aura` | AttachToSocket | root | ∞ | 🎨 follow |

## Buff  (1)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_buff_up` | AttachToSocket | root | 1.2 | 🎨 follow |

## Heal  (1)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_heal_glow` | AttachToSocket | chest | 1.2 | follow |

## Shield  (5)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_cover_shield` | AttachToSocket | chest | 6.0 | 🎨 follow |
| `vfx_resolve_shield_glow` | AttachToSocket | shield | 6.0 | 🎨 follow |
| `vfx_shield_bash` | AttachToSocket | shield | 6.0 | 🎨 follow |
| `vfx_shield_break` | AttachToSocket | shield | 6.0 | 🎨 follow |
| `vfx_shield_on` | AttachToSocket | shield | 6.0 | 🎨 follow |

## Cleanse  (1)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_cleanse_burst` | SpawnAtRoot | feet | 1.0 | — |

## Reaction  (7)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_impact_dust` | AttachToSocket | weapon_tip | 1.0 | follow |
| `vfx_knockup_circles` | AttachToSocket | root | 1.0 | follow |
| `vfx_launch` | AttachToSocket | feet | 1.0 | follow |
| `vfx_root_bind` | AttachToSocket | root | 1.0 | follow |
| `vfx_silence_seal` | AttachToSocket | root | 1.0 | follow |
| `vfx_slow_drag` | AttachToSocket | root | 1.0 | follow |
| `vfx_stun_stars` | AttachToSocket | root | 1.0 | follow |

## Death  (29)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_adamantoise_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_ahriman_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_behemoth_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_bomb_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_coeurl_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_dark_dissolve` | SpawnAtRoot | root | 2.5 | — |
| `vfx_death_dissolve` | SpawnAtRoot | root | 2.5 | — |
| `vfx_dissolve_in` | SpawnAtRoot | root | 2.5 | — |
| `vfx_dissolve_out` | SpawnAtRoot | root | 2.5 | — |
| `vfx_divine_ascend` | SpawnAtRoot | root | 2.5 | — |
| `vfx_exdeath_hit_1` | SpawnAtRoot | root | 2.5 | — |
| `vfx_exdeath_hit_2` | SpawnAtRoot | root | 2.5 | — |
| `vfx_feather_dissolve` | SpawnAtRoot | root | 2.5 | — |
| `vfx_fire_dissolve` | SpawnAtRoot | root | 2.5 | — |
| `vfx_flan_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_floating_eye_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_goblin_poof` | SpawnAtRoot | root | 2.5 | — |
| `vfx_ice_shatter_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_iron_giant_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_light_dissolve` | SpawnAtRoot | root | 2.5 | — |
| `vfx_lightning_dissolve` | SpawnAtRoot | root | 2.5 | — |
| `vfx_malboro_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_mu_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_odin_dissolve` | SpawnAtRoot | root | 2.5 | — |
| `vfx_sahagin_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_stone_crumble` | SpawnAtRoot | root | 2.5 | — |
| `vfx_tonberry_death` | SpawnAtRoot | root | 2.5 | — |
| `vfx_void_dissolve` | SpawnAtRoot | root | 2.5 | — |
| `vfx_water_dissolve` | SpawnAtRoot | root | 2.5 | — |

## Respawn  (1)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_respawn_materialize` | SpawnAtRoot | root | 3.0 | — |

## Objective  (12)

| Key | Spawn rule | Socket | Life | Flags |
|---|---|---|---|---|
| `vfx_chaos_blessing` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_cosmos_blessing` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_earthen_favor` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_edge_grant` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_glacial_favor` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_infernal_favor` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_judgment_favor` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_materia_blessing` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_spiritus_blessing` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_tempest_favor` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_tidal_favor` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |
| `vfx_zantetsuken_edge` | SpawnAtRoot | root | 3.5 | ⚠ 🎨 |

---

# SFX


## SfxVoice  (7)

| Key | Priority | 3D |
|---|---|---|
| `sfx_dark_whisper` | 7 | yes |
| `sfx_ifrit_roar` | 7 | yes |
| `sfx_leviathan_roar` | 7 | yes |
| `sfx_shiva_chant` | 7 | yes |
| `sfx_titan_bellow` | 7 | yes |
| `sfx_transform_growl` | 7 | yes |
| `sfx_wind_screech` | 7 | yes |

## SfxImpact  (66)

| Key | Priority | 3D |
|---|---|---|
| `sfx_adamantoise_hit` | 6 | yes |
| `sfx_adamantoise_tell` | 5 | yes |
| `sfx_ahriman_hit` | 6 | yes |
| `sfx_ahriman_tell` | 5 | yes |
| `sfx_arsenal_rise` | 5 | yes |
| `sfx_behemoth_hit` | 6 | yes |
| `sfx_behemoth_tell` | 5 | yes |
| `sfx_bomb_hit` | 6 | yes |
| `sfx_bomb_tell` | 5 | yes |
| `sfx_bulwark_slam` | 5 | yes |
| `sfx_chaos_boom` | 6 | yes |
| `sfx_coeurl_hit` | 6 | yes |
| `sfx_coeurl_tell` | 5 | yes |
| `sfx_crash` | 6 | yes |
| `sfx_cyclone` | 5 | yes |
| `sfx_domain` | 8 | yes |
| `sfx_dual_aura` | 5 | yes |
| `sfx_fire_blast` | 6 | yes |
| `sfx_flan_hit` | 6 | yes |
| `sfx_flan_tell` | 5 | yes |
| `sfx_floating_eye_hit` | 6 | yes |
| `sfx_floating_eye_tell` | 5 | yes |
| `sfx_force_blast` | 6 | yes |
| `sfx_goblin_armored` | 5 | yes |
| `sfx_goblin_mage` | 5 | yes |
| `sfx_goblin_melee` | 5 | yes |
| `sfx_goblin_ranged` | 5 | yes |
| `sfx_goblin_warlord` | 5 | yes |
| `sfx_grand_cross` | 8 | yes |
| `sfx_gungnir` | 5 | yes |
| `sfx_heavy_hit` | 6 | yes |
| `sfx_heavy_slash` | 6 | yes |
| `sfx_ice_shatter` | 6 | yes |
| `sfx_iron_giant_hit` | 6 | yes |
| `sfx_iron_giant_tell` | 5 | yes |
| `sfx_job_parade` | 8 | yes |
| `sfx_knockback` | 6 | yes |
| `sfx_lance_thrust` | 5 | yes |
| `sfx_land_hit` | 6 | yes |
| `sfx_launch` | 5 | yes |
| `sfx_leap` | 5 | yes |
| `sfx_light_hit` | 6 | yes |
| `sfx_light_wave` | 5 | yes |
| `sfx_malboro_hit` | 6 | yes |
| `sfx_malboro_tell` | 5 | yes |
| `sfx_mu_hit` | 6 | yes |
| `sfx_mu_tell` | 5 | yes |
| `sfx_phase` | 5 | yes |
| `sfx_quake` | 6 | yes |
| `sfx_quick_slash` | 6 | yes |
| `sfx_roots_erupt` | 5 | yes |
| `sfx_sahagin_hit` | 6 | yes |
| `sfx_sahagin_tell` | 5 | yes |
| `sfx_shield_bash` | 6 | yes |
| `sfx_spellblade` | 5 | yes |
| `sfx_sword_slash` | 6 | yes |
| `sfx_thunder` | 6 | yes |
| `sfx_thunder_roll` | 6 | yes |
| `sfx_thunder_strike` | 6 | yes |
| `sfx_tonberry_hit` | 6 | yes |
| `sfx_tonberry_tell` | 5 | yes |
| `sfx_transform` | 8 | yes |
| `sfx_ult_gather` | 8 | yes |
| `sfx_ult_power` | 8 | yes |
| `sfx_void_detonate` | 6 | yes |
| `sfx_zan_slash` | 6 | yes |

## SfxCast  (16)

| Key | Priority | 3D |
|---|---|---|
| `sfx_armor_brace` | 5 | yes |
| `sfx_cleanse` | 5 | yes |
| `sfx_dark_cast` | 5 | yes |
| `sfx_divine` | 5 | yes |
| `sfx_field_cast` | 5 | yes |
| `sfx_gravity` | 5 | yes |
| `sfx_guard_up` | 5 | yes |
| `sfx_heal_pulse` | 5 | yes |
| `sfx_magic_cast` | 5 | yes |
| `sfx_portal` | 5 | yes |
| `sfx_shield_raise` | 5 | yes |
| `sfx_sigil_cast` | 5 | yes |
| `sfx_stance_swap` | 5 | yes |
| `sfx_tidal` | 5 | yes |
| `sfx_void_beam` | 5 | yes |
| `sfx_void_wash` | 5 | yes |

## SfxWhoosh  (31)

| Key | Priority | 3D |
|---|---|---|
| `sfx_bow_release` | 4 | yes |
| `sfx_bow_snap` | 4 | yes |
| `sfx_bow_swing_1` | 4 | yes |
| `sfx_bow_swing_2` | 4 | yes |
| `sfx_chain_reel` | 4 | yes |
| `sfx_chain_throw` | 4 | yes |
| `sfx_charge` | 4 | yes |
| `sfx_dash` | 4 | yes |
| `sfx_dualblade_swing_1` | 4 | yes |
| `sfx_dualblade_swing_2` | 4 | yes |
| `sfx_dualblade_swing_3` | 4 | yes |
| `sfx_final_loose` | 4 | yes |
| `sfx_greatsword_swing` | 4 | yes |
| `sfx_greatsword_swing_1` | 4 | yes |
| `sfx_greatsword_swing_2` | 4 | yes |
| `sfx_hooves` | 4 | yes |
| `sfx_jump` | 4 | yes |
| `sfx_lance_swing_1` | 4 | yes |
| `sfx_lance_swing_2` | 4 | yes |
| `sfx_odin_charge` | 4 | yes |
| `sfx_spear_throw` | 4 | yes |
| `sfx_staff_swing_1` | 4 | yes |
| `sfx_staff_swing_2` | 4 | yes |
| `sfx_sword_swing_1` | 4 | yes |
| `sfx_sword_swing_2` | 4 | yes |
| `sfx_sword_swing_3` | 4 | yes |
| `sfx_sword_woosh` | 4 | yes |
| `sfx_tentacle_swing_1` | 4 | yes |
| `sfx_tentacle_swing_2` | 4 | yes |
| `sfx_ult_charge` | 4 | yes |
| `sfx_zan_charge` | 4 | yes |