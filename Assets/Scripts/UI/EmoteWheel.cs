// Project D | Codename: Zantetsuken — EmoteWheel.cs (in-match emotes + cooldown + spam guard)
using System; using System.Collections.Generic;
namespace ProjectD.P5.Emotes
{
    public struct EmoteEvent { public string senderId, emoteId; public float x,z,time; }
    public sealed class EmoteWheel
    {
        public float PerEmoteCooldown=2f; public int MaxBurst=5; public float BurstWindow=8f;
        readonly Dictionary<string,float> _last=new(); readonly Dictionary<string,Queue<float>> _recent=new();
        public string[] Slots={ "emote_ggwp","emote_laugh","emote_taunt","emote_thumbsup","emote_cry","emote_dance" };
        public event Action<EmoteEvent> OnEmote;
        public bool Play(string senderId,string emoteId,float x,float z,float now)
        {
            if(_last.TryGetValue(senderId,out float l)&&now-l<PerEmoteCooldown) return false;   // per-emote cooldown
            if(!_recent.TryGetValue(senderId,out var q)){ q=new Queue<float>(); _recent[senderId]=q; }
            while(q.Count>0&&now-q.Peek()>BurstWindow) q.Dequeue();
            if(q.Count>=MaxBurst) return false;                                                  // spam guard (muted)
            _last[senderId]=now; q.Enqueue(now);
            OnEmote?.Invoke(new EmoteEvent{senderId=senderId,emoteId=emoteId,x=x,z=z,time=now});
            return true;
        }
    }
}
