// Project D | Codename: Zantetsuken — LegacyRewards.cs (veteran-tenure milestones)
using System; using System.Collections.Generic;
namespace ProjectD.P13.Legacy
{
    public sealed class LegacyState { public long firstLoginUnix; public int seasonsPlayed; public int claimedMilestone; }
    public struct LegacyReward { public int milestoneMonths; public string cosmeticId, title; }
    public static class LegacyRewards
    {
        static readonly (int months,string cosmetic,string title)[] Milestones = {
            (3,"legacy_badge_3m","Veteran"),(6,"legacy_badge_6m","Seasoned"),(12,"legacy_frame_1y","Loyal"),
            (24,"legacy_frame_2y","Devoted"),(36,"legacy_crown_3y","Founder's Circle"),(60,"legacy_aura_5y","Living Legend"),
        };
        public static int TenureMonths(long firstLoginUnix,long nowUnix)=>(int)((nowUnix-firstLoginUnix)/(30L*86400));
        /// <summary>Milestones earned but not yet claimed.</summary>
        public static List<LegacyReward> Unclaimed(LegacyState s,long now)
        {
            int months=TenureMonths(s.firstLoginUnix,now); var outl=new List<LegacyReward>();
            for(int i=0;i<Milestones.Length;i++){ var m=Milestones[i]; if(months>=m.months && (i+1)>s.claimedMilestone) outl.Add(new LegacyReward{milestoneMonths=m.months,cosmeticId=m.cosmetic,title=m.title}); }
            return outl;
        }
        /// <summary>Claim all earned milestones; returns count claimed.</summary>
        public static int ClaimAll(LegacyState s,long now)
        {
            int months=TenureMonths(s.firstLoginUnix,now); int newIdx=s.claimedMilestone;
            for(int i=0;i<Milestones.Length;i++) if(months>=Milestones[i].months) newIdx=Math.Max(newIdx,i+1);
            int claimed=newIdx-s.claimedMilestone; s.claimedMilestone=newIdx; return claimed;
        }
        public static string CurrentTitle(LegacyState s,long now){ int months=TenureMonths(s.firstLoginUnix,now); string title=""; foreach(var m in Milestones) if(months>=m.months) title=m.title; return title; }
    }
}
