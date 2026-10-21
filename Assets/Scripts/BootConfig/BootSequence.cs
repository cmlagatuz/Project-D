// Project D | Codename: Zantetsuken — BootSequence.cs (startup: config -> services -> account -> landing)
using System; using System.Collections.Generic;
namespace ProjectD.P15.Boot
{
    public enum BootStep { LoadConfig, InitServices, LoadAccount, ResolveEntitlements, Ready }
    public sealed class BootSequence
    {
        readonly List<BootStep> _order=new(){BootStep.LoadConfig,BootStep.InitServices,BootStep.LoadAccount,BootStep.ResolveEntitlements,BootStep.Ready};
        int _i; public BootStep Current=>_i<_order.Count?_order[_i]:BootStep.Ready; public bool Complete=>_i>=_order.Count-1;
        public event Action<BootStep> OnStep;
        /// <summary>Advance boot one step; each step must succeed to proceed (fail halts boot).</summary>
        public bool Advance(bool stepSucceeded)
        {
            if(!stepSucceeded) return false;
            OnStep?.Invoke(Current);
            if(_i<_order.Count-1) _i++;
            return true;
        }
        public float Progress=>_order.Count<=1?1f:(float)_i/(_order.Count-1);
    }
}
