// Project D | Codename: Zantetsuken — ObjectPool.cs (mobile churn control)
using System;
using System.Collections.Generic;
namespace ProjectD.Utilities
{
    public sealed class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _available = new Stack<T>();
        private readonly Func<T> _factory; private readonly Action<T> _onGet, _onReturn;
        public int CountInactive => _available.Count;
        public int CountActive { get; private set; }
        public ObjectPool(Func<T> factory, Action<T> onGet = null, Action<T> onReturn = null, int prewarm = 0)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _onGet = onGet; _onReturn = onReturn;
            for (int i = 0; i < prewarm; i++) _available.Push(_factory());
        }
        public T Get() { T item = _available.Count > 0 ? _available.Pop() : _factory(); CountActive++; _onGet?.Invoke(item); return item; }
        public void Return(T item) { if (item == null) return; _onReturn?.Invoke(item); _available.Push(item); CountActive = Math.Max(0, CountActive - 1); }
    }
}
