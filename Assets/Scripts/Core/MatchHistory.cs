// Project D | Codename: Zantetsuken — MatchHistory.cs (persistent per-player match log)
// Closes the gap: match history viewable in the profile. Each finished match writes a
// compact summary here; the profile lists them and can open the full post-game analytics.
using System; using System.Collections.Generic;
namespace ProjectD.History
{
    public enum MatchOutcome { Victory, Defeat, Remake }
    [Serializable] public sealed class MatchHistoryEntry
    {
        public string matchId, mode, mapId, championId; public MatchOutcome outcome;
        public int kills, deaths, assists, cs; public float durationSeconds; public long playedUnix;
        public bool wasMvp; public int manifestationsClaimed;   // World Rift flavor
        public float Kda=>deaths==0?(kills+assists):(float)(kills+assists)/deaths;
    }
    public sealed class MatchHistory
    {
        readonly List<MatchHistoryEntry> _entries=new(); public int MaxKept=100;
        public void Record(MatchHistoryEntry e){ if(e==null||string.IsNullOrEmpty(e.matchId)) return; _entries.Insert(0,e); while(_entries.Count>MaxKept) _entries.RemoveAt(_entries.Count-1); }
        /// <summary>Most-recent-first page of matches (for the profile list).</summary>
        public List<MatchHistoryEntry> Recent(int count=10){ var o=new List<MatchHistoryEntry>(); for(int i=0;i<Math.Min(count,_entries.Count);i++) o.Add(_entries[i]); return o; }
        public MatchHistoryEntry Get(string matchId){ foreach(var e in _entries) if(e.matchId==matchId) return e; return null; }
        public int Count=>_entries.Count;
        // ---- profile stats derived from history ----
        public int Wins(){ int n=0; foreach(var e in _entries) if(e.outcome==MatchOutcome.Victory) n++; return n; }
        public int Games=>_entries.Count;
        public float WinRate=>Games==0?0f:(float)Wins()/Games;
        public int WinStreak(){ int s=0; foreach(var e in _entries){ if(e.outcome==MatchOutcome.Victory) s++; else break; } return s; }
        /// <summary>Most-played champion across recorded history.</summary>
        public string MostPlayedChampion(){ var c=new Dictionary<string,int>(); foreach(var e in _entries){ if(string.IsNullOrEmpty(e.championId)) continue; c.TryGetValue(e.championId,out int n); c[e.championId]=n+1; } string best=null; int bv=0; foreach(var kv in c) if(kv.Value>bv){ bv=kv.Value; best=kv.Key; } return best; }
        /// <summary>Per-champion win-rate (for the champions tab of a profile).</summary>
        public (int games,int wins) ChampionRecord(string championId){ int g=0,w=0; foreach(var e in _entries) if(e.championId==championId){ g++; if(e.outcome==MatchOutcome.Victory) w++; } return (g,w); }
    }
}
