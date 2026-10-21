// Project D | Codename: Zantetsuken — WorldEventDirector.cs (scheduled realm events)
using System; using System.Collections.Generic;
using ProjectD.Worlds;
namespace ProjectD.Events
{
    public enum WorldEventKind { MeteorFall, TidalSurge, Eruption, Blizzard, VoidRift, RootSpread, Sandstorm, Aurora }
    public struct WorldEvent { public float t; public WorldEventKind kind; public string announceKey; public bool fired; }
    public sealed class WorldEventDirector
    {
        readonly List<WorldEvent> _events=new(); public event Action<WorldEvent> OnEvent;
        /// <summary>Build a themed event timeline for a realm (deterministic per world).</summary>
        public void BuildFor(WorldDef world)
        {
            _events.Clear();
            var kinds=KindsFor(world.modifier);
            float t=240f; int i=0;
            foreach(var k in kinds){ _events.Add(new WorldEvent{t=t,kind=k,announceKey=$"event.{k.ToString().ToLowerInvariant()}"}); t+=180f; i++; if(i>=3) break; }
        }
        static WorldEventKind[] KindsFor(WorldModifierType m)=>m switch{
            WorldModifierType.Radiant=>new[]{WorldEventKind.Aurora,WorldEventKind.MeteorFall},
            WorldModifierType.Unstable=>new[]{WorldEventKind.VoidRift,WorldEventKind.Eruption,WorldEventKind.MeteorFall},
            WorldModifierType.WildRose=>new[]{WorldEventKind.MeteorFall,WorldEventKind.Sandstorm},
            WorldModifierType.Dominion=>new[]{WorldEventKind.Eruption,WorldEventKind.MeteorFall},
            WorldModifierType.JobShift=>new[]{WorldEventKind.Aurora,WorldEventKind.VoidRift},
            WorldModifierType.Umbral=>new[]{WorldEventKind.VoidRift,WorldEventKind.Blizzard},
            WorldModifierType.Twilight=>new[]{WorldEventKind.Aurora,WorldEventKind.MeteorFall},
            WorldModifierType.LowGravity=>new[]{WorldEventKind.MeteorFall,WorldEventKind.Sandstorm},
            WorldModifierType.Gale=>new[]{WorldEventKind.Sandstorm,WorldEventKind.TidalSurge},
            WorldModifierType.Rootbound=>new[]{WorldEventKind.RootSpread,WorldEventKind.Eruption},
            _=>new[]{WorldEventKind.MeteorFall} };
        public List<WorldEvent> Tick(float matchSeconds)
        {
            var fired=new List<WorldEvent>();
            for(int i=0;i<_events.Count;i++){ var e=_events[i]; if(e.fired||matchSeconds<e.t) continue; e.fired=true; _events[i]=e; fired.Add(e); OnEvent?.Invoke(e); }
            return fired;
        }
        public int Scheduled=>_events.Count;
    }
}
