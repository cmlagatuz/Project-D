// Project D | Codename: Zantetsuken — ContentValidator.cs (champion/item linter)
using System.Collections.Generic;
namespace ProjectD.P6.Validation
{
    public enum Severity { Info, Warning, Error }
    public struct ValidationIssue { public Severity severity; public string target, message; }
    public struct ChampionData { public string id, primaryRole; public int abilityCount; public float baseHealth, baseAd; public string[] skinIds; }
    public struct ItemData { public string id; public int cost; public string[] componentIds; public int totalStats; }
    public static class ContentValidator
    {
        public static List<ValidationIssue> ValidateChampion(in ChampionData c){ var i=new List<ValidationIssue>(); if(string.IsNullOrEmpty(c.id)) i.Add(E(c.id,"Champion id is empty.")); if(c.abilityCount!=5) i.Add(E(c.id,$"Expected 5 abilities, found {c.abilityCount}.")); if(c.baseHealth<300||c.baseHealth>900) i.Add(W(c.id,$"Base health {c.baseHealth} outside 300-900.")); if(c.baseAd<40||c.baseAd>80) i.Add(W(c.id,$"Base AD {c.baseAd} outside 40-80.")); if(c.skinIds==null||c.skinIds.Length==0) i.Add(I(c.id,"No skins defined.")); return i; }
        public static List<ValidationIssue> ValidateItem(in ItemData it, ISet<string> known){ var i=new List<ValidationIssue>(); if(it.cost<0) i.Add(E(it.id,"Negative cost.")); if(it.componentIds!=null) foreach(var comp in it.componentIds) if(!known.Contains(comp)) i.Add(E(it.id,$"Missing component: {comp}")); if(it.totalStats==0&&it.cost>0) i.Add(W(it.id,"Costs gold but grants no stats.")); return i; }
        static ValidationIssue E(string t,string m)=>new(){severity=Severity.Error,target=t,message=m};
        static ValidationIssue W(string t,string m)=>new(){severity=Severity.Warning,target=t,message=m};
        static ValidationIssue I(string t,string m)=>new(){severity=Severity.Info,target=t,message=m};
    }
}
