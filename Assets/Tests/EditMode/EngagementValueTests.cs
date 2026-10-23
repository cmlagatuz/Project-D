// Project D | Codename: Zantetsuken — EngagementValueTests.cs (score, tiers, healthy-play cap)
using NUnit.Framework;
using ProjectD.P10.LTV;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class EngagementValueTests
    {
        [Test]
        public void Score_NoActivity_IsZero()
        {
            var inputs = new EngagementInputs();
            Assert.AreEqual(0, EngagementValue.Score(inputs));
        }

        [Test]
        public void Score_MaxedInputs_IsOneHundred()
        {
            var inputs = new EngagementInputs
            {
                sessionsLast30 = 40, minutesLast30 = 1200, daysActiveLast30 = 30,
                friendsPlayedWith = 10, seasonsPlayed = 6
            };
            Assert.AreEqual(100, EngagementValue.Score(inputs));
        }

        [Test]
        public void Score_InputsBeyondTheCaps_AreClamped()
        {
            var inputs = new EngagementInputs
            {
                sessionsLast30 = 4000, minutesLast30 = 999999, daysActiveLast30 = 300,
                friendsPlayedWith = 1000, seasonsPlayed = 60
            };
            Assert.AreEqual(100, EngagementValue.Score(inputs));
        }

        [Test]
        public void Score_NegativeInputs_AreClampedToZero()
        {
            var inputs = new EngagementInputs
            {
                sessionsLast30 = -5, minutesLast30 = -5, daysActiveLast30 = -5,
                friendsPlayedWith = -5, seasonsPlayed = -5
            };
            Assert.AreEqual(0, EngagementValue.Score(inputs));
        }

        [Test]
        public void Score_HalfwayOnEveryAxis_IsFifty()
        {
            var inputs = new EngagementInputs
            {
                sessionsLast30 = 20, minutesLast30 = 600, daysActiveLast30 = 15,
                friendsPlayedWith = 5, seasonsPlayed = 3
            };
            Assert.AreEqual(50, EngagementValue.Score(inputs));   // 15 + 12.5 + 12.5 + 5 + 5
        }

        [TestCase(100, "Champion")]
        [TestCase(80, "Champion")]
        [TestCase(79, "Core")]
        [TestCase(60, "Core")]
        [TestCase(59, "Casual")]
        [TestCase(35, "Casual")]
        [TestCase(34, "Returning")]
        [TestCase(15, "Returning")]
        [TestCase(14, "Lapsing")]
        [TestCase(0, "Lapsing")]
        public void Tier_BoundariesAreInclusiveAtTheLowerEdge(int score, string expected)
        {
            Assert.AreEqual(expected, EngagementValue.Tier(score));
        }

        [Test]
        public void ExceedsHealthyPlay_IsStrictlyGreaterThanTheCap()
        {
            Assert.IsFalse(EngagementValue.ExceedsHealthyPlay(360));
            Assert.IsTrue(EngagementValue.ExceedsHealthyPlay(361));
            Assert.IsTrue(EngagementValue.ExceedsHealthyPlay(121, capMinutes: 120));
            Assert.IsFalse(EngagementValue.ExceedsHealthyPlay(120, capMinutes: 120));
        }
    }
}
