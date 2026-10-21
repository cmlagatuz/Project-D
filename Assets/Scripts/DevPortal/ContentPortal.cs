// Project D | Codename: Zantetsuken — ContentPortal.cs (submit -> review -> publish + versioning)
using System; using System.Collections.Generic;
namespace ProjectD.P12.DevPortal
{
    public enum SubmissionState { Draft, Submitted, InReview, ChangesRequested, Approved, Published }
    public sealed class ContentSubmission { public string id, authorId, title, type; public int version=1; public SubmissionState state=SubmissionState.Draft; public string reviewerId, reviewNote; }
    public sealed class ContentPortal
    {
        readonly Dictionary<string,ContentSubmission> _subs=new(); readonly Dictionary<string,int> _publishedVersion=new();
        public void Create(ContentSubmission s){ if(s!=null&&!string.IsNullOrEmpty(s.id)) _subs[s.id]=s; }
        public ContentSubmission Get(string id)=>_subs.TryGetValue(id,out var s)?s:null;
        public bool Submit(string id){ var s=Get(id); if(s==null||s.state!=SubmissionState.Draft&&s.state!=SubmissionState.ChangesRequested) return false; s.state=SubmissionState.Submitted; return true; }
        public bool StartReview(string id,string reviewerId){ var s=Get(id); if(s==null||s.state!=SubmissionState.Submitted) return false; s.state=SubmissionState.InReview; s.reviewerId=reviewerId; return true; }
        public bool RequestChanges(string id,string note){ var s=Get(id); if(s==null||s.state!=SubmissionState.InReview) return false; s.state=SubmissionState.ChangesRequested; s.reviewNote=note; s.version++; return true; }
        public bool Approve(string id){ var s=Get(id); if(s==null||s.state!=SubmissionState.InReview) return false; s.state=SubmissionState.Approved; return true; }
        public bool Publish(string id){ var s=Get(id); if(s==null||s.state!=SubmissionState.Approved) return false; s.state=SubmissionState.Published; _publishedVersion[id]=s.version; return true; }
        public int PublishedVersion(string id)=>_publishedVersion.TryGetValue(id,out var v)?v:0;
        /// <summary>Authors cannot review their own content (separation of duties).</summary>
        public static bool CanReview(string reviewerId,string authorId)=>reviewerId!=authorId;
    }
}
