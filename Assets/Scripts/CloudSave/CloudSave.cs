// Project D | Codename: Zantetsuken — CloudSave.cs (versioned saves + conflict resolution)
using System; using System.Collections.Generic;
namespace ProjectD.P7.CloudSave
{
    public sealed class SaveBlob { public string userId; public int version; public long updatedUnix; public Dictionary<string,long> counters=new(); }
    public enum SaveResult { Accepted, RejectedStale, MergedConflict }
    public sealed class CloudSaveService
    {
        readonly Dictionary<string,SaveBlob> _store=new();
        public SaveBlob Load(string userId)=>_store.TryGetValue(userId,out var b)?b:null;
        /// <summary>Optimistic concurrency: accept if version==expected; if stale, MERGE by MAX per counter (never lose progress).</summary>
        public SaveResult Save(SaveBlob incoming,int expectedVersion)
        {
            if(incoming==null||string.IsNullOrEmpty(incoming.userId)) return SaveResult.RejectedStale;
            if(!_store.TryGetValue(incoming.userId,out var cur)){ incoming.version=1; _store[incoming.userId]=incoming; return SaveResult.Accepted; }
            if(cur.version==expectedVersion){ incoming.version=cur.version+1; _store[incoming.userId]=incoming; return SaveResult.Accepted; }
            // Conflict -> merge counters by max, bump version.
            var merged=new SaveBlob{userId=cur.userId,version=cur.version+1,updatedUnix=Math.Max(cur.updatedUnix,incoming.updatedUnix)};
            foreach(var kv in cur.counters) merged.counters[kv.Key]=kv.Value;
            foreach(var kv in incoming.counters){ merged.counters.TryGetValue(kv.Key,out long v); merged.counters[kv.Key]=Math.Max(v,kv.Value); }
            _store[cur.userId]=merged; return SaveResult.MergedConflict;
        }
    }
}
