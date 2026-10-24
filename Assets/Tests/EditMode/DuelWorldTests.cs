// Project D | Codename: Zantetsuken — DuelWorldTests.cs (the 1v1 duel simulation + every summoner spell in play)
using System.Linq;
using NUnit.Framework;
using ProjectD.Duel;
using ProjectD.Modes;
using ProjectD.SummonerSpells;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class DuelWorldTests
    {
        private DuelWorld _w;
        private DuelUnit _p;   // player, Warrior of Light, blue, at the origin
        private DuelUnit _e;   // enemy, Garland, red, two units to the right

        private static SummonerLoadout Loadout(SummonerSpellId a, SummonerSpellId b)
        {
            var l = new SummonerLoadout();
            l.TrySet(0, a, GameMode.Practice);
            l.TrySet(1, b, GameMode.Practice);
            return l;
        }

        private void Setup(SummonerSpellId a = SummonerSpellId.Cure, SummonerSpellId b = SummonerSpellId.Warp, float enemyX = 2f)
        {
            _w = new DuelWorld();
            _p = _w.AddChampion(DuelRoster.WarriorOfLight(), 0, new DVec2(0f, 0f), 5, Loadout(a, b));
            _e = _w.AddChampion(DuelRoster.Garland(), 1, new DVec2(enemyX, 0f), 5, Loadout(SummonerSpellId.Esuna, SummonerSpellId.Cure));
            _e.HpRegen = 0f;
            _p.HpRegen = 0f;
        }

        private void Run(float seconds, float dt = 0.05f)
        {
            int steps = (int)(seconds / dt + 0.5f);
            for (int i = 0; i < steps; i++) _w.Tick(dt);
        }

        private static readonly DVec2 Right = new DVec2(1f, 0f);

        // ---------------------------------------------------------------- basic attacks

        [Test]
        public void BasicAttack_DealsMitigatedPhysicalDamage_AndHonoursRangeAndSpeed()
        {
            Setup();
            float expected = _p.Ad * (100f / (100f + _e.Armor));

            Assert.AreEqual(CastOutcome.Ok, _w.TryBasicAttack(_p.Id, _e.Id));
            Assert.AreEqual(_e.MaxHp - expected, _e.Hp, 0.01f);

            Assert.AreEqual(CastOutcome.OnCooldown, _w.TryBasicAttack(_p.Id, _e.Id));
            Run(2f);
            Assert.AreEqual(CastOutcome.Ok, _w.TryBasicAttack(_p.Id, _e.Id));

            _e.Pos = new DVec2(8f, 0f);
            Run(2f);
            Assert.AreEqual(CastOutcome.OutOfRange, _w.TryBasicAttack(_p.Id, _e.Id));
        }

        [Test]
        public void BasicAttackNearest_PicksTheClosestEnemyInRange_ElseNoTarget()
        {
            Setup();
            Assert.AreEqual(CastOutcome.Ok, _w.TryBasicAttackNearest(_p.Id));

            _e.Pos = new DVec2(9f, 0f);
            Run(2f);
            Assert.AreEqual(CastOutcome.NoTarget, _w.TryBasicAttackNearest(_p.Id));
        }

        [Test]
        public void ADeadOrStunnedUnit_CannotAttack()
        {
            Setup();
            _w.AddStatus(_p, _w.MakeStatus(StatusKind.Stun, 1f, 1f, 0f, 1f, _e.Id));
            Assert.AreEqual(CastOutcome.Stunned, _w.TryBasicAttack(_p.Id, _e.Id));

            _p.Alive = false;
            Assert.AreEqual(CastOutcome.Dead, _w.TryBasicAttack(_p.Id, _e.Id));
        }

        // ---------------------------------------------------------------- champion abilities

        [Test]
        public void Q_HitsInsideTheCone_MissesBehind_SlowsAndSpendsManaAndCooldown()
        {
            Setup();
            var behind = _w.AddMonster("Dummy", new DVec2(-2f, 0f), 1000f);
            float mana = _p.Mana;

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastAbility(_p.Id, 0, Right));

            Assert.Less(_e.Hp, _e.MaxHp);
            Assert.AreEqual(behind.MaxHp, behind.Hp, 0.001f);
            Assert.AreEqual(3.5f * (1f - 0.30f), _e.MoveSpeed(_w.Time), 0.01f);
            Assert.AreEqual(mana - 35f, _p.Mana, 0.001f);
            Assert.AreEqual(CastOutcome.OnCooldown, _w.TryCastAbility(_p.Id, 0, Right));
        }

        [Test]
        public void NoMana_BlocksACast()
        {
            Setup();
            _p.Mana = 0f;
            Assert.AreEqual(CastOutcome.NoMana, _w.TryCastAbility(_p.Id, 0, Right));
        }

        [Test]
        public void W_ShieldAbsorbsTheNextHit()
        {
            Setup();
            _w.TryCastAbility(_p.Id, 1, Right);
            Assert.Greater(_p.ShieldAmount(_w.Time), 100f);
            float hp = _p.Hp;

            _w.TryBasicAttack(_e.Id, _p.Id);

            Assert.AreEqual(hp, _p.Hp, 0.001f);
            Assert.Less(_p.ShieldAmount(_w.Time), 150f);
        }

        [Test]
        public void EnemyUltimate_StunsThePlayerInRange()
        {
            Setup();
            Assert.AreEqual(CastOutcome.Ok, _w.TryCastAbility(_e.Id, 3, new DVec2(-1f, 0f)));

            Assert.IsTrue(_p.IsStunned(_w.Time));
            Assert.AreEqual(CastOutcome.Stunned, _w.TryCastAbility(_p.Id, 0, Right));
            Run(1f);
            Assert.IsFalse(_p.IsStunned(_w.Time));
        }

        [Test]
        public void DashAbility_LandsNextToTheTarget_AndHitsIt()
        {
            Setup(enemyX: 4f);

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastAbility(_p.Id, 2, Right));

            Assert.AreEqual(3f, _p.Pos.X, 0.01f);
            Assert.Less(_e.Hp, _e.MaxHp);
        }

        // ---------------------------------------------------------------- deaths

        [Test]
        public void ADefeatedChampion_RespawnsAtFullHealthAfterTheDelay()
        {
            Setup();
            _e.Hp = 1f;
            _w.TryBasicAttack(_p.Id, _e.Id);

            Assert.IsFalse(_e.Alive);
            Assert.AreEqual(1, _p.Kills);
            Assert.AreEqual(1, _e.Deaths);

            Run(DuelWorld.ChampionRespawnSeconds + 0.2f);
            Assert.IsTrue(_e.Alive);
            Assert.AreEqual(_e.MaxHp, _e.Hp, 0.001f);
        }

        // ---------------------------------------------------------------- summoner spells

        [Test]
        public void Haste_BurstsThenSettles_AndATakedownRefreshesIt()
        {
            Setup(SummonerSpellId.Haste, SummonerSpellId.Warp);
            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));

            Assert.Greater(_p.MoveSpeed(_w.Time), 3.5f * 1.5f);
            Run(3f);
            Assert.AreEqual(3.5f * 1.25f, _p.MoveSpeed(_w.Time), 0.02f);

            Run(2f);   // 5 s in, 3 s left
            _e.Hp = 1f;
            _w.TryBasicAttack(_p.Id, _e.Id);

            Assert.AreEqual(8f, _p.Haste.Remaining(_w.Time), 0.05f);   // +6 s, capped at the original 8
        }

        [Test]
        public void Cure_HealsByLevel_SpeedsYouUp_AndRepeatHealsAreHalved()
        {
            Setup(SummonerSpellId.Cure, SummonerSpellId.Warp);
            float level5 = 110f + (400f - 110f) / 14f * 4f;

            _p.Hp = 100f;
            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.AreEqual(100f + level5, _p.Hp, 0.1f);
            Assert.AreEqual(3.5f * 1.30f, _p.MoveSpeed(_w.Time), 0.01f);

            _w.ResetCooldowns(_p.Id);
            _p.Hp = 100f;
            _w.TryCastSummoner(_p.Id, 0, Right);
            Assert.AreEqual(100f + level5 * 0.5f, _p.Hp, 0.1f);
        }

        [Test]
        public void Cure_IsWeakerWhileHealingIsReduced()
        {
            Setup(SummonerSpellId.Cure, SummonerSpellId.Warp);
            float level5 = 110f + (400f - 110f) / 14f * 4f;
            _w.AddStatus(_p, _w.MakeStatus(StatusKind.HealingReduction, 0.6f, 0.6f, 0f, 5f, _e.Id));
            _p.Hp = 100f;

            _w.TryCastSummoner(_p.Id, 0, Right);

            Assert.AreEqual(100f + level5 * 0.4f, _p.Hp, 0.1f);
        }

        [Test]
        public void Protect_GrantsAShieldThatScalesWithLevel()
        {
            Setup(SummonerSpellId.Protect, SummonerSpellId.Warp);

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));

            Assert.AreEqual(120f + (560f - 120f) / 14f * 4f, _p.ShieldAmount(_w.Time), 0.1f);
            Run(2.6f);
            Assert.AreEqual(0f, _p.ShieldAmount(_w.Time), 0.001f);
        }

        [Test]
        public void Slow_StartsAtThirtyFivePercent_FadesOut_AndCutsTheirDamage()
        {
            Setup(SummonerSpellId.Slow, SummonerSpellId.Warp);

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));

            Assert.AreEqual(3.5f * 0.65f, _e.MoveSpeed(_w.Time), 0.05f);
            Assert.AreEqual(0.60f, _e.DamageDealtMultiplier(_w.Time), 0.01f);
            Run(1.25f);
            Assert.AreEqual(3.5f * (1f - 0.175f), _e.MoveSpeed(_w.Time), 0.08f);
            Run(1.5f);
            Assert.AreEqual(3.5f, _e.MoveSpeed(_w.Time), 0.01f);
            Assert.AreEqual(1f, _e.DamageDealtMultiplier(_w.Time), 0.001f);
        }

        [Test]
        public void ASpellWithNoValidTarget_SpendsNothing()
        {
            Setup(SummonerSpellId.Slow, SummonerSpellId.Bio, enemyX: 12f);

            Assert.AreEqual(CastOutcome.NoTarget, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.AreEqual(CastOutcome.NoTarget, _w.TryCastSummoner(_p.Id, 1, Right));
            Assert.IsTrue(_p.Spells.IsReady(0, _w.Time));
            Assert.IsTrue(_p.Spells.IsReady(1, _w.Time));
        }

        [Test]
        public void Esuna_ClearsAStun_WhileStunned_ThenBlocksTheNextOneBriefly()
        {
            Setup(SummonerSpellId.Esuna, SummonerSpellId.Warp);
            _w.TryCastAbility(_e.Id, 3, new DVec2(-1f, 0f));
            Assert.IsTrue(_p.IsStunned(_w.Time));

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.IsFalse(_p.IsStunned(_w.Time));
            Assert.AreEqual(CastOutcome.Ok, _w.TryBasicAttack(_p.Id, _e.Id));

            _w.ResetCooldowns(_e.Id);
            _w.TryCastAbility(_e.Id, 3, new DVec2(-1f, 0f));
            Assert.IsFalse(_p.IsStunned(_w.Time));   // 0.25 s of disable immunity

            Run(0.4f);
            _w.ResetCooldowns(_e.Id);
            _w.TryCastAbility(_e.Id, 3, new DVec2(-1f, 0f));
            Assert.IsTrue(_p.IsStunned(_w.Time));    // the window is over
        }

        [Test]
        public void Warp_BlinksTheFullDistance_AndStaysInsideTheArena()
        {
            Setup(SummonerSpellId.Warp, SummonerSpellId.Cure);
            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.AreEqual(4.5f, _p.Pos.X, 0.01f);

            _w.ResetCooldowns(_p.Id);
            _p.Pos = new DVec2(12f, 0f);
            _w.TryCastSummoner(_p.Id, 0, Right);
            Assert.AreEqual(DuelWorld.ArenaHalf, _p.Pos.X, 0.001f);
        }

        [Test]
        public void Warp_WorksWhileRooted_ButNotWhileStunned()
        {
            Setup(SummonerSpellId.Warp, SummonerSpellId.Cure);
            _w.AddStatus(_p, _w.MakeStatus(StatusKind.Stun, 1f, 1f, 0f, 1f, _e.Id));

            Assert.AreEqual(CastOutcome.Stunned, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.IsTrue(_p.Spells.IsReady(0, _w.Time));
        }

        [Test]
        public void Bio_DealsItsTrueDamageOverFiveSeconds_AndCutsHealing()
        {
            Setup(SummonerSpellId.Bio, SummonerSpellId.Warp);
            float total = 72f + (380f - 72f) / 14f * 4f;

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.AreEqual(0.4f, _e.HealingMultiplier(_w.Time), 0.01f);

            Run(6f);

            Assert.AreEqual(total, _e.MaxHp - _e.Hp, 1.5f);
            Assert.IsTrue(_w.DrainEvents().Any(ev => ev.Kind == DuelEventKind.Damage && ev.Text == "Bio"));
        }

        [Test]
        public void Ultima_HitsAMonsterFor110PercentOf600_AndSpendsAChargeNotTheWholeSpell()
        {
            Setup(SummonerSpellId.Ultima, SummonerSpellId.Warp);
            var golem = _w.AddMonster("Golem", new DVec2(3f, 0f), 5000f);

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.AreEqual(5000f - 660f, golem.Hp, 0.5f);
            Assert.AreEqual(1, _p.Spells.Charges(0, _w.Time));

            Assert.AreEqual(CastOutcome.OnCooldown, _w.TryCastSummoner(_p.Id, 0, Right));   // 10 s between casts
            Run(10.1f);
            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
        }

        [Test]
        public void Ultima_KillingACamp_PaysHuntersResourceGold()
        {
            Setup(SummonerSpellId.Ultima, SummonerSpellId.Warp);
            var golem = _w.AddMonster("Golem", new DVec2(3f, 0f), 100f);

            _w.TryCastSummoner(_p.Id, 0, Right);

            Assert.IsFalse(golem.Alive);
            Assert.AreEqual(40, _p.Gold);
            Assert.AreEqual(1, _p.Smite.StacksConsumed);
        }

        [Test]
        public void Ultima_NeedsAMonster_UntilItIsUpgraded()
        {
            Setup(SummonerSpellId.Ultima, SummonerSpellId.Warp);

            Assert.AreEqual(CastOutcome.NoTarget, _w.TryCastSummoner(_p.Id, 0, Right));   // enemy champion in range, tier 0

            var upgraded = new SmiteProgress(-100000f);
            for (int i = 0; i < SmiteProgress.FirstUpgradeConsumed; i++) upgraded.OnCampCleared(1000f + 30f * i);
            Assert.AreEqual(1, upgraded.Tier);
            _p.Smite = upgraded;

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.AreEqual(_e.MaxHp - 40f, _e.Hp, 0.5f);
            Assert.Greater(_e.MaxValue(StatusKind.MoveSpeedSlow, _w.Time), 0.2f);
        }

        [Test]
        public void Teleport_ChannelsThenArrivesAtTheWard()
        {
            Setup(SummonerSpellId.Teleport, SummonerSpellId.Warp);
            _w.AddWard(0, new DVec2(-12f, -12f));

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.IsTrue(_p.Channeling);

            Run(3.3f);
            Assert.AreEqual(0f, _p.Pos.X, 0.001f);
            Run(0.3f);
            Assert.IsFalse(_p.Channeling);
            Assert.AreEqual(-10.8f, _p.Pos.X, 0.01f);
            Assert.AreEqual(-12f, _p.Pos.Y, 0.01f);
        }

        [Test]
        public void Teleport_IsCancelledByMovingOrByAStun()
        {
            Setup(SummonerSpellId.Teleport, SummonerSpellId.Warp);
            _w.AddWard(0, new DVec2(-12f, -12f));

            _w.TryCastSummoner(_p.Id, 0, Right);
            _w.SetMoveInput(_p.Id, Right);
            Run(0.1f);
            Assert.IsFalse(_p.Channeling);

            _w.SetMoveInput(_p.Id, DVec2.Zero);
            _w.ResetCooldowns(_p.Id);
            _w.TryCastSummoner(_p.Id, 0, Right);
            _w.AddStatus(_p, _w.MakeStatus(StatusKind.Stun, 1f, 1f, 0f, 0.5f, _e.Id));
            Assert.IsFalse(_p.Channeling);
        }

        [Test]
        public void Teleport_NeedsAWard()
        {
            Setup(SummonerSpellId.Teleport, SummonerSpellId.Warp);
            Assert.AreEqual(CastOutcome.NoTarget, _w.TryCastSummoner(_p.Id, 0, Right));
        }

        [Test]
        public void Pray_RestoresHalfTheResource()
        {
            Setup(SummonerSpellId.Pray, SummonerSpellId.Warp);
            _p.Mana = 0f;

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));

            Assert.AreEqual(_p.MaxMana * 0.5f, _p.Mana, 0.01f);
        }

        [Test]
        public void Warpstrike_MarksThenDashesOnTheSecondPress_WithoutASecondCooldown()
        {
            Setup(SummonerSpellId.Warpstrike, SummonerSpellId.Warp, enemyX: 10f);

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.AreEqual(_e.Id, _p.MarkTargetId);
            Assert.AreEqual(10f, _p.Spells.Remaining(0, _w.Time), 0.01f);

            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.AreEqual(8.8f, _p.Pos.X, 0.01f);
            Assert.AreEqual(10f, _p.Spells.Remaining(0, _w.Time), 0.01f);   // unchanged: the recast was free
        }

        [Test]
        public void Warpstrike_TheMarkExpiresAfterThreeSeconds_AndAMissMarksNothing()
        {
            Setup(SummonerSpellId.Warpstrike, SummonerSpellId.Warp, enemyX: 10f);
            _w.TryCastSummoner(_p.Id, 0, Right);
            Run(3.2f);

            Assert.AreEqual(CastOutcome.OnCooldown, _w.TryCastSummoner(_p.Id, 0, Right));   // window over, spell cooling down

            Setup(SummonerSpellId.Warpstrike, SummonerSpellId.Warp, enemyX: 10f);
            _e.Pos = new DVec2(10f, 8f);
            Assert.AreEqual(CastOutcome.Ok, _w.TryCastSummoner(_p.Id, 0, Right));
            Assert.AreEqual(-1, _p.MarkTargetId);
        }

        // ---------------------------------------------------------------- bot and determinism

        [Test]
        public void TheBot_ClosesTheDistanceAndTradesBlows()
        {
            Setup(enemyX: 9f);
            _w.AddBot(new DuelBot(_e.Id));

            Run(12f);

            Assert.Less(_p.Hp, _p.MaxHp - 50f);
        }

        [Test]
        public void ADisabledBot_StandsStill()
        {
            Setup(enemyX: 9f);
            var bot = new DuelBot(_e.Id) { Enabled = false };
            _w.AddBot(bot);

            Run(5f);

            Assert.AreEqual(9f, _e.Pos.X, 0.001f);
            Assert.AreEqual(_p.MaxHp, _p.Hp, 0.001f);
        }

        private static float Simulate()
        {
            var w = new DuelWorld();
            var p = w.AddChampion(DuelRoster.WarriorOfLight(), 0, new DVec2(0f, 0f), 5, Loadout(SummonerSpellId.Slow, SummonerSpellId.Bio));
            var e = w.AddChampion(DuelRoster.Garland(), 1, new DVec2(8f, 0f), 5, Loadout(SummonerSpellId.Esuna, SummonerSpellId.Cure));
            w.AddBot(new DuelBot(e.Id));
            for (int i = 0; i < 400; i++)
            {
                w.SetMoveInput(p.Id, new DVec2(1f, 0f));
                if (i == 20) w.TryCastSummoner(p.Id, 0, new DVec2(1f, 0f));
                if (i == 60) w.TryCastAbility(p.Id, 0, new DVec2(1f, 0f));
                w.Tick(0.05f);
            }
            return p.Hp * 1000f + e.Hp;
        }

        [Test]
        public void TheSameInputs_GiveTheSameResult()
        {
            Assert.AreEqual(Simulate(), Simulate(), 0.0001f);
        }
    }
}
