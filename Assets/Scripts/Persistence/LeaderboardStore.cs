// Project D | Codename: Zantetsuken — LeaderboardStore.cs (sorted-set store + rank queries)
using System; using System.Collections.Generic;
namespace ProjectD.P7.Persistence
{
    public struct RankedEntry { public string playerId; public long score; public int rank; }
    public sealed class LeaderboardStore
    {
        readonly Dictionary<string,long> _score=new(); List<KeyValuePair<string,long>> _sorted; bool _dirty=true;
        public void Upsert(string playerId,long score){ if(string.IsNullOrEmpty(playerId)) return; _score[playerId]=score; _dirty=true; }
        public bool Remove(string playerId){ bool r=_score.Remove(playerId); if(r) _dirty=true; return r; }
        public int Count=>_score.Count;
        void EnsureSorted(){ if(!_dirty&&_sorted!=null) return; _sorted=new List<KeyValuePair<string,long>>(_score); _sorted.Sort((a,b)=>{ int c=b.Value.CompareTo(a.Value); return c!=0?c:string.CompareOrdinal(a.Key,b.Key); }); _dirty=false; }
        public int RankOf(string playerId){ EnsureSorted(); for(int i=0;i<_sorted.Count;i++) if(_sorted[i].Key==playerId) return i+1; return -1; }
        public List<RankedEntry> Page(int page,int pageSize){ EnsureSorted(); var o=new List<RankedEntry>(); int start=Math.Max(0,page)*pageSize; for(int i=start;i<Math.Min(start+pageSize,_sorted.Count);i++) o.Add(new RankedEntry{playerId=_sorted[i].Key,score=_sorted[i].Value,rank=i+1}); return o; }
        public List<RankedEntry> Around(string playerId,int radius){ EnsureSorted(); int idx=RankOf(playerId)-1; if(idx<0) return new List<RankedEntry>(); var o=new List<RankedEntry>(); for(int i=Math.Max(0,idx-radius);i<=Math.Min(_sorted.Count-1,idx+radius);i++) o.Add(new RankedEntry{playerId=_sorted[i].Key,score=_sorted[i].Value,rank=i+1}); return o; }
    }
}
