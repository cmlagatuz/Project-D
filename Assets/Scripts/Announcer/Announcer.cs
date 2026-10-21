// Project D | Codename: Zantetsuken — Announcer.cs (kill streaks, multi-kills, objective callouts)
using System; using System.Collections.Generic;
namespace ProjectD.Announcer
{
    public enum AnnouncementKind { FirstBlood, DoubleKill, TripleKill, QuadraKill, PentaKill, KillingSpree, Rampage, Unstoppable, Dominating, Legendary, FirstTurret, TurretDestroyed, InhibitorDestroyed, SummonSlain, OdinSlain, DivineSlain, ObjectiveStolen, AceEnemyTeam, CoreExposed, VictoryImminent }
    public struct Announcement { public AnnouncementKind Kind; public int Priority; public string VoKey, SfxKey, BannerTextKey, SubjectId; public float TimeSeconds; }
    public sealed class KillStreakTracker
    {
        public float MultiKillWindow=10f;
        public int Spree{get;private set;} public int MultiKillCount{get;private set;} public float LastKillTime{get;private set;}=-999f;
        public int RegisterKill(float now){ Spree++; if(now-LastKillTime<=MultiKillWindow) MultiKillCount++; else MultiKillCount=1; LastKillTime=now; return MultiKillCount>=2?MultiKillCount:0; }
        public void RegisterDeath(){ Spree=0; MultiKillCount=0; LastKillTime=-999f; }
    }
    public sealed class AnnouncementQueue
    {
        readonly List<Announcement> _pending=new(); readonly Dictionary<AnnouncementKind,float> _last=new();
        public float PerKindCooldown=1.5f; public event Action<Announcement> OnAnnouncement;
        public bool Raise(in Announcement a){ if(_last.TryGetValue(a.Kind,out float l)&&a.TimeSeconds-l<PerKindCooldown) return false; _last[a.Kind]=a.TimeSeconds; _pending.Add(a); _pending.Sort((x,y)=>y.Priority.CompareTo(x.Priority)); return true; }
        public bool Emit(out Announcement a){ a=default; if(_pending.Count==0) return false; a=_pending[0]; _pending.RemoveAt(0); OnAnnouncement?.Invoke(a); return true; }
        public int PendingCount=>_pending.Count;
    }
    public static class Announcements
    {
        public static Announcement MultiKill(int level, string subjectId, float t){ var k=level switch{2=>AnnouncementKind.DoubleKill,3=>AnnouncementKind.TripleKill,4=>AnnouncementKind.QuadraKill,_=>AnnouncementKind.PentaKill}; return Make(k,50+level*10,subjectId,t); }
        public static Announcement Spree(int spree, string subjectId, float t){ var k=spree switch{3=>AnnouncementKind.KillingSpree,4=>AnnouncementKind.Rampage,5=>AnnouncementKind.Dominating,6=>AnnouncementKind.Unstoppable,_=>AnnouncementKind.Legendary}; return Make(k,40+spree,subjectId,t); }
        public static Announcement FirstBlood(string s,float t)=>Make(AnnouncementKind.FirstBlood,80,s,t);
        public static Announcement FirstTurret(string s,float t)=>Make(AnnouncementKind.FirstTurret,60,s,t);
        public static Announcement OdinSlain(string s,float t)=>Make(AnnouncementKind.OdinSlain,90,s,t);
        public static Announcement DivineSlain(string s,float t)=>Make(AnnouncementKind.DivineSlain,85,s,t);
        public static Announcement SummonSlain(string s,float t)=>Make(AnnouncementKind.SummonSlain,70,s,t);
        public static Announcement Ace(string s,float t)=>Make(AnnouncementKind.AceEnemyTeam,95,s,t);
        public static Announcement CoreExposed(string s,float t)=>Make(AnnouncementKind.CoreExposed,100,s,t);
        static Announcement Make(AnnouncementKind kind,int prio,string s,float t){ string k=kind.ToString().ToLowerInvariant(); return new Announcement{Kind=kind,Priority=prio,SubjectId=s,TimeSeconds=t,VoKey=$"vo_announce_{k}",SfxKey="sfx_announce_stinger",BannerTextKey=$"announce.{k}"}; }
    }
}
