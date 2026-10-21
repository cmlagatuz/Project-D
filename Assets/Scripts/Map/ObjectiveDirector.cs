// Project D  |  Codename: Zantetsuken
// ObjectiveDirector.cs  (Map — runs the pits, spawns, and BGM handoff)
//
// Server-authoritative coordinator for the two river pits:
//   - decides the dynamic pit layout (summon pit next to the marksman/support duo;
//     divine pit on the OPPOSITE side),
//   - schedules summon/divine spawns via SpawnTimerEngine + SpawnSchedule, runs the
//     disclosed 3-elemental sequence then unlocks Odin, rotates the divine
//     manifestations with no immediate repeat,
//   - tells the ProximityBgmController which objective currently occupies each pit,
//     so the correct theme plays for players near that pit.
//
// The BGM itself is per-client proximity audio (ProximityBgmController); this
// director only sets "what is in the pit," never forces music on anyone.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.Audio;

namespace ProjectD.Map
{
    public sealed class ObjectiveDirector : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SpawnSchedule schedule;
        [SerializeField] private ProximityBgmController bgm;

        [Header("Pit transforms")]
        [SerializeField] private Transform topPit;
        [SerializeField] private Transform bottomPit;
        [SerializeField] private float pitEnterRadius = 7f;
        [SerializeField] private float pitExitRadius = 10f;

        [Header("Objective sequences")]
        [SerializeField] private List<string> elementalSequence = new() { "summon_ifrit", "summon_shiva", "summon_ramuh" };
        [SerializeField] private string odinId = "summon_odin";
        [SerializeField] private List<string> divineRotation = new() { "divine_materia", "divine_cosmos", "divine_spiritus", "divine_chaos" };

        private readonly SpawnTimerEngine _timers = new();
        private PitLayout _layout;
        private int _elementalIndex;
        private bool _odinUnlocked;
        private int _divineIndex;
        private float _clock;

        private const string SummonTimerId = "pit_summon";
        private const string DivineTimerId = "pit_divine";

        public PitLayout Layout => _layout;

        public void Initialize(RiverPit duoRiverPit)
        {
            _layout = PitAssignment.FromDuoPit(duoRiverPit);

            if (bgm)
            {
                var summonT = _layout.SummonPit == RiverPit.Top ? topPit : bottomPit;
                var divineT = _layout.DivinePit == RiverPit.Top ? topPit : bottomPit;
                if (summonT) bgm.ConfigurePit(PitIdFor(_layout.SummonPit), summonT.position, pitEnterRadius, pitExitRadius);
                if (divineT) bgm.ConfigurePit(PitIdFor(_layout.DivinePit), divineT.position, pitEnterRadius, pitExitRadius);
            }

            _timers.Register(SummonTimerId, schedule ? schedule.firstElementalSeconds : 120f, schedule ? schedule.elementalRespawnSeconds : 240f);
            _timers.Register(DivineTimerId, schedule ? schedule.firstDivineSeconds : 180f, schedule ? schedule.divineRespawnSeconds : 210f);

            _elementalIndex = 0; _divineIndex = 0; _odinUnlocked = false; _clock = 0f;
            SetPitObjective(_layout.SummonPit, null);
            SetPitObjective(_layout.DivinePit, null);
        }

        public void Tick(float deltaSeconds)
        {
            _clock += deltaSeconds;
            foreach (var id in _timers.Tick(_clock))
            {
                if (id == SummonTimerId) OnSummonPitSpawn();
                else if (id == DivineTimerId) OnDivinePitSpawn();
            }
        }

        private void OnSummonPitSpawn()
        {
            string objective = (!_odinUnlocked && _elementalIndex < elementalSequence.Count)
                ? elementalSequence[_elementalIndex] : odinId;
            SetPitObjective(_layout.SummonPit, objective);
        }

        public void NotifySummonKilled()
        {
            SetPitObjective(_layout.SummonPit, null);
            if (!_odinUnlocked)
            {
                _elementalIndex++;
                if (_elementalIndex >= elementalSequence.Count)
                {
                    _odinUnlocked = true;
                    float delay = schedule ? schedule.odinDelayAfterThirdSeconds : 60f;
                    _timers.SetNextSpawn(SummonTimerId, _clock + delay, schedule ? schedule.odinRespawnSeconds : 300f);
                    return;
                }
            }
            _timers.NotifyKilled(SummonTimerId, _clock);
        }

        private void OnDivinePitSpawn()
        {
            if (divineRotation.Count == 0) return;
            SetPitObjective(_layout.DivinePit, divineRotation[_divineIndex % divineRotation.Count]);
        }

        public void NotifyDivineKilled()
        {
            SetPitObjective(_layout.DivinePit, null);
            _divineIndex = (_divineIndex + 1) % Mathf.Max(1, divineRotation.Count);
            _timers.NotifyKilled(DivineTimerId, _clock);
        }

        private void SetPitObjective(RiverPit pit, string objectiveId)
        {
            if (bgm) bgm.SetPitObjective(PitIdFor(pit), objectiveId);
        }

        public static string PitIdFor(RiverPit pit) => pit == RiverPit.Top ? "pit_top" : "pit_bottom";

        public string SummonObjectiveNow =>
            (!_odinUnlocked && _elementalIndex < elementalSequence.Count) ? elementalSequence[_elementalIndex] : odinId;
        public string DivineObjectiveNow =>
            divineRotation.Count == 0 ? null : divineRotation[_divineIndex % divineRotation.Count];

        public float Clock => _clock;
        public SpawnTimerEngine Timers => _timers;
    }
}
