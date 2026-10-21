// Project D | Codename: Zantetsuken — LoadBalancer.cs (least-connections routing)
using System.Collections.Generic;
namespace ProjectD.P7.LoadBalance
{
    public sealed class Node { public string id, region; public int connections; public bool healthy=true; public int weight=1; }
    public sealed class LoadBalancer
    {
        readonly Dictionary<string,Node> _nodes=new();
        public void Register(Node n){ if(n!=null&&!string.IsNullOrEmpty(n.id)) _nodes[n.id]=n; }
        public void SetHealthy(string id,bool h){ if(_nodes.TryGetValue(id,out var n)) n.healthy=h; }
        /// <summary>Pick the healthy node with the lowest connections/weight ratio (region-preferred).</summary>
        public Node Route(string region=null)
        {
            Node best=null; float bestRatio=float.MaxValue;
            foreach(var n in _nodes.Values){ if(!n.healthy) continue; if(!string.IsNullOrEmpty(region)&&n.region!=region) continue; float ratio=n.connections/(float)System.Math.Max(1,n.weight); if(ratio<bestRatio){ bestRatio=ratio; best=n; } }
            if(best==null&&!string.IsNullOrEmpty(region)) return Route(null); // fall back cross-region
            if(best!=null) best.connections++;
            return best;
        }
        public void Release(string id){ if(_nodes.TryGetValue(id,out var n)) n.connections=System.Math.Max(0,n.connections-1); }
        public int HealthyCount(){ int c=0; foreach(var n in _nodes.Values) if(n.healthy) c++; return c; }
    }
}
