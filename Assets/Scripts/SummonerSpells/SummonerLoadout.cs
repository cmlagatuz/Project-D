// Project D | Codename: Zantetsuken — SummonerLoadout.cs
// The two summoner-spell slots a player brings into a match, plus the legality rules
// (spell must be allowed in the mode, and the two slots may not hold the same spell).
using ProjectD.Modes;

namespace ProjectD.SummonerSpells
{
    public enum LoadoutResult { Ok, BadSlot, NotAvailableInMode, DuplicateSpell }

    public sealed class SummonerLoadout
    {
        public const int SlotCount = 2;

        private readonly SummonerSpellId?[] _slots = new SummonerSpellId?[SlotCount];

        public SummonerSpellId? Get(int slot) => slot >= 0 && slot < SlotCount ? _slots[slot] : null;

        public bool IsComplete => _slots[0].HasValue && _slots[1].HasValue;

        /// <summary>The default pairing: Cure and Warp. Legal in every mode.</summary>
        public static SummonerLoadout Default()
        {
            var l = new SummonerLoadout();
            l._slots[0] = SummonerSpellId.Cure;
            l._slots[1] = SummonerSpellId.Warp;
            return l;
        }

        public LoadoutResult TrySet(int slot, SummonerSpellId id, GameMode mode)
        {
            if (slot < 0 || slot >= SlotCount) return LoadoutResult.BadSlot;
            if (!SummonerSpellCatalog.IsAvailable(id, mode)) return LoadoutResult.NotAvailableInMode;
            int other = 1 - slot;
            if (_slots[other].HasValue && _slots[other].Value == id) return LoadoutResult.DuplicateSpell;
            _slots[slot] = id;
            return LoadoutResult.Ok;
        }

        public void Clear(int slot)
        {
            if (slot >= 0 && slot < SlotCount) _slots[slot] = null;
        }

        public void Swap()
        {
            var t = _slots[0]; _slots[0] = _slots[1]; _slots[1] = t;
        }

        /// <summary>Complete, no duplicates, and every spell allowed in this mode.</summary>
        public bool IsValidFor(GameMode mode)
        {
            if (!IsComplete) return false;
            if (_slots[0].Value == _slots[1].Value) return false;
            return SummonerSpellCatalog.IsAvailable(_slots[0].Value, mode)
                && SummonerSpellCatalog.IsAvailable(_slots[1].Value, mode);
        }

        public SummonerLoadout Clone()
        {
            var c = new SummonerLoadout();
            c._slots[0] = _slots[0]; c._slots[1] = _slots[1];
            return c;
        }
    }
}
