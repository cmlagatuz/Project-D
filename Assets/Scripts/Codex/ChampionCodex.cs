// Project D | Codename: Zantetsuken — ChampionCodex.cs (unlockable lore entries)
using System.Collections.Generic;
namespace ProjectD.P9.Codex
{
    public enum CodexUnlockType { Free, MasteryLevel, Achievement, Quest }
    public sealed class CodexEntry { public string id, championId, title, body; public CodexUnlockType unlock=CodexUnlockType.Free; public int requiredMastery; public string requiredKey; }
    public sealed class Codex
    {
        readonly Dictionary<string,CodexEntry> _entries=new(); readonly HashSet<string> _read=new();
        public void Add(CodexEntry e){ if(e!=null&&!string.IsNullOrEmpty(e.id)) _entries[e.id]=e; }
        public bool IsUnlocked(CodexEntry e, int mastery, ISet<string> achievements, ISet<string> quests)
        { return e.unlock switch{ CodexUnlockType.Free=>true, CodexUnlockType.MasteryLevel=>mastery>=e.requiredMastery, CodexUnlockType.Achievement=>achievements!=null&&achievements.Contains(e.requiredKey), CodexUnlockType.Quest=>quests!=null&&quests.Contains(e.requiredKey), _=>false }; }
        public List<CodexEntry> ForChampion(string championId,int mastery,ISet<string> ach,ISet<string> quests,bool includeLocked=true)
        { var o=new List<CodexEntry>(); foreach(var e in _entries.Values){ if(e.championId!=championId) continue; if(includeLocked||IsUnlocked(e,mastery,ach,quests)) o.Add(e); } return o; }
        public void MarkRead(string id)=>_read.Add(id); public bool IsRead(string id)=>_read.Contains(id);
        public int UnlockedCount(string championId,int mastery,ISet<string> ach,ISet<string> quests){ int n=0; foreach(var e in _entries.Values) if(e.championId==championId&&IsUnlocked(e,mastery,ach,quests)) n++; return n; }
    }
}
