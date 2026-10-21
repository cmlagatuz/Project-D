// Project D | Codename: Zantetsuken — MissionsModel.cs (missions/quests progress + claim)
using System; using System.Collections.Generic;
namespace ProjectD.LiveOps
{
    public enum MissionCadence { Daily, Weekly, Event }
    public enum MissionRewardKind { AccountXp, KP, ChestFragment }
    [Serializable] public sealed class MissionReward { public MissionRewardKind kind; public int amount; }
    [Serializable] public sealed class MissionDefinition { public string missionId, title, description; public MissionCadence cadence=MissionCadence.Daily; public int targetProgress=1; public MissionReward[] rewards=new MissionReward[0]; public int requiredAccountLevel=0; }
    [Serializable] public sealed class MissionState { public string missionId; public int progress; public bool completed, claimed; }
    public sealed class MissionsModel
    {
        readonly Dictionary<string,MissionDefinition> _defs=new(); readonly Dictionary<string,MissionState> _state=new();
        public void Register(MissionDefinition d){ if(d==null||string.IsNullOrEmpty(d.missionId)) return; _defs[d.missionId]=d; if(!_state.ContainsKey(d.missionId)) _state[d.missionId]=new MissionState{missionId=d.missionId}; }
        public MissionDefinition Def(string id)=>_defs.TryGetValue(id,out var d)?d:null;
        public MissionState State(string id)=>_state.TryGetValue(id,out var s)?s:null;
        public IEnumerable<MissionDefinition> All=>_defs.Values;
        public void AddProgress(string id,int amt){ if(!_defs.TryGetValue(id,out var d)||!_state.TryGetValue(id,out var s)) return; if(s.completed) return; s.progress=Math.Min(d.targetProgress,s.progress+Math.Max(0,amt)); if(s.progress>=d.targetProgress) s.completed=true; }
        public bool CanClaim(string id){ var s=State(id); return s!=null&&s.completed&&!s.claimed; }
        public MissionReward[] Claim(string id){ if(!CanClaim(id)) return null; _state[id].claimed=true; return _defs[id].rewards; }
        public int UnclaimedCount(){ int n=0; foreach(var s in _state.Values) if(s.completed&&!s.claimed) n++; return n; }
        public void ResetCadence(MissionCadence c){ foreach(var kv in _defs){ if(kv.Value.cadence!=c) continue; _state[kv.Key]=new MissionState{missionId=kv.Key}; } }
    }
}
