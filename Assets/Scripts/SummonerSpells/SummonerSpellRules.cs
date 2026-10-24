// Project D | Codename: Zantetsuken — SummonerSpellRules.cs
// Small, deterministic rule helpers for the spells whose behaviour needs state or a decision:
//  - HasteBuff        takedowns extend the buff and restart its speed burst
//  - CureHealTracker  halves the heal for targets healed recently
//  - Rules            most-wounded ally pick, Teleport target legality
// Time is always passed in (server match seconds). Nothing reads a clock.
using System;
using System.Collections.Generic;

namespace ProjectD.SummonerSpells
{
    /// <summary>Running state of one Haste cast.</summary>
    public sealed class HasteBuff
    {
        private readonly SpellEffect _speed;
        private readonly float _extendPerTakedown;
        private bool _active;
        private float _expiresAt;
        private float _burstStartedAt;

        public HasteBuff()
        {
            var def = SummonerSpellCatalog.Get(SummonerSpellId.Haste);
            def.TryGetEffect(SpellEffectKind.MoveSpeedBuff, out _speed);
            _extendPerTakedown = def.TakedownExtendSeconds;
        }

        public void Start(float now)
        {
            _active = true;
            _burstStartedAt = now;
            _expiresAt = now + _speed.DurationSeconds;
        }

        public bool IsActive(float now) => _active && now < _expiresAt;

        public float Remaining(float now) => IsActive(now) ? _expiresAt - now : 0f;

        /// <summary>Extends the buff (never beyond its original length) and refreshes the speed burst.</summary>
        public void OnTakedown(float now)
        {
            if (!IsActive(now)) return;
            float remaining = Math.Min(_expiresAt - now + _extendPerTakedown, _speed.DurationSeconds);
            _expiresAt = now + remaining;
            _burstStartedAt = now;
        }

        /// <summary>Bonus movement speed as a fraction (0.25 = +25%).</summary>
        public float SpeedBonus(float now) => IsActive(now) ? _speed.ValueAt(1, now - _burstStartedAt) : 0f;
    }

    /// <summary>Remembers who was healed by Cure so repeat heals can be reduced.</summary>
    public sealed class CureHealTracker
    {
        private readonly Dictionary<int, float> _lastHealedAt = new Dictionary<int, float>();

        /// <summary>Returns the heal this target should receive now, and records the heal.</summary>
        public float Heal(int targetId, int level, float now)
        {
            var def = SummonerSpellCatalog.Get(SummonerSpellId.Cure);
            if (!def.TryGetEffect(SpellEffectKind.Heal, out var heal)) return 0f;

            float amount = heal.Value(level);
            if (_lastHealedAt.TryGetValue(targetId, out float last) && now - last < def.RecentRecipientWindowSeconds)
                amount *= def.RecentRecipientMultiplier;

            _lastHealedAt[targetId] = now;
            return amount;
        }
    }

    public static class SummonerSpellRules
    {
        /// <summary>
        /// Cure's ally target: the candidate with the lowest health fraction. Ties go to the lowest id so the
        /// choice is deterministic. Returns -1 when there are no candidates. The caller excludes the caster.
        /// </summary>
        public static int PickMostWounded(IReadOnlyList<KeyValuePair<int, float>> candidates)
        {
            int bestId = -1;
            float bestFraction = float.MaxValue;
            if (candidates == null) return bestId;
            for (int i = 0; i < candidates.Count; i++)
            {
                int id = candidates[i].Key;
                float fraction = candidates[i].Value;
                if (fraction < bestFraction || (fraction == bestFraction && id < bestId))
                {
                    bestFraction = fraction;
                    bestId = id;
                }
            }
            return bestId;
        }

        /// <summary>Teleport: champions and wards any time; allied structures only in the opening minutes.</summary>
        public static bool CanTeleportTo(TeleportTargets target, float matchSeconds)
        {
            var def = SummonerSpellCatalog.Get(SummonerSpellId.Teleport);
            switch (target)
            {
                case TeleportTargets.AlliedStructure: return matchSeconds < def.StructureTeleportWindowSeconds;
                case TeleportTargets.AlliedChampion: return true;
                case TeleportTargets.Ward: return true;
                default: return false;
            }
        }
    }
}
