// Project D  |  Codename: Zantetsuken
// MenuNavButton.cs  (Home UI — one nav-rail destination with a notification badge)
//
// A single home-hub destination button (Play, Store, Champions, Profile, Events,
// Missions, Leaderboard, Social, Settings). Shows an icon-ish label, an active
// highlight, and a red notification badge with a count (hidden at 0). Raises OnClicked.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.MainMenu;

namespace ProjectD.MainMenuUI
{
    public sealed class MenuNavButton : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Text label;
        [SerializeField] private Image accent;          // active highlight
        [SerializeField] private GameObject badge;      // red dot container
        [SerializeField] private Text badgeCount;       // "3"

        public MenuDestination Destination { get; private set; }
        public event System.Action<MenuDestination> OnClicked;

        private static readonly Color Idle = new Color(0.13f, 0.15f, 0.21f, 0.95f);
        private static readonly Color Active = new Color(0.20f, 0.28f, 0.42f, 0.98f);
        private static readonly Color Accent = new Color(0.36f, 0.62f, 0.92f);

        public void Init(MenuDestination dest, string text)
        {
            Destination = dest;
            if (label) label.text = text;
            var btn = GetComponent<Button>();
            if (btn == null) btn = gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnClicked?.Invoke(Destination));
            SetActive(false);
            SetBadge(0);
        }

        public void SetActive(bool active)
        {
            if (background) background.color = active ? Active : Idle;
            if (accent) accent.color = active ? Accent : new Color(1, 1, 1, 0);
        }

        public void SetBadge(int count)
        {
            bool show = count > 0;
            if (badge) badge.SetActive(show);
            if (badgeCount) badgeCount.text = count > 99 ? "99+" : count.ToString();
        }
    }
}
