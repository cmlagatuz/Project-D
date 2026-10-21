// Project D | Codename: Zantetsuken — MatchLoop.cs (headless match runner -> result)
using System; using System.Collections.Generic;
namespace ProjectD.P15.MatchLoop
{
    public enum MatchEndReason { NexusDestroyed, Surrender, Remake }
    public sealed class MatchResult { public bool localWon; public string winningTeam; public float durationSeconds; public int localKills, localDeaths, localAssists; public MatchEndReason reason; public int manifestationsClaimed; }
    public sealed class MatchLoopConfig { public float maxSeconds=2400f; public string localTeam="Blue"; public bool worldRift; }
    // A deterministic headless match: ticks a clock, tracks a simple objective (nexus HP
    // per team), and ends when a nexus falls or time/surrender triggers. Produces a result.
    public sealed class MatchLoop
    {
        readonly MatchLoopConfig _cfg; public float Clock{get;private set;} public bool Running{get;private set;}
        float _blueNexus=1f, _redNexus=1f;   // 1.0 = full
        int _lk,_ld,_la,_claims; string _winner;
        public MatchLoop(MatchLoopConfig cfg){ _cfg=cfg; }
        public void Start(){ Running=true; Clock=0; _blueNexus=1f; _redNexus=1f; _lk=_ld=_la=0; _claims=0; _winner=null; }
        /// <summary>Advance the sim; caller feeds per-tick pressure (which team is winning objectives).</summary>
        public void Tick(float dt, float bluePressure, float redPressure)
        {
            if(!Running) return; Clock+=dt;
            _redNexus-=bluePressure*dt*0.001f; _blueNexus-=redPressure*dt*0.001f;
            if(_redNexus<=0){ _winner="Blue"; End(MatchEndReason.NexusDestroyed); }
            else if(_blueNexus<=0){ _winner="Red"; End(MatchEndReason.NexusDestroyed); }
            else if(Clock>=_cfg.maxSeconds){ _winner=_blueNexus>=_redNexus?"Blue":"Red"; End(MatchEndReason.NexusDestroyed); }
        }
        public void RecordCombat(int k,int d,int a){ _lk+=k; _ld+=d; _la+=a; }
        public void RecordManifestationClaim(){ _claims++; }
        public void Surrender(string team){ _winner=team=="Blue"?"Red":"Blue"; End(MatchEndReason.Surrender); }
        MatchEndReason _reason; void End(MatchEndReason r){ _reason=r; Running=false; }
        public MatchResult Result()=> new MatchResult{ localWon=_winner==_cfg.localTeam, winningTeam=_winner, durationSeconds=Clock, localKills=_lk, localDeaths=_ld, localAssists=_la, reason=_reason, manifestationsClaimed=_claims };
        public float BlueNexus=>_blueNexus; public float RedNexus=>_redNexus;
    }
}
