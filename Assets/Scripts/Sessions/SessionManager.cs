// Project D | Codename: Zantetsuken — SessionManager.cs (server allocation + lifecycle)
using System; using System.Collections.Generic;
namespace ProjectD.P7.Sessions
{
    public enum SessionState { Allocating, Loading, InProgress, Ended }
    public sealed class GameServer { public string id, region; public int capacity=1; public int active; public bool healthy=true; public bool HasRoom=>active<capacity; }
    public sealed class GameSession { public string sessionId, serverId; public SessionState state=SessionState.Allocating; public List<string> players=new(); public long createdUnix, endedUnix; }
    public sealed class SessionManager
    {
        readonly Dictionary<string,GameServer> _servers=new(); readonly Dictionary<string,GameSession> _sessions=new();
        public void RegisterServer(GameServer s){ if(s!=null&&!string.IsNullOrEmpty(s.id)) _servers[s.id]=s; }
        public GameSession Allocate(IEnumerable<string> players,string region,long now)
        {
            GameServer pick=null; foreach(var s in _servers.Values){ if(!s.healthy||!s.HasRoom) continue; if(!string.IsNullOrEmpty(region)&&s.region!=region) continue; if(pick==null||s.active<pick.active) pick=s; }
            if(pick==null) return null;
            pick.active++;
            var sess=new GameSession{sessionId=Guid.NewGuid().ToString("N").Substring(0,10),serverId=pick.id,createdUnix=now,state=SessionState.Loading};
            sess.players.AddRange(players); _sessions[sess.sessionId]=sess; return sess;
        }
        public bool Advance(string sessionId,SessionState state){ if(!_sessions.TryGetValue(sessionId,out var s)) return false; s.state=state; return true; }
        public bool End(string sessionId,long now){ if(!_sessions.TryGetValue(sessionId,out var s)) return false; s.state=SessionState.Ended; s.endedUnix=now; if(_servers.TryGetValue(s.serverId,out var srv)) srv.active=Math.Max(0,srv.active-1); return true; }
        public int ActiveSessions(){ int n=0; foreach(var s in _sessions.Values) if(s.state!=SessionState.Ended) n++; return n; }
        public GameSession Get(string id)=>_sessions.TryGetValue(id,out var s)?s:null;
    }
}
