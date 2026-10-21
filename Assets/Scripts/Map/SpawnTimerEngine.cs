// Project D  |  Codename: Zantetsuken
// SpawnTimerEngine.cs  (Map — deterministic spawn/respawn timers)
//
// Tracks when each objective (a monster camp, the summon pit, the divine pit) next
// becomes available on the match clock. The server advances the clock and calls
// NotifyKilled when an objective dies; the engine schedules the respawn. Pure C# —
// deterministic + unit-testable so client HUD timers match the authoritative server.

using System.Collections.Generic;

namespace ProjectD.Map
{
    public enum SpawnState { Pending, Alive, Respawning }

    public sealed class ObjectiveTimer
    {
        public string Id;
        public SpawnState State;
        public float NextSpawnAt;   // match-clock seconds when it (re)spawns
        public float RespawnSeconds;
    }

    public sealed class SpawnTimerEngine
    {
        private readonly Dictionary<string, ObjectiveTimer> _timers = new();

        public void Register(string id, float firstSpawnSeconds, float respawnSeconds)
        {
            _timers[id] = new ObjectiveTimer
            {
                Id = id, State = SpawnState.Pending,
                NextSpawnAt = firstSpawnSeconds, RespawnSeconds = respawnSeconds
            };
        }

        public ObjectiveTimer Get(string id) => _timers.TryGetValue(id, out var t) ? t : null;

        /// <summary>Advance to matchClockSeconds; return the ids that spawned on this tick.</summary>
        public List<string> Tick(float matchClockSeconds)
        {
            var spawned = new List<string>();
            foreach (var t in _timers.Values)
            {
                if ((t.State == SpawnState.Pending || t.State == SpawnState.Respawning) && matchClockSeconds >= t.NextSpawnAt)
                {
                    t.State = SpawnState.Alive;
                    spawned.Add(t.Id);
                }
            }
            return spawned;
        }

        public void NotifyKilled(string id, float matchClockSeconds)
        {
            if (!_timers.TryGetValue(id, out var t)) return;
            t.State = SpawnState.Respawning;
            t.NextSpawnAt = matchClockSeconds + t.RespawnSeconds;
        }

        public float SecondsUntilSpawn(string id, float matchClockSeconds)
        {
            var t = Get(id);
            if (t == null || t.State == SpawnState.Alive) return 0f;
            float d = t.NextSpawnAt - matchClockSeconds;
            return d > 0f ? d : 0f;
        }

        public void SetNextSpawn(string id, float matchClockSeconds, float respawnSeconds = -1f)
        {
            if (!_timers.TryGetValue(id, out var t)) return;
            t.NextSpawnAt = matchClockSeconds;
            t.State = matchClockSeconds <= 0f ? SpawnState.Pending : SpawnState.Respawning;
            if (respawnSeconds >= 0f) t.RespawnSeconds = respawnSeconds;
        }

        public IReadOnlyCollection<ObjectiveTimer> All => _timers.Values;
    }
}
