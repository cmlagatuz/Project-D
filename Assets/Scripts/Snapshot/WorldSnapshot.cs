// Project D  |  Codename: Zantetsuken
// WorldSnapshot.cs  (Sprint 5 — resync payload)
//
// Compact authoritative world state. Two flavours:
//   - Keyframe / FullState: the entire visible state, sent on connect, reconnect,
//     or an explicit ResyncRequest. Self-sufficient (client can rebuild from zero).
//   - Delta: only entities that changed since a baseline tick (bandwidth saver).
//
// Serialization lives here so both the server (write) and client (read) share one
// definition. Pure C# — unit-testable, no UnityEngine.

using System.Collections.Generic;
using ProjectD.Networking.Transport;

namespace ProjectD.Networking.Snapshot
{
    public struct EntityState
    {
        public int EntityId;
        public byte Team;
        public NetVec2 Position;
        public float Health;
        public float MaxHealth;
        public float Resource;
        public bool Alive;
    }

    public sealed class WorldSnapshot
    {
        public int Tick;
        public float ServerTime;
        public bool IsKeyframe;
        public int BaselineTick;                 // for deltas; -1 for keyframes
        public readonly List<EntityState> Entities = new();

        public void Write(NetWriter w)
        {
            w.Int(Tick);
            w.Float(ServerTime);
            w.Bool(IsKeyframe);
            w.Int(BaselineTick);
            w.Int(Entities.Count);
            foreach (var e in Entities)
            {
                w.Int(e.EntityId);
                w.Byte(e.Team);
                w.Vec2(e.Position);
                w.Float(e.Health);
                w.Float(e.MaxHealth);
                w.Float(e.Resource);
                w.Bool(e.Alive);
            }
        }

        public static WorldSnapshot Read(NetReader r)
        {
            var s = new WorldSnapshot
            {
                Tick = r.Int(),
                ServerTime = r.Float(),
                IsKeyframe = r.Bool(),
                BaselineTick = r.Int()
            };
            int count = r.Int();
            for (int i = 0; i < count; i++)
            {
                s.Entities.Add(new EntityState
                {
                    EntityId = r.Int(),
                    Team = r.Byte(),
                    Position = r.Vec2(),
                    Health = r.Float(),
                    MaxHealth = r.Float(),
                    Resource = r.Float(),
                    Alive = r.Bool()
                });
            }
            return s;
        }

        /// <summary>Build a delta containing only entities that changed vs baseline.</summary>
        public WorldSnapshot MakeDeltaFrom(WorldSnapshot baseline)
        {
            var delta = new WorldSnapshot { Tick = Tick, ServerTime = ServerTime, IsKeyframe = false, BaselineTick = baseline?.Tick ?? -1 };
            var baseById = new Dictionary<int, EntityState>();
            if (baseline != null)
                foreach (var e in baseline.Entities) baseById[e.EntityId] = e;

            foreach (var e in Entities)
            {
                if (!baseById.TryGetValue(e.EntityId, out var b) || !Same(b, e))
                    delta.Entities.Add(e);
            }
            return delta;
        }

        private static bool Same(EntityState a, EntityState b)
            => a.Position.Equals(b.Position) && a.Health == b.Health &&
               a.Resource == b.Resource && a.Alive == b.Alive;
    }
}
