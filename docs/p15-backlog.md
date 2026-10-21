# Project D — P15 Backlog (Vertical Slice Integration)

> Codename **Zantetsuken** · CONFIDENTIAL · All original design.
> P0–P14/World Rift built ~150 tested systems as independent, engine-agnostic cores.
> **P15 is the integration tier**: the glue that connects those systems into ONE
> playable flow you can run start-to-finish. This turns "many tested systems" into
> "a game you can play through."
>
> Engine-agnostic, deterministic, unit-tested — the app skeleton the client mounts on.

## Why P15
Systems in isolation don't make a game; the *flow* does. P15 provides the state machine,
service wiring, session context, and match loop that sequence the app from launch to
post-game and back — the vertical slice: **Boot → Main Menu → Mode Select → Queue →
Champion Select → In-Match → PostGame → Rewards → Main Menu.** It also glues persistence
(save/load), progression handoff (match XP → account level → KP), and a build/asset
manifest so a client can be assembled.

## Design principles
- **One flow, deterministic:** the same inputs sequence the same screens/states.
- **Loosely coupled:** systems are reached via a service locator, not hard references.
- **Server-authoritative-ready:** the flow mirrors what a server would drive.
- **Testable end-to-end:** a full match loop can run headless in a unit test.

## 10 systems
1.  **App flow controller** — the top-level orchestrator that owns the game state machine.
2.  **Game state machine** — Boot/MainMenu/ModeSelect/Queue/ChampSelect/Loading/InMatch/
    PostGame states with legal transitions.
3.  **Match loop** — a headless match runner (tick clock, objectives, end condition) that
    produces a result payload.
4.  **Service locator** — register/resolve the game services (no singletons in gameplay).
5.  **Save glue** — serialize/restore the player's persistent profile across sessions.
6.  **Scene router** — maps states to scene ids + transition guards (no illegal jumps).
7.  **Session context** — the per-run context (selected mode, map, party, world) passed
    between states.
8.  **Boot config** — startup sequence (load config → services → account → landing state).
9.  **Progression glue** — post-match handoff: result → account XP/level → KP → rewards.
10. **Build manifest** — asset/scene/version manifest for assembling a client build.

## The vertical slice (what actually runs)
```
Boot -> MainMenu -> ModeSelect -> Queue -> ChampSelect -> Loading -> InMatch -> PostGame -> (rewards) -> MainMenu
```
- `AppFlowController` advances the `GameStateMachine`, using `SessionContext` to carry the
  chosen mode/map/world.
- On InMatch, `MatchLoop` runs the game to an end condition and emits a result.
- `ProgressionGlue` applies the result (XP -> level-ups -> KP) and records rewards.
- `SceneRouter` maps each state to a scene id; `BuildManifest` lists what a build needs.
- The whole loop is covered by an end-to-end test that plays a match headless.

## Cross-links
- Sequences the P0 champion select, World Rift mode, PostGame results, and KP rewards.
- Save glue extends P7 cloud save; progression glue drives the KP Store leveling.
- Session context feeds the P7 matchmaker; the state machine is the client's backbone.
