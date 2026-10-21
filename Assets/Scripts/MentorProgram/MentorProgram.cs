// Project D | Codename: Zantetsuken — MentorProgram.cs (veteran<->newcomer pairing + goals)
using System; using System.Collections.Generic;
namespace ProjectD.P13.Mentorship
{
    public sealed class MentorPair { public string mentorId, apprenticeId; public long formedUnix; public int sharedGoalsCompleted; public bool graduated; }
    public sealed class SharedGoal { public string id, description; public long goal, progress; public int mentorReward, apprenticeReward; public bool Complete=>progress>=goal; public void Add(long a){ if(a>0) progress=Math.Min(goal,progress+a); } }
    public static class MentorProgram
    {
        public const int GraduationGoals=5;
        /// <summary>Eligibility: mentor is a veteran (high level), apprentice is a newcomer (low level).</summary>
        public static bool CanPair(int mentorLevel,int apprenticeLevel)=>mentorLevel>=30 && apprenticeLevel<=15;
        public static MentorPair Pair(string mentorId,string apprenticeId,int mentorLevel,int apprenticeLevel,long now)
        { if(!CanPair(mentorLevel,apprenticeLevel)) return null; return new MentorPair{mentorId=mentorId,apprenticeId=apprenticeId,formedUnix=now}; }
        public static void CompleteGoal(MentorPair p){ if(p==null||p.graduated) return; p.sharedGoalsCompleted++; if(p.sharedGoalsCompleted>=GraduationGoals) p.graduated=true; }
        public static (int mentorKp,int apprenticeKp) GraduationReward()=> (500, 300);
    }
}
