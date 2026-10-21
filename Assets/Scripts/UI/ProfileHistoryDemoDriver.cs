// Project D | Codename: Zantetsuken — ProfileHistoryDemoDriver.cs (sample history for the profile)
using UnityEngine; using ProjectD.History; using ProjectD.HistoryUI;
namespace ProjectD.HistoryUI
{
    public sealed class ProfileHistoryDemoDriver : MonoBehaviour
    {
        [SerializeField] private ProfileHistoryView view;
        void Start()
        {
            if(view==null) return;
            var h=new MatchHistory(); long now=1_700_000_000;
            void Rec(string id,string champ,string mode,MatchOutcome o,int k,int d,int a,int cs,float dur,bool mvp,int claims=0)
                => h.Record(new MatchHistoryEntry{matchId=id,championId=champ,mode=mode,mapId="crystalline_rift",outcome=o,kills=k,deaths=d,assists=a,cs=cs,durationSeconds=dur,playedUnix=now,wasMvp=mvp,manifestationsClaimed=claims});
            // Recorded oldest->newest; store inserts newest-first, so call oldest first.
            Rec("m1","cecil","Ranked",MatchOutcome.Defeat,3,7,10,180,1720,false);
            Rec("m2","kain","Normal",MatchOutcome.Victory,9,4,6,205,1580,false);
            Rec("m3","kain","World Rift",MatchOutcome.Victory,12,3,8,215,1875,true,2);
            Rec("m4","firion","Ranked",MatchOutcome.Victory,7,5,9,230,1650,false);
            Rec("m5","kain","Ranked",MatchOutcome.Victory,11,2,7,240,1490,true);
            view.OnOpenMatch += id => Debug.Log($"[Project D] Open post-game analytics for match {id}");
            view.Bind(h, 10);
        }
    }
}
