// Project D  |  Codename: Zantetsuken
// KpStoreScreen.cs  (Store UI — the out-of-game KP storefront)
//
// Binds the KpStoreService to the UI: a currency header (level + XP + KP), category
// tabs (Champions / Borders / Icons / Bundles / Featured), a grid of StoreItemCards,
// and a detail/buy panel that confirms a KP purchase (guardrail: KP only). Owned and
// level-locked items are reflected; buying deducts KP + grants ownership + refreshes.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Account;
using ProjectD.Store;
using ProjectD.Economy;

namespace ProjectD.StoreUI
{
    public sealed class KpStoreScreen : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private List<StoreItemDefinition> catalog = new();

        [Header("Header")]
        [SerializeField] private CurrencyHeader currencyHeader;

        [Header("Tabs")]
        [SerializeField] private Button[] categoryButtons;   // Featured, Champion, Border, Icon, Bundle
        [SerializeField] private Text[] categoryLabels;

        [Header("Grid")]
        [SerializeField] private RectTransform gridContent;
        [SerializeField] private StoreItemCard cardPrefab;

        [Header("Detail / buy")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailPreview;
        [SerializeField] private Text detailName;
        [SerializeField] private Text detailDescription;
        [SerializeField] private Text detailPrice;
        [SerializeField] private Button buyButton;
        [SerializeField] private Text buyButtonLabel;
        [SerializeField] private Button equipButton;
        [SerializeField] private Text feedbackLabel;

        [Header("Level-up")]
        [SerializeField] private LevelUpPopup levelUpPopup;

        private readonly KpStoreService _service = new();
        private readonly Dictionary<string, StoreItemDefinition> _defs = new();
        private readonly List<StoreItemCard> _cards = new();
        private string _selectedId;

        // Tab order: index 0 = Featured (special), 1..4 map to StoreCategory.
        private int _activeTab = 0;

        private void Start()
        {
            foreach (var d in catalog) if (d && !string.IsNullOrEmpty(d.itemId)) _defs[d.itemId] = d;

            RegisterCatalog();

            _service.Initialize();
            _service.OnAccountChanged += _ => RefreshHeader();
            _service.OnPurchased += _ => { RefreshGrid(); RefreshHeader(); };

            HookTabs();
            if (buyButton) buyButton.onClick.AddListener(BuySelected);
            if (equipButton) equipButton.onClick.AddListener(EquipSelected);

            SelectTab(0);
            RefreshHeader();
        }

        private void RegisterCatalog()
        {
            foreach (var d in catalog)
            {
                if (d == null || string.IsNullOrEmpty(d.itemId)) continue;
                _service.Store.Register(new StoreItemView
                {
                    ItemId = d.itemId, Category = d.category, KpPrice = d.kpPrice, KpSalePrice = d.kpSalePrice,
                    RequiredAccountLevel = d.requiredAccountLevel, ChampionId = d.championId,
                    BundleItemIds = d.bundleItemIds ?? new string[0]
                });
            }
        }

        /// <summary>Apply post-match / event XP (call from the results flow).</summary>
        public void GrantXp(int xp)
        {
            var r = _service.AddXp(xp);
            if (r.LeveledUp && levelUpPopup) levelUpPopup.Present(r);
            RefreshHeader();
        }

        // ---- tabs ----
        private void HookTabs()
        {
            for (int i = 0; i < categoryButtons.Length; i++)
            {
                int idx = i;
                if (categoryButtons[i]) categoryButtons[i].onClick.AddListener(() => SelectTab(idx));
            }
        }

        private void SelectTab(int tab)
        {
            _activeTab = tab;
            for (int i = 0; i < categoryButtons.Length; i++)
                Highlight(categoryButtons[i], i == tab);
            RefreshGrid();
        }

        private IEnumerable<StoreItemDefinition> ItemsForTab(int tab)
        {
            foreach (var d in catalog)
            {
                if (d == null) continue;
                if (tab == 0) { if (d.featured) yield return d; }
                else if ((int)d.category == tab - 1) yield return d;
            }
        }

        // ---- grid ----
        private void RefreshGrid()
        {
            foreach (var c in _cards) if (c) Destroy(c.gameObject);
            _cards.Clear();
            if (gridContent == null || cardPrefab == null) return;

            foreach (var def in ItemsForTab(_activeTab))
            {
                bool owned = IsOwned(def);
                bool levelLocked = def.requiredAccountLevel > 0 && _service.Account.level < def.requiredAccountLevel;
                var card = Instantiate(cardPrefab, gridContent);
                card.Bind(def, owned, levelLocked, _service.Account.kp);
                card.OnClicked += ShowDetail;
                _cards.Add(card);
            }

            if (_cards.Count > 0) ShowDetail(_cards[0].ItemId);
            else if (detailPanel) detailPanel.SetActive(false);
        }

        // ---- detail / buy ----
        private void ShowDetail(string itemId)
        {
            _selectedId = itemId;
            if (!_defs.TryGetValue(itemId, out var d)) return;
            if (detailPanel) detailPanel.SetActive(true);
            if (detailPreview) detailPreview.sprite = d.preview;
            if (detailName) detailName.text = d.displayName;
            if (detailDescription) detailDescription.text = d.description;
            if (detailPrice) detailPrice.text = EconomyTerms.Format(CurrencyType.KP, d.EffectivePrice);

            bool owned = IsOwned(d);
            bool levelLocked = d.requiredAccountLevel > 0 && _service.Account.level < d.requiredAccountLevel;
            bool affordable = _service.Account.kp >= d.EffectivePrice;

            if (buyButton) buyButton.interactable = !owned && !levelLocked && affordable;
            if (buyButtonLabel)
                buyButtonLabel.text = owned ? "OWNED"
                    : levelLocked ? $"UNLOCK AT Lv {d.requiredAccountLevel}"
                    : !affordable ? $"NEED {d.EffectivePrice - _service.Account.kp} KP"
                    : $"BUY  {d.EffectivePrice} KP";

            // Equip only for owned borders/icons.
            bool equippable = owned && (d.category == StoreCategory.ProfileBorder || d.category == StoreCategory.ProfileIcon);
            if (equipButton) equipButton.gameObject.SetActive(equippable);
        }

        private void BuySelected()
        {
            if (string.IsNullOrEmpty(_selectedId)) return;
            var result = _service.Buy(_selectedId);
            switch (result)
            {
                case PurchaseResult.Success: Feedback($"Purchased {_defs[_selectedId].displayName}!", true); ShowDetail(_selectedId); break;
                case PurchaseResult.NotEnoughKp: Feedback("Not enough KP.", false); break;
                case PurchaseResult.AlreadyOwned: Feedback("Already owned.", false); break;
                case PurchaseResult.LevelLocked: Feedback("Account level too low.", false); break;
                default: Feedback("Unable to purchase.", false); break;
            }
        }

        private void EquipSelected()
        {
            if (!_defs.TryGetValue(_selectedId, out var d)) return;
            bool ok = d.category == StoreCategory.ProfileBorder ? _service.EquipBorder(d.itemId) : _service.EquipIcon(d.itemId);
            Feedback(ok ? $"Equipped {d.displayName}." : "Cannot equip.", ok);
        }

        private bool IsOwned(StoreItemDefinition d)
        {
            if (_service.Inventory.Owns(d.itemId)) return true;
            if (d.category == StoreCategory.Champion && !string.IsNullOrEmpty(d.championId))
                return _service.Inventory.OwnsChampion(d.championId);
            return false;
        }

        private void RefreshHeader() { if (currencyHeader) currencyHeader.Bind(_service.Account); }

        private void Feedback(string msg, bool ok)
        {
            if (!feedbackLabel) return;
            feedbackLabel.text = msg;
            feedbackLabel.color = ok ? new Color(0.55f,0.85f,0.6f) : new Color(0.95f,0.5f,0.45f);
        }

        private static void Highlight(Button b, bool on)
        {
            if (!b) return; var img = b.GetComponent<Image>();
            if (img) img.color = on ? new Color(0.36f,0.62f,0.92f) : new Color(0.16f,0.18f,0.24f);
        }

        // Expose the service so a menu can seed the account / grant XP.
        public KpStoreService Service => _service;
    }
}
