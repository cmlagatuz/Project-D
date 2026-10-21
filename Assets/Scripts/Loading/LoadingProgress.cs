// Project D  |  Codename: Zantetsuken
// LoadingProgress.cs  (Loading — aggregate multi-source load progress)

using System.Collections.Generic;

namespace ProjectD.Loading
{
    public sealed class LoadingProgress
    {
        private struct Task { public float Weight; public float Progress; }
        private readonly Dictionary<string, Task> _tasks = new();

        public void Register(string id, float weight = 1f)
        { _tasks.TryGetValue(id, out var t); t.Weight = weight <= 0 ? 1f : weight; _tasks[id] = t; }

        public void Report(string id, float p)
        { if (!_tasks.TryGetValue(id, out var t)) t.Weight = 1f; t.Progress = p < 0 ? 0 : (p > 1 ? 1 : p); _tasks[id] = t; }

        public float Total
        {
            get { if (_tasks.Count == 0) return 0f; float w = 0f, p = 0f; foreach (var t in _tasks.Values) { w += t.Weight; p += t.Weight * t.Progress; } return w <= 0 ? 0f : p / w; }
        }
        public bool IsComplete => Total >= 0.999f;
        public void Reset() => _tasks.Clear();
    }
}
