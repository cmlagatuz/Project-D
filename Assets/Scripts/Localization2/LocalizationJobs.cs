// Project D | Codename: Zantetsuken — LocalizationJobs.cs (translation workflow + gating)
using System; using System.Collections.Generic;
namespace ProjectD.P12.LocOps
{
    public enum JobState { New, InTranslation, InReview, Approved, Published, Rejected }
    public sealed class TranslationJob { public string id, language, sourceKey; public JobState state=JobState.New; public string translated; public int wordCount; }
    public sealed class LocalizationBoard
    {
        readonly Dictionary<string,TranslationJob> _jobs=new();
        public void Add(TranslationJob j){ if(j!=null&&!string.IsNullOrEmpty(j.id)) _jobs[j.id]=j; }
        public TranslationJob Get(string id)=>_jobs.TryGetValue(id,out var j)?j:null;
        /// <summary>Valid state transitions (a simple workflow state machine).</summary>
        public bool Transition(string id,JobState to)
        {
            var j=Get(id); if(j==null) return false;
            bool ok = (j.state,to) switch {
                (JobState.New,JobState.InTranslation)=>true,
                (JobState.InTranslation,JobState.InReview)=>true,
                (JobState.InReview,JobState.Approved)=>true,
                (JobState.InReview,JobState.Rejected)=>true,
                (JobState.Rejected,JobState.InTranslation)=>true,
                (JobState.Approved,JobState.Published)=>true,
                _=>false };
            if(ok) j.state=to; return ok;
        }
        /// <summary>Publish gating: a language ships only when its coverage meets the threshold.</summary>
        public float Coverage(string language)
        {
            int total=0, published=0;
            foreach(var j in _jobs.Values){ if(j.language!=language) continue; total++; if(j.state==JobState.Published) published++; }
            return total==0?0f:(float)published/total;
        }
        public bool CanShipLanguage(string language,float threshold=0.95f)=>Coverage(language)>=threshold;
    }
}
