// Project D | Codename: Zantetsuken — IntegritySignals.cs (server-side validation hooks)
using System; using System.Collections.Generic;
namespace ProjectD.Integrity
{
    public enum SignalKind { ImpossibleMoveSpeed, ActionRateTooHigh, OutOfRangeCast, GoldMismatch, TeleportNoAbility, InputWhileDead }
    public struct IntegritySignal { public SignalKind Kind; public string PlayerId; public float Severity; public float TimeSeconds; }
    public sealed class IntegrityMonitor
    {
        public float MaxMoveSpeed=6.5f; public int MaxActionsPerSecond=12; public float FlagThreshold=3f;
        readonly Dictionary<string,Queue<float>> _actions=new(); readonly Dictionary<string,float> _accum=new();
        public event Action<IntegritySignal> OnSignal; public event Action<string> OnFlaggedForReview;
        public bool CheckMove(string playerId, float distance, float dt, float now){ if(dt<=0) return true; float speed=distance/dt; if(speed>MaxMoveSpeed*1.15f){ Raise(new IntegritySignal{Kind=SignalKind.ImpossibleMoveSpeed,PlayerId=playerId,Severity=Math.Min(3f,speed/MaxMoveSpeed),TimeSeconds=now}); return false; } return true; }
        public bool CheckActionRate(string playerId, float now){ if(!_actions.TryGetValue(playerId,out var q)){ q=new Queue<float>(); _actions[playerId]=q; } q.Enqueue(now); while(q.Count>0&&now-q.Peek()>1f) q.Dequeue(); if(q.Count>MaxActionsPerSecond){ Raise(new IntegritySignal{Kind=SignalKind.ActionRateTooHigh,PlayerId=playerId,Severity=1.5f,TimeSeconds=now}); return false; } return true; }
        public void RaiseCustom(SignalKind kind,string playerId,float severity,float now)=>Raise(new IntegritySignal{Kind=kind,PlayerId=playerId,Severity=severity,TimeSeconds=now});
        void Raise(in IntegritySignal s){ OnSignal?.Invoke(s); _accum.TryGetValue(s.PlayerId,out float a); a+=s.Severity; _accum[s.PlayerId]=a; if(a>=FlagThreshold) OnFlaggedForReview?.Invoke(s.PlayerId); }
        public float AccumulatedSeverity(string playerId)=>_accum.TryGetValue(playerId,out var a)?a:0f;
    }
}
