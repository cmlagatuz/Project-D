// Project D | Codename: Zantetsuken — SummonSequenceController.cs (GCD 7.1 disclosed shuffle-bag)
using System; using System.Collections.Generic; using ProjectD.Core; using ProjectD.Data; using ProjectD.Utilities;
namespace ProjectD.Gameplay.Objectives
{
    public sealed class SummonSequenceController
    {
        private readonly List<NeutralMonsterDefinition> _sequence = new();
        private NeutralMonsterDefinition _odin; private int _index;
        public event Action<NeutralMonsterDefinition> OnUpcomingChanged;
        public bool OdinUnlocked => _index >= _sequence.Count;
        public void Generate(IReadOnlyList<NeutralMonsterDefinition> pool, NeutralMonsterDefinition odin, int seed)
        {
            if(pool==null||pool.Count<GameConstants.ElementalSummonsPerMatch) throw new ArgumentException("Summon pool too small.");
            _odin=odin; _index=0; _sequence.Clear();
            var bag=new List<NeutralMonsterDefinition>(pool); new SeededRandom(seed).ShuffleInPlace(bag);
            for(int i=0;i<GameConstants.ElementalSummonsPerMatch;i++) _sequence.Add(bag[i]);
            OnUpcomingChanged?.Invoke(GetUpcoming());
        }
        public IReadOnlyList<NeutralMonsterDefinition> RevealedOrder => _sequence;
        public NeutralMonsterDefinition GetUpcoming()=>_index<_sequence.Count?_sequence[_index]:_odin;
        public void NotifyCurrentDefeated(){ if(_index<_sequence.Count) _index++; OnUpcomingChanged?.Invoke(GetUpcoming()); }
    }
}
