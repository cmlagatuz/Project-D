// Project D | Codename: Zantetsuken — HotReloadConfig.cs (runtime tuning + events)
using System; using System.Collections.Generic;
namespace ProjectD.P6.HotReload
{
    public sealed class HotReloadConfig
    {
        readonly Dictionary<string,string> _v=new(); public event Action<string> OnChanged; public event Action OnReloaded;
        public void Set(string k,string val){ if(string.IsNullOrEmpty(k)) return; _v.TryGetValue(k,out var old); if(old==val) return; _v[k]=val; OnChanged?.Invoke(k); }
        public void ApplyBatch(IReadOnlyDictionary<string,string> u){ foreach(var kv in u){ _v.TryGetValue(kv.Key,out var old); if(old!=kv.Value){ _v[kv.Key]=kv.Value; OnChanged?.Invoke(kv.Key); } } OnReloaded?.Invoke(); }
        public float GetFloat(string k,float fb){ if(_v.TryGetValue(k,out var v)&&float.TryParse(v,System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out var f)) return f; return fb; }
        public int GetInt(string k,int fb){ if(_v.TryGetValue(k,out var v)&&int.TryParse(v,out var i)) return i; return fb; }
        public bool GetBool(string k,bool fb){ if(_v.TryGetValue(k,out var v)) return v=="1"||v.ToLowerInvariant()=="true"; return fb; }
        public string GetString(string k,string fb)=>_v.TryGetValue(k,out var v)?v:fb; public bool Has(string k)=>_v.ContainsKey(k);
    }
}
