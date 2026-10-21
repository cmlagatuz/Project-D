// Project D | Codename: Zantetsuken — QuestChain.cs (ordered story beats + prerequisites)
using System.Collections.Generic;
namespace ProjectD.P9.Narrative
{
    public sealed class QuestBeat { public string id, title, body; public string[] prerequisites=new string[0]; public int rewardXp, rewardKp; public bool completed; }
    public sealed class QuestChain
    {
        readonly Dictionary<string,QuestBeat> _beats=new(); readonly HashSet<string> _done=new();
        public void Add(QuestBeat b){ if(b!=null&&!string.IsNullOrEmpty(b.id)) _beats[b.id]=b; }
        public bool IsUnlocked(string id){ if(!_beats.TryGetValue(id,out var b)) return false; foreach(var pre in b.prerequisites) if(!_done.Contains(pre)) return false; return true; }
        public bool Complete(string id){ if(!_beats.TryGetValue(id,out var b)||b.completed||!IsUnlocked(id)) return false; b.completed=true; _done.Add(id); return true; }
        public IEnumerable<QuestBeat> Available(){ foreach(var b in _beats.Values) if(!b.completed&&IsUnlocked(b.id)) yield return b; }
        public int CompletedCount=>_done.Count; public int Total=>_beats.Count;
        public bool ChainComplete=>_done.Count==_beats.Count&&_beats.Count>0;
    }
}
