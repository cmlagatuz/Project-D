// Project D | Codename: Zantetsuken — SeasonPass2.cs (multi-track pass + weekly gates)
using System; using System.Collections.Generic;
namespace ProjectD.P12.Pass
{
    public enum PassTrack { Free, Premium, Prestige }
    public sealed class PassReward { public int tier; public PassTrack track; public string itemId; public int amount; }
    public sealed class SeasonPass2
    {
        public int maxTier=60, xpPerTier=1000; public int seasonWeek=1;   // gates prestige unlocks by week
        public readonly List<PassReward> rewards=new();
        public int tier=1, xpIntoTier; public bool premiumUnlocked, prestigeUnlocked;
        readonly HashSet<(int,PassTrack)> _claimed=new();
        public int AddXp(int xp){ int g=0; if(xp<=0) return 0; xpIntoTier+=xp; while(tier<maxTier&&xpIntoTier>=xpPerTier){ xpIntoTier-=xpPerTier; tier++; g++; } if(tier>=maxTier) xpIntoTier=0; return g; }
        public void UnlockPremium()=>premiumUnlocked=true;
        /// <summary>Prestige track unlocks only after finishing the premium track (tier>=maxTier) AND buying prestige.</summary>
        public bool UnlockPrestige(){ if(!premiumUnlocked||tier<maxTier) return false; prestigeUnlocked=true; return true; }
        public bool IsClaimed(int t,PassTrack tr)=>_claimed.Contains((t,tr));
        public List<PassReward> Claim(int t,PassTrack tr)
        {
            if(t>tier) return null;
            if(tr==PassTrack.Premium&&!premiumUnlocked) return null;
            if(tr==PassTrack.Prestige&&!prestigeUnlocked) return null;
            if(_claimed.Contains((t,tr))) return null;
            var got=new List<PassReward>(); foreach(var r in rewards) if(r.tier==t&&r.track==tr) got.Add(r);
            if(got.Count==0) return null; _claimed.Add((t,tr)); return got;
        }
        public float TierProgress=>xpPerTier<=0?0f:(float)xpIntoTier/xpPerTier;
    }
}
