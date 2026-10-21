// Project D | Codename: Zantetsuken — SurrenderVote.cs (timed vote + thresholds + remake)
using System; using System.Collections.Generic;
namespace ProjectD.P5.Surrender
{
    public enum VoteResult { Pending, Passed, Failed }
    public sealed class SurrenderVote
    {
        public int teamSize=5; public float earliestSeconds=900f; public float voteWindowSeconds=45f; public float cooldownSeconds=180f;
        float _startedAt=-999f, _lastVoteEndAt=-999f; bool _active; readonly Dictionary<string,bool> _votes=new(); string _initiator;
        public bool CanStart(float matchSeconds)=> !_active && matchSeconds>=earliestSeconds && matchSeconds-_lastVoteEndAt>=cooldownSeconds;
        public bool Start(string initiator,float matchSeconds){ if(!CanStart(matchSeconds)) return false; _active=true; _startedAt=matchSeconds; _votes.Clear(); _initiator=initiator; _votes[initiator]=true; return true; }
        public bool CastVote(string playerId,bool yes){ if(!_active) return false; _votes[playerId]=yes; return true; }
        public int Yes(){ int n=0; foreach(var v in _votes.Values) if(v) n++; return n; }
        public int RequiredYes()=>teamSize>=5?4:teamSize>=4?3:teamSize; // 4/5, 3/4, all for smaller
        /// <summary>Seconds left before the vote window auto-closes (0 if no vote is active).</summary>
        public float RemainingWindow(float matchSeconds)=>_active?System.Math.Max(0f,voteWindowSeconds-(matchSeconds-_startedAt)):0f;
        public VoteResult Evaluate(float matchSeconds)
        {
            if(!_active) return VoteResult.Pending;
            if(Yes()>=RequiredYes()){ _active=false; _lastVoteEndAt=matchSeconds; return VoteResult.Passed; }
            if(matchSeconds-_startedAt>=voteWindowSeconds || _votes.Count>=teamSize){ _active=false; _lastVoteEndAt=matchSeconds; return VoteResult.Failed; }
            return VoteResult.Pending;
        }
        /// <summary>Early remake: if a player never connected within the grace window, allow a no-LP remake.</summary>
        public static bool RemakeAvailable(float matchSeconds,bool aPlayerNeverConnected,float graceSeconds=180f)
            => matchSeconds<=graceSeconds+15f && aPlayerNeverConnected;
    }
}
