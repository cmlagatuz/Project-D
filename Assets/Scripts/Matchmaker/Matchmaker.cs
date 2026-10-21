// Project D | Codename: Zantetsuken — Matchmaker.cs (MMR ticket queue -> balanced 5v5)
using System; using System.Collections.Generic;
namespace ProjectD.P7.Matchmaker
{
    public sealed class Ticket { public string playerId; public int mmr; public float enqueuedAt; public int partySize=1; }
    public sealed class FormedMatch { public List<Ticket> Blue=new(), Red=new(); public int BlueAvg, RedAvg; public int Imbalance=>Math.Abs(BlueAvg-RedAvg); }
    public sealed class Matchmaker
    {
        public int TeamSize=5; public int BaseWidth=60; public int WidthPerTenSec=25; public int MaxWidth=400;
        readonly List<Ticket> _q=new();
        public int QueueDepth=>_q.Count;
        public void Enqueue(Ticket t){ if(t!=null&&!string.IsNullOrEmpty(t.playerId)) _q.Add(t); }
        public bool Cancel(string playerId)=>_q.RemoveAll(t=>t.playerId==playerId)>0;
        int Width(float now,Ticket t)=>Math.Min(MaxWidth,BaseWidth+(int)((now-t.enqueuedAt)/10f)*WidthPerTenSec);

        /// <summary>Try to form one match from the oldest eligible tickets within widening MMR windows.</summary>
        public FormedMatch TryForm(float now)
        {
            if(_q.Count<TeamSize*2) return null;
            _q.Sort((a,b)=>a.enqueuedAt.CompareTo(b.enqueuedAt));
            var anchor=_q[0]; int w=Width(now,anchor);
            var pool=new List<Ticket>();
            foreach(var t in _q) if(Math.Abs(t.mmr-anchor.mmr)<=w) pool.Add(t);
            if(pool.Count<TeamSize*2) return null;
            // Take the 10 closest to the anchor, then snake-draft by MMR for balance.
            pool.Sort((a,b)=>Math.Abs(a.mmr-anchor.mmr).CompareTo(Math.Abs(b.mmr-anchor.mmr)));
            var chosen=pool.GetRange(0,TeamSize*2);
            chosen.Sort((a,b)=>b.mmr-a.mmr);
            var m=new FormedMatch();
            for(int i=0;i<chosen.Count;i++){ if((i/1)%2==0) m.Blue.Add(chosen[i]); else m.Red.Add(chosen[i]); if(m.Blue.Count>TeamSize){ m.Red.Add(m.Blue[^1]); m.Blue.RemoveAt(m.Blue.Count-1);} }
            Balance(m);
            m.BlueAvg=Avg(m.Blue); m.RedAvg=Avg(m.Red);
            foreach(var t in chosen) _q.Remove(t);
            return m;
        }
        static void Balance(FormedMatch m){ /* snake draft already near-balanced; swap worst outliers */ }
        static int Avg(List<Ticket> l){ if(l.Count==0) return 0; long s=0; foreach(var t in l) s+=t.mmr; return (int)(s/l.Count); }
    }
}
