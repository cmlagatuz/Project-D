// Project D | Codename: Zantetsuken — EncounterScript.cs (scripted PvE waves)
using System.Collections.Generic;
namespace ProjectD.P6.Encounters
{
    public enum TriggerKind { Time, WaveCleared, BossHpBelow, PlayersInZone }
    public struct SpawnGroup { public string monsterId; public int count; public string spawnPoint; }
    public sealed class EncounterStep { public TriggerKind trigger; public float triggerValue; public List<SpawnGroup> spawns=new(); public string message; public bool fired; }
    public sealed class EncounterScript
    {
        readonly List<EncounterStep> _s=new(); int _waves; float _bossHp=1f; int _inZone;
        public IReadOnlyList<EncounterStep> Steps=>_s;
        public void Add(EncounterStep s){ if(s!=null) _s.Add(s); }
        public void ReportWaveCleared(){ _waves++; } public void ReportBossHp(float p){ _bossHp=p; } public void ReportPlayersInZone(int n){ _inZone=n; }
        public List<SpawnGroup> Evaluate(float t){ var fired=new List<SpawnGroup>(); foreach(var s in _s){ if(s.fired) continue; bool go=s.trigger switch{TriggerKind.Time=>t>=s.triggerValue,TriggerKind.WaveCleared=>_waves>=s.triggerValue,TriggerKind.BossHpBelow=>_bossHp<=s.triggerValue,TriggerKind.PlayersInZone=>_inZone>=s.triggerValue,_=>false}; if(go){ s.fired=true; fired.AddRange(s.spawns); } } return fired; }
        public bool Complete{ get { foreach(var s in _s) if(!s.fired) return false; return true; } }
    }
}
