// Project D | Codename: Zantetsuken — PredictionMarket.cs (free predict-the-winner, fun points)
using System; using System.Collections.Generic;
namespace ProjectD.P11.Predictions
{
    // A FREE, no-real-money prediction game using soft "Insight Points". Purely for
    // engagement; wagers are capped and never involve currency that costs money.
    public sealed class PredictionRound
    {
        public string matchId; public bool open=true; public string outcome;   // set when resolved
        readonly Dictionary<string,(string team,int points)> _bets=new();
        public int MaxWager=100;
        public bool Predict(string userId,string team,int points)
        {
            if(!open||string.IsNullOrEmpty(team)) return false; if(points<=0||points>MaxWager) return false;
            _bets[userId]=(team,points); return true;   // one prediction per user (replaces)
        }
        /// <summary>Resolve the round; return payouts (2x on correct, 0 on wrong).</summary>
        public Dictionary<string,int> Resolve(string winningTeam)
        {
            open=false; outcome=winningTeam; var payouts=new Dictionary<string,int>();
            foreach(var kv in _bets) payouts[kv.Key]= kv.Value.team==winningTeam ? kv.Value.points*2 : 0;
            return payouts;
        }
        public int Predictions=>_bets.Count;
    }
    public sealed class InsightWallet { public int points=100; public bool Spend(int n){ if(n<0||points<n) return false; points-=n; return true; } public void Award(int n){ if(n>0) points+=n; } }
}
