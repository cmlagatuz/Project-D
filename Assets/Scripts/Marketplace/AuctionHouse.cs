// Project D | Codename: Zantetsuken — AuctionHouse.cs (listing/bid/buyout + fees + anti-snipe)
using System; using System.Collections.Generic;
namespace ProjectD.P12.Marketplace
{
    public enum ListingState { Active, Sold, Expired, Cancelled }
    public sealed class Listing { public string id, sellerId, itemId; public long startPrice, buyoutPrice, currentBid; public string highBidder; public long endUnix; public ListingState state=ListingState.Active; }
    public static class AuctionHouse
    {
        public const float FeePct=0.05f;           // 5% marketplace fee on sale
        public const long AntiSnipeExtendSec=300;  // bids in the last 5 min extend the auction
        public static bool PlaceBid(Listing l,string bidderId,long amount,long nowUnix)
        {
            if(l.state!=ListingState.Active||nowUnix>=l.endUnix) return false;
            if(bidderId==l.sellerId) return false;
            long min=l.currentBid>0?l.currentBid+1:l.startPrice;
            if(amount<min) return false;
            l.currentBid=amount; l.highBidder=bidderId;
            if(l.endUnix-nowUnix<AntiSnipeExtendSec) l.endUnix=nowUnix+AntiSnipeExtendSec;  // anti-snipe
            if(l.buyoutPrice>0 && amount>=l.buyoutPrice){ l.state=ListingState.Sold; }
            return true;
        }
        public static bool Buyout(Listing l,string buyerId,long nowUnix)
        { if(l.state!=ListingState.Active||l.buyoutPrice<=0||buyerId==l.sellerId) return false; l.currentBid=l.buyoutPrice; l.highBidder=buyerId; l.state=ListingState.Sold; return true; }
        public static void Settle(Listing l,long nowUnix){ if(l.state==ListingState.Active&&nowUnix>=l.endUnix) l.state=l.highBidder!=null?ListingState.Sold:ListingState.Expired; }
        /// <summary>Seller proceeds after the marketplace fee.</summary>
        public static long SellerProceeds(long salePrice)=>(long)Math.Round(salePrice*(1-FeePct));
        public static long Fee(long salePrice)=>salePrice-SellerProceeds(salePrice);
    }
}
