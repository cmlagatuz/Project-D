// Project D | Codename: Zantetsuken — SessionContext.cs (per-run context between states)
using System.Collections.Generic;
namespace ProjectD.P15.Session
{
    public enum FlowMode { RankedSolo, RankedFlex, NormalDraft, NormalBlind, CoOpVsAI, Practice, WorldRift }
    public sealed class SessionContext
    {
        public FlowMode mode=FlowMode.NormalDraft; public string mapId="crystalline_rift"; public string worldRiftRealmId;
        public string localPlayerId="me"; public readonly List<string> party=new();
        public string selectedChampionId; public bool isCompetitive;
        public long matchSeed;
        public void Reset(){ selectedChampionId=null; worldRiftRealmId=null; matchSeed=0; }
        public bool IsWorldRift=>mode==FlowMode.WorldRift;
        public string EffectiveMap=>IsWorldRift?(worldRiftRealmId??"") : "crystalline_rift";
    }
}
