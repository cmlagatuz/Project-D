// Project D | Codename: Zantetsuken — MatchBootstrap.cs (GCD 15.3)
using System.Collections.Generic;
using UnityEngine;
using ProjectD.Data;
using ProjectD.Gameplay.Objectives;

namespace ProjectD.Core
{
    public sealed class MatchBootstrap : MonoBehaviour
    {
        [SerializeField] private List<NeutralMonsterDefinition> elementalSummons;
        [SerializeField] private NeutralMonsterDefinition odin;
        [SerializeField] private List<NeutralMonsterDefinition> divineManifestations;
        [SerializeField] private int matchSeed;

        private readonly SummonSequenceController _summons = new();
        private readonly DivineManifestationController _divine = new();
        private readonly ObjectiveContributionTracker _contribution = new();

        public SummonSequenceController Summons => _summons;
        public DivineManifestationController Divine => _divine;

        private void Start()
        {
            if (matchSeed == 0) matchSeed = System.Environment.TickCount;

            _summons.OnUpcomingChanged += s => Debug.Log($"[Project D] Upcoming summon: {(s ? s.displayName : "None")}");
            _divine.OnUpcomingRevealed += d => Debug.Log($"[Project D] Rift reveals: {(d ? d.displayName : "None")}");

            _summons.Generate(elementalSummons, odin, matchSeed);
            _divine.Initialize(divineManifestations, matchSeed);

            Debug.Log($"[Project D] Seed {matchSeed}. Disclosed summon order:");
            int n = 1;
            foreach (var s in _summons.RevealedOrder)
                Debug.Log($"   {n++}. {s.displayName}");
        }

        public void HandleSummonDefeated(int killingTeam, float dmg)
        {
            _contribution.RecordDamage(killingTeam, dmg);
            _summons.NotifyCurrentDefeated();
        }
    }
}
