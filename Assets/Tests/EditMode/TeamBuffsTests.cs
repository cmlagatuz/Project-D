// Project D | Codename: Zantetsuken — TeamBuffsTests.cs (blessings, timers, bounty)
using NUnit.Framework;
using ProjectD.P5.Objectives;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class TeamBuffsTests
    {
        private TeamBuffs _buffs;

        [SetUp]
        public void SetUp() => _buffs = new TeamBuffs();

        [Test]
        public void Has_IsFalseUntilGranted()
        {
            Assert.IsFalse(_buffs.Has(Blessing.Infernal, 0f));
        }

        [Test]
        public void Grant_LastsForTheDuration_ExpiringExactlyAtTheEnd()
        {
            _buffs.Grant(Blessing.Infernal, 0f, 10f);

            Assert.IsTrue(_buffs.Has(Blessing.Infernal, 9.99f));
            Assert.IsFalse(_buffs.Has(Blessing.Infernal, 10f));
        }

        [Test]
        public void Grant_Again_RefreshesTheTimerAndAddsAStack()
        {
            _buffs.Grant(Blessing.Glacial, 0f, 10f);
            _buffs.Grant(Blessing.Glacial, 5f, 10f);

            Assert.IsTrue(_buffs.Has(Blessing.Glacial, 12f));    // would have expired at 10 without the refresh
            Assert.AreEqual(2, _buffs.Stacks(Blessing.Glacial, 12f));
        }

        [Test]
        public void Stacks_AreCappedAtMaxStacks()
        {
            for (int i = 0; i < 10; i++) _buffs.Grant(Blessing.Tempest, 0f, 100f);
            Assert.AreEqual(3, _buffs.Stacks(Blessing.Tempest, 1f));
        }

        [Test]
        public void Stacks_AreZeroOnceTheBuffHasExpired()
        {
            _buffs.Grant(Blessing.Earthen, 0f, 5f);
            Assert.AreEqual(0, _buffs.Stacks(Blessing.Earthen, 6f));
        }

        [Test]
        public void Remaining_CountsDown_ClampsToZero_AndIsZeroForUnknownBuffs()
        {
            _buffs.Grant(Blessing.Judgment, 0f, 10f);

            Assert.AreEqual(6f, _buffs.Remaining(Blessing.Judgment, 4f), 0.0001f);
            Assert.AreEqual(0f, _buffs.Remaining(Blessing.Judgment, 50f), 0.0001f);
            Assert.AreEqual(0f, _buffs.Remaining(Blessing.Tidal, 0f), 0.0001f);
        }

        [Test]
        public void Remaining_ForAPermanentBuff_IsMinusOne()
        {
            _buffs.Grant(Blessing.DivineFavor, 0f, 0f, permanent: true);

            Assert.IsTrue(_buffs.Has(Blessing.DivineFavor, 99999f));
            Assert.AreEqual(-1f, _buffs.Remaining(Blessing.DivineFavor, 99999f), 0.0001f);
        }

        [Test]
        public void Tick_RemovesExpiredBuffsButKeepsPermanentAndLiveOnes()
        {
            _buffs.Grant(Blessing.Infernal, 0f, 5f);
            _buffs.Grant(Blessing.Glacial, 0f, 50f);
            _buffs.Grant(Blessing.DivineFavor, 0f, 0f, permanent: true);

            Assert.AreEqual(2, _buffs.ActiveCount(10f));
            Assert.IsFalse(_buffs.Has(Blessing.Infernal, 10f));
            Assert.IsTrue(_buffs.Has(Blessing.Glacial, 10f));
            Assert.IsTrue(_buffs.Has(Blessing.DivineFavor, 10f));
        }

        [Test]
        public void ObjectiveBounty_ScalesWithMatchMinute_AndPaysMoreForTheBigObjectives()
        {
            Assert.AreEqual(150, ObjectiveBounty.Gold(Blessing.Infernal, 0));
            Assert.AreEqual(230, ObjectiveBounty.Gold(Blessing.Infernal, 10));
            Assert.AreEqual(300, ObjectiveBounty.Gold(Blessing.DivineFavor, 0));
            Assert.AreEqual(350, ObjectiveBounty.Gold(Blessing.ZantetsukenEdge, 0));
            Assert.AreEqual(430, ObjectiveBounty.Gold(Blessing.ZantetsukenEdge, 10));
        }
    }
}
