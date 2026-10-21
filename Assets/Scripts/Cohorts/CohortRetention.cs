// Project D | Codename: Zantetsuken — CohortRetention.cs (D1/D7/D30 + rolling actives)
using System; using System.Collections.Generic;
namespace ProjectD.P10.Cohorts
{
    public sealed class CohortAnalyzer
    {
        const long Day=86400;
        // userId -> install day, and userId -> set of active days.
        readonly Dictionary<string,long> _installDay=new(); readonly Dictionary<string,HashSet<long>> _active=new();
        public void RecordInstall(string userId,long unix){ if(!_installDay.ContainsKey(userId)) _installDay[userId]=unix/Day; }
        public void RecordActive(string userId,long unix){ if(!_active.TryGetValue(userId,out var set)){ set=new HashSet<long>(); _active[userId]=set; } set.Add(unix/Day); }
        /// <summary>Fraction of a day's install cohort that was active exactly dayN days later.</summary>
        public float RetentionDay(long installDayUnix,int dayN)
        {
            long cohortDay=installDayUnix/Day; int cohort=0, retained=0;
            foreach(var kv in _installDay){ if(kv.Value!=cohortDay) continue; cohort++; if(_active.TryGetValue(kv.Key,out var set)&&set.Contains(cohortDay+dayN)) retained++; }
            return cohort==0?0f:(float)retained/cohort;
        }
        /// <summary>Distinct active users within [now-windowDays, now].</summary>
        public int ActiveUsers(long nowUnix,int windowDays)
        {
            long today=nowUnix/Day; var users=new HashSet<string>();
            foreach(var kv in _active) foreach(var d in kv.Value) if(d>today-windowDays&&d<=today){ users.Add(kv.Key); break; }
            return users.Count;
        }
        public int CohortSize(long installDayUnix){ long d=installDayUnix/Day; int n=0; foreach(var v in _installDay.Values) if(v==d) n++; return n; }
    }
}
