// Project D | Codename: Zantetsuken — GuildSystem.cs
using System; using System.Collections.Generic;
namespace ProjectD.Guild
{
    public enum GuildRole { Member, Officer, Leader }
    [Serializable] public sealed class GuildMember { public string playerId, displayName; public GuildRole role=GuildRole.Member; public int contributedXp; }
    public sealed class Guild
    {
        public string guildId, name, tag; public int level=1; public long guildXp; public int capacity=50;
        readonly Dictionary<string,GuildMember> _m=new(); readonly HashSet<string> _req=new();
        public IReadOnlyCollection<GuildMember> Members=>_m.Values; public IReadOnlyCollection<string> JoinRequests=>_req;
        public int MemberCount=>_m.Count; public bool IsFull=>_m.Count>=capacity; public bool IsMember(string id)=>_m.ContainsKey(id); public GuildMember Get(string id)=>_m.TryGetValue(id,out var x)?x:null;
        public bool AddFounder(string id,string n){ if(_m.Count>0) return false; _m[id]=new GuildMember{playerId=id,displayName=n,role=GuildRole.Leader}; return true; }
        public bool RequestJoin(string id){ if(IsMember(id)||IsFull) return false; return _req.Add(id); }
        public bool ApproveRequest(string by,string id,string n){ if(!HasPerm(by,GuildRole.Officer)) return false; if(!_req.Remove(id)||IsFull) return false; _m[id]=new GuildMember{playerId=id,displayName=n}; return true; }
        public bool Promote(string by,string id){ if(!HasPerm(by,GuildRole.Leader)) return false; var t=Get(id); if(t==null||t.role==GuildRole.Leader) return false; t.role=t.role==GuildRole.Member?GuildRole.Officer:GuildRole.Leader; return true; }
        public bool HasPerm(string id,GuildRole a){ var x=Get(id); return x!=null&&(int)x.role>=(int)a; }
        public void ContributeXp(string id,int amt){ if(amt<=0) return; var x=Get(id); if(x!=null) x.contributedXp+=amt; guildXp+=amt; while(guildXp>=XpForNext(level)){ guildXp-=XpForNext(level); level++; capacity+=5; } }
        public static int XpForNext(int l)=>2000+l*800;
    }
}
