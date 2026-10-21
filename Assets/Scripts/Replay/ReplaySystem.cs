// Project D | Codename: Zantetsuken — ReplaySystem.cs (record metadata + playback timeline)
using System; using System.Collections.Generic;
namespace ProjectD.Replay
{
    [Serializable] public sealed class ReplayHeader { public string matchId, mapId; public int version=1; public long recordedUnix; public float durationSeconds; public string[] playerIds=new string[0]; public string winningTeam; }
    public enum ReplayEventKind { Kill, ObjectiveTaken, TurretDestroyed, ItemPurchased, LevelUp, Recall, TeamfightStart, Announcement }
    [Serializable] public struct ReplayEvent { public float t; public ReplayEventKind kind; public string subjectId, targetId, dataKey; }
    [Serializable] public struct ReplayKeyframe { public float t; public int byteOffset; }
    public sealed class ReplayRecording
    {
        public ReplayHeader Header=new(); public readonly List<ReplayEvent> Events=new(); public readonly List<ReplayKeyframe> Keyframes=new();
        public void AddEvent(in ReplayEvent e){ Events.Add(e); }
        public void AddKeyframe(float t,int offset){ Keyframes.Add(new ReplayKeyframe{t=t,byteOffset=offset}); }
        public int KeyframeOffsetAt(float t){ int off=0; foreach(var k in Keyframes){ if(k.t<=t) off=k.byteOffset; else break; } return off; }
        public IEnumerable<ReplayEvent> EventsBetween(float from,float to){ foreach(var e in Events) if(e.t>=from&&e.t<to) yield return e; }
    }
    public sealed class PlaybackController
    {
        public float Time{get;private set;} public float Speed=1f; public bool Playing; public float Duration{get;private set;}
        readonly ReplayRecording _rec;
        public PlaybackController(ReplayRecording rec){ _rec=rec; Duration=rec.Header.durationSeconds; }
        public void Play()=>Playing=true; public void Pause()=>Playing=false;
        public void SetSpeed(float s)=>Speed=Math.Max(0.1f,Math.Min(8f,s));
        public void Seek(float t)=>Time=Clamp(t,0,Duration);
        public List<ReplayEvent> Tick(float realDt){ var crossed=new List<ReplayEvent>(); if(!Playing) return crossed; float from=Time; Time=Clamp(Time+realDt*Speed,0,Duration); foreach(var e in _rec.EventsBetween(from,Time)) crossed.Add(e); if(Time>=Duration) Playing=false; return crossed; }
        static float Clamp(float v,float lo,float hi)=>v<lo?lo:(v>hi?hi:v);
    }
}
