// Project D | Codename: Zantetsuken — WorldRiftRewards.cs (cosmetic rewards + casual rating)
using System;
namespace ProjectD.Rewards
{
    public struct WorldRiftResult { public bool win; public int manifestationsClaimed; public float matchMinutes; public bool firstOfDay; }
    public static class WorldRiftRewards
    {
        public const int BaseWinKp=60, BaseLossKp=25, PerClaimKp=15, FirstOfDayBonusKp=100;
        /// <summary>KP (soft cosmetic currency) reward. NEVER Ranked LP.</summary>
        public static int Kp(in WorldRiftResult r)
        { int kp=(r.win?BaseWinKp:BaseLossKp)+r.manifestationsClaimed*PerClaimKp; if(r.firstOfDay) kp+=FirstOfDayBonusKp; return kp; }
        /// <summary>Separate World Rift casual rating delta (kept apart from Ranked MMR).</summary>
        public static int CasualRatingDelta(bool win)=>win?12:-8;
        /// <summary>A fun "world explorer" progress point for collecting realm-victory badges.</summary>
        public static string ExplorerBadge(string worldId,bool win)=>win?$"badge_conquered_{worldId}":null;
    }
}
