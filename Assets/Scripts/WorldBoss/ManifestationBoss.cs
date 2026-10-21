// Project D  |  Codename: Zantetsuken
// ManifestationBoss.cs  (World Rift — the contestable world boss, Dissidia-NT-inspired)
//
// The centerpiece of the World Rift mode. A powerful Divine Manifestation spawns in the
// central pit mid-match. BOTH teams contest it; the team that lands the finishing blow
// CLAIMS it. On claim the Manifestation empowers that team:
//   1) a timed team-wide Manifestation Buff (stat boost), and
//   2) a one-shot Summon Strike — an AoE nuke on nearby enemies (the "call the Eidolon"
//      swing moment, as in Dissidia NT — original implementation).
// It respawns later, scaling with match time. Engine-agnostic + deterministic + tested.
//
// RIGHTS: manifestation/summon names are placeholders; content must be original/licensed.

using System;
using System.Collections.Generic;

namespace ProjectD.WorldBoss
{
    public enum BossTeam { None, Blue, Red }
    public enum BossState { Dormant, Alive, Claimed, Respawning }

    public struct ManifestationConfig
    {
        public string manifestationId;
        public float firstSpawnSeconds;   // when it first appears
        public float respawnSeconds;      // cooldown after a claim
        public float baseHp;              // HP at first spawn
        public float hpPerMinute;         // scales tankier as the game goes long
        public float buffDurationSeconds; // how long the victor buff lasts
        public float summonStrikeDamage;  // one-shot AoE nuke on claim
        public float summonStrikeRadius;  // radius of the nuke
        public static ManifestationConfig Default(string id) => new ManifestationConfig
        {
            manifestationId = id, firstSpawnSeconds = 480f, respawnSeconds = 360f,
            baseHp = 8000f, hpPerMinute = 600f, buffDurationSeconds = 45f,
            summonStrikeDamage = 900f, summonStrikeRadius = 8f
        };
    }

    public struct ManifestationBuff
    {
        public BossTeam team;
        public float expiresAt;
        public float attackDamageBonus, abilityPowerBonus, moveSpeedBonusPct;
        public bool Active(float now) => team != BossTeam.None && now < expiresAt;
    }

    public struct SummonStrike
    {
        public BossTeam caster;
        public float damage, radius;
        public float x, z, time;   // epicenter (the pit)
    }

    public sealed class ManifestationBoss
    {
        public ManifestationConfig Config;
        public BossState State { get; private set; } = BossState.Dormant;
        public float CurrentHp { get; private set; }
        public float MaxHp { get; private set; }
        private float _nextSpawnAt;
        private readonly Dictionary<BossTeam, float> _damage = new() { { BossTeam.Blue, 0 }, { BossTeam.Red, 0 } };

        public ManifestationBuff CurrentBuff { get; private set; }
        public BossTeam LastClaimedBy { get; private set; } = BossTeam.None;
        public int ClaimCount { get; private set; }

        public event Action<BossTeam, SummonStrike> OnClaimed;   // victor + the summon strike
        public event Action OnSpawned;

        public ManifestationBoss(ManifestationConfig cfg) { Config = cfg; _nextSpawnAt = cfg.firstSpawnSeconds; }

        /// <summary>Advance the match clock; spawns/respawns the boss when its timer elapses.</summary>
        public void Tick(float matchSeconds)
        {
            // Expire the victor buff.
            if (CurrentBuff.team != BossTeam.None && !CurrentBuff.Active(matchSeconds))
                CurrentBuff = default;

            if ((State == BossState.Dormant || State == BossState.Respawning) && matchSeconds >= _nextSpawnAt)
                Spawn(matchSeconds);
        }

        private void Spawn(float matchSeconds)
        {
            int minute = (int)(matchSeconds / 60f);
            MaxHp = Config.baseHp + Config.hpPerMinute * minute;
            CurrentHp = MaxHp;
            _damage[BossTeam.Blue] = 0; _damage[BossTeam.Red] = 0;
            State = BossState.Alive;
            OnSpawned?.Invoke();
        }

        /// <summary>A team deals damage. If this is the finishing blow, that team CLAIMS the boss.</summary>
        public bool DealDamage(BossTeam team, float amount, float matchSeconds, float pitX = 0, float pitZ = 0)
        {
            if (State != BossState.Alive || team == BossTeam.None || amount <= 0) return false;
            _damage[team] += amount;
            CurrentHp -= amount;
            if (CurrentHp <= 0f)
            {
                CurrentHp = 0f;
                Claim(team, matchSeconds, pitX, pitZ);
                return true;   // finishing blow
            }
            return false;
        }

        private void Claim(BossTeam team, float matchSeconds, float pitX, float pitZ)
        {
            State = BossState.Claimed;
            LastClaimedBy = team; ClaimCount++;

            // Victor buff scales slightly with each claim (comeback-friendly but bounded).
            float scale = 1f + Math.Min(0.5f, (ClaimCount - 1) * 0.15f);
            CurrentBuff = new ManifestationBuff
            {
                team = team,
                expiresAt = matchSeconds + Config.buffDurationSeconds,
                attackDamageBonus = 30f * scale,
                abilityPowerBonus = 45f * scale,
                moveSpeedBonusPct = 0.08f
            };

            var strike = new SummonStrike
            {
                caster = team, damage = Config.summonStrikeDamage, radius = Config.summonStrikeRadius,
                x = pitX, z = pitZ, time = matchSeconds
            };
            OnClaimed?.Invoke(team, strike);

            // Schedule the respawn.
            State = BossState.Respawning;
            _nextSpawnAt = matchSeconds + Config.respawnSeconds;
        }

        /// <summary>Which team currently has the most damage staked (for the contest indicator).</summary>
        public BossTeam Leading()
        {
            if (_damage[BossTeam.Blue] == _damage[BossTeam.Red]) return BossTeam.None;
            return _damage[BossTeam.Blue] > _damage[BossTeam.Red] ? BossTeam.Blue : BossTeam.Red;
        }
        public float DamageBy(BossTeam t) => _damage.TryGetValue(t, out var v) ? v : 0f;
        public float HpPercent => MaxHp <= 0 ? 0f : CurrentHp / MaxHp;
        public float SecondsUntilSpawn(float now) => State == BossState.Alive ? 0f : Math.Max(0f, _nextSpawnAt - now);

        /// <summary>Apply the summon strike to a target position; returns true if the target is in the blast.</summary>
        public static bool StrikeHits(in SummonStrike s, float targetX, float targetZ)
        {
            float dx = targetX - s.x, dz = targetZ - s.z;
            return dx * dx + dz * dz <= s.radius * s.radius;
        }
    }
}
