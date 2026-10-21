// Project D | Codename: Zantetsuken — DivineManifestationController.cs (GCD 7.2)
using System; using System.Collections.Generic; using ProjectD.Data; using ProjectD.Utilities;
namespace ProjectD.Gameplay.Objectives
{
    public sealed class DivineManifestationController
    {
        private readonly List<NeutralMonsterDefinition> _cycle = new(); private int _index; private SeededRandom _rng;
        public event Action<NeutralMonsterDefinition> OnUpcomingRevealed;
        public void Initialize(IReadOnlyList<NeutralMonsterDefinition> m, int seed)
        { _rng=new SeededRandom(seed^0x5EED); _cycle.Clear(); _cycle.AddRange(m); _rng.ShuffleInPlace(_cycle); _index=0; OnUpcomingRevealed?.Invoke(GetUpcoming()); }
        public NeutralMonsterDefinition GetUpcoming()=>_cycle.Count==0?null:_cycle[_index%_cycle.Count];
        public void Advance()
        {
            _index++;
            if(_index%_cycle.Count==0){ var last=_cycle[_cycle.Count-1]; _rng.ShuffleInPlace(_cycle); if(_cycle.Count>1&&_cycle[0]==last) (_cycle[0],_cycle[1])=(_cycle[1],_cycle[0]); }
            OnUpcomingRevealed?.Invoke(GetUpcoming());
        }
    }
}
