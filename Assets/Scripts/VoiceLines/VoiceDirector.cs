// Project D | Codename: Zantetsuken — VoiceDirector.cs (contextual VO + cooldown + priority)
using System; using System.Collections.Generic;
namespace ProjectD.P9.VoiceLines
{
    public enum VoContext { Spawn, Kill, Death, Recall, Objective, Taunt, LowHealth, Victory, FirstEncounter }
    public sealed class VoLine { public string id, voKey; public VoContext context; public int priority; public float cooldown=8f; public string vsChampionId; }
    public sealed class VoiceDirector
    {
        readonly List<VoLine> _lines=new(); readonly Dictionary<string,float> _lastPlayed=new();
        public void Register(VoLine l){ if(l!=null&&!string.IsNullOrEmpty(l.id)) _lines.Add(l); }
        /// <summary>Pick the highest-priority off-cooldown line for a context (optionally vs a specific enemy).</summary>
        public VoLine Select(VoContext ctx, float now, string vsChampionId=null)
        {
            VoLine best=null;
            foreach(var l in _lines)
            {
                if(l.context!=ctx) continue;
                if(!string.IsNullOrEmpty(l.vsChampionId) && l.vsChampionId!=vsChampionId) continue;
                if(_lastPlayed.TryGetValue(l.id,out float t)&&now-t<l.cooldown) continue;
                // Prefer specific-matchup lines, then priority.
                if(best==null || Score(l,vsChampionId)>Score(best,vsChampionId)) best=l;
            }
            if(best!=null) _lastPlayed[best.id]=now;
            return best;
        }
        static int Score(VoLine l,string vs){ int s=l.priority; if(!string.IsNullOrEmpty(l.vsChampionId)&&l.vsChampionId==vs) s+=100; return s; }
    }
}
