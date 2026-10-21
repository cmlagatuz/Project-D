// Project D | Codename: Zantetsuken — ProgressionGlue.cs (post-match result -> XP/level/KP/rewards)
using System; using System.Collections.Generic;
using ProjectD.P15.MatchLoop; using ProjectD.P15.Save;
namespace ProjectD.P15.Progression
{
    public struct ProgressionResult { public int xpGained; public int levelsGained; public long kpGained; public List<string> rewardIds; }
    public static class ProgressionGlue
    {
        public const int BaseWinXp=300, BaseLossXp=180, PerMinuteXp=12, MvpBonusXp=60;
        public const int KpPerLevel=60, KpMilestoneEvery=10, KpMilestoneBonus=250;
        public static int XpToNext(int level)=>600+120*(Math.Max(1,level)-1);
        public static int KpForLevel(int level){ if(level<=1) return 0; int kp=KpPerLevel; if(level%KpMilestoneEvery==0) kp+=KpMilestoneBonus; return kp; }

        /// <summary>Apply a match result to the profile: award XP, roll up level-ups (granting KP), and rewards.</summary>
        public static ProgressionResult Apply(PlayerProfileSave profile, in MatchResult result, bool isWorldRift, bool firstWinOfDay)
        {
            int minutes=Math.Max(0,(int)(result.durationSeconds/60f));
            int xp=(result.localWon?BaseWinXp:BaseLossXp)+minutes*PerMinuteXp;
            var pr=new ProgressionResult{ rewardIds=new List<string>() };
            pr.xpGained=xp;

            profile.xpIntoLevel+=xp; long kp=0; int levels=0;
            while(profile.xpIntoLevel>=XpToNext(profile.accountLevel)){ profile.xpIntoLevel-=XpToNext(profile.accountLevel); profile.accountLevel++; kp+=KpForLevel(profile.accountLevel); levels++; }
            // World Rift adds cosmetic KP + a casual rating change; Ranked/Normal use account leveling.
            if(isWorldRift){ kp+=result.localWon?60:25; kp+=result.manifestationsClaimed*15; profile.worldRiftRating+=result.localWon?12:-8; if(result.localWon) pr.rewardIds.Add("worldrift_badge"); }
            if(firstWinOfDay&&result.localWon){ kp+=400; pr.rewardIds.Add("first_win_of_day"); }

            profile.kp+=kp; pr.kpGained=kp; pr.levelsGained=levels;
            return pr;
        }
    }
}
