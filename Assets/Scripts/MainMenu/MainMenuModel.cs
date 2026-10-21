// Project D | Codename: Zantetsuken — MainMenuModel.cs (home shell + notification badges)
using System.Collections.Generic;
namespace ProjectD.MainMenu
{
    public enum MenuDestination { Play, Store, Champions, Profile, Events, Missions, Leaderboard, Social, Settings }
    public sealed class MainMenuModel
    {
        readonly Dictionary<MenuDestination,int> _b=new();
        public void SetBadge(MenuDestination d,int c)=>_b[d]=c<0?0:c;
        public int Badge(MenuDestination d)=>_b.TryGetValue(d,out var c)?c:0;
        public bool HasBadge(MenuDestination d)=>Badge(d)>0;
        public int TotalBadges(){ int t=0; foreach(var kv in _b) t+=kv.Value; return t; }
        public void Clear(MenuDestination d)=>_b[d]=0;
        public void RefreshFrom(int unclaimedMissions,int friendRequests,int newStoreItems,int pendingRewards,bool hasNewEvents){ SetBadge(MenuDestination.Missions,unclaimedMissions); SetBadge(MenuDestination.Social,friendRequests); SetBadge(MenuDestination.Store,newStoreItems); SetBadge(MenuDestination.Profile,pendingRewards); SetBadge(MenuDestination.Events,hasNewEvents?1:0); }
    }
}
