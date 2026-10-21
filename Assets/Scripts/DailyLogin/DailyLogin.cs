// Project D | Codename: Zantetsuken — DailyLogin.cs (login calendar + streak bonuses)
using System;
namespace ProjectD.P9.DailyLogin
{
    public sealed class LoginState { public int streak; public int longestStreak; public long lastLoginDay; public int totalLogins; }
    public static class DailyLogin
    {
        public const long SecondsPerDay=86400;
        public static long DayOf(long unix)=>unix/SecondsPerDay;
        /// <summary>Register a login; continues the streak if consecutive, resets if a day was missed.</summary>
        public static bool RegisterLogin(LoginState s,long nowUnix)
        {
            long day=DayOf(nowUnix);
            if(day==s.lastLoginDay) return false;                 // already counted today
            if(day==s.lastLoginDay+1) s.streak++;                 // consecutive
            else s.streak=1;                                      // missed a day -> reset
            s.lastLoginDay=day; s.totalLogins++;
            if(s.streak>s.longestStreak) s.longestStreak=s.streak;
            return true;
        }
        /// <summary>Reward escalates with streak, with a weekly milestone bump every 7 days.</summary>
        public static int RewardKp(int streak){ int baseKp=25+streak*5; if(streak%7==0) baseKp+=200; return baseKp; }
    }
}
