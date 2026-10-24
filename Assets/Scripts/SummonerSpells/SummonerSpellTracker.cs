// Project D | Codename: Zantetsuken — SummonerSpellTracker.cs
// Per-player cooldown state for the two equipped summoner spells. Deterministic: time is always passed in
// (server match seconds), never read from a clock, so it can be replayed and unit-tested.
//
// Plain spells: cast -> cooldown. Charged spells (Ultima): a short delay between casts, plus stored charges that
// refill one at a time (MaxCharges, ChargeRegenSeconds). A new match starts with every charge full.
using System;
using ProjectD.Modes;

namespace ProjectD.SummonerSpells
{
    public enum SummonerCastResult { Success, EmptySlot, OnCooldown, ModeLocked }

    public sealed class SummonerSpellTracker
    {
        private readonly SummonerSpellId?[] _spells = new SummonerSpellId?[SummonerLoadout.SlotCount];
        private readonly float[] _readyAt = new float[SummonerLoadout.SlotCount];
        private readonly float[] _fullCooldown = new float[SummonerLoadout.SlotCount];
        private readonly int[] _maxCharges = new int[SummonerLoadout.SlotCount];
        private readonly int[] _charges = new int[SummonerLoadout.SlotCount];
        private readonly float[] _regenSeconds = new float[SummonerLoadout.SlotCount];
        private readonly float[] _nextChargeAt = new float[SummonerLoadout.SlotCount];
        private readonly GameMode _mode;
        private float _cooldownScale = 1f;

        /// <summary>Snapshots the loadout: changing it later does not affect a running match.</summary>
        public SummonerSpellTracker(SummonerLoadout loadout, GameMode mode)
        {
            _mode = mode;
            for (int i = 0; i < SummonerLoadout.SlotCount; i++)
            {
                _spells[i] = loadout != null ? loadout.Get(i) : null;
                if (!_spells[i].HasValue) continue;
                var def = SummonerSpellCatalog.Get(_spells[i].Value);
                _fullCooldown[i] = def.CooldownSeconds;
                _maxCharges[i] = def.MaxCharges;
                _charges[i] = def.MaxCharges;
                _regenSeconds[i] = def.ChargeRegenSeconds;
            }
        }

        private static bool ValidSlot(int slot) => slot >= 0 && slot < SummonerLoadout.SlotCount;

        public SummonerSpellId? SpellIn(int slot) => ValidSlot(slot) ? _spells[slot] : null;

        /// <summary>Summoner-spell haste (same formula as ability haste): time * 100 / (100 + haste).</summary>
        public void SetHaste(float hastePercent)
        {
            _cooldownScale = 100f / (100f + Math.Max(0f, hastePercent));
        }

        /// <summary>Casts available right now: stored charges for a charged spell, 1 or 0 for a plain spell.</summary>
        public int Charges(int slot, float now)
        {
            if (!ValidSlot(slot) || !_spells[slot].HasValue) return 0;
            Sync(slot, now);
            if (_maxCharges[slot] <= 1) return _readyAt[slot] <= now ? 1 : 0;
            return _charges[slot];
        }

        public float Remaining(int slot, float now)
        {
            if (!ValidSlot(slot) || !_spells[slot].HasValue) return 0f;
            Sync(slot, now);
            float wait = Math.Max(0f, _readyAt[slot] - now);
            if (_charges[slot] <= 0) wait = Math.Max(wait, _nextChargeAt[slot] - now);
            return Math.Max(0f, wait);
        }

        public bool IsReady(int slot, float now)
            => ValidSlot(slot) && _spells[slot].HasValue && Remaining(slot, now) <= 0f;

        /// <summary>0 = ready, 1 = just cast. Drives the radial cooldown fill on the HUD button.</summary>
        public float Remaining01(int slot, float now)
        {
            if (!ValidSlot(slot) || !_spells[slot].HasValue) return 0f;
            Sync(slot, now);
            float full = _charges[slot] <= 0 && _maxCharges[slot] > 1 ? _regenSeconds[slot] * _cooldownScale : _fullCooldown[slot];
            if (full <= 0f) return 0f;
            return Math.Min(1f, Remaining(slot, now) / full);
        }

        public SummonerCastResult TryCast(int slot, float now, out SummonerSpellDef def)
        {
            def = null;
            if (!ValidSlot(slot) || !_spells[slot].HasValue) return SummonerCastResult.EmptySlot;
            var id = _spells[slot].Value;
            if (!SummonerSpellCatalog.IsAvailable(id, _mode)) return SummonerCastResult.ModeLocked;
            if (Remaining(slot, now) > 0f) return SummonerCastResult.OnCooldown;   // Remaining() also syncs charges

            def = SummonerSpellCatalog.Get(id);
            _fullCooldown[slot] = def.CooldownSeconds * _cooldownScale;
            _readyAt[slot] = now + _fullCooldown[slot];

            if (_maxCharges[slot] > 1)
            {
                bool wasFull = _charges[slot] >= _maxCharges[slot];
                _charges[slot]--;
                if (wasFull) _nextChargeAt[slot] = now + _regenSeconds[slot] * _cooldownScale;
            }
            return SummonerCastResult.Success;
        }

        /// <summary>Refills every charge and clears both cooldowns (Practice mode "reset cooldowns").</summary>
        public void ResetCooldowns()
        {
            for (int i = 0; i < _readyAt.Length; i++)
            {
                _readyAt[i] = 0f;
                _charges[i] = _maxCharges[i];
            }
        }

        private void Sync(int slot, float now)
        {
            int max = _maxCharges[slot];
            if (max <= 1) return;
            float step = _regenSeconds[slot] * _cooldownScale;
            if (step <= 0f) { _charges[slot] = max; return; }
            while (_charges[slot] < max && now >= _nextChargeAt[slot])
            {
                _charges[slot]++;
                if (_charges[slot] < max) _nextChargeAt[slot] += step;
            }
        }
    }
}
