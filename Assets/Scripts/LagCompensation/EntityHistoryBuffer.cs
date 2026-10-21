// Project D  |  Codename: Zantetsuken
// EntityHistoryBuffer.cs  (Sprint 5 — lag compensation)
//
// A per-entity ring buffer of timestamped authoritative positions/hitbox radii.
// The server samples every simulation tick; when validating a hit it asks this
// buffer for the entity's interpolated position AT THE ATTACKER'S RENDER TIME,
// so a shot that looked like a hit on the shooter's screen is judged fairly.
//
// Pure C# (no UnityEngine) — deterministic and unit-testable.

using System;
using ProjectD.Networking.Transport;

namespace ProjectD.Networking.LagCompensation
{
    public struct HistorySample
    {
        public float Time;       // server time (seconds)
        public NetVec2 Position;
        public float HitRadius;
        public bool Alive;
    }

    public sealed class EntityHistoryBuffer
    {
        private readonly HistorySample[] _ring;
        private int _head;        // index of most-recent sample
        private int _count;

        public int EntityId { get; }
        public float NewestTime => _count == 0 ? 0f : _ring[_head].Time;
        public float OldestTime => _count == 0 ? 0f : _ring[OldestIndex()].Time;
        public int Count => _count;

        /// <param name="capacity">Samples to retain (tickRate * historySeconds).</param>
        public EntityHistoryBuffer(int entityId, int capacity = 64)
        {
            EntityId = entityId;
            _ring = new HistorySample[Math.Max(2, capacity)];
        }

        public void Record(float time, NetVec2 position, float hitRadius, bool alive)
        {
            _head = (_head + 1) % _ring.Length;
            _ring[_head] = new HistorySample { Time = time, Position = position, HitRadius = hitRadius, Alive = alive };
            if (_count < _ring.Length) _count++;
        }

        private int OldestIndex() => (_head - _count + 1 + _ring.Length) % _ring.Length;

        /// <summary>
        /// Reconstruct the entity's state at <paramref name="time"/> by interpolating
        /// between the two bracketing samples. Clamps to the buffer's time range.
        /// </summary>
        public bool TrySampleAt(float time, out HistorySample result)
        {
            result = default;
            if (_count == 0) return false;

            // Clamp to available window.
            if (time >= NewestTime) { result = _ring[_head]; return true; }
            if (time <= OldestTime) { result = _ring[OldestIndex()]; return true; }

            // Walk backward from head to find the bracketing pair [older, newer].
            int newerIdx = _head;
            for (int i = 0; i < _count - 1; i++)
            {
                int olderIdx = (newerIdx - 1 + _ring.Length) % _ring.Length;
                var newer = _ring[newerIdx];
                var older = _ring[olderIdx];
                if (older.Time <= time && time <= newer.Time)
                {
                    float span = newer.Time - older.Time;
                    float t = span <= 0f ? 0f : (time - older.Time) / span;
                    result = new HistorySample
                    {
                        Time = time,
                        Position = NetVec2.Lerp(older.Position, newer.Position, t),
                        HitRadius = older.HitRadius + (newer.HitRadius - older.HitRadius) * t,
                        Alive = older.Alive && newer.Alive
                    };
                    return true;
                }
                newerIdx = olderIdx;
            }
            result = _ring[OldestIndex()];
            return true;
        }

        public void Clear() { _head = 0; _count = 0; }
    }
}
