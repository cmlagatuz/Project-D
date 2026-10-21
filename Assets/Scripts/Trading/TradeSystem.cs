// Project D | Codename: Zantetsuken — TradeSystem.cs (offer/escrow/accept + guards)
using System; using System.Collections.Generic;
namespace ProjectD.P11.Trading
{
    public enum TradeState { Open, BothAccepted, Completed, Cancelled }
    public sealed class TradeOffer { public string tradeId, playerA, playerB; public readonly List<string> itemsA=new(), itemsB=new(); public bool acceptedA, acceptedB; public TradeState state=TradeState.Open; }
    public static class TradeSystem
    {
        /// <summary>An item is tradeable only if it's flagged tradeable AND not on cooldown from a recent acquire.</summary>
        public static bool IsTradeable(bool flaggedTradeable, long acquiredUnix, long nowUnix, long cooldownSec=7*86400)
            => flaggedTradeable && (nowUnix-acquiredUnix)>=cooldownSec;
        public static bool AddItem(TradeOffer t,string playerId,string itemId,Func<string,bool> tradeableCheck)
        {
            if(t.state!=TradeState.Open) return false;
            if(!tradeableCheck(itemId)) return false;
            if(playerId==t.playerA) t.itemsA.Add(itemId); else if(playerId==t.playerB) t.itemsB.Add(itemId); else return false;
            t.acceptedA=t.acceptedB=false; // any change resets acceptance
            return true;
        }
        public static void Accept(TradeOffer t,string playerId){ if(t.state!=TradeState.Open) return; if(playerId==t.playerA) t.acceptedA=true; else if(playerId==t.playerB) t.acceptedB=true; if(t.acceptedA&&t.acceptedB) t.state=TradeState.BothAccepted; }
        /// <summary>Finalize once both accepted (server swaps ownership). Idempotent.</summary>
        public static bool Finalize(TradeOffer t){ if(t.state!=TradeState.BothAccepted) return false; t.state=TradeState.Completed; return true; }
        public static void Cancel(TradeOffer t){ if(t.state==TradeState.Open||t.state==TradeState.BothAccepted) t.state=TradeState.Cancelled; }
    }
}
