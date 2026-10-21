// Project D | Codename: Zantetsuken — SpectatorSystem.cs (delayed observer + fog options)
using System; using System.Collections.Generic;
namespace ProjectD.Spectator
{
    public enum FogMode { BlueTeam, RedTeam, BothTeams, None }
    public sealed class SpectatorSession
    {
        public float BroadcastDelaySeconds=180f; public FogMode Fog=FogMode.BothTeams; public bool IsLiveGame=true;
        readonly HashSet<string> _viewers=new();
        public int ViewerCount=>_viewers.Count; public IReadOnlyCollection<string> Viewers=>_viewers;
        public bool AddViewer(string id){ if(string.IsNullOrEmpty(id)) return false; return _viewers.Add(id); }
        public bool RemoveViewer(string id)=>_viewers.Remove(id);
        public float VisibleMatchTime(float liveMatchTime)=>IsLiveGame?Math.Max(0f,liveMatchTime-BroadcastDelaySeconds):liveMatchTime;
        public bool CanSeeTeam(string team)=>Fog switch{FogMode.None=>true,FogMode.BothTeams=>true,FogMode.BlueTeam=>team=="Blue",FogMode.RedTeam=>team=="Red",_=>false};
        public void SetFog(FogMode m)=>Fog=m;
    }
}
