// Project D | Codename: Zantetsuken — PurchaseHistory.cs
using System.Collections.Generic;
namespace ProjectD.P4.Commerce
{
    public enum Currency { KP, RealMoney }
    public sealed class Receipt { public string receiptId, itemId, itemName; public Currency currency; public long amount, purchasedUnix; public bool refunded, used; }
    public static class RefundPolicy { public const long WindowSeconds=14L*86400L; public const int MaxRefundsPerYear=3; public static bool IsRefundable(Receipt r,long now,int refundsThisYear)=>r!=null&&!r.refunded&&!r.used&&(now-r.purchasedUnix)<=WindowSeconds&&refundsThisYear<MaxRefundsPerYear; }
    public sealed class PurchaseHistory
    {
        readonly List<Receipt> _r=new(); int _refunds;
        public IReadOnlyList<Receipt> Receipts=>_r;
        public void Record(Receipt r){ if(r!=null&&!string.IsNullOrEmpty(r.receiptId)) _r.Add(r); }
        public Receipt Get(string id){ foreach(var r in _r) if(r.receiptId==id) return r; return null; }
        public bool Refund(string id,long now){ var r=Get(id); if(r==null||!RefundPolicy.IsRefundable(r,now,_refunds)) return false; r.refunded=true; _refunds++; return true; }
        public long TotalSpent(Currency c){ long t=0; foreach(var r in _r) if(r.currency==c&&!r.refunded) t+=r.amount; return t; }
    }
}
