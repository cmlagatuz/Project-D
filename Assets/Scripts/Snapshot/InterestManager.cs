// Project D  |  Codename: Zantetsuken
// InterestManager.cs  (Sprint 5+ — interest-managed snapshots)
//
// Decides which entities each viewer needs this snapshot. A MOBA doesn't send the
// whole world to everyone every tick: a viewer receives entities within their
// vision/interest radius (their champion + allies + nearby visible enemies,
// minions, monsters, and structures). This cuts bandwidth dramatically as minion
// and monster counts grow, and it is also an anti-cheat measure (clients never
// receive positions of enemies they shouldn't see through fog).
//
// Pure C# (no UnityEngine) — deterministic and unit-testable.

using System.Collections.Generic;
using ProjectD.Networking.Transport;

namespace ProjectD.Networking.Snapshot
{
    public struct ReplicatedEntity
    {
        public int EntityId;
        public byte Team;           // 2 = neutral
        public NetVec2 Position;
        public float Health, MaxHealth, Resource;
        public bool Alive;
        public bool AlwaysRelevant;  // structures/objectives everyone should track
        public float VisionRadius;   // how far this entity can see (for viewers it owns)
    }

    public sealed class InterestManager
    {
        /// <summary>Default interest radius when an entity provides none.</summary>
        public float DefaultVisionRadius { get; set; } = 12f;

        /// <summary>
        /// Build the set of entities relevant to <paramref name="viewerId"/> on
        /// <paramref name="viewerTeam"/>. Always includes: the viewer, all allies,
        /// always-relevant objects, and any entity within the union of the viewer's
        /// team's vision radii (approximated by per-entity vision here).
        /// </summary>
        public List<ReplicatedEntity> BuildRelevantSet(
            int viewerId, byte viewerTeam, IReadOnlyList<ReplicatedEntity> all)
        {
            // 1) Collect this team's vision sources (their alive units).
            var visionSources = new List<ReplicatedEntity>();
            foreach (var e in all)
                if (e.Team == viewerTeam && e.Alive)
                    visionSources.Add(e);

            var result = new List<ReplicatedEntity>();
            foreach (var e in all)
            {
                if (e.Team == viewerTeam || e.AlwaysRelevant)
                {
                    result.Add(e); // own team + structures/objectives are always sent
                    continue;
                }

                // Enemy/neutral: include only if within any friendly vision radius.
                if (IsVisibleTo(e, visionSources))
                    result.Add(e);
            }
            return result;
        }

        private bool IsVisibleTo(ReplicatedEntity target, List<ReplicatedEntity> sources)
        {
            foreach (var s in sources)
            {
                float r = s.VisionRadius > 0f ? s.VisionRadius : DefaultVisionRadius;
                if (s.Position.DistanceTo(target.Position) <= r)
                    return true;
            }
            return false;
        }

        /// <summary>Convenience: convert a relevant set into a WorldSnapshot for a viewer.</summary>
        public WorldSnapshot ToSnapshot(int tick, float serverTime, List<ReplicatedEntity> relevant)
        {
            var snap = new WorldSnapshot { Tick = tick, ServerTime = serverTime, IsKeyframe = true, BaselineTick = -1 };
            foreach (var e in relevant)
                snap.Entities.Add(new EntityState
                {
                    EntityId = e.EntityId, Team = e.Team, Position = e.Position,
                    Health = e.Health, MaxHealth = e.MaxHealth, Resource = e.Resource, Alive = e.Alive
                });
            return snap;
        }
    }
}
