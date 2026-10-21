// Project D  |  Codename: Zantetsuken
// SimEntityRegistry.cs  (Sprint 5+ — unified server-side entity registry)
//
// One place the authoritative server tracks EVERY simulated actor: player
// champions, minions, jungle monsters, summons, and structures. Each frame the
// server records positions into the LagCompensator (so minions/monsters are
// lag-compensated too, not just champions) and exposes a ReplicatedEntity view
// for the InterestManager.
//
// Pure C# (no UnityEngine) so it runs headless and is unit-testable.

using System.Collections.Generic;
using ProjectD.Networking.Transport;
using ProjectD.Networking.LagCompensation;
using ProjectD.Networking.Snapshot;

namespace ProjectD.Networking.Server
{
    public enum SimEntityKind : byte { Champion, Minion, Monster, Summon, Structure }

    public sealed class SimEntity
    {
        public int EntityId;
        public byte Team;                 // 0/1 players, 2 neutral
        public SimEntityKind Kind;
        public NetVec2 Position;
        public float Health, MaxHealth, Resource;
        public bool Alive = true;
        public float HitRadius = 0.5f;
        public float VisionRadius = 12f;
        public bool AlwaysRelevant;       // structures/objectives
    }

    public sealed class SimEntityRegistry
    {
        private readonly Dictionary<int, SimEntity> _entities = new();
        private readonly LagCompensator _lagComp;
        private int _nextId = 1;

        public SimEntityRegistry(LagCompensator lagComp) { _lagComp = lagComp; }

        public IReadOnlyDictionary<int, SimEntity> Entities => _entities;

        public SimEntity Spawn(SimEntityKind kind, byte team, NetVec2 pos, float maxHealth,
                               float hitRadius = 0.5f, float visionRadius = 12f, bool alwaysRelevant = false, int forcedId = -1)
        {
            int id = forcedId > 0 ? forcedId : _nextId++;
            if (forcedId > 0 && id >= _nextId) _nextId = id + 1;
            var e = new SimEntity
            {
                EntityId = id, Team = team, Kind = kind, Position = pos,
                Health = maxHealth, MaxHealth = maxHealth, HitRadius = hitRadius,
                VisionRadius = visionRadius, AlwaysRelevant = alwaysRelevant
            };
            _entities[id] = e;
            return e;
        }

        public bool TryGet(int id, out SimEntity e) => _entities.TryGetValue(id, out e);

        public void Despawn(int id) => _entities.Remove(id);

        /// <summary>Record every entity's authoritative state into lag-comp history.</summary>
        public void RecordHistory(float serverTime)
        {
            foreach (var e in _entities.Values)
                _lagComp.Record(e.EntityId, serverTime, e.Position, e.HitRadius, e.Alive);
        }

        /// <summary>Snapshot the whole world as ReplicatedEntity for interest filtering.</summary>
        public List<ReplicatedEntity> BuildReplicationView()
        {
            var list = new List<ReplicatedEntity>(_entities.Count);
            foreach (var e in _entities.Values)
                list.Add(new ReplicatedEntity
                {
                    EntityId = e.EntityId, Team = e.Team, Position = e.Position,
                    Health = e.Health, MaxHealth = e.MaxHealth, Resource = e.Resource,
                    Alive = e.Alive, AlwaysRelevant = e.AlwaysRelevant, VisionRadius = e.VisionRadius
                });
            return list;
        }
    }
}
