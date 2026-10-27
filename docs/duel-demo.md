# 1v1 duel demo (summoner spell test bed)

> CONFIDENTIAL. All Final Fantasy / Dissidia names are placeholders and every shape on screen is original
> placeholder art. Licensed assets need a documented chain of title, or must be replaced with original IP,
> before any public build or test.

A small arena with two champions, two summons (Shiva and Garuda) as neutral training targets, and an ally ward. It exists to answer one question:
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
| Pick fighters | FIGHTERS button | FIGHTERS button |
| Test helpers | TOOLS button | TOOLS button |

## Fighters and 3D models

Six fighters are playable on either side (pick them with **FIGHTERS**, then **FIGHT**; your spells and level are kept):

| Warriors of Light | Warriors of Dark |
|---|---|
| Warrior of Light (melee tank) | Garland (melee bruiser) |
| Firion (ranged marksman, Blood Blade heals) | The Emperor (ranged mage, ground-targeted sigils) |
| Cecil Harvey (melee, Redemption cleanses + shields) | Golbez (hybrid mage-bruiser, **placeholder kit**: he is not in `champions.seed.json` yet) |

Shiva and Garuda stand in the arena as neutral training targets for Ultima. They do not fight back yet.

**Using your own models (drop-in folders)**

Unity creates these folders by itself when the project loads (or run **Tools > Project D > Create Model Folders**):

```
Assets/Models/WarriorOfLight   Firion   Cecil   Garland   Emperor   Golbez   Shiva   Garuda
```

1. Copy a character's **Raw Files** contents into that character's folder, e.g. Cecil's `Model` folder
   (`Cecil Harvey.fbx` plus `0.tga` ... `29.tga`) and, if you want, his `Weapon` folder, into `Assets/Models/Cecil`.
   Shiva's `Shiva XVI.fbx` and all its `t_..._*.tga` files go into `Assets/Models/Shiva`.
   Copy them in Windows Explorer (`D:\Project D\Assets\Models\Cecil`) or drag them into the Project window.
2. Wait for Unity to import. Imports are tidied automatically for phones (textures capped at 1024 px and compressed,
   meshes compressed, cameras/lights ignored) and the materials are switched to URP Lit, with base / normal /
   emission textures matched by file name when the FBX did not link them.
   If a model looks grey or pink, run **Tools > Project D > Fix Model Materials (reimport models)**.
3. Run **Tools > Project D > Link Duel Demo Models**. It matches by folder and file name, ignores `Weapon` folders and
   animation-only files, and reports what it found and what is missing.
4. Run **Build Duel Demo Scene** and press Play. Missing models fall back to placeholder shapes.

**Textures.** The rip FBX files list a texture for each material (as `.dds` names; your files are `.tga`, matched by number).
**Tools > Project D > Apply Model Textures (from FBX)** reads those links, creates one URP Lit material per FBX material in
`Assets/Models/<unit>/Materials/` and remaps the model to them. Link Duel Demo Models, Build Duel Demo Scene and Fix Model
Materials run it automatically.

**White models.** White means a material has no texture: run **Fix Model Materials (reimport models)** once, and to see exactly
what each model got, run **Tools > Project D > Save Model Material Report** and read `Builds/ModelMaterialReport.txt`.

**Animation (procedural).** Rips come without clips, so the demo animates the skeleton itself (`DuelRigFinder` finds the
hips, spine, arms and legs, from the Humanoid avatar or, for numbered-bone skeletons, by geometry; `DuelCharacterAnimator`
poses them every frame). It does not need an Animator Controller and works on any skeleton:
- Stance and run: arms down with bent elbows and weapon-ready poses, two-bone leg IK so knees bend and feet plant, hip bob,
  torso lean and twist. Each champion has a style: Warrior of Light and Cecil (knight), Firion (rebel), Garland (brute,
  two-handed), the Emperor (sorcerer, hovers), Golbez (dark mage, hovers low, arms crossed).
- Basic attacks cycle through the style's slashes, thrusts, smashes or palm blasts. Abilities pick a clip from their shape
  (cone = wide sweep or palm blast, line = thrust or two-hand cast, circle = smash or arms-to-the-sky, shield = guard,
  dash = lunge, ground circle = sigil); the fourth ability always gets the big version.
- Summoner spells: Warp and Warpstrike lunge, Cure/Esuna/Pray pray, Protect guards, Haste/Ultima raise the arms, Slow and Bio
  cast. Teleport: kneel and pray while channelling, stretch into a beam on completion, reappear at the ward.
- Hit flinch, death (falls back, lies, shrinks away) and respawn (rises). Use TOOLS > **Kill me** to see death.
If a model cannot be rigged the console says why, and it falls back to a simple bob and lean. If you later get real clips
(for example Mixamo, retargeted through the Humanoid rig) and give the prefab an Animator Controller with `Speed`,
`TrigAttack`, `TrigQ/W/E/R`, `TrigHit`, those win and the procedural animation switches itself off for that model.
All the numbers are first guesses; tune them in `DuelCharacterAnimator.cs` (the `DuelClips` table and `DuelStyles`).

**Summons hold the pit.** Shiva, Garuda and Bahamut are the guardians of the arena (`DuelSummonGuard`): they turn to keep the
nearest champion in view, breathe and sway (Garuda hovers), rear up and roar when a champion comes within 9 units, and recoil
when hit. They do not attack yet: that needs a combat change in the duel rules, which is a separate step.

**Summon effects.** Built from code (`DuelSummonFx`, `DuelSecondaryMotion`), no art files needed. Shiva: her gown, skirt and hair
bones (`j_skirt*`, hair) stream back and ripple in an icy wind, with frost motes, wind streaks, orbiting ice shards, sparkles and a
frost ring on the ground. Garuda: the wing bones beat up and down (faster when a champion is near), with a ground cyclone, wind
spiralling up around her, gusts off the wings and drifting feathers. Bahamut: a rune ring, rising embers, sparkles, and wing beats if
his model has wing bones. The console says "cloth/wing motion on (N bones)" or why it is off. The champions' capes and skirts (the rips' `nuno` bone
chains) trail back when they run. Garuda and Bahamut are modelled with the wings folded up and back in a vertical plane, so the
wing motion opens them outward first and then beats them.

**Rigs.** The champion rips share one skeleton (`bone_2` hips, `bone_3/4` thighs, `bone_7/8` ankles, `bone_15..20` arms,
`bone_12` head); the animator finds bones by those names first, then the Humanoid avatar, then geometry. This fixed Garland
(Unity's Humanoid auto-mapping put his head on an elbow and his left arm on cloth bones), Warrior of Light (geometry could not
find his A-pose arms) and Golbez (geometry picked cloth bones as legs).

**Size on screen.** The demo camera now sits closer (zoom 0.72) and models are bigger (champions 2.5, Shiva and Garuda 5, Bahamut 7).
TOOLS > Camera closer / Camera farther adjusts the zoom and remembers it. Heights you already tuned in the library are kept.

**Bahamut (.glb).** Needs the glTFast package once: Window > Package Manager > + > **Add package by name** >
`com.unity.cloud.gltfast`. Then put `Bahamut.glb` (and its textures) in `Assets/Models/Bahamut/`, run Link Duel Demo Models
and Build Duel Demo Scene. He appears at the top of the arena only when his model is linked. The GLB is large (about 80 MB);
if the phone build is slow or too big, reduce his textures.

Weapons (e.g. Holy Fang) are separate FBX files; they are imported but not attached to a hand yet.

Each model is automatically scaled to a fixed height (champions 2.5, Shiva and Garuda 5, Bahamut 7 units) and stood on the ground.
If one faces the wrong way, use TOOLS > **Turn my model 90**, **Turn enemy model 90**, **Turn Shiva 90**, **Turn Garuda 90** or
**Turn Bahamut 90** until it faces forward. In the editor the answer is saved into `Assets/Resources/DuelModelLibrary.asset`
(Yaw Offset) automatically, so you only do it once. First guesses: Shiva 180, Garuda 90 (a wing-on-edge model looks flat).
The same file has **Height** if a model is the wrong size.

Animation is best effort: if a prefab's Animator has the project's parameters (`Speed`, `TrigAttack`, `TrigQ/W/E/R`,
`TrigHit`) they are driven; otherwise the model just slides around. Pink models mean the materials are not URP
materials (Edit > Rendering > Materials > Convert Selected Built-in Materials to URP).

**Tracking the models in git (optional).** Only do this in a PRIVATE repository: a public repo would redistribute the art, whatever
the intent. `.gitattributes` already stores `Assets/Models/**` with Git LFS. To start tracking: make the repo private
(GitHub > Settings > Danger Zone), delete the `/Assets/Models/` and `/Assets/Models.meta` lines from `.gitignore`, run
`git lfs install`, then `git add Assets/Models .gitattributes` and commit and push from your PC. Expect hundreds of MB
(Bahamut alone is about 80 MB); GitHub's free LFS quota is 1 GB of storage and 1 GB of bandwidth per month. Until then, the
**Save Model Rig Report** tool gives the skeleton data needed to tune the animation.

`Assets/Models/` is in `.gitignore` on purpose: the repo is public and licensed art needs a documented chain of title
before it is shared (or must be replaced with original IP). Phone builds made on your PC still include the models,
so do not hand those APKs to anyone outside the team. If `Assets/Models` is already tracked in your local repo, the
ignore rule does not apply to it; run `git rm -r --cached Assets/Models` first.

## What you can test

- **SPELLS** opens a picker with all 11 spells. Choose slot D or F, then tap a spell. The duel pauses while it is open.
  Changing a spell restarts its cooldown. Choosing the spell already in the other slot swaps the two.
- **TOOLS** has: reset cooldowns, heal me, hurt me 50% (for Cure), stun me 2 s and slow me 3 s (for Esuna and Warp),
  reset enemy, restore summons, unlock champion Ultima, level +/-1, the model turn buttons, Kill me (death animation) and
  the enemy bot ON/OFF (OFF makes it a dummy).
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
| Ultima | Cast near Shiva or Garuda (monsters). Two charges. TOOLS > Unlock champion Ultima lets it hit the enemy too. |
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

If Gradle fails with `Duplicate class kotlin.collections.jdk8...` (Unity IAP 5 pulling mixed Kotlin versions), the build
tool now fixes it itself by adding a block to `Assets/Plugins/Android/baseProjectTemplate.gradle`. You can also run
**Tools > Project D > Fix Android Kotlin Duplicate-Class Build Error**. The many `Hidden/Sentis/...` shader warnings
come from the Unity AI Inference package and are harmless.

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
- Audio is not used by this scene. Weapons are not attached to the models, and the animation is procedural (a stand-in for real clips).

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
