// Project D | Codename: Zantetsuken — DuelItemTests.cs (shop, gold, practice flags, dummies, team totals)
using NUnit.Framework;
using ProjectD.Duel;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class DuelItemTests
    {
        private DuelWorld _w;
        private DuelUnit _p;
        private DuelUnit _e;

        [SetUp]
        public void SetUp()
        {
            _w = new DuelWorld();
            _p = _w.AddChampion(DuelRoster.WarriorOfLight(), 0, new DVec2(0f, 0f), 5, null);
            _e = _w.AddChampion(DuelRoster.Garland(), 1, new DVec2(2f, 0f), 5, null);
            _p.HpRegen = 0f;
            _e.HpRegen = 0f;
        }

        [Test]
        public void Catalog_EveryRecipeComponentExists_AndFullPriceCoversItsParts()
        {
            foreach (var item in DuelItemCatalog.All)
            {
                Assert.IsFalse(string.IsNullOrEmpty(item.Name));
                if (item.BuildsFrom == null) continue;
                int parts = 0;
                foreach (var id in item.BuildsFrom)
                {
                    var c = DuelItemCatalog.Get(id);
                    Assert.IsNotNull(c, item.Id + " needs " + id);
                    parts += c.Cost;
                }
                Assert.GreaterOrEqual(item.Cost, parts - 100, item.Id + " should not be much cheaper than its parts");
            }
        }

        [Test]
        public void Buy_NeedsGold_ThenAppliesStats()
        {
            float ad = _p.Ad;
            Assert.AreEqual(ShopOutcome.NotEnoughGold, _w.BuyItem(_p.Id, "iron_edge"));
            Assert.AreEqual(ad, _p.Ad, 0.001f);

            _w.GrantGold(_p.Id, 400);
            Assert.AreEqual(ShopOutcome.Ok, _w.BuyItem(_p.Id, "iron_edge"));
            Assert.AreEqual(50, _p.Gold);
            Assert.AreEqual(ad + 12f, _p.Ad, 0.001f);
            Assert.AreEqual(1, _p.Items.Count);
        }

        [Test]
        public void Buy_UnknownItem_IsRejected()
        {
            _w.GrantGold(_p.Id, 5000);
            Assert.AreEqual(ShopOutcome.UnknownItem, _w.BuyItem(_p.Id, "no_such_item"));
            Assert.AreEqual(5000, _p.Gold);
        }

        [Test]
        public void Buy_UpgradeCreditsOwnedComponents_AndConsumesThem()
        {
            _w.GrantGold(_p.Id, 2000);
            _w.BuyItem(_p.Id, "iron_edge");
            _w.BuyItem(_p.Id, "iron_edge");
            Assert.AreEqual(1300, _p.Gold);

            var saber = DuelItemCatalog.Get("duelist_saber");
            Assert.AreEqual(300, _w.PriceFor(_p, saber));   // 1000 - 2 x 350
            Assert.AreEqual(ShopOutcome.Ok, _w.BuyItem(_p.Id, "duelist_saber"));
            Assert.AreEqual(1000, _p.Gold);
            Assert.AreEqual(1, _p.Items.Count);
            Assert.AreEqual("duelist_saber", _p.Items[0]);
        }

        [Test]
        public void Buy_FullInventory_IsRejectedUnlessTheUpgradeFreesSlots()
        {
            _w.GrantGold(_p.Id, 10000);
            for (int i = 0; i < DuelItemCatalog.MaxSlots; i++) Assert.AreEqual(ShopOutcome.Ok, _w.BuyItem(_p.Id, "iron_edge"));
            Assert.AreEqual(ShopOutcome.InventoryFull, _w.BuyItem(_p.Id, "vital_charm"));
            Assert.AreEqual(ShopOutcome.Ok, _w.BuyItem(_p.Id, "duelist_saber"));   // eats two edges, so there is room
            Assert.AreEqual(DuelItemCatalog.MaxSlots - 1, _p.Items.Count);
        }

        [Test]
        public void Sell_RefundsSeventyPercent_AndRemovesTheStats()
        {
            float baseAd = _p.Ad;
            _w.GrantGold(_p.Id, 350);
            _w.BuyItem(_p.Id, "iron_edge");
            Assert.AreEqual(ShopOutcome.Ok, _w.SellItem(_p.Id, 0));
            Assert.AreEqual((int)(350 * DuelItemCatalog.SellRatio), _p.Gold);
            Assert.AreEqual(baseAd, _p.Ad, 0.001f);
            Assert.AreEqual(ShopOutcome.EmptySlot, _w.SellItem(_p.Id, 0));
        }

        [Test]
        public void HealthItems_RaiseCurrentHealthByTheGain_NotJustTheMaximum()
        {
            _p.Hp = _p.MaxHp * 0.5f;
            float before = _p.Hp;
            _w.GrantGold(_p.Id, 300);
            _w.BuyItem(_p.Id, "vital_charm");
            Assert.AreEqual(before + 120f, _p.Hp, 0.01f);
        }

        [Test]
        public void AbilityHaste_ShortensCooldowns()
        {
            _w.GrantGold(_p.Id, 350);
            _w.BuyItem(_p.Id, "focus_band");   // 10 haste: x 100/110
            Assert.AreEqual(CastOutcome.Ok, _w.TryCastAbility(_p.Id, 0, new DVec2(1f, 0f)));
            float cd = DuelRoster.WarriorOfLight().Abilities[0].Cooldown;
            Assert.AreEqual(cd * 100f / 110f, _w.AbilityCooldownRemaining(_p, 0), 0.01f);
        }

        [Test]
        public void Invincible_BlocksDamage_ButSlayStillWorks()
        {
            _w.SetInvincible(_p.Id, true);
            float dealt = _w.DealDamage(_e, _p, 9999f, DuelDamage.True);
            Assert.AreEqual(0f, dealt, 0.001f);
            Assert.IsTrue(_p.Alive);

            _w.Slay(_p.Id, _e.Id);
            Assert.IsFalse(_p.Alive);
            Assert.AreEqual(1, _e.Kills);
            Assert.AreEqual(1, _p.Deaths);
        }

        [Test]
        public void NoCooldown_RechargesInstantly_AndCostsNoMana()
        {
            _w.SetNoCooldown(_p.Id, true);
            float mana = _p.Mana;
            Assert.AreEqual(CastOutcome.Ok, _w.TryCastAbility(_p.Id, 0, new DVec2(1f, 0f)));
            Assert.AreEqual(mana, _p.Mana, 0.001f);
            Assert.AreEqual(CastOutcome.Ok, _w.TryCastAbility(_p.Id, 0, new DVec2(1f, 0f)));

            _w.SetNoCooldown(_p.Id, false);
            Assert.AreEqual(CastOutcome.Ok, _w.TryCastAbility(_p.Id, 0, new DVec2(1f, 0f)));
            Assert.AreEqual(CastOutcome.OnCooldown, _w.TryCastAbility(_p.Id, 0, new DVec2(1f, 0f)));
        }

        [Test]
        public void ChampionKillGold_AndPassiveGold_AreOffUnlessConfigured()
        {
            _w.Slay(_e.Id, _p.Id);
            Assert.AreEqual(0, _p.Gold);
            for (int i = 0; i < 40; i++) _w.Tick(0.05f);
            Assert.AreEqual(0, _p.Gold);

            _w.ChampionKillGold = 300;
            _w.PassiveGoldPerSecond = 3f;
            _w.ForceRespawn(_e.Id);
            _w.Slay(_e.Id, _p.Id);
            Assert.AreEqual(300, _p.Gold);
            for (int i = 0; i < 20; i++) _w.Tick(0.05f);   // one second
            Assert.GreaterOrEqual(_p.Gold, 302);
        }

        [Test]
        public void Dummy_CanBeHit_HealsBack_AndIsRemovedOnClear()
        {
            var d = _w.AddDummy(1, new DVec2(1f, 0f), 1000f);
            Assert.AreEqual(CastOutcome.Ok, _w.TryBasicAttack(_p.Id, d.Id));
            Assert.Less(d.Hp, d.MaxHp);

            float hurt = d.Hp;
            for (int i = 0; i < 20; i++) _w.Tick(0.05f);
            Assert.Greater(d.Hp, hurt);

            Assert.AreEqual(1, _w.CountDummies());
            var ids = _w.RemoveDummies();
            Assert.AreEqual(1, ids.Count);
            Assert.AreEqual(0, _w.CountDummies());
            Assert.IsNull(_w.Get(d.Id));
        }

        [Test]
        public void Refresh_FillsResources_AndClearsCooldownsAndStatuses()
        {
            _w.TryCastAbility(_p.Id, 0, new DVec2(1f, 0f));
            _w.DealDamage(_e, _p, 200f, DuelDamage.True);
            _w.AddStatus(_p, _w.MakeStatus(StatusKind.Stun, 1f, 1f, 0f, 5f, _e.Id));

            _w.Refresh(_p.Id);
            Assert.AreEqual(_p.MaxHp, _p.Hp, 0.01f);
            Assert.AreEqual(_p.MaxMana, _p.Mana, 0.01f);
            Assert.AreEqual(0f, _w.AbilityCooldownRemaining(_p, 0), 0.001f);
            Assert.IsFalse(_p.IsStunned(_w.Time));
        }

        [Test]
        public void TeamTotals_SumKillsAndGold()
        {
            _w.Slay(_e.Id, _p.Id);
            _w.GrantGold(_p.Id, 250);
            Assert.AreEqual(1, _w.TeamKills(0));
            Assert.AreEqual(0, _w.TeamKills(1));
            Assert.AreEqual(250, _w.TeamGold(0));
        }
    }
}
