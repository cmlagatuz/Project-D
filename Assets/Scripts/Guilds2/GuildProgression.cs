// Project D | Codename: Zantetsuken — GuildProgression.cs (guild levels + perks + weekly)
using System; using System.Collections.Generic;
namespace ProjectD.P11.Guilds
{
    public enum GuildPerk { ExtraMemberSlots, XpBonus, WeeklyChestBonus, CustomEmblem, GuildShop }
    public sealed class GuildProgression
    {
        public int level=1; public long xp; public readonly Dictionary<string,long> contributions=new();
        public static long XpForLevel(int lvl)=>1500+(long)lvl*1000;
        public void Contribute(string memberId,long amount){ if(amount<=0) return; contributions.TryGetValue(memberId,out long v); contributions[memberId]=v+amount; xp+=amount; while(xp>=XpForLevel(level)){ xp-=XpForLevel(level); level++; } }
        public IEnumerable<GuildPerk> UnlockedPerks(){ if(level>=2) yield return GuildPerk.ExtraMemberSlots; if(level>=3) yield return GuildPerk.XpBonus; if(level>=5) yield return GuildPerk.WeeklyChestBonus; if(level>=7) yield return GuildPerk.CustomEmblem; if(level>=10) yield return GuildPerk.GuildShop; }
        public bool HasPerk(GuildPerk p){ foreach(var x in UnlockedPerks()) if(x==p) return true; return false; }
        public string TopContributor(){ string best=null; long bv=-1; foreach(var kv in contributions) if(kv.Value>bv){ bv=kv.Value; best=kv.Key; } return best; }
    }
    public sealed class WeeklyGuildQuest { public string id; public long goal, progress; public int rewardGuildXp; public bool Complete=>progress>=goal; public void Add(long a){ if(a>0) progress=Math.Min(goal,progress+a); } }
}
