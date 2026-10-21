# Project D — *Dissidia: Final Fantasy Ultima Pugma Clamoris*

> **Codename:** Zantetsuken · **Status:** Pre-production (Sprint 5 — real transport) · **Classification: CONFIDENTIAL**
> Mobile-first 5v5 MOBA. Engine target: **Unity** (Unreal fallback per GCD §14.1).

Engineering realization of the Game Concept Document v1.0. Contains the data-driven
gameplay foundation, fairness-critical objective + draft systems, a **server-authoritative
networking layer with client prediction & reconciliation**, the touch-controls greybox,
and the **champion select** screen.

## ⚠️ Rights notice
Final Fantasy / Dissidia names, characters, and models are owned by **Square Enix**.
Possessing model files grants no commercial rights. Private prototype / licensing pitch
only until written authorization + chain of title exist. All titles, lore, and ability
names here are **original Project D design**. See `docs/LICENSE-NOTICE.md`.

## Repository layout
```
ProjectD/
├─ Assets/
│  ├─ Scripts/
│  │  ├─ Core/          Match bootstrap, constants
│  │  ├─ Data/          ScriptableObjects: Champion (+bio), Ability, Buff, Monster
│  │  ├─ Gameplay/
│  │  │  ├─ Combat/     StatBlock, DamageSystem, IDamageable
│  │  │  ├─ Champions/  AbilitySystem, PrototypePawn (networked)
│  │  │  ├─ Objectives/ Summon shuffle-bag, Divine rotation, Contribution
│  │  │  ├─ Input/      TouchJoystick, AbilityButton
│  │  │  ├─ Camera/     ArenaCameraRig
│  │  │  ├─ Minions/    Goblin wave spawner
│  │  │  └─ Map/        Structures, Crystal Core
│  │  ├─ Networking/    IAuthoritativeServer, NetworkTransportClient,
│  │  │                 NetworkedCastController, ClientMoveReconciler,
│  │  │                 LocalLoopbackServer (offline test double)       ← Sprint 5
│  │  │     ├─ Transport/       ITransport, UdpTransport, NetPrimitives
│  │  │     ├─ Server/          AuthoritativeMatchServer
│  │  │     ├─ Reconnect/       SessionManager, ReconnectController
│  │  │     ├─ LagCompensation/ EntityHistoryBuffer, LagCompensator
│  │  │     └─ Snapshot/        WorldSnapshot
│  │  ├─ UI/ChampionSelect/  Controller, View, Card                     ← Sprint 5
│  │  ├─ Utilities/     SeededRandom, ObjectPool
│  │  ├─ Editor/        MiniJSON, GameDataImporter, GreyboxSceneBuilder,
│  │  │                 ChampionSelectSceneBuilder
│  │  └─ Tests/         Objective, NetworkReconciliation, DraftController
│  ├─ GameData/         JSON seeds (champions w/ lore, summons) + generated assets
│  └─ Scenes/           Greybox_Prototype + ChampionSelect (generated)
├─ docs/ · server/ · .github/workflows/
```

## Editor tools (menu: **Tools ▸ Project D**)
| Menu item | What it does |
|---|---|
| Import All Game Data | Generates all champion/ability/summon/buff assets from the JSON seeds. |
| Import Champion Data | 10 champions + 50 abilities (with lore/playstyle/tips). |
| Import Summon Data | 6 elemental + Odin + 4 divine objective assets. |
| Build Greybox Prototype Scene | Networked touch-controls arena (predict + reconcile). |
| Build Champion Select Scene | Pick-phase screen populated from champion assets. |

**Quick start:** *Import All Game Data* → *Build Greybox Prototype Scene* (play: drag to
move, Q/W/E/R to cast; Console shows CONFIRMED/REJECTED) → *Build Champion Select Scene*
(play: hover to inspect, LOCK IN). Details in `docs/prototype-setup.md`.

## Sprint 5 — networking (real transport)
Full server-authoritative stack over a **real UDP transport**:
- **`ITransport` + `UdpTransport`** — dependency-free sockets; Netcode/Mirror drop in via the same interface.
- **`AuthoritativeMatchServer`** — fixed-tick sim, anti-teleport movement, cast validation, delta/keyframe snapshots.
- **`NetworkTransportClient`** — implements `IAuthoritativeServer`; the drop-in replacement for `LocalLoopbackServer` (pawn unchanged).
- **Reconnect + resync** — `SessionManager` (token/grace/same-entity reclaim), `ReconnectController` (backoff), `FullState`/`ResyncRequest`.
- **Lag-compensated hit validation** — `EntityHistoryBuffer` + `LagCompensator` rewind targets to the shooter's render time, clamped for anti-abuse.

Cast flow: **predict → send `CastIntent` → server validates + lag-comp hit test → `CastAck` (accept/reject) → client reconciles**. See `docs/networking.md`.

## Champion roster & codex
First 10 authored with source world, original release date, base stats, growth, full
Passive/Q/W/E/Ultimate kits, and detailed lore/playstyle/strengths/weaknesses/tips:
Warrior of Light, Garland *(FF I)* · Firion, The Emperor *(FF II)* · Onion Knight,
Cloud of Darkness *(FF III)* · Cecil, Kain *(FF IV)* · Bartz, Exdeath *(FF V)*.
Full write-ups in `docs/champion-codex.md`.
