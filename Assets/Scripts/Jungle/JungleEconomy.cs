// Project D | Codename: Zantetsuken — JungleEconomy.cs (camp values + smite + scaling)
using System; using System.Collections.Generic;
namespace ProjectD.P5.Jungle
{
    public enum CampTier { Small, Medium, Large }
    public sealed class JungleCamp { public string id; public CampTier tier; public bool alive=true; public float nextSpawnAt; }
    public static class JungleEconomy
    {
        public static int BaseGold(CampTier t)=>t switch{CampTier.Small=>25,CampTier.Medium=>60,CampTier.Large=>90,_=>50};
        public static int BaseXp(CampTier t)=>t switch{CampTier.Small=>40,CampTier.Medium=>90,CampTier.Large=>130,_=>70};
        /// <summary>Camps scale up with match time (monsters get stronger + worth more).</summary>
        public static int GoldAt(CampTier t,int matchMinute)=>BaseGold(t)+matchMinute*2;
        public static int XpAt(CampTier t,int matchMinute)=>BaseXp(t)+matchMinute*3;
        public const int SmiteTrueDamage=900; public const float SmiteCooldown=15f;
        /// <summary>Smite secures a camp if its HP is at/below the smite damage.</summary>
        public static bool SmiteSecures(float campHp)=>campHp<=SmiteTrueDamage;
    }
    public sealed class SmiteCharge
    {
        public float lastUsedAt=-999f;
        public bool Ready(float now)=>now-lastUsedAt>=JungleEconomy.SmiteCooldown;
        public bool Use(float now){ if(!Ready(now)) return false; lastUsedAt=now; return true; }
    }
}
