// Project D | Codename: Zantetsuken — DuelBot.cs
// A simple scripted opponent for the duel demo: walks up, trades basic attacks, uses its abilities and its
// summoner spells sensibly (so your own spells have something to react to). Not the production bot brain.
using ProjectD.SummonerSpells;

namespace ProjectD.Duel
{
    public sealed class DuelBot
    {
        private readonly int _unitId;
        private float _nextThink;

        /// <summary>When false the bot stands still (handy for testing spells on a target).</summary>
        public bool Enabled = true;

        public DuelBot(int unitId) { _unitId = unitId; }

        public void Think(DuelWorld w)
        {
            var me = w.Get(_unitId);
            if (me == null || !me.Alive) return;
            if (!Enabled) { me.MoveInput = DVec2.Zero; return; }
            if (w.Time < _nextThink) return;
            _nextThink = w.Time + 0.15f;

            var foe = w.Nearest(me, 999f, t => t.Kind == UnitKind.Champion && t.Team != me.Team);
            if (foe == null) { me.MoveInput = DVec2.Zero; return; }

            float dist = DVec2.Distance(me.Pos, foe.Pos);
            var toFoe = (foe.Pos - me.Pos).Normalized;
            float hpFrac = me.Hp / me.MaxHp;
            bool stunned = me.IsStunned(w.Time);

            DecideSummoners(w, me, foe, dist, toFoe, hpFrac, stunned);
            if (stunned) { me.MoveInput = DVec2.Zero; return; }

            me.MoveInput = dist > me.AttackRange * 0.9f ? toFoe : DVec2.Zero;

            var q = me.Template.Abilities[0];
            var dash = me.Template.Abilities[1];
            var chain = me.Template.Abilities[2];
            var ult = me.Template.Abilities[3];

            if (dist <= ult.Radius - 0.3f && w.AbilityCooldownRemaining(me, 3) <= 0f) w.TryCastAbility(me.Id, 3, toFoe);
            else if (dist <= q.Range && w.AbilityCooldownRemaining(me, 0) <= 0f) w.TryCastAbility(me.Id, 0, toFoe);
            else if (dist <= chain.Range && dist > q.Range && w.AbilityCooldownRemaining(me, 2) <= 0f) w.TryCastAbility(me.Id, 2, toFoe);
            else if (dist > 3f && dist <= dash.Range && w.AbilityCooldownRemaining(me, 1) <= 0f) w.TryCastAbility(me.Id, 1, toFoe);

            w.TryBasicAttack(me.Id, foe.Id);
        }

        private void DecideSummoners(DuelWorld w, DuelUnit me, DuelUnit foe, float dist, DVec2 toFoe, float hpFrac, bool stunned)
        {
            for (int slot = 0; slot < SummonerLoadout.SlotCount; slot++)
            {
                var spell = me.Spells.SpellIn(slot);
                if (!spell.HasValue || !me.Spells.IsReady(slot, w.Time)) continue;

                bool cast = false;
                var aim = toFoe;
                switch (spell.Value)
                {
                    case SummonerSpellId.Cure: cast = hpFrac < 0.4f; break;
                    case SummonerSpellId.Esuna: cast = stunned || me.MaxValue(StatusKind.MoveSpeedSlow, w.Time) > 0.2f; break;
                    case SummonerSpellId.Protect: cast = hpFrac < 0.5f && dist < 4f; break;
                    case SummonerSpellId.Haste: cast = dist > 8f; break;
                    case SummonerSpellId.Slow: cast = dist <= 5f; break;
                    case SummonerSpellId.Bio: cast = dist <= 4.5f && foe.Hp / foe.MaxHp < 0.6f; break;
                    case SummonerSpellId.Warp: cast = hpFrac < 0.25f && dist < 3f; aim = toFoe * -1f; break;
                }
                if (cast) w.TryCastSummoner(me.Id, slot, aim);
            }
        }
    }
}
