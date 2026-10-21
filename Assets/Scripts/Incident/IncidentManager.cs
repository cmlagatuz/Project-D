// Project D | Codename: Zantetsuken — IncidentManager.cs (sev-tiered incidents + MTTR)
using System; using System.Collections.Generic;
namespace ProjectD.P12.Incidents
{
    public enum Severity { Sev4, Sev3, Sev2, Sev1 }   // Sev1 = most severe
    public enum IncidentState { Open, Investigating, Mitigated, Resolved, PostmortemDone }
    public sealed class Incident { public string id, title; public Severity severity; public IncidentState state=IncidentState.Open; public long openedUnix, resolvedUnix; public bool postmortemRequired; public string postmortemUrl; }
    public sealed class IncidentManager
    {
        readonly Dictionary<string,Incident> _incs=new();
        public Incident Open(string id,string title,Severity sev,long now){ var i=new Incident{id=id,title=title,severity=sev,openedUnix=now,postmortemRequired=sev<=Severity.Sev2}; _incs[id]=i; return i; }
        public Incident Get(string id)=>_incs.TryGetValue(id,out var i)?i:null;
        public bool Advance(string id,IncidentState to,long now)
        {
            var i=Get(id); if(i==null) return false;
            // Enforce order; Resolved requires Mitigated first; PostmortemDone requires Resolved (+ required).
            bool ok = (i.state,to) switch {
                (IncidentState.Open,IncidentState.Investigating)=>true,
                (IncidentState.Investigating,IncidentState.Mitigated)=>true,
                (IncidentState.Mitigated,IncidentState.Resolved)=>true,
                (IncidentState.Resolved,IncidentState.PostmortemDone)=>true,
                _=>false };
            if(!ok) return false;
            if(to==IncidentState.Resolved) i.resolvedUnix=now;
            i.state=to; return true;
        }
        /// <summary>A Sev1/Sev2 incident can't be fully closed until a postmortem is attached.</summary>
        public bool CanClose(string id){ var i=Get(id); if(i==null) return false; if(i.state!=IncidentState.PostmortemDone&&i.state!=IncidentState.Resolved) return false; if(i.postmortemRequired&&string.IsNullOrEmpty(i.postmortemUrl)) return false; return true; }
        public long Mttr(string id){ var i=Get(id); if(i==null||i.resolvedUnix<=0) return -1; return i.resolvedUnix-i.openedUnix; }
        public List<Incident> OpenIncidents(){ var l=new List<Incident>(); foreach(var i in _incs.Values) if(i.state!=IncidentState.Resolved&&i.state!=IncidentState.PostmortemDone) l.Add(i); l.Sort((a,b)=>((int)b.severity).CompareTo((int)a.severity)); return l; }
    }
}
