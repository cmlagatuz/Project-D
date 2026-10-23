// Project D | Codename: Zantetsuken — SurrenderVoteTests.cs (timed vote + thresholds + remake)
using NUnit.Framework;
using ProjectD.P5.Surrender;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class SurrenderVoteTests
    {
        private SurrenderVote _vote;

        [SetUp]
        public void SetUp() => _vote = new SurrenderVote();   // 5v5, earliest 900s, window 45s, cooldown 180s

        [Test]
        public void CanStart_NotBeforeTheEarliestTime()
        {
            Assert.IsFalse(_vote.CanStart(899f));
            Assert.IsTrue(_vote.CanStart(900f));
        }

        [Test]
        public void Start_CountsTheInitiatorAsYes_AndBlocksASecondVote()
        {
            Assert.IsTrue(_vote.Start("p1", 900f));
            Assert.AreEqual(1, _vote.Yes());
            Assert.IsFalse(_vote.CanStart(901f));
            Assert.IsFalse(_vote.Start("p2", 901f));
        }

        [Test]
        public void Start_BeforeEarliest_IsRejected()
        {
            Assert.IsFalse(_vote.Start("p1", 100f));
            Assert.AreEqual(0, _vote.Yes());
        }

        [Test]
        public void CastVote_WithoutAnActiveVote_IsRejected()
        {
            Assert.IsFalse(_vote.CastVote("p1", true));
        }

        [TestCase(5, 4)]
        [TestCase(4, 3)]
        [TestCase(3, 3)]
        [TestCase(2, 2)]
        public void RequiredYes_ScalesWithTeamSize(int teamSize, int expected)
        {
            _vote.teamSize = teamSize;
            Assert.AreEqual(expected, _vote.RequiredYes());
        }

        [Test]
        public void Evaluate_FourYesOfFive_Passes()
        {
            _vote.Start("p1", 900f);
            _vote.CastVote("p2", true);
            _vote.CastVote("p3", true);
            Assert.AreEqual(VoteResult.Pending, _vote.Evaluate(910f));

            _vote.CastVote("p4", true);
            Assert.AreEqual(VoteResult.Passed, _vote.Evaluate(912f));
        }

        [Test]
        public void Evaluate_WindowTimesOut_Fails()
        {
            _vote.Start("p1", 900f);
            _vote.CastVote("p2", true);

            Assert.AreEqual(VoteResult.Pending, _vote.Evaluate(944f));
            Assert.AreEqual(VoteResult.Failed, _vote.Evaluate(945f));
        }

        [Test]
        public void Evaluate_EveryoneVotedWithoutEnoughYes_FailsEarly()
        {
            _vote.Start("p1", 900f);
            _vote.CastVote("p2", false);
            _vote.CastVote("p3", false);
            _vote.CastVote("p4", false);
            _vote.CastVote("p5", false);

            Assert.AreEqual(VoteResult.Failed, _vote.Evaluate(905f));
        }

        [Test]
        public void Evaluate_WithNoActiveVote_IsPending()
        {
            Assert.AreEqual(VoteResult.Pending, _vote.Evaluate(1000f));
        }

        [Test]
        public void Cooldown_BlocksANewVoteUntilItExpires()
        {
            _vote.Start("p1", 900f);
            _vote.Evaluate(945f);   // fails, vote ends at 945

            Assert.IsFalse(_vote.CanStart(1124f));
            Assert.IsTrue(_vote.CanStart(1125f));   // 945 + 180
        }

        [Test]
        public void RemainingWindow_CountsDown_AndIsZeroWhenInactive()
        {
            Assert.AreEqual(0f, _vote.RemainingWindow(900f), 0.001f);

            _vote.Start("p1", 900f);
            Assert.AreEqual(45f, _vote.RemainingWindow(900f), 0.001f);
            Assert.AreEqual(30f, _vote.RemainingWindow(915f), 0.001f);
            Assert.AreEqual(0f, _vote.RemainingWindow(1000f), 0.001f);   // clamped, never negative
        }

        [Test]
        public void RemainingWindow_AfterTheVoteEnds_IsZero()
        {
            _vote.Start("p1", 900f);
            _vote.Evaluate(945f);
            Assert.AreEqual(0f, _vote.RemainingWindow(946f), 0.001f);
        }

        [Test]
        public void RemakeAvailable_OnlyEarly_AndOnlyIfSomeoneNeverConnected()
        {
            Assert.IsTrue(SurrenderVote.RemakeAvailable(100f, true));
            Assert.IsTrue(SurrenderVote.RemakeAvailable(195f, true));    // grace 180 + 15
            Assert.IsFalse(SurrenderVote.RemakeAvailable(196f, true));
            Assert.IsFalse(SurrenderVote.RemakeAvailable(100f, false));
        }
    }
}
