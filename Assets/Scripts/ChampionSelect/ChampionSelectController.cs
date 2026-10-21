// Project D  |  Codename: Zantetsuken
// ChampionSelectController.cs  (GCD 5.4 Draft & Team Composition)
//
// Pure pick-phase state machine (no UnityEngine on the core logic path so it is
// unit-testable and reusable on the server for authoritative draft validation).
// Handles: phase order (ban/pick), per-step timer, whose turn it is, hover,
// lock-in, duplicate prevention within a match, and completion.
//
// The view (ChampionSelectView) subscribes to events and renders; the server
// mirrors the same steps to keep the draft authoritative.

using System;
using System.Collections.Generic;

namespace ProjectD.UI.ChampionSelect
{
    public enum DraftStepType { Ban, Pick }
    public enum DraftTeam { Blue, Red }

    public struct DraftStep
    {
        public DraftTeam Team;
        public DraftStepType Type;
        public int SlotIndex;        // pick slot on that team (0..4); -1 for bans
    }

    public sealed class ChampionSelectController
    {
        // Standard-ish draft: alternating bans then picks. Tunable per mode.
        private static readonly DraftStep[] DefaultSequence =
        {
            new DraftStep{ Team=DraftTeam.Blue, Type=DraftStepType.Ban,  SlotIndex=-1 },
            new DraftStep{ Team=DraftTeam.Red,  Type=DraftStepType.Ban,  SlotIndex=-1 },
            new DraftStep{ Team=DraftTeam.Blue, Type=DraftStepType.Pick, SlotIndex=0 },
            new DraftStep{ Team=DraftTeam.Red,  Type=DraftStepType.Pick, SlotIndex=0 },
            new DraftStep{ Team=DraftTeam.Red,  Type=DraftStepType.Pick, SlotIndex=1 },
            new DraftStep{ Team=DraftTeam.Blue, Type=DraftStepType.Pick, SlotIndex=1 },
            new DraftStep{ Team=DraftTeam.Blue, Type=DraftStepType.Pick, SlotIndex=2 },
            new DraftStep{ Team=DraftTeam.Red,  Type=DraftStepType.Pick, SlotIndex=2 },
        };

        private readonly DraftStep[] _sequence;
        private readonly float _secondsPerStep;

        private int _stepIndex = -1;
        private float _stepTimeRemaining;
        private string _hoverChampionId;

        private readonly HashSet<string> _banned = new();
        private readonly Dictionary<DraftTeam, string[]> _picks = new()
        {
            { DraftTeam.Blue, new string[3] },
            { DraftTeam.Red,  new string[3] },
        };

        // ---- Events for the view ---- //
        public event Action<DraftStep> OnStepStarted;
        public event Action<string> OnHoverChanged;                 // championId
        public event Action<DraftTeam, int, string> OnPickLocked;   // team, slot, championId
        public event Action<DraftTeam, string> OnBanLocked;         // team, championId
        public event Action<float> OnTimerTick;                     // seconds remaining
        public event Action OnDraftComplete;

        public bool IsComplete => _stepIndex >= _sequence.Length;
        public DraftStep CurrentStep => _sequence[Math.Clamp(_stepIndex, 0, _sequence.Length - 1)];
        public string HoverChampionId => _hoverChampionId;

        public ChampionSelectController(float secondsPerStep = 30f, DraftStep[] customSequence = null)
        {
            _secondsPerStep = secondsPerStep;
            _sequence = customSequence ?? DefaultSequence;
        }

        public void Begin() => AdvanceStep();

        /// <summary>Call every frame. Auto-locks the hovered champion on timeout.</summary>
        public void Tick(float deltaTime)
        {
            if (IsComplete) return;
            _stepTimeRemaining -= deltaTime;
            OnTimerTick?.Invoke(Math.Max(0f, _stepTimeRemaining));
            if (_stepTimeRemaining <= 0f)
                AutoLockOnTimeout();
        }

        /// <summary>Player hovers a champion in the grid.</summary>
        public void Hover(string championId)
        {
            if (IsComplete || !IsSelectable(championId)) return;
            _hoverChampionId = championId;
            OnHoverChanged?.Invoke(championId);
        }

        /// <summary>Lock in the current hover for the active step.</summary>
        public bool LockIn()
        {
            if (IsComplete || string.IsNullOrEmpty(_hoverChampionId)) return false;
            if (!IsSelectable(_hoverChampionId)) return false;

            var step = CurrentStep;
            if (step.Type == DraftStepType.Ban)
            {
                _banned.Add(_hoverChampionId);
                OnBanLocked?.Invoke(step.Team, _hoverChampionId);
            }
            else
            {
                _picks[step.Team][step.SlotIndex] = _hoverChampionId;
                OnPickLocked?.Invoke(step.Team, step.SlotIndex, _hoverChampionId);
            }

            _hoverChampionId = null;
            AdvanceStep();
            return true;
        }

        /// <summary>A champion is selectable if not banned and not already picked.</summary>
        public bool IsSelectable(string championId)
        {
            if (string.IsNullOrEmpty(championId)) return false;
            if (_banned.Contains(championId)) return false;
            foreach (var team in _picks.Values)
                foreach (var c in team)
                    if (c == championId) return false;
            return true;
        }

        public IReadOnlyCollection<string> Banned => _banned;
        public string GetPick(DraftTeam team, int slot) => _picks[team][slot];

        // ---- internals ---- //
        private void AdvanceStep()
        {
            _stepIndex++;
            if (IsComplete) { OnDraftComplete?.Invoke(); return; }
            _stepTimeRemaining = _secondsPerStep;
            OnStepStarted?.Invoke(CurrentStep);
        }

        private void AutoLockOnTimeout()
        {
            // If nothing hovered, the step is skipped (ban) or auto-assigned later
            // by the server's fill logic. For the prototype we just advance.
            if (!string.IsNullOrEmpty(_hoverChampionId) && IsSelectable(_hoverChampionId))
                LockIn();
            else
                AdvanceStep();
        }
    }
}
