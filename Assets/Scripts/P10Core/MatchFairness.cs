// Project D | Codename: Zantetsuken — MatchFairness.cs (per-match balance analytics)
using System; using System.Collections.Generic;
namespace ProjectD.P10.Fairness
{
    public static class MatchFairness
    {
        public static float PredictBlueWin(int blueAvg,int redAvg)=>1f/(1f+(float)Math.Pow(10.0,(redAvg-blueAvg)/400.0));
        public static float PreMatchFairness(int blueAvg,int redAvg){ float p=PredictBlueWin(blueAvg,redAvg); return 1f-2f*Math.Abs(p-0.5f); }
        public static int MmrSpread(IReadOnlyList<int> allMmr){ if(allMmr==null||allMmr.Count==0) return 0; int min=int.MaxValue,max=int.MinValue; foreach(var m in allMmr){ if(m<min) min=m; if(m>max) max=m; } return max-min; }
        public static bool WasBlowout(int goldDiff,int killDiff,float durationSec)=>Math.Abs(goldDiff)>=20000 && Math.Abs(killDiff)>=20 && durationSec<1200f;
        public static string Grade(float preFairness,bool blowout){ if(blowout) return "Poor"; return preFairness>=0.85f?"Excellent":preFairness>=0.6f?"Good":preFairness>=0.4f?"Fair":"Poor"; }
    }
}
