// Project D | Codename: Zantetsuken — AntiCheatPipeline.cs (signals -> risk -> action)
using System; using System.Collections.Generic;
namespace ProjectD.P7.AntiCheat
{
    public enum CheatSignal { ImpossibleAim, SpeedHack, ManyReports, ClientTamper, MacroPattern, WinTradeSuspect }
    public enum CheatAction { None, Watch, Shadowban, Suspend, Ban }
    public sealed class RiskProfile { public string playerId; public float score; public readonly List<CheatSignal> signals=new(); }
    public static class AntiCheatPipeline
    {
        public static float Weight(CheatSignal s)=>s switch{
            CheatSignal.ImpossibleAim=>0.45f, CheatSignal.SpeedHack=>0.5f, CheatSignal.ClientTamper=>0.6f,
            CheatSignal.MacroPattern=>0.25f, CheatSignal.ManyReports=>0.2f, CheatSignal.WinTradeSuspect=>0.3f, _=>0f };
        public static void Ingest(RiskProfile p,CheatSignal s){ if(p==null) return; p.signals.Add(s); p.score=Math.Min(1f,p.score+Weight(s)); }
        public static void Decay(RiskProfile p,float amount){ if(p!=null) p.score=Math.Max(0f,p.score-amount); }
        public static CheatAction Recommend(float score)=>score>=0.9f?CheatAction.Ban:score>=0.7f?CheatAction.Suspend:score>=0.5f?CheatAction.Shadowban:score>=0.3f?CheatAction.Watch:CheatAction.None;
    }
}
