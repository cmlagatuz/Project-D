// Project D | Codename: Zantetsuken — BanAppeal.cs
using System; using System.Collections.Generic;
namespace ProjectD.P4.Moderation
{
    public enum PenaltyType { None, ChatMute, RankedBan, TempBan, PermaBan }
    public enum AppealState { None, Submitted, UnderReview, Upheld, Overturned }
    public sealed class Penalty { public string penaltyId, playerId, reason, appealText; public PenaltyType type; public long issuedUnix, expiresUnix; public AppealState appeal=AppealState.None; }
    public sealed class ModerationSystem
    {
        readonly Dictionary<string,Penalty> _byId=new(); readonly Dictionary<string,List<Penalty>> _byPlayer=new();
        public Penalty Issue(string playerId,PenaltyType type,string reason,long now,long dur){ var p=new Penalty{penaltyId=Guid.NewGuid().ToString("N").Substring(0,10),playerId=playerId,type=type,reason=reason,issuedUnix=now,expiresUnix=type==PenaltyType.PermaBan?0:now+dur}; _byId[p.penaltyId]=p; if(!_byPlayer.TryGetValue(playerId,out var l)){ l=new List<Penalty>(); _byPlayer[playerId]=l; } l.Add(p); return p; }
        public bool IsActive(Penalty p,long now)=>p!=null&&p.appeal!=AppealState.Overturned&&(p.type==PenaltyType.PermaBan||p.expiresUnix>now);
        public bool HasActive(string playerId,PenaltyType type,long now){ if(!_byPlayer.TryGetValue(playerId,out var l)) return false; foreach(var p in l) if(p.type==type&&IsActive(p,now)) return true; return false; }
        public bool SubmitAppeal(string id,string text){ var p=Get(id); if(p==null||p.appeal!=AppealState.None) return false; p.appeal=AppealState.Submitted; p.appealText=text; return true; }
        public bool BeginReview(string id){ var p=Get(id); if(p==null||p.appeal!=AppealState.Submitted) return false; p.appeal=AppealState.UnderReview; return true; }
        public bool ResolveAppeal(string id,bool overturn){ var p=Get(id); if(p==null||p.appeal!=AppealState.UnderReview) return false; p.appeal=overturn?AppealState.Overturned:AppealState.Upheld; return true; }
        public Penalty Get(string id)=>_byId.TryGetValue(id,out var p)?p:null;
    }
}
