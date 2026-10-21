// Project D | Codename: Zantetsuken — CinematicTimeline.cs (keyframed cutscene sequencer)
using System; using System.Collections.Generic;
namespace ProjectD.P9.Cinematics
{
    public enum CineEventKind { CameraMove, PlayVo, SpawnActor, Fade, ShowSubtitle, ShakeCamera, EndScene }
    public struct CineEvent { public float t; public CineEventKind kind; public string dataKey; }
    public sealed class CinematicTimeline
    {
        readonly List<CineEvent> _events=new(); public float Duration{get;private set;} float _time; int _cursor;
        public bool Playing{get;private set;} public bool Skippable=true;
        public void Add(float t,CineEventKind kind,string dataKey=null){ _events.Add(new CineEvent{t=t,kind=kind,dataKey=dataKey}); _events.Sort((a,b)=>a.t.CompareTo(b.t)); Duration=System.Math.Max(Duration,t); }
        public event Action<CineEvent> OnEvent; public event Action OnFinished;
        public void Play(){ Playing=true; _time=0; _cursor=0; }
        public List<CineEvent> Tick(float dt)
        {
            var fired=new List<CineEvent>(); if(!Playing) return fired; _time+=dt;
            while(_cursor<_events.Count && _events[_cursor].t<=_time){ var e=_events[_cursor]; fired.Add(e); OnEvent?.Invoke(e); _cursor++; }
            if(_time>=Duration){ Playing=false; OnFinished?.Invoke(); }
            return fired;
        }
        public void Skip(){ if(!Skippable) return; Playing=false; OnFinished?.Invoke(); }
        public float Progress=>Duration<=0?0f:System.Math.Min(1f,_time/Duration);
    }
}
