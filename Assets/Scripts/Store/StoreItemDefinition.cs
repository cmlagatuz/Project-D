// Project D  |  Codename: Zantetsuken
// StoreItemDefinition.cs  (Store — a KP-purchasable meta item)
//
// One asset per Store item: a champion unlock, a profile border, a profile icon, or
// a bundle. All are cosmetic/unlock only (no combat power) and priced in KP. This is
// the OUT-OF-GAME Store (distinct from the in-match Gil Shop). Data-driven so live-
// ops can add/price items without code.

using System;
using UnityEngine;
using ProjectD.Economy;

namespace ProjectD.Store
{
    public enum StoreRarity { Common, Rare, Epic, Legendary }

    [CreateAssetMenu(fileName = "StoreItem_", menuName = "Project D/Store Item (KP)")]
    public sealed class StoreItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;                 // e.g. champ_kain, border_crystal_s1, icon_moogle
        public string displayName;
        [TextArea(1, 3)] public string description;
        public StoreCategory category = StoreCategory.ProfileIcon;
        public StoreRarity rarity = StoreRarity.Common;
        public Sprite preview;                // icon/border art or champion splash

        [Header("Price (KP only)")]
        public int kpPrice = 500;
        [Tooltip("Optional sale price; <=0 means no sale.")]
        public int kpSalePrice = 0;

        [Header("Champion link (category == Champion)")]
        [Tooltip("Champion id this unlocks (for Champion category).")]
        public string championId = "";

        [Header("Unlock gating")]
        [Tooltip("Account level required to purchase (0 = none).")]
        public int requiredAccountLevel = 0;
        [Tooltip("If true this item is featured/rotated in the storefront.")]
        public bool featured = false;

        [Header("Bundle contents (category == Bundle)")]
        [Tooltip("Item ids granted by this bundle.")]
        public string[] bundleItemIds = new string[0];

        public bool OnSale => kpSalePrice > 0 && kpSalePrice < kpPrice;
        public int EffectivePrice => OnSale ? kpSalePrice : kpPrice;
    }
}
