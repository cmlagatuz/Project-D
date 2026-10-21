// Project D | Codename: Zantetsuken — TutorialModel.cs (onboarding steps, gating, unlocks)
using System; using System.Collections.Generic;
namespace ProjectD.Tutorial
{
    public enum TutorialTrigger { Move, BasicAttack, CastAbility, LastHitMinion, Recall, BuyItem, DestroyTurret, TakeObjective, UsePing, FinishBotMatch }
    [Serializable] public sealed class TutorialStep { public string stepId, titleKey, bodyKey; public TutorialTrigger completeOn; public int requiredCount=1, rewardXp, rewardKp; }
    public sealed class TutorialFlow
    {
        readonly List<TutorialStep> _steps=new(); readonly Dictionary<string,int> _prog=new(); readonly HashSet<string> _done=new(); int _i;
        public event Action<TutorialStep> OnStepStarted, OnStepCompleted; public event Action OnFlowCompleted;
        public bool IsComplete=>_i>=_steps.Count;
        public TutorialStep Current=>(_i>=0&&_i<_steps.Count)?_steps[_i]:null;
        public int Index=>_i; public int Count=>_steps.Count;
        public void SetSteps(IEnumerable<TutorialStep> s){ _steps.Clear(); _steps.AddRange(s); _i=0; _prog.Clear(); _done.Clear(); }
        public void Begin(){ _i=0; if(Current!=null) OnStepStarted?.Invoke(Current); }
        public int ProgressOf(string id)=>_prog.TryGetValue(id,out var c)?c:0;
        public void Report(TutorialTrigger tr,int amount=1){ var st=Current; if(st==null||st.completeOn!=tr) return; _prog.TryGetValue(st.stepId,out int c); c+=Math.Max(1,amount); _prog[st.stepId]=c; if(c>=st.requiredCount) CompleteCurrent(); }
        void CompleteCurrent(){ var st=Current; if(st==null||_done.Contains(st.stepId)) return; _done.Add(st.stepId); OnStepCompleted?.Invoke(st); _i++; if(IsComplete) OnFlowCompleted?.Invoke(); else OnStepStarted?.Invoke(Current); }
        public void SkipAll(){ foreach(var s in _steps) _done.Add(s.stepId); _i=_steps.Count; OnFlowCompleted?.Invoke(); }
        public bool IsStepComplete(string id)=>_done.Contains(id);
        public int CompletedCount=>_done.Count;
        public (int xp,int kp) TotalEarned(){ int xp=0,kp=0; foreach(var s in _steps) if(_done.Contains(s.stepId)){ xp+=s.rewardXp; kp+=s.rewardKp; } return (xp,kp); }
    }
    public static class FeatureUnlocks
    {
        public const int RankedUnlockLevel=10, NormalDraftUnlockLevel=5;
        public static bool RankedUnlocked(int lvl,bool tut)=>tut&&lvl>=RankedUnlockLevel;
        public static bool DraftUnlocked(int lvl)=>lvl>=NormalDraftUnlockLevel;
    }
}
