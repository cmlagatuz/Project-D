// Project D | Codename: Zantetsuken — ProgressionGlueTests.cs (post-match XP / level / KP rewards)
using NUnit.Framework;
using ProjectD.P15.MatchLoop;
using ProjectD.P15.Progression;
using ProjectD.P15.Save;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class ProgressionGlueTests
    {
        private static MatchResult Win(float seconds = 0f, int manifestations = 0)
            => new MatchResult { localWon = true, winningTeam = "Blue", durationSeconds = seconds, manifestationsClaimed = manifestations };

        private static MatchResult Loss(float seconds = 0f)
            => new MatchResult { localWon = false, winningTeam = "Red", durationSeconds = seconds };

        // ---- tables ---- //

        [Test]
        public void XpToNext_GrowsLinearlyAndNeverDropsBelowLevelOne()
        {
            Assert.AreEqual(600, ProgressionGlue.XpToNext(1));
            Assert.AreEqual(720, ProgressionGlue.XpToNext(2));
            Assert.AreEqual(600, ProgressionGlue.XpToNext(0));
            Assert.AreEqual(600, ProgressionGlue.XpToNext(-5));
        }

        [Test]
        public void KpForLevel_HasMilestoneBonusesEveryTenthLevel()
        {
            Assert.AreEqual(0, ProgressionGlue.KpForLevel(1));
            Assert.AreEqual(60, ProgressionGlue.KpForLevel(2));
            Assert.AreEqual(60, ProgressionGlue.KpForLevel(9));
            Assert.AreEqual(310, ProgressionGlue.KpForLevel(10));
            Assert.AreEqual(310, ProgressionGlue.KpForLevel(20));
        }

        // ---- XP ---- //

        [Test]
        public void Win_AwardsBaseXpPlusPerMinuteXp()
        {
            var profile = new PlayerProfileSave { playerId = "p" };

            var r = ProgressionGlue.Apply(profile, Win(seconds: 600f), false, false);

            Assert.AreEqual(300 + 10 * 12, r.xpGained);
            Assert.AreEqual(420, profile.xpIntoLevel);
            Assert.AreEqual(1, profile.accountLevel);
            Assert.AreEqual(0, r.levelsGained);
        }

        [Test]
        public void Loss_AwardsLessXpThanAWin()
        {
            var profile = new PlayerProfileSave { playerId = "p" };

            var r = ProgressionGlue.Apply(profile, Loss(seconds: 600f), false, false);

            Assert.AreEqual(180 + 10 * 12, r.xpGained);
        }

        [Test]
        public void PartialMinutes_AreTruncated()
        {
            var profile = new PlayerProfileSave { playerId = "p" };

            var r = ProgressionGlue.Apply(profile, Win(seconds: 119f), false, false);

            Assert.AreEqual(300 + 1 * 12, r.xpGained);
        }

        // ---- levelling ---- //

        [Test]
        public void EnoughXp_LevelsUp_CarriesOverTheRemainder_AndGrantsKp()
        {
            var profile = new PlayerProfileSave { playerId = "p", xpIntoLevel = 590 };

            var r = ProgressionGlue.Apply(profile, Win(), false, false);

            Assert.AreEqual(1, r.levelsGained);
            Assert.AreEqual(2, profile.accountLevel);
            Assert.AreEqual(590 + 300 - 600, profile.xpIntoLevel);
            Assert.AreEqual(60, r.kpGained);
            Assert.AreEqual(60, profile.kp);
        }

        [Test]
        public void HugeXp_CanLevelUpSeveralTimesInOneMatch()
        {
            var profile = new PlayerProfileSave { playerId = "p", xpIntoLevel = 5000 };

            var r = ProgressionGlue.Apply(profile, Win(), false, false);

            Assert.AreEqual(5, r.levelsGained);
            Assert.AreEqual(6, profile.accountLevel);
            Assert.AreEqual(1100, profile.xpIntoLevel);
            Assert.AreEqual(5 * 60, r.kpGained);
        }

        [Test]
        public void ReachingLevelTen_PaysTheMilestoneBonus()
        {
            var profile = new PlayerProfileSave
            {
                playerId = "p", accountLevel = 9,
                xpIntoLevel = ProgressionGlue.XpToNext(9) - 1
            };

            var r = ProgressionGlue.Apply(profile, Win(), false, false);

            Assert.AreEqual(10, profile.accountLevel);
            Assert.AreEqual(310, r.kpGained);
        }

        // ---- first win of the day ---- //

        [Test]
        public void FirstWinOfDay_AddsKpAndARewardId_OnlyWhenTheMatchWasWon()
        {
            var won = new PlayerProfileSave { playerId = "p" };
            var winResult = ProgressionGlue.Apply(won, Win(), false, true);
            Assert.AreEqual(400, winResult.kpGained);
            CollectionAssert.Contains(winResult.rewardIds, "first_win_of_day");

            var lost = new PlayerProfileSave { playerId = "p" };
            var lossResult = ProgressionGlue.Apply(lost, Loss(), false, true);
            Assert.AreEqual(0, lossResult.kpGained);
            CollectionAssert.DoesNotContain(lossResult.rewardIds, "first_win_of_day");
        }

        // ---- World Rift ---- //

        [Test]
        public void WorldRiftWin_AwardsCosmeticKpPerClaim_BadgeAndRatingGain()
        {
            var profile = new PlayerProfileSave { playerId = "p" };

            var r = ProgressionGlue.Apply(profile, Win(manifestations: 2), true, false);

            Assert.AreEqual(60 + 2 * 15, r.kpGained);
            Assert.AreEqual(1012, profile.worldRiftRating);
            CollectionAssert.Contains(r.rewardIds, "worldrift_badge");
        }

        [Test]
        public void WorldRiftLoss_StillPaysSmallKp_ButLowersTheCasualRating()
        {
            var profile = new PlayerProfileSave { playerId = "p" };

            var r = ProgressionGlue.Apply(profile, Loss(), true, false);

            Assert.AreEqual(25, r.kpGained);
            Assert.AreEqual(992, profile.worldRiftRating);
            CollectionAssert.DoesNotContain(r.rewardIds, "worldrift_badge");
        }

        [Test]
        public void NonWorldRiftMatches_NeverTouchTheWorldRiftRating()
        {
            var profile = new PlayerProfileSave { playerId = "p" };

            ProgressionGlue.Apply(profile, Win(manifestations: 5), false, false);

            Assert.AreEqual(1000, profile.worldRiftRating);
            Assert.AreEqual(0, profile.kp);
        }
    }
}
