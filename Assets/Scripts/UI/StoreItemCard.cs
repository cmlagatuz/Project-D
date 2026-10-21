// Project D  |  Codename: Zantetsuken
// StoreItemCard.cs  (Store UI — one purchasable KP item tile)
//
// Shows a Store item's preview, name, rarity accent, and KP price (with a sale strike
// when discounted). Reflects OWNED / LEVEL-LOCKED / affordable states and raises a
// click for the detail/buy flow. Bind a StoreItemDefinition + the account context.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectD.Store;
using ProjectD.Economy;

namespace ProjectD.StoreUI
{
    public sealed class StoreItemCard : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image rarityFrame;
        [SerializeField] private Image preview;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text priceLabel;      // "1,500 KP"
        [SerializeField] private Text salePriceLabel;  // shown when on sale
        [SerializeField] private GameObject ownedBadge;
        [SerializeField] private GameObject lockBadge;
        [SerializeField] private Text lockText;        // "Lv 10"

        public string ItemId { get; private set; }
        public event System.Action<string> OnClicked;

        private static readonly Color[] Rarity =
        {
            new Color(0.55f,0.60f,0.66f),  // Common
            new Color(0.35f,0.62f,0.92f),  // Rare
            new Color(0.65f,0.40f,0.90f),  // Epic
            new Color(0.95f,0.72f,0.30f),  // Legendary
        };

        public void Bind(StoreItemDefinition def, bool owned, bool levelLocked, long kpBalance)
        {
            ItemId = def.itemId;
            if (nameLabel) nameLabel.text = def.displayName;
            if (preview) { preview.sprite = def.preview; if (def.preview == null) preview.color = Rarity[(int)def.rarity]; }
            if (rarityFrame) rarityFrame.color = Rarity[(int)def.rarity];

            bool onSale = def.OnSale;
            if (priceLabel)
            {
                priceLabel.text = EconomyTerms.Format(CurrencyType.KP, def.kpPrice);
                priceLabel.fontStyle = onSale ? FontStyle.Italic : FontStyle.Normal;
                priceLabel.color = onSale ? new Color(0.7f,0.7f,0.7f) : (kpBalance >= def.EffectivePrice ? new Color(0.92f,0.86f,0.55f) : new Color(0.9f,0.5f,0.45f));
            }
            if (salePriceLabel)
            {
                salePriceLabel.gameObject.SetActive(onSale);
                if (onSale) salePriceLabel.text = EconomyTerms.Format(CurrencyType.KP, def.kpSalePrice);
            }

            if (ownedBadge) ownedBadge.SetActive(owned);
            if (lockBadge) lockBadge.SetActive(levelLocked && !owned);
            if (lockText && levelLocked) lockText.text = $"Lv {def.requiredAccountLevel}";
            if (background) background.color = owned ? new Color(0.12f,0.18f,0.14f,0.95f) : new Color(0.13f,0.15f,0.21f,0.95f);
        }

        public void OnPointerClick(PointerEventData e) => OnClicked?.Invoke(ItemId);
    }
}
