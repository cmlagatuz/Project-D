// Project D  |  Codename: Zantetsuken
// SpawnSchedule.cs  (Map — spawn/respawn timing config)
//
// Data-driven spawn cadence for map objectives (GCD Appendix A): first-spawn times
// and respawn intervals for jungle monster tiers, the elemental summons, the elder
// (Odin), and the divine manifestations. Times are match-clock seconds. The server
// ticks these; values live in data so live-ops can tune them.

using System;
using UnityEngine;

namespace ProjectD.Map
{
    [Serializable]
    public struct SpawnTiming
    {
        [Tooltip("Match-clock seconds of the FIRST spawn.")]
        public float firstSpawnSeconds;
        [Tooltip("Seconds between despawn/kill and the next spawn.")]
        public float respawnSeconds;
    }

    [CreateAssetMenu(fileName = "SpawnSchedule", menuName = "Project D/Map Spawn Schedule")]
    public sealed class SpawnSchedule : ScriptableObject
    {
        [Header("Jungle monster camps (per tier)")]
        public SpawnTiming smallCamp = new SpawnTiming { firstSpawnSeconds = 30f, respawnSeconds = 90f };
        public SpawnTiming mediumCamp = new SpawnTiming { firstSpawnSeconds = 30f, respawnSeconds = 100f };
        public SpawnTiming largeCamp = new SpawnTiming { firstSpawnSeconds = 90f, respawnSeconds = 180f };

        [Header("Summon pit (elemental sequence -> Odin)")]
        public float firstElementalSeconds = 120f;
        public float elementalRespawnSeconds = 240f;
        public float odinDelayAfterThirdSeconds = 60f;
        public float odinRespawnSeconds = 300f;

        [Header("Divine pit (manifestation rotation)")]
        public float firstDivineSeconds = 180f;
        public float divineRespawnSeconds = 210f;

        public SpawnTiming ForTier(MonsterTier tier) => tier switch
        {
            MonsterTier.Small => smallCamp,
            MonsterTier.Medium => mediumCamp,
            MonsterTier.Large => largeCamp,
            _ => mediumCamp
        };
    }
}
