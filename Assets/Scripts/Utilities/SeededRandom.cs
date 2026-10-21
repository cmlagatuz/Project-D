// Project D | Codename: Zantetsuken — SeededRandom.cs (deterministic, server-seeded)
using System;
namespace ProjectD.Utilities
{
    public sealed class SeededRandom
    {
        private readonly Random _random;
        public int Seed { get; }
        public SeededRandom(int seed) { Seed = seed; _random = new Random(seed); }
        public int Range(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
        public float Value01() => (float)_random.NextDouble();
        public void ShuffleInPlace<T>(System.Collections.Generic.IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            { int j = _random.Next(i + 1); (list[i], list[j]) = (list[j], list[i]); }
        }
    }
}
