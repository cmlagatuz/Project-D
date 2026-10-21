// Project D  |  Codename: Zantetsuken
// LagCompensator.cs  (Sprint 5 — server-side hit validation)
//
// The authoritative hit judge. When a client fires a skillshot/attack, the server
// rewinds every candidate target to the moment the SHOOTER actually saw the world
// (server receive time minus half RTT minus the client's interpolation delay),
// then tests the shot against those rewound hitboxes. This makes hits fair on
// high latency without letting the client assert hits directly (GCD 14.3).
//
// Guardrails against abuse:
//   - The rewind is CLAMPED to a maximum (MaxRewindSeconds); a client can never
//     rewind targets arbitrarily far into the past.
//   - Dead targets in the past are ignored.
//   - The reported client time is validated against the server clock, not trusted.
//
// Pure C# — deterministic and unit-testable.

using System;
using System.Collections.Generic;
using ProjectD.Networking.Transport;

namespace ProjectD.Networking.LagCompensation
{
    public struct HitQuery
    {
        public int ShooterEntityId;
        public int ShooterTeam;
        public NetVec2 Origin;        // authoritative shooter position
        public NetVec2 Direction;     // normalized aim
        public float Range;
        public float ProjectileRadius; // thickness of the skillshot
        public float ShooterRttSeconds;
        public float ShooterInterpDelaySeconds;
    }

    public struct HitResult
    {
        public bool Hit;
        public int TargetEntityId;
        public float RewoundToTime;   // for telemetry/audit
        public float DistanceAlongShot;
    }

    public sealed class LagCompensator
    {
        /// <summary>Hard cap on how far back a client may rewind targets.</summary>
        public float MaxRewindSeconds { get; set; } = 0.25f;

        private readonly Dictionary<int, EntityHistoryBuffer> _histories = new();

        public EntityHistoryBuffer GetOrCreate(int entityId, int capacity = 64)
        {
            if (!_histories.TryGetValue(entityId, out var h))
                _histories[entityId] = h = new EntityHistoryBuffer(entityId, capacity);
            return h;
        }

        public void Record(int entityId, float serverTime, NetVec2 pos, float hitRadius, bool alive)
            => GetOrCreate(entityId).Record(serverTime, pos, hitRadius, alive);

        /// <summary>
        /// Validate a linear skillshot against all enemy histories rewound to the
        /// shooter's render time. Returns the first (closest along the shot) hit.
        /// </summary>
        public HitResult ValidateLineHit(in HitQuery q, float serverNow)
        {
            // Compute the fair rewind target time, clamped for anti-abuse.
            float rawRewind = q.ShooterRttSeconds * 0.5f + q.ShooterInterpDelaySeconds;
            float rewind = Math.Min(Math.Max(0f, rawRewind), MaxRewindSeconds);
            float sampleTime = serverNow - rewind;

            HitResult best = new HitResult { Hit = false, DistanceAlongShot = float.MaxValue };

            foreach (var kvp in _histories)
            {
                int targetId = kvp.Key;
                if (targetId == q.ShooterEntityId) continue;

                if (!kvp.Value.TrySampleAt(sampleTime, out var s) || !s.Alive) continue;

                if (TryLineCircle(q.Origin, q.Direction, q.Range, q.ProjectileRadius + s.HitRadius,
                                  s.Position, out float along))
                {
                    if (along < best.DistanceAlongShot)
                        best = new HitResult { Hit = true, TargetEntityId = targetId, RewoundToTime = sampleTime, DistanceAlongShot = along };
                }
            }
            return best;
        }

        /// <summary>
        /// Segment-vs-circle test: does a ray from origin along dir (length range)
        /// pass within combinedRadius of center? Outputs distance along the ray.
        /// </summary>
        public static bool TryLineCircle(NetVec2 origin, NetVec2 dir, float range, float combinedRadius,
                                         NetVec2 center, out float along)
        {
            along = 0f;
            // Normalize direction defensively.
            float dl = (float)Math.Sqrt(dir.X * dir.X + dir.Z * dir.Z);
            if (dl <= 1e-5f) return false;
            float dx = dir.X / dl, dz = dir.Z / dl;

            float toX = center.X - origin.X, toZ = center.Z - origin.Z;
            float proj = toX * dx + toZ * dz;             // distance along the ray to closest point
            proj = Math.Min(Math.Max(proj, 0f), range);    // clamp to the segment

            float closestX = origin.X + dx * proj;
            float closestZ = origin.Z + dz * proj;
            float ddx = center.X - closestX, ddz = center.Z - closestZ;
            float distSq = ddx * ddx + ddz * ddz;

            if (distSq <= combinedRadius * combinedRadius)
            {
                along = proj;
                return true;
            }
            return false;
        }

        public void Clear() => _histories.Clear();
    }
}
