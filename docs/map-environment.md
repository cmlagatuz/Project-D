# Map Environment — Pits, Spawns & Per-Pit BGM

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

The river-pit objective system: dynamic summon/divine placement, spawn cadence, and
per-pit background music that only the players near the pit hear. Server-authoritative
logic (pit layout, spawn timers, sequences); the BGM proximity runs per client.

## Dynamic pit placement
- The **summon pit** sits in the river pit NEXT TO the bottom duo (marksman + support).
  Which pit that is (Top or Bottom) depends on the side the duo laned on that match,
  so it can flip like Wild Rift — `PitAssignment.FromDuoPit/FromDuoLane/FromRoleLanes`.
- The **divine manifestation pit** is ALWAYS the opposite river pit.

## Spawns & respawns (data-driven)
`SpawnSchedule` (ScriptableObject) sets first-spawn + respawn intervals: jungle camps
by tier, summon pit (first elemental, respawn, Odin unlock delay, Odin respawn), and
divine pit (first + rotation respawn). `SpawnTimerEngine` tracks each objective
deterministically. `ObjectiveDirector` runs the elemental sequence -> Odin and rotates
the divine manifestations with no immediate repeat.

## Per-pit BGM (imported audio, proximity-based)
Each elemental summon, Odin, and each divine manifestation has its OWN music track.
Import audio and assign it in a **`PitBgmSet`**: drop tracks in
`Assets/GameData/Audio/Summons/<clipKey>.mp3` and `.../Divine/<clipKey>.mp3`, then
**Tools > Project D > Import Pit BGM** auto-binds by filename.

**Local, not broadcast:** `ProximityBgmController` runs on each client for its OWN
player. When the local player enters/nears a pit, it crossfades to whatever objective
occupies that pit (Ifrit's theme while Ifrit is up, Materia's at the divine pit,
Odin's once Odin spawns). Players NOT near the pit hear only their default map music
(which ducks slightly while a pit theme plays for them). Hysteresis prevents flicker;
intensity ramps toward the center. `ObjectiveDirector.SetPitObjective` is the only
thing that tells the audio "what's in the pit" — it never forces music on anyone.

## Wiring
```csharp
director.Initialize(duoRiverPit);          // summon pit = duo side, divine = opposite
director.Tick(deltaSeconds);               // server clock -> spawns
director.NotifySummonKilled();             // advance elemental sequence / unlock Odin
director.NotifyDivineKilled();             // rotate the divine pit
bgm.SetLocalPlayer(myChampionTransform);   // per client
bgm.SetMusicVolume(settings.musicVolume);  // from the settings pack
```

## Demo
**Tools > Project D > Build Map Env Demo**: two pits + a movable marker. Move
(WASD/arrows) into a pit to hear its theme (assign a PitBgmSet with your clips).
Buttons kill the summon/divine (advancing sequence/rotation) and **flip the duo side**
so you see the summon pit move and the divine pit follow to the opposite side.
