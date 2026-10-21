# Project D — Neutral Animation: Summons · Monsters · Minions

> Codename **Zantetsuken** · CONFIDENTIAL · Source: `Assets/GameData/Animations/neutrals.seed.json`

Every objective and creature: spawn, idle/move, basic attack(s), the **signature mechanic** that defines the fight, hit/death, optional low-HP enrage, and — for objectives — the **reward-grant flourish** when the killing team claims the blessing. Signatures are **server-triggered**; keep every wind-up readable so players can dodge.


---

# Elemental Summons


## Ifrit  (`ifrit`)

**States:** spawn `ifrit_spawn` · idle `ifrit_idle` · move `ifrit_move` · attacks `ifrit_attack_1` (hit 0.34s) · hit `ifrit_hit_react` · death `ifrit_death` + `vfx_fire_dissolve`
**Reward flourish:** `ifrit_reward_grant` + `vfx_infernal_favor` (granted to the killing team).

### Signature A — Hellfire Cone  ·  clip `ifrit_hellfire`
- Timing: 0.45 / 0.3 / 0.45s · targeting Directional
- VFX: `vfx_fire_cone`@0.45s, `vfx_fire_impact`@0.55s · SFX: `sfx_ifrit_roar`@0.2s, `sfx_fire_blast`@0.45s
- Rear back, inhale (0.45), then breathe a wide fire cone; scorched-ground residue during active. Heavy stomp recovery.

### Signature B — Meteor Punch  ·  clip `ifrit_punch`
- Timing: 0.35 / 0.2 / 0.4s · targeting Directional · RootMotion ~4.0m
- VFX: `vfx_leap`@0.35s, `vfx_ground_crack`@0.55s · SFX: —
- Leaping fist-slam that cracks the ground; expanding fire rings on impact.

*Fire summon. Big, aggressive body language. On death, kneel and shatter into embers while the Infernal Favor blessing streams to the killing team.*


## Shiva  (`shiva`)

**States:** spawn `shiva_spawn` · idle `shiva_idle` · move `shiva_move` · attacks `shiva_attack_1` (hit 0.3s) · hit `shiva_hit_react` · death `shiva_death` + `vfx_ice_shatter_death`
**Reward flourish:** `shiva_reward_grant` + `vfx_glacial_favor` (granted to the killing team).

### Signature A — Diamond Dust  ·  clip `shiva_diamond_dust`
- Timing: 0.4 / 0.35 / 0.4s · targeting Point
- VFX: `vfx_ice_lanes`@0.4s, `vfx_frost_burst`@0.6s · SFX: `sfx_shiva_chant`@0.15s, `sfx_ice_shatter`@0.4s
- Graceful arm sweep spawns delayed ice crystals in lanes, then a radial frost burst. Elegant, unhurried.

*Ice summon. Serene, dancer-like motion; brief slow applied by ability hits.*


## Ramuh  (`ramuh`)

**States:** spawn `ramuh_spawn` · idle `ramuh_idle` · move `ramuh_move` · attacks `ramuh_attack_1` (hit 0.32s) · hit `ramuh_hit_react` · death `ramuh_death` + `vfx_lightning_dissolve`
**Reward flourish:** `ramuh_reward_grant` + `vfx_judgment_favor` (granted to the killing team).

### Signature A — Judgment Bolt  ·  clip `ramuh_judgment`
- Timing: 0.42 / 0.25 / 0.4s · targeting Point
- VFX: `vfx_bolt_mark`@0.42s, `vfx_chain_lightning`@0.62s · SFX: `sfx_thunder_roll`@0.2s, `sfx_thunder_strike`@0.62s
- Raise the staff; a telegraphed bolt marks the ground then chains between nearby targets. Patriarchal, deliberate.

*Lightning summon.*


## Garuda  (`garuda`)

**States:** spawn `garuda_spawn` · idle `garuda_idle` · move `garuda_move` · attacks `garuda_attack_1` (hit 0.28s) · hit `garuda_hit_react` · death `garuda_death` + `vfx_feather_dissolve`
**Reward flourish:** `garuda_reward_grant` + `vfx_tempest_favor` (granted to the killing team).

### Signature A — Aerial Blast  ·  clip `garuda_aerial_blast`
- Timing: 0.35 / 0.3 / 0.35s · targeting Directional
- VFX: `vfx_cyclone`@0.35s, `vfx_gust_displace`@0.55s · SFX: `sfx_wind_screech`@0.15s, `sfx_cyclone`@0.35s
- Beat the wings to spawn cyclones and displacing gusts; rotating safe sectors. Fast, erratic, avian.

*Wind summon; grants move+attack speed.*


## Titan  (`titan`)

**States:** spawn `titan_spawn` · idle `titan_idle` · move `titan_move` · attacks `titan_attack_1` (hit 0.45s) · hit `titan_hit_react` · death `titan_death` + `vfx_stone_crumble`
**Reward flourish:** `titan_reward_grant` + `vfx_earthen_favor` (granted to the killing team).

### Signature A — Gaia's Wrath  ·  clip `titan_gaias_wrath`
- Timing: 0.5 / 0.3 / 0.5s · targeting Directional
- VFX: `vfx_ground_lines`@0.5s, `vfx_rock_fall`@0.7s · SFX: `sfx_titan_bellow`@0.25s, `sfx_quake`@0.5s
- Massive double-fist ground pound sends cracking lines forward; falling-rock markers then a frontal slam. Immense weight; slow everything.

*Earth summon; defensive blessing.*


## Leviathan  (`leviathan`)

**States:** spawn `leviathan_spawn` · idle `leviathan_idle` · move `leviathan_move` · attacks `leviathan_attack_1` (hit 0.36s) · hit `leviathan_hit_react` · death `leviathan_death` + `vfx_water_dissolve`
**Reward flourish:** `leviathan_reward_grant` + `vfx_tidal_favor` (granted to the killing team).

### Signature A — Tidal Wave  ·  clip `leviathan_tidal_wave`
- Timing: 0.4 / 0.35 / 0.42s · targeting Directional
- VFX: `vfx_water_surge`@0.4s, `vfx_knockup_circles`@0.62s · SFX: `sfx_leviathan_roar`@0.18s, `sfx_tidal`@0.4s
- Serpentine rear-and-crash; a tidal sweep with water lanes and knock-up circles. Sinuous, flowing spine motion.

*Water summon; regen + resource blessing near river.*


---

# Elder Summon (Odin)


## Odin  (`odin`)

**States:** spawn `odin_spawn` · idle `odin_idle` · move `odin_move` · attacks `odin_attack_1` (hit 0.5s) · hit `odin_hit_react` · death `odin_death` + `vfx_odin_dissolve` · enrage `odin_enrage`
**Reward flourish:** `odin_reward_grant` + `vfx_zantetsuken_edge` (granted to the killing team).

### Signature A — Gungnir  ·  clip `odin_gungnir`
- Timing: 0.5 / 0.25 / 0.45s · targeting Directional
- VFX: `vfx_gungnir_line`@0.5s, `vfx_pierce_impact`@0.75s · SFX: `sfx_odin_charge`@0.25s, `sfx_gungnir`@0.5s
- Rear the mount, level the spear, and hurl a long telegraphed line. Regal, terrifying poise.

### Signature B — Sleipnir Charge  ·  clip `odin_sleipnir`
- Timing: 0.4 / 0.4 / 0.4s · targeting Directional · RootMotion ~10.0m
- VFX: `vfx_charge_trail`@0.4s, `vfx_trample`@0.8s · SFX: `sfx_hooves`@0.2s, `sfx_charge`@0.4s
- Sleipnir gallops across the arena trampling all in the lane; long root-motion charge.

### Ultimate — Zantetsuken  ·  clip `odin_zantetsuken`
- Timing: 0.9 / 0.2 / 0.7s · targeting Self
- VFX: `vfx_blade_gather`@0.3s, `vfx_execute_slash`@0.9s, `vfx_edge_grant`@1.05s · SFX: `sfx_zan_charge`@0.1s, `sfx_zan_slash`@0.9s
- THE finisher. A long, dread-building unsheathe (0.90) with the whole arena darkening, then a single horizontal cut that executes low-HP heroes and slays neutral summons. The killing team receives Zantetsuken's Edge — grant flourish on recovery. Camera + hitstop moment.

*Elder objective boss. Mounted, three signatures, a low-HP enrage, and a screen-defining ultimate.*


---

# Divine Manifestations


## Materia  (`materia`)

**States:** spawn `materia_spawn` · idle `materia_idle` · move `materia_move` · attacks `materia_attack_1` (hit 0.3s) · hit `materia_hit_react` · death `materia_death` + `vfx_divine_ascend`
**Reward flourish:** `materia_reward_grant` + `vfx_materia_blessing` (granted to the killing team).

### Signature A — Materia Smite  ·  clip `materia_smite`
- Timing: 0.35 / 0.2 / 0.35s · targeting Point
- VFX: `vfx_divine_bolt`@0.35s · SFX: `sfx_divine`@0.35s
- A slow, imperious ranged smite so the manifestation can defend its pit. Low mobility by design.

*Restoration avatar. Gentle floating idle, hands clasped; on defeat, radiate a healing bloom that grants the Materia blessing (team recovery + haste). Manifestation stays near the Rift; the reward flourish is the visual payoff of the objective.*


## Cosmos  (`cosmos`)

**States:** spawn `cosmos_spawn` · idle `cosmos_idle` · move `cosmos_move` · attacks `cosmos_attack_1` (hit 0.3s) · hit `cosmos_hit_react` · death `cosmos_death` + `vfx_divine_ascend`
**Reward flourish:** `cosmos_reward_grant` + `vfx_cosmos_blessing` (granted to the killing team).

### Signature A — Cosmos Smite  ·  clip `cosmos_smite`
- Timing: 0.35 / 0.2 / 0.35s · targeting Point
- VFX: `vfx_divine_bolt`@0.35s · SFX: `sfx_divine`@0.35s
- A slow, imperious ranged smite so the manifestation can defend its pit. Low mobility by design.

*Protection avatar. Serene, regal; grants defenses + periodic shield. Warm, harmonious light on the grant flourish. Manifestation stays near the Rift; the reward flourish is the visual payoff of the objective.*


## Spiritus  (`spiritus`)

**States:** spawn `spiritus_spawn` · idle `spiritus_idle` · move `spiritus_move` · attacks `spiritus_attack_1` (hit 0.3s) · hit `spiritus_hit_react` · death `spiritus_death` + `vfx_divine_ascend`
**Reward flourish:** `spiritus_reward_grant` + `vfx_spiritus_blessing` (granted to the killing team).

### Signature A — Spiritus Smite  ·  clip `spiritus_smite`
- Timing: 0.35 / 0.2 / 0.35s · targeting Point
- VFX: `vfx_divine_bolt`@0.35s · SFX: `sfx_divine`@0.35s
- A slow, imperious ranged smite so the manifestation can defend its pit. Low mobility by design.

*Arcane-aggression avatar. Imposing, arms crossed; grants magic power + penetration. Sharp violet burst on grant. Manifestation stays near the Rift; the reward flourish is the visual payoff of the objective.*


## Chaos  (`chaos`)

**States:** spawn `chaos_spawn` · idle `chaos_idle` · move `chaos_move` · attacks `chaos_attack_1` (hit 0.3s) · hit `chaos_hit_react` · death `chaos_death` + `vfx_divine_ascend`
**Reward flourish:** `chaos_reward_grant` + `vfx_chaos_blessing` (granted to the killing team).

### Signature A — Chaos Smite  ·  clip `chaos_smite`
- Timing: 0.35 / 0.2 / 0.35s · targeting Point
- VFX: `vfx_divine_bolt`@0.35s · SFX: `sfx_divine`@0.35s
- A slow, imperious ranged smite so the manifestation can defend its pit. Low mobility by design.

*Martial-execution avatar. Menacing, hunched; grants attack damage + penetration. Fiery red eruption on grant. Manifestation stays near the Rift; the reward flourish is the visual payoff of the objective.*


---

# Small Monsters


## Bomb  (`bomb`)

**States:** spawn `bomb_spawn` · idle `bomb_idle` · move `bomb_move` · attacks `bomb_attack_1` (hit 0.22s) · hit `bomb_hit_react` · death `bomb_death` + `vfx_bomb_death`

### Signature A — Self-Destruct  ·  clip `bomb_boom`
- Timing: 0.6 / 0.15 / 0.1s · targeting Self
- VFX: `vfx_bomb_windup`@0.6s, `vfx_bomb_impact`@0.75s · SFX: `sfx_bomb_tell`@0.24s, `sfx_bomb_hit`@0.6s
- Swell larger per Heat stack; at max, a telegraphed countdown then explode. INTERRUPTIBLE/avoidable — the swell is the tell.

*MonsterSmall jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


## Flan  (`flan`)

**States:** spawn `flan_spawn` · idle `flan_idle` · move `flan_move` · attacks `flan_attack_1` (hit 0.28s) · hit `flan_hit_react` · death `flan_death` + `vfx_flan_death`

### Signature A — Slow Spit  ·  clip `flan_spit`
- Timing: 0.3 / 0.12 / 0.24s · targeting Directional
- VFX: `vfx_flan_windup`@0.3s, `vfx_flan_impact`@0.42s · SFX: `sfx_flan_tell`@0.12s, `sfx_flan_hit`@0.3s
- Gelatinous lob that briefly slows; wobbly, jiggly body sim on the additive layer.

*MonsterSmall jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


## Mu  (`mu`)

**States:** spawn `mu_spawn` · idle `mu_idle` · move `mu_move` · attacks `mu_attack_1` (hit 0.16s) · hit `mu_hit_react` · death `mu_death` + `vfx_mu_death`

### Signature A — Swarm Bite  ·  clip `mu_bite`
- Timing: 0.14 / 0.1 / 0.16s · targeting Unit
- VFX: `vfx_mu_windup`@0.14s, `vfx_mu_impact`@0.24s · SFX: `sfx_mu_tell`@0.056s, `sfx_mu_hit`@0.14s
- Quick darting bite; the pack tries to surround, then briefly flees at low HP before re-engaging.

*MonsterSmall jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


## Floating Eye  (`floating_eye`)

**States:** spawn `floating_eye_spawn` · idle `floating_eye_idle` · move `floating_eye_move` · attacks `floating_eye_attack_1` (hit 0.2s) · hit `floating_eye_hit_react` · death `floating_eye_death` + `vfx_floating_eye_death`

### Signature A — Reveal Pulse  ·  clip `eye_pulse`
- Timing: 0.3 / 0.2 / 0.24s · targeting Self
- VFX: `vfx_floating_eye_windup`@0.3s, `vfx_floating_eye_impact`@0.5s · SFX: `sfx_floating_eye_tell`@0.12s, `sfx_floating_eye_hit`@0.3s
- Hovering idle; periodic ranged magic + a reveal pulse. Kites away from melee.

*MonsterSmall jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


---

# Medium Monsters


## Tonberry  (`tonberry`)

**States:** spawn `tonberry_spawn` · idle `tonberry_idle` · move `tonberry_move` · attacks `tonberry_attack_1` (hit 0.35s) · hit `tonberry_hit_react` · death `tonberry_death` + `vfx_tonberry_death`

### Signature A — Chef's Knife  ·  clip `tonberry_knife`
- Timing: 0.55 / 0.2 / 0.4s · targeting Unit
- VFX: `vfx_tonberry_windup`@0.55s, `vfx_tonberry_impact`@0.75s · SFX: `sfx_tonberry_tell`@0.22s, `sfx_tonberry_hit`@0.55s
- Slow relentless walk; a HIGHLY telegraphed knife lunge that hits hard. Brief speed-up after a miss so it can't be perfectly kited.

*MonsterMedium jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


## Coeurl  (`coeurl`)

**States:** spawn `coeurl_spawn` · idle `coeurl_idle` · move `coeurl_move` · attacks `coeurl_attack_1` (hit 0.24s) · hit `coeurl_hit_react` · death `coeurl_death` + `vfx_coeurl_death`

### Signature A — Blaster  ·  clip `coeurl_blaster`
- Timing: 0.35 / 0.18 / 0.3s · targeting Directional
- VFX: `vfx_coeurl_windup`@0.35s, `vfx_coeurl_impact`@0.53s · SFX: `sfx_coeurl_tell`@0.14s, `sfx_coeurl_hit`@0.35s
- Dash behind the target, then a cone of lightning. Cubs apply small attack-speed slows.

*MonsterMedium jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


## Ahriman  (`ahriman`)

**States:** spawn `ahriman_spawn` · idle `ahriman_idle` · move `ahriman_move` · attacks `ahriman_attack_1` (hit 0.26s) · hit `ahriman_hit_react` · death `ahriman_death` + `vfx_ahriman_death`

### Signature A — Vision Gaze  ·  clip `ahriman_gaze`
- Timing: 0.4 / 0.25 / 0.3s · targeting Point
- VFX: `vfx_ahriman_windup`@0.4s, `vfx_ahriman_impact`@0.65s · SFX: `sfx_ahriman_tell`@0.16s, `sfx_ahriman_hit`@0.4s
- Floating; a channelled circular gaze that must be avoided + a short vision reduction. Drifts from melee.

*MonsterMedium jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


## Sahagin  (`sahagin`)

**States:** spawn `sahagin_spawn` · idle `sahagin_idle` · move `sahagin_move` · attacks `sahagin_attack_1` (hit 0.24s) · hit `sahagin_hit_react` · death `sahagin_death` + `vfx_sahagin_death`

### Signature A — Spear Thrust  ·  clip `sahagin_spear`
- Timing: 0.28 / 0.14 / 0.26s · targeting Directional
- VFX: `vfx_sahagin_windup`@0.28s, `vfx_sahagin_impact`@0.42s · SFX: `sfx_sahagin_tell`@0.112s, `sfx_sahagin_hit`@0.28s
- River guardian; a forward spear thrust (captain) with water-shot escorts. Bonus damage in the river.

*MonsterMedium jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


---

# Large Monsters


## Iron Giant  (`iron_giant`)

**States:** spawn `iron_giant_spawn` · idle `iron_giant_idle` · move `iron_giant_move` · attacks `iron_giant_attack_1` (hit 0.4s) · hit `iron_giant_hit_react` · death `iron_giant_death` + `vfx_iron_giant_death` · enrage `iron_giant_harden`

### Signature A — Ground Slam  ·  clip `iron_giant_slam`
- Timing: 0.55 / 0.25 / 0.5s · targeting Point
- VFX: `vfx_iron_giant_windup`@0.55s, `vfx_iron_giant_impact`@0.8s · SFX: `sfx_iron_giant_tell`@0.22s, `sfx_iron_giant_hit`@0.55s
- Wide frontal sword swing + a slow ground-slam AoE. Gains armor after losing health — sell a stagger-and-harden beat.

*MonsterLarge jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


## Malboro  (`malboro`)

**States:** spawn `malboro_spawn` · idle `malboro_idle` · move `malboro_move` · attacks `malboro_attack_1` (hit 0.42s) · hit `malboro_hit_react` · death `malboro_death` + `vfx_malboro_death`

### Signature A — Bad Breath  ·  clip `malboro_bad_breath`
- Timing: 0.6 / 0.3 / 0.45s · targeting Directional
- VFX: `vfx_malboro_windup`@0.6s, `vfx_malboro_impact`@0.9s · SFX: `sfx_malboro_tell`@0.24s, `sfx_malboro_hit`@0.6s
- Rear back and belch a wide cone applying reduced slow/heal-cut/atk-down; sprouts leave poison zones. Grotesque, writhing.

*MonsterLarge jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


## Behemoth  (`behemoth`)

**States:** spawn `behemoth_spawn` · idle `behemoth_idle` · move `behemoth_move` · attacks `behemoth_attack_1` (hit 0.38s) · hit `behemoth_hit_react` · death `behemoth_death` + `vfx_behemoth_death` · enrage `behemoth_enrage`

### Signature A — Meteor  ·  clip `behemoth_meteor`
- Timing: 0.7 / 0.3 / 0.5s · targeting Point
- VFX: `vfx_behemoth_windup`@0.7s, `vfx_behemoth_impact`@1.0s · SFX: `sfx_behemoth_tell`@0.28s, `sfx_behemoth_hit`@0.7s
- Charge + tail sweep; at low HP casts a meteor marker. Add an enrage clip — more aggressive, not faster.

*MonsterLarge jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


## Adamantoise  (`adamantoise`)

**States:** spawn `adamantoise_spawn` · idle `adamantoise_idle` · move `adamantoise_move` · attacks `adamantoise_attack_1` (hit 0.45s) · hit `adamantoise_hit_react` · death `adamantoise_death` + `vfx_adamantoise_death`

### Signature A — Shell Guard  ·  clip `adamantoise_shell`
- Timing: 0.5 / 0.6 / 0.5s · targeting Self
- VFX: `vfx_adamantoise_windup`@0.5s, `vfx_adamantoise_impact`@1.1s · SFX: `sfx_adamantoise_tell`@0.2s, `sfx_adamantoise_hit`@0.5s
- Withdraw into the shell (defensive phase) then a slow frontal knockback; exposes a vulnerable state right after.

*MonsterLarge jungle camp. Signature mechanic defines the fight; keep the wind-up readable (telegraph) so players can dodge.*


---

# Goblin Minions


## Melee Goblin  (`goblin_melee`)

**States:** spawn `goblin_melee_spawn` · idle `goblin_melee_idle` · move `goblin_melee_move` · attacks `goblin_melee_attack_1` (hit 0.22s) · hit `goblin_melee_hit_react` · death `goblin_melee_death` + `vfx_goblin_poof`

### Signature A — Melee Goblin Attack  ·  clip `goblin_melee_swing`
- Timing: 0.2 / 0.12 / 0.2s · targeting Unit
- VFX: `vfx_goblin_melee_hit`@0.2s · SFX: `sfx_goblin_melee`@0.2s
- Short sword-and-shield chop; front line.

*Lane minion. Keep clips SHORT and cheap; many are on-screen at once — reuse a shared rig and pool aggressively.*


## Ranged Goblin  (`goblin_ranged`)

**States:** spawn `goblin_ranged_spawn` · idle `goblin_ranged_idle` · move `goblin_ranged_move` · attacks `goblin_ranged_attack_1` (hit 0.22s) · hit `goblin_ranged_hit_react` · death `goblin_ranged_death` + `vfx_goblin_poof`

### Signature A — Ranged Goblin Attack  ·  clip `goblin_ranged_shot`
- Timing: 0.2 / 0.12 / 0.2s · targeting Unit
- VFX: `vfx_goblin_ranged_hit`@0.2s · SFX: `sfx_goblin_ranged`@0.2s
- Sling/short-bow shot; back line, low HP.

*Lane minion. Keep clips SHORT and cheap; many are on-screen at once — reuse a shared rig and pool aggressively.*


## Goblin Mage  (`goblin_mage`)

**States:** spawn `goblin_mage_spawn` · idle `goblin_mage_idle` · move `goblin_mage_move` · attacks `goblin_mage_attack_1` (hit 0.22s) · hit `goblin_mage_hit_react` · death `goblin_mage_death` + `vfx_goblin_poof`

### Signature A — Goblin Mage Attack  ·  clip `goblin_mage_cast`
- Timing: 0.2 / 0.12 / 0.2s · targeting Unit
- VFX: `vfx_goblin_mage_hit`@0.2s · SFX: `sfx_goblin_mage`@0.2s
- Slow magic bolt; periodic siege unit, extra structure damage.

*Lane minion. Keep clips SHORT and cheap; many are on-screen at once — reuse a shared rig and pool aggressively.*


## Armored Goblin  (`goblin_armored`)

**States:** spawn `goblin_armored_spawn` · idle `goblin_armored_idle` · move `goblin_armored_move` · attacks `goblin_armored_attack_1` (hit 0.22s) · hit `goblin_armored_hit_react` · death `goblin_armored_death` + `vfx_goblin_poof`

### Signature A — Armored Goblin Attack  ·  clip `goblin_armored_ram`
- Timing: 0.2 / 0.12 / 0.2s · targeting Unit
- VFX: `vfx_goblin_armored_hit`@0.2s · SFX: `sfx_goblin_armored`@0.2s
- Crystal-ram shove; late-match siege upgrade, heavy.

*Lane minion. Keep clips SHORT and cheap; many are on-screen at once — reuse a shared rig and pool aggressively.*


## Goblin Warlord  (`goblin_warlord`)

**States:** spawn `goblin_warlord_spawn` · idle `goblin_warlord_idle` · move `goblin_warlord_move` · attacks `goblin_warlord_attack_1` (hit 0.22s) · hit `goblin_warlord_hit_react` · death `goblin_warlord_death` + `vfx_goblin_poof`

### Signature A — Goblin Warlord Attack  ·  clip `goblin_warlord_cleave`
- Timing: 0.2 / 0.12 / 0.2s · targeting Point
- VFX: `vfx_goblin_warlord_hit`@0.2s · SFX: `sfx_goblin_warlord`@0.2s
- Big axe cleave AoE; super-minion after a lane seal falls.

*Lane minion. Keep clips SHORT and cheap; many are on-screen at once — reuse a shared rig and pool aggressively.*
