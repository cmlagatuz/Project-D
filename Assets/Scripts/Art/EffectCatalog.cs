// Project D  |  Codename: Zantetsuken
// EffectCatalog.cs  (Art runtime — key -> EffectDefinition lookup)
//
// A single asset holding all EffectDefinitions, with an O(1) dictionary built on
// first access. The EffectSpawner and AudioCuePlayer resolve cue keys through this.

using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.Art.Effects
{
    [CreateAssetMenu(fileName = "EffectCatalog", menuName = "Project D/Effect Catalog")]
    public sealed class EffectCatalog : ScriptableObject
    {
        public EffectDefinition[] entries = new EffectDefinition[0];

        private Dictionary<string, EffectDefinition> _map;

        public EffectDefinition Resolve(string key)
        {
            if (_map == null) Build();
            return key != null && _map.TryGetValue(key, out var d) ? d : null;
        }

        public bool TryResolve(string key, out EffectDefinition def)
        {
            def = Resolve(key);
            return def != null;
        }

        private void Build()
        {
            _map = new Dictionary<string, EffectDefinition>(entries.Length);
            foreach (var e in entries)
                if (e != null && !string.IsNullOrEmpty(e.key))
                    _map[e.key] = e;
        }

#if UNITY_EDITOR
        private void OnValidate() => _map = null; // rebuild after edits
#endif
    }
}
