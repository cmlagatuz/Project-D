// Project D | Codename: Zantetsuken — SaveGlue.cs (serialize/restore persistent profile)
using System; using System.Collections.Generic; using System.Text;
namespace ProjectD.P15.Save
{
    [Serializable] public sealed class PlayerProfileSave { public string playerId; public int accountLevel=1; public int xpIntoLevel; public long kp; public int worldRiftRating=1000; public readonly List<string> ownedChampions=new(); public readonly List<string> ownedCosmetics=new(); }
    // A minimal, dependency-free serializer (pipe/semicolon delimited) so the profile can
    // round-trip without a JSON lib. Deterministic + testable.
    public static class SaveGlue
    {
        public static string Serialize(PlayerProfileSave p)
        {
            if(p==null) return "";
            var sb=new StringBuilder();
            sb.Append(p.playerId).Append('|').Append(p.accountLevel).Append('|').Append(p.xpIntoLevel).Append('|').Append(p.kp).Append('|').Append(p.worldRiftRating).Append('|');
            sb.Append(string.Join(",",p.ownedChampions)).Append('|').Append(string.Join(",",p.ownedCosmetics));
            return sb.ToString();
        }
        public static PlayerProfileSave Deserialize(string data)
        {
            var p=new PlayerProfileSave(); if(string.IsNullOrEmpty(data)) return p;
            var f=data.Split('|');
            try{
                p.playerId=f[0]; p.accountLevel=int.Parse(f[1]); p.xpIntoLevel=int.Parse(f[2]); p.kp=long.Parse(f[3]); p.worldRiftRating=int.Parse(f[4]);
                if(f.Length>5&&f[5].Length>0) p.ownedChampions.AddRange(f[5].Split(','));
                if(f.Length>6&&f[6].Length>0) p.ownedCosmetics.AddRange(f[6].Split(','));
            } catch { return new PlayerProfileSave(); }
            return p;
        }
        public static bool RoundTrips(PlayerProfileSave p)=>Serialize(Deserialize(Serialize(p)))==Serialize(p);
    }
    // In-memory store (swap for cloud save in production).
    public sealed class SaveStore { readonly Dictionary<string,string> _s=new(); public void Save(string id,PlayerProfileSave p)=>_s[id]=SaveGlue.Serialize(p); public PlayerProfileSave Load(string id)=>_s.TryGetValue(id,out var d)?SaveGlue.Deserialize(d):new PlayerProfileSave{playerId=id}; public bool Has(string id)=>_s.ContainsKey(id); }
}
