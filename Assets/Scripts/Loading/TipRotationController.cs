// Project D  |  Codename: Zantetsuken
// TipRotationController.cs  (Loading — build + rotate the tip/trivia carousel)
//
// Builds a no-immediate-repeat playlist of TipCards. A MATCHUP CALLOUT (from
// MatchupCallout) is pinned FIRST so the player immediately sees advice for the
// exact laner they'll face; then the local champion's tips/trivia, enemy "against"
// tips, and general tips are interleaved. Deterministic for a given seed.

using System.Collections.Generic;

namespace ProjectD.Loading
{
    public sealed class TipRotationController
    {
        private readonly List<TipCard> _playlist = new();
        private int _index = -1;

        public IReadOnlyList<TipCard> Playlist => _playlist;
        public int Count => _playlist.Count;

        public void Build(
            string localChampionId,
            IReadOnlyList<string> enemyChampionIds,
            IReadOnlyDictionary<string, ChampionTipView> champions,
            IReadOnlyList<string> generalTips,
            int seed,
            TipCard? pinnedCallout = null)
        {
            _playlist.Clear();
            _index = -1;
            var rng = new DeterministicRng(seed);

            // Pin the personalized matchup callout at the very front.
            if (pinnedCallout.HasValue && !string.IsNullOrEmpty(pinnedCallout.Value.Body))
                _playlist.Add(pinnedCallout.Value);

            var primary = new List<TipCard>();
            var against = new List<TipCard>();
            var general = new List<TipCard>();

            if (localChampionId != null && champions.TryGetValue(localChampionId, out var lc))
            {
                var play = Shuffle(lc.PlayingTips, rng);
                var triv = Shuffle(lc.Trivia, rng);
                int n = System.Math.Max(play.Count, triv.Count);
                for (int i = 0; i < n; i++)
                {
                    if (i < play.Count) primary.Add(Card(TipKind.PlayingTip, $"TIP - Playing {lc.DisplayName}", play[i], lc.ChampionId));
                    if (i < triv.Count) primary.Add(Card(TipKind.Trivia, "DID YOU KNOW?", triv[i], lc.ChampionId));
                }
            }

            if (enemyChampionIds != null)
                foreach (var eid in enemyChampionIds)
                    if (eid != null && champions.TryGetValue(eid, out var ec) && ec.AgainstTips.Count > 0)
                        against.Add(Card(TipKind.AgainstTip, $"TIP - Facing {ec.DisplayName}", Shuffle(ec.AgainstTips, rng)[0], ec.ChampionId));

            if (generalTips != null)
                foreach (var g in Shuffle(new List<string>(generalTips), rng))
                    general.Add(Card(TipKind.General, "GAMEPLAY TIP", g, ""));

            int pi = 0, ai = 0, gi = 0; bool toggle = false;
            while (pi < primary.Count || ai < against.Count || gi < general.Count)
            {
                if (pi < primary.Count) _playlist.Add(primary[pi++]);
                if (toggle && ai < against.Count) _playlist.Add(against[ai++]);
                else if (gi < general.Count) _playlist.Add(general[gi++]);
                else if (ai < against.Count) _playlist.Add(against[ai++]);
                toggle = !toggle;
            }
            DeDupeAdjacent();
        }

        public TipCard Next()
        {
            if (_playlist.Count == 0) return default;
            _index = (_index + 1) % _playlist.Count;
            return _playlist[_index];
        }

        public TipCard Current => (_index >= 0 && _index < _playlist.Count) ? _playlist[_index] : default;

        private static TipCard Card(TipKind kind, string heading, string body, string champ)
            => new TipCard { Kind = kind, Heading = heading, Body = body, ChampionId = champ };

        private void DeDupeAdjacent()
        {
            for (int i = 1; i < _playlist.Count; i++)
                if (_playlist[i].Body == _playlist[i - 1].Body)
                    for (int j = i + 1; j < _playlist.Count; j++)
                        if (_playlist[j].Body != _playlist[i - 1].Body) { (_playlist[i], _playlist[j]) = (_playlist[j], _playlist[i]); break; }
        }

        private static List<T> Shuffle<T>(IReadOnlyList<T> src, DeterministicRng rng)
        {
            var list = new List<T>(src);
            for (int i = list.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (list[i], list[j]) = (list[j], list[i]); }
            return list;
        }

        private sealed class DeterministicRng
        {
            private uint _s;
            public DeterministicRng(int seed) { _s = seed == 0 ? 0x9E3779B9u : (uint)seed; }
            public int Next(int maxExclusive)
            {
                _s ^= _s << 13; _s ^= _s >> 17; _s ^= _s << 5;
                return maxExclusive <= 1 ? 0 : (int)(_s % (uint)maxExclusive);
            }
        }
    }

    public struct ChampionTipView
    {
        public string ChampionId;
        public string DisplayName;
        public IReadOnlyList<string> PlayingTips;
        public IReadOnlyList<string> AgainstTips;
        public IReadOnlyList<string> Trivia;
    }
}
