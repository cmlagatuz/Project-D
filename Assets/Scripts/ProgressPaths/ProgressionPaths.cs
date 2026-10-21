// Project D | Codename: Zantetsuken — ProgressionPaths.cs (multi-track long-term goals)
using System; using System.Collections.Generic;
namespace ProjectD.P9.ProgressPaths
{
    public enum PathTrack { Mastery, Collector, Ranked, Social, Explorer }
    public sealed class PathTier { public int tier; public long requirement; public int rewardKp; public string rewardItemId; }
    public sealed class ProgressionPath { public PathTrack track; public long progress; public readonly List<PathTier> tiers=new(); public int claimedTier; }
    public static class ProgressionPaths
    {
        public static int CurrentTier(ProgressionPath p){ int t=0; foreach(var tier in p.tiers) if(p.progress>=tier.requirement) t=tier.tier; return t; }
        public static void AddProgress(ProgressionPath p,long amount){ if(amount>0) p.progress+=amount; }
        public static (int tiersClaimed,int kp,List<string> items) Claim(ProgressionPath p)
        {
            int cur=CurrentTier(p); int kp=0; var items=new List<string>(); int claimed=0;
            foreach(var tier in p.tiers){ if(tier.tier>p.claimedTier && tier.tier<=cur){ kp+=tier.rewardKp; if(!string.IsNullOrEmpty(tier.rewardItemId)) items.Add(tier.rewardItemId); claimed++; } }
            p.claimedTier=Math.Max(p.claimedTier,cur); return (claimed,kp,items);
        }
        public static float TierProgress(ProgressionPath p)
        {
            int cur=CurrentTier(p); long floor=0,next=long.MaxValue;
            foreach(var t in p.tiers){ if(t.tier==cur) floor=t.requirement; if(t.tier==cur+1) next=t.requirement; }
            if(next==long.MaxValue) return 1f; if(next<=floor) return 1f; return Math.Min(1f,(float)(p.progress-floor)/(next-floor));
        }
    }
}
