// Project D | Codename: Zantetsuken — ModerationPipeline.cs (report triage + action routing)
using System; using System.Collections.Generic;
namespace ProjectD.P12.Moderation
{
    public enum ReportCategory { Chat, Gameplay, Cheating, Name, Fraud }
    public enum ModAction { NoAction, Warn, Mute, TempBan, PermBan, EscalateHuman }
    public sealed class ReportCase { public string id, offenderId; public ReportCategory category; public int reportCount; public float behaviorScore; public float antiCheatSeverity; public long createdUnix; public float priority; public ModAction routed=ModAction.NoAction; }
    public static class ModerationPipeline
    {
        /// <summary>Priority score: more reports, worse behavior, higher cheat severity, older = more urgent.</summary>
        public static float Priority(in ReportCase c,long now)
        {
            float reports=Math.Min(1f,c.reportCount/8f)*0.35f;
            float behavior=(1f-Clamp01(c.behaviorScore))*0.25f;
            float cheat=Clamp01(c.antiCheatSeverity/3f)*0.30f;
            float age=Clamp01((now-c.createdUnix)/86400f)*0.10f;
            return reports+behavior+cheat+age;
        }
        /// <summary>Auto-route an action; severe/ambiguous cases escalate to a human.</summary>
        public static ModAction Route(in ReportCase c)
        {
            if(c.category==ReportCategory.Cheating && c.antiCheatSeverity>=2.5f) return ModAction.PermBan;
            if(c.antiCheatSeverity>=1.5f) return ModAction.TempBan;
            if(c.reportCount>=6 && c.behaviorScore<0.3f) return ModAction.TempBan;
            if(c.reportCount>=3 && c.behaviorScore<0.45f) return ModAction.Mute;
            if(c.category==ReportCategory.Fraud || c.category==ReportCategory.Name) return ModAction.EscalateHuman;
            if(c.reportCount>=1) return ModAction.Warn;
            return ModAction.NoAction;
        }
        static float Clamp01(float v)=>v<0?0:(v>1?1:v);
    }
    public sealed class ModerationQueue
    {
        readonly List<ReportCase> _cases=new();
        public void Enqueue(ReportCase c,long now){ c.priority=ModerationPipeline.Priority(c,now); c.routed=ModerationPipeline.Route(c); _cases.Add(c); }
        /// <summary>Highest-priority cases first (triage order).</summary>
        public List<ReportCase> Triage(){ var l=new List<ReportCase>(_cases); l.Sort((a,b)=>b.priority.CompareTo(a.priority)); return l; }
        public int Pending=>_cases.Count;
    }
}
