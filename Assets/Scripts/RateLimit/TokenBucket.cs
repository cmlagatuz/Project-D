// Project D | Codename: Zantetsuken — TokenBucket.cs (per-client/route rate limiting)
using System; using System.Collections.Generic;
namespace ProjectD.P7.RateLimit
{
    public sealed class TokenBucket
    {
        public readonly double capacity, refillPerSec; double _tokens; double _lastRefill;
        public TokenBucket(double capacity,double refillPerSec,double now){ this.capacity=capacity; this.refillPerSec=refillPerSec; _tokens=capacity; _lastRefill=now; }
        public bool TryConsume(double now,double cost=1){ Refill(now); if(_tokens>=cost){ _tokens-=cost; return true; } return false; }
        public double Available(double now){ Refill(now); return _tokens; }
        void Refill(double now){ double dt=now-_lastRefill; if(dt<=0) return; _tokens=Math.Min(capacity,_tokens+dt*refillPerSec); _lastRefill=now; }
    }
    // Keyed limiter: one bucket per client+route.
    public sealed class RateLimiter
    {
        readonly Dictionary<string,TokenBucket> _b=new(); public double Capacity=20, RefillPerSec=10;
        public bool Allow(string key,double now,double cost=1){ if(!_b.TryGetValue(key,out var b)){ b=new TokenBucket(Capacity,RefillPerSec,now); _b[key]=b; } return b.TryConsume(now,cost); }
    }
}
