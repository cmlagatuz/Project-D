// Project D | Codename: Zantetsuken — SummonerSpellTracker.cs
// Per-player cooldown state for the two equipped summoner spells. Deterministic: time is always passed in
// (server match seconds), never read from a clock, so it can be replayed and unit-tested.
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
        private readonly GameMode _mode;
        private float _cooldownScale = 1f;

        /// <summary>Snapshots the loadout: changing it later does not affect a running match.</summary>
        public SummonerSpellTracker(SummonerLoadout loadout, GameMode mode)
        {
            _mode = mode;
            for (int i = 0; i < SummonerLoadout.SlotCount; i++)
            {
                _spells[i] = loadout != null ? loadout.Get(i) : null;
                _fullCooldown[i] = _spells[i].HasValue ? SummonerSpellCatalog.Get(_spells[i].Value).CooldownSeconds : 0f;
            }
        }

        private static bool ValidSlot(int slot) => slot >= 0 && slot < SummonerLoadout.SlotCount;

        public SummonerSpellId? SpellIn(int slot) => ValidSlot(slot) ? _spells[slot] : null;

        /// <summary>Summoner-spell haste (same formula as ability haste): cooldown * 100 / (100 + haste).</summary>
        public void SetHaste(float hastePercent)
        {
            _cooldownScale = 100f / (100f + Math.Max(0f, hastePercent));
        }

        public float Remaining(int slot, float now)
        {
            if (!ValidSlot(slot)) return 0f;
            return Math.Max(0f, _readyAt[slot] - now);
        }

        public bool IsReady(int slot, float now)
            => ValidSlot(slot) && _spells[slot].HasValue && Remaining(slot, now) <= 0f;

        /// <summary>0 = ready, 1 = just cast. Drives the radial cooldown fill on the HUD button.</summary>
        public float Remaining01(int slot, float now)
        {
            if (!ValidSlot(slot) || _fullCooldown[slot] <= 0f) return 0f;
            return Math.Min(1f, Remaining(slot, now) / _fullCooldown[slot]);
        }

        public SummonerCastResult TryCast(int slot, float now, out SummonerSpellDef def)
        {
            def = null;
            if (!ValidSlot(slot) || !_spells[slot].HasValue) return SummonerCastResult.EmptySlot;
            var id = _spells[slot].Value;
            if (!SummonerSpellCatalog.IsAvailable(id, _mode)) return SummonerCastResult.ModeLocked;
            if (Remaining(slot, now) > 0f) return SummonerCastResult.OnCooldown;

            def = SummonerSpellCatalog.Get(id);
            _fullCooldown[slot] = def.CooldownSeconds * _cooldownScale;
            _readyAt[slot] = now + _fullCooldown[slot];
            return SummonerCastResult.Success;
        }

        /// <summary>Clears both cooldowns (Practice mode "reset cooldowns").</summary>
        public void ResetCooldowns()
        {
            for (int i = 0; i < _readyAt.Length; i++) _readyAt[i] = 0f;
        }
    }
}
