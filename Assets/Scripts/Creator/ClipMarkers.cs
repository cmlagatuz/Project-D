// Project D | Codename: Zantetsuken — ClipMarkers.cs
using System; using System.Collections.Generic;
namespace ProjectD.P4.Creator
{
    public enum ClipTag { Multikill, Objective, Clutch, Funny, Play, Fail }
    public sealed class ClipMarker { public string id, matchId, label; public float startT, endT; public ClipTag tag; public float Duration=>System.Math.Max(0,endT-startT); }
    public sealed class ClipEditor
    {
        readonly List<ClipMarker> _clips=new(); public float MinClip=3f, MaxClip=60f; public IReadOnlyList<ClipMarker> Clips=>_clips;
        public ClipMarker Add(string matchId,float startT,float endT,ClipTag tag,string label){ float d=endT-startT; if(d<MinClip||d>MaxClip) return null; var c=new ClipMarker{id=Guid.NewGuid().ToString("N").Substring(0,8),matchId=matchId,startT=startT,endT=endT,tag=tag,label=label}; _clips.Add(c); return c; }
        public bool Trim(string id,float startT,float endT){ foreach(var c in _clips) if(c.id==id){ float d=endT-startT; if(d<MinClip||d>MaxClip) return false; c.startT=startT; c.endT=endT; return true; } return false; }
        public bool Remove(string id)=>_clips.RemoveAll(c=>c.id==id)>0;
        public static List<(float start,float end,ClipTag tag)> Suggest(IEnumerable<(float t,ClipTag tag)> events,float pre=4f,float post=3f){ var o=new List<(float,float,ClipTag)>(); foreach(var (t,tag) in events) o.Add((System.Math.Max(0,t-pre),t+post,tag)); return o; }
    }
}
