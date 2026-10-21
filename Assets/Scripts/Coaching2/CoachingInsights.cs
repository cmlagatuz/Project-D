// Project D | Codename: Zantetsuken — CoachingInsights.cs (post-match tips from thresholds)
using System.Collections.Generic;
namespace ProjectD.P8.Coaching
{
    public enum InsightSeverity { Praise, Tip, Warning }
    public struct Insight { public InsightSeverity severity; public string key, message; }
    public struct MatchProfile { public float minutes; public int csAt10, deaths, visionScore, kills, assists, objectivesTaken; public string role; }
    public static class CoachingInsights
    {
        public static List<Insight> Analyze(in MatchProfile m)
        {
            var outl=new List<Insight>();
            float csPerMin = m.minutes>0 ? m.csAt10/10f : 0;   // cs/min proxy from cs@10
            // CS (skip supports).
            if(m.role!="Support"){ if(m.csAt10>=80) outl.Add(P("cs","Great early farm — strong CS@10.")); else if(m.csAt10<50) outl.Add(T("cs","Focus on last-hitting: aim for 60+ CS by 10 min.")); }
            // Deaths.
            if(m.deaths>=8) outl.Add(W("deaths","High deaths — play safer and respect enemy cooldowns.")); else if(m.deaths<=2) outl.Add(P("deaths","Excellent survivability — very few deaths."));
            // Vision.
            if(m.visionScore<15 && m.minutes>=20) outl.Add(T("vision","Buy control wards and ward objectives for more vision.")); else if(m.visionScore>=40) outl.Add(P("vision","Outstanding vision control."));
            // Objectives.
            if(m.objectivesTaken==0 && m.minutes>=20) outl.Add(T("objectives","Group for objectives — they win games more reliably than kills."));
            if(outl.Count==0) outl.Add(P("general","Solid, well-rounded game."));
            return outl;
        }
        static Insight P(string k,string m)=>new(){severity=InsightSeverity.Praise,key=k,message=m};
        static Insight T(string k,string m)=>new(){severity=InsightSeverity.Tip,key=k,message=m};
        static Insight W(string k,string m)=>new(){severity=InsightSeverity.Warning,key=k,message=m};
    }
}
