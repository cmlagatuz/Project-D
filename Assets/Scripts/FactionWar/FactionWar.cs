// Project D | Codename: Zantetsuken — FactionWar.cs (persistent 3-faction meta-conflict)
using System; using System.Collections.Generic;
namespace ProjectD.P13.FactionWar
{
    public enum Faction { Order, Chaos, Balance }
    public sealed class FactionWar
    {
        public int week=1; readonly Dictionary<Faction,long> _territory=new(){{Faction.Order,0},{Faction.Chaos,0},{Faction.Balance,0}};
        readonly Dictionary<string,Faction> _pledges=new();
        public void Pledge(string playerId,Faction f){ if(!string.IsNullOrEmpty(playerId)) _pledges[playerId]=f; }
        public Faction PledgeOf(string playerId)=>_pledges.TryGetValue(playerId,out var f)?f:Faction.Balance;
        public void Contribute(string playerId,int points){ if(points<=0) return; var f=PledgeOf(playerId); _territory[f]+=points; }
        public long TerritoryOf(Faction f)=>_territory[f];
        public Faction WeeklyWinner(){ Faction best=Faction.Order; long bv=-1; foreach(var kv in _territory) if(kv.Value>bv){ bv=kv.Value; best=kv.Key; } return best; }
        /// <summary>Weekly reset: winner keeps a carryover head-start; others reset to 0.</summary>
        public Faction AdvanceWeek()
        {
            var winner=WeeklyWinner(); week++;
            long carry=_territory[winner]/10;   // 10% carryover advantage
            _territory[Faction.Order]=0; _territory[Faction.Chaos]=0; _territory[Faction.Balance]=0;
            _territory[winner]=carry;
            return winner;
        }
        public float Share(Faction f){ long total=0; foreach(var v in _territory.Values) total+=v; return total==0?0f:(float)_territory[f]/total; }
    }
}
