// Project D | Codename: Zantetsuken — RewardVault.cs (claimable ledger + dedupe + expiry)
using System; using System.Collections.Generic;
namespace ProjectD.P8.Rewards
{
    public enum RewardSource { LevelUp, Season, Event, Mission, BattlePass, Compensation }
    public sealed class VaultEntry { public string id, itemId, displayName; public RewardSource source; public int amount=1; public long grantedUnix, expiresUnix; public bool claimed; }
    public sealed class RewardVault
    {
        readonly Dictionary<string,VaultEntry> _entries=new();
        public IEnumerable<VaultEntry> Entries=>_entries.Values;
        /// <summary>Grant a reward; dedupes by id (idempotent for retries).</summary>
        public bool Grant(VaultEntry e){ if(e==null||string.IsNullOrEmpty(e.id)) return false; if(_entries.ContainsKey(e.id)) return false; _entries[e.id]=e; return true; }
        public List<VaultEntry> Claimable(long now){ var o=new List<VaultEntry>(); foreach(var e in _entries.Values) if(!e.claimed&&(e.expiresUnix==0||e.expiresUnix>now)) o.Add(e); return o; }
        public bool Claim(string id,long now){ if(!_entries.TryGetValue(id,out var e)) return false; if(e.claimed) return false; if(e.expiresUnix!=0&&e.expiresUnix<=now) return false; e.claimed=true; return true; }
        public int UnclaimedCount(long now)=>Claimable(now).Count;
        public void PurgeExpired(long now){ var rm=new List<string>(); foreach(var kv in _entries) if(!kv.Value.claimed&&kv.Value.expiresUnix!=0&&kv.Value.expiresUnix<=now) rm.Add(kv.Key); foreach(var k in rm) _entries.Remove(k); }
    }
}
