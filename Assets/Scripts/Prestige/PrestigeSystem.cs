// Project D | Codename: Zantetsuken — PrestigeSystem.cs (post-max prestige tiers, cosmetic)
using System;
namespace ProjectD.P11.Prestige
{
    public sealed class PrestigeState { public int accountLevel; public int prestigeTier; public long prestigeXp; }
    public static class PrestigeSystem
    {
        public const int MaxLevel=300;
        public static long XpPerPrestige(int tier)=>50000+(long)tier*25000;
        public static bool CanPrestige(PrestigeState s)=>s.accountLevel>=MaxLevel;
        /// <summary>Prestige: consumes max level, bumps prestige tier, grants a cosmetic-only border id.</summary>
        public static string Prestige(PrestigeState s)
        {
            if(!CanPrestige(s)) return null;
            s.accountLevel=1; s.prestigeTier++;
            return $"prestige_border_{s.prestigeTier}";   // cosmetic reward only
        }
        public static string BadgeFor(int tier)=>tier<=0?"":tier<5?"prestige_bronze":tier<10?"prestige_silver":tier<20?"prestige_gold":"prestige_crystal";
    }
}
