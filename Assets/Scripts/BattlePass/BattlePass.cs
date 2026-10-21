// Project D | Codename: Zantetsuken — BattlePass.cs
using System; using System.Collections.Generic;
namespace ProjectD.BattlePass
{
    public enum RewardTrack { Free, Premium }
    public enum BpRewardKind { KP, ProfileIcon, ProfileBorder, ChestFragment, ChampionToken, Emote, XpBoost }
    [Serializable] public sealed class BpReward { public int tier; public RewardTrack track; public BpRewardKind kind; public int amount; public string itemId, displayName; }
    public sealed class BattlePassDefinition { public int seasonId=1; public string name="Season 1"; public int maxTier=50, xpPerTier=1000; public readonly List<BpReward> rewards=new(); public IEnumerable<BpReward> RewardsForTier(int t){ foreach(var r in rewards) if(r.tier==t) yield return r; } }
    public sealed class BattlePassProgress
    {
        public int seasonId, tier=1, xpIntoTier; public bool premiumUnlocked;
        readonly HashSet<int> _cf=new(), _cp=new();
        public bool IsClaimed(int t,RewardTrack tr)=>(tr==RewardTrack.Free?_cf:_cp).Contains(t);
        public int AddXp(BattlePassDefinition d,int xp){ int g=0; if(xp<=0) return 0; xpIntoTier+=xp; while(tier<d.maxTier&&xpIntoTier>=d.xpPerTier){ xpIntoTier-=d.xpPerTier; tier++; g++; } if(tier>=d.maxTier) xpIntoTier=0; return g; }
        public void UnlockPremium()=>premiumUnlocked=true;
        public List<BpReward> Claim(BattlePassDefinition d,int t,RewardTrack tr){ if(t>tier) return null; if(tr==RewardTrack.Premium&&!premiumUnlocked) return null; var set=tr==RewardTrack.Free?_cf:_cp; if(set.Contains(t)) return null; var rs=new List<BpReward>(); foreach(var r in d.RewardsForTier(t)) if(r.track==tr) rs.Add(r); if(rs.Count==0) return null; set.Add(t); return rs; }
        public float TierProgress(BattlePassDefinition d)=>d.xpPerTier<=0?0f:(float)xpIntoTier/d.xpPerTier;
    }
}
