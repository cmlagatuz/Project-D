// Project D | Codename: Zantetsuken — FriendsModel.cs (social graph, requests, presence)
using System; using System.Collections.Generic;
namespace ProjectD.Friends
{
    public enum Presence { Offline, Online, InQueue, InGame, Away }
    [Serializable] public sealed class FriendEntry { public string playerId, displayName, statusText, rankLabel; public int accountLevel; public Presence presence=Presence.Offline; public long lastOnlineUnix; }
    public sealed class FriendsModel
    {
        readonly Dictionary<string,FriendEntry> _f=new(); readonly HashSet<string> _in=new(), _out=new();
        public IReadOnlyCollection<FriendEntry> Friends=>_f.Values;
        public IReadOnlyCollection<string> IncomingRequests=>_in;
        public IReadOnlyCollection<string> OutgoingRequests=>_out;
        public int FriendCount=>_f.Count; public int IncomingCount=>_in.Count;
        public bool IsFriend(string id)=>_f.ContainsKey(id);
        public void AddFriendDirect(FriendEntry f){ if(f!=null&&!string.IsNullOrEmpty(f.playerId)) _f[f.playerId]=f; }
        public bool SendRequest(string t){ if(string.IsNullOrEmpty(t)||IsFriend(t)) return false; if(_in.Contains(t)) return AcceptRequest(t); _out.Add(t); return true; }
        public bool ReceiveRequest(string from){ if(string.IsNullOrEmpty(from)||IsFriend(from)) return false; _in.Add(from); return true; }
        public bool AcceptRequest(string from){ if(!_in.Remove(from)){ if(!_out.Remove(from)) return false; } _out.Remove(from); _f[from]=new FriendEntry{playerId=from,displayName=from,presence=Presence.Online}; return true; }
        public bool DeclineRequest(string from)=>_in.Remove(from);
        public bool CancelOutgoing(string to)=>_out.Remove(to);
        public bool RemoveFriend(string id)=>_f.Remove(id);
        public void SetPresence(string id,Presence p,string status=null){ if(_f.TryGetValue(id,out var f)){ f.presence=p; if(status!=null) f.statusText=status; } }
        public List<FriendEntry> Sorted(){ var l=new List<FriendEntry>(_f.Values); l.Sort((a,b)=>{ int pa=Rank(a.presence),pb=Rank(b.presence); if(pa!=pb) return pa-pb; return string.CompareOrdinal(a.displayName??"",b.displayName??""); }); return l; }
        static int Rank(Presence p)=>p switch{Presence.InGame=>0,Presence.InQueue=>1,Presence.Online=>2,Presence.Away=>3,_=>4};
    }
}
