// Project D  |  Codename: Zantetsuken
// EconomyTerms.cs  (Economy — the two-economy terminology, single source of truth)
//
// OUT-OF-GAME (meta account)   : STORE  · currency KP ("Kupo")  · buys champions/borders/icons
// IN-GAME (per-match)          : SHOP   · currency Gil          · Moogle stalls · in-match items
//
// KP is a PROFILE/COSMETIC currency (no combat power), earned from account level-ups.
// Names are data so they localize/swap. ("Kupo/Gil/Moogle" are placeholders.)

namespace ProjectD.Economy
{
    public enum EconomyScope { Store, Shop }
    public enum CurrencyType { KP, Gil }
    public enum StoreCategory { Champion, ProfileBorder, ProfileIcon, Bundle }

    public static class EconomyTerms
    {
        public const string StoreSurfaceName = "Store";
        public const string ShopSurfaceName = "Shop";
        public const string KpDisplayName = "KP";
        public const string GilDisplayName = "Gil";
        public const string ShopMerchantName = "Moogle";

        public static string SurfaceName(EconomyScope s) => s == EconomyScope.Store ? StoreSurfaceName : ShopSurfaceName;
        public static CurrencyType CurrencyFor(EconomyScope s) => s == EconomyScope.Store ? CurrencyType.KP : CurrencyType.Gil;
        public static string CurrencyName(CurrencyType c) => c == CurrencyType.KP ? KpDisplayName : GilDisplayName;
        public static string Format(CurrencyType c, long amount) => $"{amount:N0} {CurrencyName(c)}";

        /// <summary>Guardrail: KP and Gil must never be spent in the same transaction.</summary>
        public static bool IsValidPurchase(EconomyScope surface, CurrencyType payWith)
            => CurrencyFor(surface) == payWith;

        public static string CurrencyKey(CurrencyType c) => c == CurrencyType.KP ? "currency.kp" : "currency.gil";
    }
}
