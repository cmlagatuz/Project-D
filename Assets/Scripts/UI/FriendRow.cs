// Project D  |  Codename: Zantetsuken
// FriendRow.cs  (Social UI — one friend on the friends list)
//
// Shows a friend's name, level, rank, presence dot + status text, and quick actions
// (invite to party / view profile / remove). Presence colors: in-game green, in-queue
// yellow, online blue, away gray, offline dim. Bind a FriendEntry; raises action events.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Friends;

namespace ProjectD.SocialUI
{
    public sealed class FriendRow : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image presenceDot;
        [SerializeField] private Text nameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text rankText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button inviteButton;
        [SerializeField] private Button profileButton;

        public string PlayerId { get; private set; }
        public event System.Action<string> OnInvite;
        public event System.Action<string> OnViewProfile;

        private static readonly Color InGame = new Color(0.45f, 0.85f, 0.5f);
        private static readonly Color InQueue = new Color(0.95f, 0.80f, 0.32f);
        private static readonly Color Online = new Color(0.36f, 0.62f, 0.92f);
        private static readonly Color Away = new Color(0.6f, 0.6f, 0.65f);
        private static readonly Color Offline = new Color(0.4f, 0.42f, 0.48f);

        public void Bind(FriendEntry f)
        {
            PlayerId = f.playerId;
            if (nameText) nameText.text = f.displayName;
            if (levelText) levelText.text = f.accountLevel > 0 ? $"Lv {f.accountLevel}" : "";
            if (rankText) rankText.text = string.IsNullOrEmpty(f.rankLabel) ? "" : f.rankLabel;
            if (statusText) statusText.text = StatusFor(f);
            if (presenceDot) presenceDot.color = ColorFor(f.presence);
            if (background) background.color = new Color(0.13f, 0.15f, 0.21f, 0.9f);

            bool joinable = f.presence == Presence.Online || f.presence == Presence.InQueue;
            if (inviteButton)
            {
                inviteButton.interactable = joinable;
                inviteButton.onClick.RemoveAllListeners();
                inviteButton.onClick.AddListener(() => OnInvite?.Invoke(PlayerId));
            }
            if (profileButton)
            {
                profileButton.onClick.RemoveAllListeners();
                profileButton.onClick.AddListener(() => OnViewProfile?.Invoke(PlayerId));
            }
        }

        private static string StatusFor(FriendEntry f)
        {
            if (!string.IsNullOrEmpty(f.statusText)) return f.statusText;
            return f.presence switch
            {
                Presence.InGame => "In game", Presence.InQueue => "In queue",
                Presence.Online => "Online", Presence.Away => "Away", _ => "Offline"
            };
        }

        private static Color ColorFor(Presence p) => p switch
        {
            Presence.InGame => InGame, Presence.InQueue => InQueue,
            Presence.Online => Online, Presence.Away => Away, _ => Offline
        };
    }
}
