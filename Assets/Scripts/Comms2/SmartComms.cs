// Project D | Codename: Zantetsuken — SmartComms.cs (smart pings + chat wheel + mute)
using System; using System.Collections.Generic;
namespace ProjectD.P8.Comms
{
    public enum SmartPingContext { OnEnemy, OnObjective, OnGround, OnAlly, OnMissing }
    public enum PingIntent { Danger, AssistMe, OnMyWay, EnemyMissing, GroupUp, TakeObjective, Retreat, Careful }
    public static class SmartPing
    {
        /// <summary>Auto-choose the ping intent from what the player pinged on (context-aware).</summary>
        public static PingIntent Resolve(SmartPingContext ctx)=>ctx switch{
            SmartPingContext.OnEnemy=>PingIntent.Danger, SmartPingContext.OnObjective=>PingIntent.TakeObjective,
            SmartPingContext.OnAlly=>PingIntent.AssistMe, SmartPingContext.OnMissing=>PingIntent.EnemyMissing, _=>PingIntent.OnMyWay };
        public static bool TeamWide(PingIntent i)=>i==PingIntent.EnemyMissing||i==PingIntent.TakeObjective||i==PingIntent.GroupUp;
    }
    // Chat-wheel categories + a mute registry (per-target, honoring behavior restrictions).
    public sealed class ChatModeration
    {
        readonly HashSet<string> _muted=new(); public bool GlobalChatDisabled;
        public void Mute(string playerId){ if(!string.IsNullOrEmpty(playerId)) _muted.Add(playerId); }
        public void Unmute(string playerId)=>_muted.Remove(playerId);
        public bool IsMuted(string playerId)=>_muted.Contains(playerId);
        /// <summary>Should a message from sender be shown to receiver? (mute + global toggle + restriction).</summary>
        public bool ShouldDeliver(string senderId,bool senderChatRestricted)
        { if(GlobalChatDisabled) return false; if(senderChatRestricted) return false; if(IsMuted(senderId)) return false; return true; }
    }
}
