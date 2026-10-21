// Project D | Codename: Zantetsuken — CompanionSystem.cs (collectible companions, cosmetic)
using System; using System.Collections.Generic;
namespace ProjectD.P13.Companions
{
    public enum CompanionTrait { Loyal, Curious, Brave, Mischievous, Wise }
    public sealed class Companion { public string id, name, species; public int level=1; public long xp; public readonly HashSet<CompanionTrait> traits=new(); }
    public sealed class CompanionRoster
    {
        readonly Dictionary<string,Companion> _companions=new(); public string EquippedId{get;private set;}
        public static long XpForLevel(int lvl)=>200+(long)lvl*150;
        public const int MaxLevel=20;
        public void Add(Companion c){ if(c!=null&&!string.IsNullOrEmpty(c.id)) _companions[c.id]=c; }
        public Companion Get(string id)=>_companions.TryGetValue(id,out var c)?c:null;
        public int AddXp(string id,long xp){ var c=Get(id); if(c==null||xp<=0) return 0; c.xp+=xp; int gained=0; while(c.level<MaxLevel&&c.xp>=XpForLevel(c.level)){ c.xp-=XpForLevel(c.level); c.level++; gained++; UnlockTrait(c); } return gained; }
        static void UnlockTrait(Companion c){ if(c.level==5) c.traits.Add(CompanionTrait.Loyal); if(c.level==10) c.traits.Add(CompanionTrait.Brave); if(c.level==15) c.traits.Add(CompanionTrait.Wise); }
        public bool Equip(string id){ if(!_companions.ContainsKey(id)) return false; EquippedId=id; return true; }
        public int Count=>_companions.Count;
    }
}
