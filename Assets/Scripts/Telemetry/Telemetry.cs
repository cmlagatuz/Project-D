// Project D | Codename: Zantetsuken — Telemetry.cs (event ingestion, funnels, metrics)
using System; using System.Collections.Generic;
namespace ProjectD.P7.Telemetry
{
    public struct TelemetryEvent { public string userId, name; public long unix; public Dictionary<string,string> props; }
    public sealed class TelemetrySink
    {
        readonly List<TelemetryEvent> _events=new();
        public void Track(string userId,string name,long unix,Dictionary<string,string> props=null){ if(string.IsNullOrEmpty(name)) return; _events.Add(new TelemetryEvent{userId=userId,name=name,unix=unix,props=props}); }
        public int Count(string name){ int n=0; foreach(var e in _events) if(e.name==name) n++; return n; }
        public int Unique(string name){ var set=new HashSet<string>(); foreach(var e in _events) if(e.name==name&&e.userId!=null) set.Add(e.userId); return set.Count; }

        /// <summary>Funnel conversion: fraction of users who did step[0] that also reached the last step (in order).</summary>
        public float FunnelConversion(params string[] steps)
        {
            if(steps.Length==0) return 0f;
            var progress=new Dictionary<string,int>();
            foreach(var e in _events){ if(e.userId==null) continue; progress.TryGetValue(e.userId,out int cur); int want=cur<steps.Length?Array.IndexOf(steps,e.name):-1; if(e.name==steps[cur]) progress[e.userId]=cur+1; }
            int started=0, finished=0; var seenStart=new HashSet<string>();
            foreach(var e in _events) if(e.name==steps[0]&&e.userId!=null) seenStart.Add(e.userId);
            started=seenStart.Count;
            foreach(var kv in progress) if(kv.Value>=steps.Length) finished++;
            return started==0?0f:(float)finished/started;
        }
    }
}
