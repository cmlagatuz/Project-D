// Project D | Codename: Zantetsuken — CustomLobby.cs
using System; using System.Collections.Generic;
namespace ProjectD.P4.Lobby
{
    public enum TeamSide { Blue, Red, Spectator }
    public sealed class LobbySlot { public TeamSide side; public string playerId, displayName; public bool IsEmpty=>string.IsNullOrEmpty(playerId); }
    public sealed class CustomLobby
    {
        public string lobbyId, joinCode, hostId, mapId="rift"; public int perTeam=5, maxSpectators=3; public bool tournamentDraft=true;
        readonly List<LobbySlot> _blue=new(), _red=new(), _spec=new();
        public IReadOnlyList<LobbySlot> Blue=>_blue; public IReadOnlyList<LobbySlot> Red=>_red; public IReadOnlyList<LobbySlot> Spectators=>_spec;
        public void Init(){ _blue.Clear(); _red.Clear(); _spec.Clear(); for(int i=0;i<perTeam;i++){ _blue.Add(new LobbySlot{side=TeamSide.Blue}); _red.Add(new LobbySlot{side=TeamSide.Red}); } for(int i=0;i<maxSpectators;i++) _spec.Add(new LobbySlot{side=TeamSide.Spectator}); }
        public static string GenerateCode(int seed){ var r=new Random(seed); const string ch="ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; var s=new char[6]; for(int i=0;i<6;i++) s[i]=ch[r.Next(ch.Length)]; return new string(s); }
        public bool Join(string playerId,string name,TeamSide side){ foreach(var s in Pick(side)) if(s.IsEmpty){ s.playerId=playerId; s.displayName=name; return true; } return false; }
        public bool Leave(string playerId){ foreach(var s in AllSlots()) if(s.playerId==playerId){ s.playerId=null; s.displayName=null; return true; } return false; }
        public bool MoveTo(string playerId,TeamSide side){ string name=null; foreach(var s in AllSlots()) if(s.playerId==playerId){ name=s.displayName; s.playerId=null; s.displayName=null; break; } if(name==null) return false; return Join(playerId,name,side); }
        public bool CanStart(){ return Filled(_blue)==perTeam && Filled(_red)==perTeam; }
        int Filled(List<LobbySlot> l){ int n=0; foreach(var s in l) if(!s.IsEmpty) n++; return n; }
        List<LobbySlot> Pick(TeamSide s)=>s==TeamSide.Blue?_blue:s==TeamSide.Red?_red:_spec;
        IEnumerable<LobbySlot> AllSlots(){ foreach(var s in _blue) yield return s; foreach(var s in _red) yield return s; foreach(var s in _spec) yield return s; }
    }
}
