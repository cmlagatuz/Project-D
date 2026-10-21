# P15 — Systems Reference (Vertical Slice Integration)
> Project D · Codename Zantetsuken · CONFIDENTIAL · All original design. Engine-agnostic + tested (P15Tests, 13 incl. full end-to-end).
1.  StateMachine/GameStateMachine — Boot..PostGame states + legal-transition guard.
2.  Session/SessionContext — per-run mode/map/world/champion; World Rift map resolution.
3.  Services/ServiceLocator — register/resolve services (no gameplay singletons).
4.  MatchLoop/MatchLoop — headless match runner (nexus pressure, end conditions, result).
5.  Save/SaveGlue + SaveStore — dependency-free profile serialize/restore + round-trip.
6.  Routing/SceneRouter — state->scene id + scene-load detection.
7.  Boot/BootSequence — startup steps to Ready, halts on failure.
8.  Progression/ProgressionGlue — result -> XP -> level-ups -> KP + rewards (+ World Rift rating).
9.  Build/BuildManifest — scene/asset manifest, validation, fingerprint.
10. App/AppFlowController — the top-level orchestrator sequencing the whole vertical slice.

## The vertical slice (proven end-to-end in a test)
MainMenu -> ModeSelect -> Queue -> ChampSelect -> Loading -> InMatch -> PostGame -> MainMenu,
playing a World Rift match headless, applying progression, and PERSISTING the profile
(a fresh controller reloads the same KP + rating).

## Tests
P15Tests (13) incl. VerticalSlice_FullLoopRunsAndPersists — the full loop as one test.
