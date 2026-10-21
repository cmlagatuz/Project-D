// Project D | Codename: Zantetsuken — HonorSystem.cs
using System.Collections.Generic;
namespace ProjectD.Honor
{
    public enum ReportReason { Toxicity, Griefing, Afk, Cheating, Feeding, HateSpeech }
    public sealed class HonorState { public int honorLevel=2; public float behaviorScore=0.75f; public int commendationsReceived, reportsReceived; }
    public static class HonorSystem
    {
        public const int CommendationsPerMatchCap=2; public const float ReportPenalty=0.06f, CommendReward=0.02f, DecayTowardNeutral=0.01f;
        public sealed class MatchTally { public readonly Dictionary<string,int> Commends=new(); public readonly Dictionary<string,List<ReportReason>> Reports=new(); }
        public static bool Commend(MatchTally t,string from,string to,Dictionary<string,int> per){ if(from==to||string.IsNullOrEmpty(to)) return false; per.TryGetValue(from,out int u); if(u>=CommendationsPerMatchCap) return false; per[from]=u+1; t.Commends.TryGetValue(to,out int c); t.Commends[to]=c+1; return true; }
        public static void ApplyMatch(HonorState s,int commends,int reports){ s.commendationsReceived+=commends; s.reportsReceived+=reports; s.behaviorScore=C(s.behaviorScore+commends*CommendReward-reports*ReportPenalty); s.honorLevel=LevelFor(s.behaviorScore); }
        public static int LevelFor(float s)=>s>=0.92f?5:s>=0.8f?4:s>=0.6f?3:s>=0.4f?2:s>=0.2f?1:0;
        public static bool RankedAllowed(HonorState s)=>s.behaviorScore>=0.3f;
        static float C(float v)=>v<0?0:(v>1?1:v);
    }
}
