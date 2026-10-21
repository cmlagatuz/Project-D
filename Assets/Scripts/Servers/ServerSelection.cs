// Project D | Codename: Zantetsuken — ServerSelection.cs
using System.Collections.Generic;
namespace ProjectD.P4.Servers
{
    public sealed class ServerRegion { public string code, name; public int pingMs=9999; public bool online=true; public float loadPct; }
    public sealed class ServerSelection
    {
        readonly Dictionary<string,ServerRegion> _r=new();
        public void Register(ServerRegion r){ if(r!=null&&!string.IsNullOrEmpty(r.code)) _r[r.code]=r; }
        public void ReportPing(string code,int ms){ if(_r.TryGetValue(code,out var r)) r.pingMs=ms<0?9999:ms; }
        public ServerRegion Get(string code)=>_r.TryGetValue(code,out var r)?r:null; public IEnumerable<ServerRegion> All=>_r.Values;
        public ServerRegion Recommended(){ ServerRegion best=null; foreach(var r in _r.Values){ if(!r.online) continue; if(best==null||r.pingMs<best.pingMs||(r.pingMs==best.pingMs&&r.loadPct<best.loadPct)) best=r; } return best; }
        public static string Quality(int pingMs)=>pingMs<40?"Excellent":pingMs<80?"Good":pingMs<140?"Fair":"Poor";
    }
}
