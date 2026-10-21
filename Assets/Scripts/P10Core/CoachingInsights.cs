// Project D | Codename: Zantetsuken — CoachingInsights.cs (post-match tips from thresholds)
using System.Collections.Generic;
namespace ProjectD.P10.Coaching
{
    public enum InsightSeverity { Praise, Tip, Warning }
    public struct Insight { public InsightSeverity severity; public string key, message; }
    public struct MatchProfile { public float minutes; public int csAt10, deaths, visionScore, kills, assists, objectivesTaken; public string role; }
    public static class CoachingInsights
    {
        public static List<Insight> Analyze(in MatchProfile m)
        {
            var o=new List<Insight>();
            if(m.role!="Support"){ if(m.csAt10>=80) o.Add(P("cs","Great early farm — strong CS@10.")); else if(m.csAt10<50) o.Add(T("cs","Focus on last-hitting: aim for 60+ CS by 10 min.")); }
            if(m.deaths>=8) o.Add(W("deaths","High deaths — play safer and respect enemy cooldowns.")); else if(m.deaths<=2) o.Add(P("deaths","Excellent survivability — very few deaths."));
            if(m.visionScore<15 && m.minutes>=20) o.Add(T("vision","Buy control wards and ward objectives for more vision.")); else if(m.visionScore>=40) o.Add(P("vision","Outstanding vision control."));
            if(m.objectivesTaken==0 && m.minutes>=20) o.Add(T("objectives","Group for objectives — they win games more reliably than kills."));
            if(o.Count==0) o.Add(P("general","Solid, well-rounded game."));
            return o;
        }
        static Insight P(string k,string m)=>new(){severity=InsightSeverity.Praise,key=k,message=m};
        static Insight T(string k,string m)=>new(){severity=InsightSeverity.Tip,key=k,message=m};
        static Insight W(string k,string m)=>new(){severity=InsightSeverity.Warning,key=k,message=m};
    }
}
