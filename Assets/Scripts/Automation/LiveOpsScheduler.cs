// Project D | Codename: Zantetsuken — LiveOpsScheduler.cs (rule/time-driven event activation)
using System; using System.Collections.Generic;
namespace ProjectD.P12.Automation
{
    public enum ScheduleKind { OneShot, Recurring }
    public sealed class ScheduledEvent { public string id, name; public ScheduleKind kind=ScheduleKind.OneShot; public long startUnix, endUnix; public long recurEverySec; public bool active; public int minAccountLevel; }
    public struct AuditEntry { public long unix; public string eventId, action; }
    public sealed class LiveOpsScheduler
    {
        readonly Dictionary<string,ScheduledEvent> _events=new(); public readonly List<AuditEntry> Audit=new();
        public void Schedule(ScheduledEvent e){ if(e!=null&&!string.IsNullOrEmpty(e.id)) _events[e.id]=e; }
        public ScheduledEvent Get(string id)=>_events.TryGetValue(id,out var e)?e:null;
        /// <summary>Advance to 'now': activate/deactivate events by their window; log an audit trail.</summary>
        public List<string> Tick(long now)
        {
            var changed=new List<string>();
            foreach(var e in _events.Values)
            {
                long start=e.startUnix, end=e.endUnix;
                if(e.kind==ScheduleKind.Recurring && e.recurEverySec>0)
                {
                    long span=end-start; if(span<=0) span=3600;
                    long since=now-start; if(since>=0){ long cycle=since%e.recurEverySec; start=now-cycle; end=start+span; }
                }
                bool shouldBeActive = now>=start && now<end;
                if(shouldBeActive!=e.active){ e.active=shouldBeActive; changed.Add(e.id); Audit.Add(new AuditEntry{unix=now,eventId=e.id,action=shouldBeActive?"activate":"deactivate"}); }
            }
            return changed;
        }
        public IEnumerable<ScheduledEvent> ActiveEvents(){ foreach(var e in _events.Values) if(e.active) yield return e; }
    }
}
