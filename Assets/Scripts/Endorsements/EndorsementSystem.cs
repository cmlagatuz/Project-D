// Project D | Codename: Zantetsuken — EndorsementSystem.cs (positive-only reputation)
using System; using System.Collections.Generic;
namespace ProjectD.P11.Endorsements
{
    public enum EndorseKind { Leadership, GoodTeammate, ShotCaller, GreatAttitude, Clutch }
    public sealed class ReputationState { public int level=1; public float progress; public readonly Dictionary<EndorseKind,int> received=new(); }
    public static class EndorsementSystem
    {
        public const int PerMatchCap=2;   // each player may endorse up to N teammates
        public static bool Endorse(ReputationState target, EndorseKind kind, Dictionary<string,int> perVoter, string voterId)
        {
            if(perVoter!=null){ perVoter.TryGetValue(voterId,out int used); if(used>=PerMatchCap) return false; perVoter[voterId]=used+1; }
            target.received.TryGetValue(kind,out int c); target.received[kind]=c+1;
            target.progress+=0.15f; while(target.progress>=1f && target.level<5){ target.progress-=1f; target.level++; }
            if(target.level>=5) target.progress=Math.Min(target.progress,0.99f);
            return true;
        }
        public static int TotalEndorsements(ReputationState s){ int t=0; foreach(var v in s.received.Values) t+=v; return t; }
        public static EndorseKind TopTrait(ReputationState s){ EndorseKind best=EndorseKind.GoodTeammate; int bv=-1; foreach(var kv in s.received) if(kv.Value>bv){ bv=kv.Value; best=kv.Key; } return best; }
    }
}
