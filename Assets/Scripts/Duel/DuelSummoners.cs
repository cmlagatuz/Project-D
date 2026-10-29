// Project D | Codename: Zantetsuken — DuelSummoners.cs
// Applies the eleven summoner spells to units in the duel. Cooldowns and charges come from SummonerSpellTracker;
// numbers come from SummonerSpellCatalog, so tuning the catalog retunes the duel.
using System;
using System.Collections.Generic;
using ProjectD.SummonerSpells;

namespace ProjectD.Duel
{
    public sealed partial class DuelWorld
    {
        /// <summary>
        /// Casts the spell in a summoner slot (0 or 1). Targets are validated before any cooldown is spent, so a
        /// cast with no valid target costs nothing. Stunned units can still use Esuna, Cure and the other
        /// self spells, but not Warp, Warpstrike or Teleport.
        /// </summary>
        public CastOutcome TryCastSummoner(int unitId, int slot, DVec2 aim)
        {
            var u = Get(unitId);
            if (u == null || u.Kind != UnitKind.Champion || u.Spells == null) return CastOutcome.NoUnit;
            if (!u.Alive) return CastOutcome.Dead;

            var chosen = u.Spells.SpellIn(slot);
            if (!chosen.HasValue) return CastOutcome.EmptySlot;
            var id = chosen.Value;
            var dir = aim.Length > 0.01f ? aim.Normalized : u.Facing;

            bool needsControl = id == SummonerSpellId.Warp || id == SummonerSpellId.Warpstrike || id == SummonerSpellId.Teleport;
            if (needsControl && u.IsStunned(Time)) return CastOutcome.Stunned;

            // Warpstrike second press: dash to the marked target. Free, no new cooldown.
            if (id == SummonerSpellId.Warpstrike && u.MarkTargetId >= 0 && Time < u.MarkExpiresAt)
                return RecastWarpstrike(u);

            if (!u.Spells.IsReady(slot, Time)) return CastOutcome.OnCooldown;

            var def = SummonerSpellCatalog.Get(id);
            DuelUnit target = null;
            switch (id)
            {
                case SummonerSpellId.Slow:
                case SummonerSpellId.Bio:
                    target = Nearest(u, def.RangeUnits, t => (t.Kind == UnitKind.Champion || t.Kind == UnitKind.Dummy) && t.Team != u.Team);
                    if (target == null) return CastOutcome.NoTarget;
                    break;

                case SummonerSpellId.Ultima:
                    target = Nearest(u, def.RangeUnits, t => t.Kind == UnitKind.Monster);
                    if (target == null && u.Smite != null && u.Smite.CanTargetChampions)
                        target = Nearest(u, def.RangeUnits, t => t.Kind == UnitKind.Champion && t.Team != u.Team);
                    if (target == null) return CastOutcome.NoTarget;
                    break;

                case SummonerSpellId.Teleport:
                    if (u.Channeling) return CastOutcome.Busy;
                    if (!SummonerSpellRules.CanTeleportTo(TeleportTargets.Ward, Time)) return CastOutcome.NoTarget;
                    target = Nearest(u, 1000f, t => t.Kind == UnitKind.Ward && t.Team == u.Team);
                    if (target == null) return CastOutcome.NoTarget;
                    break;
            }

            if (u.Spells.TryCast(slot, Time, out var cast) != SummonerCastResult.Success) return CastOutcome.OnCooldown;

            switch (id)
            {
                case SummonerSpellId.Haste: ApplyHaste(u); break;
                case SummonerSpellId.Cure: ApplyCure(u, cast); break;
                case SummonerSpellId.Protect: ApplyProtect(u, cast); break;
                case SummonerSpellId.Slow: ApplySlow(u, target, cast); break;
                case SummonerSpellId.Esuna: ApplyEsuna(u, cast); break;
                case SummonerSpellId.Warp: ApplyWarp(u, cast, dir); break;
                case SummonerSpellId.Bio: ApplyBio(u, target, cast); break;
                case SummonerSpellId.Ultima: ApplyUltima(u, target); break;
                case SummonerSpellId.Teleport: ApplyTeleport(u, target, cast); break;
                case SummonerSpellId.Pray: ApplyPray(u, cast); break;
                case SummonerSpellId.Warpstrike: ApplyWarpstrike(u, cast, dir); break;
            }
            Emit(DuelEventKind.Text, u, null, 0f, cast.DisplayName);
            return CastOutcome.Ok;
        }

        // ------------------------------------------------------------------ individual spells

        private void ApplyHaste(DuelUnit u)
        {
            u.Haste.Start(Time);
        }

        private void ApplyCure(DuelUnit u, SummonerSpellDef def)
        {
            def.TryGetEffect(SpellEffectKind.MoveSpeedBuff, out var speed);

            Heal(u, HealLog.Heal(u.Id, u.Level, Time), "Cure");
            AddStatus(u, MakeStatus(StatusKind.MoveSpeedBuff, speed.Value(1), speed.Value(1), 0f, speed.DurationSeconds, u.Id));

            // Plus the most wounded nearby ally champion (none in a 1v1, but the rule is live for later modes).
            var candidates = new List<KeyValuePair<int, float>>();
            for (int i = 0; i < Units.Count; i++)
            {
                var a = Units[i];
                if (a == u || !a.Alive || a.Kind != UnitKind.Champion || a.Team != u.Team) continue;
                if (DVec2.Distance(u.Pos, a.Pos) > def.RangeUnits) continue;
                candidates.Add(new KeyValuePair<int, float>(a.Id, a.Hp / a.MaxHp));
            }
            int allyId = SummonerSpellRules.PickMostWounded(candidates);
            if (allyId >= 0)
            {
                var ally = Get(allyId);
                Heal(ally, HealLog.Heal(allyId, u.Level, Time), "Cure");
                AddStatus(ally, MakeStatus(StatusKind.MoveSpeedBuff, speed.Value(1), speed.Value(1), 0f, speed.DurationSeconds, u.Id));
            }
        }

        private void ApplyProtect(DuelUnit u, SummonerSpellDef def)
        {
            def.TryGetEffect(SpellEffectKind.Shield, out var shield);
            float amount = shield.Value(u.Level);
            AddStatus(u, MakeStatus(StatusKind.Shield, 0f, 0f, 0f, shield.DurationSeconds, u.Id, amount));
            Emit(DuelEventKind.Shield, u, u, amount, "Protect");
        }

        private void ApplySlow(DuelUnit caster, DuelUnit target, SummonerSpellDef def)
        {
            def.TryGetEffect(SpellEffectKind.MoveSpeedSlow, out var slow);
            def.TryGetEffect(SpellEffectKind.DamageDealtReduction, out var weaken);

            AddStatus(target, MakeStatus(StatusKind.MoveSpeedSlow, slow.StartMagnitude, slow.Value(1),
                slow.DecaySeconds, slow.DurationSeconds, caster.Id));
            AddStatus(target, MakeStatus(StatusKind.DamageDealtReduction, weaken.Value(1), weaken.Value(1),
                0f, weaken.DurationSeconds, caster.Id));
            Emit(DuelEventKind.Text, target, caster, 0f, "Slowed");
        }

        private void ApplyEsuna(DuelUnit u, SummonerSpellDef def)
        {
            int removed = u.Statuses.RemoveAll(s => s.Kind == StatusKind.Stun
                || s.Kind == StatusKind.MoveSpeedSlow || s.Kind == StatusKind.DamageDealtReduction);

            def.TryGetEffect(SpellEffectKind.DisableImmunity, out var immunity);
            u.Statuses.Add(MakeStatus(StatusKind.DisableImmunity, 1f, 1f, 0f, immunity.DurationSeconds, u.Id));
            Emit(DuelEventKind.Text, u, null, 0f, removed > 0 ? "Cleansed" : "Cleansed (nothing to remove)");
        }

        private void ApplyWarp(DuelUnit u, SummonerSpellDef def, DVec2 dir)
        {
            var from = u.Pos;
            u.Pos = ClampToArena(u.Pos + dir * def.RangeUnits);
            u.Facing = dir;
            if (u.Channeling) { u.Channeling = false; Emit(DuelEventKind.Text, u, null, 0f, "Teleport cancelled"); }
            EmitArea(AreaShape.Line, from, dir, DVec2.Distance(from, u.Pos), 0.6f, u);
        }

        private void ApplyBio(DuelUnit caster, DuelUnit target, SummonerSpellDef def)
        {
            def.TryGetEffect(SpellEffectKind.DamageOverTime, out var dot);
            def.TryGetEffect(SpellEffectKind.HealingReduction, out var wounds);

            float perSecond = dot.Value(caster.Level) / dot.DurationSeconds;
            AddStatus(target, MakeStatus(StatusKind.DamageOverTime, perSecond, perSecond, 0f, dot.DurationSeconds, caster.Id));
            AddStatus(target, MakeStatus(StatusKind.HealingReduction, wounds.Value(1), wounds.Value(1), 0f, wounds.DurationSeconds, caster.Id));
            Emit(DuelEventKind.Text, target, caster, 0f, "Poisoned (revealed)");
        }

        private void ApplyUltima(DuelUnit caster, DuelUnit target)
        {
            int tier = caster.Smite != null ? caster.Smite.Tier : 0;
            EmitArea(AreaShape.Circle, target.Pos, caster.Facing, 0f, 2.2f, caster);

            if (target.Kind == UnitKind.Monster)
            {
                DealDamage(caster, target, SmiteMath.DamageVs(SmiteTarget.Monster, tier), DuelDamage.True, true, "Ultima");
                return;
            }

            // Upgraded Ultima on a champion: small true damage and a speed steal.
            DealDamage(caster, target, SmiteMath.DamageVs(SmiteTarget.Champion, tier), DuelDamage.True, true, "Ultima");
            if (!target.Alive) return;
            float steal = SmiteMath.ChampionSpeedSteal;
            AddStatus(target, MakeStatus(StatusKind.MoveSpeedSlow, steal, steal, 0f, SmiteMath.ChampionSpeedStealSeconds, caster.Id));
            float gain = steal * target.BaseMoveSpeed / Math.Max(0.1f, caster.BaseMoveSpeed);
            AddStatus(caster, MakeStatus(StatusKind.MoveSpeedBuff, gain, gain, 0f, SmiteMath.ChampionSpeedStealSeconds, caster.Id));
        }

        private void ApplyTeleport(DuelUnit u, DuelUnit ward, SummonerSpellDef def)
        {
            u.Channeling = true;
            u.ChannelEndsAt = Time + def.CastTimeSeconds;
            u.ChannelDest = ClampToArena(ward.Pos + new DVec2(1.2f, 0f));
            Emit(DuelEventKind.Text, u, null, 0f, "Teleporting... (stand still)");
        }

        private void ApplyPray(DuelUnit u, SummonerSpellDef def)
        {
            def.TryGetEffect(SpellEffectKind.ResourceRestore, out var self);
            def.TryGetEffect(SpellEffectKind.ResourceRestoreAllies, out var allies);

            float before = u.Mana;
            u.Mana = Math.Min(u.MaxMana, u.Mana + u.MaxMana * self.Value(1));
            Emit(DuelEventKind.Text, u, null, u.Mana - before, "+" + (int)(u.Mana - before) + " mana");

            for (int i = 0; i < Units.Count; i++)
            {
                var a = Units[i];
                if (a == u || !a.Alive || a.Kind != UnitKind.Champion || a.Team != u.Team) continue;
                if (DVec2.Distance(u.Pos, a.Pos) > def.RangeUnits) continue;
                a.Mana = Math.Min(a.MaxMana, a.Mana + a.MaxMana * allies.Value(1));
            }
        }

        private void ApplyWarpstrike(DuelUnit u, SummonerSpellDef def, DVec2 dir)
        {
            float range = def.RangeUnits;
            var end = u.Pos + dir * range;
            EmitArea(AreaShape.Line, u.Pos, dir, range, 1.0f, u);

            DuelUnit hit = null;
            float bestAlong = float.MaxValue;
            for (int i = 0; i < Units.Count; i++)
            {
                var t = Units[i];
                if (!IsHittableBy(u, t)) continue;
                if (DistanceToSegment(t.Pos, u.Pos, end) > 0.5f + DuelUnit.Radius) continue;
                float along = DVec2.Dot(t.Pos - u.Pos, dir);
                if (along < 0f || along > range) continue;
                if (along < bestAlong) { bestAlong = along; hit = t; }
            }

            if (hit == null)
            {
                u.MarkTargetId = -1;
                Emit(DuelEventKind.Text, u, null, 0f, "Warpstrike missed");
                return;
            }
            u.MarkTargetId = hit.Id;
            u.MarkExpiresAt = Time + def.RecastWindowSeconds;
            Emit(DuelEventKind.Text, hit, u, 0f, "Marked: press again to dash");
        }

        private CastOutcome RecastWarpstrike(DuelUnit u)
        {
            var target = Get(u.MarkTargetId);
            u.MarkTargetId = -1;
            if (target == null || !target.Alive) return CastOutcome.NoTarget;

            var toward = (target.Pos - u.Pos).Normalized;
            var from = u.Pos;
            u.Pos = ClampToArena(target.Pos - toward * 1.2f);
            u.Facing = toward;
            if (u.Channeling) { u.Channeling = false; Emit(DuelEventKind.Text, u, null, 0f, "Teleport cancelled"); }
            EmitArea(AreaShape.Line, from, toward, DVec2.Distance(from, u.Pos), 0.8f, u);
            Emit(DuelEventKind.Text, u, null, 0f, "Warpstrike dash");
            return CastOutcome.Ok;
        }
    }
}
