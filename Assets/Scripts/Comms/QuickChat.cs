// Project D | Codename: Zantetsuken — QuickChat.cs (ping wheel + quick-chat, rate-limited)
using System; using System.Collections.Generic;
namespace ProjectD.Comms
{
    public enum PingType { Danger, OnMyWay, AssistMe, Retreat, EnemyMissing, Objective, NeedVision, PushLane, HoldLane, AllIn, Bait, Wait }
    public struct PingEvent { public PingType Type; public string SenderId, TextKey, SfxKey; public float WorldX, WorldZ, TimeSeconds; public bool TeamWide; }
    public sealed class PingRateLimiter
    {
        public int MaxBurst=4; public float WindowSeconds=4f; readonly Dictionary<string,Queue<float>> _r=new();
        public bool Allow(string sender,float now){ if(!_r.TryGetValue(sender,out var q)){ q=new Queue<float>(); _r[sender]=q; } while(q.Count>0&&now-q.Peek()>WindowSeconds) q.Dequeue(); if(q.Count>=MaxBurst) return false; q.Enqueue(now); return true; }
    }
    public static class QuickChat
    {
        public static PingEvent Build(PingType type, string sender, float x, float z, float t){ string k=type.ToString().ToLowerInvariant(); return new PingEvent{Type=type,SenderId=sender,WorldX=x,WorldZ=z,TimeSeconds=t,TextKey=$"ping.{k}",SfxKey=$"sfx_ping_{k}",TeamWide=type==PingType.Objective||type==PingType.EnemyMissing||type==PingType.NeedVision}; }
        public static PingType[] DefaultWheel=>new[]{PingType.Danger,PingType.Objective,PingType.OnMyWay,PingType.AssistMe,PingType.EnemyMissing,PingType.Retreat};
        public static readonly string[] QuickChatPhrases={ "chat.good_luck","chat.well_played","chat.my_bad","chat.thanks","chat.group_up","chat.take_objective","chat.defend","chat.careful" };
        public static string Label(PingType t)=>t switch{PingType.Danger=>"Danger!",PingType.OnMyWay=>"On My Way",PingType.AssistMe=>"Assist Me",PingType.Retreat=>"Retreat!",PingType.EnemyMissing=>"Enemy Missing",PingType.Objective=>"Objective",PingType.NeedVision=>"Need Vision",PingType.PushLane=>"Push",PingType.HoldLane=>"Hold",PingType.AllIn=>"All In!",PingType.Bait=>"Bait",PingType.Wait=>"Wait",_=>t.ToString()};
    }
    public sealed class PingBroadcaster
    {
        readonly PingRateLimiter _lim=new(); public event Action<PingEvent> OnPingBroadcast;
        public bool Send(PingType type, string sender, float x, float z, float now){ if(!_lim.Allow(sender,now)) return false; OnPingBroadcast?.Invoke(QuickChat.Build(type,sender,x,z,now)); return true; }
    }
}
