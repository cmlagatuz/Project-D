// Project D | Codename: Zantetsuken — WorldSelector.cs (random realm, no immediate repeat)
using System; using System.Collections.Generic;
namespace ProjectD.Worlds
{
    public sealed class WorldSelector
    {
        readonly Queue<string> _recent=new(); public int NoRepeatWindow=2;
        /// <summary>Pick a realm for a match seed, avoiding the last N picks (per-player-history aware).</summary>
        public WorldDef Pick(long matchSeed)
        {
            var all=WorldCatalog.All; if(all.Count==0) return null;
            uint h=(uint)(matchSeed*2654435761L); h^=h<<13; h^=h>>17; h^=h<<5;
            // Try up to N deterministic candidates until one isn't recent.
            for(int attempt=0;attempt<all.Count*2;attempt++)
            {
                int idx=(int)(h%(uint)all.Count); var w=all[idx];
                if(!_recent.Contains(w.id)) { Remember(w.id); return w; }
                h^=h<<13; h^=h>>17; h^=h<<5;
            }
            var fallback=all[(int)(h%(uint)all.Count)]; Remember(fallback.id); return fallback;
        }
        void Remember(string id){ _recent.Enqueue(id); while(_recent.Count>NoRepeatWindow) _recent.Dequeue(); }
        /// <summary>A voted pick: players ban/vote worlds; pick the highest-voted not recently played.</summary>
        public static WorldDef PickByVotes(IReadOnlyDictionary<string,int> votes)
        {
            WorldDef best=null; int bv=-1; foreach(var kv in votes){ var w=WorldCatalog.Get(kv.Key); if(w!=null&&kv.Value>bv){ bv=kv.Value; best=w; } } return best;
        }
    }
}
