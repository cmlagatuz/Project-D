// Project D | Codename: Zantetsuken — LocalizationQA.cs
using System.Collections.Generic; using System.Text;
namespace ProjectD.P4.LocQA
{
    public sealed class LocQaReport { public readonly List<string> MissingKeys=new(); public readonly List<string> EmptyValues=new(); public int SourceCount, TargetCount; public float Coverage=>SourceCount<=0?1f:(float)(SourceCount-MissingKeys.Count-EmptyValues.Count)/SourceCount; }
    public static class LocalizationQA
    {
        public static LocQaReport Audit(IReadOnlyDictionary<string,string> source, IReadOnlyDictionary<string,string> target){ var r=new LocQaReport{SourceCount=source.Count,TargetCount=target.Count}; foreach(var kv in source){ if(!target.TryGetValue(kv.Key,out var v)) r.MissingKeys.Add(kv.Key); else if(string.IsNullOrWhiteSpace(v)) r.EmptyValues.Add(kv.Key); } return r; }
        public static string Pseudo(string s){ if(string.IsNullOrEmpty(s)) return s; var sb=new StringBuilder("["); foreach(char c in s) sb.Append(Accent(c)); sb.Append('~'); int extra=System.Math.Max(1,s.Length/3); for(int i=0;i<extra;i++) sb.Append('x'); sb.Append(']'); return sb.ToString(); }
        static char Accent(char c)=>c switch{'a'=>'\u00e5','e'=>'\u00e9','i'=>'\u00ee','o'=>'\u00f8','u'=>'\u00fc','A'=>'\u00c5','E'=>'\u00c9','O'=>'\u00d8',_=>c};
    }
}
