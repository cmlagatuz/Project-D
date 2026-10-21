// Project D | Codename: Zantetsuken — NewsFeed.cs
using System; using System.Collections.Generic;
namespace ProjectD.News
{
    public enum NewsCategory { Patch, Event, Store, Esports, Maintenance, Reward }
    [Serializable] public sealed class NewsItem { public string id, title, body, imageKey; public NewsCategory category; public long publishedUnix, expiresUnix; public bool pinned; }
    public sealed class NewsFeed
    {
        readonly List<NewsItem> _i=new(); readonly HashSet<string> _read=new();
        public void Publish(NewsItem it){ if(it!=null&&!string.IsNullOrEmpty(it.id)) _i.Add(it); }
        public void MarkRead(string id)=>_read.Add(id); public bool IsRead(string id)=>_read.Contains(id);
        public List<NewsItem> Active(long now){ var o=new List<NewsItem>(); foreach(var i in _i) if(i.expiresUnix==0||i.expiresUnix>now) o.Add(i); o.Sort((a,b)=>{ if(a.pinned!=b.pinned) return a.pinned?-1:1; return b.publishedUnix.CompareTo(a.publishedUnix); }); return o; }
        public int UnreadCount(long now){ int n=0; foreach(var i in Active(now)) if(!IsRead(i.id)) n++; return n; }
    }
}
