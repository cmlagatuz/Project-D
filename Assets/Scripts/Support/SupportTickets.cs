// Project D | Codename: Zantetsuken — SupportTickets.cs
using System; using System.Collections.Generic;
namespace ProjectD.P4.Support
{
    public enum TicketCategory { Account, Billing, Bug, Gameplay, Report, Other }
    public enum TicketState { Open, AwaitingPlayer, InProgress, Resolved, Closed }
    public sealed class SupportMessage { public string authorId, body; public long unix; public bool fromStaff; }
    public sealed class Ticket { public string id, playerId, subject; public TicketCategory category; public TicketState state=TicketState.Open; public long createdUnix, updatedUnix; public readonly List<SupportMessage> Messages=new(); }
    public sealed class SupportSystem
    {
        readonly Dictionary<string,Ticket> _t=new();
        public Ticket Open(string playerId,TicketCategory cat,string subject,string body,long now){ var t=new Ticket{id=Guid.NewGuid().ToString("N").Substring(0,8),playerId=playerId,category=cat,subject=subject,createdUnix=now,updatedUnix=now}; t.Messages.Add(new SupportMessage{authorId=playerId,body=body,unix=now}); _t[t.id]=t; return t; }
        public Ticket Get(string id)=>_t.TryGetValue(id,out var t)?t:null;
        public bool Reply(string id,string authorId,string body,bool fromStaff,long now){ var t=Get(id); if(t==null||t.state==TicketState.Closed) return false; t.Messages.Add(new SupportMessage{authorId=authorId,body=body,unix=now,fromStaff=fromStaff}); t.updatedUnix=now; t.state=fromStaff?TicketState.AwaitingPlayer:TicketState.InProgress; return true; }
        public bool Resolve(string id,long now){ var t=Get(id); if(t==null||t.state==TicketState.Closed) return false; t.state=TicketState.Resolved; t.updatedUnix=now; return true; }
        public bool Close(string id,long now){ var t=Get(id); if(t==null) return false; t.state=TicketState.Closed; t.updatedUnix=now; return true; }
        public int OpenCount(){ int n=0; foreach(var t in _t.Values) if(t.state!=TicketState.Closed&&t.state!=TicketState.Resolved) n++; return n; }
    }
}
