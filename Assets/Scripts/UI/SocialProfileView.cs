// Project D  |  Codename: Zantetsuken
// SocialProfileView.cs  (Social/Profile UI — showcase card + friends list + requests)
//
// The combined Social screen: a big PROFILE SHOWCASE card on the left (equipped
// border/icon, name, level, rank, season win-rate, favorite champion, title) and a
// FRIENDS panel on the right (online-sorted list, incoming-request chips with accept/
// decline, and an add-friend field). Binds a ProfileCard + FriendsModel; raises events
// for equip/party/profile-view so the app + server complete them.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Profile;
using ProjectD.Friends;

namespace ProjectD.SocialUI
{
    public sealed class SocialProfileView : MonoBehaviour
    {
        [Header("Profile showcase")]
        [SerializeField] private Image borderImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text rankText;
        [SerializeField] private Text winRateText;
        [SerializeField] private Text favoriteChampionText;
        [SerializeField] private Text friendCodeText;

        [Header("Friends list")]
        [SerializeField] private RectTransform friendRowParent;
        [SerializeField] private FriendRow friendRowPrefab;
        [SerializeField] private Text friendCountText;

        [Header("Requests")]
        [SerializeField] private RectTransform requestRowParent;
        [SerializeField] private FriendRow requestRowPrefab;  // reused, invite button = accept
        [SerializeField] private Text requestCountText;

        [Header("Add friend")]
        [SerializeField] private InputField addFriendInput;
        [SerializeField] private Button addFriendButton;
        [SerializeField] private Text feedbackText;

        private readonly List<FriendRow> _friendRows = new();
        private readonly List<FriendRow> _requestRows = new();
        private ProfileCard _card;
        private FriendsModel _friends;

        public event System.Action<string> OnInviteToParty;
        public event System.Action<string> OnViewProfile;
        public event System.Action<string> OnSendRequest;

        public void Bind(ProfileCard card, FriendsModel friends)
        {
            _card = card; _friends = friends;
            BindProfile();
            if (addFriendButton) { addFriendButton.onClick.RemoveAllListeners(); addFriendButton.onClick.AddListener(SubmitAddFriend); }
            RefreshFriends();
            RefreshRequests();
        }

        private void BindProfile()
        {
            if (_card == null) return;
            if (nameText) nameText.text = _card.displayName;
            if (levelText) levelText.text = $"Lv {_card.accountLevel}";
            if (titleText) titleText.text = string.IsNullOrEmpty(_card.title) ? "—" : _card.title;
            if (rankText) rankText.text = _card.RankLabel;
            if (winRateText) winRateText.text = _card.seasonGames > 0
                ? $"{Mathf.RoundToInt(_card.WinRate * 100)}% WR  ({_card.seasonWins}-{_card.seasonGames - _card.seasonWins})"
                : "No ranked games";
            if (favoriteChampionText) favoriteChampionText.text = string.IsNullOrEmpty(_card.favoriteChampionId) ? "—" : _card.favoriteChampionId;
            if (friendCodeText) friendCodeText.text = $"ID: {_card.playerId}";
            if (borderImage && borderImage.sprite == null) borderImage.color = new Color(0.95f, 0.80f, 0.32f);
            if (iconImage && iconImage.sprite == null) iconImage.color = new Color(0.36f, 0.62f, 0.92f);
        }

        public void RefreshFriends()
        {
            foreach (var r in _friendRows) if (r) Destroy(r.gameObject);
            _friendRows.Clear();
            if (_friends == null || friendRowParent == null || friendRowPrefab == null) return;

            var sorted = _friends.Sorted();
            foreach (var f in sorted)
            {
                var row = Instantiate(friendRowPrefab, friendRowParent);
                row.Bind(f);
                row.OnInvite += id => OnInviteToParty?.Invoke(id);
                row.OnViewProfile += id => OnViewProfile?.Invoke(id);
                _friendRows.Add(row);
            }
            if (friendCountText) friendCountText.text = $"Friends ({sorted.Count})";
        }

        public void RefreshRequests()
        {
            foreach (var r in _requestRows) if (r) Destroy(r.gameObject);
            _requestRows.Clear();
            if (_friends == null || requestRowParent == null || requestRowPrefab == null) return;

            int count = 0;
            foreach (var fromId in _friends.IncomingRequests)
            {
                var row = Instantiate(requestRowPrefab, requestRowParent);
                row.Bind(new FriendEntry { playerId = fromId, displayName = fromId, presence = Presence.Online, statusText = "Wants to be friends" });
                // In this reused row, the "invite" button acts as ACCEPT.
                row.OnInvite += id => { if (_friends.AcceptRequest(id)) { RefreshFriends(); RefreshRequests(); Feedback($"Added {id}.", true); } };
                _requestRows.Add(row);
                count++;
            }
            if (requestCountText) requestCountText.text = count > 0 ? $"Requests ({count})" : "No pending requests";
        }

        private void SubmitAddFriend()
        {
            string id = addFriendInput ? addFriendInput.text?.Trim() : null;
            if (string.IsNullOrEmpty(id)) { Feedback("Enter a player ID.", false); return; }
            if (_friends.IsFriend(id)) { Feedback("Already your friend.", false); return; }
            if (_friends.SendRequest(id)) { OnSendRequest?.Invoke(id); Feedback($"Request sent to {id}.", true); if (addFriendInput) addFriendInput.text = ""; RefreshRequests(); RefreshFriends(); }
            else Feedback("Could not send request.", false);
        }

        private void Feedback(string msg, bool ok)
        {
            if (!feedbackText) return;
            feedbackText.text = msg;
            feedbackText.color = ok ? new Color(0.55f, 0.85f, 0.6f) : new Color(0.95f, 0.5f, 0.45f);
        }
    }
}
