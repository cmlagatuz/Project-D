// Project D | Codename: Zantetsuken — PartySystem.cs
using System; using System.Collections.Generic;
namespace ProjectD.Party
{
    public enum PartyMode { Solo, Duo, Flex5, CoOp }
    [Serializable] public sealed class PartyMember { public string playerId, displayName; public int mmr=1000; public bool ready; }
    public sealed class Party
    {
        public string leaderId; public PartyMode mode=PartyMode.Flex5;
        readonly Dictionary<string,PartyMember> _m=new(); readonly HashSet<string> _inv=new();
        public IReadOnlyCollection<PartyMember> Members=>_m.Values; public IReadOnlyCollection<string> PendingInvites=>_inv;
        public int Size=>_m.Count; public int Capacity=>mode switch{PartyMode.Solo=>1,PartyMode.Duo=>2,_=>5}; public bool IsFull=>_m.Count>=Capacity; public bool Contains(string id)=>_m.ContainsKey(id);
        public bool CreateWithLeader(string id,string n,int mmr){ if(_m.Count>0) return false; leaderId=id; _m[id]=new PartyMember{playerId=id,displayName=n,mmr=mmr}; return true; }
        public bool Invite(string by,string t){ if(by!=leaderId||Contains(t)||IsFull) return false; return _inv.Add(t); }
        public bool AcceptInvite(string id,string n,int mmr){ if(!_inv.Remove(id)||IsFull) return false; _m[id]=new PartyMember{playerId=id,displayName=n,mmr=mmr}; return true; }
        public bool Leave(string id){ bool r=_m.Remove(id); if(id==leaderId&&_m.Count>0) foreach(var m in _m.Values){ leaderId=m.playerId; break; } return r; }
        public void SetReady(string id,bool r){ if(_m.TryGetValue(id,out var m)) m.ready=r; }
        public bool AllReady(){ if(_m.Count==0) return false; foreach(var m in _m.Values) if(!m.ready) return false; return true; }
        public int AverageMmr(){ if(_m.Count==0) return 0; long s=0; foreach(var m in _m.Values) s+=m.mmr; return (int)(s/_m.Count)+(_m.Count>=5?60:_m.Count>=3?40:_m.Count==2?20:0); }
    }
}
