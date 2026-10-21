// Project D  |  Codename: Zantetsuken
// StoreItemCard.cs  (Gold Shop UI — one purchasable in-match item tile)
//
// NOTE: there is a DIFFERENT "StoreItemCard" at Assets/Scripts/UI/StoreItemCard.cs in
// namespace ProjectD.StoreUI — that one is for the out-of-game KP store and binds
// StoreItemDefinition. This one is for the in-match gold shop (Editor/StoreScreenBuilder.cs)
// and binds ItemDefinition. Same simple name, different namespace/folder — not a duplicate.
//
// Field names below are load-bearing: the builder assigns them via
// SerializedObject.FindProperty("background"/"frame"/"icon"/"exclusiveRibbon"/
// "recommendBadge"/"nameLabel"/"costLabel"/"ownedCheck"/"quickBuyButton").

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectD.Store;

namespace ProjectD.UI.Store
{
    public sealed class StoreItemCard : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;
        [SerializeField] private Image exclusiveRibbon;
        [SerializeField] private Image recommendBadge;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text costLabel;
        [SerializeField] private GameObject ownedCheck;
        [SerializeField] private Button quickBuyButton; // only wired on the quick-buy row variant

        public string ItemId { get; private set; } = "";
        public event System.Action<string> OnClicked;
        public event System.Action<string> OnQuickBuyClicked;

        private static readonly Color OwnedBg = new Color(0.14f, 0.20f, 0.16f);
        private static readonly Color DefaultBg = new Color(0.14f, 0.16f, 0.23f);

        private void Awake()
        {
            if (quickBuyButton) quickBuyButton.onClick.AddListener(() => OnQuickBuyClicked?.Invoke(ItemId));
        }

        public void Bind(ItemDefinition item, bool owned, bool recommended, bool affordable)
        {
            if (item == null) return;
            ItemId = item.itemId;
            if (nameLabel) nameLabel.text = item.displayName;
            if (icon) { icon.sprite = item.icon; icon.enabled = item.icon != null; }
            if (costLabel)
            {
                costLabel.text = $"{item.totalCost}g";
                costLabel.color = affordable ? new Color(0.92f, 0.86f, 0.55f) : new Color(0.9f, 0.5f, 0.45f);
            }
            if (exclusiveRibbon) exclusiveRibbon.gameObject.SetActive(item.IsChampionExclusive);
            if (recommendBadge) recommendBadge.gameObject.SetActive(recommended);
            if (ownedCheck) ownedCheck.SetActive(owned);
            if (background) background.color = owned ? OwnedBg : DefaultBg;
            if (quickBuyButton) quickBuyButton.interactable = affordable && !owned;
        }

        public void OnPointerClick(PointerEventData e) => OnClicked?.Invoke(ItemId);
    }
}
