// Project D | Codename: Zantetsuken — ReplayBookmarks.cs (tagged moments + auto-highlights)
using System; using System.Collections.Generic;
namespace ProjectD.P8.Replays
{
    public enum BookmarkTag { Kill, Teamfight, Objective, Clutch, Funny, Custom }
    public struct Bookmark { public float t; public BookmarkTag tag; public string label; }
    public sealed class ReplayBookmarks
    {
        readonly List<Bookmark> _marks=new(); public IReadOnlyList<Bookmark> Marks=>_marks;
        public void Add(float t,BookmarkTag tag,string label=null){ _marks.Add(new Bookmark{t=t,tag=tag,label=label}); }
        /// <summary>Auto-highlight: find windows where event density is highest (teamfights/multikills).</summary>
        public static List<(float start,float end,int events)> AutoHighlights(IReadOnlyList<float> eventTimes, float window=8f, int minEvents=3)
        {
            var outl=new List<(float,float,int)>(); if(eventTimes==null||eventTimes.Count==0) return outl;
            var times=new List<float>(eventTimes); times.Sort();
            int i=0;
            while(i<times.Count)
            {
                float start=times[i]; int j=i, count=0;
                while(j<times.Count && times[j]-start<=window){ count++; j++; }
                if(count>=minEvents) outl.Add((start,times[j-1],count));
                i=j>i?j:i+1;
            }
            return outl;
        }
    }
}
