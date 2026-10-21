// Project D | Codename: Zantetsuken — CommunityTournament.cs (open registration + seeding)
using System; using System.Collections.Generic;
namespace ProjectD.P11.LiveTournaments
{
    public enum TournamentState { Registration, Seeded, InProgress, Complete }
    public sealed class Registrant { public string teamId; public int seedRating; }
    public sealed class CommunityTournament
    {
        public string id, name; public int maxTeams=32; public TournamentState state=TournamentState.Registration;
        readonly List<Registrant> _regs=new(); public IReadOnlyList<Registrant> Registrants=>_regs;
        public bool Register(string teamId,int seedRating){ if(state!=TournamentState.Registration||_regs.Count>=maxTeams) return false; foreach(var r in _regs) if(r.teamId==teamId) return false; _regs.Add(new Registrant{teamId=teamId,seedRating=seedRating}); return true; }
        /// <summary>Close registration + seed: sort by rating, then pair strong-vs-weak (1v16, 2v15...).</summary>
        public List<(string a,string b)> Seed()
        {
            if(state!=TournamentState.Registration) return null;
            _regs.Sort((x,y)=>y.seedRating-x.seedRating);
            var pairs=new List<(string,string)>(); int n=_regs.Count;
            for(int i=0;i<n/2;i++) pairs.Add((_regs[i].teamId, _regs[n-1-i].teamId));
            state=TournamentState.Seeded; return pairs;
        }
        public bool Start(){ if(state!=TournamentState.Seeded) return false; state=TournamentState.InProgress; return true; }
        public bool Complete(){ if(state!=TournamentState.InProgress) return false; state=TournamentState.Complete; return true; }
        public int Count=>_regs.Count;
    }
}
