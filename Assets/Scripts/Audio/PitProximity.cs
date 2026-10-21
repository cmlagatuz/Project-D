// Project D  |  Codename: Zantetsuken
// PitProximity.cs  (Audio — decide which pit theme the LOCAL player should hear)
//
// Pure logic: given the local player's position and each pit's center + radius, which
// objective theme (if any) should play, and at what intensity? Only ONE pit can be
// active (the nearest one the player is inside/near). The music is a per-client
// decision, so only players near the pit get a theme — never broadcast.
//
// Hysteresis (enter radius < exit radius) prevents rapid on/off toggling at the edge.
// Engine-agnostic + unit-testable.

using System;
using System.Collections.Generic;

namespace ProjectD.Audio
{
    public struct PitZone
    {
        public string PitId;            // "pit_top" / "pit_bottom"
        public string ObjectiveId;      // whatever objective currently occupies the pit
        public float CenterX, CenterZ;  // pit center (planar)
        public float EnterRadius;
        public float ExitRadius;        // >= EnterRadius
    }

    public struct ProximityResult
    {
        public bool HasTheme;
        public string PitId;
        public string ObjectiveId;
        public float Intensity;         // 0..1, grows toward the center
    }

    public static class PitProximity
    {
        public static ProximityResult Resolve(float x, float z, IReadOnlyList<PitZone> pits, string currentPitId)
        {
            ProximityResult best = new ProximityResult { HasTheme = false };
            float bestDist = float.MaxValue;

            foreach (var pit in pits)
            {
                if (string.IsNullOrEmpty(pit.ObjectiveId)) continue; // empty pit -> no theme

                float dx = x - pit.CenterX, dz = z - pit.CenterZ;
                float dist = (float)Math.Sqrt(dx * dx + dz * dz);

                bool isCurrent = pit.PitId == currentPitId;
                float threshold = isCurrent ? pit.ExitRadius : pit.EnterRadius;
                if (dist > threshold) continue;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    float inner = pit.EnterRadius <= 0f ? 1f : Math.Max(0f, Math.Min(1f, 1f - dist / pit.EnterRadius));
                    best = new ProximityResult
                    {
                        HasTheme = true,
                        PitId = pit.PitId,
                        ObjectiveId = pit.ObjectiveId,
                        Intensity = isCurrent && dist > pit.EnterRadius ? Math.Max(0.15f, inner) : inner
                    };
                }
            }
            return best;
        }
    }
}
