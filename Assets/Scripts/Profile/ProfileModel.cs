// Project D | Codename: Zantetsuken — ProfileModel.cs (showcase card)
using System;
namespace ProjectD.Profile
{
    [Serializable] public sealed class ProfileCard
    {
        public string playerId, displayName; public int accountLevel=1;
        public string equippedIconId="", equippedBorderId="", title="";
        public string rankTier="", rankDivision=""; public int rankLp=0; public bool showRank=true;
        public int seasonWins=0, seasonGames=0; public string mostPlayedChampionId="", favoriteChampionId="";
        public float WinRate=>seasonGames>0?(float)seasonWins/seasonGames:0f;
        public string RankLabel=>showRank && !string.IsNullOrEmpty(rankTier)?$"{rankTier} {rankDivision}":"Unranked";
    }
    public static class ProfileService
    {
        public static void EquipIcon(ProfileCard c,string id){ if(c!=null) c.equippedIconId=id; }
        public static void EquipBorder(ProfileCard c,string id){ if(c!=null) c.equippedBorderId=id; }
        public static void SetTitle(ProfileCard c,string t){ if(c!=null) c.title=t; }
        public static void PinFavoriteChampion(ProfileCard c,string id){ if(c!=null) c.favoriteChampionId=id; }
        public static void SetRankVisibility(ProfileCard c,bool s){ if(c!=null) c.showRank=s; }
    }
}
