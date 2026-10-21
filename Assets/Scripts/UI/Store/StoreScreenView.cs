// Project D  |  Codename: Zantetsuken
// StoreScreenView.cs  (Gold Shop UI — runtime controller for the in-match item shop)
//
// Wires the pieces built by Editor/StoreScreenBuilder.cs to the engine-agnostic gold-shop
// backend (StoreModel + InventoryModel + StoreTransactionManager + ChampionExclusiveController).
// Handles: category tabs, the item grid (with "recommended only" filtering off the locked
// champion's build path), the detail panel + BUY, SELL + UNDO (with a countdown fill), the
// 6-slot inventory row, and the recommended quick-buy row.
//
// Field names below are load-bearing: the builder assigns them via SerializedObject.FindProperty.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Store;

namespace ProjectD.UI.Store
{
    [Serializable]
    public struct ChampionBuildRecommendation
    {
        public string championId;
        public string role;
        public string[] order; // ordered itemIds, drives the quick-buy row
    }

    public sealed class StoreScreenView : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private RectTransform tabBar;
        [SerializeField] private StoreCategoryTab tabPrefab;

        [Header("Grid")]
        [SerializeField] private RectTransform gridContent;
        [SerializeField] private StoreItemCard cardPrefab;
        [SerializeField] private Toggle recommendedOnlyToggle;

        [Header("Detail panel")]
        [SerializeField] private Text detailName;
        [SerializeField] private Text detailCost;
        [SerializeField] private Text detailStats;
        [SerializeField] private Text detailPassive;
        [SerializeField] private Text detailBuildPath;
        [SerializeField] private Text detailFlavor;
        [SerializeField] private Image detailRoleBanner;
        [SerializeField] private Button buyButton;
        [SerializeField] private Text buyButtonLabel;

        [Header("Top bar")]
        [SerializeField] private Text goldLabel;
        [SerializeField] private Text purchaseFeedback;

        [Header("Sell / Undo")]
        [SerializeField] private Button sellButton;
        [SerializeField] private Text sellButtonLabel;
        [SerializeField] private Button undoButton;
        [SerializeField] private Text undoButtonLabel;
        [SerializeField] private Image undoTimerFill;

        [Header("Quick buy")]
        [SerializeField] private RectTransform quickBuyRow;
        [SerializeField] private StoreItemCard quickBuyCardPrefab;
        [SerializeField] private Text quickBuyTitle;

        [Header("Inventory (5 general + slot 6 = boots)")]
        [SerializeField] private StoreItemCard[] inventorySlots;

        [Header("Data")]
        [SerializeField] private ItemDefinition[] allItems;
        [SerializeField] private string lockedChampionId;
        [SerializeField] private StoreRole playerRole;
        [SerializeField] private ChampionBuildRecommendation[] championBuilds;
        [SerializeField] private int startingGold = 3000;

        private static readonly string[] Categories = { "All", "Boots", "Consumable", "Exclusive" };

        private readonly StoreModel _store = new StoreModel();
        private InventoryModel _inventory;
        private StoreTransactionManager _tx;
        private ChampionExclusiveController _exclusives;
        private Dictionary<string, ItemDefinition> _byId;
        private readonly List<StoreCategoryTab> _tabs = new();
        private readonly List<StoreItemCard> _gridCards = new();
        private readonly List<StoreItemCard> _quickCards = new();
        private string _selectedCategory = "All";
        private string _selectedItemId;
        private float _clock;
        private const bool ShopAvailable = true; // this scene is always "at fountain"

        private void Awake()
        {
            BuildBackend();
            BuildTabs();
            RebuildGrid();
            BuildQuickBuyRow();
            RefreshInventoryRow();
            RefreshGold();
            ShowDetail(null);

            if (buyButton) buyButton.onClick.AddListener(OnBuyClicked);
            if (sellButton) sellButton.onClick.AddListener(OnSellClicked);
            if (undoButton) undoButton.onClick.AddListener(OnUndoClicked);
            if (recommendedOnlyToggle) recommendedOnlyToggle.onValueChanged.AddListener(_ => RebuildGrid());
        }

        private void Update()
        {
            _clock += Time.deltaTime;
            _tx?.Tick(_clock);
            RefreshUndoButton();
        }

        // ---- setup ---- //
        private void BuildBackend()
        {
            _byId = new Dictionary<string, ItemDefinition>();
            foreach (var item in allItems)
            {
                if (item == null) continue;
                _byId[item.itemId] = item;
                _store.Register(ToView(item), item.sellRefund);
            }
            _store.LockedChampionId = lockedChampionId;

            _exclusives = new ChampionExclusiveController(_store);
            foreach (var item in allItems)
                if (item != null && item.IsChampionExclusive && !string.IsNullOrEmpty(item.exclusiveToChampionId))
                    _exclusives.Register(item.exclusiveToChampionId, item.itemId);

            _inventory = new InventoryModel(_store);
            _tx = new StoreTransactionManager(_store, _inventory, startingGold);
        }

        private static ItemView ToView(ItemDefinition d) => new ItemView
        {
            ItemId = d.itemId, IsBoots = d.IsBoots, MaxStack = d.maxStack, TotalCost = d.totalCost,
            ExclusiveToChampionId = d.exclusiveToChampionId, Category = d.category,
            Stats = d.stats?.Select(s => new StatModView { stat = s.stat, value = s.value }).ToArray() ?? Array.Empty<StatModView>()
        };

        private void BuildTabs()
        {
            if (tabBar == null || tabPrefab == null) return;
            foreach (var cat in Categories)
            {
                var tab = Instantiate(tabPrefab, tabBar);
                tab.gameObject.SetActive(true);
                tab.Bind(cat, cat == "Exclusive" ? "Exclusive" : cat);
                tab.OnClicked += OnCategoryClicked;
                tab.SetSelected(cat == _selectedCategory);
                _tabs.Add(tab);
            }
        }

        private void OnCategoryClicked(string categoryId)
        {
            _selectedCategory = categoryId;
            foreach (var t in _tabs) t.SetSelected(t.CategoryId == categoryId);
            RebuildGrid();
        }

        // ---- grid ---- //
        private void RebuildGrid()
        {
            if (gridContent == null || cardPrefab == null) return;
            foreach (var c in _gridCards) if (c) Destroy(c.gameObject);
            _gridCards.Clear();

            bool recommendedOnly = recommendedOnlyToggle != null && recommendedOnlyToggle.isOn;
            var recommendedIds = CurrentRecommendedOrder();

            foreach (var item in VisibleItems())
            {
                if (recommendedOnly && !recommendedIds.Contains(item.itemId)) continue;

                var card = Instantiate(cardPrefab, gridContent);
                card.gameObject.SetActive(true);
                bool owned = _inventory.Contains(item.itemId);
                bool affordable = _tx.Gold >= item.totalCost;
                card.Bind(item, owned, recommendedIds.Contains(item.itemId), affordable);
                card.OnClicked += OnCardClicked;
                _gridCards.Add(card);
            }
        }

        private IEnumerable<ItemDefinition> VisibleItems()
        {
            foreach (var item in allItems)
            {
                if (item == null) continue;
                if (item.IsChampionExclusive && !_store.IsAvailable(item.itemId)) continue; // wrong/no champion locked
                switch (_selectedCategory)
                {
                    case "Boots": if (!item.IsBoots) continue; break;
                    case "Consumable": if (item.category != ItemCategory.Consumable) continue; break;
                    case "Exclusive": if (!item.IsChampionExclusive) continue; break;
                    default: if (item.IsChampionExclusive) continue; break; // "All" excludes exclusives; they live on their own tab
                }
                yield return item;
            }
        }

        private List<string> CurrentRecommendedOrder()
        {
            foreach (var b in championBuilds)
                if (b.championId == lockedChampionId && string.Equals(b.role, playerRole.ToString(), StringComparison.OrdinalIgnoreCase))
                    return b.order?.ToList() ?? new List<string>();
            return new List<string>();
        }

        private void OnCardClicked(string itemId) => ShowDetail(itemId);

        // ---- detail panel ---- //
        private void ShowDetail(string itemId)
        {
            _selectedItemId = itemId;
            ItemDefinition item = null;
            if (!string.IsNullOrEmpty(itemId)) _byId.TryGetValue(itemId, out item);

            if (item == null)
            {
                if (detailName) detailName.text = "Select an item";
                if (detailCost) detailCost.text = "0g";
                if (detailStats) detailStats.text = "";
                if (detailPassive) detailPassive.text = "";
                if (detailBuildPath) detailBuildPath.text = "";
                if (detailFlavor) detailFlavor.text = "";
                if (buyButton) buyButton.interactable = false;
                return;
            }

            if (detailName) detailName.text = item.displayName;
            if (detailCost) detailCost.text = $"{item.totalCost}g";
            if (detailStats) detailStats.text = string.Join("\n", item.stats.Select(FormatStat));
            if (detailPassive) detailPassive.text = string.IsNullOrEmpty(item.passiveName) ? "" : $"<b>{item.passiveName}</b>\n{item.passiveDescription}";
            if (detailBuildPath) detailBuildPath.text = item.buildsIntoIds.Length == 0 ? "" : "Builds into: " + string.Join(", ", item.buildsIntoIds);
            if (detailFlavor) detailFlavor.text = item.flavor;
            if (detailRoleBanner) detailRoleBanner.gameObject.SetActive(item.IsChampionExclusive);

            bool owned = _inventory.Contains(item.itemId);
            bool affordable = _tx.Gold >= item.totalCost;
            if (buyButton) buyButton.interactable = !owned && affordable;
            if (buyButtonLabel) buyButtonLabel.text = owned ? "OWNED" : affordable ? "BUY" : "NOT ENOUGH GOLD";
        }

        private static string FormatStat(StatMod s) => $"+{s.value:0.#} {s.stat}";

        // ---- buy / sell / undo ---- //
        private void OnBuyClicked()
        {
            if (string.IsNullOrEmpty(_selectedItemId)) return;

            GoldPurchaseResult result;
            if (_selectedItemId == _exclusives.ExclusiveItemId)
            {
                result = _exclusives.TryPurchaseExclusive(_tx.Gold, _inventory, out int exclusiveCost, out _);
                if (result == GoldPurchaseResult.Success) _tx.AddGold(-exclusiveCost); // exclusive path bypasses StoreTransactionManager's own ledger
            }
            else
            {
                result = _tx.Buy(_selectedItemId, _clock, out _);
            }

            SetFeedback(result switch
            {
                GoldPurchaseResult.Success => "Purchased!",
                GoldPurchaseResult.NotEnoughGold => "Not enough gold.",
                GoldPurchaseResult.InventoryFull => "Inventory full.",
                GoldPurchaseResult.AlreadyOwned => "Already owned.",
                _ => "Unavailable.",
            });

            if (result == GoldPurchaseResult.Success)
            {
                RefreshGold(); RefreshInventoryRow(); RebuildGrid(); ShowDetail(_selectedItemId);
            }
        }

        private void OnSellClicked()
        {
            int slot = SelectedInventorySlot();
            if (slot < 0) { SetFeedback("Select an inventory item to sell."); return; }

            var result = _tx.Sell(slot, _clock, ShopAvailable, out int refund);
            SetFeedback(result == SellResult.Success ? $"Sold for {refund}g." : "Can't sell that.");
            if (result == SellResult.Success) { RefreshGold(); RefreshInventoryRow(); RebuildGrid(); }
        }

        private void OnUndoClicked()
        {
            var result = _tx.Undo(_clock, ShopAvailable, out int refund);
            SetFeedback(result == UndoResult.Success ? $"Undone, refunded {refund}g." : "Nothing to undo.");
            if (result == UndoResult.Success) { RefreshGold(); RefreshInventoryRow(); RebuildGrid(); }
        }

        private int SelectedInventorySlot()
        {
            if (string.IsNullOrEmpty(_selectedItemId)) return -1;
            for (int i = 0; i < inventorySlots.Length; i++)
                if (_inventory.GetSlot(i).ItemId == _selectedItemId) return i;
            return -1;
        }

        private void RefreshUndoButton()
        {
            if (undoButton == null || _tx == null) return;
            bool canUndo = _tx.CanUndo(_clock, ShopAvailable);
            undoButton.interactable = canUndo;
            float remaining = _tx.UndoTimeRemaining(_clock);
            if (undoTimerFill) undoTimerFill.fillAmount = _tx.UndoWindowSeconds <= 0f ? 0f : Mathf.Clamp01(remaining / _tx.UndoWindowSeconds);
            if (undoButtonLabel) undoButtonLabel.text = canUndo ? $"UNDO ({remaining:0}s)" : "UNDO";
        }

        // ---- inventory row ---- //
        private void RefreshInventoryRow()
        {
            if (inventorySlots == null) return;
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                var card = inventorySlots[i];
                if (card == null) continue;
                var slot = _inventory.GetSlot(i);
                if (slot.IsEmpty) { card.gameObject.SetActive(false); continue; }
                if (!_byId.TryGetValue(slot.ItemId, out var item)) { card.gameObject.SetActive(false); continue; }
                card.gameObject.SetActive(true);
                card.Bind(item, owned: true, recommended: false, affordable: true);
            }
        }

        // ---- quick buy ---- //
        private void BuildQuickBuyRow()
        {
            if (quickBuyRow == null || quickBuyCardPrefab == null) return;
            var order = CurrentRecommendedOrder();
            if (quickBuyTitle) quickBuyTitle.gameObject.SetActive(order.Count > 0);

            foreach (var itemId in order)
            {
                if (!_byId.TryGetValue(itemId, out var item)) continue;
                var card = Instantiate(quickBuyCardPrefab, quickBuyRow);
                card.gameObject.SetActive(true);
                card.Bind(item, _inventory.Contains(itemId), recommended: true, _tx.Gold >= item.totalCost);
                card.OnQuickBuyClicked += id => { _selectedItemId = id; OnBuyClicked(); RefreshQuickBuyRow(); };
                _quickCards.Add(card);
            }
        }

        private void RefreshQuickBuyRow()
        {
            var order = CurrentRecommendedOrder();
            for (int i = 0; i < _quickCards.Count && i < order.Count; i++)
            {
                if (!_byId.TryGetValue(order[i], out var item)) continue;
                _quickCards[i].Bind(item, _inventory.Contains(order[i]), recommended: true, _tx.Gold >= item.totalCost);
            }
        }

        // ---- shared ---- //
        private void RefreshGold()
        {
            if (goldLabel) goldLabel.text = $"{_tx.Gold}g";
        }

        private void SetFeedback(string message)
        {
            if (purchaseFeedback) purchaseFeedback.text = message;
        }
    }
}
