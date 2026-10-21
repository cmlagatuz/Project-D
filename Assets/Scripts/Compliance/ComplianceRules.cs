// Project D | Codename: Zantetsuken — ComplianceRules.cs (age gates + region rules + retention)
using System; using System.Collections.Generic;
namespace ProjectD.P12.Compliance
{
    public enum Feature { Chat, Trading, Marketplace, Purchases, RankedPlay, SocialAdd }
    public sealed class PlayerCompliance { public int age; public string region; public bool parentalConsent; }
    public static class ComplianceRules
    {
        /// <summary>Whether a feature is allowed given age/region/consent (fail-safe: default deny for gated features).</summary>
        public static bool FeatureAllowed(Feature f, in PlayerCompliance p)
        {
            // Under-13 (COPPA-like): no chat/trading/marketplace/social/purchases without consent.
            if(p.age<13)
            {
                switch(f){ case Feature.Chat: case Feature.Trading: case Feature.Marketplace: case Feature.SocialAdd: return false; case Feature.Purchases: return p.parentalConsent; default: return true; }
            }
            // 13-17: purchases require consent; trading/marketplace region-gated.
            if(p.age<18)
            {
                if(f==Feature.Purchases) return p.parentalConsent;
                if((f==Feature.Trading||f==Feature.Marketplace) && RegionRestrictsMinorTrade(p.region)) return false;
            }
            return true;
        }
        public static bool RegionRestrictsMinorTrade(string region)=>region=="KR"||region=="CN";   // example region rules
        /// <summary>Age gate at account creation: minimum age to play at all in a region.</summary>
        public static bool CanCreateAccount(int age,string region){ int min=region=="DE"?16:13; return age>=min; }
        /// <summary>Data-retention window (seconds) by data type — for scheduled purges.</summary>
        public static long RetentionSeconds(string dataType)=>dataType switch{"chat_logs"=>90L*86400,"match_replays"=>180L*86400,"telemetry"=>365L*86400,"audit"=>730L*86400,_=>365L*86400};
        public static bool ShouldPurge(string dataType,long createdUnix,long nowUnix)=>(nowUnix-createdUnix)>=RetentionSeconds(dataType);
    }
}
