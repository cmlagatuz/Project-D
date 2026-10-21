// Project D | Codename: Zantetsuken — MatchBackfill.cs (fill empty slots after a leaver)
using System; using System.Collections.Generic;
namespace ProjectD.P7.Backfill
{
    public struct BackfillCandidate { public string playerId; public int mmr; }
    public sealed class BackfillRequest { public string sessionId; public int neededBlue, neededRed; public int targetMmr; public bool Open=>neededBlue>0||neededRed>0; }
    public static class MatchBackfill
    {
        public const int MmrTolerance=150;
        /// <summary>Pick the closest-MMR candidate within tolerance for an open slot; assigns to the shorter side.</summary>
        public static (BackfillCandidate? pick,string side) Fill(BackfillRequest req, IReadOnlyList<BackfillCandidate> pool)
        {
            if(req==null||!req.Open||pool==null||pool.Count==0) return (null,null);
            BackfillCandidate? best=null; int bestDiff=int.MaxValue;
            foreach(var c in pool){ int d=Math.Abs(c.mmr-req.targetMmr); if(d<=MmrTolerance&&d<bestDiff){ bestDiff=d; best=c; } }
            if(best==null) return (null,null);
            string side = req.neededBlue>=req.neededRed ? "Blue" : "Red";
            if(side=="Blue") req.neededBlue=Math.Max(0,req.neededBlue-1); else req.neededRed=Math.Max(0,req.neededRed-1);
            return (best,side);
        }
    }
}
