// Project D | Codename: Zantetsuken — OnboardingFunnel.cs (staged milestones + gating)
using System; using System.Collections.Generic;
namespace ProjectD.P11.Onboarding
{
    public sealed class Milestone { public string id, title; public string[] prerequisites=new string[0]; public int rewardKp; public string unlocksFeature; }
    public sealed class OnboardingFunnel
    {
        readonly Dictionary<string,Milestone> _ms=new(); readonly HashSet<string> _done=new();
        public void Add(Milestone m){ if(m!=null&&!string.IsNullOrEmpty(m.id)) _ms[m.id]=m; }
        public bool IsUnlocked(string id){ if(!_ms.TryGetValue(id,out var m)) return false; foreach(var pre in m.prerequisites) if(!_done.Contains(pre)) return false; return true; }
        public int Complete(string id){ if(!_ms.TryGetValue(id,out var m)||_done.Contains(id)||!IsUnlocked(id)) return 0; _done.Add(id); return m.rewardKp; }
        public bool FeatureUnlocked(string feature){ foreach(var id in _done) if(_ms[id].unlocksFeature==feature) return true; return false; }
        public IEnumerable<Milestone> NextSteps(){ foreach(var m in _ms.Values) if(!_done.Contains(m.id)&&IsUnlocked(m.id)) yield return m; }
        public float Progress=>_ms.Count==0?0f:(float)_done.Count/_ms.Count;
        public bool GraduatedNewPlayer=>Progress>=1f;
    }
}
