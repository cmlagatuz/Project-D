// Project D | Codename: Zantetsuken — MatchLoopTests.cs (headless match runner)
using NUnit.Framework;
using ProjectD.P15.MatchLoop;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class MatchLoopTests
    {
        private static void RunToEnd(ProjectD.P15.MatchLoop.MatchLoop loop, float bluePressure, float redPressure, float dt = 1f)
        {
            loop.Start();
            int guard = 0;
            while (loop.Running && guard++ < 100000) loop.Tick(dt, bluePressure, redPressure);
        }

        [Test]
        public void Start_BeginsRunningAtClockZeroWithFullNexuses()
        {
            var loop = new ProjectD.P15.MatchLoop.MatchLoop(new MatchLoopConfig());
            loop.Start();

            Assert.IsTrue(loop.Running);
            Assert.AreEqual(0f, loop.Clock, 0.0001f);
            Assert.AreEqual(1f, loop.BlueNexus, 0.0001f);
            Assert.AreEqual(1f, loop.RedNexus, 0.0001f);
        }

        [Test]
        public void Tick_BeforeStart_DoesNothing()
        {
            var loop = new ProjectD.P15.MatchLoop.MatchLoop(new MatchLoopConfig());
            loop.Tick(1f, 10f, 10f);

            Assert.IsFalse(loop.Running);
            Assert.AreEqual(0f, loop.Clock, 0.0001f);
        }

        [Test]
        public void BluePressure_DestroysTheRedNexus_AndBlueWins()
        {
            var loop = new ProjectD.P15.MatchLoop.MatchLoop(new MatchLoopConfig());
            RunToEnd(loop, 10f, 1f);
            var result = loop.Result();

            Assert.IsFalse(loop.Running);
            Assert.AreEqual("Blue", result.winningTeam);
            Assert.IsTrue(result.localWon);
            Assert.AreEqual(MatchEndReason.NexusDestroyed, result.reason);
            Assert.That(result.durationSeconds, Is.InRange(99f, 102f));
            Assert.Greater(loop.BlueNexus, 0f);
        }

        [Test]
        public void RedPressure_DestroysTheBlueNexus_AndTheLocalBluePlayerLoses()
        {
            var loop = new ProjectD.P15.MatchLoop.MatchLoop(new MatchLoopConfig());
            RunToEnd(loop, 1f, 10f);
            var result = loop.Result();

            Assert.AreEqual("Red", result.winningTeam);
            Assert.IsFalse(result.localWon);
        }

        [Test]
        public void LocalTeam_DecidesWhetherARedVictoryCountsAsAWin()
        {
            var loop = new ProjectD.P15.MatchLoop.MatchLoop(new MatchLoopConfig { localTeam = "Red" });
            RunToEnd(loop, 1f, 10f);

            Assert.IsTrue(loop.Result().localWon);
        }

        [Test]
        public void Timeout_EndsAtMaxSeconds_AndTheTeamWithTheHealthierNexusWins()
        {
            var cfg = new MatchLoopConfig { maxSeconds = 100f };

            var blueAhead = new ProjectD.P15.MatchLoop.MatchLoop(cfg);
            RunToEnd(blueAhead, 0.5f, 0f);
            Assert.AreEqual("Blue", blueAhead.Result().winningTeam);
            Assert.AreEqual(100f, blueAhead.Result().durationSeconds, 0.001f);

            var redAhead = new ProjectD.P15.MatchLoop.MatchLoop(cfg);
            RunToEnd(redAhead, 0f, 0.5f);
            Assert.AreEqual("Red", redAhead.Result().winningTeam);
        }

        [Test]
        public void Tick_AfterTheMatchEnded_IsIgnored()
        {
            var loop = new ProjectD.P15.MatchLoop.MatchLoop(new MatchLoopConfig());
            RunToEnd(loop, 10f, 1f);
            float clock = loop.Clock;

            loop.Tick(5f, 10f, 10f);

            Assert.AreEqual(clock, loop.Clock, 0.0001f);
        }

        [Test]
        public void Surrender_EndsTheMatch_AndTheOtherTeamWins()
        {
            var loop = new ProjectD.P15.MatchLoop.MatchLoop(new MatchLoopConfig());
            loop.Start();
            loop.Surrender("Blue");
            var result = loop.Result();

            Assert.IsFalse(loop.Running);
            Assert.AreEqual("Red", result.winningTeam);
            Assert.AreEqual(MatchEndReason.Surrender, result.reason);
            Assert.IsFalse(result.localWon);
        }

        [Test]
        public void RecordCombat_AndManifestationClaims_AccumulateIntoTheResult()
        {
            var loop = new ProjectD.P15.MatchLoop.MatchLoop(new MatchLoopConfig());
            loop.Start();
            loop.RecordCombat(2, 1, 3);
            loop.RecordCombat(1, 0, 4);
            loop.RecordManifestationClaim();
            loop.RecordManifestationClaim();
            loop.Surrender("Red");
            var result = loop.Result();

            Assert.AreEqual(3, result.localKills);
            Assert.AreEqual(1, result.localDeaths);
            Assert.AreEqual(7, result.localAssists);
            Assert.AreEqual(2, result.manifestationsClaimed);
        }

        [Test]
        public void Start_ResetsEarlierStats()
        {
            var loop = new ProjectD.P15.MatchLoop.MatchLoop(new MatchLoopConfig());
            loop.Start();
            loop.RecordCombat(5, 5, 5);
            loop.RecordManifestationClaim();

            loop.Start();
            loop.Surrender("Blue");
            var result = loop.Result();

            Assert.AreEqual(0, result.localKills);
            Assert.AreEqual(0, result.manifestationsClaimed);
        }
    }
}
