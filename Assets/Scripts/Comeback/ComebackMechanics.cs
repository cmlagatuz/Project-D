// Project D | Codename: Zantetsuken — ComebackMechanics.cs (shutdowns + catch-up)
using System;
namespace ProjectD.P5.Comeback
{
    public static class ComebackMechanics
    {
        /// <summary>Shutdown bounty on a fed enemy: grows with their kill streak, capped.</summary>
        public static int ShutdownGold(int killStreak)
        {
            if(killStreak<2) return 300;                 // base kill value
            int bounty=150+ (killStreak-1)*125; return Math.Min(bounty,1000);
        }
        /// <summary>Catch-up XP for a player behind the average level (percent bonus on XP gained).</summary>
        public static float CatchUpXpMultiplier(int playerLevel,float averageLevel)
        {
            float behind=averageLevel-playerLevel; if(behind<=1f) return 1f;
            return 1f + Math.Min(0.35f, (behind-1f)*0.10f);   // up to +35%
        }
        /// <summary>Team-wide gold bonus when a team is far behind in total gold.</summary>
        public static float CatchUpGoldMultiplier(int teamGold,int enemyGold)
        {
            int behind=enemyGold-teamGold; if(behind<3000) return 1f;
            return 1f + Math.Min(0.20f, (behind-3000)/20000f);  // up to +20%
        }
    }
}
