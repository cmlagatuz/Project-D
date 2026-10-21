// Project D | Codename: Zantetsuken — TournamentBracket.cs
using System; using System.Collections.Generic;
namespace ProjectD.P4.Tournaments
{
    public sealed class BracketMatch { public int round, index; public string teamA, teamB, winner; public bool Decided=>!string.IsNullOrEmpty(winner); }
    public sealed class Tournament
    {
        public string id, name; public readonly List<BracketMatch> Matches=new(); public int Rounds{get;private set;}
        public void Seed(IReadOnlyList<string> teams){ Matches.Clear(); int n=1; while(n<teams.Count) n<<=1; Rounds=(int)Math.Round(Math.Log(n,2)); var slots=new string[n]; for(int i=0;i<n;i++) slots[i]=i<teams.Count?teams[i]:null; for(int i=0;i<n/2;i++){ var m=new BracketMatch{round=1,index=i,teamA=slots[i*2],teamB=slots[i*2+1]}; if(m.teamB==null&&m.teamA!=null) m.winner=m.teamA; if(m.teamA==null&&m.teamB!=null) m.winner=m.teamB; Matches.Add(m); } }
        public IEnumerable<BracketMatch> RoundMatches(int round){ foreach(var m in Matches) if(m.round==round) yield return m; }
        public bool ReportResult(int round,int index,string winner){ foreach(var m in Matches) if(m.round==round&&m.index==index){ if(winner!=m.teamA&&winner!=m.teamB) return false; m.winner=winner; TryAdvance(round); return true; } return false; }
        void TryAdvance(int round){ if(round>=Rounds) return; var rm=new List<BracketMatch>(RoundMatches(round)); rm.Sort((a,b)=>a.index-b.index); foreach(var m in rm) if(!m.Decided) return; if(HasRound(round+1)) return; for(int i=0;i<rm.Count;i+=2){ var a=rm[i].winner; var b=(i+1<rm.Count)?rm[i+1].winner:null; Matches.Add(new BracketMatch{round=round+1,index=i/2,teamA=a,teamB=b}); } }
        bool HasRound(int r){ foreach(var m in Matches) if(m.round==r) return true; return false; }
        public string Champion(){ foreach(var m in Matches) if(m.round==Rounds&&m.Decided) return m.winner; return null; }
    }
}
