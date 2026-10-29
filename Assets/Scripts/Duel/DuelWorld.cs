// Project D | Codename: Zantetsuken — DuelWorld.cs
// The 1v1 duel simulation: units, movement, basic attacks, champion abilities, damage, deaths and respawns.
// Engine-agnostic and deterministic: time only advances through Tick(dt). Summoner spells live in DuelSummoners.cs.
using System;
using System.Collections.Generic;
using ProjectD.Modes;
using ProjectD.SummonerSpells;

namespace ProjectD.Duel
{
    public sealed partial class DuelWorld
    {
        public const float ArenaHalf = 14f;
        public const float ChampionRespawnSeconds = 6f;
        public const float MonsterRespawnSeconds = 15f;

        public GameMode Mode = GameMode.Practice;
        public float Time { get; private set; }

        public readonly List<DuelUnit> Units = new List<DuelUnit>();
        public readonly List<DuelEvent> Events = new List<DuelEvent>();
        public readonly CureHealTracker HealLog = new CureHealTracker();

        private readonly List<DuelBot> _bots = new List<DuelBot>();
        private int _nextId = 1;

        // ------------------------------------------------------------------ units

        public DuelUnit Get(int id)
        {
            for (int i = 0; i < Units.Count; i++) if (Units[i].Id == id) return Units[i];
            return null;
        }

        public DuelUnit AddChampion(DuelChampionTemplate template, int team, DVec2 pos, int level, SummonerLoadout loadout)
        {
            var u = new DuelUnit
            {
                Id = _nextId++, Name = template.Name, Kind = UnitKind.Champion, Team = team,
                Template = template, ModelId = template.Id, Pos = pos, SpawnPos = pos, Level = Math.Max(1, level)
            };
            u.Loadout = loadout ?? SummonerLoadout.Default();
            ApplyLevelStats(u, true);
            u.RebuildSpells(Mode);
            u.Smite = new SmiteProgress(-300f);   // demo: start with stored Hunter's Resource stacks
            Units.Add(u);
            return u;
        }

        public DuelUnit AddMonster(string name, DVec2 pos, float hp)
        {
            var u = new DuelUnit
            {
                Id = _nextId++, Name = name, Kind = UnitKind.Monster, Team = 2, Pos = pos, SpawnPos = pos,
                MaxHp = hp, Hp = hp, Armor = 20f, Mr = 20f
            };
            Units.Add(u);
            return u;
        }

        public DuelUnit AddWard(int team, DVec2 pos)
        {
            var u = new DuelUnit
            {
                Id = _nextId++, Name = "Ward", Kind = UnitKind.Ward, Team = team, Pos = pos, SpawnPos = pos,
                MaxHp = 1f, Hp = 1f
            };
            Units.Add(u);
            return u;
        }

        public void AddBot(DuelBot bot) => _bots.Add(bot);

        public void SetLevel(int unitId, int level)
        {
            var u = Get(unitId);
            if (u == null || u.Kind != UnitKind.Champion) return;
            u.Level = Math.Max(1, Math.Min(SummonerSpellCatalog.MaxLevel, level));
            ApplyLevelStats(u, false);
        }

        private void ApplyLevelStats(DuelUnit u, bool fill)
        {
            var t = u.Template;
            int g = u.Level - 1;
            float frac = u.MaxHp > 0f ? u.Hp / u.MaxHp : 1f;
            var it = DuelItemCatalog.StatsOf(u.Items);
            u.MaxHp = t.Hp + t.HpGrow * g + it.Hp;
            u.MaxMana = t.Mana + it.Mana;
            u.Ad = t.Ad + t.AdGrow * g + it.Ad;
            u.Ap = t.Ap + it.Ap;
            u.Armor = t.Armor + t.ArmorGrow * g + it.Armor;
            u.Mr = t.Mr + t.MrGrow * g + it.Mr;
            u.BaseMoveSpeed = t.MoveSpeed * (1f + it.MoveSpeedPct);
            u.AttackRange = t.AttackRange;
            u.AttackSpeed = t.AttackSpeed * (1f + it.AttackSpeedPct);
            u.AbilityHaste = it.AbilityHaste;
            if (fill) { u.Hp = u.MaxHp; u.Mana = u.MaxMana; }
            else u.Hp = Math.Max(1f, u.MaxHp * frac);
        }

        public void SetLoadout(int unitId, SummonerLoadout loadout)
        {
            var u = Get(unitId);
            if (u == null) return;
            u.Loadout = loadout;
            u.RebuildSpells(Mode);
        }

        public void ResetCooldowns(int unitId)
        {
            var u = Get(unitId);
            if (u == null) return;
            u.Spells?.ResetCooldowns();
            for (int i = 0; i < u.AbilityReadyAt.Length; i++) u.AbilityReadyAt[i] = 0f;
            u.NextAttackAt = 0f;
            u.MarkTargetId = -1;
        }

        public void ForceRespawn(int unitId)
        {
            var u = Get(unitId);
            if (u != null) Respawn(u);
        }

        // ------------------------------------------------------------------ queries

        public static bool IsHittableBy(DuelUnit attacker, DuelUnit t)
            => t != null && t.Alive && t.Kind != UnitKind.Ward && t.Team != attacker.Team;

        public DuelUnit Nearest(DuelUnit from, float maxDistance, Func<DuelUnit, bool> predicate)
        {
            DuelUnit best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < Units.Count; i++)
            {
                var u = Units[i];
                if (u == from || !u.Alive || !predicate(u)) continue;
                float d = DVec2.Distance(from.Pos, u.Pos);
                if (d > maxDistance + DuelUnit.Radius) continue;
                if (d < bestDist || (d == bestDist && best != null && u.Id < best.Id)) { best = u; bestDist = d; }
            }
            return best;
        }

        public float AbilityCooldownRemaining(DuelUnit u, int slot) => Math.Max(0f, u.AbilityReadyAt[slot] - Time);

        public int AbilityRank(DuelUnit u, DuelAbility ab)
        {
            int ranks = Math.Max(1, ab.Damage.Length);
            return Math.Max(1, Math.Min(ranks, 1 + (u.Level - 1) / 3));
        }

        public float AbilityDamage(DuelUnit u, DuelAbility ab)
        {
            if (ab.Damage.Length == 0) return 0f;
            float baseDamage = ab.Damage[AbilityRank(u, ab) - 1];
            float scale = ab.Type == DuelDamage.Magical ? u.Ap : u.Ad;
            return baseDamage + ab.Ratio * scale;
        }

        // ------------------------------------------------------------------ events

        public DuelEvent Emit(DuelEventKind kind, DuelUnit unit, DuelUnit other, float amount, string text)
        {
            var e = new DuelEvent
            {
                Time = Time, Kind = kind, UnitId = unit != null ? unit.Id : -1, OtherId = other != null ? other.Id : -1,
                Amount = amount, Text = text
            };
            Events.Add(e);
            return e;
        }

        private void EmitArea(AreaShape shape, DVec2 pos, DVec2 dir, float length, float width, DuelUnit caster)
        {
            Events.Add(new DuelEvent
            {
                Time = Time, Kind = DuelEventKind.Area, UnitId = caster != null ? caster.Id : -1, OtherId = -1,
                Shape = shape, Pos = pos, Dir = dir, Length = length, Width = width
            });
        }

        /// <summary>Hands the view everything that happened since the last call.</summary>
        public List<DuelEvent> DrainEvents()
        {
            var copy = new List<DuelEvent>(Events);
            Events.Clear();
            return copy;
        }

        // ------------------------------------------------------------------ tick

        public void SetMoveInput(int unitId, DVec2 dir)
        {
            var u = Get(unitId);
            if (u != null) u.MoveInput = dir;
        }

        public void Tick(float dt)
        {
            Time += dt;
            for (int i = 0; i < _bots.Count; i++) _bots[i].Think(this);

            for (int i = 0; i < Units.Count; i++)
            {
                var u = Units[i];
                if (!u.Alive)
                {
                    if (u.Kind != UnitKind.Ward && Time >= u.RespawnAt) Respawn(u);
                    continue;
                }

                u.Statuses.RemoveAll(s => !s.IsActive(Time) || (s.Kind == StatusKind.Shield && s.Amount <= 0f));

                if (u.Channeling && Time >= u.ChannelEndsAt)
                {
                    u.Channeling = false;
                    u.Pos = u.ChannelDest;
                    Emit(DuelEventKind.Text, u, null, 0f, "Teleported");
                }

                if (u.Kind == UnitKind.Champion)
                {
                    u.Hp = Math.Min(u.MaxHp, u.Hp + u.HpRegen * u.HealingMultiplier(Time) * dt);
                    u.Mana = Math.Min(u.MaxMana, u.Mana + u.ManaRegen * dt);
                    if (PassiveGoldPerSecond > 0f)
                    {
                        u.GoldCarry += PassiveGoldPerSecond * dt;
                        int whole = (int)u.GoldCarry;
                        if (whole > 0) { u.Gold += whole; u.GoldCarry -= whole; }
                    }
                    if (u.NoCooldown)
                    {
                        u.Spells?.ResetCooldowns();
                        u.Mana = u.MaxMana;
                    }
                    TickDamageOverTime(u, dt);
                    if (!u.Alive) continue;
                    Move(u, dt);
                }
                else if (u.Kind == UnitKind.Dummy)
                {
                    u.Hp = Math.Min(u.MaxHp, u.Hp + u.MaxHp * 0.04f * dt);   // a dummy heals itself so it can be hit all day
                    TickDamageOverTime(u, dt);
                }
            }
        }

        private void TickDamageOverTime(DuelUnit u, float dt)
        {
            for (int i = 0; i < u.Statuses.Count && u.Alive; i++)
            {
                var s = u.Statuses[i];
                if (s.Kind != StatusKind.DamageOverTime || !s.IsActive(Time)) continue;

                var src = Get(s.SourceId);
                float dealt = DealDamage(src, u, s.StartValue * dt, DuelDamage.True, false);
                s.Amount += dealt;

                float prev = Time - dt;
                if (Math.Floor(Time - s.StartedAt) > Math.Floor(prev - s.StartedAt) && s.Amount > 0f)
                {
                    Emit(DuelEventKind.Damage, u, src, s.Amount, "Bio");
                    s.Amount = 0f;
                }
            }
        }

        private void Move(DuelUnit u, float dt)
        {
            if (u.IsStunned(Time)) return;
            var dir = u.MoveInput;
            float mag = dir.Length;
            if (mag < 0.1f) return;
            if (u.Channeling) { u.Channeling = false; Emit(DuelEventKind.Text, u, null, 0f, "Teleport cancelled"); }

            var step = mag > 1f ? dir.Normalized : dir;
            u.Pos = ClampToArena(u.Pos + step * (u.MoveSpeed(Time) * dt));
            u.Facing = dir.Normalized;
        }

        public static DVec2 ClampToArena(DVec2 p)
        {
            float x = Math.Max(-ArenaHalf, Math.Min(ArenaHalf, p.X));
            float y = Math.Max(-ArenaHalf, Math.Min(ArenaHalf, p.Y));
            return new DVec2(x, y);
        }

        private void Respawn(DuelUnit u)
        {
            u.Alive = true;
            u.Hp = u.MaxHp;
            u.Mana = u.MaxMana;
            u.Pos = u.SpawnPos;
            u.Statuses.Clear();
            u.Channeling = false;
            u.MarkTargetId = -1;
            Emit(DuelEventKind.Respawn, u, null, 0f, "Respawned");
        }

        // ------------------------------------------------------------------ damage, healing, statuses

        public float DealDamage(DuelUnit src, DuelUnit dst, float raw, DuelDamage type, bool emit = true, string label = null)
        {
            if (dst == null || !dst.Alive || dst.Kind == UnitKind.Ward || raw <= 0f) return 0f;
            if (dst.Invincible)
            {
                if (emit) Emit(DuelEventKind.Text, dst, src, 0f, "Invincible");
                return 0f;
            }

            float dmg = raw * (src != null ? src.DamageDealtMultiplier(Time) : 1f);
            if (type == DuelDamage.Physical) dmg = ProjectD.Gameplay.Combat.DamageSystem.Mitigate(dmg, dst.Armor);
            else if (type == DuelDamage.Magical) dmg = ProjectD.Gameplay.Combat.DamageSystem.Mitigate(dmg, dst.Mr);

            float absorbed = 0f;
            for (int i = 0; i < dst.Statuses.Count && dmg > 0f; i++)
            {
                var s = dst.Statuses[i];
                if (s.Kind != StatusKind.Shield || !s.IsActive(Time) || s.Amount <= 0f) continue;
                float take = Math.Min(s.Amount, dmg);
                s.Amount -= take;
                dmg -= take;
                absorbed += take;
            }

            dst.Hp -= dmg;
            if (emit)
            {
                if (dmg > 0f) Emit(DuelEventKind.Damage, dst, src, dmg, label);
                else if (absorbed > 0f) Emit(DuelEventKind.Text, dst, src, 0f, "Blocked");
            }
            if (dst.Hp <= 0f) Kill(src, dst);
            return dmg;
        }

        private void Kill(DuelUnit src, DuelUnit dst)
        {
            dst.Hp = 0f;
            dst.Alive = false;
            dst.Channeling = false;
            dst.MoveInput = DVec2.Zero;
            dst.RespawnAt = Time + (dst.Kind == UnitKind.Champion ? ChampionRespawnSeconds : MonsterRespawnSeconds);
            Emit(DuelEventKind.Kill, dst, src, 0f, dst.Name + " defeated");

            if (dst.Kind == UnitKind.Champion) dst.Deaths++;
            if (src == null) return;

            if (dst.Kind == UnitKind.Champion)
            {
                src.Kills++;
                src.Haste.OnTakedown(Time);   // Haste: a takedown extends the buff
                if (src.Kind == UnitKind.Champion && ChampionKillGold > 0)
                {
                    src.Gold += ChampionKillGold;
                    Emit(DuelEventKind.Gold, src, null, ChampionKillGold, "+" + ChampionKillGold + " gold (takedown)");
                }
            }
            else if (dst.Kind == UnitKind.Monster && src.Smite != null)
            {
                float gold = src.Smite.OnCampCleared(Time);   // Hunter's Resource
                if (gold > 0f)
                {
                    src.Gold += (int)gold;
                    Emit(DuelEventKind.Gold, src, null, gold, "+" + (int)gold + " gold (Hunter's Resource)");
                }
            }
        }

        public float Heal(DuelUnit u, float amount, string label = null)
        {
            if (u == null || !u.Alive) return 0f;
            float healed = Math.Max(0f, amount * u.HealingMultiplier(Time));
            u.Hp = Math.Min(u.MaxHp, u.Hp + healed);
            Emit(DuelEventKind.Heal, u, null, healed, label);
            return healed;
        }

        /// <summary>Adds a status. Stuns and slows are ignored while the target has disable immunity.</summary>
        public bool AddStatus(DuelUnit target, Status s)
        {
            if (target == null || !target.Alive) return false;
            bool isDisable = s.Kind == StatusKind.Stun || s.Kind == StatusKind.MoveSpeedSlow;
            if (isDisable && target.IsActive(StatusKind.DisableImmunity, Time))
            {
                Emit(DuelEventKind.Text, target, null, 0f, "Immune");
                return false;
            }
            if (s.Kind == StatusKind.Stun && target.Channeling)
            {
                target.Channeling = false;
                Emit(DuelEventKind.Text, target, null, 0f, "Teleport interrupted");
            }
            target.Statuses.Add(s);
            return true;
        }

        public Status MakeStatus(StatusKind kind, float startValue, float endValue, float decaySeconds, float seconds, int sourceId, float amount = 0f)
        {
            return new Status
            {
                Kind = kind, StartValue = startValue, EndValue = endValue, DecaySeconds = decaySeconds,
                StartedAt = Time, ExpiresAt = Time + seconds, SourceId = sourceId, Amount = amount
            };
        }

        // ------------------------------------------------------------------ basic attack

        public CastOutcome TryBasicAttack(int attackerId, int targetId)
        {
            var a = Get(attackerId);
            if (a == null || a.Kind != UnitKind.Champion) return CastOutcome.NoUnit;
            if (!a.Alive) return CastOutcome.Dead;
            if (a.IsStunned(Time)) return CastOutcome.Stunned;

            var t = Get(targetId);
            if (!IsHittableBy(a, t)) return CastOutcome.NoTarget;
            if (DVec2.Distance(a.Pos, t.Pos) > a.AttackRange + DuelUnit.Radius) return CastOutcome.OutOfRange;
            if (Time < a.NextAttackAt) return CastOutcome.OnCooldown;

            a.NextAttackAt = Time + 1f / Math.Max(0.1f, a.AttackSpeed);
            a.Facing = (t.Pos - a.Pos).Normalized;
            if (a.Channeling) { a.Channeling = false; Emit(DuelEventKind.Text, a, null, 0f, "Teleport cancelled"); }
            Emit(DuelEventKind.Attack, a, t, 0f, null);   // visual cue only (attack animation)
            DealDamage(a, t, a.Ad, DuelDamage.Physical);
            return CastOutcome.Ok;
        }

        public CastOutcome TryBasicAttackNearest(int attackerId)
        {
            var a = Get(attackerId);
            if (a == null || a.Kind != UnitKind.Champion) return CastOutcome.NoUnit;
            var t = Nearest(a, a.AttackRange, u => IsHittableBy(a, u));
            return t == null ? CastOutcome.NoTarget : TryBasicAttack(attackerId, t.Id);
        }

        // ------------------------------------------------------------------ champion abilities (Q W E R = slots 0..3)

        public CastOutcome TryCastAbility(int unitId, int slot, DVec2 aim)
        {
            var u = Get(unitId);
            if (u == null || u.Kind != UnitKind.Champion) return CastOutcome.NoUnit;
            if (!u.Alive) return CastOutcome.Dead;
            if (slot < 0 || slot >= u.Template.Abilities.Length || u.Template.Abilities[slot] == null) return CastOutcome.EmptySlot;
            if (u.IsStunned(Time)) return CastOutcome.Stunned;

            var ab = u.Template.Abilities[slot];
            if (Time < u.AbilityReadyAt[slot]) return CastOutcome.OnCooldown;
            bool free = u.NoCooldown;   // practice tool: no cooldown, no mana cost
            if (!free && u.Mana < ab.Cost) return CastOutcome.NoMana;

            var dir = aim.Length > 0.01f ? aim.Normalized : u.Facing;
            if (!free) u.Mana -= ab.Cost;
            u.AbilityReadyAt[slot] = free ? Time : Time + ab.Cooldown * (100f / (100f + Math.Max(0f, u.AbilityHaste)));
            u.Facing = dir;
            if (u.Channeling) { u.Channeling = false; Emit(DuelEventKind.Text, u, null, 0f, "Teleport cancelled"); }

            float dmg = AbilityDamage(u, ab);
            switch (ab.Shape)
            {
                case AbilityShape.Cone: CastCone(u, ab, dir, dmg); break;
                case AbilityShape.Line: CastLine(u, ab, dir, dmg); break;
                case AbilityShape.Circle: CastCircle(u, ab, u.Pos, dmg); break;
                case AbilityShape.GroundCircle: CastGroundCircle(u, ab, dir, dmg); break;
                case AbilityShape.Shield: CastShield(u, ab, dmg); break;
                case AbilityShape.Dash: CastDash(u, ab, dir, dmg); break;
            }
            Emit(DuelEventKind.Text, u, null, 0f, ab.Name);
            return CastOutcome.Ok;
        }

        private void HitTarget(DuelUnit caster, DuelUnit t, DuelAbility ab, float dmg)
        {
            float dealt = DealDamage(caster, t, dmg, ab.Type);
            if (ab.LifestealPct > 0f && dealt > 0f) Heal(caster, dealt * ab.LifestealPct, ab.Name);
            if (!t.Alive) return;
            if (ab.SlowPct > 0f)
                AddStatus(t, MakeStatus(StatusKind.MoveSpeedSlow, ab.SlowPct, ab.SlowPct, 0f, ab.SlowSeconds, caster.Id));
            if (ab.StunSeconds > 0f)
                AddStatus(t, MakeStatus(StatusKind.Stun, 1f, 1f, 0f, ab.StunSeconds, caster.Id));
        }

        private void CastCone(DuelUnit u, DuelAbility ab, DVec2 dir, float dmg)
        {
            EmitArea(AreaShape.Line, u.Pos, dir, ab.Range, ab.Range * 1.2f, u);
            float halfAngle = ab.AngleDegrees * 0.5f * (float)Math.PI / 180f;
            float minDot = (float)Math.Cos(halfAngle);
            for (int i = 0; i < Units.Count; i++)
            {
                var t = Units[i];
                if (!IsHittableBy(u, t)) continue;
                var to = t.Pos - u.Pos;
                float d = to.Length;
                if (d > ab.Range + DuelUnit.Radius) continue;
                if (d > 0.05f && DVec2.Dot(dir, to.Normalized) < minDot) continue;
                HitTarget(u, t, ab, dmg);
            }
        }

        private static float DistanceToSegment(DVec2 p, DVec2 a, DVec2 b)
        {
            var ab = b - a;
            float len2 = DVec2.Dot(ab, ab);
            if (len2 < 1e-6f) return DVec2.Distance(p, a);
            float t = Math.Max(0f, Math.Min(1f, DVec2.Dot(p - a, ab) / len2));
            return DVec2.Distance(p, a + ab * t);
        }

        private void CastLine(DuelUnit u, DuelAbility ab, DVec2 dir, float dmg)
        {
            var end = u.Pos + dir * ab.Range;
            EmitArea(AreaShape.Line, u.Pos, dir, ab.Range, ab.Radius * 2f, u);
            for (int i = 0; i < Units.Count; i++)
            {
                var t = Units[i];
                if (!IsHittableBy(u, t)) continue;
                if (DistanceToSegment(t.Pos, u.Pos, end) <= ab.Radius + DuelUnit.Radius) HitTarget(u, t, ab, dmg);
            }
        }

        private void CastCircle(DuelUnit u, DuelAbility ab, DVec2 center, float dmg)
        {
            EmitArea(AreaShape.Circle, center, u.Facing, 0f, ab.Radius * 2f, u);
            for (int i = 0; i < Units.Count; i++)
            {
                var t = Units[i];
                if (!IsHittableBy(u, t)) continue;
                if (DVec2.Distance(center, t.Pos) <= ab.Radius + DuelUnit.Radius) HitTarget(u, t, ab, dmg);
            }
        }

        private void CastGroundCircle(DuelUnit u, DuelAbility ab, DVec2 dir, float dmg)
        {
            var center = ClampToArena(u.Pos + dir * ab.Range);
            DuelUnit snap = null;
            float best = float.MaxValue;
            for (int i = 0; i < Units.Count; i++)
            {
                var t = Units[i];
                if (!IsHittableBy(u, t)) continue;
                var to = t.Pos - u.Pos;
                float d = to.Length;
                if (d > ab.Range) continue;
                if (d > 0.05f && DVec2.Dot(dir, to.Normalized) < 0.8f) continue;
                if (d < best) { best = d; snap = t; }
            }
            if (snap != null) center = snap.Pos;
            CastCircle(u, ab, center, dmg);
        }

        private void CastShield(DuelUnit u, DuelAbility ab, float dmg)
        {
            if (ab.Cleanse)
                u.Statuses.RemoveAll(s => s.Kind == StatusKind.Stun || s.Kind == StatusKind.MoveSpeedSlow || s.Kind == StatusKind.DamageDealtReduction);
            if (ab.ShieldAmount > 0f)
            {
                AddStatus(u, MakeStatus(StatusKind.Shield, 0f, 0f, 0f, ab.ShieldSeconds, u.Id, ab.ShieldAmount));
                Emit(DuelEventKind.Shield, u, u, ab.ShieldAmount, null);
            }
            if (ab.Radius > 0f && dmg > 0f) CastCircle(u, ab, u.Pos, dmg);
        }

        private void CastDash(DuelUnit u, DuelAbility ab, DVec2 dir, float dmg)
        {
            // Dash to the nearest enemy in front (landing just short of it), otherwise a full dash along the aim.
            DuelUnit target = null;
            float best = float.MaxValue;
            for (int i = 0; i < Units.Count; i++)
            {
                var t = Units[i];
                if (!IsHittableBy(u, t)) continue;
                var to = t.Pos - u.Pos;
                float d = to.Length;
                if (d > ab.Range || d < 0.05f || DVec2.Dot(dir, to.Normalized) < 0.5f) continue;
                if (d < best) { best = d; target = t; }
            }

            DVec2 dest;
            if (target != null)
            {
                var toward = (target.Pos - u.Pos).Normalized;
                dest = target.Pos - toward * 1.0f;
            }
            else dest = u.Pos + dir * ab.Range;

            EmitArea(AreaShape.Line, u.Pos, dir, DVec2.Distance(u.Pos, dest), 0.8f, u);
            u.Pos = ClampToArena(dest);
            if (dmg > 0f || ab.StunSeconds > 0f) CastCircle(u, ab, u.Pos, dmg);
        }
    }
}
