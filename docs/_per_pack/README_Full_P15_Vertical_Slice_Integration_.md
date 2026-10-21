# Project D — P15 (Vertical Slice Integration)
> Codename Zantetsuken · CONFIDENTIAL
The glue that turns ~150 tested systems into one playable flow: a game state machine,
service locator, session context, headless match loop, save glue, scene router, boot
sequence, progression handoff, build manifest, and the AppFlowController that sequences
MainMenu -> ModeSelect -> Queue -> ChampSelect -> Loading -> InMatch -> PostGame -> MainMenu.
Proven by an end-to-end test that plays a World Rift match headless and persists the profile.
Engine-agnostic + unit-tested (P15Tests, 13). See docs/p15-backlog.md + docs/p15-systems.md.
