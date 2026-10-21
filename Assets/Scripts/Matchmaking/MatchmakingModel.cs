// Project D | Codename: Zantetsuken — MatchmakingModel.cs (mode picker, queue, match-found, hints)
using System; using System.Collections.Generic;
namespace ProjectD.Matchmaking
{
    public enum GameMode { RankedSolo, RankedFlex, Normal, CoOpVsAI, Custom, Practice }
    public enum QueueState { Idle, Searching, MatchFound, Accepted, Declined, Cancelled }
    public struct QueueStatus { public GameMode Mode; public QueueState State; public float ElapsedSeconds; public int EstimatedWaitSeconds, BracketWidth; }
    public sealed class MatchmakingModel
    {
        public GameMode SelectedMode{get;private set;}=GameMode.Normal;
        public QueueState State{get;private set;}=QueueState.Idle;
        public float Elapsed{get;private set;} public int PlayerMmr{get;set;}=1000;
        public int BaseBracket=60, BracketPerTenSeconds=25, MaxBracket=300;
        public event Action OnMatchFound, OnQueueCancelled;
        public void SelectMode(GameMode m){ if(State==QueueState.Idle) SelectedMode=m; }
        public void StartQueue(){ if(State!=QueueState.Idle) return; State=QueueState.Searching; Elapsed=0f; }
        public void Cancel(){ if(State==QueueState.Searching||State==QueueState.MatchFound){ State=QueueState.Cancelled; OnQueueCancelled?.Invoke(); State=QueueState.Idle; } }
        public void Tick(float dt){ if(State==QueueState.Searching) Elapsed+=dt; }
        public void NotifyMatchFound(){ if(State!=QueueState.Searching) return; State=QueueState.MatchFound; OnMatchFound?.Invoke(); }
        public void Accept(){ if(State==QueueState.MatchFound) State=QueueState.Accepted; }
        public void Decline(){ if(State==QueueState.MatchFound){ State=QueueState.Declined; State=QueueState.Idle; } }
        public int CurrentBracket(){ int w=BaseBracket+(int)(Elapsed/10f)*BracketPerTenSeconds; return Math.Min(w,MaxBracket); }
        public (int min,int max) Bracket(){ int w=CurrentBracket(); return (Math.Max(0,PlayerMmr-w),PlayerMmr+w); }
        public QueueStatus Status()=>new QueueStatus{Mode=SelectedMode,State=State,ElapsedSeconds=Elapsed,EstimatedWaitSeconds=EstWait(),BracketWidth=CurrentBracket()};
        int EstWait()=>SelectedMode switch{GameMode.RankedSolo or GameMode.RankedFlex=>45,GameMode.CoOpVsAI=>20,GameMode.Custom or GameMode.Practice=>0,_=>30};
    }
    public enum PreGameHint { MapMirrored, MoveToBestLane, ObjectivesRevealed, WardTheRiver }
    public static class PreGameHints
    {
        public static IEnumerable<string> Sequence(bool mirrored){ if(mirrored) yield return "hint.map_mirrored"; yield return "hint.move_to_best_lane"; yield return "hint.objectives_revealed"; yield return "hint.ward_the_river"; }
    }
}
