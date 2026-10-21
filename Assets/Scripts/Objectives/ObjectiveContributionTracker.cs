// Project D | Codename: Zantetsuken — ObjectiveContributionTracker.cs
using System.Collections.Generic;
namespace ProjectD.Gameplay.Objectives
{
    public sealed class ObjectiveContributionTracker
    {
        private readonly Dictionary<int,float> _teamDamage = new();
        public int LastHitTeam { get; private set; } = -1;
        public void RecordDamage(int teamId,float dmg){ if(dmg<=0f)return; _teamDamage.TryGetValue(teamId,out float c); _teamDamage[teamId]=c+dmg; LastHitTeam=teamId; }
        public float DamageBy(int teamId)=>_teamDamage.TryGetValue(teamId,out float v)?v:0f;
        public int ResolveOwner()=>LastHitTeam;
        public void Reset(){ _teamDamage.Clear(); LastHitTeam=-1; }
    }
}
