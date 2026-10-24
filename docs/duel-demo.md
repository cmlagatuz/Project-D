# 1v1 duel demo (summoner spell test bed)

> CONFIDENTIAL. All Final Fantasy / Dissidia names are placeholders and every shape on screen is original
> placeholder art. Licensed assets need a documented chain of title, or must be replaced with original IP,
> before any public build or test.

A small arena with two champions, a training golem and an ally ward. It exists to answer one question:
**do the 11 summoner spells feel right to use?** The rules live in `Assets/Scripts/Duel/` (engine-agnostic,
deterministic, unit-tested); the Unity view and touch HUD live in `Assets/Scripts/DuelDemo/`.

> **Status: written but not yet compiled or run.** No Unity editor was available when this was written. Expect one
> to three rounds of compile fixes. Paste the Console errors and they get fixed by root cause.

## Run it in the Unity Editor (start here)

1. `git pull`, then let Unity import and compile. If the Console shows errors, send them over before going further.
2. Menu **Tools > Project D > Build Duel Demo Scene**. This creates `Assets/Scenes/DuelDemo.unity` and the placeholder
   materials under `Assets/Materials/DuelDemo/`.
3. Press **Play**.

| Input | Editor | Phone |
|---|---|---|
| Move | WASD / arrows | left thumb, floating stick |
| Basic attack | hold Space | hold ATTACK |
| Q W E R | keys 1 2 3 4 (aimed at the mouse) | tap = auto-aim, drag = aim |
| Summoner D / F | keys 5 6, or Z X | the two round buttons under the arc |
| Pick spells | SPELLS button | SPELLS button |
| Test helpers | TOOLS button | TOOLS button |

## What you can test

- **SPELLS** opens a picker with all 11 spells. Choose slot D or F, then tap a spell. The duel pauses while it is open.
  Changing a spell restarts its cooldown. Choosing the spell already in the other slot swaps the two.
- **TOOLS** has: reset cooldowns, heal me, hurt me 50% (for Cure), stun me 2 s and slow me 3 s (for Esuna and Warp),
  reset enemy, restore golem, unlock champion Ultima, level +/-1, and the enemy bot ON/OFF (OFF makes it a dummy).
- Status tags show above each unit panel as text (`[STUN]`, `[SLOW -35%]`, `[SHIELD 120]`, `[BIO]`, `[WOUNDS]`,
  `[IMMUNE]`, `[HASTE 4.2s]`, `[TELEPORT 2.1s]`, `[MARK 3.0s]`), so no state depends on colour alone.

| Spell | How to see it work |
|---|---|
| Haste | Cast, then run: speed starts high and eases off. Kill the enemy while it is active to extend it. |
| Cure | TOOLS > Hurt me 50%, then cast. A second Cure within the recently-healed window heals half. |
| Protect | Cast, let the bot hit you: the `[SHIELD]` tag drains and hits read "Blocked". |
| Slow | Cast within range of the enemy: it slows and its damage drops. |
| Esuna | TOOLS > Stun me, then cast: the stun clears and `[IMMUNE]` appears. |
| Warp | Cast while holding the stick to blink that way. Stunned = refused. |
| Bio | Cast near the enemy: damage ticks every second and the enemy's healing is reduced. |
| Ultima | Cast near the golem (monster). Two charges. TOOLS > Unlock champion Ultima lets it hit the enemy too. |
| Teleport | Stand still for the channel to the ward at the far corner. Moving, attacking or Warp cancels it. |
| Pray | Spend mana on abilities first, then cast to see the restore. |
| Warpstrike | Cast toward the enemy to mark it; press again (button shows AGAIN) to dash to it. |

## Put it on a phone

**Android (works today, if the editor compiles).**

1. Unity Hub > Installs > your editor (6000.5.10f1) > Add modules > **Android Build Support**, with **OpenJDK** and
   **Android SDK & NDK Tools** ticked.
2. On the phone: Settings > About phone > tap *Build number* 7 times, then Developer options > **USB debugging**.
3. Connect by USB and accept the prompt on the phone.
4. Menu **Tools > Project D > Build and Run Duel Demo on Phone**. It switches to Android, uses IL2CPP + ARM64,
   landscape, builds `Builds/ProjectD_DuelDemo.apk` and installs it.
   Or use **Build Duel Demo APK (Android)**, copy the file to the phone and open it (allow "install unknown apps").

The first Android build is slow (several minutes: Gradle and IL2CPP). Later builds are faster.

**iOS** needs a Mac with Xcode and an Apple developer account. Not set up here.

**Browser / WebGL** is not recommended for a touch game on phones: heavy download, weak mobile support.

**Optional: build the APK in GitHub Actions** instead of on your PC. See `docs/ci/android-demo.yml.example`
(GameCI; needs a Unity licence stored as a repository secret). It is deliberately not enabled so pushes do not fail
without secrets.

## Known limits of the demo

- One map-less arena (28 x 28), no minions, towers, jungle, shop, XP or fog of war.
- Two placeholder champions with shortened cooldowns; the enemy is a scripted bot, not the production bot brain.
- Teleport only targets the ward (structures and minions do not exist yet), so the "structures only for the first
  6 minutes" rule is in the rules code but not exercised.
- Placeholder numbers still to confirm are listed in `docs/summoner-spells.md`.
- Audio, character models and animation are not used by this scene.

## Roadmap: the rest of the game (rough order)

Each row is one focused piece of work. "Sessions" are rough, assume the previous row compiles and plays.

| # | Feature | What it adds | Rough size |
|---|---|---|---|
| 1 | Make the demo compile and feel right | Fix compile errors, tune spells, real champion kits | 1-3 sessions |
| 2 | Lane + minions + gold/XP + last hit | The core "MOBA" loop, shared simulation | 3-4 |
| 3 | Towers, inhibitors, core, win condition | A match that can actually end | 2-3 |
| 4 | Jungle camps + Ultima/Smite for real | Hunter's Resource stacks from camps, buffs, objectives | 2 |
| 5 | Shop + items + recall | The existing item/store systems wired into a live match | 2-3 |
| 6 | Fog of war + vision | Wards, brush, reveal effects (Bio reveals) | 2 |
| 7 | Bots for 5v5 | Reuse the BotAI layer on the new simulation | 2-3 |
| 8 | Champion roster | Real kits for the Dissidia-style cast, one per session or less | ongoing |
| 9 | Models, animation, VFX, audio | Replace primitives with original art and the existing import pipelines | ongoing |
| 10 | Online play | The authoritative server + two-client harness driving the same simulation | 4+ |

Rows 2 to 4 are what turns this from a spell test into a Wild Rift-style match; everything after builds on them.
