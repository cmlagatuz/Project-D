// Project D  |  Codename: Zantetsuken
// MainMenuShell.cs  (Home UI — the main-menu hub)
//
// The home screen shell: a left nav rail of destinations (each with a live
// notification badge from MainMenuModel), a player summary header (name, level, KP,
// equipped border/icon, rank), and a big PLAY button. Selecting a destination fires
// OnNavigate so the app swaps the content panel (Store, Champions, Profile, Events,
// Leaderboard, Social, Settings). Badges refresh from the aggregated app state.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.MainMenu;
using ProjectD.Profile;

namespace ProjectD.MainMenuUI
{
    public sealed class MainMenuShell : MonoBehaviour
    {
        [Header("Nav rail")]
        [SerializeField] private RectTransform navParent;
        [SerializeField] private MenuNavButton navPrefab;

        [Header("Player summary header")]
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text playerLevelText;
        [SerializeField] private Text kpText;
        [SerializeField] private Image borderImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text rankText;

        [Header("Play")]
        [SerializeField] private Button playButton;
        [SerializeField] private Text playModeLabel;     // "RANKED" / "NORMAL"
        [SerializeField] private Text contentTitle;      // shows the active destination

        [Header("Content host (panels swapped by the app)")]
        [SerializeField] private RectTransform contentHost;

        private readonly MainMenuModel _model = new();
        private readonly List<MenuNavButton> _buttons = new();
        private MenuDestination _active = MenuDestination.Play;

        public event System.Action<MenuDestination> OnNavigate;
        public event System.Action OnPlayPressed;

        public MainMenuModel Model => _model;
        public RectTransform ContentHost => contentHost;

        // Nav order + labels for the rail.
        private static readonly (MenuDestination dest, string label)[] Nav =
        {
            (MenuDestination.Play, "Play"),
            (MenuDestination.Store, "Store"),
            (MenuDestination.Champions, "Champions"),
            (MenuDestination.Profile, "Profile"),
            (MenuDestination.Events, "Events"),
            (MenuDestination.Missions, "Missions"),
            (MenuDestination.Leaderboard, "Leaderboard"),
            (MenuDestination.Social, "Social"),
            (MenuDestination.Settings, "Settings"),
        };

        private void Start()
        {
            BuildNav();
            if (playButton) playButton.onClick.AddListener(() => OnPlayPressed?.Invoke());
            Select(MenuDestination.Play);
        }

        private void BuildNav()
        {
            if (navParent == null || navPrefab == null) return;
            foreach (var (dest, label) in Nav)
            {
                var b = Instantiate(navPrefab, navParent);
                b.Init(dest, label);
                b.OnClicked += Select;
                _buttons.Add(b);
            }
        }

        public void Select(MenuDestination dest)
        {
            _active = dest;
            foreach (var b in _buttons) b.SetActive(b.Destination == dest);
            if (contentTitle) contentTitle.text = dest.ToString();
            OnNavigate?.Invoke(dest);
        }

        /// <summary>Bind the player header (call after loading the account/profile).</summary>
        public void BindPlayer(ProfileCard card, long kp)
        {
            if (card == null) return;
            if (playerNameText) playerNameText.text = card.displayName;
            if (playerLevelText) playerLevelText.text = $"Lv {card.accountLevel}";
            if (kpText) kpText.text = $"{kp:N0} KP";
            if (rankText) rankText.text = card.RankLabel;
            if (borderImage && borderImage.sprite == null) borderImage.color = new Color(0.95f, 0.80f, 0.32f);
            if (iconImage && iconImage.sprite == null) iconImage.color = new Color(0.36f, 0.62f, 0.92f);
        }

        public void SetPlayMode(string modeLabel) { if (playModeLabel) playModeLabel.text = modeLabel; }

        /// <summary>Refresh notification badges from aggregated app state.</summary>
        public void RefreshBadges(int missions, int friendRequests, int newStoreItems, int pendingRewards, bool newEvents)
        {
            _model.RefreshFrom(missions, friendRequests, newStoreItems, pendingRewards, newEvents);
            foreach (var b in _buttons) b.SetBadge(_model.Badge(b.Destination));
        }
    }
}
