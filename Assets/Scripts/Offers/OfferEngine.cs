// Project D | Codename: Zantetsuken — OfferEngine.cs (respectful, explainable offers)
using System; using System.Collections.Generic;
namespace ProjectD.P10.Offers
{
    public enum OfferKind { ChampionUnlock, SkinBundle, BorderIcon, XpBoost, ComebackGift }
    public sealed class Offer { public string id, name; public OfferKind kind; public int priceKp; public string relatedChampionId; public int minAccountLevel; }
    public struct OfferContext { public long kpBalance; public int accountLevel; public HashSet<string> owned; public float churnRisk; public string favoriteRole; public bool spendCapReached; }
    public struct OfferPick { public Offer offer; public float score; public string reason; }
    public static class OfferEngine
    {
        /// <summary>Pick the best-fit offer honoring ownership, level gates, spend caps, and wellbeing.</summary>
        public static OfferPick? Best(IReadOnlyList<Offer> offers, in OfferContext ctx)
        {
            if(ctx.spendCapReached) return null;                       // wellbeing guardrail
            Offer best=null; float bestScore=float.MinValue; string reason="";
            foreach(var o in offers)
            {
                if(ctx.owned!=null && ctx.owned.Contains(o.id)) continue;                  // don't re-offer owned
                if(ctx.accountLevel<o.minAccountLevel) continue;                            // gated
                if(o.kind!=OfferKind.ComebackGift && ctx.kpBalance<o.priceKp) continue;     // can't afford (gifts exempt)
                float score=0f; string r="Recommended for you";
                if(o.kind==OfferKind.ComebackGift && ctx.churnRisk>=0.5f){ score+=0.6f; r="Welcome-back gift"; }
                if(!string.IsNullOrEmpty(o.relatedChampionId)) { score+=0.2f; }
                score += 0.3f;                                                              // base fit
                if(score>bestScore){ bestScore=score; best=o; reason=r; }
            }
            return best==null?(OfferPick?)null:new OfferPick{offer=best,score=bestScore,reason=reason};
        }
    }
}
