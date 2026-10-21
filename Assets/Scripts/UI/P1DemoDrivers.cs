// Project D  |  Codename: Zantetsuken
// P1DemoDrivers.cs  (Home + Social sandboxes)
//
// Feeds sample data to the Main-menu shell and the Social/Profile view so you can see
// the nav rail + notification badges and the profile showcase + friends list/requests.
// In a real build the app aggregates badge counts from the live systems and the server
// owns the profile + social graph.

using UnityEngine;
using ProjectD.MainMenu;
using ProjectD.MainMenuUI;
using ProjectD.Profile;
using ProjectD.Friends;
using ProjectD.SocialUI;

namespace ProjectD.P1UI
{
    public sealed class MainMenuDemoDriver : MonoBehaviour
    {
        [SerializeField] private MainMenuShell shell;

        private void Start()
        {
            if (shell == null) return;
            var card = new ProfileCard
            {
                playerId = "you", displayName = "You", accountLevel = 42,
                rankTier = "Crystal", rankDivision = "A", showRank = true,
                seasonWins = 137, seasonGames = 242, favoriteChampionId = "kain", title = "Dragoon"
            };
            shell.BindPlayer(card, kp: 12500);
            shell.SetPlayMode("RANKED");
            // Sample notification badges (missions, friend requests, new store items, pending rewards, new events).
            shell.RefreshBadges(missions: 3, friendRequests: 2, newStoreItems: 5, pendingRewards: 1, newEvents: true);

            shell.OnNavigate += d => Debug.Log($"[Project D] Navigate -> {d}");
            shell.OnPlayPressed += () => Debug.Log("[Project D] PLAY -> matchmaking queue");
        }
    }

    public sealed class SocialDemoDriver : MonoBehaviour
    {
        [SerializeField] private SocialProfileView view;

        private void Start()
        {
            if (view == null) return;
            var card = new ProfileCard
            {
                playerId = "you", displayName = "You", accountLevel = 42,
                rankTier = "Crystal", rankDivision = "A", showRank = true,
                seasonWins = 137, seasonGames = 242, favoriteChampionId = "kain", title = "Dragoon",
                equippedBorderId = "border_crystal", equippedIconId = "icon_odin"
            };
            var friends = new FriendsModel();
            friends.AddFriendDirect(new FriendEntry { playerId = "zan", displayName = "Zantetsuken", accountLevel = 55, presence = Presence.InGame, statusText = "In Ranked", rankLabel = "Crystal B" });
            friends.AddFriendDirect(new FriendEntry { playerId = "wr", displayName = "WildRose", accountLevel = 38, presence = Presence.InQueue, statusText = "In queue", rankLabel = "Adamantite A" });
            friends.AddFriendDirect(new FriendEntry { playerId = "dg", displayName = "DragoonX", accountLevel = 47, presence = Presence.Online, rankLabel = "Mythril C" });
            friends.AddFriendDirect(new FriendEntry { playerId = "mm", displayName = "MimicLord", accountLevel = 22, presence = Presence.Away });
            friends.AddFriendDirect(new FriendEntry { playerId = "of", displayName = "OldFriend", accountLevel = 30, presence = Presence.Offline, lastOnlineUnix = 1 });
            friends.ReceiveRequest("NewChallenger");
            friends.ReceiveRequest("Cactuar99");

            view.OnInviteToParty += id => Debug.Log($"[Project D] Invite {id} to party");
            view.OnViewProfile += id => Debug.Log($"[Project D] View profile: {id}");
            view.OnSendRequest += id => Debug.Log($"[Project D] Friend request -> {id}");
            view.Bind(card, friends);
        }
    }
}
