// Project D | Codename: Zantetsuken — WorldEvent.cs (server-wide community goals)
using System; using System.Collections.Generic;
namespace ProjectD.P9.WorldEvents
{
    public sealed class WorldEvent
    {
        public string id, name; public long goal; public long progress; public long startUnix, endUnix;
        public readonly List<long> milestones=new(); readonly HashSet<long> _claimed=new();
        public bool Active(long now)=>now>=startUnix&&now<=endUnix;
        public void Contribute(long amount){ if(amount>0) progress=Math.Min(goal,progress+amount); }
        public float Percent=>goal<=0?0f:(float)progress/goal;
        public bool GoalReached=>progress>=goal;
        /// <summary>Milestones the community has passed but the player hasn't claimed yet.</summary>
        public List<long> UnclaimedMilestones(){ var o=new List<long>(); foreach(var m in milestones) if(progress>=m&&!_claimed.Contains(m)) o.Add(m); return o; }
        public bool ClaimMilestone(long m){ if(progress<m||_claimed.Contains(m)) return false; return _claimed.Add(m); }
    }
}
