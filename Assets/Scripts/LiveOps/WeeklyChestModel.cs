// Project D | Codename: Zantetsuken — WeeklyChestModel.cs (fragments -> chest -> key -> open)
using System;
namespace ProjectD.LiveOps
{
    public sealed class WeeklyChestModel
    {
        public int FragmentsPerChest=4, WeeklyChestCap=2, Fragments, Chests, Keys, ChestsEarnedThisWeek;
        public int AddFragments(int amount){ int made=0; Fragments+=Math.Max(0,amount); while(Fragments>=FragmentsPerChest && ChestsEarnedThisWeek<WeeklyChestCap){ Fragments-=FragmentsPerChest; Chests++; ChestsEarnedThisWeek++; made++; } if(ChestsEarnedThisWeek>=WeeklyChestCap && Fragments>FragmentsPerChest) Fragments=FragmentsPerChest; return made; }
        public void AddKeys(int a)=>Keys+=Math.Max(0,a);
        public bool CanOpen=>Chests>0&&Keys>0;
        public bool OpenChest(){ if(!CanOpen) return false; Chests--; Keys--; return true; }
        public void WeeklyReset()=>ChestsEarnedThisWeek=0;
        public float FragmentProgress=>FragmentsPerChest<=0?0f:(float)Fragments/FragmentsPerChest;
    }
}
